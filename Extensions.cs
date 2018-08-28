using BotAuth.Models;
using Microsoft.Identity.Client;

namespace ChatterBot
{
    public static class Extensions
    {
        public static AuthResult FromMSALAuthenticationResult(this AuthenticationResult authResult, TokenCache tokenCache)
        {
            var result = new AuthResult
            {
                AccessToken = authResult.AccessToken,
                UserName = $"{authResult.User.Name}",
                UserUniqueId = authResult.User.Identifier,
                ExpiresOnUtcTicks = authResult.ExpiresOn.UtcTicks,
                TokenCache = tokenCache.Serialize()
            };

            return result;
        }
    }
}
