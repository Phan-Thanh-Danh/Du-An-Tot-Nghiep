using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class InspectAllTeachersTest
    {
        [Test]
        public async Task ListAllTeachers()
        {
            var connStr = "Server=localhost,1433;Database=LMS;User Id=sa;Password=Test@123_PassWord!;TrustServerCertificate=True;";
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            var teachers = await db.NguoiDungs
                .Where(u => u.VaiTroChinh == "giao_vien" || u.VaiTroChinh == "Teacher" || u.Email.Contains("teacher") || u.Email.Contains("lecturer"))
                .Take(20)
                .ToListAsync();

            foreach (var t in teachers)
            {
                TestContext.Progress.WriteLine($"Teacher ID={t.MaNguoiDung}, Name=\"{t.HoTen}\", Email=\"{t.Email}\"");
            }
        }
    }
}
