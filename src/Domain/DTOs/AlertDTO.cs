using System;

namespace Domain.DTOs;

public record AlertDTO(
    Guid Id,
    Guid VehicleId,
    string Type,
    string Severity,
    string Message,
    DateTime Timestamp,
    bool IsRead,
    string? AdditionalData
);