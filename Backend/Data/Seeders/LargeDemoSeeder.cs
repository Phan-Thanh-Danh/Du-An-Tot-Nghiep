using Backend.Constants;
using Backend.Helpers;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Seeders;

public static class LargeDemoSeeder
{
    private const string LargeDemoCampus = "Cơ sở Large Demo V9";
    private const string Password = "password123";
    private const string StudentRole = "student";
    private const string TeacherRole = "teacher";
    private const string AdminRole = "admin";

    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Kiểm tra idempotency
        var exists = await context.DonVis.AnyAsync(x => x.TenDonVi == LargeDemoCampus);
        if (exists)
        {
            Console.WriteLine("LargeDemo data already exists. Skipping.");
            return;
        }

        Console.WriteLine("Starting LargeDemo Seed...");

        // 1. Tạo Root Campus
        var rootCampus = await context.DonVis.FirstOrDefaultAsync(x => x.CapDonVi == "root")
            ?? new DonVi { TenDonVi = "LMS Root", CapDonVi = "root", ConHoatDong = true, NgayTao = DateTime.UtcNow };
        
        if (rootCampus.MaDonVi == 0)
        {
            context.DonVis.Add(rootCampus);
            await context.SaveChangesAsync();
        }

        // 2. Tạo Campuses
        var campuses = new List<DonVi>();
        for (int i = 1; i <= 3; i++)
        {
            var campusName = i == 1 ? LargeDemoCampus : $"Cơ sở Large Demo V9 {i}";
            var c = new DonVi { TenDonVi = campusName, CapDonVi = "co_so", MaDonViCha = rootCampus.MaDonVi, ConHoatDong = true, NgayTao = DateTime.UtcNow };
            campuses.Add(c);
        }
        context.DonVis.AddRange(campuses);
        await context.SaveChangesAsync();
        var mainCampus = campuses[0];

        // 3. Tạo Chuyên ngành & Môn học
        var major = new NganhDaoTao { MaCodeNganh = "IT_LARGE_V9", TenNganh = "Công nghệ thông tin (Large V9)", ConHoatDong = true, NgayTao = DateTime.UtcNow };
        context.NganhDaoTaos.Add(major);
        await context.SaveChangesAsync();

        var specializations = new List<ChuyenNganh>();
        for(int i = 1; i <= 8; i++)
        {
            specializations.Add(new ChuyenNganh { MaNganh = major.MaNganh, TenChuyenNganh = $"Chuyên ngành {i}", ConHoatDong = true, NgayTao = DateTime.UtcNow });
        }
        context.ChuyenNganhs.AddRange(specializations);
        await context.SaveChangesAsync();

        var subjects = new List<DanhMucMonHoc>();
        for(int i = 1; i <= 50; i++)
        {
            subjects.Add(new DanhMucMonHoc { MaCodeMonHoc = $"SUB_LV9_{i}", TenMonHoc = $"Môn học Large V9 {i}", SoTinChi = 3, ConHoatDong = true });
        }
        context.DanhMucMonHocs.AddRange(subjects);
        await context.SaveChangesAsync();

        // 4. Khóa & Chương trình & Lớp hành chính
        var cohort = new KhoaTuyenSinh { MaCodeKhoa = "K_LARGE_DEMO_V9", TenKhoa = "Khóa Large Demo V9", NamBatDau = 2026, ConHoatDong = true, NgayTao = DateTime.UtcNow };
        context.KhoaTuyenSinhs.Add(cohort);
        await context.SaveChangesAsync();

        var program = new ChuongTrinhDaoTao { 
            MaCodeChuongTrinh = "CT_LARGE_V9", 
            TenChuongTrinh = "CTĐT Large V9", 
            MaChuyenNganh = specializations[0].MaChuyenNganh, 
            MaKhoaTuyenSinh = cohort.MaKhoaTuyenSinh,
            SoHocKy = 7, ThoiGianDaoTaoThang = 28, TongTinChiYeuCau = 120, Version = "1.0",
            TrangThai = "active", ConHoatDong = true, NgayTao = DateTime.UtcNow 
        };
        context.ChuongTrinhDaoTaos.Add(program);
        await context.SaveChangesAsync();

        var classes = new List<LopHanhChinh>();
        for (int i = 1; i <= 300; i++)
        {
            classes.Add(new LopHanhChinh { 
                MaCodeLop = $"L_CLASS_V9_{i}", 
                TenLop = $"Lớp Large V9 {i}", 
                MaDonVi = mainCampus.MaDonVi, 
                MaChuongTrinh = program.MaChuongTrinh, 
                NamNhapHoc = 2026, 
                ConHoatDong = true 
            });
        }
        context.LopHanhChinhs.AddRange(classes);
        await context.SaveChangesAsync();

        // 5. Teachers & NangLucGiangVien
        var teacherRole = AuthRoles.ToDatabaseCode(AuthRoles.Teacher);
        var passwordHash = PasswordHelper.HashPassword(Password);
        
        var teachers = new List<NguoiDung>();
        for (int i = 1; i <= 100; i++)
        {
            teachers.Add(new NguoiDung {
                Email = $"teacher.large.v9.{i:D3}@edulms.local",
                HoTen = $"Giảng Viên Large V9 {i}",
                VaiTroChinh = teacherRole,
                MaDonVi = mainCampus.MaDonVi,
                TrangThai = UserStatuses.DbActive,
                MatKhauHash = passwordHash,
                NgayTao = DateTime.UtcNow
            });
        }
        context.NguoiDungs.AddRange(teachers);
        await context.SaveChangesAsync();

        var nangLucs = new List<NangLucGiangVien>();
        var random = new Random(42); // deterministic
        foreach(var t in teachers)
        {
            // Mỗi giáo viên dạy 3-5 môn
            var numSubjects = random.Next(3, 6);
            var selectedSubjects = subjects.OrderBy(x => random.Next()).Take(numSubjects).ToList();
            foreach(var s in selectedSubjects)
            {
                nangLucs.Add(new NangLucGiangVien {
                    MaGiaoVien = t.MaNguoiDung,
                    MaMonHoc = s.MaMonHoc,
                    MucDoPhuHop = random.Next(3, 6), // 3-5
                    SoLanDaDay = random.Next(0, 10),
                    UuTien = random.Next(0, 3)
                });
            }
        }
        context.NangLucGiangViens.AddRange(nangLucs);
        await context.SaveChangesAsync();

        // 6. Students
        Console.WriteLine("Generating 10,000 students...");
        var studentRole = AuthRoles.ToDatabaseCode(AuthRoles.Student);
        var batchSize = 1000;
        var allStudents = new List<NguoiDung>();
        for (int b = 0; b < 10; b++) // 10 batches of 1000
        {
            var studentsBatch = new List<NguoiDung>();
            for (int i = 1; i <= batchSize; i++)
            {
                int studentIdx = b * batchSize + i;
                int classIdx = studentIdx % classes.Count;

                studentsBatch.Add(new NguoiDung {
                    Email = $"student.large.v9.{studentIdx:D5}@edulms.local",
                    HoTen = $"Sinh Viên Large V9 {studentIdx}",
                    VaiTroChinh = studentRole,
                    MaDonVi = mainCampus.MaDonVi,
                    TrangThai = UserStatuses.DbActive,
                    MatKhauHash = passwordHash,
                    MaLop = classes[classIdx].MaLop,
                    NamNhapHoc = 2026,
                    NgayTao = DateTime.UtcNow
                });
            }
            context.NguoiDungs.AddRange(studentsBatch);
            await context.SaveChangesAsync();
            allStudents.AddRange(studentsBatch);
        }

        // 7. Courses, Registrations, Assignments, Attendance
        await SeedCoursesAndActivitiesAsync(context, mainCampus, subjects, teachers, allStudents, nangLucs);

        Console.WriteLine("LargeDemo Seed completed successfully.");
    }

    private static async Task SeedCoursesAndActivitiesAsync(ApplicationDbContext context, DonVi campus, List<DanhMucMonHoc> subjects, List<NguoiDung> teachers, List<NguoiDung> students, List<NangLucGiangVien> nangLucs)
    {
        Console.WriteLine("Generating Courses...");
        var hocKy = await context.HocKys.FirstOrDefaultAsync(x => x.MaDonVi == campus.MaDonVi) 
            ?? new HocKy { MaCodeHocKy = "HK_LARGE", TenHocKy = "HK Large", NamHoc = "2026", ThuTuTrongNam = 1, NgayBatDau = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)), NgayKetThuc = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(60)), MaDonVi = campus.MaDonVi };
        if (hocKy.MaHocKy == 0) { context.HocKys.Add(hocKy); await context.SaveChangesAsync(); }

        var courses = new List<KhoaHoc>();
        var random = new Random(42);
        
        var lopHanhChinhs = await context.LopHanhChinhs.Where(x => x.MaDonVi == campus.MaDonVi).ToListAsync();
        
        var existingCourseCombos = new HashSet<string>();

        // Tạo 1000 KhoaHoc (1 Môn cho 1 Lớp), đảm bảo unique (MaDonVi, MaMonHoc, MaHocKy, MaLop)
        for(int i = 1; i <= 1000; i++)
        {
            var teacher = teachers[random.Next(teachers.Count)];
            var teacherNangLuc = nangLucs.Where(n => n.MaGiaoVien == teacher.MaNguoiDung).ToList();
            
            int monHocId;
            int lopId;
            string comboKey;
            
            do
            {
                monHocId = teacherNangLuc.Any() ? teacherNangLuc[random.Next(teacherNangLuc.Count)].MaMonHoc : subjects[random.Next(subjects.Count)].MaMonHoc;
                lopId = lopHanhChinhs[random.Next(lopHanhChinhs.Count)].MaLop;
                comboKey = $"{campus.MaDonVi}_{monHocId}_{hocKy.MaHocKy}_{lopId}";
            } while (existingCourseCombos.Contains(comboKey));
            
            existingCourseCombos.Add(comboKey);

            var targetSubject = subjects.First(s => s.MaMonHoc == monHocId);
            var targetClass = lopHanhChinhs.First(l => l.MaLop == lopId);

            courses.Add(new KhoaHoc {
                TieuDe = $"Môn {targetSubject.TenMonHoc} - {targetClass.TenLop}",
                MaMonHoc = monHocId,
                MaHocKy = hocKy.MaHocKy,
                MaLop = lopId,
                MaGiaoVien = teacher.MaNguoiDung,
                MaDonVi = campus.MaDonVi,
                TrangThai = "da_xuat_ban",
                NgayTao = DateTime.UtcNow
            });
        }
        context.KhoaHocs.AddRange(courses);
        await context.SaveChangesAsync();

        Console.WriteLine("Generating Assignments and Submissions...");
        var baiTaps = new List<BaiTap>();
        foreach(var c in courses.Take(100))
        {
            baiTaps.Add(new BaiTap {
                MaMonHoc = c.MaMonHoc,
                TieuDe = $"Bài tập 1 của môn {c.MaMonHoc}",
                HanNop = DateTime.UtcNow.AddDays(5),
                TrangThai = "da_xuat_ban",
                SoLanNopToiDa = 3,
                DinhDangChoPhep = "[\".pdf\", \".docx\", \".zip\"]"
            });
        }
        context.BaiTaps.AddRange(baiTaps);
        await context.SaveChangesAsync();

        var baiNops = new List<BaiNop>();
        foreach(var bt in baiTaps)
        {
            var targetCourse = courses.First(c => c.MaMonHoc == bt.MaMonHoc);
            // Các sinh viên trong lớp hành chính của khóa học này
            var studentsInClass = students.Where(s => s.MaLop == targetCourse.MaLop).ToList();
            foreach(var sv in studentsInClass)
            {
                if (random.NextDouble() > 0.3)
                {
                    baiNops.Add(new BaiNop {
                        MaBaiTap = bt.MaBaiTap,
                        MaHocSinh = sv.MaNguoiDung,
                        ThoiDiemNop = DateTime.UtcNow,
                        DiemSo = random.Next(5, 11),
                        NhanXet = "Bài nộp khá tốt.",
                        SoLanNop = 1,
                        UrlTapTin = "https://example.com/submission.pdf",
                        DaCongBo = true
                    });
                }
            }
        }
        context.BaiNops.AddRange(baiNops);
        await context.SaveChangesAsync();
    }
}
