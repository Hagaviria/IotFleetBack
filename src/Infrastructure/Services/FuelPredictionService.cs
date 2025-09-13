using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Services;
using Domain.DTOs;
using Domain.Models;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

internal sealed class FuelPredictionService(ILogger<FuelPredictionService> logger) : IFuelPredictionService
{
    public async Task<FuelAlertDTO?> CalculateFuelAutonomy(
        Vehicle vehicle, 
        SensorData sensorData, 
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogDebug("Calculando autonomía de combustible para vehículo {VehicleId}", vehicle.Id);

            var currentFuelLiters = (sensorData.FuelLevel / 100) * vehicle.FuelCapacity;
            
            var currentConsumption = sensorData.FuelConsumption ?? 
                (vehicle.AverageConsumption * (sensorData.Speed ?? 0));
            
            if (currentConsumption <= 0)
                currentConsumption = vehicle.AverageConsumption * 50;
            
            var autonomyHours = currentConsumption > 0 ? currentFuelLiters / currentConsumption : 0;
            
            logger.LogDebug("Autonomía calculada: {AutonomyHours} horas para vehículo {VehicleId}", 
                autonomyHours, vehicle.Id);
            
            if (autonomyHours < 1.0)
            {
                var severity = autonomyHours < 0.5 ? "CRITICAL" : "LOW";
                
                logger.LogWarning("Alerta de combustible para vehículo {VehicleId}: {Severity} - {AutonomyHours} horas restantes", 
                    vehicle.Id, severity, autonomyHours);
                
                return new FuelAlertDTO(
                    vehicle.Id,
                    vehicle.LicensePlate,
                    sensorData.FuelLevel,
                    autonomyHours,
                    DateTime.UtcNow,
                    severity
                );
            }
            
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al calcular autonomía de combustible para vehículo {VehicleId}", vehicle.Id);
            return null;
        }
    }
}
