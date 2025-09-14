using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Infrastructure.Hubs;

public class FleetHub : Hub
{
    private readonly ILogger<FleetHub> _logger;

    public FleetHub(ILogger<FleetHub> logger)
    {
        _logger = logger;
    }

    public async Task JoinFleetGroup(string fleetId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Fleet_{fleetId}");
        _logger.LogInformation("User {ConnectionId} joined fleet group {FleetId}", Context.ConnectionId, fleetId);
    }

    public async Task LeaveFleetGroup(string fleetId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Fleet_{fleetId}");
        _logger.LogInformation("User {ConnectionId} left fleet group {FleetId}", Context.ConnectionId, fleetId);
    }

    public async Task JoinVehicleGroup(string vehicleId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Vehicle_{vehicleId}");
        _logger.LogInformation("User {ConnectionId} joined vehicle group {VehicleId}", Context.ConnectionId, vehicleId);
    }

    public async Task LeaveVehicleGroup(string vehicleId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Vehicle_{vehicleId}");
        _logger.LogInformation("User {ConnectionId} left vehicle group {VehicleId}", Context.ConnectionId, vehicleId);
    }

    public async Task JoinAdminGroup()
    {
        var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole == "Admin")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admin");
            _logger.LogInformation("Admin user {ConnectionId} joined admin group", Context.ConnectionId);
        }
    }

    public async Task LeaveAdminGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admin");
        _logger.LogInformation("User {ConnectionId} left admin group", Context.ConnectionId);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        
        // Unirse automáticamente al grupo de administradores si es admin
        var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole == "Admin")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admin");
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        
        if (exception != null)
        {
            _logger.LogError(exception, "Client disconnected with error: {ConnectionId}", Context.ConnectionId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}
