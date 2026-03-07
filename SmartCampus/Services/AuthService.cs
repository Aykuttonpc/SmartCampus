using Microsoft.EntityFrameworkCore;
using SmartCampus.Data;
using SmartCampus.Models;

namespace SmartCampus.Services;

public class AuthService(AppDbContext db)
{
    public async Task<User?> ValidateAsync(string email, string password)
    {
        var user = await db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

        if (user == null || user.PasswordHash == null) return null;

        // BCrypt hash ise Verify, eski düz-metin hash ise fallback
        var valid = user.PasswordHash.StartsWith("$2")
            ? BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)
            : user.PasswordHash == password;

        return valid ? user : null;
    }

    public async Task<(bool ok, string error)> RegisterAsync(string firstName, string lastName, string email, string password)
    {
        if (await db.Users.AnyAsync(u => u.Email == email))
            return (false, "Bu e-posta zaten kayıtlı.");

        var studentRoleId = await db.Roles.Where(r => r.RoleName == "Student").Select(r => r.RoleID).FirstAsync();

        var user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            IsActive = true,
            CreatedAt = DateTime.Now
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        db.UserRoles.Add(new UserRole { UserID = user.UserID, RoleID = studentRoleId });
        await db.SaveChangesAsync();
        return (true, "");
    }

    public async Task<List<string>> GetRolesAsync(int userId)
    {
        return await db.UserRoles
            .Where(ur => ur.UserID == userId)
            .Select(ur => ur.Role.RoleName)
            .ToListAsync();
    }
}
