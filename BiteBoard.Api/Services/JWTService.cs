using Microsoft.IdentityModel.Tokens;
using BiteBoard.Data.Constants;
using BiteBoard.API.Services.Interfaces;
using BiteBoard.API.Settings;
using BiteBoard.Data.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BiteBoard.API.Services;

public class JWTService : IJWTService
{
    private readonly JWTSettings _jwtSettings;

    public JWTService(JWTSettings jwtSettings)
    {
        _jwtSettings = jwtSettings;
    }

    public string GenerateToken(ApplicationUser user, IList<Claim> userClaims, string tenant)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("tenant", tenant),
            new Claim("tenant_id", tenant)
        };
        foreach (var userClaim in userClaims.Where(c => c.Type == CustomClaimTypes.Permission))
        {
            claims.Add(new Claim(CustomClaimTypes.Permission, userClaim.Value));
        }
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_jwtSettings.ExpiryInMinutes),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}