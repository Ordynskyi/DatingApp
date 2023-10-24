using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService : ITokenService
{
    private readonly SecurityKey _key;
    private readonly TimeSpan _tokenLifeTime;
    private readonly UserManager<AppUser> _userManager;

    public TokenService(
        IConfiguration config,
        ISecurityKeyService securityKeyService,
        UserManager<AppUser> userManager)
    {
        _key = securityKeyService.GetSecurityKey();

        var tokenLifeTimeSec = config.GetValue<int>("TokenLifeTimeSec");
        if (tokenLifeTimeSec == 0) 
        {
            throw new ArgumentException("TokenLifeTimeSec does not set");
        }

        _tokenLifeTime = TimeSpan.FromSeconds(tokenLifeTimeSec);
        _userManager = userManager;
    }

    public async Task<string> CreateToken(AppUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var rolesCount = roles.Count;
        var claims = new Claim[rolesCount + 2];
        claims[0] = new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString());
        claims[1] = new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty);

        for (var i = 0; i < rolesCount; i++)
        {
            claims[i + 2] = new Claim(ClaimTypes.Role, roles[i]);
        }

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

        var descriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(_tokenLifeTime),
            SigningCredentials = creds,
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(descriptor);
        
        return tokenHandler.WriteToken(token);
    }
}