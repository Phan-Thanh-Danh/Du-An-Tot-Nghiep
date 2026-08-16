using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class SubjectListInspector
    {
        [Test]
        public async Task ListAllSubjectsInDb()
        {
            var connStr = "Server=localhost,1433;Database=LMS;User Id=sa;Password=Test@123_PassWord!;TrustServerCertificate=True;";
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            var subjects = await db.DanhMucMonHocs
                .Include(m => m.Nganh)
                .Include(m => m.ChuyenNganh)
                .OrderBy(m => m.MaMonHoc)
                .ToListAsync();

            TestContext.Progress.WriteLine($"=== TOTAL SUBJECTS IN DB: {subjects.Count} ===");
            foreach (var s in subjects)
            {
                TestContext.Progress.WriteLine($"ID: {s.MaMonHoc} | Code: {s.MaCodeMonHoc} | Name: {s.TenMonHoc} | Nganh: {s.Nganh?.TenNganh} | ChuyenNganh: {s.ChuyenNganh?.TenChuyenNganh}");
            }
        }
    }
}
