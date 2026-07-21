using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Grading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
public class GradeAggregationServiceTests
{
    private ApplicationDbContext _db;
    private GradeAggregationService _service;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _db = new ApplicationDbContext(options);
        
        _service = new GradeAggregationService(_db, new NullLogger<GradeAggregationService>());
    }

    [TearDown]
    public void TearDown()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    [Test]
    public async Task CalculateAssignmentGradeAsync_MissingSubmissions_TreatedAsZero()
    {
        int studentId = 1;
        int subjectId = 100;

        var config = new CauHinhDauDiemQuaTrinh
        {
            MaCauHinhDauDiem = 10,
            MaMonHoc = subjectId,
            TrongSoNoiBo = 100,
            SoLuongCot = 10 // Expecting 10 grades
        };
        _db.CauHinhDauDiemQuaTrinhs.Add(config);

        // 10 assignments
        for (int i = 1; i <= 10; i++)
        {
            _db.BaiTaps.Add(new BaiTap { MaBaiTap = i, MaMonHoc = subjectId, MaCauHinhDauDiem = 10 });
        }

        // Only 9 submissions
        for (int i = 1; i <= 9; i++)
        {
            _db.BaiNops.Add(new BaiNop { MaBaiNop = i, MaBaiTap = i, MaHocSinh = studentId, DiemSo = 10 });
        }

        await _db.SaveChangesAsync();

        var result = await _service.CalculateAssignmentGradeAsync(studentId, subjectId, config);

        // 90 / 10 = 9.0
        Assert.That(result, Is.EqualTo(9.0m));
    }

    [Test]
    public async Task CalculateQuizGradeAsync_MissingQuizzes_TreatedAsZero()
    {
        int studentId = 1;
        int subjectId = 100;
        int termId = 5;

        var config = new CauHinhDauDiemQuaTrinh
        {
            MaCauHinhDauDiem = 20,
            MaMonHoc = subjectId,
            TrongSoNoiBo = 100,
            SoLuongCot = 10 // Expecting 10 quizzes
        };
        _db.CauHinhDauDiemQuaTrinhs.Add(config);

        // 10 quizzes
        for (int i = 1; i <= 10; i++)
        {
            _db.DeKiemTras.Add(new DeKiemTra { 
                MaDeKiemTra = i, 
                MaMonHoc = subjectId, 
                MaHocKy = termId, 
                LoaiDeThi = "quiz_bai_hoc",
                CauHinhDeThi = "{\"cachTinhDiemCuoi\":\"cao_nhat\"}"
            });
        }

        // Only 8 attempts
        for (int i = 1; i <= 8; i++)
        {
            _db.PhienThiHocSinhs.Add(new PhienThiHocSinh { 
                MaPhienThi = i, 
                MaDeKiemTra = i, 
                MaHocSinh = studentId, 
                DiemCuoiCung = 10,
                TrangThaiLuong = "da_dung"
            });
        }

        await _db.SaveChangesAsync();

        var result = await _service.CalculateQuizGradeAsync(studentId, subjectId, termId, "quiz", config);

        // 80 / 10 = 8.0
        Assert.That(result, Is.EqualTo(8.0m));
    }

    [Test]
    public async Task CalculateGradeAsync_ThresholdDependsOnSubject()
    {
        int studentId = 1;
        int subjectA = 101;
        int subjectB = 102;
        int termId = 5;

        _db.NguoiDungs.Add(new NguoiDung { MaNguoiDung = studentId });

        // Subject A: Threshold 5.0
        _db.CauHinhDiemMonHocs.Add(new CauHinhDiemMonHoc { MaMonHoc = subjectA, MaHocKy = termId, TrongSoQuaTrinh = 100, NguongDat = 5.0m });
        var configA = new CauHinhDauDiemQuaTrinh { MaCauHinhDauDiem = 1, MaMonHoc = subjectA, MaHocKy = termId, TrongSoNoiBo = 100, LoaiDauDiem = new LoaiDauDiemQuaTrinh { MaCode = "chuyen_can" } };
        _db.CauHinhDauDiemQuaTrinhs.Add(configA);

        // Subject B: Threshold 6.5
        _db.CauHinhDiemMonHocs.Add(new CauHinhDiemMonHoc { MaMonHoc = subjectB, MaHocKy = termId, TrongSoQuaTrinh = 100, NguongDat = 6.5m });
        var configB = new CauHinhDauDiemQuaTrinh { MaCauHinhDauDiem = 2, MaMonHoc = subjectB, MaHocKy = termId, TrongSoNoiBo = 100, LoaiDauDiem = new LoaiDauDiemQuaTrinh { MaCode = "chuyen_can" } };
        _db.CauHinhDauDiemQuaTrinhs.Add(configB);

        // Pre-seed DiemSo to 6.0
        _db.DiemSos.Add(new DiemSo { MaHocSinh = studentId, MaMonHoc = subjectA, MaHocKy = termId, DiemQuaTrinh = 6.0m, DiemGiuaKy = 0, DiemCuoiKy = 0 });
        _db.DiemSos.Add(new DiemSo { MaHocSinh = studentId, MaMonHoc = subjectB, MaHocKy = termId, DiemQuaTrinh = 6.0m, DiemGiuaKy = 0, DiemCuoiKy = 0 });

        // We also need to mock CalculateAttendanceGradeAsync to return 6.0, but it queries BuoiHoc.
        // Let's seed 1 session, 1 presence, then grade = 10.0m * 1 / 1 = 10.0m. But we want 6.0m.
        // Or we just seed 10 sessions, 6 presents.
        _db.KhoaHocs.Add(new KhoaHoc { MaKhoaHoc = 1, MaMonHoc = subjectA, MaHocKy = termId });
        _db.KhoaHocs.Add(new KhoaHoc { MaKhoaHoc = 2, MaMonHoc = subjectB, MaHocKy = termId });
        
        for (int i = 1; i <= 10; i++)
        {
            _db.BuoiHocs.Add(new BuoiHoc { MaBuoiHoc = i, MaKhoaHoc = 1, TrangThaiBuoi = "da_dien_ra" });
            _db.BuoiHocs.Add(new BuoiHoc { MaBuoiHoc = 10+i, MaKhoaHoc = 2, TrangThaiBuoi = "da_dien_ra" });
        }
        for (int i = 1; i <= 6; i++)
        {
            _db.DiemDanhs.Add(new DiemDanh { MaDiemDanh = i, MaHocSinh = studentId, TrangThai = "co_mat", BuoiHoc = _db.BuoiHocs.Local.First(b => b.MaBuoiHoc == i) });
            _db.DiemDanhs.Add(new DiemDanh { MaDiemDanh = 10+i, MaHocSinh = studentId, TrangThai = "co_mat", BuoiHoc = _db.BuoiHocs.Local.First(b => b.MaBuoiHoc == 10+i) });
        }
        await _db.SaveChangesAsync();

        await _service.CalculateGradeAsync(studentId, subjectA, termId);
        await _service.CalculateGradeAsync(studentId, subjectB, termId);

        var gradeA = await _db.DiemSos.FirstAsync(d => d.MaMonHoc == subjectA);
        var gradeB = await _db.DiemSos.FirstAsync(d => d.MaMonHoc == subjectB);

        Assert.Multiple(() =>
        {
            Assert.That(gradeA.GpaMonHoc, Is.EqualTo(6.0m));
            Assert.That(gradeA.TrangThai, Is.EqualTo("Đạt"));
            Assert.That(gradeB.GpaMonHoc, Is.EqualTo(6.0m));
            Assert.That(gradeB.TrangThai, Is.EqualTo("Rớt"));
        });
    }

    [Test]
    public async Task CalculateGradeAsync_TotalWeightNot100_ThrowsApiException()
    {
        int studentId = 1;
        int subjectId = 100;
        int termId = 5;

        _db.CauHinhDauDiemQuaTrinhs.Add(new CauHinhDauDiemQuaTrinh { 
            MaCauHinhDauDiem = 1, 
            MaMonHoc = subjectId, 
            MaHocKy = termId, 
            TrongSoNoiBo = 90,
            LoaiDauDiem = new LoaiDauDiemQuaTrinh { MaCode = "test_loai" }
        });
        await _db.SaveChangesAsync();

        var ex = Assert.ThrowsAsync<ApiException>(() => _service.CalculateGradeAsync(studentId, subjectId, termId));
        Assert.Multiple(() =>
        {
            Assert.That(ex.StatusCode, Is.EqualTo(400));
            Assert.That(ex.Message, Does.Contain("Tổng trọng số đầu điểm quá trình phải là 100%"));
        });
    }

    [Test]
    public void CalculateGradeAsync_NoConfiguration_ThrowsApiException()
    {
        int studentId = 1;
        int subjectId = 100;
        int termId = 5;

        var ex = Assert.ThrowsAsync<ApiException>(() => _service.CalculateGradeAsync(studentId, subjectId, termId));
        Assert.Multiple(() =>
        {
            Assert.That(ex.StatusCode, Is.EqualTo(400));
            Assert.That(ex.Message, Is.EqualTo("Môn học chưa cấu hình đầu điểm quá trình."));
        });
    }
}
