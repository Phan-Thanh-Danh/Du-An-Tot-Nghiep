using Backend.Constants;
using Backend.Helpers;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Seeders;

public static class LargeDemoSeeder
{
    private const string LargeDemoCampus = "Cơ sở Large Demo V10";
    private const string Password = "password123";
    
    private static readonly Dictionary<string, List<string>> MajorSpecializations = new()
    {
        { "Công nghệ thông tin", new List<string> { "Phát triển phần mềm", "Lập trình Web", "Lập trình Mobile", "An toàn thông tin", "Quản trị hệ thống mạng", "Cơ sở dữ liệu" } },
        { "Thiết kế đồ họa", new List<string> { "Thiết kế đồ họa số", "UI/UX Design", "Thiết kế thương hiệu", "Motion Graphic", "Nhiếp ảnh / xử lý ảnh" } },
        { "Kinh tế / Marketing", new List<string> { "Digital Marketing", "Quản trị kinh doanh", "Thương mại điện tử", "Truyền thông đa phương tiện" } },
        { "Kế toán / Tài chính", new List<string> { "Kế toán doanh nghiệp", "Tài chính doanh nghiệp" } },
        { "Ngoại ngữ / Kỹ năng", new List<string> { "Tiếng Anh", "Kỹ năng mềm", "Tin học văn phòng" } }
    };

    private static readonly Dictionary<string, List<string>> SpecializationSubjects = new()
    {
        { "Phát triển phần mềm", new List<string> { "C# căn bản", "Lập trình Java", "Cơ sở dữ liệu SQL Server", "Kiểm thử phần mềm" } },
        { "Lập trình Web", new List<string> { "Thiết kế Web", "ASP.NET Core", "API Backend", "JavaScript", "Vue.js", "React" } },
        { "Lập trình Mobile", new List<string> { "Mobile App" } },
        { "An toàn thông tin", new List<string> { "An toàn thông tin cơ bản" } },
        { "Quản trị hệ thống mạng", new List<string> { "Quản trị Linux", "Mạng máy tính" } },
        { "Cơ sở dữ liệu", new List<string> { "Cơ sở dữ liệu SQL Server" } },
        
        { "Thiết kế đồ họa số", new List<string> { "Photoshop", "Illustrator", "Layout Design" } },
        { "UI/UX Design", new List<string> { "Figma", "UI/UX Design" } },
        { "Thiết kế thương hiệu", new List<string> { "Typography", "Branding" } },
        { "Motion Graphic", new List<string> { "Motion Graphic" } },
        { "Nhiếp ảnh / xử lý ảnh", new List<string> { "Thiết kế ấn phẩm truyền thông" } },
        
        { "Digital Marketing", new List<string> { "Marketing căn bản", "Digital Marketing", "SEO" } },
        { "Quản trị kinh doanh", new List<string> { "Quản trị dự án" } },
        { "Thương mại điện tử", new List<string> { "Thương mại điện tử" } },
        { "Truyền thông đa phương tiện", new List<string> { "Content Marketing" } },
        
        { "Kế toán doanh nghiệp", new List<string> { "Nguyên lý kế toán", "Kế toán doanh nghiệp", "Thuế căn bản" } },
        { "Tài chính doanh nghiệp", new List<string> { "Tài chính doanh nghiệp" } },
        
        { "Tiếng Anh", new List<string> { "Tiếng Anh 1", "Tiếng Anh 2", "Tiếng Anh 3" } },
        { "Kỹ năng mềm", new List<string> { "Kỹ năng mềm", "Pháp luật đại cương" } },
        { "Tin học văn phòng", new List<string> { "Tin học văn phòng" } }
    };

    private static readonly Dictionary<string, int> TeacherDistribution = new()
    {
        { "Công nghệ thông tin", 35 },
        { "Thiết kế đồ họa", 20 },
        { "Kinh tế / Marketing", 15 },
        { "Kế toán / Tài chính", 10 },
        { "Ngoại ngữ / Kỹ năng", 20 }
    };

    public static async Task SeedAsync(ApplicationDbContext context)
    {
        var exists = await context.DonVis.AnyAsync(x => x.TenDonVi == LargeDemoCampus);
        if (exists)
        {
            Console.WriteLine("LargeDemo data (V10) already exists. Skipping.");
            return;
        }

        Console.WriteLine("Starting LargeDemo Seed V10...");
        
        var rootCampus = await context.DonVis.FirstOrDefaultAsync(x => x.CapDonVi == "root")
            ?? new DonVi { TenDonVi = "LMS Root", CapDonVi = "root", ConHoatDong = true, NgayTao = DateTime.UtcNow };
        if (rootCampus.MaDonVi == 0) { context.DonVis.Add(rootCampus); await context.SaveChangesAsync(); }

        var mainCampus = new DonVi { TenDonVi = LargeDemoCampus, CapDonVi = "co_so", MaDonViCha = rootCampus.MaDonVi, ConHoatDong = true, NgayTao = DateTime.UtcNow };
        context.DonVis.Add(mainCampus);
        await context.SaveChangesAsync();

        Console.WriteLine("Seeding Majors and Specializations...");
        var majorDict = new Dictionary<string, NganhDaoTao>();
        var specDict = new Dictionary<string, ChuyenNganh>();
        
        foreach(var majorKvp in MajorSpecializations)
        {
            var majorName = majorKvp.Key;
            var major = await context.NganhDaoTaos.FirstOrDefaultAsync(x => x.TenNganh == majorName);
            if (major == null)
            {
                major = new NganhDaoTao { MaCodeNganh = $"CODE_{Guid.NewGuid().ToString().Substring(0,5)}", TenNganh = majorName, ConHoatDong = true, NgayTao = DateTime.UtcNow };
                context.NganhDaoTaos.Add(major);
                await context.SaveChangesAsync();
            }
            majorDict[majorName] = major;

            foreach(var specName in majorKvp.Value)
            {
                var spec = await context.ChuyenNganhs.FirstOrDefaultAsync(x => x.TenChuyenNganh == specName);
                if (spec == null)
                {
                    spec = new ChuyenNganh { MaNganh = major.MaNganh, TenChuyenNganh = specName, ConHoatDong = true, NgayTao = DateTime.UtcNow };
                    context.ChuyenNganhs.Add(spec);
                }
                specDict[specName] = spec;
            }
        }
        await context.SaveChangesAsync();

        Console.WriteLine("Seeding Subjects...");
        var subjectDict = new Dictionary<string, DanhMucMonHoc>();
        foreach(var specs in SpecializationSubjects.Values)
        {
            foreach(var sub in specs)
            {
                if (!subjectDict.ContainsKey(sub))
                {
                    var existingSub = await context.DanhMucMonHocs.FirstOrDefaultAsync(x => x.TenMonHoc == sub);
                    if (existingSub == null)
                    {
                        existingSub = new DanhMucMonHoc { MaCodeMonHoc = $"SUB_{Guid.NewGuid().ToString().Substring(0,5)}", TenMonHoc = sub, SoTinChi = 3, ConHoatDong = true };
                        context.DanhMucMonHocs.Add(existingSub);
                        await context.SaveChangesAsync();
                    }
                    subjectDict[sub] = existingSub;
                }
            }
        }

        Console.WriteLine("Seeding Teachers and Capabilities...");
        var teacherRole = AuthRoles.ToDatabaseCode(AuthRoles.Teacher);
        var passwordHash = PasswordHelper.HashPassword(Password);
        var random = new Random(42);
        int teacherCounter = 1;
        
        var allSeededTeachers = new List<NguoiDung>();
        var teacherCapabilities = new List<GiaoVienMonHoc>();

        foreach (var dist in TeacherDistribution)
        {
            var majorName = dist.Key;
            var specList = MajorSpecializations[majorName];
            
            for (int i = 0; i < dist.Value; i++)
            {
                var teacher = new NguoiDung {
                    Email = $"teacher.{majorName.Replace(" ", "").Replace("/", "").ToLower()}.v10.{teacherCounter:D3}@edulms.local",
                    HoTen = $"Giảng Viên {majorName} {teacherCounter}",
                    VaiTroChinh = teacherRole,
                    MaDonVi = mainCampus.MaDonVi,
                    TrangThai = UserStatuses.DbActive,
                    MatKhauHash = passwordHash,
                    NgayTao = DateTime.UtcNow
                };
                context.NguoiDungs.Add(teacher);
                await context.SaveChangesAsync();
                allSeededTeachers.Add(teacher);

                // Assign Specialization
                var assignedSpecName = specList[random.Next(specList.Count)];
                var spec = specDict[assignedSpecName];
                
                context.GiaoVienChuyenNganhs.Add(new GiaoVienChuyenNganh {
                    MaGiaoVien = teacher.MaNguoiDung,
                    MaChuyenNganh = spec.MaChuyenNganh,
                    LaChuyenMonChinh = true,
                    MucDoPhuHop = random.Next(80, 101),
                    SoNamKinhNghiem = random.Next(2, 15)
                });

                // Assign Subjects (3 to 8 subjects)
                var possibleSubjects = SpecializationSubjects[assignedSpecName].ToList();
                // Add some other subjects from the same major
                foreach(var otherSpec in specList)
                {
                    if (otherSpec != assignedSpecName)
                    {
                        possibleSubjects.AddRange(SpecializationSubjects[otherSpec]);
                    }
                }
                
                possibleSubjects = possibleSubjects.Distinct().OrderBy(x => random.Next()).Take(random.Next(3, 8)).ToList();
                
                bool firstMonChinh = true;
                foreach(var subName in possibleSubjects)
                {
                    var sub = subjectDict[subName];
                    var tCap = new GiaoVienMonHoc {
                        MaGiaoVien = teacher.MaNguoiDung,
                        MaMonHoc = sub.MaMonHoc,
                        MucDoPhuHop = random.Next(70, 101),
                        SoLanDaDay = random.Next(1, 20),
                        SoNamKinhNghiem = random.Next(1, 10),
                        LaMonChinh = firstMonChinh
                    };
                    firstMonChinh = false;
                    teacherCapabilities.Add(tCap);
                }
                teacherCounter++;
            }
        }
        context.GiaoVienMonHocs.AddRange(teacherCapabilities);
        await context.SaveChangesAsync();

        Console.WriteLine("Seeding Administrative Classes & Students...");
        var cohort = new KhoaTuyenSinh { MaCodeKhoa = "K_LARGE_DATA_V10", TenKhoa = "Khóa Large Demo V10", NamBatDau = 2026, ConHoatDong = true, NgayTao = DateTime.UtcNow };
        context.KhoaTuyenSinhs.Add(cohort);
        await context.SaveChangesAsync();

        var program = new ChuongTrinhDaoTao { 
            MaCodeChuongTrinh = "CT_LARGE_V10", 
            TenChuongTrinh = "CTĐT Large V10", 
            MaChuyenNganh = specDict.Values.First().MaChuyenNganh, 
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
                MaCodeLop = $"L_CLASS_V10_{i}", 
                TenLop = $"Lớp Large V10 {i}", 
                MaDonVi = mainCampus.MaDonVi, 
                MaChuongTrinh = program.MaChuongTrinh, 
                NamNhapHoc = 2026, 
                ConHoatDong = true 
            });
        }
        context.LopHanhChinhs.AddRange(classes);
        await context.SaveChangesAsync();

        var studentRole = AuthRoles.ToDatabaseCode(AuthRoles.Student);
        var batchSize = 1000;
        var allStudents = new List<NguoiDung>();
        for (int b = 0; b < 10; b++)
        {
            var studentsBatch = new List<NguoiDung>();
            for (int i = 1; i <= batchSize; i++)
            {
                int studentIdx = b * batchSize + i;
                int classIdx = studentIdx % classes.Count;

                studentsBatch.Add(new NguoiDung {
                    Email = $"student.large.v10.{studentIdx:D5}@edulms.local",
                    HoTen = $"Sinh Viên Large V10 {studentIdx}",
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

        Console.WriteLine("Generating Smart Course Allocations...");
        var hocKy = new HocKy { MaCodeHocKy = "HK_LARGE_V10", TenHocKy = "HK Large V10", NamHoc = "2026", ThuTuTrongNam = 1, NgayBatDau = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)), NgayKetThuc = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(60)), MaDonVi = mainCampus.MaDonVi };
        context.HocKys.Add(hocKy);
        await context.SaveChangesAsync();

        var courses = new List<KhoaHoc>();
        var subjectsList = subjectDict.Values.ToList();
        
        var teacherWorkload = allSeededTeachers.ToDictionary(t => t.MaNguoiDung, t => 0);
        var existingCourseCombos = new HashSet<string>();
        int failedAllocations = 0;

        for(int i = 1; i <= 1000; i++)
        {
            var sub = subjectsList[random.Next(subjectsList.Count)];
            var lopId = classes[random.Next(classes.Count)].MaLop;
            
            string comboKey = $"{mainCampus.MaDonVi}_{sub.MaMonHoc}_{hocKy.MaHocKy}_{lopId}";
            if (existingCourseCombos.Contains(comboKey)) continue;
            
            // Smart Course Allocation Logic: Find eligible teachers for this subject
            var eligibleTeacherIds = teacherCapabilities
                .Where(tc => tc.MaMonHoc == sub.MaMonHoc)
                .OrderByDescending(tc => tc.LaMonChinh)
                .ThenByDescending(tc => tc.MucDoPhuHop)
                .Select(tc => tc.MaGiaoVien)
                .ToList();
                
            if (!eligibleTeacherIds.Any())
            {
                Console.WriteLine($"[WARNING] No eligible teacher found for subject: {sub.TenMonHoc}");
                failedAllocations++;
                continue;
            }

            // Load balancing: pick the teacher with the least workload among the top candidates
            var topCandidates = eligibleTeacherIds.Take(5).ToList();
            var selectedTeacherId = topCandidates.OrderBy(tid => teacherWorkload[tid]).First();
            teacherWorkload[selectedTeacherId]++;
            
            var targetClass = classes.First(c => c.MaLop == lopId);

            existingCourseCombos.Add(comboKey);
            courses.Add(new KhoaHoc {
                TieuDe = $"Môn {sub.TenMonHoc} - {targetClass.TenLop}",
                MaMonHoc = sub.MaMonHoc,
                MaHocKy = hocKy.MaHocKy,
                MaLop = lopId,
                MaGiaoVien = selectedTeacherId,
                MaDonVi = mainCampus.MaDonVi,
                TrangThai = "da_xuat_ban",
                NgayTao = DateTime.UtcNow
            });
        }
        
        if(failedAllocations > 0)
        {
            Console.WriteLine($"[WARNING] Failed to allocate {failedAllocations} courses due to missing teacher specializations.");
        }
        
        context.KhoaHocs.AddRange(courses);
        await context.SaveChangesAsync();

        Console.WriteLine("LargeDemo Seed V10 completed successfully.");
    }
}
