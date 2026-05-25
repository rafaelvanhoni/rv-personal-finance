namespace RvPersonalFinance.Api.Features.Categories;

public class CreateCategoryDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class UpdateCategoryDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;    
}

public class CategoryResponseDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }    
}