using System.Text;
using backend.Data;
using backend.Endpoints;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

//יצירת אובייקט השירותים של האפלקצייה שתבנה בהמשך
var builder = WebApplication.CreateBuilder(args);

// ConnectionString  לפי ה  SQL Serverעם חיבור ל  AppDbContext חיבור למסד נתונים - הגדרת ה 
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );
});

//DI
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<AdminService>();

//לאישור בקשות הדפדפן- אנגולר- 4200 CORS הגדרות     
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

//לטיפול באימות המשתמשים JWT הגדרות    
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException("JWT key is missing.");
}

//Authentication  
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            //בדיקת תקינות יוצר הטוקן
            ValidateIssuer = true,
            //בדיקת תקינות נמען הטוקן
            ValidateAudience = true,
            //בדיקת תקפות התוקן
            ValidateLifetime = true,
            //בדיקת תקינות חתימת הטוקן
            ValidateIssuerSigningKey = true,

            //בדיקת תאימות נתוני הטוקן להגדרות שהוגדו בשרת
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],

            //KEY- הגדרת מפתח לפתחית הטוקן- עם ה     
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            ),
            
            ClockSkew = TimeSpan.Zero
        };
    });
//Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireRole("Admin");
    });
});

//swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//בניית האפליקציה עם השירותים שהטמענו בתוכה
var app = builder.Build();

//Middleware
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AngularClient");

app.UseAuthentication();
app.UseAuthorization();

// Endpoints- מיפוי ה 
app.MapGet("/", () => "Favorite Products API is running.")
    .WithTags("Health");

app.MapAuthEndpoints();
app.MapProductEndpoints();
app.MapAdminEndpoints();

//הפעלת השרת
app.Run();