using Microsoft.AspNetCore.Mvc;
using IotFleet.Extensions;
using SharedKernel;
using IotFleet.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Application.Abstractions.Data;

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
    }
}
