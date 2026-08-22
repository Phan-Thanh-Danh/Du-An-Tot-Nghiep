using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.QuestionBank;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Audit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace Backend.Services.QuestionBank;

public class QuestionBankService : IQuestionBankService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true };

    public QuestionBankService(ApplicationDbContext context, IAuditLogService auditLogService, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _auditLogService = auditLogService;
        _httpContextAccessor = httpContextAccessor;
        ExcelPackage.License.SetNonCommercialPersonal("LMS Admin");
    }

    private CurrentUserContext GetCurrentUser()
    {
        var currentUser = _httpContextAccessor.HttpContext?.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser == null) throw new ApiException(StatusCodes.Status401Unauthorized, "Unauthorized");
        return currentUser;
    }

    public async Task<PagedResultDto<QuestionDto>> GetQuestionsAsync(QuestionFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = _context.CauHois.Include(x => x.MonHoc).AsNoTracking()
            .Where(x => x.NoiDung != "undefined" && x.NoiDung != "" && x.NoiDung != null);

        if (filter.MaMonHoc.HasValue)
            query = query.Where(x => x.MaMonHoc == filter.MaMonHoc.Value);

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var kw = filter.Keyword.Trim();
            query = query.Where(x => x.NoiDung.Contains(kw));
        }

        if (!string.IsNullOrWhiteSpace(filter.DoKho))
            query = query.Where(x => x.DoKho == filter.DoKho);

        if (!string.IsNullOrWhiteSpace(filter.LoaiCauHoi))
            query = query.Where(x => x.LoaiCauHoi == filter.LoaiCauHoi);

        if (!string.IsNullOrWhiteSpace(filter.KieuLuaChon))
            query = query.Where(x => x.KieuLuaChon == filter.KieuLuaChon);

        if (filter.ConHoatDong.HasValue)
            query = query.Where(x => x.ConHoatDong == filter.ConHoatDong.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.MaCauHoi)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        var questionIds = items.Select(x => x.MaCauHoi).ToList();
        var usageCounts = await _context.CauHoiDeKiemTras
            .Where(x => questionIds.Contains(x.MaCauHoi))
            .GroupBy(x => x.MaCauHoi)
            .Select(g => new { MaCauHoi = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.MaCauHoi, x => x.Count, cancellationToken);

        var dtos = items.Select(x => MapToDto(x, usageCounts.GetValueOrDefault(x.MaCauHoi, 0))).ToList();

        return new PagedResultDto<QuestionDto>
        {
            Items = dtos,
            PageIndex = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalItems = total
        };
    }

    public async Task<QuestionDto> GetQuestionByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cauHoi = await _context.CauHois.Include(x => x.MonHoc).AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaCauHoi == id, cancellationToken);
        if (cauHoi == null) throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy câu hỏi");

        var usageCount = await _context.CauHoiDeKiemTras.CountAsync(x => x.MaCauHoi == id, cancellationToken);
        return MapToDto(cauHoi, usageCount);
    }

    public async Task<QuestionDto> CreateQuestionAsync(CreateQuestionDto input, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        await ValidateQuestionAsync(null, input.MaMonHoc, input.LoaiCauHoi, input.NoiDung, input.KieuLuaChon, input.LuaChon, input.DapAnDung, cancellationToken);

        var cauHoi = new CauHoi
        {
            MaMonHoc = input.MaMonHoc,
            NguoiTao = currentUser.UserId,
            LoaiCauHoi = input.LoaiCauHoi,
            NoiDung = input.NoiDung.Trim(),
            KieuLuaChon = input.KieuLuaChon,
            LuaChon = input.LuaChon != null ? JsonSerializer.Serialize(input.LuaChon, JsonOptions) : null,
            DapAnDung = input.DapAnDung != null ? JsonSerializer.Serialize(input.DapAnDung, JsonOptions) : null,
            GiaiThichDapAn = input.GiaiThichDapAn,
            DoKho = input.DoKho,
            ConHoatDong = true,
            NgayTao = DateTime.UtcNow
        };

        _context.CauHois.Add(cauHoi);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.AddAsync(currentUser.CampusId, "CauHoi", cauHoi.MaCauHoi, "CREATE_QUESTION", currentUser.UserId, null, cauHoi, cancellationToken);

        return await GetQuestionByIdAsync(cauHoi.MaCauHoi, cancellationToken);
    }

    public async Task<QuestionDto> UpdateQuestionAsync(int id, UpdateQuestionDto input, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var cauHoi = await _context.CauHois.FirstOrDefaultAsync(x => x.MaCauHoi == id, cancellationToken);
        if (cauHoi == null) throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy câu hỏi");

        await ValidateQuestionAsync(id, input.MaMonHoc, input.LoaiCauHoi, input.NoiDung, input.KieuLuaChon, input.LuaChon, input.DapAnDung, cancellationToken);

        var oldValue = JsonSerializer.Serialize(cauHoi, JsonOptions);

        cauHoi.MaMonHoc = input.MaMonHoc;
        cauHoi.LoaiCauHoi = input.LoaiCauHoi;
        cauHoi.NoiDung = input.NoiDung.Trim();
        cauHoi.KieuLuaChon = input.KieuLuaChon;
        cauHoi.LuaChon = input.LuaChon != null ? JsonSerializer.Serialize(input.LuaChon, JsonOptions) : null;
        cauHoi.DapAnDung = input.DapAnDung != null ? JsonSerializer.Serialize(input.DapAnDung, JsonOptions) : null;
        cauHoi.GiaiThichDapAn = input.GiaiThichDapAn;
        cauHoi.DoKho = input.DoKho;
        cauHoi.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.AddAsync(currentUser.CampusId, "CauHoi", cauHoi.MaCauHoi, "UPDATE_QUESTION", currentUser.UserId, oldValue, cauHoi, cancellationToken);

        return await GetQuestionByIdAsync(cauHoi.MaCauHoi, cancellationToken);
    }

    public async Task DeleteQuestionAsync(int id, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var cauHoi = await _context.CauHois.FirstOrDefaultAsync(x => x.MaCauHoi == id, cancellationToken);
        if (cauHoi == null) throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy câu hỏi");

        var isUsed = await _context.CauHoiDeKiemTras.AnyAsync(x => x.MaCauHoi == id, cancellationToken);
        if (isUsed) throw new ApiException(StatusCodes.Status400BadRequest, "Câu hỏi đang được sử dụng trong đề kiểm tra, không thể xoá");

        var oldValue = JsonSerializer.Serialize(cauHoi, JsonOptions);
        _context.CauHois.Remove(cauHoi);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.AddAsync(currentUser.CampusId, "CauHoi", id, "DELETE_QUESTION", currentUser.UserId, oldValue, null, cancellationToken);
    }

    public async Task ActivateQuestionAsync(int id, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var cauHoi = await _context.CauHois.FirstOrDefaultAsync(x => x.MaCauHoi == id, cancellationToken);
        if (cauHoi == null) throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy câu hỏi");

        if (!cauHoi.ConHoatDong)
        {
            var oldValue = JsonSerializer.Serialize(cauHoi, JsonOptions);
            cauHoi.ConHoatDong = true;
            cauHoi.NgayCapNhat = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            await _auditLogService.AddAsync(currentUser.CampusId, "CauHoi", id, "ACTIVATE_QUESTION", currentUser.UserId, oldValue, cauHoi, cancellationToken);
        }
    }

    public async Task DeactivateQuestionAsync(int id, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var cauHoi = await _context.CauHois.FirstOrDefaultAsync(x => x.MaCauHoi == id, cancellationToken);
        if (cauHoi == null) throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy câu hỏi");

        if (cauHoi.ConHoatDong)
        {
            var oldValue = JsonSerializer.Serialize(cauHoi, JsonOptions);
            cauHoi.ConHoatDong = false;
            cauHoi.NgayCapNhat = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            await _auditLogService.AddAsync(currentUser.CampusId, "CauHoi", id, "DEACTIVATE_QUESTION", currentUser.UserId, oldValue, cauHoi, cancellationToken);
        }
    }

    public async Task<byte[]> GenerateImportTemplateAsync(CancellationToken cancellationToken = default)
    {
        using var package = new ExcelPackage();

        var wsQuestions = package.Workbook.Worksheets.Add("Questions");
        wsQuestions.Cells[1, 1].Value = "MaCodeMonHoc";
        wsQuestions.Cells[1, 2].Value = "LoaiCauHoi";
        wsQuestions.Cells[1, 3].Value = "DoKho";
        wsQuestions.Cells[1, 4].Value = "KieuLuaChon";
        wsQuestions.Cells[1, 5].Value = "NoiDung";
        wsQuestions.Cells[1, 6].Value = "LuaChonA";
        wsQuestions.Cells[1, 7].Value = "LuaChonB";
        wsQuestions.Cells[1, 8].Value = "LuaChonC";
        wsQuestions.Cells[1, 9].Value = "LuaChonD";
        wsQuestions.Cells[1, 10].Value = "LuaChonE";
        wsQuestions.Cells[1, 11].Value = "DapAnDung";
        wsQuestions.Cells[1, 12].Value = "GiaiThichDapAn";

        // Mẫu 1: Trắc nghiệm (Chọn một)
        wsQuestions.Cells[2, 1].Value = "COM101";
        wsQuestions.Cells[2, 2].Value = "trac_nghiem";
        wsQuestions.Cells[2, 3].Value = "de";
        wsQuestions.Cells[2, 4].Value = "chon_mot";
        wsQuestions.Cells[2, 5].Value = "1+1 bằng mấy?";
        wsQuestions.Cells[2, 6].Value = "1";
        wsQuestions.Cells[2, 7].Value = "2";
        wsQuestions.Cells[2, 8].Value = "3";
        wsQuestions.Cells[2, 9].Value = "4";
        wsQuestions.Cells[2, 10].Value = "";
        wsQuestions.Cells[2, 11].Value = "B";
        wsQuestions.Cells[2, 12].Value = "Phép tính cộng cơ bản";

        // Mẫu 2: Tự luận
        wsQuestions.Cells[3, 1].Value = "COM101";
        wsQuestions.Cells[3, 2].Value = "tu_luan";
        wsQuestions.Cells[3, 3].Value = "trung_binh";
        wsQuestions.Cells[3, 4].Value = "";
        wsQuestions.Cells[3, 5].Value = "Trình bày quy trình phát triển phần mềm theo phương pháp Agile và ưu điểm của mô hình Scrum.";
        wsQuestions.Cells[3, 6].Value = "";
        wsQuestions.Cells[3, 7].Value = "";
        wsQuestions.Cells[3, 8].Value = "";
        wsQuestions.Cells[3, 9].Value = "";
        wsQuestions.Cells[3, 10].Value = "";
        wsQuestions.Cells[3, 11].Value = "";
        wsQuestions.Cells[3, 12].Value = "Hướng dẫn chấm: Trình bày đủ 4 giá trị cốt lõi của Agile (2.5đ), các vai trò trong Scrum (2.5đ), các sự kiện Scrum (2.5đ) và ưu điểm (2.5đ).";

        // Mẫu 3: Trắc nghiệm (Chọn nhiều)
        wsQuestions.Cells[4, 1].Value = "COM101";
        wsQuestions.Cells[4, 2].Value = "trac_nghiem";
        wsQuestions.Cells[4, 3].Value = "trung_binh";
        wsQuestions.Cells[4, 4].Value = "chon_nhieu";
        wsQuestions.Cells[4, 5].Value = "Những ngôn ngữ nào sau đây là ngôn ngữ lập trình hướng đối tượng (OOP)?";
        wsQuestions.Cells[4, 6].Value = "Java";
        wsQuestions.Cells[4, 7].Value = "C++";
        wsQuestions.Cells[4, 8].Value = "HTML";
        wsQuestions.Cells[4, 9].Value = "CSS";
        wsQuestions.Cells[4, 10].Value = "";
        wsQuestions.Cells[4, 11].Value = "A, B";
        wsQuestions.Cells[4, 12].Value = "Java và C++ là ngôn ngữ OOP, HTML và CSS là ngôn ngữ đánh dấu/trang trí UI.";

        var wsGuide = package.Workbook.Worksheets.Add("HuongDan");
        wsGuide.Cells[1, 1].Value = "Cột";
        wsGuide.Cells[1, 2].Value = "Bắt buộc";
        wsGuide.Cells[1, 3].Value = "Giá trị hợp lệ";
        wsGuide.Cells[1, 4].Value = "Mô tả / Ghi chú";

        wsGuide.Cells[2, 1].Value = "MaCodeMonHoc";
        wsGuide.Cells[2, 2].Value = "Có";
        wsGuide.Cells[2, 3].Value = "Mã môn học có sẵn (xem sheet DanhSachMonHoc)";
        wsGuide.Cells[2, 4].Value = "Ví dụ: COM101, WEB201";

        wsGuide.Cells[3, 1].Value = "LoaiCauHoi";
        wsGuide.Cells[3, 2].Value = "Có";
        wsGuide.Cells[3, 3].Value = "trac_nghiem | tu_luan";
        wsGuide.Cells[3, 4].Value = "trac_nghiem: Trắc nghiệm; tu_luan: Tự luận";

        wsGuide.Cells[4, 1].Value = "DoKho";
        wsGuide.Cells[4, 2].Value = "Có";
        wsGuide.Cells[4, 3].Value = "de | trung_binh | kho";
        wsGuide.Cells[4, 4].Value = "de: Dễ; trung_binh: Trung bình; kho: Khó";

        wsGuide.Cells[5, 1].Value = "KieuLuaChon";
        wsGuide.Cells[5, 2].Value = "Trắc nghiệm";
        wsGuide.Cells[5, 3].Value = "chon_mot | chon_nhieu";
        wsGuide.Cells[5, 4].Value = "Dành riêng cho câu hỏi trắc nghiệm. Câu hỏi tự luận ĐỂ TRỐNG.";

        wsGuide.Cells[6, 1].Value = "NoiDung";
        wsGuide.Cells[6, 2].Value = "Có";
        wsGuide.Cells[6, 3].Value = "Văn bản đề bài";
        wsGuide.Cells[6, 4].Value = "Nội dung đề bài câu hỏi";

        wsGuide.Cells[7, 1].Value = "LuaChonA";
        wsGuide.Cells[7, 2].Value = "Trắc nghiệm";
        wsGuide.Cells[7, 3].Value = "Văn bản nội dung lựa chọn A";
        wsGuide.Cells[7, 4].Value = "Nội dung đáp án A. Đề trắc nghiệm cần ít nhất 2 lựa chọn A và B. Tự luận ĐỂ TRỐNG.";

        wsGuide.Cells[8, 1].Value = "LuaChonB";
        wsGuide.Cells[8, 2].Value = "Trắc nghiệm";
        wsGuide.Cells[8, 3].Value = "Văn bản nội dung lựa chọn B";
        wsGuide.Cells[8, 4].Value = "Nội dung đáp án B. Tự luận ĐỂ TRỐNG.";

        wsGuide.Cells[9, 1].Value = "LuaChonC";
        wsGuide.Cells[9, 2].Value = "Không";
        wsGuide.Cells[9, 3].Value = "Văn bản nội dung lựa chọn C";
        wsGuide.Cells[9, 4].Value = "Nội dung đáp án C (nếu có). Tự luận ĐỂ TRỐNG.";

        wsGuide.Cells[10, 1].Value = "LuaChonD";
        wsGuide.Cells[10, 2].Value = "Không";
        wsGuide.Cells[10, 3].Value = "Văn bản nội dung lựa chọn D";
        wsGuide.Cells[10, 4].Value = "Nội dung đáp án D (nếu có). Tự luận ĐỂ TRỐNG.";

        wsGuide.Cells[11, 1].Value = "LuaChonE";
        wsGuide.Cells[11, 2].Value = "Không";
        wsGuide.Cells[11, 3].Value = "Văn bản nội dung lựa chọn E";
        wsGuide.Cells[11, 4].Value = "Nội dung đáp án E (nếu có). Tự luận ĐỂ TRỐNG.";

        wsGuide.Cells[12, 1].Value = "DapAnDung";
        wsGuide.Cells[12, 2].Value = "Trắc nghiệm";
        wsGuide.Cells[12, 3].Value = "Ví dụ: B hoặc A, B";
        wsGuide.Cells[12, 4].Value = "Nhập tên chữ cái của đáp án đúng (ví dụ: B cho chọn một, hoặc A, B cho chọn nhiều). Tự luận ĐỂ TRỐNG.";

        wsGuide.Cells[13, 1].Value = "GiaiThichDapAn";
        wsGuide.Cells[13, 2].Value = "Không";
        wsGuide.Cells[13, 3].Value = "Văn bản";
        wsGuide.Cells[13, 4].Value = "Giải thích đáp án cho trắc nghiệm hoặc Hướng dẫn chấm cho tự luận";

        var wsSubjects = package.Workbook.Worksheets.Add("DanhSachMonHoc");
        wsSubjects.Cells[1, 1].Value = "MaCodeMonHoc";
        wsSubjects.Cells[1, 2].Value = "TenMonHoc";
        
        var monHocs = await _context.DanhMucMonHocs.AsNoTracking().ToListAsync(cancellationToken);
        for (int i = 0; i < monHocs.Count; i++)
        {
            wsSubjects.Cells[i + 2, 1].Value = monHocs[i].MaCodeMonHoc;
            wsSubjects.Cells[i + 2, 2].Value = monHocs[i].TenMonHoc;
        }

        return await package.GetAsByteArrayAsync(cancellationToken);
    }

    public async Task<int> ImportQuestionsAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        if (file == null || file.Length == 0) throw new ApiException(StatusCodes.Status400BadRequest, "File không hợp lệ");

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);
        using var package = new ExcelPackage(stream);
        var ws = package.Workbook.Worksheets["Questions"] ?? package.Workbook.Worksheets.FirstOrDefault();
        if (ws == null) throw new ApiException(StatusCodes.Status400BadRequest, "Không tìm thấy sheet dữ liệu trong file Excel");

        int rowCount = ws.Dimension?.Rows ?? 0;
        int colCount = ws.Dimension?.Columns ?? 0;
        if (rowCount <= 1) throw new ApiException(StatusCodes.Status400BadRequest, "File mẫu trống");

        int colMaCode = 1, colLoai = 2, colDoKho = 3, colKieuLuaChon = 4, colNoiDung = 5;
        int colLuaChonA = 6, colLuaChonB = 7, colLuaChonC = 8, colLuaChonD = 9, colLuaChonE = 10;
        int colDapAnDung = 11, colGiaiThich = 12;
        int colLuaChonLegacy = -1;

        bool hasCustomHeaders = false;
        for (int c = 1; c <= Math.Max(colCount, 15); c++)
        {
            var h = ws.Cells[1, c].Text?.Trim().ToLowerInvariant() ?? "";
            if (string.IsNullOrEmpty(h)) continue;

            if (h == "macodemonhoc" || h == "mamonhoc" || h == "mã môn" || h == "mã môn học") { colMaCode = c; hasCustomHeaders = true; }
            else if (h == "loaicauhoi" || h == "loại câu hỏi") { colLoai = c; hasCustomHeaders = true; }
            else if (h == "dokho" || h == "độ khó") { colDoKho = c; hasCustomHeaders = true; }
            else if (h == "kieuluachon" || h == "kiểu lựa chọn") { colKieuLuaChon = c; hasCustomHeaders = true; }
            else if (h == "noidung" || h == "nội dung" || h == "nội dung câu hỏi") { colNoiDung = c; hasCustomHeaders = true; }
            else if (h == "luachona" || h == "lựa chọn a" || h == "đáp án a" || h == "a") { colLuaChonA = c; hasCustomHeaders = true; }
            else if (h == "luachonb" || h == "lựa chọn b" || h == "đáp án b" || h == "b") { colLuaChonB = c; hasCustomHeaders = true; }
            else if (h == "luachonc" || h == "lựa chọn c" || h == "đáp án c" || h == "c") { colLuaChonC = c; hasCustomHeaders = true; }
            else if (h == "luachond" || h == "lựa chọn d" || h == "đáp án d" || h == "d") { colLuaChonD = c; hasCustomHeaders = true; }
            else if (h == "luachone" || h == "lựa chọn e" || h == "đáp án e" || h == "e") { colLuaChonE = c; hasCustomHeaders = true; }
            else if (h == "luachon" || h == "lựa chọn") { colLuaChonLegacy = c; hasCustomHeaders = true; }
            else if (h == "dapandung" || h == "đáp án đúng" || h == "đáp án") { colDapAnDung = c; hasCustomHeaders = true; }
            else if (h == "giaithichdapan" || h == "giaithich" || h == "giai thich" || h == "giải thích" || h == "hướng dẫn chấm") { colGiaiThich = c; hasCustomHeaders = true; }
        }

        if (!hasCustomHeaders && colLuaChonLegacy == -1 && ws.Cells[1, 6].Text?.Trim().Equals("LuaChon", StringComparison.OrdinalIgnoreCase) == true)
        {
            colLuaChonLegacy = 6;
            colDapAnDung = 7;
            colGiaiThich = 8;
        }

        var questionsToImport = new List<CauHoi>();
        var monHocList = await _context.DanhMucMonHocs.AsNoTracking().ToListAsync(cancellationToken);

        for (int row = 2; row <= rowCount; row++)
        {
            var rawMaCode = ws.Cells[row, colMaCode].Text?.Trim();
            if (string.IsNullOrWhiteSpace(rawMaCode)) continue; // skip empty rows

            var cleanCode = rawMaCode.Split('-')[0].Trim().ToUpper();
            var monHoc = monHocList.FirstOrDefault(x => x.MaCodeMonHoc.Trim().Equals(cleanCode, StringComparison.OrdinalIgnoreCase));

            if (monHoc == null)
            {
                monHoc = monHocList.FirstOrDefault(x => x.TenMonHoc.Trim().Equals(rawMaCode, StringComparison.OrdinalIgnoreCase));
            }

            if (monHoc == null)
                throw new ApiException(StatusCodes.Status400BadRequest, $"Dòng {row}: Không tìm thấy môn học với mã/tên '{rawMaCode}'");

            int maMonHoc = monHoc.MaMonHoc;

            var rawLoai = ws.Cells[row, colLoai].Text?.Trim() ?? "";
            var rawDoKho = ws.Cells[row, colDoKho].Text?.Trim() ?? "";
            var rawKieuLuaChon = ws.Cells[row, colKieuLuaChon].Text?.Trim();
            var noiDung = ws.Cells[row, colNoiDung].Text?.Trim() ?? "";
            var giaiThich = ws.Cells[row, colGiaiThich].Text?.Trim();

            List<QuestionChoiceDto>? luaChon = null;
            List<string>? dapAn = null;

            // 1. Read Choices
            string? legacyLuaChonStr = colLuaChonLegacy > 0 ? ws.Cells[row, colLuaChonLegacy].Text?.Trim() : null;
            if (!string.IsNullOrWhiteSpace(legacyLuaChonStr) && legacyLuaChonStr.StartsWith("["))
            {
                try
                {
                    luaChon = JsonSerializer.Deserialize<List<QuestionChoiceDto>>(legacyLuaChonStr, JsonOptions);
                }
                catch
                {
                    throw new ApiException(StatusCodes.Status400BadRequest, $"Dòng {row}: Sai định dạng JSON của LuaChon");
                }
            }
            else
            {
                var choiceList = new List<QuestionChoiceDto>();
                var optA = ws.Cells[row, colLuaChonA].Text?.Trim();
                var optB = ws.Cells[row, colLuaChonB].Text?.Trim();
                var optC = ws.Cells[row, colLuaChonC].Text?.Trim();
                var optD = ws.Cells[row, colLuaChonD].Text?.Trim();
                var optE = colLuaChonE > 0 ? ws.Cells[row, colLuaChonE].Text?.Trim() : null;

                if (!string.IsNullOrWhiteSpace(optA)) choiceList.Add(new QuestionChoiceDto { Id = "A", Content = optA });
                if (!string.IsNullOrWhiteSpace(optB)) choiceList.Add(new QuestionChoiceDto { Id = "B", Content = optB });
                if (!string.IsNullOrWhiteSpace(optC)) choiceList.Add(new QuestionChoiceDto { Id = "C", Content = optC });
                if (!string.IsNullOrWhiteSpace(optD)) choiceList.Add(new QuestionChoiceDto { Id = "D", Content = optD });
                if (!string.IsNullOrWhiteSpace(optE)) choiceList.Add(new QuestionChoiceDto { Id = "E", Content = optE });

                if (choiceList.Count > 0) luaChon = choiceList;
            }

            // 2. Read Correct Answers
            var rawDapAn = ws.Cells[row, colDapAnDung].Text?.Trim();
            if (!string.IsNullOrWhiteSpace(rawDapAn))
            {
                if (rawDapAn.StartsWith("["))
                {
                    try
                    {
                        dapAn = JsonSerializer.Deserialize<List<string>>(rawDapAn, JsonOptions);
                    }
                    catch
                    {
                        throw new ApiException(StatusCodes.Status400BadRequest, $"Dòng {row}: Sai định dạng JSON của DapAnDung");
                    }
                }
                else
                {
                    dapAn = rawDapAn
                        .Split(new[] { ',', ';', ' ', '|' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim().ToUpper())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .Distinct()
                        .ToList();
                }
            }

            var loai = NormalizeLoaiCauHoi(rawLoai);
            var doKho = NormalizeDoKho(rawDoKho);
            var kieuLuaChon = NormalizeKieuLuaChon(loai, rawKieuLuaChon, dapAn?.Count ?? 0);

            try
            {
                await ValidateQuestionAsync(null, maMonHoc, loai, noiDung, kieuLuaChon, luaChon, dapAn, cancellationToken);
            }
            catch (ApiException ex)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, $"Dòng {row}: {ex.Message}");
            }

            string? luaChonJson = luaChon != null && luaChon.Count > 0 ? JsonSerializer.Serialize(luaChon, JsonOptions) : null;
            string? dapAnDungJson = dapAn != null && dapAn.Count > 0 ? JsonSerializer.Serialize(dapAn, JsonOptions) : null;

            questionsToImport.Add(new CauHoi
            {
                MaMonHoc = maMonHoc,
                NguoiTao = currentUser.UserId,
                LoaiCauHoi = loai,
                NoiDung = noiDung,
                KieuLuaChon = kieuLuaChon,
                LuaChon = luaChonJson,
                DapAnDung = dapAnDungJson,
                GiaiThichDapAn = string.IsNullOrWhiteSpace(giaiThich) ? null : giaiThich,
                DoKho = doKho,
                ConHoatDong = true,
                NgayTao = DateTime.UtcNow
            });
        }

        if (questionsToImport.Count > 0)
        {
            try
            {
                _context.CauHois.AddRange(questionsToImport);
                await _context.SaveChangesAsync(cancellationToken);
                await _auditLogService.AddAsync(currentUser.CampusId, "CauHoi", 0, "IMPORT_QUESTIONS", currentUser.UserId, null, new { Count = questionsToImport.Count }, cancellationToken);
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                throw new ApiException(StatusCodes.Status400BadRequest, $"Lỗi lưu cơ sở dữ liệu: {msg}");
            }
        }

        return questionsToImport.Count;
    }

    private static string NormalizeLoaiCauHoi(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "trac_nghiem";
        var s = input.Trim().ToLower();
        if (s.Contains("tu_luan") || s.Contains("tự luận") || s.Contains("essay")) return "tu_luan";
        return "trac_nghiem";
    }

    private static string NormalizeDoKho(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "trung_binh";
        var s = input.Trim().ToLower();
        if (s.Contains("de") || s.Contains("dễ") || s.Contains("easy") || s == "1") return "de";
        if (s.Contains("kho") || s.Contains("khó") || s.Contains("hard") || s == "3") return "kho";
        return "trung_binh";
    }

    private static string? NormalizeKieuLuaChon(string loaiCauHoi, string? input, int answerCount)
    {
        if (loaiCauHoi == "tu_luan") return null;
        if (!string.IsNullOrWhiteSpace(input))
        {
            var s = input.Trim().ToLower();
            if (s.Contains("nhieu") || s.Contains("nhiều") || s.Contains("multi") || s == "2") return "chon_nhieu";
            if (s.Contains("mot") || s.Contains("một") || s.Contains("single") || s == "1") return "chon_mot";
        }
        return answerCount > 1 ? "chon_nhieu" : "chon_mot";
    }

    private async Task ValidateQuestionAsync(int? id, int? maMonHoc, string loaiCauHoi, string noiDung, string? kieuLuaChon, List<QuestionChoiceDto>? choices, List<string>? answers, CancellationToken cancellationToken)
    {
        if (loaiCauHoi != "trac_nghiem" && loaiCauHoi != "tu_luan") throw new ApiException(StatusCodes.Status400BadRequest, "Loại câu hỏi không hợp lệ");

        var isDuplicate = await _context.CauHois.AnyAsync(x => x.MaMonHoc == maMonHoc && x.NoiDung.ToLower() == noiDung.ToLower() && x.MaCauHoi != id, cancellationToken);
        if (isDuplicate) throw new ApiException(StatusCodes.Status400BadRequest, $"Nội dung câu hỏi '{noiDung}' bị trùng lặp trong môn học này");

        if (loaiCauHoi == "trac_nghiem")
        {
            if (kieuLuaChon != "chon_mot" && kieuLuaChon != "chon_nhieu") throw new ApiException(StatusCodes.Status400BadRequest, "Câu trắc nghiệm phải xác định kiểu chọn một hoặc chọn nhiều");
            if (choices == null || choices.Count < 2) throw new ApiException(StatusCodes.Status400BadRequest, "Câu trắc nghiệm phải có ít nhất 2 lựa chọn");
            if (answers == null || answers.Count == 0) throw new ApiException(StatusCodes.Status400BadRequest, "Câu trắc nghiệm phải có đáp án đúng");

            if (choices.Select(c => c.Id).Distinct().Count() != choices.Count) throw new ApiException(StatusCodes.Status400BadRequest, "ID đáp án không được trùng nhau");
            if (choices.Any(c => string.IsNullOrWhiteSpace(c.Content))) throw new ApiException(StatusCodes.Status400BadRequest, "Nội dung đáp án không được rỗng");

            if (!answers.All(a => choices.Any(c => c.Id == a))) throw new ApiException(StatusCodes.Status400BadRequest, $"Đáp án đúng ({string.Join(',', answers)}) phải tồn tại trong danh sách lựa chọn ({string.Join(',', choices.Select(c => c.Id))})");

            if (kieuLuaChon == "chon_mot" && answers.Count != 1) throw new ApiException(StatusCodes.Status400BadRequest, "Câu chọn một chỉ được phép có 1 đáp án đúng");
            if (kieuLuaChon == "chon_nhieu" && answers.Count < 2) throw new ApiException(StatusCodes.Status400BadRequest, "Câu chọn nhiều phải có ít nhất 2 đáp án đúng");
        }
    }

    private static QuestionDto MapToDto(CauHoi entity, int usageCount = 0)
    {
        return new QuestionDto
        {
            MaCauHoi = entity.MaCauHoi,
            MaMonHoc = entity.MaMonHoc,
            MaCodeMonHoc = entity.MonHoc?.MaCodeMonHoc,
            TenMonHoc = entity.MonHoc?.TenMonHoc,
            LoaiCauHoi = entity.LoaiCauHoi,
            NoiDung = entity.NoiDung,
            KieuLuaChon = entity.KieuLuaChon,
            LuaChon = entity.LuaChon != null ? JsonSerializer.Deserialize<List<QuestionChoiceDto>>(entity.LuaChon, JsonOptions) : null,
            DapAnDung = entity.DapAnDung != null ? JsonSerializer.Deserialize<List<string>>(entity.DapAnDung, JsonOptions) : null,
            GiaiThichDapAn = entity.GiaiThichDapAn,
            DoKho = entity.DoKho,
            ConHoatDong = entity.ConHoatDong,
            SoLanSuDung = usageCount,
            NgayTao = entity.NgayTao,
            NgayCapNhat = entity.NgayCapNhat
        };
    }
}
