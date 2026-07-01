using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Exam;

// ===== KyThi DTOs =====
public class KyThiDto
{
    public int MaKyThi { get; set; }
    public string TenKyThi { get; set; } = string.Empty;
    public int MaHocKy { get; set; }
    public string? TenHocKy { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
    public int SoLichThiTong { get; set; }
}

public class CreateKyThiRequest
{
    [Required(ErrorMessage = "Tên kỳ thi là bắt buộc.")]
    [MaxLength(255, ErrorMessage = "Tên kỳ thi tối đa 255 ký tự.")]
    public string TenKyThi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mã học kỳ là bắt buộc.")]
    public int MaHocKy { get; set; }
}

public class UpdateKyThiRequest
{
    [MaxLength(255, ErrorMessage = "Tên kỳ thi tối đa 255 ký tự.")]
    public string? TenKyThi { get; set; }

    public string? TrangThai { get; set; }
}

// ===== LichThiTong DTOs =====
public class LichThiTongDto
{
    public int MaLichThiTong { get; set; }
    public int MaKyThi { get; set; }
    public string? TenKyThi { get; set; }
    public int MaMonHoc { get; set; }
    public string? TenMonHoc { get; set; }
    public int? MaDeKiemTra { get; set; }
    public string? TenDeKiemTra { get; set; }
    public string HinhThucThi { get; set; } = string.Empty;
    public DateTime NgayThiDuKien { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
    public int SoCaThi { get; set; }
}

public class CreateLichThiTongRequest
{
    [Required(ErrorMessage = "Mã kỳ thi là bắt buộc.")]
    public int MaKyThi { get; set; }

    [Required(ErrorMessage = "Mã môn học là bắt buộc.")]
    public int MaMonHoc { get; set; }

    public int? MaDeKiemTra { get; set; }

    [Required(ErrorMessage = "Hình thức thi là bắt buộc.")]
    public string HinhThucThi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ngày thi dự kiến là bắt buộc.")]
    public DateTime NgayThiDuKien { get; set; }
}

public class UpdateLichThiTongRequest
{
    public int? MaDeKiemTra { get; set; }
    public string? HinhThucThi { get; set; }
    public DateTime? NgayThiDuKien { get; set; }
    public string? TrangThai { get; set; }
}

// ===== CaThi DTOs =====
public class CaThiDto
{
    public int MaCaThi { get; set; }
    public int MaLichThiTong { get; set; }
    public string TenCaThi { get; set; } = string.Empty;
    public int? MaPhong { get; set; }
    public string? TenPhong { get; set; }
    public DateTime NgayThi { get; set; }
    public DateTime ThoiGianBatDau { get; set; }
    public DateTime ThoiGianKetThuc { get; set; }
    public int MaDonVi { get; set; }
    public string? TenDonVi { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string? GhiChu { get; set; }
    public int SoThiSinh { get; set; }
    public int SoGiamThi { get; set; }
}

public class CreateCaThiRequest
{
    [Required(ErrorMessage = "Mã lịch thi tổng là bắt buộc.")]
    public int MaLichThiTong { get; set; }

    [Required(ErrorMessage = "Tên ca thi là bắt buộc.")]
    [MaxLength(100, ErrorMessage = "Tên ca thi tối đa 100 ký tự.")]
    public string TenCaThi { get; set; } = string.Empty;

    public int? MaPhong { get; set; }

    [Required(ErrorMessage = "Ngày thi là bắt buộc.")]
    public DateTime NgayThi { get; set; }

    [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc.")]
    public DateTime ThoiGianBatDau { get; set; }

    [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc.")]
    public DateTime ThoiGianKetThuc { get; set; }

    [Required(ErrorMessage = "Mã đơn vị là bắt buộc.")]
    public int MaDonVi { get; set; }

    public string? GhiChu { get; set; }
}

public class UpdateCaThiRequest
{
    public string? TenCaThi { get; set; }
    public int? MaPhong { get; set; }
    public DateTime? NgayThi { get; set; }
    public DateTime? ThoiGianBatDau { get; set; }
    public DateTime? ThoiGianKetThuc { get; set; }
    public string? TrangThai { get; set; }
    public string? GhiChu { get; set; }
    public string? LyDoDieuChinh { get; set; }
}

// ===== PhanCongGiamThi DTOs =====
public class PhanCongGiamThiDto
{
    public int MaPhanCong { get; set; }
    public int MaCaThi { get; set; }
    public int MaGiamThi { get; set; }
    public string? TenGiamThi { get; set; }
    public string VaiTroGiamThi { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public string? LyDoThayDoi { get; set; }
}

public class CreatePhanCongGiamThiRequest
{
    [Required(ErrorMessage = "Mã ca thi là bắt buộc.")]
    public int MaCaThi { get; set; }

    [Required(ErrorMessage = "Mã giám thị là bắt buộc.")]
    public int MaGiamThi { get; set; }

    [Required(ErrorMessage = "Vai trò giám thị là bắt buộc.")]
    public string VaiTroGiamThi { get; set; } = "giam_thi_phu";
}

// ===== ThiSinhCaThi DTOs =====
public class ThiSinhCaThiDto
{
    public int MaThiSinhCaThi { get; set; }
    public int MaCaThi { get; set; }
    public int MaHocSinh { get; set; }
    public string? TenHocSinh { get; set; }
    public string? Email { get; set; }
    public string TrangThaiDuThi { get; set; } = string.Empty;
    public string? GhiChu { get; set; }
}

public class AddThiSinhCaThiRequest
{
    [Required(ErrorMessage = "Mã ca thi là bắt buộc.")]
    public int MaCaThi { get; set; }

    [Required(ErrorMessage = "Danh sách mã học sinh là bắt buộc.")]
    public List<int> DanhSachMaHocSinh { get; set; } = new();
}

// ===== DiemDanhThi DTOs =====
public class DiemDanhThiDto
{
    public int MaDiemDanhThi { get; set; }
    public int MaCaThi { get; set; }
    public int MaHocSinh { get; set; }
    public string? TenHocSinh { get; set; }
    public string TrangThaiDiemDanh { get; set; } = string.Empty;
    public DateTime? ThoiDiemDiemDanh { get; set; }
    public int? MaNguoiDiemDanh { get; set; }
    public string? TenNguoiDiemDanh { get; set; }
    public string? GhiChu { get; set; }
}

public class UpdateDiemDanhThiRequest
{
    [Required(ErrorMessage = "Mã học sinh là bắt buộc.")]
    public int MaHocSinh { get; set; }

    [Required(ErrorMessage = "Trạng thái điểm danh là bắt buộc.")]
    public string TrangThaiDiemDanh { get; set; } = string.Empty;

    public string? GhiChu { get; set; }
}

public class BatchDiemDanhRequest
{
    [Required(ErrorMessage = "Mã ca thi là bắt buộc.")]
    public int MaCaThi { get; set; }

    [Required(ErrorMessage = "Danh sách điểm danh là bắt buộc.")]
    public List<UpdateDiemDanhThiRequest> DanhSachDiemDanh { get; set; } = new();
}

// ===== NhatKyViPhamThi DTOs =====
public class NhatKyViPhamThiDto
{
    public int MaViPham { get; set; }
    public int? MaPhienThi { get; set; }
    public int MaHocSinh { get; set; }
    public string? TenHocSinh { get; set; }
    public int MaCaThi { get; set; }
    public string LoaiViPham { get; set; } = string.Empty;
    public string MucDo { get; set; } = string.Empty;
    public string? ChiTietJson { get; set; }
    public DateTime ThoiDiem { get; set; }
    public int SoLanXuLy { get; set; }
}

public class CreateNhatKyViPhamRequest
{
    public int? MaPhienThi { get; set; }

    [Required(ErrorMessage = "Mã học sinh là bắt buộc.")]
    public int MaHocSinh { get; set; }

    [Required(ErrorMessage = "Mã ca thi là bắt buộc.")]
    public int MaCaThi { get; set; }

    [Required(ErrorMessage = "Loại vi phạm là bắt buộc.")]
    public string LoaiViPham { get; set; } = string.Empty;

    public string MucDo { get; set; } = "nhac_nho";
    public string? ChiTietJson { get; set; }
}

// ===== XuLyViPhamThi DTOs =====
public class XuLyViPhamThiDto
{
    public int MaXuLy { get; set; }
    public int MaViPham { get; set; }
    public string HanhDongXuLy { get; set; } = string.Empty;
    public int LanNhacNho { get; set; }
    public int MaNguoiXuLy { get; set; }
    public string? TenNguoiXuLy { get; set; }
    public DateTime ThoiDiem { get; set; }
    public string? LyDo { get; set; }
    public string? GhiChu { get; set; }
}

public class CreateXuLyViPhamRequest
{
    [Required(ErrorMessage = "Mã vi phạm là bắt buộc.")]
    public int MaViPham { get; set; }

    [Required(ErrorMessage = "Hành động xử lý là bắt buộc.")]
    public string HanhDongXuLy { get; set; } = string.Empty;

    public string? LyDo { get; set; }
    public string? GhiChu { get; set; }
}

// ===== BienBanThi DTOs =====
public class BienBanThiDto
{
    public int MaBienBan { get; set; }
    public int MaCaThi { get; set; }
    public int? MaPhienThi { get; set; }
    public string LoaiBienBan { get; set; } = string.Empty;
    public string NoiDung { get; set; } = string.Empty;
    public int MaNguoiLap { get; set; }
    public string? TenNguoiLap { get; set; }
    public DateTime ThoiDiemLap { get; set; }
    public string TrangThaiXuLy { get; set; } = string.Empty;
}

public class CreateBienBanThiRequest
{
    [Required(ErrorMessage = "Mã ca thi là bắt buộc.")]
    public int MaCaThi { get; set; }

    public int? MaPhienThi { get; set; }

    [Required(ErrorMessage = "Loại biên bản là bắt buộc.")]
    public string LoaiBienBan { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nội dung biên bản là bắt buộc.")]
    public string NoiDung { get; set; } = string.Empty;
}

// ===== Signature DTOs =====
public class ConfirmSignatureRequest
{
    [Required(ErrorMessage = "Mã phiên thi là bắt buộc.")]
    public int MaPhienThi { get; set; }
}

public class ReportMissingSignatureRequest
{
    [Required(ErrorMessage = "Mã phiên thi là bắt buộc.")]
    public int MaPhienThi { get; set; }

    public string? GhiChu { get; set; }
}

// ===== Exam Taking DTOs =====
public class StartExamRequest
{
    [Required(ErrorMessage = "Mã ca thi là bắt buộc.")]
    public int MaCaThi { get; set; }
}

public class AutoSaveAnswerRequest
{
    [Required(ErrorMessage = "Mã phiên thi là bắt buộc.")]
    public int MaPhienThi { get; set; }

    [Required(ErrorMessage = "Câu trả lời JSON là bắt buộc.")]
    public string CauTraLoiJson { get; set; } = string.Empty;
}

public class SubmitExamRequest
{
    [Required(ErrorMessage = "Mã phiên thi là bắt buộc.")]
    public int MaPhienThi { get; set; }

    [Required(ErrorMessage = "Câu trả lời JSON là bắt buộc.")]
    public string CauTraLoiJson { get; set; } = string.Empty;
}

// ===== Grading DTOs =====
public class GradeEssayRequest
{
    [Required(ErrorMessage = "Mã phiên thi là bắt buộc.")]
    public int MaPhienThi { get; set; }

    [Required(ErrorMessage = "Điểm cuối cùng là bắt buộc.")]
    [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10.")]
    public decimal DiemCuoiCung { get; set; }
}

public class PublishScoresRequest
{
    [Required(ErrorMessage = "Mã ca thi là bắt buộc.")]
    public int MaCaThi { get; set; }
}

// ===== Report DTOs =====
public class ExamReportSummaryDto
{
    public int TongSoKyThi { get; set; }
    public int TongSoCaThi { get; set; }
    public int TongSoThiSinh { get; set; }
    public int SoThiSinhCoMat { get; set; }
    public int SoThiSinhVangMat { get; set; }
    public int TongSoViPham { get; set; }
    public int TongSoBienBan { get; set; }
    public decimal DiemTrungBinh { get; set; }
}

// ===== Query Parameters =====
public class ExamQueryParameters
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public int? MaHocKy { get; set; }
    public string? TrangThai { get; set; }
}

public class CaThiQueryParameters
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? MaLichThiTong { get; set; }
    public int? MaDonVi { get; set; }
    public string? TrangThai { get; set; }
    public DateTime? TuNgay { get; set; }
    public DateTime? DenNgay { get; set; }
}

public class DeKiemTraDto
{
    public int MaDeKiemTra { get; set; }
    public int? MaMonHoc { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public int ThoiGianPhut { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string? LoaiDeThi { get; set; }
    public string? HinhThucThi { get; set; }
}

public class StudentExamListItemDto
{
    public string Id { get; set; } = string.Empty;
    public int MaDeKiemTra { get; set; }
    public int? MaCaThi { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string SubjectCode { get; set; } = string.Empty;
    public string MajorName { get; set; } = string.Empty;
    public string FacultyName { get; set; } = string.Empty;
    public string SemesterName { get; set; } = string.Empty;
    public string BlockName { get; set; } = string.Empty;
    public int PlannedSemesterIndex { get; set; }
    public int PlannedBlockIndex { get; set; }
    public int StudentCurrentSemesterIndex { get; set; }
    public int StudentCurrentBlockIndex { get; set; }
    public int DurationMinutes { get; set; }
    public int TotalQuestions { get; set; }
    public string ExamTypeLabel { get; set; } = string.Empty;
    public int UsedAttempts { get; set; }
    public int MaxAttempts { get; set; } = 1;
    public string Status { get; set; } = string.Empty;
    public string AccessStatus { get; set; } = string.Empty;
    public string? OpenAt { get; set; }
    public string? CloseAt { get; set; }
    public decimal? Score { get; set; }
    public string? ResultId { get; set; }
    public string? ClassSectionCode { get; set; }
    public string? TrangThaiDuThi { get; set; }
}
