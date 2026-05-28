using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.TrainingProgramTerms;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.AcademicTerms;

public class TrainingProgramTermService : ITrainingProgramTermService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TrainingProgramTermService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<TrainingProgramTermDto>> GetByProgramAsync(
        int programId,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();

        var program = await _context.ChuongTrinhDaoTaos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaChuongTrinh == programId, cancellationToken);

        if (program is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy chương trình đào tạo.");
        }

        return await _context.ChuongTrinhHocKys
            .AsNoTracking()
            .Where(x => x.MaChuongTrinh == programId)
            .Join(_context.HocKys.AsNoTracking(),
                ct => ct.MaHocKy,
                hk => hk.MaHocKy,
                (ct, hk) => new { ct, hk })
            .OrderBy(x => x.ct.ThuTuHocKy)
            .Select(x => new TrainingProgramTermDto
            {
                MaChuongTrinhHocKy = x.ct.MaChuongTrinhHocKy,
                MaChuongTrinh = x.ct.MaChuongTrinh,
                TenChuongTrinh = program.TenChuongTrinh,
                MaHocKy = x.hk.MaHocKy,
                MaCodeHocKy = x.hk.MaCodeHocKy,
                TenHocKy = x.hk.TenHocKy,
                NamHoc = x.hk.NamHoc,
                ThuTuTrongNam = x.hk.ThuTuTrongNam,
                ThuTuHocKy = x.ct.ThuTuHocKy,
                NgayBatDau = x.hk.NgayBatDau,
                NgayKetThuc = x.hk.NgayKetThuc
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<TrainingProgramTermDto> CreateAsync(
        CreateTrainingProgramTermRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();

        var program = await _context.ChuongTrinhDaoTaos
            .FirstOrDefaultAsync(x => x.MaChuongTrinh == request.MaChuongTrinh, cancellationToken);

        if (program is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy chương trình đào tạo.");
        }

        var term = await _context.HocKys
            .FirstOrDefaultAsync(x => x.MaHocKy == request.MaHocKy, cancellationToken);

        if (term is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy học kỳ.");
        }

        if (request.ThuTuHocKy < 1 || request.ThuTuHocKy > program.SoHocKy)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                $"Thứ tự học kỳ phải từ 1 đến {program.SoHocKy}.");
        }

        var duplicatePosition = await _context.ChuongTrinhHocKys
            .AnyAsync(x =>
                x.MaChuongTrinh == request.MaChuongTrinh &&
                x.ThuTuHocKy == request.ThuTuHocKy,
                cancellationToken);

        if (duplicatePosition)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                $"Học kỳ thứ {request.ThuTuHocKy} đã được gán trong chương trình này.");
        }

        var duplicateTerm = await _context.ChuongTrinhHocKys
            .AnyAsync(x =>
                x.MaChuongTrinh == request.MaChuongTrinh &&
                x.MaHocKy == request.MaHocKy,
                cancellationToken);

        if (duplicateTerm)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "Học kỳ này đã được gán vào chương trình.");
        }

        var mapping = new ChuongTrinhHocKy
        {
            MaChuongTrinh = request.MaChuongTrinh,
            MaHocKy = request.MaHocKy,
            ThuTuHocKy = request.ThuTuHocKy
        };

        _context.ChuongTrinhHocKys.Add(mapping);
        await _context.SaveChangesAsync(cancellationToken);

        return new TrainingProgramTermDto
        {
            MaChuongTrinhHocKy = mapping.MaChuongTrinhHocKy,
            MaChuongTrinh = mapping.MaChuongTrinh,
            TenChuongTrinh = program.TenChuongTrinh,
            MaHocKy = term.MaHocKy,
            MaCodeHocKy = term.MaCodeHocKy,
            TenHocKy = term.TenHocKy,
            NamHoc = term.NamHoc,
            ThuTuTrongNam = term.ThuTuTrongNam,
            ThuTuHocKy = mapping.ThuTuHocKy,
            NgayBatDau = term.NgayBatDau,
            NgayKetThuc = term.NgayKetThuc
        };
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();

        var mapping = await _context.ChuongTrinhHocKys
            .FirstOrDefaultAsync(x => x.MaChuongTrinhHocKy == id, cancellationToken);

        if (mapping is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy mapping.");
        }

        _context.ChuongTrinhHocKys.Remove(mapping);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private CurrentUserContext GetCurrentUser()
    {
        var currentUser = _httpContextAccessor.HttpContext?.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser is null)
        {
            throw new ApiException(StatusCodes.Status401Unauthorized, "Token xác thực không hợp lệ.");
        }

        return currentUser;
    }
}
