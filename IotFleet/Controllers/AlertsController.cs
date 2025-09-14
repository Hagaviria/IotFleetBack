using Microsoft.AspNetCore.Mvc;
using IotFleet.Extensions;
using SharedKernel;
using IotFleet.Infrastructure;
using Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore;
using Domain.DTOs;
using Application.Abstractions.Services;
using Microsoft.AspNetCore.Authorization;

namespace IotFleet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AlertsController(
        IApplicationDbContext context,
        IFuelPredictionService fuelPredictionService,
        IWebSocketService webSocketService
        ) : ControllerBase
    {
        /// <summary>
        /// Gets all active fuel alerts for admin users.
        /// </summary>
        /// <returns>List of active fuel alerts.</returns>
        [HttpGet("fuel-alerts")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetFuelAlerts()
        {
            try
            {
                var cutoffTime = DateTime.UtcNow.AddHours(-24);
                
                var alerts = await context.SensorData
                    .AsNoTracking()
                    .Include(sd => sd.Vehicle)
                    .Where(sd => sd.Timestamp >= cutoffTime && sd.FuelLevel < 20) // Nivel bajo de combustible
                    .Select(sd => new
                    {
                        sd.VehicleId,
                        sd.Vehicle.LicensePlate,
                        sd.FuelLevel,
                        sd.Timestamp,
                        sd.Speed,
                        sd.EngineTemperature
                    })
                    .OrderByDescending(sd => sd.Timestamp)
                    .ToListAsync();

                var fuelAlerts = new List<FuelAlertDTO>();
                
                foreach (var alert in alerts)
                {
                    var vehicle = await context.Vehicles
                        .AsNoTracking()
                        .FirstOrDefaultAsync(v => v.Id == alert.VehicleId);
                    
                    if (vehicle != null)
                    {
                        var sensorData = new Domain.Models.SensorData
                        {
                            VehicleId = alert.VehicleId,
                            FuelLevel = alert.FuelLevel,
                            Speed = alert.Speed,
                            EngineTemperature = alert.EngineTemperature,
                            Timestamp = alert.Timestamp
                        };
                        
                        var fuelAlert = await fuelPredictionService.CalculateFuelAutonomy(
                            vehicle, 
                            sensorData, 
                            CancellationToken.None);
                        
                        if (fuelAlert != null)
                        {
                            fuelAlerts.Add(fuelAlert);
                        }
                    }
                }

                return CustomResults.Success<object>(fuelAlerts);
            }
            catch (Exception ex)
            {
                return CustomResults.Problem(Result.Failure(Error.Failure("Alerts.FuelAlertsError", $"Error retrieving fuel alerts: {ex.Message}")));
            }
        }

        /// <summary>
        /// Gets fuel alert statistics for admin users.
        /// </summary>
        /// <returns>Fuel alert statistics.</returns>
        [HttpGet("fuel-alerts/statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetFuelAlertStatistics()
        {
            try
            {
                var cutoffTime = DateTime.UtcNow.AddHours(-24);
                
                var statistics = await context.SensorData
                    .AsNoTracking()
                    .Include(sd => sd.Vehicle)
                    .Where(sd => sd.Timestamp >= cutoffTime)
                    .GroupBy(sd => new { 
                        Critical = sd.FuelLevel < 5,
                        High = sd.FuelLevel >= 5 && sd.FuelLevel < 10,
                        Medium = sd.FuelLevel >= 10 && sd.FuelLevel < 15,
                        Low = sd.FuelLevel >= 15 && sd.FuelLevel < 20
                    })
                    .Select(g => new
                    {
                        CriticalAlerts = g.Count(x => x.FuelLevel < 5),
                        HighAlerts = g.Count(x => x.FuelLevel >= 5 && x.FuelLevel < 10),
                        MediumAlerts = g.Count(x => x.FuelLevel >= 10 && x.FuelLevel < 15),
                        LowAlerts = g.Count(x => x.FuelLevel >= 15 && x.FuelLevel < 20),
                        TotalVehicles = g.Select(x => x.VehicleId).Distinct().Count(),
                        AverageFuelLevel = g.Average(x => x.FuelLevel),
                        LastUpdate = g.Max(x => x.Timestamp)
                    })
                    .FirstOrDefaultAsync();

                return CustomResults.Success<object>(statistics ?? new
                {
                    CriticalAlerts = 0,
                    HighAlerts = 0,
                    MediumAlerts = 0,
                    LowAlerts = 0,
                    TotalVehicles = 0,
                    AverageFuelLevel = 0.0,
                    LastUpdate = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return CustomResults.Problem(Result.Failure(Error.Failure("Alerts.StatisticsError", $"Error retrieving alert statistics: {ex.Message}")));
            }
        }

        /// <summary>
        /// Triggers a fuel alert calculation for a specific vehicle.
        /// </summary>
        /// <param name="vehicleId">The vehicle ID.</param>
        /// <returns>Fuel alert if generated.</returns>
        [HttpPost("fuel-alerts/calculate/{vehicleId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CalculateFuelAlert(string vehicleId)
        {
            try
            {
                if (!Guid.TryParse(vehicleId, out var vehicleIdGuid))
                {
                    return CustomResults.Problem(Result.Failure(Error.Problem("Alerts.InvalidVehicleId", "Invalid vehicle ID format")));
                }

                var vehicle = await context.Vehicles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(v => v.Id == vehicleIdGuid);

                if (vehicle == null)
                {
                    return CustomResults.Problem(Result.Failure(Error.NotFound("Vehicle.NotFound", "Vehicle not found")));
                }

                var latestSensorData = await context.SensorData
                    .AsNoTracking()
                    .Where(sd => sd.VehicleId == vehicleIdGuid)
                    .OrderByDescending(sd => sd.Timestamp)
                    .FirstOrDefaultAsync();

                if (latestSensorData == null)
                {
                    return CustomResults.Problem(Result.Failure(Error.NotFound("SensorData.NotFound", "No sensor data found for this vehicle")));
                }

                var fuelAlert = await fuelPredictionService.CalculateFuelAutonomy(
                    vehicle, 
                    latestSensorData, 
                    CancellationToken.None);

                if (fuelAlert != null)
                {
                    await webSocketService.BroadcastAlert(fuelAlert);
                    
                    return CustomResults.Success<object>(fuelAlert, title: "Alert.Generated");
                }
                else
                {
                    return CustomResults.Success<object>(new { message = "No fuel alert needed for this vehicle" }, title: "Alert.NoAlert");
                }
            }
            catch (Exception ex)
            {
                return CustomResults.Problem(Result.Failure(Error.Failure("Alerts.CalculationError", $"Error calculating fuel alert: {ex.Message}")));
            }
        }

        /// <summary>
        /// Generates predictive alerts for all vehicles.
        /// </summary>
        /// <returns>List of generated alerts.</returns>
        [HttpPost("predictive/generate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GeneratePredictiveAlerts()
        {
            try
            {
                var vehicles = await context.Vehicles
                    .AsNoTracking()
                    .ToListAsync();

                var generatedAlerts = new List<object>();

                foreach (var vehicle in vehicles)
                {
                    var latestSensorData = await context.SensorData
                        .AsNoTracking()
                        .Where(sd => sd.VehicleId == vehicle.Id)
                        .OrderByDescending(sd => sd.Timestamp)
                        .FirstOrDefaultAsync();

                    if (latestSensorData != null)
                    {
                        var fuelAlert = await fuelPredictionService.CalculateFuelAutonomy(
                            vehicle, 
                            latestSensorData, 
                            CancellationToken.None);

                        if (fuelAlert != null)
                        {
                            await webSocketService.BroadcastAlert(fuelAlert);
                            generatedAlerts.Add(new
                            {
                                VehicleId = vehicle.Id,
                                LicensePlate = vehicle.LicensePlate,
                                Alert = fuelAlert
                            });
                        }
                    }
                }

                return CustomResults.Success<object>(new
                {
                    GeneratedAlerts = generatedAlerts.Count,
                    Alerts = generatedAlerts
                }, title: "Alerts.Generated");
            }
            catch (Exception ex)
            {
                return CustomResults.Problem(Result.Failure(Error.Failure("Alerts.GenerationError", $"Error generating predictive alerts: {ex.Message}")));
            }
        }

        /// <summary>
        /// Gets recent alerts for a specific vehicle.
        /// </summary>
        /// <param name="vehicleId">The vehicle ID.</param>
        /// <param name="hours">Number of hours to look back (default: 24).</param>
        /// <returns>Recent alerts for the vehicle.</returns>
        [HttpGet("vehicle/{vehicleId}/recent")]
        public async Task<IActionResult> GetVehicleRecentAlerts(string vehicleId, [FromQuery] int hours = 24)
        {
            try
            {
                if (!Guid.TryParse(vehicleId, out var vehicleIdGuid))
                {
                    return CustomResults.Problem(Result.Failure(Error.Problem("Alerts.InvalidVehicleId", "Invalid vehicle ID format")));
                }

                var cutoffTime = DateTime.UtcNow.AddHours(-hours);
                
                var alerts = await context.SensorData
                    .AsNoTracking()
                    .Include(sd => sd.Vehicle)
                    .Where(sd => sd.VehicleId == vehicleIdGuid && sd.Timestamp >= cutoffTime)
                    .Select(sd => new
                    {
                        sd.Id,
                        sd.VehicleId,
                        sd.Vehicle.LicensePlate,
                        sd.FuelLevel,
                        sd.Speed,
                        sd.EngineTemperature,
                        sd.Timestamp,
                        AlertType = sd.FuelLevel < 20 ? "fuel" : 
                                   sd.EngineTemperature > 100 ? "temperature" : "normal"
                    })
                    .OrderByDescending(sd => sd.Timestamp)
                    .ToListAsync();

                return CustomResults.Success<object>(alerts);
            }
            catch (Exception ex)
            {
                return CustomResults.Problem(Result.Failure(Error.Failure("Alerts.VehicleAlertsError", $"Error retrieving vehicle alerts: {ex.Message}")));
            }
        }
    }
}

