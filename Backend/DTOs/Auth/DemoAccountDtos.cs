namespace Backend.DTOs.Auth;

public class DemoAccountFiltersDto
{
    public List<DemoFilterItemDto> Roles { get; set; } = new();
    public List<DemoFilterItemDto> Campuses { get; set; } = new();
}

public class DemoFilterItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public class DemoAccountQueryParameters
{
    public string? Search { get; set; }
    public string? Role { get; set; }
    public string? Campus { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class DemoAccountItemDto
{
    public int MaNguoiDung { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = "123456";
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string Campus { get; set; } = string.Empty;
    public string CampusName { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
}

public class DemoAccountPagedResultDto
{
    public List<DemoAccountItemDto> Items { get; set; } = new();
    public int TotalItems { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalItems / PageSize) : 0;
}

