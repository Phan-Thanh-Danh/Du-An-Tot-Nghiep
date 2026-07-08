using Backend.Data;
using Backend.DTOs.Applications;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Applications;

public interface IApplicationWorkflowService
{
    Task<IReadOnlyList<WorkflowConfigDto>> GetWorkflowsAsync(CancellationToken cancellationToken);
    Task<WorkflowConfigDto> UpdateWorkflowAsync(int id, UpdateWorkflowRequest request, CancellationToken cancellationToken);
}

public class ApplicationWorkflowService : IApplicationWorkflowService
{
    private readonly ApplicationDbContext _db;

    public ApplicationWorkflowService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<WorkflowConfigDto>> GetWorkflowsAsync(CancellationToken cancellationToken)
    {
        var hasAny = await _db.QuyTrinhDonTus.AnyAsync(cancellationToken);
        if (!hasAny)
        {
            await SeedDefaultWorkflowsAsync(cancellationToken);
        }

        var workflows = await _db.QuyTrinhDonTus
            .Include(q => q.BuocQuyTrinhs)
            .OrderBy(q => q.MaQuyTrinh)
            .ToListAsync(cancellationToken);

        return workflows.Select(MapToDto).ToList();
    }

    public async Task<WorkflowConfigDto> UpdateWorkflowAsync(int id, UpdateWorkflowRequest request, CancellationToken cancellationToken)
    {
        var workflow = await _db.QuyTrinhDonTus
            .Include(q => q.BuocQuyTrinhs)
            .FirstOrDefaultAsync(q => q.MaQuyTrinh == id, cancellationToken)
            ?? throw new Exception("Không tìm thấy quy trình.");

        workflow.IsActive = request.Active;
        await _db.SaveChangesAsync(cancellationToken);

        return MapToDto(workflow);
    }

    private static WorkflowConfigDto MapToDto(QuyTrinhDonTu entity)
    {
        return new WorkflowConfigDto
        {
            Id = entity.MaQuyTrinh,
            LoaiDon = entity.LoaiDon,
            Name = entity.TenQuyTrinh,
            Active = entity.IsActive,
            Sla = entity.SlaKhoangThoiGian,
            Steps = entity.BuocQuyTrinhs.Count,
            WfSteps = entity.BuocQuyTrinhs.OrderBy(b => b.ThuTu).Select(b => new WorkflowStepDto
            {
                Id = b.MaBuoc,
                Name = b.TenBuoc,
                Role = b.VaiTroXuLy,
                Type = b.KieuBuoc,
                Sla = b.SlaKhoangThoiGian,
                Order = b.ThuTu
            }).ToList()
        };
    }

    private async Task SeedDefaultWorkflowsAsync(CancellationToken cancellationToken)
    {
        var w1 = new QuyTrinhDonTu
        {
            LoaiDon = "NghiHoc",
            TenQuyTrinh = "Nghỉ học / Bảo lưu",
            IsActive = true,
            SlaKhoangThoiGian = "3 ngày",
            BuocQuyTrinhs = new List<BuocQuyTrinh>
            {
                new BuocQuyTrinh { ThuTu = 1, TenBuoc = "Tiếp nhận đơn", VaiTroXuLy = "Hệ thống", KieuBuoc = "auto", SlaKhoangThoiGian = "Tức thì" },
                new BuocQuyTrinh { ThuTu = 2, TenBuoc = "Phê duyệt GVCN", VaiTroXuLy = "Giáo viên", KieuBuoc = "manual", SlaKhoangThoiGian = "1 ngày" },
                new BuocQuyTrinh { ThuTu = 3, TenBuoc = "Phê duyệt Giáo vụ", VaiTroXuLy = "Giáo vụ khoa", KieuBuoc = "manual", SlaKhoangThoiGian = "2 ngày" }
            }
        };

        var w2 = new QuyTrinhDonTu
        {
            LoaiDon = "ChuyenLop",
            TenQuyTrinh = "Chuyển lớp học phần",
            IsActive = true,
            SlaKhoangThoiGian = "2 ngày",
            BuocQuyTrinhs = new List<BuocQuyTrinh>
            {
                new BuocQuyTrinh { ThuTu = 1, TenBuoc = "Tiếp nhận đơn", VaiTroXuLy = "Hệ thống", KieuBuoc = "auto", SlaKhoangThoiGian = "Tức thì" },
                new BuocQuyTrinh { ThuTu = 2, TenBuoc = "Kiểm tra điều kiện", VaiTroXuLy = "AI Assistant", KieuBuoc = "auto", SlaKhoangThoiGian = "1 phút" },
                new BuocQuyTrinh { ThuTu = 3, TenBuoc = "Phê duyệt cuối cùng", VaiTroXuLy = "Giáo vụ khoa", KieuBuoc = "manual", SlaKhoangThoiGian = "2 ngày" },
                new BuocQuyTrinh { ThuTu = 4, TenBuoc = "Cập nhật hệ thống", VaiTroXuLy = "Hệ thống", KieuBuoc = "auto", SlaKhoangThoiGian = "Tức thì" }
            }
        };

        var w3 = new QuyTrinhDonTu
        {
            LoaiDon = "GiayXacNhan",
            TenQuyTrinh = "Cấp giấy xác nhận",
            IsActive = true,
            SlaKhoangThoiGian = "Tự động",
            BuocQuyTrinhs = new List<BuocQuyTrinh>
            {
                new BuocQuyTrinh { ThuTu = 1, TenBuoc = "Tiếp nhận đơn", VaiTroXuLy = "Hệ thống", KieuBuoc = "auto", SlaKhoangThoiGian = "Tức thì" },
                new BuocQuyTrinh { ThuTu = 2, TenBuoc = "Cấp giấy", VaiTroXuLy = "Hệ thống", KieuBuoc = "auto", SlaKhoangThoiGian = "Tức thì" }
            }
        };

        var w4 = new QuyTrinhDonTu
        {
            LoaiDon = "HoanThi",
            TenQuyTrinh = "Thi lại / Hoãn thi",
            IsActive = false,
            SlaKhoangThoiGian = "5 ngày",
            BuocQuyTrinhs = new List<BuocQuyTrinh>
            {
                new BuocQuyTrinh { ThuTu = 1, TenBuoc = "Tiếp nhận đơn", VaiTroXuLy = "Hệ thống", KieuBuoc = "auto", SlaKhoangThoiGian = "Tức thì" },
                new BuocQuyTrinh { ThuTu = 2, TenBuoc = "Kiểm tra minh chứng", VaiTroXuLy = "Giáo vụ khoa", KieuBuoc = "manual", SlaKhoangThoiGian = "2 ngày" },
                new BuocQuyTrinh { ThuTu = 3, TenBuoc = "Xếp lịch thi mới", VaiTroXuLy = "Phòng Khảo thí", KieuBuoc = "manual", SlaKhoangThoiGian = "3 ngày" },
                new BuocQuyTrinh { ThuTu = 4, TenBuoc = "Thông báo", VaiTroXuLy = "Hệ thống", KieuBuoc = "auto", SlaKhoangThoiGian = "Tức thì" }
            }
        };

        _db.QuyTrinhDonTus.AddRange(w1, w2, w3, w4);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
