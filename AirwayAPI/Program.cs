using AirwayAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure CORS to allow specific origins, methods, and headers
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        policy =>
        {
            policy.AllowAnyHeader()
                  .AllowAnyMethod()
                  .WithOrigins("http://localhost:3000", "http://localhost:5001", "http://10.0.0.8");
            //.AllowCredentials(); // if you need to support credentials
        });
});

// Registering the DbContext with dependency injection
builder.Services.AddDbContext<eHelpDeskContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
}, ServiceLifetime.Transient);

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Airway API", Version = "v1" });
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
        c.RoutePrefix = string.Empty; // To serve the Swagger UI at the app's root
    });
}

//app.UseHttpsRedirection();

// Serve static files from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting(); // Ensure UseRouting is called before UseCors and UseAuthorization

app.UseCors("CorsPolicy"); // Apply CORS policy

//app.UseAuthentication(); // Use Authentication middleware if needed
app.UseAuthorization();

// Fallback to serve index.html for client-side routing
app.MapFallbackToFile("/index.html");

app.MapControllers();

app.Run();
