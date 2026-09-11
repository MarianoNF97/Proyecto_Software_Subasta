using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SubastaYa.Api.Middlewares;
using SubastaYa.Api.Workers;
using SubastaYa.Api.Hubs;
using SubastaYa.Application.Common.Interfaces;
using SubastaYa.Application.Features.Auctions.Commands.Handlers;
using SubastaYa.Application.Features.Auctions.Queries.Handlers;
using SubastaYa.Application.Features.Wallets.Commands.Handlers;
using SubastaYa.Application.Features.Wallets.Queries.Handlers;
using SubastaYa.Application.Interfaces.Repositories;
using SubastaYa.Application.Services;
using SubastaYa.Infrastructure.Identity;
using SubastaYa.Infrastructure.Persistence;
using SubastaYa.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Cadena de conexión y DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Cadena de conexión 'DefaultConnection' no encontrada.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Configuración de ASP.NET Core Identity
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
.AddSignInManager()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. Configuración de Autenticación JWT y validación Fail-Fast
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
        IssuerSigningKey = new SymmetricSecurityKey(secretKey)
    };
});

builder.Services.AddAuthorization();

// 4. Inyección de Repositorios (SRP) y Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();
builder.Services.AddScoped<IBidRepository, BidRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

// 5. Inyección de Servicios de Dominio (SRP)
builder.Services.AddScoped<IBidWinnerService, BidWinnerService>();
builder.Services.AddScoped<IBidValidationService, BidValidationService>();
builder.Services.AddScoped<IBidPaymentService, BidPaymentService>();
builder.Services.AddScoped<IAntiSnipingService, AntiSnipingService>();
builder.Services.AddScoped<IAuctionClosureService, AuctionClosureService>();
builder.Services.AddScoped<IDepositService, DepositService>();

// 6. Inyección de Handlers CQRS (Auctions)
builder.Services.AddScoped<GetAuctionsHandler>();
builder.Services.AddScoped<GetAuctionByIdHandler>();
builder.Services.AddScoped<CreateAuctionHandler>();
builder.Services.AddScoped<PlaceBidHandler>();
builder.Services.AddScoped<CloseExpiredAuctionsHandler>();

// 7. Inyección de Handlers CQRS (Wallets)
builder.Services.AddScoped<GetWalletBalanceHandler>();
builder.Services.AddScoped<GetWalletTransactionsHandler>();
builder.Services.AddScoped<DepositFundsHandler>();

// 8. Background Worker (Cierre automático de subastas)
builder.Services.AddHostedService<AuctionClosingWorker>();

// 9. Controladores, Swagger con soporte para Bearer Token y CORS
builder.Services.AddControllers();
builder.Services.AddSignalR(); // INYECCION DE SIGNALR
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SubastaYa API", Version = "v1" });

    // Habilita el botón Authorize en Swagger para probar con tokens JWT
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
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 10. Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Autenticación siempre antes de Autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<AuctionHub>("/auctionHub"); 

app.Run();

