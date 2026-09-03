using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.TeacherPersonnel;
using Backend.Services.AdminUsers;
using Backend.Services.Audit;
using Backend.Services.TeacherPersonnel;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class TestTeacherUpdateRemoveSubject
    {
        [Test]
        public async Task TestRemoveSubjectFromTeacher()
        {
            var connStr = TestDatabaseSafetyGuard.GetVerifiedTestConnectionString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            var lecturer = await db.NguoiDungs.FirstOrDefaultAsync(u => u.Email == "lecturer01@edulms.local");
            Assert.That(lecturer, Is.Not.Null);

            var bghUser = await db.NguoiDungs.FirstOrDefaultAsync(u => u.VaiTroChinh == "hieu_truong" || u.VaiTroChinh == "Principal" || u.Email.Contains("admin"));
            Assert.That(bghUser, Is.Not.Null);

            var currentUser = new CurrentUserContext
            {
                UserId = bghUser.MaNguoiDung,
                Email = bghUser.Email,
                Role = "Principal",
                CampusId = bghUser.MaDonVi
            };

            var auditLog = Moq.Mock.Of<IAuditLogService>();
            var service = new TeacherPersonnelService(db, auditLog);

            // Get current detail
            var detail = await service.GetTeacherDetailAsync(currentUser, lecturer.MaNguoiDung, default);
            TestContext.Progress.WriteLine($"Lecturer: {detail.HoTen}, Current Subjects Count: {detail.MonHocList.Count}");
            foreach (var m in detail.MonHocList)
            {
                TestContext.Progress.WriteLine($" - Subject: ID={m.MaMonHoc}, Name={m.TenMonHoc}");
            }

            // Let's see what happens if we remove COM102 (Cơ sở dữ liệu)
            var com102 = detail.MonHocList.FirstOrDefault(m => m.TenMonHoc.Contains("Cơ sở dữ liệu") || m.MaCodeMonHoc == "COM102");
            var keepSubjects = detail.MonHocList
                .Where(m => m.MaMonHoc != com102?.MaMonHoc)
                .Select(m => new UpdateTeacherSubjectItemDto
                {
                    MaMonHoc = m.MaMonHoc,
                    MucDoPhuHop = m.MucDoPhuHop,
                    SoNamKinhNghiem = m.SoNamKinhNghiem,
                    LaMonChinh = m.LaMonChinh,
                    ConHoatDong = m.ConHoatDong
                })
                .ToList();

            var req = new UpdateTeacherPersonnelRequestDto
            {
                HoTen = detail.HoTen,
                SoDienThoai = detail.SoDienThoai,
                TrangThai = detail.TrangThai,
                MaChuyenNganhChinh = detail.ChuyenNganhList.FirstOrDefault(c => c.LaChuyenMonChinh)?.MaChuyenNganh,
                DanhSachMonHoc = keepSubjects,
                LyDo = "Test BGH remove subject"
            };

            try
            {
                var updateRes = await service.UpdateTeacherAsync(currentUser, lecturer.MaNguoiDung, req, default);
                TestContext.Progress.WriteLine($"Update succeeded! New subjects count: {updateRes.MonHocList.Count}");
            }
            catch (Exception ex)
            {
                TestContext.Progress.WriteLine($"Update FAILED with exception: {ex.GetType().Name} - {ex.Message}");
                TestContext.Progress.WriteLine($"Stack: {ex.StackTrace}");
            }
        }
    }
}
