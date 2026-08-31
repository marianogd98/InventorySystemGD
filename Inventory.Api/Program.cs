// Inventory.Api/Program.cs
using System.Text;
using Inventory.Application.Products.Commands.AddProduct;
using Inventory.Domain.Interfaces;
using Inventory.Infrastructure.Persistence;
using Inventory.Infrastructure.Queries;
using Inventory.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

// 1. Configuración de Serilog Logger
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Iniciando host de Inventory.Api...");

    var builder = WebApplication.CreateBuilder(args);

    // Integrar Serilog como proveedor de logging
    builder.Host.UseSerilog();

    // 2. Configuración de ConnectionStrings
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no fue configurada.");

    // 3. Persistencia con EF Core (Command Side)
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));

    // 4. Registro de Repositorios y Consultas (DIP & CQRS)
    builder.Services.AddScoped<IProductRepository, ProductRepository>();
    builder.Services.AddScoped<IInventoryQueries>(sp => new InventoryQueries(connectionString));

    // 5. Configuración de MediatR (Assembly de Inventory.Application)
    builder.Services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(AddProductCommand).Assembly));

    // 6. Configuración de Autenticación con JWT
    var jwtKey = builder.Configuration["Jwt:SecretKey"]
        ?? throw new InvalidOperationException("La clave secreta JWT 'Jwt:SecretKey' no fue configurada.");
    var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "InventoryApi";
    var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "InventoryApp";

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddAuthorization();

    // 7. Registro de Controladores
    builder.Services.AddControllers();

    // 8. Configuración de Swagger con soporte para Bearer Token
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Mini Inventory System API",
            Version = "v1",
            Description = "API de Gestión de Inventario con Clean Architecture, CQRS, EF Core y Dapper."
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Ingresa el token JWT en el formato: Bearer {tu_token}"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

    // Middleware de logging de peticiones HTTP de Serilog
    app.UseSerilogRequestLogging();

    // Pipeline de Middleware HTTP
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mini Inventory System API v1");
        });
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación Inventory.Api finalizó inesperadamente.");
}
finally
{
    Log.CloseAndFlush();
}
