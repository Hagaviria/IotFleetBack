using Application.Abstractions.Data;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using System;
using System.Threading;
using System.Threading.Tasks;
using VehicleEntity = Domain.Models.Vehicle;

namespace Application.Features.Vehicle.Command
{
    public class VehicleCommandHandler(
        IApplicationDbContext context
        )
    {
        /// <summary>
        /// Creates a new vehicle.
        /// </summary>
        /// <param name="command">The vehicle create command containing vehicle details.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with a string vehicle ID if successful.</returns>
        public async Task<Result<string>> CreateVehicleAsync(CreateVehicleCommand command, CancellationToken cancellationToken)
        {
            if (command == null)
                return Result.Failure<string>(Error.Problem("Vehicle.Null", "Vehicle cannot be null"));

            // Check if vehicle with same license plate already exists
            var existingVehicle = await context.Vehicles
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.LicensePlate == command.LicensePlate, cancellationToken);

            if (existingVehicle != null)
                return Result.Failure<string>(Error.Conflict("Vehicle.LicensePlateExists", "Vehicle with this license plate already exists"));

            var newVehicle = new VehicleEntity
            {
                Id = Guid.NewGuid(),
                LicensePlate = command.LicensePlate,
                Model = command.Model,
                Brand = command.Brand,
                FuelCapacity = command.FuelCapacity,
                AverageConsumption = command.AverageConsumption,
                FleetId = command.FleetId,
                CreatedAt = DateTime.UtcNow,
                LastMaintenance = command.LastMaintenance
            };

            context.Vehicles.Add(newVehicle);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Success(newVehicle.Id.ToString());
        }

        /// <summary>
        /// Updates an existing vehicle.
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle to update.</param>
        /// <param name="command">The vehicle update command with updated details.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with the updated vehicle details if successful.</returns>
        public async Task<Result<VehicleEntity>> UpdateVehicleAsync(string vehicleId, UpdateVehicleCommand command, CancellationToken cancellationToken)
        {
            if (command == null)
                return Result.Failure<VehicleEntity>(Error.Problem("Vehicle.Null", "Vehicle cannot be null"));

            if (!Guid.TryParse(vehicleId, out var vehicleIdGuid))
                return Result.Failure<VehicleEntity>(Error.Problem("Vehicle.InvalidId", "Invalid vehicle ID format"));

            var existingVehicle = await context.Vehicles
                .FirstOrDefaultAsync(v => v.Id == vehicleIdGuid, cancellationToken);

            if (existingVehicle == null)
                return Result.Failure<VehicleEntity>(Error.NotFound("Vehicle.NotFound", "Vehicle not found"));

            // Check if another vehicle with same license plate exists (excluding current vehicle)
            var duplicateVehicle = await context.Vehicles
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.LicensePlate == command.LicensePlate && v.Id != vehicleIdGuid, cancellationToken);

            if (duplicateVehicle != null)
                return Result.Failure<VehicleEntity>(Error.Conflict("Vehicle.LicensePlateExists", "Another vehicle with this license plate already exists"));

            existingVehicle.LicensePlate = command.LicensePlate;
            existingVehicle.Model = command.Model;
            existingVehicle.Brand = command.Brand;
            existingVehicle.FuelCapacity = command.FuelCapacity;
            existingVehicle.AverageConsumption = command.AverageConsumption;
            existingVehicle.FleetId = command.FleetId;
            existingVehicle.LastMaintenance = command.LastMaintenance;

            await context.SaveChangesAsync(cancellationToken);

            return Result.Success(existingVehicle);
        }

        /// <summary>
        /// Deletes a vehicle by their ID.
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle to delete.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a Result object indicating success or failure.</returns>
        public async Task<Result> DeleteVehicleAsync(string vehicleId, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(vehicleId, out var vehicleIdGuid))
                return Result.Failure(Error.Problem("Vehicle.InvalidId", "Invalid vehicle ID format"));

            var vehicle = await context.Vehicles
                .FirstOrDefaultAsync(v => v.Id == vehicleIdGuid, cancellationToken);

            if (vehicle == null)
                return Result.Failure(Error.NotFound("Vehicle.NotFound", "Vehicle not found"));

            // Check if vehicle has sensor data
            var hasSensorData = await context.SensorData
                .AsNoTracking()
                .AnyAsync(sd => sd.VehicleId == vehicleIdGuid, cancellationToken);

            if (hasSensorData)
                return Result.Failure(Error.Conflict("Vehicle.HasSensorData", "Cannot delete vehicle that has sensor data. Delete sensor data first."));

            context.Vehicles.Remove(vehicle);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
