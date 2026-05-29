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
    private const string CampusHcmAdminEmail = "campusadmin.hcm@lms.local";
    private const string CampusHcmAdminDefaultPassword = "CampusAdmin@123456";
    private const string CampusHcmAdminFullName = "Quản trị cơ sở Hồ Chí Minh";
    private const string CampusAdminRoleCode = "quan_tri_co_so";
    private const string CampusHcmName = "Cơ sở Hồ Chí Minh";
    private const string CampusLevel = "co_so";
    private const string ApprovedStatus = "approved";
    private const string ActiveStatus = "active";
    private const string RoomActiveStatus = "hoat_dong";
    private const string RequiredSubjectType = "bat_buoc";
    private const string SyllabusDraftStatus = "draft";
    private const string TrainingProgramSourceCode = "PTPM-K21";
    private const string TuitionCampusHcmName = "FPT HCM";
    private const string TuitionCampusCanThoName = "FPT Cần Thơ";
    private const string TuitionProgramCode = "CT_CNTT_K2026";
    private const string TuitionCalculationType = "co_dinh_theo_hoc_ky";

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
        await SeedCampusHcmAdminUserAsync(context, campus);
        await SeedFacilityTestDataAsync(context, campus);

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
        await SeedAcademicTermsAndMappingAsync(context, campus, trainingProgram);
        await SeedProgramTuitionConfigTestDataAsync(context, rootUnit, specialization);
    }

    private static async Task SeedCampusHcmAdminUserAsync(
        ApplicationDbContext context,
        DonVi campus
    )
    {
        var campusAdmin = await context.NguoiDungs.FirstOrDefaultAsync(x =>
            x.Email == CampusHcmAdminEmail
        );

        if (campusAdmin is null)
        {
            campusAdmin = new NguoiDung
            {
                MaDonVi = campus.MaDonVi,
                Email = CampusHcmAdminEmail,
                HoTen = CampusHcmAdminFullName,
                VaiTroChinh = CampusAdminRoleCode,
                TrangThai = UserStatuses.DbFirstLogin,
                MatKhauHash = PasswordHelper.HashPassword(CampusHcmAdminDefaultPassword),
                NgayTao = DateTime.UtcNow,
                SoLanSaiMatKhau = 0,
                DangNhapLanDau = true,
            };

            context.NguoiDungs.Add(campusAdmin);
            await context.SaveChangesAsync();
        }
        else
        {
            campusAdmin.MaDonVi = campus.MaDonVi;
            campusAdmin.HoTen = string.IsNullOrWhiteSpace(campusAdmin.HoTen)
                ? CampusHcmAdminFullName
                : campusAdmin.HoTen;
            campusAdmin.VaiTroChinh = CampusAdminRoleCode;

            if (string.IsNullOrWhiteSpace(campusAdmin.MatKhauHash))
            {
                campusAdmin.MatKhauHash = PasswordHelper.HashPassword(CampusHcmAdminDefaultPassword);
                campusAdmin.TrangThai = UserStatuses.DbFirstLogin;
                campusAdmin.DangNhapLanDau = true;
            }

            await context.SaveChangesAsync();
        }

        var campusAdminRole = await context.VaiTros.FirstOrDefaultAsync(x =>
            x.MaCodeVaiTro == CampusAdminRoleCode
        );

        if (campusAdminRole is not null)
        {
            var hasRoleAssignment = await context.PhanQuyenNguoiDungs.AnyAsync(x =>
                x.MaNguoiDung == campusAdmin.MaNguoiDung && x.MaVaiTro == campusAdminRole.MaVaiTro
            );

            if (!hasRoleAssignment)
            {
                context.PhanQuyenNguoiDungs.Add(new PhanQuyenNguoiDung
                {
                    MaNguoiDung = campusAdmin.MaNguoiDung,
                    MaVaiTro = campusAdminRole.MaVaiTro,
                    NgayGan = DateTime.UtcNow,
                });
            }
        }
    }

    private static async Task SeedFacilityTestDataAsync(ApplicationDbContext context, DonVi campus)
    {
        var buildingPlans = new[]
        {
            (Code: "A", Name: "Tòa nhà A", FloorCount: 4),
            (Code: "B", Name: "Tòa nhà B", FloorCount: 5),
            (Code: "C", Name: "Tòa nhà C", FloorCount: 6),
        };

        foreach (var plan in buildingPlans)
        {
            var building = await GetOrCreateBuildingAsync(
                context,
                campus,
                plan.Code,
                plan.Name,
                plan.FloorCount
            );

            for (var floorNumber = 1; floorNumber <= plan.FloorCount; floorNumber++)
            {
                var floor = await GetOrCreateFloorAsync(context, building, floorNumber);
                await SeedRoomsForFloorAsync(context, campus, building, floor, floorNumber);
            }
        }
    }

    private static async Task<ToaNha> GetOrCreateBuildingAsync(
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
        building.DiaChi = $"Khu {code} - {campus.TenDonVi}";
        building.SoTang = floorCount;
        building.ConHoatDong = true;
        building.NgayCapNhat = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return building;
    }

    private static async Task<Tang> GetOrCreateFloorAsync(
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
            floor = new Tang
            {
                MaToaNha = building.MaToaNha,
                ThuTuTang = floorNumber,
            };

            context.Tangs.Add(floor);
        }

        floor.TenTang = $"Tầng {floorNumber}";
        floor.MoTa = $"Tầng {floorNumber} thuộc {building.TenToaNha}";
        floor.ConHoatDong = true;

        await context.SaveChangesAsync();
        return floor;
    }

    private static async Task SeedRoomsForFloorAsync(
        ApplicationDbContext context,
        DonVi campus,
        ToaNha building,
        Tang floor,
        int floorNumber
    )
    {
        for (var roomNumber = 1; roomNumber <= 10; roomNumber++)
        {
            var roomCode = $"{building.MaCodeToaNha}{floorNumber}{roomNumber:00}";
            var room = await context.PhongHocs.FirstOrDefaultAsync(x =>
                x.MaDonVi == campus.MaDonVi && x.MaCodePhong == roomCode
            );

            if (room is null)
            {
                room = new PhongHoc
                {
                    MaDonVi = campus.MaDonVi,
                    MaCodePhong = roomCode,
                };

                context.PhongHocs.Add(room);
            }

            room.MaToaNha = building.MaToaNha;
            room.MaTang = floor.MaTang;
            room.TenPhong = $"Phòng {roomCode}";
            room.SucChua = GetSeedRoomCapacity(roomNumber);
            room.LoaiPhong = GetSeedRoomType(roomNumber);
            room.TrangThaiPhong = RoomActiveStatus;
            room.GhiChu = $"Dữ liệu seed phòng học {building.TenToaNha} - {floor.TenTang}";
        }

        await context.SaveChangesAsync();
    }

    private static int GetSeedRoomCapacity(int roomNumber)
    {
        return roomNumber % 5 == 0 ? 80 : roomNumber % 3 == 0 ? 60 : 40;
    }

    private static string GetSeedRoomType(int roomNumber)
    {
        return roomNumber switch
        {
            9 => "lab",
            10 => "hoi_truong",
            3 or 6 => "phong_thi_nghiem",
            4 or 8 => "thuc_hanh",
            _ => "ly_thuyet"
        };
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
        trainingProgram.SoHocKy = 9;
        trainingProgram.ThoiGianDaoTaoThang = 36;
        trainingProgram.TongTinChiYeuCau = 125;
        trainingProgram.SoTinChiToiThieuMoiKy = 12;
        trainingProgram.SoTinChiToiDaMoiKy = 24;
        trainingProgram.TrangThai = ActiveStatus;
        trainingProgram.MoTa = "Chương trình nguồn để test clone sang K22";
        trainingProgram.NguonChuongTrinhId = null;
        trainingProgram.GhiChuThayDoi = null;
        trainingProgram.NgayHieuLuc = new DateOnly(2025, 9, 1);
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

    private static async Task SeedAcademicTermsAndMappingAsync(
        ApplicationDbContext context,
        DonVi campus,
        ChuongTrinhDaoTao trainingProgram)
    {
        var terms = new[]
        {
            // (maCode, ten, namHoc, thuTuTrongNam, start, end, block5, thuTuHocKy)
            ("FAL2025", "Học kỳ Fall 2025",              "2025-2026", 3, new DateOnly(2025, 9, 1),  new DateOnly(2025, 12, 31), new DateOnly(2026, 1, 5),   1),
            ("SPR2026", "Học kỳ Spring 2026",            "2025-2026", 1, new DateOnly(2026, 1, 1),  new DateOnly(2026, 4, 30),  new DateOnly(2026, 5, 5),   2),
            ("SUM2026", "Học kỳ Summer 2026",            "2025-2026", 2, new DateOnly(2026, 5, 1),  new DateOnly(2026, 8, 31),  new DateOnly(2026, 9, 5),   3),
            ("FAL2026", "Học kỳ Fall 2026",              "2026-2027", 3, new DateOnly(2026, 9, 1),  new DateOnly(2026, 12, 31), new DateOnly(2027, 1, 5),   4),
            ("SPR2027", "Học kỳ Spring 2027",            "2026-2027", 1, new DateOnly(2027, 1, 1),  new DateOnly(2027, 4, 30),  new DateOnly(2027, 5, 5),   5),
            ("SUM2027", "Học kỳ Summer 2027 (OJT)",      "2026-2027", 2, new DateOnly(2027, 5, 1),  new DateOnly(2027, 8, 31),  new DateOnly(2027, 9, 5),   6),
            ("FAL2027", "Học kỳ Fall 2027",              "2027-2028", 3, new DateOnly(2027, 9, 1),  new DateOnly(2027, 12, 31), new DateOnly(2028, 1, 5),   7),
            ("SPR2028", "Học kỳ Spring 2028",            "2027-2028", 1, new DateOnly(2028, 1, 1),  new DateOnly(2028, 4, 30),  new DateOnly(2028, 5, 5),   8),
            ("SUM2028", "Học kỳ Summer 2028 (Tốt nghiệp)","2027-2028", 2, new DateOnly(2028, 5, 1),  new DateOnly(2028, 8, 31),  new DateOnly(2028, 9, 5),   9),
        };

        foreach (var (maCode, ten, namHoc, thuTuTrongNam, start, end, block5, thuTuHocKy) in terms)
        {
            var hocKy = await context.HocKys.FirstOrDefaultAsync(x =>
                x.MaCodeHocKy == maCode && x.MaDonVi == campus.MaDonVi);

            if (hocKy is null)
            {
                hocKy = new HocKy
                {
                    MaDonVi = campus.MaDonVi,
                    MaCodeHocKy = maCode,
                    TenHocKy = ten,
                    NamHoc = namHoc,
                    ThuTuTrongNam = thuTuTrongNam,
                    NgayBatDau = start,
                    NgayKetThuc = end,
                    NgayKetThucBlock5 = block5,
                    DaKhoa = false,
                    SoTinChiToiDa = 24,
                    HanRutMon = start.AddDays(14),
                };
                context.HocKys.Add(hocKy);
                await context.SaveChangesAsync();
            }
            else
            {
                hocKy.MaCodeHocKy = maCode;
                hocKy.TenHocKy = ten;
                hocKy.NamHoc = namHoc;
                hocKy.ThuTuTrongNam = thuTuTrongNam;
                hocKy.NgayBatDau = start;
                hocKy.NgayKetThuc = end;
                hocKy.NgayKetThucBlock5 = block5;
            }

            var mapping = await context.ChuongTrinhHocKys.FirstOrDefaultAsync(x =>
                x.MaChuongTrinh == trainingProgram.MaChuongTrinh &&
                x.ThuTuHocKy == thuTuHocKy);

            if (mapping is null)
            {
                mapping = new ChuongTrinhHocKy
                {
                    MaChuongTrinh = trainingProgram.MaChuongTrinh,
                    MaHocKy = hocKy.MaHocKy,
                    ThuTuHocKy = thuTuHocKy,
                };
                context.ChuongTrinhHocKys.Add(mapping);
            }
            else
            {
                mapping.MaHocKy = hocKy.MaHocKy;
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedProgramTuitionConfigTestDataAsync(
        ApplicationDbContext context,
        DonVi rootUnit,
        ChuyenNganh specialization)
    {
        var fptHcm = await GetOrCreateCampusByNameAsync(context, rootUnit, TuitionCampusHcmName);
        var fptCanTho = await GetOrCreateCampusByNameAsync(context, rootUnit, TuitionCampusCanThoName);

        await GetOrCreateCampusSpecializationAsync(context, specialization, fptHcm);
        await GetOrCreateCampusSpecializationAsync(context, specialization, fptCanTho);

        var cohort = await GetOrCreateCohortAsync(
            context,
            "K2026",
            "Khóa K2026",
            2026,
            2030,
            "Khóa tuyển sinh K2026 dùng để demo cấu hình học phí chương trình"
        );

        var trainingProgram = await GetOrCreateProgramTuitionTrainingProgramAsync(
            context,
            specialization,
            cohort
        );
        await context.SaveChangesAsync();

        var hcmTerms = await GetOrCreateProgramTuitionTermsAsync(context, fptHcm);
        var canThoTerms = await GetOrCreateProgramTuitionTermsAsync(context, fptCanTho);

        await UpsertProgramTuitionConfigsAsync(
            context,
            fptHcm,
            trainingProgram,
            hcmTerms,
            28_000_000m,
            2_000_000m
        );

        await UpsertProgramTuitionConfigsAsync(
            context,
            fptCanTho,
            trainingProgram,
            canThoTerms,
            25_000_000m,
            2_000_000m
        );

        await context.SaveChangesAsync();
    }

    private static async Task<DonVi> GetOrCreateCampusByNameAsync(
        ApplicationDbContext context,
        DonVi rootUnit,
        string campusName)
    {
        var campus = await context.DonVis.FirstOrDefaultAsync(x => x.TenDonVi == campusName);
        if (campus is null)
        {
            campus = new DonVi
            {
                MaDonViCha = rootUnit.MaDonVi,
                TenDonVi = campusName,
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

    private static async Task<ChuongTrinhDaoTao> GetOrCreateProgramTuitionTrainingProgramAsync(
        ApplicationDbContext context,
        ChuyenNganh specialization,
        KhoaTuyenSinh cohort)
    {
        var trainingProgram = await context.ChuongTrinhDaoTaos.FirstOrDefaultAsync(x =>
            x.MaCodeChuongTrinh == TuitionProgramCode
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
                MaCodeChuongTrinh = TuitionProgramCode,
                NgayTao = DateTime.UtcNow,
            };

            context.ChuongTrinhDaoTaos.Add(trainingProgram);
        }

        trainingProgram.MaChuyenNganh = specialization.MaChuyenNganh;
        trainingProgram.MaKhoaTuyenSinh = cohort.MaKhoaTuyenSinh;
        trainingProgram.MaCodeChuongTrinh = TuitionProgramCode;
        trainingProgram.TenChuongTrinh = "Công nghệ thông tin K2026";
        trainingProgram.Version = "2026.1";
        trainingProgram.SoHocKy = 9;
        trainingProgram.ThoiGianDaoTaoThang = 36;
        trainingProgram.TongTinChiYeuCau = 125;
        trainingProgram.SoTinChiToiThieuMoiKy = 12;
        trainingProgram.SoTinChiToiDaMoiKy = 24;
        trainingProgram.TrangThai = ActiveStatus;
        trainingProgram.MoTa = "Chương trình demo cho cấu hình học phí cố định theo học kỳ";
        trainingProgram.NguonChuongTrinhId = null;
        trainingProgram.GhiChuThayDoi = null;
        trainingProgram.NgayHieuLuc = new DateOnly(2026, 1, 1);
        trainingProgram.NgayHetHieuLuc = null;
        trainingProgram.ConHoatDong = true;
        trainingProgram.NgayCapNhat = DateTime.UtcNow;

        return trainingProgram;
    }

    private static async Task<IReadOnlyList<HocKy>> GetOrCreateProgramTuitionTermsAsync(
        ApplicationDbContext context,
        DonVi campus)
    {
        var termPlans = new[]
        {
            (Code: "HK1_2026", Name: "CNTT K2026 - Năm 1 - Học kỳ 1", Year: "2026-2027", TermInYear: 1, Start: new DateOnly(2026, 9, 1), End: new DateOnly(2026, 12, 31)),
            (Code: "HK2_2027", Name: "CNTT K2026 - Năm 1 - Học kỳ 2", Year: "2026-2027", TermInYear: 2, Start: new DateOnly(2027, 1, 1), End: new DateOnly(2027, 4, 30)),
            (Code: "HK3_2027", Name: "CNTT K2026 - Năm 1 - Học kỳ 3", Year: "2026-2027", TermInYear: 3, Start: new DateOnly(2027, 5, 1), End: new DateOnly(2027, 8, 31)),
            (Code: "HK4_2027", Name: "CNTT K2026 - Năm 2 - Học kỳ 1", Year: "2027-2028", TermInYear: 1, Start: new DateOnly(2027, 9, 1), End: new DateOnly(2027, 12, 31)),
            (Code: "HK5_2028", Name: "CNTT K2026 - Năm 2 - Học kỳ 2", Year: "2027-2028", TermInYear: 2, Start: new DateOnly(2028, 1, 1), End: new DateOnly(2028, 4, 30)),
            (Code: "HK6_2028", Name: "CNTT K2026 - Năm 2 - Học kỳ 3", Year: "2027-2028", TermInYear: 3, Start: new DateOnly(2028, 5, 1), End: new DateOnly(2028, 8, 31)),
            (Code: "HK7_2028", Name: "CNTT K2026 - Năm 3 - Học kỳ 1", Year: "2028-2029", TermInYear: 1, Start: new DateOnly(2028, 9, 1), End: new DateOnly(2028, 12, 31)),
            (Code: "HK8_2029", Name: "CNTT K2026 - Năm 3 - Học kỳ 2", Year: "2028-2029", TermInYear: 2, Start: new DateOnly(2029, 1, 1), End: new DateOnly(2029, 4, 30)),
            (Code: "HK9_2029", Name: "CNTT K2026 - Năm 3 - Học kỳ 3", Year: "2028-2029", TermInYear: 3, Start: new DateOnly(2029, 5, 1), End: new DateOnly(2029, 8, 31)),
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
                    && x.ThuTuTrongNam == plan.TermInYear
                );
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
            term.ThuTuTrongNam = plan.TermInYear;
            term.NgayBatDau = plan.Start;
            term.NgayKetThuc = plan.End;
            term.NgayKetThucBlock5 = plan.End.AddDays(5);
            term.DaKhoa = false;
            term.SoTinChiToiDa = 24;
            term.HanRutMon = plan.Start.AddDays(14);
            terms.Add(term);
        }

        await context.SaveChangesAsync();
        return terms;
    }

    private static async Task UpsertProgramTuitionConfigsAsync(
        ApplicationDbContext context,
        DonVi campus,
        ChuongTrinhDaoTao trainingProgram,
        IReadOnlyList<HocKy> terms,
        decimal tuitionAmount,
        decimal materialAmount)
    {
        for (var index = 0; index < terms.Count; index++)
        {
            var term = terms[index];
            var programTermOrder = index + 1;
            var config = await context.CauHinhHocPhiChuongTrinhs.FirstOrDefaultAsync(x =>
                x.MaDonVi == campus.MaDonVi
                && x.MaChuongTrinhDaoTao == trainingProgram.MaChuongTrinh
                && x.MaHocKy == term.MaHocKy
            );

            if (config is null)
            {
                config = new CauHinhHocPhiChuongTrinh
                {
                    MaDonVi = campus.MaDonVi,
                    MaChuongTrinhDaoTao = trainingProgram.MaChuongTrinh,
                    MaHocKy = term.MaHocKy,
                    NgayTao = DateTime.UtcNow,
                };

                context.CauHinhHocPhiChuongTrinhs.Add(config);
            }

            var yearInProgram = (index / 3) + 1;
            var termInYear = (index % 3) + 1;
            var yearTuitionAmount = tuitionAmount + ((yearInProgram - 1) * 1_000_000m);

            config.NamHocTrongChuongTrinh = yearInProgram;
            config.HocKyTrongNam = termInYear;
            config.SoThuTuHocKy = programTermOrder;
            config.LoaiCachTinhHocPhi = TuitionCalculationType;
            config.SoTienHocPhi = yearTuitionAmount;
            config.TienHocLieu = materialAmount;
            config.TongTienDuKien = yearTuitionAmount + materialAmount;
            config.ConHoatDong = true;
            config.GhiChu = $"Seed cấu hình học phí {trainingProgram.TenChuongTrinh} - {campus.TenDonVi} - năm {yearInProgram} kỳ {termInYear}";
            config.NgayCapNhat = DateTime.UtcNow;
        }
    }
}
