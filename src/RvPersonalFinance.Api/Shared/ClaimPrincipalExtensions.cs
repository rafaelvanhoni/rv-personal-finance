using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RvPersonalFinance.Api.Shared;

public static class ClaimPrincipalExtensions
{
        public static Guid GetUserId(this ClaimsPrincipal user) => Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}