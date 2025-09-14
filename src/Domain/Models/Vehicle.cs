using System;

namespace Domain.Models;

public class Vehicle
{
    public Guid Id { get; set; }
    public string LicensePlate { get; set; }
    public string Model { get; set; }
    public string Brand { get; set; }
    public double FuelCapacity { get; set; } 
    public double AverageConsumption { get; set; } 
    public Guid? FleetId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastMaintenance { get; set; }
}
