using backend.Authentication;
using Microsoft.Extensions.Configuration;

namespace backend.Extensions;

public static class AuthCookieExtensions
{
    public static void AppendAuthCookie(
        this HttpResponse response,
        string token,
        IConfiguration configuration)
    {
        var expirationMinutes = configuration.GetValue<int?>("Jwt:ExpiresInMinutes") ?? 120;

        response.Cookies.Append(
            AuthCookieNames.AccessToken,
            token,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes)
            });
    }

    public static void DeleteAuthCookie(this HttpResponse response)
    {
        response.Cookies.Delete(
            AuthCookieNames.AccessToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });
    }
}