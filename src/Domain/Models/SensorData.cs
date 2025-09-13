using System;

namespace Domain.Models;

public class SensorData
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; }
    
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Altitude { get; set; }
    public double? Speed { get; set; } 
    
    public double FuelLevel { get; set; } 
    public double? FuelConsumption { get; set; } 
    
    public double EngineTemperature { get; set; } 
    public double? AmbientTemperature { get; set; } 
    
    public DateTime Timestamp { get; set; }
}