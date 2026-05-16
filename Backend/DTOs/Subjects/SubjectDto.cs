namespace Backend.DTOs.Subjects;

public class SubjectDto
{
    public int MaMonHoc { get; set; }
    public string MaCodeMonHoc { get; set; } = string.Empty;
    public string TenMonHoc { get; set; } = string.Empty;
    public int SoTinChi { get; set; }
    public bool ConHoatDong { get; set; }
}
