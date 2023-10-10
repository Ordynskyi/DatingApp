using Microsoft.IdentityModel.Tokens;

namespace API.Interfaces;

public interface ISecurityKeyService
{
    SecurityKey GetSecurityKey();
}
