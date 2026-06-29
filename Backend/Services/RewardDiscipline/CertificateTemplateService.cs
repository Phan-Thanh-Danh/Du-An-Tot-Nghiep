using System.Text.Json;
using System.Text.RegularExpressions;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.RewardDiscipline;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.RewardDiscipline;

public partial class CertificateTemplateService : ICertificateTemplateService
{
    private const string EntityType = "MauBangKhen";
    private const string CreateAction = "CREATE_CERTIFICATE_TEMPLATE";
    private const string UpdateAction = "UPDATE_CERTIFICATE_TEMPLATE";
    private const string DisableAction = "DISABLE_CERTIFICATE_TEMPLATE";

    private static readonly HashSet<string> AllowedFieldKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "hoTen",
        "mssv",
        "tenHocKy",
        "danhHieu",
        "xepHang",
        "diemXet",
        "ngayCap"
    };

    private static readonly HashSet<string> AllowedFieldProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "key",
        "x",
        "y",
        "fontSize",
        "align",
        "color",
        "bold"
    };

    private static readonly HashSet<string> AllowedAlignments = new(StringComparer.OrdinalIgnoreCase)
    {
        "left",
        "center",
        "right"
    };

    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditLogService _auditLogService;

    public CertificateTemplateService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IAuditLogService auditLogService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _auditLogService = auditLogService;
    }

    public async Task<PagedResultDto<CertificateTemplateDto>> GetAsync(
        CertificateTemplateQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin(GetCurrentUser());

        var pageIndex = Math.Max(1, parameters.PageIndex);
        var pageSize = Math.Clamp(parameters.PageSize, 1, 100);
        var query = _context.MauBangKhens.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(parameters.LoaiMau))
        {
            var type = NormalizeTemplateType(parameters.LoaiMau);
            query = query.Where(x => x.LoaiMau == type);
        }

        if (parameters.ConHoatDong.HasValue)
        {
            query = query.Where(x => x.ConHoatDong == parameters.ConHoatDong.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var keyword = parameters.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x => x.TenMau.ToLower().Contains(keyword));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await ProjectTemplate(query)
            .OrderByDescending(x => x.Template.NgayTao)
            .ThenByDescending(x => x.Template.MaMauBangKhen)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(x => ToDto(x.Template, x.CreatorName))
            .ToListAsync(cancellationToken);

        return new PagedResultDto<CertificateTemplateDto>
        {
            Items = items,
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<CertificateTemplateDto> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin(GetCurrentUser());
        var row = await ProjectTemplate(_context.MauBangKhens.AsNoTracking().Where(x => x.MaMauBangKhen == id))
            .FirstOrDefaultAsync(cancellationToken);
        if (row is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy mẫu bằng khen.");
        }

        return ToDto(row.Template, row.CreatorName);
    }

    public async Task<CertificateTemplateDto> CreateAsync(
        CreateCertificateTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureSuperAdmin(currentUser);

        var template = new MauBangKhen
        {
            TenMau = NormalizeRequiredText(request.TenMau, "Tên mẫu", 200),
            LoaiMau = NormalizeTemplateType(request.LoaiMau),
            FileNenUrl = NormalizeFileUrl(request.FileNenUrl),
            ChieuRong = NormalizePositiveInt(request.ChieuRong, "Chiều rộng"),
            ChieuCao = NormalizePositiveInt(request.ChieuCao, "Chiều cao"),
            HuongGiay = NormalizePaperOrientation(request.HuongGiay),
            CauHinhJson = NormalizeConfigJson(request.CauHinhJson),
            ConHoatDong = true,
            NguoiTao = currentUser.UserId,
            NgayTao = DateTime.UtcNow
        };

        _context.MauBangKhens.Add(template);
        await _context.SaveChangesAsync(cancellationToken);
        await _auditLogService.LogAsync(
            EntityType,
            template.MaMauBangKhen.ToString(),
            CreateAction,
            null,
            CreateAuditSnapshot(template),
            currentUser.UserId,
            null,
            "Tạo mẫu bằng khen.",
            cancellationToken);

        return await LoadDtoAsync(template.MaMauBangKhen, cancellationToken);
    }

    public async Task<CertificateTemplateDto> UpdateAsync(
        int id,
        UpdateCertificateTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureSuperAdmin(currentUser);

        var template = await _context.MauBangKhens.FirstOrDefaultAsync(x => x.MaMauBangKhen == id, cancellationToken);
        if (template is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy mẫu bằng khen.");
        }

        if (!template.ConHoatDong)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Chỉ được cập nhật mẫu bằng khen đang hoạt động.");
        }

        var oldSnapshot = CreateAuditSnapshot(template);
        template.TenMau = NormalizeRequiredText(request.TenMau, "Tên mẫu", 200);
        template.LoaiMau = NormalizeTemplateType(request.LoaiMau);
        template.FileNenUrl = NormalizeFileUrl(request.FileNenUrl);
        template.ChieuRong = NormalizePositiveInt(request.ChieuRong, "Chiều rộng");
        template.ChieuCao = NormalizePositiveInt(request.ChieuCao, "Chiều cao");
        template.HuongGiay = NormalizePaperOrientation(request.HuongGiay);
        template.CauHinhJson = NormalizeConfigJson(request.CauHinhJson);
        template.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        await _auditLogService.LogAsync(
            EntityType,
            template.MaMauBangKhen.ToString(),
            UpdateAction,
            oldSnapshot,
            CreateAuditSnapshot(template),
            currentUser.UserId,
            null,
            "Cập nhật mẫu bằng khen.",
            cancellationToken);

        return await LoadDtoAsync(template.MaMauBangKhen, cancellationToken);
    }

    public async Task<CertificateTemplateDto> DisableAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureSuperAdmin(currentUser);

        var template = await _context.MauBangKhens.FirstOrDefaultAsync(x => x.MaMauBangKhen == id, cancellationToken);
        if (template is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy mẫu bằng khen.");
        }

        if (!template.ConHoatDong)
        {
            return await LoadDtoAsync(template.MaMauBangKhen, cancellationToken);
        }

        var oldSnapshot = CreateAuditSnapshot(template);
        template.ConHoatDong = false;
        template.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        await _auditLogService.LogAsync(
            EntityType,
            template.MaMauBangKhen.ToString(),
            DisableAction,
            oldSnapshot,
            CreateAuditSnapshot(template),
            currentUser.UserId,
            null,
            "Vô hiệu hóa mẫu bằng khen.",
            cancellationToken);

        return await LoadDtoAsync(template.MaMauBangKhen, cancellationToken);
    }

    public async Task<CertificateTemplatePreviewDto> PreviewAsync(
        int id,
        CertificateTemplatePreviewRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin(GetCurrentUser());

        var template = await _context.MauBangKhens.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaMauBangKhen == id, cancellationToken);
        if (template is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy mẫu bằng khen.");
        }

        var data = await ResolvePreviewDataAsync(request, cancellationToken);
        ApplySampleOverrides(data, request.DuLieuMau);
        var fields = ParseConfigFields(template.CauHinhJson)
            .Select(field => new CertificateTemplatePreviewFieldDto
            {
                Key = field.Key,
                Value = data.TryGetValue(field.Key, out var value) ? value : null,
                X = field.X,
                Y = field.Y,
                FontSize = field.FontSize,
                Align = field.Align,
                Color = field.Color,
                Bold = field.Bold
            })
            .ToList();

        return new CertificateTemplatePreviewDto
        {
            MaMauBangKhen = template.MaMauBangKhen,
            TenMau = template.TenMau,
            LoaiMau = template.LoaiMau,
            FileNenUrl = template.FileNenUrl,
            ChieuRong = template.ChieuRong,
            ChieuCao = template.ChieuCao,
            HuongGiay = template.HuongGiay,
            Fields = fields,
            Data = data,
            IsPdfGenerated = false,
            Note = "RD5 chỉ trả payload preview an toàn; sinh PDF bằng khen sẽ thực hiện ở RD6."
        };
    }

    private async Task<CertificateTemplateDto> LoadDtoAsync(int id, CancellationToken cancellationToken)
    {
        var row = await ProjectTemplate(_context.MauBangKhens.AsNoTracking().Where(x => x.MaMauBangKhen == id))
            .FirstAsync(cancellationToken);
        return ToDto(row.Template, row.CreatorName);
    }

    private IQueryable<TemplateQueryRow> ProjectTemplate(IQueryable<MauBangKhen> query)
    {
        return
            from template in query
            join creator in _context.NguoiDungs.AsNoTracking()
                on template.NguoiTao equals creator.MaNguoiDung into creatorGroup
            from creator in creatorGroup.DefaultIfEmpty()
            select new TemplateQueryRow
            {
                Template = template,
                CreatorName = creator != null ? creator.HoTen : null
            };
    }

    private async Task<Dictionary<string, string?>> ResolvePreviewDataAsync(
        CertificateTemplatePreviewRequest request,
        CancellationToken cancellationToken)
    {
        if (!request.MaKhenThuong.HasValue)
        {
            return CreateDefaultPreviewData();
        }

        var row = await _context.KhenThuongs.AsNoTracking()
            .Where(x => x.MaKhenThuong == request.MaKhenThuong.Value)
            .Select(x => new
            {
                Reward = x,
                StudentName = x.HocSinh != null ? x.HocSinh.HoTen : null,
                StudentCode = x.HocSinh != null ? x.HocSinh.Email : null,
                TermName = x.HocKy != null ? x.HocKy.TenHocKy : null
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (row is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy khen thưởng để preview.");
        }

        return new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["hoTen"] = FirstNonBlank(row.Reward.HoTenSnapshot, row.StudentName, "Nguyễn Văn A"),
            ["mssv"] = FirstNonBlank(row.Reward.MssvSnapshot, row.StudentCode, "SV000001"),
            ["tenHocKy"] = FirstNonBlank(row.Reward.TenHocKySnapshot, row.TermName, "Học kỳ mẫu"),
            ["danhHieu"] = FirstNonBlank(row.Reward.DanhHieuSnapshot, "Top 100 học kỳ"),
            ["xepHang"] = row.Reward.XepHang?.ToString(),
            ["diemXet"] = row.Reward.DiemXet?.ToString("0.##"),
            ["ngayCap"] = row.Reward.CapLuc.ToString("yyyy-MM-dd")
        };
    }

    private static void ApplySampleOverrides(Dictionary<string, string?> data, JsonElement? sampleData)
    {
        if (!sampleData.HasValue || sampleData.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return;
        }

        if (sampleData.Value.ValueKind != JsonValueKind.Object)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Dữ liệu mẫu preview phải là JSON object.");
        }

        foreach (var property in sampleData.Value.EnumerateObject())
        {
            if (!AllowedFieldKeys.Contains(property.Name))
            {
                throw new ApiException(StatusCodes.Status400BadRequest, $"Dữ liệu mẫu chứa field không được hỗ trợ: {property.Name}.");
            }

            data[property.Name] = property.Value.ValueKind switch
            {
                JsonValueKind.String => property.Value.GetString(),
                JsonValueKind.Number => property.Value.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => null,
                _ => throw new ApiException(StatusCodes.Status400BadRequest, "Dữ liệu mẫu preview chỉ nhận string, number, boolean hoặc null.")
            };
        }
    }

    private static Dictionary<string, string?> CreateDefaultPreviewData()
    {
        return new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["hoTen"] = "Nguyễn Văn A",
            ["mssv"] = "SV000001",
            ["tenHocKy"] = "Học kỳ 1 năm học 2026-2027",
            ["danhHieu"] = "Top 100 học kỳ",
            ["xepHang"] = "1",
            ["diemXet"] = "9.25",
            ["ngayCap"] = DateTime.UtcNow.ToString("yyyy-MM-dd")
        };
    }

    private static string NormalizeConfigJson(JsonElement? config)
    {
        if (!config.HasValue || config.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Cấu hình mẫu bằng khen là bắt buộc.");
        }

        if (config.Value.ValueKind == JsonValueKind.String)
        {
            var raw = config.Value.GetString();
            if (string.IsNullOrWhiteSpace(raw))
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Cấu hình mẫu bằng khen là bắt buộc.");
            }

            try
            {
                using var document = JsonDocument.Parse(raw);
                ValidateConfigRoot(document.RootElement);
                return document.RootElement.GetRawText();
            }
            catch (JsonException)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Cấu hình mẫu bằng khen không phải JSON hợp lệ.");
            }
        }

        ValidateConfigRoot(config.Value);
        return config.Value.GetRawText();
    }

    private static IReadOnlyList<TemplateField> ParseConfigFields(string? configJson)
    {
        try
        {
            using var document = JsonDocument.Parse(configJson ?? string.Empty);
            ValidateConfigRoot(document.RootElement);
            return ReadFields(document.RootElement).ToList();
        }
        catch (JsonException)
        {
            throw new ApiException(StatusCodes.Status500InternalServerError, "Cấu hình mẫu bằng khen không hợp lệ.");
        }
    }

    private static void ValidateConfigRoot(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Cấu hình mẫu bằng khen phải là JSON object.");
        }

        if (!TryGetPropertyIgnoreCase(root, "fields", out var fields) ||
            fields.ValueKind != JsonValueKind.Array)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Cấu hình mẫu bằng khen phải có fields dạng array.");
        }

        var count = fields.GetArrayLength();
        if (count is < 1 or > 50)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Cấu hình fields phải có từ 1 đến 50 phần tử.");
        }

        _ = ReadFields(root).ToList();
    }

    private static IEnumerable<TemplateField> ReadFields(JsonElement root)
    {
        var fields = GetPropertyIgnoreCase(root, "fields");
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var field in fields.EnumerateArray())
        {
            if (field.ValueKind != JsonValueKind.Object)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Mỗi field trong cấu hình phải là JSON object.");
            }

            foreach (var property in field.EnumerateObject())
            {
                if (!AllowedFieldProperties.Contains(property.Name))
                {
                    throw new ApiException(StatusCodes.Status400BadRequest, $"Field cấu hình chứa thuộc tính không được hỗ trợ: {property.Name}.");
                }
            }

            var key = GetRequiredString(field, "key", "Field key không được để trống.");
            if (!AllowedFieldKeys.Contains(key))
            {
                throw new ApiException(StatusCodes.Status400BadRequest, $"Field key không được hỗ trợ: {key}.");
            }

            if (!keys.Add(key))
            {
                throw new ApiException(StatusCodes.Status400BadRequest, $"Field key bị trùng: {key}.");
            }

            var align = GetRequiredString(field, "align", "Field align không được để trống.").ToLowerInvariant();
            if (!AllowedAlignments.Contains(align))
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Field align chỉ nhận left, center hoặc right.");
            }

            var color = GetRequiredString(field, "color", "Field color không được để trống.");
            if (!HexColorRegex().IsMatch(color))
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Field color phải là mã màu hex dạng #RRGGBB.");
            }

            yield return new TemplateField(
                key,
                GetRequiredDecimal(field, "x", "Field x phải là số không âm.", minInclusive: 0, maxInclusive: 100000),
                GetRequiredDecimal(field, "y", "Field y phải là số không âm.", minInclusive: 0, maxInclusive: 100000),
                GetRequiredDecimal(field, "fontSize", "Field fontSize phải từ 1 đến 200.", minInclusive: 1, maxInclusive: 200),
                align,
                color,
                GetRequiredBoolean(field, "bold", "Field bold phải là boolean."));
        }
    }

    private static JsonElement GetPropertyIgnoreCase(JsonElement element, string propertyName)
    {
        if (TryGetPropertyIgnoreCase(element, propertyName, out var property))
        {
            return property;
        }

        throw new ApiException(StatusCodes.Status400BadRequest, $"Thiếu thuộc tính {propertyName}.");
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string propertyName, out JsonElement property)
    {
        foreach (var candidate in element.EnumerateObject())
        {
            if (string.Equals(candidate.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                property = candidate.Value;
                return true;
            }
        }

        property = default;
        return false;
    }

    private static string GetRequiredString(JsonElement element, string propertyName, string message)
    {
        var property = GetPropertyIgnoreCase(element, propertyName);
        if (property.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(property.GetString()))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, message);
        }

        return property.GetString()!.Trim();
    }

    private static decimal GetRequiredDecimal(
        JsonElement element,
        string propertyName,
        string message,
        decimal minInclusive,
        decimal maxInclusive)
    {
        var property = GetPropertyIgnoreCase(element, propertyName);
        if (property.ValueKind != JsonValueKind.Number || !property.TryGetDecimal(out var value) ||
            value < minInclusive ||
            value > maxInclusive)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, message);
        }

        return value;
    }

    private static bool GetRequiredBoolean(JsonElement element, string propertyName, string message)
    {
        var property = GetPropertyIgnoreCase(element, propertyName);
        return property.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => throw new ApiException(StatusCodes.Status400BadRequest, message)
        };
    }

    private static string NormalizeTemplateType(string value)
    {
        var normalized = NormalizeRequiredText(value, "Loại mẫu", 50);
        var canonical = RewardDisciplineConstants.CertificateTemplateTypes.All
            .FirstOrDefault(x => x.Equals(normalized, StringComparison.OrdinalIgnoreCase));
        if (canonical is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Loại mẫu bằng khen không hợp lệ.");
        }

        return canonical;
    }

    private static string NormalizePaperOrientation(string value)
    {
        var normalized = NormalizeRequiredText(value, "Hướng giấy", 20);
        var canonical = RewardDisciplineConstants.PaperOrientations.All
            .FirstOrDefault(x => x.Equals(normalized, StringComparison.OrdinalIgnoreCase));
        if (canonical is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Hướng giấy không hợp lệ.");
        }

        return canonical;
    }

    private static string NormalizeFileUrl(string value)
    {
        var normalized = NormalizeRequiredText(value, "File nền", 1000);
        if (normalized.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains(";base64", StringComparison.OrdinalIgnoreCase))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "File nền không được là base64.");
        }

        if (normalized.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase) ||
            normalized.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "File nền chỉ nhận URL/path an toàn.");
        }

        if (Uri.TryCreate(normalized, UriKind.Absolute, out var uri) &&
            uri.Scheme is not ("http" or "https"))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "File nền chỉ nhận URL http/https hoặc path nội bộ.");
        }

        return normalized;
    }

    private static int NormalizePositiveInt(int value, string fieldName)
    {
        if (value <= 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, $"{fieldName} phải lớn hơn 0.");
        }

        return value;
    }

    private static string NormalizeRequiredText(string value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, $"{fieldName} không được để trống.");
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, $"{fieldName} không được vượt quá {maxLength} ký tự.");
        }

        return normalized;
    }

    private CurrentUserContext GetCurrentUser()
    {
        var currentUser = _httpContextAccessor.HttpContext?.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser is null)
        {
            throw new ApiException(StatusCodes.Status401Unauthorized, "Bạn cần đăng nhập để thực hiện thao tác này.");
        }

        return currentUser;
    }

    private static void EnsureSuperAdmin(CurrentUserContext currentUser)
    {
        if (currentUser.Role != AuthRoles.SuperAdmin)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ SuperAdmin được quản lý mẫu bằng khen.");
        }
    }

    private static CertificateTemplateDto ToDto(MauBangKhen template, string? creatorName)
    {
        return new CertificateTemplateDto
        {
            MaMauBangKhen = template.MaMauBangKhen,
            TenMau = template.TenMau,
            LoaiMau = template.LoaiMau,
            FileNenUrl = template.FileNenUrl,
            ChieuRong = template.ChieuRong,
            ChieuCao = template.ChieuCao,
            HuongGiay = template.HuongGiay,
            CauHinhJson = template.CauHinhJson ?? string.Empty,
            ConHoatDong = template.ConHoatDong,
            NguoiTao = template.NguoiTao,
            TenNguoiTao = creatorName,
            NgayTao = template.NgayTao,
            NgayCapNhat = template.NgayCapNhat
        };
    }

    private static object CreateAuditSnapshot(MauBangKhen template)
    {
        return new
        {
            template.MaMauBangKhen,
            template.TenMau,
            template.LoaiMau,
            template.FileNenUrl,
            template.ChieuRong,
            template.ChieuCao,
            template.HuongGiay,
            template.ConHoatDong,
            template.NguoiTao,
            template.NgayTao,
            template.NgayCapNhat,
            HasCauHinhJson = !string.IsNullOrWhiteSpace(template.CauHinhJson),
            CauHinhJsonLength = template.CauHinhJson?.Length ?? 0
        };
    }

    private static string? FirstNonBlank(params string?[] values)
    {
        return values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
    }

    [GeneratedRegex("^#[0-9A-Fa-f]{6}$")]
    private static partial Regex HexColorRegex();

    private sealed record TemplateField(
        string Key,
        decimal X,
        decimal Y,
        decimal FontSize,
        string Align,
        string Color,
        bool Bold);

    private sealed class TemplateQueryRow
    {
        public MauBangKhen Template { get; init; } = null!;
        public string? CreatorName { get; init; }
    }
}
