using System;
using System.Threading.Tasks;
using Application.Abstractions.Services;
using Domain.DTOs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

internal sealed class WebSocketService(
    IHubContext<Hub> hubContext,
    ILogger<WebSocketService> logger) : IWebSocketService
{
    public async Task BroadcastAlert(FuelAlertDTO alert)
    {
        try
        {
            logger.LogInformation("Enviando alerta de combustible via WebSocket para vehículo {VehicleId}", alert.VehicleId);
            
            await hubContext.Clients.Group("Admin").SendAsync("FuelAlert", alert);
            
            await hubContext.Clients.Group($"Vehicle_{alert.VehicleId}").SendAsync("FuelAlert", alert);
            
            logger.LogDebug("Alerta de combustible enviada exitosamente");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al enviar alerta de combustible via WebSocket para vehículo {VehicleId}", alert.VehicleId);
        }
    }
    
    public async Task BroadcastSensorData(SensorDataDTO sensorData)
    {
        try
        {
            logger.LogDebug("Enviando datos de sensor via WebSocket para vehículo {VehicleId}", sensorData.VehicleId);
            
            await hubContext.Clients.All.SendAsync("SensorData", sensorData);
            
            logger.LogDebug("Datos de sensor enviados exitosamente");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al enviar datos de sensor via WebSocket para vehículo {VehicleId}", sensorData.VehicleId);
        }
    }
}
