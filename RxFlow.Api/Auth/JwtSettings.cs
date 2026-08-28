namespace RxFlow.Api.Auth;

public sealed class JwtSettings
{
    public string Issuer { get; set; } = "rxflow-api";
    public string Audience { get; set; } = "rxflow-clients";
    public string SigningKey { get; set; } = "rxflow-local-training-signing-key-2026";
}