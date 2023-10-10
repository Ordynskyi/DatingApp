using System.Text;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class SymmetricKeyService : ISecurityKeyService
{
    private readonly SymmetricSecurityKey _key;
    public SymmetricKeyService(IConfiguration config)
    {
        var tokenKey = config.GetValue<string>("TokenKey");
        if (tokenKey == null) 
        {
            throw new ArgumentException("TokenKey does not set");
        }

        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
    }

    public SecurityKey GetSecurityKey()
    {
        return _key;
    }
}
