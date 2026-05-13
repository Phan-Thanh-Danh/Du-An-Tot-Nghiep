using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Rbac;

public class AssignUserRolesRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Vai trò chính không hợp lệ.")]
    public int MaVaiTroChinh { get; set; }

    public List<int> MaVaiTroBoSung { get; set; } = [];
}
