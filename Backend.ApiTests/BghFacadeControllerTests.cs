using System.Text.Json;
using Backend.Constants;
using Backend.Controllers;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests;

public class BghFacadeControllerTests
{
    [Test]
    public async Task MasterDataAndSchedules_ShouldRespectPrincipalCampusScope()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);

        var campusOneTerm = CreateTerm(1, 1, "HK-CAMPUS-ONE");
        var campusTwoTerm = CreateTerm(2, 2, "HK-CAMPUS-TWO");
        var campusOneCohort = CreateCohort(1, "COHORT-ONE");
        var campusTwoCohort = CreateCohort(2, "COHORT-TWO");
        var campusOneProgram = CreateProgram(1, campusOneCohort);
        var campusTwoProgram = CreateProgram(2, campusTwoCohort);
        var campusOneClass = CreateClass(1, 1, campusOneProgram);
        var campusTwoClass = CreateClass(2, 2, campusTwoProgram);
        var campusOneCourse = CreateCourse(1, 1, campusOneClass, campusOneTerm);
        var campusTwoCourse = CreateCourse(2, 2, campusTwoClass, campusTwoTerm);

        context.HocKys.AddRange(campusOneTerm, campusTwoTerm);
        context.KhoaTuyenSinhs.AddRange(campusOneCohort, campusTwoCohort);
        context.ChuongTrinhDaoTaos.AddRange(campusOneProgram, campusTwoProgram);
        context.LopHanhChinhs.AddRange(campusOneClass, campusTwoClass);
        context.KhoaHocs.AddRange(campusOneCourse, campusTwoCourse);
        context.ThoiKhoaBieus.AddRange(
            CreateSchedule(1, campusOneCourse),
            CreateSchedule(2, campusTwoCourse));
        await context.SaveChangesAsync();

        Assert.That(await context.KhoaHocs.CountAsync(), Is.EqualTo(2));
        Assert.That(await context.ThoiKhoaBieus.CountAsync(), Is.EqualTo(2));
        Assert.That(await context.ThoiKhoaBieus.Join(
            context.KhoaHocs,
            schedule => schedule.MaKhoaHoc,
            course => course.MaKhoaHoc,
            (schedule, course) => new { schedule, course })
            .CountAsync(x => x.course.MaDonVi == 1 && x.schedule.TrangThai == "nhap"), Is.EqualTo(1));

        var controller = CreatePrincipalController(context, 1);

        var termsJson = SerializeOk(await controller.GetAcademicTerms());
        var cohortsJson = SerializeOk(await controller.GetCohorts());
        var schedulesJson = SerializeOk(await controller.GetSchedules());

        Assert.Multiple(() =>
        {
            Assert.That(termsJson, Does.Contain("HK-CAMPUS-ONE"));
            Assert.That(termsJson, Does.Not.Contain("HK-CAMPUS-TWO"));
            Assert.That(cohortsJson, Does.Contain("COHORT-ONE"));
            Assert.That(cohortsJson, Does.Not.Contain("COHORT-TWO"));
            Assert.That(schedulesJson, Does.Contain("TKB-00001"));
            Assert.That(schedulesJson, Does.Not.Contain("TKB-00002"));
            Assert.That(schedulesJson, Does.Contain("pending"));
        });
    }

    private static BghFacadeController CreatePrincipalController(ApplicationDbContext context, int campusId)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Items["CurrentUser"] = new CurrentUserContext
        {
            UserId = 99,
            Email = "principal@test.local",
            Role = AuthRoles.Principal,
            CampusId = campusId,
            Status = "hoat_dong"
        };

        return new BghFacadeController(context)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }

    private static string SerializeOk(IActionResult result)
    {
        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        return JsonSerializer.Serialize(ok!.Value);
    }

    private static HocKy CreateTerm(int id, int campusId, string code) => new()
    {
        MaHocKy = id,
        MaDonVi = campusId,
        MaCodeHocKy = code,
        TenHocKy = code,
        NamHoc = "2026",
        ThuTuTrongNam = 1,
        NgayBatDau = new DateOnly(2026, 1, 1),
        NgayKetThuc = new DateOnly(2026, 4, 30)
    };

    private static KhoaTuyenSinh CreateCohort(int id, string code) => new()
    {
        MaKhoaTuyenSinh = id,
        MaCodeKhoa = code,
        TenKhoa = code,
        NamBatDau = 2026,
        ConHoatDong = true
    };

    private static ChuongTrinhDaoTao CreateProgram(int id, KhoaTuyenSinh cohort) => new()
    {
        MaChuongTrinh = id,
        MaChuyenNganh = id,
        MaKhoaTuyenSinh = cohort.MaKhoaTuyenSinh,
        MaCodeChuongTrinh = $"PROGRAM-{id}",
        TenChuongTrinh = $"Program {id}",
        Version = "1.0",
        SoHocKy = 6,
        ThoiGianDaoTaoThang = 36,
        TrangThai = "active",
        ConHoatDong = true,
        KhoaTuyenSinh = cohort,
        ChuyenNganh = new ChuyenNganh
        {
            MaChuyenNganh = id,
            MaNganh = id,
            TenChuyenNganh = $"Specialization {id}",
            ConHoatDong = true
        }
    };

    private static LopHanhChinh CreateClass(int id, int campusId, ChuongTrinhDaoTao program)
    {
        var result = new LopHanhChinh
        {
            MaLop = id,
            MaDonVi = campusId,
            MaChuongTrinh = program.MaChuongTrinh,
            MaCodeLop = $"CLASS-{id}",
            TenLop = $"Class {id}",
            ConHoatDong = true,
            ChuongTrinh = program
        };
        program.LopHanhChinhs.Add(result);
        return result;
    }

    private static KhoaHoc CreateCourse(int id, int campusId, LopHanhChinh administrativeClass, HocKy term)
    {
        var campus = new DonVi
        {
            MaDonVi = campusId,
            TenDonVi = $"Campus {campusId}",
            CapDonVi = "co_so",
            ConHoatDong = true
        };
        var teacher = new NguoiDung
        {
            MaNguoiDung = id,
            MaDonVi = campusId,
            Email = $"teacher{id}@test.local",
            HoTen = $"Teacher {id}",
            VaiTroChinh = "giao_vien",
            TrangThai = "hoat_dong",
            DonVi = campus
        };
        var subject = new DanhMucMonHoc
        {
            MaMonHoc = id,
            MaCodeMonHoc = $"SUBJECT-{id}",
            TenMonHoc = $"Subject {id}",
            SoTinChi = 3,
            ConHoatDong = true
        };

        return new KhoaHoc
        {
            MaKhoaHoc = id,
            MaDonVi = campusId,
            MaGiaoVien = teacher.MaNguoiDung,
            MaMonHoc = subject.MaMonHoc,
            MaHocKy = term.MaHocKy,
            MaLop = administrativeClass.MaLop,
            TieuDe = $"Course {id}",
            TrangThai = "da_xuat_ban",
            DonVi = campus,
            GiaoVien = teacher,
            MonHoc = subject,
            Lop = administrativeClass,
            HocKy = term
        };
    }

    private static ThoiKhoaBieu CreateSchedule(int id, KhoaHoc course)
    {
        var room = new PhongHoc
        {
            MaPhong = id,
            MaDonVi = course.MaDonVi,
            MaTang = id,
            MaCodePhong = $"ROOM-{id}",
            TenPhong = $"Room {id}",
            LoaiPhong = "ly_thuyet"
        };
        var shift = new CaHoc
        {
            MaCaHoc = id,
            TenCa = $"Shift {id}",
            Buoi = "sang",
            GioBatDau = new TimeOnly(7, 0),
            GioKetThuc = new TimeOnly(9, 0),
            ThuTu = id,
            ConHoatDong = true
        };

        return new ThoiKhoaBieu
        {
            MaTkb = id,
            MaKhoaHoc = course.MaKhoaHoc,
            MaPhong = room.MaPhong,
            MaCaHoc = shift.MaCaHoc,
            ThuTrongTuan = 2,
            TrangThai = "nhap",
            KhoaHoc = course,
            Phong = room,
            CaHoc = shift
        };
    }
}
