using SharedKernel;

namespace Domain.Errors;

public static class SensorDataErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "SensorData.NotFound",
        "No se encontraron datos de sensores para el vehículo especificado.");

    public static readonly Error NoDataFound = Error.NotFound(
        "SensorData.NoDataFound",
        "No se encontraron datos de sensores en el período especificado.");

    public static readonly Error InvalidVehicle = Error.Invalid(
        "SensorData.InvalidVehicle",
        "El vehículo especificado no es válido.");

    public static readonly Error InvalidCoordinates = Error.Invalid(
        "SensorData.InvalidCoordinates",
        "Las coordenadas GPS proporcionadas no son válidas.");

    public static readonly Error InvalidFuelLevel = Error.Invalid(
        "SensorData.InvalidFuelLevel",
        "El nivel de combustible debe estar entre 0 y 100.");

    public static readonly Error InvalidTemperature = Error.Invalid(
        "SensorData.InvalidTemperature",
        "La temperatura del motor debe ser un valor válido.");

    public static readonly Error DuplicateData = Error.Conflict(
        "SensorData.DuplicateData",
        "Ya existe un registro de datos de sensores para este vehículo en este momento.");
}
