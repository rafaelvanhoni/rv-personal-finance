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
            _ => Results.StatusCode(500),
        };
    }
}
