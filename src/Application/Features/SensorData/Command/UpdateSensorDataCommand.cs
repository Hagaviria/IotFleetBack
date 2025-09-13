using System.ComponentModel.DataAnnotations;

namespace Application.Features.SensorData.Command
{
    public sealed record UpdateSensorDataCommand
    {
        [Required]
        public double Latitude { get; init; }

        [Required]
        public double Longitude { get; init; }

        public double? Altitude { get; init; }

        public double? Speed { get; init; }

        public double? FuelLevel { get; init; }

        public double? FuelConsumption { get; init; }

        public double? EngineTemperature { get; init; }

        public double? AmbientTemperature { get; init; }
    }
}
