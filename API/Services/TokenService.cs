using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService : ITokenService
{
    private readonly SymmetricSecurityKey _key;
    private readonly TimeSpan _tokenLifeTime;

    public TokenService(IConfiguration config)
    {
        var tokenKey = config.GetValue<string>("TokenKey");
        if (tokenKey == null) 
        {
            throw new ArgumentException("TokenKey does not set");
        }

        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

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
            new Claim(ClaimTypes.NameIdentifier, user.UserName)
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
