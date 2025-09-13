using FluentValidation;

namespace Application.Features.SensorData.Command;

internal sealed class IngestSensorDataValidator : AbstractValidator<IngestSensorDataCommand>
{
    public IngestSensorDataValidator()
    {
        RuleFor(x => x.VehicleId)
            .NotEmpty()
            .WithMessage("El ID del vehículo es requerido.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .WithMessage("La latitud debe estar entre -90 y 90 grados.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .WithMessage("La longitud debe estar entre -180 y 180 grados.");

        RuleFor(x => x.Altitude)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Altitude.HasValue)
            .WithMessage("La altitud no puede ser negativa.");

        RuleFor(x => x.Speed)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(200)
            .When(x => x.Speed.HasValue)
            .WithMessage("La velocidad debe estar entre 0 y 200 km/h.");

        RuleFor(x => x.FuelLevel)
            .InclusiveBetween(0, 100)
            .WithMessage("El nivel de combustible debe estar entre 0 y 100%.");

        RuleFor(x => x.FuelConsumption)
            .GreaterThan(0)
            .LessThanOrEqualTo(50)
            .When(x => x.FuelConsumption.HasValue)
            .WithMessage("El consumo de combustible debe estar entre 0 y 50 litros/hora.");

        RuleFor(x => x.EngineTemperature)
            .InclusiveBetween(-40, 150)
            .WithMessage("La temperatura del motor debe estar entre -40 y 150°C.");

        RuleFor(x => x.AmbientTemperature)
            .InclusiveBetween(-50, 60)
            .When(x => x.AmbientTemperature.HasValue)
            .WithMessage("La temperatura ambiente debe estar entre -50 y 60°C.");
    }
}