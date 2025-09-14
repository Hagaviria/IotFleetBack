using Microsoft.AspNetCore.Mvc;
using IotFleet.Extensions;
using SharedKernel;
using Application.Features.SensorData.Command;
using Application.Features.SensorData.Query;
using IotFleet.Infrastructure;
using Application.Abstractions.Services;
using Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore;
using Domain.Models;

namespace IotFleet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SensorDataController(
        SensorDataCommandHandler sensorDataCommand,
        SensorDataQueryHandler sensorDataQuery,
        IFuelPredictionService fuelPredictionService,
        IWebSocketService webSocketService,
        IApplicationDbContext context
        ) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetSensorData()
        {
            var result = await sensorDataQuery.GetAllSensorDataAsync(new CancellationToken());
            return result.Match(
                value => CustomResults.Success(value),
                CustomResults.Problem
            );
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSensorDataById(string id)
        {
            var result = await sensorDataQuery.GetSensorDataByIdAsync(id, new CancellationToken());
            return result.Match(
                value => CustomResults.Success(value),
                CustomResults.Problem
            );
        }

        [HttpPost]
        public async Task<IActionResult> CreateSensorData([FromBody] IngestSensorDataCommand sensorData)
        {
            if (!ModelState.IsValid)
                return CustomResults.Problem(Result.Failure(Error.Problem("General.ModelInvalid", ModelState.SerializeModelStateErrors())));

            var result = await sensorDataCommand.IngestSensorDataAsync(sensorData, new CancellationToken());
            
            if (result.IsSuccess)
            {
                var vehicle = await context.Vehicles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(v => v.Id == sensorData.VehicleId);

                if (vehicle != null)
                {
                    var sensorDataModel = new SensorData
                    {
                        VehicleId = sensorData.VehicleId,
                        Latitude = sensorData.Latitude,
                        Longitude = sensorData.Longitude,
                        Altitude = sensorData.Altitude,
                        Speed = sensorData.Speed,
                        FuelLevel = sensorData.FuelLevel,
                        FuelConsumption = sensorData.FuelConsumption,
                        EngineTemperature = sensorData.EngineTemperature,
                        AmbientTemperature = sensorData.AmbientTemperature,
                        Timestamp = DateTime.UtcNow
                    };

                    var fuelAlert = await fuelPredictionService.CalculateFuelAutonomy(
                        vehicle, 
                        sensorDataModel, 
                        new CancellationToken());

                    if (fuelAlert != null)
                    {
                        await webSocketService.BroadcastAlert(fuelAlert);
                    }

                    var sensorDataDto = new Domain.DTOs.SensorDataDTO(
                        sensorData.VehicleId,
                        sensorData.Latitude,
                        sensorData.Longitude,
                        sensorData.Altitude,
                        sensorData.Speed,
                        sensorData.FuelLevel,
                        sensorData.FuelConsumption,
                        sensorData.EngineTemperature,
                        sensorData.AmbientTemperature,
                        DateTime.UtcNow
                    );
                    
                    await webSocketService.BroadcastSensorData(sensorDataDto);
                }
            }

            return result.Match(
                value => CustomResults.Success(title: "SensorData.Created",
                result: value, status: StatusCodes.Status201Created),
                CustomResults.Problem
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSensorData(string id, [FromBody] UpdateSensorDataCommand sensorData)
        {
            if (!ModelState.IsValid)
                return CustomResults.Problem(Result.Failure(Error.Problem("General.ModelInvalid", ModelState.SerializeModelStateErrors())));

            var result = await sensorDataCommand.UpdateSensorDataAsync(id, sensorData, new CancellationToken());
            return result.Match(
                value => CustomResults.Success(value, title: "SensorData.Updated"),
                CustomResults.Problem
            );
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSensorData(string id)
        {
            var result = await sensorDataCommand.DeleteSensorDataAsync(id, new CancellationToken());
            return result.Match(
                () => CustomResults.Success<object>(result: null, title: "SensorData.Deleted", status: StatusCodes.Status202Accepted),
                CustomResults.Problem
            );
        }
    }
}
