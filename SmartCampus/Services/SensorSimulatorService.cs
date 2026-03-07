using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SmartCampus.Data;
using SmartCampus.Hubs;
using SmartCampus.Models;

namespace SmartCampus.Services;

public class SensorSimulatorService(IServiceProvider services, IHubContext<LiveOccupancyHub> hub) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Daha gerçekçi ve akıcı: Her 1 saniyede bir tetiklenir ama çok az kişi değişir
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);

            try
            {
                using var scope = services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Tüm sensörlerde aynı anda ufak değişimler yaratıyoruz
                var allSensors = await db.Sensors
                    .Include(s => s.Facility)
                    .ToListAsync(stoppingToken);

                foreach (var sensor in allSensors)
                {
                    var lastLog = await db.OccupancyLogs
                        .Where(l => l.SensorID == sensor.SensorID)
                        .OrderByDescending(l => l.LoggedAt)
                        .FirstOrDefaultAsync(stoppingToken);

                    int current = lastLog?.CurrentCount ?? 0;
                    int maxCap = sensor.Facility?.MaxCapacity ?? 100;
                    
                    // Akıcı değişim: İçeri 1-2 kişi girer veya 1-2 kişi çıkar
                    var rnd = new Random();
                    int diff = rnd.Next(-2, 3); // -2, -1, 0, 1, 2
                    if (diff == 0) diff = rnd.Next(0, 2) == 0 ? 1 : -1; // 0 olmasın, illa ki 1 kişi kıpırdasın
                    
                    int newCount = current + diff;

                    // Gerçeklik kontrolleri
                    if (newCount < 0) newCount = 0; // 0'ın altına düşmesin
                    if (newCount > maxCap) newCount = maxCap; // Kapasiteyi aşmasın

                    // Yeni record at
                    db.OccupancyLogs.Add(new OccupancyLog
                    {
                        SensorID = sensor.SensorID,
                        LoggedAt = DateTime.Now,
                        CurrentCount = newCount
                    });
                    
                    await db.SaveChangesAsync(stoppingToken);

                    // Değişikliği SignalR ile tüm clientlara yayınla
                    int pct = maxCap > 0 ? (int)Math.Round((double)newCount / maxCap * 100) : 0;
                    
                    string crowd = pct switch
                    {
                        0 => "Empty",
                        < 40 => "Low",
                        < 75 => "Moderate",
                        < 100 => "High",
                        _ => "Crowded"
                    };

                    await hub.Clients.All.SendAsync("UpdateOccupancy", new
                    {
                        facilityId = sensor.FacilityID,
                        liveCount = newCount,
                        occupancyPct = pct,
                        crowdLevel = crowd,
                        lastUpdated = DateTime.Now.ToString("HH:mm:ss")
                    }, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[SignalR Service Error] " + ex.Message);
            }
        }
    }
}
