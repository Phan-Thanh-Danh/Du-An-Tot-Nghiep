using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class FirstUsersInspectorTest
    {
        [Test]
        public async Task ListFirstUsers()
        {
            var connStr = TestDatabaseSafetyGuard.GetVerifiedTestConnectionString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            var users = await db.NguoiDungs
                .Where(u => u.MaNguoiDung <= 35)
                .OrderBy(u => u.MaNguoiDung)
                .ToListAsync();

            foreach (var u in users)
            {
                TestContext.Progress.WriteLine($"ID={u.MaNguoiDung}, Name={u.HoTen}, Email={u.Email}, Role={u.VaiTroChinh}, ClassID={u.MaLop}");
            }
        }
    }
}
