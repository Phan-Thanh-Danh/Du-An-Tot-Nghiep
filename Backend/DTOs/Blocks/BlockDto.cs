namespace Backend.DTOs.Blocks;

public class BlockDto
{
    public int MaBlock { get; set; }
    public int ThuTuBlock { get; set; }
    public DateOnly NgayBatDau { get; set; }
    public DateOnly NgayKetThuc { get; set; }
    public int MaHocKy { get; set; }
}
