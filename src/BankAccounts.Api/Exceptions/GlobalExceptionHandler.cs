using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BankAccounts.Api.Exceptions;

/// <summary>
/// Maps the domain exceptions to HTTP status codes and ProblemDetails responses.
/// </summary>
public class GlobalExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            AccountNotFoundException => (StatusCodes.Status404NotFound, "Account not found"),
            InvalidAmountException => (StatusCodes.Status400BadRequest, "Invalid amount"),
            InsufficientFundsException => (StatusCodes.Status422UnprocessableEntity, "Insufficient funds"),
            InterestNotAllowedException => (StatusCodes.Status422UnprocessableEntity, "Interest not allowed"),
            _ => (0, string.Empty)
        };

        if (status == 0)
        {
            return false;
        }

        httpContext.Response.StatusCode = status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = exception.Message
            }
        });
    }
}
