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
        if (!allTeachers.Any()) allTeachers = await context.NguoiDungs.Where(u => u.VaiTroChinh == teacherRoleStr).ToListAsync();
        if (!allClasses.Any()) allClasses = await context.LopHanhChinhs.ToListAsync();
        if (!allStudents.Any()) allStudents = await context.NguoiDungs.Where(u => u.VaiTroChinh == studentRoleStr).ToListAsync();

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
            var campusTerms = allCampusTerms.Where(t => t.MaDonVi == campus.MaDonVi).ToList();
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

        Console.WriteLine("Multi-Campus LargeDemo Seed V11 completed successfully!");
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
        var studentSamples = students.Where(x => x.VaiTroChinh == studentRole).Take(320).ToList();
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
