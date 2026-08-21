using System.Text;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.Models;
using Backend.Services.AdminUsers;
using Backend.Services.Audit;
using Backend.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using OfficeOpenXml;

namespace Backend.ApiTests;

public class UserBulkImportServiceTests
{
    [Test]
    public async Task TeacherPersonnelDryRun_ShouldValidateWithoutWritingDatabase()
    {
        await using var context = CreateContext();
        await SeedReferenceDataAsync(context);
        var service = CreateService(context, AuthRoles.Principal, campusId: 1);
        var file = CreateCsvFile(
            "Email,HoTen,MatKhau,MaCodeVaiTro,MaDonVi,SoDienThoai\n" +
            "teacher.one@lms.local,Giảng viên Một,Strong@123,Teacher,1,0901234567");

        var result = await service.ImportAsync(file, true, null);

        Assert.Multiple(() =>
        {
            Assert.That(result.TongSoDong, Is.EqualTo(1));
            Assert.That(result.SoDongHopLe, Is.EqualTo(1));
            Assert.That(result.SoDongLoi, Is.Zero);
            Assert.That(result.SoDongDaNhap, Is.Zero);
            Assert.That(result.SoDongTaoMoi, Is.EqualTo(1));
            Assert.That(result.SoDongCapNhat, Is.Zero);
            Assert.That(result.DaLuu, Is.False);
        });
        Assert.That(await context.NguoiDungs.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task TeacherPersonnelDryRun_ShouldReadRealXlsxStream()
    {
        await using var context = CreateContext();
        await SeedReferenceDataAsync(context);
        var service = CreateService(context, AuthRoles.Principal, campusId: 1);
        var file = CreateXlsxFile();

        var result = await service.ImportAsync(file, true, null);

        Assert.Multiple(() =>
        {
            Assert.That(result.TongSoDong, Is.EqualTo(1));
            Assert.That(result.SoDongHopLe, Is.EqualTo(1));
            Assert.That(result.SoDongLoi, Is.Zero);
        });
        Assert.That(await context.NguoiDungs.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task TeacherPersonnelImport_ShouldRejectWrongRoleAndCampusAndWriteNothing()
    {
        await using var context = CreateContext();
        await SeedReferenceDataAsync(context);
        // Use campus 3 so it is NOT a global admin and campus 1 is outside scope
        context.DonVis.Add(new DonVi { MaDonVi = 3, TenDonVi = "Cơ sở 3", CapDonVi = "co_so", ConHoatDong = true });
        await context.SaveChangesAsync();
        var service = CreateService(context, AuthRoles.AcademicStaff, campusId: 3);
        var file = CreateCsvFile(
            "Email,HoTen,MatKhau,MaCodeVaiTro,MaDonVi\n" +
            "admin@lms.local,Quản trị Sai,Strong@123,SuperAdmin,3\n" +
            "teacher.two@lms.local,Giảng viên Sai Cơ Sở,Strong@123,Teacher,1");

        var result = await service.ImportAsync(file, false, null);

        Assert.Multiple(() =>
        {
            Assert.That(result.SoDongLoi, Is.EqualTo(2));
            Assert.That(result.SoDongDaNhap, Is.Zero);
            Assert.That(result.DaLuu, Is.False);
            Assert.That(result.ChiTietLoi.Any(x => x.LyDo.Contains("không được gán BGH/Admin/SuperAdmin")), Is.True);
            Assert.That(result.ChiTietLoi.Any(x => x.LyDo.Contains("không có quyền")), Is.True);
        });
        Assert.That(await context.NguoiDungs.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task TeacherPersonnelImport_ShouldPersistUsersAssignmentsAndHashedPasswordsWhenAllRowsAreValid()
    {
        await using var context = CreateContext();
        await SeedReferenceDataAsync(context);
        var service = CreateService(context, AuthRoles.Principal, campusId: 1);
        var file = CreateCsvFile(
            "Email,HoTen,MatKhau,MaCodeVaiTro,MaDonVi\n" +
            "teacher.saved@lms.local,Giảng viên Đã Lưu,Strong@123,giao_vien,1");

        var result = await service.ImportAsync(file, false, null);

        var user = await context.NguoiDungs.SingleAsync();
        var assignment = await context.PhanQuyenNguoiDungs.SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(result.SoDongDaNhap, Is.EqualTo(1));
            Assert.That(result.SoDongTaoMoi, Is.EqualTo(1));
            Assert.That(result.SoDongCapNhat, Is.Zero);
            Assert.That(result.DaLuu, Is.True);
            Assert.That(user.Email, Is.EqualTo("teacher.saved@lms.local"));
            Assert.That(user.MaDonVi, Is.EqualTo(1));
            Assert.That(user.MatKhauHash, Is.Not.EqualTo("Strong@123"));
            Assert.That(user.MatKhauHash, Is.Not.Null.And.Not.Empty);
            Assert.That(assignment.MaNguoiDung, Is.EqualTo(user.MaNguoiDung));
            Assert.That(assignment.MaVaiTro, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Import_ShouldMatchExistingEmailThenValidateAndUpdateAllOtherFields()
    {
        await using var context = CreateContext();
        await SeedReferenceDataAsync(context);
        var existing = new NguoiDung
        {
            MaNguoiDung = 20,
            MaDonVi = 1,
            Email = "existing@lms.local",
            HoTen = "Tên cũ",
            VaiTroChinh = "giao_vien",
            SoDienThoai = "0900000000",
            MatKhauHash = "old-hash",
            TrangThai = UserStatuses.DbActive,
            NgayTao = DateTime.UtcNow
        };
        context.NguoiDungs.Add(existing);
        context.PhanQuyenNguoiDungs.Add(new PhanQuyenNguoiDung
        {
            MaNguoiDung = 20,
            MaVaiTro = 1,
            NgayGan = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
        var service = CreateService(context, AuthRoles.Principal, campusId: 1);
        var file = CreateCsvFile(
            "Email,HoTen,MatKhau,MaCodeVaiTro,MaDonVi,SoDienThoai\n" +
            "existing@lms.local,Tên đã cập nhật,Changed@123,Student,1,0999999999");

        var result = await service.ImportAsync(file, false, null);

        var updated = await context.NguoiDungs.SingleAsync();
        var assignment = await context.PhanQuyenNguoiDungs.SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(result.SoDongLoi, Is.Zero);
            Assert.That(result.SoDongDaNhap, Is.EqualTo(1));
            Assert.That(result.SoDongTaoMoi, Is.Zero);
            Assert.That(result.SoDongCapNhat, Is.EqualTo(1));
            Assert.That(updated.HoTen, Is.EqualTo("Tên đã cập nhật"));
            Assert.That(updated.SoDienThoai, Is.EqualTo("0999999999"));
            Assert.That(updated.VaiTroChinh, Is.EqualTo("hoc_sinh"));
            Assert.That(updated.MatKhauHash, Is.Not.EqualTo("old-hash").And.Not.EqualTo("Changed@123"));
            Assert.That(assignment.MaVaiTro, Is.EqualTo(2));
        });
        Assert.That(await context.NguoiDungs.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task Import_ShouldAcceptTeacherStudentAndAcademicStaffRoles()
    {
        await using var context = CreateContext();
        await SeedReferenceDataAsync(context);
        var service = CreateService(context, AuthRoles.Principal, campusId: 1);
        var file = CreateCsvFile(
            "Email,HoTen,MatKhau,MaCodeVaiTro,MaDonVi\n" +
            "teacher@lms.local,Giảng viên,Teacher@123,Teacher,1\n" +
            "student@lms.local,Sinh viên,Student@123,Student,1\n" +
            "staff@lms.local,Giáo vụ,Staff@123,AcademicStaff,1");

        var result = await service.ImportAsync(file, false, null);

        Assert.Multiple(() =>
        {
            Assert.That(result.SoDongDaNhap, Is.EqualTo(3));
            Assert.That(result.SoDongTaoMoi, Is.EqualTo(3));
            Assert.That(result.SoDongCapNhat, Is.Zero);
            Assert.That(result.SoDongLoi, Is.Zero);
        });
        Assert.That(await context.NguoiDungs.Select(x => x.VaiTroChinh).ToListAsync(),
            Is.EquivalentTo(new[] { "giao_vien", "hoc_sinh", "nhan_vien" }));
    }

    [Test]
    public async Task Import_ShouldRejectDuplicateEmailInsideFileAndWriteNothing()
    {
        await using var context = CreateContext();
        await SeedReferenceDataAsync(context);
        var service = CreateService(context, AuthRoles.Principal, campusId: 1);
        var file = CreateCsvFile(
            "Email,HoTen,MatKhau,MaCodeVaiTro,MaDonVi\n" +
            "same@lms.local,Dòng Một,Strong@123,Teacher,1\n" +
            "SAME@lms.local,Dòng Hai,Strong@123,Teacher,1");

        var result = await service.ImportAsync(file, false, null);

        Assert.That(result.ChiTietLoi.Any(x => x.LyDo.Contains("trùng trong file")), Is.True);
        Assert.That(result.SoDongDaNhap, Is.Zero);
        Assert.That(await context.NguoiDungs.CountAsync(), Is.Zero);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task SeedReferenceDataAsync(ApplicationDbContext context)
    {
        context.DonVis.AddRange(
            new DonVi { MaDonVi = 1, TenDonVi = "Cơ sở 1", CapDonVi = "co_so", ConHoatDong = true },
            new DonVi { MaDonVi = 2, TenDonVi = "Cơ sở 2", CapDonVi = "co_so", ConHoatDong = true });
        context.VaiTros.AddRange(
            new VaiTro { MaVaiTro = 1, MaCodeVaiTro = "giao_vien", TenVaiTro = "Giảng viên" },
            new VaiTro { MaVaiTro = 2, MaCodeVaiTro = "hoc_sinh", TenVaiTro = "Sinh viên" },
            new VaiTro { MaVaiTro = 3, MaCodeVaiTro = "sieu_quan_tri", TenVaiTro = "Siêu quản trị" },
            new VaiTro { MaVaiTro = 4, MaCodeVaiTro = "nhan_vien", TenVaiTro = "Giáo vụ" });
        await context.SaveChangesAsync();
    }

    private static UserBulkImportService CreateService(ApplicationDbContext context, string role, int campusId)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Items["CurrentUser"] = new CurrentUserContext
        {
            UserId = 99,
            Email = "actor@lms.local",
            Role = role,
            CampusId = campusId,
            Status = UserStatuses.DbActive
        };
        var accessor = new HttpContextAccessor { HttpContext = httpContext };
        var audit = new Mock<IAuditLogService>();
        return new UserBulkImportService(context, new PasswordHasherService(), audit.Object, accessor);
    }

    private static IFormFile CreateCsvFile(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", "users.csv")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/csv"
        };
    }

    private static IFormFile CreateXlsxFile()
    {
        ExcelPackage.License.SetNonCommercialOrganization("LMS Academic Management System Tests");
        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("NguoiDung");
        sheet.Cells[1, 1].Value = "Email";
        sheet.Cells[1, 2].Value = "HoTen";
        sheet.Cells[1, 3].Value = "MatKhau";
        sheet.Cells[1, 4].Value = "MaCodeVaiTro";
        sheet.Cells[1, 5].Value = "MaDonVi";
        sheet.Cells[2, 1].Value = "teacher.xlsx@lms.local";
        sheet.Cells[2, 2].Value = "Giảng viên Excel";
        sheet.Cells[2, 3].Value = "Strong@123";
        sheet.Cells[2, 4].Value = "Teacher";
        sheet.Cells[2, 5].Value = 1;
        var bytes = package.GetAsByteArray();
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", "users.xlsx")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };
    }
}
