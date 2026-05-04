using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public static class Data
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var roles = new[]
        {
            new VaiTro { MaVaiTro = 1, MaCodeVaiTro = "quan_tri", TenVaiTro = "Quản trị" },
            new VaiTro { MaVaiTro = 2, MaCodeVaiTro = "giao_vien", TenVaiTro = "Giáo viên" },
            new VaiTro { MaVaiTro = 3, MaCodeVaiTro = "hoc_sinh", TenVaiTro = "Học sinh" },
            new VaiTro { MaVaiTro = 4, MaCodeVaiTro = "nhan_vien", TenVaiTro = "Nhân viên/Giáo vụ" },
            new VaiTro { MaVaiTro = 5, MaCodeVaiTro = "hieu_truong", TenVaiTro = "Hiệu trưởng/BGH" },
            new VaiTro { MaVaiTro = 6, MaCodeVaiTro = "phu_huynh", TenVaiTro = "Phụ huynh" },
            new VaiTro { MaVaiTro = 7, MaCodeVaiTro = "sieu_quan_tri", TenVaiTro = "Siêu quản trị" },
            new VaiTro { MaVaiTro = 8, MaCodeVaiTro = "quan_tri_co_so", TenVaiTro = "Quản trị cơ sở" }
        };

        foreach (var role in roles)
        {
            var existingRole = await context.VaiTros
                .FirstOrDefaultAsync(x => x.MaCodeVaiTro == role.MaCodeVaiTro);

            if (existingRole is null)
            {
                context.VaiTros.Add(role);
                continue;
            }

            existingRole.TenVaiTro = role.TenVaiTro;
        }

        await context.SaveChangesAsync();
    }
}
