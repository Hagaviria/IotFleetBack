using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.DTOs;
using Domain.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Features.SensorData.Query.GetByVehicle;

internal sealed class GetSensorDataByVehicleQueryHandler(
    IApplicationDbContext context,
    ILogger<GetSensorDataByVehicleQueryHandler> logger) : IRequestHandler<GetSensorDataByVehicleQuery, Result<PagedResult<SensorDataDTO>>>
{
    public async Task<Result<PagedResult<SensorDataDTO>>> Handle(
        GetSensorDataByVehicleQuery query, 
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Consultando datos de sensor para vehículo {VehicleId}, página {Page}", 
                query.VehicleId, query.Page);

            var vehicleExists = await context.Vehicles
                .AsNoTracking()
                .AnyAsync(v => v.Id == query.VehicleId, cancellationToken);

            if (!vehicleExists)
            {
                logger.LogWarning("Vehículo {VehicleId} no encontrado", query.VehicleId);
                return Result.Failure<PagedResult<SensorDataDTO>>(VehicleErrors.NotFound);
            }

            var sensorDataQuery = context.SensorData
                .AsNoTracking()
                .Where(s => s.VehicleId == query.VehicleId);

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

            logger.LogInformation("Consulta completada: {Count} registros encontrados de {TotalCount} totales", 
                sensorData.Count, totalCount);

            return pagedResult;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al consultar datos de sensor para vehículo {VehicleId}", query.VehicleId);
            return Result.Failure<PagedResult<SensorDataDTO>>(
                Error.Failure("SensorData.QueryError", "Error interno al consultar los datos del sensor."));
        }
    }
}
