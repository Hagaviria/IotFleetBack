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

    }
}
