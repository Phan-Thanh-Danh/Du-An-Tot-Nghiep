using Backend.Models;
using Backend.Constants;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace Backend.Data.Seeders;

public static class ApplicationTemplateSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (!await context.MauDonTus.AnyAsync())
        {
            Console.WriteLine("Seeding MauDonTus...");
        }
        else
        {
            Console.WriteLine("Updating MauDonTus...");
        }

        Console.WriteLine("Seeding MauDonTus...");

        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var templates = new List<MauDonTu>
        {
            new MauDonTu
            {
                LoaiDon = "phuc_tra_diem",
                TenMau = "Đơn phúc tra điểm",
                PhienBan = 1,
                BatBuocMinhChung = false,
                SoTepToiDa = 0,
                DungLuongTepToiDaByte = 1024,
                TongDungLuongToiDaByte = 1024,
                SlaGio = 48,
                DangHoatDong = true,
                NgayTao = DateTime.UtcNow,
                NgayCapNhat = DateTime.UtcNow,
                CauHinhJson = JsonSerializer.Serialize(new
                {
                    fields = new object[]
                    {
                        new { key = "title", type = "text", label = "Tiêu đề đơn", required = true, maxLength = 200 },
                        new { key = "khoa_hoc_id", type = "number", label = "Khóa học", required = true, relatedEntity = "KhoaHoc" },
                        new { key = "diem_mong_muon", type = "number", label = "Điểm mong muốn", required = true },
                        new { key = "reason", type = "textarea", label = "Lý do phúc tra", required = true, maxLength = 1000 }
                    }
                }, options)
            },
            new MauDonTu
            {
                LoaiDon = "nghi_phep",
                TenMau = "Đơn xin nghỉ phép",
                PhienBan = 1,
                BatBuocMinhChung = true,
                SoTepToiDa = 3,
                DungLuongTepToiDaByte = 5 * 1024 * 1024,
                TongDungLuongToiDaByte = 15 * 1024 * 1024,
                SlaGio = 24,
                DangHoatDong = true,
                NgayTao = DateTime.UtcNow,
                NgayCapNhat = DateTime.UtcNow,
                CauHinhJson = JsonSerializer.Serialize(new
                {
                    fields = new object[]
                    {
                        new { key = "student_info", type = "studentInfo", label = "Thông tin sinh viên", @readonly = true },
                        new { key = "from_date", type = "date", label = "Từ ngày", required = true },
                        new { key = "to_date", type = "date", label = "Đến ngày", required = true },
                        new { key = "reason", type = "textarea", label = "Lý do nghỉ", required = true, maxLength = 1000 },
                        new { key = "contact_address", type = "text", label = "Địa chỉ liên hệ", required = true },
                        new { key = "phone", type = "tel", label = "Số điện thoại", required = true }
                    }
                }, options)
            },
            new MauDonTu
            {
                LoaiDon = "chuyen_co_so",
                TenMau = "Đơn chuyển cơ sở",
                PhienBan = 1,
                BatBuocMinhChung = false,
                SoTepToiDa = 1,
                DungLuongTepToiDaByte = 5 * 1024 * 1024,
                TongDungLuongToiDaByte = 5 * 1024 * 1024,
                SlaGio = 72,
                DangHoatDong = true,
                NgayTao = DateTime.UtcNow,
                NgayCapNhat = DateTime.UtcNow,
                CauHinhJson = JsonSerializer.Serialize(new
                {
                    fields = new object[]
                    {
                        new { key = "student_info", type = "studentInfo", label = "Thông tin sinh viên", @readonly = true },
                        new { key = "ma_don_vi_mong_muon", type = "select", label = "Cơ sở muốn chuyển đến", required = true, autoFill = "campuses" },
                        new { key = "ma_hoc_ky", type = "select", label = "Học kỳ áp dụng", required = true, autoFill = "availableSemesters" },
                        new { key = "ly_do", type = "textarea", label = "Lý do chuyển cơ sở", required = true, maxLength = 1000 },
                        new { key = "dia_chi_lien_he", type = "text", label = "Địa chỉ liên hệ", required = true, maxLength = 200 },
                        new { key = "so_dien_thoai", type = "tel", label = "Số điện thoại", required = true },
                        new { key = "email_lien_he", type = "email", label = "Email liên hệ", required = true, autoFill = "studentEmail" },
                        new { key = "ghi_chu", type = "textarea", label = "Ghi chú", required = false, maxLength = 500 }
                    }
                }, options)
            },
            new MauDonTu
            {
                LoaiDon = "bao_luu",
                TenMau = "Đơn bảo lưu",
                PhienBan = 1,
                BatBuocMinhChung = false,
                SoTepToiDa = 2,
                DungLuongTepToiDaByte = 5 * 1024 * 1024,
                TongDungLuongToiDaByte = 10 * 1024 * 1024,
                SlaGio = 72,
                DangHoatDong = true,
                NgayTao = DateTime.UtcNow,
                NgayCapNhat = DateTime.UtcNow,
                CauHinhJson = JsonSerializer.Serialize(new
                {
                    fields = new object[]
                    {
                        new { key = "student_info", type = "studentInfo", label = "Thông tin sinh viên", @readonly = true },
                        new { 
                            key = "hoc_ky_bao_luu", 
                            type = "select", 
                            label = "Học kỳ bảo lưu", 
                            required = true, 
                            autoFill = "studentSemesters",
                            options = new object[] { new { value = "auto", label = "Đang tải..." } }
                        },
                        new { 
                            key = "reason_type", 
                            type = "select", 
                            label = "Nhóm lý do", 
                            required = true, 
                            options = new object[] {
                                new { value = "Sức khỏe", label = "Sức khỏe" },
                                new { value = "Hoàn cảnh gia đình", label = "Hoàn cảnh gia đình" },
                                new { value = "Nghĩa vụ quân sự", label = "Nghĩa vụ quân sự" },
                                new { value = "Đi làm", label = "Đi làm" },
                                new { value = "Khác", label = "Khác" }
                            }
                        },
                        new { key = "reason_detail", type = "textarea", label = "Lý do bảo lưu", required = true, maxLength = 2000 },
                        new { 
                            key = "thoi_luong_du_kien", 
                            type = "number", 
                            label = "Thời lượng bảo lưu (tháng)", 
                            required = true 
                        },
                        new { key = "dia_chi_lien_he", type = "text", label = "Địa chỉ liên hệ", required = true, maxLength = 200 },
                        new { key = "so_dien_thoai", type = "tel", label = "Số điện thoại", required = true, pattern = "^(0|\\+84)[0-9]{9,10}$" },
                        new { key = "email_lien_he", type = "email", label = "Email liên hệ", required = true, autoFill = "studentEmail" },
                        new { key = "ghi_chu", type = "textarea", label = "Ghi chú", required = false, maxLength = 500 }
                    }
                }, options)
            },
            new MauDonTu
            {
                LoaiDon = "xac_nhan",
                TenMau = "Đơn xác nhận sinh viên",
                PhienBan = 1,
                BatBuocMinhChung = false,
                SoTepToiDa = 2,
                DungLuongTepToiDaByte = 5 * 1024 * 1024,
                TongDungLuongToiDaByte = 10 * 1024 * 1024,
                SlaGio = 48,
                DangHoatDong = true,
                NgayTao = DateTime.UtcNow,
                NgayCapNhat = DateTime.UtcNow,
                CauHinhJson = JsonSerializer.Serialize(new
                {
                    fields = new object[]
                    {
                        new { key = "student_info", type = "studentInfo", label = "Thông tin sinh viên", @readonly = true },
                        new { key = "confirmation_type", type = "select", label = "Loại xác nhận", required = true, options = new[] {
                            new { value = "Xác nhận đang là sinh viên", label = "Xác nhận đang là sinh viên" },
                            new { value = "Xác nhận vay vốn", label = "Xác nhận vay vốn" },
                            new { value = "Xác nhận tạm hoãn nghĩa vụ quân sự", label = "Xác nhận tạm hoãn nghĩa vụ quân sự" },
                            new { value = "Xác nhận làm thủ tục xin việc", label = "Xác nhận làm thủ tục xin việc" },
                            new { value = "Xác nhận hưởng chế độ", label = "Xác nhận hưởng chế độ" },
                            new { value = "Khác", label = "Khác" }
                        } },
                        new { key = "recipient", type = "text", label = "Nơi nhận", required = true, maxLength = 200 },
                        new { key = "purpose", type = "textarea", label = "Mục đích sử dụng", required = true, maxLength = 1000 },
                        new { key = "copies", type = "number", label = "Số lượng bản", required = true, min = 1, max = 5 },
                        new { key = "contact_address", type = "text", label = "Địa chỉ liên hệ", required = true, maxLength = 200 },
                        new { key = "phone_number", type = "tel", label = "Số điện thoại", required = true, pattern = "^(0|\\+84)[0-9]{9,10}$" },
                        new { key = "email", type = "email", label = "Email liên hệ", required = true, autoFill = "studentEmail" }
                    }
                }, options)
            },
            new MauDonTu
            {
                LoaiDon = "chuyen_nganh",
                TenMau = "Đơn chuyển ngành/chuyên ngành",
                PhienBan = 1,
                BatBuocMinhChung = false,
                SoTepToiDa = 2,
                DungLuongTepToiDaByte = 5 * 1024 * 1024,
                TongDungLuongToiDaByte = 10 * 1024 * 1024,
                SlaGio = 72,
                DangHoatDong = true,
                NgayTao = DateTime.UtcNow,
                NgayCapNhat = DateTime.UtcNow,
                CauHinhJson = JsonSerializer.Serialize(new
                {
                    fields = new object[]
                    {
                        new { key = "student_info", type = "studentInfo", label = "Thông tin sinh viên", @readonly = true },
                        new { 
                            key = "target_major_id", 
                            type = "related_entity", 
                            label = "Ngành đào tạo muốn chuyển", 
                            required = true, 
                            autoFill = "majors",
                            relatedEntity = "nganh"
                        },
                        new { 
                            key = "target_specialization_id", 
                            type = "related_entity", 
                            label = "Chuyên ngành muốn chuyển", 
                            required = true, 
                            autoFill = "specializationsByMajor",
                            dependsOn = "target_major_id",
                            relatedEntity = "chuyen_nganh"
                        },
                        new { key = "reason", type = "textarea", label = "Lý do chuyển", required = true, maxLength = 1000 },
                        new { key = "contact_address", type = "text", label = "Địa chỉ liên hệ", required = true, maxLength = 200 },
                        new { key = "phone", type = "tel", label = "Số điện thoại", required = true, pattern = "^(0|\\+84)[0-9]{9,10}$" },
                        new { key = "email", type = "email", label = "Email liên hệ", required = true, autoFill = "studentEmail" }
                    }
                }, options)
            },
            new MauDonTu
            {
                LoaiDon = "thi_lai",
                TenMau = "Đơn đăng ký thi lại",
                PhienBan = 1,
                BatBuocMinhChung = false,
                SoTepToiDa = 1,
                DungLuongTepToiDaByte = 5 * 1024 * 1024,
                TongDungLuongToiDaByte = 5 * 1024 * 1024,
                SlaGio = 72,
                DangHoatDong = true,
                NgayTao = DateTime.UtcNow,
                NgayCapNhat = DateTime.UtcNow,
                CauHinhJson = JsonSerializer.Serialize(new
                {
                    fields = new object[]
                    {
                        new { key = "student_info", type = "studentInfo", label = "Thông tin sinh viên", @readonly = true },
                        new { 
                            key = "subject_id", 
                            type = "related_entity", 
                            label = "Môn học đăng ký thi lại", 
                            required = true, 
                            autoFill = "availableRetakeSubjects",
                            relatedEntity = "mon_hoc"
                        },
                        new { 
                            key = "exam_session_id", 
                            type = "related_entity", 
                            label = "Ca thi", 
                            required = true, 
                            autoFill = "availableExamSessions",
                            dependsOn = "subject_id",
                            relatedEntity = "ca_thi"
                        },
                        new { key = "reason", type = "textarea", label = "Ghi chú", required = false, maxLength = 500 },
                        new { key = "phone", type = "tel", label = "Số điện thoại", required = true, pattern = "^(0|\\+84)[0-9]{9,10}$" },
                        new { key = "email", type = "email", label = "Email liên hệ", required = true, autoFill = "studentEmail" }
                    }
                }, options)
            },
            new MauDonTu
            {
                LoaiDon = ApplicationTypes.GradeAppeal,
                TenMau = "Đơn phúc tra điểm",
                PhienBan = 1,
                BatBuocMinhChung = false,
                SoTepToiDa = 2,
                DungLuongTepToiDaByte = 5 * 1024 * 1024,
                TongDungLuongToiDaByte = 10 * 1024 * 1024,
                SlaGio = 72,
                DangHoatDong = true,
                NgayTao = DateTime.UtcNow,
                NgayCapNhat = DateTime.UtcNow,
                CauHinhJson = JsonSerializer.Serialize(new
                {
                    fields = new object[]
                    {
                        new { key = "student_info", type = "studentInfo", label = "Thông tin sinh viên", @readonly = true },
                        new { 
                            key = "ma_diem_so", 
                            type = "select", 
                            label = "Môn học cần phúc tra", 
                            required = true, 
                            autoFill = "availableRegradeScores"
                        },
                        new { 
                            key = "cot_diem", 
                            type = "select", 
                            label = "Thành phần điểm cần phúc tra", 
                            required = true, 
                            options = new object[] {
                                new { value = "diem_qua_trinh", label = "Điểm quá trình" },
                                new { value = "diem_giua_ky", label = "Điểm giữa kỳ" },
                                new { value = "diem_cuoi_ky", label = "Điểm cuối kỳ" },
                                new { value = "gpa_mon_hoc", label = "Điểm tổng kết" }
                            }
                        },
                        new { key = "diem_hien_tai", type = "number", label = "Điểm hiện tại", @readonly = true },
                        new { key = "ly_do", type = "textarea", label = "Lý do phúc tra", required = true, maxLength = 1000 },
                        new { key = "so_dien_thoai", type = "tel", label = "Số điện thoại", required = true, pattern = "^(0|\\+84)[0-9]{9,10}$" },
                        new { key = "email_lien_he", type = "email", label = "Email liên hệ", required = true, autoFill = "studentEmail" }
                    }
                }, options)
            },
            new MauDonTu
            {
                LoaiDon = ApplicationTypes.Withdrawal,
                TenMau = "Đơn xin rút học bạ",
                PhienBan = 1,
                BatBuocMinhChung = false,
                SoTepToiDa = 3,
                DungLuongTepToiDaByte = 5 * 1024 * 1024,
                TongDungLuongToiDaByte = 15 * 1024 * 1024,
                SlaGio = 72,
                DangHoatDong = true,
                NgayTao = DateTime.UtcNow,
                NgayCapNhat = DateTime.UtcNow,
                CauHinhJson = JsonSerializer.Serialize(new
                {
                    fields = new object[]
                    {
                        new { key = "student_info", type = "studentInfo", label = "Thông tin sinh viên", @readonly = true },
                        new { key = "reason", type = "textarea", label = "Lý do xin rút học bạ", required = true, maxLength = 1000 },
                        new { 
                            key = "documents", 
                            type = "multiselect", 
                            label = "Hồ sơ đề nghị nhận lại", 
                            required = true,
                            options = new object[] {
                                new { value = "hoc_ba_thpt", label = "Học bạ THPT" },
                                new { value = "bang_tot_nghiep_thpt", label = "Bằng tốt nghiệp THPT" },
                                new { value = "giay_cn_tot_nghiep_tam_thoi", label = "Giấy chứng nhận tốt nghiệp tạm thời" },
                                new { value = "ho_so_khac", label = "Hồ sơ khác" }
                            }
                        },
                        new { 
                            key = "receiver_type", 
                            type = "select", 
                            label = "Người nhận hồ sơ", 
                            required = true,
                            options = new object[] {
                                new { value = "sinh_vien", label = "Sinh viên nhận trực tiếp" },
                                new { value = "nguoi_duoc_uy_quyen", label = "Người được ủy quyền nhận" }
                            }
                        },
                        new { key = "authorized_name", type = "text", label = "Họ tên người nhận (nếu ủy quyền)", required = false, dependsOn = "receiver_type", dependsOnValue = "nguoi_duoc_uy_quyen" },
                        new { key = "authorized_cccd", type = "text", label = "CCCD người nhận (nếu ủy quyền)", required = false, dependsOn = "receiver_type", dependsOnValue = "nguoi_duoc_uy_quyen" },
                        new { key = "authorized_relation", type = "text", label = "Quan hệ với sinh viên (nếu ủy quyền)", required = false, dependsOn = "receiver_type", dependsOnValue = "nguoi_duoc_uy_quyen" },
                        new { key = "authorized_phone", type = "tel", label = "Số điện thoại người nhận (nếu ủy quyền)", required = false, dependsOn = "receiver_type", dependsOnValue = "nguoi_duoc_uy_quyen" },
                        new { key = "contact_address", type = "text", label = "Địa chỉ liên hệ", required = true },
                        new { key = "contact_phone", type = "tel", label = "Số điện thoại", required = true },
                        new { key = "contact_email", type = "email", label = "Email", required = true, autoFill = "studentEmail" }
                    }
                }, options)
            }
        };

        foreach (var t in templates)
        {
            var existing = await context.MauDonTus.FirstOrDefaultAsync(m => m.LoaiDon == t.LoaiDon);
            if (existing != null)
            {
                existing.TenMau = t.TenMau;
                existing.PhienBan = t.PhienBan;
                existing.BatBuocMinhChung = t.BatBuocMinhChung;
                existing.SoTepToiDa = t.SoTepToiDa;
                existing.DungLuongTepToiDaByte = t.DungLuongTepToiDaByte;
                existing.TongDungLuongToiDaByte = t.TongDungLuongToiDaByte;
                existing.SlaGio = t.SlaGio;
                existing.DangHoatDong = t.DangHoatDong;
                existing.CauHinhJson = t.CauHinhJson;
                existing.NgayCapNhat = DateTime.UtcNow;
            }
            else
            {
                context.MauDonTus.Add(t);
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine("MauDonTus seeded/updated successfully.");
    }
}
