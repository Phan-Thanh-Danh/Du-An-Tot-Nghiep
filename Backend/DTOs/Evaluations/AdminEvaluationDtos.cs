namespace Backend.DTOs.Evaluations;

public class EvaluationQuestionDto
{
    public int MaCauHoiDg { get; set; }
    public string NoiDungCauHoi { get; set; } = string.Empty;
    public bool ConHoatDong { get; set; }
    public int LuotSuDung { get; set; }
}

public class CreateEvaluationQuestionRequest
{
    public string NoiDungCauHoi { get; set; } = string.Empty;
}

public class UpdateEvaluationQuestionRequest
{
    public string NoiDungCauHoi { get; set; } = string.Empty;
}

public class EvaluationConfigSummaryDto
{
    public int TongCauHoi { get; set; }
    public int CauHoiHoatDong { get; set; }
    public int TongLuotDanhGia { get; set; }
    public int SoGiaoVienDuocDanhGia { get; set; }
    public int SoHocKyCoDanhGia { get; set; }
}

public class EvaluationConfigDto
{
    public int MaMauDanhGia { get; set; }
    public string TenMau { get; set; } = string.Empty;
    public string CauHinhJson { get; set; } = string.Empty;
    public bool DangHoatDong { get; set; }
    public DateTime NgayCapNhat { get; set; }
}

public class UpdateEvaluationConfigRequest
{
    public string TenMau { get; set; } = string.Empty;
    public string CauHinhJson { get; set; } = string.Empty;
    public bool DangHoatDong { get; set; }
}
