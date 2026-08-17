using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class QuestionBankInspector
    {
        [Test]
        public async Task TestQuestionBank()
        {
            var connStr = "Server=localhost,1433;Database=LMS;User Id=sa;Password=Test@123_PassWord!;TrustServerCertificate=True;";
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            var questions = await db.CauHois.ToListAsync();
            TestContext.Progress.WriteLine($"Total questions in DB: {questions.Count}");

            foreach (var q in questions.Take(10))
            {
                TestContext.Progress.WriteLine($"ID={q.MaCauHoi}, MonHocID={q.MaMonHoc}, Content={q.NoiDung}, Choices={q.LuaChon}, Answer={q.DapAnDung}");
            }
        }
    }
}
