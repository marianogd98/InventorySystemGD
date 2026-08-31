// Inventory.Api/Controllers/AuthController.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Endpoint de autenticación para obtener un Token JWT de prueba.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Validación básica de credenciales para prueba técnica
        if (request.Username != "admin" || request.Password != "admin123")
        {
            return Unauthorized(new { Message = "Credenciales inválidas. Usa admin / admin123 para pruebas." });
        }

        var jwtKey = _configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("La clave secreta JWT no está configurada en appsettings.");
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "InventoryApi";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "InventoryApp";
        var expirationMinutes = int.TryParse(_configuration["Jwt:ExpirationInMinutes"], out var minutes) ? minutes : 60;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(jwtKey);
        var expires = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, request.Username),
                new Claim(ClaimTypes.Role, "Administrator"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }),
            Expires = expires,
            Issuer = jwtIssuer,
            Audience = jwtAudience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Ok(new
        {
            Token = tokenString,
            TokenType = "Bearer",
            ExpiresAt = expires
        });
    }
}

public record LoginRequest(string Username, string Password);

