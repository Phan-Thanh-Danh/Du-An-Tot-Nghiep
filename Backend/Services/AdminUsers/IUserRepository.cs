using Backend.Models;

namespace Backend.Services.AdminUsers;

public interface IUserRepository
{
    IQueryable<NguoiDung> QueryUsers();
    IQueryable<VaiTro> QueryRoles();
    IQueryable<DonVi> QueryOrganizations();
    IQueryable<LopHanhChinh> QueryClasses();

    Task<NguoiDung?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<VaiTro?> GetRoleByIdAsync(int roleId, CancellationToken cancellationToken = default);
    Task<DonVi?> GetOrganizationByIdAsync(int organizationId, CancellationToken cancellationToken = default);
    Task<LopHanhChinh?> GetClassByIdAsync(int classId, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string normalizedEmail, int? excludedUserId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PhanQuyenNguoiDung>> GetRoleAssignmentsAsync(int userId, CancellationToken cancellationToken = default);
    Task AddAsync(NguoiDung user, CancellationToken cancellationToken = default);
    Task AddRoleAssignmentAsync(PhanQuyenNguoiDung roleAssignment, CancellationToken cancellationToken = default);
    void RemoveRoleAssignments(IEnumerable<PhanQuyenNguoiDung> roleAssignments);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
