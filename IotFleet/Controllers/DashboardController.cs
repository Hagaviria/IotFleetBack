using Microsoft.AspNetCore.Mvc;
using IotFleet.Extensions;
using SharedKernel;
using Microsoft.EntityFrameworkCore;
using Application.Abstractions.Data;
using IotFleet.Infrastructure;

namespace IotFleet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController(
        IApplicationDbContext context
        ) : ControllerBase
    {
        /// <summary>
        /// Gets dashboard statistics.
        /// </summary>
        /// <returns>Dashboard statistics including vehicle count, sensor data count, and user count.</returns>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var vehicleCount = await context.Vehicles.CountAsync();
                var sensorDataCount = await context.SensorData.CountAsync();
                var userCount = await context.Users.CountAsync(u => u.Estado);

                var statistics = new
                {
                    TotalVehicles = vehicleCount,
                    TotalSensorData = sensorDataCount,
                    TotalUsers = userCount,
                    LastUpdated = DateTime.UtcNow
                };

                return CustomResults.Success<object>(statistics);
            }
            catch (Exception ex)
            {
                return CustomResults.Problem(Result.Failure(Error.Failure("Dashboard.StatisticsError", $"Error retrieving statistics: {ex.Message}")));
            }
        }

        /// <summary>
        /// Gets recent sensor data for dashboard.
        /// </summary>
        /// <param name="limit">Number of recent records to retrieve (default: 10).</param>
        /// <returns>Recent sensor data records.</returns>
        [HttpGet("recent-sensor-data")]
        public async Task<IActionResult> GetRecentSensorData([FromQuery] int limit = 10)
        {
            try
            {
                if (limit <= 0 || limit > 100)
                    limit = 10;

                var recentSensorData = await context.SensorData
                    .AsNoTracking()
                    .Include(sd => sd.Vehicle)
                    .OrderByDescending(sd => sd.Timestamp)
                    .Take(limit)
                    .Select(sd => new
                    {
                        sd.Id,
                        sd.VehicleId,
                        VehicleLicensePlate = sd.Vehicle.LicensePlate,
                        sd.Latitude,
                        sd.Longitude,
                        sd.Speed,
                        sd.FuelLevel,
                        sd.EngineTemperature,
                        sd.Timestamp
                    })
                    .ToListAsync();

                return CustomResults.Success<object>(recentSensorData);
            }
            catch (Exception ex)
            {
                return CustomResults.Problem(Result.Failure(Error.Failure("Dashboard.RecentSensorDataError", $"Error retrieving recent sensor data: {ex.Message}")));
            }
        }

        /// <summary>
        /// Gets vehicle status summary.
        /// </summary>
        /// <returns>Summary of vehicle statuses including fuel levels and maintenance status.</returns>
        [HttpGet("vehicle-status")]
        public async Task<IActionResult> GetVehicleStatus()
        {
            try
            {
                var vehicleStatus = await context.Vehicles
                    .AsNoTracking()
                    .Select(v => new
                    {
                        v.Id,
                        v.LicensePlate,
                        v.Model,
                        v.Brand,
                        v.FuelCapacity,
                        v.AverageConsumption,
                        v.LastMaintenance,
                        LastSensorData = context.SensorData
                            .Where(sd => sd.VehicleId == v.Id)
                            .OrderByDescending(sd => sd.Timestamp)
                            .Select(sd => new
                            {
                                sd.FuelLevel,
                                sd.EngineTemperature,
                                sd.Timestamp
                            })
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                return CustomResults.Success<object>(vehicleStatus);
            }
            catch (Exception ex)
            {
                return CustomResults.Problem(Result.Failure(Error.Failure("Dashboard.VehicleStatusError", $"Error retrieving vehicle status: {ex.Message}")));
            }
        }

        /// <summary>
        /// Gets fuel consumption statistics.
        /// </summary>
        /// <returns>Fuel consumption statistics by vehicle.</returns>
        [HttpGet("fuel-consumption")]
        public async Task<IActionResult> GetFuelConsumptionStats()
        {
            try
            {
                var fuelStats = await context.SensorData
                    .AsNoTracking()
                    .Include(sd => sd.Vehicle)
                    .Where(sd => sd.FuelConsumption.HasValue && sd.FuelConsumption > 0)
                    .GroupBy(sd => new { sd.VehicleId, sd.Vehicle.LicensePlate, sd.Vehicle.Model, sd.Vehicle.Brand })
                    .Select(g => new
                    {
                        VehicleId = g.Key.VehicleId,
                        LicensePlate = g.Key.LicensePlate,
                        Model = g.Key.Model,
                        Brand = g.Key.Brand,
                        AverageFuelConsumption = g.Average(sd => sd.FuelConsumption ?? 0),
                        MaxFuelConsumption = g.Max(sd => sd.FuelConsumption ?? 0),
                        MinFuelConsumption = g.Min(sd => sd.FuelConsumption ?? 0),
                        RecordCount = g.Count()
                    })
                    .OrderByDescending(x => x.AverageFuelConsumption)
                    .ToListAsync();

                return CustomResults.Success<object>(fuelStats);
            }
            catch (Exception ex)
            {
                return CustomResults.Problem(Result.Failure(Error.Failure("Dashboard.FuelConsumptionError", $"Error retrieving fuel consumption statistics: {ex.Message}")));
            }
        }

        /// <summary>
        /// Gets historical data for charts and analysis.
        /// </summary>
        /// <param name="vehicleId">Vehicle ID to get data for.</param>
        /// <param name="startDate">Start date for the data range.</param>
        /// <param name="endDate">End date for the data range.</param>
        /// <param name="limit">Maximum number of records to return (default: 1000).</param>
        /// <returns>Historical sensor data for the specified vehicle and date range.</returns>
        [HttpGet("historical-data")]
        public async Task<IActionResult> GetHistoricalData(
            [FromQuery] string vehicleId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int limit = 1000)
        {
            try
            {
                if (string.IsNullOrEmpty(vehicleId))
                {
                    return CustomResults.Problem(Result.Failure(Error.Problem("Dashboard.InvalidVehicleId", "Vehicle ID is required")));
                }

                if (!Guid.TryParse(vehicleId, out var vehicleIdGuid))
                {
                    return CustomResults.Problem(Result.Failure(Error.Problem("Dashboard.InvalidVehicleId", "Invalid vehicle ID format")));
                }

                if (limit <= 0 || limit > 5000)
                    limit = 1000;

                var historicalData = await context.SensorData
                    .AsNoTracking()
                    .Include(sd => sd.Vehicle)
                    .Where(sd => sd.VehicleId == vehicleIdGuid && 
                                sd.Timestamp >= startDate && 
                                sd.Timestamp <= endDate)
                    .OrderBy(sd => sd.Timestamp)
                    .Take(limit)
                    .Select(sd => new
                    {
                        Id = sd.Id,
                        VehicleId = sd.VehicleId,
                        Timestamp = sd.Timestamp,
                        Speed = sd.Speed,
                        FuelLevel = sd.FuelLevel,
                        FuelConsumption = sd.FuelConsumption,
                        EngineTemperature = sd.EngineTemperature,
                        AmbientTemperature = sd.AmbientTemperature,
                        Latitude = sd.Latitude,
                        Longitude = sd.Longitude,
                        Altitude = sd.Altitude,
                        Vehicle = new
                        {
                            LicensePlate = sd.Vehicle.LicensePlate,
                            Model = sd.Vehicle.Model,
                            Brand = sd.Vehicle.Brand,
                            FuelCapacity = sd.Vehicle.FuelCapacity,
                            AverageConsumption = sd.Vehicle.AverageConsumption
                        }
                    })
                    .ToListAsync();

                return CustomResults.Success<object>(historicalData);
            }
            catch (Exception ex)
            {
                return CustomResults.Problem(Result.Failure(Error.Failure("Dashboard.HistoricalDataError", $"Error retrieving historical data: {ex.Message}")));
            }
        }

        /// <summary>
        /// Gets aggregated historical statistics for charts.
        /// </summary>
        /// <param name="vehicleId">Vehicle ID to get statistics for.</param>
        /// <param name="startDate">Start date for the data range.</param>
        /// <param name="endDate">End date for the data range.</param>
        /// <param name="groupBy">Grouping interval: hour, day, week (default: day).</param>
        /// <returns>Aggregated historical statistics.</returns>
        [HttpGet("historical-statistics")]
        public async Task<IActionResult> GetHistoricalStatistics(
            [FromQuery] string vehicleId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string groupBy = "day")
        {
            try
            {
                if (string.IsNullOrEmpty(vehicleId))
                {
                    return CustomResults.Problem(Result.Failure(Error.Problem("Dashboard.InvalidVehicleId", "Vehicle ID is required")));
                }

                if (!Guid.TryParse(vehicleId, out var vehicleIdGuid))
                {
                    return CustomResults.Problem(Result.Failure(Error.Problem("Dashboard.InvalidVehicleId", "Invalid vehicle ID format")));
                }

                var query = context.SensorData
                    .AsNoTracking()
                    .Where(sd => sd.VehicleId == vehicleIdGuid && 
                                sd.Timestamp >= startDate && 
                                sd.Timestamp <= endDate);

                var statistics = await query.GroupBy(sd => sd.Timestamp.Date)
                    .Select(g => new
                    {
                        Period = g.Key,
                        AverageSpeed = g.Average(sd => sd.Speed),
                        AverageFuelLevel = g.Average(sd => sd.FuelLevel),
                        AverageTemperature = g.Average(sd => sd.EngineTemperature),
                        Count = g.Count()
                    })
                    .ToListAsync();

                return CustomResults.Success<object>(statistics);
            }
            catch (Exception ex)
            {
                return CustomResults.Problem(Result.Failure(Error.Failure("Dashboard.HistoricalStatisticsError", $"Error retrieving historical statistics: {ex.Message}")));
            }
        }
    }
}
