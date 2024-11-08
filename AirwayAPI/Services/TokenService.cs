using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AirwayAPI.Services
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TokenService> _logger;


        public TokenService(IConfiguration configuration, ILogger<TokenService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public string GenerateJwtToken(string username)
        {
            _logger.LogInformation("Generating new JWT token for user: {UserName}", username);

            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                _logger.LogError("JWT Key is not configured in appsettings.json.");
                throw new InvalidOperationException("JWT Key is not configured in appsettings.json.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, username), // Important: 'sub' claim should be present
                new(ClaimTypes.NameIdentifier, username),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds);

            _logger.LogInformation("JWT token successfully generated for user: {UserName}", username);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            _logger.LogInformation("Validating expired token.");

            try
            {
                var jwtKey = _configuration["Jwt:Key"];
                if (string.IsNullOrEmpty(jwtKey))
                {
                    _logger.LogError("JWT Key is not configured in appsettings.json.");
                    throw new InvalidOperationException("JWT Key is not configured in appsettings.json.");
                }

                _logger.LogInformation("Using key for validation: {Key}", jwtKey);

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = false, // Allows validation of expired tokens
                    ClockSkew = TimeSpan.Zero
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

                if (securityToken is JwtSecurityToken jwtSecurityToken &&
                    jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Extract claims manually
                    var jwtToken = tokenHandler.ReadJwtToken(token);
                    var claims = jwtToken.Claims.ToList();

                    // Log all claims for debugging
                    foreach (var claim in claims)
                    {
                        _logger.LogInformation("Claim Type: {Type}, Claim Value: {Value}", claim.Type, claim.Value);
                    }

                    // Manually find the 'sub' claim
                    var subClaim = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                    if (string.IsNullOrEmpty(subClaim))
                    {
                        _logger.LogWarning("Token does not contain a 'sub' claim or the claim is empty.");
                        throw new SecurityTokenException("Token does not contain a valid user identity.");
                    }

                    _logger.LogInformation("Successfully extracted 'sub' claim: {Sub}", subClaim);

                    // Create a new ClaimsIdentity with the correct claims
                    var identity = new ClaimsIdentity(claims, "Jwt");

                    // Add the Name claim manually if not present
                    identity.AddClaim(new Claim(ClaimTypes.Name, subClaim));

                    return new ClaimsPrincipal(identity);
                }

                _logger.LogWarning("Token header or algorithm mismatch.");
                throw new SecurityTokenException("Invalid token algorithm or header.");
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogWarning("Token has expired.");
                throw;
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning("Security token exception: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token.");
                throw;
            }
        }
    }
}
