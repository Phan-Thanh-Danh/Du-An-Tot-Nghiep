using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("MonHocChuyenNganh")]
public class MonHocChuyenNganh
{
    public int MaMonHoc { get; set; }
    public int MaChuyenNganh { get; set; }

    public DanhMucMonHoc? MonHoc { get; set; }
    public ChuyenNganh? ChuyenNganh { get; set; }
}
