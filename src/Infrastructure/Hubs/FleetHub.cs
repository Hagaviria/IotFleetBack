using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Hubs;

public class FleetHub : Hub
{
    public async Task JoinFleetGroup(string fleetId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Fleet_{fleetId}");
    }

    public async Task LeaveFleetGroup(string fleetId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Fleet_{fleetId}");
    }

    public async Task JoinVehicleGroup(string vehicleId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Vehicle_{vehicleId}");
    }

    public async Task LeaveVehicleGroup(string vehicleId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Vehicle_{vehicleId}");
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
