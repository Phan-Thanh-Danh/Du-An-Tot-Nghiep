using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Constants;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.StudentCurriculum;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/student/curriculum")]
    public class StudentCurriculumController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StudentCurriculumController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = AuthRoles.Student)]
        public async Task<ActionResult<ApiResponseDto<StudentCurriculumResponseDto>>> GetCurriculum()
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var student = await _context.NguoiDungs
                .AsNoTracking()
                .Include(user => user.Lop)
                    .ThenInclude(classEntity => classEntity!.DonVi)
                .FirstOrDefaultAsync(user => user.MaNguoiDung == currentUser.UserId);

            if (student == null)
            {
                return NotFound(new ApiResponseDto<StudentCurriculumResponseDto>
                {
                    Success = false,
                    Message = "Không tìm thấy sinh viên hiện tại"
                });
            }

            if (student.MaLop == null || student.Lop?.MaChuongTrinh == null)
            {
                return NotFound(new ApiResponseDto<StudentCurriculumResponseDto>
                {
                    Success = false,
                    Message = "Sinh viên chưa được gán lớp hoặc chương trình đào tạo"
                });
            }

            var program = await _context.ChuongTrinhDaoTaos
                .AsNoTracking()
                .Include(programEntity => programEntity.ChuyenNganh)
                    .ThenInclude(specialization => specialization!.NganhDaoTao)
                .Include(programEntity => programEntity.KhoaTuyenSinh)
                .FirstOrDefaultAsync(programEntity =>
                    programEntity.MaChuongTrinh == student.Lop.MaChuongTrinh.Value &&
                    programEntity.ConHoatDong);

            if (program == null)
            {
                return NotFound(new ApiResponseDto<StudentCurriculumResponseDto>
                {
                    Success = false,
                    Message = "Không tìm thấy chương trình đào tạo của sinh viên"
                });
            }

            var programSubjects = await _context.MonHocTrongChuongTrinhs
                .AsNoTracking()
                .Include(m => m.DanhMucMonHoc)
                .Where(m => m.MaChuongTrinh == program.MaChuongTrinh && m.ConHoatDong)
                .OrderBy(m => m.HocKyDuKien)
                .ThenBy(m => m.ThuTu)
                .ThenBy(m => m.DanhMucMonHoc!.TenMonHoc)
                .ToListAsync();

            var subjectIds = programSubjects.Select(subject => subject.MaMonHoc).Distinct().ToList();
            var gradeRows = await _context.DiemSos
                .AsNoTracking()
                .Where(grade => grade.MaHocSinh == student.MaNguoiDung && subjectIds.Contains(grade.MaMonHoc))
                .OrderByDescending(grade => grade.DaKhoa)
                .ThenByDescending(grade => grade.MaHocKy)
                .ToListAsync();
            var grades = gradeRows
                .GroupBy(grade => grade.MaMonHoc)
                .ToDictionary(group => group.Key, group => group.First());

            var completedSubjectIds = grades
                .Where(item => item.Value.TrangThai == "dat")
                .Select(item => item.Key)
                .ToHashSet();
            var completedCredits = programSubjects
                .Where(subject => completedSubjectIds.Contains(subject.MaMonHoc))
                .Sum(subject => subject.SoTinChi);
            var currentSemesterIndex = DetermineCurrentSemester(programSubjects, completedSubjectIds, program.SoHocKy);

            var response = new StudentCurriculumResponseDto
            {
                StudentName = student.HoTen,
                MajorName = program.ChuyenNganh?.TenChuyenNganh ?? program.TenChuongTrinh,
                FacultyName = program.ChuyenNganh?.NganhDaoTao?.TenNganh ?? student.Lop.DonVi?.TenDonVi ?? string.Empty,
                ProgramCode = program.MaCodeChuongTrinh,
                ProgramVersion = program.Version,
                ProgramName = program.TenChuongTrinh,
                ClassCode = student.Lop.MaCodeLop,
                ClassName = student.Lop.TenLop,
                CohortName = program.KhoaTuyenSinh?.TenKhoa ?? string.Empty,
                TrainingMonths = program.ThoiGianDaoTaoThang,
                TotalSemesters = program.SoHocKy,
                CurrentSemesterIndex = currentSemesterIndex,
                CurrentBlockIndex = 1,
                TotalCredits = program.TongTinChiYeuCau ?? programSubjects.Sum(subject => subject.SoTinChi),
                CompletedCredits = completedCredits,
                TotalSubjects = programSubjects.Count,
                CompletedSubjects = completedSubjectIds.Count
            };

            for (var semesterIndex = 1; semesterIndex <= program.SoHocKy; semesterIndex++)
            {
                var subjects = programSubjects
                    .Where(subject => subject.HocKyDuKien == semesterIndex)
                    .ToList();

                var semesterDto = new SemesterDto
                {
                    SemesterIndex = semesterIndex,
                    SemesterName = $"Kỳ {semesterIndex}",
                    Blocks = new List<BlockDto>()
                };

                if (subjects.Any())
                {
                    semesterDto.Blocks.Add(new BlockDto
                    {
                        BlockIndex = 1,
                        BlockName = "Danh sách môn học",
                        Subjects = subjects.Select(subject =>
                        {
                            grades.TryGetValue(subject.MaMonHoc, out var grade);
                            return MapToDto(subject, semesterIndex, currentSemesterIndex, grade);
                        }).ToList()
                    });
                }

                response.Semesters.Add(semesterDto);
            }

            return Ok(ApiResponseDto<StudentCurriculumResponseDto>.Ok(response));
        }

        private static int DetermineCurrentSemester(
            IReadOnlyCollection<Backend.Models.MonHocTrongChuongTrinh> programSubjects,
            IReadOnlySet<int> completedSubjectIds,
            int totalSemesters)
        {
            if (!programSubjects.Any())
            {
                return totalSemesters > 0 ? 1 : 0;
            }

            var firstOpenSemester = programSubjects
                .GroupBy(subject => subject.HocKyDuKien)
                .OrderBy(group => group.Key)
                .FirstOrDefault(group => group.Any(subject => !completedSubjectIds.Contains(subject.MaMonHoc)))
                ?.Key;

            return firstOpenSemester ?? totalSemesters;
        }

        private static CurriculumSubjectDto MapToDto(
            Backend.Models.MonHocTrongChuongTrinh subject,
            int semesterIndex,
            int currentSemesterIndex,
            Backend.Models.DiemSo? grade)
        {
            var status = ResolveStatus(semesterIndex, currentSemesterIndex, grade);
            var score = grade == null ? null : (double?)grade.GpaMonHoc;

            return new CurriculumSubjectDto
            {
                Id = subject.MaChuongTrinhMonHoc.ToString(),
                SubjectCode = subject.DanhMucMonHoc?.MaCodeMonHoc ?? string.Empty,
                SubjectName = subject.DanhMucMonHoc?.TenMonHoc ?? string.Empty,
                Credits = subject.SoTinChi,
                Status = status,
                ProgressPercent = status == "completed" ? 100 : (status == "current" ? 50 : 0),
                QuizScore = null,
                Score = score,
                IsEarlyLearning = false,
                SubjectType = subject.LoaiMonHoc,
                IsRequired = subject.BatBuoc,
                Note = subject.GhiChu ?? string.Empty,
                VersionStatus = "current_program"
            };
        }

        private static string ResolveStatus(int semesterIndex, int currentSemesterIndex, Backend.Models.DiemSo? grade)
        {
            if (grade?.TrangThai == "dat")
            {
                return "completed";
            }

            if (grade is not null && grade.TrangThai != "chua_hoan_thanh")
            {
                return "current";
            }

            if (semesterIndex < currentSemesterIndex)
            {
                return "current";
            }

            return semesterIndex == currentSemesterIndex ? "current" : "future_locked";
        }
    }
}
