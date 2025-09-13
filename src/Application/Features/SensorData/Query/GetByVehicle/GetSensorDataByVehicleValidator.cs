using System;
using FluentValidation;

namespace Application.Features.SensorData.Query.GetByVehicle;

internal sealed class GetSensorDataByVehicleValidator : AbstractValidator<GetSensorDataByVehicleQuery>
{
    public GetSensorDataByVehicleValidator()
    {
        RuleFor(x => x.VehicleId)
            .NotEmpty()
            .WithMessage("El ID del vehículo es requerido.");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("La página debe ser mayor a 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 1000)
            .WithMessage("El tamaño de página debe estar entre 1 y 1000.");

        RuleFor(x => x.FromDate)
            .LessThan(x => x.ToDate)
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue)
            .WithMessage("La fecha de inicio debe ser menor a la fecha de fin.");

        RuleFor(x => x.ToDate)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.ToDate.HasValue)
            .WithMessage("La fecha de fin no puede ser futura.");
    }
}
