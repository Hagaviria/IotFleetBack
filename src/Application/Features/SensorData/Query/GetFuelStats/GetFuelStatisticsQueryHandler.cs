using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.DTOs;
using Domain.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.SensorData.Query.GetFuelStats;

internal sealed class GetFuelStatisticsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetFuelStatisticsQuery, Result<FuelStatisticsDTO>>
{
    public async Task<Result<FuelStatisticsDTO>> Handle(
        GetFuelStatisticsQuery query, 
        CancellationToken cancellationToken)
    {
        var sensorData = await context.SensorData
            .Where(s => s.VehicleId == query.VehicleId && 
                       s.Timestamp >= query.FromDate && 
                       s.Timestamp <= query.ToDate)
            .OrderBy(s => s.Timestamp)
            .ToListAsync(cancellationToken);

        if (!sensorData.Any())
            return Result.Failure<FuelStatisticsDTO>(SensorDataErrors.NoDataFound);

        var stats = new FuelStatisticsDTO(
            query.VehicleId,
            sensorData.Average(s => s.FuelLevel),
            sensorData.Min(s => s.FuelLevel),
            sensorData.Max(s => s.FuelLevel),
            sensorData.First().FuelLevel - sensorData.Last().FuelLevel,
            sensorData.Where(s => s.FuelConsumption.HasValue)
                     .Average(s => s.FuelConsumption!.Value),
            sensorData.Count,
            query.FromDate,
            query.ToDate
        );

        return stats;
    }
}
