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

namespace AirwayAPI.Services
{
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
            _logger.LogInformation("Generating JWT for user {Username}", username);

            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, username),
                    new Claim(ClaimTypes.Name, username)
                };

                var token = new JwtSecurityToken(
                    issuer: _jwt.Issuer,
                    audience: _jwt.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_jwt.TokenLifetimeMinutes),
                    signingCredentials: creds
                );

                var jwt = new JwtSecurityTokenHandler().WriteToken(token);
                _logger.LogInformation("JWT generated successfully for user {Username}", username);
                return jwt;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT for user {Username}", username);
                throw;
            }
        }

        public async Task<string> GenerateRefreshTokenAsync(string username)
        {
            _logger.LogInformation("Generating refresh token for user {Username}", username);

            try
            {
                var refreshTokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                var refreshToken = new RefreshToken
                {
                    Token = refreshTokenValue,
                    Username = username,
                    ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshLifetimeDays)
                };

                _context.RefreshTokens.Add(refreshToken);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Refresh token persisted for user {Username}", username);
                return refreshTokenValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating refresh token for user {Username}", username);
                throw;
            }
        }

        public async Task<bool> ValidateRefreshTokenAsync(string token)
        {
            _logger.LogDebug("Validating refresh token");
            var valid = await _context.RefreshTokens.AnyAsync(rt =>
                rt.Token == token &&
                !rt.IsRevoked &&
                rt.ExpiresAt > DateTime.UtcNow);

            _logger.LogDebug("Refresh token validation result: {IsValid}", valid);
            return valid;
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            _logger.LogInformation("Revoking refresh token");
            var rt = await _context.RefreshTokens.SingleOrDefaultAsync(x => x.Token == token);
            if (rt != null)
            {
                rt.IsRevoked = true;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Refresh token revoked successfully");
            }
            else
            {
                _logger.LogWarning("Attempted to revoke non-existent or already revoked token");
            }
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            _logger.LogDebug("Extracting principal from expired token");
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
                var validationParams = new TokenValidationParameters
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

                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(token, validationParams, out _);
                _logger.LogDebug("Principal extracted successfully");
                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract principal from expired token");
                throw;
            }
        }
    }
}
