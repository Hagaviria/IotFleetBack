using System;

namespace Domain.DTOs;

public record SensorDataDTO(
    Guid VehicleId,
    double Latitude,
    double Longitude,
    double? Altitude,
    double? Speed,
    double FuelLevel,
    double? FuelConsumption,
    double EngineTemperature,
    double? AmbientTemperature,
    DateTime Timestamp
);
