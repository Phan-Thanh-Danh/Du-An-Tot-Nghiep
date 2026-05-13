using System.Text.Json;
using Backend.Data;
using Backend.Models;

namespace Backend.Services.Audit;

public class AuditLogService : IAuditLogService
{
    private readonly ApplicationDbContext _context;

    public AuditLogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        int campusId,
        string entityName,
        int entityId,
        string action,
        int actorUserId,
        object? oldValue,
        object? newValue,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new NhatKyKiemToan
        {
            MaDonVi = campusId,
            LoaiDoiTuong = entityName,
            MaDoiTuong = entityId,
            HanhDong = action,
            GiaTriCu = oldValue is null ? null : JsonSerializer.Serialize(oldValue),
            GiaTriMoi = newValue is null ? null : JsonSerializer.Serialize(newValue),
            NguoiThayDoi = actorUserId,
            ThoiDiemThayDoi = DateTime.UtcNow
        };

        await _context.NhatKyKiemToans.AddAsync(auditLog, cancellationToken);
    }
}
