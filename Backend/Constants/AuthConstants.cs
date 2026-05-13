namespace Backend.Constants;

public static class AuthRoles
{
    public const string Admin = "Admin";
    public const string Student = "Student";
    public const string Teacher = "Teacher";
    public const string AcademicStaff = "AcademicStaff";
    public const string Principal = "Principal";
    public const string Parent = "Parent";
    public const string SuperAdmin = "SuperAdmin";
    public const string CampusAdmin = "CampusAdmin";
    public const string SubCampusAdmin = "SubCampusAdmin";

    public static string FromDatabaseCode(string roleCode)
    {
        return roleCode switch
        {
            "quan_tri" => Admin,
            "hoc_sinh" => Student,
            "giao_vien" => Teacher,
            "nhan_vien" => AcademicStaff,
            "hieu_truong" => Principal,
            "phu_huynh" => Parent,
            "sieu_quan_tri" => SuperAdmin,
            "quan_tri_co_so" => CampusAdmin,
            "quan_tri_co_so_con" => SubCampusAdmin,
            _ => roleCode
        };
    }

    public static string ToDatabaseCode(string role)
    {
        return role switch
        {
            Admin => "quan_tri",
            Student => "hoc_sinh",
            Teacher => "giao_vien",
            AcademicStaff => "nhan_vien",
            Principal => "hieu_truong",
            Parent => "phu_huynh",
            SuperAdmin => "sieu_quan_tri",
            CampusAdmin => "quan_tri_co_so",
            SubCampusAdmin => "quan_tri_co_so_con",
            _ => role
        };
    }

    public static readonly IReadOnlySet<string> DatabaseRoleCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "quan_tri",
        "hoc_sinh",
        "giao_vien",
        "nhan_vien",
        "hieu_truong",
        "phu_huynh",
        "sieu_quan_tri",
        "quan_tri_co_so",
        "quan_tri_co_so_con"
    };

    public static bool IsKnownDatabaseCode(string roleCode)
    {
        return DatabaseRoleCodes.Contains(roleCode);
    }
}

public static class UserStatuses
{
    public const string Active = "Active";
    public const string Locked = "Locked";
    public const string FirstLogin = "FirstLogin";

    public const string DbActive = "hoat_dong";
    public const string DbLocked = "bi_khoa";
    public const string DbFirstLogin = "dang_nhap_lan_dau";

    public static string FromDatabaseStatus(string status, bool isFirstLogin)
    {
        if (status == DbLocked)
        {
            return Locked;
        }

        if (isFirstLogin || status == DbFirstLogin)
        {
            return FirstLogin;
        }

        return status switch
        {
            DbActive => Active,
            _ => status
        };
    }
}

public static class CustomClaimTypes
{
    public const string UserId = "UserId";
    public const string Email = "Email";
    public const string Role = "Role";
    public const string CampusId = "CampusId";
    public const string Status = "Status";
}
