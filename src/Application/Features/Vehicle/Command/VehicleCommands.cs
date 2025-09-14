using System;
using System.ComponentModel.DataAnnotations;

namespace Application.Features.Vehicle.Command
{
    public sealed record CreateVehicleCommand
    {
        [Required]
        [StringLength(20, MinimumLength = 1)]
        public string LicensePlate { get; init; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Model { get; init; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Brand { get; init; }

        [Required]
        [Range(0.1, double.MaxValue, ErrorMessage = "Fuel capacity must be greater than 0")]
        public double FuelCapacity { get; init; }

        [Required]
        [Range(0.1, double.MaxValue, ErrorMessage = "Average consumption must be greater than 0")]
        public double AverageConsumption { get; init; }

        public Guid? FleetId { get; init; }

        public DateTime? LastMaintenance { get; init; }
    }

    public sealed record UpdateVehicleCommand
    {
        [Required]
        [StringLength(20, MinimumLength = 1)]
        public string LicensePlate { get; init; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Model { get; init; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Brand { get; init; }

        [Required]
        [Range(0.1, double.MaxValue, ErrorMessage = "Fuel capacity must be greater than 0")]
        public double FuelCapacity { get; init; }

        [Required]
        [Range(0.1, double.MaxValue, ErrorMessage = "Average consumption must be greater than 0")]
        public double AverageConsumption { get; init; }

        public Guid? FleetId { get; init; }

        public DateTime? LastMaintenance { get; init; }
    }
}
