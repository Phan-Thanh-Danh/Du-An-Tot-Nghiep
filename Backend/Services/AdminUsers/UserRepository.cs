using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.AdminUsers;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public IQueryable<NguoiDung> QueryUsers()
    {
        return _context.NguoiDungs;
    }

    public IQueryable<VaiTro> QueryRoles()
    {
        return _context.VaiTros;
    }

    public IQueryable<DonVi> QueryOrganizations()
    {
        return _context.DonVis;
    }

    public IQueryable<LopHanhChinh> QueryClasses()
    {
        return _context.LopHanhChinhs;
    }

    public async Task<NguoiDung?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.NguoiDungs.FirstOrDefaultAsync(x => x.MaNguoiDung == userId, cancellationToken);
    }

    public async Task<VaiTro?> GetRoleByIdAsync(int roleId, CancellationToken cancellationToken = default)
    {
        return await _context.VaiTros.FirstOrDefaultAsync(x => x.MaVaiTro == roleId, cancellationToken);
    }

    public async Task<DonVi?> GetOrganizationByIdAsync(int organizationId, CancellationToken cancellationToken = default)
    {
        return await _context.DonVis.FirstOrDefaultAsync(x => x.MaDonVi == organizationId, cancellationToken);
    }

    public async Task<LopHanhChinh?> GetClassByIdAsync(int classId, CancellationToken cancellationToken = default)
    {
        return await _context.LopHanhChinhs.FirstOrDefaultAsync(x => x.MaLop == classId, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(
        string normalizedEmail,
        int? excludedUserId = null,
        CancellationToken cancellationToken = default)
    {
        return await _context.NguoiDungs.AnyAsync(
            x => x.Email.ToLower() == normalizedEmail &&
                 (!excludedUserId.HasValue || x.MaNguoiDung != excludedUserId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyList<PhanQuyenNguoiDung>> GetRoleAssignmentsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.PhanQuyenNguoiDungs
            .Where(x => x.MaNguoiDung == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(NguoiDung user, CancellationToken cancellationToken = default)
    {
        await _context.NguoiDungs.AddAsync(user, cancellationToken);
    }

    public async Task AddRoleAssignmentAsync(
        PhanQuyenNguoiDung roleAssignment,
        CancellationToken cancellationToken = default)
    {
        await _context.PhanQuyenNguoiDungs.AddAsync(roleAssignment, cancellationToken);
    }

    public void RemoveRoleAssignments(IEnumerable<PhanQuyenNguoiDung> roleAssignments)
    {
        _context.PhanQuyenNguoiDungs.RemoveRange(roleAssignments);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
