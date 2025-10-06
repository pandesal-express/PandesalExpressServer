namespace PandesalExpress.Infrastructure.Configs;

public class JwtOptions
{
    // Face Recognition Service Settings
    public string FaceIssuer { get; set; } = "face-service";
    public string FaceAudience { get; set; } = "core-service";
    public string JwksUri { get; set; } = null!;
    public string InternalServiceKey { get; set; } = null!;

    // Your Main JWT Settings (for user sessions)
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public int AccessTokenExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
