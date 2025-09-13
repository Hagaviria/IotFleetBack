using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.SensorData.Query.GetByFleet;

internal sealed class GetSensorDataByFleetQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetSensorDataByFleetQuery, Result<PagedResult<SensorDataDTO>>>
{
    public async Task<Result<PagedResult<SensorDataDTO>>> Handle(
        GetSensorDataByFleetQuery query, 
        CancellationToken cancellationToken)
    {
        var sensorDataQuery = context.SensorData
            .Include(s => s.Vehicle)
            .Where(s => s.Vehicle.FleetId == query.FleetId);

        if (query.FromDate.HasValue)
            sensorDataQuery = sensorDataQuery.Where(s => s.Timestamp >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            sensorDataQuery = sensorDataQuery.Where(s => s.Timestamp <= query.ToDate.Value);

        sensorDataQuery = sensorDataQuery.OrderByDescending(s => s.Timestamp);

        var totalCount = await sensorDataQuery.CountAsync(cancellationToken);
        
        var sensorData = await sensorDataQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(s => new SensorDataDTO(
                s.VehicleId,
                s.Latitude,
                s.Longitude,
                s.Altitude,
                s.Speed,
                s.FuelLevel,
                s.FuelConsumption,
                s.EngineTemperature,
                s.AmbientTemperature,
                s.Timestamp
            ))
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<SensorDataDTO>(
            sensorData,
            query.Page,
            query.PageSize,
            totalCount);

        return pagedResult;
    }
}
