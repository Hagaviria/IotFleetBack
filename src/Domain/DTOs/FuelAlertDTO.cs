using System;

namespace Domain.DTOs;

public record FuelAlertDTO(
    Guid VehicleId,
    string LicensePlate,
    double FuelLevel,
    double EstimatedAutonomyHours,
    DateTime Timestamp,
    string Severity,
    string Message,
    double RemainingDistance,
    string AlertType = "fuel"
);

