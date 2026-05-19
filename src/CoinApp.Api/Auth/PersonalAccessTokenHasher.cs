using System.Security.Cryptography;
using System.Text;

namespace CoinApp.Api.Auth;

public static class PersonalAccessTokenHasher
{
    public static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token.Trim()));
        return Convert.ToHexString(bytes);
    }
}
