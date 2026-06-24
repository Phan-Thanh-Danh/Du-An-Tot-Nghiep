using System.Text.Json;

namespace Backend.DTOs.Applications;

public class CreateStudentApplicationRequest
{
    public string LoaiDon { get; set; } = string.Empty;
    public string? TieuDe { get; set; }
    public JsonElement? DuLieuBieuMau { get; set; }
}

public class UpdateStudentApplicationRequest
{
    public string? TieuDe { get; set; }
    public JsonElement DuLieuBieuMau { get; set; }
    public string RowVersion { get; set; } = string.Empty;
}

public class SubmitStudentApplicationRequest
{
    public string RowVersion { get; set; } = string.Empty;
}

public class ResubmitStudentApplicationRequest
{
    public string RowVersion { get; set; } = string.Empty;
}

public class CancelStudentApplicationRequest
{
    public string RowVersion { get; set; } = string.Empty;
    public string? LyDo { get; set; }
}
