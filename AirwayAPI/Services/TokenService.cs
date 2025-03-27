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

public class TokenService : ITokenService
{
    private readonly eHelpDeskContext _context;
    private readonly IConfiguration _config;
    private readonly ILogger<TokenService> _logger;

    public TokenService(eHelpDeskContext context, IConfiguration config, ILogger<TokenService> logger)
    {
        _context = context;
        _config = config;
        _logger = logger;
    }

    public string GenerateJwtToken(string username)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, username), new Claim(ClaimTypes.Name, username) };
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"], audience: _config["Jwt:Audience"], claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> GenerateRefreshTokenAsync(string username)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        _context.RefreshTokens.Add(new RefreshToken
        {
            Token = token,
            Username = username,
            ExpiresAt = DateTime.UtcNow.AddDays(14)
        });
        await _context.SaveChangesAsync();
        return token;
    }

    public async Task<bool> ValidateRefreshTokenAsync(string token) =>
        await _context.RefreshTokens.AnyAsync(rt => rt.Token == token && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow);

    public async Task RevokeRefreshTokenAsync(string token)
    {
        var rt = await _context.RefreshTokens.SingleOrDefaultAsync(x => x.Token == token);
        if (rt != null) { rt.IsRevoked = true; await _context.SaveChangesAsync(); }
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = _config["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = _config["Jwt:Audience"],
            ValidateLifetime = false,
            ClockSkew = TimeSpan.Zero
        };
        var handler = new JwtSecurityTokenHandler();
        return handler.ValidateToken(token, validationParams, out _);
    }
}