using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Rbac;

public class RbacRepository : IRbacRepository
{
    private readonly ApplicationDbContext _context;

    public RbacRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public IQueryable<VaiTro> QueryRoles()
    {
        return _context.VaiTros;
    }

    public IQueryable<NguoiDung> QueryUsers()
    {
        return _context.NguoiDungs;
    }

    public IQueryable<DonVi> QueryOrganizations()
    {
        return _context.DonVis;
    }

    public IQueryable<PhanQuyenNguoiDung> QueryRoleAssignments()
    {
        return _context.PhanQuyenNguoiDungs;
    }

    public async Task<VaiTro?> GetRoleByIdAsync(int roleId, CancellationToken cancellationToken = default)
    {
        return await _context.VaiTros.FirstOrDefaultAsync(x => x.MaVaiTro == roleId, cancellationToken);
    }

    public async Task<VaiTro?> GetRoleByCodeAsync(string roleCode, CancellationToken cancellationToken = default)
    {
        return await _context.VaiTros.FirstOrDefaultAsync(x => x.MaCodeVaiTro == roleCode, cancellationToken);
    }

    public async Task<NguoiDung?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.NguoiDungs.FirstOrDefaultAsync(x => x.MaNguoiDung == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<PhanQuyenNguoiDung>> GetUserRoleAssignmentsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.PhanQuyenNguoiDungs
            .Where(x => x.MaNguoiDung == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetNextRoleIdAsync(CancellationToken cancellationToken = default)
    {
        var maxRoleId = await _context.VaiTros
            .Select(x => (int?)x.MaVaiTro)
            .MaxAsync(cancellationToken);

        return (maxRoleId ?? 0) + 1;
    }

    public async Task AddRoleAsync(VaiTro role, CancellationToken cancellationToken = default)
    {
        await _context.VaiTros.AddAsync(role, cancellationToken);
    }

    public async Task AddRoleAssignmentAsync(
        PhanQuyenNguoiDung roleAssignment,
        CancellationToken cancellationToken = default)
    {
        await _context.PhanQuyenNguoiDungs.AddAsync(roleAssignment, cancellationToken);
    }

    public void RemoveRole(VaiTro role)
    {
        _context.VaiTros.Remove(role);
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
