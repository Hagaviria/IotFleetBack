using Application.Features.SensorData.Query.GetFuelAlerts;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.SensorData;

internal sealed class GetFuelAlerts : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("sensor-data/fuel-alerts", async (
            [AsParameters] GetFuelAlertsQuery query,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(query, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.SensorData)
        .RequireAuthorization();
    }
}
