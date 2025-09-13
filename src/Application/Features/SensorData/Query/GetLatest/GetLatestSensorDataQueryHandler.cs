using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.SensorData.Query.GetLatest;

internal sealed class GetLatestSensorDataQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetLatestSensorDataQuery, Result<SensorDataDTO?>>
{
    public async Task<Result<SensorDataDTO?>> Handle(
        GetLatestSensorDataQuery query, 
        CancellationToken cancellationToken)
    {
        var latestSensorData = await context.SensorData
            .Where(s => s.VehicleId == query.VehicleId)
            .OrderByDescending(s => s.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestSensorData is null)
            return Result.Success<SensorDataDTO?>(null);

        var sensorDataDTO = new SensorDataDTO(
            latestSensorData.VehicleId,
            latestSensorData.Latitude,
            latestSensorData.Longitude,
            latestSensorData.Altitude,
            latestSensorData.Speed,
            latestSensorData.FuelLevel,
            latestSensorData.FuelConsumption,
            latestSensorData.EngineTemperature,
            latestSensorData.AmbientTemperature,
            latestSensorData.Timestamp
        );

        return sensorDataDTO;
    }
}
