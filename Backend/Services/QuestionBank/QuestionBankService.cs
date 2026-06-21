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
        var query = _context.CauHois.Include(x => x.MonHoc).AsNoTracking();

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

        var dtos = items.Select(MapToDto).ToList();

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
        return MapToDto(cauHoi);
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
        wsQuestions.Cells[1, 6].Value = "LuaChon";
        wsQuestions.Cells[1, 7].Value = "DapAnDung";
        wsQuestions.Cells[1, 8].Value = "GiaiThichDapAn";

        wsQuestions.Cells[2, 1].Value = "COM101";
        wsQuestions.Cells[2, 2].Value = "trac_nghiem";
        wsQuestions.Cells[2, 3].Value = "de";
        wsQuestions.Cells[2, 4].Value = "chon_mot";
        wsQuestions.Cells[2, 5].Value = "1+1 bằng mấy?";
        wsQuestions.Cells[2, 6].Value = "[{\"Id\":\"A\",\"Content\":\"1\"},{\"Id\":\"B\",\"Content\":\"2\"}]";
        wsQuestions.Cells[2, 7].Value = "[\"B\"]";
        wsQuestions.Cells[2, 8].Value = "Toán học cơ bản";

        var wsGuide = package.Workbook.Worksheets.Add("HuongDan");
        wsGuide.Cells[1, 1].Value = "Hướng dẫn import câu hỏi";
        // Bổ sung hướng dẫn cơ bản nếu cần
        
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
        var ws = package.Workbook.Worksheets["Questions"];
        if (ws == null) throw new ApiException(StatusCodes.Status400BadRequest, "Không tìm thấy sheet Questions");

        int rowCount = ws.Dimension?.Rows ?? 0;
        if (rowCount <= 1) throw new ApiException(StatusCodes.Status400BadRequest, "File mẫu trống");

        var questionsToImport = new List<CauHoi>();
        var monHocDict = await _context.DanhMucMonHocs.AsNoTracking().ToDictionaryAsync(x => x.MaCodeMonHoc, x => x.MaMonHoc, cancellationToken);

        for (int row = 2; row <= rowCount; row++)
        {
            var maCode = ws.Cells[row, 1].Text?.Trim();
            if (string.IsNullOrWhiteSpace(maCode)) continue; // skip empty rows

            if (!monHocDict.TryGetValue(maCode, out int maMonHoc))
                throw new ApiException(StatusCodes.Status400BadRequest, $"Dòng {row}: Mã môn học {maCode} không tồn tại");

            var loai = ws.Cells[row, 2].Text?.Trim() ?? "";
            var doKho = ws.Cells[row, 3].Text?.Trim() ?? "";
            var kieuLuaChon = ws.Cells[row, 4].Text?.Trim();
            var noiDung = ws.Cells[row, 5].Text?.Trim() ?? "";
            var luaChonStr = ws.Cells[row, 6].Text?.Trim();
            var dapAnDungStr = ws.Cells[row, 7].Text?.Trim();
            var giaiThich = ws.Cells[row, 8].Text?.Trim();

            List<QuestionChoiceDto>? luaChon = null;
            List<string>? dapAn = null;
            
            try
            {
                if (!string.IsNullOrWhiteSpace(luaChonStr)) luaChon = JsonSerializer.Deserialize<List<QuestionChoiceDto>>(luaChonStr, JsonOptions);
                if (!string.IsNullOrWhiteSpace(dapAnDungStr)) dapAn = JsonSerializer.Deserialize<List<string>>(dapAnDungStr, JsonOptions);
            }
            catch
            {
                throw new ApiException(StatusCodes.Status400BadRequest, $"Dòng {row}: Sai định dạng JSON của LuaChon hoặc DapAnDung");
            }

            await ValidateQuestionAsync(null, maMonHoc, loai, noiDung, string.IsNullOrWhiteSpace(kieuLuaChon) ? null : kieuLuaChon, luaChon, dapAn, cancellationToken);

            questionsToImport.Add(new CauHoi
            {
                MaMonHoc = maMonHoc,
                NguoiTao = currentUser.UserId,
                LoaiCauHoi = loai,
                NoiDung = noiDung,
                KieuLuaChon = string.IsNullOrWhiteSpace(kieuLuaChon) ? null : kieuLuaChon,
                LuaChon = luaChonStr,
                DapAnDung = dapAnDungStr,
                GiaiThichDapAn = string.IsNullOrWhiteSpace(giaiThich) ? null : giaiThich,
                DoKho = doKho,
                ConHoatDong = true,
                NgayTao = DateTime.UtcNow
            });
        }

        if (questionsToImport.Count > 0)
        {
            _context.CauHois.AddRange(questionsToImport);
            await _context.SaveChangesAsync(cancellationToken);
            await _auditLogService.AddAsync(currentUser.CampusId, "CauHoi", 0, "IMPORT_QUESTIONS", currentUser.UserId, null, new { Count = questionsToImport.Count }, cancellationToken);
        }

        return questionsToImport.Count;
    }

    private async Task ValidateQuestionAsync(int? id, int? maMonHoc, string loaiCauHoi, string noiDung, string? kieuLuaChon, List<QuestionChoiceDto>? choices, List<string>? answers, CancellationToken cancellationToken)
    {
        if (loaiCauHoi != "trac_nghiem" && loaiCauHoi != "tu_luan") throw new ApiException(StatusCodes.Status400BadRequest, "Loại câu hỏi không hợp lệ");

        var isDuplicate = await _context.CauHois.AnyAsync(x => x.MaMonHoc == maMonHoc && x.NoiDung.ToLower() == noiDung.ToLower() && x.MaCauHoi != id, cancellationToken);
        if (isDuplicate) throw new ApiException(StatusCodes.Status400BadRequest, "Nội dung câu hỏi bị trùng lặp trong môn học này");

        if (loaiCauHoi == "trac_nghiem")
        {
            if (kieuLuaChon != "chon_mot" && kieuLuaChon != "chon_nhieu") throw new ApiException(StatusCodes.Status400BadRequest, "Câu trắc nghiệm phải xác định kiểu chọn một hoặc chọn nhiều");
            if (choices == null || choices.Count < 2) throw new ApiException(StatusCodes.Status400BadRequest, "Câu trắc nghiệm phải có ít nhất 2 lựa chọn");
            if (answers == null || answers.Count == 0) throw new ApiException(StatusCodes.Status400BadRequest, "Câu trắc nghiệm phải có đáp án đúng");

            if (choices.Select(c => c.Id).Distinct().Count() != choices.Count) throw new ApiException(StatusCodes.Status400BadRequest, "ID đáp án không được trùng nhau");
            if (choices.Any(c => string.IsNullOrWhiteSpace(c.Content))) throw new ApiException(StatusCodes.Status400BadRequest, "Nội dung đáp án không được rỗng");

            if (!answers.All(a => choices.Any(c => c.Id == a))) throw new ApiException(StatusCodes.Status400BadRequest, "Đáp án đúng phải tồn tại trong danh sách lựa chọn");

            if (kieuLuaChon == "chon_mot" && answers.Count != 1) throw new ApiException(StatusCodes.Status400BadRequest, "Câu chọn một chỉ được phép có 1 đáp án đúng");
            if (kieuLuaChon == "chon_nhieu" && answers.Count < 2) throw new ApiException(StatusCodes.Status400BadRequest, "Câu chọn nhiều phải có ít nhất 2 đáp án đúng");
        }
    }

    private static QuestionDto MapToDto(CauHoi entity)
    {
        return new QuestionDto
        {
            MaCauHoi = entity.MaCauHoi,
            MaMonHoc = entity.MaMonHoc,
            TenMonHoc = entity.MonHoc?.TenMonHoc,
            LoaiCauHoi = entity.LoaiCauHoi,
            NoiDung = entity.NoiDung,
            KieuLuaChon = entity.KieuLuaChon,
            LuaChon = entity.LuaChon != null ? JsonSerializer.Deserialize<List<QuestionChoiceDto>>(entity.LuaChon, JsonOptions) : null,
            DapAnDung = entity.DapAnDung != null ? JsonSerializer.Deserialize<List<string>>(entity.DapAnDung, JsonOptions) : null,
            GiaiThichDapAn = entity.GiaiThichDapAn,
            DoKho = entity.DoKho,
            ConHoatDong = entity.ConHoatDong,
            NgayTao = entity.NgayTao,
            NgayCapNhat = entity.NgayCapNhat
        };
    }
}
