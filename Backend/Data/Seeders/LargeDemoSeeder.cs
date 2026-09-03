using Backend.Constants;
using Backend.DTOs.Auth;
using Backend.Helpers;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Data.Seeders;

public static class LargeDemoSeeder
{
    public static readonly string Password = "Test@123";

    public static readonly Dictionary<string, List<string>> MajorSpecializations = new()
    {
        { "Công nghệ thông tin", new() { "Lập trình Web", "Lập trình Mobile", "Xử lý dữ liệu / AI", "An toàn thông tin", "Kiểm thử phần mềm" } },
        { "Thiết kế đồ họa", new() { "Thiết kế UI/UX", "Đồ họa truyền thông", "Hoạt hình 3D" } },
        { "Quản trị kinh doanh", new() { "Digital Marketing", "Kinh doanh quốc tế", "Quản trị Nhân sự" } },
        { "Du lịch / Nhà hàng / Khách sạn", new() { "Quản trị Khách sạn", "Hướng dẫn viên Du lịch" } },
        { "Kế toán / Tài chính", new() { "Kế toán doanh nghiệp", "Tài chính ngân hàng" } },
        { "Ngoại ngữ / Kỹ năng", new() { "Tiếng Anh thương mại", "Tiếng Hàn tổng hợp" } }
    };

    public static readonly Dictionary<string, List<string>> SpecializationSubjects = new()
    {
        { "Lập trình Web", new() { "Cơ sở dữ liệu SQL", "HTML/CSS/JS Cơ bản", "Lập trình C# ASP.NET Core", "Lập trình Frontend Vue 3", "Kiến trúc Microservices" } },
        { "Lập trình Mobile", new() { "Lập trình Dart & Flutter", "Lập trình Android Kotlin", "Kiến trúc Ứng dụng Di động", "Tối ưu hóa Ứng dụng Mobile" } },
        { "Xử lý dữ liệu / AI", new() { "Nhập môn Khoa học dữ liệu", "Lập trình Python cho AI", "Học máy (Machine Learning)", "Học sâu & Thị giác máy tính" } },
        { "An toàn thông tin", new() { "Nhập môn An ninh mạng", "Mật mã học & Xung đột mạng", "Kiểm thử xâm nhập (PenTest)" } },
        { "Kiểm thử phần mềm", new() { "Kiểm thử cơ bản (Manual Testing)", "Kiểm thử tự động (Selenium/Cypress)", "Quản lý chất lượng phần mềm" } },
        { "Thiết kế UI/UX", new() { "Nguyên lý Thiết kế UI", "Nghiên cứu Trải nghiệm Người dùng UX", "Thiết kế Hệ thống (Design System)" } },
        { "Đồ họa truyền thông", new() { "Xử lý ảnh Photoshop", "Thiết kế Vector Illustrator", "Biên tập Video After Effects" } },
        { "Hoạt hình 3D", new() { "Dựng hình 3D Maya", "Diễn hoạt 3D Animation", "Xử lý ánh sáng & Render" } },
        { "Digital Marketing", new() { "Nhập môn Digital Marketing", "SEO & Content Marketing", "Quảng cáo Facebook & Google Ads" } },
        { "Kinh doanh quốc tế", new() { "Thương mại quốc tế", "Thanh toán quốc tế", "Quản trị Chuỗi cung ứng (SCM)" } },
        { "Quản trị Nhân sự", new() { "Quản trị Nguồn nhân lực", "Tuyển dụng & Đào tạo", "Luật lao động & Tiền lương" } },
        { "Quản trị Khách sạn", new() { "Quản trị Lễ tân", "Quản trị Buồng phòng", "Tổ chức Sự kiện & Hội nghị" } },
        { "Hướng dẫn viên Du lịch", new() { "Tuyến điểm du lịch Việt Nam", "Nghiệp vụ Hướng dẫn du lịch", "Địa lý & Văn hóa du lịch" } },
        { "Kế toán doanh nghiệp", new() { "Kế toán tài chính 1", "Kế toán quản trị", "Thuế & Báo cáo thuế" } },
        { "Tài chính ngân hàng", new() { "Thị trường tài chính", "Nghiệp vụ Ngân hàng thương mại", "Phân tích Đầu tư tài chính" } },
        { "Tiếng Anh thương mại", new() { "Tiếng Anh Giao tiếp 1", "Tiếng Anh Thương mại", "Kỹ năng Thuyết trình Tiếng Anh" } },
        { "Tiếng Hàn tổng hợp", new() { "Tiếng Hàn Sơ cấp 1", "Tiếng Hàn Trung cấp", "Tiếng Hàn Biên phiên dịch" } }
    };

    public static async Task SeedAsync(ApplicationDbContext context)
    {
        Console.WriteLine("Starting Multi-Campus LargeDemo Seed V11...");

        var gradeCount = await context.DiemSos.CountAsync();
        if (gradeCount > 20000)
        {
            Console.WriteLine("Multi-Campus LargeDemo data (V11) already exists. Skipping.");
            await SeedSmartSchedulingDemoTermAsync(context);
            return;
        }

        // Get or Create Campuses
        var campuses = await context.DonVis.Where(x => x.CapDonVi == "co_so" && x.ConHoatDong).ToListAsync();
        if (campuses.Count < 3)
        {
            var root = await context.DonVis.FirstOrDefaultAsync(x => x.CapDonVi == "root")
                ?? new DonVi { TenDonVi = "LMS Root", CapDonVi = "root", ConHoatDong = true, NgayTao = DateTime.UtcNow };
            if (root.MaDonVi == 0) { context.DonVis.Add(root); await context.SaveChangesAsync(); }

            var hanoi = new DonVi { TenDonVi = "FPT Polytechnic Hà Nội", CapDonVi = "co_so", MaDonViCha = root.MaDonVi, ConHoatDong = true, NgayTao = DateTime.UtcNow };
            var hcm = new DonVi { TenDonVi = "FPT Polytechnic Hồ Chí Minh", CapDonVi = "co_so", MaDonViCha = root.MaDonVi, ConHoatDong = true, NgayTao = DateTime.UtcNow };
            var danang = new DonVi { TenDonVi = "FPT Polytechnic Đà Nẵng", CapDonVi = "co_so", MaDonViCha = root.MaDonVi, ConHoatDong = true, NgayTao = DateTime.UtcNow };
            context.DonVis.AddRange(hanoi, hcm, danang);
            await context.SaveChangesAsync();
            campuses = new List<DonVi> { hanoi, hcm, danang };
        }

        var random = new Random(42);

        // 1. Seed Academic Terms for ALL Campuses
        Console.WriteLine("Seeding Academic Terms for ALL Campuses...");
        var termNames = new List<(string Code, string Name, string Year, int Order, bool Closed)>
        {
            ("HK1_2024", "Học kỳ 1 Năm 2024", "2024", 1, true),
            ("HK2_2024", "Học kỳ 2 Năm 2024", "2024", 2, true),
            ("HK3_2024", "Học kỳ 3 Năm 2024", "2024", 3, true),
            ("HK1_2025", "Học kỳ 1 Năm 2025", "2025", 1, true),
            ("HK2_2025", "Học kỳ 2 Năm 2025", "2025", 2, true),
            ("HK3_2025", "Học kỳ 3 Năm 2025", "2025", 3, true),
            ("HK1_2026", "Học kỳ 1 Năm 2026", "2026", 1, false),
            ("HK2_2026", "Học kỳ 2 Năm 2026", "2026", 2, false),
            ("HK3_2026", "Học kỳ 3 Năm 2026", "2026", 3, false)
        };

        var allCampusTerms = new List<HocKy>();
        foreach (var campus in campuses)
        {
            foreach (var t in termNames)
            {
                var termCode = $"{t.Code}_{campus.MaDonVi}";
                var existingTerm = await context.HocKys.FirstOrDefaultAsync(x => (x.MaDonVi == campus.MaDonVi && x.NamHoc == t.Year && x.ThuTuTrongNam == t.Order) || x.MaCodeHocKy == termCode);
                if (existingTerm == null)
                {
                    existingTerm = new HocKy
                    {
                        MaCodeHocKy = termCode,
                        TenHocKy = $"{t.Name} ({campus.TenDonVi.Replace("FPT Polytechnic ", "")})",
                        NamHoc = t.Year,
                        ThuTuTrongNam = t.Order,
                        DaKhoa = t.Closed,
                        MaDonVi = campus.MaDonVi,
                        NgayBatDau = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths((t.Order - 2) * 4)),
                        NgayKetThuc = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths((t.Order - 1) * 4))
                    };
                    context.HocKys.Add(existingTerm);
                }
                allCampusTerms.Add(existingTerm);
            }
        }
        await context.SaveChangesAsync();

        // 2. Majors & Specializations
        Console.WriteLine("Seeding Majors & Specializations...");
        var majorDict = new Dictionary<string, NganhDaoTao>();
        var specDict = new Dictionary<string, ChuyenNganh>();

        foreach (var majorKvp in MajorSpecializations)
        {
            var majorName = majorKvp.Key;
            var major = await context.NganhDaoTaos.FirstOrDefaultAsync(x => x.TenNganh == majorName);
            if (major == null)
            {
                major = new NganhDaoTao { MaCodeNganh = $"M_{Guid.NewGuid().ToString().Substring(0, 5)}", TenNganh = majorName, ConHoatDong = true, NgayTao = DateTime.UtcNow };
                context.NganhDaoTaos.Add(major);
                await context.SaveChangesAsync();
            }
            majorDict[majorName] = major;

            foreach (var specName in majorKvp.Value)
            {
                var spec = await context.ChuyenNganhs.FirstOrDefaultAsync(x => x.TenChuyenNganh == specName);
                if (spec == null)
                {
                    spec = new ChuyenNganh { MaNganh = major.MaNganh, TenChuyenNganh = specName, ConHoatDong = true, NgayTao = DateTime.UtcNow };
                    context.ChuyenNganhs.Add(spec);
                    await context.SaveChangesAsync();
                }
                specDict[specName] = spec;
            }
        }

        // 3. Subjects
        Console.WriteLine("Seeding Subjects...");
        var subjectDict = new Dictionary<string, DanhMucMonHoc>();
        foreach (var specs in SpecializationSubjects.Values)
        {
            foreach (var sub in specs)
            {
                if (!subjectDict.ContainsKey(sub))
                {
                    var existingSub = await context.DanhMucMonHocs.FirstOrDefaultAsync(x => x.TenMonHoc == sub);
                    if (existingSub == null)
                    {
                        existingSub = new DanhMucMonHoc { MaCodeMonHoc = $"SUB_{Guid.NewGuid().ToString().Substring(0, 5)}", TenMonHoc = sub, SoTinChi = 3, ConHoatDong = true };
                        context.DanhMucMonHocs.Add(existingSub);
                        await context.SaveChangesAsync();
                    }
                    subjectDict[sub] = existingSub;
                }
            }
        }

        // 4. Teachers across ALL Campuses
        Console.WriteLine("Seeding Teachers across Campuses...");
        var teacherRole = AuthRoles.ToDatabaseCode(AuthRoles.Teacher);
        var passwordHash = PasswordHelper.HashPassword(Password);
        var allTeachers = new List<NguoiDung>();

        int teacherCount = 1;
        foreach (var campus in campuses)
        {
            for (int i = 1; i <= 40; i++)
            {
                var majorName = MajorSpecializations.Keys.ElementAt(random.Next(MajorSpecializations.Count));
                var specList = MajorSpecializations[majorName];
                var assignedSpecName = specList[random.Next(specList.Count)];
                var spec = specDict[assignedSpecName];

                var email = $"teacher.v11.{campus.MaDonVi}.{teacherCount:D3}@edulms.local";
                var existingTeacher = await context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == email);
                if (existingTeacher != null)
                {
                    allTeachers.Add(existingTeacher);
                    teacherCount++;
                    continue;
                }

                var teacher = new NguoiDung
                {
                    Email = email,
                    HoTen = $"Giảng Viên {assignedSpecName} ({campus.TenDonVi.Replace("FPT Polytechnic ", "")})",
                    VaiTroChinh = teacherRole,
                    MaDonVi = campus.MaDonVi,
                    TrangThai = UserStatuses.DbActive,
                    MatKhauHash = passwordHash,
                    NgayTao = DateTime.UtcNow
                };
                context.NguoiDungs.Add(teacher);
                await context.SaveChangesAsync();
                allTeachers.Add(teacher);

                // Capability
                context.GiaoVienChuyenNganhs.Add(new GiaoVienChuyenNganh
                {
                    MaGiaoVien = teacher.MaNguoiDung,
                    MaChuyenNganh = spec.MaChuyenNganh,
                    LaChuyenMonChinh = true,
                    MucDoPhuHop = random.Next(80, 101),
                    SoNamKinhNghiem = random.Next(2, 15)
                });

                var possibleSubs = SpecializationSubjects[assignedSpecName].Take(4).ToList();
                bool first = true;
                foreach (var subName in possibleSubs)
                {
                    var sub = subjectDict[subName];
                    context.GiaoVienMonHocs.Add(new GiaoVienMonHoc
                    {
                        MaGiaoVien = teacher.MaNguoiDung,
                        MaMonHoc = sub.MaMonHoc,
                        MucDoPhuHop = random.Next(70, 101),
                        SoLanDaDay = random.Next(1, 20),
                        SoNamKinhNghiem = random.Next(1, 10),
                        LaMonChinh = first
                    });
                    first = false;
                }
                teacherCount++;
            }
        }
        await context.SaveChangesAsync();

        // 5. Classes & Students across Campuses
        Console.WriteLine("Seeding Classes and 10,000 Students across Campuses...");
        var studentRole = AuthRoles.ToDatabaseCode(AuthRoles.Student);
        var allClasses = new List<LopHanhChinh>();
        var allStudents = new List<NguoiDung>();

        foreach (var campus in campuses)
        {
            var cohortCode = $"K2026_{campus.MaDonVi}";
            var cohort = await context.KhoaTuyenSinhs.FirstOrDefaultAsync(x => x.MaCodeKhoa == cohortCode);
            if (cohort == null)
            {
                cohort = new KhoaTuyenSinh { MaCodeKhoa = cohortCode, TenKhoa = $"Khóa 2026 {campus.TenDonVi}", NamBatDau = 2026, ConHoatDong = true, NgayTao = DateTime.UtcNow };
                context.KhoaTuyenSinhs.Add(cohort);
                await context.SaveChangesAsync();
            }

            var programCode = $"CT2026_{campus.MaDonVi}";
            var program = await context.ChuongTrinhDaoTaos.FirstOrDefaultAsync(x => x.MaCodeChuongTrinh == programCode);
            if (program == null)
            {
                program = new ChuongTrinhDaoTao
                {
                    MaCodeChuongTrinh = programCode,
                    TenChuongTrinh = $"Chương Trình Đào Tạo 2026 ({campus.TenDonVi})",
                    MaChuyenNganh = specDict.Values.First().MaChuyenNganh,
                    MaKhoaTuyenSinh = cohort.MaKhoaTuyenSinh,
                    SoHocKy = 7,
                    ThoiGianDaoTaoThang = 28,
                    TongTinChiYeuCau = 120,
                    Version = "2026.1",
                    TrangThai = "active",
                    ConHoatDong = true,
                    NgayTao = DateTime.UtcNow
                };
                context.ChuongTrinhDaoTaos.Add(program);
                await context.SaveChangesAsync();
            }

            var campusClasses = await context.LopHanhChinhs.Where(c => c.MaDonVi == campus.MaDonVi).ToListAsync();
            if (campusClasses.Count < 100)
            {
                var existingClassCodes = campusClasses.Select(c => c.MaCodeLop).ToHashSet();
                for (int i = 1; i <= 100; i++)
                {
                    var classCode = $"L_{campus.MaDonVi}_{i:D3}";
                    if (existingClassCodes.Contains(classCode)) continue;

                    campusClasses.Add(new LopHanhChinh
                    {
                        MaCodeLop = classCode,
                        TenLop = $"Lớp {campus.TenDonVi.Replace("FPT Polytechnic ", "")} {i:D3}",
                        MaDonVi = campus.MaDonVi,
                        MaChuongTrinh = program.MaChuongTrinh,
                        NamNhapHoc = 2026,
                        ConHoatDong = true
                    });
                }
                var newClasses = campusClasses.Where(c => c.MaLop == 0).ToList();
                if (newClasses.Any())
                {
                    context.LopHanhChinhs.AddRange(newClasses);
                    await context.SaveChangesAsync();
                }
            }
            allClasses.AddRange(campusClasses);

            // 1,000 Students per campus (Total 3,000 across 3 main campuses)
            var validCampusClasses = campusClasses.Where(c => c.MaLop > 0).ToList();
            var campusStudents = new List<NguoiDung>();
            var existingStudentEmails = (await context.NguoiDungs
                .Where(u => u.Email.StartsWith($"student.v11.{campus.MaDonVi}."))
                .Select(u => u.Email)
                .ToListAsync())
                .ToHashSet();

            for (int i = 1; i <= 1000; i++)
            {
                var email = $"student.v11.{campus.MaDonVi}.{i:D4}@edulms.local";
                if (existingStudentEmails.Contains(email)) continue;

                var lop = validCampusClasses[i % validCampusClasses.Count];
                campusStudents.Add(new NguoiDung
                {
                    Email = email,
                    HoTen = $"Sinh Viên {campus.TenDonVi.Replace("FPT Polytechnic ", "")} {i:D4}",
                    VaiTroChinh = studentRole,
                    MaDonVi = campus.MaDonVi,
                    TrangThai = UserStatuses.DbActive,
                    MatKhauHash = passwordHash,
                    MaLop = lop.MaLop,
                    NamNhapHoc = 2026,
                    NgayTao = DateTime.UtcNow
                });
            }

            // Save in batches of 1,000
            if (campusStudents.Any())
            {
                for (int b = 0; b < campusStudents.Count; b += 1000)
                {
                    var batch = campusStudents.Skip(b).Take(1000).ToList();
                    context.NguoiDungs.AddRange(batch);
                    await context.SaveChangesAsync();
                    allStudents.AddRange(batch);
                }
            }
        }

        var teacherRoleStr = AuthRoles.ToDatabaseCode(AuthRoles.Teacher);
        var studentRoleStr = AuthRoles.ToDatabaseCode(AuthRoles.Student);
        allTeachers = await context.NguoiDungs.Where(u => u.VaiTroChinh == teacherRoleStr).ToListAsync();
        allClasses = await context.LopHanhChinhs.ToListAsync();
        allStudents = await context.NguoiDungs.Where(u => u.VaiTroChinh == studentRoleStr).ToListAsync();

        // 6. Courses across Campuses & Terms
        Console.WriteLine("Seeding Courses across Campuses & Terms...");
        var allCourses = new List<KhoaHoc>();
        var subs = subjectDict.Values.ToList();
        var existingCourseKeys = (await context.KhoaHocs
            .Select(k => $"{k.MaDonVi}_{k.MaMonHoc}_{k.MaHocKy}_{k.MaLop}")
            .ToListAsync())
            .ToHashSet();

        foreach (var campus in campuses)
        {
            var campusTerms = allCampusTerms.Where(t => t.MaDonVi == campus.MaDonVi && t.ThuTuTrongNam != 2).ToList();
            var campusTeachers = allTeachers.Where(t => t.MaDonVi == campus.MaDonVi).ToList();
            var campusClasses = allClasses.Where(c => c.MaDonVi == campus.MaDonVi).ToList();

            foreach (var term in campusTerms)
            {
                for (int c = 0; c < 35; c++)
                {
                    var sub = subs[random.Next(subs.Count)];
                    var teacher = campusTeachers[random.Next(campusTeachers.Count)];
                    var lop = campusClasses[random.Next(campusClasses.Count)];
                    var key = $"{campus.MaDonVi}_{sub.MaMonHoc}_{term.MaHocKy}_{lop.MaLop}";
                    if (existingCourseKeys.Contains(key)) continue;

                    existingCourseKeys.Add(key);
                    allCourses.Add(new KhoaHoc
                    {
                        TieuDe = $"Môn {sub.TenMonHoc} - {lop.TenLop}",
                        MaMonHoc = sub.MaMonHoc,
                        MaHocKy = term.MaHocKy,
                        MaLop = lop.MaLop,
                        MaGiaoVien = teacher.MaNguoiDung,
                        MaDonVi = campus.MaDonVi,
                        TrangThai = "da_xuat_ban",
                        NgayTao = DateTime.UtcNow
                    });
                }
            }
        }
        if (allCourses.Any())
        {
            context.KhoaHocs.AddRange(allCourses);
            await context.SaveChangesAsync();
        }
        else
        {
            allCourses = await context.KhoaHocs.ToListAsync();
        }

        // 7. Grades (DiemSo) across Terms & Campuses
        Console.WriteLine("Seeding Grades across Campuses & Terms...");
        var allGrades = new List<DiemSo>();
        var existingGradeKeys = (await context.DiemSos
            .Select(g => $"{g.MaHocSinh}_{g.MaMonHoc}_{g.MaHocKy}")
            .ToListAsync())
            .ToHashSet();

        foreach (var course in allCourses)
        {
            var courseStudents = allStudents.Where(s => s.MaLop == course.MaLop && s.MaDonVi == course.MaDonVi).Take(25).ToList();
            if (!courseStudents.Any())
            {
                courseStudents = allStudents.Where(s => s.MaDonVi == course.MaDonVi).Take(15).ToList();
            }

            foreach (var st in courseStudents)
            {
                var termId = course.MaHocKy ?? 1;
                var gradeKey = $"{st.MaNguoiDung}_{course.MaMonHoc}_{termId}";
                if (existingGradeKeys.Contains(gradeKey)) continue;

                existingGradeKeys.Add(gradeKey);

                double roll = random.NextDouble();
                decimal gpa = roll > 0.80 ? (decimal)Math.Round(1.5 + random.NextDouble() * 2.3, 1)
                            : roll > 0.40 ? (decimal)Math.Round(5.0 + random.NextDouble() * 2.5, 1)
                            : (decimal)Math.Round(7.5 + random.NextDouble() * 2.5, 1);

                string status = gpa >= 4.0m ? "dat" : "rot";
                string? reason = gpa < 4.0m ? "{\"reason\":\"Không đạt điểm thi kết thúc môn\"}" : null;

                allGrades.Add(new DiemSo
                {
                    MaDonVi = st.MaDonVi,
                    MaHocSinh = st.MaNguoiDung,
                    MaMonHoc = course.MaMonHoc,
                    MaHocKy = termId,
                    DiemQuaTrinh = Math.Min(10m, gpa + 0.5m),
                    DiemGiuaKy = gpa,
                    DiemCuoiKy = gpa,
                    GpaMonHoc = gpa,
                    TrangThai = status,
                    DaKhoa = true,
                    LyDoRot = reason,
                    NamNhapHoc = st.NamNhapHoc ?? 2026
                });
            }
        }

        // Batch save grades
        for (int i = 0; i < allGrades.Count; i += 2000)
        {
            var batch = allGrades.Skip(i).Take(2000).ToList();
            context.DiemSos.AddRange(batch);
            await context.SaveChangesAsync();
        }

        // 8. Teacher Evaluation Questions & Evaluation Scores
        Console.WriteLine("Seeding Teacher Evaluations (DanhGiaGiaoVien)...");
        var evalQuestions = new List<CauHoiDanhGia>
        {
            new() { NoiDungCauHoi = "Phương pháp giảng dạy truyền đạt dễ hiểu và hấp dẫn", ConHoatDong = true },
            new() { NoiDungCauHoi = "Đúng giờ, tuân thủ tiến độ và nội quy lớp học", ConHoatDong = true },
            new() { NoiDungCauHoi = "Tài liệu học tập đầy đủ, chuẩn bị bài chu đáo", ConHoatDong = true },
            new() { NoiDungCauHoi = "Nhiệt tình giải đáp thắc mắc và hỗ trợ sinh viên", ConHoatDong = true },
            new() { NoiDungCauHoi = "Đánh giá cho điểm công bằng, minh bạch", ConHoatDong = true }
        };
        context.CauHoiDanhGias.AddRange(evalQuestions);
        await context.SaveChangesAsync();

        var positiveComments = new[]
        {
            "Giảng viên dạy rất nhiệt tình, truyền đạt dễ hiểu.",
            "Bài giảng sinh động, hỗ trợ sinh viên làm bài tập chu đáo.",
            "Thầy/Cô cho điểm công bằng, minh bạch.",
            "Rất hài lòng với phương pháp giảng dạy."
        };

        var negativeComments = new[]
        {
            "Tốc độ giảng dạy quá nhanh, chưa hỗ trợ sinh viên chậm.",
            "Giảng viên ít giải đáp câu hỏi trên forum.",
            "Cần bổ sung thêm các ví dụ thực tế trong bài giảng.",
            "Giảng dạy khá lý thuyết, thiếu thời gian thực hành."
        };

        var evaluations = new List<DanhGiaGiaoVien>();
        int teacherEvalIdx = 0;

        if (await context.DanhGiaGiaoViens.CountAsync() < 1000)
        {
            foreach (var teacher in allTeachers)
            {
                teacherEvalIdx++;
                // 4 teachers will have low ratings (< 3.5) to trigger the Low Rating Alert
                bool isLowRatingTeacher = teacherEvalIdx <= 4;
                var term = allCampusTerms.FirstOrDefault(t => t.MaDonVi == teacher.MaDonVi) ?? allCampusTerms.First();

                for (int evalCount = 0; evalCount < 15; evalCount++)
                {
                    foreach (var q in evalQuestions)
                    {
                        int score = isLowRatingTeacher
                            ? random.Next(1, 4) // 1, 2, 3 stars
                            : random.Next(3, 6); // 3, 4, 5 stars

                        string comment = isLowRatingTeacher
                            ? negativeComments[random.Next(negativeComments.Length)]
                            : positiveComments[random.Next(positiveComments.Length)];

                        evaluations.Add(new DanhGiaGiaoVien
                        {
                            MaGiaoVien = teacher.MaNguoiDung,
                            MaHocKy = term.MaHocKy,
                            MaCauHoiDg = q.MaCauHoiDg,
                            DiemSo = score,
                            NhanXetTuDo = comment,
                            NgayTao = DateTime.UtcNow.AddDays(-random.Next(1, 90))
                        });
                    }
                }
            }

            for (int i = 0; i < evaluations.Count; i += 2000)
            {
                var batch = evaluations.Skip(i).Take(2000).ToList();
                context.DanhGiaGiaoViens.AddRange(batch);
                await context.SaveChangesAsync();
            }
        }

        // 9. At-Risk Student Reports (BaoCaoRuiRoRotMon)
        Console.WriteLine("Seeding At-Risk Student Reports (BaoCaoRuiRoRotMon)...");
        if (!await context.BaoCaoRuiRoRotMons.AnyAsync())
        {
            var failedGrades = await context.DiemSos.Where(g => g.TrangThai == "rot" || g.GpaMonHoc < 4.0m).Take(300).ToListAsync();
            var riskReports = new List<BaoCaoRuiRoRotMon>();

            foreach (var fg in failedGrades)
            {
                riskReports.Add(new BaoCaoRuiRoRotMon
                {
                    MaHocSinh = fg.MaHocSinh,
                    MaMonHoc = fg.MaMonHoc,
                    MaHocKy = fg.MaHocKy,
                    XacSuatRotMon = (decimal)Math.Round(0.50 + random.NextDouble() * 0.45, 2),
                    DacTrungJson = $"{{\"absences\":{random.Next(3, 7)},\"midterm_gpa\":{fg.GpaMonHoc.ToString(System.Globalization.CultureInfo.InvariantCulture)},\"missing_homework\":{random.Next(1, 4)}}}",
                    TaoLuc = DateTime.UtcNow
                });
            }
            if (riskReports.Any())
            {
                context.BaoCaoRuiRoRotMons.AddRange(riskReports);
                await context.SaveChangesAsync();
            }
        }

        // 10. Timetables (ThoiKhoaBieu)
        Console.WriteLine("Seeding Timetables (ThoiKhoaBieu)...");
        if (await context.ThoiKhoaBieus.CountAsync() < 50)
        {
            var rooms = await context.PhongHocs.ToListAsync();
            var shifts = await context.CaHocs.ToListAsync();
            var sampleCourses = await context.KhoaHocs.Take(100).ToListAsync();

            if (rooms.Any() && shifts.Any() && sampleCourses.Any())
            {
                var tkbs = new List<ThoiKhoaBieu>();
                var statuses = new[] { "nhap", "da_xuat_ban" };

                foreach (var course in sampleCourses)
                {
                    var room = rooms[random.Next(rooms.Count)];
                    var shift = shifts[random.Next(shifts.Count)];
                    var status = statuses[random.Next(statuses.Length)];

                    tkbs.Add(new ThoiKhoaBieu
                    {
                        MaKhoaHoc = course.MaKhoaHoc,
                        MaPhong = room.MaPhong,
                        MaCaHoc = shift.MaCaHoc,
                        ThuTrongTuan = random.Next(2, 8),
                        NgayBatDau = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
                        NgayKetThuc = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(60)),
                        TrangThai = status,
                        NgayTao = DateTime.UtcNow
                    });
                }
                context.ThoiKhoaBieus.AddRange(tkbs);
                await context.SaveChangesAsync();
            }
        }

        Console.WriteLine("Seeding Missing Data for Parents, Invoices, Support Tickets, Attendance...");
        await SeedMissingDataAsync(context, allStudents, allTeachers, random, passwordHash);

        // The D0 term depends on the broad profile's campuses, classes,
        // students, teachers, rooms and credit mappings.  Ensure it only after
        // those prerequisites exist, while the early-return path above still
        // ensures it for an already seeded database.
        await SeedSmartSchedulingDemoTermAsync(context);

        Console.WriteLine("Multi-Campus LargeDemo Seed V11 completed successfully!");
    }

    /// <summary>
    /// Creates one future, unscheduled term for the Smart Timetable acceptance
    /// flow.  It deliberately reuses real campus-14 classes, students, rooms,
    /// teachers and subject capabilities rather than manufacturing a parallel
    /// toy dataset.  The stable code makes the operation idempotent.
    /// </summary>
    private static async Task SeedSmartSchedulingDemoTermAsync(ApplicationDbContext context)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            await SeedSmartSchedulingDemoTermCoreAsync(context);
            await transaction.CommitAsync();
        });
    }

    private static async Task SeedSmartSchedulingDemoTermCoreAsync(ApplicationDbContext context)
    {
        const string targetTermCode = "HK1_2027";
        const int targetCourseCount = 30;
        var now = DateTime.UtcNow;
        var studentRole = AuthRoles.ToDatabaseCode(AuthRoles.Student);
        var teacherRole = AuthRoles.ToDatabaseCode(AuthRoles.Teacher);

        var campus = await context.DonVis.SingleOrDefaultAsync(x =>
            x.CapDonVi == "co_so" && x.ConHoatDong &&
            x.TenDonVi == "FPT Polytechnic Hồ Chí Minh");
        if (campus is null)
        {
            Console.WriteLine("SmartScheduling D0: campus Hồ Chí Minh was not found; skipped.");
            return;
        }

        // C2: Academic Scheduling Context is the source of truth.  LargeDemo
        // deliberately targets the configured nearest-future fixture, never a
        // private far-future term that the real API cannot schedule.
        var term = await context.HocKys.SingleOrDefaultAsync(x =>
            x.MaDonVi == campus.MaDonVi && x.MaCodeHocKy == targetTermCode);

        if (term is null)
        {
            term = new HocKy
            {
                MaDonVi = campus.MaDonVi,
                MaCodeHocKy = targetTermCode,
                TenHocKy = "Học kỳ Demo Smart Scheduling 2029 (Hồ Chí Minh)",
                NamHoc = "2029",
                ThuTuTrongNam = 1,
                NgayBatDau = new DateOnly(2029, 1, 1),
                NgayKetThuc = new DateOnly(2029, 4, 30),
                DaKhoa = false
            };
            context.HocKys.Add(term);
            await context.SaveChangesAsync();
        }
        else
        {
            // Keep the academic term's existing code/name/dates intact.
        }

        // HK1_2027's legacy fixture has cancelled schedules but no sections or
        // enrolments. Preserve it for audit while excluding it from the live
        // scheduler, so the D0 target remains the intended 30 complete courses.
        var activeTargetCourses = await context.KhoaHocs
            .Where(x => x.MaHocKy == term.MaHocKy && x.TrangThai != "luu_tru")
            .ToListAsync();
        if (activeTargetCourses.Count > targetCourseCount
            && !await context.LopHocPhans.AnyAsync(x => x.MaHocKy == term.MaHocKy)
            && !await context.DangKyHocPhans.AnyAsync(x => x.LopHocPhan != null && x.LopHocPhan.MaHocKy == term.MaHocKy))
        {
            foreach (var course in activeTargetCourses)
            {
                course.TrangThai = "luu_tru";
            }
            await context.SaveChangesAsync();
        }

        var existingBlocks = await context.Blocks
            .Where(x => x.MaHocKy == term.MaHocKy)
            .OrderBy(x => x.ThuTuBlock)
            .ToListAsync();
        if (existingBlocks.Count == 0)
        {
            const int blockDays = 24;
            for (var order = 1; order <= 5; order++)
            {
                var start = term.NgayBatDau.AddDays((order - 1) * blockDays);
                context.Blocks.Add(new Block
                {
                    MaHocKy = term.MaHocKy,
                    ThuTuBlock = order,
                    TenBlock = $"Block {order}",
                    NgayBatDau = start,
                    NgayKetThuc = order == 5 ? term.NgayKetThuc : start.AddDays(blockDays - 1)
                });
            }
            await context.SaveChangesAsync();
            existingBlocks = await context.Blocks.Where(x => x.MaHocKy == term.MaHocKy).OrderBy(x => x.ThuTuBlock).ToListAsync();
        }

        // A clean LargeDemo database has no room bootstrap outside this seed.
        // Ensure a small, explicit campus-14 inventory before measuring capacity
        // so the same first startup can produce a schedulable demo term.
        if (!await context.PhongHocs.AnyAsync(x => x.MaDonVi == campus.MaDonVi && x.TrangThaiPhong == "hoat_dong"))
        {
            for (var number = 1; number <= 10; number++)
            {
                context.PhongHocs.Add(new PhongHoc
                {
                    MaDonVi = campus.MaDonVi,
                    MaCodePhong = $"LDM-SMART-{number:00}",
                    TenPhong = $"Phòng Smart Demo {number:00}",
                    SucChua = 50,
                    LoaiPhong = number <= 6 ? "ly_thuyet" : "lab",
                    TrangThaiPhong = "hoat_dong",
                    GhiChu = "LargeDemo Smart Scheduling bootstrap"
                });
            }
            await context.SaveChangesAsync();
        }

        var largestRoomCapacity = await context.PhongHocs
            .Where(x => x.MaDonVi == campus.MaDonVi && x.TrangThaiPhong == "hoat_dong")
            .Select(x => (int?)x.SucChua)
            .MaxAsync() ?? 0;
        if (largestRoomCapacity <= 0)
        {
            Console.WriteLine("SmartScheduling D0: no active room capacity for campus; skipped.");
            return;
        }

        var classes = await context.LopHanhChinhs
            .Where(x => x.MaDonVi == campus.MaDonVi && x.ConHoatDong)
            .Select(x => new
            {
                Class = x,
                ActiveStudents = context.NguoiDungs.Count(u =>
                    u.MaLop == x.MaLop && u.VaiTroChinh == studentRole && u.TrangThai == UserStatuses.DbActive)
            })
            .Where(x => x.ActiveStudents > 0 && x.ActiveStudents <= largestRoomCapacity)
            .OrderByDescending(x => x.ActiveStudents)
            .ThenBy(x => x.Class.MaCodeLop)
            .Take(targetCourseCount)
            .ToListAsync();
        if (classes.Count < 25)
        {
            Console.WriteLine($"SmartScheduling D0: only {classes.Count} capacity-safe classes found; skipped.");
            return;
        }

        var capabilities = await (
            from capability in context.GiaoVienMonHocs
            join teacher in context.NguoiDungs on capability.MaGiaoVien equals teacher.MaNguoiDung
            join subject in context.DanhMucMonHocs on capability.MaMonHoc equals subject.MaMonHoc
            where teacher.MaDonVi == campus.MaDonVi && teacher.VaiTroChinh == teacherRole &&
                  teacher.TrangThai == UserStatuses.DbActive && capability.ConHoatDong &&
                  capability.MucDoPhuHop >= 70 && capability.PhuHopChuyenMon != false && subject.ConHoatDong
            select new { capability.MaMonHoc, capability.MaGiaoVien, capability.MucDoPhuHop, subject.SoTinChi }
        ).ToListAsync();
        var selectedSubjectIds = capabilities
            .GroupBy(x => x.MaMonHoc)
            .Where(x => x.Select(v => v.MaGiaoVien).Distinct().Count() >= 2)
            .OrderBy(x => x.Key)
            .Take(6)
            .Select(x => x.Key)
            .ToList();
        if (selectedSubjectIds.Count < 3)
        {
            Console.WriteLine("SmartScheduling D0: fewer than three subjects have two qualified teachers; skipped.");
            return;
        }

        // Thirty three-credit courses require 90 weekly sessions.  The original
        // campus fixture has only nine active teachers, so it cannot satisfy the
        // solver's real six-session weekly cap.  Add six named LargeDemo faculty
        // members and certify them only for the selected demo subjects.  This is
        // idempotent and preserves actual credit/session policy instead of
        // weakening the solver cap.
        var demoTeacherPasswordHash = await context.NguoiDungs
            .Where(x => x.MaDonVi == campus.MaDonVi && x.VaiTroChinh == teacherRole && x.MatKhauHash != null)
            .Select(x => x.MatKhauHash)
            .FirstOrDefaultAsync() ?? string.Empty;
        var demoTeachers = new List<NguoiDung>();
        for (var number = 1; number <= 6; number++)
        {
            var email = $"largedemo.smart.gv{number:00}@lms.local";
            var teacher = await context.NguoiDungs.SingleOrDefaultAsync(x => x.Email == email);
            if (teacher is null)
            {
                teacher = new NguoiDung
                {
                    MaDonVi = campus.MaDonVi,
                    Email = email,
                    HoTen = $"Giảng viên Smart Demo {number:00}",
                    VaiTroChinh = teacherRole,
                    TrangThai = UserStatuses.DbActive,
                    MatKhauHash = demoTeacherPasswordHash,
                    DangNhapLanDau = false,
                    NgayTao = now
                };
                context.NguoiDungs.Add(teacher);
                await context.SaveChangesAsync();
            }
            demoTeachers.Add(teacher);
        }

        var demoTeacherIds = demoTeachers.Select(x => x.MaNguoiDung).ToList();
        var existingDemoCapabilities = await context.GiaoVienMonHocs
            .Where(x => demoTeacherIds.Contains(x.MaGiaoVien) && selectedSubjectIds.Contains(x.MaMonHoc))
            .Select(x => new { x.MaGiaoVien, x.MaMonHoc })
            .ToListAsync();
        var existingDemoCapabilityKeys = existingDemoCapabilities
            .Select(x => (x.MaGiaoVien, x.MaMonHoc))
            .ToHashSet();
        foreach (var teacher in demoTeachers)
        foreach (var subjectId in selectedSubjectIds)
        {
            if (existingDemoCapabilityKeys.Contains((teacher.MaNguoiDung, subjectId))) continue;
            context.GiaoVienMonHocs.Add(new GiaoVienMonHoc
            {
                MaGiaoVien = teacher.MaNguoiDung,
                MaMonHoc = subjectId,
                MucDoPhuHop = 80,
                PhuHopChuyenMon = true,
                DiemDanhGia = 80,
                SoNamKinhNghiem = 3,
                ConHoatDong = true,
                NgayTao = now
            });
        }
        await context.SaveChangesAsync();

        capabilities = await (
            from capability in context.GiaoVienMonHocs
            join teacher in context.NguoiDungs on capability.MaGiaoVien equals teacher.MaNguoiDung
            join subject in context.DanhMucMonHocs on capability.MaMonHoc equals subject.MaMonHoc
            where teacher.MaDonVi == campus.MaDonVi && teacher.VaiTroChinh == teacherRole &&
                  teacher.TrangThai == UserStatuses.DbActive && capability.ConHoatDong &&
                  capability.MucDoPhuHop >= 70 && capability.PhuHopChuyenMon != false && subject.ConHoatDong
            select new { capability.MaMonHoc, capability.MaGiaoVien, capability.MucDoPhuHop, subject.SoTinChi }
        ).ToListAsync();
        var subjectTeachers = capabilities
            .Where(x => selectedSubjectIds.Contains(x.MaMonHoc))
            .GroupBy(x => x.MaMonHoc)
            .ToDictionary(
                x => x.Key,
                x => x.OrderByDescending(v => v.MucDoPhuHop).ThenBy(v => v.MaGiaoVien).ToList());

        var mappings = await context.QuyDoiTinChis.ToDictionaryAsync(x => x.SoTinChi, x => x);
        var missingCreditMappings = subjectTeachers.Values
            .SelectMany(x => x)
            .Select(x => x.SoTinChi)
            .Where(x => x > 0 && !mappings.ContainsKey(x))
            .Distinct()
            .ToList();
        if (missingCreditMappings.Count > 0)
        {
            Console.WriteLine($"SmartScheduling D0: DATA_POLICY_MISSING for credit mappings: {string.Join(',', missingCreditMappings)}.");
            return;
        }

        var existingCourses = await context.KhoaHocs
            .Where(x => x.MaHocKy == term.MaHocKy)
            .Select(x => new { x.MaKhoaHoc, x.MaMonHoc, x.MaLop, x.TrangThai })
            .ToListAsync();
        var existingCourseByKey = existingCourses.ToDictionary(x => (x.MaMonHoc, x.MaLop));
        var subjectSets = subjectTeachers.OrderBy(x => x.Key).ToList();
        var firstBlock = existingBlocks.First(x => x.ThuTuBlock == 1);
        var addedCourses = new List<KhoaHoc>();
        var teacherWeeklyLoads = new Dictionary<int, int>();
        for (var index = 0; index < classes.Count; index++)
        {
            var subjectSet = subjectSets[index % subjectSets.Count];
            var selectedTeacher = subjectSet.Value
                .OrderBy(x => teacherWeeklyLoads.GetValueOrDefault(x.MaGiaoVien))
                .ThenByDescending(x => x.MucDoPhuHop)
                .ThenBy(x => x.MaGiaoVien)
                .First();
            var conversion = mappings[selectedTeacher.SoTinChi];
            teacherWeeklyLoads[selectedTeacher.MaGiaoVien] =
                teacherWeeklyLoads.GetValueOrDefault(selectedTeacher.MaGiaoVien) + conversion.SoBuoiMoiTuan;
            var classInfo = classes[index];
            if (existingCourseByKey.TryGetValue((subjectSet.Key, classInfo.Class.MaLop), out var existingCourse))
            {
                // Reuse the existing business key rather than violating the
                // unique (campus, subject, term, class) constraint.
                var course = await context.KhoaHocs.SingleAsync(x => x.MaKhoaHoc == existingCourse.MaKhoaHoc);
                course.TrangThai = "nhap";
                course.MaGiaoVien = selectedTeacher.MaGiaoVien;
                course.MaBlockBatDau = firstBlock.MaBlock;
                course.SoBlockHoc = conversion.SoBlockHoc;
                continue;
            }
            addedCourses.Add(new KhoaHoc
            {
                MaDonVi = campus.MaDonVi,
                MaHocKy = term.MaHocKy,
                MaBlockBatDau = firstBlock.MaBlock,
                SoBlockHoc = conversion.SoBlockHoc,
                MaMonHoc = subjectSet.Key,
                MaGiaoVien = selectedTeacher.MaGiaoVien,
                MaLop = classInfo.Class.MaLop,
                TieuDe = $"Smart Demo 2029 - {classInfo.Class.MaCodeLop}",
                TrangThai = "nhap",
                NgayTao = now
            });
        }
        if (addedCourses.Count > 0)
        {
            context.KhoaHocs.AddRange(addedCourses);
            await context.SaveChangesAsync();
        }

        var termCourses = await context.KhoaHocs
            .Where(x => x.MaHocKy == term.MaHocKy && x.MaDonVi == campus.MaDonVi && x.TrangThai != "luu_tru")
            .OrderBy(x => x.MaKhoaHoc)
            .ToListAsync();
        var sectionByCourse = new Dictionary<int, LopHocPhan>();
        foreach (var course in termCourses)
        {
            var code = $"LHP-SMART-2029-{course.MaKhoaHoc:D4}";
            var section = await context.LopHocPhans.SingleOrDefaultAsync(x => x.MaCodeLopHocPhan == code);
            if (section is null)
            {
                section = new LopHocPhan
                {
                    MaCodeLopHocPhan = code,
                    MaDonVi = campus.MaDonVi,
                    MaMonHoc = course.MaMonHoc,
                    MaHocKy = term.MaHocKy,
                    SucChua = largestRoomCapacity,
                    SoDangKyToiThieu = 1,
                    SoDaDangKy = 0,
                    TrangThai = "mo",
                    QuotaVangToiDa = 0
                };
                context.LopHocPhans.Add(section);
                await context.SaveChangesAsync();
            }
            course.MaLopHocPhan = section.MaLopHocPhan;
            sectionByCourse[course.MaKhoaHoc] = section;
        }
        await context.SaveChangesAsync();

        var courseClasses = termCourses.ToDictionary(x => x.MaKhoaHoc, x => x.MaLop);
        var termClassIds = courseClasses.Values.Distinct().ToList();
        var students = await context.NguoiDungs
            .Where(x => x.MaLop != null && termClassIds.Contains(x.MaLop.Value) &&
                        x.VaiTroChinh == studentRole && x.TrangThai == UserStatuses.DbActive)
            .Select(x => new { x.MaNguoiDung, ClassId = x.MaLop!.Value })
            .ToListAsync();
        var existingEnrollments = await context.DangKyHocPhans
            .Where(x => sectionByCourse.Values.Select(s => s.MaLopHocPhan).Contains(x.MaLopHocPhan))
            .Select(x => new { x.MaHocSinh, x.MaLopHocPhan })
            .ToListAsync();
        var enrollmentKeys = existingEnrollments.Select(x => (x.MaHocSinh, x.MaLopHocPhan)).ToHashSet();
        var enrollments = new List<DangKyHocPhan>();
        foreach (var course in termCourses)
        {
            var section = sectionByCourse[course.MaKhoaHoc];
            var courseStudents = students.Where(x => x.ClassId == course.MaLop).ToList();
            foreach (var student in courseStudents)
            {
                if (!enrollmentKeys.Add((student.MaNguoiDung, section.MaLopHocPhan))) continue;
                enrollments.Add(new DangKyHocPhan
                {
                    MaHocSinh = student.MaNguoiDung,
                    MaLopHocPhan = section.MaLopHocPhan,
                    TrangThai = "da_dang_ky",
                    LaHocLai = false,
                    KiemTraTienQuyet = false,
                    DaKiemTraTienQuyet = true,
                    NgayTao = now
                });
            }
            section.SoDaDangKy = courseStudents.Count;
        }
        if (enrollments.Count > 0)
        {
            context.DangKyHocPhans.AddRange(enrollments);
            await context.SaveChangesAsync();
        }

        var assignedTeacherIds = termCourses.Select(x => x.MaGiaoVien).Distinct().ToList();
        var activeShifts = await context.CaHocs.Where(x => x.ConHoatDong).OrderBy(x => x.ThuTu).ToListAsync();
        foreach (var teacherId in assignedTeacherIds)
        {
            var preference = await context.GiaoVienNguyenVongHocKys.SingleOrDefaultAsync(x =>
                x.MaGiaoVien == teacherId && x.MaHocKy == term.MaHocKy);
            if (preference is null)
            {
                preference = new GiaoVienNguyenVongHocKy
                {
                    MaGiaoVien = teacherId,
                    MaHocKy = term.MaHocKy,
                    MaDonVi = campus.MaDonVi,
                    SoLopToiDaMongMuon = 4,
                    SoCaToiDaMoiTuan = 8,
                    TrangThai = "submitted",
                    NgayTao = now,
                    NgayGui = now,
                    GhiChu = "Dữ liệu D0 cho học kỳ demo Smart Scheduling"
                };
                context.GiaoVienNguyenVongHocKys.Add(preference);
                await context.SaveChangesAsync();
            }
            if (activeShifts.Count > 0 && !await context.GiaoVienNguyenVongCaDays.AnyAsync(x => x.NguyenVongId == preference.Id))
            {
                context.GiaoVienNguyenVongCaDays.Add(new GiaoVienNguyenVongCaDay
                {
                    NguyenVongId = preference.Id,
                    ThuTrongTuan = 2 + (teacherId % 5),
                    MaCaHoc = activeShifts[teacherId % activeShifts.Count].MaCaHoc,
                    MucDo = "preferred",
                    NgayTao = now
                });
            }
        }
        await context.SaveChangesAsync();
        Console.WriteLine($"SmartScheduling D0: {targetTermCode} is ready with {termCourses.Count} unscheduled courses.");
    }

    private static async Task RemoveD0TermAsync(ApplicationDbContext context, HocKy term)
    {
        // Only a term bearing the D0 code reaches this method. Preserve it as a
        // legacy fixture instead of deleting: integrations such as tuition
        // configuration can legally reference an otherwise unscheduled term.
        // This keeps referential integrity intact and removes it from the live
        // D0 lookup without touching its dependent records.
        var hasSchedule = await context.ThoiKhoaBieus.AnyAsync(x => x.KhoaHoc != null && x.KhoaHoc.MaHocKy == term.MaHocKy)
            || await context.ScheduleGenerationJobs.AnyAsync(x => x.MaHocKy == term.MaHocKy);
        if (hasSchedule)
        {
            Console.WriteLine("SmartScheduling D0: legacy target has schedule data; preserving it.");
            return;
        }
        term.MaCodeHocKy = $"HK_D0_LEGACY_{term.MaHocKy}";
        term.TenHocKy = "Học kỳ Demo Smart Scheduling cũ (không dùng để Generate)";
        await context.SaveChangesAsync();
    }

    private static async Task SeedMissingDataAsync(ApplicationDbContext context, List<NguoiDung> allStudents, List<NguoiDung> allTeachers, Random random, string passwordHash)
    {
        var now = DateTime.UtcNow;

        // 1. Phụ huynh & Liên kết phụ huynh
        var parentRole = AuthRoles.ToDatabaseCode(AuthRoles.Parent);
        var existingParentsCount = await context.NguoiDungs.CountAsync(u => u.VaiTroChinh == parentRole && u.Email.Contains("parent.v11"));
        if (existingParentsCount == 0 && allStudents.Any())
        {
            var parents = new List<NguoiDung>();
            for (int i = 1; i <= 500; i++)
            {
                parents.Add(new NguoiDung
                {
                    Email = $"parent.v11.{i:D4}@edulms.local",
                    HoTen = $"Phụ Huynh {i:D4}",
                    VaiTroChinh = parentRole,
                    MaDonVi = allStudents.First().MaDonVi,
                    TrangThai = UserStatuses.DbActive,
                    MatKhauHash = passwordHash,
                    NgayTao = now
                });
            }
            context.NguoiDungs.AddRange(parents);
            await context.SaveChangesAsync();

            var links = new List<LienKetPhuHuynh>();
            int studentIdx = 0;
            foreach (var p in parents)
            {
                int childrenCount = random.Next(1, 3);
                for (int c = 0; c < childrenCount; c++)
                {
                    if (studentIdx >= allStudents.Count) break;
                    links.Add(new LienKetPhuHuynh
                    {
                        MaPhuHuynh = p.MaNguoiDung,
                        MaHocSinh = allStudents[studentIdx].MaNguoiDung,
                        QuyenXem = "[\"xem_diem\", \"xem_hoc_phi\"]",
                        TrangThai = "hoat_dong",
                        LienKetLuc = now
                    });
                    studentIdx++;
                }
            }
            context.LienKetPhuHuynhs.AddRange(links);
            await context.SaveChangesAsync();
        }

        // 2. Hóa đơn (Invoices)
        if (await context.HoaDons.CountAsync() == 0 && allStudents.Any())
        {
            var invoices = new List<HoaDon>();
            var terms = await context.HocKys.ToListAsync();
            var activeTerm = terms.FirstOrDefault(t => t.NgayBatDau <= DateOnly.FromDateTime(now) && t.NgayKetThuc >= DateOnly.FromDateTime(now)) ?? terms.First();

            foreach (var student in allStudents)
            {
                bool isPaid = random.NextDouble() > 0.4;
                decimal amount = 8500000m;
                invoices.Add(new HoaDon
                {
                    MaDonVi = student.MaDonVi,
                    MaHocSinh = student.MaNguoiDung,
                    MaHocKy = activeTerm.MaHocKy,
                    MaHoaDonCode = $"INV-{now.Year}-{student.MaNguoiDung}-{random.Next(1000, 9999)}",
                    LoaiHoaDon = "hoc_phi",
                    SoTien = amount,
                    GiamTru = 0,
                    DaThanhToan = isPaid ? amount : 0,
                    TrangThai = isPaid ? "da_thanh_toan" : "chua_thanh_toan",
                    HanThanhToan = DateOnly.FromDateTime(now.AddDays(15)),
                    GhiChu = "Học phí kỳ " + activeTerm.TenHocKy,
                    NgayTao = now.AddDays(-random.Next(1, 30))
                });
            }

            for (int i = 0; i < invoices.Count; i += 2000)
            {
                var batch = invoices.Skip(i).Take(2000).ToList();
                context.HoaDons.AddRange(batch);
                await context.SaveChangesAsync();
            }
        }

        // 3. Phiếu hỗ trợ (Support Tickets)
        if (await context.PhieuHoTros.CountAsync() == 0 && allStudents.Any())
        {
            var tickets = new List<PhieuHoTro>();
            var categories = new[] { "hoc_vu", "tai_chinh", "ky_thuat", "khac" };
            var statuses = new[] { "mo", "dang_xu_ly", "da_dong" };
            
            for (int i = 0; i < 200; i++)
            {
                var st = allStudents[random.Next(allStudents.Count)];
                tickets.Add(new PhieuHoTro
                {
                    MaHocSinh = st.MaNguoiDung,
                    DanhMuc = categories[random.Next(categories.Length)],
                    TieuDe = "Yêu cầu hỗ trợ sinh viên " + st.HoTen,
                    MoTa = "Mô tả chi tiết vấn đề của sinh viên cần được giáo vụ giải quyết...",
                    TrangThai = statuses[random.Next(statuses.Length)],
                    NgayTao = now.AddDays(-random.Next(1, 30)),
                    DoUuTien = random.NextDouble() > 0.8 ? "cao" : "binh_thuong"
                });
            }
            context.PhieuHoTros.AddRange(tickets);
            await context.SaveChangesAsync();
        }

        // 4. Buổi học & Điểm danh (Lessons & Attendance)
        if (await context.BuoiHocs.CountAsync() == 0)
        {
            var tkbs = await context.ThoiKhoaBieus.Include(t => t.KhoaHoc).Take(200).ToListAsync();
            var buoiHocs = new List<BuoiHoc>();
            var diemDanhs = new List<DiemDanh>();

            foreach (var tkb in tkbs)
            {
                if (tkb.KhoaHoc == null) continue;

                for (int l = 0; l < 2; l++)
                {
                    var bh = new BuoiHoc
                    {
                        MaTkb = tkb.MaTkb,
                        MaKhoaHoc = tkb.MaKhoaHoc,
                        NgayHoc = DateOnly.FromDateTime(now.AddDays(-random.Next(1, 14))),
                        MaCaHoc = tkb.MaCaHoc,
                        MaPhong = tkb.MaPhong,
                        MaGiaoVien = tkb.KhoaHoc.MaGiaoVien,
                        TrangThaiBuoi = "da_day",
                        TrangThaiDiemDanh = "da_khoa",
                        KhoaLuc = now,
                        NgayTao = now.AddDays(-20)
                    };
                    buoiHocs.Add(bh);
                }
            }

            if (buoiHocs.Any())
            {
                context.BuoiHocs.AddRange(buoiHocs);
                await context.SaveChangesAsync();

                var allBuoiHocs = await context.BuoiHocs.Include(b => b.KhoaHoc).ToListAsync();
                foreach (var bh in allBuoiHocs)
                {
                    if (bh.KhoaHoc == null) continue;
                    
                    var courseStudents = allStudents.Where(s => s.MaLop == bh.KhoaHoc.MaLop).Take(30).ToList();
                    
                    foreach (var st in courseStudents)
                    {
                        double roll = random.NextDouble();
                        string status = roll > 0.9 ? "vang_mat" : (roll > 0.8 ? "di_muon" : "co_mat");

                        diemDanhs.Add(new DiemDanh
                        {
                            MaDonVi = bh.KhoaHoc.MaDonVi,
                            MaBuoiHoc = bh.MaBuoiHoc,
                            MaHocSinh = st.MaNguoiDung,
                            TrangThai = status,
                            NguoiGhiNhan = bh.MaGiaoVien,
                            GhiNhanLuc = bh.NgayHoc.ToDateTime(TimeOnly.MinValue).AddHours(8),
                            HeSoVang = status == "vang_mat" ? 1 : 0
                        });
                    }
                }
                
                for (int i = 0; i < diemDanhs.Count; i += 2000)
                {
                    var batch = diemDanhs.Skip(i).Take(2000).ToList();
                    context.DiemDanhs.AddRange(batch);
                    await context.SaveChangesAsync();
                }
            }
        }
        
        // 5. Nhật ký hệ thống (Audit Logs)
        if (await context.NhatKyKiemToans.CountAsync() == 0 && allTeachers.Any())
        {
            var logs = new List<NhatKyKiemToan>();
            for(int i = 0; i < 300; i++)
            {
                var teacher = allTeachers[random.Next(allTeachers.Count)];
                logs.Add(new NhatKyKiemToan
                {
                    NguoiThayDoi = teacher.MaNguoiDung,
                    HanhDong = random.NextDouble() > 0.5 ? "UPDATE_GRADES" : "LOGIN_SUCCESS",
                    MoTa = "Hệ thống ghi nhận thao tác của người dùng",
                    LoaiDoiTuong = "GiaoVien",
                    MaDoiTuong = teacher.MaNguoiDung.ToString(),
                    DiaChiIp = "10.0.0.1",
                    ThoiDiemThayDoi = now.AddHours(-random.Next(1, 100))
                });
            }
            context.NhatKyKiemToans.AddRange(logs);
            await context.SaveChangesAsync();
        }

        // 6. Dữ liệu đặc tả cho các tài khoản test (p12test_teacher01, staff, p15test_parent01)
        var p12Teacher = allTeachers.FirstOrDefault(t => t.Email == "p12test_teacher01@lms.local");
        if (p12Teacher != null)
        {
            var teacherCourses = await context.KhoaHocs.Where(k => k.MaDonVi == p12Teacher.MaDonVi && k.MaGiaoVien != p12Teacher.MaNguoiDung).Take(5).ToListAsync();
            foreach (var c in teacherCourses) { c.MaGiaoVien = p12Teacher.MaNguoiDung; }
            await context.SaveChangesAsync();
        }

        var staff = await context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == "staff@edulms.local");
        if (staff != null && await context.DonTus.CountAsync() == 0)
        {
            var dontus = new List<DonTu>();
            for (int i = 0; i < 30; i++)
            {
                var st = allStudents[random.Next(allStudents.Count)];
                dontus.Add(new DonTu
                {
                    MaDonVi = staff.MaDonVi,
                    MaHocSinh = st.MaNguoiDung,
                    LoaiDon = "chuyen_nganh",
                    TieuDe = "Xin chuyển ngành học",
                    TrangThai = random.NextDouble() > 0.5 ? "cho_xu_ly" : "da_duyet",
                    TrangThaiXuLyNghiepVu = "dang_cho_duyet",
                    DuLieuBieuMau = "{}",
                    NgayTao = now.AddDays(-random.Next(1, 10))
                });
            }
            context.DonTus.AddRange(dontus);
            await context.SaveChangesAsync();
        }

        var testParent = await context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == "p15test_parent01@lms.local");
        if (testParent != null)
        {
            var testSt1 = allStudents.FirstOrDefault(s => s.Email == "student.cntt01@lms.local");
            var testSt2 = allStudents.FirstOrDefault(s => s.Email == "p12test_student011@lms.local");

            if (testSt1 != null && !await context.LienKetPhuHuynhs.AnyAsync(l => l.MaPhuHuynh == testParent.MaNguoiDung && l.MaHocSinh == testSt1.MaNguoiDung))
            {
                context.LienKetPhuHuynhs.Add(new LienKetPhuHuynh { MaPhuHuynh = testParent.MaNguoiDung, MaHocSinh = testSt1.MaNguoiDung, QuyenXem = "[\"xem_diem\", \"xem_hoc_phi\"]", TrangThai = "hoat_dong", LienKetLuc = now });
            }
            if (testSt2 != null && !await context.LienKetPhuHuynhs.AnyAsync(l => l.MaPhuHuynh == testParent.MaNguoiDung && l.MaHocSinh == testSt2.MaNguoiDung))
            {
                context.LienKetPhuHuynhs.Add(new LienKetPhuHuynh { MaPhuHuynh = testParent.MaNguoiDung, MaHocSinh = testSt2.MaNguoiDung, QuyenXem = "[\"xem_diem\", \"xem_hoc_phi\"]", TrangThai = "hoat_dong", LienKetLuc = now });
            }
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedRegistrationWorkflowAsync(
        ApplicationDbContext context,
        DonVi campus,
        IReadOnlyList<DanhMucMonHoc> subjects,
        HocKy activeTerm,
        HocKy prepTerm,
        IReadOnlyList<NguoiDung> students,
        IReadOnlyList<KhoaHoc> courses)
    {
        var now = DateTime.UtcNow;
        var studentRole = AuthRoles.ToDatabaseCode(AuthRoles.Student);

        // ── 1. Registration periods (idempotent) ──
        var periodKeys = (await context.GiaiDoanDangKys
            .Where(p => p.MaDonVi == campus.MaDonVi)
            .Select(p => new { p.MaHocKy, p.TrangThai })
            .ToListAsync())
            .ToHashSet();

        if (!periodKeys.Contains(new { activeTerm.MaHocKy, TrangThai = "dang_mo" }))
        {
            context.GiaiDoanDangKys.Add(new GiaiDoanDangKy
            {
                MaDonVi = campus.MaDonVi,
                MaHocKy = activeTerm.MaHocKy,
                BatDauLuc = now.AddDays(-7),
                KetThucLuc = now.AddDays(30),
                TrangThai = "dang_mo",
                SoTinChiToiDa = 24,
            });
        }
        if (!periodKeys.Contains(new { prepTerm.MaHocKy, TrangThai = "nhap" }))
        {
            context.GiaiDoanDangKys.Add(new GiaiDoanDangKy
            {
                MaDonVi = campus.MaDonVi,
                MaHocKy = prepTerm.MaHocKy,
                BatDauLuc = prepTerm.NgayBatDau.ToDateTime(TimeOnly.MinValue).AddDays(-20),
                KetThucLuc = prepTerm.NgayBatDau.ToDateTime(TimeOnly.MinValue).AddDays(-5),
                TrangThai = "nhap",
                SoTinChiToiDa = 24,
            });
        }
        await context.SaveChangesAsync();

        // ── 2. Course sections for all large-demo courses ──
        var sectionByCode = new Dictionary<string, LopHocPhan>(StringComparer.OrdinalIgnoreCase);
        foreach (var course in courses)
        {
            var subject = course.MaMonHoc != 0
                ? await context.DanhMucMonHocs.FirstOrDefaultAsync(x => x.MaMonHoc == course.MaMonHoc)
                : null;
            var code = $"LHP-{subject?.MaCodeMonHoc ?? course.MaMonHoc.ToString()}-{activeTerm.MaCodeHocKy}-{course.MaKhoaHoc}";
            if (sectionByCode.ContainsKey(code))
            {
                continue;
            }

            var section = await context.LopHocPhans.FirstOrDefaultAsync(x => x.MaCodeLopHocPhan == code);
            if (section is null)
            {
                section = new LopHocPhan { MaCodeLopHocPhan = code };
                context.LopHocPhans.Add(section);
            }

            section.MaDonVi = campus.MaDonVi;
            section.MaMonHoc = course.MaMonHoc;
            section.MaHocKy = course.MaHocKy ?? 0;
            section.SucChua = 40;
            section.SoDangKyToiThieu = 15;
            section.SoDaDangKy = 0;
            section.TrangThai = "mo";
            section.QuotaVangToiDa = 10;
            sectionByCode[code] = section;
        }

        if (sectionByCode.Count > 0)
        {
            await context.SaveChangesAsync();
            foreach (var course in courses)
            {
                var subject = course.MaMonHoc != 0
                    ? await context.DanhMucMonHocs.FirstOrDefaultAsync(x => x.MaMonHoc == course.MaMonHoc)
                    : null;
                var code = $"LHP-{subject?.MaCodeMonHoc ?? course.MaMonHoc.ToString()}-{activeTerm.MaCodeHocKy}-{course.MaKhoaHoc}";
                if (sectionByCode.TryGetValue(code, out var section) && section.MaLopHocPhan > 0)
                {
                    course.MaLopHocPhan = section.MaLopHocPhan;
                }
            }
            await context.SaveChangesAsync();
        }

        // ── 3. Sample enrollments: enroll a subset of students into their own-class courses ──
        var studentSamples = students.Where(x => x.VaiTroChinh == studentRole).ToList();
        var courseByClass = courses
            .GroupBy(c => c.MaLop)
            .ToDictionary(g => g.Key, g => g.OrderBy(c => c.MaKhoaHoc).ToList());

        foreach (var student in studentSamples)
        {
            if (student.MaLop is null)
            {
                continue;
            }
            if (!courseByClass.TryGetValue(student.MaLop.Value, out var classCourses))
            {
                continue;
            }

            var picks = classCourses.Take(3).ToList();
            foreach (var course in picks)
            {
                if (course.MaLopHocPhan is null)
                {
                    continue;
                }

                var exists = await context.DangKyHocPhans.AnyAsync(r =>
                    r.MaHocSinh == student.MaNguoiDung && r.MaLopHocPhan == course.MaLopHocPhan.Value);
                if (exists)
                {
                    continue;
                }

                context.DangKyHocPhans.Add(new DangKyHocPhan
                {
                    MaHocSinh = student.MaNguoiDung,
                    MaLopHocPhan = course.MaLopHocPhan.Value,
                    TrangThai = "da_dang_ky",
                    LaHocLai = false,
                    KiemTraTienQuyet = false,
                    DaKiemTraTienQuyet = true,
                    NgayTao = now.AddDays(-3),
                });
            }
        }

        await context.SaveChangesAsync();

        await Data.SeedDeKiemTraForAllSubjectsAsync(context);
    }
}
