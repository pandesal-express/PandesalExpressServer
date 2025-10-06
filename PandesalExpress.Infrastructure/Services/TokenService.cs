using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PandesalExpress.Infrastructure.Models;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace PandesalExpress.Infrastructure.Services;

public interface ITokenService
{
    Task<(string token, DateTime expiration)> GenerateJwtTokenAsync(Employee employee);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    ClaimsPrincipal? ValidateFaceServiceToken(string token);
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly ILogger<TokenService> _logger;
    private readonly UserManager<Employee> _userManager;

    public TokenService(IConfiguration config, UserManager<Employee> userManager, ILogger<TokenService> logger)
    {
        _config = config;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<(string token, DateTime expiration)> GenerateJwtTokenAsync(Employee employee)
    {
        IList<string> userRoles = await _userManager.GetRolesAsync(employee);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, employee.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, employee.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, $"{employee.FirstName} {employee.LastName}"),
            new("position", employee.Position),
            new("department", employee.Department.Name)
        };
        claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        IConfigurationSection jwtSettings = _config.GetSection("JwtSettings");
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        DateTime expiration = DateTime.UtcNow.AddHours(double.Parse(jwtSettings["ExpirationHours"] ?? "1"));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken? token = tokenHandler.CreateToken(tokenDescriptor);

        return (tokenHandler.WriteToken(token), expiration);
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        IConfigurationSection jwtSettings = _config.GetSection("JwtSettings");
        string? secretKey = jwtSettings["SecretKey"];

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        ClaimsPrincipal? principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token algorithm.");

        return principal;
    }

    public string GenerateRefreshToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    public ClaimsPrincipal? ValidateFaceServiceToken(string token)
    {
        try
        {
            string? rsaPublicKeyPem = _config["FaceService:PublicKeyPem"];
            if (string.IsNullOrWhiteSpace(rsaPublicKeyPem))
                throw new InvalidOperationException("Face Auth service public key not configured.");

            var rsa = RSA.Create();
            rsa.ImportFromPem(rsaPublicKeyPem);

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "face-service",
                ValidAudience = "core-service",
                IssuerSigningKey = new RsaSecurityKey(rsa),
                ClockSkew = TimeSpan.FromSeconds(10)
            };

            var handler = new JwtSecurityTokenHandler();
            ClaimsPrincipal principal = handler.ValidateToken(token, validationParams, out SecurityToken validatedToken);

            if (validatedToken is not JwtSecurityToken jwt ||
                !jwt.Header.Alg.Equals(SecurityAlgorithms.RsaSha256, StringComparison.OrdinalIgnoreCase))
                throw new SecurityTokenException("Invalid RSA algorithm.");

            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate face auth token: {Message}", ex.Message);
            return null;
        }
    }
}
