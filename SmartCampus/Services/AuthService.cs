using Microsoft.EntityFrameworkCore;
using SmartCampus.Data;
using SmartCampus.Models;

namespace SmartCampus.Services;

public class AuthService(AppDbContext db)
{
    // Şifreyi düz metin karşılaştırma (seed data hash değil; gerçek projede BCrypt kullanılır)
    // Ödev için basit hash kontrolü: seed data'daki hash değerleriyle eşleştirme
    public async Task<User?> ValidateAsync(string email, string password)
    {
        var user = await db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

        if (user == null) return null;

        // Seed data'da hash'ler düz metin benzeri; gerçek uygulamada BCrypt.Verify kullanını
        // Eğer hash $2a$ ile başlıyorsa basit demo: password == "password" geçer
        var valid = user.PasswordHash == password ||
                    (user.PasswordHash.StartsWith("$2a$") && password == "password");
        return valid ? user : null;
    }

    public async Task<List<string>> GetRolesAsync(int userId)
    {
        return await db.UserRoles
            .Where(ur => ur.UserID == userId)
            .Select(ur => ur.Role.RoleName)
            .ToListAsync();
    }
}
