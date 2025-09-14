using Microsoft.AspNetCore.Mvc;
using IotFleet.Infrastructure;
using SharedKernel;
using Microsoft.EntityFrameworkCore;
using Application.Abstractions.Data;

namespace IotFleet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoutesController(
        IApplicationDbContext context
        ) : ControllerBase
    {
        /// <summary>
        /// Gets historical routes for a specific vehicle.
        /// </summary>
        /// <param name="vehicleId">The vehicle ID.</param>
        /// <param name="startDate">Start date for the route history.</param>
        /// <param name="endDate">End date for the route history.</param>
        /// <param name="limit">Maximum number of routes to return.</param>
        /// <returns>List of historical routes.</returns>
        [HttpGet("historical")]
        public async Task<IActionResult> GetHistoricalRoutes(
            [FromQuery] string? vehicleId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int limit = 20)
        {
            try
            {
                var query = context.SensorData.AsNoTracking();

                if (!string.IsNullOrEmpty(vehicleId) && Guid.TryParse(vehicleId, out var vehicleIdGuid))
                {
                    query = query.Where(sd => sd.VehicleId == vehicleIdGuid);
                }

                var start = startDate ?? DateTime.UtcNow.AddDays(-7);
                var end = endDate ?? DateTime.UtcNow;
                query = query.Where(sd => sd.Timestamp >= start && sd.Timestamp <= end);

                var routes = await query
                    .Include(sd => sd.Vehicle)
                    .OrderBy(sd => sd.Timestamp)
                    .GroupBy(sd => sd.Timestamp.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        VehicleId = g.First().VehicleId,
                        VehicleLicensePlate = g.First().Vehicle.LicensePlate,
                        Points = g.Select(p => new
                        {
                            p.Latitude,
                            p.Longitude,
                            p.Speed,
                            p.FuelLevel,
                            p.Timestamp
                        }).ToList(),
                        TotalDistance = CalculateTotalDistance(g.Select(p => new { p.Latitude, p.Longitude, p.Timestamp }).Cast<object>().ToList()),
                        AverageSpeed = g.Average(p => p.Speed ?? 0),
                        MaxSpeed = g.Max(p => p.Speed ?? 0),
                        MinSpeed = g.Min(p => p.Speed ?? 0),
                        StartTime = g.Min(p => p.Timestamp),
                        EndTime = g.Max(p => p.Timestamp),
                        Duration = (g.Max(p => p.Timestamp) - g.Min(p => p.Timestamp)).TotalMinutes
                    })
                    .OrderByDescending(r => r.Date)
                    .Take(limit)
                    .ToListAsync();

                return CustomResults.Success<object>(routes);
            }
            catch (Exception ex)
            {
                return CustomResults.Problem(Result.Failure(Error.Failure("Routes.HistoricalError", $"Error retrieving historical routes: {ex.Message}")));
            }
        }

        /// <summary>
        /// Gets route details for a specific date and vehicle.
        /// </summary>
        /// <param name="vehicleId">The vehicle ID.</param>
        /// <param name="date">The date for the route.</param>
        /// <returns>Detailed route information.</returns>
        [HttpGet("details")]
        public async Task<IActionResult> GetRouteDetails(
            [FromQuery] string vehicleId,
            [FromQuery] DateTime date)
        {
            try
            {
                if (!Guid.TryParse(vehicleId, out var vehicleIdGuid))
                {
                    return CustomResults.Problem(Result.Failure(Error.Failure("Routes.InvalidVehicleId", "Invalid vehicle ID format")));
                }

                var startOfDay = date.Date;
                var endOfDay = startOfDay.AddDays(1);

                var routePoints = await context.SensorData
                    .AsNoTracking()
                    .Include(sd => sd.Vehicle)
                    .Where(sd => sd.VehicleId == vehicleIdGuid && 
                                sd.Timestamp >= startOfDay && 
                                sd.Timestamp < endOfDay)
                    .OrderBy(sd => sd.Timestamp)
                    .Select(sd => new
                    {
                        sd.Latitude,
                        sd.Longitude,
                        sd.Speed,
                        sd.FuelLevel,
                        sd.EngineTemperature,
                        sd.Timestamp
                    })
                    .ToListAsync();

                if (!routePoints.Any())
                {
                    return CustomResults.Problem(Result.Failure(Error.NotFound("Routes.NotFound", "No route data found for the specified date and vehicle")));
                }

                var routeDetails = new
                {
                    VehicleId = vehicleIdGuid,
                    Date = date.Date,
                    TotalPoints = routePoints.Count,
                    TotalDistance = CalculateTotalDistance(routePoints.Select(p => new { p.Latitude, p.Longitude, p.Timestamp }).Cast<object>().ToList()),
                    AverageSpeed = routePoints.Average(p => p.Speed ?? 0),
                    MaxSpeed = routePoints.Max(p => p.Speed ?? 0),
                    MinSpeed = routePoints.Min(p => p.Speed ?? 0),
                    StartTime = routePoints.Min(p => p.Timestamp),
                    EndTime = routePoints.Max(p => p.Timestamp),
                    Duration = (routePoints.Max(p => p.Timestamp) - routePoints.Min(p => p.Timestamp)).TotalMinutes,
                    FuelConsumption = CalculateFuelConsumption(routePoints.Select(p => new { p.FuelLevel, p.Timestamp }).Cast<object>().ToList()),
                    Points = routePoints
                };

                return CustomResults.Success<object>(routeDetails);
            }
            catch (Exception ex)
            {
                return CustomResults.Problem(Result.Failure(Error.Failure("Routes.DetailsError", $"Error retrieving route details: {ex.Message}")));
            }
        }

        /// <summary>
        /// Gets route statistics for a vehicle over a date range.
        /// </summary>
        /// <param name="vehicleId">The vehicle ID.</param>
        /// <param name="startDate">Start date for statistics.</param>
        /// <param name="endDate">End date for statistics.</param>
        /// <returns>Route statistics.</returns>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetRouteStatistics(
            [FromQuery] string vehicleId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                if (!Guid.TryParse(vehicleId, out var vehicleIdGuid))
                {
                    return CustomResults.Problem(Result.Failure(Error.Failure("Routes.InvalidVehicleId", "Invalid vehicle ID format")));
                }

                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var sensorData = await context.SensorData
                    .AsNoTracking()
                    .Include(sd => sd.Vehicle)
                    .Where(sd => sd.VehicleId == vehicleIdGuid && 
                                sd.Timestamp >= start && 
                                sd.Timestamp <= end)
                    .OrderBy(sd => sd.Timestamp)
                    .ToListAsync();

                if (!sensorData.Any())
                {
                    return CustomResults.Problem(Result.Failure(Error.NotFound("Routes.NoData", "No route data found for the specified period")));
                }

                var statistics = new
                {
                    VehicleId = vehicleIdGuid,
                    VehicleLicensePlate = sensorData.First().Vehicle.LicensePlate,
                    Period = new { Start = start, End = end },
                    TotalDistance = CalculateTotalDistance(sensorData.Select(sd => new { sd.Latitude, sd.Longitude, sd.Timestamp }).Cast<object>().ToList()),
                    TotalTrips = sensorData.GroupBy(sd => sd.Timestamp.Date).Count(),
                    AverageSpeed = sensorData.Average(sd => sd.Speed ?? 0),
                    MaxSpeed = sensorData.Max(sd => sd.Speed ?? 0),
                    MinSpeed = sensorData.Min(sd => sd.Speed ?? 0),
                    TotalDuration = (sensorData.Max(sd => sd.Timestamp) - sensorData.Min(sd => sd.Timestamp)).TotalHours,
                    FuelConsumption = CalculateFuelConsumption(sensorData.Select(sd => new { sd.FuelLevel, sd.Timestamp }).Cast<object>().ToList()),
                    Efficiency = CalculateEfficiency(sensorData.Select(sd => new { sd.Latitude, sd.Longitude, sd.FuelLevel, sd.Timestamp }).Cast<object>().ToList())
                };

                return CustomResults.Success<object>(statistics);
            }
            catch (Exception ex)
            {
                return CustomResults.Problem(Result.Failure(Error.Failure("Routes.StatisticsError", $"Error retrieving route statistics: {ex.Message}")));
            }
        }

        private static double CalculateTotalDistance(List<object> points)
        {
            if (points.Count < 2) return 0;

            double totalDistance = 0;
            for (int i = 1; i < points.Count; i++)
            {
                var prev = points[i - 1];
                var current = points[i];
                
                var prevLat = (double)prev.GetType().GetProperty("Latitude")!.GetValue(prev)!;
                var prevLon = (double)prev.GetType().GetProperty("Longitude")!.GetValue(prev)!;
                var currLat = (double)current.GetType().GetProperty("Latitude")!.GetValue(current)!;
                var currLon = (double)current.GetType().GetProperty("Longitude")!.GetValue(current)!;
                
                totalDistance += CalculateDistanceBetweenPoints(prevLat, prevLon, currLat, currLon);
            }
            return Math.Round(totalDistance, 2);
        }

        private static double CalculateDistanceBetweenPoints(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in kilometers
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        private static double CalculateFuelConsumption(List<object> points)
        {
            if (points.Count < 2) return 0;

            var firstPoint = points.First();
            var lastPoint = points.Last();
            
            var firstFuel = (double)firstPoint.GetType().GetProperty("FuelLevel")!.GetValue(firstPoint)!;
            var lastFuel = (double)lastPoint.GetType().GetProperty("FuelLevel")!.GetValue(lastPoint)!;
            
            var fuelUsed = firstFuel - lastFuel;
            return Math.Max(0, fuelUsed);
        }

        private static double CalculateEfficiency(List<object> points)
        {
            var totalDistance = CalculateTotalDistance(points);
            var fuelConsumption = CalculateFuelConsumption(points);
            
            if (fuelConsumption <= 0) return 0;
            
            return Math.Round(totalDistance / fuelConsumption, 2);
        }
    }
}
