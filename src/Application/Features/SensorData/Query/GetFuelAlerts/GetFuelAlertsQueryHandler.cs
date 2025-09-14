using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Domain.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.SensorData.Query.GetFuelAlerts;

internal sealed class GetFuelAlertsQueryHandler(
    IApplicationDbContext context,
    IFuelPredictionService fuelPredictionService) : IRequestHandler<GetFuelAlertsQuery, Result<List<FuelAlertDTO>>>
{
    public async Task<Result<List<FuelAlertDTO>>> Handle(
        GetFuelAlertsQuery query, 
        CancellationToken cancellationToken)
    {
        var vehiclesQuery = context.Vehicles.AsQueryable();
        
        if (query.FleetId.HasValue)
            vehiclesQuery = vehiclesQuery.Where(v => v.FleetId == query.FleetId.Value);
        
        if (query.VehicleId.HasValue)
            vehiclesQuery = vehiclesQuery.Where(v => v.Id == query.VehicleId.Value);

        var vehicles = await vehiclesQuery.ToListAsync(cancellationToken);
        var alerts = new List<FuelAlertDTO>();

        foreach (var vehicle in vehicles)
        {
            var latestSensorData = await context.SensorData
                .Where(s => s.VehicleId == vehicle.Id)
                .OrderByDescending(s => s.Timestamp)
                .FirstOrDefaultAsync(cancellationToken);

            if (latestSensorData is null) continue;

            var fuelAlert = await fuelPredictionService.CalculateFuelAutonomy(
                vehicle, latestSensorData, cancellationToken);

            if (fuelAlert is not null)
            {
                if (query.Severity is null || fuelAlert.Severity == query.Severity)
                {
                    if (query.FromDate is null || fuelAlert.Timestamp >= query.FromDate.Value)
                    {
                        if (query.ToDate is null || fuelAlert.Timestamp <= query.ToDate.Value)
                        {
                            alerts.Add(fuelAlert);
                        }
                    }
                }
            }
        }

        return alerts.OrderByDescending(a => a.Timestamp).ToList();
    }
}
