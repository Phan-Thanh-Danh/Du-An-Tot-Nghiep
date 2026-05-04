namespace Backend.DTOs.Auth;

public class CurrentUserContext
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int CampusId { get; set; }
    public string Status { get; set; } = string.Empty;
}
