using Backend.Constants;
using Backend.Helpers;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public static class Data
{
    private const string RootUnitName = "LMS Root";
    private const string SuperAdminEmail = "superadmin@lms.local";
    private const string SuperAdminDefaultPassword = "Admin@123456";
    private const string SuperAdminFullName = "Super Administrator";
    private const string SuperAdminRoleCode = "sieu_quan_tri";
    private const string ChairmanEmail = "chairman@lms.local";
    private const string ChairmanDefaultPassword = "Chairman@123456";
    private const string ChairmanFullName = "Chủ tịch hệ thống";
    private const string ChairmanRoleCode = "chu_tich";
    private const string CampusHcmName = "Cơ sở Hồ Chí Minh";
    private const string CampusLevel = "co_so";
    private const string ApprovedStatus = "approved";
    private const string ActiveStatus = "active";
    private const string RequiredSubjectType = "bat_buoc";
    private const string SyllabusDraftStatus = "draft";
    private const string TrainingProgramSourceCode = "PTPM-K21";

    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await SeedRolesAsync(context);

        var rootUnit = await GetOrCreateRootUnitAsync(context);
        await SeedSuperAdminUserAsync(context, rootUnit);
        await SeedChairmanUserAsync(context, rootUnit);
        await SeedTrainingProgramTestDataAsync(context, rootUnit);

        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(ApplicationDbContext context)
    {
        var roles = new[]
        {
            new VaiTro
            {
                MaVaiTro = 1,
                MaCodeVaiTro = "quan_tri",
                TenVaiTro = "Quản trị",
            },
            new VaiTro
            {
                MaVaiTro = 2,
                MaCodeVaiTro = "giao_vien",
                TenVaiTro = "Giáo viên",
            },
            new VaiTro
            {
                MaVaiTro = 3,
                MaCodeVaiTro = "hoc_sinh",
                TenVaiTro = "Học sinh",
            },
            new VaiTro
            {
                MaVaiTro = 4,
                MaCodeVaiTro = "nhan_vien",
                TenVaiTro = "Nhân viên/Giáo vụ",
            },
            new VaiTro
            {
                MaVaiTro = 5,
                MaCodeVaiTro = "hieu_truong",
                TenVaiTro = "Hiệu trưởng/BGH",
            },
            new VaiTro
            {
                MaVaiTro = 6,
                MaCodeVaiTro = "phu_huynh",
                TenVaiTro = "Phụ huynh",
            },
            new VaiTro
            {
                MaVaiTro = 7,
                MaCodeVaiTro = "sieu_quan_tri",
                TenVaiTro = "Siêu quản trị",
            },
            new VaiTro
            {
                MaVaiTro = 8,
                MaCodeVaiTro = "quan_tri_co_so",
                TenVaiTro = "Quản trị cơ sở",
            },
            new VaiTro
            {
                MaVaiTro = 9,
                MaCodeVaiTro = "quan_tri_co_so_con",
                TenVaiTro = "Quản trị cơ sở con",
            },
            new VaiTro
            {
                MaVaiTro = 10,
                MaCodeVaiTro = "chu_tich",
                TenVaiTro = "Chủ tịch",
            },
        };

        foreach (var role in roles)
        {
            var existingRole = await context.VaiTros.FirstOrDefaultAsync(x =>
                x.MaCodeVaiTro == role.MaCodeVaiTro
            );

            if (existingRole is null)
            {
                context.VaiTros.Add(role);
                continue;
            }

            existingRole.TenVaiTro = role.TenVaiTro;
        }

        await context.SaveChangesAsync();
    }

    private static async Task<DonVi> GetOrCreateRootUnitAsync(ApplicationDbContext context)
    {
        var rootUnit = await context.DonVis.FirstOrDefaultAsync(x => x.CapDonVi == "root");
        if (rootUnit is null)
        {
            rootUnit = new DonVi
            {
                TenDonVi = RootUnitName,
                CapDonVi = "root",
                ConHoatDong = true,
                NgayTao = DateTime.UtcNow,
            };

            context.DonVis.Add(rootUnit);
            await context.SaveChangesAsync();
        }
        else
        {
            if (string.IsNullOrWhiteSpace(rootUnit.TenDonVi))
            {
                rootUnit.TenDonVi = RootUnitName;
            }

            rootUnit.ConHoatDong = true;
        }

        return rootUnit;
    }

    private static async Task SeedSuperAdminUserAsync(ApplicationDbContext context, DonVi rootUnit)
    {
        var superAdmin = await context.NguoiDungs.FirstOrDefaultAsync(x =>
            x.Email == SuperAdminEmail
        );

        if (superAdmin is null)
        {
            superAdmin = new NguoiDung
            {
                MaDonVi = rootUnit.MaDonVi,
                Email = SuperAdminEmail,
                HoTen = SuperAdminFullName,
                VaiTroChinh = SuperAdminRoleCode,
                TrangThai = UserStatuses.DbFirstLogin,
                MatKhauHash = PasswordHelper.HashPassword(SuperAdminDefaultPassword),
                NgayTao = DateTime.UtcNow,
                SoLanSaiMatKhau = 0,
                DangNhapLanDau = true,
            };

            context.NguoiDungs.Add(superAdmin);
        }
        else
        {
            superAdmin.MaDonVi = rootUnit.MaDonVi;
            superAdmin.HoTen = string.IsNullOrWhiteSpace(superAdmin.HoTen)
                ? SuperAdminFullName
                : superAdmin.HoTen;
            superAdmin.VaiTroChinh = SuperAdminRoleCode;

            if (string.IsNullOrWhiteSpace(superAdmin.MatKhauHash))
            {
                superAdmin.MatKhauHash = PasswordHelper.HashPassword(SuperAdminDefaultPassword);
                superAdmin.TrangThai = UserStatuses.DbFirstLogin;
                superAdmin.DangNhapLanDau = true;
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedChairmanUserAsync(ApplicationDbContext context, DonVi rootUnit)
    {
        var chairman = await context.NguoiDungs.FirstOrDefaultAsync(x => x.Email == ChairmanEmail);

        if (chairman is null)
        {
            chairman = new NguoiDung
            {
                MaDonVi = rootUnit.MaDonVi,
                Email = ChairmanEmail,
                HoTen = ChairmanFullName,
                VaiTroChinh = ChairmanRoleCode,
                TrangThai = UserStatuses.DbFirstLogin,
                MatKhauHash = PasswordHelper.HashPassword(ChairmanDefaultPassword),
                NgayTao = DateTime.UtcNow,
                SoLanSaiMatKhau = 0,
                DangNhapLanDau = true,
            };

            context.NguoiDungs.Add(chairman);
        }
        else
        {
            chairman.MaDonVi = rootUnit.MaDonVi;
            chairman.HoTen = string.IsNullOrWhiteSpace(chairman.HoTen)
                ? ChairmanFullName
                : chairman.HoTen;
            chairman.VaiTroChinh = ChairmanRoleCode;

            if (string.IsNullOrWhiteSpace(chairman.MatKhauHash))
            {
                chairman.MatKhauHash = PasswordHelper.HashPassword(ChairmanDefaultPassword);
                chairman.TrangThai = UserStatuses.DbFirstLogin;
                chairman.DangNhapLanDau = true;
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedTrainingProgramTestDataAsync(
        ApplicationDbContext context,
        DonVi rootUnit
    )
    {
        var campus = await GetOrCreateCampusAsync(context, rootUnit);
        var cohortK21 = await GetOrCreateCohortAsync(
            context,
            "K21",
            "Khóa K21",
            2026,
            2030,
            "Khóa tuyển sinh K21 dùng để test chương trình đào tạo nguồn"
        );

        await GetOrCreateCohortAsync(
            context,
            "K22",
            "Khóa K22",
            2027,
            2031,
            "Khóa tuyển sinh K22 dùng để test clone chương trình đào tạo"
        );

        var major = await GetOrCreateMajorAsync(context);
        var specialization = await GetOrCreateSpecializationAsync(context, major);
        await GetOrCreateCampusSpecializationAsync(context, specialization, campus);
        var trainingProgram = await GetOrCreateTrainingProgramAsync(
            context,
            specialization,
            cohortK21
        );
        await context.SaveChangesAsync();

        var pro101 = await GetOrCreateSubjectAsync(context, "PRO101", "Nhập môn lập trình", 3);
        var dbi101 = await GetOrCreateSubjectAsync(context, "DBI101", "Cơ sở dữ liệu", 3);
        var web101 = await GetOrCreateSubjectAsync(context, "WEB101", "Thiết kế web", 3);
        var oop101 = await GetOrCreateSubjectAsync(
            context,
            "OOP101",
            "Lập trình hướng đối tượng",
            3
        );

        await UpsertTrainingProgramSubjectAsync(
            context,
            trainingProgram,
            pro101,
            1,
            3,
            1,
            "Môn học kỳ 1 của PTPM-K21"
        );
        await UpsertTrainingProgramSubjectAsync(
            context,
            trainingProgram,
            dbi101,
            2,
            3,
            1,
            "Môn học kỳ 2 của PTPM-K21"
        );
        await UpsertTrainingProgramSubjectAsync(
            context,
            trainingProgram,
            web101,
            2,
            3,
            2,
            "Môn học kỳ 2 của PTPM-K21"
        );
        await UpsertTrainingProgramSubjectAsync(
            context,
            trainingProgram,
            oop101,
            3,
            3,
            1,
            "Môn học kỳ 3 của PTPM-K21"
        );

        await SeedCourseSyllabusTestDataAsync(context, trainingProgram, specialization, pro101);
    }

    private static async Task SeedCourseSyllabusTestDataAsync(
        ApplicationDbContext context,
        ChuongTrinhDaoTao trainingProgram,
        ChuyenNganh specialization,
        DanhMucMonHoc pro101
    )
    {
        var pro101ProgramSubject = await context.MonHocTrongChuongTrinhs.FirstOrDefaultAsync(x =>
            x.MaChuongTrinh == trainingProgram.MaChuongTrinh && x.MaMonHoc == pro101.MaMonHoc
        );

        if (pro101ProgramSubject != null)
        {
            await UpsertCourseSyllabusAsync(
                context,
                pro101,
                specialization,
                pro101ProgramSubject,
                "Syllabus PRO101 - PTPM-K21",
                "2026.1",
                "Syllabus test cho môn PRO101 trong chương trình PTPM-K21"
            );
        }

        await UpsertCourseSyllabusAsync(
            context,
            pro101,
            specialization,
            null,
            "Syllabus chung PRO101",
            "common-2026",
            "Syllabus chung cho môn PRO101"
        );
    }

    private static async Task UpsertCourseSyllabusAsync(
        ApplicationDbContext context,
        DanhMucMonHoc subject,
        ChuyenNganh specialization,
        MonHocTrongChuongTrinh? programSubject,
        string name,
        string version,
        string description
    )
    {
        var maChuongTrinhMonHoc = programSubject?.MaChuongTrinhMonHoc;

        var syllabus = await context.CourseSyllabuses.FirstOrDefaultAsync(x =>
            x.MaMonHoc == subject.MaMonHoc
            && x.MaChuongTrinhMonHoc == maChuongTrinhMonHoc
            && x.Version == version
        );

        if (syllabus is null)
        {
            syllabus = new CourseSyllabus
            {
                MaMonHoc = subject.MaMonHoc,
                MaChuyenNganh = specialization.MaChuyenNganh,
                NgayTao = DateTime.UtcNow,
            };
            context.CourseSyllabuses.Add(syllabus);
        }

        syllabus.TenSyllabus = name;
        syllabus.Version = version;
        syllabus.MaChuongTrinhMonHoc = programSubject?.MaChuongTrinhMonHoc;
        syllabus.TrangThai = SyllabusDraftStatus;
        syllabus.ConHoatDong = true;
        syllabus.BatBuoc = true;
        syllabus.NgayCapNhat = DateTime.UtcNow;
    }

    private static async Task<DonVi> GetOrCreateCampusAsync(
        ApplicationDbContext context,
        DonVi rootUnit
    )
    {
        var campus = await context.DonVis.FirstOrDefaultAsync(x => x.TenDonVi == CampusHcmName);
        if (campus is null)
        {
            campus = new DonVi
            {
                MaDonViCha = rootUnit.MaDonVi,
                TenDonVi = CampusHcmName,
                CapDonVi = CampusLevel,
                ConHoatDong = true,
                NgayTao = DateTime.UtcNow,
            };

            context.DonVis.Add(campus);
            await context.SaveChangesAsync();
        }
        else
        {
            campus.MaDonViCha = rootUnit.MaDonVi;
            campus.CapDonVi = CampusLevel;
            campus.ConHoatDong = true;
            campus.NgayCapNhat = DateTime.UtcNow;
        }

        return campus;
    }

    private static async Task<KhoaTuyenSinh> GetOrCreateCohortAsync(
        ApplicationDbContext context,
        string code,
        string name,
        int startYear,
        int? expectedEndYear,
        string description
    )
    {
        var cohort = await context.KhoaTuyenSinhs.FirstOrDefaultAsync(x => x.MaCodeKhoa == code);
        if (cohort is null)
        {
            cohort = new KhoaTuyenSinh { MaCodeKhoa = code, NgayTao = DateTime.UtcNow };

            context.KhoaTuyenSinhs.Add(cohort);
        }

        cohort.TenKhoa = name;
        cohort.NamBatDau = startYear;
        cohort.NamKetThucDuKien = expectedEndYear;
        cohort.MoTa = description;
        cohort.ConHoatDong = true;
        cohort.NgayCapNhat = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return cohort;
    }

    private static async Task<NganhDaoTao> GetOrCreateMajorAsync(ApplicationDbContext context)
    {
        var major = await context.NganhDaoTaos.FirstOrDefaultAsync(x => x.MaCodeNganh == "CNTT");
        if (major is null)
        {
            major = new NganhDaoTao { MaCodeNganh = "CNTT", NgayTao = DateTime.UtcNow };

            context.NganhDaoTaos.Add(major);
        }

        major.TenNganh = "Công nghệ thông tin";
        major.MoTa = "Ngành Công nghệ thông tin dùng để test chương trình đào tạo";
        major.ConHoatDong = true;
        major.NgayCapNhat = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return major;
    }

    private static async Task<ChuyenNganh> GetOrCreateSpecializationAsync(
        ApplicationDbContext context,
        NganhDaoTao major
    )
    {
        var specialization = await context.ChuyenNganhs.FirstOrDefaultAsync(x =>
            x.MaCodeChuyenNganh == "PTPM"
        );
        if (specialization is null)
        {
            specialization = new ChuyenNganh
            {
                MaCodeChuyenNganh = "PTPM",
                NgayTao = DateTime.UtcNow,
            };

            context.ChuyenNganhs.Add(specialization);
        }

        specialization.MaNganh = major.MaNganh;
        specialization.TenChuyenNganh = "Phát triển phần mềm";
        specialization.MoTa =
            "Chuyên ngành Phát triển phần mềm dùng để test clone chương trình đào tạo";
        specialization.ConHoatDong = true;
        specialization.NgayCapNhat = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return specialization;
    }

    private static async Task<ChuyenNganhTheoCoSo> GetOrCreateCampusSpecializationAsync(
        ApplicationDbContext context,
        ChuyenNganh specialization,
        DonVi campus
    )
    {
        var campusSpecialization = await context.ChuyenNganhTheoCoSos.FirstOrDefaultAsync(x =>
            x.MaChuyenNganh == specialization.MaChuyenNganh && x.MaDonVi == campus.MaDonVi
        );

        if (campusSpecialization is null)
        {
            campusSpecialization = new ChuyenNganhTheoCoSo
            {
                MaChuyenNganh = specialization.MaChuyenNganh,
                MaDonVi = campus.MaDonVi,
                NgayTao = DateTime.UtcNow,
            };

            context.ChuyenNganhTheoCoSos.Add(campusSpecialization);
        }

        campusSpecialization.TrangThai = ApprovedStatus;
        campusSpecialization.NamBatDau = 2026;
        campusSpecialization.ChiTieuDuKien = 100;
        campusSpecialization.GhiChu = "Dữ liệu test cho campus xem chương trình đào tạo";
        campusSpecialization.ConHoatDong = true;
        campusSpecialization.NgayCapNhat = DateTime.UtcNow;

        return campusSpecialization;
    }

    private static async Task<ChuongTrinhDaoTao> GetOrCreateTrainingProgramAsync(
        ApplicationDbContext context,
        ChuyenNganh specialization,
        KhoaTuyenSinh cohort
    )
    {
        var trainingProgram = await context.ChuongTrinhDaoTaos.FirstOrDefaultAsync(x =>
            x.MaCodeChuongTrinh == TrainingProgramSourceCode
        );

        if (trainingProgram is null)
        {
            trainingProgram = await context.ChuongTrinhDaoTaos.FirstOrDefaultAsync(x =>
                x.MaChuyenNganh == specialization.MaChuyenNganh
                && x.MaKhoaTuyenSinh == cohort.MaKhoaTuyenSinh
                && x.Version == "2026.1"
            );
        }

        if (trainingProgram is null)
        {
            trainingProgram = new ChuongTrinhDaoTao
            {
                MaCodeChuongTrinh = TrainingProgramSourceCode,
                NgayTao = DateTime.UtcNow,
            };

            context.ChuongTrinhDaoTaos.Add(trainingProgram);
        }

        trainingProgram.MaChuyenNganh = specialization.MaChuyenNganh;
        trainingProgram.MaKhoaTuyenSinh = cohort.MaKhoaTuyenSinh;
        trainingProgram.MaCodeChuongTrinh = TrainingProgramSourceCode;
        trainingProgram.TenChuongTrinh = "Chương trình đào tạo Phát triển phần mềm K21";
        trainingProgram.Version = "2026.1";
        trainingProgram.SoHocKy = 7;
        trainingProgram.ThoiGianDaoTaoThang = 28;
        trainingProgram.TongTinChiYeuCau = 125;
        trainingProgram.SoTinChiToiThieuMoiKy = 12;
        trainingProgram.SoTinChiToiDaMoiKy = 24;
        trainingProgram.TrangThai = ActiveStatus;
        trainingProgram.MoTa = "Chương trình nguồn để test clone sang K22";
        trainingProgram.NguonChuongTrinhId = null;
        trainingProgram.GhiChuThayDoi = null;
        trainingProgram.NgayHieuLuc = new DateOnly(2026, 9, 1);
        trainingProgram.NgayHetHieuLuc = null;
        trainingProgram.ConHoatDong = true;
        trainingProgram.NgayCapNhat = DateTime.UtcNow;

        return trainingProgram;
    }

    private static async Task<DanhMucMonHoc> GetOrCreateSubjectAsync(
        ApplicationDbContext context,
        string code,
        string name,
        int credits
    )
    {
        var subject = await context.DanhMucMonHocs.FirstOrDefaultAsync(x => x.MaCodeMonHoc == code);
        if (subject is null)
        {
            subject = new DanhMucMonHoc { MaCodeMonHoc = code };

            context.DanhMucMonHocs.Add(subject);
        }

        subject.TenMonHoc = name;
        subject.SoTinChi = credits;
        subject.ConHoatDong = true;

        await context.SaveChangesAsync();
        return subject;
    }

    private static async Task UpsertTrainingProgramSubjectAsync(
        ApplicationDbContext context,
        ChuongTrinhDaoTao trainingProgram,
        DanhMucMonHoc subject,
        int expectedTerm,
        int credits,
        int order,
        string note
    )
    {
        var programSubject = await context.MonHocTrongChuongTrinhs.FirstOrDefaultAsync(x =>
            x.MaChuongTrinh == trainingProgram.MaChuongTrinh && x.MaMonHoc == subject.MaMonHoc
        );

        if (programSubject is null)
        {
            programSubject = new MonHocTrongChuongTrinh
            {
                MaChuongTrinh = trainingProgram.MaChuongTrinh,
                MaMonHoc = subject.MaMonHoc,
                NgayTao = DateTime.UtcNow,
            };

            context.MonHocTrongChuongTrinhs.Add(programSubject);
        }

        programSubject.HocKyDuKien = expectedTerm;
        programSubject.SoTinChi = credits;
        programSubject.LoaiMonHoc = RequiredSubjectType;
        programSubject.BatBuoc = true;
        programSubject.ThuTu = order;
        programSubject.GhiChu = note;
        programSubject.ConHoatDong = true;
        programSubject.NgayCapNhat = DateTime.UtcNow;
    }
}
