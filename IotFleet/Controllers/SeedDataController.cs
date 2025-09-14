using Microsoft.AspNetCore.Mvc;
using IotFleet.Extensions;
using SharedKernel;
using IotFleet.Infrastructure;
using Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore;
using Domain.Models;
using Application.Abstractions.Authentication;
using System.Security.Cryptography;
using System.Text;

namespace IotFleet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedDataController(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher
        ) : ControllerBase
    {
        /// <summary>
        /// Seeds the database with initial data for testing.
        /// </summary>
        /// <returns>Success message.</returns>
        [HttpPost]
        public async Task<IActionResult> SeedData()
        {
            try
            {
                // Verificar si ya hay datos
                var hasUsers = await context.Users.AnyAsync();
                if (hasUsers)
                {
                    return CustomResults.Success<object>(new { message = "Database already seeded" });
                }

                // Crear usuarios de prueba
                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    Identificacion = "12345678",
                    Nombre_completo = "Admin User",
                    Correo = "admin@iotfleet.com",
                    PasswordHash = passwordHasher.Hash("admin123"),
                    Id_perfil = 1,
                    Nombre_perfil = "Admin",
                    Estado = true,
                    Creado_en = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Direccion = "Calle Admin 123",
                    TelefonoFijo = "6012345678",
                    TelefonoCelular = "3001234567",
                    TipoIdentificacion = "CC"
                };

                var operatorUser = new User
                {
                    Id = Guid.NewGuid(),
                    Identificacion = "87654321",
                    Nombre_completo = "Operator User",
                    Correo = "operator@iotfleet.com",
                    PasswordHash = passwordHasher.Hash("operator123"),
                    Id_perfil = 2,
                    Nombre_perfil = "User",
                    Estado = true,
                    Creado_en = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Direccion = "Calle Operator 456",
                    TelefonoFijo = "6018765432",
                    TelefonoCelular = "3008765432",
                    TipoIdentificacion = "CC"
                };

                context.Users.AddRange(adminUser, operatorUser);

                // Crear flota de prueba
                var fleetId = Guid.NewGuid();

                // Crear vehículos de prueba
                var vehicles = new List<Vehicle>
                {
                    new Vehicle
                    {
                        Id = Guid.NewGuid(),
                        LicensePlate = "ABC-123",
                        Model = "Sprinter",
                        Brand = "Mercedes",
                        FuelCapacity = 75.0,
                        AverageConsumption = 8.5,
                        FleetId = fleetId,
                        CreatedAt = DateTime.UtcNow,
                        LastMaintenance = DateTime.UtcNow.AddDays(-30)
                    },
                    new Vehicle
                    {
                        Id = Guid.NewGuid(),
                        LicensePlate = "DEF-456",
                        Model = "Transit",
                        Brand = "Ford",
                        FuelCapacity = 80.0,
                        AverageConsumption = 9.2,
                        FleetId = fleetId,
                        CreatedAt = DateTime.UtcNow,
                        LastMaintenance = DateTime.UtcNow.AddDays(-15)
                    },
                    new Vehicle
                    {
                        Id = Guid.NewGuid(),
                        LicensePlate = "GHI-789",
                        Model = "Master",
                        Brand = "Renault",
                        FuelCapacity = 70.0,
                        AverageConsumption = 7.8,
                        FleetId = fleetId,
                        CreatedAt = DateTime.UtcNow,
                        LastMaintenance = DateTime.UtcNow.AddDays(-7)
                    }
                };

                context.Vehicles.AddRange(vehicles);

                // Crear datos de sensores de prueba
                var sensorData = new List<SensorData>();
                var random = new Random();

                foreach (var vehicle in vehicles)
                {
                    // Crear datos históricos para los últimos 7 días
                    for (int day = 0; day < 7; day++)
                    {
                        var baseDate = DateTime.UtcNow.AddDays(-day);
                        
                        // Crear 24 registros por día (uno por hora)
                        for (int hour = 0; hour < 24; hour++)
                        {
                            var timestamp = baseDate.AddHours(hour);
                            
                            // Simular ubicaciones en Bogotá, Colombia
                            var latitude = 4.6097 + (random.NextDouble() - 0.5) * 0.1; // ±0.05 grados
                            var longitude = -74.0817 + (random.NextDouble() - 0.5) * 0.1; // ±0.05 grados
                            
                            // Simular nivel de combustible decreciente
                            var fuelLevel = Math.Max(5, 100 - (day * 15) - (hour * 2) + random.Next(-5, 5));
                            
                            // Simular velocidad basada en la hora del día
                            var speed = hour >= 6 && hour <= 9 || hour >= 17 && hour <= 19 
                                ? random.Next(20, 40) // Hora pico
                                : random.Next(40, 80); // Hora normal
                            
                            // Simular temperatura del motor
                            var engineTemperature = 85 + random.Next(-10, 15);
                            
                            sensorData.Add(new SensorData
                            {
                                Id = Guid.NewGuid(),
                                VehicleId = vehicle.Id,
                                Latitude = latitude,
                                Longitude = longitude,
                                Altitude = 2600 + random.Next(-100, 100), // Altitud de Bogotá
                                Speed = speed,
                                FuelLevel = fuelLevel,
                                FuelConsumption = vehicle.AverageConsumption * (speed / 60.0) * (1 + random.NextDouble() * 0.3),
                                EngineTemperature = engineTemperature,
                                AmbientTemperature = 18 + random.Next(-5, 10), // Temperatura de Bogotá
                                Timestamp = timestamp
                            });
                        }
                    }
                }

                context.SensorData.AddRange(sensorData);

                await context.SaveChangesAsync(CancellationToken.None);

                return CustomResults.Success<object>(new 
                { 
                    message = "Database seeded successfully",
                    usersCreated = 2,
                    vehiclesCreated = vehicles.Count,
                    sensorDataCreated = sensorData.Count,
                    adminCredentials = new { email = "admin@iotfleet.com", password = "admin123" },
                    operatorCredentials = new { email = "operator@iotfleet.com", password = "operator123" }
                });
            }
            catch (Exception ex)
            {
                return CustomResults.Problem(Result.Failure(Error.Failure("SeedData.Error", $"Error seeding database: {ex.Message}")));
            }
        }

        /// <summary>
        /// Clears all data from the database.
        /// </summary>
        /// <returns>Success message.</returns>
        [HttpDelete]
        public async Task<IActionResult> ClearData()
        {
            try
            {
                // Eliminar datos en orden correcto debido a las foreign keys
                context.SensorData.RemoveRange(context.SensorData);
                context.Vehicles.RemoveRange(context.Vehicles);
                context.Users.RemoveRange(context.Users);

                await context.SaveChangesAsync(CancellationToken.None);

                return CustomResults.Success<object>(new { message = "Database cleared successfully" });
            }
            catch (Exception ex)
            {
                return CustomResults.Problem(Result.Failure(Error.Failure("ClearData.Error", $"Error clearing database: {ex.Message}")));
            }
        }
    }
}
