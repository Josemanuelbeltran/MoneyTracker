using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MoneyTracker.Domain.Application.Security;

namespace ClassLibrary1MoneyTracker.Application.Security
{
    // Implementación del generador de tokens JWT
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly string _key;                 // Clave secreta para firmar el token
        private readonly string? _issuer;             // Emisor (opcional)
        private readonly string? _audience;           // Audiencia (opcional)
        private readonly IConfiguration _config;      // Configuración

        public JwtTokenGenerator(IConfiguration config)
        {
            _config = config;

            _key = _config["Jwt:Key"]
                   ?? throw new InvalidOperationException("Jwt:Key no configurado");

            // Opcionales: se usarán si en JwtBearer activas la validación de issuer/audience
            _issuer = _config["Jwt:Issuer"];
            _audience = _config["Jwt:Audience"];
        }

        // Método que genera el token JWT usando el userId como claim
        public string GenerateJwtToken(string id, bool isAdmin)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_key);
            var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);

            var now = DateTime.UtcNow;

            // Claims mínimos + recomendables
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, id),
                // nombre del usuario
                new Claim(ClaimTypes.NameIdentifier, id),            // tu userId
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // identificador único del token
                new Claim(JwtRegisteredClaimNames.Iat,
                          new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                // Rol: el admin es quien decide qué categorías son comunes para todos.
                new Claim("is_admin", isAdmin ? "true" : "false", ClaimValueTypes.Boolean)
            };

            var token = new JwtSecurityToken(
                issuer: _issuer,             // puede ir null si no validas issuer
                audience: _audience,         // puede ir null si no validas audience
                claims: claims,
                notBefore: now,
                expires: now.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        string IJwtTokenGenerator.GenerateJwtToken(string id, bool isAdmin) => GenerateJwtToken(id, isAdmin);
    }
}
