using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.TeacherPersonnel;

public class CreateTeacherPersonnelRequestDto
{
    [Required(ErrorMessage = "Họ tên giảng viên không được để trống")]
    [MaxLength(255)]
    public string HoTen { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? SoDienThoai { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu khởi tạo")]
    [MinLength(6, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên")]
    public string MatKhau { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn cơ sở/đơn vị trực thuộc")]
    public int MaDonVi { get; set; }

    public int? MaChuyenNganhChinh { get; set; }
    public List<int> DanhSachMonDuocPhepDay { get; set; } = [];
    public string? GhiChu { get; set; }
}

public class UpdateTeacherPersonnelRequestDto
{
    [Required(ErrorMessage = "Họ tên giảng viên không được để trống")]
    [MaxLength(255)]
    public string HoTen { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? SoDienThoai { get; set; }

    public string TrangThai { get; set; } = "hoat_dong"; // hoat_dong, bi_khoa, tam_nghi

    public int? MaChuyenNganhChinh { get; set; }
    public List<UpdateTeacherSubjectItemDto> DanhSachMonHoc { get; set; } = [];
    public string? LyDo { get; set; }
}

public class UpdateTeacherSubjectItemDto
{
    public int MaMonHoc { get; set; }
    public int MucDoPhuHop { get; set; } = 80;
    public int? SoNamKinhNghiem { get; set; }
    public bool LaMonChinh { get; set; }
    public bool ConHoatDong { get; set; } = true;
}

public class ToggleTeacherLockRequestDto
{
    [Required(ErrorMessage = "Vui lòng cung cấp lý do khóa/mở khóa")]
    public string LyDo { get; set; } = string.Empty;
}
