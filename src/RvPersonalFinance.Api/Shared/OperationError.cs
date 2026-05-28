namespace RvPersonalFinance.Api.Shared;

public class OperationError
{
    public string? Property { get; set; }
    public string Message { get; set; } = string.Empty;
}