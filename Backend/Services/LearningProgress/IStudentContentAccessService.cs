using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.LearningProgress;

public interface IStudentContentAccessService
{
    Task<bool> CanAccessContentAsync(int studentId, int contentId);
}

public class StudentContentAccessService : IStudentContentAccessService
{
    private readonly ApplicationDbContext _context;

    public StudentContentAccessService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CanAccessContentAsync(int studentId, int contentId)
    {
        // Yêu cầu: Không kiểm tra LopHocPhan/DangKyHocPhan
        // Học sinh được phép xem nếu BaiHocNoiDung có trạng thái "da_xuat_ban"
        // và bài học, chương không bị ẩn.
        
        var content = await _context.BaiHocNoiDungs
            .Include(c => c.BaiHoc)
                .ThenInclude(b => b!.Chuong)
            .FirstOrDefaultAsync(c => c.MaNoiDung == contentId);

        if (content == null) return false;

        if (content.TrangThai != "da_xuat_ban") return false;
        
        if (content.BaiHoc == null || content.BaiHoc.TrangThai != "da_xuat_ban" || content.BaiHoc.DaAn) return false;

        if (content.BaiHoc.Chuong == null || content.BaiHoc.Chuong.DaAn) return false;

        return true;
    }
}
