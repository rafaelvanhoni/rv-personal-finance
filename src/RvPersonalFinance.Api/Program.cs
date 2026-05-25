using Microsoft.EntityFrameworkCore;
using RvPersonalFinance.Api.Infrastructure.Persistence;
using RvPersonalFinance.Api.Features.Accounts;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;
using RvPersonalFinance.Api.Features.Categories;
using RvPersonalFinance.Api.Features.Transactions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();    
}

app.UseHttpsRedirection();

app.MapGet("/", () => new
{
    status = "ok"
}).WithName("HealthCheck");

app.MapAccountEndpoints();
app.MapCategoryEndpoints();
app.MapTransactionEndpoints();

app.Run();