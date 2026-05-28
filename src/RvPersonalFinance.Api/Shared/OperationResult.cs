namespace RvPersonalFinance.Api.Shared;

public class OperationResult<T>
{

    private OperationResult(){ }

    public ResultStatus Status { get; init; } = ResultStatus.Success;
    public bool IsSuccess => Status is ResultStatus.Success or ResultStatus.Created;

    public List<OperationError> Errors { get; init; } = [];
    public T? Data { get; init; }

    public static OperationResult<T> Success(T data)
    {
        return new OperationResult<T>
        {
            Data = data
        };
    }

    public static OperationResult<T> NotFound(string message)
    {
        return new OperationResult<T>
        {
            Status = ResultStatus.NotFound,
            Errors = [new OperationError
            {
                Message = message,
            }]
        };
    }

    public static OperationResult<T> ValidationError(IEnumerable<OperationError> errors)
    {
        return new OperationResult<T>
        {
            Status = ResultStatus.ValidationError,
            Errors = errors.ToList()
        };
    }

    public static OperationResult<T> Created(T data)
    {
        return new OperationResult<T>
        {
            Status = ResultStatus.Created,
            Data = data
        };
    }
}