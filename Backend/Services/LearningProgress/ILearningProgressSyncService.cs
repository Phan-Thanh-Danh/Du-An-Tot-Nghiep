using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.LearningProgress;

public interface ILearningProgressSyncService
{
    Task SyncLessonProgressAsync(int studentId, int lessonId);
    Task SyncQuizProgressAsync(int studentId, int contentId, PhienThiHocSinh phienThi);
}

public class LearningProgressSyncService : ILearningProgressSyncService
{
    private readonly ApplicationDbContext _context;

    public LearningProgressSyncService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SyncLessonProgressAsync(int studentId, int lessonId)
    {
        // Tính tổng tiến độ của Bài học dựa trên trung bình cộng tiến độ các Nội dung (trừ Quiz nếu cần thiết)
        // Yêu cầu: Tất cả nội dung trong bài học (bao gồm Quiz, Video, v.v) đều chia đều tỉ trọng hoặc cấu hình riêng
        // Giả sử chia đều:
        var contents = await _context.BaiHocNoiDungs
            .Where(c => c.MaBaiHoc == lessonId && c.TrangThai == "da_xuat_ban")
            .Select(c => c.MaNoiDung)
            .ToListAsync();

        if (!contents.Any()) return;

        var progresses = await _context.TienDoNoiDungHocTaps
            .Where(p => p.MaHocSinh == studentId && contents.Contains(p.MaNoiDung))
            .ToListAsync();

        decimal totalPercent = 0;
        foreach (var contentId in contents)
        {
            var p = progresses.FirstOrDefault(x => x.MaNoiDung == contentId);
            if (p != null)
            {
                totalPercent += p.PhanTramTienDo;
            }
        }

        var avgPercent = totalPercent / contents.Count;

        var lessonProgress = await _context.TienDoBaiHocs
            .FirstOrDefaultAsync(p => p.MaHocSinh == studentId && p.MaBaiHoc == lessonId);

        if (lessonProgress == null)
        {
            lessonProgress = new TienDoBaiHoc
            {
                MaHocSinh = studentId,
                MaBaiHoc = lessonId,
                PhanTramTienDo = avgPercent,
                LanGuiNhipTimCuoi = DateTime.UtcNow
            };
            _context.TienDoBaiHocs.Add(lessonProgress);
        }
        else
        {
            lessonProgress.PhanTramTienDo = avgPercent;
            lessonProgress.LanGuiNhipTimCuoi = DateTime.UtcNow;
        }

        if (avgPercent >= 100 && lessonProgress.HoanThanhLuc == null)
        {
            lessonProgress.PhanTramTienDo = 100;
            lessonProgress.HoanThanhLuc = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task SyncQuizProgressAsync(int studentId, int contentId, PhienThiHocSinh phienThi)
    {
        var progress = await _context.TienDoNoiDungHocTaps
            .FirstOrDefaultAsync(p => p.MaHocSinh == studentId && p.MaNoiDung == contentId);

        if (progress == null)
        {
            progress = new TienDoNoiDungHocTap
            {
                MaHocSinh = studentId,
                MaNoiDung = contentId,
                LoaiNoiDung = "trac_nghiem",
                BatDauLuc = DateTime.UtcNow
            };
            _context.TienDoNoiDungHocTaps.Add(progress);
        }

        progress.LanTuongTacCuoi = DateTime.UtcNow;

        if (phienThi.TrangThaiLuong == "da_dung")
        {
            // Quiz đã nộp -> cập nhật 100% nếu chỉ cần hoàn thành, 
            // hoặc phải đạt điểm qua môn? Yêu cầu không bắt buộc điểm qua để tính tiến độ video, 
            // nhưng với quiz có thể tính là 100% khi nộp.
            progress.PhanTramTienDo = 100;
            progress.TrangThai = "hoan_thanh";
            if (progress.HoanThanhLuc == null) progress.HoanThanhLuc = DateTime.UtcNow;
        }
        else
        {
            progress.PhanTramTienDo = 50; // Dang lam
            progress.TrangThai = "dang_hoc";
        }

        await _context.SaveChangesAsync();

        // Đồng bộ lên Bài Học
        var content = await _context.BaiHocNoiDungs.FindAsync(contentId);
        if (content != null)
        {
            await SyncLessonProgressAsync(studentId, content.MaBaiHoc);
        }
    }
}
