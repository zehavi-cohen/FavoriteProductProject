using backend.Endpoints;
using backend.Extensions;
using backend.Validators.Auth;
using FluentValidation;
using backend.Contexts.CurrentUser;

//יצירת אובייקט השירותים של האפלקצייה שתבנה בהמשך
var builder = WebApplication.CreateBuilder(args);

// ConnectionString  לפי ה  SQL Serverעם חיבור ל  AppDbContext חיבור למסד נתונים - הגדרת ה 
builder.Services.AddApplicationDatabase(builder.Configuration);

//DI
builder.Services.AddApplicationServices();

//לאישור בקשות הדפדפן- אנגולר- 4200 CORS הגדרות     
builder.Services.AddApplicationCors(builder.Configuration);
  
//Authentication  
builder.Services.AddApplicationAuthentication(builder.Configuration);

//Authorization
builder.Services.AddApplicationAuthorization();

// הגדרות חיבור לולידציות
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

//הגדרות הנתונים לקונטקסט
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();

//swagger
builder.Services.AddApplicationSwagger();

//בניית האפליקציה עם השירותים שהטמענו בתוכה
var app = builder.Build();

//Middleware
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(ApplicationServiceExtensions.AngularClientCorsPolicy);

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