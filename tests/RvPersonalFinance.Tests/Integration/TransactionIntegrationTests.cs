using System.Net;
using RvPersonalFinance.Api.Features.Categories;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using RvPersonalFinance.Api.Features.Transactions;
using RvPersonalFinance.Api.Domain.Enums;

namespace RvPersonalFinance.Tests.Integration;

public class TransactionIntegrationTests : IntegrationTestBase
{
    public TransactionIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
    {
        
    }

    [Fact]
    public async Task CreateTransaction_WhenReferencesAreValid_ShouldReturnCreated()
    {
        // Given
        var user = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Login.Token);

        var account = await CreateAccountAsync();

        var dtoCategory = new CreateCategoryDto()
        {
            Name = $"Category {Guid.CreateVersion7()}"
        };

        var responseCategory = await _client.PostAsJsonAsync("/categories", dtoCategory);
        Assert.Equal(HttpStatusCode.Created, responseCategory.StatusCode);

        var category = await responseCategory.Content.ReadFromJsonAsync<ResponseEnvelope<CategoryResponseDto>>();
        Assert.NotNull(category);
        Assert.NotNull(category.Data);

        var dtoTransaction = new CreateTransactionDto()
        {
            AccountId = account.Id,
            CategoryId = category.Data.Id,
            Description = $"Transaction {Guid.CreateVersion7()}",
            Amount = 1000m,
            Type = TransactionType.Expense,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // When
        var responseTransaction = await _client.PostAsJsonAsync("/transactions", dtoTransaction);
    
        // Then
        Assert.Equal(HttpStatusCode.Created, responseTransaction.StatusCode);
    }
}