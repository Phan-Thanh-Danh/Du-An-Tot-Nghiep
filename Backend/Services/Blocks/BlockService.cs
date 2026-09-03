using Backend.Constants;
using Backend.DTOs.Auth;
using Backend.Exceptions;
using Backend.Middlewares;
using Backend.Data;
using Backend.DTOs.Blocks;
using Backend.Helpers;
using Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Blocks;

public class BlockService : IBlockService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BlockService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IReadOnlyList<BlockDto>> GetByTermIdAsync(int termId, CancellationToken cancellationToken = default)
    {
        var currentUser = _httpContextAccessor.HttpContext?.Items["CurrentUser"] as CurrentUserContext;
        var term = await _context.HocKys
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.MaHocKy == termId, cancellationToken);

        if (term != null && currentUser != null && currentUser.Role != AuthRoles.SuperAdmin && term.MaDonVi != currentUser.CampusId)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xem cấu hình block của cơ sở này.", "FORBIDDEN_CAMPUS");
        }

        var blocks = await _context.Blocks
            .AsNoTracking()
            .Where(b => b.MaHocKy == termId)
            .OrderBy(b => b.ThuTuBlock)
            .Select(b => new BlockDto
            {
                MaBlock = b.MaBlock,
                ThuTuBlock = b.ThuTuBlock,
                NgayBatDau = b.NgayBatDau,
                NgayKetThuc = b.NgayKetThuc,
                MaHocKy = b.MaHocKy
            })
            .ToListAsync(cancellationToken);

        return blocks;
    }

    public async Task<BlockDto> UpdateAsync(int blockId, UpdateBlockRequest request, CancellationToken cancellationToken = default)
    {
        var block = await _context.Blocks
            .Include(b => b.HocKy)
            .FirstOrDefaultAsync(b => b.MaBlock == blockId, cancellationToken);

        if (block == null)
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy Block.");

        var currentUser = _httpContextAccessor.HttpContext?.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser != null && currentUser.Role != AuthRoles.SuperAdmin && block.HocKy != null && block.HocKy.MaDonVi != currentUser.CampusId)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền chỉnh sửa block của cơ sở này.", "FORBIDDEN_CAMPUS");
        }

        if (request.NgayBatDau >= request.NgayKetThuc)
            throw new ApiException(StatusCodes.Status400BadRequest, "Ngày bắt đầu phải trước ngày kết thúc.");

        if (request.NgayBatDau < block.HocKy.NgayBatDau || request.NgayKetThuc > block.HocKy.NgayKetThuc)
            throw new ApiException(StatusCodes.Status400BadRequest, $"Thời gian Block phải nằm trong khoảng thời gian của học kỳ ({block.HocKy.NgayBatDau:dd/MM/yyyy} - {block.HocKy.NgayKetThuc:dd/MM/yyyy}).");

        var allBlocksInTerm = await _context.Blocks
            .Where(b => b.MaHocKy == block.MaHocKy)
            .OrderBy(b => b.ThuTuBlock)
            .ToListAsync(cancellationToken);

        // Lấy block trước và block sau (nếu có)
        var previousBlock = allBlocksInTerm.LastOrDefault(b => b.ThuTuBlock < block.ThuTuBlock);
        var nextBlock = allBlocksInTerm.FirstOrDefault(b => b.ThuTuBlock > block.ThuTuBlock);

        if (previousBlock != null && request.NgayBatDau <= previousBlock.NgayKetThuc)
            throw new ApiException(StatusCodes.Status400BadRequest, $"Ngày bắt đầu của Block {block.ThuTuBlock} phải sau ngày kết thúc của Block {previousBlock.ThuTuBlock} ({previousBlock.NgayKetThuc:dd/MM/yyyy}).");

        if (nextBlock != null && request.NgayKetThuc >= nextBlock.NgayBatDau)
            throw new ApiException(StatusCodes.Status400BadRequest, $"Ngày kết thúc của Block {block.ThuTuBlock} phải trước ngày bắt đầu của Block {nextBlock.ThuTuBlock} ({nextBlock.NgayBatDau:dd/MM/yyyy}).");

        block.NgayBatDau = request.NgayBatDau;
        block.NgayKetThuc = request.NgayKetThuc;

        await _context.SaveChangesAsync(cancellationToken);

        return new BlockDto
        {
            MaBlock = block.MaBlock,
            ThuTuBlock = block.ThuTuBlock,
            NgayBatDau = block.NgayBatDau,
            NgayKetThuc = block.NgayKetThuc,
            MaHocKy = block.MaHocKy
        };
    }
}
