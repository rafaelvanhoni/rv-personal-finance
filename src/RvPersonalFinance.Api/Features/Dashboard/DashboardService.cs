using Microsoft.EntityFrameworkCore;
using RvPersonalFinance.Api.Infrastructure.Persistence;
using RvPersonalFinance.Api.Domain.Enums;
using RvPersonalFinance.Api.Shared;

namespace RvPersonalFinance.Api.Features.Dashboard;

public class DashboardService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(AppDbContext context, ILogger<DashboardService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<OperationResult<DashboardDto>> GetDashboard(Guid userId)
    {

        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            _logger.LogWarning("User not found: {UserId}.", userId);
            return OperationResult<DashboardDto>.NotFound($"User not found: {userId}.");
        }

        var totalIncome = await _context.Transactions
                            .Where(t => t.UserId == userId && t.Type == TransactionType.Income)
                            .SumAsync(t => t.Amount);

        var totalExpense = await _context.Transactions
                            .Where(t => t.UserId == userId && t.Type == TransactionType.Expense)
                            .SumAsync(t => t.Amount);

        var totalInicialBalance = await _context.Accounts
                                    .Where(a => a.UserId == userId)
                                    .SumAsync(a => a.InitialBalance);

        var currentBalance = totalInicialBalance + totalIncome - totalExpense;

        var spendingByCategory = await _context.Transactions
                                    .Where(t => t.UserId == userId && t.Type == TransactionType.Expense)
                                    .GroupBy(t => new {t.CategoryId, t.Category.Name})
                                    .Select(g => new CategorySpendingDto
                                    {
                                        CategoryId = g.Key.CategoryId,
                                        CategoryName = g.Key.Name,
                                        Total = g.Sum(t => t.Amount)
                                    })
                                    .ToListAsync();

        var dashboardDto = new DashboardDto()
        {
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            CurrentBalance = currentBalance,
            SpendingByCategory = spendingByCategory
        };

        return OperationResult<DashboardDto>.Success(dashboardDto);

    }

}