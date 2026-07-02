using Backend.DTOs.Common;
using Backend.DTOs.Exam;
using Backend.DTOs.QuizAttempts;

namespace Backend.Services.Exam;

public interface IExamService
{
    // KyThi
    Task<PagedResultDto<KyThiDto>> GetKyThisAsync(ExamQueryParameters parameters, CancellationToken ct);
    Task<KyThiDto> GetKyThiByIdAsync(int id, CancellationToken ct);
    Task<KyThiDto> CreateKyThiAsync(CreateKyThiRequest request, CancellationToken ct);
    Task<KyThiDto> UpdateKyThiAsync(int id, UpdateKyThiRequest request, CancellationToken ct);

    // LichThiTong
    Task<PagedResultDto<LichThiTongDto>> GetLichThiTongsAsync(ExamQueryParameters parameters, CancellationToken ct);
    Task<LichThiTongDto> GetLichThiTongByIdAsync(int id, CancellationToken ct);
    Task<LichThiTongDto> CreateLichThiTongAsync(CreateLichThiTongRequest request, CancellationToken ct);
    Task<LichThiTongDto> UpdateLichThiTongAsync(int id, UpdateLichThiTongRequest request, CancellationToken ct);
    Task SendToCoSoAsync(int id, CancellationToken ct);

    // CaThi
    Task<PagedResultDto<CaThiDto>> GetCaThisAsync(CaThiQueryParameters parameters, CancellationToken ct);
    Task<CaThiDto> GetCaThiByIdAsync(int id, CancellationToken ct);
    Task<CaThiDto> CreateCaThiAsync(CreateCaThiRequest request, CancellationToken ct);
    Task<CaThiDto> UpdateCaThiAsync(int id, UpdateCaThiRequest request, CancellationToken ct);
    Task StartCaThiAsync(int id, CancellationToken ct);

    // PhanCongGiamThi
    Task<IReadOnlyList<PhanCongGiamThiDto>> GetGiamThisByCaThiAsync(int maCaThi, CancellationToken ct);
    Task<PhanCongGiamThiDto> AssignGiamThiAsync(CreatePhanCongGiamThiRequest request, CancellationToken ct);
    Task RemoveGiamThiAsync(int maPhanCong, CancellationToken ct);

    // ThiSinhCaThi
    Task<IReadOnlyList<ThiSinhCaThiDto>> GetThiSinhsByCaThiAsync(int maCaThi, CancellationToken ct);
    Task<IReadOnlyList<ThiSinhCaThiDto>> AddThiSinhsToCaThiAsync(AddThiSinhCaThiRequest request, CancellationToken ct);

    // DiemDanhThi
    Task<IReadOnlyList<DiemDanhThiDto>> GetDiemDanhByCaThiAsync(int maCaThi, CancellationToken ct);
    Task<IReadOnlyList<DiemDanhThiDto>> BatchDiemDanhAsync(BatchDiemDanhRequest request, int maNguoiDiemDanh, CancellationToken ct);

    // NhatKyViPhamThi
    Task<IReadOnlyList<NhatKyViPhamThiDto>> GetViPhamsByCaThiAsync(int maCaThi, CancellationToken ct);
    Task<NhatKyViPhamThiDto> CreateViPhamAsync(CreateNhatKyViPhamRequest request, CancellationToken ct);

    // XuLyViPhamThi
    Task<XuLyViPhamThiDto> XuLyViPhamAsync(CreateXuLyViPhamRequest request, int maNguoiXuLy, CancellationToken ct);

    // BienBanThi
    Task<IReadOnlyList<BienBanThiDto>> GetBienBansByCaThiAsync(int maCaThi, CancellationToken ct);
    Task<BienBanThiDto> CreateBienBanAsync(CreateBienBanThiRequest request, int maNguoiLap, CancellationToken ct);

    // Signature
    Task<PhienThiDto> ConfirmSignatureAsync(ConfirmSignatureRequest request, int maNguoiXacNhan, CancellationToken ct);
    Task<PhienThiDto> ReportMissingSignatureAsync(ReportMissingSignatureRequest request, int maNguoiXacNhan, CancellationToken ct);
    Task<IReadOnlyList<PhienThiDto>> GetMissingSignatureSessionsAsync(int? maCaThi, CancellationToken ct);
    Task<IReadOnlyList<PhienThiDto>> GetSignedSessionsAsync(int? maCaThi, CancellationToken ct);

    // Exam Taking
    Task<IReadOnlyList<StudentExamListItemDto>> GetStudentExamsAsync(int maHocSinh, CancellationToken ct);
    Task<PhienThiDto> GetExamSessionAsync(int maPhienThi, int maHocSinh, CancellationToken ct);
    Task<PhienThiDto> StartExamAsync(StartExamRequest request, int maHocSinh, CancellationToken ct);
    Task<IReadOnlyList<QuizAttemptQuestionDto>> GetExamQuestionsAsync(int maPhienThi, int maHocSinh, CancellationToken ct);
    Task AutoSaveAnswerAsync(AutoSaveAnswerRequest request, int maHocSinh, CancellationToken ct);
    Task<PhienThiDto> SubmitExamAsync(SubmitExamRequest request, int maHocSinh, CancellationToken ct);

    // Grading
    Task FinalizeAutoGradeAsync(int maCaThi, CancellationToken ct);
    Task<PhienThiDto> GradeEssayAsync(GradeEssayRequest request, CancellationToken ct);
    Task PublishScoresAsync(PublishScoresRequest request, CancellationToken ct);

    // Reports
    Task<ExamReportSummaryDto> GetReportSummaryAsync(int? maKyThi, int? maDonVi, CancellationToken ct);

    // DeKiemTra Helper
    Task<IReadOnlyList<DeKiemTraDto>> GetDeKiemTrasAsync(CancellationToken ct);
}

// Response DTO for exam session (PhienThiHocSinh)
public class PhienThiDto
{
    public int MaPhienThi { get; set; }
    public int MaDeKiemTra { get; set; }
    public int MaHocSinh { get; set; }
    public string? TenHocSinh { get; set; }
    public DateTime? BatDauLuc { get; set; }
    public DateTime? NopLuc { get; set; }
    public string? CauTraLoiJson { get; set; }
    public string TrangThaiLuong { get; set; } = string.Empty;
    public decimal? DiemTuDong { get; set; }
    public decimal? DiemCuoiCung { get; set; }
    public decimal? DiemTuLuanAiGoiY { get; set; }
    public int LanThu { get; set; }
    public DateTime? HanNopLuc { get; set; }
    public int? SoCauDung { get; set; }
    public bool? KetQuaDat { get; set; }
    public int? MaCaThi { get; set; }
    public string? TrangThaiKyTen { get; set; }
    public DateTime? ThoiDiemKy { get; set; }
    public int? NguoiXacNhanKyTen { get; set; }
    public string? TrangThaiCongBo { get; set; }
}
