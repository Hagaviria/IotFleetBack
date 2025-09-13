using System.Threading;
using System.Threading.Tasks;
using Domain.DTOs;

namespace Application.Abstractions.Services;

public interface IWebSocketService
{
    Task BroadcastAlert(FuelAlertDTO alert);
    Task BroadcastSensorData(SensorDataDTO sensorData);
}
