using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SubastaYa.Api.Hubs;
using SubastaYa.Api.Middlewares;
using SubastaYa.Api.Workers;
using SubastaYa.Application.Common.Interfaces;
using SubastaYa.Application.Features.Auctions.Commands.Handlers;
using SubastaYa.Application.Features.Auctions.Queries.Handlers;
using SubastaYa.Application.Features.Categories.Queries.Handlers;
using SubastaYa.Application.Features.Wallets.Commands.Handlers;
using SubastaYa.Application.Features.Wallets.Queries.Handlers;
using SubastaYa.Application.Interfaces.Repositories;
using SubastaYa.Application.Interfaces.Services;
using SubastaYa.Application.Services;
using SubastaYa.Infrastructure.Identity;
using SubastaYa.Infrastructure.Persistence;
using SubastaYa.Infrastructure.Persistence.Repositories;
using SubastaYa.Infrastructure.Persistence.Seeders;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Cadena de conexión y DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Cadena de conexión 'DefaultConnection' no encontrada.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 1. Configuración de ASP.NET Core Identity (sin AddSignInManager para no sobreescribir el esquema JWT por cookies)
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddRoles<IdentityRole<int>>()
.AddUserManager<UserManager<ApplicationUser>>()
.AddRoleManager<RoleManager<IdentityRole<int>>>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 2. Desactivar mapeo automático de claims XML/SOAP para respetar los claims directos (sub, name, role)
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// 3. Configuración explícita de Autenticación JWT y validación Fail-Fast
var jwtSection = builder.Configuration.GetSection("JwtSettings");
var secret = jwtSection["Secret"];

if (string.IsNullOrWhiteSpace(secret) || secret.Length < 32)
{
    throw new InvalidOperationException("Configuración inválida: 'JwtSettings:Secret' no fue configurado o tiene una longitud menor a 32 caracteres.");
}

var secretKey = Encoding.UTF8.GetBytes(secret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ClockSkew = TimeSpan.Zero
    };

    // Permite a SignalR recibir el JWT a través de la query string en el Handshake del WebSocket
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var exception = context.Exception; 
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            var error = context.Error;
            var description = context.ErrorDescription;
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/auctionHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    }; ;
});

builder.Services.AddAuthorization();

// Inyección de Repositorios (SRP) y Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();
builder.Services.AddScoped<IBidRepository, BidRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

// Inyección de Servicios de Dominio y Aplicación (SRP)
builder.Services.AddScoped<IBidWinnerService, BidWinnerService>();
builder.Services.AddScoped<IBidValidationService, BidValidationService>();
builder.Services.AddScoped<IBidPaymentService, BidPaymentService>();
builder.Services.AddScoped<IAntiSnipingService, AntiSnipingService>();
builder.Services.AddScoped<IAuctionClosureService, AuctionClosureService>();
builder.Services.AddScoped<IDepositService, DepositService>();
builder.Services.AddScoped<IAuctionSettlementService, AuctionSettlementService>();
builder.Services.AddScoped<IAuctionAuditService, AuctionAuditService>();
builder.Services.AddScoped<IAuctionActivationService, AuctionActivationService>();

// Inyección de Handlers CQRS
builder.Services.AddScoped<GetAuctionsHandler>();
builder.Services.AddScoped<GetAuctionByIdHandler>();
builder.Services.AddScoped<CreateAuctionHandler>();
builder.Services.AddScoped<GetCategoriesHandler>();
builder.Services.AddScoped<PlaceBidHandler>();
builder.Services.AddScoped<CloseExpiredAuctionsHandler>();
builder.Services.AddScoped<ActivateScheduledAuctionsHandler>();
builder.Services.AddScoped<GetWalletBalanceHandler>();
builder.Services.AddScoped<GetWalletTransactionsHandler>();
builder.Services.AddScoped<DepositFundsHandler>();

// Background Workers
builder.Services.AddHostedService<AuctionClosingWorker>();
builder.Services.AddHostedService<AuctionActivationWorker>();

// Inyección de servicios de notificación en tiempo real (SignalR)
builder.Services.AddScoped<IAuctionNotificationService, AuctionNotificationService>();

// Controladores, SignalR, Swagger con soporte para Bearer Token y CORS
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value != null && e.Value.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors)
                .Select(x => x.ErrorMessage);

            var response = new
            {
                status = 400,
                error = string.Join(", ", errors),
                timestamp = DateTime.UtcNow
            };

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
        };
    });

builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SubastaYa API", Version = "v1" });

    // Habilita el botón Authorize para probar con tokens JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Encabezado de autorización JWT usando el esquema Bearer. Ejemplo: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Inicialización de base de datos y sembrado dinámico al iniciar
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        await context.Database.MigrateAsync();
        await DbInitializer.SeedAsync(context, userManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al aplicar migraciones o sembrar la base de datos.");
    }
}

// Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();


app.UseCors("AllowAll");

// Autenticación siempre antes de Autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<AuctionHub>("/auctionHub");

await app.RunAsync();