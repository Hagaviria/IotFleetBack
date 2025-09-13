using System;

namespace Domain.DTOs;

public record FuelAlertDTO(
    Guid VehicleId,
    string LicensePlate,
    double CurrentFuelLevel,
    double EstimatedAutonomyHours,
    DateTime AlertTimestamp,
    string Severity 
);