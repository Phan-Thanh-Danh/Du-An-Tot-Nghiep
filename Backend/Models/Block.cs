using System;

namespace Backend.Models;

public class Block
{
    public int MaBlock { get; set; }
    public string TenBlock { get; set; } = string.Empty;
    public int MaHocKy { get; set; }
    public DateOnly NgayBatDau { get; set; }
    public DateOnly NgayKetThuc { get; set; }

    public HocKy? HocKy { get; set; }
}
