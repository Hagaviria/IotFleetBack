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
    public Task<FuelAlertDTO?> CalculateFuelAutonomy(
        Vehicle vehicle, 
        SensorData sensorData, 
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogDebug("Calculando autonomía de combustible para vehículo {VehicleId}", vehicle.Id);

            // Calcular combustible actual en litros
            var currentFuelLiters = (sensorData.FuelLevel / 100) * vehicle.FuelCapacity;
            
            // Calcular consumo actual basado en velocidad y consumo promedio
            var currentConsumption = CalculateCurrentConsumption(vehicle, sensorData);
            
            // Calcular autonomía en horas
            var autonomyHours = currentConsumption > 0 ? currentFuelLiters / currentConsumption : 0;
            
            // Calcular distancia restante en km
            var remainingDistance = autonomyHours * (sensorData.Speed ?? 50); // Velocidad promedio si no hay datos
            
            logger.LogDebug("Autonomía calculada: {AutonomyHours} horas ({RemainingDistance} km) para vehículo {VehicleId}", 
                autonomyHours, remainingDistance, vehicle.Id);
            
            // Generar alerta si la autonomía es menor a 1 hora
            if (autonomyHours < 1.0)
            {
                var severity = DetermineSeverity(autonomyHours, sensorData.FuelLevel);
                var message = GenerateAlertMessage(autonomyHours, remainingDistance, severity);
                
                logger.LogWarning("Alerta de combustible para vehículo {VehicleId}: {Severity} - {AutonomyHours} horas restantes", 
                    vehicle.Id, severity, autonomyHours);
                
                return Task.FromResult<FuelAlertDTO?>(new FuelAlertDTO(
                    vehicle.Id,
                    vehicle.LicensePlate,
                    sensorData.FuelLevel,
                    autonomyHours,
                    DateTime.UtcNow,
                    severity,
                    message,
                    remainingDistance
                ));
            }
            
            return Task.FromResult<FuelAlertDTO?>(null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al calcular autonomía de combustible para vehículo {VehicleId}", vehicle.Id);
            return Task.FromResult<FuelAlertDTO?>(null);
        }
    }

    private double CalculateCurrentConsumption(Vehicle vehicle, SensorData sensorData)
    {
        // Si tenemos consumo directo del sensor, usarlo
        if (sensorData.FuelConsumption.HasValue && sensorData.FuelConsumption > 0)
        {
            return sensorData.FuelConsumption.Value;
        }

        // Calcular consumo basado en velocidad y consumo promedio del vehículo
        var speed = sensorData.Speed ?? 50; // Velocidad promedio si no hay datos
        
        // Factor de consumo basado en velocidad (más velocidad = más consumo)
        var speedFactor = Math.Max(0.5, Math.Min(2.0, speed / 60.0));
        
        // Consumo base del vehículo ajustado por velocidad
        var calculatedConsumption = vehicle.AverageConsumption * speedFactor;
        
        // Ajustar por temperatura del motor (temperatura alta = más consumo)
        if (sensorData.EngineTemperature > 90)
        {
            calculatedConsumption *= 1.2; // 20% más consumo si el motor está caliente
        }
        
        return calculatedConsumption;
    }

    private string DetermineSeverity(double autonomyHours, double fuelLevel)
    {
        if (autonomyHours < 0.25 || fuelLevel < 5)
            return "CRITICAL";
        else if (autonomyHours < 0.5 || fuelLevel < 10)
            return "HIGH";
        else if (autonomyHours < 0.75 || fuelLevel < 15)
            return "MEDIUM";
        else
            return "LOW";
    }

    private string GenerateAlertMessage(double autonomyHours, double remainingDistance, string severity)
    {
        var timeString = autonomyHours < 1 
            ? $"{(int)(autonomyHours * 60)} minutos"
            : $"{autonomyHours:F1} horas";
            
        return severity switch
        {
            "CRITICAL" => $"¡ALERTA CRÍTICA! Combustible muy bajo. Solo {timeString} de autonomía restante ({remainingDistance:F0} km).",
            "HIGH" => $"Alerta alta: Combustible bajo. Aproximadamente {timeString} de autonomía restante ({remainingDistance:F0} km).",
            "MEDIUM" => $"Alerta media: Nivel de combustible moderado. {timeString} de autonomía restante ({remainingDistance:F0} km).",
            "LOW" => $"Alerta baja: Nivel de combustible bajo. {timeString} de autonomía restante ({remainingDistance:F0} km).",
            _ => $"Alerta de combustible: {timeString} de autonomía restante ({remainingDistance:F0} km)."
        };
    }
}
