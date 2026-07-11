using Backend.Exceptions;
using Backend.Middlewares;
using Backend.Data;
using Backend.DTOs.QuyDoiTinChis;
using Backend.Helpers;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.QuyDoiTinChis;

public class QuyDoiTinChiService : IQuyDoiTinChiService
{
    private readonly ApplicationDbContext _context;

    public QuyDoiTinChiService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<QuyDoiTinChiDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.QuyDoiTinChis
            .AsNoTracking()
            .OrderBy(q => q.SoTinChi)
            .Select(q => new QuyDoiTinChiDto
            {
                MaQuyDoi = q.MaQuyDoi,
                SoTinChi = q.SoTinChi,
                SoBlockHoc = q.SoBlockHoc,
                SoBuoiMoiTuan = q.SoBuoiMoiTuan,
                SoCaMoiBuoi = q.SoCaMoiBuoi
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<QuyDoiTinChiDto> CreateAsync(CreateQuyDoiTinChiRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await _context.QuyDoiTinChis.AnyAsync(q => q.SoTinChi == request.SoTinChi, cancellationToken);
        if (exists)
            throw new ApiException(StatusCodes.Status400BadRequest, $"Đã tồn tại quy đổi cho mức {request.SoTinChi} tín chỉ.");

        var entity = new QuyDoiTinChi
        {
            SoTinChi = request.SoTinChi,
            SoBlockHoc = request.SoBlockHoc,
            SoBuoiMoiTuan = request.SoBuoiMoiTuan,
            SoCaMoiBuoi = request.SoCaMoiBuoi
        };

        _context.QuyDoiTinChis.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new QuyDoiTinChiDto
        {
            MaQuyDoi = entity.MaQuyDoi,
            SoTinChi = entity.SoTinChi,
            SoBlockHoc = entity.SoBlockHoc,
            SoBuoiMoiTuan = entity.SoBuoiMoiTuan,
            SoCaMoiBuoi = entity.SoCaMoiBuoi
        };
    }

    public async Task<QuyDoiTinChiDto> UpdateAsync(int id, UpdateQuyDoiTinChiRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _context.QuyDoiTinChis.FirstOrDefaultAsync(q => q.MaQuyDoi == id, cancellationToken);
        if (entity == null)
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy cấu hình quy đổi tín chỉ.");

        if (entity.SoTinChi != request.SoTinChi)
        {
            var exists = await _context.QuyDoiTinChis.AnyAsync(q => q.SoTinChi == request.SoTinChi, cancellationToken);
            if (exists)
                throw new ApiException(StatusCodes.Status400BadRequest, $"Đã tồn tại quy đổi cho mức {request.SoTinChi} tín chỉ.");
        }

        entity.SoTinChi = request.SoTinChi;
        entity.SoBlockHoc = request.SoBlockHoc;
        entity.SoBuoiMoiTuan = request.SoBuoiMoiTuan;
        entity.SoCaMoiBuoi = request.SoCaMoiBuoi;

        await _context.SaveChangesAsync(cancellationToken);

        return new QuyDoiTinChiDto
        {
            MaQuyDoi = entity.MaQuyDoi,
            SoTinChi = entity.SoTinChi,
            SoBlockHoc = entity.SoBlockHoc,
            SoBuoiMoiTuan = entity.SoBuoiMoiTuan,
            SoCaMoiBuoi = entity.SoCaMoiBuoi
        };
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.QuyDoiTinChis.FirstOrDefaultAsync(q => q.MaQuyDoi == id, cancellationToken);
        if (entity == null)
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy cấu hình quy đổi tín chỉ.");

        var isUsed = await _context.DanhMucMonHocs.AnyAsync(m => m.SoTinChi == entity.SoTinChi, cancellationToken);
        if (isUsed)
            throw new ApiException(StatusCodes.Status400BadRequest, "Không thể xóa do đang có môn học sử dụng mức tín chỉ này.");

        _context.QuyDoiTinChis.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
