using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Hangfire;
using Hangfire.PostgreSql;
using System.Text;
using Erp.Data;
using Erp.Models.ApplicationUsers;
using Erp.Services;
using Erp.Hubs;
// using Erp.Schedulers; scheduling jobs

var builder = WebApplication.CreateBuilder(args);

// ------------------ DATABASE ------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ------------------ Hangfire ------------------
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(options =>
    {
        options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"));
    }));

// ------------------ IDENTITY ------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// ------------------ EMAIL + CUSTOM SERVICES + Hangfire +SignalIr------------------
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddHangfireServer();
builder.Services.AddSignalR();
// builder.Services.AddScoped<AccountCleanupService>();

// ------------------ JWT AUTH ------------------
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new Exception("Jwt:Key not configured");
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };

    // ✅ Read JWT from cookies
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = ctx =>
        {
            if (ctx.Request.Cookies.ContainsKey("jwt"))
            {
                ctx.Token = ctx.Request.Cookies["jwt"];
            }
            return Task.CompletedTask;
        }
    };
});

// ------------------ CORS ------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // ✅ Required for cookie's
    });
});

builder.Services.AddControllers();

var app = builder.Build();

// ------------------ Manual DI Registration ------------------

using (var scope = app.Services.CreateScope())
{
    await ApplicationDbSeeder.SeedAsync(scope.ServiceProvider);
}


// ------------------ MIDDLEWARE ORDER MATTERS ------------------
app.UseCors("AllowReactApp");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ------------------ Endpoints ------------------
app.MapGet("/", () => "Hello, you are connected to the server!");
app.UseHangfireDashboard("/hangfire");
app.MapHub<NotificationHub>("/notificationHub");
app.MapControllers();


// ------------------ REGISTER HANGFIRE JOBS ------------------
// using (var scope = app.Services.CreateScope())
// {
//     var recurringJobs = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
//     JobScheduler.ScheduleJobs(recurringJobs);
// }

app.MapControllers();

app.Run();
