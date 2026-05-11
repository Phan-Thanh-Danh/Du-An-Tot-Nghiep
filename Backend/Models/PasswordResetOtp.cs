namespace Backend.Models;

public class PasswordResetOtp
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
    public DateTime ExpiredAt { get; set; }
    public bool IsVerified { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
