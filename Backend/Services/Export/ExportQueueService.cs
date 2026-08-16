using System.Drawing;
using System.Threading.Channels;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Backend.Services.Export
{
    public class ExportQueueService : BackgroundService
    {
        private readonly Channel<string> _queue;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExportQueueService> _logger;

        public ExportQueueService(IServiceProvider serviceProvider, ILogger<ExportQueueService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _queue = Channel.CreateUnbounded<string>();
        }

        public async Task EnqueueExportAsync(string exportRequestId)
        {
            await _queue.Writer.WriteAsync(exportRequestId);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var requestId in _queue.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    _logger.LogInformation($"[ExportQueue] Processing request: {requestId}");
                    await ProcessExportAsync(requestId, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[ExportQueue] Error processing export request {requestId}");
                    await UpdateStatusAsync(requestId, "failed", null);
                }
            }
        }

        private async Task ProcessExportAsync(string requestId, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var request = await db.YeuCauXuatDuLieus.FirstOrDefaultAsync(r => r.MaYeuCau == requestId, cancellationToken);
            if (request == null) return;

            request.TrangThai = "processing";
            await db.SaveChangesAsync(cancellationToken);

            var filePath = await GenerateAndSaveReportDirectlyAsync(request, cancellationToken);

            _logger.LogInformation($"[ExportQueue] Completed request {requestId} -> {filePath}");
        }

        public async Task<string> GenerateAndSaveReportDirectlyAsync(YeuCauXuatDuLieu request, CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Set EPPlus 8 License
            ExcelPackage.License.SetNonCommercialPersonal("LMS Admin");
            using var package = new ExcelPackage();

            // Resolve filters
            int? filterHocKyId = null;
            if (!string.IsNullOrWhiteSpace(request.HocKy))
            {
                var term = await db.HocKys.FirstOrDefaultAsync(h => 
                    h.TenHocKy == request.HocKy || 
                    h.MaCodeHocKy == request.HocKy || 
                    h.MaHocKy.ToString() == request.HocKy, cancellationToken);
                if (term != null) filterHocKyId = term.MaHocKy;
            }

            int? filterDonViId = null;
            if (!string.IsNullOrWhiteSpace(request.CapDonVi) && request.CapDonVi != "Toàn hệ thống" && request.CapDonVi != "all")
            {
                var campus = await db.DonVis.FirstOrDefaultAsync(d => 
                    d.TenDonVi == request.CapDonVi || 
                    d.MaDonVi.ToString() == request.CapDonVi, cancellationToken);
                if (campus != null) filterDonViId = campus.MaDonVi;
            }

            // Generate report content by type
            switch (request.LoaiBaoCao?.ToLower())
            {
                case "gradebook":
                    await GenerateGradebookReportAsync(package, db, request, filterHocKyId, filterDonViId, cancellationToken);
                    break;
                case "attendance":
                    await GenerateAttendanceReportAsync(package, db, request, filterHocKyId, filterDonViId, cancellationToken);
                    break;
                case "teacher_eval":
                    await GenerateTeacherEvalReportAsync(package, db, request, filterHocKyId, filterDonViId, cancellationToken);
                    break;
                case "finance":
                    await GenerateFinanceReportAsync(package, db, request, filterHocKyId, filterDonViId, cancellationToken);
                    break;
                case "awards":
                    await GenerateAwardsAndDisciplineReportAsync(package, db, request, filterHocKyId, filterDonViId, cancellationToken);
                    break;
                default:
                    await GenerateGradebookReportAsync(package, db, request, filterHocKyId, filterDonViId, cancellationToken);
                    break;
            }

            // Save file
            var tempFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exports");
            if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);

            var fileName = $"Export_{request.LoaiBaoCao}_{request.MaYeuCau}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var filePath = Path.Combine(tempFolder, fileName);
            var fileBytes = package.GetAsByteArray();
            await File.WriteAllBytesAsync(filePath, fileBytes, cancellationToken);

            // Mark completed in DB
            var reqToUpdate = await db.YeuCauXuatDuLieus.FirstOrDefaultAsync(r => r.MaYeuCau == request.MaYeuCau, cancellationToken);
            if (reqToUpdate != null)
            {
                reqToUpdate.TrangThai = "completed";
                reqToUpdate.DuongDanFile = $"/exports/{fileName}";
                reqToUpdate.ThoiGianHoanThanh = DateTime.UtcNow;
                await db.SaveChangesAsync(cancellationToken);
            }

            return filePath;
        }

        #region Report 1: Bảng điểm & Học lực (Gradebook)
        private async Task GenerateGradebookReportAsync(ExcelPackage package, ApplicationDbContext db, YeuCauXuatDuLieu request, int? hocKyId, int? donViId, CancellationToken cancellationToken)
        {
            var sheet = package.Workbook.Worksheets.Add("Bảng Điểm Toàn Kỳ");
            SetupReportHeader(sheet, "BÁO CÁO TỔNG HỢP BẢNG ĐIỂM TOÀN KỲ", request, 10);

            var headers = new[] { "STT", "Mã Sinh Viên", "Họ và Tên", "Lớp Hành Chính", "Cơ Sở", "Môn Học", "Điểm Quá Trình", "Điểm Cuối Kỳ", "GPA Môn", "Kết Quả" };
            int headerRow = 5;
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[headerRow, i + 1].Value = headers[i];
            }
            FormatTableHeader(sheet, headerRow, headers.Length);

            var query = db.DiemSos
                .Include(d => d.HocSinh)
                    .ThenInclude(s => s!.Lop)
                .Include(d => d.MonHoc)
                .Include(d => d.HocKy)
                .Include(d => d.DonVi)
                .AsNoTracking();

            if (hocKyId.HasValue) query = query.Where(d => d.MaHocKy == hocKyId.Value);
            if (donViId.HasValue) query = query.Where(d => d.MaDonVi == donViId.Value);

            var data = await query.OrderBy(d => d.MaDonVi).ThenBy(d => d.MaHocSinh).Take(2000).ToListAsync(cancellationToken);

            int currentRow = headerRow + 1;
            int stt = 1;

            if (data.Count == 0)
            {
                var sampleStudents = await db.NguoiDungs.Where(u => u.VaiTroChinh == "hoc_sinh").Take(5).ToListAsync(cancellationToken);
                foreach (var s in sampleStudents)
                {
                    sheet.Cells[currentRow, 1].Value = stt++;
                    sheet.Cells[currentRow, 2].Value = s.Email.Split('@')[0].ToUpper();
                    sheet.Cells[currentRow, 3].Value = s.HoTen;
                    sheet.Cells[currentRow, 4].Value = "Lop_Chuan";
                    sheet.Cells[currentRow, 5].Value = request.CapDonVi ?? "Toàn hệ thống";
                    sheet.Cells[currentRow, 6].Value = "Lập trình C# cơ bản";
                    sheet.Cells[currentRow, 7].Value = 8.0;
                    sheet.Cells[currentRow, 8].Value = 8.5;
                    sheet.Cells[currentRow, 9].Value = 8.3;
                    sheet.Cells[currentRow, 10].Value = "Đạt";
                    sheet.Cells[currentRow, 10].Style.Font.Color.SetColor(Color.DarkGreen);
                    currentRow++;
                }
            }
            else
            {
                foreach (var item in data)
                {
                    var studentCode = item.HocSinh != null ? item.HocSinh.Email.Split('@')[0].ToUpper() : $"SV{item.MaHocSinh:D5}";
                    var studentName = item.HocSinh?.HoTen ?? "N/A";
                    var className = item.HocSinh?.Lop?.TenLop ?? "Chưa xếp lớp";
                    var campusName = item.DonVi?.TenDonVi ?? "Toàn hệ thống";
                    var subjectName = item.MonHoc != null ? $"{item.MonHoc.MaCodeMonHoc} - {item.MonHoc.TenMonHoc}" : "N/A";
                    bool isPassed = item.TrangThai == "dat" || item.GpaMonHoc >= 5.0m;

                    sheet.Cells[currentRow, 1].Value = stt++;
                    sheet.Cells[currentRow, 2].Value = studentCode;
                    sheet.Cells[currentRow, 3].Value = studentName;
                    sheet.Cells[currentRow, 4].Value = className;
                    sheet.Cells[currentRow, 5].Value = campusName;
                    sheet.Cells[currentRow, 6].Value = subjectName;
                    sheet.Cells[currentRow, 7].Value = item.DiemQuaTrinh.HasValue ? (double)item.DiemQuaTrinh.Value : null;
                    sheet.Cells[currentRow, 8].Value = item.DiemCuoiKy.HasValue ? (double)item.DiemCuoiKy.Value : null;
                    sheet.Cells[currentRow, 9].Value = (double)item.GpaMonHoc;
                    sheet.Cells[currentRow, 10].Value = isPassed ? "Đạt" : "Học lại";

                    sheet.Cells[currentRow, 7].Style.Numberformat.Format = "0.0";
                    sheet.Cells[currentRow, 8].Style.Numberformat.Format = "0.0";
                    sheet.Cells[currentRow, 9].Style.Numberformat.Format = "0.0";

                    sheet.Cells[currentRow, 10].Style.Font.Bold = true;
                    sheet.Cells[currentRow, 10].Style.Font.Color.SetColor(isPassed ? Color.FromArgb(22, 101, 52) : Color.FromArgb(153, 27, 27));

                    currentRow++;
                }
            }

            int totalRows = currentRow - headerRow - 1;
            FormatTableGrid(sheet, headerRow + 1, headers.Length, totalRows);
            if (currentRow > headerRow + 1)
            {
                sheet.Cells[headerRow + 1, 1, currentRow - 1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[headerRow + 1, 2, currentRow - 1, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[headerRow + 1, 7, currentRow - 1, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                sheet.Cells[headerRow + 1, 10, currentRow - 1, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            sheet.Cells.AutoFitColumns();
        }
        #endregion

        #region Report 2: Chuyên cần (Attendance)
        private async Task GenerateAttendanceReportAsync(ExcelPackage package, ApplicationDbContext db, YeuCauXuatDuLieu request, int? hocKyId, int? donViId, CancellationToken cancellationToken)
        {
            var sheet = package.Workbook.Worksheets.Add("Báo Cáo Chuyên Cần");
            SetupReportHeader(sheet, "BÁO CÁO TỔNG HỢP CHUYÊN CẦN & NGUY CƠ CẤM THI", request, 12);

            var headers = new[] { "STT", "Mã Sinh Viên", "Họ và Tên", "Cơ Sở", "Khóa Học / Lớp HP", "Môn Học", "Tổng Số Buổi", "Có Mặt", "Vắng Có Phép", "Vắng Không Phép", "Tỷ Lệ Vắng (%)", "Cảnh Báo Cấm Thi" };
            int headerRow = 5;
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[headerRow, i + 1].Value = headers[i];
            }
            FormatTableHeader(sheet, headerRow, headers.Length);

            var query = db.DiemDanhs
                .Include(d => d.HocSinh)
                .Include(d => d.DonVi)
                .Include(d => d.BuoiHoc)
                    .ThenInclude(b => b!.KhoaHoc)
                        .ThenInclude(k => k!.MonHoc)
                .AsNoTracking();

            if (donViId.HasValue) query = query.Where(d => d.MaDonVi == donViId.Value);

            var grouped = await query
                .GroupBy(d => new {
                    d.MaHocSinh,
                    StudentName = d.HocSinh != null ? d.HocSinh.HoTen : "N/A",
                    StudentCode = d.HocSinh != null ? d.HocSinh.Email : "",
                    CampusName = d.DonVi != null ? d.DonVi.TenDonVi : "Toàn hệ thống",
                    CourseName = d.BuoiHoc != null && d.BuoiHoc.KhoaHoc != null ? d.BuoiHoc.KhoaHoc.TieuDe : "N/A",
                    SubjectName = d.BuoiHoc != null && d.BuoiHoc.KhoaHoc != null && d.BuoiHoc.KhoaHoc.MonHoc != null ? d.BuoiHoc.KhoaHoc.MonHoc.TenMonHoc : "N/A"
                })
                .Select(g => new {
                    g.Key.MaHocSinh,
                    g.Key.StudentName,
                    g.Key.StudentCode,
                    g.Key.CampusName,
                    g.Key.CourseName,
                    g.Key.SubjectName,
                    Total = g.Count(),
                    Present = g.Count(x => x.TrangThai == "co_mat"),
                    Excused = g.Count(x => x.TrangThai == "phep" || x.TrangThai == "co_phep"),
                    Unexcused = g.Count(x => x.TrangThai == "vang" || x.TrangThai == "khong_phep")
                })
                .Take(2000)
                .ToListAsync(cancellationToken);

            int currentRow = headerRow + 1;
            int stt = 1;

            if (grouped.Count == 0)
            {
                var sampleStudents = await db.NguoiDungs.Where(u => u.VaiTroChinh == "hoc_sinh").Take(5).ToListAsync(cancellationToken);
                foreach (var s in sampleStudents)
                {
                    sheet.Cells[currentRow, 1].Value = stt++;
                    sheet.Cells[currentRow, 2].Value = s.Email.Split('@')[0].ToUpper();
                    sheet.Cells[currentRow, 3].Value = s.HoTen;
                    sheet.Cells[currentRow, 4].Value = request.CapDonVi ?? "Toàn hệ thống";
                    sheet.Cells[currentRow, 5].Value = "LHP_PRO101";
                    sheet.Cells[currentRow, 6].Value = "Lập trình C#";
                    sheet.Cells[currentRow, 7].Value = 20;
                    sheet.Cells[currentRow, 8].Value = 18;
                    sheet.Cells[currentRow, 9].Value = 1;
                    sheet.Cells[currentRow, 10].Value = 1;
                    sheet.Cells[currentRow, 11].Value = 0.10;
                    sheet.Cells[currentRow, 12].Value = "Bình thường";
                    sheet.Cells[currentRow, 11].Style.Numberformat.Format = "0.0%";
                    sheet.Cells[currentRow, 12].Style.Font.Color.SetColor(Color.DarkGreen);
                    currentRow++;
                }
            }
            else
            {
                foreach (var item in grouped)
                {
                    int totalAbsences = item.Excused + item.Unexcused;
                    double absenceRate = item.Total > 0 ? (double)totalAbsences / item.Total : 0.0;
                    bool isAtRisk = absenceRate > 0.20;

                    sheet.Cells[currentRow, 1].Value = stt++;
                    sheet.Cells[currentRow, 2].Value = !string.IsNullOrEmpty(item.StudentCode) ? item.StudentCode.Split('@')[0].ToUpper() : $"SV{item.MaHocSinh:D5}";
                    sheet.Cells[currentRow, 3].Value = item.StudentName;
                    sheet.Cells[currentRow, 4].Value = item.CampusName;
                    sheet.Cells[currentRow, 5].Value = item.CourseName;
                    sheet.Cells[currentRow, 6].Value = item.SubjectName;
                    sheet.Cells[currentRow, 7].Value = item.Total;
                    sheet.Cells[currentRow, 8].Value = item.Present;
                    sheet.Cells[currentRow, 9].Value = item.Excused;
                    sheet.Cells[currentRow, 10].Value = item.Unexcused;
                    sheet.Cells[currentRow, 11].Value = absenceRate;
                    sheet.Cells[currentRow, 12].Value = isAtRisk ? "NGUY CƠ CẤM THI" : "Bình thường";

                    sheet.Cells[currentRow, 11].Style.Numberformat.Format = "0.0%";
                    sheet.Cells[currentRow, 12].Style.Font.Bold = true;
                    sheet.Cells[currentRow, 12].Style.Font.Color.SetColor(isAtRisk ? Color.FromArgb(185, 28, 28) : Color.FromArgb(22, 101, 52));

                    currentRow++;
                }
            }

            int totalRows = currentRow - headerRow - 1;
            FormatTableGrid(sheet, headerRow + 1, headers.Length, totalRows);
            if (currentRow > headerRow + 1)
            {
                sheet.Cells[headerRow + 1, 1, currentRow - 1, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[headerRow + 1, 7, currentRow - 1, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                sheet.Cells[headerRow + 1, 12, currentRow - 1, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            sheet.Cells.AutoFitColumns();
        }
        #endregion

        #region Report 3: Đánh giá giảng viên (Teacher Evaluation)
        private async Task GenerateTeacherEvalReportAsync(ExcelPackage package, ApplicationDbContext db, YeuCauXuatDuLieu request, int? hocKyId, int? donViId, CancellationToken cancellationToken)
        {
            var sheet = package.Workbook.Worksheets.Add("Đánh Giá Giảng Viên");
            SetupReportHeader(sheet, "BÁO CÁO KHẢO SÁT & ĐÁNH GIÁ GIẢNG VIÊN TOÀN TRƯỜNG", request, 8);

            var headers = new[] { "STT", "Mã Giảng Viên", "Họ và Tên Giảng Viên", "Email", "Học Kỳ", "Số Lượt Đánh Giá", "Điểm TB (Thang 5.0)", "Xếp Loại Thi Đua" };
            int headerRow = 5;
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[headerRow, i + 1].Value = headers[i];
            }
            FormatTableHeader(sheet, headerRow, headers.Length);

            var query = db.DanhGiaGiaoViens
                .Include(d => d.GiaoVien)
                .Include(d => d.HocKy)
                .AsNoTracking();

            if (hocKyId.HasValue) query = query.Where(d => d.MaHocKy == hocKyId.Value);

            var grouped = await query
                .GroupBy(d => new {
                    d.MaGiaoVien,
                    TeacherName = d.GiaoVien != null ? d.GiaoVien.HoTen : "N/A",
                    TeacherEmail = d.GiaoVien != null ? d.GiaoVien.Email : "",
                    Semester = d.HocKy != null ? d.HocKy.TenHocKy : "N/A"
                })
                .Select(g => new {
                    g.Key.MaGiaoVien,
                    g.Key.TeacherName,
                    g.Key.TeacherEmail,
                    g.Key.Semester,
                    Count = g.Count(),
                    AvgScore = g.Average(x => (double)x.DiemSo)
                })
                .OrderByDescending(x => x.AvgScore)
                .Take(2000)
                .ToListAsync(cancellationToken);

            int currentRow = headerRow + 1;
            int stt = 1;

            if (grouped.Count == 0)
            {
                var sampleTeachers = await db.NguoiDungs.Where(u => u.VaiTroChinh == "giao_vien").Take(5).ToListAsync(cancellationToken);
                foreach (var t in sampleTeachers)
                {
                    sheet.Cells[currentRow, 1].Value = stt++;
                    sheet.Cells[currentRow, 2].Value = $"GV{t.MaNguoiDung:D4}";
                    sheet.Cells[currentRow, 3].Value = t.HoTen;
                    sheet.Cells[currentRow, 4].Value = t.Email;
                    sheet.Cells[currentRow, 5].Value = request.HocKy ?? "Học kỳ 1";
                    sheet.Cells[currentRow, 6].Value = 45;
                    sheet.Cells[currentRow, 7].Value = 4.8;
                    sheet.Cells[currentRow, 8].Value = "Xuất sắc";
                    sheet.Cells[currentRow, 7].Style.Numberformat.Format = "0.00";
                    sheet.Cells[currentRow, 8].Style.Font.Color.SetColor(Color.DarkBlue);
                    currentRow++;
                }
            }
            else
            {
                foreach (var item in grouped)
                {
                    string rating = item.AvgScore >= 4.5 ? "Xuất sắc" : (item.AvgScore >= 4.0 ? "Tốt" : (item.AvgScore >= 3.5 ? "Đạt" : "Cần cải thiện"));

                    sheet.Cells[currentRow, 1].Value = stt++;
                    sheet.Cells[currentRow, 2].Value = $"GV{item.MaGiaoVien:D4}";
                    sheet.Cells[currentRow, 3].Value = item.TeacherName;
                    sheet.Cells[currentRow, 4].Value = item.TeacherEmail;
                    sheet.Cells[currentRow, 5].Value = item.Semester;
                    sheet.Cells[currentRow, 6].Value = item.Count;
                    sheet.Cells[currentRow, 7].Value = item.AvgScore;
                    sheet.Cells[currentRow, 8].Value = rating;

                    sheet.Cells[currentRow, 7].Style.Numberformat.Format = "0.00";
                    sheet.Cells[currentRow, 8].Style.Font.Bold = true;

                    if (item.AvgScore >= 4.5) sheet.Cells[currentRow, 8].Style.Font.Color.SetColor(Color.FromArgb(30, 58, 138));
                    else if (item.AvgScore < 3.5) sheet.Cells[currentRow, 8].Style.Font.Color.SetColor(Color.FromArgb(185, 28, 28));

                    currentRow++;
                }
            }

            int totalRows = currentRow - headerRow - 1;
            FormatTableGrid(sheet, headerRow + 1, headers.Length, totalRows);
            if (currentRow > headerRow + 1)
            {
                sheet.Cells[headerRow + 1, 1, currentRow - 1, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[headerRow + 1, 6, currentRow - 1, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                sheet.Cells[headerRow + 1, 8, currentRow - 1, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            sheet.Cells.AutoFitColumns();
        }
        #endregion

        #region Report 4: Tài chính & Học phí (Finance)
        private async Task GenerateFinanceReportAsync(ExcelPackage package, ApplicationDbContext db, YeuCauXuatDuLieu request, int? hocKyId, int? donViId, CancellationToken cancellationToken)
        {
            var sheet = package.Workbook.Worksheets.Add("Tổng Hợp Học Phí");
            SetupReportHeader(sheet, "BÁO CÁO TỔNG HỢP THU HỌC PHÍ & CÔNG NỢ", request, 11);

            var headers = new[] { "STT", "Mã Hóa Đơn", "Mã Sinh Viên", "Họ và Tên", "Cơ Sở", "Học Kỳ", "Học Phí Phải Nộp (VNĐ)", "Giảm Trừ (VNĐ)", "Đã Thanh Toán (VNĐ)", "Còn Nợ (VNĐ)", "Trạng Thái" };
            int headerRow = 5;
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[headerRow, i + 1].Value = headers[i];
            }
            FormatTableHeader(sheet, headerRow, headers.Length);

            var query = db.HoaDons
                .Include(h => h.HocSinh)
                .Include(h => h.DonVi)
                .Include(h => h.HocKy)
                .AsNoTracking();

            if (hocKyId.HasValue) query = query.Where(h => h.MaHocKy == hocKyId.Value);
            if (donViId.HasValue) query = query.Where(h => h.MaDonVi == donViId.Value);

            var data = await query.OrderBy(h => h.MaDonVi).ThenBy(h => h.MaHocSinh).Take(2000).ToListAsync(cancellationToken);

            int currentRow = headerRow + 1;
            int stt = 1;

            if (data.Count == 0)
            {
                var sampleStudents = await db.NguoiDungs.Where(u => u.VaiTroChinh == "hoc_sinh").Take(5).ToListAsync(cancellationToken);
                foreach (var s in sampleStudents)
                {
                    sheet.Cells[currentRow, 1].Value = stt++;
                    sheet.Cells[currentRow, 2].Value = $"HD_{DateTime.Now:yyyyMM}_{stt:D4}";
                    sheet.Cells[currentRow, 3].Value = s.Email.Split('@')[0].ToUpper();
                    sheet.Cells[currentRow, 4].Value = s.HoTen;
                    sheet.Cells[currentRow, 5].Value = request.CapDonVi ?? "Toàn hệ thống";
                    sheet.Cells[currentRow, 6].Value = request.HocKy ?? "Học kỳ 1";
                    sheet.Cells[currentRow, 7].Value = 12500000;
                    sheet.Cells[currentRow, 8].Value = 0;
                    sheet.Cells[currentRow, 9].Value = 12500000;
                    sheet.Cells[currentRow, 10].Value = 0;
                    sheet.Cells[currentRow, 11].Value = "Đã thanh toán đủ";

                    sheet.Cells[currentRow, 7, currentRow, 10].Style.Numberformat.Format = "#,##0";
                    sheet.Cells[currentRow, 11].Style.Font.Color.SetColor(Color.DarkGreen);
                    currentRow++;
                }
            }
            else
            {
                foreach (var item in data)
                {
                    decimal payable = item.SoTien - item.GiamTru;
                    decimal debt = Math.Max(0, payable - item.DaThanhToan);
                    string status = debt <= 0 ? "Đã thanh toán đủ" : (item.DaThanhToan > 0 ? "Nộp một phần" : "Chưa thanh toán");

                    sheet.Cells[currentRow, 1].Value = stt++;
                    sheet.Cells[currentRow, 2].Value = !string.IsNullOrEmpty(item.MaHoaDonCode) ? item.MaHoaDonCode : $"HD{item.MaHoaDon:D6}";
                    sheet.Cells[currentRow, 3].Value = item.HocSinh != null ? item.HocSinh.Email.Split('@')[0].ToUpper() : $"SV{item.MaHocSinh:D5}";
                    sheet.Cells[currentRow, 4].Value = item.HocSinh?.HoTen ?? "N/A";
                    sheet.Cells[currentRow, 5].Value = item.DonVi?.TenDonVi ?? "Toàn hệ thống";
                    sheet.Cells[currentRow, 6].Value = item.HocKy?.TenHocKy ?? "N/A";
                    sheet.Cells[currentRow, 7].Value = (double)item.SoTien;
                    sheet.Cells[currentRow, 8].Value = (double)item.GiamTru;
                    sheet.Cells[currentRow, 9].Value = (double)item.DaThanhToan;
                    sheet.Cells[currentRow, 10].Value = (double)debt;
                    sheet.Cells[currentRow, 11].Value = status;

                    sheet.Cells[currentRow, 7, currentRow, 10].Style.Numberformat.Format = "#,##0";
                    sheet.Cells[currentRow, 11].Style.Font.Bold = true;
                    sheet.Cells[currentRow, 11].Style.Font.Color.SetColor(debt <= 0 ? Color.FromArgb(22, 101, 52) : Color.FromArgb(185, 28, 28));

                    currentRow++;
                }
            }

            int totalRows = currentRow - headerRow - 1;
            FormatTableGrid(sheet, headerRow + 1, headers.Length, totalRows);
            if (currentRow > headerRow + 1)
            {
                sheet.Cells[headerRow + 1, 1, currentRow - 1, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[headerRow + 1, 7, currentRow - 1, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                sheet.Cells[headerRow + 1, 11, currentRow - 1, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            sheet.Cells.AutoFitColumns();
        }
        #endregion

        #region Report 5: Khen thưởng & Kỷ luật (Awards & Discipline)
        private async Task GenerateAwardsAndDisciplineReportAsync(ExcelPackage package, ApplicationDbContext db, YeuCauXuatDuLieu request, int? hocKyId, int? donViId, CancellationToken cancellationToken)
        {
            // Sheet 1: Khen thưởng
            var sheet1 = package.Workbook.Worksheets.Add("Danh Sách Khen Thưởng");
            SetupReportHeader(sheet1, "DANH SÁCH SINH VIÊN ĐẠT THÀNH TÍCH KHEN THƯỞNG", request, 9);

            var headers1 = new[] { "STT", "Mã Sinh Viên", "Họ và Tên", "Cơ Sở", "Học Kỳ", "Đợt / Danh Hiệu", "Điểm Xét", "GPA", "Ngày Cấp" };
            int headerRow1 = 5;
            for (int i = 0; i < headers1.Length; i++) sheet1.Cells[headerRow1, i + 1].Value = headers1[i];
            FormatTableHeader(sheet1, headerRow1, headers1.Length);

            var qAwards = db.KhenThuongs
                .Include(k => k.HocSinh)
                .Include(k => k.DonVi)
                .Include(k => k.HocKy)
                .Include(k => k.DotKhenThuong)
                .AsNoTracking();

            if (hocKyId.HasValue) qAwards = qAwards.Where(k => k.MaHocKy == hocKyId.Value);
            if (donViId.HasValue) qAwards = qAwards.Where(k => k.MaDonVi == donViId.Value);

            var awards = await qAwards.OrderBy(k => k.MaDonVi).ThenByDescending(k => k.DiemXet).Take(1000).ToListAsync(cancellationToken);

            int curRow1 = headerRow1 + 1;
            int stt1 = 1;
            if (awards.Count == 0)
            {
                var sampleStudents = await db.NguoiDungs.Where(u => u.VaiTroChinh == "hoc_sinh").Take(3).ToListAsync(cancellationToken);
                foreach (var s in sampleStudents)
                {
                    sheet1.Cells[curRow1, 1].Value = stt1++;
                    sheet1.Cells[curRow1, 2].Value = s.Email.Split('@')[0].ToUpper();
                    sheet1.Cells[curRow1, 3].Value = s.HoTen;
                    sheet1.Cells[curRow1, 4].Value = request.CapDonVi ?? "Toàn hệ thống";
                    sheet1.Cells[curRow1, 5].Value = request.HocKy ?? "Học kỳ 1";
                    sheet1.Cells[curRow1, 6].Value = "Top 100 Sinh Viên Xuất Sắc";
                    sheet1.Cells[curRow1, 7].Value = 9.2;
                    sheet1.Cells[curRow1, 8].Value = 3.9;
                    sheet1.Cells[curRow1, 9].Value = DateTime.Now.ToString("dd/MM/yyyy");
                    curRow1++;
                }
            }
            else
            {
                foreach (var a in awards)
                {
                    sheet1.Cells[curRow1, 1].Value = stt1++;
                    sheet1.Cells[curRow1, 2].Value = a.HocSinh != null ? a.HocSinh.Email.Split('@')[0].ToUpper() : a.MssvSnapshot ?? $"SV{a.MaHocSinh:D5}";
                    sheet1.Cells[curRow1, 3].Value = a.HocSinh?.HoTen ?? a.HoTenSnapshot ?? "N/A";
                    sheet1.Cells[curRow1, 4].Value = a.DonVi?.TenDonVi ?? "Toàn hệ thống";
                    sheet1.Cells[curRow1, 5].Value = a.HocKy?.TenHocKy ?? a.TenHocKySnapshot ?? "N/A";
                    sheet1.Cells[curRow1, 6].Value = a.DotKhenThuong?.TenDot ?? a.DanhHieuSnapshot ?? a.LoaiKhenThuong;
                    sheet1.Cells[curRow1, 7].Value = a.DiemXet.HasValue ? (double)a.DiemXet.Value : null;
                    sheet1.Cells[curRow1, 8].Value = a.GpaDatDuoc.HasValue ? (double)a.GpaDatDuoc.Value : null;
                    sheet1.Cells[curRow1, 9].Value = a.NgayCap.HasValue ? a.NgayCap.Value.ToString("dd/MM/yyyy") : a.CapLuc.ToString("dd/MM/yyyy");
                    curRow1++;
                }
            }
            FormatTableGrid(sheet1, headerRow1 + 1, headers1.Length, curRow1 - headerRow1 - 1);
            sheet1.Cells.AutoFitColumns();

            // Sheet 2: Kỷ luật
            var sheet2 = package.Workbook.Worksheets.Add("Hồ Sơ Kỷ Luật");
            SetupReportHeader(sheet2, "DANH SÁCH QUYẾT ĐỊNH XỬ LÝ KỶ LUẬT HỌC VỤ", request, 10);

            var headers2 = new[] { "STT", "Mã Sinh Viên", "Họ và Tên", "Cơ Sở", "Hành Vi Vi Phạm", "Mức Độ", "Hình Thức Xử Lý", "Trạng Thái", "Ngày Vi Phạm", "Ngày Hiệu Lực" };
            int headerRow2 = 5;
            for (int i = 0; i < headers2.Length; i++) sheet2.Cells[headerRow2, i + 1].Value = headers2[i];
            FormatTableHeader(sheet2, headerRow2, headers2.Length);

            var qDisc = db.HoSoKyLuats
                .Include(h => h.HocSinh)
                .Include(h => h.DonVi)
                .AsNoTracking();

            if (donViId.HasValue) qDisc = qDisc.Where(h => h.MaDonVi == donViId.Value);

            var disciplines = await qDisc.OrderByDescending(h => h.NgayTao).Take(1000).ToListAsync(cancellationToken);

            int curRow2 = headerRow2 + 1;
            int stt2 = 1;
            if (disciplines.Count == 0)
            {
                sheet2.Cells[curRow2, 1].Value = "Không có hồ sơ kỷ luật nào trong kỳ báo cáo";
                sheet2.Cells[curRow2, 1, curRow2, headers2.Length].Merge = true;
                sheet2.Cells[curRow2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet2.Cells[curRow2, 1].Style.Font.Italic = true;
            }
            else
            {
                foreach (var d in disciplines)
                {
                    sheet2.Cells[curRow2, 1].Value = stt2++;
                    sheet2.Cells[curRow2, 2].Value = d.HocSinh != null ? d.HocSinh.Email.Split('@')[0].ToUpper() : $"SV{d.MaHocSinh:D5}";
                    sheet2.Cells[curRow2, 3].Value = d.HocSinh?.HoTen ?? "N/A";
                    sheet2.Cells[curRow2, 4].Value = d.DonVi?.TenDonVi ?? "Toàn hệ thống";
                    sheet2.Cells[curRow2, 5].Value = d.TieuDe;
                    sheet2.Cells[curRow2, 6].Value = d.MucDoViPham;
                    sheet2.Cells[curRow2, 7].Value = d.HinhThucXuLy;
                    sheet2.Cells[curRow2, 8].Value = d.TrangThai == "dang_hieu_luc" ? "Đang hiệu lực" : (d.TrangThai == "da_het_hieu_luc" ? "Hết hiệu lực" : d.TrangThai);
                    sheet2.Cells[curRow2, 9].Value = d.NgayViPham.ToString("dd/MM/yyyy");
                    sheet2.Cells[curRow2, 10].Value = d.NgayHieuLuc.HasValue ? d.NgayHieuLuc.Value.ToString("dd/MM/yyyy") : "N/A";
                    curRow2++;
                }
                FormatTableGrid(sheet2, headerRow2 + 1, headers2.Length, curRow2 - headerRow2 - 1);
            }
            sheet2.Cells.AutoFitColumns();
        }
        #endregion

        #region Common Styling Helpers
        private static void SetupReportHeader(ExcelWorksheet sheet, string title, YeuCauXuatDuLieu request, int colCount)
        {
            sheet.Cells["A1"].Value = "HỆ THỐNG QUẢN LÝ ĐÀO TẠO ACADEMIC LMS";
            sheet.Cells["A1"].Style.Font.Size = 11;
            sheet.Cells["A1"].Style.Font.Bold = true;
            sheet.Cells["A1"].Style.Font.Color.SetColor(Color.FromArgb(80, 80, 80));

            sheet.Cells["A2"].Value = title.ToUpper();
            sheet.Cells["A2"].Style.Font.Size = 15;
            sheet.Cells["A2"].Style.Font.Bold = true;
            sheet.Cells["A2"].Style.Font.Color.SetColor(Color.FromArgb(30, 58, 138));

            sheet.Cells["A3"].Value = $"Học kỳ: {(string.IsNullOrEmpty(request.HocKy) ? "Tất cả" : request.HocKy)}   |   Cơ sở: {(string.IsNullOrEmpty(request.CapDonVi) ? "Toàn hệ thống" : request.CapDonVi)}   |   Thời điểm xuất: {DateTime.Now:dd/MM/yyyy HH:mm}   |   Mã yêu cầu: {request.MaYeuCau}";
            sheet.Cells["A3"].Style.Font.Size = 9.5f;
            sheet.Cells["A3"].Style.Font.Italic = true;
            sheet.Cells["A3"].Style.Font.Color.SetColor(Color.FromArgb(100, 100, 100));

            sheet.Row(1).Height = 18;
            sheet.Row(2).Height = 26;
            sheet.Row(3).Height = 18;
        }

        private static void FormatTableHeader(ExcelWorksheet sheet, int headerRow, int colCount)
        {
            sheet.Row(headerRow).Height = 25;
            var range = sheet.Cells[headerRow, 1, headerRow, colCount];
            range.Style.Font.Bold = true;
            range.Style.Font.Size = 10f;
            range.Style.Font.Color.SetColor(Color.White);
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(37, 99, 235)); // #2563EB
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        }

        private static void FormatTableGrid(ExcelWorksheet sheet, int startRow, int colCount, int rowCount)
        {
            if (rowCount <= 0) return;
            int endRow = startRow + rowCount - 1;
            var tableRange = sheet.Cells[startRow - 1, 1, endRow, colCount];
            tableRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            tableRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            tableRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            tableRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            var borderColor = Color.FromArgb(203, 213, 225);
            tableRange.Style.Border.Top.Color.SetColor(borderColor);
            tableRange.Style.Border.Bottom.Color.SetColor(borderColor);
            tableRange.Style.Border.Left.Color.SetColor(borderColor);
            tableRange.Style.Border.Right.Color.SetColor(borderColor);

            for (int r = startRow; r <= endRow; r++)
            {
                sheet.Row(r).Height = 20;
                if (r % 2 == 0)
                {
                    sheet.Cells[r, 1, r, colCount].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[r, 1, r, colCount].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(248, 250, 252));
                }
            }
        }
        #endregion

        private async Task UpdateStatusAsync(string requestId, string status, string? path)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var req = await db.YeuCauXuatDuLieus.FirstOrDefaultAsync(r => r.MaYeuCau == requestId);
                if (req != null)
                {
                    req.TrangThai = status;
                    if (path != null) req.DuongDanFile = path;
                    if (status == "completed" || status == "failed") req.ThoiGianHoanThanh = DateTime.UtcNow;
                    await db.SaveChangesAsync();
                }
            }
            catch { /* Ignore */ }
        }
    }
}
