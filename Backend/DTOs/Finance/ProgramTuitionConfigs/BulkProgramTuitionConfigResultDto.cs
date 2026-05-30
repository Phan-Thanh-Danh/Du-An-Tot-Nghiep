namespace Backend.DTOs.Finance.ProgramTuitionConfigs;

public class BulkProgramTuitionConfigResultDto
{
    public int TongSoDongYeuCau { get; set; }
    public int SoDongTaoMoi { get; set; }
    public int SoDongThayTheCapNhat { get; set; }
    public int SoDongBoQua { get; set; }
    public IReadOnlyList<ProgramTuitionConfigDetailDto> Items { get; set; } = [];
}
