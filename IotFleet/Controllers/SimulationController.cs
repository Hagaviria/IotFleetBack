using Microsoft.AspNetCore.Mvc;
using IotFleet.Extensions;
using SharedKernel;
using Infrastructure.Services;
using Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore;
using IotFleet.Infrastructure;

namespace IotFleet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SimulationController(
        IVehicleSimulationService simulationService,
        IApplicationDbContext context
        ) : ControllerBase
    {
        /// <summary>
        /// Starts the vehicle simulation.
        /// </summary>
        /// <returns>Success message.</returns>
        [HttpPost("start")]
        public async Task<IActionResult> StartSimulation()
        {
            try
            {
                await simulationService.StartSimulationAsync();
                return CustomResults.Success<object>(new { message = "Simulation started successfully" });
            }
            catch (Exception ex)
            {
                return CustomResults.Problem(Result.Failure(Error.Failure("Simulation.StartError", $"Error starting simulation: {ex.Message}")));
            }
        }

        /// <summary>
        /// Stops the vehicle simulation.
        /// </summary>
        /// <returns>Success message.</returns>
        [HttpPost("stop")]
        public async Task<IActionResult> StopSimulation()
        {
            try
            {
                await simulationService.StopSimulationAsync();
                return CustomResults.Success<object>(new { message = "Simulation stopped successfully" });
            }
            catch (Exception ex)
            {
                return CustomResults.Problem(Result.Failure(Error.Failure("Simulation.StopError", $"Error stopping simulation: {ex.Message}")));
            }
        }

        /// <summary>
        /// Gets the current simulation status.
        /// </summary>
        /// <returns>Simulation status information.</returns>
        [HttpGet("status")]
        public async Task<IActionResult> GetSimulationStatus()
        {
            try
            {
                var isRunning = await simulationService.IsSimulationRunningAsync();
                var vehicles = await simulationService.GetSimulatedVehiclesAsync();
                
                var status = new
                {
                    IsRunning = isRunning,
                    VehicleCount = vehicles.Count,
                    LastUpdate = DateTime.UtcNow
                };

                return CustomResults.Success<object>(status);
            }
            catch (Exception ex)
            {
                return CustomResults.Problem(Result.Failure(Error.Failure("Simulation.StatusError", $"Error getting simulation status: {ex.Message}")));
            }
        }

        /// <summary>
        /// Gets real-time sensor data for all vehicles.
        /// </summary>
        /// <returns>Latest sensor data for all vehicles.</returns>
        [HttpGet("realtime-data")]
        public async Task<IActionResult> GetRealTimeData()
        {
            try
            {
                var vehicles = await context.Vehicles.ToListAsync();
                var realTimeData = new List<object>();

                foreach (var vehicle in vehicles)
                {
                    var latestSensorData = await context.SensorData
                        .Where(s => s.VehicleId == vehicle.Id)
                        .OrderByDescending(s => s.Timestamp)
                        .FirstOrDefaultAsync();

                    if (latestSensorData != null)
                    {
                        realTimeData.Add(new
                        {
                            VehicleId = vehicle.Id,
                            LicensePlate = vehicle.LicensePlate,
                            Model = vehicle.Model,
                            Brand = vehicle.Brand,
                            Latitude = latestSensorData.Latitude,
                            Longitude = latestSensorData.Longitude,
                            Speed = latestSensorData.Speed,
                            FuelLevel = latestSensorData.FuelLevel,
                            FuelConsumption = latestSensorData.FuelConsumption,
                            EngineTemperature = latestSensorData.EngineTemperature,
                            AmbientTemperature = latestSensorData.AmbientTemperature,
                            Altitude = latestSensorData.Altitude,
                            Timestamp = latestSensorData.Timestamp
                        });
                    }
                }

                return CustomResults.Success<object>(realTimeData);
            }
            catch (Exception ex)
            {
                return CustomResults.Problem(Result.Failure(Error.Failure("Simulation.RealTimeDataError", $"Error getting real-time data: {ex.Message}")));
            }
        }

        /// <summary>
        /// Gets real-time sensor data for a specific vehicle.
        /// </summary>
        /// <param name="vehicleId">The vehicle ID.</param>
        /// <returns>Latest sensor data for the specified vehicle.</returns>
        [HttpGet("realtime-data/{vehicleId}")]
        public async Task<IActionResult> GetRealTimeDataForVehicle(Guid vehicleId)
        {
            try
            {
                var vehicle = await context.Vehicles
                    .FirstOrDefaultAsync(v => v.Id == vehicleId);

                if (vehicle == null)
                {
                    return CustomResults.Problem(Result.Failure(Error.NotFound("Vehicle.NotFound", "Vehicle not found")));
                }

                var latestSensorData = await context.SensorData
                    .Where(s => s.VehicleId == vehicleId)
                    .OrderByDescending(s => s.Timestamp)
                    .FirstOrDefaultAsync();

                if (latestSensorData == null)
                {
                    return CustomResults.Problem(Result.Failure(Error.NotFound("SensorData.NotFound", "No sensor data found for this vehicle")));
                }

                var realTimeData = new
                {
                    VehicleId = vehicle.Id,
                    LicensePlate = vehicle.LicensePlate,
                    Model = vehicle.Model,
                    Brand = vehicle.Brand,
                    Latitude = latestSensorData.Latitude,
                    Longitude = latestSensorData.Longitude,
                    Speed = latestSensorData.Speed,
                    FuelLevel = latestSensorData.FuelLevel,
                    FuelConsumption = latestSensorData.FuelConsumption,
                    EngineTemperature = latestSensorData.EngineTemperature,
                    AmbientTemperature = latestSensorData.AmbientTemperature,
                    Altitude = latestSensorData.Altitude,
                    Timestamp = latestSensorData.Timestamp
                };

                return CustomResults.Success<object>(realTimeData);
            }
            catch (Exception ex)
            {
                return CustomResults.Problem(Result.Failure(Error.Failure("Simulation.RealTimeDataError", $"Error getting real-time data for vehicle: {ex.Message}")));
            }
        }

        /// <summary>
        /// Gets diagnostic information about the simulation system.
        /// </summary>
        /// <returns>Diagnostic information including vehicles, sensor data, and simulation status.</returns>
        [HttpGet("diagnostics")]
        public async Task<IActionResult> GetDiagnostics()
        {
            try
            {
                var vehicles = await context.Vehicles.ToListAsync();
                var totalSensorData = await context.SensorData.CountAsync();
                var latestSensorData = await context.SensorData
                    .OrderByDescending(s => s.Timestamp)
                    .FirstOrDefaultAsync();
                
                var isRunning = await simulationService.IsSimulationRunningAsync();

                var diagnostics = new
                {
                    SimulationStatus = new
                    {
                        IsRunning = isRunning,
                        VehicleCount = vehicles.Count
                    },
                    DatabaseInfo = new
                    {
                        TotalVehicles = vehicles.Count,
                        TotalSensorData = totalSensorData,
                        LatestSensorDataTimestamp = latestSensorData?.Timestamp,
                        LatestSensorDataVehicleId = latestSensorData?.VehicleId
                    },
                    Vehicles = vehicles.Select(v => new
                    {
                        v.Id,
                        v.LicensePlate,
                        v.Model,
                        v.Brand,
                        v.CreatedAt
                    }).ToList()
                };

                return CustomResults.Success<object>(diagnostics);
            }
            catch (Exception ex)
            {
                return CustomResults.Problem(Result.Failure(Error.Failure("Simulation.DiagnosticsError", $"Error getting diagnostics: {ex.Message}")));
            }
        }
    }
}
