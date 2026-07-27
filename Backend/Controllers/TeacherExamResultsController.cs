using System.Text.Json;
using Backend.Data;
using Backend.DTOs.Common;
using Backend.Services.QuizGrading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/teacher/exam-results")]
[Authorize(Roles = "Teacher,CampusAdmin,AcademicStaff,Admin,SuperAdmin")]
public class TeacherExamResultsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IQuizGradingService _gradingService;

    public TeacherExamResultsController(ApplicationDbContext context, IQuizGradingService gradingService)
    {
        _context = context;
        _gradingService = gradingService;
    }

    // ── 1. Lấy danh sách tất cả các CaThi mà giáo viên được phân công ─────────────
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<object>>> GetExamSessions()
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as Backend.DTOs.Auth.CurrentUserContext;
            var userId = currentUser?.UserId ?? 0;

            // Lấy tất cả ca thi giảng viên được phân công làm giám thị
            var caThiIds = await _context.PhanCongGiamThis
                .Where(pcg => pcg.MaGiamThi == userId)
                .Select(pcg => pcg.MaCaThi)
                .Distinct()
                .ToListAsync();

            List<int> targetCaIds = caThiIds;
            if (!targetCaIds.Any())
            {
                targetCaIds = await _context.CaThis
                    .OrderByDescending(c => c.MaCaThi)
                    .Take(20)
                    .Select(c => c.MaCaThi)
                    .ToListAsync();
            }

            var caThis = await _context.CaThis
                .Include(c => c.Phong)
                .Include(c => c.LichThiTong)
                    .ThenInclude(l => l!.MonHoc)
                .Include(c => c.ThiSinhCaThis)
                .Where(c => targetCaIds.Contains(c.MaCaThi))
                .OrderByDescending(c => c.NgayThi)
                .ThenByDescending(c => c.MaCaThi)
                .ToListAsync();

            var phienThis = await _context.PhienThiHocSinhs
                .Where(p => targetCaIds.Contains(p.MaCaThi ?? 0))
                .ToListAsync();

            var result = caThis.Select(c =>
            {
                var phienInCa = phienThis.Where(p => p.MaCaThi == c.MaCaThi).ToList();
                var totalStudents = c.ThiSinhCaThis.Count;
                var submittedCount = phienInCa.Count(p => p.NopLuc.HasValue || p.DiemCuoiCung.HasValue || p.DiemTuDong.HasValue);

                var scores = phienInCa
                    .Select(p => p.DiemCuoiCung ?? p.DiemTuDong ?? 0m)
                    .ToList();

                decimal avgScore = scores.Any() ? Math.Round(scores.Average(), 1) : 0m;
                decimal highestScore = scores.Any() ? scores.Max() : 0m;
                int passedCount = scores.Count(s => s >= 5.0m);
                int passRate = scores.Any() ? (int)Math.Round((double)passedCount / scores.Count * 100) : 0;

                return new
                {
                    ExamId = c.MaCaThi,
                    ExamTitle = c.TenCaThi,
                    Subject = c.LichThiTong?.MonHoc?.TenMonHoc ?? "Chưa rõ môn",
                    SubjectCode = c.LichThiTong?.MonHoc?.MaCodeMonHoc ?? "",
                    Room = c.Phong?.TenPhong ?? "Phòng trực tuyến",
                    Date = c.NgayThi.ToString("dd/MM/yyyy"),
                    StartTime = c.ThoiGianBatDau.ToString("HH:mm"),
                    EndTime = c.ThoiGianKetThuc.ToString("HH:mm"),
                    Status = c.TrangThai,
                    TotalStudents = totalStudents,
                    SubmittedCount = submittedCount,
                    AvgScore = avgScore,
                    PassRate = passRate,
                    HighestScore = highestScore
                };
            }).ToList();

            return Ok(ApiResponseDto<object>.Ok(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi tải danh sách ca thi: " + ex.Message));
        }
    }

    // ── 2. Lấy danh sách sinh viên & điểm số trong 1 CaThi ────────────────────
    [HttpGet("ca-thi/{maCaThi:int}")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetCaThiStudentResults(int maCaThi)
    {
        try
        {
            var caThi = await _context.CaThis
                .Include(c => c.Phong)
                .Include(c => c.LichThiTong)
                    .ThenInclude(l => l!.MonHoc)
                .Include(c => c.ThiSinhCaThis)
                    .ThenInclude(t => t.HocSinh)
                .FirstOrDefaultAsync(c => c.MaCaThi == maCaThi);

            if (caThi == null)
            {
                return NotFound(ApiResponseDto.Fail("Không tìm thấy ca thi."));
            }

            int? maDeKiemTra = caThi.LichThiTong?.MaDeKiemTra;

            int tongSoCau = 0;
            if (maDeKiemTra.HasValue)
            {
                tongSoCau = await _context.CauHoiDeKiemTras
                    .CountAsync(q => q.MaDeKiemTra == maDeKiemTra.Value);
            }

            var phienThis = await _context.PhienThiHocSinhs
                .Where(p => p.MaCaThi == maCaThi)
                .ToListAsync();

            var studentList = new List<object>();

            foreach (var ts in caThi.ThiSinhCaThis)
            {
                var hs = ts.HocSinh;
                var pt = phienThis.FirstOrDefault(p => p.MaHocSinh == ts.MaHocSinh);

                decimal diem = pt?.DiemCuoiCung ?? pt?.DiemTuDong ?? 0m;
                int soCauDung = pt?.SoCauDung ?? 0;
                string thoiGianLam = "--";

                if (pt?.BatDauLuc.HasValue == true && pt?.NopLuc.HasValue == true)
                {
                    var span = pt.NopLuc.Value - pt.BatDauLuc.Value;
                    if (span.TotalMinutes >= 1)
                        thoiGianLam = $"{(int)span.TotalMinutes} phút {span.Seconds} giây";
                    else
                        thoiGianLam = $"{span.Seconds} giây";
                }

                studentList.Add(new
                {
                    MaHocSinh = ts.MaHocSinh,
                    HoTen = hs?.HoTen ?? $"Học sinh #{ts.MaHocSinh}",
                    MaSinhVien = (hs?.Email ?? $"SV{hs?.MaNguoiDung}").Split('@')[0],
                    TrangThaiDuThi = ts.TrangThaiDuThi,
                    MaPhienThi = pt?.MaPhienThi,
                    Diem = diem,
                    SoCauDung = soCauDung,
                    TongSoCau = tongSoCau,
                    BatDauLuc = pt?.BatDauLuc?.ToString("HH:mm:ss dd/MM/yyyy"),
                    NopLuc = pt?.NopLuc?.ToString("HH:mm:ss dd/MM/yyyy"),
                    ThoiGianLam = thoiGianLam,
                    NgayThi = caThi.NgayThi.ToString("dd/MM/yyyy")
                });
            }

            var scores = studentList.Select(s => (decimal)((dynamic)s).Diem).ToList();
            decimal avgScore = scores.Any() ? Math.Round(scores.Average(), 1) : 0m;
            decimal highestScore = scores.Any() ? scores.Max() : 0m;
            int passedCount = scores.Count(s => s >= 5.0m);
            int passRate = scores.Any() ? (int)Math.Round((double)passedCount / scores.Count * 100) : 0;

            var result = new
            {
                ExamId = caThi.MaCaThi,
                ExamTitle = caThi.TenCaThi,
                Subject = caThi.LichThiTong?.MonHoc?.TenMonHoc ?? "Chưa rõ môn",
                SubjectCode = caThi.LichThiTong?.MonHoc?.MaCodeMonHoc ?? "",
                Room = caThi.Phong?.TenPhong ?? "Phòng trực tuyến",
                Date = caThi.NgayThi.ToString("dd/MM/yyyy"),
                TotalStudents = studentList.Count,
                SubmittedCount = phienThis.Count(p => p.NopLuc.HasValue),
                AvgScore = avgScore,
                PassRate = passRate,
                HighestScore = highestScore,
                Students = studentList
            };

            return Ok(ApiResponseDto<object>.Ok(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi tải kết quả chi tiết ca thi: " + ex.Message));
        }
    }

    // ── 3. Lấy chi tiết câu hỏi & đáp án của 1 sinh viên trong CaThi ──────────
    [HttpGet("ca-thi/{maCaThi:int}/student/{maHocSinh:int}")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetStudentExamDetail(int maCaThi, int maHocSinh)
    {
        try
        {
            var caThi = await _context.CaThis
                .Include(c => c.LichThiTong)
                .FirstOrDefaultAsync(c => c.MaCaThi == maCaThi);

            if (caThi == null)
            {
                return NotFound(ApiResponseDto.Fail("Không tìm thấy ca thi."));
            }

            var hocSinh = await _context.NguoiDungs.FindAsync(maHocSinh);
            if (hocSinh == null)
            {
                return NotFound(ApiResponseDto.Fail("Không tìm thấy sinh viên."));
            }

            var phienThi = await _context.PhienThiHocSinhs
                .FirstOrDefaultAsync(p => p.MaCaThi == maCaThi && p.MaHocSinh == maHocSinh);

            int maDeKiemTra = phienThi?.MaDeKiemTra ?? caThi.LichThiTong?.MaDeKiemTra ?? 0;

            var questions = await _context.CauHoiDeKiemTras
                .Include(q => q.CauHoi)
                .Where(q => q.MaDeKiemTra == maDeKiemTra)
                .OrderBy(q => q.ThuTu ?? int.MaxValue)
                .ThenBy(q => q.MaCauHoi)
                .ToListAsync();

            // Parse câu trả lời của sinh viên bằng QuizGradingService
            var studentAnswerList = _gradingService.ParseAnswersJson(phienThi?.CauTraLoiJson);
            var studentAnswerMap = studentAnswerList
                .GroupBy(a => a.MaCauHoi)
                .ToDictionary(g => g.Key, g => g.Last().SelectedOptionIds);

            var questionDetails = new List<object>();
            int correctCount = 0;

            foreach (var rel in questions)
            {
                var q = rel.CauHoi;
                if (q == null) continue;

                var options = ParseOptions(q.LuaChon);
                var correctAnswers = ParseStringList(q.DapAnDung);

                studentAnswerMap.TryGetValue(rel.MaCauHoi, out var studentSelected);
                studentSelected ??= new List<string>();

                var normStudent = studentSelected.OrderBy(x => x).ToList();
                var normCorrect = correctAnswers.OrderBy(x => x).ToList();

                bool isUnanswered = !studentSelected.Any();
                bool isCorrect = !isUnanswered && normStudent.SequenceEqual(normCorrect);

                if (isCorrect) correctCount++;

                questionDetails.Add(new
                {
                    MaCauHoi = rel.MaCauHoi,
                    ThuTu = rel.ThuTu ?? (questionDetails.Count + 1),
                    NoiDung = q.NoiDung,
                    LoaiCauHoi = q.LoaiCauHoi,
                    DiemToiDa = rel.DiemSo,
                    GiaiThich = q.GiaiThichDapAn,
                    Options = options,
                    DapAnDung = correctAnswers,
                    DapAnHocSinh = studentSelected,
                    IsCorrect = isCorrect,
                    IsUnanswered = isUnanswered
                });
            }

            decimal finalScore = phienThi?.DiemCuoiCung ?? phienThi?.DiemTuDong ?? 0m;

            string thoiGianLam = "--";
            if (phienThi?.BatDauLuc.HasValue == true && phienThi?.NopLuc.HasValue == true)
            {
                var span = phienThi.NopLuc.Value - phienThi.BatDauLuc.Value;
                if (span.TotalMinutes >= 1)
                    thoiGianLam = $"{(int)span.TotalMinutes} phút {span.Seconds} giây";
                else
                    thoiGianLam = $"{span.Seconds} giây";
            }

            var result = new
            {
                MaCaThi = maCaThi,
                MaHocSinh = maHocSinh,
                HoTen = hocSinh.HoTen,
                MaSinhVien = (hocSinh.Email ?? $"SV{hocSinh.MaNguoiDung}").Split('@')[0],
                Score = finalScore,
                SoCauDung = correctCount,
                TongSoCau = questions.Count,
                BatDauLuc = phienThi?.BatDauLuc?.ToString("HH:mm:ss dd/MM/yyyy"),
                NopLuc = phienThi?.NopLuc?.ToString("HH:mm:ss dd/MM/yyyy"),
                ThoiGianLam = thoiGianLam,
                Questions = questionDetails
            };

            return Ok(ApiResponseDto<object>.Ok(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi tải chi tiết bài làm sinh viên: " + ex.Message));
        }
    }

    // ── Helper parsing functions ──────────────────────────────────────────────
    private static List<object> ParseOptions(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<object>();
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                var list = new List<object>();
                int idx = 0;
                string[] defaultKeys = new[] { "A", "B", "C", "D", "E", "F", "G", "H" };

                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    if (el.ValueKind == JsonValueKind.String)
                    {
                        var textVal = el.GetString() ?? "";
                        string keyVal = idx < defaultKeys.Length ? defaultKeys[idx] : (idx + 1).ToString();
                        list.Add(new { key = keyVal, text = textVal });
                        idx++;
                        continue;
                    }

                    if (el.ValueKind == JsonValueKind.Object)
                    {
                        string? key = null;
                        string[] keyNames = new[] { "key", "Key", "id", "Id", "label", "Label", "code", "Code", "value", "Value" };
                        foreach (var kn in keyNames)
                        {
                            if (el.TryGetProperty(kn, out var prop) && prop.ValueKind == JsonValueKind.String)
                            {
                                key = prop.GetString();
                                if (!string.IsNullOrWhiteSpace(key)) break;
                            }
                        }

                        if (string.IsNullOrWhiteSpace(key))
                        {
                            key = idx < defaultKeys.Length ? defaultKeys[idx] : (idx + 1).ToString();
                        }

                        string? text = null;
                        string[] textNames = new[] { "text", "Text", "content", "Content", "noiDung", "NoiDung", "val", "Val", "value", "Value" };
                        foreach (var tn in textNames)
                        {
                            if (el.TryGetProperty(tn, out var prop) && prop.ValueKind == JsonValueKind.String)
                            {
                                text = prop.GetString();
                                if (!string.IsNullOrWhiteSpace(text)) break;
                            }
                        }

                        list.Add(new { key = key.Trim(), text = text ?? "" });
                        idx++;
                    }
                }
                return list;
            }
        }
        catch { }
        return new List<object>();
    }

    private static List<string> ParseStringList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<string>();
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                return doc.RootElement.EnumerateArray()
                    .Select(x => x.GetString())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => x!)
                    .ToList();
            }
            if (doc.RootElement.ValueKind == JsonValueKind.String)
            {
                var str = doc.RootElement.GetString();
                if (!string.IsNullOrEmpty(str)) return new List<string> { str };
            }
        }
        catch
        {
            if (!json.StartsWith("[") && !json.StartsWith("{"))
            {
                return new List<string> { json.Trim('"') };
            }
        }
        return new List<string>();
    }
}
