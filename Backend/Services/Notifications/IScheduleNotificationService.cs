namespace Backend.Services.Notifications;

public interface IScheduleNotificationService
{
    /// <summary>
    /// Gửi thông báo lịch học mới cho sinh viên và giảng viên liên quan.
    /// Gọi SAU KHI transaction publish đã commit thành công.
    /// Không throw exception — chỉ log warning nếu fail để không làm hỏng kết quả Publish.
    /// </summary>
    Task NotifySchedulePublishedAsync(
        int maHocKy,
        int maDonVi,
        List<int> maGiaoVienList,
        List<int> maLopList,
        CancellationToken cancellationToken = default);
}
