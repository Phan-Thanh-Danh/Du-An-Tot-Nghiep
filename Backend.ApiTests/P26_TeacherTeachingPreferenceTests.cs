using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Configuration;
using Backend.Data;
using Backend.DTOs.AcademicSchedulingContext;
using Backend.DTOs.TeachingPreferences;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.AcademicSchedulingContext;
using Backend.Services.TeachingPreferences;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
public class P26_TeacherTeachingPreferenceTests
{
    private ApplicationDbContext _db;
    private Mock<IAcademicSchedulingContextService> _mockSchedulingContext;
    private TeachingPreferenceService _service;
    private TimeProvider _timeProvider;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _db = new ApplicationDbContext(options);

        _mockSchedulingContext = new Mock<IAcademicSchedulingContextService>();
        _timeProvider = TimeProvider.System;
        
        var optionsMock = Options.Create(new TeachingPreferenceOptions
        {
            OpenDaysBeforeTermStart = 30,
            DeadlineDaysBeforeTermStart = 14
        });

        _service = new TeachingPreferenceService(_db, _mockSchedulingContext.Object, _timeProvider, optionsMock);
    }

    [TearDown]
    public void TearDown()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    [Test]
    public async Task SaveDraft_ShouldCreateNewPreference_IfNoneExists()
    {
        // Arrange
        int teacherId = 1;
        int maHocKy = 1;

        _db.NguoiDungs.Add(new NguoiDung { MaNguoiDung = teacherId, MaDonVi = 1, HoTen = "Teacher A", VaiTroChinh = "Teacher" });
        _db.HocKys.Add(new HocKy { MaHocKy = maHocKy, MaDonVi = 1, TenHocKy = "HK1", NgayBatDau = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)) });
        _db.CaHocs.AddRange(new CaHoc { MaCaHoc = 1, TenCa = "Ca 1", ConHoatDong = true, GioBatDau = new TimeOnly(7, 0), GioKetThuc = new TimeOnly(9, 0) }, new CaHoc { MaCaHoc = 2, TenCa = "Ca 2", ConHoatDong = true, GioBatDau = new TimeOnly(9, 0), GioKetThuc = new TimeOnly(11, 0) }); await _db.SaveChangesAsync();

        _mockSchedulingContext.Setup(x => x.GetContextAsync(1)).ReturnsAsync(new AcademicSchedulingContextDto
        {
            SchedulableTerm = new SchedulingTermDto { MaHocKy = maHocKy, TenHocKy = "HK1" }
        });

        var dto = new UpdateTeachingPreferenceDto
        {
            SoLopToiDaMongMuon = 5,
            Slots = new List<TeachingPreferenceSlotDto>
            {
                new TeachingPreferenceSlotDto { ThuTrongTuan = 2, MaCaHoc = 1, MucDo = "preferred" }
            }
        };

        // Act
        var result = await _service.SaveDraftAsync(teacherId, maHocKy, dto);

        // Assert
        Assert.That(result.TrangThai, Is.EqualTo("draft"));
        Assert.That(result.SoLopToiDaMongMuon, Is.EqualTo(5));
        Assert.That(result.Slots.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task Submit_ShouldChangeStatusToSubmitted_AndSetNgayGui()
    {
        // Arrange
        int teacherId = 2;
        int maHocKy = 2;

        _db.NguoiDungs.Add(new NguoiDung { MaNguoiDung = teacherId, MaDonVi = 1, HoTen = "Teacher B", VaiTroChinh = "Teacher" });
        _db.HocKys.Add(new HocKy { MaHocKy = maHocKy, MaDonVi = 1, TenHocKy = "HK2", NgayBatDau = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)) });
        _db.CaHocs.AddRange(new CaHoc { MaCaHoc = 1, TenCa = "Ca 1", ConHoatDong = true, GioBatDau = new TimeOnly(7, 0), GioKetThuc = new TimeOnly(9, 0) }, new CaHoc { MaCaHoc = 2, TenCa = "Ca 2", ConHoatDong = true, GioBatDau = new TimeOnly(9, 0), GioKetThuc = new TimeOnly(11, 0) }); await _db.SaveChangesAsync();

        _mockSchedulingContext.Setup(x => x.GetContextAsync(1)).ReturnsAsync(new AcademicSchedulingContextDto
        {
            SchedulableTerm = new SchedulingTermDto { MaHocKy = maHocKy, TenHocKy = "HK2" }
        });

        var dto = new SubmitTeachingPreferenceDto();

        // Act
        var result = await _service.SubmitAsync(teacherId, maHocKy, dto);

        // Assert
        Assert.That(result.TrangThai, Is.EqualTo("submitted"));
        Assert.That(result.NgayGui, Is.Not.Null);
    }
    
    [Test]
    public void SaveDraft_ShouldThrow_IfPastDeadline()
    {
        // Arrange
        int teacherId = 3;
        int maHocKy = 3;

        _db.NguoiDungs.Add(new NguoiDung { MaNguoiDung = teacherId, MaDonVi = 1, HoTen = "Teacher C", VaiTroChinh = "Teacher" });
        // Term starts in 5 days, so deadline was 14 days before start (i.e. 9 days ago), thus we are past deadline.
        _db.HocKys.Add(new HocKy { MaHocKy = maHocKy, MaDonVi = 1, TenHocKy = "HK3", NgayBatDau = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)) });
        _db.SaveChanges();

        _mockSchedulingContext.Setup(x => x.GetContextAsync(1)).ReturnsAsync(new AcademicSchedulingContextDto
        {
            SchedulableTerm = new SchedulingTermDto { MaHocKy = maHocKy, TenHocKy = "HK3" }
        });

        // Act & Assert
        var ex = Assert.ThrowsAsync<ApiException>(async () => await _service.SaveDraftAsync(teacherId, maHocKy, new UpdateTeachingPreferenceDto()));
        Assert.That(ex.Message, Does.Contain("hạn"));
    }

    [Test]
    public async Task SaveDraft_ShouldRemoveSlots_IfNotInPayload()
    {
        // Arrange
        int teacherId = 4;
        int maHocKy = 4;

        _db.NguoiDungs.Add(new NguoiDung { MaNguoiDung = teacherId, MaDonVi = 1, HoTen = "Teacher D", VaiTroChinh = "Teacher" });
        _db.HocKys.Add(new HocKy { MaHocKy = maHocKy, MaDonVi = 1, TenHocKy = "HK4", NgayBatDau = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)) });
        
        var pref = new GiaoVienNguyenVongHocKy
        {
            MaGiaoVien = teacherId,
            MaHocKy = maHocKy,
            MaDonVi = 1,
            TrangThai = "draft",
            ChiTietNguyenVong = new List<GiaoVienNguyenVongCaDay>
            {
                new GiaoVienNguyenVongCaDay { ThuTrongTuan = 2, MaCaHoc = 1, MucDo = "preferred" },
                new GiaoVienNguyenVongCaDay { ThuTrongTuan = 2, MaCaHoc = 2, MucDo = "available" }
            }
        };
        _db.GiaoVienNguyenVongHocKys.Add(pref);
        _db.CaHocs.AddRange(new CaHoc { MaCaHoc = 1, TenCa = "Ca 1", ConHoatDong = true, GioBatDau = new TimeOnly(7, 0), GioKetThuc = new TimeOnly(9, 0) }, new CaHoc { MaCaHoc = 2, TenCa = "Ca 2", ConHoatDong = true, GioBatDau = new TimeOnly(9, 0), GioKetThuc = new TimeOnly(11, 0) }); await _db.SaveChangesAsync();

        _mockSchedulingContext.Setup(x => x.GetContextAsync(1)).ReturnsAsync(new AcademicSchedulingContextDto
        {
            SchedulableTerm = new SchedulingTermDto { MaHocKy = maHocKy, TenHocKy = "HK4" }
        });

        // Act - Only submit slot 2, slot 1 should be removed
        var result = await _service.SaveDraftAsync(teacherId, maHocKy, new UpdateTeachingPreferenceDto
        {
            Slots = new List<TeachingPreferenceSlotDto>
            {
                new TeachingPreferenceSlotDto { ThuTrongTuan = 2, MaCaHoc = 2, MucDo = "unavailable" } // Changed from available to unavailable
            }
        });

        // Assert
        Assert.That(result.Slots.Count, Is.EqualTo(1));
        Assert.That(result.Slots[0].MaCaHoc, Is.EqualTo(2));
        Assert.That(result.Slots[0].MucDo, Is.EqualTo("unavailable"));
    }
}
