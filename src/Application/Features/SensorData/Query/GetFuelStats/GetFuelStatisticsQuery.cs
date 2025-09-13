using System;
using Application.Abstractions.Messaging;
using Domain.DTOs;
using SharedKernel;

namespace Application.Features.SensorData.Query.GetFuelStats;

public record GetFuelStatisticsQuery(
    Guid VehicleId,
    DateTime FromDate,
    DateTime ToDate
) : IQuery<Result<FuelStatisticsDTO>>;
