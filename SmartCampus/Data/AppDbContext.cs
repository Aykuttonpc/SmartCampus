using Microsoft.EntityFrameworkCore;
using SmartCampus.Models;

namespace SmartCampus.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<FacilityType> FacilityTypes => Set<FacilityType>();
    public DbSet<Status> Statuses => Set<Status>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<Facility> Facilities => Set<Facility>();
    public DbSet<Sensor> Sensors => Set<Sensor>();
    public DbSet<OccupancyLog> OccupancyLogs => Set<OccupancyLog>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<ReservationAttendee> ReservationAttendees => Set<ReservationAttendee>();
    public DbSet<ReservationAuditLog> ReservationAuditLogs => Set<ReservationAuditLog>();
    public DbSet<UserPenalty> UserPenalties => Set<UserPenalty>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // ── Keyless types (Views) ──────────────────────────────────
        mb.Entity<LiveOccupancyVm>().HasNoKey().ToView("vw_LiveOccupancy");
        mb.Entity<TodayReservationVm>().HasNoKey().ToView("vw_TodayReservations");

        // Stored Procedure sonuç modellerini normal EF keyless varlık olarak bırak
        mb.Entity<FreeSlotVm>().HasNoKey().ToView(null);
        mb.Entity<PeakHourVm>().HasNoKey().ToView(null);
        mb.Entity<NoShowReportVm>().HasNoKey().ToView(null);
        mb.Entity<AlternativeVm>().HasNoKey().ToView(null);

        // ── Explicit PKs ────────────────────────────────────────────────────────
        // EF Core yalnizca "Id" / "{ClassName}Id" formatini convention ile tanir.
        // TypeID, StatusID, RoleID vb. buna uymaz, HasKey() zorunlu.
        mb.Entity<Role>().HasKey(r => r.RoleID);
        mb.Entity<FacilityType>().HasKey(ft => ft.TypeID);
        mb.Entity<Status>().HasKey(s => s.StatusID);
        mb.Entity<User>().HasKey(u => u.UserID);
        mb.Entity<Zone>().HasKey(z => z.ZoneID);
        mb.Entity<Facility>().HasKey(f => f.FacilityID);
        mb.Entity<Sensor>().HasKey(s => s.SensorID);
        mb.Entity<OccupancyLog>().HasKey(o => o.LogID);
        mb.Entity<Reservation>().HasKey(r => r.ReservationID);

        // trg_BlockSuspendedUser bir INSTEAD OF INSERT trigger.
        // SQL Server, INSTEAD OF trigger olan tablolarda EF Core'un
        // kullandığı "OUTPUT INSERTED.x" sorgusuna izin vermiyor.
        // UseSqlOutputClause(false) → EF Core SCOPE_IDENTITY() kullanır.
        mb.Entity<Reservation>().ToTable(tb => tb.UseSqlOutputClause(false));
        mb.Entity<ReservationAuditLog>().HasKey(a => a.AuditID);
        mb.Entity<UserPenalty>().HasKey(p => p.PenaltyID);

        // Composite PKs
        mb.Entity<UserRole>().HasKey(ur => new { ur.UserID, ur.RoleID });
        mb.Entity<ReservationAttendee>().HasKey(ra => new { ra.ReservationID, ra.UserID });

        // ── Computed column ─────────────────────────────────────────────────────
        mb.Entity<User>()
            .Property(u => u.FullName)
            .HasComputedColumnSql("([FirstName] + N' ' + [LastName])", stored: true);

        // ── Relationships (Restrict - cascade catismasini onler) ────────────────

        // Facility → Zone ve FacilityType: TypeID convention dışı olduğu için
        // EF Core 'FacilityTypeTypeID' üretir. HasForeignKey ile düzeltiyoruz.
        mb.Entity<Facility>()
            .HasOne(f => f.Zone).WithMany(z => z.Facilities)
            .HasForeignKey(f => f.ZoneID);

        mb.Entity<Facility>()
            .HasOne(f => f.FacilityType).WithMany(ft => ft.Facilities)
            .HasForeignKey(f => f.TypeID);

        mb.Entity<Sensor>()
            .HasOne(s => s.Facility).WithMany(f => f.Sensors)
            .HasForeignKey(s => s.FacilityID);

        mb.Entity<OccupancyLog>()
            .HasOne(o => o.Sensor).WithMany(s => s.OccupancyLogs)
            .HasForeignKey(o => o.SensorID);

        mb.Entity<Reservation>()
            .HasOne(r => r.Facility).WithMany(f => f.Reservations)
            .HasForeignKey(r => r.FacilityID);

        mb.Entity<Reservation>()
            .HasOne(r => r.Status).WithMany(s => s.Reservations)
            .HasForeignKey(r => r.StatusID);

        mb.Entity<UserRole>()
            .HasOne(ur => ur.Role).WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleID);

        mb.Entity<UserRole>()
            .HasOne(ur => ur.User).WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserID);

        mb.Entity<Zone>()
            .HasOne(z => z.Parent).WithMany(z => z.Children)
            .HasForeignKey(z => z.ParentZoneID).OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Reservation>()
            .HasOne(r => r.Organizer).WithMany(u => u.Reservations)
            .HasForeignKey(r => r.OrganizerUserID).OnDelete(DeleteBehavior.Restrict);

        mb.Entity<ReservationAttendee>()
            .HasOne(ra => ra.Reservation).WithMany(r => r.Attendees)
            .HasForeignKey(ra => ra.ReservationID);

        mb.Entity<ReservationAttendee>()
            .HasOne(ra => ra.User).WithMany(u => u.Attendances)
            .HasForeignKey(ra => ra.UserID).OnDelete(DeleteBehavior.Restrict);

        mb.Entity<ReservationAuditLog>()
            .HasOne(a => a.Reservation).WithMany(r => r.AuditLogs)
            .HasForeignKey(a => a.ReservationID).OnDelete(DeleteBehavior.Restrict);

        mb.Entity<UserPenalty>()
            .HasOne(p => p.User).WithMany(u => u.Penalties)
            .HasForeignKey(p => p.UserID);

        mb.Entity<UserPenalty>()
            .HasOne(p => p.Reservation).WithMany()
            .HasForeignKey(p => p.ReservationID).OnDelete(DeleteBehavior.Restrict);


        // ── Check constraints ───────────────────────────────────────────────────
        mb.Entity<Facility>()
            .HasCheckConstraint("chk_OperatingHours", "[CloseTime] > [OpenTime]");

        mb.Entity<Reservation>()
            .HasCheckConstraint("chk_ReservationTime", "[EndTime] > [StartTime]")
            .HasCheckConstraint("chk_CancellationLogic", "[CancelledAt] IS NULL OR [CancelledAt] >= [CreatedAt]");

        mb.Entity<ReservationAttendee>()
            .HasCheckConstraint("chk_CheckIn", "[CheckedInAt] IS NULL OR [IsAttended] = 1");
    }
}
