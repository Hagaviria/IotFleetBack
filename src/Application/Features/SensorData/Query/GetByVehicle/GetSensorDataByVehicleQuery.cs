using System;
using Application.Abstractions.Messaging;
using Domain.DTOs;
using SharedKernel;

namespace Application.Features.SensorData.Query.GetByVehicle;

public record GetSensorDataByVehicleQuery(
    Guid VehicleId,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Page = 1,
    int PageSize = 50
) : IQuery<Result<PagedResult<SensorDataDTO>>>;
