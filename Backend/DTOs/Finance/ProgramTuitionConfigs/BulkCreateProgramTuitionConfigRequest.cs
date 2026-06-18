using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Finance.ProgramTuitionConfigs;

public class BulkCreateProgramTuitionConfigRequest
{
    public List<CreateProgramTuitionConfigRequest> Items { get; set; } = [];

    [Range(1, int.MaxValue, ErrorMessage = "Mã đơn vị không hợp lệ.")]
    public int MaDonVi { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Mã chương trình đào tạo không hợp lệ.")]
    public int MaChuongTrinhDaoTao { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Số năm đào tạo phải lớn hơn hoặc bằng 1.")]
    public int SoNamDaoTao { get; set; }

    [Range(1, 3, ErrorMessage = "Số học kỳ mỗi năm trong MVP chỉ được từ 1 đến 3.")]
    public int SoHocKyMoiNam { get; set; }

    public string? LoaiCachTinhHocPhi { get; set; }
    public bool ConfirmReplace { get; set; }
    public List<BulkProgramTuitionConfigItemRequest> Configs { get; set; } = [];
}
