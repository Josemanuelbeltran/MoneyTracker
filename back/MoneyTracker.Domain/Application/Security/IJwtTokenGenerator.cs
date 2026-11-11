namespace MoneyTracker.Domain.Application.Security;

public interface IJwtTokenGenerator
{
        /// <summary>Genera el JWT del usuario; <paramref name="isAdmin"/> viaja como claim "is_admin".</summary>
        string GenerateJwtToken(string id, bool isAdmin);
    
}
