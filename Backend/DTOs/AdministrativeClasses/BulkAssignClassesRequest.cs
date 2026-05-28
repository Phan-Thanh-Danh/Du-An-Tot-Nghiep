using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.AdministrativeClasses;

public class BulkAssignClassesRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Chương trình đào tạo không hợp lệ.")]
    public int MaChuongTrinh { get; set; }

    [Required(ErrorMessage = "Danh sách lớp không được để trống.")]
    [MinLength(1, ErrorMessage = "Phải có ít nhất một lớp.")]
    public List<int> MaLops { get; set; } = [];
}
