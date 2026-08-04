using Backend.Models;
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
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
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
                        new { key = "title", type = "text", label = "Tiêu đề đơn", required = true, maxLength = 200 },
                        new { key = "tu_ngay", type = "date", label = "Từ ngày", required = true },
                        new { key = "den_ngay", type = "date", label = "Đến ngày", required = true },
                        new { key = "reason", type = "textarea", label = "Lý do nghỉ phép", required = true, maxLength = 1000 }
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
                        new { key = "title", type = "text", label = "Tiêu đề đơn", required = true, maxLength = 200 },
                        new { key = "co_so_hien_tai", type = "text", label = "Cơ sở hiện tại", required = true, maxLength = 200 },
                        new { key = "co_so_muon_chuyen", type = "text", label = "Cơ sở muốn chuyển tới", required = true, maxLength = 200 },
                        new { key = "reason", type = "textarea", label = "Lý do chuyển", required = true, maxLength = 1000 }
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
