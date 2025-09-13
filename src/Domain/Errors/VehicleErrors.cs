using SharedKernel;

namespace Domain.Errors;

public static class VehicleErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Vehicle.NotFound",
        "No se encontró el vehículo especificado.");

    public static readonly Error InvalidLicensePlate = Error.Invalid(
        "Vehicle.InvalidLicensePlate",
        "La placa del vehículo no es válida.");

    public static readonly Error DuplicateLicensePlate = Error.Conflict(
        "Vehicle.DuplicateLicensePlate",
        "Ya existe un vehículo con esta placa.");

    public static readonly Error InvalidFuelCapacity = Error.Invalid(
        "Vehicle.InvalidFuelCapacity",
        "La capacidad de combustible debe ser mayor a 0.");

    public static readonly Error InvalidConsumption = Error.Invalid(
        "Vehicle.InvalidConsumption",
        "El consumo promedio debe ser mayor a 0.");
}
