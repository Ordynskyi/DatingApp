using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API.Entities;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService : ITokenService
{
    private readonly SecurityKey _key;
    private readonly TimeSpan _tokenLifeTime;

    public TokenService(IConfiguration config, ISecurityKeyService securityKeyService)
    {
        _key = securityKeyService.GetSecurityKey();

        var tokenLifeTimeSec = config.GetValue<int>("TokenLifeTimeSec");
        if (tokenLifeTimeSec == 0) 
        {
            throw new ArgumentException("TokenLifeTimeSec does not set");
        }

        _tokenLifeTime = TimeSpan.FromSeconds(tokenLifeTimeSec);
    }

    public string CreateToken(AppUser user)
    {
        var claims = new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Username)
        };

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
