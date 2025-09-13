using Microsoft.AspNetCore.Mvc;
using IotFleet.Extensions;
using SharedKernel;
using Application.Features.SensorData.Command;
using Application.Features.SensorData.Query;
using IotFleet.Infrastructure;

namespace IotFleet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SensorDataController(
        SensorDataCommandHandler sensorDataCommand,
        SensorDataQueryHandler sensorDataQuery
        ) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetSensorData()
        {
            var result = await sensorDataQuery.GetAllSensorDataAsync(new CancellationToken());
            return result.Match(
                value => CustomResults.Success(value),
                CustomResults.Problem
            );
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSensorDataById(string id)
        {
            var result = await sensorDataQuery.GetSensorDataByIdAsync(id, new CancellationToken());
            return result.Match(
                value => CustomResults.Success(value),
                CustomResults.Problem
            );
        }

        [HttpPost]
        public async Task<IActionResult> CreateSensorData([FromBody] IngestSensorDataCommand sensorData)
        {
            if (!ModelState.IsValid)
                return CustomResults.Problem(Result.Failure(Error.Problem("General.ModelInvalid", ModelState.SerializeModelStateErrors())));

            var result = await sensorDataCommand.IngestSensorDataAsync(sensorData, new CancellationToken());
            return result.Match(
                value => CustomResults.Success(title: "SensorData.Created",
                result: value, status: StatusCodes.Status201Created),
                CustomResults.Problem
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSensorData(string id, [FromBody] UpdateSensorDataCommand sensorData)
        {
            if (!ModelState.IsValid)
                return CustomResults.Problem(Result.Failure(Error.Problem("General.ModelInvalid", ModelState.SerializeModelStateErrors())));

            var result = await sensorDataCommand.UpdateSensorDataAsync(id, sensorData, new CancellationToken());
            return result.Match(
                value => CustomResults.Success(value, title: "SensorData.Updated"),
                CustomResults.Problem
            );
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSensorData(string id)
        {
            var result = await sensorDataCommand.DeleteSensorDataAsync(id, new CancellationToken());
            return result.Match(
                () => CustomResults.Success<object>(result: null, title: "SensorData.Deleted", status: StatusCodes.Status202Accepted),
                CustomResults.Problem
            );
        }
    }
}
