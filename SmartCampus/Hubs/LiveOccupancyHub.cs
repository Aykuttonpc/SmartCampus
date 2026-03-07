using Microsoft.AspNetCore.SignalR;

namespace SmartCampus.Hubs;

public class LiveOccupancyHub : Hub
{
    // Cihazlar bu hub'a bağlandığında çalışır.
    // Client'lara veri göndermek için IHubContext<LiveOccupancyHub> kullanılacak.
}
