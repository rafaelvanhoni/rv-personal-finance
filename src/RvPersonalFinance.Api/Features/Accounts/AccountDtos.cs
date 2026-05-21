namespace RvPersonalFinance.Api.Features.Accounts;

public class CreateAccountDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal InitialBalance { get; set; }
}

public class UpdateAccountDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal InitialBalance { get; set; }
    
}

public class AccountResponseDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal InitialBalance { get; init; }
    public DateTime CreatedAt { get; init; }
    
}