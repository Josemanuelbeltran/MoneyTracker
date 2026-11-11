using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MoneyTracker.Host.Api.Security;

/// <summary>
/// Lectura del usuario autenticado desde el JWT: el id va en el claim `sub`
/// (o `nameidentifier`) y el rol de administrador en el claim `is_admin`.
/// </summary>
public static class ClaimsExtension
{
    public const string IsAdminClaim = "is_admin";

    public static int GetUserId(this HttpContext ctx)
    {
        var claim = ctx.User.FindFirst(JwtRegisteredClaimNames.Sub)
                    ?? ctx.User.FindFirst(ClaimTypes.NameIdentifier)
                    ?? throw new UnauthorizedAccessException("Token sin identificador de usuario");

        return int.Parse(claim.Value);
    }

    public static bool IsAdmin(this HttpContext ctx) =>
        string.Equals(ctx.User.FindFirst(IsAdminClaim)?.Value, "true", StringComparison.OrdinalIgnoreCase);
}
