using Microsoft.AspNetCore.Mvc;
using IotFleet.Extensions;
using SharedKernel;
using Application.Features.Vehicle.Command;
using Application.Features.Vehicle.Query;
using IotFleet.Infrastructure;

namespace IotFleet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleController(
        VehicleCommandHandler vehicleCommand,
        VehicleQueryHandler vehicleQuery
        ) : ControllerBase
    {
        /// <summary>
        /// Gets all vehicles.
        /// </summary>
        /// <returns>A list of all vehicles.</returns>
        [HttpGet]
        public async Task<IActionResult> GetVehicles()
        {
            var result = await vehicleQuery.GetAllVehiclesAsync(new CancellationToken());
            return result.Match(
                value => CustomResults.Success<object>(value),
                CustomResults.Problem
            );
        }

        /// <summary>
        /// Gets a vehicle by ID.
        /// </summary>
        /// <param name="id">The vehicle ID.</param>
        /// <returns>The vehicle with the specified ID.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicle(string id)
        {
            var result = await vehicleQuery.GetVehicleByIdAsync(id, new CancellationToken());
            return result.Match(
                value => CustomResults.Success<object>(value),
                CustomResults.Problem
            );
        }

        /// <summary>
        /// Gets vehicles by fleet ID.
        /// </summary>
        /// <param name="fleetId">The fleet ID.</param>
        /// <returns>A list of vehicles in the specified fleet.</returns>
        [HttpGet("fleet/{fleetId}")]
        public async Task<IActionResult> GetVehiclesByFleet(string fleetId)
        {
            var result = await vehicleQuery.GetVehiclesByFleetIdAsync(fleetId, new CancellationToken());
            return result.Match(
                value => CustomResults.Success<object>(value),
                CustomResults.Problem
            );
        }

        /// <summary>
        /// Gets a vehicle by license plate.
        /// </summary>
        /// <param name="licensePlate">The license plate.</param>
        /// <returns>The vehicle with the specified license plate.</returns>
        [HttpGet("license-plate/{licensePlate}")]
        public async Task<IActionResult> GetVehicleByLicensePlate(string licensePlate)
        {
            var result = await vehicleQuery.GetVehicleByLicensePlateAsync(licensePlate, new CancellationToken());
            return result.Match(
                value => CustomResults.Success<object>(value),
                CustomResults.Problem
            );
        }

        /// <summary>
        /// Creates a new vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle data.</param>
        /// <returns>The created vehicle ID.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleCommand vehicle)
        {
            if (!ModelState.IsValid)
                return CustomResults.Problem(Result.Failure(Error.Problem("General.ModelInvalid", ModelState.SerializeModelStateErrors())));

            var result = await vehicleCommand.CreateVehicleAsync(vehicle, new CancellationToken());
            return result.Match(
                value => CustomResults.Success<object>(title: "Vehicle.Created",
                result: value, status: StatusCodes.Status201Created),
                CustomResults.Problem
            );
        }

        /// <summary>
        /// Updates an existing vehicle.
        /// </summary>
        /// <param name="id">The vehicle ID.</param>
        /// <param name="vehicle">The updated vehicle data.</param>
        /// <returns>The updated vehicle.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(string id, [FromBody] UpdateVehicleCommand vehicle)
        {
            if (!ModelState.IsValid)
                return CustomResults.Problem(Result.Failure(Error.Problem("General.ModelInvalid", ModelState.SerializeModelStateErrors())));

            var result = await vehicleCommand.UpdateVehicleAsync(id, vehicle, new CancellationToken());
            return result.Match(
                value => CustomResults.Success<object>(value, title: "Vehicle.Updated"),
                CustomResults.Problem
            );
        }

        /// <summary>
        /// Deletes a vehicle.
        /// </summary>
        /// <param name="id">The vehicle ID.</param>
        /// <returns>Success confirmation.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(string id)
        {
            var result = await vehicleCommand.DeleteVehicleAsync(id, new CancellationToken());
            return result.Match(
                () => CustomResults.Success<object>(result: null, title: "Vehicle.Deleted", status: StatusCodes.Status202Accepted),
                CustomResults.Problem
            );
        }
    }
}

