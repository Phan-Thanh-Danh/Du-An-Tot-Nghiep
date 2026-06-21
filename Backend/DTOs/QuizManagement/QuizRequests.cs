using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.QuizManagement;

public class CreateQuizRequest
{
    [Required]
    public int MaMonHoc { get; set; }
    public int? MaHocKy { get; set; }
    [Required(AllowEmptyStrings = false)]
    public string TieuDe { get; set; } = string.Empty;
    [Range(1, int.MaxValue)]
    public int ThoiGianPhut { get; set; }
    public string? LoaiDeThi { get; set; }
    public string? HinhThucThi { get; set; }
    
    [Range(0, 100)]
    public decimal? TyLeTracNghiem { get; set; }
    
    [Range(0, 100)]
    public decimal? TyLeTuLuan { get; set; }

    public QuizConfigurationDto CauHinh { get; set; } = new();
}

public class UpdateQuizRequest : CreateQuizRequest
{
    // Same as Create
}

public class QuizQuestionInputDto
{
    [Required]
    public int MaCauHoi { get; set; }
    
    [Range(0.01, double.MaxValue, ErrorMessage = "Điểm số phải lớn hơn 0")]
    public decimal DiemSo { get; set; }
    
    [Range(1, int.MaxValue)]
    public int ThuTu { get; set; }
}

public class AssignQuizQuestionsRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "Danh sách câu hỏi không được rỗng")]
    public List<QuizQuestionInputDto> Questions { get; set; } = new();
}

public class UpdateQuizQuestionRequest
{
    [Range(0.01, double.MaxValue, ErrorMessage = "Điểm số phải lớn hơn 0")]
    public decimal DiemSo { get; set; }
    
    [Range(1, int.MaxValue)]
    public int ThuTu { get; set; }
}

public class QuizQuestionReorderInputDto
{
    [Required]
    public int MaCauHoi { get; set; }
    
    [Range(1, int.MaxValue)]
    public int ThuTu { get; set; }
}

public class ReorderQuizQuestionsRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "Danh sách câu hỏi không được rỗng")]
    public List<QuizQuestionReorderInputDto> Items { get; set; } = new();
}
