using Backend.Data;
using Backend.DTOs.LearningProgress;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Backend.Services.LearningProgress;

public interface ILearningProgressService
{
    Task<ContentSessionResponseDto> StartContentSessionAsync(int studentId, ContentSessionRequestDto request, string? userAgent, string? ipAddress);
    Task EndContentSessionAsync(int studentId, Guid sessionToken);
    Task<ContentSessionResponseDto> ProcessHeartbeatAsync(int studentId, ContentHeartbeatRequestDto request);
}

public class LearningProgressService : ILearningProgressService
{
    private readonly ApplicationDbContext _context;
    private readonly IStudentContentAccessService _accessService;
    private readonly ILearningProgressCalculator _calculator;
    private readonly ILearningProgressSyncService _syncService;
    private readonly LearningProgressOptions _options;

    public LearningProgressService(
        ApplicationDbContext context,
        IStudentContentAccessService accessService,
        ILearningProgressCalculator calculator,
        ILearningProgressSyncService syncService,
        IOptions<LearningProgressOptions> options)
    {
        _context = context;
        _accessService = accessService;
        _calculator = calculator;
        _syncService = syncService;
        _options = options.Value;
    }

    public async Task<ContentSessionResponseDto> StartContentSessionAsync(int studentId, ContentSessionRequestDto request, string? userAgent, string? ipAddress)
    {
        if (!await _accessService.CanAccessContentAsync(studentId, request.MaNoiDung))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Không có quyền truy cập nội dung này.");
        }

        var content = await _context.BaiHocNoiDungs.FindAsync(request.MaNoiDung);
        if (content == null) throw new KeyNotFoundException("Không tìm thấy nội dung.");

        if (content.LoaiNoiDung == "trac_nghiem")
        {
            throw new InvalidOperationException("Quiz không sử dụng heartbeat tracking. Hãy dùng hệ thống thi.");
        }

        // Kết thúc các phiên cũ của nội dung này
        var oldSessions = await _context.PhienHocNoiDungs
            .Where(s => s.MaHocSinh == studentId && s.MaNoiDung == request.MaNoiDung && s.TrangThai == "dang_hoat_dong")
            .ToListAsync();
        
        foreach(var old in oldSessions)
        {
            old.TrangThai = "bi_thay_the";
            old.KetThucLuc = DateTime.UtcNow;
        }

        // Tạo phiên mới
        var session = new PhienHocNoiDung
        {
            SessionToken = Guid.NewGuid(),
            MaHocSinh = studentId,
            MaNoiDung = request.MaNoiDung,
            BatDauLuc = DateTime.UtcNow,
            TrangThai = "dang_hoat_dong",
            UserAgentHash = userAgent,
            DiaChiIpHash = ipAddress
        };
        _context.PhienHocNoiDungs.Add(session);

        // Lấy hoặc tạo tiến độ
        var progress = await _context.TienDoNoiDungHocTaps
            .FirstOrDefaultAsync(p => p.MaHocSinh == studentId && p.MaNoiDung == request.MaNoiDung);

        if (progress == null)
        {
            progress = new TienDoNoiDungHocTap
            {
                MaHocSinh = studentId,
                MaNoiDung = request.MaNoiDung,
                LoaiNoiDung = content.LoaiNoiDung,
                BatDauLuc = DateTime.UtcNow
            };
            _context.TienDoNoiDungHocTaps.Add(progress);
        }

        progress.LanTuongTacCuoi = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ContentSessionResponseDto
        {
            SessionToken = session.SessionToken,
            TrangThaiHienTai = progress.TrangThai,
            PhanTramTienDo = progress.PhanTramTienDo,
            HeartbeatExpectedSeconds = _options.HeartbeatExpectedSeconds,
            ViTriVideoBatDau = progress.ViTriVideoCuoiGiay
        };
    }

    public async Task EndContentSessionAsync(int studentId, Guid sessionToken)
    {
        var session = await _context.PhienHocNoiDungs
            .FirstOrDefaultAsync(s => s.SessionToken == sessionToken && s.MaHocSinh == studentId);

        if (session != null && session.TrangThai == "dang_hoat_dong")
        {
            session.TrangThai = "da_ket_thuc";
            session.KetThucLuc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<ContentSessionResponseDto> ProcessHeartbeatAsync(int studentId, ContentHeartbeatRequestDto request)
    {
        var session = await _context.PhienHocNoiDungs
            .Include(s => s.NoiDung)
            .FirstOrDefaultAsync(s => s.SessionToken == request.SessionToken && s.MaHocSinh == studentId);

        if (session == null || session.TrangThai != "dang_hoat_dong")
        {
            throw new InvalidOperationException("Phiên học không hợp lệ hoặc đã kết thúc.");
        }

        // Chống gian lận: Kiểm tra khoảng thời gian giữa các heartbeat
        var now = DateTime.UtcNow;
        var lastTime = session.NhipTimCuoiLuc ?? session.BatDauLuc;
        var elapsed = (now - lastTime).TotalSeconds;

        var expectedElapsed = _options.HeartbeatExpectedSeconds * _options.MaxBatchHeartbeats;
        if (elapsed > expectedElapsed + _options.HeartbeatToleranceSeconds)
        {
            // Quá thời gian tối đa cho phép do treo máy hoặc cố ý gọi api trễ -> đánh dấu hết hạn
            session.TrangThai = "het_han";
            session.KetThucLuc = now;
            await _context.SaveChangesAsync();
            throw new InvalidOperationException("Phiên học đã hết hạn do không tương tác quá lâu.");
        }

        // Cập nhật session
        session.NhipTimCuoiLuc = now;
        session.SoThuTuNhipTimCuoi = request.SoThuTuNhipTim;
        
        // Chỉ tính toán số giây hợp lệ dựa vào elapsed nhưng không vượt quá cấu hình tối đa 1 heartbeat + tolerance
        var validSecondsToAdd = (int)Math.Min(elapsed, _options.HeartbeatExpectedSeconds + _options.HeartbeatToleranceSeconds);
        session.SoGiayHoatDongDaXacNhan += validSecondsToAdd;

        if (request.ViTriVideoGiay.HasValue) session.ViTriVideoCuoiGiay = request.ViTriVideoGiay.Value;
        if (request.PhanTramCuon.HasValue && (!session.PhanTramCuonLonNhat.HasValue || request.PhanTramCuon.Value > session.PhanTramCuonLonNhat.Value))
        {
            session.PhanTramCuonLonNhat = request.PhanTramCuon.Value;
        }

        // Cập nhật Tiến độ chính
        var progress = await _context.TienDoNoiDungHocTaps
            .FirstOrDefaultAsync(p => p.MaHocSinh == studentId && p.MaNoiDung == session.MaNoiDung);

        if (progress != null)
        {
            progress.SoGiayDaXacNhan += validSecondsToAdd;
            progress.LanTuongTacCuoi = now;
            if (session.ViTriVideoCuoiGiay.HasValue) progress.ViTriVideoCuoiGiay = session.ViTriVideoCuoiGiay;
            if (session.PhanTramCuonLonNhat.HasValue && (!progress.PhanTramCuonLonNhat.HasValue || session.PhanTramCuonLonNhat.Value > progress.PhanTramCuonLonNhat.Value))
            {
                progress.PhanTramCuonLonNhat = session.PhanTramCuonLonNhat.Value;
            }

            _calculator.CalculateProgress(progress, session.NoiDung!);
        }

        await _context.SaveChangesAsync();

        // Đồng bộ tiến độ lên mức bài học
        if (session.NoiDung != null)
        {
            await _syncService.SyncLessonProgressAsync(studentId, session.NoiDung.MaBaiHoc);
        }

        return new ContentSessionResponseDto
        {
            SessionToken = session.SessionToken,
            TrangThaiHienTai = progress?.TrangThai ?? "dang_hoc",
            PhanTramTienDo = progress?.PhanTramTienDo ?? 0,
            HeartbeatExpectedSeconds = _options.HeartbeatExpectedSeconds,
            ViTriVideoBatDau = progress?.ViTriVideoCuoiGiay
        };
    }
}
