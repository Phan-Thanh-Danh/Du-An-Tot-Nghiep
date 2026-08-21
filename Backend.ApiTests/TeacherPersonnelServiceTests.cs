using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.TeacherPersonnel;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Audit;
using Backend.Services.TeacherPersonnel;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
public class TeacherPersonnelServiceTests
{
    private ApplicationDbContext CreateInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    [Test]
    public async Task GetTeachersAsync_ShouldReturnTeachersScopedToCampus()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateInMemoryDbContext(dbName);
        var mockAudit = new Mock<IAuditLogService>();

        // Seed DonVi
        context.DonVis.AddRange(
            new DonVi { MaDonVi = 1, TenDonVi = "Cơ sở Hà Nội", CapDonVi = "co_so", ConHoatDong = true },
            new DonVi { MaDonVi = 2, TenDonVi = "Cơ sở HCM", CapDonVi = "co_so", ConHoatDong = true }
        );

        // Seed Users
        context.NguoiDungs.AddRange(
            new NguoiDung { MaNguoiDung = 101, HoTen = "GV HN 1", Email = "gvhn1@edulms.local", VaiTroChinh = "giao_vien", MaDonVi = 1, TrangThai = "hoat_dong" },
            new NguoiDung { MaNguoiDung = 102, HoTen = "GV HN 2", Email = "gvhn2@edulms.local", VaiTroChinh = "giao_vien", MaDonVi = 1, TrangThai = "hoat_dong" },
            new NguoiDung { MaNguoiDung = 201, HoTen = "GV HCM 1", Email = "gvhcm1@edulms.local", VaiTroChinh = "giao_vien", MaDonVi = 2, TrangThai = "hoat_dong" }
        );
        await context.SaveChangesAsync();

        var service = new TeacherPersonnelService(context, mockAudit.Object);
        var principalUser = new CurrentUserContext { UserId = 99, Role = "Principal", CampusId = 1 };

        // Act
        var result = await service.GetTeachersAsync(principalUser, new TeacherPersonnelQueryParameters());

        // Assert
        Assert.That(result.TotalItems, Is.EqualTo(2));
        Assert.That(result.Items.All(t => t.MaDonVi == 1), Is.True);
    }

    [Test]
    public async Task CreateTeacher_ShouldSaveToDbAndLogAudit()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateInMemoryDbContext(dbName);
        var mockAudit = new Mock<IAuditLogService>();

        context.DonVis.Add(new DonVi { MaDonVi = 1, TenDonVi = "Cơ sở Hà Nội", CapDonVi = "co_so", ConHoatDong = true });
        context.ChuyenNganhs.Add(new ChuyenNganh { MaChuyenNganh = 10, TenChuyenNganh = "Lập trình Web", ConHoatDong = true });
        context.DanhMucMonHocs.Add(new DanhMucMonHoc { MaMonHoc = 100, TenMonHoc = "Vue 3", MaCodeMonHoc = "WEB101", SoTinChi = 3, ConHoatDong = true });
        await context.SaveChangesAsync();

        var service = new TeacherPersonnelService(context, mockAudit.Object);
        var principalUser = new CurrentUserContext { UserId = 99, Role = "Principal", CampusId = 1 };

        var request = new CreateTeacherPersonnelRequestDto
        {
            HoTen = "Nguyễn Văn Giảng Viên Mới",
            Email = "gvmoi@edulms.local",
            MatKhau = "123456",
            MaDonVi = 1,
            MaChuyenNganhChinh = 10,
            DanhSachMonDuocPhepDay = [100]
        };

        // Act
        var detail = await service.CreateTeacherAsync(principalUser, request);

        // Assert
        Assert.That(detail, Is.Not.Null);
        Assert.That(detail.HoTen, Is.EqualTo("Nguyễn Văn Giảng Viên Mới"));
        Assert.That(detail.Email, Is.EqualTo("gvmoi@edulms.local"));
        Assert.That(detail.ChuyenNganhList.Count, Is.EqualTo(1));
        Assert.That(detail.MonHocList.Count, Is.EqualTo(1));

        // Check DB
        var savedInDb = await context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == "gvmoi@edulms.local");
        Assert.That(savedInDb, Is.Not.Null);
        Assert.That(savedInDb!.VaiTroChinh, Is.EqualTo("giao_vien"));

        // Check Audit Call
        mockAudit.Verify(a => a.AddAsync(
            1,
            "GiangVien",
            savedInDb.MaNguoiDung,
            "CREATE_TEACHER",
            99,
            null,
            It.IsAny<object>(),
            default), Times.Once);
    }

    [Test]
    public async Task ToggleLockTeacher_ShouldUpdateStatusAndLogAudit()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateInMemoryDbContext(dbName);
        var mockAudit = new Mock<IAuditLogService>();

        context.DonVis.Add(new DonVi { MaDonVi = 1, TenDonVi = "Cơ sở Hà Nội", CapDonVi = "co_so", ConHoatDong = true });
        context.NguoiDungs.Add(new NguoiDung { MaNguoiDung = 105, HoTen = "GV Khóa", Email = "gvkhoa@edulms.local", VaiTroChinh = "giao_vien", MaDonVi = 1, TrangThai = "hoat_dong" });
        await context.SaveChangesAsync();

        var service = new TeacherPersonnelService(context, mockAudit.Object);
        var principalUser = new CurrentUserContext { UserId = 99, Role = "Principal", CampusId = 1 };

        // Act - Lock
        var lockResult = await service.ToggleLockTeacherAsync(principalUser, 105, new ToggleTeacherLockRequestDto { LyDo = "Tạm dừng công tác" });
        Assert.That(lockResult, Is.True);

        var teacherLocked = await context.NguoiDungs.FindAsync(105);
        Assert.That(teacherLocked!.TrangThai, Is.EqualTo("bi_khoa"));

        // Act - Unlock
        var unlockResult = await service.ToggleLockTeacherAsync(principalUser, 105, new ToggleTeacherLockRequestDto { LyDo = "Mở lại hoạt động" });
        Assert.That(unlockResult, Is.True);

        var teacherUnlocked = await context.NguoiDungs.FindAsync(105);
        Assert.That(teacherUnlocked!.TrangThai, Is.EqualTo("hoat_dong"));
    }

    [Test]
    public async Task GetHierarchyTreeAsync_ShouldReturnHierarchicalNodes()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateInMemoryDbContext(dbName);
        var mockAudit = new Mock<IAuditLogService>();

        context.DonVis.Add(new DonVi { MaDonVi = 1, TenDonVi = "Cơ sở Hà Nội", CapDonVi = "co_so", ConHoatDong = true });
        context.VaiTros.AddRange(
            new VaiTro { MaVaiTro = 1, MaCodeVaiTro = "giao_vien", TenVaiTro = "Giảng viên" },
            new VaiTro { MaVaiTro = 2, MaCodeVaiTro = "hoc_sinh", TenVaiTro = "Sinh viên" }
        );
        context.NguoiDungs.AddRange(
            new NguoiDung { MaNguoiDung = 1, HoTen = "GV 1", Email = "gv1@lms.local", VaiTroChinh = "giao_vien", MaDonVi = 1, TrangThai = "hoat_dong" },
            new NguoiDung { MaNguoiDung = 2, HoTen = "SV 1", Email = "sv1@lms.local", VaiTroChinh = "hoc_sinh", MaDonVi = 1, TrangThai = "hoat_dong" }
        );
        await context.SaveChangesAsync();

        var service = new TeacherPersonnelService(context, mockAudit.Object);
        var principalUser = new CurrentUserContext { UserId = 99, Role = "Principal", CampusId = 1 };

        // Act
        var tree = await service.GetHierarchyTreeAsync(principalUser);

        // Assert
        Assert.That(tree.Count, Is.EqualTo(1));
        var orgNode = tree[0];
        Assert.That(orgNode.Id, Is.EqualTo("org-1"));
        Assert.That(orgNode.TotalMembers, Is.EqualTo(2));
        Assert.That(orgNode.Children.Count, Is.EqualTo(2));
    }
}
