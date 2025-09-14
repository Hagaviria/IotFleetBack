using Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Services;

public interface IVehicleSimulationService
{
    Task StartSimulationAsync();
    Task StopSimulationAsync();
    Task<bool> IsSimulationRunningAsync();
    Task<List<Vehicle>> GetSimulatedVehiclesAsync();
    Task<SensorData> GetLatestSensorDataAsync(Guid vehicleId);
}
