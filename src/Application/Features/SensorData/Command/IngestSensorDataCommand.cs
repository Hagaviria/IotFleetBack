using System;
using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Features.SensorData.Command;

public record IngestSensorDataCommand(
    Guid VehicleId,
    double Latitude,
    double Longitude,
    double? Altitude,
    double? Speed,
    double FuelLevel,
    double? FuelConsumption,
    double EngineTemperature,
    double? AmbientTemperature
) : ICommand<Result<Guid>>;
