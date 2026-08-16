using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.AdminUsers;
using Backend.DTOs.Auth;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Audit;
using Backend.Services.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.VisualBasic.FileIO;
using OfficeOpenXml;

namespace Backend.Services.AdminUsers;

public class UserBulkImportService : IUserBulkImportService
{
    private const long MaxFileSize = 10 * 1024 * 1024;
    private const int MaxRows = 1000;

    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly HashSet<string> AllowedRoleCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        AuthRoles.ToDatabaseCode(AuthRoles.Teacher),
        AuthRoles.ToDatabaseCode(AuthRoles.Student),
        AuthRoles.ToDatabaseCode(AuthRoles.AcademicStaff)
    };

    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IAuditLogService _auditLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserBulkImportService(
        ApplicationDbContext context,
        IPasswordHasherService passwordHasher,
        IAuditLogService auditLogService,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _auditLogService = auditLogService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<UserImportResultDto> ImportAsync(
        IFormFile file,
        bool dryRun,
        int? defaultMaDonVi,
        CancellationToken cancellationToken = default)
    {
        ValidateFile(file);
        var currentUser = GetCurrentUser();
        var rows = await ReadRowsAsync(file, cancellationToken);
        if (rows.Count == 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "File không có dòng dữ liệu nào.");
        }

        if (rows.Count > MaxRows)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, $"File chỉ được chứa tối đa {MaxRows} dòng dữ liệu.");
        }

        var roles = await _context.VaiTros.AsNoTracking().ToListAsync(cancellationToken);
        var roleByCode = roles.ToDictionary(x => x.MaCodeVaiTro, StringComparer.OrdinalIgnoreCase);
        var organizations = await _context.DonVis.AsNoTracking().ToListAsync(cancellationToken);
        var organizationById = organizations.ToDictionary(x => x.MaDonVi);
        var allowedOrganizationIds = GetAllowedOrganizationIds(currentUser, organizations);

        var normalizedEmails = rows
            .Select(x => NormalizeEmail(x.Email))
            .Where(x => x is not null)
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var existingUsers = await _context.NguoiDungs
            .Where(x => normalizedEmails.Contains(x.Email.ToLower()))
            .ToListAsync(cancellationToken);
        var existingUserByEmail = existingUsers.ToDictionary(
            x => NormalizeEmail(x.Email)!,
            StringComparer.OrdinalIgnoreCase);
        var seenEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var errors = new List<UserImportErrorDto>();
        var validRows = new List<ValidatedImportRow>();

        foreach (var row in rows)
        {
            var rowErrors = ValidateRow(
                row,
                defaultMaDonVi,
                roleByCode,
                organizations,
                organizationById,
                allowedOrganizationIds,
                existingUserByEmail,
                seenEmails,
                currentUser);

            if (rowErrors.Count > 0)
            {
                errors.AddRange(rowErrors.Select(reason => new UserImportErrorDto
                {
                    Dong = row.RowNumber,
                    Email = NormalizeEmail(row.Email) ?? row.Email?.Trim(),
                    LyDo = reason
                }));
                continue;
            }

            var roleCode = NormalizeRoleCode(row.RoleCode!);
            var email = NormalizeEmail(row.Email)!;
            var finalOrgId = ResolveOrganizationId(row.OrganizationId, row.OrganizationIdText, defaultMaDonVi, organizations, currentUser);
            validRows.Add(new ValidatedImportRow(
                row.RowNumber,
                email,
                row.FullName!.Trim(),
                row.Password!,
                roleByCode[roleCode],
                finalOrgId,
                NormalizeOptional(row.PhoneNumber),
                existingUserByEmail.GetValueOrDefault(email)));
        }

        var result = new UserImportResultDto
        {
            TongSoDong = rows.Count,
            SoDongHopLe = validRows.Count,
            SoDongLoi = rows.Count - validRows.Count,
            SoDongDaNhap = 0,
            SoDongTaoMoi = validRows.Count(x => x.ExistingUser is null),
            SoDongCapNhat = validRows.Count(x => x.ExistingUser is not null),
            DryRun = dryRun,
            DaLuu = false,
            ChiTietLoi = errors
        };

        if (dryRun || errors.Count > 0)
        {
            return result;
        }

        try
        {
            await _context.ExecuteInTransactionAsync(async () =>
            {
                var now = DateTime.UtcNow;
                foreach (var row in validRows.Where(x => x.ExistingUser is not null))
                {
                    var user = row.ExistingUser!;
                    user.Email = row.Email;
                    user.HoTen = row.FullName;
                    user.MaDonVi = row.OrganizationId;
                    user.VaiTroChinh = row.Role.MaCodeVaiTro;
                    user.SoDienThoai = row.PhoneNumber;
                    user.MatKhauHash = _passwordHasher.HashPassword(row.Password);
                    user.SoLanSaiMatKhau = 0;
                    user.DangNhapLanDau = true;
                }

                var createdPairs = validRows
                    .Where(x => x.ExistingUser is null)
                    .Select(row => new CreatedUserPair(row, new NguoiDung
                    {
                        MaDonVi = row.OrganizationId,
                        Email = row.Email,
                        HoTen = row.FullName,
                        VaiTroChinh = row.Role.MaCodeVaiTro,
                        SoDienThoai = row.PhoneNumber,
                        TrangThai = UserStatuses.DbActive,
                        MatKhauHash = _passwordHasher.HashPassword(row.Password),
                        NgayTao = now,
                        SoLanSaiMatKhau = 0,
                        DangNhapLanDau = true
                    }))
                    .ToList();

                var existingIds = validRows
                    .Where(x => x.ExistingUser is not null)
                    .Select(x => x.ExistingUser!.MaNguoiDung)
                    .ToList();
                if (existingIds.Count > 0)
                {
                    var oldAssignments = await _context.PhanQuyenNguoiDungs
                        .Where(x => existingIds.Contains(x.MaNguoiDung))
                        .ToListAsync(cancellationToken);
                    _context.PhanQuyenNguoiDungs.RemoveRange(oldAssignments);
                }

                await _context.NguoiDungs.AddRangeAsync(createdPairs.Select(x => x.User), cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                var assignments = validRows.Select(row => new PhanQuyenNguoiDung
                {
                    MaNguoiDung = row.ExistingUser?.MaNguoiDung ?? createdPairs.Single(x => ReferenceEquals(x.Row, row)).User.MaNguoiDung,
                    MaVaiTro = row.Role.MaVaiTro,
                    NgayGan = now
                });
                await _context.PhanQuyenNguoiDungs.AddRangeAsync(assignments, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                await _auditLogService.LogAsync(
                    "UserImport",
                    Guid.NewGuid().ToString("N"),
                    "BULK_IMPORT",
                    null,
                    new
                    {
                        SoLuong = validRows.Count,
                        TaoMoi = createdPairs.Count,
                        CapNhat = validRows.Count - createdPairs.Count,
                        Scope = "BghManagedUsers",
                        DonVi = validRows.Select(x => x.OrganizationId).Distinct().OrderBy(x => x).ToArray()
                    },
                    currentUser.UserId,
                    currentUser.CampusId,
                    $"Import {createdPairs.Count} tài khoản mới và cập nhật {validRows.Count - createdPairs.Count} tài khoản từ file {Path.GetFileName(file.FileName)}.",
                    cancellationToken);
            }, cancellationToken);

            result.SoDongDaNhap = validRows.Count;
            result.DaLuu = true;
            return result;
        }
        catch (DbUpdateException)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "Dữ liệu đã thay đổi trong lúc nhập. Không có tài khoản nào được lưu; vui lòng chạy dry-run lại.");
        }
    }

    private List<string> ValidateRow(
        RawImportRow row,
        int? defaultMaDonVi,
        IReadOnlyDictionary<string, VaiTro> roleByCode,
        IReadOnlyList<DonVi> organizations,
        IReadOnlyDictionary<int, DonVi> organizationById,
        IReadOnlySet<int> allowedOrganizationIds,
        IReadOnlyDictionary<string, NguoiDung> existingUserByEmail,
        ISet<string> seenEmails,
        CurrentUserContext currentUser)
    {
        var errors = new List<string>();
        var email = NormalizeEmail(row.Email);
        if (string.IsNullOrWhiteSpace(email) || email.Length > 255 || !EmailRegex.IsMatch(email))
        {
            errors.Add("Email không đúng định dạng.");
        }
        else
        {
            if (!seenEmails.Add(email))
            {
                errors.Add("Email bị trùng trong file import.");
            }
        }

        if (string.IsNullOrWhiteSpace(row.FullName))
        {
            errors.Add("Họ tên không được để trống.");
        }
        else if (row.FullName.Trim().Length > 255)
        {
            errors.Add("Họ tên không được vượt quá 255 ký tự.");
        }

        if (!string.IsNullOrWhiteSpace(row.PhoneNumber) && row.PhoneNumber.Trim().Length > 15)
        {
            errors.Add("Số điện thoại không được vượt quá 15 ký tự.");
        }

        var passwordError = _passwordHasher.GetPasswordStrengthError(row.Password ?? string.Empty);
        if (passwordError is not null)
        {
            errors.Add(passwordError);
        }

        var roleCode = NormalizeRoleCode(row.RoleCode ?? string.Empty);
        if (!roleByCode.ContainsKey(roleCode))
        {
            errors.Add($"Mã vai trò '{row.RoleCode}' không tồn tại trong hệ thống.");
        }
        else if (!AllowedRoleCodes.Contains(roleCode))
        {
            errors.Add("BGH/giáo vụ chỉ được import Giảng viên, Sinh viên hoặc Giáo vụ; không được gán BGH/Admin/SuperAdmin.");
        }

        var organizationId = ResolveOrganizationId(row.OrganizationId, row.OrganizationIdText, defaultMaDonVi, organizations, currentUser);
        if (!organizationById.TryGetValue(organizationId, out var organization) || !organization.ConHoatDong)
        {
            var validOrganizations = organizations
                .Where(x => x.ConHoatDong && allowedOrganizationIds.Contains(x.MaDonVi))
                .OrderBy(x => x.MaDonVi)
                .Select(x => $"{x.MaDonVi} - {x.TenDonVi}");
            errors.Add($"Đơn vị mã {organizationId} không hợp lệ. Mã được phép: {string.Join("; ", validOrganizations)}.");
        }
        else if (!allowedOrganizationIds.Contains(organizationId))
        {
            errors.Add($"Bạn không có quyền nhập tài khoản vào đơn vị '{organization.TenDonVi}'.");
        }
        else if (existingUserByEmail.TryGetValue(email, out var existingUser))
        {
            if (!allowedOrganizationIds.Contains(existingUser.MaDonVi))
            {
                errors.Add("Email đã tồn tại ở đơn vị ngoài phạm vi quản lý nên không thể cập nhật.");
            }
            else if (existingUser.MaDonVi != organizationId)
            {
                var existingOrgName = organizationById.TryGetValue(existingUser.MaDonVi, out var org) ? org.TenDonVi : $"Mã {existingUser.MaDonVi}";
                errors.Add($"Email này đã thuộc cơ sở '{existingOrgName}'. Không thể chuyển cơ sở qua import Excel.");
            }
        }

        return errors;
    }

    private static int ResolveOrganizationId(
        int? rowOrganizationId,
        string? rowOrganizationText,
        int? defaultOrganizationId,
        IReadOnlyList<DonVi> organizations,
        CurrentUserContext currentUser)
    {
        if (rowOrganizationId.HasValue && rowOrganizationId.Value > 0)
        {
            if (organizations.Any(x => x.MaDonVi == rowOrganizationId.Value))
            {
                return rowOrganizationId.Value;
            }
        }

        if (!string.IsNullOrWhiteSpace(rowOrganizationText))
        {
            var text = rowOrganizationText.Trim();
            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedId) && parsedId > 0)
            {
                if (organizations.Any(x => x.MaDonVi == parsedId))
                {
                    return parsedId;
                }
            }

            var clean = RemoveDiacritics(text).ToLowerInvariant();

            // 1. Nhận diện các từ viết tắt hoặc tên phổ biến
            string? aliasKeyword = clean switch
            {
                "dn" or "dong nai" or "d nai" or "dongnai" => "đồng nai",
                "hn" or "ha noi" or "hanoi" or "hn1" or "hn2" => "hà nội",
                "hcm" or "tp hcm" or "tphcm" or "sg" or "sai gon" or "ho chi minh" or "hcmc" => "hồ chí minh",
                "da nang" or "danang" or "dng" => "đà nẵng",
                "ct" or "can tho" or "cantho" => "cần thơ",
                "tn" or "tay nguyen" or "taynguyen" or "bmt" or "dak lak" => "tây nguyên",
                "hp" or "hai phong" or "haiphong" => "hải phòng",
                "qn" or "quy nhon" or "quynhon" or "binh dinh" => "quy nhơn",
                "th" or "thanh hoa" or "thanhhoa" => "thanh hóa",
                "lms" or "root" or "tong" or "tru so" or "tru so chinh" => "lms",
                _ => null
            };

            if (aliasKeyword != null)
            {
                var matched = organizations.FirstOrDefault(x =>
                    x.TenDonVi.Contains(aliasKeyword, StringComparison.OrdinalIgnoreCase) ||
                    RemoveDiacritics(x.TenDonVi).Contains(RemoveDiacritics(aliasKeyword), StringComparison.OrdinalIgnoreCase));
                if (matched != null)
                {
                    return matched.MaDonVi;
                }
            }

            // 2. Tìm kiếm gần đúng theo tên cơ sở trong hệ thống
            var byName = organizations.FirstOrDefault(x =>
                string.Equals(x.TenDonVi, text, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(RemoveDiacritics(x.TenDonVi), clean, StringComparison.OrdinalIgnoreCase) ||
                x.TenDonVi.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                RemoveDiacritics(x.TenDonVi).Contains(clean, StringComparison.OrdinalIgnoreCase) ||
                clean.Contains(RemoveDiacritics(x.TenDonVi), StringComparison.OrdinalIgnoreCase));

            if (byName is not null)
            {
                return byName.MaDonVi;
            }
        }

        // 3. Fallback lấy theo cơ sở được chọn trên giao diện combobox
        if (defaultOrganizationId.HasValue && defaultOrganizationId.Value > 0)
        {
            return defaultOrganizationId.Value;
        }

        // 4. Fallback lấy theo cơ sở của tài khoản hiện tại
        if (currentUser.CampusId > 0)
        {
            return currentUser.CampusId;
        }

        return organizations.FirstOrDefault(x => x.ConHoatDong)?.MaDonVi ?? 1;
    }

    private static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();
        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }
        return stringBuilder.ToString().Normalize(NormalizationForm.FormC).Replace('đ', 'd').Replace('Đ', 'D');
    }

    private static IReadOnlySet<int> GetAllowedOrganizationIds(
        CurrentUserContext currentUser,
        IReadOnlyList<DonVi> organizations)
    {
        var role = currentUser.Role ?? string.Empty;
        var isGlobalAdmin = string.Equals(role, AuthRoles.SuperAdmin, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, AuthRoles.Admin, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, "sieu_quan_tri", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, "quan_tri", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, "SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, AuthRoles.Principal, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, "hieu_truong", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, "bgh", StringComparison.OrdinalIgnoreCase);

        if (isGlobalAdmin || currentUser.CampusId <= 0)
        {
            return organizations.Select(x => x.MaDonVi).ToHashSet();
        }

        var allowed = new HashSet<int> { currentUser.CampusId };
        var added = true;
        while (added)
        {
            added = false;
            foreach (var organization in organizations.Where(x => x.MaDonViCha.HasValue && allowed.Contains(x.MaDonViCha.Value)))
            {
                added |= allowed.Add(organization.MaDonVi);
            }
        }
        return allowed;
    }

    private CurrentUserContext GetCurrentUser()
    {
        return _httpContextAccessor.HttpContext?.Items["CurrentUser"] as CurrentUserContext
            ?? throw new ApiException(StatusCodes.Status401Unauthorized, "Không tìm thấy thông tin người dùng hiện tại.");
    }

    private static void ValidateFile(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Vui lòng chọn file cần import.");
        }

        if (file.Length > MaxFileSize)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Dung lượng file không được vượt quá 10 MB.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension is not ".xlsx" and not ".csv")
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ hỗ trợ file .xlsx hoặc .csv.");
        }
    }

    private static async Task<List<RawImportRow>> ReadRowsAsync(IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = file.OpenReadStream();
            return Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase)
                ? ReadExcelRows(stream)
                : await ReadCsvRowsAsync(stream, cancellationToken);
        }
        catch (Exception exception) when (exception is not ApiException and not OperationCanceledException)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Không thể đọc file. Vui lòng kiểm tra định dạng .xlsx/.csv và cấu trúc cột.");
        }
    }

    private static List<RawImportRow> ReadExcelRows(Stream stream)
    {
        ExcelPackage.License.SetNonCommercialOrganization("LMS Academic Management System");
        using var package = new ExcelPackage(stream);
        var worksheet = package.Workbook.Worksheets.FirstOrDefault()
            ?? throw new ApiException(StatusCodes.Status400BadRequest, "File Excel không có worksheet.");
        if (worksheet.Dimension is null)
        {
            return [];
        }

        var headers = BuildHeaderMap(Enumerable.Range(1, worksheet.Dimension.End.Column)
            .Select(column => (worksheet.Cells[1, column].Text, column)));
        ValidateRequiredHeaders(headers);

        var rows = new List<RawImportRow>();
        for (var row = 2; row <= worksheet.Dimension.End.Row; row++)
        {
            var values = headers.ToDictionary(x => x.Key, x => worksheet.Cells[row, x.Value].Text);
            if (values.Values.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }
            rows.Add(ToRawRow(row, values));
        }
        return rows;
    }

    private static async Task<List<RawImportRow>> ReadCsvRowsAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);
        memory.Position = 0;
        using var reader = new StreamReader(memory, Encoding.UTF8, true, leaveOpen: true);
        var firstLine = await reader.ReadLineAsync(cancellationToken) ?? string.Empty;
        var delimiter = firstLine.Count(x => x == ';') > firstLine.Count(x => x == ',') ? ";" : ",";
        memory.Position = 0;

        using var parser = new TextFieldParser(memory, Encoding.UTF8, true)
        {
            TextFieldType = FieldType.Delimited,
            HasFieldsEnclosedInQuotes = true,
            TrimWhiteSpace = false
        };
        parser.SetDelimiters(delimiter);
        if (parser.EndOfData)
        {
            return [];
        }

        var headerFields = parser.ReadFields() ?? [];
        var headers = BuildHeaderMap(headerFields.Select((name, index) => (name, index)));
        ValidateRequiredHeaders(headers);

        var rows = new List<RawImportRow>();
        var rowNumber = 1;
        while (!parser.EndOfData)
        {
            cancellationToken.ThrowIfCancellationRequested();
            rowNumber++;
            var fields = parser.ReadFields() ?? [];
            if (fields.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }
            var values = headers.ToDictionary(x => x.Key, x => x.Value < fields.Length ? fields[x.Value] : string.Empty);
            rows.Add(ToRawRow(rowNumber, values));
        }
        return rows;
    }

    private static RawImportRow ToRawRow(int rowNumber, IReadOnlyDictionary<string, string> values)
    {
        var rawOrg = GetFirstValue(values, "tendonvi", "donvi", "coso", "tencoso", "madonvi", "macoso", "campus").Trim();

        int? organizationId = null;
        if (!string.IsNullOrWhiteSpace(rawOrg))
        {
            if (int.TryParse(rawOrg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedId))
            {
                organizationId = parsedId;
            }
            else if (decimal.TryParse(rawOrg, NumberStyles.Any, CultureInfo.InvariantCulture, out var decId))
            {
                organizationId = (int)decId;
            }
        }

        return new RawImportRow(
            rowNumber,
            GetValue(values, "email"),
            GetValue(values, "hoten"),
            GetValue(values, "matkhau"),
            GetValue(values, "macodevaitro"),
            organizationId,
            rawOrg,
            GetValue(values, "sodienthoai"));
    }

    private static void ValidateRequiredHeaders<T>(IReadOnlyDictionary<string, T> headers)
    {
        var required = new[] { "email", "hoten", "matkhau", "macodevaitro" };
        var missing = required.Where(x => !headers.ContainsKey(x)).ToList();
        if (missing.Count > 0)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                $"File thiếu cột bắt buộc: {string.Join(", ", missing)}. Các cột chuẩn: Email, HoTen, MatKhau, MaCodeVaiTro, MaDonVi, SoDienThoai.");
        }
    }

    private static Dictionary<string, int> BuildHeaderMap(IEnumerable<(string Name, int Index)> columns)
    {
        var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var column in columns)
        {
            var normalized = NormalizeHeader(column.Name);
            if (string.IsNullOrEmpty(normalized))
            {
                continue;
            }
            if (!headers.TryAdd(normalized, column.Index))
            {
                throw new ApiException(StatusCodes.Status400BadRequest, $"File có cột bị trùng: {column.Name}.");
            }
        }
        return headers;
    }

    private static string GetValue(IReadOnlyDictionary<string, string> values, string key)
        => values.TryGetValue(key, out var value) ? value : string.Empty;

    private static string GetFirstValue(IReadOnlyDictionary<string, string> values, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }
        return string.Empty;
    }

    private static string NormalizeHeader(string value)
    {
        var decomposed = value.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark && char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToLowerInvariant(character));
            }
        }
        return builder.ToString();
    }

    private static string NormalizeRoleCode(string value)
    {
        var trimmed = value.Trim().ToLowerInvariant();
        return trimmed switch
        {
            "teacher" or "giang_vien" or "giao_vien" or "giangvien" or "giaovien" => AuthRoles.ToDatabaseCode(AuthRoles.Teacher),
            "student" or "sinh_vien" or "hoc_sinh" or "sinhvien" or "hocsinh" => AuthRoles.ToDatabaseCode(AuthRoles.Student),
            "staff" or "academicstaff" or "academic_staff" or "giao_vu" or "nhan_vien" or "giaovu" or "nhanvien" => AuthRoles.ToDatabaseCode(AuthRoles.AcademicStaff),
            "principal" or "bgh" or "ban_giam_hieu" or "hieu_truong" or "bangiamhieu" or "hieutruong" => AuthRoles.ToDatabaseCode(AuthRoles.Principal),
            "admin" or "quan_tri" or "quantri" => AuthRoles.ToDatabaseCode(AuthRoles.Admin),
            "superadmin" or "super_admin" or "sieu_quan_tri" or "sieuquantri" => AuthRoles.ToDatabaseCode(AuthRoles.SuperAdmin),
            _ => AuthRoles.IsKnownDatabaseCode(trimmed) ? trimmed : AuthRoles.ToDatabaseCode(trimmed)
        };
    }

    private static string? NormalizeEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var trimmed = value.Trim();
        if (trimmed.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed.Substring(7).Trim();
        }
        trimmed = trimmed.Trim('"', '\'', ' ', '\t', '\r', '\n');
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed.ToLowerInvariant();
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed record RawImportRow(
        int RowNumber,
        string? Email,
        string? FullName,
        string? Password,
        string? RoleCode,
        int? OrganizationId,
        string? OrganizationIdText,
        string? PhoneNumber);

    private sealed record ValidatedImportRow(
        int RowNumber,
        string Email,
        string FullName,
        string Password,
        VaiTro Role,
        int OrganizationId,
        string? PhoneNumber,
        NguoiDung? ExistingUser);

    private sealed record CreatedUserPair(ValidatedImportRow Row, NguoiDung User);
}
