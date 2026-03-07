using Microsoft.EntityFrameworkCore;
using SmartCampus.Data;
using SmartCampus.Data.DAL;
using SmartCampus.Hubs;
using SmartCampus.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<OccupancyDal>();
builder.Services.AddScoped<ReservationDal>();
builder.Services.AddScoped<AdminDal>();

builder.Services.AddScoped<OccupancyService>();
builder.Services.AddScoped<ReservationService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AdminService>();

builder.Services.AddSignalR();
builder.Services.AddHostedService<SensorSimulatorService>();

builder.Services.AddSession(opt => { opt.IdleTimeout = TimeSpan.FromHours(2); });
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<LiveOccupancyHub>("/occupancyHub");

app.Run();
