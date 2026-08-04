namespace Backend.DTOs.Auth;

public class AuthUserDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int CampusId { get; set; }
    public string CampusName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string MajorName { get; set; } = string.Empty;
}
