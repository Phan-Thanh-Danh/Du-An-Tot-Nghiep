using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.Constants;
using Backend.Models;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Helpers;

public class JwtHelper
{
    private readonly IConfiguration _configuration;

    public JwtHelper(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTime ExpiresAt) GenerateToken(NguoiDung user, string role, string status)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(GetExpiresInMinutes());
        var credentials = new SigningCredentials(GetSecurityKey(), SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(CustomClaimTypes.UserId, user.MaNguoiDung.ToString()),
            new(CustomClaimTypes.Email, user.Email),
            new(CustomClaimTypes.Role, role),
            new(CustomClaimTypes.CampusId, user.MaDonVi.ToString()),
            new(CustomClaimTypes.Status, status),
            new(ClaimTypes.NameIdentifier, user.MaNguoiDung.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, role),
            new(JwtRegisteredClaimNames.Sub, user.MaNguoiDung.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: GetRequiredSetting("JwtSettings:Issuer"),
            audience: GetRequiredSetting("JwtSettings:Audience"),
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        try
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(
                token,
                GetTokenValidationParameters(),
                out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch (SecurityTokenException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    public TokenValidationParameters GetTokenValidationParameters()
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = GetRequiredSetting("JwtSettings:Issuer"),
            ValidAudience = GetRequiredSetting("JwtSettings:Audience"),
            IssuerSigningKey = GetSecurityKey(),
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };
    }

    private SymmetricSecurityKey GetSecurityKey()
    {
        var secret = GetRequiredSetting("JwtSettings:Secret");
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    }

    private int GetExpiresInMinutes()
    {
        return int.TryParse(_configuration["JwtSettings:ExpiresInMinutes"], out var minutes)
            ? minutes
            : 60;
    }

    private string GetRequiredSetting(string key)
    {
        var value = _configuration[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Thiếu cấu hình bắt buộc: {key}");
        }

        return value;
    }
}
