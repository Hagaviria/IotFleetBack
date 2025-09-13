namespace SharedKernel;

public record Error
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    public static readonly Error NullValue = new(
        "General.Null",
        "Null value was provided",
        ErrorType.Failure);
    public static readonly Error ModelNullValue = new(
        "General.ModelNull",
        "Null value was provided by the client",
        ErrorType.Problem);
    public static readonly Error DatabaseTransaction = new(
        "General.DatabaseTransaction",
        "An error occurred while executing the transaction in the database",
        ErrorType.DatabaseTransaction);

    public Error(string code, string description, ErrorType type)
    {
        Code = code;
        Description = description;
        Type = type;
    }

    public string Code { get; }

    public string Description { get; }

    public ErrorType Type { get; }

    public static Error Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);

    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    public static Error Problem(string code, string description) =>
        new(code, description, ErrorType.Problem);

    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);

    public static Error Invalid(string code, string description) =>
        new(code, description, ErrorType.Validation);

    public static Error DBTransactionFailure(string code, string description) =>
        new(code, description, ErrorType.DatabaseTransaction);

    public static Error FileType(string code, string description) =>
        new(code, description, ErrorType.UnsupportedMediaType);
}
