using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;

namespace Backend.Data.Seeders;

public static class TeacherRichDataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        var lecturer = await db.NguoiDungs.FirstOrDefaultAsync(u => u.Email == "lecturer01@edulms.local");
        if (lecturer == null) return;

        int lecturerId = lecturer.MaNguoiDung;
        int orgId = lecturer.MaDonVi;

        // Reset password to 123456 and status to active
        lecturer.MatKhauHash = Backend.Helpers.PasswordHelper.HashPassword("123456");
        lecturer.SoLanSaiMatKhau = 0;
        lecturer.TrangThai = "hoat_dong";
        lecturer.VaiTroChinh = "giao_vien";
        await db.SaveChangesAsync();

        var semester = await db.HocKys.OrderByDescending(h => h.NgayBatDau).FirstOrDefaultAsync();
        if (semester == null) return;
        int semesterId = semester.MaHocKy;

        var subjects = await db.DanhMucMonHocs.Take(6).ToListAsync();
        if (subjects.Count == 0) return;

        var classes = await db.LopHanhChinhs.Where(l => l.MaDonVi == orgId).Take(6).ToListAsync();
        if (classes.Count == 0)
        {
            classes = await db.LopHanhChinhs.Take(6).ToListAsync();
        }

        var rooms = await db.PhongHocs.Where(p => p.MaDonVi == orgId).Take(5).ToListAsync();
        if (rooms.Count == 0)
        {
            rooms = await db.PhongHocs.Take(5).ToListAsync();
        }

        var shifts = await db.CaHocs.Take(6).ToListAsync();
        if (shifts.Count == 0) return;

        // 1. Gán sinh viên vào các lớp của Giảng viên (mỗi lớp 15-20 sinh viên)
        var allStudents = await db.NguoiDungs
            .Where(u => (u.VaiTroChinh == "hoc_sinh" || u.VaiTroChinh == "Student") && u.MaDonVi == orgId)
            .Take(120)
            .ToListAsync();

        if (allStudents.Count < 60)
        {
            allStudents = await db.NguoiDungs
                .Where(u => u.VaiTroChinh == "hoc_sinh" || u.VaiTroChinh == "Student")
                .Take(120)
                .ToListAsync();
        }

        for (int i = 0; i < allStudents.Count; i++)
        {
            var targetClass = classes[i % classes.Count];
            if (allStudents[i].MaLop != targetClass.MaLop)
            {
                allStudents[i].MaLop = targetClass.MaLop;
            }
        }
        await db.SaveChangesAsync();

        var random = new Random(42);

        // 2. Khóa học cho Giảng viên (6 lớp, 6 môn)
        var courses = await db.KhoaHocs.Where(k => k.MaGiaoVien == lecturerId).ToListAsync();
        for (int i = 0; i < Math.Min(classes.Count, subjects.Count); i++)
        {
            var sub = subjects[i];
            var cls = classes[i];
            var existing = courses.FirstOrDefault(c => c.MaMonHoc == sub.MaMonHoc && c.MaLop == cls.MaLop);
            if (existing == null)
            {
                var newCourse = new KhoaHoc
                {
                    MaDonVi = orgId,
                    MaMonHoc = sub.MaMonHoc,
                    MaLop = cls.MaLop,
                    MaGiaoVien = lecturerId,
                    MaHocKy = semesterId,
                    TieuDe = $"{sub.TenMonHoc} - {cls.TenLop}",
                    TrangThai = "da_xuat_ban",
                    NgayTao = DateTime.UtcNow.AddDays(-60)
                };
                db.KhoaHocs.Add(newCourse);
            }
        }
        await db.SaveChangesAsync();
        courses = await db.KhoaHocs.Where(k => k.MaGiaoVien == lecturerId).ToListAsync();

        // 3. Thời khóa biểu, Buổi học, Điểm danh, Bài tập, Bài nộp, Điểm số, Tiến độ
        var now = DateOnly.FromDateTime(DateTime.Today);

        for (int cIdx = 0; cIdx < courses.Count; cIdx++)
        {
            var course = courses[cIdx];
            var room = rooms[cIdx % rooms.Count];
            var shift = shifts[cIdx % shifts.Count];

            // TKB
            var existingTkb = await db.ThoiKhoaBieus.FirstOrDefaultAsync(t => t.MaKhoaHoc == course.MaKhoaHoc);
            if (existingTkb == null)
            {
                existingTkb = new ThoiKhoaBieu
                {
                    MaKhoaHoc = course.MaKhoaHoc,
                    MaPhong = room.MaPhong,
                    MaCaHoc = shift.MaCaHoc,
                    ThuTrongTuan = (cIdx % 6) + 2,
                    TrangThai = "da_xuat_ban",
                    NgayTao = DateTime.UtcNow.AddDays(-60)
                };
                db.ThoiKhoaBieus.Add(existingTkb);
                await db.SaveChangesAsync();
            }

            // Buổi học
            var existingSessions = await db.BuoiHocs.Where(b => b.MaKhoaHoc == course.MaKhoaHoc).ToListAsync();
            if (existingSessions.Count < 8)
            {
                var newSessions = new List<BuoiHoc>();
                // 5 buổi quá khứ
                for (int s = 5; s >= 1; s--)
                {
                    var sessionDate = now.AddDays(-s * 7);
                    if (!existingSessions.Any(b => b.NgayHoc == sessionDate))
                    {
                        newSessions.Add(new BuoiHoc
                        {
                            MaKhoaHoc = course.MaKhoaHoc,
                            MaTkb = existingTkb.MaTkb,
                            MaPhong = room.MaPhong,
                            MaCaHoc = shift.MaCaHoc,
                            MaGiaoVien = lecturerId,
                            NgayHoc = sessionDate,
                            TrangThaiBuoi = "da_dien_ra",
                            TrangThaiDiemDanh = "da_khoa",
                            NgayTao = DateTime.UtcNow.AddDays(-60)
                        });
                    }
                }

                // 1 buổi hôm nay
                if (!existingSessions.Any(b => b.NgayHoc == now))
                {
                    newSessions.Add(new BuoiHoc
                    {
                        MaKhoaHoc = course.MaKhoaHoc,
                        MaTkb = existingTkb.MaTkb,
                        MaPhong = room.MaPhong,
                        MaCaHoc = shift.MaCaHoc,
                        MaGiaoVien = lecturerId,
                        NgayHoc = now,
                        TrangThaiBuoi = "du_kien",
                        TrangThaiDiemDanh = "dang_diem_danh",
                        NgayTao = DateTime.UtcNow.AddDays(-60)
                    });
                }

                // 3 buổi tương lai
                for (int s = 1; s <= 3; s++)
                {
                    var sessionDate = now.AddDays(s * 7);
                    if (!existingSessions.Any(b => b.NgayHoc == sessionDate))
                    {
                        newSessions.Add(new BuoiHoc
                        {
                            MaKhoaHoc = course.MaKhoaHoc,
                            MaTkb = existingTkb.MaTkb,
                            MaPhong = room.MaPhong,
                            MaCaHoc = shift.MaCaHoc,
                            MaGiaoVien = lecturerId,
                            NgayHoc = sessionDate,
                            TrangThaiBuoi = "du_kien",
                            TrangThaiDiemDanh = "chua_mo",
                            NgayTao = DateTime.UtcNow.AddDays(-60)
                        });
                    }
                }

                if (newSessions.Count > 0)
                {
                    db.BuoiHocs.AddRange(newSessions);
                    await db.SaveChangesAsync();
                }
            }

            // Điểm danh
            var classStudents = await db.NguoiDungs
                .Where(u => u.MaLop == course.MaLop && (u.VaiTroChinh == "hoc_sinh" || u.VaiTroChinh == "Student"))
                .ToListAsync();

            var completedSessions = await db.BuoiHocs
                .Where(b => b.MaKhoaHoc == course.MaKhoaHoc && (b.TrangThaiBuoi == "da_dien_ra" || b.TrangThaiBuoi == "dang_dien_ra"))
                .ToListAsync();

            foreach (var session in completedSessions)
            {
                var existingAttendance = await db.DiemDanhs.Where(d => d.MaBuoiHoc == session.MaBuoiHoc).ToListAsync();
                var newAttendances = new List<DiemDanh>();

                for (int stIdx = 0; stIdx < classStudents.Count; stIdx++)
                {
                    var st = classStudents[stIdx];
                    if (!existingAttendance.Any(d => d.MaHocSinh == st.MaNguoiDung))
                    {
                        string status = (stIdx % 10 == 9) ? "vang" : ((stIdx % 10 == 8) ? "di_muon" : "co_mat");
                        newAttendances.Add(new DiemDanh
                        {
                            MaDonVi = orgId,
                            MaBuoiHoc = session.MaBuoiHoc,
                            MaHocSinh = st.MaNguoiDung,
                            TrangThai = status,
                            NguoiGhiNhan = lecturerId,
                            GhiNhanLuc = DateTime.UtcNow.AddDays(-1),
                            HeSoVang = status == "vang" ? 1 : 0
                        });
                    }
                }

                if (newAttendances.Count > 0)
                {
                    db.DiemDanhs.AddRange(newAttendances);
                }
            }
            await db.SaveChangesAsync();

            // Bài tập & Bài nộp
            var existingAssignments = await db.BaiTaps.Where(b => b.MaMonHoc == course.MaMonHoc).ToListAsync();
            if (existingAssignments.Count == 0)
            {
                var asm1 = new BaiTap
                {
                    MaMonHoc = course.MaMonHoc,
                    TieuDe = $"Lab Thực Hành 01 - {course.TieuDe}",
                    MoTa = "Sinh viên hoàn thành bài thực hành theo yêu cầu trong tài liệu hướng dẫn và nộp file zip source code.",
                    HanNop = DateTime.UtcNow.AddDays(7),
                    SoLanNopToiDa = 3,
                    DinhDangChoPhep = JsonSerializer.Serialize(new[] { "zip", "rar", "pdf" }),
                    TrangThai = "da_xuat_ban",
                    DungLuongToiThieuKB = 10,
                    DungLuongToiDaMB = 50
                };

                var asm2 = new BaiTap
                {
                    MaMonHoc = course.MaMonHoc,
                    TieuDe = $"Đồ Án Giữa Kỳ - {course.TieuDe}",
                    MoTa = "Xây dựng ứng dụng hoàn chỉnh theo đề tài đã đăng ký, viết báo cáo kỹ thuật và quay video demo chức năng.",
                    HanNop = DateTime.UtcNow.AddDays(14),
                    SoLanNopToiDa = 5,
                    DinhDangChoPhep = JsonSerializer.Serialize(new[] { "pdf", "zip", "rar" }),
                    TrangThai = "da_xuat_ban",
                    DungLuongToiThieuKB = 10,
                    DungLuongToiDaMB = 100
                };

                db.BaiTaps.AddRange(asm1, asm2);
                await db.SaveChangesAsync();

                for (int stIdx = 0; stIdx < classStudents.Count; stIdx++)
                {
                    var st = classStudents[stIdx];
                    db.BaiNops.Add(new BaiNop
                    {
                        MaBaiTap = asm1.MaBaiTap,
                        MaHocSinh = st.MaNguoiDung,
                        UrlTapTin = $"/storage/submissions/asm_{asm1.MaBaiTap}_student_{st.MaNguoiDung}.zip",
                        SoLanNop = 1,
                        NopTre = stIdx % 8 == 7,
                        DiemDaoVan = (decimal)(random.Next(2, 12) * 1.0),
                        DiemSo = (decimal)Math.Round(7.0 + (random.NextDouble() * 2.8), 1),
                        DiemAiDeXuat = (decimal)Math.Round(7.5 + (random.NextDouble() * 2.2), 1),
                        NhanXet = stIdx % 2 == 0 ? "Bài làm tốt, cấu trúc rõ ràng, logic code chặt chẽ." : "Hoàn thành đầy đủ các yêu cầu, giao diện đẹp.",
                        ThoiDiemNop = DateTime.UtcNow.AddDays(-3).AddHours(random.Next(1, 10)),
                        DaCongBo = true
                    });

                    if (stIdx < 10)
                    {
                        db.BaiNops.Add(new BaiNop
                        {
                            MaBaiTap = asm2.MaBaiTap,
                            MaHocSinh = st.MaNguoiDung,
                            UrlTapTin = $"/storage/submissions/asm_{asm2.MaBaiTap}_student_{st.MaNguoiDung}.pdf",
                            SoLanNop = 1,
                            NopTre = false,
                            DiemDaoVan = (decimal)(random.Next(1, 8) * 1.0),
                            DiemSo = (decimal)Math.Round(8.0 + (random.NextDouble() * 1.8), 1),
                            DiemAiDeXuat = (decimal)Math.Round(8.2 + (random.NextDouble() * 1.5), 1),
                            NhanXet = "Đồ án đạt chất lượng xuất sắc",
                            ThoiDiemNop = DateTime.UtcNow.AddDays(-1).AddHours(random.Next(1, 8)),
                            DaCongBo = true
                        });
                    }
                }
                await db.SaveChangesAsync();
            }

            // Điểm số học phần (DiemSo)
            var existingGrades = await db.DiemSos
                .Where(d => d.MaMonHoc == course.MaMonHoc && d.MaHocKy == semesterId)
                .ToListAsync();

            foreach (var st in classStudents)
            {
                var grade = existingGrades.FirstOrDefault(d => d.MaHocSinh == st.MaNguoiDung);
                decimal qt = (decimal)Math.Round(7.5 + (random.NextDouble() * 2.3), 1);
                decimal ck = (decimal)Math.Round(7.0 + (random.NextDouble() * 2.5), 1);
                decimal gpa = Math.Round(qt * 0.4m + ck * 0.6m, 1);

                if (grade == null)
                {
                    db.DiemSos.Add(new DiemSo
                    {
                        MaDonVi = orgId,
                        MaHocSinh = st.MaNguoiDung,
                        MaMonHoc = course.MaMonHoc,
                        MaHocKy = semesterId,
                        DiemQuaTrinh = qt,
                        DiemGiuaKy = (decimal)Math.Round(7.0 + (random.NextDouble() * 2.5), 1),
                        DiemCuoiKy = ck,
                        GpaMonHoc = gpa,
                        TrangThai = "dat",
                        DaKhoa = false,
                        NamNhapHoc = 2026
                    });
                }
                else
                {
                    grade.DiemQuaTrinh = qt;
                    grade.DiemCuoiKy = ck;
                    grade.GpaMonHoc = gpa;
                    grade.TrangThai = "dat";
                }
            }
            await db.SaveChangesAsync();

            // Tiến độ bài học (TienDoBaiHoc)
            var lessonIds = await db.BaiHocs
                .Where(b => b.Chuong != null && b.Chuong.MaMonHoc == course.MaMonHoc)
                .Select(b => b.MaBaiHoc)
                .ToListAsync();

            if (lessonIds.Count > 0)
            {
                var existingProgress = await db.TienDoBaiHocs
                    .Where(t => lessonIds.Contains(t.MaBaiHoc))
                    .ToListAsync();

                var newProgressList = new List<TienDoBaiHoc>();
                foreach (var st in classStudents)
                {
                    foreach (var lId in lessonIds.Take(8))
                    {
                        if (!existingProgress.Any(p => p.MaHocSinh == st.MaNguoiDung && p.MaBaiHoc == lId))
                        {
                            newProgressList.Add(new TienDoBaiHoc
                            {
                                MaHocSinh = st.MaNguoiDung,
                                MaBaiHoc = lId,
                                PhanTramTienDo = 100m,
                                HoanThanhLuc = DateTime.UtcNow.AddDays(-random.Next(1, 20))
                            });
                        }
                    }
                }
                if (newProgressList.Count > 0)
                {
                    db.TienDoBaiHocs.AddRange(newProgressList);
                    await db.SaveChangesAsync();
                }
            }
        }

        // 4. Ca thi & Giám thị: Xóa toàn bộ liên kết giám thị cũ và gán 4 Ca thi mới sạch đẹp
        var oldAssignments = await db.PhanCongGiamThis.Where(pc => pc.MaGiamThi == lecturerId).ToListAsync();
        if (oldAssignments.Count > 0)
        {
            db.PhanCongGiamThis.RemoveRange(oldAssignments);
            await db.SaveChangesAsync();
        }

        var defaultKyThi = await db.KyThis.FirstOrDefaultAsync();
        if (defaultKyThi == null)
        {
            defaultKyThi = new KyThi
            {
                TenKyThi = "Kỳ thi Đánh giá Học kỳ Chính",
                MaHocKy = semesterId,
                LoaiKyThi = "cuoi_ky",
                TrangThai = "dang_dien_ra",
                NgayTao = DateTime.UtcNow.AddDays(-30)
            };
            db.KyThis.Add(defaultKyThi);
            await db.SaveChangesAsync();
        }
        int kyThiId = defaultKyThi.MaKyThi;

        var examDefs = new[]
        {
            new { 
                Title = "Ca 01: Thi kết thúc học phần - Lập trình C# cơ bản & nâng cao",
                Room = rooms[0],
                Status = "da_ket_thuc",
                Date = DateTime.Today.AddDays(-2),
                Start = DateTime.Today.AddDays(-2).AddHours(7).AddMinutes(30),
                End = DateTime.Today.AddDays(-2).AddHours(9).AddMinutes(30),
                Subj = subjects[0]
            },
            new { 
                Title = "Ca 02: Thi trắc nghiệm trực tuyến - Thiết kế UI/UX hiện đại",
                Room = rooms.Count > 1 ? rooms[1] : rooms[0],
                Status = "dang_thi",
                Date = DateTime.Today,
                Start = DateTime.Today.AddHours(9).AddMinutes(45),
                End = DateTime.Today.AddHours(11).AddMinutes(45),
                Subj = subjects.Count > 1 ? subjects[1] : subjects[0]
            },
            new { 
                Title = "Ca 03: Thi thực hành - Cơ sở dữ liệu SQL Server",
                Room = rooms.Count > 2 ? rooms[2] : rooms[0],
                Status = "dang_diem_danh",
                Date = DateTime.Today,
                Start = DateTime.Today.AddHours(13).AddMinutes(30),
                End = DateTime.Today.AddHours(15).AddMinutes(30),
                Subj = subjects.Count > 2 ? subjects[2] : subjects[0]
            },
            new { 
                Title = "Ca 04: Thi vấn đáp & đồ án - Phát triển Web Frontend với Vue 3",
                Room = rooms.Count > 3 ? rooms[3] : rooms[0],
                Status = "da_san_sang",
                Date = DateTime.Today.AddDays(3),
                Start = DateTime.Today.AddDays(3).AddHours(15).AddMinutes(45),
                End = DateTime.Today.AddDays(3).AddHours(17).AddMinutes(45),
                Subj = subjects.Count > 3 ? subjects[3] : subjects[0]
            }
        };

        for (int eIdx = 0; eIdx < examDefs.Length; eIdx++)
        {
            var ed = examDefs[eIdx];
            var existingCa = await db.CaThis.FirstOrDefaultAsync(c => c.TenCaThi == ed.Title);
            if (existingCa == null)
            {
                // Đảm bảo có LichThiTong trỏ đúng môn học
                var ltt = await db.LichThiTongs.FirstOrDefaultAsync(l => l.MaMonHoc == ed.Subj.MaMonHoc);
                if (ltt == null)
                {
                    ltt = new LichThiTong
                    {
                        MaKyThi = kyThiId,
                        MaMonHoc = ed.Subj.MaMonHoc,
                        HinhThucThi = "online_tap_trung",
                        NgayThiDuKien = ed.Date,
                        TrangThai = "da_gui_ve_co_so",
                        NgayTao = DateTime.UtcNow.AddDays(-30)
                    };
                    db.LichThiTongs.Add(ltt);
                    await db.SaveChangesAsync();
                }

                existingCa = new CaThi
                {
                    TenCaThi = ed.Title,
                    MaLichThiTong = ltt.MaLichThiTong,
                    MaPhong = ed.Room.MaPhong,
                    MaDonVi = orgId,
                    NgayThi = ed.Date,
                    ThoiGianBatDau = ed.Start,
                    ThoiGianKetThuc = ed.End,
                    TrangThai = ed.Status,
                    GhiChu = "Ca thi chính thức",
                    NgayTao = DateTime.UtcNow.AddDays(-15)
                };
                db.CaThis.Add(existingCa);
                await db.SaveChangesAsync();
            }

            // Phân công giám thị
            var hasAssignment = await db.PhanCongGiamThis
                .AnyAsync(pc => pc.MaCaThi == existingCa.MaCaThi && pc.MaGiamThi == lecturerId);
            if (!hasAssignment)
            {
                db.PhanCongGiamThis.Add(new PhanCongGiamThi
                {
                    MaCaThi = existingCa.MaCaThi,
                    MaGiamThi = lecturerId,
                    VaiTroGiamThi = "giam_thi_chinh",
                    TrangThai = "da_xac_nhan",
                    NgayTao = DateTime.UtcNow.AddDays(-10)
                });
                await db.SaveChangesAsync();
            }

            // Đề kiểm tra tương ứng cho môn thi
            var testDe = await db.DeKiemTras.FirstOrDefaultAsync(d => d.MaMonHoc == ed.Subj.MaMonHoc);
            if (testDe == null)
            {
                testDe = new DeKiemTra
                {
                    TieuDe = $"Đề thi trắc nghiệm - {ed.Subj.TenMonHoc}",
                    MaMonHoc = ed.Subj.MaMonHoc,
                    MaHocKy = semesterId,
                    ThoiGianPhut = 60,
                    HinhThucThi = "online_tap_trung",
                    LoaiDeThi = "trac_nghiem",
                    TrangThai = "dang_mo",
                    TrangThaiDuyet = "da_duyet",
                    TyLeTracNghiem = 100,
                    TyLeTuLuan = 0,
                    MaNguoiSoan = lecturerId,
                    MaNguoiDuyet = lecturerId,
                    NgayTao = DateTime.UtcNow.AddDays(-30),
                    NgayCapNhat = DateTime.UtcNow.AddDays(-10)
                };
                db.DeKiemTras.Add(testDe);
                await db.SaveChangesAsync();
            }

            // Gán 20-25 thí sinh vào ca thi
            var examCandidates = allStudents.Skip(eIdx * 20).Take(22).ToList();
            if (examCandidates.Count == 0) examCandidates = allStudents.Take(20).ToList();

            var existingThiSinh = await db.ThiSinhCaThis
                .Where(t => t.MaCaThi == existingCa.MaCaThi)
                .ToListAsync();

            var existingDiemDanhThi = await db.DiemDanhThis
                .Where(t => t.MaCaThi == existingCa.MaCaThi)
                .ToListAsync();

            var newCandidates = new List<ThiSinhCaThi>();
            var newExamAttendances = new List<DiemDanhThi>();
            var newExamSessions = new List<PhienThiHocSinh>();

            for (int cndIdx = 0; cndIdx < examCandidates.Count; cndIdx++)
            {
                var cand = examCandidates[cndIdx];
                if (!existingThiSinh.Any(t => t.MaHocSinh == cand.MaNguoiDung))
                {
                    newCandidates.Add(new ThiSinhCaThi
                    {
                        MaCaThi = existingCa.MaCaThi,
                        MaHocSinh = cand.MaNguoiDung,
                        TrangThaiDuThi = (ed.Status == "da_ket_thuc" || ed.Status == "dang_thi") ? "duoc_thi" : "cho_thi",
                        GhiChu = "Đủ điều kiện dự thi",
                        NgayTao = DateTime.UtcNow.AddDays(-10)
                    });

                    if (!existingDiemDanhThi.Any(d => d.MaHocSinh == cand.MaNguoiDung))
                    {
                        newExamAttendances.Add(new DiemDanhThi
                        {
                            MaCaThi = existingCa.MaCaThi,
                            MaHocSinh = cand.MaNguoiDung,
                            TrangThaiDiemDanh = "co_mat",
                            ThoiDiemDiemDanh = DateTime.UtcNow.AddDays(-2),
                            GhiChu = "Có mặt đúng giờ",
                            NgayTao = DateTime.UtcNow.AddDays(-2)
                        });
                    }

                    if (ed.Status == "da_ket_thuc" || ed.Status == "dang_thi")
                    {
                        var hasPhien = await db.PhienThiHocSinhs.AnyAsync(p => p.MaDeKiemTra == testDe.MaDeKiemTra && p.MaHocSinh == cand.MaNguoiDung && p.LanThu == 1);
                        if (!hasPhien)
                        {
                            decimal score = (decimal)Math.Round(6.5 + (random.NextDouble() * 3.3), 1);
                            int correct = (int)Math.Round(score * 4); // 40 câu
                            newExamSessions.Add(new PhienThiHocSinh
                            {
                                MaDeKiemTra = testDe.MaDeKiemTra,
                                MaCaThi = existingCa.MaCaThi,
                                MaHocSinh = cand.MaNguoiDung,
                                LanThu = 1,
                                TrangThaiLuong = ed.Status == "da_ket_thuc" ? "da_dung" : "dang_hoat_dong",
                                TrangThaiCongBo = ed.Status == "da_ket_thuc" ? "da_cong_bo" : "chua_co_diem",
                                TrangThaiKyTen = ed.Status == "da_ket_thuc" ? "da_ky" : "chua_ky",
                                BatDauLuc = DateTime.UtcNow.AddDays(-2).AddHours(-2),
                                NopLuc = ed.Status == "da_ket_thuc" ? DateTime.UtcNow.AddDays(-2).AddHours(-1) : null,
                                DiemTuDong = score,
                                DiemCuoiCung = score,
                                SoCauDung = correct
                            });
                        }
                    }
                }
            }

            if (newCandidates.Count > 0) db.ThiSinhCaThis.AddRange(newCandidates);
            if (newExamAttendances.Count > 0) db.DiemDanhThis.AddRange(newExamAttendances);
            if (newExamSessions.Count > 0) db.PhienThiHocSinhs.AddRange(newExamSessions);
            await db.SaveChangesAsync();

            // Vi phạm thi & Biên bản thi cho Ca 01
            if (eIdx == 0)
            {
                var existingViPham = await db.NhatKyViPhamThis.AnyAsync(v => v.MaCaThi == existingCa.MaCaThi);
                if (!existingViPham && examCandidates.Count > 1)
                {
                    db.NhatKyViPhamThis.AddRange(
                        new NhatKyViPhamThi
                        {
                            MaCaThi = existingCa.MaCaThi,
                            MaHocSinh = examCandidates[0].MaNguoiDung,
                            LoaiViPham = "chuyen_tab",
                            MucDo = "nghiem_trong",
                            ChiTietJson = JsonSerializer.Serialize(new { MoTa = "Chuyển tab trình duyệt trong khi làm bài thi trắc nghiệm", GiamThiPhatHien = lecturer.HoTen }),
                            ThoiDiem = DateTime.UtcNow.AddDays(-2).AddMinutes(-45),
                            NgayTao = DateTime.UtcNow.AddDays(-2)
                        },
                        new NhatKyViPhamThi
                        {
                            MaCaThi = existingCa.MaCaThi,
                            MaHocSinh = examCandidates[1].MaNguoiDung,
                            LoaiViPham = "mat_focus",
                            MucDo = "nhac_nho",
                            ChiTietJson = JsonSerializer.Serialize(new { MoTa = "Rời khỏi màn hình làm bài thi 2 lần", CanhBaoHeThong = true }),
                            ThoiDiem = DateTime.UtcNow.AddDays(-2).AddMinutes(-30),
                            NgayTao = DateTime.UtcNow.AddDays(-2)
                        }
                    );

                    db.BienBanThis.Add(new BienBanThi
                    {
                        MaCaThi = existingCa.MaCaThi,
                        LoaiBienBan = "gian_lan",
                        NoiDung = $"Biên bản ca thi: {existingCa.TenCaThi}. Tổng số thí sinh: 22, Có mặt: 22. Số trường hợp vi phạm quy chế: 1 (Cảnh cáo do chuyển tab). Ca thi diễn ra an toàn, nghiêm túc.",
                        MaNguoiLap = lecturerId,
                        ThoiDiemLap = DateTime.UtcNow.AddDays(-2).AddMinutes(-10),
                        TrangThaiXuLy = "da_xu_ly",
                        NgayTao = DateTime.UtcNow.AddDays(-2)
                    });
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}
