using Microsoft.AspNetCore.Mvc;
using IotFleet.Extensions;
using SharedKernel;
using IotFleet.Infrastructure;
using Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace IotFleet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController(IApplicationDbContext context) : ControllerBase
    {
        /// <summary>
        /// Basic health check endpoint.
        /// </summary>
        /// <returns>Basic health status.</returns>
        [HttpGet]
        public IActionResult GetHealth()
        {
            var health = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
            };

            return CustomResults.Success<object>(health);
        }

        /// <summary>
        /// Detailed health check including database connectivity.
        /// </summary>
        /// <returns>Detailed health status including database connectivity.</returns>
        [HttpGet("detailed")]
        public async Task<IActionResult> GetDetailedHealth()
        {
            try
            {
                var health = new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Version = "1.0.0",
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                    Services = new
                    {
                        Database = await CheckDatabaseHealth(),
                        Memory = GetMemoryHealth(),
                        Disk = GetDiskHealth()
                    }
                };

                return CustomResults.Success<object>(health);
            }
            catch (Exception ex)
            {
                var unhealthyHealth = new
                {
                    Status = "Unhealthy",
                    Timestamp = DateTime.UtcNow,
                    Error = ex.Message
                };

                return CustomResults.Problem(Result.Failure(Error.Failure("Health.CheckError", $"Health check failed: {ex.Message}")));
            }
        }

        /// <summary>
        /// Database connectivity check.
        /// </summary>
        /// <returns>Database health status.</returns>
        [HttpGet("database")]
        public async Task<IActionResult> GetDatabaseHealth()
        {
            try
            {
                var dbHealth = await CheckDatabaseHealth();
                return CustomResults.Success<object>(dbHealth);
            }
            catch (Exception ex)
            {
                return CustomResults.Problem(Result.Failure(Error.Failure("Health.DatabaseError", $"Database health check failed: {ex.Message}")));
            }
        }

        private async Task<object> CheckDatabaseHealth()
        {
            try
            {
                // Test database connectivity
                await context.Database.CanConnectAsync();
                
                // Get basic statistics
                var vehicleCount = await context.Vehicles.CountAsync();
                var sensorDataCount = await context.SensorData.CountAsync();
                var userCount = await context.Users.CountAsync();

                return new
                {
                    Status = "Connected",
                    VehicleCount = vehicleCount,
                    SensorDataCount = sensorDataCount,
                    UserCount = userCount,
                    LastChecked = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    Status = "Disconnected",
                    Error = ex.Message,
                    LastChecked = DateTime.UtcNow
                };
            }
        }

        private object GetMemoryHealth()
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var workingSet = process.WorkingSet64;
            var privateMemory = process.PrivateMemorySize64;

            return new
            {
                WorkingSetMB = Math.Round(workingSet / 1024.0 / 1024.0, 2),
                PrivateMemoryMB = Math.Round(privateMemory / 1024.0 / 1024.0, 2),
                LastChecked = DateTime.UtcNow
            };
        }

        private object GetDiskHealth()
        {
            try
            {
                var drive = new DriveInfo(Directory.GetCurrentDirectory());
                var totalSpace = drive.TotalSize;
                var freeSpace = drive.AvailableFreeSpace;
                var usedSpace = totalSpace - freeSpace;

                return new
                {
                    TotalSpaceGB = Math.Round(totalSpace / 1024.0 / 1024.0 / 1024.0, 2),
                    FreeSpaceGB = Math.Round(freeSpace / 1024.0 / 1024.0 / 1024.0, 2),
                    UsedSpaceGB = Math.Round(usedSpace / 1024.0 / 1024.0 / 1024.0, 2),
                    UsagePercentage = Math.Round((double)usedSpace / totalSpace * 100, 2),
                    LastChecked = DateTime.UtcNow
                };
            }
            catch
            {
                return new
                {
                    Status = "Unable to retrieve disk information",
                    LastChecked = DateTime.UtcNow
                };
            }
        }
    }
}

