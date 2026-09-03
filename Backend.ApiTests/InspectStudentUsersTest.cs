using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class InspectStudentUsersTest
    {
        [Test]
        public async Task InspectStudents()
        {
            var connStr = TestDatabaseSafetyGuard.GetVerifiedTestConnectionString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            var students = await db.NguoiDungs
                .Where(u => u.Email != null && u.Email.Contains("student"))
                .Take(20)
                .ToListAsync();

            foreach (var s in students)
            {
                TestContext.Progress.WriteLine($"Student ID={s.MaNguoiDung}, Email=\"{s.Email}\", Name=\"{s.HoTen}\", MaLop={s.MaLop}, Status={s.TrangThai}");
            }
        }
    }
}
