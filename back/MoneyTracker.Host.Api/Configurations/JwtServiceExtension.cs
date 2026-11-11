using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace MoneyTracker.Host.Api.Configurations
{
    // Clase estática para agregar la autenticación JWT al contenedor de servicios
    public static class JwtServiceExtensions
    {
        // Método de extensión que registra la configuración de autenticación JWT
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // Lee la clave secreta desde appsettings.json → "Jwt": { "Key": "..." }
            var key = configuration["Jwt:Key"];
            var keyBytes = Encoding.UTF8.GetBytes(key);

            // Configura el esquema de autenticación predeterminado
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // "Bearer"
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;    // "Bearer"
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false; // Permite HTTP en desarrollo
                    options.SaveToken = true;             // Guarda el token en el contexto

                    // Reglas de validación del token
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false, // No valida el emisor (Issuer)
                        ValidateAudience = false, // No valida la audiencia
                        ValidateLifetime = true, // Valida que el token no esté expirado
                        ValidateIssuerSigningKey = true, // Valida la firma
                        IssuerSigningKey = new SymmetricSecurityKey(keyBytes) // Clave para validar firma
                    };
                });

            return services;
        }
    }
}