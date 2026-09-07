using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Middlewares;
using SubastaYa.Api.Workers;
using SubastaYa.Application.Features.Auctions.Commands.Handlers;
using SubastaYa.Application.Features.Auctions.Queries.Handlers;
using SubastaYa.Application.Features.Wallets.Commands.Handlers;
using SubastaYa.Application.Features.Wallets.Queries.Handlers;
using SubastaYa.Application.Interfaces.Repositories;
using SubastaYa.Application.Services;
using SubastaYa.Infrastructure.Persistence;
using SubastaYa.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Cadena de conexión 'DefaultConnection' no encontrada.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Inyección de Repositorios (SRP) y Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();
builder.Services.AddScoped<IBidRepository, BidRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

//Inyección de Servicios de Dominio (SRP)
builder.Services.AddScoped<IBidWinnerService, BidWinnerService>();
builder.Services.AddScoped<IBidValidationService, BidValidationService>();
builder.Services.AddScoped<IBidPaymentService, BidPaymentService>();
builder.Services.AddScoped<IAntiSnipingService, AntiSnipingService>();
builder.Services.AddScoped<IAuctionClosureService, AuctionClosureService>();
builder.Services.AddScoped<IDepositService, DepositService>();

//Inyección de Handlers CQRS (Auctions)
builder.Services.AddScoped<GetAuctionsHandler>();
builder.Services.AddScoped<GetAuctionByIdHandler>();
builder.Services.AddScoped<CreateAuctionHandler>();
builder.Services.AddScoped<PlaceBidHandler>();
builder.Services.AddScoped<CloseExpiredAuctionsHandler>();

//Inyección de Handlers CQRS (Wallets)
builder.Services.AddScoped<GetWalletBalanceHandler>();
builder.Services.AddScoped<GetWalletTransactionsHandler>();
builder.Services.AddScoped<DepositFundsHandler>();

// Background Worker (Cierre automático de subastas)
builder.Services.AddHostedService<AuctionClosingWorker>();

//Controladores, Swagger y CORS
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();