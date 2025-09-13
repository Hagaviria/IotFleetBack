using System;
using System.Collections.Generic;
using Application.Abstractions.Messaging;
using Domain.DTOs;
using SharedKernel;

namespace Application.Features.SensorData.Query.GetFuelAlerts;

public record GetFuelAlertsQuery(
    Guid? FleetId = null,
    Guid? VehicleId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? Severity = null
) : IQuery<Result<List<FuelAlertDTO>>>;
