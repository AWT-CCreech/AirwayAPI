using AirwayAPI.Configuration;
using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AirwayAPI.Services;
public class TokenService(
    eHelpDeskContext context,
    JwtSettings jwtSettings,
    ILogger<TokenService> logger) : ITokenService
{
    private readonly eHelpDeskContext _context = context;
    private readonly JwtSettings _jwt = jwtSettings;
    private readonly ILogger<TokenService> _logger = logger;

    public string GenerateJwtToken(string username)
    {
        _logger.LogInformation("Generating JWT for {Username}", username);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[] { new Claim(ClaimTypes.Name, username) };

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.TokenLifetimeMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> GenerateRefreshTokenAsync(string username)
    {
        var rtValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var rt = new RefreshToken { Token = rtValue, Username = username, ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshLifetimeDays) };
        _context.RefreshTokens.Add(rt);
        await _context.SaveChangesAsync();
        return rtValue;
    }

    public Task<bool> ValidateRefreshTokenAsync(string token)
        => _context.RefreshTokens.AnyAsync(rt => rt.Token == token && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow);

    public async Task RevokeRefreshTokenAsync(string token)
    {
        var rt = await _context.RefreshTokens.SingleOrDefaultAsync(x => x.Token == token);
        if (rt != null) { rt.IsRevoked = true; await _context.SaveChangesAsync(); }
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var valParams = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = _jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwt.Audience,
            ValidateLifetime = false,
            ClockSkew = TimeSpan.Zero
        };

        return new JwtSecurityTokenHandler().ValidateToken(token, valParams, out _);
    }
}