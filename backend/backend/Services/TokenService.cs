using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Entities;
using Microsoft.IdentityModel.Tokens;

namespace backend.Services;

public class TokenService
{
    public const string IsImpersonatingClaim = "is_impersonating";
    public const string ImpersonatedByUserIdClaim = "impersonated_by_user_id";
    public const string ImpersonatedByUserNameClaim = "impersonated_by_user_name";

    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    //לאחר כניסת משתמש חוקי- נבדק באמצעות סיסמה JWT יצירת ה
    public string CreateToken(
        AppUser user,
        int? impersonatedByUserId = null,
        string? impersonatedByUserName = null)
    {
        //appsettings- מתוך קובץ ההגדרות JWT הגדות 
        var jwtSection = _configuration.GetSection("Jwt");

        // המפתח הסודי לחתימה על הטוקן
        var key = jwtSection["Key"];

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("JWT key is missing.");
        }
        //יוצר הטוקן- השרת שהוגדר
        var issuer = jwtSection["Issuer"];

        // נמען הטוקן- הלקוח שהוגדר
        var audience = jwtSection["Audience"];

        //אורך החיים של הטוקן
        var expiresInMinutes = int.TryParse(jwtSection["ExpiresInMinutes"], out var minutes)
            ? minutes
            : 120;

        //הכנסת מידע מהמשתמש הנכנס לטוקן
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email)
        };

        foreach (var userRole in user.UserRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
        }

        if (impersonatedByUserId.HasValue)
        {
            claims.Add(new Claim(IsImpersonatingClaim, "true"));
            claims.Add(new Claim(ImpersonatedByUserIdClaim, impersonatedByUserId.Value.ToString()));

            if (!string.IsNullOrWhiteSpace(impersonatedByUserName))
            {
                claims.Add(new Claim(ImpersonatedByUserNameClaim, impersonatedByUserName));
            }
        }

        //HASH והאלגוריתם  KEY יצירת החתימה על הטוקן עם ה
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256
        );

        //יצירת הטוקן עצמו עם התכונות שהוגדרו לעיל
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: credentials
        );

        //הפיכת הטוקן מאובייקט למחרוזת
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}