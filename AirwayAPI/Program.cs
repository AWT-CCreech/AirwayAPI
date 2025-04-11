using AirwayAPI.Data;
using AirwayAPI.Services;
using AirwayAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
    // Alternatively, use null to keep PascalCase
    // options.JsonSerializerOptions.PropertyNamingPolicy = null;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase
);

// Configure CORS to allow specific origins, methods, and headers
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        policy =>
        {
            policy.AllowAnyHeader()
                  .AllowAnyMethod()
                  .WithOrigins("http://localhost:3000", "http://localhost:5001", "http://10.0.0.8");
        });
});

// Registering the DbContext with dependency injection as Scoped (default)
// eHelpDeskContext from the eHelpDesk database
builder.Services.AddDbContext<eHelpDeskContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("eHelpDeskConnection"));
});

// REGISTER THE SECOND DB CONTEXT
// MAS500AppContext from the mas500_app database (scaffolded with tarInvoice table)
builder.Services.AddDbContext<MAS500AppContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("MAS500AppConnection"));
});

// Register services with their corresponding interfaces
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IScanService, ScanService>();
builder.Services.AddScoped<ISalesOrderService, SalesOrderService>();
builder.Services.AddScoped<IStringService, StringService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();

// Register IHttpContextAccessor to access HttpContext from service classes
builder.Services.AddHttpContextAccessor();

// Configure JWT authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var keyString = jwtSettings["Key"];

if (string.IsNullOrEmpty(keyString))
{
    throw new InvalidOperationException("JWT Key is not configured in appsettings.json.");
}

var key = Encoding.ASCII.GetBytes(keyString);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        // Configure JwtBearer events
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                // Log the authentication failure
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(context.Exception, "Authentication failed.");

                // Do not modify the response here to prevent response already started issues
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                // Override the default challenge response
                context.HandleResponse();

                // Check if the response has already started
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";

                    var result = JsonSerializer.Serialize(new { message = "Authentication failed." });
                    return context.Response.WriteAsync(result);
                }

                // If the response has already started, do not attempt to modify it
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                // Optional: Additional token validation or logging
                return Task.CompletedTask;
            }
        };
    });

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Airway API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Use the Bearer scheme to authenticate requests with a JSON Web Token (JWT). Provide the token in the Authorization header with the format: 'Bearer {token}'."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Airway API v1");
        c.RoutePrefix = string.Empty;
    });
}

// Uncomment the following line if you want to enforce HTTPS
// app.UseHttpsRedirection();

// Serve static files from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

// Fallback to serve index.html for client-side routing
app.MapFallbackToFile("/index.html");

app.MapControllers();

app.Run();
