using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.AdminUsers;

public class UserQueryParameters
{
    [Range(1, int.MaxValue, ErrorMessage = "pageIndex phải lớn hơn 0.")]
    public int PageIndex { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "pageSize phải từ 1 đến 100.")]
    public int PageSize { get; set; } = 20;

    public string? Keyword { get; set; }
    public string? Role { get; set; }
    public string? TrangThai { get; set; }
    public int? MaDonVi { get; set; }
}
