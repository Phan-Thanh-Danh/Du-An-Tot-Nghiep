using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Seeders;

public static class PermissionCatalogSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // 1. Ensure Table exists & Seed Permissions Catalog
        var catalog = new List<(string Code, string Name, string Module, string Action, string Desc)>
        {
            // Training
            ("training.read", "Xem chương trình đào tạo & môn học", "training", "read", "Cho phép xem danh sách ngành, chuyên ngành, môn học và khung chương trình"),
            ("training.create", "Tạo mới môn học & khung chương trình", "training", "create", "Cho phép tạo môn học mới, thiết lập khung chương trình"),
            ("training.update", "Chỉnh sửa môn học & chương trình", "training", "update", "Cho phép chỉnh sửa thông tin môn học, số tín chỉ, điều kiện tiên quyết"),
            ("training.delete", "Xóa / Tạm ngừng môn học", "training", "delete", "Cho phép xóa hoặc chuyển trạng thái tạm ngừng môn học"),
            ("training.manage_curriculum", "Phê duyệt & quản lý đề cương", "training", "approve", "Cho phép ban giám hiệu duyệt và ban hành khung chương trình chuẩn"),

            // Schedules
            ("schedules.read", "Xem thời khóa biểu & lịch học", "schedules", "read", "Cho phép xem lịch học, lịch dạy và phòng học"),
            ("schedules.create", "Xếp lịch & tạo thời khóa biểu", "schedules", "create", "Cho phép tạo mới bộ thời khóa biểu, xếp lịch học cho các lớp"),
            ("schedules.update", "Điều chỉnh & đổi ca học", "schedules", "update", "Cho phép đổi phòng, dời ca học, sắp xếp dạy bù"),
            ("schedules.delete", "Hủy bộ thời khóa biểu", "schedules", "delete", "Cho phép xóa hoặc hủy bản nháp thời khóa biểu"),
            ("schedules.approve", "Phê duyệt công bố thời khóa biểu", "schedules", "approve", "Cho phép BGH phê duyệt và xuất bản TKB chính thức cho sinh viên/giảng viên"),

            // Exams & Grades
            ("exams.read", "Xem bảng điểm & lịch thi", "exams", "read", "Cho phép xem điểm số của các lớp, ca thi và đề thi"),
            ("exams.create", "Tạo ca thi & ngân hàng đề", "exams", "create", "Cho phép tạo ca thi mới, tạo đề thi trắc nghiệm/tự luận"),
            ("exams.update", "Nhập & cập nhật điểm số", "exams", "update", "Cho phép giảng viên nhập điểm quá trình, điểm thi và nhận xét"),
            ("exams.delete", "Hủy ca thi & xóa đề thi", "exams", "delete", "Cho phép hủy ca thi hoặc xóa câu hỏi ngân hàng đề"),
            ("exams.grade", "Chấm bài thi & tổng kết GPA", "exams", "update", "Cho phép chấm bài nộp, chốt điểm môn và tính GPA học phần"),
            ("exams.unlock_grade", "Phê duyệt mở khóa bảng điểm", "exams", "approve", "Cho phép BGH phê duyệt yêu cầu sửa điểm sau khi bảng điểm đã khóa"),

            // Requests & Applications
            ("requests.read", "Xem danh sách đơn từ", "requests", "read", "Cho phép xem đơn xin nghỉ, đơn thi lại, đơn phúc khảo của sinh viên"),
            ("requests.create", "Tạo mẫu đơn & gửi yêu cầu", "requests", "create", "Cho phép gửi đơn từ hoặc cấu hình mẫu đơn mới"),
            ("requests.update", "Tiếp nhận & xử lý đơn", "requests", "update", "Cho phép giáo vụ tiếp nhận, yêu cầu bổ sung minh chứng"),
            ("requests.delete", "Hủy bỏ đơn từ", "requests", "delete", "Cho phép xóa đơn từ không hợp lệ hoặc hủy mẫu đơn"),
            ("requests.process", "Phê duyệt / Từ chối đơn từ", "requests", "approve", "Cho phép ra quyết định chấp thuận hoặc từ chối đơn từ học vụ"),

            // Accounts & RBAC
            ("accounts.read", "Xem Tài Khoản & Phân Quyền", "accounts", "read", "Cho phép xem danh sách người dùng, vai trò và phân quyền"),
            ("accounts.create", "Tạo Mới Tài Khoản", "accounts", "create", "Cho phép tạo mới tài khoản người dùng và vai trò"),
            ("accounts.update", "Chỉnh Sửa Tài Khoản", "accounts", "update", "Cho phép cập nhật thông tin tài khoản, phân quyền"),
            ("accounts.delete", "Xóa Tài Khoản", "accounts", "delete", "Cho phép vô hiệu hóa hoặc xóa tài khoản"),

            // Campus
            ("campus.read", "Xem Cơ Sở (Campus)", "campus", "read", "Cho phép xem danh sách cơ sở, phòng học, trang thiết bị"),
            ("campus.create", "Tạo Mới Cơ Sở", "campus", "create", "Cho phép tạo mới cơ sở, chi nhánh, phòng ban"),
            ("campus.update", "Chỉnh Sửa Cơ Sở", "campus", "update", "Cho phép cập nhật thông tin cơ sở, cơ sở vật chất"),
            ("campus.delete", "Xóa Cơ Sở", "campus", "delete", "Cho phép đóng cửa hoặc xóa bỏ cơ sở, phòng học"),

            // Finance
            ("finance.read", "Xem Tài Chính & Học Phí", "finance", "read", "Cho phép xem các khoản thu học phí, công nợ, học bổng"),
            ("finance.create", "Tạo Giao Dịch Tài Chính", "finance", "create", "Cho phép lập hóa đơn, tạo khoản thu, biên lai thu tiền"),
            ("finance.update", "Chỉnh Sửa Giao Dịch", "finance", "update", "Cho phép cập nhật trạng thái thanh toán, điều chỉnh công nợ"),
            ("finance.delete", "Xóa / Hủy Giao Dịch", "finance", "delete", "Cho phép hủy giao dịch sai, hoàn trả học phí"),

            // Reports & Analytics
            ("reports.read", "Xem báo cáo học vụ & GPA", "reports", "read", "Cho phép xem biểu đồ GPA, tỷ lệ đạt/rớt, chuyên cần toàn trường"),
            ("reports.export", "Xuất dữ liệu báo cáo (Excel/PDF)", "reports", "export", "Cho phép xuất danh sách điểm, danh sách sinh viên ra file"),
            ("reports.ai_analysis", "Xem phân tích AI & cảnh báo At-Risk", "reports", "approve", "Cho phép xem phân tích cảm xúc phản hồi sinh viên và danh sách nguy cơ")
        };

        var existingPerms = await context.QuyenHans.ToDictionaryAsync(p => p.MaCode, p => p);
        foreach (var item in catalog)
        {
            if (!existingPerms.TryGetValue(item.Code, out var perm))
            {
                perm = new QuyenHan
                {
                    MaCode = item.Code,
                    TenQuyenHan = item.Name,
                    Module = item.Module,
                    Action = item.Action,
                    MoTa = item.Desc
                };
                context.QuyenHans.Add(perm);
            }
            else
            {
                perm.TenQuyenHan = item.Name;
                perm.Module = item.Module;
                perm.Action = item.Action;
                perm.MoTa = item.Desc;
            }
        }
        await context.SaveChangesAsync();

        // Refresh perms dict
        var allPerms = await context.QuyenHans.ToDictionaryAsync(p => p.MaCode, p => p.MaQuyenHan);

        // 2. Default Roles & Permissions Mapping
        var defaultRolePerms = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["sieu_quan_tri"] = allPerms.Keys.ToList(),
            ["quan_tri"] = allPerms.Keys.ToList(),
            ["quan_tri_co_so"] = allPerms.Keys.ToList(),
            ["hieu_truong"] = new List<string>
            {
                "training.read", "training.manage_curriculum",
                "schedules.read", "schedules.approve",
                "exams.read", "exams.unlock_grade",
                "requests.read", "requests.process",
                "reports.read", "reports.export", "reports.ai_analysis"
            },
            ["nhan_vien"] = new List<string>
            {
                "training.read", "training.create", "training.update",
                "schedules.read", "schedules.create", "schedules.update",
                "exams.read", "exams.create",
                "requests.read", "requests.update", "requests.process",
                "reports.read", "reports.export"
            },
            ["giao_vien"] = new List<string>
            {
                "training.read",
                "schedules.read",
                "exams.read", "exams.update", "exams.grade",
                "requests.read", "requests.update", "requests.create",
                "reports.read"
            },
            ["hoc_sinh"] = new List<string>
            {
                "training.read",
                "schedules.read",
                "exams.read",
                "requests.read", "requests.create"
            },
            ["phu_huynh"] = new List<string>
            {
                "training.read",
                "schedules.read",
                "exams.read",
                "reports.read"
            }
        };

        var roles = await context.VaiTros.ToListAsync();
        var existingRolePerms = await context.VaiTroQuyenHans.ToListAsync();
        var existingSet = new HashSet<(int RoleId, int PermId)>(existingRolePerms.Select(x => (x.MaVaiTro, x.MaQuyenHan)));

        foreach (var role in roles)
        {
            if (defaultRolePerms.TryGetValue(role.MaCodeVaiTro, out var permCodes))
            {
                foreach (var code in permCodes)
                {
                    if (allPerms.TryGetValue(code, out var permId) && !existingSet.Contains((role.MaVaiTro, permId)))
                    {
                        context.VaiTroQuyenHans.Add(new VaiTroQuyenHan
                        {
                            MaVaiTro = role.MaVaiTro,
                            MaQuyenHan = permId,
                            NgayCap = DateTime.UtcNow
                        });
                        existingSet.Add((role.MaVaiTro, permId));
                    }
                }
            }
        }

        await context.SaveChangesAsync();
    }
}
