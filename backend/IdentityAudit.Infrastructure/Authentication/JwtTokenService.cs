using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IdentityAudit.Application.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IdentityAudit.Infrastructure.Authentication;

public sealed class JwtTokenService
    : IAccessTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(
        IOptions<JwtOptions> options)
    {
        _options = options.Value;
        _options.Validate();
    }

    public AccessTokenResult CreateToken(
        AccessTokenRequest request)
    {
        var issuedAt = DateTimeOffset.UtcNow;

        var expiresAt = issuedAt.AddMinutes(
            _options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                request.UserId.ToString()),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()),

            new(
                JwtRegisteredClaimNames.Iat,
                issuedAt.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),

            new(
                JwtRegisteredClaimNames.Email,
                request.Email),

            new(
                "name",
                request.DisplayName)
        };

        claims.AddRange(
            request.Roles.Select(
                role => new Claim(
                    "role",
                    role)));

        var signingKey = new SymmetricSecurityKey(
            _options.GetSigningKeyBytes());

        var signingCredentials =
            new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: issuedAt.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: signingCredentials);

        var accessToken =
            new JwtSecurityTokenHandler()
                .WriteToken(token);

        return new AccessTokenResult(
            accessToken,
            expiresAt);
    }
}