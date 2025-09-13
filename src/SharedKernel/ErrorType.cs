namespace SharedKernel;

public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    Problem = 2,
    NotFound = 3,
    Conflict = 4,
    DatabaseTransaction = 5,
    Invalid = 6,
    UnsupportedMediaType = 7,
}
