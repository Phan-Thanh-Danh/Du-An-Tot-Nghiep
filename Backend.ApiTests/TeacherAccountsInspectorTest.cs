using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class TeacherAccountsInspectorTest
    {
        [Test]
        public async Task ListTeacherAccounts()
        {
            var connStr = TestDatabaseSafetyGuard.GetVerifiedTestConnectionString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            var teachers = await db.NguoiDungs
                .Where(u => u.VaiTroChinh.Contains("Teacher") || u.VaiTroChinh.Contains("giao_vien") || u.Email.Contains("lecturer") || u.Email.Contains("teacher"))
                .ToListAsync();

            TestContext.Progress.WriteLine($"Found {teachers.Count} teachers:");
            foreach (var t in teachers)
            {
                TestContext.Progress.WriteLine($"ID={t.MaNguoiDung}, Name={t.HoTen}, Email={t.Email}, Role={t.VaiTroChinh}");
            }
        }
    }
}
