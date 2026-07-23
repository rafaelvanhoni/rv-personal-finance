using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;
using RvPersonalFinance.Api.Infrastructure.Persistence;
using RvPersonalFinance.Api.Features.Accounts;
using RvPersonalFinance.Api.Features.Categories;
using RvPersonalFinance.Api.Features.Transactions;
using FluentValidation;
using RvPersonalFinance.Api.Middleware;
using RvPersonalFinance.Api.Features.Dashboard;
using RvPersonalFinance.Api.Features.Auth;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<IValidator<CreateAccountDto>, CreateAccountValidator>();
builder.Services.AddScoped<IValidator<UpdateAccountDto>, UpdateAccountValidator>();
builder.Services.AddScoped<IValidator<CreateCategoryDto>, CreateCategoryValidator>();
builder.Services.AddScoped<IValidator<UpdateCategoryDto>, UpdateCategoryValidator>();
builder.Services.AddScoped<IValidator<CreateTransactionDto>, CreateTransactionValidator>();
builder.Services.AddScoped<IValidator<UpdateTransactionDto>, UpdateTransactionValidator>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks().AddNpgSql(connectionString);
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();    
}

app.UseHttpsRedirection();

app.MapHealthChecks("/health");

app.MapAuthEndpoints();
app.MapAccountEndpoints();
app.MapCategoryEndpoints();
app.MapTransactionEndpoints();
app.MapDashboardEndpoints();

app.Run();