using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Services.Notifications;

public class ScheduleNotificationService : IScheduleNotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ScheduleNotificationService> _logger;

    public ScheduleNotificationService(
        ApplicationDbContext context,
        ILogger<ScheduleNotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task NotifySchedulePublishedAsync(
        int maHocKy,
        int maDonVi,
        List<int> maGiaoVienList,
        List<int> maLopList,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cleanTeacherIds = maGiaoVienList?
                .Where(id => id > 0)
                .Distinct()
                .ToList() ?? new List<int>();

            var cleanClassIds = maLopList?
                .Where(id => id > 0)
                .Distinct()
                .ToList() ?? new List<int>();

            var now = DateTime.UtcNow;

            // 1. Lấy tên học kỳ theo đúng cơ sở và mã học kỳ
            var tenHocKy = await _context.HocKys
                .AsNoTracking()
                .Where(hk => hk.MaHocKy == maHocKy && hk.MaDonVi == maDonVi)
                .Select(hk => hk.TenHocKy)
                .FirstOrDefaultAsync(cancellationToken)
                ?? $"Học kỳ {maHocKy}";

            // 2. Lấy danh sách sinh viên thuộc các lớp liên quan và đúng cơ sở
            var maSinhVienList = new List<int>();
            if (cleanClassIds.Count > 0)
            {
                maSinhVienList = await _context.NguoiDungs
                    .AsNoTracking()
                    .Where(nd => nd.MaDonVi == maDonVi
                              && nd.MaLop != null
                              && cleanClassIds.Contains(nd.MaLop.Value)
                              && nd.VaiTroChinh == "hoc_sinh")
                    .Select(nd => nd.MaNguoiDung)
                    .ToListAsync(cancellationToken);
            }

            // 3. Gộp người nhận: sinh viên + giảng viên, loại trùng hoàn toàn
            var nguoiNhanIds = maSinhVienList
                .Concat(cleanTeacherIds)
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (nguoiNhanIds.Count == 0)
            {
                _logger.LogInformation(
                    "No recipients found for schedule notification HocKy {MaHocKy}, Campus {MaDonVi}",
                    maHocKy, maDonVi);
                return;
            }

            var firstRecipient = nguoiNhanIds[0];

            // 4. Tạo ThongBao & ThongBaoNguoiNhan trong transaction riêng biệt để đảm bảo tính nguyên tử (Atomicity)
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var thongBao = new ThongBao
                    {
                        MaNhomThongBao = Guid.NewGuid(),
                        MaNguoiNhan = firstRecipient,
                        MaDonVi = maDonVi,
                        LoaiSuKien = "schedule_published",
                        LoaiThongBao = "hoc_vu",
                        TieuDe = $"Thời khóa biểu {tenHocKy} đã được cập nhật",
                        TomTat = $"Thời khóa biểu học kỳ {tenHocKy} vừa được xuất bản.",
                        TomTatNoiDung = $"Thời khóa biểu học kỳ {tenHocKy} vừa được xuất bản.",
                        NoiDung = $"Thời khóa biểu học kỳ {tenHocKy} vừa được xuất bản. Vui lòng kiểm tra lịch học mới của bạn.",
                        NoiDungText = $"Thời khóa biểu học kỳ {tenHocKy} vừa được xuất bản. Vui lòng kiểm tra lịch học mới của bạn.",
                        MucDo = "info",
                        PhamViGui = "nguoi_dung",
                        TrangThai = "da_gui",
                        DaDoc = false,
                        NgayTao = now,
                        GuiLuc = now
                    };

                    _context.ThongBaos.Add(thongBao);
                    await _context.SaveChangesAsync(cancellationToken);

                    var nguoiNhans = nguoiNhanIds.Select(id => new ThongBaoNguoiNhan
                    {
                        MaThongBao = thongBao.MaThongBao,
                        MaNguoiNhan = id,
                        MaDonVi = maDonVi,
                        DaDoc = false,
                        DaAn = false,
                        NhanLuc = now,
                        NgayTao = now
                    }).ToList();

                    _context.ThongBaoNguoiNhans.AddRange(nguoiNhans);
                    await _context.SaveChangesAsync(cancellationToken);

                    await tx.CommitAsync(cancellationToken);
                }
                catch
                {
                    await tx.RollbackAsync(cancellationToken);
                    throw;
                }
            });

            _logger.LogInformation(
                "Schedule notification sent for HocKy {MaHocKy} (Campus {MaDonVi}): {GvCount} teachers + {SvCount} students = {Total} recipients",
                maHocKy,
                maDonVi,
                cleanTeacherIds.Count,
                maSinhVienList.Count,
                nguoiNhanIds.Count);
        }
        catch (Exception ex)
        {
            // TUYỆT ĐỐI không rethrow — lỗi gửi thông báo không được ảnh hưởng luồng Publish lịch đã thành công
            _logger.LogWarning(ex,
                "Failed to send schedule notification for HocKy {MaHocKy}, Campus {MaDonVi}. Schedule publish was already committed successfully.",
                maHocKy, maDonVi);
        }
    }
}
