using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.AcademicSchedulingContext;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.AcademicSchedulingContext;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests;

public class P25_AcademicSchedulingContextTests
{
    private ApplicationDbContext _db;
    private AcademicSchedulingContextService _service;

    [SetUp] public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new ApplicationDbContext(options);
        _service = new AcademicSchedulingContextService(_db);
    }

    [TearDown] public void TearDown()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    private DateOnly GetToday()
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(Environment.OSVersion.Platform == PlatformID.Win32NT ? "SE Asia Standard Time" : "Asia/Ho_Chi_Minh");
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        return DateOnly.FromDateTime(now);
    }

    [Test]
    public async Task P25_A01_ReturnsCurrentAndNearestFutureTerm()
    {
        var today = GetToday();
        var campusId = 1;

        _db.HocKys.Add(new HocKy { MaHocKy = 1, MaDonVi = campusId, NgayBatDau = today.AddDays(-30), NgayKetThuc = today.AddDays(30) });
        _db.HocKys.Add(new HocKy { MaHocKy = 2, MaDonVi = campusId, NgayBatDau = today.AddDays(40), NgayKetThuc = today.AddDays(100) });
        
        // Add course so Readiness works
        _db.KhoaHocs.Add(new KhoaHoc { MaKhoaHoc = 1, MaHocKy = 2, MaDonVi = campusId, TrangThai = "nhap" });
        await _db.SaveChangesAsync();

        var result = await _service.GetContextAsync(campusId);

        Assert.That(result.CurrentTerm, Is.Not.Null);
        Assert.That(result.CurrentTerm!.MaHocKy, Is.EqualTo(1));
        
        Assert.That(result.NextTerm, Is.Not.Null);
        Assert.That(result.NextTerm!.MaHocKy, Is.EqualTo(2));
        
        Assert.That(result.SchedulableTerm, Is.Not.Null);
        Assert.That(result.SchedulableTerm!.MaHocKy, Is.EqualTo(2));
        
        Assert.That(result.ReasonCode, Is.EqualTo(SchedulingContextReasonCodes.NextTermAvailable));
    }

    [Test]
    public async Task P25_A02_DoesNotSelectFarFutureTerm()
    {
        var today = GetToday();
        var campusId = 1;

        // Current term
        _db.HocKys.Add(new HocKy { MaHocKy = 1, MaDonVi = campusId, NgayBatDau = today.AddDays(-30), NgayKetThuc = today.AddDays(30) });
        // Nearest future term
        _db.HocKys.Add(new HocKy { MaHocKy = 2, MaDonVi = campusId, NgayBatDau = today.AddDays(40), NgayKetThuc = today.AddDays(100) });
        // Far future term
        _db.HocKys.Add(new HocKy { MaHocKy = 3, MaDonVi = campusId, NgayBatDau = today.AddDays(110), NgayKetThuc = today.AddDays(170) });
        
        await _db.SaveChangesAsync();

        var result = await _service.GetContextAsync(campusId);

        Assert.That(result.NextTerm, Is.Not.Null);
        Assert.That(result.NextTerm!.MaHocKy, Is.EqualTo(2));
        
        Assert.That(result.SchedulableTerm, Is.Not.Null);
        Assert.That(result.SchedulableTerm!.MaHocKy, Is.EqualTo(2));
    }

    [Test]
    public async Task P25_A03_RejectsSkippingNearestTerm()
    {
        var today = GetToday();
        var campusId = 1;

        _db.HocKys.Add(new HocKy { MaHocKy = 1, MaDonVi = campusId, NgayBatDau = today.AddDays(-30), NgayKetThuc = today.AddDays(30) });
        _db.HocKys.Add(new HocKy { MaHocKy = 2, MaDonVi = campusId, NgayBatDau = today.AddDays(40), NgayKetThuc = today.AddDays(100) });
        _db.HocKys.Add(new HocKy { MaHocKy = 3, MaDonVi = campusId, NgayBatDau = today.AddDays(110), NgayKetThuc = today.AddDays(170) });
        
        await _db.SaveChangesAsync();

        var exception = Assert.ThrowsAsync<ApiException>(() => _service.ValidateSchedulableTermAsync(campusId, 3));
        Assert.That(exception.StatusCode, Is.EqualTo(400));
        Assert.That(exception.Message, Does.Contain("Chỉ được chuẩn bị lịch cho học kỳ tương lai gần nhất"));
    }

    [Test]
    public async Task P25_A06_NoFutureTerm()
    {
        var today = GetToday();
        var campusId = 1;

        _db.HocKys.Add(new HocKy { MaHocKy = 1, MaDonVi = campusId, NgayBatDau = today.AddDays(-30), NgayKetThuc = today.AddDays(30) });
        await _db.SaveChangesAsync();

        var result = await _service.GetContextAsync(campusId);

        Assert.That(result.NextTerm, Is.Null);
        Assert.That(result.SchedulableTerm, Is.Null);
        Assert.That(result.CanPrepareSchedule, Is.False);
        Assert.That(result.ReasonCode, Is.EqualTo(SchedulingContextReasonCodes.NoFutureTerm));
        
        var exception = Assert.ThrowsAsync<ApiException>(() => _service.ValidateSchedulableTermAsync(campusId, 2));
        Assert.That(exception.StatusCode, Is.EqualTo(400));
        Assert.That(exception.Message, Does.Contain("Không thể chuẩn bị lịch"));
    }

    [Test]
    public async Task P25_A07_CrossCampusIsolation()
    {
        var today = GetToday();
        var campusId = 1;
        var otherCampusId = 2;

        _db.HocKys.Add(new HocKy { MaHocKy = 1, MaDonVi = campusId, NgayBatDau = today.AddDays(-30), NgayKetThuc = today.AddDays(30) });
        _db.HocKys.Add(new HocKy { MaHocKy = 2, MaDonVi = otherCampusId, NgayBatDau = today.AddDays(40), NgayKetThuc = today.AddDays(100) });
        
        await _db.SaveChangesAsync();

        var result = await _service.GetContextAsync(campusId);

        // Term 2 is future, but for another campus.
        Assert.That(result.NextTerm, Is.Null);
        
        var exception = Assert.ThrowsAsync<ApiException>(() => _service.ValidateSchedulableTermAsync(campusId, 2));
        Assert.That(exception.StatusCode, Is.EqualTo(400));
        Assert.That(exception.Message, Does.Contain("Không thể chuẩn bị lịch do không có học kỳ hợp lệ"));
    }
}
