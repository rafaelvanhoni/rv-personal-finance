namespace RvPersonalFinance.Api.Features.Dashboard;

public class DashboardDto
{
    public decimal TotalIncome { get; init; }
    public decimal TotalExpense { get; init; }
    /// <summary>
    /// Current net worth from the beginning of records.
    /// Formula: sum of accounts' InitialBalance + TotalIncome − TotalExpense.
    /// NOT simply TotalIncome − TotalExpense.
    /// </summary>    
    public decimal CurrentBalance { get; init; }
    public List<CategorySpendingDto> SpendingByCategory { get; init; } = [];
}

public class CategorySpendingDto
{
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public decimal Total { get; init; }
}