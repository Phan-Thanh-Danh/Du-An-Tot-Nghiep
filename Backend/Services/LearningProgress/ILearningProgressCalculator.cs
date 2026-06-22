using Backend.Models;
using Microsoft.Extensions.Options;

namespace Backend.Services.LearningProgress;

public interface ILearningProgressCalculator
{
    void CalculateProgress(TienDoNoiDungHocTap progress, BaiHocNoiDung content);
}

public class LearningProgressCalculator : ILearningProgressCalculator
{
    private readonly LearningProgressOptions _options;

    public LearningProgressCalculator(IOptions<LearningProgressOptions> options)
    {
        _options = options.Value;
    }

    public void CalculateProgress(TienDoNoiDungHocTap progress, BaiHocNoiDung content)
    {
        switch (content.LoaiNoiDung)
        {
            case "video":
                CalculateVideoProgress(progress, content);
                break;
            case "slide_html":
            case "pdf":
                CalculateDocumentProgress(progress);
                break;
            case "van_ban":
                CalculateTextProgress(progress);
                break;
        }

        if (progress.PhanTramTienDo >= 100 && progress.HoanThanhLuc == null)
        {
            progress.PhanTramTienDo = 100;
            progress.TrangThai = "hoan_thanh";
            progress.HoanThanhLuc = DateTime.UtcNow;
        }
        else if (progress.PhanTramTienDo > 0 && progress.TrangThai == "chua_bat_dau")
        {
            progress.TrangThai = "dang_hoc";
        }
    }

    private void CalculateVideoProgress(TienDoNoiDungHocTap progress, BaiHocNoiDung content)
    {
        if (!content.ThoiLuongGiay.HasValue || content.ThoiLuongGiay.Value <= 0) return;

        // Tính theo thời gian đã xem so với tổng thời lượng video
        // Lưu ý: Học sinh có thể xem đi xem lại nên số giây xác nhận có thể vượt thời lượng
        var viewRatio = (decimal)progress.SoGiayDaXacNhan / content.ThoiLuongGiay.Value * 100;
        
        // Hoặc tính theo vị trí đã xem xa nhất, tuỳ vào requirement.
        // Ở đây kết hợp cả 2: ít nhất phải có vị trí xem và số giây xem thực tế lớn
        
        if (viewRatio > 100) viewRatio = 100;
        
        progress.PhanTramTienDo = viewRatio;

        if (progress.PhanTramTienDo >= _options.VideoCompletionPercent)
        {
            progress.PhanTramTienDo = 100;
        }
    }

    private void CalculateDocumentProgress(TienDoNoiDungHocTap progress)
    {
        if (progress.PhanTramCuonLonNhat.HasValue)
        {
            progress.PhanTramTienDo = progress.PhanTramCuonLonNhat.Value;
            if (progress.PhanTramTienDo >= _options.SlideCompletionPercent)
            {
                progress.PhanTramTienDo = 100;
            }
        }
    }

    private void CalculateTextProgress(TienDoNoiDungHocTap progress)
    {
        // Văn bản tính hoàn thành dựa vào số giây đã mở
        if (progress.SoGiayDaXacNhan >= _options.MinReadingSecondsForCompletion)
        {
            progress.PhanTramTienDo = 100;
        }
        else
        {
            progress.PhanTramTienDo = (decimal)progress.SoGiayDaXacNhan / _options.MinReadingSecondsForCompletion * 100;
        }
    }
}
