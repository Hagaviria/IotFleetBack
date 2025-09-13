using Application.Abstractions.Data;
using Domain.Models;
using Domain.Errors;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.SensorData.Query
{
    public class SensorDataQueryHandler(
        IApplicationDbContext context
        )
    {
        public async Task<Result<List<Domain.Models.SensorData>>> GetAllSensorDataAsync(CancellationToken cancellationToken)
        {
            var sensorData = await context.SensorData
                .AsNoTracking()
                .Include(sd => sd.Vehicle)
                .OrderByDescending(sd => sd.Timestamp)
                .ToListAsync(cancellationToken);

            return Result.Success(sensorData);
        }

        public async Task<Result<Domain.Models.SensorData>> GetSensorDataByIdAsync(string id, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(id, out var sensorDataId))
            {
                return Result.Failure<Domain.Models.SensorData>(Error.NotFound("SensorData.NotFound", "Sensor data not found"));
            }

            var sensorData = await context.SensorData
                .AsNoTracking()
                .Include(sd => sd.Vehicle)
                .FirstOrDefaultAsync(sd => sd.Id == sensorDataId, cancellationToken);

            if (sensorData is null)
            {
                return Result.Failure<Domain.Models.SensorData>(Error.NotFound("SensorData.NotFound", "Sensor data not found"));
            }

            return Result.Success(sensorData);
        }
    }
}
