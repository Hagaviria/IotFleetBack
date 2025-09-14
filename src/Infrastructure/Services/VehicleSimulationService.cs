using Application.Abstractions.Data;
using Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using Infrastructure.Hubs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Services;

public class VehicleSimulationService : BackgroundService, IVehicleSimulationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VehicleSimulationService> _logger;
    private readonly Random _random = new();
    private bool _isSimulationRunning = false;
    private readonly List<VehicleSimulationData> _vehicleSimulations = new();
    private readonly Timer _simulationTimer;

    public VehicleSimulationService(
        IServiceProvider serviceProvider,
        ILogger<VehicleSimulationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _simulationTimer = new Timer(SimulateVehicles, null, Timeout.Infinite, Timeout.Infinite);
    }

    public async Task StartSimulationAsync()
    {
        if (_isSimulationRunning)
        {
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var vehicles = await context.Vehicles.ToListAsync();
        
        if (!vehicles.Any())
        {
            return;
        }

        _vehicleSimulations.Clear();
        foreach (var vehicle in vehicles)
        {
            var simulationData = new VehicleSimulationData
            {
                VehicleId = vehicle.Id,
                CurrentLatitude = 4.6097 + (_random.NextDouble() - 0.5) * 0.1,
                CurrentLongitude = -74.0817 + (_random.NextDouble() - 0.5) * 0.1,
                CurrentSpeed = _random.Next(20, 60),
                CurrentFuelLevel = _random.Next(30, 100),
                CurrentDirection = _random.Next(0, 360),
                RouteIndex = _random.Next(0, GetBogotaRoutes().Count),
                WaypointIndex = 0,
                WaypointProgress = 0.0,
                RouteDirection = _random.Next(0, 2) == 0 ? 1 : -1,
                LastUpdate = DateTime.UtcNow,
                Behavior = GetRandomBehavior()
            };
            _vehicleSimulations.Add(simulationData);
        }

        _isSimulationRunning = true;
        _simulationTimer.Change(0, 5000);
    }

    public async Task StopSimulationAsync()
    {
        if (!_isSimulationRunning)
        {
            return;
        }

        _isSimulationRunning = false;
        _simulationTimer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    public async Task<bool> IsSimulationRunningAsync()
    {
        return _isSimulationRunning;
    }

    public async Task<List<Vehicle>> GetSimulatedVehiclesAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        return await context.Vehicles.ToListAsync();
    }

    public async Task<SensorData> GetLatestSensorDataAsync(Guid vehicleId)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        
        return await context.SensorData
            .Where(s => s.VehicleId == vehicleId)
            .OrderByDescending(s => s.Timestamp)
            .FirstOrDefaultAsync();
    }

    private async void SimulateVehicles(object state)
    {
        if (!_isSimulationRunning) return;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            foreach (var simulation in _vehicleSimulations)
            {
                await SimulateVehicleMovement(simulation, context);
            }

                await context.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during vehicle simulation");
        }
    }

    private async Task SimulateVehicleMovement(VehicleSimulationData simulation, IApplicationDbContext context)
    {
        var routes = GetBogotaRoutes();
        var currentRoute = routes[simulation.RouteIndex];
        var waypoints = currentRoute.Waypoints;

        var currentWaypoint = waypoints[simulation.WaypointIndex];
        var nextWaypointIndex = simulation.WaypointIndex + simulation.RouteDirection;

        if (nextWaypointIndex < 0 || nextWaypointIndex >= waypoints.Count)
        {
            simulation.RouteDirection *= -1;
            simulation.WaypointIndex = Math.Max(0, Math.Min(waypoints.Count - 2, 
                simulation.WaypointIndex + simulation.RouteDirection));
            simulation.WaypointProgress = 0;
        }
        else
        {
            var nextWaypoint = waypoints[nextWaypointIndex];
            var segmentDistance = CalculateDistance(currentWaypoint, nextWaypoint);
            
            if (segmentDistance > 0)
            {
                var distanceKm = (simulation.CurrentSpeed * 5) / 3600;
                var progressChange = distanceKm / segmentDistance;
                simulation.WaypointProgress += progressChange;

                if (simulation.WaypointProgress >= 1)
                {
                    simulation.WaypointIndex = nextWaypointIndex;
                    simulation.WaypointProgress = 0;
                }
            }
        }

        var finalCurrentWaypoint = waypoints[simulation.WaypointIndex];
        var finalNextWaypointIndex = simulation.WaypointIndex + simulation.RouteDirection;
        
        if (finalNextWaypointIndex >= 0 && finalNextWaypointIndex < waypoints.Count)
        {
            var finalNextWaypoint = waypoints[finalNextWaypointIndex];
            
            simulation.CurrentLatitude = finalCurrentWaypoint.Lat + 
                (finalNextWaypoint.Lat - finalCurrentWaypoint.Lat) * simulation.WaypointProgress;
            simulation.CurrentLongitude = finalCurrentWaypoint.Lng + 
                (finalNextWaypoint.Lng - finalCurrentWaypoint.Lng) * simulation.WaypointProgress;
        }

        var newSpeed = CalculateRealisticSpeed(simulation);
        simulation.CurrentSpeed = newSpeed;

        var fuelConsumption = (newSpeed * 5) / 3600 * 0.1;
        simulation.CurrentFuelLevel = Math.Max(0, simulation.CurrentFuelLevel - fuelConsumption);

        var sensorData = new SensorData
        {
            Id = Guid.NewGuid(),
            VehicleId = simulation.VehicleId,
            Latitude = simulation.CurrentLatitude,
            Longitude = simulation.CurrentLongitude,
            Altitude = 2600 + (_random.NextDouble() - 0.5) * 20,
            Speed = simulation.CurrentSpeed,
            FuelLevel = simulation.CurrentFuelLevel,
            FuelConsumption = 8.5 + (_random.NextDouble() - 0.5) * 0.3,
            EngineTemperature = 85 + (_random.NextDouble() - 0.5) * 3,
            AmbientTemperature = 22 + (_random.NextDouble() - 0.5) * 2,
            Timestamp = DateTime.UtcNow
        };

        context.SensorData.Add(sensorData);
        simulation.LastUpdate = DateTime.UtcNow;

        await SendRealTimeUpdate(simulation.VehicleId, sensorData);

        var oldRecords = await context.SensorData
            .Where(s => s.VehicleId == simulation.VehicleId)
            .OrderByDescending(s => s.Timestamp)
            .Skip(1000)
            .ToListAsync();

        if (oldRecords.Any())
        {
            context.SensorData.RemoveRange(oldRecords);
        }
    }

    private async Task SendRealTimeUpdate(Guid vehicleId, SensorData sensorData)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<FleetHub>>();
            
            var locationData = new
            {
                Latitude = sensorData.Latitude,
                Longitude = sensorData.Longitude,
                Speed = sensorData.Speed,
                Heading = CalculateDirection(sensorData.Latitude, sensorData.Longitude)
            };

            var sensorDataUpdate = new
            {
                FuelLevel = sensorData.FuelLevel,
                Temperature = sensorData.EngineTemperature,
                EngineRpm = 2000 + _random.Next(-200, 200),
                BatteryVoltage = 12.5 + (_random.NextDouble() - 0.5) * 0.5
            };

            await hubContext.Clients.All.SendAsync("LocationUpdate", new
            {
                VehicleId = vehicleId.ToString(),
                Location = locationData,
                Timestamp = DateTime.UtcNow
            });

            await hubContext.Clients.All.SendAsync("SensorDataUpdate", new
            {
                VehicleId = vehicleId.ToString(),
                SensorData = sensorDataUpdate,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending real-time update for vehicle {VehicleId}", vehicleId);
        }
    }

    private double CalculateDirection(double lat1, double lng1)
    {
        return _random.Next(0, 360);
    }

    private double CalculateRealisticSpeed(VehicleSimulationData simulation)
    {
        var baseSpeed = 35.0;
        var behavior = simulation.Behavior;
        
        var targetSpeed = baseSpeed * behavior.SpeedMultiplier;
        var speedVariation = (_random.NextDouble() - 0.5) * 8;
        targetSpeed += speedVariation;

        if (_random.NextDouble() < behavior.StopProbability)
        {
            targetSpeed = Math.Max(0, targetSpeed - 15);
        }

        var speedDifference = targetSpeed - simulation.CurrentSpeed;
        var maxAcceleration = 5.0;
        var acceleration = Math.Sign(speedDifference) * Math.Min(Math.Abs(speedDifference), maxAcceleration * 0.1);
        
        var newSpeed = simulation.CurrentSpeed + acceleration;
        return Math.Max(0, Math.Min(60, newSpeed));
    }

    private double CalculateDistance((double Lat, double Lng) start, (double Lat, double Lng) end)
    {
        const double R = 6371; // Radio de la Tierra en km
        var dLat = (end.Lat - start.Lat) * Math.PI / 180;
        var dLng = (end.Lng - start.Lng) * Math.PI / 180;
        var a = Math.Sin(dLat/2) * Math.Sin(dLat/2) +
                Math.Cos(start.Lat * Math.PI / 180) * Math.Cos(end.Lat * Math.PI / 180) *
                Math.Sin(dLng/2) * Math.Sin(dLng/2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a));
        return R * c;
    }

    private DriverBehavior GetRandomBehavior()
    {
        var behaviors = new[]
        {
            new DriverBehavior { Type = "aggressive", SpeedMultiplier = 1.3, StopProbability = 0.05 },
            new DriverBehavior { Type = "normal", SpeedMultiplier = 1.0, StopProbability = 0.15 },
            new DriverBehavior { Type = "cautious", SpeedMultiplier = 0.7, StopProbability = 0.25 }
        };
        
        return behaviors[_random.Next(behaviors.Length)];
    }

    private List<RouteData> GetBogotaRoutes()
    {
        return new List<RouteData>
        {
            new RouteData
            {
                Waypoints = new List<(double Lat, double Lng)>
                {
                    (4.6097100, -74.0817500), // Centro (Plaza de Bolívar)
                    (4.6112000, -74.0815000), // Calle 19
                    (4.6127000, -74.0812500), // Calle 26
                    (4.6142000, -74.0810000), // Calle 32
                    (4.6157000, -74.0807500), // Calle 39
                    (4.6172000, -74.0805000), // Calle 45
                    (4.6187000, -74.0802500), // Calle 53
                    (4.6202000, -74.0800000), // Calle 60
                }
            },
            new RouteData
            {
                Waypoints = new List<(double Lat, double Lng)>
                {
                    (4.6000000, -74.0900000), // Oeste (Suba)
                    (4.6010000, -74.0880000), // Carrera 50
                    (4.6020000, -74.0860000), // Carrera 30
                    (4.6030000, -74.0840000), // Carrera 15
                    (4.6040000, -74.0820000), // Carrera 7
                    (4.6050000, -74.0800000), // Carrera 1
                    (4.6060000, -74.0780000), // Este
                }
            },
            new RouteData
            {
                Waypoints = new List<(double Lat, double Lng)>
                {
                    (4.6080000, -74.0880000), // Sur (Kennedy)
                    (4.6100000, -74.0860000), // Calle 26
                    (4.6120000, -74.0840000), // Calle 32
                    (4.6140000, -74.0820000), // Calle 39
                    (4.6160000, -74.0800000), // Calle 45
                    (4.6180000, -74.0780000), // Calle 53
                    (4.6200000, -74.0760000), // Norte
                }
            }
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await StopSimulationAsync();
        _simulationTimer?.Dispose();
        await base.StopAsync(cancellationToken);
    }
}

public class VehicleSimulationData
{
    public Guid VehicleId { get; set; }
    public double CurrentLatitude { get; set; }
    public double CurrentLongitude { get; set; }
    public double CurrentSpeed { get; set; }
    public double CurrentFuelLevel { get; set; }
    public double CurrentDirection { get; set; }
    public int RouteIndex { get; set; }
    public int WaypointIndex { get; set; }
    public double WaypointProgress { get; set; }
    public int RouteDirection { get; set; }
    public DateTime LastUpdate { get; set; }
    public DriverBehavior Behavior { get; set; }
}

public class DriverBehavior
{
    public string Type { get; set; } = string.Empty;
    public double SpeedMultiplier { get; set; }
    public double StopProbability { get; set; }
}

public class RouteData
{
    public List<(double Lat, double Lng)> Waypoints { get; set; } = new();
}
