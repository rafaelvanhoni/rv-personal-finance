using Microsoft.EntityFrameworkCore;
using RvPersonalFinance.Api.Domain.Entities;
using RvPersonalFinance.Api.Infrastructure.Persistence;
using RvPersonalFinance.Api.Shared;

namespace RvPersonalFinance.Api.Features.Auth;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuthService> _logger;

    public AuthService(AppDbContext context, ILogger<AuthService> logger)
    {
        _context = context;
        _logger = logger;
    }    

    public async Task<OperationResult<UserResponseDto>> Register(RegisterDto dto)
    {
        var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email.Trim());
        if (emailExists)
        {
            _logger.LogWarning("Email already registered: {Email}.", dto.Email);
            return OperationResult<UserResponseDto>.Conflict($"Email already registered: {dto.Email}");
        }

        var user = new User()
        {
            Name = dto.Name.Trim(),
            Email = dto.Email.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)    
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        _logger.LogInformation("User created: {UserId}.", user.Id);

        var userResponseDto = ToResponseDto(user);

        return OperationResult<UserResponseDto>.Created(userResponseDto);
    }

    private static UserResponseDto ToResponseDto(User user)
    {
        return new UserResponseDto()
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };
    }
}