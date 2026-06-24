namespace Backend.DTOs.Applications;

public class ApplicationEvidenceUploadResponseDto
{
    public IReadOnlyList<StudentApplicationAttachmentDto> UploadedFiles { get; set; } = [];
    public string RowVersion { get; set; } = string.Empty;
    public int ActiveFileCount { get; set; }
    public long TotalSizeBytes { get; set; }
}

public class ApplicationEvidenceDeleteResponseDto
{
    public int MaTep { get; set; }
    public string RowVersion { get; set; } = string.Empty;
    public int ActiveFileCount { get; set; }
    public long TotalSizeBytes { get; set; }
}

public class ApplicationEvidenceDownloadDto
{
    public Stream Content { get; set; } = Stream.Null;
    public long ContentLength { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}
