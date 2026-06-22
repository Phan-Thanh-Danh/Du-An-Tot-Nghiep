using System.Text.Json;

namespace Backend.DTOs.QuizAttempts;

public class QuizAvailabilityDto
{
    public int MaDeKiemTra { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public bool CoTheLam { get; set; }
    public string? LyDoKhongTheLam { get; set; }
    public int SoLanDaLam { get; set; }
    public int? SoLanLamToiDa { get; set; }
    public bool KhongGioiHanSoLan { get; set; }
    public DateTime? MoLuc { get; set; }
    public DateTime? DongLuc { get; set; }
    public DateTime? HanNopDangHoatDong { get; set; }
    public QuizResultSummaryDto? KetQuaHienTai { get; set; }
}

public class QuizResultSummaryDto
{
    public decimal? DiemCuoiCung { get; set; }
    public bool? KetQuaDat { get; set; }
    public int? SoCauDung { get; set; }
    public int TongSoCau { get; set; }
}

public class StartQuizAttemptResponse
{
    public int MaPhienThi { get; set; }
    public int MaDeKiemTra { get; set; }
    public int LanThu { get; set; }
    public DateTime? BatDauLuc { get; set; }
    public DateTime? HanNopLuc { get; set; }
    public int ThoiGianPhut { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public IReadOnlyList<QuizAttemptQuestionDto> CauHoi { get; set; } = Array.Empty<QuizAttemptQuestionDto>();
}

public class QuizAttemptQuestionDto
{
    public int MaCauHoi { get; set; }
    public string LoaiCauHoi { get; set; } = string.Empty;
    public string NoiDung { get; set; } = string.Empty;
    public string? KieuLuaChon { get; set; }
    public JsonElement? LuaChon { get; set; }
    public decimal DiemSo { get; set; }
    public int? ThuTu { get; set; }
}

public class QuizAttemptAnswerDto
{
    public int MaCauHoi { get; set; }
    public List<string> SelectedOptionIds { get; set; } = new();
    public string? EssayText { get; set; }
}

public class SaveQuizAnswersRequest
{
    public List<QuizAttemptAnswerDto> Answers { get; set; } = new();
}

public class SubmitQuizAttemptRequest : SaveQuizAnswersRequest
{
}

public class QuizAttemptResultDto
{
    public int MaPhienThi { get; set; }
    public int MaDeKiemTra { get; set; }
    public int LanThu { get; set; }
    public string TrangThaiLuong { get; set; } = string.Empty;
    public DateTime? NopLuc { get; set; }
    public decimal? DiemTuDong { get; set; }
    public decimal? DiemCuoiCung { get; set; }
    public int? SoCauDung { get; set; }
    public int TongSoCau { get; set; }
    public bool? KetQuaDat { get; set; }
    public bool CoCauTuLuanChoCham { get; set; }
    public bool HienKetQua { get; set; }
    public bool HienDapAnDung { get; set; }
    public IReadOnlyList<QuizQuestionGradingDetailDto>? ChiTiet { get; set; }
}

public class QuizAttemptHistoryDto
{
    public int MaDeKiemTra { get; set; }
    public IReadOnlyList<QuizAttemptHistoryItemDto> LanLam { get; set; } = Array.Empty<QuizAttemptHistoryItemDto>();
    public QuizResultSummaryDto? KetQuaCuoi { get; set; }
}

public class QuizAttemptHistoryItemDto
{
    public int MaPhienThi { get; set; }
    public int LanThu { get; set; }
    public DateTime? BatDauLuc { get; set; }
    public DateTime? NopLuc { get; set; }
    public string TrangThaiLuong { get; set; } = string.Empty;
    public decimal? DiemTuDong { get; set; }
    public decimal? DiemCuoiCung { get; set; }
    public int? SoCauDung { get; set; }
    public bool? KetQuaDat { get; set; }
}

public class QuizAttemptSnapshotDto
{
    public List<QuizQuestionSnapshotDto> Questions { get; set; } = new();
}

public class QuizQuestionSnapshotDto
{
    public int MaCauHoi { get; set; }
    public int ThuTu { get; set; }
    public List<string> ChoiceOrder { get; set; } = new();
}

public class QuizGradingResultDto
{
    public decimal DiemTracNghiem { get; set; }
    public int SoCauDung { get; set; }
    public int SoCauSai { get; set; }
    public int SoCauChuaTraLoi { get; set; }
    public int TongSoCau { get; set; }
    public bool CoCauTuLuan { get; set; }
    public IReadOnlyList<QuizQuestionGradingDetailDto> ChiTiet { get; set; } = Array.Empty<QuizQuestionGradingDetailDto>();
}

public class QuizQuestionGradingDetailDto
{
    public int MaCauHoi { get; set; }
    public decimal DiemToiDa { get; set; }
    public decimal? DiemDatDuoc { get; set; }
    public bool? Dung { get; set; }
    public bool ChuaTraLoi { get; set; }
    public IReadOnlyList<string>? DapAnDung { get; set; }
    public string? GiaiThichDapAn { get; set; }
}
