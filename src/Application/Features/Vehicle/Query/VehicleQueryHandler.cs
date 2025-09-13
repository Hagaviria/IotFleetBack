using Application.Abstractions.Data;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VehicleEntity = Domain.Models.Vehicle;

namespace Application.Features.Vehicle.Query
{
    public class VehicleQueryHandler(
        IApplicationDbContext context
        )
    {
        /// <summary>
        /// Gets a vehicle by their ID.
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with the vehicle if successful.</returns>
        public async Task<Result<VehicleEntity>> GetVehicleByIdAsync(string vehicleId, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(vehicleId, out var vehicleIdGuid))
                return Result.Failure<VehicleEntity>(Error.NotFound("Vehicle.NotFound", "Vehicle not found"));

            var vehicle = await context.Vehicles
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == vehicleIdGuid, cancellationToken);

            if (vehicle == null)
                return Result.Failure<VehicleEntity>(Error.NotFound("Vehicle.NotFound", "Vehicle not found"));

            return Result.Success(vehicle);
        }

        /// <summary>
        /// Gets all vehicles.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with a list of vehicles if successful.</returns>
        public async Task<Result<List<VehicleEntity>>> GetAllVehiclesAsync(CancellationToken cancellationToken)
        {
            var vehicles = await context.Vehicles
                .AsNoTracking()
                .OrderBy(v => v.LicensePlate)
                .ToListAsync(cancellationToken);

            return Result.Success(vehicles);
        }

        /// <summary>
        /// Gets vehicles by fleet ID.
        /// </summary>
        /// <param name="fleetId">The ID of the fleet.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with a list of vehicles if successful.</returns>
        public async Task<Result<List<VehicleEntity>>> GetVehiclesByFleetIdAsync(string fleetId, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(fleetId, out var fleetIdGuid))
                return Result.Failure<List<VehicleEntity>>(Error.Problem("Fleet.InvalidId", "Invalid fleet ID format"));

            var vehicles = await context.Vehicles
                .AsNoTracking()
                .Where(v => v.FleetId == fleetIdGuid)
                .OrderBy(v => v.LicensePlate)
                .ToListAsync(cancellationToken);

            return Result.Success(vehicles);
        }

        /// <summary>
        /// Gets a vehicle by license plate.
        /// </summary>
        /// <param name="licensePlate">The license plate of the vehicle.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with the vehicle if successful.</returns>
        public async Task<Result<VehicleEntity>> GetVehicleByLicensePlateAsync(string licensePlate, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(licensePlate))
                return Result.Failure<VehicleEntity>(Error.Problem("Vehicle.InvalidLicensePlate", "License plate cannot be empty"));

            var vehicle = await context.Vehicles
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.LicensePlate == licensePlate, cancellationToken);

            if (vehicle == null)
                return Result.Failure<VehicleEntity>(Error.NotFound("Vehicle.NotFound", "Vehicle not found"));

            return Result.Success(vehicle);
        }
    }
}
