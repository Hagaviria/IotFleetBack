using SharedKernel;
using System;

namespace Domain.Errors;

public static class UserErrors
{
    public static readonly Error UserNull = Error.ModelNullValue;

    public static readonly Error InvalidId = Error.Invalid(
        "User.InvalidId",
        "The provided user Id is invalid");

    public static Error NotFound(string userId) => Error.NotFound(
        "Users.NotFound",
        $"The user with the Id = '{userId}' was not found");

    public static Error Unauthorized() => Error.Failure(
        "Users.Unauthorized",
        "You are not authorized to perform this action.");

    public static readonly Error NotFoundByEmail = Error.NotFound(
        "Users.NotFoundByEmail",
        "The user with the specified email was not found");

    public static readonly Error NotFoundByName = Error.NotFound(
        "Users.NotFoundByName",
        "The user with the specified name was not found");

    public static readonly Error EmailNotUnique = Error.Conflict(
        "Users.EmailNotUnique",
        "The provided email is not unique");

    public static readonly Error PasswordNotSet = Error.Problem(
        "Users.PasswordNotSet",
        "The password for the user is not set");

    public static readonly Error SamePasswordValidation = Error.Invalid(
        "Users.PasswordError",
        "The password new password is the same of the old.");

    public static readonly Error IdConflict = Error.Conflict(
        "Users.IdConflict",
        $"The given IDs do not match.");

    public static Error UnknownDatabaseTransaction(string errorMessage) => Error.DBTransactionFailure(
        "Users.UnknownDatabaseTransaction",
        errorMessage);

}
