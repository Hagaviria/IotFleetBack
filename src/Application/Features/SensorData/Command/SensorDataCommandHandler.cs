using Application.Abstractions.Data;
using Domain.Models;
using Domain.Errors;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.SensorData.Command
{
    public class SensorDataCommandHandler(
        IApplicationDbContext context
        )
    {
        public async Task<Result<Guid>> IngestSensorDataAsync(IngestSensorDataCommand command, CancellationToken cancellationToken)
        {
            var vehicle = await context.Vehicles
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == command.VehicleId, cancellationToken);
            
            if (vehicle is null)
            {
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
            await context.SaveChangesAsync(cancellationToken);

            return Result.Success(sensorData.Id);
        }

        public async Task<Result<Domain.Models.SensorData>> UpdateSensorDataAsync(string id, UpdateSensorDataCommand command, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(id, out var sensorDataId))
            {
                return Result.Failure<Domain.Models.SensorData>(Error.NotFound("SensorData.NotFound", "Sensor data not found"));
            }

            var existingSensorData = await context.SensorData
                .FirstOrDefaultAsync(sd => sd.Id == sensorDataId, cancellationToken);

            if (existingSensorData is null)
            {
                return Result.Failure<Domain.Models.SensorData>(Error.NotFound("SensorData.NotFound", "Sensor data not found"));
            }

            existingSensorData.Latitude = command.Latitude;
            existingSensorData.Longitude = command.Longitude;
            existingSensorData.Altitude = command.Altitude ?? 0;
            existingSensorData.Speed = command.Speed ?? 0;
            existingSensorData.FuelLevel = command.FuelLevel ?? 0;
            existingSensorData.FuelConsumption = command.FuelConsumption ?? 0;
            existingSensorData.EngineTemperature = command.EngineTemperature ?? 0;
            existingSensorData.AmbientTemperature = command.AmbientTemperature ?? 0;

            await context.SaveChangesAsync(cancellationToken);

            return Result.Success(existingSensorData);
        }

        public async Task<Result> DeleteSensorDataAsync(string id, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(id, out var sensorDataId))
            {
                return Result.Failure(Error.NotFound("SensorData.NotFound", "Sensor data not found"));
            }

            var sensorData = await context.SensorData
                .FirstOrDefaultAsync(sd => sd.Id == sensorDataId, cancellationToken);

            if (sensorData is null)
            {
                return Result.Failure(Error.NotFound("SensorData.NotFound", "Sensor data not found"));
            }

            context.SensorData.Remove(sensorData);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
