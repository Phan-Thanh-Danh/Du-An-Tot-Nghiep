using System.Text.Json;

namespace Backend.DTOs.QuizManagement;

public class QuizConfigurationDto
{
    public decimal TongDiem { get; set; } = 10;
    public decimal DiemDat { get; set; } = 5;
    public string CachTinhDat { get; set; } = "theo_diem"; // theo_diem, theo_so_cau_dung
    public int? SoCauDungToiThieu { get; set; }
    public int? SoLanLamToiDa { get; set; }
    public bool KhongGioiHanSoLan { get; set; }
    public string CachTinhDiemCuoi { get; set; } = "lay_diem_cao_nhat"; // lay_diem_cao_nhat, lay_lan_cuoi, lay_trung_binh
    public DateTime? MoLuc { get; set; }
    public DateTime? DongLuc { get; set; }
    public bool XaoTronCauHoi { get; set; }
    public bool XaoTronDapAn { get; set; }
    public bool HienKetQuaSauKhiNop { get; set; } = true;
    public bool HienDapAnDungSauKhiNop { get; set; }

    public static QuizConfigurationDto Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new QuizConfigurationDto();
        }

        try
        {
            var config = JsonSerializer.Deserialize<QuizConfigurationDto>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return config ?? new QuizConfigurationDto();
        }
        catch
        {
            // Trả về mặc định nếu JSON không hợp lệ, theo yêu cầu
            return new QuizConfigurationDto();
        }
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    public void Validate()
    {
        if (TongDiem <= 0) throw new ArgumentException("Tổng điểm phải lớn hơn 0");
        if (DiemDat < 0 || DiemDat > TongDiem) throw new ArgumentException("Điểm đạt phải từ 0 đến Tổng điểm");
        
        if (CachTinhDat != "theo_diem" && CachTinhDat != "theo_so_cau_dung")
            throw new ArgumentException("Cách tính đạt không hợp lệ");

        if (CachTinhDiemCuoi != "lay_diem_cao_nhat" && CachTinhDiemCuoi != "lay_lan_cuoi" && CachTinhDiemCuoi != "lay_trung_binh")
            throw new ArgumentException("Cách tính điểm cuối không hợp lệ");

        if (KhongGioiHanSoLan)
        {
            SoLanLamToiDa = null;
        }
        else
        {
            if (SoLanLamToiDa == null || SoLanLamToiDa <= 0)
                throw new ArgumentException("Số lần làm tối đa phải lớn hơn 0 nếu không chọn không giới hạn");
        }

        if (MoLuc.HasValue && DongLuc.HasValue && MoLuc.Value >= DongLuc.Value)
        {
            throw new ArgumentException("Thời gian mở phải trước thời gian đóng");
        }
    }
}
