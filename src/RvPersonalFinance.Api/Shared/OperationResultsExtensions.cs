using FluentValidation.Results;

namespace RvPersonalFinance.Api.Shared;

public static class OperationResultsExtensions
{
    public static IResult ToHttpResult<T> (this OperationResult<T> result)
    {
        return result.Status switch
        {
            ResultStatus.Success => Results.Ok(result),
            ResultStatus.NotFound => Results.NotFound(result),
            ResultStatus.ValidationError => Results.BadRequest(result),
            ResultStatus.BusinessError => Results.UnprocessableEntity(result),
            ResultStatus.Created => Results.Created((string?)null, result),
            ResultStatus.Conflict => Results.Conflict(result),
            ResultStatus.Unauthorized => Results.Json(result, statusCode: 401),
            _ => Results.StatusCode(500),
        };
    }

    public static List<OperationError> ToOperationErrors (this IEnumerable<ValidationFailure> failures)
    {
        return failures.Select(e => new OperationError
        {
            Property = e.PropertyName,
            Message = e.ErrorMessage
        }).ToList();
    }
}
