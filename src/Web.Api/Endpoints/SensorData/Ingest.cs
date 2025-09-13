using System;
using Application.Features.SensorData.Command;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace Web.Api.Endpoints.SensorData;

internal sealed class Ingest : IEndpoint
{
    public sealed record Request(
        Guid VehicleId,
        double Latitude,
        double Longitude,
        double? Altitude,
        double? Speed,
        double FuelLevel,
        double? FuelConsumption,
        double EngineTemperature,
        double? AmbientTemperature
    );

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("sensor-data/ingest", async (
            Request request, 
            ISender sender, 
            CancellationToken cancellationToken) =>
        {
            var command = new IngestSensorDataCommand(
                request.VehicleId,
                request.Latitude,
                request.Longitude,
                request.Altitude,
                request.Speed,
                request.FuelLevel,
                request.FuelConsumption,
                request.EngineTemperature,
                request.AmbientTemperature
            );

            Result<Guid> result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.SensorData)
        .RequireAuthorization()
        .RequireRateLimiting("SensorDataIngestPolicy")
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Ingesta de datos de sensores",
            Description = "Endpoint para recibir datos de sensores de vehículos (GPS, combustible, temperatura)",
            Tags = new List<Microsoft.OpenApi.Models.OpenApiTag> 
            { 
                new() { Name = "Sensor Data" } 
            }
        });
    }
}
