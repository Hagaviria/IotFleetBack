using System;

namespace SharedKernel;

public interface IDateTimeProvider
{
    public DateTime UtcNow { get; }
}
