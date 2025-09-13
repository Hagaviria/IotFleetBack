using System.Threading;
using System.Threading.Tasks;
using Domain.DTOs;
using Domain.Models;

namespace Application.Abstractions.Services;

public interface IFuelPredictionService
{
    Task<FuelAlertDTO?> CalculateFuelAutonomy(
        Vehicle vehicle, 
        SensorData sensorData, 
        CancellationToken cancellationToken);
}
