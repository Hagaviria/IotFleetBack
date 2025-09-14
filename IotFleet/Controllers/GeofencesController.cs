using Microsoft.AspNetCore.Mvc;
using IotFleet.Infrastructure;
using SharedKernel;
using Microsoft.EntityFrameworkCore;
using Application.Abstractions.Data;

namespace IotFleet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GeofencesController(
        IApplicationDbContext context
        ) : ControllerBase
    {
        /// <summary>
        /// Gets all geofences.
        /// </summary>
        /// <returns>List of all geofences.</returns>
        [HttpGet]
        public async Task<IActionResult> GetGeofences()
        {
            try
            {
                var geofences = await context.Geofences
                    .AsNoTracking()
                    .Select(g => new
                    {
                        g.Id,
                        g.Name,
                        g.Center,
                        g.Radius,
                        g.Type,
                        g.IsActive,
                        g.CreatedAt,
                        g.UpdatedAt,
                        g.Description,
                        g.Color,
                        g.AlertOnEnter,
                        g.AlertOnExit
                    })
                    .ToListAsync();

                return CustomResults.Success<object>(geofences);
            }
            catch (Exception ex)
            {
                return CustomResults.Problem(Result.Failure(Error.Failure("Geofences.GetError", $"Error retrieving geofences: {ex.Message}")));
            }
        }

        /// <summary>
        /// Gets a specific geofence by ID.
        /// </summary>
        /// <param name="id">The geofence ID.</param>
        /// <returns>The geofence details.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGeofence(Guid id)
        {
            try
            {
                var geofence = await context.Geofences
                    .AsNoTracking()
                    .Where(g => g.Id == id)
                    .Select(g => new
                    {
                        g.Id,
                        g.Name,
                        g.Center,
                        g.Radius,
                        g.Type,
                        g.IsActive,
                        g.CreatedAt,
                        g.UpdatedAt,
                        g.Description,
                        g.Color,
                        g.AlertOnEnter,
                        g.AlertOnExit
                    })
                    .FirstOrDefaultAsync();

                if (geofence == null)
                {
                    return CustomResults.Problem(Result.Failure(Error.NotFound("Geofence.NotFound", "Geofence not found")));
                }

                return CustomResults.Success<object>(geofence);
            }
            catch (Exception ex)
            {
                return CustomResults.Problem(Result.Failure(Error.Failure("Geofences.GetError", $"Error retrieving geofence: {ex.Message}")));
            }
        }

        /// <summary>
        /// Creates a new geofence.
        /// </summary>
        /// <param name="request">The geofence creation request.</param>
        /// <returns>The created geofence.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateGeofence([FromBody] CreateGeofenceRequest request)
        {
            try
            {
                var geofence = new Domain.Models.Geofence
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Center = new Domain.Models.GeofenceCenter 
                    { 
                        Latitude = request.Center.Latitude, 
                        Longitude = request.Center.Longitude 
                    },
                    Radius = request.Radius,
                    Type = request.Type,
                    IsActive = request.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Description = request.Description,
                    Color = request.Color,
                    AlertOnEnter = request.AlertOnEnter,
                    AlertOnExit = request.AlertOnExit
                };

                context.Geofences.Add(geofence);
                await context.SaveChangesAsync(CancellationToken.None);

                var response = new
                {
                    geofence.Id,
                    geofence.Name,
                    geofence.Center,
                    geofence.Radius,
                    geofence.Type,
                    geofence.IsActive,
                    geofence.CreatedAt,
                    geofence.UpdatedAt,
                    geofence.Description,
                    geofence.Color,
                    geofence.AlertOnEnter,
                    geofence.AlertOnExit
                };

                return CustomResults.Success<object>(response);
            }
            catch (Exception ex)
            {
                return CustomResults.Problem(Result.Failure(Error.Failure("Geofences.CreateError", $"Error creating geofence: {ex.Message}")));
            }
        }

        /// <summary>
        /// Updates an existing geofence.
        /// </summary>
        /// <param name="id">The geofence ID.</param>
        /// <param name="request">The geofence update request.</param>
        /// <returns>The updated geofence.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGeofence(Guid id, [FromBody] UpdateGeofenceRequest request)
        {
            try
            {
                var geofence = await context.Geofences.FindAsync(id);
                if (geofence == null)
                {
                    return CustomResults.Problem(Result.Failure(Error.NotFound("Geofence.NotFound", "Geofence not found")));
                }

                geofence.Name = request.Name;
                geofence.Center = new Domain.Models.GeofenceCenter 
                { 
                    Latitude = request.Center.Latitude, 
                    Longitude = request.Center.Longitude 
                };
                geofence.Radius = request.Radius;
                geofence.Type = request.Type;
                geofence.IsActive = request.IsActive;
                geofence.UpdatedAt = DateTime.UtcNow;
                geofence.Description = request.Description;
                geofence.Color = request.Color;
                geofence.AlertOnEnter = request.AlertOnEnter;
                geofence.AlertOnExit = request.AlertOnExit;

                await context.SaveChangesAsync(CancellationToken.None);

                var response = new
                {
                    geofence.Id,
                    geofence.Name,
                    geofence.Center,
                    geofence.Radius,
                    geofence.Type,
                    geofence.IsActive,
                    geofence.CreatedAt,
                    geofence.UpdatedAt,
                    geofence.Description,
                    geofence.Color,
                    geofence.AlertOnEnter,
                    geofence.AlertOnExit
                };

                return CustomResults.Success<object>(response);
            }
            catch (Exception ex)
            {
                return CustomResults.Problem(Result.Failure(Error.Failure("Geofences.UpdateError", $"Error updating geofence: {ex.Message}")));
            }
        }

        /// <summary>
        /// Deletes a geofence.
        /// </summary>
        /// <param name="id">The geofence ID.</param>
        /// <returns>Success response.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGeofence(Guid id)
        {
            try
            {
                var geofence = await context.Geofences.FindAsync(id);
                if (geofence == null)
                {
                    return CustomResults.Problem(Result.Failure(Error.NotFound("Geofence.NotFound", "Geofence not found")));
                }

                context.Geofences.Remove(geofence);
                await context.SaveChangesAsync(CancellationToken.None);

                return CustomResults.Success<object>("Geofence deleted successfully");
            }
            catch (Exception ex)
            {
                return CustomResults.Problem(Result.Failure(Error.Failure("Geofences.DeleteError", $"Error deleting geofence: {ex.Message}")));
            }
        }
    }

    public class CreateGeofenceRequest
    {
        public string Name { get; set; } = string.Empty;
        public GeofenceCenter Center { get; set; } = new();
        public double Radius { get; set; }
        public string Type { get; set; } = "inclusion";
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public bool AlertOnEnter { get; set; } = true;
        public bool AlertOnExit { get; set; } = true;
    }

    public class UpdateGeofenceRequest
    {
        public string Name { get; set; } = string.Empty;
        public GeofenceCenter Center { get; set; } = new();
        public double Radius { get; set; }
        public string Type { get; set; } = "inclusion";
        public bool IsActive { get; set; } = true;
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
}
