using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.Features;

namespace CentralStore.LocalStore.ProductManagent.Filters
{
  public class ValidationFilter<T>(
    ILogger<ValidationFilter<T>> logger,
    IValidator<T> pageParamsValidator) : IEndpointFilter
    where T : class
  {
    public async ValueTask<object?> InvokeAsync(
      EndpointFilterInvocationContext invocationContext,
      EndpointFilterDelegate next)
    {
      var input = invocationContext.Arguments
        .Where(arg => arg is T)
        .Select(arg => arg as T)
        .FirstOrDefault();

      var path = invocationContext.HttpContext.Request.Path;
      var traceId = invocationContext.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity.Id;

      var extensions = new Dictionary<string, object?>()
        {
          { "traceId", traceId }
        };

      if (input is null)
      {
        logger.LogError("Validation error at: {Path} | Trace id: {TraceId} => {Error}",
          path,
          traceId,
          $"No argument of type {typeof(T)} provided.");

        return TypedResults.ValidationProblem(
          new Dictionary<string, string[]>()
          { { "Invalid argument", new[] { $"No argument of type {typeof(T)} provided." } } },
          "Invalid argument for validation",
          $"{invocationContext.HttpContext?.Request.Method} {path}",
          "One or more validation errors occurred.",
          "https://tools.ietf.org/html/rfc9110#section-15.5.1",
          extensions
          );
      }

      ValidationResult validationResult = await pageParamsValidator.ValidateAsync(input);

      if (!validationResult.IsValid)
      {
        var errorsAsString = string.Join(Environment.NewLine,
          validationResult.Errors
          .Select(error => error.ErrorMessage));

        logger.LogError("Validation error at: {Path} | Trace id: {TraceId} => {Error}",
          path,
          traceId,
          errorsAsString);

        return TypedResults.ValidationProblem(
          validationResult.ToDictionary(),
          errorsAsString,
          $"{invocationContext.HttpContext?.Request.Method} {path}",
          "One or more validation errors occurred.",
          "https://tools.ietf.org/html/rfc9110#section-15.5.1",
          extensions);
      }

      return await next(invocationContext);
    }
  }
}
