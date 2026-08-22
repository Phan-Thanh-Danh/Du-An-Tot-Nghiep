using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Rbac;

public class UpdateRolePermissionsRequest
{
    public List<string> PermissionCodes { get; set; } = new List<string>();

    [Required(ErrorMessage = "Vui lòng cung cấp lý do (Audit Reason) cho thay đổi này.")]
    public string AuditReason { get; set; } = string.Empty;
}
