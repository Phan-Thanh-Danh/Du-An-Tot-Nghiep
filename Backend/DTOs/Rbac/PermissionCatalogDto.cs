using System.Collections.Generic;

namespace Backend.DTOs.Rbac;

public class PermissionItemDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class ModulePermissionsDto
{
    public string ModuleKey { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public List<PermissionItemDto> Permissions { get; set; } = new();
}

public class RolePermissionsDto
{
    public int RoleId { get; set; }
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public List<string> PermissionCodes { get; set; } = new();
}

public class UpdateRolePermissionsDto
{
    public List<string> PermissionCodes { get; set; } = new();
}
