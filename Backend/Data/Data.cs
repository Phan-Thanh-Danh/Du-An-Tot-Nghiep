using System.Text.Json;
using Backend.Constants;
using Backend.Helpers;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public static class Data
{
    private const string RootCampusName = "LMS Root";
    private const string DefaultPassword = "123456";
    private const string RootLevel = "root";
    private const string CampusLevel = "co_so";
    private const string ApprovedStatus = "approved";
    private const string ActiveStatus = "active";
    private const string PublishedStatus = "da_xuat_ban";
    private const string ClassSectionOpenStatus = "mo";
    private const string RoomActiveStatus = "hoat_dong";
    private const string RequiredSubjectType = "bat_buoc";
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
        await SeedLearningContentAsync(context, subjects);
        var programs = await SeedTrainingProgramsAsync(context, cohortK2026, specializations);

        await SeedProgramTermsAsync(context, programs.Values, terms);
        var programSubjects = await SeedProgramSubjectsAsync(context, programs, subjects);
        var users = await SeedDemoUsersAsync(context, rootCampus, hcmCampus);
        await SeedBaseTeacherMajorsAsync(context, users, specializations);
        var administrativeClasses = await SeedAdministrativeClassesAsync(
            context,
            hcmCampus,
            programs,
            users
        );
        await EnsureStudentsHaveTrainingProgramsAsync(
            context,
            hcmCampus,
            programs,
            administrativeClasses
        );
        await SeedTeachingCoursesAsync(
            context,
            hcmCampus,
            subjects,
            terms,
            users,
            administrativeClasses
        );
        var shifts = await SeedClassShiftsAsync(context);
        await SeedTeachingPreferencesAsync(context, hcmCampus, terms, users, shifts);
        await SeedParentLinkAsync(context, users);
        await SeedFacilitiesAsync(context, hcmCampus);
        await SeedScheduleTemplatesAsync(
            context,
            hcmCampus,
            subjects,
            terms,
            administrativeClasses,
            shifts
        );
        await SeedCourseSyllabusesAsync(
            context,
            hcmCampus,
            programs,
            specializations,
            programSubjects,
            subjects
        );
        await SeedProgramTuitionConfigsAsync(context, hcmCampus, programs, terms);
        await SeedTuitionReceivingAccountAsync(context, hcmCampus);
        await SeedDeKiemTraAsync(context, subjects, terms);

        // Seed CaThi & Assign lecturer01 & student01
        await SeedCaThiTestEnvironmentAsync(context, hcmCampus);

        // Seed P15G smoke detail data for 166/166 browser smoke
        await SeedP15GSmokeDetailDataAsync(
            context,
            hcmCampus,
            subjects,
            terms,
            users,
            administrativeClasses
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedP15GSmokeDetailDataAsync(
        ApplicationDbContext context,
        DonVi campus,
        IReadOnlyDictionary<string, DanhMucMonHoc> subjects,
        IReadOnlyList<HocKy> terms,
        IReadOnlyDictionary<string, NguoiDung> users,
        IReadOnlyDictionary<string, LopHanhChinh> administrativeClasses
    )
    {
        var teacher = users.GetValueOrDefault("p12test_teacher01@lms.local");
        var bghUser = users.GetValueOrDefault("p15test_bgh01@lms.local");
        var student = users.GetValueOrDefault("p12test_student011@lms.local");
        if (teacher == null || bghUser == null || student == null) return;

        var hk3_2026 = terms.FirstOrDefault(t => t.MaCodeHocKy == "HK3_2026");
        if (hk3_2026 == null) return;

        var subjectCom103 = subjects.GetValueOrDefault("COM103");
        if (subjectCom103 == null) return;

        var subjectCom102 = subjects.GetValueOrDefault("COM102");
        var subjectWeb101 = subjects.GetValueOrDefault("WEB101");
        if (subjectCom102 == null || subjectWeb101 == null) return;

        var sd1904 = administrativeClasses.GetValueOrDefault("SD1904");
        if (sd1904 == null) return;

        student.MaDonVi = campus.MaDonVi;
        student.MaLop = sd1904.MaLop;
        teacher.MaDonVi = campus.MaDonVi;
        bghUser.MaDonVi = campus.MaDonVi;

        async Task UpsertFailingGradeAsync(
            DanhMucMonHoc subject,
            decimal processScore,
            decimal midtermScore,
            decimal finalScore,
            decimal gpa
        )
        {
            var grade = await context.DiemSos.FirstOrDefaultAsync(d =>
                d.MaHocSinh == student.MaNguoiDung &&
                d.MaMonHoc == subject.MaMonHoc &&
                d.MaHocKy == hk3_2026.MaHocKy);

            if (grade == null)
            {
                grade = new DiemSo
                {
                    MaHocSinh = student.MaNguoiDung,
                    MaHocKy = hk3_2026.MaHocKy,
                    NamNhapHoc = 2026,
                };
                context.DiemSos.Add(grade);
            }

            grade.MaDonVi = campus.MaDonVi;
            grade.MaMonHoc = subject.MaMonHoc;
            grade.DiemQuaTrinh = processScore;
            grade.DiemGiuaKy = midtermScore;
            grade.DiemCuoiKy = finalScore;
            grade.GpaMonHoc = gpa;
            grade.TrangThai = "rot";
            grade.DaKhoa = false;
            grade.NamNhapHoc = 2026;
        }

        await UpsertFailingGradeAsync(subjectCom102, 4.0m, 3.0m, 2.5m, 3.2m);
        await UpsertFailingGradeAsync(subjectWeb101, 4.5m, 3.5m, 2.5m, 3.5m);
        await UpsertFailingGradeAsync(subjectCom103, 3.0m, 2.5m, 2.0m, 2.8m);

        var existingTeacherCourse = await context.KhoaHocs.AnyAsync(k =>
            k.MaGiaoVien == teacher.MaNguoiDung &&
            k.MaLop == sd1904.MaLop &&
            k.MaHocKy == hk3_2026.MaHocKy);
        if (!existingTeacherCourse)
        {
            var web102 = subjects.GetValueOrDefault("WEB102");
            if (web102 != null)
            {
                var course = await context.KhoaHocs.FirstOrDefaultAsync(k =>
                    k.MaDonVi == campus.MaDonVi &&
                    k.MaMonHoc == web102.MaMonHoc &&
                    k.MaHocKy == hk3_2026.MaHocKy &&
                    k.MaLop == sd1904.MaLop);

                if (course == null)
                {
                    course = new KhoaHoc
                    {
                        MaDonVi = campus.MaDonVi,
                        MaMonHoc = web102.MaMonHoc,
                        MaHocKy = hk3_2026.MaHocKy,
                        MaLop = sd1904.MaLop,
                        NgayTao = DateTime.UtcNow
                    };
                    context.KhoaHocs.Add(course);
                }

                course.MaGiaoVien = teacher.MaNguoiDung;
                course.MaLopHocPhan = null;
                course.TieuDe = $"{web102.TenMonHoc} - {sd1904.MaCodeLop} - {hk3_2026.TenHocKy} - {teacher.HoTen}";
                course.MoTa = "Dữ liệu kiểm thử trình duyệt P15G cho lớp giáo viên.";
                course.TrangThai = PublishedStatus;
            }
        }

        var cauHoi = await context.CauHoiDanhGias.FirstOrDefaultAsync();
        if (cauHoi == null)
        {
            cauHoi = new CauHoiDanhGia
            {
                NoiDungCauHoi = "Chất lượng giảng dạy tổng thể",
                ConHoatDong = true
            };
            context.CauHoiDanhGias.Add(cauHoi);
            await context.SaveChangesAsync();
        }

        var existingEval = await context.DanhGiaGiaoViens.AnyAsync(g =>
            g.MaGiaoVien == teacher.MaNguoiDung &&
            g.NhanXetTuDo != null &&
            g.NhanXetTuDo.Contains("P15G smoke"));
        if (!existingEval)
        {
            context.DanhGiaGiaoViens.AddRange(
                new DanhGiaGiaoVien
                {
                    MaGiaoVien = teacher.MaNguoiDung,
                    MaHocKy = hk3_2026.MaHocKy,
                    MaCauHoiDg = cauHoi.MaCauHoiDg,
                    DiemSo = 5,
                    NhanXetTuDo = "P15G smoke: giảng dạy rõ ràng, dễ hiểu",
                    NgayTao = DateTime.UtcNow.AddDays(-3)
                },
                new DanhGiaGiaoVien
                {
                    MaGiaoVien = teacher.MaNguoiDung,
                    MaHocKy = hk3_2026.MaHocKy,
                    MaCauHoiDg = cauHoi.MaCauHoiDg,
                    DiemSo = 4,
                    NhanXetTuDo = "P15G smoke: hỗ trợ sinh viên tận tình",
                    NgayTao = DateTime.UtcNow.AddDays(-2)
                },
                new DanhGiaGiaoVien
                {
                    MaGiaoVien = teacher.MaNguoiDung,
                    MaHocKy = hk3_2026.MaHocKy,
                    MaCauHoiDg = cauHoi.MaCauHoiDg,
                    DiemSo = 5,
                    NhanXetTuDo = "P15G smoke: phản hồi bài tập nhanh chóng",
                    NgayTao = DateTime.UtcNow.AddDays(-1)
                }
            );
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedCaThiTestEnvironmentAsync(ApplicationDbContext context, DonVi hcmCampus)
    {
        var lecturer = await context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == "lecturer01@edulms.local");
        var student = await context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == "student01@edulms.local");
        
        if (lecturer == null || student == null) return;
        
        // Ensure both are in the same campus
        lecturer.MaDonVi = hcmCampus.MaDonVi;
        student.MaDonVi = hcmCampus.MaDonVi;
        await context.SaveChangesAsync();

        var examNames = new[] { 
            "Quiz Cơ sở dữ liệu", 
            "Đề thi mẫu Lập trình C#", 
            "Kiểm tra Thiết kế Web", 
            "Quiz Xây dựng REST API" 
        };

        var testPapers = await context.DeKiemTras
            .Where(d => examNames.Contains(d.TieuDe))
            .ToListAsync();

        var kyThi = await context.KyThis.FirstOrDefaultAsync(k => k.TenKyThi == "Kỳ thi thử nghiệm WebRTC");
        if (kyThi == null)
        {
            kyThi = new KyThi
            {
                TenKyThi = "Kỳ thi thử nghiệm WebRTC",
                MaHocKy = testPapers.FirstOrDefault()?.MaHocKy ?? 0,
                TrangThai = "dang_dien_ra",
                NgayTao = DateTime.UtcNow
            };
            context.KyThis.Add(kyThi);
            await context.SaveChangesAsync();
        }

        var room = await context.PhongHocs.FirstOrDefaultAsync();

        foreach (var testPaper in testPapers)
        {
            var activeLichThi = await context.LichThiTongs.FirstOrDefaultAsync(l => l.MaDeKiemTra == testPaper.MaDeKiemTra);
            if (activeLichThi == null)
            {
                activeLichThi = new LichThiTong
                {
                    MaKyThi = kyThi.MaKyThi,
                    MaMonHoc = testPaper.MaMonHoc ?? 0,
                    MaDeKiemTra = testPaper.MaDeKiemTra,
                    HinhThucThi = testPaper.HinhThucThi ?? "ket_hop",
                    NgayThiDuKien = DateTime.Today,
                    TrangThai = "da_gui_ve_co_so",
                    NgayTao = DateTime.UtcNow
                };
                context.LichThiTongs.Add(activeLichThi);
                await context.SaveChangesAsync();
            }

            var caThiName = $"Thi thử nghiệm WebRTC - {testPaper.TieuDe}";
            var caThi = await context.CaThis.FirstOrDefaultAsync(c => c.TenCaThi == caThiName);
            if (caThi == null)
            {
                caThi = new CaThi
                {
                    MaLichThiTong = activeLichThi.MaLichThiTong,
                    TenCaThi = caThiName,
                    MaPhong = room?.MaPhong,
                    NgayThi = DateTime.Today,
                    ThoiGianBatDau = DateTime.Today.AddHours(8),
                    ThoiGianKetThuc = DateTime.Today.AddDays(1).AddHours(22),
                    MaDonVi = hcmCampus.MaDonVi,
                    TrangThai = "dang_diem_danh", // Đang điểm danh thí sinh
                    NgayTao = DateTime.UtcNow
                };
                context.CaThis.Add(caThi);
                await context.SaveChangesAsync();
            }

            var isProctorAssigned = await context.PhanCongGiamThis.AnyAsync(p => p.MaCaThi == caThi.MaCaThi && p.MaGiamThi == lecturer.MaNguoiDung);
            if (!isProctorAssigned)
            {
                context.PhanCongGiamThis.Add(new PhanCongGiamThi
                {
                    MaCaThi = caThi.MaCaThi,
                    MaGiamThi = lecturer.MaNguoiDung,
                    VaiTroGiamThi = "giam_thi_chinh",
                    TrangThai = "du_kien",
                    NgayTao = DateTime.UtcNow
                });
            }

            var isStudentAssigned = await context.ThiSinhCaThis.AnyAsync(t => t.MaCaThi == caThi.MaCaThi && t.MaHocSinh == student.MaNguoiDung);
            if (!isStudentAssigned)
            {
                context.ThiSinhCaThis.Add(new ThiSinhCaThi
                {
                    MaCaThi = caThi.MaCaThi,
                    MaHocSinh = student.MaNguoiDung,
                    TrangThaiDuThi = "cho_thi",
                    NgayTao = DateTime.UtcNow
                });
            }
        }
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
            new RoleSeed(
                AuthRoles.ToDatabaseCode(AuthRoles.HoiDongQuanLyNoiDung),
                "Hội đồng quản lý nội dung"
            ),
            new RoleSeed(AuthRoles.ToDatabaseCode(AuthRoles.FinanceAdmin), "Admin tài chính"),
            new RoleSeed(AuthRoles.ToDatabaseCode(AuthRoles.CampusAccountant), "Kế toán cơ sở"),
            new RoleSeed(
                AuthRoles.ToDatabaseCode(AuthRoles.CampusChiefAccountant),
                "Kế toán trưởng cơ sở"
            ),
        };

        var nextRoleId = ((await context.VaiTros.MaxAsync(x => (int?)x.MaVaiTro)) ?? 0) + 1;

        foreach (var plan in rolePlans)
        {
            var role = await context.VaiTros.FirstOrDefaultAsync(x => x.MaCodeVaiTro == plan.Code);

            if (role is null)
            {
                role = new VaiTro { MaVaiTro = nextRoleId++, MaCodeVaiTro = plan.Code };

                context.VaiTros.Add(role);
            }

            role.TenVaiTro = plan.Name;
        }

        await context.SaveChangesAsync();
    }

    private static async Task<DonVi> GetOrCreateRootCampusAsync(ApplicationDbContext context)
    {
        var root =
            await context.DonVis.FirstOrDefaultAsync(x => x.CapDonVi == RootLevel)
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
        DonVi rootCampus
    )
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
        CampusSeed plan
    )
    {
        var campus = await context.DonVis.FirstOrDefaultAsync(x => x.TenDonVi == plan.Name);

        if (campus is null && plan.Aliases.Length > 0)
        {
            campus = await context.DonVis.FirstOrDefaultAsync(x =>
                plan.Aliases.Contains(x.TenDonVi)
            );
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
        ApplicationDbContext context
    )
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
                x.MaCodeNganh == plan.Code
            );

            if (major is null)
            {
                major = new NganhDaoTao { MaCodeNganh = plan.Code, NgayTao = DateTime.UtcNow };

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
        IReadOnlyDictionary<string, NganhDaoTao> majors
    )
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
            var major = majors[plan.MajorCode];
            var specialization = await context.ChuyenNganhs.FirstOrDefaultAsync(x =>
                x.MaNganh == major.MaNganh && x.TenChuyenNganh == plan.Name
            );

            if (specialization is null)
            {
                specialization = new ChuyenNganh
                {
                    MaNganh = major.MaNganh,
                    NgayTao = DateTime.UtcNow,
                };

                context.ChuyenNganhs.Add(specialization);
            }

            specialization.MaNganh = major.MaNganh;
            specialization.TenChuyenNganh = plan.Name;
            specialization.MoTa = $"Chuyên ngành {plan.Name} thuộc ngành {major.TenNganh}.";
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
        IEnumerable<ChuyenNganh> specializations
    )
    {
        foreach (var specialization in specializations)
        {
            var campusSpecialization = await context.ChuyenNganhTheoCoSos.FirstOrDefaultAsync(x =>
                x.MaDonVi == campus.MaDonVi && x.MaChuyenNganh == specialization.MaChuyenNganh
            );

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
            campusSpecialization.GhiChu =
                $"Mở chuyên ngành {specialization.TenChuyenNganh} tại {campus.TenDonVi}.";
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
            cohort = new KhoaTuyenSinh { MaCodeKhoa = "K2026", NgayTao = DateTime.UtcNow };

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
        DonVi campus
    )
    {
        var termPlans = new[]
        {
            new AcademicTermSeed(
                "HK1_2026",
                "Học kỳ 1 năm 2026",
                "2026",
                1,
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 4, 30)
            ),
            new AcademicTermSeed(
                "HK2_2026",
                "Học kỳ 2 năm 2026",
                "2026",
                2,
                new DateOnly(2026, 5, 1),
                new DateOnly(2026, 8, 31)
            ),
            new AcademicTermSeed(
                "HK3_2026",
                "Học kỳ 3 năm 2026",
                "2026",
                3,
                new DateOnly(2026, 9, 1),
                new DateOnly(2026, 12, 31)
            ),
            new AcademicTermSeed(
                "HK1_2027",
                "Học kỳ 1 năm 2027",
                "2027",
                1,
                new DateOnly(2027, 1, 1),
                new DateOnly(2027, 4, 30)
            ),
            new AcademicTermSeed(
                "HK2_2027",
                "Học kỳ 2 năm 2027",
                "2027",
                2,
                new DateOnly(2027, 5, 1),
                new DateOnly(2027, 8, 31)
            ),
            new AcademicTermSeed(
                "HK3_2027",
                "Học kỳ 3 năm 2027",
                "2027",
                3,
                new DateOnly(2027, 9, 1),
                new DateOnly(2027, 12, 31)
            ),
            new AcademicTermSeed(
                "HK1_2028",
                "Học kỳ 1 năm 2028",
                "2028",
                1,
                new DateOnly(2028, 1, 1),
                new DateOnly(2028, 4, 30)
            ),
            new AcademicTermSeed(
                "HK2_2028",
                "Học kỳ 2 năm 2028",
                "2028",
                2,
                new DateOnly(2028, 5, 1),
                new DateOnly(2028, 8, 31)
            ),
            new AcademicTermSeed(
                "HK3_2028",
                "Học kỳ 3 năm 2028",
                "2028",
                3,
                new DateOnly(2028, 9, 1),
                new DateOnly(2028, 12, 31)
            ),
        };

        var terms = new List<HocKy>();

        foreach (var plan in termPlans)
        {
            var term = await context.HocKys.FirstOrDefaultAsync(x =>
                x.MaDonVi == campus.MaDonVi && x.MaCodeHocKy == plan.Code
            );

            if (term is null)
            {
                term = await context.HocKys.FirstOrDefaultAsync(x =>
                    x.MaDonVi == campus.MaDonVi
                    && x.NamHoc == plan.Year
                    && x.ThuTuTrongNam == plan.TermOrderInYear
                );
            }

            if (term is null)
            {
                term = new HocKy { MaDonVi = campus.MaDonVi, MaCodeHocKy = plan.Code };

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
        ApplicationDbContext context
    )
    {
        var subjectPlans = new[]
        {
            new SubjectSeed("CTDL101", "Cấu trúc dữ liệu & Giải thuật", 3),
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
            new SubjectSeed("MOB101", "Lập trình ứng dụng di động", 3),
            new SubjectSeed("DEV201", "DevOps và triển khai phần mềm", 3),
            new SubjectSeed("SEC101", "An toàn thông tin căn bản", 3),
            new SubjectSeed("CLOUD101", "Điện toán đám mây", 3),
            new SubjectSeed("CAP101", "Đồ án tốt nghiệp phần mềm", 4),
            new SubjectSeed("INT101", "Thực tập doanh nghiệp CNTT", 4),
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
            new SubjectSeed("DES111", "Thiết kế sản phẩm số", 3),
            new SubjectSeed("DES112", "Nghiên cứu người dùng", 3),
            new SubjectSeed("DES113", "3D căn bản", 3),
            new SubjectSeed("DES114", "Thiết kế hệ thống thương hiệu", 3),
            new SubjectSeed("DES115", "Đồ án tốt nghiệp thiết kế", 4),
            new SubjectSeed("DES116", "Thực tập doanh nghiệp thiết kế", 4),
            new SubjectSeed("MKT101", "Marketing căn bản", 3),
            new SubjectSeed("MKT102", "Hành vi khách hàng", 3),
            new SubjectSeed("MKT103", "Digital Marketing", 3),
            new SubjectSeed("MKT104", "Content Marketing", 3),
            new SubjectSeed("MKT105", "SEO căn bản", 2),
            new SubjectSeed("MKT106", "Quảng cáo mạng xã hội", 3),
            new SubjectSeed("MKT107", "Marketing Analytics", 3),
            new SubjectSeed("MKT108", "Kỹ năng bán hàng", 2),
            new SubjectSeed("MKT109", "Xây dựng thương hiệu", 3),
            new SubjectSeed("MKT110", "Dự án Marketing tổng hợp", 3),
            new SubjectSeed("MKT111", "Marketing automation", 3),
            new SubjectSeed("MKT112", "Quản trị quan hệ khách hàng", 3),
            new SubjectSeed("MKT113", "Nghiên cứu thị trường", 3),
            new SubjectSeed("MKT114", "Chiến lược truyền thông tích hợp", 3),
            new SubjectSeed("MKT115", "Đồ án tốt nghiệp Marketing", 4),
            new SubjectSeed("MKT116", "Thực tập doanh nghiệp Marketing", 4),
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
                x.MaCodeMonHoc == plan.Code
            );

            if (subject is null)
            {
                subject = new DanhMucMonHoc { MaCodeMonHoc = plan.Code };

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

    private static async Task SeedLearningContentAsync(
        ApplicationDbContext context,
        IReadOnlyDictionary<string, DanhMucMonHoc> subjects
    )
    {
        var chapterPlansBySubject = new Dictionary<string, StandardChapterSeed[]>(
            StringComparer.OrdinalIgnoreCase
        )
        {
            ["GEN101"] =
            [
                new(
                    1,
                    "Giới thiệu kỹ năng học tập",
                    [
                        new(1, "Cách quản lý thời gian", "video", "/demo/lessons/gen101/time.mp4", 900, null),
                        new(2, "Lập kế hoạch học tập", "van_ban", null, null, "Thực hành lập kế hoạch học tập cho tuần này.")
                    ]
                )
            ],
            ["GEN102"] =
            [
                new(
                    1,
                    "Tin học cơ bản",
                    [
                        new(1, "Sử dụng hệ điều hành", "video", "/demo/lessons/gen102/os.mp4", 900, null),
                        new(2, "Soạn thảo văn bản", "van_ban", null, null, "Soạn thảo văn bản cơ bản.")
                    ]
                )
            ],
            ["COM101"] =
            [
                new(
                    1,
                    "Nhập môn lập trình",
                    [
                        new(1, "Thuật toán và lưu đồ", "video", "/demo/lessons/com101/algorithm.mp4", 1200, null),
                        new(2, "Bài tập thuật toán", "van_ban", null, null, "Thực hành vẽ lưu đồ.")
                    ]
                )
            ],
            ["CTDL101"] =
            [
                new(
                    1,
                    "Ôn tập nền tảng lập trình",
                    [
                        new(
                            1,
                            "Con trỏ và cấp phát động",
                            "video",
                            "/demo/lessons/ctdl101/contro.mp4",
                            1104,
                            null
                        ),
                        new(
                            2,
                            "Struct và Class cơ bản",
                            "video",
                            "/demo/lessons/ctdl101/struct.mp4",
                            1330,
                            null
                        ),
                        new(
                            3,
                            "Quiz ôn tập chương 1",
                            "trac_nghiem",
                            "/demo/lessons/ctdl101/quiz1.pdf",
                            null,
                            null
                        ),
                    ]
                ),
                new(
                    2,
                    "Cấu trúc dữ liệu tuyến tính",
                    [
                        new(
                            1,
                            "Danh sách liên kết đơn",
                            "video",
                            "/demo/lessons/ctdl101/dslk.mp4",
                            1590,
                            null
                        ),
                        new(
                            2,
                            "Stack và ứng dụng",
                            "video",
                            "/demo/lessons/ctdl101/stack.mp4",
                            1245,
                            null
                        ),
                        new(
                            3,
                            "Queue và vòng lặp",
                            "video",
                            "/demo/lessons/ctdl101/queue.mp4",
                            1440,
                            null
                        ),
                        new(
                            4,
                            "Bài tập thực hành",
                            "van_ban",
                            "/demo/lessons/ctdl101/bt1.pdf",
                            null,
                            null
                        ),
                    ]
                ),
                new(3, "Cây nhị phân và đồ thị", []),
                new(4, "Sắp xếp & Tìm kiếm", []),
            ],
            ["COM103"] =
            [
                new(
                    1,
                    "Tổng quan C# và .NET",
                    [
                        new(
                            1,
                            "Cài đặt môi trường .NET SDK và IDE",
                            "video",
                            "/demo/lessons/com103/setup-dotnet.mp4",
                            900,
                            null
                        ),
                        new(
                            2,
                            "Cấu trúc chương trình C# đầu tiên",
                            "pdf",
                            "/demo/lessons/com103/chuong-trinh-dau-tien.pdf",
                            null,
                            null
                        ),
                    ]
                ),
                new(
                    2,
                    "Cú pháp nền tảng",
                    [
                        new(
                            1,
                            "Biến, kiểu dữ liệu và toán tử",
                            "video",
                            "/demo/lessons/com103/bien-kieu-du-lieu.mp4",
                            780,
                            null
                        ),
                        new(
                            2,
                            "Câu lệnh điều kiện và vòng lặp",
                            "van_ban",
                            null,
                            null,
                            "Thực hành if/else, switch, for, while và foreach trong ứng dụng console."
                        ),
                    ]
                ),
            ],
        };

        foreach (var (subjectCode, chapterPlans) in chapterPlansBySubject)
        {
            var subject = subjects[subjectCode];

            foreach (var chapterPlan in chapterPlans)
            {
                var chapter = await context.Chuongs.FirstOrDefaultAsync(x =>
                    x.MaMonHoc == subject.MaMonHoc && x.ThuTu == chapterPlan.Order
                );

                if (chapter is null)
                {
                    chapter = new Chuong { MaMonHoc = subject.MaMonHoc };

                    context.Chuongs.Add(chapter);
                }

                chapter.TieuDe = chapterPlan.Title;
                chapter.ThuTu = chapterPlan.Order;
                chapter.DaAn = false;

                await context.SaveChangesAsync();

                foreach (var lessonPlan in chapterPlan.Lessons)
                {
                    var lesson = await context.BaiHocs.FirstOrDefaultAsync(x =>
                        x.MaChuong == chapter.MaChuong && x.ThuTu == lessonPlan.Order
                    );

                    if (lesson is null)
                    {
                        lesson = new BaiHoc { MaChuong = chapter.MaChuong };

                        context.BaiHocs.Add(lesson);
                    }

                    lesson.TieuDe = lessonPlan.Title;
                    lesson.LoaiBaiHoc = lessonPlan.LessonType;
                    lesson.UrlTapTin = lessonPlan.FileUrl;
                    lesson.ThoiLuongGiay = lessonPlan.DurationSeconds;
                    lesson.NoiDungVanBan = lessonPlan.TextContent;
                    lesson.DieuKienMoKhoa = null;
                    lesson.TomTatAi = null;
                    lesson.ThuTu = lessonPlan.Order;
                    lesson.DaAn = false;
                }
            }
        }

        var assignmentPlans = new[]
        {
            new StandardAssignmentSeed(
                "COM103",
                "Bài tập 1 - Console App quản lý sinh viên",
                "Xây dựng ứng dụng console C# quản lý danh sách sinh viên, nhập/xuất dữ liệu và tìm kiếm theo mã sinh viên.",
                new DateTime(2026, 4, 25, 23, 59, 0, DateTimeKind.Utc),
                3,
                [".pdf", ".docx", ".zip"],
                "Chấm theo tính đúng chức năng, cấu trúc code, xử lý lỗi nhập liệu và báo cáo ngắn.",
                PublishedStatus
            ),
        };

        foreach (var plan in assignmentPlans)
        {
            var subject = subjects[plan.SubjectCode];
            var assignment = await context.BaiTaps.FirstOrDefaultAsync(x =>
                x.MaMonHoc == subject.MaMonHoc && x.TieuDe == plan.Title
            );

            if (assignment is null)
            {
                assignment = new BaiTap { MaMonHoc = subject.MaMonHoc };

                context.BaiTaps.Add(assignment);
            }

            assignment.TieuDe = plan.Title;
            assignment.MoTa = plan.Description;
            assignment.HanNop = plan.DueAt;
            assignment.SoLanNopToiDa = plan.MaxSubmissions;
            assignment.DinhDangChoPhep = JsonSerializer.Serialize(plan.AllowedFormats, JsonOptions);
            assignment.HuongDanChamDiem = plan.GradingGuide;
            assignment.TrangThai = plan.Status;
        }

        await context.SaveChangesAsync();
    }

    private static async Task<Dictionary<string, ChuongTrinhDaoTao>> SeedTrainingProgramsAsync(
        ApplicationDbContext context,
        KhoaTuyenSinh cohort,
        IReadOnlyDictionary<string, ChuyenNganh> specializations
    )
    {
        var programPlans = new[]
        {
            new TrainingProgramSeed(
                "CT_CNTT_K2026",
                "Chương trình Công nghệ thông tin K2026",
                "CNTT_PTPM",
                122,
                "Chương trình đào tạo K2026 cho ngành Công nghệ thông tin."
            ),
            new TrainingProgramSeed(
                "CT_TKDH_K2026",
                "Chương trình Thiết kế đồ họa K2026",
                "TKDH_UIUX",
                118,
                "Chương trình đào tạo K2026 cho ngành Thiết kế đồ họa."
            ),
            new TrainingProgramSeed(
                "CT_MKT_K2026",
                "Chương trình Marketing K2026",
                "MKT_DIGITAL",
                116,
                "Chương trình đào tạo K2026 cho ngành Marketing."
            ),
        };

        var result = new Dictionary<string, ChuongTrinhDaoTao>(StringComparer.OrdinalIgnoreCase);

        foreach (var plan in programPlans)
        {
            var program = await context.ChuongTrinhDaoTaos.FirstOrDefaultAsync(x =>
                x.MaCodeChuongTrinh == plan.Code
            );

            if (program is null)
            {
                program = await context.ChuongTrinhDaoTaos.FirstOrDefaultAsync(x =>
                    x.MaChuyenNganh == specializations[plan.SpecializationCode].MaChuyenNganh
                    && x.MaKhoaTuyenSinh == cohort.MaKhoaTuyenSinh
                    && x.Version == ProgramVersion
                );
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
        IReadOnlyList<HocKy> terms
    )
    {
        foreach (var program in programs)
        {
            for (var index = 0; index < terms.Count; index++)
            {
                var term = terms[index];
                var programTermOrder = index + 1;
                var mapping = await context.ChuongTrinhHocKys.FirstOrDefaultAsync(x =>
                    x.MaChuongTrinh == program.MaChuongTrinh && x.ThuTuHocKy == programTermOrder
                );

                if (mapping is null)
                {
                    mapping = await context.ChuongTrinhHocKys.FirstOrDefaultAsync(x =>
                        x.MaChuongTrinh == program.MaChuongTrinh && x.MaHocKy == term.MaHocKy
                    );
                }

                if (mapping is null)
                {
                    mapping = new ChuongTrinhHocKy { MaChuongTrinh = program.MaChuongTrinh };

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
        IReadOnlyDictionary<string, DanhMucMonHoc> subjects
    )
    {
        var planByProgram = new Dictionary<string, ProgramSubjectSeed[]>(
            StringComparer.OrdinalIgnoreCase
        )
        {
            ["CT_CNTT_K2026"] =
            [
                new(1, "GEN101"),
                new(1, "GEN102"),
                new(1, "COM101"),
                new(2, "COM102"),
                new(2, "WEB101"),
                new(2, "GEN104"),
                new(3, "COM103"),
                new(3, "WEB102"),
                new(3, "DBI101"),
                new(4, "API101"),
                new(4, "FE101"),
                new(5, "BE101"),
                new(5, "PRO101"),
                new(6, "GEN103"),
                new(6, "GEN105"),
                new(7, "MOB101"),
                new(7, "DEV201"),
                new(8, "SEC101"),
                new(8, "CLOUD101"),
                new(9, "CAP101"),
                new(9, "INT101"),
            ],
            ["CT_TKDH_K2026"] =
            [
                new(1, "GEN101"),
                new(1, "DES101"),
                new(1, "DES102"),
                new(2, "DES103"),
                new(2, "DES105"),
                new(2, "GEN104"),
                new(3, "DES104"),
                new(3, "DES106"),
                new(3, "DES107"),
                new(4, "DES108"),
                new(4, "DES109"),
                new(5, "DES110"),
                new(5, "GEN102"),
                new(6, "GEN103"),
                new(6, "GEN105"),
                new(7, "DES111"),
                new(7, "DES112"),
                new(8, "DES113"),
                new(8, "DES114"),
                new(9, "DES115"),
                new(9, "DES116"),
            ],
            ["CT_MKT_K2026"] =
            [
                new(1, "GEN101"),
                new(1, "MKT101"),
                new(1, "MKT102"),
                new(2, "MKT103"),
                new(2, "MKT104"),
                new(2, "GEN104"),
                new(3, "MKT105"),
                new(3, "MKT106"),
                new(3, "MKT109"),
                new(4, "MKT107"),
                new(4, "MKT108"),
                new(5, "MKT110"),
                new(5, "GEN102"),
                new(6, "GEN103"),
                new(6, "GEN105"),
                new(7, "MKT111"),
                new(7, "MKT112"),
                new(8, "MKT113"),
                new(8, "MKT114"),
                new(9, "MKT115"),
                new(9, "MKT116"),
            ],
        };

        var result = new Dictionary<string, MonHocTrongChuongTrinh>(
            StringComparer.OrdinalIgnoreCase
        );

        foreach (var (programCode, subjectPlans) in planByProgram)
        {
            var program = programs[programCode];
            var order = 1;

            foreach (var plan in subjectPlans)
            {
                var subject = subjects[plan.SubjectCode];
                var programSubject = await context.MonHocTrongChuongTrinhs.FirstOrDefaultAsync(x =>
                    x.MaChuongTrinh == program.MaChuongTrinh && x.MaMonHoc == subject.MaMonHoc
                );

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
                programSubject.GhiChu =
                    $"Môn {subject.MaCodeMonHoc} trong {program.TenChuongTrinh}.";
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
        DonVi hcmCampus
    )
    {
        var userPlans = new[]
        {
            new DemoUserSeed(
                "superadmin@lms.local",
                "Super Admin",
                AuthRoles.ToDatabaseCode(AuthRoles.SuperAdmin),
                rootCampus.MaDonVi
            ),
            new DemoUserSeed(
                "admin@edulms.local",
                "Quản trị hệ thống",
                AuthRoles.ToDatabaseCode(AuthRoles.Admin),
                rootCampus.MaDonVi
            ),
            new DemoUserSeed(
                "admin@lms.local",
                "Admin Hệ Thống",
                AuthRoles.ToDatabaseCode(AuthRoles.Admin),
                rootCampus.MaDonVi
            ),
            new DemoUserSeed(
                "campusadmin.hcm@lms.local",
                "Admin Cơ Sở Hồ Chí Minh",
                AuthRoles.ToDatabaseCode(AuthRoles.CampusAdmin),
                hcmCampus.MaDonVi
            ),
            new DemoUserSeed(
                "giaovu.hcm@lms.local",
                "Giáo Vụ Hồ Chí Minh",
                AuthRoles.ToDatabaseCode(AuthRoles.AcademicStaff),
                hcmCampus.MaDonVi
            ),
            new DemoUserSeed(
                "p12test_staff01@lms.local",
                "P12 Test Giáo Vụ",
                AuthRoles.ToDatabaseCode(AuthRoles.AcademicStaff),
                hcmCampus.MaDonVi,
                Password: "Test@123"
            ),
            new DemoUserSeed(
                "daotao@edulms.local",
                "Phòng Đào Tạo",
                AuthRoles.ToDatabaseCode(AuthRoles.AcademicStaff),
                hcmCampus.MaDonVi
            ),
            new DemoUserSeed(
                "khoa@edulms.local",
                "Cán bộ Khoa",
                AuthRoles.ToDatabaseCode(AuthRoles.AcademicStaff),
                hcmCampus.MaDonVi
            ),
            new DemoUserSeed(
                "bomon@edulms.local",
                "Cán bộ Bộ môn",
                AuthRoles.ToDatabaseCode(AuthRoles.AcademicStaff),
                hcmCampus.MaDonVi
            ),
            new DemoUserSeed(
                "principal@lms.local",
                "Ban Giám Hiệu",
                AuthRoles.ToDatabaseCode(AuthRoles.Principal),
                hcmCampus.MaDonVi
            ),
            new DemoUserSeed(
                "bgh@edulms.local",
                "Ban Giám Hiệu",
                AuthRoles.ToDatabaseCode(AuthRoles.Principal),
                hcmCampus.MaDonVi
            ),
            new DemoUserSeed(
                "p15test_bgh01@lms.local",
                "P15 Test Ban Giám Hiệu",
                AuthRoles.ToDatabaseCode(AuthRoles.Principal),
                hcmCampus.MaDonVi,
                Password: "Test@123"
            ),
            new DemoUserSeed(
                "teacher.cntt@lms.local",
                "Nguyễn Văn Lập Trình",
                AuthRoles.ToDatabaseCode(AuthRoles.Teacher),
                hcmCampus.MaDonVi
            ),
            new DemoUserSeed(
                "p12test_teacher01@lms.local",
                "P12 Test Giảng Viên",
                AuthRoles.ToDatabaseCode(AuthRoles.Teacher),
                hcmCampus.MaDonVi,
                Password: "Test@123"
            ),
            new DemoUserSeed(
                "lecturer01@edulms.local",
                "Trần Thị Giảng Viên",
                AuthRoles.ToDatabaseCode(AuthRoles.Teacher),
                hcmCampus.MaDonVi
            ),
            new DemoUserSeed(
                "teacher.tkdh@lms.local",
                "Trần Thị Thiết Kế",
                AuthRoles.ToDatabaseCode(AuthRoles.Teacher),
                hcmCampus.MaDonVi
            ),
            new DemoUserSeed(
                "teacher.mkt@lms.local",
                "Lê Văn Marketing",
                AuthRoles.ToDatabaseCode(AuthRoles.Teacher),
                hcmCampus.MaDonVi
            ),
            new DemoUserSeed(
                "teacher.csharp.a@lms.local",
                "Nguyễn Văn An",
                AuthRoles.ToDatabaseCode(AuthRoles.Teacher),
                hcmCampus.MaDonVi
            ),
            new DemoUserSeed(
                "teacher.csharp.b@lms.local",
                "Trần Thị Bình",
                AuthRoles.ToDatabaseCode(AuthRoles.Teacher),
                hcmCampus.MaDonVi
            ),
            new DemoUserSeed(
                "teacher.database.c@lms.local",
                "Phạm Minh Cường",
                AuthRoles.ToDatabaseCode(AuthRoles.Teacher),
                hcmCampus.MaDonVi
            ),
            new DemoUserSeed(
                "teacher.database.d@lms.local",
                "Đỗ Thị Dung",
                AuthRoles.ToDatabaseCode(AuthRoles.Teacher),
                hcmCampus.MaDonVi
            ),
            new DemoUserSeed(
                "teacher.marketing.e@lms.local",
                "Lê Thị Em",
                AuthRoles.ToDatabaseCode(AuthRoles.Teacher),
                hcmCampus.MaDonVi
            ),
            new DemoUserSeed(
                "student.cntt01@lms.local",
                "Nguyễn Văn Sinh Viên CNTT",
                AuthRoles.ToDatabaseCode(AuthRoles.Student),
                hcmCampus.MaDonVi,
                2026
            ),
            new DemoUserSeed(
                "p12test_student011@lms.local",
                "P12 Test Sinh Viên",
                AuthRoles.ToDatabaseCode(AuthRoles.Student),
                hcmCampus.MaDonVi,
                2026,
                "Test@123"
            ),
            new DemoUserSeed(
                "student01@edulms.local",
                "Nguyễn Văn An",
                AuthRoles.ToDatabaseCode(AuthRoles.Student),
                hcmCampus.MaDonVi,
                2026
            ),
            new DemoUserSeed(
                "student.tkdh01@lms.local",
                "Trần Thị Sinh Viên Thiết Kế",
                AuthRoles.ToDatabaseCode(AuthRoles.Student),
                hcmCampus.MaDonVi,
                2026
            ),
            new DemoUserSeed(
                "student.mkt01@lms.local",
                "Lê Văn Sinh Viên Marketing",
                AuthRoles.ToDatabaseCode(AuthRoles.Student),
                hcmCampus.MaDonVi,
                2026
            ),
            new DemoUserSeed(
                "parent01@lms.local",
                "Phụ Huynh Demo",
                AuthRoles.ToDatabaseCode(AuthRoles.Parent),
                hcmCampus.MaDonVi
            ),
            new DemoUserSeed(
                "p15test_parent01@lms.local",
                "P15 Test Phụ Huynh",
                AuthRoles.ToDatabaseCode(AuthRoles.Parent),
                hcmCampus.MaDonVi,
                Password: "Test@123"
            ),
            new DemoUserSeed(
                "hoidong.quanly.noidung@lms.local",
                "Hội Đồng Quản Lý Nội Dung",
                AuthRoles.ToDatabaseCode(AuthRoles.HoiDongQuanLyNoiDung),
                hcmCampus.MaDonVi
            ),
            new DemoUserSeed(
                "p15test_content01@lms.local",
                "P15 Test Hội Đồng Nội Dung",
                AuthRoles.ToDatabaseCode(AuthRoles.HoiDongQuanLyNoiDung),
                hcmCampus.MaDonVi,
                Password: "Test@123"
            ),
        };

        var roles = await context.VaiTros.ToDictionaryAsync(
            x => x.MaCodeVaiTro,
            StringComparer.OrdinalIgnoreCase
        );
        var result = new Dictionary<string, NguoiDung>(StringComparer.OrdinalIgnoreCase);

        foreach (var plan in userPlans)
        {
            var user = await context.NguoiDungs.FirstOrDefaultAsync(x => x.Email == plan.Email);

            if (user is null)
            {
                user = new NguoiDung { Email = plan.Email, NgayTao = DateTime.UtcNow };

                context.NguoiDungs.Add(user);
            }

            user.MaDonVi = plan.CampusId;
            user.HoTen = plan.FullName;
            user.VaiTroChinh = plan.RoleCode;
            user.NamNhapHoc = plan.EnrollmentYear;
            user.TrangThai = UserStatuses.DbActive;
            user.MatKhauHash = PasswordHelper.HashPassword(plan.Password ?? DefaultPassword);
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
                x.MaNguoiDung == user.MaNguoiDung && x.MaVaiTro == role.MaVaiTro
            );

            if (!hasAssignment)
            {
                context.PhanQuyenNguoiDungs.Add(
                    new PhanQuyenNguoiDung
                    {
                        MaNguoiDung = user.MaNguoiDung,
                        MaVaiTro = role.MaVaiTro,
                        NgayGan = DateTime.UtcNow,
                    }
                );
            }
        }

        await context.SaveChangesAsync();
        return result;
    }

    private static async Task<Dictionary<string, LopHanhChinh>> SeedAdministrativeClassesAsync(
        ApplicationDbContext context,
        DonVi campus,
        IReadOnlyDictionary<string, ChuongTrinhDaoTao> programs,
        IReadOnlyDictionary<string, NguoiDung> users
    )
    {
        var classPlans = new[]
        {
            new AdministrativeClassSeed(
                "SD1901",
                "SD1901 - Công nghệ thông tin K2026",
                "CT_CNTT_K2026",
                "teacher.csharp.a@lms.local",
                "student.cntt01@lms.local"
            ),
            new AdministrativeClassSeed(
                "SD1902",
                "SD1902 - Công nghệ thông tin K2026",
                "CT_CNTT_K2026",
                "teacher.csharp.a@lms.local",
                null
            ),
            new AdministrativeClassSeed(
                "SD1903",
                "SD1903 - Công nghệ thông tin K2026",
                "CT_CNTT_K2026",
                "teacher.csharp.a@lms.local",
                null
            ),
            new AdministrativeClassSeed(
                "SD1904",
                "SD1904 - Công nghệ thông tin K2026",
                "CT_CNTT_K2026",
                "teacher.csharp.b@lms.local",
                null
            ),
            new AdministrativeClassSeed(
                "SD1905",
                "SD1905 - Công nghệ thông tin K2026",
                "CT_CNTT_K2026",
                "teacher.csharp.b@lms.local",
                null
            ),
            new AdministrativeClassSeed(
                "SD1906",
                "SD1906 - Công nghệ thông tin K2026",
                "CT_CNTT_K2026",
                "teacher.database.c@lms.local",
                null
            ),
            new AdministrativeClassSeed(
                "TKDH1901",
                "TKDH1901 - Thiết kế đồ họa K2026",
                "CT_TKDH_K2026",
                "teacher.tkdh@lms.local",
                "student.tkdh01@lms.local"
            ),
            new AdministrativeClassSeed(
                "MKT1901",
                "MKT1901 - Marketing K2026",
                "CT_MKT_K2026",
                "teacher.marketing.e@lms.local",
                "student.mkt01@lms.local"
            ),
            new AdministrativeClassSeed(
                "MKT1902",
                "MKT1902 - Marketing K2026",
                "CT_MKT_K2026",
                "teacher.marketing.e@lms.local",
                null
            ),
        };

        var result = new Dictionary<string, LopHanhChinh>(StringComparer.OrdinalIgnoreCase);

        foreach (var plan in classPlans)
        {
            var administrativeClass = await context.LopHanhChinhs.FirstOrDefaultAsync(x =>
                x.MaCodeLop == plan.Code
            );

            if (administrativeClass is null)
            {
                administrativeClass = new LopHanhChinh { MaCodeLop = plan.Code };

                context.LopHanhChinhs.Add(administrativeClass);
            }

            administrativeClass.MaDonVi = campus.MaDonVi;
            administrativeClass.TenLop = plan.Name;
            administrativeClass.MaGiaoVienChuNhiem = users[plan.TeacherEmail].MaNguoiDung;
            administrativeClass.MaChuongTrinh = programs[plan.ProgramCode].MaChuongTrinh;
            administrativeClass.NamNhapHoc = 2026;
            administrativeClass.ConHoatDong = true;
            result[plan.Code] = administrativeClass;

            await context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(plan.StudentEmail))
            {
                var student = users[plan.StudentEmail];
                student.MaLop = administrativeClass.MaLop;
                student.NamNhapHoc = 2026;
            }
        }

        await context.SaveChangesAsync();
        return result;
    }

    private static async Task EnsureStudentsHaveTrainingProgramsAsync(
        ApplicationDbContext context,
        DonVi campus,
        IReadOnlyDictionary<string, ChuongTrinhDaoTao> programs,
        IReadOnlyDictionary<string, LopHanhChinh> administrativeClasses
    )
    {
        var defaultClass = administrativeClasses["SD1901"];
        var students = await context.NguoiDungs
            .Where(user => user.VaiTroChinh == AuthRoles.ToDatabaseCode(AuthRoles.Student))
            .ToListAsync();

        foreach (var student in students)
        {
            student.MaDonVi = campus.MaDonVi;
            student.NamNhapHoc ??= 2026;

            if (student.MaLop is null)
            {
                student.MaLop = defaultClass.MaLop;
            }
        }

        await context.SaveChangesAsync();

        var studentClassIds = students
            .Where(student => student.MaLop.HasValue)
            .Select(student => student.MaLop!.Value)
            .Distinct()
            .ToList();
        var classes = await context.LopHanhChinhs
            .Where(classEntity => studentClassIds.Contains(classEntity.MaLop))
            .ToListAsync();

        foreach (var classEntity in classes)
        {
            if (classEntity.MaChuongTrinh.HasValue)
            {
                continue;
            }

            var programCode = ResolveProgramCodeForClass(classEntity.MaCodeLop);
            classEntity.MaDonVi = campus.MaDonVi;
            classEntity.MaChuongTrinh = programs[programCode].MaChuongTrinh;
            classEntity.NamNhapHoc ??= 2026;
            classEntity.ConHoatDong = true;
        }

        await context.SaveChangesAsync();
    }

    private static string ResolveProgramCodeForClass(string classCode)
    {
        if (classCode.StartsWith("TKDH", StringComparison.OrdinalIgnoreCase))
        {
            return "CT_TKDH_K2026";
        }

        if (classCode.StartsWith("MKT", StringComparison.OrdinalIgnoreCase))
        {
            return "CT_MKT_K2026";
        }

        return "CT_CNTT_K2026";
    }

    private static async Task<Dictionary<string, LopHocPhan>> SeedCourseSectionsAsync(
        ApplicationDbContext context,
        DonVi campus,
        IReadOnlyDictionary<string, DanhMucMonHoc> subjects,
        IReadOnlyList<HocKy> terms
    )
    {
        var termsByCode = terms.ToDictionary(x => x.MaCodeHocKy, StringComparer.OrdinalIgnoreCase);
        var sectionPlans = new[]
        {
            new CourseSectionSeed("SD1809-COM103-HK1-2026", "COM103", "HK1_2026", 35, 15, 4),
        };

        var result = new Dictionary<string, LopHocPhan>(StringComparer.OrdinalIgnoreCase);

        foreach (var plan in sectionPlans)
        {
            var subject = subjects[plan.SubjectCode];
            var term = termsByCode[plan.TermCode];
            var section = await context.LopHocPhans.FirstOrDefaultAsync(x =>
                x.MaCodeLopHocPhan == plan.Code
            );

            if (section is null)
            {
                section = new LopHocPhan { MaCodeLopHocPhan = plan.Code };

                context.LopHocPhans.Add(section);
            }

            section.MaDonVi = campus.MaDonVi;
            section.MaMonHoc = subject.MaMonHoc;
            section.MaHocKy = term.MaHocKy;
            section.SucChua = plan.Capacity;
            section.SoDangKyToiThieu = plan.MinimumEnrollment;
            section.SoDaDangKy = 0;
            section.TrangThai = ClassSectionOpenStatus;
            section.QuotaVangToiDa = plan.AbsenceQuota;
            result[plan.Code] = section;
        }

        await context.SaveChangesAsync();
        return result;
    }

    private static async Task SeedTeachingCoursesAsync(
        ApplicationDbContext context,
        DonVi campus,
        IReadOnlyDictionary<string, DanhMucMonHoc> subjects,
        IReadOnlyList<HocKy> terms,
        IReadOnlyDictionary<string, NguoiDung> users,
        IReadOnlyDictionary<string, LopHanhChinh> administrativeClasses
    )
    {
        var termsByCode = terms.ToDictionary(x => x.MaCodeHocKy, StringComparer.OrdinalIgnoreCase);
        var coursePlans = new[]
        {
            // HK1_2026 - GEN101 (Kỹ năng học tập)
            new TeachingCourseSeed("GEN101", "lecturer01@edulms.local", "HK1_2026", "SD1901", "Bản phân công giảng dạy môn Kỹ năng học tập cho lớp SD1901 trong HK1_2026."),
            new TeachingCourseSeed("GEN101", "lecturer01@edulms.local", "HK1_2026", "SD1902", "Bản phân công giảng dạy môn Kỹ năng học tập cho lớp SD1902 trong HK1_2026."),
            new TeachingCourseSeed("GEN101", "lecturer01@edulms.local", "HK1_2026", "SD1903", "Bản phân công giảng dạy môn Kỹ năng học tập cho lớp SD1903 trong HK1_2026."),
            new TeachingCourseSeed("GEN101", "lecturer01@edulms.local", "HK1_2026", "SD1904", "Bản phân công giảng dạy môn Kỹ năng học tập cho lớp SD1904 trong HK1_2026."),
            new TeachingCourseSeed("GEN101", "lecturer01@edulms.local", "HK1_2026", "SD1905", "Bản phân công giảng dạy môn Kỹ năng học tập cho lớp SD1905 trong HK1_2026."),
            new TeachingCourseSeed("GEN101", "lecturer01@edulms.local", "HK1_2026", "SD1906", "Bản phân công giảng dạy môn Kỹ năng học tập cho lớp SD1906 trong HK1_2026."),
            
            // HK1_2026 - GEN102 (Tin học cơ bản)
            new TeachingCourseSeed("GEN102", "lecturer01@edulms.local", "HK1_2026", "SD1901", "Bản phân công giảng dạy môn Tin học cơ bản cho lớp SD1901 trong HK1_2026."),
            new TeachingCourseSeed("GEN102", "lecturer01@edulms.local", "HK1_2026", "SD1902", "Bản phân công giảng dạy môn Tin học cơ bản cho lớp SD1902 trong HK1_2026."),
            new TeachingCourseSeed("GEN102", "lecturer01@edulms.local", "HK1_2026", "SD1903", "Bản phân công giảng dạy môn Tin học cơ bản cho lớp SD1903 trong HK1_2026."),
            new TeachingCourseSeed("GEN102", "lecturer01@edulms.local", "HK1_2026", "SD1904", "Bản phân công giảng dạy môn Tin học cơ bản cho lớp SD1904 trong HK1_2026."),
            new TeachingCourseSeed("GEN102", "lecturer01@edulms.local", "HK1_2026", "SD1905", "Bản phân công giảng dạy môn Tin học cơ bản cho lớp SD1905 trong HK1_2026."),
            new TeachingCourseSeed("GEN102", "lecturer01@edulms.local", "HK1_2026", "SD1906", "Bản phân công giảng dạy môn Tin học cơ bản cho lớp SD1906 trong HK1_2026."),
            
            // HK1_2026 - COM101 (Nhập môn lập trình)
            new TeachingCourseSeed("COM101", "teacher.csharp.a@lms.local", "HK1_2026", "SD1901", "Bản phân công giảng dạy môn Nhập môn lập trình cho lớp SD1901 trong HK1_2026."),
            new TeachingCourseSeed("COM101", "teacher.csharp.a@lms.local", "HK1_2026", "SD1902", "Bản phân công giảng dạy môn Nhập môn lập trình cho lớp SD1902 trong HK1_2026."),
            new TeachingCourseSeed("COM101", "teacher.csharp.a@lms.local", "HK1_2026", "SD1903", "Bản phân công giảng dạy môn Nhập môn lập trình cho lớp SD1903 trong HK1_2026."),
            new TeachingCourseSeed("COM101", "teacher.csharp.b@lms.local", "HK1_2026", "SD1904", "Bản phân công giảng dạy môn Nhập môn lập trình cho lớp SD1904 trong HK1_2026."),
            new TeachingCourseSeed("COM101", "teacher.csharp.b@lms.local", "HK1_2026", "SD1905", "Bản phân công giảng dạy môn Nhập môn lập trình cho lớp SD1905 trong HK1_2026."),
            new TeachingCourseSeed("COM101", "teacher.csharp.b@lms.local", "HK1_2026", "SD1906", "Bản phân công giảng dạy môn Nhập môn lập trình cho lớp SD1906 trong HK1_2026."),

            new TeachingCourseSeed(
                "WEB102",
                "p12test_teacher01@lms.local",
                "HK3_2026",
                "SD1904",
                "Bản phân công giảng dạy môn Lập trình JavaScript cho lớp SD1904 trong HK3_2026 (P15G smoke)."
            ),
            new TeachingCourseSeed(
                "COM103",
                "teacher.csharp.a@lms.local",
                "HK3_2026",
                "SD1901",
                "Bản phân công giảng dạy môn Lập trình C# cho lớp SD1901 trong HK3_2026."
            ),
            new TeachingCourseSeed(
                "COM103",
                "teacher.csharp.a@lms.local",
                "HK3_2026",
                "SD1902",
                "Bản phân công giảng dạy môn Lập trình C# cho lớp SD1902 trong HK3_2026."
            ),
            new TeachingCourseSeed(
                "COM103",
                "teacher.csharp.a@lms.local",
                "HK3_2026",
                "SD1903",
                "Bản phân công giảng dạy môn Lập trình C# cho lớp SD1903 trong HK3_2026."
            ),
            new TeachingCourseSeed(
                "COM103",
                "teacher.csharp.b@lms.local",
                "HK3_2026",
                "SD1904",
                "Bản phân công giảng dạy môn Lập trình C# cho lớp SD1904 trong HK3_2026."
            ),
            new TeachingCourseSeed(
                "COM103",
                "teacher.csharp.b@lms.local",
                "HK3_2026",
                "SD1905",
                "Bản phân công giảng dạy môn Lập trình C# cho lớp SD1905 trong HK3_2026."
            ),
            new TeachingCourseSeed(
                "COM102",
                "teacher.database.c@lms.local",
                "HK3_2026",
                "SD1901",
                "Bản phân công giảng dạy môn Cơ sở dữ liệu cho lớp SD1901 trong HK3_2026."
            ),
            new TeachingCourseSeed(
                "COM102",
                "teacher.database.c@lms.local",
                "HK3_2026",
                "SD1902",
                "Bản phân công giảng dạy môn Cơ sở dữ liệu cho lớp SD1902 trong HK3_2026."
            ),
            new TeachingCourseSeed(
                "COM102",
                "teacher.database.d@lms.local",
                "HK3_2026",
                "SD1903",
                "Bản phân công giảng dạy môn Cơ sở dữ liệu cho lớp SD1903 trong HK3_2026."
            ),
            new TeachingCourseSeed(
                "COM102",
                "teacher.database.d@lms.local",
                "HK3_2026",
                "SD1906",
                "Bản phân công giảng dạy môn Cơ sở dữ liệu cho lớp SD1906 trong HK3_2026."
            ),
            new TeachingCourseSeed(
                "MKT101",
                "teacher.marketing.e@lms.local",
                "HK3_2026",
                "MKT1901",
                "Bản phân công giảng dạy môn Marketing căn bản cho lớp MKT1901 trong HK3_2026."
            ),
            new TeachingCourseSeed(
                "MKT101",
                "teacher.marketing.e@lms.local",
                "HK3_2026",
                "MKT1902",
                "Bản phân công giảng dạy môn Marketing căn bản cho lớp MKT1902 trong HK3_2026."
            ),
        };

        foreach (var plan in coursePlans)
        {
            var subject = subjects[plan.SubjectCode];
            var teacher = users[plan.TeacherEmail];
            var term = termsByCode[plan.TermCode];
            var administrativeClass = administrativeClasses[plan.ClassCode];

            var course = await context.KhoaHocs.FirstOrDefaultAsync(x =>
                x.MaDonVi == campus.MaDonVi
                && x.MaMonHoc == subject.MaMonHoc
                && x.MaHocKy == term.MaHocKy
                && x.MaLop == administrativeClass.MaLop
            );

            if (course is null)
            {
                course = new KhoaHoc
                {
                    MaDonVi = campus.MaDonVi,
                    MaGiaoVien = teacher.MaNguoiDung,
                    MaMonHoc = subject.MaMonHoc,
                    MaLop = administrativeClass.MaLop,
                    NgayTao = DateTime.UtcNow,
                };

                context.KhoaHocs.Add(course);
            }

            course.MaDonVi = campus.MaDonVi;
            course.MaGiaoVien = teacher.MaNguoiDung;
            course.MaMonHoc = subject.MaMonHoc;
            course.MaHocKy = term.MaHocKy;
            course.MaLop = administrativeClass.MaLop;
            course.MaLopHocPhan = null;
            course.TieuDe =
                $"{subject.TenMonHoc} - {administrativeClass.MaCodeLop} - {term.TenHocKy} - {teacher.HoTen}";
            course.MoTa = plan.Description;
            course.TrangThai = PublishedStatus;
            course.UrlAnhBia =
                $"/demo/courses/{subject.MaCodeMonHoc.ToLowerInvariant()}-{administrativeClass.MaCodeLop.ToLowerInvariant()}-cover.jpg";
        }

        await context.SaveChangesAsync();
    }

    private static async Task<Dictionary<string, CaHoc>> SeedClassShiftsAsync(
        ApplicationDbContext context
    )
    {
        var shiftPlans = new[]
        {
            new ShiftSeed("Ca 1", "sang", new TimeOnly(7, 30), new TimeOnly(9, 0), 1),
            new ShiftSeed("Ca 2", "sang", new TimeOnly(9, 5), new TimeOnly(12, 0), 2),
            new ShiftSeed("Ca 3", "chieu", new TimeOnly(13, 0), new TimeOnly(14, 30), 3),
            new ShiftSeed("Ca 4", "chieu", new TimeOnly(14, 35), new TimeOnly(16, 5), 4),
            new ShiftSeed("Ca 5", "chieu", new TimeOnly(16, 10), new TimeOnly(17, 40), 5),
        };

        var result = new Dictionary<string, CaHoc>(StringComparer.OrdinalIgnoreCase);

        foreach (var plan in shiftPlans)
        {
            var shift = await context.CaHocs.FirstOrDefaultAsync(x => x.TenCa == plan.Name);

            if (shift is null)
            {
                shift = new CaHoc { TenCa = plan.Name };

                context.CaHocs.Add(shift);
            }

            shift.Buoi = plan.Session;
            shift.GioBatDau = plan.StartTime;
            shift.GioKetThuc = plan.EndTime;
            shift.ThuTu = plan.DisplayOrder;
            shift.ConHoatDong = true;
            result[plan.Name] = shift;
        }

        await context.SaveChangesAsync();
        return result;
    }

    private static async Task SeedParentLinkAsync(
        ApplicationDbContext context,
        IReadOnlyDictionary<string, NguoiDung> users
    )
    {
        var parent = users["p15test_parent01@lms.local"];
        var student = users["student.cntt01@lms.local"];
        var link = await context.LienKetPhuHuynhs.FirstOrDefaultAsync(x =>
            x.MaPhuHuynh == parent.MaNguoiDung && x.MaHocSinh == student.MaNguoiDung
        );

        if (link is null)
        {
            link = new LienKetPhuHuynh
            {
                MaPhuHuynh = parent.MaNguoiDung,
                MaHocSinh = student.MaNguoiDung,
            };

            context.LienKetPhuHuynhs.Add(link);
        }

        link.QuyenXem = JsonSerializer.Serialize(
            new
            {
                grades = true,
                attendance = true,
                tuition = true,
            },
            JsonOptions
        );
        link.TrangThai = UserStatuses.DbActive;
        link.LienKetLuc ??= DateTime.UtcNow;

        await context.SaveChangesAsync();
    }

    private static async Task SeedFacilitiesAsync(ApplicationDbContext context, DonVi campus)
    {
        var buildingA = await UpsertBuildingAsync(context, campus, "A", "Tòa A", 3);
        var buildingB = await UpsertBuildingAsync(context, campus, "B", "Tòa B", 2);
        var buildingC = await UpsertBuildingAsync(context, campus, "C", "Tòa C", 1);
        var buildingP = await UpsertBuildingAsync(context, campus, "P", "Tòa P", 3);

        var floors = new Dictionary<string, Tang>(StringComparer.OrdinalIgnoreCase)
        {
            ["A1"] = await UpsertFloorAsync(context, buildingA, 1),
            ["A2"] = await UpsertFloorAsync(context, buildingA, 2),
            ["A3"] = await UpsertFloorAsync(context, buildingA, 3),
            ["B1"] = await UpsertFloorAsync(context, buildingB, 1),
            ["B2"] = await UpsertFloorAsync(context, buildingB, 2),
            ["C1"] = await UpsertFloorAsync(context, buildingC, 1),
            ["P1"] = await UpsertFloorAsync(context, buildingP, 1),
            ["P2"] = await UpsertFloorAsync(context, buildingP, 2),
            ["P3"] = await UpsertFloorAsync(context, buildingP, 3),
        };

        var roomPlans = new[]
        {
            new RoomSeed(
                "A101",
                "Phòng A101",
                buildingA,
                floors["A1"],
                40,
                "ly_thuyet",
                "Phòng lý thuyết 40 chỗ."
            ),
            new RoomSeed(
                "A102",
                "Phòng A102",
                buildingA,
                floors["A1"],
                35,
                "ly_thuyet",
                "Phòng lý thuyết 35 chỗ."
            ),
            new RoomSeed(
                "A201",
                "Phòng Lab A201",
                buildingA,
                floors["A2"],
                30,
                "lab",
                "Phòng lab thực hành phần mềm."
            ),
            new RoomSeed(
                "A202",
                "Phòng Lab A202",
                buildingA,
                floors["A2"],
                30,
                "lab",
                "Phòng lab thực hành phần mềm."
            ),
            new RoomSeed(
                "A301",
                "Hội trường A301",
                buildingA,
                floors["A3"],
                50,
                "hoi_truong",
                "Hội trường demo bảo vệ đồ án."
            ),
            new RoomSeed(
                "B101",
                "Phòng B101",
                buildingB,
                floors["B1"],
                45,
                "ly_thuyet",
                "Phòng lý thuyết 45 chỗ."
            ),
            new RoomSeed(
                "B201",
                "Phòng B201",
                buildingB,
                floors["B2"],
                35,
                "ly_thuyet",
                "Phòng lý thuyết 35 chỗ."
            ),
            new RoomSeed(
                "C101",
                "Studio thiết kế C101",
                buildingC,
                floors["C1"],
                25,
                "thuc_hanh",
                "Studio thiết kế đồ họa."
            ),
            new RoomSeed(
                "P301",
                "Phòng P301",
                buildingP,
                floors["P3"],
                40,
                "ly_thuyet",
                "Phòng lý thuyết phục vụ lịch học demo."
            ),
            new RoomSeed(
                "P302",
                "Phòng P302",
                buildingP,
                floors["P3"],
                40,
                "ly_thuyet",
                "Phòng lý thuyết phục vụ lịch học demo."
            ),
        };

        foreach (var plan in roomPlans)
        {
            var room = await context.PhongHocs.FirstOrDefaultAsync(x =>
                x.MaDonVi == campus.MaDonVi && x.MaCodePhong == plan.Code
            );

            if (room is null)
            {
                room = new PhongHoc { MaDonVi = campus.MaDonVi, MaCodePhong = plan.Code };

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

    private static async Task SeedScheduleTemplatesAsync(
        ApplicationDbContext context,
        DonVi campus,
        IReadOnlyDictionary<string, DanhMucMonHoc> subjects,
        IReadOnlyList<HocKy> terms,
        IReadOnlyDictionary<string, LopHanhChinh> administrativeClasses,
        IReadOnlyDictionary<string, CaHoc> shifts
    )
    {
        var termsByCode = terms.ToDictionary(x => x.MaCodeHocKy, StringComparer.OrdinalIgnoreCase);
        var schedulePlans = new[]
        {
            new ScheduleTemplateSeed("COM103", "HK3_2026", "SD1904", 2, "Ca 1", "P301"),
            new ScheduleTemplateSeed("COM103", "HK3_2026", "SD1905", 2, "Ca 2", "P302"),
            new ScheduleTemplateSeed("COM102", "HK3_2026", "SD1906", 4, "Ca 4", "B201"),
        };

        foreach (var plan in schedulePlans)
        {
            var subject = subjects[plan.SubjectCode];
            var term = termsByCode[plan.TermCode];
            var administrativeClass = administrativeClasses[plan.ClassCode];
            var shift = shifts[plan.ShiftName];
            var room = await context.PhongHocs.FirstAsync(x =>
                x.MaDonVi == campus.MaDonVi && x.MaCodePhong == plan.RoomCode
            );
            var course = await context.KhoaHocs.FirstAsync(x =>
                x.MaDonVi == campus.MaDonVi
                && x.MaMonHoc == subject.MaMonHoc
                && x.MaHocKy == term.MaHocKy
                && x.MaLop == administrativeClass.MaLop
            );

            var schedule = await context.ThoiKhoaBieus.FirstOrDefaultAsync(x =>
                x.MaKhoaHoc == course.MaKhoaHoc
                && x.ThuTrongTuan == plan.DayOfWeek
                && x.MaCaHoc == shift.MaCaHoc
            );

            if (schedule is null)
            {
                schedule = new ThoiKhoaBieu
                {
                    MaKhoaHoc = course.MaKhoaHoc,
                    ThuTrongTuan = plan.DayOfWeek,
                    MaCaHoc = shift.MaCaHoc,
                    NgayTao = DateTime.UtcNow,
                };

                context.ThoiKhoaBieus.Add(schedule);
            }

            schedule.MaPhong = room.MaPhong;
            schedule.NgayBatDau = term.NgayBatDau;
            schedule.NgayKetThuc = term.NgayKetThuc;
            schedule.TrangThai = PublishedStatus;
            schedule.NgayCapNhat = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
    }

    private static async Task<ToaNha> UpsertBuildingAsync(
        ApplicationDbContext context,
        DonVi campus,
        string code,
        string name,
        int floorCount
    )
    {
        var building = await context.ToaNhas.FirstOrDefaultAsync(x =>
            x.MaDonVi == campus.MaDonVi && x.MaCodeToaNha == code
        );

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
        int floorNumber
    )
    {
        var floor = await context.Tangs.FirstOrDefaultAsync(x =>
            x.MaToaNha == building.MaToaNha && x.ThuTuTang == floorNumber
        );

        if (floor is null)
        {
            floor = new Tang { MaToaNha = building.MaToaNha, ThuTuTang = floorNumber };

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
        IReadOnlyDictionary<string, DanhMucMonHoc> subjects
    )
    {
        var syllabusPlans = new[]
        {
            new SyllabusSeed("CT_CNTT_K2026", "CNTT_PTPM", "COM101", "Đề cương Nhập môn lập trình"),
            new SyllabusSeed("CT_CNTT_K2026", "CNTT_PTPM", "WEB101", "Đề cương Thiết kế Web"),
            new SyllabusSeed("CT_TKDH_K2026", "TKDH_UIUX", "DES101", "Đề cương Nguyên lý thị giác"),
            new SyllabusSeed("CT_TKDH_K2026", "TKDH_UIUX", "DES106", "Đề cương UI/UX Design"),
            new SyllabusSeed(
                "CT_MKT_K2026",
                "MKT_DIGITAL",
                "MKT101",
                "Đề cương Nguyên lý Marketing"
            ),
            new SyllabusSeed("CT_MKT_K2026", "MKT_DIGITAL", "MKT103", "Đề cương Digital Marketing"),
        };

        foreach (var plan in syllabusPlans)
        {
            var subject = subjects[plan.SubjectCode];
            var specialization = specializations[plan.SpecializationCode];
            programSubjects.TryGetValue(
                $"{plan.ProgramCode}:{plan.SubjectCode}",
                out var programSubject
            );

            var syllabus = await context.CourseSyllabuses.FirstOrDefaultAsync(x =>
                x.MaMonHoc == subject.MaMonHoc
                && x.MaChuyenNganh == specialization.MaChuyenNganh
                && x.MaDonVi == campus.MaDonVi
                && x.Version == ProgramVersion
            );

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
        IReadOnlyList<HocKy> terms
    )
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
                    x.MaDonVi == campus.MaDonVi
                    && x.MaChuongTrinhDaoTao == program.MaChuongTrinh
                    && x.MaHocKy == term.MaHocKy
                    && x.ConHoatDong
                );

                if (config is null)
                {
                    config = await context.CauHinhHocPhiChuongTrinhs.FirstOrDefaultAsync(x =>
                        x.MaDonVi == campus.MaDonVi
                        && x.MaChuongTrinhDaoTao == program.MaChuongTrinh
                        && x.MaHocKy == term.MaHocKy
                    );
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
                config.LoaiCachTinhHocPhi = FinanceConstants.TuitionCalculationTypes.FixedPerTerm;
                config.SoTienHocPhi = tuitionAmount;
                config.TienHocLieu = plan.MaterialAmount;
                config.TongTienDuKien = tuitionAmount + plan.MaterialAmount;
                config.ConHoatDong = true;
                config.GhiChu =
                    $"{program.TenChuongTrinh} - năm {yearInProgram} kỳ {termInYear} tại {campus.TenDonVi}.";
                config.NgayCapNhat = DateTime.UtcNow;
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedTuitionReceivingAccountAsync(
        ApplicationDbContext context,
        DonVi campus
    )
    {
        const string bankCode = "MB";
        const string demoAccountNumber = "123456789";

        var account = await context.TaiKhoanNhanTiens.FirstOrDefaultAsync(x =>
            x.MaDonVi == campus.MaDonVi
            && x.MaNganHang == bankCode
            && x.SoTaiKhoan == demoAccountNumber
        );

        if (account is null)
        {
            account = new TaiKhoanNhanTien
            {
                MaDonVi = campus.MaDonVi,
                MaNganHang = bankCode,
                SoTaiKhoan = demoAccountNumber,
                NgayTao = DateTime.UtcNow,
            };

            context.TaiKhoanNhanTiens.Add(account);
        }

        var hasOtherDefaultAccount = await context.TaiKhoanNhanTiens.AnyAsync(x =>
            x.MaDonVi == campus.MaDonVi
            && x.MaTaiKhoanNhanTien != account.MaTaiKhoanNhanTien
            && x.LaMacDinh
            && x.ConHoatDong
        );

        account.TenNganHang = "MB Bank";
        account.TenChuTaiKhoan = "TRUONG CAO DANG DEMO";
        account.ChiNhanh = "HCM";
        account.NhaCungCapThanhToan = FinanceConstants.PaymentProviders.PayOs;
        account.TrangThaiDuyet = FinanceConstants.PaymentAccountApprovalStatuses.Approved;
        account.ConHoatDong = true;
        account.LaMacDinh = !hasOtherDefaultAccount;
        account.NgayDuyet ??= DateTime.UtcNow;
        account.NgayCapNhat = DateTime.UtcNow;

        await context.SaveChangesAsync();
    }

    private static async Task SeedDeKiemTraAsync(
        ApplicationDbContext context,
        IReadOnlyDictionary<string, DanhMucMonHoc> subjects,
        IReadOnlyList<HocKy> terms
    )
    {
        var term = terms.FirstOrDefault(t => t.MaCodeHocKy == "HK3_2026");
        if (term == null)
        {
            return;
        }

        var plans = new[]
        {
            new ExamSeed("COM103", "Đề thi mẫu Lập trình C#", 60, "ket_hop", "dang_mo"),
            new ExamSeed("COM102", "Quiz Cơ sở dữ liệu", 45, "trac_nghiem", "dang_mo"),
            new ExamSeed("WEB101", "Kiểm tra Thiết kế Web", 45, "ket_hop", "da_len_lich"),
            new ExamSeed("API101", "Quiz Xây dựng REST API", 35, "trac_nghiem", "dang_mo"),
            new ExamSeed("DES106", "Kiểm tra UI/UX Design", 45, "ket_hop", "dang_mo"),
            new ExamSeed("DES104", "Quiz Thiết kế nhận diện thương hiệu", 35, "trac_nghiem", "da_len_lich"),
            new ExamSeed("MKT101", "Quiz Marketing căn bản", 35, "trac_nghiem", "dang_mo"),
            new ExamSeed("MKT103", "Kiểm tra Digital Marketing", 45, "ket_hop", "dang_mo"),
        };

        foreach (var plan in plans)
        {
            if (!subjects.TryGetValue(plan.SubjectCode, out var subject))
            {
                continue;
            }

            var deKiemTra = await context.DeKiemTras.FirstOrDefaultAsync(x =>
                x.TieuDe == plan.Title && x.MaMonHoc == subject.MaMonHoc
            );

            if (deKiemTra is null)
            {
                deKiemTra = new DeKiemTra
                {
                    MaMonHoc = subject.MaMonHoc,
                    TieuDe = plan.Title,
                    NgayTao = DateTime.UtcNow,
                };

                context.DeKiemTras.Add(deKiemTra);
            }

            deKiemTra.MaMonHoc = subject.MaMonHoc;
            deKiemTra.MaHocKy = term.MaHocKy;
            deKiemTra.TieuDe = plan.Title;
            deKiemTra.ThoiGianPhut = plan.DurationMinutes;
            deKiemTra.CauHinhDeThi =
                "{\"questions\":[{\"id\":1,\"content\":\"Câu hỏi mẫu trắc nghiệm\",\"type\":\"mcq\",\"options\":[\"A\",\"B\",\"C\",\"D\"],\"answer\":\"A\"}]}";
            deKiemTra.TrangThai = plan.Status;
            deKiemTra.LoaiDeThi = plan.ExamType;
            deKiemTra.HinhThucThi = "online_tu_do";
            deKiemTra.TyLeTracNghiem = plan.ExamType == "tu_luan" ? 0m : 70m;
            deKiemTra.TyLeTuLuan = plan.ExamType == "trac_nghiem" ? 0m : 30m;
            deKiemTra.NgayCapNhat = DateTime.UtcNow;
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
        DateOnly EndDate
    );

    private sealed record SubjectSeed(string Code, string Name, int Credits);

    private sealed record StandardChapterSeed(int Order, string Title, LessonSeed[] Lessons);

    private sealed record LessonSeed(
        int Order,
        string Title,
        string LessonType,
        string? FileUrl,
        int? DurationSeconds,
        string? TextContent
    );

    private sealed record StandardAssignmentSeed(
        string SubjectCode,
        string Title,
        string Description,
        DateTime DueAt,
        int MaxSubmissions,
        string[] AllowedFormats,
        string GradingGuide,
        string Status
    );

    private sealed record TrainingProgramSeed(
        string Code,
        string Name,
        string SpecializationCode,
        int RequiredCredits,
        string Description
    );

    private sealed record ProgramSubjectSeed(int ExpectedTerm, string SubjectCode);

    private sealed record DemoUserSeed(
        string Email,
        string FullName,
        string RoleCode,
        int CampusId,
        int? EnrollmentYear = null,
        string? Password = null
    );

    private sealed record AdministrativeClassSeed(
        string Code,
        string Name,
        string ProgramCode,
        string TeacherEmail,
        string? StudentEmail
    );

    private sealed record CourseSectionSeed(
        string Code,
        string SubjectCode,
        string TermCode,
        int Capacity,
        int MinimumEnrollment,
        int AbsenceQuota
    );

    private sealed record TeachingCourseSeed(
        string SubjectCode,
        string TeacherEmail,
        string TermCode,
        string ClassCode,
        string Description
    );

    private sealed record ShiftSeed(
        string Name,
        string Session,
        TimeOnly StartTime,
        TimeOnly EndTime,
        int DisplayOrder
    );

    private sealed record ScheduleTemplateSeed(
        string SubjectCode,
        string TermCode,
        string ClassCode,
        int DayOfWeek,
        string ShiftName,
        string RoomCode
    );

    private sealed record RoomSeed(
        string Code,
        string Name,
        ToaNha Building,
        Tang Floor,
        int Capacity,
        string Type,
        string Note
    );

    private sealed record SyllabusSeed(
        string ProgramCode,
        string SpecializationCode,
        string SubjectCode,
        string Name
    );

    private sealed record TuitionSeed(
        string ProgramCode,
        decimal[] YearlyTuitionAmounts,
        decimal MaterialAmount
    );

    private sealed record ExamSeed(
        string SubjectCode,
        string Title,
        int DurationMinutes,
        string ExamType,
        string Status
    );
    private static async Task SeedTeachingPreferencesAsync(
        ApplicationDbContext context,
        DonVi campus,
        IReadOnlyList<HocKy> terms,
        Dictionary<string, NguoiDung> users,
        Dictionary<string, CaHoc> shifts)
    {
        var term = terms.FirstOrDefault(x => x.MaCodeHocKy == "HK3_2026");
        if (term == null) return;

        var teacherKeys = new[] { "teacher.cntt@lms.local", "teacher.csharp.a@lms.local", "teacher.database.c@lms.local" };
        var activeShifts = shifts.Values.Where(x => x.ConHoatDong).OrderBy(x => x.ThuTu).ToList();
        if (!activeShifts.Any()) return;

        foreach (var key in teacherKeys)
        {
            if (!users.TryGetValue(key, out var teacher)) continue;

            var existing = await context.GiaoVienNguyenVongHocKys
                .AnyAsync(x => x.MaGiaoVien == teacher.MaNguoiDung && x.MaHocKy == term.MaHocKy);
            if (existing) continue;

            var status = key == "teacher.cntt@lms.local" ? "draft" : "submitted";
            
            var preference = new GiaoVienNguyenVongHocKy
            {
                MaGiaoVien = teacher.MaNguoiDung,
                MaHocKy = term.MaHocKy,
                MaDonVi = teacher.MaDonVi,
                TrangThai = status,
                NgayTao = DateTime.UtcNow,
                NgayGui = status == "submitted" ? DateTime.UtcNow : null,
                SoLopToiDaMongMuon = 4,
                SoCaToiDaMoiTuan = 8,
                GhiChu = "Dữ liệu mẫu từ hệ thống"
            };

            context.GiaoVienNguyenVongHocKys.Add(preference);
            await context.SaveChangesAsync();

            var details = new List<GiaoVienNguyenVongCaDay>();
            
            if (key == "teacher.cntt@lms.local")
            {
                details.Add(new GiaoVienNguyenVongCaDay { NguyenVongId = preference.Id, ThuTrongTuan = 2, MaCaHoc = activeShifts[0].MaCaHoc, MucDo = "preferred", NgayTao = DateTime.UtcNow });
                if (activeShifts.Count > 1) details.Add(new GiaoVienNguyenVongCaDay { NguyenVongId = preference.Id, ThuTrongTuan = 2, MaCaHoc = activeShifts[1].MaCaHoc, MucDo = "available", NgayTao = DateTime.UtcNow });
                details.Add(new GiaoVienNguyenVongCaDay { NguyenVongId = preference.Id, ThuTrongTuan = 3, MaCaHoc = activeShifts[0].MaCaHoc, MucDo = "preferred", NgayTao = DateTime.UtcNow });
                if (activeShifts.Count > 3) details.Add(new GiaoVienNguyenVongCaDay { NguyenVongId = preference.Id, ThuTrongTuan = 5, MaCaHoc = activeShifts[3].MaCaHoc, MucDo = "unavailable", NgayTao = DateTime.UtcNow });
            }
            else if (key == "teacher.csharp.a@lms.local")
            {
                details.Add(new GiaoVienNguyenVongCaDay { NguyenVongId = preference.Id, ThuTrongTuan = 4, MaCaHoc = activeShifts[0].MaCaHoc, MucDo = "preferred", NgayTao = DateTime.UtcNow });
                if (activeShifts.Count > 2) details.Add(new GiaoVienNguyenVongCaDay { NguyenVongId = preference.Id, ThuTrongTuan = 4, MaCaHoc = activeShifts[2].MaCaHoc, MucDo = "unavailable", NgayTao = DateTime.UtcNow });
            }
            else
            {
                details.Add(new GiaoVienNguyenVongCaDay { NguyenVongId = preference.Id, ThuTrongTuan = 6, MaCaHoc = activeShifts[0].MaCaHoc, MucDo = "available", NgayTao = DateTime.UtcNow });
            }

            context.GiaoVienNguyenVongCaDays.AddRange(details);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedBaseTeacherMajorsAsync(
        ApplicationDbContext context,
        Dictionary<string, NguoiDung> users,
        Dictionary<string, ChuyenNganh> specializations
    )
    {
        var cntt = specializations.Values.FirstOrDefault(x => x.TenChuyenNganh.Contains("Phần mềm") || x.TenChuyenNganh.Contains("CNTT")) ?? specializations.Values.FirstOrDefault();
        var mkt = specializations.Values.FirstOrDefault(x => x.TenChuyenNganh.Contains("Marketing")) ?? specializations.Values.FirstOrDefault();
        var tkdh = specializations.Values.FirstOrDefault(x => x.TenChuyenNganh.Contains("Đồ họa")) ?? specializations.Values.FirstOrDefault();

        foreach (var kvp in users)
        {
            if (kvp.Value.VaiTroChinh != AuthRoles.ToDatabaseCode(AuthRoles.Teacher)) continue;

            var existing = await context.GiaoVienChuyenNganhs.AnyAsync(x => x.MaGiaoVien == kvp.Value.MaNguoiDung);
            if (existing) continue;

            var spec = cntt;
            if (kvp.Key.Contains("mkt") || kvp.Key.Contains("marketing")) spec = mkt;
            else if (kvp.Key.Contains("tkdh")) spec = tkdh;

            if (spec != null)
            {
                context.GiaoVienChuyenNganhs.Add(new GiaoVienChuyenNganh
                {
                    MaGiaoVien = kvp.Value.MaNguoiDung,
                    MaChuyenNganh = spec.MaChuyenNganh,
                    LaChuyenMonChinh = true,
                    MucDoPhuHop = 100,
                    SoNamKinhNghiem = 5,
                    NgayTao = DateTime.UtcNow
                });
            }
        }
        await context.SaveChangesAsync();
    }
}
