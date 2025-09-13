using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Domain.Models;
using Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Features.SensorData.Command;

internal sealed class IngestSensorDataCommandHandler(
    IApplicationDbContext context,
    IFuelPredictionService fuelPredictionService,
    IWebSocketService webSocketService,
    ILogger<IngestSensorDataCommandHandler> logger) 
{
    public async Task<Result<Guid>> Handle(IngestSensorDataCommand command, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Iniciando ingesta de datos de sensor para vehículo {VehicleId}", command.VehicleId);

            var vehicle = await context.Vehicles
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == command.VehicleId, cancellationToken);
            
            if (vehicle is null)
            {
                logger.LogWarning("Vehículo {VehicleId} no encontrado", command.VehicleId);
                return Result.Failure<Guid>(VehicleErrors.NotFound);
            }
            var sensorData = new Domain.Models.SensorData
            {
                Id = Guid.NewGuid(),
                VehicleId = command.VehicleId,
                Latitude = command.Latitude,
                Longitude = command.Longitude,
                Altitude = command.Altitude,
                Speed = command.Speed,
                FuelLevel = command.FuelLevel,
                FuelConsumption = command.FuelConsumption,
                EngineTemperature = command.EngineTemperature,
                AmbientTemperature = command.AmbientTemperature,
                Timestamp = DateTime.UtcNow
            };

            context.SensorData.Add(sensorData);

            var fuelAlert = await fuelPredictionService.CalculateFuelAutonomy(
                vehicle, sensorData, cancellationToken);

            if (fuelAlert is not null)
            {
                logger.LogWarning("Alerta de combustible detectada para vehículo {VehicleId}: {Severity}", 
                    command.VehicleId, fuelAlert.Severity);
                await webSocketService.BroadcastAlert(fuelAlert);
            }

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Datos de sensor guardados exitosamente con ID {SensorDataId}", sensorData.Id);
            return Result.Success(sensorData.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al procesar ingesta de datos de sensor para vehículo {VehicleId}", command.VehicleId);
            return Result.Failure<Guid>(Error.Failure("SensorData.ProcessingError", "Error interno al procesar los datos del sensor."));
        }
    }
}
