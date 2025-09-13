using System;
using Application.Features.SensorData.Query.GetLatest;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.SensorData;

internal sealed class GetLatest : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("sensor-data/vehicle/{vehicleId:guid}/latest", async (
            Guid vehicleId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetLatestSensorDataQuery(vehicleId);
            var result = await sender.Send(query, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.SensorData)
        .RequireAuthorization();
    }
}
