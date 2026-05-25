using InvoicesWebService.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Shared.Results;

namespace InvoicesWebService.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result, Func<T, IActionResult> success)
    {
        return result.Match(
            success,
            error => error.Type switch
            {
                ErrorType.NotFound => new NotFoundObjectResult(Envelope.Error(error)),
                ErrorType.Conflict => new ConflictObjectResult(Envelope.Error(error)),
                ErrorType.Validation => new BadRequestObjectResult(Envelope.Error(error)),
                ErrorType.Internal => new ObjectResult(Envelope.Error(error))
                {
                    StatusCode = 500,
                },
                _ => new BadRequestObjectResult(Envelope.Error(error))
            }
        );
    }
}