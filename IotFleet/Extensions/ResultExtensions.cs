using Microsoft.AspNetCore.Mvc.ModelBinding;
using SharedKernel;

namespace IotFleet.Extensions;

public static class ResultExtensions
{
    /// <summary>
    /// Matches the result and executes the appropriate function based on the success or failure of the result.
    /// </summary>
    /// <typeparam name="TOut">The type of the output.</typeparam>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <param name="onFailure">The function to execute if the result is a failure.</param>
    /// <returns>The output of the executed function.</returns>
    public static TOut Match<TOut>(
        this Result result,
        Func<TOut> onSuccess,
        Func<Result, TOut> onFailure)
    {
        return result.IsSuccess ? onSuccess() : onFailure(result);
    }

    /// <summary>
    /// Matches the result and executes the appropriate function based on the success or failure of the result.
    /// </summary>
    /// <typeparam name="TIn">The type of the input value.</typeparam>
    /// <typeparam name="TOut">The type of the output.</typeparam>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <param name="onFailure">The function to execute if the result is a failure.</param>
    /// <returns>The output of the executed function.</returns>
    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> onSuccess,
        Func<Result<TIn>, TOut> onFailure)
    {
        return result.IsSuccess ? onSuccess(result.Value) : onFailure(result);
    }

    /// <summary>
    /// Serializes ModelState errors into a string format.
    /// </summary>
    /// <param name="modelState">The ModelState to serialize.</param>
    /// <returns>A string representation of the ModelState errors.</returns>
    public static string SerializeModelStateErrors(this ModelStateDictionary modelState)
    {
        var errors = modelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"))
            .ToList();

        return string.Join("; ", errors);
    }
}
