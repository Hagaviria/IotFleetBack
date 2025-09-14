using System;

namespace Domain.Models;

public class Geofence
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public GeofenceCenter Center { get; set; } = new();
    public double Radius { get; set; }
    public string Type { get; set; } = "inclusion"; // "inclusion" or "exclusion"
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? Description { get; set; }
    public string? Color { get; set; }
    public bool AlertOnEnter { get; set; } = true;
    public bool AlertOnExit { get; set; } = true;
}

public class GeofenceCenter
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

