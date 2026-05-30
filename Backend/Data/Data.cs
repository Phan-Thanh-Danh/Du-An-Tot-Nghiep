using System.Text.Json;
using Backend.Constants;
using Backend.Helpers;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public static class Data
{
    private const string RootCampusName = "LMS Root";
    private const string DefaultPassword = "Admin@123";
    private const string RootLevel = "root";
    private const string CampusLevel = "co_so";
    private const string ApprovedStatus = "approved";
    private const string ActiveStatus = "active";
    private const string RoomActiveStatus = "hoat_dong";
    private const string RequiredSubjectType = "bat_buoc";
    private const string TuitionCalculationType = "co_dinh_theo_hoc_ky";
    private const string ProgramVersion = "2026.1";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await SeedRolesAsync(context);

        var rootCampus = await GetOrCreateRootCampusAsync(context);
        var campuses = await SeedCampusesAsync(context, rootCampus);
        var hcmCampus = campuses["FPT Polytechnic Hồ Chí Minh"];

        var majors = await SeedMajorsAsync(context);
        var specializations = await SeedSpecializationsAsync(context, majors);
        await SeedCampusSpecializationsAsync(context, hcmCampus, specializations.Values);

        var cohortK2026 = await GetOrCreateCohortAsync(context);
        var terms = await SeedAcademicTermsAsync(context, hcmCampus);
        var subjects = await SeedSubjectsAsync(context);
        var programs = await SeedTrainingProgramsAsync(context, cohortK2026, specializations);

        await SeedProgramTermsAsync(context, programs.Values, terms);
        var programSubjects = await SeedProgramSubjectsAsync(context, programs, subjects);
        var users = await SeedDemoUsersAsync(context, rootCampus, hcmCampus);
        await SeedAdministrativeClassesAsync(context, hcmCampus, programs, users);
        await SeedParentLinkAsync(context, users);
        await SeedFacilitiesAsync(context, hcmCampus);
        await SeedCourseSyllabusesAsync(context, hcmCampus, programs, specializations, programSubjects, subjects);
        await SeedProgramTuitionConfigsAsync(context, hcmCampus, programs, terms);

        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(ApplicationDbContext context)
    {
        var rolePlans = new[]
        {
            new RoleSeed(AuthRoles.ToDatabaseCode(AuthRoles.Admin), "Quản trị hệ thống"),
            new RoleSeed(AuthRoles.ToDatabaseCode(AuthRoles.Teacher), "Giảng viên"),
            new RoleSeed(AuthRoles.ToDatabaseCode(AuthRoles.Student), "Sinh viên"),
            new RoleSeed(AuthRoles.ToDatabaseCode(AuthRoles.AcademicStaff), "Giáo vụ"),
            new RoleSeed(AuthRoles.ToDatabaseCode(AuthRoles.Principal), "Ban Giám Hiệu"),
            new RoleSeed(AuthRoles.ToDatabaseCode(AuthRoles.Parent), "Phụ huynh"),
            new RoleSeed(AuthRoles.ToDatabaseCode(AuthRoles.SuperAdmin), "Siêu quản trị"),
            new RoleSeed(AuthRoles.ToDatabaseCode(AuthRoles.CampusAdmin), "Quản trị cơ sở"),
            new RoleSeed(AuthRoles.ToDatabaseCode(AuthRoles.SubCampusAdmin), "Quản trị cơ sở con"),
            new RoleSeed(AuthRoles.ToDatabaseCode(AuthRoles.Chairman), "Chủ tịch hệ thống"),
        };

        var nextRoleId = ((await context.VaiTros.MaxAsync(x => (int?)x.MaVaiTro)) ?? 0) + 1;

        foreach (var plan in rolePlans)
        {
            var role = await context.VaiTros.FirstOrDefaultAsync(x =>
                x.MaCodeVaiTro == plan.Code);

            if (role is null)
            {
                role = new VaiTro
                {
                    MaVaiTro = nextRoleId++,
                    MaCodeVaiTro = plan.Code,
                };

                context.VaiTros.Add(role);
            }

            role.TenVaiTro = plan.Name;
        }

        await context.SaveChangesAsync();
    }

    private static async Task<DonVi> GetOrCreateRootCampusAsync(ApplicationDbContext context)
    {
        var root = await context.DonVis.FirstOrDefaultAsync(x => x.CapDonVi == RootLevel)
            ?? await context.DonVis.FirstOrDefaultAsync(x => x.TenDonVi == RootCampusName);

        if (root is null)
        {
            root = new DonVi
            {
                TenDonVi = RootCampusName,
                CapDonVi = RootLevel,
                ConHoatDong = true,
                NgayTao = DateTime.UtcNow,
            };

            context.DonVis.Add(root);
        }

        root.MaDonViCha = null;
        root.TenDonVi = RootCampusName;
        root.CapDonVi = RootLevel;
        root.ConHoatDong = true;
        root.NgayCapNhat = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return root;
    }

    private static async Task<Dictionary<string, DonVi>> SeedCampusesAsync(
        ApplicationDbContext context,
        DonVi rootCampus)
    {
        var campusPlans = new[]
        {
            new CampusSeed("FPT Polytechnic Hà Nội"),
            new CampusSeed("FPT Polytechnic Hồ Chí Minh", ["Cơ sở Hồ Chí Minh", "FPT HCM"]),
            new CampusSeed("FPT Polytechnic Đà Nẵng"),
            new CampusSeed("FPT Polytechnic Cần Thơ", ["FPT Cần Thơ"]),
            new CampusSeed("FPT Polytechnic Tây Nguyên"),
            new CampusSeed("FPT Polytechnic Hải Phòng"),
            new CampusSeed("FPT Polytechnic Đồng Nai"),
            new CampusSeed("FPT Polytechnic Bình Dương"),
            new CampusSeed("FPT Polytechnic Quy Nhơn"),
            new CampusSeed("FPT Polytechnic Huế"),
        };

        var result = new Dictionary<string, DonVi>(StringComparer.OrdinalIgnoreCase)
        {
            [RootCampusName] = rootCampus,
        };

        foreach (var plan in campusPlans)
        {
            var campus = await GetOrCreateCampusAsync(context, rootCampus, plan);
            result[plan.Name] = campus;
        }

        return result;
    }

    private static async Task<DonVi> GetOrCreateCampusAsync(
        ApplicationDbContext context,
        DonVi rootCampus,
        CampusSeed plan)
    {
        var campus = await context.DonVis.FirstOrDefaultAsync(x => x.TenDonVi == plan.Name);

        if (campus is null && plan.Aliases.Length > 0)
        {
            campus = await context.DonVis.FirstOrDefaultAsync(x => plan.Aliases.Contains(x.TenDonVi));
        }

        if (campus is null)
        {
            campus = new DonVi
            {
                TenDonVi = plan.Name,
                CapDonVi = CampusLevel,
                MaDonViCha = rootCampus.MaDonVi,
                ConHoatDong = true,
                NgayTao = DateTime.UtcNow,
            };

            context.DonVis.Add(campus);
        }

        campus.MaDonViCha = rootCampus.MaDonVi;
        campus.TenDonVi = plan.Name;
        campus.CapDonVi = CampusLevel;
        campus.ConHoatDong = true;
        campus.NgayCapNhat = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return campus;
    }

    private static async Task<Dictionary<string, NganhDaoTao>> SeedMajorsAsync(
        ApplicationDbContext context)
    {
        var majorPlans = new[]
        {
            new MajorSeed("CNTT", "Công nghệ thông tin"),
            new MajorSeed("TKDH", "Thiết kế đồ họa"),
            new MajorSeed("MKT", "Marketing"),
        };

        var result = new Dictionary<string, NganhDaoTao>(StringComparer.OrdinalIgnoreCase);

        foreach (var plan in majorPlans)
        {
            var major = await context.NganhDaoTaos.FirstOrDefaultAsync(x =>
                x.MaCodeNganh == plan.Code);

            if (major is null)
            {
                major = new NganhDaoTao
                {
                    MaCodeNganh = plan.Code,
                    NgayTao = DateTime.UtcNow,
                };

                context.NganhDaoTaos.Add(major);
            }

            major.TenNganh = plan.Name;
            major.MoTa = $"Ngành {plan.Name} dùng cho dữ liệu demo tốt nghiệp.";
            major.ConHoatDong = true;
            major.NgayCapNhat = DateTime.UtcNow;
            result[plan.Code] = major;
        }

        await context.SaveChangesAsync();
        return result;
    }

    private static async Task<Dictionary<string, ChuyenNganh>> SeedSpecializationsAsync(
        ApplicationDbContext context,
        IReadOnlyDictionary<string, NganhDaoTao> majors)
    {
        var specializationPlans = new[]
        {
            new SpecializationSeed("CNTT", "CNTT_PTPM", "Phát triển phần mềm"),
            new SpecializationSeed("CNTT", "CNTT_WEB", "Lập trình Web"),
            new SpecializationSeed("CNTT", "CNTT_UDPM", "Ứng dụng phần mềm"),
            new SpecializationSeed("TKDH", "TKDH_BRAND", "Thiết kế nhận diện thương hiệu"),
            new SpecializationSeed("TKDH", "TKDH_UIUX", "Thiết kế UI/UX"),
            new SpecializationSeed("TKDH", "TKDH_3D", "Thiết kế 3D / Motion Graphic"),
            new SpecializationSeed("MKT", "MKT_DIGITAL", "Digital Marketing"),
            new SpecializationSeed("MKT", "MKT_CONTENT", "Content Marketing"),
            new SpecializationSeed("MKT", "MKT_SALES", "Marketing & Sales"),
        };

        var result = new Dictionary<string, ChuyenNganh>(StringComparer.OrdinalIgnoreCase);

        foreach (var plan in specializationPlans)
        {
            var specialization = await context.ChuyenNganhs.FirstOrDefaultAsync(x =>
                x.MaCodeChuyenNganh == plan.Code);

            if (specialization is null)
            {
                specialization = new ChuyenNganh
                {
                    MaCodeChuyenNganh = plan.Code,
                    NgayTao = DateTime.UtcNow,
                };

                context.ChuyenNganhs.Add(specialization);
            }

            specialization.MaNganh = majors[plan.MajorCode].MaNganh;
            specialization.TenChuyenNganh = plan.Name;
            specialization.MoTa =
                $"Chuyên ngành {plan.Name} thuộc ngành {majors[plan.MajorCode].TenNganh}.";
            specialization.ConHoatDong = true;
            specialization.NgayCapNhat = DateTime.UtcNow;
            result[plan.Code] = specialization;
        }

        await context.SaveChangesAsync();
        return result;
    }

    private static async Task SeedCampusSpecializationsAsync(
        ApplicationDbContext context,
        DonVi campus,
        IEnumerable<ChuyenNganh> specializations)
    {
        foreach (var specialization in specializations)
        {
            var campusSpecialization = await context.ChuyenNganhTheoCoSos.FirstOrDefaultAsync(x =>
                x.MaDonVi == campus.MaDonVi &&
                x.MaChuyenNganh == specialization.MaChuyenNganh);

            if (campusSpecialization is null)
            {
                campusSpecialization = new ChuyenNganhTheoCoSo
                {
                    MaDonVi = campus.MaDonVi,
                    MaChuyenNganh = specialization.MaChuyenNganh,
                    NgayTao = DateTime.UtcNow,
                };

                context.ChuyenNganhTheoCoSos.Add(campusSpecialization);
            }

            campusSpecialization.TrangThai = ApprovedStatus;
            campusSpecialization.NamBatDau = 2026;
            campusSpecialization.ChiTieuDuKien = 120;
            campusSpecialization.GhiChu = $"Mở chuyên ngành {specialization.TenChuyenNganh} tại {campus.TenDonVi}.";
            campusSpecialization.ConHoatDong = true;
            campusSpecialization.NgayCapNhat = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
    }

    private static async Task<KhoaTuyenSinh> GetOrCreateCohortAsync(ApplicationDbContext context)
    {
        var cohort = await context.KhoaTuyenSinhs.FirstOrDefaultAsync(x => x.MaCodeKhoa == "K2026");

        if (cohort is null)
        {
            cohort = new KhoaTuyenSinh
            {
                MaCodeKhoa = "K2026",
                NgayTao = DateTime.UtcNow,
            };

            context.KhoaTuyenSinhs.Add(cohort);
        }

        cohort.TenKhoa = "Khóa K2026";
        cohort.NamBatDau = 2026;
        cohort.NamKetThucDuKien = 2029;
        cohort.MoTa = "Khóa tuyển sinh K2026 dùng cho dữ liệu demo tốt nghiệp.";
        cohort.ConHoatDong = true;
        cohort.NgayCapNhat = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return cohort;
    }

    private static async Task<IReadOnlyList<HocKy>> SeedAcademicTermsAsync(
        ApplicationDbContext context,
        DonVi campus)
    {
        var termPlans = new[]
        {
            new AcademicTermSeed("HK1_2026", "Học kỳ 1 năm 2026", "2026", 1, new DateOnly(2026, 1, 1), new DateOnly(2026, 4, 30)),
            new AcademicTermSeed("HK2_2026", "Học kỳ 2 năm 2026", "2026", 2, new DateOnly(2026, 5, 1), new DateOnly(2026, 8, 31)),
            new AcademicTermSeed("HK3_2026", "Học kỳ 3 năm 2026", "2026", 3, new DateOnly(2026, 9, 1), new DateOnly(2026, 12, 31)),
            new AcademicTermSeed("HK1_2027", "Học kỳ 1 năm 2027", "2027", 1, new DateOnly(2027, 1, 1), new DateOnly(2027, 4, 30)),
            new AcademicTermSeed("HK2_2027", "Học kỳ 2 năm 2027", "2027", 2, new DateOnly(2027, 5, 1), new DateOnly(2027, 8, 31)),
            new AcademicTermSeed("HK3_2027", "Học kỳ 3 năm 2027", "2027", 3, new DateOnly(2027, 9, 1), new DateOnly(2027, 12, 31)),
            new AcademicTermSeed("HK1_2028", "Học kỳ 1 năm 2028", "2028", 1, new DateOnly(2028, 1, 1), new DateOnly(2028, 4, 30)),
            new AcademicTermSeed("HK2_2028", "Học kỳ 2 năm 2028", "2028", 2, new DateOnly(2028, 5, 1), new DateOnly(2028, 8, 31)),
            new AcademicTermSeed("HK3_2028", "Học kỳ 3 năm 2028", "2028", 3, new DateOnly(2028, 9, 1), new DateOnly(2028, 12, 31)),
        };

        var terms = new List<HocKy>();

        foreach (var plan in termPlans)
        {
            var term = await context.HocKys.FirstOrDefaultAsync(x =>
                x.MaDonVi == campus.MaDonVi &&
                x.MaCodeHocKy == plan.Code);

            if (term is null)
            {
                term = await context.HocKys.FirstOrDefaultAsync(x =>
                    x.MaDonVi == campus.MaDonVi &&
                    x.NamHoc == plan.Year &&
                    x.ThuTuTrongNam == plan.TermOrderInYear);
            }

            if (term is null)
            {
                term = new HocKy
                {
                    MaDonVi = campus.MaDonVi,
                    MaCodeHocKy = plan.Code,
                };

                context.HocKys.Add(term);
            }

            term.MaCodeHocKy = plan.Code;
            term.TenHocKy = plan.Name;
            term.NamHoc = plan.Year;
            term.ThuTuTrongNam = plan.TermOrderInYear;
            term.NgayBatDau = plan.StartDate;
            term.NgayKetThuc = plan.EndDate;
            term.NgayKetThucBlock5 = plan.EndDate.AddDays(5);
            term.DaKhoa = false;
            term.SoTinChiToiDa = 24;
            term.HanRutMon = plan.StartDate.AddDays(14);
            terms.Add(term);
        }

        await context.SaveChangesAsync();
        return terms;
    }

    private static async Task<Dictionary<string, DanhMucMonHoc>> SeedSubjectsAsync(
        ApplicationDbContext context)
    {
        var subjectPlans = new[]
        {
            new SubjectSeed("COM101", "Nhập môn lập trình", 3),
            new SubjectSeed("COM102", "Cơ sở dữ liệu", 3),
            new SubjectSeed("COM103", "Lập trình C#", 3),
            new SubjectSeed("WEB101", "Thiết kế Web", 3),
            new SubjectSeed("WEB102", "Lập trình JavaScript", 3),
            new SubjectSeed("DBI101", "SQL Server căn bản", 3),
            new SubjectSeed("PRO101", "Dự án mẫu phần mềm", 3),
            new SubjectSeed("API101", "Xây dựng REST API", 3),
            new SubjectSeed("FE101", "Vue.js căn bản", 3),
            new SubjectSeed("BE101", "ASP.NET Core căn bản", 3),
            new SubjectSeed("DES101", "Nguyên lý thị giác", 2),
            new SubjectSeed("DES102", "Photoshop căn bản", 3),
            new SubjectSeed("DES103", "Illustrator căn bản", 3),
            new SubjectSeed("DES104", "Thiết kế nhận diện thương hiệu", 3),
            new SubjectSeed("DES105", "Typography", 2),
            new SubjectSeed("DES106", "UI/UX Design", 3),
            new SubjectSeed("DES107", "Thiết kế ấn phẩm truyền thông", 3),
            new SubjectSeed("DES108", "Motion Graphic căn bản", 3),
            new SubjectSeed("DES109", "Thiết kế portfolio", 2),
            new SubjectSeed("DES110", "Dự án thiết kế đồ họa", 3),
            new SubjectSeed("MKT101", "Nguyên lý Marketing", 3),
            new SubjectSeed("MKT102", "Hành vi khách hàng", 3),
            new SubjectSeed("MKT103", "Digital Marketing", 3),
            new SubjectSeed("MKT104", "Content Marketing", 3),
            new SubjectSeed("MKT105", "SEO căn bản", 2),
            new SubjectSeed("MKT106", "Quảng cáo mạng xã hội", 3),
            new SubjectSeed("MKT107", "Marketing Analytics", 3),
            new SubjectSeed("MKT108", "Kỹ năng bán hàng", 2),
            new SubjectSeed("MKT109", "Xây dựng thương hiệu", 3),
            new SubjectSeed("MKT110", "Dự án Marketing tổng hợp", 3),
            new SubjectSeed("GEN101", "Kỹ năng học tập", 2),
            new SubjectSeed("GEN102", "Tin học cơ bản", 2),
            new SubjectSeed("GEN103", "Tiếng Anh cơ bản", 3),
            new SubjectSeed("GEN104", "Kỹ năng giao tiếp", 2),
            new SubjectSeed("GEN105", "Khởi sự doanh nghiệp", 2),
        };

        var result = new Dictionary<string, DanhMucMonHoc>(StringComparer.OrdinalIgnoreCase);

        foreach (var plan in subjectPlans)
        {
            var subject = await context.DanhMucMonHocs.FirstOrDefaultAsync(x =>
                x.MaCodeMonHoc == plan.Code);

            if (subject is null)
            {
                subject = new DanhMucMonHoc
                {
                    MaCodeMonHoc = plan.Code,
                };

                context.DanhMucMonHocs.Add(subject);
            }

            subject.TenMonHoc = plan.Name;
            subject.SoTinChi = plan.Credits;
            subject.ConHoatDong = true;
            result[plan.Code] = subject;
        }

        await context.SaveChangesAsync();
        return result;
    }

    private static async Task<Dictionary<string, ChuongTrinhDaoTao>> SeedTrainingProgramsAsync(
        ApplicationDbContext context,
        KhoaTuyenSinh cohort,
        IReadOnlyDictionary<string, ChuyenNganh> specializations)
    {
        var programPlans = new[]
        {
            new TrainingProgramSeed(
                "CT_CNTT_K2026",
                "Chương trình Công nghệ thông tin K2026",
                "CNTT_PTPM",
                122,
                "Chương trình đào tạo K2026 cho ngành Công nghệ thông tin."),
            new TrainingProgramSeed(
                "CT_TKDH_K2026",
                "Chương trình Thiết kế đồ họa K2026",
                "TKDH_UIUX",
                118,
                "Chương trình đào tạo K2026 cho ngành Thiết kế đồ họa."),
            new TrainingProgramSeed(
                "CT_MKT_K2026",
                "Chương trình Marketing K2026",
                "MKT_DIGITAL",
                116,
                "Chương trình đào tạo K2026 cho ngành Marketing."),
        };

        var result = new Dictionary<string, ChuongTrinhDaoTao>(StringComparer.OrdinalIgnoreCase);

        foreach (var plan in programPlans)
        {
            var program = await context.ChuongTrinhDaoTaos.FirstOrDefaultAsync(x =>
                x.MaCodeChuongTrinh == plan.Code);

            if (program is null)
            {
                program = await context.ChuongTrinhDaoTaos.FirstOrDefaultAsync(x =>
                    x.MaChuyenNganh == specializations[plan.SpecializationCode].MaChuyenNganh &&
                    x.MaKhoaTuyenSinh == cohort.MaKhoaTuyenSinh &&
                    x.Version == ProgramVersion);
            }

            if (program is null)
            {
                program = new ChuongTrinhDaoTao
                {
                    MaCodeChuongTrinh = plan.Code,
                    NgayTao = DateTime.UtcNow,
                };

                context.ChuongTrinhDaoTaos.Add(program);
            }

            program.MaChuyenNganh = specializations[plan.SpecializationCode].MaChuyenNganh;
            program.MaKhoaTuyenSinh = cohort.MaKhoaTuyenSinh;
            program.MaCodeChuongTrinh = plan.Code;
            program.TenChuongTrinh = plan.Name;
            program.Version = ProgramVersion;
            program.SoHocKy = 9;
            program.ThoiGianDaoTaoThang = 36;
            program.TongTinChiYeuCau = plan.RequiredCredits;
            program.SoTinChiToiThieuMoiKy = 12;
            program.SoTinChiToiDaMoiKy = 24;
            program.TrangThai = ActiveStatus;
            program.MoTa = plan.Description;
            program.NguonChuongTrinhId = null;
            program.GhiChuThayDoi = "Seed dữ liệu demo K2026.";
            program.NgayHieuLuc = new DateOnly(2026, 1, 1);
            program.NgayHetHieuLuc = null;
            program.ConHoatDong = true;
            program.NgayCapNhat = DateTime.UtcNow;
            result[plan.Code] = program;
        }

        await context.SaveChangesAsync();
        return result;
    }

    private static async Task SeedProgramTermsAsync(
        ApplicationDbContext context,
        IEnumerable<ChuongTrinhDaoTao> programs,
        IReadOnlyList<HocKy> terms)
    {
        foreach (var program in programs)
        {
            for (var index = 0; index < terms.Count; index++)
            {
                var term = terms[index];
                var programTermOrder = index + 1;
                var mapping = await context.ChuongTrinhHocKys.FirstOrDefaultAsync(x =>
                    x.MaChuongTrinh == program.MaChuongTrinh &&
                    x.ThuTuHocKy == programTermOrder);

                if (mapping is null)
                {
                    mapping = await context.ChuongTrinhHocKys.FirstOrDefaultAsync(x =>
                        x.MaChuongTrinh == program.MaChuongTrinh &&
                        x.MaHocKy == term.MaHocKy);
                }

                if (mapping is null)
                {
                    mapping = new ChuongTrinhHocKy
                    {
                        MaChuongTrinh = program.MaChuongTrinh,
                    };

                    context.ChuongTrinhHocKys.Add(mapping);
                }

                mapping.MaHocKy = term.MaHocKy;
                mapping.ThuTuHocKy = programTermOrder;
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task<Dictionary<string, MonHocTrongChuongTrinh>> SeedProgramSubjectsAsync(
        ApplicationDbContext context,
        IReadOnlyDictionary<string, ChuongTrinhDaoTao> programs,
        IReadOnlyDictionary<string, DanhMucMonHoc> subjects)
    {
        var planByProgram = new Dictionary<string, ProgramSubjectSeed[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["CT_CNTT_K2026"] =
            [
                new(1, "GEN101"), new(1, "GEN102"), new(1, "COM101"),
                new(2, "COM102"), new(2, "WEB101"), new(2, "GEN104"),
                new(3, "COM103"), new(3, "WEB102"), new(3, "DBI101"),
                new(4, "API101"), new(4, "FE101"),
                new(5, "BE101"), new(5, "PRO101"),
                new(6, "GEN103"), new(6, "GEN105"),
            ],
            ["CT_TKDH_K2026"] =
            [
                new(1, "GEN101"), new(1, "DES101"), new(1, "DES102"),
                new(2, "DES103"), new(2, "DES105"), new(2, "GEN104"),
                new(3, "DES104"), new(3, "DES106"), new(3, "DES107"),
                new(4, "DES108"), new(4, "DES109"),
                new(5, "DES110"), new(5, "GEN102"),
                new(6, "GEN103"), new(6, "GEN105"),
            ],
            ["CT_MKT_K2026"] =
            [
                new(1, "GEN101"), new(1, "MKT101"), new(1, "MKT102"),
                new(2, "MKT103"), new(2, "MKT104"), new(2, "GEN104"),
                new(3, "MKT105"), new(3, "MKT106"), new(3, "MKT109"),
                new(4, "MKT107"), new(4, "MKT108"),
                new(5, "MKT110"), new(5, "GEN102"),
                new(6, "GEN103"), new(6, "GEN105"),
            ],
        };

        var result = new Dictionary<string, MonHocTrongChuongTrinh>(StringComparer.OrdinalIgnoreCase);

        foreach (var (programCode, subjectPlans) in planByProgram)
        {
            var program = programs[programCode];
            var order = 1;

            foreach (var plan in subjectPlans)
            {
                var subject = subjects[plan.SubjectCode];
                var programSubject = await context.MonHocTrongChuongTrinhs.FirstOrDefaultAsync(x =>
                    x.MaChuongTrinh == program.MaChuongTrinh &&
                    x.MaMonHoc == subject.MaMonHoc);

                if (programSubject is null)
                {
                    programSubject = new MonHocTrongChuongTrinh
                    {
                        MaChuongTrinh = program.MaChuongTrinh,
                        MaMonHoc = subject.MaMonHoc,
                        NgayTao = DateTime.UtcNow,
                    };

                    context.MonHocTrongChuongTrinhs.Add(programSubject);
                }

                programSubject.HocKyDuKien = plan.ExpectedTerm;
                programSubject.SoTinChi = subject.SoTinChi;
                programSubject.LoaiMonHoc = RequiredSubjectType;
                programSubject.BatBuoc = true;
                programSubject.ThuTu = order++;
                programSubject.GhiChu = $"Môn {subject.MaCodeMonHoc} trong {program.TenChuongTrinh}.";
                programSubject.ConHoatDong = true;
                programSubject.NgayCapNhat = DateTime.UtcNow;
                result[$"{programCode}:{plan.SubjectCode}"] = programSubject;
            }
        }

        await context.SaveChangesAsync();
        return result;
    }

    private static async Task<Dictionary<string, NguoiDung>> SeedDemoUsersAsync(
        ApplicationDbContext context,
        DonVi rootCampus,
        DonVi hcmCampus)
    {
        var userPlans = new[]
        {
            new DemoUserSeed("superadmin@lms.local", "Super Admin", AuthRoles.ToDatabaseCode(AuthRoles.SuperAdmin), rootCampus.MaDonVi),
            new DemoUserSeed("admin@lms.local", "Admin Hệ Thống", AuthRoles.ToDatabaseCode(AuthRoles.Admin), rootCampus.MaDonVi),
            new DemoUserSeed("campusadmin.hcm@lms.local", "Admin Cơ Sở Hồ Chí Minh", AuthRoles.ToDatabaseCode(AuthRoles.CampusAdmin), hcmCampus.MaDonVi),
            new DemoUserSeed("giaovu.hcm@lms.local", "Giáo Vụ Hồ Chí Minh", AuthRoles.ToDatabaseCode(AuthRoles.AcademicStaff), hcmCampus.MaDonVi),
            new DemoUserSeed("principal@lms.local", "Ban Giám Hiệu", AuthRoles.ToDatabaseCode(AuthRoles.Principal), hcmCampus.MaDonVi),
            new DemoUserSeed("teacher.cntt@lms.local", "Nguyễn Văn Lập Trình", AuthRoles.ToDatabaseCode(AuthRoles.Teacher), hcmCampus.MaDonVi),
            new DemoUserSeed("teacher.tkdh@lms.local", "Trần Thị Thiết Kế", AuthRoles.ToDatabaseCode(AuthRoles.Teacher), hcmCampus.MaDonVi),
            new DemoUserSeed("teacher.mkt@lms.local", "Lê Văn Marketing", AuthRoles.ToDatabaseCode(AuthRoles.Teacher), hcmCampus.MaDonVi),
            new DemoUserSeed("student.cntt01@lms.local", "Nguyễn Văn Sinh Viên CNTT", AuthRoles.ToDatabaseCode(AuthRoles.Student), hcmCampus.MaDonVi, 2026),
            new DemoUserSeed("student.tkdh01@lms.local", "Trần Thị Sinh Viên Thiết Kế", AuthRoles.ToDatabaseCode(AuthRoles.Student), hcmCampus.MaDonVi, 2026),
            new DemoUserSeed("student.mkt01@lms.local", "Lê Văn Sinh Viên Marketing", AuthRoles.ToDatabaseCode(AuthRoles.Student), hcmCampus.MaDonVi, 2026),
            new DemoUserSeed("parent01@lms.local", "Phụ Huynh Demo", AuthRoles.ToDatabaseCode(AuthRoles.Parent), hcmCampus.MaDonVi),
        };

        var roles = await context.VaiTros.ToDictionaryAsync(
            x => x.MaCodeVaiTro,
            StringComparer.OrdinalIgnoreCase);
        var result = new Dictionary<string, NguoiDung>(StringComparer.OrdinalIgnoreCase);

        foreach (var plan in userPlans)
        {
            var user = await context.NguoiDungs.FirstOrDefaultAsync(x => x.Email == plan.Email);

            if (user is null)
            {
                user = new NguoiDung
                {
                    Email = plan.Email,
                    NgayTao = DateTime.UtcNow,
                };

                context.NguoiDungs.Add(user);
            }

            user.MaDonVi = plan.CampusId;
            user.HoTen = plan.FullName;
            user.VaiTroChinh = plan.RoleCode;
            user.NamNhapHoc = plan.EnrollmentYear;
            user.TrangThai = UserStatuses.DbActive;
            user.MatKhauHash = PasswordHelper.HashPassword(DefaultPassword);
            user.SoLanSaiMatKhau = 0;
            user.DangNhapLanDau = false;
            result[plan.Email] = user;
        }

        await context.SaveChangesAsync();

        foreach (var plan in userPlans)
        {
            if (!roles.TryGetValue(plan.RoleCode, out var role))
            {
                continue;
            }

            var user = result[plan.Email];
            var hasAssignment = await context.PhanQuyenNguoiDungs.AnyAsync(x =>
                x.MaNguoiDung == user.MaNguoiDung &&
                x.MaVaiTro == role.MaVaiTro);

            if (!hasAssignment)
            {
                context.PhanQuyenNguoiDungs.Add(new PhanQuyenNguoiDung
                {
                    MaNguoiDung = user.MaNguoiDung,
                    MaVaiTro = role.MaVaiTro,
                    NgayGan = DateTime.UtcNow,
                });
            }
        }

        await context.SaveChangesAsync();
        return result;
    }

    private static async Task SeedAdministrativeClassesAsync(
        ApplicationDbContext context,
        DonVi campus,
        IReadOnlyDictionary<string, ChuongTrinhDaoTao> programs,
        IReadOnlyDictionary<string, NguoiDung> users)
    {
        var classPlans = new[]
        {
            new AdministrativeClassSeed(
                "SE1901",
                "SE1901 - Công nghệ thông tin K2026",
                "CT_CNTT_K2026",
                "teacher.cntt@lms.local",
                "student.cntt01@lms.local"),
            new AdministrativeClassSeed(
                "GD1901",
                "GD1901 - Thiết kế đồ họa K2026",
                "CT_TKDH_K2026",
                "teacher.tkdh@lms.local",
                "student.tkdh01@lms.local"),
            new AdministrativeClassSeed(
                "MK1901",
                "MK1901 - Marketing K2026",
                "CT_MKT_K2026",
                "teacher.mkt@lms.local",
                "student.mkt01@lms.local"),
        };

        foreach (var plan in classPlans)
        {
            var administrativeClass = await context.LopHanhChinhs.FirstOrDefaultAsync(x =>
                x.MaCodeLop == plan.Code);

            if (administrativeClass is null)
            {
                administrativeClass = new LopHanhChinh
                {
                    MaCodeLop = plan.Code,
                };

                context.LopHanhChinhs.Add(administrativeClass);
            }

            administrativeClass.MaDonVi = campus.MaDonVi;
            administrativeClass.TenLop = plan.Name;
            administrativeClass.MaGiaoVienChuNhiem = users[plan.TeacherEmail].MaNguoiDung;
            administrativeClass.MaChuongTrinh = programs[plan.ProgramCode].MaChuongTrinh;
            administrativeClass.NamNhapHoc = 2026;
            administrativeClass.ConHoatDong = true;

            await context.SaveChangesAsync();

            var student = users[plan.StudentEmail];
            student.MaLop = administrativeClass.MaLop;
            student.NamNhapHoc = 2026;
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedParentLinkAsync(
        ApplicationDbContext context,
        IReadOnlyDictionary<string, NguoiDung> users)
    {
        var parent = users["parent01@lms.local"];
        var student = users["student.cntt01@lms.local"];
        var link = await context.LienKetPhuHuynhs.FirstOrDefaultAsync(x =>
            x.MaPhuHuynh == parent.MaNguoiDung &&
            x.MaHocSinh == student.MaNguoiDung);

        if (link is null)
        {
            link = new LienKetPhuHuynh
            {
                MaPhuHuynh = parent.MaNguoiDung,
                MaHocSinh = student.MaNguoiDung,
            };

            context.LienKetPhuHuynhs.Add(link);
        }

        link.QuyenXem = JsonSerializer.Serialize(new
        {
            grades = true,
            attendance = true,
            tuition = true,
        }, JsonOptions);
        link.TrangThai = UserStatuses.DbActive;
        link.LienKetLuc ??= DateTime.UtcNow;

        await context.SaveChangesAsync();
    }

    private static async Task SeedFacilitiesAsync(ApplicationDbContext context, DonVi campus)
    {
        var buildingA = await UpsertBuildingAsync(context, campus, "A", "Tòa A", 3);
        var buildingB = await UpsertBuildingAsync(context, campus, "B", "Tòa B", 2);
        var buildingC = await UpsertBuildingAsync(context, campus, "C", "Tòa C", 1);

        var floors = new Dictionary<string, Tang>(StringComparer.OrdinalIgnoreCase)
        {
            ["A1"] = await UpsertFloorAsync(context, buildingA, 1),
            ["A2"] = await UpsertFloorAsync(context, buildingA, 2),
            ["A3"] = await UpsertFloorAsync(context, buildingA, 3),
            ["B1"] = await UpsertFloorAsync(context, buildingB, 1),
            ["B2"] = await UpsertFloorAsync(context, buildingB, 2),
            ["C1"] = await UpsertFloorAsync(context, buildingC, 1),
        };

        var roomPlans = new[]
        {
            new RoomSeed("A101", "Phòng A101", buildingA, floors["A1"], 40, "ly_thuyet", "Phòng lý thuyết 40 chỗ."),
            new RoomSeed("A102", "Phòng A102", buildingA, floors["A1"], 35, "ly_thuyet", "Phòng lý thuyết 35 chỗ."),
            new RoomSeed("A201", "Phòng Lab A201", buildingA, floors["A2"], 30, "lab", "Phòng lab thực hành phần mềm."),
            new RoomSeed("A202", "Phòng Lab A202", buildingA, floors["A2"], 30, "lab", "Phòng lab thực hành phần mềm."),
            new RoomSeed("A301", "Hội trường A301", buildingA, floors["A3"], 50, "hoi_truong", "Hội trường demo bảo vệ đồ án."),
            new RoomSeed("B101", "Phòng B101", buildingB, floors["B1"], 45, "ly_thuyet", "Phòng lý thuyết 45 chỗ."),
            new RoomSeed("B201", "Phòng B201", buildingB, floors["B2"], 35, "ly_thuyet", "Phòng lý thuyết 35 chỗ."),
            new RoomSeed("C101", "Studio thiết kế C101", buildingC, floors["C1"], 25, "thuc_hanh", "Studio thiết kế đồ họa."),
        };

        foreach (var plan in roomPlans)
        {
            var room = await context.PhongHocs.FirstOrDefaultAsync(x =>
                x.MaDonVi == campus.MaDonVi &&
                x.MaCodePhong == plan.Code);

            if (room is null)
            {
                room = new PhongHoc
                {
                    MaDonVi = campus.MaDonVi,
                    MaCodePhong = plan.Code,
                };

                context.PhongHocs.Add(room);
            }

            room.MaToaNha = plan.Building.MaToaNha;
            room.MaTang = plan.Floor.MaTang;
            room.TenPhong = plan.Name;
            room.SucChua = plan.Capacity;
            room.LoaiPhong = plan.Type;
            room.TrangThaiPhong = RoomActiveStatus;
            room.GhiChu = plan.Note;
        }

        await context.SaveChangesAsync();
    }

    private static async Task<ToaNha> UpsertBuildingAsync(
        ApplicationDbContext context,
        DonVi campus,
        string code,
        string name,
        int floorCount)
    {
        var building = await context.ToaNhas.FirstOrDefaultAsync(x =>
            x.MaDonVi == campus.MaDonVi &&
            x.MaCodeToaNha == code);

        if (building is null)
        {
            building = new ToaNha
            {
                MaDonVi = campus.MaDonVi,
                MaCodeToaNha = code,
                NgayTao = DateTime.UtcNow,
            };

            context.ToaNhas.Add(building);
        }

        building.TenToaNha = name;
        building.DiaChi = $"{name} - {campus.TenDonVi}";
        building.SoTang = floorCount;
        building.ConHoatDong = true;
        building.NgayCapNhat = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return building;
    }

    private static async Task<Tang> UpsertFloorAsync(
        ApplicationDbContext context,
        ToaNha building,
        int floorNumber)
    {
        var floor = await context.Tangs.FirstOrDefaultAsync(x =>
            x.MaToaNha == building.MaToaNha &&
            x.ThuTuTang == floorNumber);

        if (floor is null)
        {
            floor = new Tang
            {
                MaToaNha = building.MaToaNha,
                ThuTuTang = floorNumber,
            };

            context.Tangs.Add(floor);
        }

        floor.TenTang = $"Tầng {floorNumber}";
        floor.MoTa = $"{building.TenToaNha} - Tầng {floorNumber}";
        floor.ConHoatDong = true;

        await context.SaveChangesAsync();
        return floor;
    }

    private static async Task SeedCourseSyllabusesAsync(
        ApplicationDbContext context,
        DonVi campus,
        IReadOnlyDictionary<string, ChuongTrinhDaoTao> programs,
        IReadOnlyDictionary<string, ChuyenNganh> specializations,
        IReadOnlyDictionary<string, MonHocTrongChuongTrinh> programSubjects,
        IReadOnlyDictionary<string, DanhMucMonHoc> subjects)
    {
        var syllabusPlans = new[]
        {
            new SyllabusSeed("CT_CNTT_K2026", "CNTT_PTPM", "COM101", "Đề cương Nhập môn lập trình"),
            new SyllabusSeed("CT_CNTT_K2026", "CNTT_PTPM", "WEB101", "Đề cương Thiết kế Web"),
            new SyllabusSeed("CT_TKDH_K2026", "TKDH_UIUX", "DES101", "Đề cương Nguyên lý thị giác"),
            new SyllabusSeed("CT_TKDH_K2026", "TKDH_UIUX", "DES106", "Đề cương UI/UX Design"),
            new SyllabusSeed("CT_MKT_K2026", "MKT_DIGITAL", "MKT101", "Đề cương Nguyên lý Marketing"),
            new SyllabusSeed("CT_MKT_K2026", "MKT_DIGITAL", "MKT103", "Đề cương Digital Marketing"),
        };

        foreach (var plan in syllabusPlans)
        {
            var subject = subjects[plan.SubjectCode];
            var specialization = specializations[plan.SpecializationCode];
            programSubjects.TryGetValue($"{plan.ProgramCode}:{plan.SubjectCode}", out var programSubject);

            var syllabus = await context.CourseSyllabuses.FirstOrDefaultAsync(x =>
                x.MaMonHoc == subject.MaMonHoc &&
                x.MaChuyenNganh == specialization.MaChuyenNganh &&
                x.MaDonVi == campus.MaDonVi &&
                x.Version == ProgramVersion);

            if (syllabus is null)
            {
                syllabus = new CourseSyllabus
                {
                    MaMonHoc = subject.MaMonHoc,
                    MaChuyenNganh = specialization.MaChuyenNganh,
                    MaDonVi = campus.MaDonVi,
                    Version = ProgramVersion,
                    NgayTao = DateTime.UtcNow,
                };

                context.CourseSyllabuses.Add(syllabus);
            }

            syllabus.MaChuongTrinhMonHoc = programSubject?.MaChuongTrinhMonHoc;
            syllabus.TenSyllabus = $"{plan.Name} - {programs[plan.ProgramCode].TenChuongTrinh}";
            syllabus.HocKyDuKien = programSubject?.HocKyDuKien;
            syllabus.BatBuoc = true;
            syllabus.TrangThai = ActiveStatus;
            syllabus.ConHoatDong = true;
            syllabus.NgayCapNhat = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedProgramTuitionConfigsAsync(
        ApplicationDbContext context,
        DonVi campus,
        IReadOnlyDictionary<string, ChuongTrinhDaoTao> programs,
        IReadOnlyList<HocKy> terms)
    {
        var tuitionPlans = new[]
        {
            new TuitionSeed("CT_CNTT_K2026", [28_000_000m, 29_000_000m, 30_000_000m], 2_000_000m),
            new TuitionSeed("CT_TKDH_K2026", [24_000_000m, 25_000_000m, 26_000_000m], 2_000_000m),
            new TuitionSeed("CT_MKT_K2026", [23_000_000m, 24_000_000m, 25_000_000m], 1_500_000m),
        };

        foreach (var plan in tuitionPlans)
        {
            var program = programs[plan.ProgramCode];

            for (var index = 0; index < terms.Count; index++)
            {
                var term = terms[index];
                var yearInProgram = (index / 3) + 1;
                var termInYear = (index % 3) + 1;
                var tuitionAmount = plan.YearlyTuitionAmounts[yearInProgram - 1];
                var config = await context.CauHinhHocPhiChuongTrinhs.FirstOrDefaultAsync(x =>
                    x.MaDonVi == campus.MaDonVi &&
                    x.MaChuongTrinhDaoTao == program.MaChuongTrinh &&
                    x.MaHocKy == term.MaHocKy &&
                    x.ConHoatDong);

                if (config is null)
                {
                    config = await context.CauHinhHocPhiChuongTrinhs.FirstOrDefaultAsync(x =>
                        x.MaDonVi == campus.MaDonVi &&
                        x.MaChuongTrinhDaoTao == program.MaChuongTrinh &&
                        x.MaHocKy == term.MaHocKy);
                }

                if (config is null)
                {
                    config = new CauHinhHocPhiChuongTrinh
                    {
                        MaDonVi = campus.MaDonVi,
                        MaChuongTrinhDaoTao = program.MaChuongTrinh,
                        MaHocKy = term.MaHocKy,
                        NgayTao = DateTime.UtcNow,
                    };

                    context.CauHinhHocPhiChuongTrinhs.Add(config);
                }

                config.NamHocTrongChuongTrinh = yearInProgram;
                config.HocKyTrongNam = termInYear;
                config.SoThuTuHocKy = index + 1;
                config.LoaiCachTinhHocPhi = TuitionCalculationType;
                config.SoTienHocPhi = tuitionAmount;
                config.TienHocLieu = plan.MaterialAmount;
                config.TongTienDuKien = tuitionAmount + plan.MaterialAmount;
                config.ConHoatDong = true;
                config.GhiChu = $"{program.TenChuongTrinh} - năm {yearInProgram} kỳ {termInYear} tại {campus.TenDonVi}.";
                config.NgayCapNhat = DateTime.UtcNow;
            }
        }

        await context.SaveChangesAsync();
    }

    private sealed record RoleSeed(string Code, string Name);

    private sealed record CampusSeed(string Name, string[]? AliasList = null)
    {
        public string[] Aliases { get; } = AliasList ?? [];
    }

    private sealed record MajorSeed(string Code, string Name);

    private sealed record SpecializationSeed(string MajorCode, string Code, string Name);

    private sealed record AcademicTermSeed(
        string Code,
        string Name,
        string Year,
        int TermOrderInYear,
        DateOnly StartDate,
        DateOnly EndDate);

    private sealed record SubjectSeed(string Code, string Name, int Credits);

    private sealed record TrainingProgramSeed(
        string Code,
        string Name,
        string SpecializationCode,
        int RequiredCredits,
        string Description);

    private sealed record ProgramSubjectSeed(int ExpectedTerm, string SubjectCode);

    private sealed record DemoUserSeed(
        string Email,
        string FullName,
        string RoleCode,
        int CampusId,
        int? EnrollmentYear = null);

    private sealed record AdministrativeClassSeed(
        string Code,
        string Name,
        string ProgramCode,
        string TeacherEmail,
        string StudentEmail);

    private sealed record RoomSeed(
        string Code,
        string Name,
        ToaNha Building,
        Tang Floor,
        int Capacity,
        string Type,
        string Note);

    private sealed record SyllabusSeed(
        string ProgramCode,
        string SpecializationCode,
        string SubjectCode,
        string Name);

    private sealed record TuitionSeed(
        string ProgramCode,
        decimal[] YearlyTuitionAmounts,
        decimal MaterialAmount);
}
