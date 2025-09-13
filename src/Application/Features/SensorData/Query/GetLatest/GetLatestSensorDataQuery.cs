using System;
using Application.Abstractions.Messaging;
using Domain.DTOs;
using SharedKernel;

namespace Application.Features.SensorData.Query.GetLatest;

public record GetLatestSensorDataQuery(Guid VehicleId) 
    : IQuery<Result<SensorDataDTO?>>;
