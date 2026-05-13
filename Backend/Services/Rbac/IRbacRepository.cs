using Backend.Models;

namespace Backend.Services.Rbac;

public interface IRbacRepository
{
    IQueryable<VaiTro> QueryRoles();
    IQueryable<NguoiDung> QueryUsers();
    IQueryable<DonVi> QueryOrganizations();
    IQueryable<PhanQuyenNguoiDung> QueryRoleAssignments();

    Task<VaiTro?> GetRoleByIdAsync(int roleId, CancellationToken cancellationToken = default);
    Task<VaiTro?> GetRoleByCodeAsync(string roleCode, CancellationToken cancellationToken = default);
    Task<NguoiDung?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PhanQuyenNguoiDung>> GetUserRoleAssignmentsAsync(int userId, CancellationToken cancellationToken = default);
    Task<int> GetNextRoleIdAsync(CancellationToken cancellationToken = default);
    Task AddRoleAsync(VaiTro role, CancellationToken cancellationToken = default);
    Task AddRoleAssignmentAsync(PhanQuyenNguoiDung roleAssignment, CancellationToken cancellationToken = default);
    void RemoveRole(VaiTro role);
    void RemoveRoleAssignments(IEnumerable<PhanQuyenNguoiDung> roleAssignments);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
