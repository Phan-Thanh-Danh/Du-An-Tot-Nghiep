using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class R2ToDbPopulatorTest
    {
        [Test]
        public async Task SyncAllR2VideosIntoDatabase()
        {
            var endpoint = "https://87934b0fb36afe0a6b19db75efc7fe24.r2.cloudflarestorage.com";
            var accessKey = "872e796be9c27223e4d2b7fe48afd75e";
            var secretKey = "46a0c09da41ff2f0a7cc7aacad3bb8ed6c418eb4530617862c860d248bf2e28b";
            var bucketName = "aet-lms-media";

            var config = new AmazonS3Config
            {
                ServiceURL = endpoint,
                AuthenticationRegion = "auto"
            };

            using var s3Client = new AmazonS3Client(accessKey, secretKey, config);

            var listReq = new ListObjectsV2Request { BucketName = bucketName };
            var listRes = await s3Client.ListObjectsV2Async(listReq);

            var mediaObjects = listRes.S3Objects
                .Where(o => o.Size > 0 && (
                    o.Key.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
                    o.Key.EndsWith(".webm", StringComparison.OrdinalIgnoreCase) ||
                    o.Key.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)))
                .OrderBy(o => o.Key)
                .ToList();

            TestContext.Progress.WriteLine($"Found {mediaObjects.Count} valid media files in Cloudflare R2.");

            var connStr = TestDatabaseSafetyGuard.GetVerifiedTestConnectionString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            int createdSubjects = 0;
            int createdChapters = 0;
            int createdLessons = 0;
            int updatedLessons = 0;

            // Load existing subjects and majors
            var subjects = await db.DanhMucMonHocs.ToListAsync();
            var nganhs = await db.NganhDaoTaos.ToListAsync();
            var chuyenNganhs = await db.ChuyenNganhs.ToListAsync();

            var cnttNganh = nganhs.FirstOrDefault(n => n.TenNganh.Contains("Công nghệ thông tin") || n.MaCodeNganh.Contains("CNTT")) ?? nganhs.FirstOrDefault();
            var mktNganh = nganhs.FirstOrDefault(n => n.TenNganh.Contains("Marketing") || n.MaCodeNganh.Contains("MKT")) ?? nganhs.FirstOrDefault();
            var tkdhNganh = nganhs.FirstOrDefault(n => n.TenNganh.Contains("Thiết kế") || n.MaCodeNganh.Contains("TKDH") || n.MaCodeNganh.Contains("DES")) ?? nganhs.FirstOrDefault();

            foreach (var s3Obj in mediaObjects)
            {
                // Format:
                // 1. videos/2026/08/15/CNTT/{TOPIC}/{LESSON_FOLDER}/{FILENAME}.mp4
                // 2. videos/2026/08/15/Marketing/{YEAR}/{SUBJECT_FOLDER}/{FILENAME}.mp4
                // 3. videos/2026/08/15/TKDH/{YEAR}/{SUBJECT_FOLDER}/{FILENAME}.mp4

                var parts = s3Obj.Key.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 6) continue;

                var category = parts[4]; // CNTT, Marketing, TKDH
                string subjectCode = "";
                string subjectName = "";
                string chapterTitle = "";
                string lessonTitle = "";
                int lessonOrder = 1;
                NganhDaoTao? targetNganh = null;

                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(s3Obj.Key);

                if (category.Equals("CNTT", StringComparison.OrdinalIgnoreCase))
                {
                    targetNganh = cnttNganh;
                    var topic = parts[5]; // C#, JAVA, SQL, HTML, CSS
                    var lessonFolder = parts.Length > 6 ? parts[6] : "BÀI 1";
                    
                    // Parse lesson order from "BÀI X"
                    var match = Regex.Match(lessonFolder, @"\d+");
                    if (match.Success && int.TryParse(match.Value, out int ord))
                        lessonOrder = ord;

                    switch (topic.ToUpperInvariant())
                    {
                        case "C#":
                            subjectCode = "COM103";
                            subjectName = "Lập trình C#";
                            break;
                        case "SQL":
                            subjectCode = "COM102";
                            subjectName = "Cơ sở dữ liệu SQL Server";
                            break;
                        case "JAVA":
                            subjectCode = "COM104";
                            subjectName = "Lập trình Java căn bản";
                            break;
                        case "HTML":
                            subjectCode = "WEB101";
                            subjectName = "Thiết kế Web với HTML & CSS";
                            chapterTitle = "Phần 1: Ngôn ngữ đánh dấu HTML";
                            break;
                        case "CSS":
                            subjectCode = "WEB101";
                            subjectName = "Thiết kế Web với HTML & CSS";
                            chapterTitle = "Phần 2: Định dạng & Bố cục CSS";
                            break;
                        default:
                            subjectCode = "COM_" + topic.ToUpperInvariant();
                            subjectName = "Lập trình " + topic;
                            break;
                    }

                    if (string.IsNullOrEmpty(chapterTitle))
                    {
                        chapterTitle = $"Chương {(lessonOrder <= 5 ? 1 : 2)}: {(lessonOrder <= 5 ? "Kiến thức nền tảng" : "Kỹ thuật nâng cao")}";
                    }
                    lessonTitle = fileNameWithoutExt;
                }
                else if (category.Equals("Marketing", StringComparison.OrdinalIgnoreCase))
                {
                    targetNganh = mktNganh;
                    var year = parts[5]; // Nam 1, Nam 2, Nam 3
                    var subFolder = parts.Length > 6 ? parts[6] : "01_Marketing";
                    
                    // Parse subFolder: "01_Kinh te vi mo" -> "Kinh tế vi mô"
                    var subName = subFolder.Contains('_') ? subFolder.Substring(subFolder.IndexOf('_') + 1) : subFolder;
                    subjectCode = "MKT_" + Regex.Replace(subFolder, @"[^\w]", "").ToUpperInvariant();
                    subjectName = "Môn học " + subName;
                    chapterTitle = $"Chương 1: Tổng quan {subName}";
                    lessonTitle = fileNameWithoutExt;
                    lessonOrder = 1;

                    // Match with standard MKT subjects if available
                    var existingSub = subjects.FirstOrDefault(s => s.TenMonHoc.Contains(subName, StringComparison.OrdinalIgnoreCase));
                    if (existingSub != null)
                    {
                        subjectCode = existingSub.MaCodeMonHoc;
                        subjectName = existingSub.TenMonHoc;
                    }
                }
                else if (category.Equals("TKDH", StringComparison.OrdinalIgnoreCase))
                {
                    targetNganh = tkdhNganh;
                    var year = parts[5]; // Nam 1, Nam 2...
                    var subFolder = parts.Length > 6 ? parts[6] : "01_Bo cuc";
                    var subName = subFolder.Contains('_') ? subFolder.Substring(subFolder.IndexOf('_') + 1) : subFolder;
                    subjectCode = "DES_" + Regex.Replace(subFolder, @"[^\w]", "").ToUpperInvariant();
                    subjectName = "Môn học " + subName;
                    chapterTitle = $"Chương 1: Nhập môn {subName}";
                    lessonTitle = fileNameWithoutExt;
                    lessonOrder = 1;

                    var existingSub = subjects.FirstOrDefault(s => s.TenMonHoc.Contains(subName, StringComparison.OrdinalIgnoreCase));
                    if (existingSub != null)
                    {
                        subjectCode = existingSub.MaCodeMonHoc;
                        subjectName = existingSub.TenMonHoc;
                    }
                }

                // 1. Find or create Subject (DanhMucMonHoc)
                var subject = await db.DanhMucMonHocs.FirstOrDefaultAsync(s => s.MaCodeMonHoc == subjectCode || s.TenMonHoc == subjectName);
                if (subject == null)
                {
                    subject = new DanhMucMonHoc
                    {
                        MaCodeMonHoc = subjectCode,
                        TenMonHoc = subjectName,
                        SoTinChi = 3,
                        ConHoatDong = true,
                        MaNganh = targetNganh?.MaNganh
                    };
                    db.DanhMucMonHocs.Add(subject);
                    await db.SaveChangesAsync();
                    subjects.Add(subject);
                    createdSubjects++;
                    TestContext.Progress.WriteLine($"[CREATED SUBJECT] {subject.MaCodeMonHoc} - {subject.TenMonHoc}");
                }

                // 2. Find or create Chapter (Chuong)
                var chapter = await db.Chuongs.FirstOrDefaultAsync(c => c.MaMonHoc == subject.MaMonHoc && c.TieuDe == chapterTitle);
                if (chapter == null)
                {
                    var maxOrder = await db.Chuongs.Where(c => c.MaMonHoc == subject.MaMonHoc).MaxAsync(c => (int?)c.ThuTu) ?? 0;
                    chapter = new Chuong
                    {
                        MaMonHoc = subject.MaMonHoc,
                        TieuDe = chapterTitle,
                        ThuTu = maxOrder + 1,
                        DaAn = false,
                        NgayTao = DateTime.UtcNow
                    };
                    db.Chuongs.Add(chapter);
                    await db.SaveChangesAsync();
                    createdChapters++;
                    TestContext.Progress.WriteLine($"   [CREATED CHAPTER] {chapter.TieuDe} for Subject {subject.MaCodeMonHoc}");
                }

                // 3. Find or create Lesson (BaiHoc)
                var streamUrl = $"/api/storage/stream?key={Uri.EscapeDataString(s3Obj.Key)}";
                int durationSec = Math.Max(180, (int)(s3Obj.Size / (62 * 1024))); // Estimate realistic seconds from size
                var lesson = await db.BaiHocs.Include(b => b.BaiHocNoiDungs).FirstOrDefaultAsync(b => b.MaChuong == chapter.MaChuong && b.TieuDe == lessonTitle);
                if (lesson == null)
                {
                    lesson = new BaiHoc
                    {
                        MaChuong = chapter.MaChuong,
                        TieuDe = lessonTitle,
                        LoaiBaiHoc = "video",
                        UrlTapTin = streamUrl,
                        ThoiLuongGiay = durationSec,
                        ThuTu = lessonOrder,
                        DaAn = false,
                        TrangThai = "da_xuat_ban",
                        NgayTao = DateTime.UtcNow
                    };
                    db.BaiHocs.Add(lesson);
                    await db.SaveChangesAsync();

                    // Create BaiHocNoiDung
                    var noiDung = new BaiHocNoiDung
                    {
                        MaBaiHoc = lesson.MaBaiHoc,
                        LoaiNoiDung = "video",
                        UrlTapTin = streamUrl,
                        StorageKey = s3Obj.Key,
                        KichThuocByte = s3Obj.Size,
                        ThoiLuongGiay = durationSec,
                        TrangThai = "da_xuat_ban",
                        ThuTu = 1,
                        NgayTao = DateTime.UtcNow
                    };
                    db.BaiHocNoiDungs.Add(noiDung);
                    await db.SaveChangesAsync();
                    createdLessons++;
                    TestContext.Progress.WriteLine($"      [CREATED LESSON] ID: {lesson.MaBaiHoc} | {lesson.TieuDe} | Duration: {durationSec}s");
                }
                else
                {
                    lesson.UrlTapTin = streamUrl;
                    lesson.LoaiBaiHoc = "video";
                    lesson.TrangThai = "da_xuat_ban";
                    lesson.ThoiLuongGiay = durationSec;
                    lesson.NgayCapNhat = DateTime.UtcNow;

                    var noiDung = lesson.BaiHocNoiDungs.FirstOrDefault(n => n.LoaiNoiDung == "video");
                    if (noiDung == null)
                    {
                        noiDung = new BaiHocNoiDung
                        {
                            MaBaiHoc = lesson.MaBaiHoc,
                            LoaiNoiDung = "video",
                            UrlTapTin = streamUrl,
                            StorageKey = s3Obj.Key,
                            KichThuocByte = s3Obj.Size,
                            ThoiLuongGiay = durationSec,
                            TrangThai = "da_xuat_ban",
                            ThuTu = 1,
                            NgayTao = DateTime.UtcNow
                        };
                        db.BaiHocNoiDungs.Add(noiDung);
                    }
                    else
                    {
                        noiDung.UrlTapTin = streamUrl;
                        noiDung.StorageKey = s3Obj.Key;
                        noiDung.KichThuocByte = s3Obj.Size;
                        noiDung.ThoiLuongGiay = durationSec;
                        noiDung.TrangThai = "da_xuat_ban";
                        noiDung.NgayCapNhat = DateTime.UtcNow;
                    }
                    await db.SaveChangesAsync();
                    updatedLessons++;
                    TestContext.Progress.WriteLine($"      [UPDATED LESSON] ID: {lesson.MaBaiHoc} | {lesson.TieuDe} | Duration: {durationSec}s");
                }
            }

            TestContext.Progress.WriteLine($"\n=== SYNC COMPLETE ===");
            TestContext.Progress.WriteLine($"Created Subjects: {createdSubjects}");
            TestContext.Progress.WriteLine($"Created Chapters: {createdChapters}");
            TestContext.Progress.WriteLine($"Created Lessons: {createdLessons}");
            TestContext.Progress.WriteLine($"Updated Lessons: {updatedLessons}");
        }
    }
}
