using Backend.Data;
using Backend.DTOs.Common;
using Backend.DTOs.QuizManagement;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Audit;
using Backend.Services.QuizRuntime;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Backend.Services.QuizManagement;

public class QuizManagementService : IQuizManagementService
{
    private readonly ApplicationDbContext _db;
    private readonly IAuditLogService _auditLogService;
    private readonly IQuizAvailabilityService _availabilityService;

    public QuizManagementService(ApplicationDbContext db, IAuditLogService auditLogService, IQuizAvailabilityService availabilityService)
    {
        _db = db;
        _auditLogService = auditLogService;
        _availabilityService = availabilityService;
    }

    private async Task<bool> IsQuizInUseAsync(int quizId, CancellationToken ct)
    {
        var usedInLichThi = await _db.LichThiTongs.AnyAsync(x => x.MaDeKiemTra == quizId, ct);
        if (usedInLichThi) return true;

        var usedInPhienThi = await _db.PhienThiHocSinhs.AnyAsync(x => x.MaDeKiemTra == quizId, ct);
        if (usedInPhienThi) return true;

        return false;
    }

    public async Task<PagedResultDto<QuizDto>> GetQuizzesAsync(QuizFilterDto filter, CancellationToken ct)
    {
        await _availabilityService.SynchronizeScheduledQuizzesAsync(DateTime.UtcNow, ct);

        var query = _db.DeKiemTras
            .Include(x => x.MonHoc)
            .Include(x => x.HocKy)
            .Include(x => x.NguoiSoan)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var kw = filter.Keyword.Trim().ToLower();
            query = query.Where(x => x.TieuDe.ToLower().Contains(kw));
        }

        if (filter.MaMonHoc.HasValue)
        {
            query = query.Where(x => x.MaMonHoc == filter.MaMonHoc);
        }

        if (filter.MaHocKy.HasValue)
        {
            query = query.Where(x => x.MaHocKy == filter.MaHocKy);
        }

        if (!string.IsNullOrWhiteSpace(filter.TrangThai))
        {
            query = query.Where(x => x.TrangThai == filter.TrangThai);
        }

        if (!string.IsNullOrWhiteSpace(filter.LoaiDeThi))
        {
            query = query.Where(x => x.LoaiDeThi == filter.LoaiDeThi);
        }

        if (!string.IsNullOrWhiteSpace(filter.HinhThucThi))
        {
            query = query.Where(x => x.HinhThucThi == filter.HinhThucThi);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.NgayCapNhat)
            .ThenByDescending(x => x.NgayTao)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(x => new QuizDto
            {
                MaDeKiemTra = x.MaDeKiemTra,
                MaMonHoc = x.MaMonHoc,
                MaCodeMonHoc = x.MonHoc != null ? x.MonHoc.MaCodeMonHoc : null,
                TenMonHoc = x.MonHoc != null ? x.MonHoc.TenMonHoc : null,
                MaHocKy = x.MaHocKy,
                TenHocKy = x.HocKy != null ? x.HocKy.TenHocKy : null,
                TieuDe = x.TieuDe,
                ThoiGianPhut = x.ThoiGianPhut,
                TrangThai = x.TrangThai,
                LoaiDeThi = x.LoaiDeThi,
                HinhThucThi = x.HinhThucThi,
                TyLeTracNghiem = x.TyLeTracNghiem,
                TyLeTuLuan = x.TyLeTuLuan,
                MaNguoiSoan = x.MaNguoiSoan,
                TenNguoiSoan = x.NguoiSoan != null ? x.NguoiSoan.HoTen : null,
                TrangThaiDuyet = x.TrangThaiDuyet,
                NgayTao = x.NgayTao,
                NgayCapNhat = x.NgayCapNhat,
                SoCauHoi = _db.CauHoiDeKiemTras.Count(c => c.MaDeKiemTra == x.MaDeKiemTra),
                TongDiem = _db.CauHoiDeKiemTras.Where(c => c.MaDeKiemTra == x.MaDeKiemTra).Sum(c => c.DiemSo)
            })
            .ToListAsync(ct);

        return new PagedResultDto<QuizDto>
        {
            Items = items,
            TotalItems = totalCount,
            PageIndex = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    public async Task<QuizDetailDto> GetQuizByIdAsync(int id, CancellationToken ct)
    {
        await _availabilityService.SynchronizeQuizStatusAsync(id, DateTime.UtcNow, ct);

        var entity = await _db.DeKiemTras
            .Include(x => x.MonHoc)
            .FirstOrDefaultAsync(x => x.MaDeKiemTra == id, ct);

        if (entity == null)
            throw new ApiException(404, "Không tìm thấy Đề kiểm tra");

        var questions = await _db.CauHoiDeKiemTras
            .Include(x => x.CauHoi)
            .Where(x => x.MaDeKiemTra == id)
            .OrderBy(x => x.ThuTu)
            .ToListAsync(ct);

        var config = QuizConfigurationDto.Parse(entity.CauHinhDeThi);

        var dto = new QuizDetailDto
        {
            MaDeKiemTra = entity.MaDeKiemTra,
            MaMonHoc = entity.MaMonHoc,
            TenMonHoc = entity.MonHoc?.TenMonHoc,
            MaHocKy = entity.MaHocKy,
            TieuDe = entity.TieuDe,
            ThoiGianPhut = entity.ThoiGianPhut,
            TrangThai = entity.TrangThai,
            LoaiDeThi = entity.LoaiDeThi,
            HinhThucThi = entity.HinhThucThi,
            TyLeTracNghiem = entity.TyLeTracNghiem,
            TyLeTuLuan = entity.TyLeTuLuan,
            MaNguoiSoan = entity.MaNguoiSoan,
            TrangThaiDuyet = entity.TrangThaiDuyet,
            NgayTao = entity.NgayTao,
            NgayCapNhat = entity.NgayCapNhat,
            CauHinh = config,
            TongDiemCauHoi = questions.Sum(x => x.DiemSo),
            TongSoCauHoi = questions.Count,
            SoCauTracNghiem = questions.Count(x => x.CauHoi?.LoaiCauHoi == "trac_nghiem"),
            SoCauTuLuan = questions.Count(x => x.CauHoi?.LoaiCauHoi == "tu_luan"),
            DanhSachCauHoi = questions.Select(x => new QuizQuestionDto
            {
                MaCauHoi = x.MaCauHoi,
                MaMonHoc = x.CauHoi!.MaMonHoc,
                LoaiCauHoi = x.CauHoi.LoaiCauHoi,
                NoiDung = x.CauHoi.NoiDung,
                KieuLuaChon = x.CauHoi.KieuLuaChon,
                LuaChon = QuizQuestionDto.ParseJson(x.CauHoi.LuaChon),
                DapAnDung = QuizQuestionDto.ParseJson(x.CauHoi.DapAnDung),
                GiaiThichDapAn = x.CauHoi.GiaiThichDapAn,
                DoKho = x.CauHoi.DoKho,
                ConHoatDong = x.CauHoi.ConHoatDong,
                DiemSo = x.DiemSo,
                ThuTu = x.ThuTu
            }).ToList()
        };

        return dto;
    }

    public async Task<QuizDetailDto> CreateQuizAsync(CreateQuizRequest request, int userId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.TieuDe))
            throw new ApiException(400, "Tiêu đề không được để trống");

        var monHocExists = await _db.DanhMucMonHocs.AnyAsync(x => x.MaMonHoc == request.MaMonHoc, ct);
        if (!monHocExists)
            throw new ApiException(404, "Môn học không tồn tại");

        if (request.TyLeTracNghiem.HasValue && request.TyLeTuLuan.HasValue)
        {
            if (request.TyLeTracNghiem.Value + request.TyLeTuLuan.Value != 100)
                throw new ApiException(400, "Tổng tỷ lệ trắc nghiệm và tự luận phải bằng 100");
        }

        request.CauHinh.Validate();

        var entity = new DeKiemTra
        {
            MaMonHoc = request.MaMonHoc,
            MaHocKy = request.MaHocKy,
            TieuDe = request.TieuDe.Trim(),
            ThoiGianPhut = request.ThoiGianPhut,
            LoaiDeThi = request.LoaiDeThi,
            HinhThucThi = request.HinhThucThi,
            TyLeTracNghiem = request.TyLeTracNghiem,
            TyLeTuLuan = request.TyLeTuLuan,
            MaNguoiSoan = userId,
            TrangThai = "nhap",
            NgayTao = DateTime.UtcNow,
            CauHinhDeThi = request.CauHinh.ToJson()
        };

        _db.DeKiemTras.Add(entity);
        await _db.SaveChangesAsync(ct);

        await _auditLogService.LogAsync(
            "DeKiemTra", 
            entity.MaDeKiemTra.ToString(), 
            "CREATE_QUIZ", 
            null, 
            new { entity.TieuDe, entity.MaMonHoc, request.CauHinh }, 
            userId, 
            null, 
            "Tạo đề kiểm tra mới", 
            ct);

        return await GetQuizByIdAsync(entity.MaDeKiemTra, ct);
    }

    public async Task<QuizDetailDto> UpdateQuizAsync(int id, UpdateQuizRequest request, int userId, CancellationToken ct)
    {
        var entity = await _db.DeKiemTras.FirstOrDefaultAsync(x => x.MaDeKiemTra == id, ct);
        if (entity == null)
            throw new ApiException(404, "Không tìm thấy Đề kiểm tra");

        if (entity.TrangThai != "nhap")
            throw new ApiException(409, "Chỉ có thể cập nhật thông tin khi đề kiểm tra ở trạng thái nháp");

        var inUse = await IsQuizInUseAsync(id, ct);
        if (inUse)
        {
            if (entity.MaMonHoc != request.MaMonHoc ||
                entity.LoaiDeThi != request.LoaiDeThi ||
                entity.HinhThucThi != request.HinhThucThi)
            {
                throw new ApiException(409, "Đề kiểm tra đã được sử dụng, không thể thay đổi Môn học, Loại đề thi hoặc Hình thức thi");
            }
        }

        if (string.IsNullOrWhiteSpace(request.TieuDe))
            throw new ApiException(400, "Tiêu đề không được để trống");

        if (entity.MaMonHoc != request.MaMonHoc)
        {
            var monHocExists = await _db.DanhMucMonHocs.AnyAsync(x => x.MaMonHoc == request.MaMonHoc, ct);
            if (!monHocExists)
                throw new ApiException(404, "Môn học không tồn tại");
        }

        if (request.TyLeTracNghiem.HasValue && request.TyLeTuLuan.HasValue)
        {
            if (request.TyLeTracNghiem.Value + request.TyLeTuLuan.Value != 100)
                throw new ApiException(400, "Tổng tỷ lệ trắc nghiệm và tự luận phải bằng 100");
        }

        request.CauHinh.Validate();

        var oldValues = new { entity.TieuDe, entity.MaMonHoc, entity.ThoiGianPhut, entity.CauHinhDeThi };

        entity.MaMonHoc = request.MaMonHoc;
        entity.MaHocKy = request.MaHocKy;
        entity.TieuDe = request.TieuDe.Trim();
        entity.ThoiGianPhut = request.ThoiGianPhut;
        entity.LoaiDeThi = request.LoaiDeThi;
        entity.HinhThucThi = request.HinhThucThi;
        entity.TyLeTracNghiem = request.TyLeTracNghiem;
        entity.TyLeTuLuan = request.TyLeTuLuan;
        entity.CauHinhDeThi = request.CauHinh.ToJson();
        entity.NgayCapNhat = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        await _auditLogService.LogAsync(
            "DeKiemTra", 
            id.ToString(), 
            "UPDATE_QUIZ", 
            oldValues, 
            new { entity.TieuDe, entity.MaMonHoc, entity.ThoiGianPhut, entity.CauHinhDeThi }, 
            userId, 
            null, 
            "Cập nhật đề kiểm tra", 
            ct);

        return await GetQuizByIdAsync(id, ct);
    }

    public async Task DeleteQuizAsync(int id, int userId, CancellationToken ct)
    {
        var entity = await _db.DeKiemTras.FirstOrDefaultAsync(x => x.MaDeKiemTra == id, ct);
        if (entity == null)
            throw new ApiException(404, "Không tìm thấy Đề kiểm tra");

        if (entity.TrangThai != "nhap")
            throw new ApiException(409, "Chỉ có thể xóa đề kiểm tra ở trạng thái nháp");

        var inUse = await IsQuizInUseAsync(id, ct);
        if (inUse)
            throw new ApiException(409, "Đề kiểm tra đã được sử dụng trong lịch thi hoặc phiên làm bài, không thể xóa");

        using var transaction = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var relations = await _db.CauHoiDeKiemTras.Where(x => x.MaDeKiemTra == id).ToListAsync(ct);
            _db.CauHoiDeKiemTras.RemoveRange(relations);
            _db.DeKiemTras.Remove(entity);

            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            await _auditLogService.LogAsync(
                "DeKiemTra", 
                id.ToString(), 
                "DELETE_QUIZ", 
                new { entity.TieuDe, entity.MaMonHoc }, 
                null, 
                userId, 
                null, 
                "Xóa đề kiểm tra", 
                ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<IReadOnlyList<QuizQuestionDto>> GetQuizQuestionsAsync(int quizId, CancellationToken ct)
    {
        var quiz = await GetQuizByIdAsync(quizId, ct);
        return quiz.DanhSachCauHoi;
    }

    public async Task<IReadOnlyList<QuizQuestionDto>> AssignQuestionsAsync(int quizId, AssignQuizQuestionsRequest request, int userId, CancellationToken ct)
    {
        var quiz = await _db.DeKiemTras.FirstOrDefaultAsync(x => x.MaDeKiemTra == quizId, ct);
        if (quiz == null) throw new ApiException(404, "Không tìm thấy Đề kiểm tra");
        if (quiz.TrangThai != "nhap") throw new ApiException(409, "Chỉ có thể gán câu hỏi khi đề kiểm tra ở trạng thái nháp");

        var inUse = await IsQuizInUseAsync(quizId, ct);
        if (inUse) throw new ApiException(409, "Đề kiểm tra đã được sử dụng, không thể thêm câu hỏi");

        if (request.Questions.Select(x => x.MaCauHoi).Distinct().Count() != request.Questions.Count)
            throw new ApiException(400, "Danh sách câu hỏi đầu vào bị trùng lặp");

        if (request.Questions.Select(x => x.ThuTu).Distinct().Count() != request.Questions.Count)
            throw new ApiException(400, "Thứ tự câu hỏi bị trùng lặp");

        var existingQuestionIds = await _db.CauHoiDeKiemTras
            .Where(x => x.MaDeKiemTra == quizId)
            .Select(x => x.MaCauHoi)
            .ToListAsync(ct);

        var newQuestionIds = request.Questions.Select(x => x.MaCauHoi).ToList();
        if (existingQuestionIds.Intersect(newQuestionIds).Any())
            throw new ApiException(409, "Một số câu hỏi đã tồn tại trong Đề kiểm tra");

        var questionsFromDb = await _db.CauHois
            .Where(x => newQuestionIds.Contains(x.MaCauHoi))
            .ToListAsync(ct);

        if (questionsFromDb.Count != newQuestionIds.Count)
            throw new ApiException(404, "Một số câu hỏi không tồn tại");

        if (questionsFromDb.Any(x => !x.ConHoatDong))
            throw new ApiException(400, "Không thể gán câu hỏi đang bị vô hiệu hóa");

        if (questionsFromDb.Any(x => x.MaMonHoc != quiz.MaMonHoc))
            throw new ApiException(400, "Tất cả câu hỏi phải thuộc cùng môn học với Đề kiểm tra");

        if (quiz.HinhThucThi == "trac_nghiem" && questionsFromDb.Any(x => x.LoaiCauHoi == "tu_luan"))
            throw new ApiException(400, "Đề trắc nghiệm không được chứa câu tự luận");

        if (quiz.HinhThucThi == "tu_luan" && questionsFromDb.Any(x => x.LoaiCauHoi != "tu_luan"))
            throw new ApiException(400, "Đề tự luận không được chứa câu trắc nghiệm");

        using var transaction = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var newRelations = request.Questions.Select(q => new CauHoiDeKiemTra
            {
                MaDeKiemTra = quizId,
                MaCauHoi = q.MaCauHoi,
                DiemSo = q.DiemSo,
                ThuTu = q.ThuTu
            }).ToList();

            _db.CauHoiDeKiemTras.AddRange(newRelations);
            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            await _auditLogService.LogAsync(
                "CauHoiDeKiemTra", 
                quizId.ToString(), 
                "ASSIGN_QUIZ_QUESTIONS", 
                null, 
                newQuestionIds, 
                userId, 
                null, 
                "Gán thêm câu hỏi vào đề kiểm tra", 
                ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }

        return await GetQuizQuestionsAsync(quizId, ct);
    }

    public async Task<IReadOnlyList<QuizQuestionDto>> ReplaceQuestionsAsync(int quizId, AssignQuizQuestionsRequest request, int userId, CancellationToken ct)
    {
        var quiz = await _db.DeKiemTras.FirstOrDefaultAsync(x => x.MaDeKiemTra == quizId, ct);
        if (quiz == null) throw new ApiException(404, "Không tìm thấy Đề kiểm tra");
        if (quiz.TrangThai != "nhap") throw new ApiException(409, "Chỉ có thể sửa đổi cấu trúc đề khi ở trạng thái nháp");

        var inUse = await IsQuizInUseAsync(quizId, ct);
        if (inUse) throw new ApiException(409, "Đề kiểm tra đã được sử dụng, không thể thay đổi cấu trúc đề");

        if (request.Questions.Select(x => x.MaCauHoi).Distinct().Count() != request.Questions.Count)
            throw new ApiException(400, "Danh sách câu hỏi đầu vào bị trùng lặp");

        if (request.Questions.Select(x => x.ThuTu).Distinct().Count() != request.Questions.Count)
            throw new ApiException(400, "Thứ tự câu hỏi bị trùng lặp");

        var newQuestionIds = request.Questions.Select(x => x.MaCauHoi).ToList();
        var questionsFromDb = await _db.CauHois
            .Where(x => newQuestionIds.Contains(x.MaCauHoi))
            .ToListAsync(ct);

        if (questionsFromDb.Count != newQuestionIds.Count)
            throw new ApiException(404, "Một số câu hỏi không tồn tại");

        if (questionsFromDb.Any(x => !x.ConHoatDong))
            throw new ApiException(400, "Không thể gán câu hỏi đang bị vô hiệu hóa");

        if (questionsFromDb.Any(x => x.MaMonHoc != quiz.MaMonHoc))
            throw new ApiException(400, "Tất cả câu hỏi phải thuộc cùng môn học với Đề kiểm tra");

        if (quiz.HinhThucThi == "trac_nghiem" && questionsFromDb.Any(x => x.LoaiCauHoi == "tu_luan"))
            throw new ApiException(400, "Đề trắc nghiệm không được chứa câu tự luận");

        if (quiz.HinhThucThi == "tu_luan" && questionsFromDb.Any(x => x.LoaiCauHoi != "tu_luan"))
            throw new ApiException(400, "Đề tự luận không được chứa câu trắc nghiệm");

        using var transaction = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var oldRelations = await _db.CauHoiDeKiemTras.Where(x => x.MaDeKiemTra == quizId).ToListAsync(ct);
            _db.CauHoiDeKiemTras.RemoveRange(oldRelations);

            var newRelations = request.Questions.Select(q => new CauHoiDeKiemTra
            {
                MaDeKiemTra = quizId,
                MaCauHoi = q.MaCauHoi,
                DiemSo = q.DiemSo,
                ThuTu = q.ThuTu
            }).ToList();

            _db.CauHoiDeKiemTras.AddRange(newRelations);
            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            await _auditLogService.LogAsync(
                "CauHoiDeKiemTra", 
                quizId.ToString(), 
                "REPLACE_QUIZ_QUESTIONS", 
                oldRelations.Select(x => x.MaCauHoi).ToList(), 
                newQuestionIds, 
                userId, 
                null, 
                "Thay thế toàn bộ câu hỏi trong đề kiểm tra", 
                ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }

        return await GetQuizQuestionsAsync(quizId, ct);
    }

    public async Task<QuizQuestionDto> UpdateQuestionAsync(int quizId, int questionId, UpdateQuizQuestionRequest request, int userId, CancellationToken ct)
    {
        var relation = await _db.CauHoiDeKiemTras.FirstOrDefaultAsync(x => x.MaDeKiemTra == quizId && x.MaCauHoi == questionId, ct);
        if (relation == null) throw new ApiException(404, "Câu hỏi không tồn tại trong Đề kiểm tra này");

        var inUse = await IsQuizInUseAsync(quizId, ct);
        if (inUse) throw new ApiException(409, "Đề kiểm tra đã được sử dụng, không thể sửa điểm hay thứ tự câu hỏi");

        if (relation.ThuTu != request.ThuTu)
        {
            var existsOrder = await _db.CauHoiDeKiemTras.AnyAsync(x => x.MaDeKiemTra == quizId && x.MaCauHoi != questionId && x.ThuTu == request.ThuTu, ct);
            if (existsOrder) throw new ApiException(400, "Thứ tự câu hỏi bị trùng lặp với câu khác");
        }

        var oldValues = new { relation.DiemSo, relation.ThuTu };
        relation.DiemSo = request.DiemSo;
        relation.ThuTu = request.ThuTu;

        await _db.SaveChangesAsync(ct);

        await _auditLogService.LogAsync(
            "CauHoiDeKiemTra", 
            $"{quizId}_{questionId}", 
            "UPDATE_QUIZ_QUESTION", 
            oldValues, 
            new { request.DiemSo, request.ThuTu }, 
            userId, 
            null, 
            "Cập nhật điểm và thứ tự câu hỏi", 
            ct);

        var questions = await GetQuizQuestionsAsync(quizId, ct);
        return questions.First(x => x.MaCauHoi == questionId);
    }

    public async Task RemoveQuestionAsync(int quizId, int questionId, int userId, CancellationToken ct)
    {
        var relation = await _db.CauHoiDeKiemTras.FirstOrDefaultAsync(x => x.MaDeKiemTra == quizId && x.MaCauHoi == questionId, ct);
        if (relation == null) throw new ApiException(404, "Câu hỏi không tồn tại trong Đề kiểm tra này");

        var inUse = await IsQuizInUseAsync(quizId, ct);
        if (inUse) throw new ApiException(409, "Đề kiểm tra đã được sử dụng, không thể xóa câu hỏi");

        _db.CauHoiDeKiemTras.Remove(relation);
        await _db.SaveChangesAsync(ct);

        await _auditLogService.LogAsync(
            "CauHoiDeKiemTra", 
            $"{quizId}_{questionId}", 
            "REMOVE_QUIZ_QUESTION", 
            new { relation.MaCauHoi, relation.DiemSo, relation.ThuTu }, 
            null, 
            userId, 
            null, 
            "Xóa câu hỏi khỏi đề kiểm tra", 
            ct);
    }

    public async Task ReorderQuestionsAsync(int quizId, ReorderQuizQuestionsRequest request, int userId, CancellationToken ct)
    {
        var relations = await _db.CauHoiDeKiemTras.Where(x => x.MaDeKiemTra == quizId).ToListAsync(ct);
        
        if (relations.Count != request.Items.Count)
            throw new ApiException(400, "Danh sách sắp xếp không khớp với số lượng câu hỏi hiện tại trong Đề");

        var requestIds = request.Items.Select(x => x.MaCauHoi).ToList();
        var dbIds = relations.Select(x => x.MaCauHoi).ToList();

        if (requestIds.Except(dbIds).Any() || dbIds.Except(requestIds).Any())
            throw new ApiException(400, "Danh sách sắp xếp chứa câu hỏi không hợp lệ hoặc thiếu câu hỏi");

        if (request.Items.Select(x => x.ThuTu).Distinct().Count() != request.Items.Count)
            throw new ApiException(400, "Thứ tự câu hỏi bị trùng lặp");

        var expectedOrders = Enumerable.Range(1, request.Items.Count).ToList();
        var providedOrders = request.Items.Select(x => x.ThuTu).OrderBy(x => x).ToList();
        if (!expectedOrders.SequenceEqual(providedOrders))
            throw new ApiException(400, "Thứ tự câu hỏi phải liên tục từ 1 đến N");

        var inUse = await IsQuizInUseAsync(quizId, ct);
        if (inUse) throw new ApiException(409, "Đề kiểm tra đã được sử dụng, không thể thay đổi thứ tự");

        using var transaction = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var oldOrder = relations.Select(x => new { x.MaCauHoi, x.ThuTu }).ToList();

            foreach (var rel in relations)
            {
                var match = request.Items.First(x => x.MaCauHoi == rel.MaCauHoi);
                rel.ThuTu = match.ThuTu;
            }

            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            await _auditLogService.LogAsync(
                "CauHoiDeKiemTra", 
                quizId.ToString(), 
                "REORDER_QUIZ_QUESTIONS", 
                oldOrder, 
                request.Items, 
                userId, 
                null, 
                "Sắp xếp lại câu hỏi trong đề kiểm tra", 
                ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task PublishQuizAsync(int id, int userId, CancellationToken ct)
    {
        var quiz = await _db.DeKiemTras.FirstOrDefaultAsync(x => x.MaDeKiemTra == id, ct);
        if (quiz == null) throw new ApiException(404, "Không tìm thấy Đề kiểm tra");
        if (quiz.TrangThai != "nhap") throw new ApiException(409, "Chỉ có thể xuất bản đề ở trạng thái nháp");

        var config = QuizConfigurationDto.Parse(quiz.CauHinhDeThi);
        config.Validate();
        if (config.DongLuc.HasValue && config.DongLuc.Value <= DateTime.UtcNow)
            throw new ApiException(409, "Không thể xuất bản quiz đã quá thời gian đóng");
        
        var questions = await _db.CauHoiDeKiemTras.Include(x => x.CauHoi).Where(x => x.MaDeKiemTra == id).ToListAsync(ct);
        if (!questions.Any()) throw new ApiException(409, "Đề kiểm tra chưa có câu hỏi nào");

        if (questions.Any(x => x.CauHoi == null || !x.CauHoi.ConHoatDong))
            throw new ApiException(409, "Có câu hỏi đang bị vô hiệu hóa, không thể xuất bản");

        if (questions.Any(x => x.CauHoi!.MaMonHoc != quiz.MaMonHoc))
            throw new ApiException(409, "Có câu hỏi không thuộc môn học của đề");

        var totalScore = questions.Sum(x => x.DiemSo);
        if (totalScore != config.TongDiem)
            throw new ApiException(409, $"Tổng điểm các câu hỏi ({totalScore}) không khớp với cấu hình tổng điểm của đề ({config.TongDiem})");

        if (config.DiemDat > config.TongDiem)
            throw new ApiException(409, "Điểm đạt không được lớn hơn tổng điểm");

        if (config.CachTinhDat == "theo_so_cau_dung" && config.SoCauDungToiThieu > questions.Count)
            throw new ApiException(409, "Số câu đúng tối thiểu không được lớn hơn số câu hỏi");

        var providedOrders = questions.Where(x => x.ThuTu.HasValue).Select(x => x.ThuTu!.Value).OrderBy(x => x).ToList();
        var expectedOrders = Enumerable.Range(1, questions.Count).ToList();
        if (providedOrders.Count != questions.Count || !expectedOrders.SequenceEqual(providedOrders))
            throw new ApiException(409, "Thứ tự câu hỏi không hợp lệ hoặc không liên tục từ 1 đến N");

        var oldStatus = quiz.TrangThai;
        quiz.TrangThai = DetermineInitialPublishedStatus(config, DateTime.UtcNow);
        quiz.NgayCapNhat = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        await _auditLogService.LogAsync(
            "DeKiemTra", 
            id.ToString(), 
            "PUBLISH_QUIZ", 
            oldStatus, 
            quiz.TrangThai, 
            userId, 
            null, 
            "Xuất bản đề kiểm tra", 
            ct);
    }

    public async Task UnpublishQuizAsync(int id, int userId, CancellationToken ct)
    {
        var quiz = await _db.DeKiemTras.FirstOrDefaultAsync(x => x.MaDeKiemTra == id, ct);
        if (quiz == null) throw new ApiException(404, "Không tìm thấy Đề kiểm tra");
        if (quiz.TrangThai == "nhap") throw new ApiException(409, "Đề đã ở trạng thái nháp");

        var inUse = await IsQuizInUseAsync(id, ct);
        if (inUse) throw new ApiException(409, "Đề kiểm tra đã được sử dụng, không thể chuyển về nháp");

        quiz.TrangThai = "nhap";
        quiz.NgayCapNhat = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        await _auditLogService.LogAsync(
            "DeKiemTra", 
            id.ToString(), 
            "UNPUBLISH_QUIZ", 
            "da_xuat_ban", 
            "nhap", 
            userId, 
            null, 
            "Chuyển đề kiểm tra về trạng thái nháp", 
            ct);
    }

    public async Task OpenQuizAsync(int id, int userId, CancellationToken ct)
    {
        var quiz = await _db.DeKiemTras.FirstOrDefaultAsync(x => x.MaDeKiemTra == id, ct);
        if (quiz == null) throw new ApiException(404, "Không tìm thấy Đề kiểm tra");
        if (quiz.TrangThai != "da_len_lich" && quiz.TrangThai != "da_dong")
            throw new ApiException(409, "Chỉ có thể mở quiz ở trạng thái đã lên lịch hoặc đã đóng");

        var oldStatus = quiz.TrangThai;
        quiz.TrangThai = "dang_mo";
        quiz.NgayCapNhat = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        await _auditLogService.LogAsync(
            "DeKiemTra",
            id.ToString(),
            "OPEN_QUIZ",
            oldStatus,
            quiz.TrangThai,
            userId,
            null,
            "Mở quiz thủ công",
            ct);
    }

    public async Task CloseQuizAsync(int id, int userId, CancellationToken ct)
    {
        var quiz = await _db.DeKiemTras.FirstOrDefaultAsync(x => x.MaDeKiemTra == id, ct);
        if (quiz == null) throw new ApiException(404, "Không tìm thấy Đề kiểm tra");
        if (quiz.TrangThai != "dang_mo" && quiz.TrangThai != "da_len_lich")
            throw new ApiException(409, "Chỉ có thể đóng quiz ở trạng thái đang mở hoặc đã lên lịch");

        var oldStatus = quiz.TrangThai;
        quiz.TrangThai = "da_dong";
        quiz.NgayCapNhat = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        await _auditLogService.LogAsync(
            "DeKiemTra",
            id.ToString(),
            "CLOSE_QUIZ",
            oldStatus,
            quiz.TrangThai,
            userId,
            null,
            "Đóng quiz thủ công",
            ct);
    }

    private static string DetermineInitialPublishedStatus(QuizConfigurationDto config, DateTime utcNow)
    {
        if (config.MoLuc.HasValue && config.MoLuc.Value > utcNow)
        {
            return "da_len_lich";
        }

        return "dang_mo";
    }
}
