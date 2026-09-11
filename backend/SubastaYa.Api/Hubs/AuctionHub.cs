using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace SubastaYa.Api.Hubs
{
    /// <summary>
    /// Hub de SignalR exclusivo para comunicación en tiempo real de subastas.
    /// Arquitectura Limpia: Mantenido sin dependencias a casos de uso o repositorios para cumplir con SRP.
    /// </summary>
    public class AuctionHub : Hub
    {
        public async Task JoinAuctionGroup(string auctionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, auctionId);
        }

        public async Task LeaveAuctionGroup(string auctionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, auctionId);
        }
    }
}

