using System;
using Application.Features.SensorData.Query.GetFuelStats;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.SensorData;

internal sealed class GetFuelStats : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("sensor-data/vehicle/{vehicleId:guid}/fuel-stats", async (
            Guid vehicleId,
            [AsParameters] GetFuelStatisticsQuery query,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var queryWithVehicleId = query with { VehicleId = vehicleId };
            var result = await sender.Send(queryWithVehicleId, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.SensorData)
        .RequireAuthorization();
    }
}
