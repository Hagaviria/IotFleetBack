using System;
using Application.Abstractions.Messaging;
using Domain.DTOs;
using SharedKernel;

namespace Application.Features.SensorData.Query.GetByFleet;

public record GetSensorDataByFleetQuery(
    Guid FleetId,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Page = 1,
    int PageSize = 100
) : IQuery<Result<PagedResult<SensorDataDTO>>>;
