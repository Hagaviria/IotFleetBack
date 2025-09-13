using System;

namespace Domain.DTOs;

public record FuelStatisticsDTO(
    Guid VehicleId,
    double AverageFuelLevel,
    double MinFuelLevel,
    double MaxFuelLevel,
    double TotalFuelConsumed,
    double AverageConsumption,
    int DataPointsCount,
    DateTime PeriodStart,
    DateTime PeriodEnd
);
