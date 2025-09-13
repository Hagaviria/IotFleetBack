using System;
using Application.Features.SensorData.Query.GetByFleet;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.SensorData;

internal sealed class GetByFleet : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("sensor-data/fleet/{fleetId:guid}", async (
            Guid fleetId,
            [AsParameters] GetSensorDataByFleetQuery query,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var queryWithFleetId = query with { FleetId = fleetId };
            var result = await sender.Send(queryWithFleetId, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.SensorData)
        .RequireAuthorization();
    }
}
