/**
 * studentData.mock.js
 * File dữ liệu mẫu hợp nhất cho toàn bộ hệ thống Sinh viên (Student Module).
 * Sử dụng cho giao diện frontend demo, giúp đồng bộ thông tin của Nguyễn Văn An (SV2026001).
 */
import { reactive } from 'vue'

// ==========================================
// 1. THÔNG TIN CÁ NHÂN & LIÊN LẠC (Profile)
// ==========================================
function getActiveEmail() {
  try {
    const userStr = localStorage.getItem('lms_auth_user') || sessionStorage.getItem('lms_auth_user')
    if (userStr) {
      const user = JSON.parse(userStr)
      return user.email || ''
    }
  } catch (e) {
    // ignore
  }
  return 'student@mock.local'
}

export const mockProfile = reactive({})
export const mockAwards = reactive([])
export const mockDisciplines = reactive([])
export const mockParents = reactive([])
export const mockStudentCurriculum = reactive({})
export const studentDashboardMock = reactive({})
export const mockSubjects = reactive([])
export const mockGradeSummary = reactive({})
export const mockSemesterGPA = reactive([])
export const mockExams = reactive([])

export const mockAssignment = reactive({})
export const mockSubmissionRules = reactive({})
export const mockAttachments = reactive([])
export const mockSubmissions = reactive([])
export const mockPlagiarismResult = reactive({})
export const mockFeedback = reactive({})
export const mockCourse = reactive({})
export const mockStats = reactive([])
export const mockLessons = reactive([])
export const mockCurrentLesson = reactive({})
export const mockQuizQuestions = reactive([])
export const mockComments = reactive([])
export const mockTimeline = reactive([])
export const mockAISummary = reactive({})



const originalProfile = {
  studentId: 'SV2026001',
  fullName: 'Nguyễn Văn An',
  email: 'an.nv.sv@student.edu.vn',
  phone: '0901234567',
  address: '123 Nguyễn Hữu Cảnh, P.22, Bình Thạnh, TP.HCM',
  className: 'SE1601',
  major: 'Kỹ thuật Phần mềm',
  campus: 'Cơ sở TP.Hồ Chí Minh',
  status: 'Active',
  role: 'Student'
}

const originalAwards = [
  { id: 'AW-01', title: 'Học bổng Xuất sắc Học kỳ 1', type: 'Học thuật', gpa: '3.8/4.0', date: '15/01/2026' }
]
const originalDisciplines = []
const originalParents = [
  { id: 'PR-01', name: 'Nguyễn Văn Định (Bố)', email: 'dinh.nv@gmail.com', status: 'Connected', permissions: { grades: true, attendance: true, finance: false, schedule: true } }
]

const profileGD = {
  studentId: 'SV2026002',
  fullName: 'Nguyễn Thiết Kế',
  email: 'student_gd@mock.local',
  phone: '0902345678',
  address: '456 Lê Lợi, Quận 1, TP.HCM',
  className: 'GD1701',
  major: 'Thiết kế đồ họa',
  campus: 'Cơ sở TP.Hồ Chí Minh',
  status: 'Active',
  role: 'Student'
}

const profileMKT = {
  studentId: 'SV2026003',
  fullName: 'Trần Thị Marketing',
  email: 'student_mkt@mock.local',
  phone: '0903456789',
  address: '789 Nguyễn Huệ, Quận 1, TP.HCM',
  className: 'MR1801',
  major: 'Marketing',
  campus: 'Cơ sở TP.Hồ Chí Minh',
  status: 'Active',
  role: 'Student'
}



// ==========================================
// 2. KHUNG CHƯƠNG TRÌNH & TIẾN ĐỘ (Curriculum)
// ==========================================
function createSubject(id, subjectCode, subjectName, credits, status, progressPercent, score, allowEarlyLearning, extra = {}) {
  return {
    id,
    subjectCode,
    subjectName,
    credits,
    status,
    progressPercent,
    score,
    allowEarlyLearning,
    earlyProgressPercent: null,
    earlyScore: null,
    quizScore: null,
    officialScore: score,
    versionStatus: 'current_program',
    ...extra,
  }
}

const originalStudentCurriculum = {
  studentName: 'Nguyễn Văn An',
  majorName: 'Phát triển phần mềm',
  facultyName: 'Công nghệ thông tin',
  programCode: 'SD-FPT-2026',
  programVersion: 'Version 2026',
  currentSemesterIndex: 2,
  currentBlockIndex: 1,
  totalCredits: 48,
  completedCredits: 18,
  totalSubjects: 16,
  completedSubjects: 6,
  semesters: [
    {
      semesterIndex: 1,
      semesterName: 'Kỳ 1',
      blocks: [
        {
          blockIndex: 1,
          blockName: 'Block 1',
          subjects: [
            createSubject('web101', 'WEB101', 'Nhập môn lập trình web', 3, 'completed', 100, 8.5, false),
            createSubject('sdlc101', 'SDLC101', 'Nhập môn quy trình phần mềm', 3, 'completed', 100, 8.2, false),
            createSubject('tin101', 'TIN101', 'Tin học văn phòng', 3, 'completed', 100, 9.0, false),
            createSubject('eng101', 'ENG101', 'Tiếng Anh cơ sở 1', 3, 'completed', 100, 8.0, false),
            createSubject('mat101', 'MAT101', 'Toán rời rạc', 3, 'completed', 100, 7.5, false),
            createSubject('soft101', 'SOFT101', 'Kỹ năng giao tiếp', 3, 'completed', 100, 8.8, false),
          ],
        },
        {
          blockIndex: 2,
          blockName: 'Block 2',
          subjects: [
            createSubject('net102', 'NET102', 'Lập trình C# nâng cấp', 3, 'completed', 70, null, false, {
              versionStatus: 'partial_equivalent',
              replacedSubjectCode: 'NET101',
              previousVersionName: 'Version 2026 (Cũ)',
              earlyScoreFromOldVersion: 8.5,
              requiresSupplement: true,
              supplementPercent: 30,
              versionNote: 'Bạn đã học NET101 ở Version 2026 (Cũ). Version 2026 thay bằng NET102 và cần học bổ sung 30% nội dung mới.',
            }),
            createSubject('dbi101', 'DBI101', 'Cơ sở dữ liệu', 3, 'completed', 100, 8.0, false),
          ],
        },
      ],
    },
    {
      semesterIndex: 2,
      semesterName: 'Kỳ 2',
      blocks: [
        {
          blockIndex: 1,
          blockName: 'Block 1',
          subjects: [
            createSubject('web201', 'WEB201', 'Frontend với Vue', 3, 'current', 72, null, false, { quizScore: 8.1 }),
            createSubject('api201', 'API201', 'ASP.NET Core API', 3, 'current', 58, null, false, { quizScore: 7.6 }),
          ],
        },
        {
          blockIndex: 2,
          blockName: 'Block 2',
          subjects: [
            createSubject('prj301', 'PRJ301', 'Dự án mẫu', 3, 'early_available', 0, null, true),
            createSubject('test101', 'TEST101', 'Kiểm thử phần mềm', 3, 'early_completed', 0, null, true, {
              earlyProgressPercent: 64,
              earlyScore: 8.4,
              earlyCompletedAt: '22/05/2026',
            }),
          ],
        },
      ],
    },
    {
      semesterIndex: 3,
      semesterName: 'Kỳ 3',
      blocks: [
        {
          blockIndex: 1,
          blockName: 'Block 1',
          subjects: [
            createSubject('mobile301', 'MOB301', 'Phát triển ứng dụng di động', 3, 'early_available', 0, null, true),
            createSubject('cloud301', 'CLD301', 'Triển khai Cloud căn bản', 3, 'future_locked', 0, null, false),
          ],
        },
        {
          blockIndex: 2,
          blockName: 'Block 2',
          subjects: [
            createSubject('devops301', 'DOP301', 'DevOps cho nhóm phần mềm', 3, 'early_available', 0, null, true),
            createSubject('cap401', 'CAP401', 'Capstone định hướng', 3, 'future_locked', 0, null, false),
          ],
        },
      ],
    },
    {
      semesterIndex: 4,
      semesterName: 'Kỳ 4',
      blocks: [
        {
          blockIndex: 1,
          blockName: 'Block 1',
          subjects: [
            createSubject('ai401', 'AI401', 'Ứng dụng AI trong sản phẩm phần mềm', 3, 'changed_program', 0, null, false, {
              versionStatus: 'history_only',
              note: 'Môn này thuộc phiên bản cũ, chờ đối chiếu chương trình.',
            }),
            createSubject('ux401', 'UX401', 'UX cho sản phẩm số', 3, 'future_locked', 0, null, false),
          ],
        },
        {
          blockIndex: 2,
          blockName: 'Block 2',
          subjects: [
            createSubject('intern401', 'INT401', 'Thực tập doanh nghiệp', 6, 'future_locked', 0, null, false),
            createSubject('grad401', 'GRD401', 'Đồ án tốt nghiệp', 6, 'future_locked', 0, null, false),
          ],
        },
      ],
    },
  ],
}

export const mockCurriculumVersionData = reactive({})

const originalCurriculumVersionData = {
  currentProgram: {
    programId: 'sd-2026',
    programCode: 'SD-FPT-2026',
    majorName: 'Phát triển phần mềm',
    versionName: 'Version 2026',
    effectiveFromYear: 2026,
    studentCohort: 2026,
  },
  availableVersions: [
    {
      programId: 'sd-2026',
      versionName: 'Version 2026',
      isCurrent: true,
      hasEarlyLearningHistory: false,
    },
    {
      programId: 'sd-2026-old',
      versionName: 'Version 2026 (Cũ)',
      isCurrent: false,
      hasEarlyLearningHistory: true,
    },
  ],
  versionChanges: {
    replacedSubjects: 2,
    changedSubjects: 1,
    addedSubjects: 1,
    historyOnlySubjects: 1,
  },
  earlyLearningHistory: [
    {
      id: 'early-net101-2026-old',
      oldProgramVersion: 'Version 2026 (Cũ)',
      oldSubjectCode: 'NET101',
      oldSubjectName: 'Lập trình C# cơ bản',
      newSubjectCode: 'NET102',
      newSubjectName: 'Lập trình C# nâng cấp',
      relationType: 'partial_equivalent',
      progressPercent: 100,
      quizScore: 8.5,
      learnedAt: '12/03/2026',
      applyStatus: 'requires_supplement',
      supplementPercent: 30,
      note: 'Nội dung version mới thay đổi khoảng 30%, cần học bổ sung.',
    },
    {
      id: 'early-web101-2026-old',
      oldProgramVersion: 'Version 2026 (Cũ)',
      oldSubjectCode: 'WEB101-Old',
      oldSubjectName: 'Nhập môn lập trình web (Cũ)',
      newSubjectCode: 'WEB101',
      newSubjectName: 'Nhập môn lập trình web',
      relationType: 'equivalent',
      progressPercent: 100,
      quizScore: 8.5,
      learnedAt: '12/03/2026',
      applyStatus: 'applied',
      supplementPercent: 0,
    },
    {
      id: 'early-sdlc101-2026-old',
      oldProgramVersion: 'Version 2026 (Cũ)',
      oldSubjectCode: 'SDLC101-Old',
      oldSubjectName: 'Nhập môn quy trình phần mềm (Cũ)',
      newSubjectCode: 'SDLC101',
      newSubjectName: 'Nhập môn quy trình phần mềm',
      relationType: 'equivalent',
      progressPercent: 100,
      quizScore: 8.2,
      learnedAt: '12/03/2026',
      applyStatus: 'applied',
      supplementPercent: 0,
    },
    {
      id: 'early-tin101-2026-old',
      oldProgramVersion: 'Version 2026 (Cũ)',
      oldSubjectCode: 'TIN101-Old',
      oldSubjectName: 'Tin học văn phòng (Cũ)',
      newSubjectCode: 'TIN101',
      newSubjectName: 'Tin học văn phòng',
      relationType: 'equivalent',
      progressPercent: 100,
      quizScore: 9.0,
      learnedAt: '12/03/2026',
      applyStatus: 'applied',
      supplementPercent: 0,
    },
    {
      id: 'early-eng101-2026-old',
      oldProgramVersion: 'Version 2026 (Cũ)',
      oldSubjectCode: 'ENG101-Old',
      oldSubjectName: 'Tiếng Anh cơ sở 1 (Cũ)',
      newSubjectCode: 'ENG101',
      newSubjectName: 'Tiếng Anh cơ sở 1',
      relationType: 'equivalent',
      progressPercent: 100,
      quizScore: 8.0,
      learnedAt: '12/03/2026',
      applyStatus: 'applied',
      supplementPercent: 0,
    },
    {
      id: 'early-mat101-2026-old',
      oldProgramVersion: 'Version 2026 (Cũ)',
      oldSubjectCode: 'MAT101-Old',
      oldSubjectName: 'Toán rời rạc (Cũ)',
      newSubjectCode: 'MAT101',
      newSubjectName: 'Toán rời rạc',
      relationType: 'equivalent',
      progressPercent: 100,
      quizScore: 7.5,
      learnedAt: '12/03/2026',
      applyStatus: 'applied',
      supplementPercent: 0,
    },
    {
      id: 'early-soft101-2026-old',
      oldProgramVersion: 'Version 2026 (Cũ)',
      oldSubjectCode: 'SOFT101-Old',
      oldSubjectName: 'Kỹ năng giao tiếp (Cũ)',
      newSubjectCode: 'SOFT101',
      newSubjectName: 'Kỹ năng giao tiếp',
      relationType: 'equivalent',
      progressPercent: 100,
      quizScore: 8.8,
      learnedAt: '12/03/2026',
      applyStatus: 'applied',
      supplementPercent: 0,
    },
    {
      id: 'early-dbi101-2026-old',
      oldProgramVersion: 'Version 2026 (Cũ)',
      oldSubjectCode: 'DBI101-Old',
      oldSubjectName: 'Cơ sở dữ liệu (Cũ)',
      newSubjectCode: 'DBI101',
      newSubjectName: 'Cơ sở dữ liệu',
      relationType: 'equivalent',
      progressPercent: 100,
      quizScore: 8.0,
      learnedAt: '12/03/2026',
      applyStatus: 'applied',
      supplementPercent: 0,
    },
    {
      id: 'early-web201-2026-old',
      oldProgramVersion: 'Version 2026 (Cũ)',
      oldSubjectCode: 'WEB201-Old',
      oldSubjectName: 'Frontend với Vue (Cũ)',
      newSubjectCode: 'WEB201',
      newSubjectName: 'Frontend với Vue',
      relationType: 'equivalent',
      progressPercent: 72,
      quizScore: 8.1,
      learnedAt: '22/05/2026',
      applyStatus: 'applied',
      supplementPercent: 0,
    },
    {
      id: 'early-api201-2026-old',
      oldProgramVersion: 'Version 2026 (Cũ)',
      oldSubjectCode: 'API201-Old',
      oldSubjectName: 'ASP.NET Core API (Cũ)',
      newSubjectCode: 'API201',
      newSubjectName: 'ASP.NET Core API',
      relationType: 'equivalent',
      progressPercent: 58,
      quizScore: 7.6,
      learnedAt: '22/05/2026',
      applyStatus: 'applied',
      supplementPercent: 0,
    },
    {
      id: 'early-prj301-2026-old',
      oldProgramVersion: 'Version 2026 (Cũ)',
      oldSubjectCode: 'PRJ301-Old',
      oldSubjectName: 'Dự án mẫu (Cũ)',
      newSubjectCode: 'PRJ301',
      newSubjectName: 'Dự án mẫu',
      relationType: 'equivalent',
      progressPercent: 0,
      learnedAt: '22/05/2026',
      applyStatus: 'applied',
      supplementPercent: 0,
    },
    {
      id: 'early-test101-2026-old',
      oldProgramVersion: 'Version 2026 (Cũ)',
      oldSubjectCode: 'TEST101-Old',
      oldSubjectName: 'Kiểm thử phần mềm (Cũ)',
      newSubjectCode: 'TEST101',
      newSubjectName: 'Kiểm thử phần mềm',
      relationType: 'equivalent',
      progressPercent: 64,
      quizScore: 8.4,
      learnedAt: '22/05/2026',
      applyStatus: 'applied',
      supplementPercent: 0,
      note: 'Điểm học trước đã được giữ để xét áp dụng khi học chính thức.',
    },
    {
      id: 'early-ai-old-2026',
      oldProgramVersion: 'Version 2026 (Cũ)',
      oldSubjectCode: 'AI301',
      oldSubjectName: 'AI nhập môn',
      newSubjectCode: 'AI401',
      newSubjectName: 'Ứng dụng AI trong sản phẩm phần mềm',
      relationType: 'history_only',
      progressPercent: 45,
      quizScore: 7.2,
      learnedAt: '04/04/2026',
      applyStatus: 'history_only',
      supplementPercent: null,
      note: 'Kết quả này chỉ lưu làm lịch sử tự học và không tính vào điểm chính thức.',
    },
  ],
}

const curriculumVersionDataGD = {
  currentProgram: {
    programId: 'gd-2026',
    programCode: 'GD-FPT-2026',
    majorName: 'Thiết kế đồ họa',
    versionName: 'Version 2026',
    effectiveFromYear: 2026,
    studentCohort: 2026,
  },
  availableVersions: [
    {
      programId: 'gd-2026',
      versionName: 'Version 2026',
      isCurrent: true,
      hasEarlyLearningHistory: false,
    },
    {
      programId: 'gd-2026-old',
      versionName: 'Version 2026 (Cũ)',
      isCurrent: false,
      hasEarlyLearningHistory: true,
    },
  ],
  versionChanges: {
    replacedSubjects: 6,
    changedSubjects: 0,
    addedSubjects: 0,
    historyOnlySubjects: 0,
  },
  earlyLearningHistory: [
    {
      id: 'early-gd101-2026-old',
      oldProgramVersion: 'Version 2026 (Cũ)',
      oldSubjectCode: 'GD101-Old',
      oldSubjectName: 'Cơ sở tạo hình (Cũ)',
      newSubjectCode: 'GD101',
      newSubjectName: 'Cơ sở tạo hình',
      relationType: 'equivalent',
      progressPercent: 100,
      quizScore: 8.5,
      learnedAt: '12/03/2026',
      applyStatus: 'applied',
      supplementPercent: 0,
      note: 'Mã môn đã đổi thành GD101.',
    },
    {
      id: 'early-gd102-2026-old',
      oldProgramVersion: 'Version 2026 (Cũ)',
      oldSubjectCode: 'GD102-Old',
      oldSubjectName: 'Luật xa gần (Cũ)',
      newSubjectCode: 'GD102',
      newSubjectName: 'Luật xa gần',
      relationType: 'equivalent',
      progressPercent: 100,
      quizScore: 8.0,
      learnedAt: '22/05/2026',
      applyStatus: 'applied',
      supplementPercent: 0,
      note: 'Mã môn đã đổi thành GD102.',
    },
    {
      id: 'early-gd103-2026-old',
      oldProgramVersion: 'Version 2026 (Cũ)',
      oldSubjectCode: 'GD103-Old',
      oldSubjectName: 'Lịch sử mỹ thuật (Cũ)',
      newSubjectCode: 'GD103',
      newSubjectName: 'Lịch sử mỹ thuật',
      relationType: 'equivalent',
      progressPercent: 100,
      quizScore: 7.5,
      learnedAt: '04/04/2026',
      applyStatus: 'applied',
      supplementPercent: 0,
      note: 'Mã môn đã đổi thành GD103.',
    },
    {
      id: 'early-gd201-2026-old',
      oldProgramVersion: 'Version 2026 (Cũ)',
      oldSubjectCode: 'GD201-Old',
      oldSubjectName: 'Thiết kế Illustrator (Cũ)',
      newSubjectCode: 'GD201',
      newSubjectName: 'Thiết kế Illustrator',
      relationType: 'equivalent',
      progressPercent: 100,
      quizScore: 8.2,
      learnedAt: '15/05/2026',
      applyStatus: 'applied',
      supplementPercent: 0,
      note: 'Mã môn đã đổi thành GD201.',
    },
    {
      id: 'early-gd202-2026-old',
      oldProgramVersion: 'Version 2026 (Cũ)',
      oldSubjectCode: 'GD202-Old',
      oldSubjectName: 'Xử lý ảnh Photoshop (Cũ)',
      newSubjectCode: 'GD202',
      newSubjectName: 'Xử lý ảnh Photoshop',
      relationType: 'equivalent',
      progressPercent: 100,
      quizScore: 7.8,
      learnedAt: '18/05/2026',
      applyStatus: 'applied',
      supplementPercent: 0,
      note: 'Mã môn đã đổi thành GD202.',
    },
    {
      id: 'early-gd203-2026-old',
      oldProgramVersion: 'Version 2026 (Cũ)',
      oldSubjectCode: 'GD203-Old',
      oldSubjectName: 'Ý tưởng sáng tạo (Cũ)',
      newSubjectCode: 'GD203',
      newSubjectName: 'Ý tưởng sáng tạo',
      relationType: 'equivalent',
      progressPercent: 100,
      quizScore: 8.5,
      learnedAt: '20/05/2026',
      applyStatus: 'applied',
      supplementPercent: 0,
      note: 'Mã môn đã đổi thành GD203.',
    },
  ],
}

const curriculumVersionDataMKT = {
  currentProgram: {
    programId: 'mkt-2026',
    programCode: 'MKT-FPT-2026',
    majorName: 'Marketing',
    versionName: 'Version 2026',
    effectiveFromYear: 2026,
    studentCohort: 2026,
  },
  availableVersions: [
    {
      programId: 'mkt-2026',
      versionName: 'Version 2026',
      isCurrent: true,
      hasEarlyLearningHistory: false,
    },
    {
      programId: 'mkt-2026-old',
      versionName: 'Version 2026 (Cũ)',
      isCurrent: false,
      hasEarlyLearningHistory: true,
    },
  ],
  versionChanges: {
    replacedSubjects: 6,
    changedSubjects: 0,
    addedSubjects: 0,
    historyOnlySubjects: 0,
  },
  earlyLearningHistory: [
    {
      id: 'early-mr101-2026-old',
      oldProgramVersion: 'Version 2026 (Cũ)',
      oldSubjectCode: 'MR101-Old',
      oldSubjectName: 'Nhập môn Marketing (Cũ)',
      newSubjectCode: 'MR101',
      newSubjectName: 'Nhập môn Marketing',
      relationType: 'equivalent',
      progressPercent: 100,
      quizScore: 8.0,
      learnedAt: '12/03/2026',
      applyStatus: 'applied',
      supplementPercent: 0,
      note: 'Mã môn đã đổi thành MR101.',
    },
    {
      id: 'early-mr201-2026-old',
      oldProgramVersion: 'Version 2026 (Cũ)',
      oldSubjectCode: 'MR201-Old',
      oldSubjectName: 'Hành vi người tiêu dùng (Cũ)',
      newSubjectCode: 'MR201',
      newSubjectName: 'Hành vi người tiêu dùng',
      relationType: 'equivalent',
      progressPercent: 100,
      quizScore: 8.2,
      learnedAt: '22/05/2026',
      applyStatus: 'applied',
      supplementPercent: 0,
      note: 'Mã môn đã đổi thành MR201.',
    },
    {
      id: 'early-mr202-2026-old',
      oldProgramVersion: 'Version 2026 (Cũ)',
      oldSubjectCode: 'MR202-Old',
      oldSubjectName: 'Nghiên cứu Marketing (Cũ)',
      newSubjectCode: 'MR202',
      newSubjectName: 'Nghiên cứu Marketing',
      relationType: 'equivalent',
      progressPercent: 100,
      quizScore: 7.8,
      learnedAt: '04/04/2026',
      applyStatus: 'applied',
      supplementPercent: 0,
      note: 'Mã môn đã đổi thành MR202.',
    },
    {
      id: 'early-mr501-2026-old',
      oldProgramVersion: 'Version 2026 (Cũ)',
      oldSubjectCode: 'MR501-Old',
      oldSubjectName: 'Digital Marketing (Cũ)',
      newSubjectCode: 'MR501',
      newSubjectName: 'Digital Marketing',
      relationType: 'equivalent',
      progressPercent: 100,
      quizScore: 7.8,
      learnedAt: '15/05/2026',
      applyStatus: 'applied',
      supplementPercent: 0,
      note: 'Mã môn đã đổi thành MR501.',
    },
    {
      id: 'early-mr502-2026-old',
      oldProgramVersion: 'Version 2026 (Cũ)',
      oldSubjectCode: 'MR502-Old',
      oldSubjectName: 'Quản trị thương hiệu (Cũ)',
      newSubjectCode: 'MR502',
      newSubjectName: 'Quản trị thương hiệu',
      relationType: 'equivalent',
      progressPercent: 100,
      quizScore: 8.2,
      learnedAt: '18/05/2026',
      applyStatus: 'applied',
      supplementPercent: 0,
      note: 'Mã môn đã đổi thành MR502.',
    },
    {
      id: 'early-mr503-2026-old',
      oldProgramVersion: 'Version 2026 (Cũ)',
      oldSubjectCode: 'MR503-Old',
      oldSubjectName: 'Marketing quan hệ khách hàng (Cũ)',
      newSubjectCode: 'MR503',
      newSubjectName: 'Marketing quan hệ khách hàng',
      relationType: 'equivalent',
      progressPercent: 100,
      quizScore: 7.5,
      learnedAt: '20/05/2026',
      applyStatus: 'applied',
      supplementPercent: 0,
      note: 'Mã môn đã đổi thành MR503.',
    },
  ],
}

// ==========================================
// 3. CHI TIẾT BÀI TẬP (Assignment Detail)
// ==========================================
const originalAssignment = {
  id: 'BT-CTDL-02',
  title: 'Bài tập thực hành: Cài đặt Stack bằng Linked List',
  courseCode: 'CTDL101',
  courseTitle: 'Cấu trúc dữ liệu & Giải thuật',
  teacher: 'TS. Nguyễn Minh Khoa',
  class: 'CNTT K28A',
  assignedAt: '10/05/2026',
  deadline: '2026-05-31T23:59:00',
  deadlineDisplay: '23:59 – 31/05/2026',
  status: 'submitted',
  statusLabel: 'Đã nộp',
  description: `Sinh viên cài đặt cấu trúc Stack sử dụng Linked List trong C++. Bài làm phải bao gồm:\n\n1. Định nghĩa Node và lớp Stack với các thao tác: push(), pop(), peek(), isEmpty(), size().\n2. Demo chương trình chuyển đổi biểu thức trung tố sang hậu tố (Infix → Postfix) sử dụng Stack.\n3. File báo cáo PDF giải thích thuật toán, độ phức tạp và kết quả kết thử.\n4. Đặt tên file theo format: MSSV_HoTen_BT02.zip`,
}

const originalSubmissionRules = {
  allowedFormats: ['.zip', '.rar', '.pdf', '.docx'],
  maxSizeMB: 20,
  maxAttempts: 3,
  currentAttempt: 2,
  canSubmit: true,
  note: 'File .zip phải chứa toàn bộ source code và file báo cáo PDF. Không nộp file .exe.',
}

const originalAttachments = [
  { id: 'a1', name: 'Đề bài BT02 – Stack.pdf', size: '234 KB', type: 'pdf' },
  { id: 'a2', name: 'Template báo cáo.docx', size: '45 KB', type: 'docx' },
  { id: 'a3', name: 'Test cases mẫu.zip', size: '12 KB', type: 'zip' },
]

const originalSubmissions = [
  {
    id: 's1', attempt: 1,
    submittedAt: '15/05/2026 – 10:32', status: 'graded', statusLabel: 'Đã chấm',
    onTime: true, timeLabel: 'Đúng hạn',
    file: 'BT2101_NguyenVanA_BT02.zip', fileSize: '2.3 MB',
    note: 'Lần nộp đầu tiên.', isLatest: false,
  },
  {
    id: 's2', attempt: 2,
    submittedAt: '18/05/2026 – 22:14', status: 'graded', statusLabel: 'Đã chấm',
    onTime: true, timeLabel: 'Đúng hạn',
    file: 'BT2101_NguyenVanA_BT02_v2.zip', fileSize: '2.6 MB',
    note: 'Cập nhật phần báo cáo.', isLatest: true,
  },
]

const originalPlagiarismResult = {
  status: 'safe',
  percentage: 12,
  checkedAt: '18/05/2026 – 22:20',
  detail: 'Tỷ lệ trùng lặp trong ngưỡng cho phép. Bài làm được xác nhận hợp lệ.',
}

const originalFeedback = {
  graded: true,
  score: 8.5,
  maxScore: 10,
  gradedAt: '20/05/2026',
  gradedBy: 'TS. Nguyễn Minh Khoa',
  comment: 'Bài làm đúng yêu cầu, code rõ ràng và có chú thích tốt. Phần Infix→Postfix cần giải thích thuật toán rõ hơn trong báo cáo. Các test case cơ bản đều pass.',
  aiSuggestion: 'Bài làm đúng cấu trúc. Cần bổ sung phân tích độ phức tạp O(n) và xử lý trường hợp biên (stack rỗng, ký tự đặc biệt).',
  rubric: [
    { label: 'Cài đặt Stack (push/pop/peek)', score: 3.0, max: 3.0 },
    { label: 'Demo Infix → Postfix', score: 2.5, max: 3.0 },
    { label: 'Báo cáo & giải thích', score: 2.0, max: 3.0 },
    { label: 'Phong cách code & comment', score: 1.0, max: 1.0 },
  ],
}

// ==========================================
// 4. CHI TIẾT KHÓA HỌC (Course Detail)
// ==========================================
const originalCourse = {
  id: 'CTDL101',
  title: 'Cấu trúc dữ liệu & Giải thuật',
  code: 'CTDL101',
  teacher: 'TS. Nguyễn Minh Khoa',
  semester: 'HK2 2025–2026',
  credits: 3,
  coverGradient: 'from-blue-700 via-blue-600 to-cyan-500',
  description:
    'Môn học cung cấp kiến thức nền tảng về cấu trúc dữ liệu và các thuật toán sắp xếp, tìm kiếm, đệ quy phổ biến trong lập trình.',
}

const originalStats = [
  { label: 'Tiến độ', value: '72', unit: '%', icon: 'Gauge', tone: 'blue', progress: 72, hint: '9/12 bài đã hoàn thành' },
  { label: 'Bài học', value: '9', unit: '/12', icon: 'BookOpenCheck', tone: 'green', progress: 75, hint: 'Đã hoàn thành 9 bài' },
  { label: 'Bài tập', value: '2', unit: 'mục', icon: 'ClipboardList', tone: 'orange', progress: 40, hint: '1 bài gần đến hạn' },
  { label: 'Tài liệu', value: '18', unit: 'file', icon: 'Files', tone: 'violet', progress: 60, hint: 'PDF, video, quiz' },
]

const originalLessons = [
  {
    id: 'ch1',
    chapter: 'Chương 1',
    title: 'Ôn tập nền tảng lập trình',
    description: 'Ôn lại kiến thức C/C++, con trỏ, cấp phát động và các khái niệm OOP cơ bản.',
    status: 'completed',
    badge: 'Hoàn thành',
    tone: 'green',
    icon: 'CheckCircle2',
    meta: ['3 bài học', '1 quiz'],
    progress: 100,
    lessons: [
      { id: 'l1-1', title: 'Con trỏ và cấp phát động', duration: '18:24', status: 'completed' },
      { id: 'l1-2', title: 'Struct và Class cơ bản', duration: '22:10', status: 'completed' },
      { id: 'l1-3', title: 'Quiz ôn tập chương 1', duration: '10 câu', status: 'completed', type: 'quiz' },
    ],
  },
  {
    id: 'ch2',
    chapter: 'Chương 2',
    title: 'Cấu trúc dữ liệu tuyến tính',
    description: 'Danh sách liên kết, Stack, Queue và ứng dụng trong các bài toán thực tế.',
    status: 'active',
    badge: 'Đang học',
    tone: 'blue',
    icon: 'ListTree',
    meta: ['4 bài học', '2 tài liệu', '1 bài tập'],
    progress: 50,
    lessons: [
      { id: 'l2-1', title: 'Danh sách liên kết đơn', duration: '26:30', status: 'completed' },
      { id: 'l2-2', title: 'Stack và ứng dụng', duration: '20:45', status: 'active' },
      { id: 'l2-3', title: 'Queue và vòng lặp', duration: '24:00', status: 'locked' },
      { id: 'l2-4', title: 'Bài tập thực hành', duration: '–', status: 'locked', type: 'assignment' },
    ],
  },
  {
    id: 'ch3',
    chapter: 'Chương 3',
    title: 'Cây nhị phân và đồ thị',
    description: 'BST, cây AVL, duyệt cây, DFS, BFS và ứng dụng trong tìm đường.',
    status: 'locked',
    badge: 'Bị khóa',
    tone: 'slate',
    icon: 'Lock',
    meta: ['5 bài học', '2 bài tập'],
    progress: 0,
    lessons: [],
  },
  {
    id: 'ch4',
    chapter: 'Chương 4',
    title: 'Sắp xếp & Tìm kiếm',
    description: 'Bubble Sort, Quick Sort, Merge Sort, Binary Search và phân tích độ phức tạp.',
    status: 'upcoming',
    badge: 'Dự kiến',
    tone: 'amber',
    icon: 'GitBranch',
    meta: ['4 bài học', '1 quiz cuối kỳ'],
    progress: 0,
    lessons: [],
  },
]

const originalCurrentLesson = {
  id: 'l2-2',
  chapterId: 'ch2',
  chapterTitle: 'Chương 2: Cấu trúc dữ liệu tuyến tính',
  title: 'Stack và ứng dụng trong lập trình',
  lessonType: 'video',
  videoUrl: '',
  duration: '20:45',
  watchedSeconds: 743,
  totalSeconds: 1245,
  durationSeconds: 1245,
  allowSeek: false,
  pauseOnBlur: true,
  minWatchPercentToComplete: 80,
  progressPercent: 60,
  maxWatchedSeconds: 743,
  completedAt: null,
  videoThumb: null,
  documentTitle: 'Slide bài giảng Chương 2.pdf',
  documentPages: 34,
  documentCurrentPage: 12,
}

const originalQuizQuestions = [
  {
    id: 'q1',
    question: 'Cấu trúc dữ liệu nào hoạt động theo nguyên tắc LIFO (Last In First Out)?',
    options: ['Queue', 'Stack', 'Linked List', 'Tree'],
    correctIndex: 1,
  },
  {
    id: 'q2',
    question: 'Độ phức tạp thời gian trung bình của thuật toán Quick Sort là?',
    options: ['O(n)', 'O(n log n)', 'O(n²)', 'O(log n)'],
    correctIndex: 1,
  },
]

const originalComments = [
  {
    id: 'c1',
    author: 'Trần Văn An',
    initials: 'TA',
    avatarColor: 'from-blue-500 to-cyan-400',
    content: 'Thầy ơi, phần push/pop trong Stack có thể giải thích thêm về trường hợp stack overflow không ạ?',
    time: '2 giờ trước',
    likes: 4,
    replies: [
      {
        id: 'r1',
        author: 'TS. Nguyễn Minh Khoa',
        initials: 'GV',
        avatarColor: 'from-violet-600 to-indigo-500',
        content: 'Stack overflow xảy ra khi đệ quy quá sâu hoặc cấp phát stack vượt giới hạn. Mình sẽ bổ sung slide vào buổi sau nhé.',
        time: '1 giờ trước',
        isTeacher: true,
      },
    ],
  },
  {
    id: 'c2',
    author: 'Lê Thị Bích',
    initials: 'LB',
    avatarColor: 'from-emerald-500 to-teal-400',
    content: 'Bài giảng rất dễ hiểu. Mình đã làm bài tập và pass được 3/4 test case rồi ạ!',
    time: '5 giờ trước',
    likes: 7,
    replies: [],
  },
]

const originalTimeline = [
  {
    id: 't1',
    title: 'Tiếp tục bài đang học',
    description: 'Stack và ứng dụng – Chương 2',
    time: 'Đang học',
    tone: 'blue',
    icon: 'Play',
  },
  {
    id: 't2',
    title: 'Hoàn thành Quiz chương 2',
    description: 'Nộp trước 31/05/2026',
    time: 'Sắp đến hạn',
    tone: 'orange',
    icon: 'ClipboardList',
  },
  {
    id: 't3',
    title: 'Mở khóa Chương 3',
    description: 'Hoàn thành 100% chương 2 để tiếp tục',
    time: 'Điều kiện tiên quyết',
    tone: 'slate',
    icon: 'Lock',
  },
  {
    id: 't4',
    title: 'Thi cuối kỳ',
    description: 'Dự kiến tuần 16 – phòng thi A201',
    time: 'Dự kiến 20/06',
    tone: 'violet',
    icon: 'GraduationCap',
  },
]

const originalAISummary = {
  points: [
    'Stack là cấu trúc LIFO – phần tử cuối vào sẽ được lấy ra đầu tiên.',
    'Hai thao tác cơ bản: push (thêm vào đỉnh) và pop (lấy khỏi đỉnh), đều O(1).',
    'Ứng dụng phổ biến: kiểm tra dấu ngoặc, đảo ngược chuỗi, thực thi đệ quy.',
    'Cần phân biệt stack cấp phát tĩnh (array-based) và cấp phát động (linked list-based).',
    'Bài tiếp theo sẽ học về Queue – cấu trúc FIFO bổ sung cho Stack.',
  ],
}

// ==========================================
// 5. CA THI & LUỒNG THI (Exams)
// ==========================================
const originalExams = [
  {
    id: 'exam-toan-001',
    title: 'Thi giữa kỳ Toán rời rạc',
    subject: 'Toán rời rạc',
    subjectCode: 'TOAN201',
    classCode: 'CNTT-K28A',
    openAt: '2026-05-22T07:30:00',
    closeAt: '2026-05-22T09:30:00',
    durationMinutes: 90,
    totalQuestions: 30,
    examType: 'multiple_choice',
    examTypeLabel: 'Trắc nghiệm',
    status: 'upcoming',
    statusLabel: 'Sắp diễn ra',
    room: 'P.201',
    teacher: 'TS. Lê Văn Minh',
    attempts: 0,
    maxAttempts: 1,
    riskScore: 18,
    majorName: 'Phát triển phần mềm',
    semesterName: 'Kỳ 1',
    blockName: 'Block 1',
    score: null,
  },
  {
    id: 'exam-ctdl-002',
    title: 'Quiz Cấu trúc dữ liệu – Cây & Đồ thị',
    subject: 'Cấu trúc dữ liệu & Giải thuật',
    subjectCode: 'CTDL101',
    classCode: 'CNTT-K28A',
    openAt: '2026-05-21T10:00:00',
    closeAt: '2026-05-21T23:59:00',
    durationMinutes: 15,
    totalQuestions: 12,
    examType: 'multiple_choice',
    examTypeLabel: 'Trắc nghiệm',
    status: 'open',
    statusLabel: 'Đang mở',
    room: null,
    teacher: 'ThS. Nguyễn Minh Khoa',
    attempts: 0,
    maxAttempts: 1,
    riskScore: 24,
    majorName: 'Phát triển phần mềm',
    semesterName: 'Kỳ 1',
    blockName: 'Block 2',
    score: null,
  },
  {
    id: 'exam-ltw-003',
    title: 'Kiểm tra giữa kỳ Lập trình Web',
    subject: 'Lập trình Web',
    subjectCode: 'LTW301',
    classCode: 'CNTT-K28B',
    openAt: '2026-05-10T08:00:00',
    closeAt: '2026-05-10T10:00:00',
    durationMinutes: 120,
    totalQuestions: 25,
    examType: 'mixed',
    examTypeLabel: 'Kết hợp',
    status: 'completed',
    statusLabel: 'Đã hoàn thành',
    room: 'Lab 3',
    teacher: 'TS. Trần Thị Lan',
    attempts: 1,
    maxAttempts: 1,
    riskScore: 12,
    resultId: 'result-ltw-003',
    majorName: 'Thiết kế đồ họa',
    semesterName: 'Kỳ 2',
    blockName: 'Block 1',
    score: 8.8,
  },
  {
    id: 'exam-mmt-004',
    title: 'Thi cuối kỳ Mạng máy tính',
    subject: 'Mạng máy tính',
    subjectCode: 'MMT401',
    classCode: 'CNTT-K28A',
    openAt: '2026-05-18T13:00:00',
    closeAt: '2026-05-18T15:00:00',
    durationMinutes: 120,
    totalQuestions: 40,
    examType: 'multiple_choice',
    examTypeLabel: 'Trắc nghiệm',
    status: 'awaiting_result',
    statusLabel: 'Chờ công bố điểm',
    room: 'Hội trường A',
    teacher: 'PGS. Hoàng Đức Bình',
    attempts: 1,
    maxAttempts: 1,
    riskScore: 5,
    majorName: 'Digital Marketing',
    semesterName: 'Kỳ 2',
    blockName: 'Block 2',
    score: null,
  },
  {
    id: 'quiz-net-005',
    title: 'Quiz C# cơ bản',
    subject: 'Lập trình C#',
    subjectCode: 'NET101',
    classCode: 'PM-K29A',
    majorName: 'Phát triển phần mềm',
    semesterName: 'Kỳ 1',
    blockName: 'Block 2',
    openAt: '2026-05-23T08:00:00',
    closeAt: '2026-05-23T20:00:00',
    durationMinutes: 30,
    totalQuestions: 25,
    examType: 'multiple_choice',
    examTypeLabel: 'Quiz',
    status: 'open',
    attempts: 0,
    maxAttempts: 1,
    score: null,
  },
  {
    id: 'quiz-ui-006',
    title: 'Kiểm tra nguyên lý thị giác',
    subject: 'Cơ sở thiết kế đồ họa',
    subjectCode: 'DES102',
    classCode: 'TK-K29B',
    majorName: 'Thiết kế đồ họa',
    semesterName: 'Kỳ 1',
    blockName: 'Block 1',
    openAt: '2026-05-24T09:00:00',
    closeAt: '2026-05-24T11:00:00',
    durationMinutes: 45,
    totalQuestions: 20,
    examType: 'mixed',
    examTypeLabel: 'Kết hợp',
    status: 'upcoming',
    attempts: 0,
    maxAttempts: 1,
    score: null,
  },
  {
    id: 'quiz-mkt-007',
    title: 'Mini test SEO & Analytics',
    subject: 'Digital Marketing căn bản',
    subjectCode: 'MKT201',
    classCode: 'DM-K29A',
    majorName: 'Digital Marketing',
    semesterName: 'Kỳ 2',
    blockName: 'Block 1',
    openAt: '2026-05-18T13:00:00',
    closeAt: '2026-05-18T14:00:00',
    durationMinutes: 35,
    totalQuestions: 30,
    examType: 'multiple_choice',
    examTypeLabel: 'Quiz',
    status: 'completed',
    attempts: 1,
    maxAttempts: 1,
    resultId: 'result-mkt-007',
    score: 9.1,
  },
  {
    id: 'exam-bus-008',
    title: 'Thi giữa kỳ Quản trị học',
    subject: 'Quản trị học',
    subjectCode: 'BUS101',
    classCode: 'QTKD-K29A',
    majorName: 'Quản trị kinh doanh',
    semesterName: 'Kỳ 1',
    blockName: 'Block 2',
    openAt: '2026-05-25T14:00:00',
    closeAt: '2026-05-25T16:00:00',
    durationMinutes: 90,
    totalQuestions: 40,
    examType: 'multiple_choice',
    examTypeLabel: 'Thi',
    status: 'not_open',
    attempts: 0,
    maxAttempts: 1,
    score: null,
  },
  {
    id: 'quiz-db-009',
    title: 'Quiz SQL nâng cao',
    subject: 'Cơ sở dữ liệu',
    subjectCode: 'DBI202',
    classCode: 'PM-K29C',
    majorName: 'Phát triển phần mềm',
    semesterName: 'Kỳ 2',
    blockName: 'Block 2',
    openAt: '2026-05-20T08:00:00',
    closeAt: '2026-05-20T23:59:00',
    durationMinutes: 25,
    totalQuestions: 18,
    examType: 'multiple_choice',
    examTypeLabel: 'Quiz',
    status: 'expired',
    attempts: 0,
    maxAttempts: 1,
    score: null,
  },
  {
    id: 'quiz-brand-010',
    title: 'Nhận diện thương hiệu',
    subject: 'Brand Design',
    subjectCode: 'DES204',
    classCode: 'TK-K29A',
    majorName: 'Thiết kế đồ họa',
    semesterName: 'Kỳ 2',
    blockName: 'Block 2',
    openAt: '2026-05-26T09:00:00',
    closeAt: '2026-05-26T12:00:00',
    durationMinutes: 40,
    totalQuestions: 22,
    examType: 'mixed',
    examTypeLabel: 'Kết hợp',
    status: 'early_learning',
    attempts: 0,
    maxAttempts: 1,
    isEarlyLearning: true,
    score: null,
  },
  {
    id: 'quiz-crm-011',
    title: 'Kiểm tra CRM & hành vi khách hàng',
    subject: 'Marketing quan hệ khách hàng',
    subjectCode: 'MKT305',
    classCode: 'DM-K29B',
    majorName: 'Digital Marketing',
    semesterName: 'Kỳ 3',
    blockName: 'Block 1',
    openAt: '2026-05-27T15:00:00',
    closeAt: '2026-05-27T16:00:00',
    durationMinutes: 30,
    totalQuestions: 24,
    examType: 'multiple_choice',
    examTypeLabel: 'Quiz',
    status: 'upcoming',
    attempts: 0,
    maxAttempts: 1,
    score: null,
  },
  {
    id: 'exam-fin-012',
    title: 'Thi cuối kỳ Tài chính doanh nghiệp',
    subject: 'Tài chính doanh nghiệp',
    subjectCode: 'FIN301',
    classCode: 'QTKD-K29B',
    majorName: 'Quản trị kinh doanh',
    semesterName: 'Kỳ 3',
    blockName: 'Block 2',
    openAt: '2026-05-15T08:00:00',
    closeAt: '2026-05-15T10:00:00',
    durationMinutes: 120,
    totalQuestions: 35,
    examType: 'essay',
    examTypeLabel: 'Tự luận',
    status: 'awaiting_result',
    attempts: 1,
    maxAttempts: 1,
    score: null,
  },
  {
    id: 'quiz-api-013',
    title: 'REST API & Authentication',
    subject: 'Lập trình Web API',
    subjectCode: 'API302',
    classCode: 'PM-K29B',
    majorName: 'Phát triển phần mềm',
    semesterName: 'Kỳ 3',
    blockName: 'Block 1',
    openAt: '2026-05-28T10:00:00',
    closeAt: '2026-05-28T22:00:00',
    durationMinutes: 45,
    totalQuestions: 28,
    examType: 'mixed',
    examTypeLabel: 'Kết hợp',
    status: 'open',
    attempts: 0,
    maxAttempts: 2,
    score: null,
  },
  {
    id: 'quiz-ux-014',
    title: 'UX Research quick check',
    subject: 'Nghiên cứu người dùng',
    subjectCode: 'UXR201',
    classCode: 'TK-K29C',
    majorName: 'Thiết kế đồ họa',
    semesterName: 'Kỳ 3',
    blockName: 'Block 1',
    openAt: '2026-05-19T07:30:00',
    closeAt: '2026-05-19T08:30:00',
    durationMinutes: 30,
    totalQuestions: 16,
    examType: 'multiple_choice',
    examTypeLabel: 'Quiz',
    status: 'completed',
    attempts: 1,
    maxAttempts: 1,
    resultId: 'result-ux-014',
    score: 7.6,
  },
  {
    id: 'quiz-ops-015',
    title: 'Vận hành chuỗi cung ứng',
    subject: 'Quản trị vận hành',
    subjectCode: 'OPS202',
    classCode: 'QTKD-K29C',
    majorName: 'Quản trị kinh doanh',
    semesterName: 'Kỳ 2',
    blockName: 'Block 1',
    openAt: '2026-05-29T13:00:00',
    closeAt: '2026-05-29T15:00:00',
    durationMinutes: 60,
    totalQuestions: 32,
    examType: 'multiple_choice',
    examTypeLabel: 'Thi',
    status: 'not_open',
    attempts: 0,
    maxAttempts: 1,
    score: null,
  },
  {
    id: 'quiz-ads-016',
    title: 'Campaign Planning Simulation',
    subject: 'Quảng cáo số',
    subjectCode: 'ADS203',
    classCode: 'DM-K29C',
    majorName: 'Digital Marketing',
    semesterName: 'Kỳ 2',
    blockName: 'Block 2',
    openAt: '2026-05-30T09:00:00',
    closeAt: '2026-05-30T11:00:00',
    durationMinutes: 50,
    totalQuestions: 26,
    examType: 'mixed',
    examTypeLabel: 'Kết hợp',
    status: 'early_learning',
    attempts: 0,
    maxAttempts: 1,
    isEarlyLearning: true,
    score: null,
  },
]

export const mockExamDetail = {
  id: 'exam-ctdl-002',
  title: 'Quiz Cấu trúc dữ liệu – Cây & Đồ thị',
  subject: 'Cấu trúc dữ liệu & Giải thuật',
  subjectCode: 'CTDL101',
  classCode: 'CNTT-K28A',
  teacher: 'ThS. Nguyễn Minh Khoa',
  openAt: '2026-05-21T10:00:00',
  closeAt: '2026-05-21T23:59:00',
  durationMinutes: 15,
  totalQuestions: 12,
  totalPoints: 10,
  examType: 'multiple_choice',
  examTypeLabel: 'Trắc nghiệm',
  maxAttempts: 1,
  status: 'open',
  rules: [
    'Bạn chỉ được phép làm bài một lần duy nhất.',
    'Không được thoát khỏi chế độ toàn màn hình trong khi làm bài.',
    'Không được chuyển tab hoặc cửa sổ trình duyệt.',
    'Mọi hành động copy/paste sẽ bị ghi nhận là vi phạm.',
    'Bài thi sẽ tự động nộp khi hết thời gian.',
  ],
}

export const mockPreflightChecks = [
  { id: 'browser', label: 'Trình duyệt hỗ trợ', description: 'Chrome/Edge ≥ 110', status: 'pass', detail: 'Chrome 124 – Đạt yêu cầu', icon: 'Globe' },
  { id: 'fullscreen', label: 'Chế độ toàn màn hình', description: 'Fullscreen API khả dụng', status: 'pass', detail: 'Fullscreen API khả dụng', icon: 'Maximize' },
  { id: 'screen_share', label: 'Chia sẻ màn hình', description: 'Quyền Screen Capture', status: 'warning', detail: 'Chưa cấp quyền – Cần cho phép trước khi vào thi', icon: 'MonitorCheck' },
  { id: 'network', label: 'Kết nối mạng', description: 'Độ trễ < 200ms', status: 'pass', detail: 'Độ trễ: 42ms – Ổn định', icon: 'Wifi' },
  { id: 'extensions', label: 'Tiện ích bị cấm', description: 'Không có extension gian lận', status: 'pass', detail: 'Không phát hiện tiện ích bị cấm', icon: 'Puzzle' },
  { id: 'vm', label: 'Máy ảo / Remote Desktop', description: 'Không phát hiện VM', status: 'pass', detail: 'Không phát hiện VM hoặc RDP', icon: 'Server' },
  { id: 'ai_tools', label: 'Công cụ AI / Dịch thuật', description: 'Không có tab AI', status: 'pass', detail: 'Không phát hiện công cụ bị cấm', icon: 'Bot' },
]

export const mockPreflightChecksHighRisk = [
  { id: 'browser', label: 'Trình duyệt hỗ trợ', description: 'Chrome/Edge ≥ 110', status: 'pass', detail: 'Chrome 124 – Đạt yêu cầu', icon: 'Globe' },
  { id: 'fullscreen', label: 'Chế độ toàn màn hình', description: 'Fullscreen API khả dụng', status: 'fail', detail: 'Không thể kích hoạt fullscreen – Kiểm tra cài đặt', icon: 'Maximize' },
  { id: 'screen_share', label: 'Chia sẻ màn hình', description: 'Quyền Screen Capture', status: 'fail', detail: 'Quyền bị từ chối – Vui lòng cấp quyền để tiếp tục', icon: 'MonitorCheck' },
  { id: 'network', label: 'Kết nối mạng', description: 'Độ trễ < 200ms', status: 'warning', detail: 'Độ trễ: 185ms – Không ổn định', icon: 'Wifi' },
  { id: 'extensions', label: 'Tiện ích bị cấm', description: 'Không có extension gian lận', status: 'fail', detail: 'Phát hiện: "AI Answer Helper" – Hãy gỡ cài đặt', icon: 'Puzzle' },
  { id: 'vm', label: 'Máy ảo / Remote Desktop', description: 'Không phát hiện VM', status: 'pass', detail: 'Không phát hiện VM hoặc RDP', icon: 'Server' },
  { id: 'ai_tools', label: 'Công cụ AI / Dịch thuật', description: 'Không có tab AI', status: 'warning', detail: 'Phát hiện tab ChatGPT – Hãy đóng trước khi thi', icon: 'Bot' },
]

export const mockQuestions = [
  {
    id: 'q1', order: 1, type: 'single_choice', typeLabel: 'Chọn một đáp án', points: 1.0,
    content: 'Cây nhị phân tìm kiếm (BST) có tính chất nào sau đây?',
    choices: [
      { id: 'a', label: 'A', text: 'Nút trái luôn lớn hơn nút cha' },
      { id: 'b', label: 'B', text: 'Nút phải luôn nhỏ hơn nút cha' },
      { id: 'c', label: 'C', text: 'Nút trái nhỏ hơn nút cha, nút phải lớn hơn nút cha' },
      { id: 'd', label: 'D', text: 'Tất cả nút có cùng giá trị' },
    ],
    answer: null, flagged: false,
  },
  {
    id: 'q2', order: 2, type: 'single_choice', typeLabel: 'Chọn một đáp án', points: 1.0,
    content: 'Độ phức tạp thời gian trung bình của phép tìm kiếm trong BST cân bằng là?',
    choices: [
      { id: 'a', label: 'A', text: 'O(1)' },
      { id: 'b', label: 'B', text: 'O(log n)' },
      { id: 'c', label: 'C', text: 'O(n)' },
      { id: 'd', label: 'D', text: 'O(n log n)' },
    ],
    answer: 'b', flagged: false,
  },
  {
    id: 'q3', order: 3, type: 'single_choice', typeLabel: 'Chọn một đáp án', points: 1.0,
    content: 'Trong duyệt cây theo thứ tự Inorder (LNR), thứ tự duyệt là?',
    choices: [
      { id: 'a', label: 'A', text: 'Gốc → Trái → Phải' },
      { id: 'b', label: 'B', text: 'Trái → Gốc → Phải' },
      { id: 'c', label: 'C', text: 'Trái → Phải → Gốc' },
      { id: 'd', label: 'D', text: 'Phải → Gốc → Trái' },
    ],
    answer: 'b', flagged: true,
  },
  {
    id: 'q4', order: 4, type: 'multiple_choice', typeLabel: 'Chọn nhiều đáp án', points: 1.5,
    content: 'Những cấu trúc dữ liệu nào có thể biểu diễn đồ thị? (Chọn tất cả đáp án đúng)',
    choices: [
      { id: 'a', label: 'A', text: 'Ma trận kề (Adjacency Matrix)' },
      { id: 'b', label: 'B', text: 'Danh sách kề (Adjacency List)' },
      { id: 'c', label: 'C', text: 'Ngăn xếp (Stack)' },
      { id: 'd', label: 'D', text: 'Danh sách cạnh (Edge List)' },
    ],
    answer: [], flagged: false,
  },
  {
    id: 'q5', order: 5, type: 'multiple_choice', typeLabel: 'Chọn nhiều đáp án', points: 1.5,
    content: 'Thuật toán nào dùng để duyệt đồ thị? (Chọn tất cả đáp án đúng)',
    choices: [
      { id: 'a', label: 'A', text: 'BFS (Breadth-First Search)' },
      { id: 'b', label: 'B', text: 'DFS (Depth-First Search)' },
      { id: 'c', label: 'C', text: 'Binary Search' },
      { id: 'd', label: 'D', text: 'Dijkstra' },
    ],
    answer: ['a', 'b'], flagged: true,
  },
  {
    id: 'q6', order: 6, type: 'short_answer', typeLabel: 'Trả lời ngắn', points: 1.0,
    content: 'Viết tên thuật toán sắp xếp có độ phức tạp trung bình O(n log n) và được cài đặt bằng đệ quy phân chia đôi mảng.',
    answer: '', flagged: false,
  },
  {
    id: 'q7', order: 7, type: 'short_answer', typeLabel: 'Trả lời ngắn', points: 1.0,
    content: 'Cho cây nhị phân với 7 nút, chiều cao tối thiểu của cây là bao nhiêu?',
    answer: '', flagged: false,
  },
  {
    id: 'q8', order: 8, type: 'essay', typeLabel: 'Tự luận', points: 3.0,
    content: 'Phân tích và so sánh BFS và Dijkstra:\n1. Trình bày ý tưởng chính.\n2. Nêu độ phức tạp thời gian và không gian.\n3. Khi nào dùng BFS, khi nào dùng Dijkstra?\n4. Cho ví dụ minh họa.',
    answer: '', flagged: false,
  },
  {
    id: 'q9', order: 9, type: 'single_choice', typeLabel: 'Chọn một đáp án', points: 1.0,
    content: 'Cây AVL là cây BST tự cân bằng. Điều kiện cân bằng của cây AVL là?',
    choices: [
      { id: 'a', label: 'A', text: 'Chênh lệch chiều cao cây con ≤ 2' },
      { id: 'b', label: 'B', text: 'Chênh lệch chiều cao cây con ≤ 1' },
      { id: 'c', label: 'C', text: 'Tất cả lá cùng chiều cao' },
      { id: 'd', label: 'D', text: 'Số nút cây con trái và phải bằng nhau' },
    ],
    answer: null, flagged: false,
  },
  {
    id: 'q10', order: 10, type: 'single_choice', typeLabel: 'Chọn một đáp án', points: 1.0,
    content: 'Thuật toán Prim và Kruskal đều dùng để tìm?',
    choices: [
      { id: 'a', label: 'A', text: 'Đường đi ngắn nhất' },
      { id: 'b', label: 'B', text: 'Cây khung nhỏ nhất (Minimum Spanning Tree)' },
      { id: 'c', label: 'C', text: 'Chu trình Euler' },
      { id: 'd', label: 'D', text: 'Thành phần liên thông mạnh' },
    ],
    answer: null, flagged: false,
  },
  {
    id: 'q11', order: 11, type: 'single_choice', typeLabel: 'Chọn một đáp án', points: 1.0,
    content: 'Trong đồ thị có hướng, sắp xếp tô-pô áp dụng được khi?',
    choices: [
      { id: 'a', label: 'A', text: 'Đồ thị có chu trình' },
      { id: 'b', label: 'B', text: 'Đồ thị không có chu trình (DAG)' },
      { id: 'c', label: 'C', text: 'Đồ thị vô hướng' },
      { id: 'd', label: 'D', text: 'Đồ thị đầy đủ' },
    ],
    answer: null, flagged: false,
  },
  {
    id: 'q12', order: 12, type: 'single_choice', typeLabel: 'Chọn một đáp án', points: 1.0,
    content: 'Hash table giải quyết xung đột bằng phương pháp "Chaining" bằng cách?',
    choices: [
      { id: 'a', label: 'A', text: 'Tìm ô kế tiếp còn trống trong bảng' },
      { id: 'b', label: 'B', text: 'Sử dụng danh sách liên kết tại mỗi ô hash' },
      { id: 'c', label: 'C', text: 'Tăng kích thước bảng khi xảy ra xung đột' },
      { id: 'd', label: 'D', text: 'Xóa phần tử cũ và chèn phần tử mới' },
    ],
    answer: null, flagged: false,
  },
]

export const mockViolationLogs = [
  { id: 'v1', type: 'tab_switch', label: 'Chuyển tab trình duyệt', description: 'Phát hiện sinh viên chuyển sang tab khác', severity: 'high', severityLabel: 'Cao', occurredAt: '10:04:23', count: 1 },
  { id: 'v2', type: 'copy_paste', label: 'Sao chép / Dán nội dung', description: 'Phát hiện hành động Ctrl+C hoặc Ctrl+V', severity: 'medium', severityLabel: 'Trung bình', occurredAt: '10:06:11', count: 2 },
  { id: 'v3', type: 'fullscreen_exit', label: 'Thoát toàn màn hình', description: 'Sinh viên nhấn Escape hoặc chuyển cửa sổ', severity: 'high', severityLabel: 'Cao', occurredAt: '10:07:45', count: 1 },
]

export const mockExamResult = {
  id: 'result-ltw-003',
  examId: 'exam-ltw-003',
  examTitle: 'Kiểm tra giữa kỳ Lập trình Web',
  subject: 'Lập trình Web',
  subjectCode: 'LTW301',
  studentId: 'SV2026001',
  studentName: 'Nguyễn Văn An',
  startedAt: '2026-05-10T08:00:00',
  submittedAt: '2026-05-10T09:42:00',
  durationActual: 102,
  durationAllowed: 120,
  totalQuestions: 25,
  answeredQuestions: 25,
  correctAnswers: 22,
  incorrectAnswers: 2,
  skippedAnswers: 1,
  score: 8.8,
  maxScore: 10,
  grade: 'A',
  status: 'graded',
  statusLabel: 'Đã có điểm',
  essayStatus: 'graded',
  teacherComment: 'Bài làm tốt, nắm vững kiến thức HTML/CSS và JavaScript cơ bản. Phần REST API cần bổ sung thêm hiểu biết về status code.',
  aiSuggestion: 'Sinh viên đạt trên 88% câu trắc nghiệm. Cần ôn thêm HTTP Methods và RESTful API conventions cho bài cuối kỳ.',
  violations: [
    { type: 'copy_paste', label: 'Sao chép / Dán', severity: 'low', severityLabel: 'Thấp', occurredAt: '08:23:10', count: 1 },
  ],
  scoreBreakdown: [
    { section: 'HTML & CSS cơ bản', correct: 8, total: 8, points: 3.2 },
    { section: 'JavaScript & DOM', correct: 7, total: 8, points: 2.8 },
    { section: 'REST API & Fetch', correct: 5, total: 7, points: 2.0 },
    { section: 'Tự luận – Thiết kế giao diện', score: 0.8, maxScore: 2.0, status: 'graded' },
  ],
}

// ==========================================
// 6. BẢNG ĐIỂM (Grades)
// ==========================================
const originalGradeSummary = {
  cumulativeGPA: 3.42,
  maxGPA: 4.0,
  currentSemesterGPA: 3.55,
  previousSemesterGPA: 3.35,
  gpaChange: +0.20,
  classification: 'Giỏi',
  totalCreditsEarned: 86,
  totalCreditsRequired: 120,
  totalSubjectsPassed: 24,
  totalSubjectsFailed: 1,
  incompleteSubjects: 1,
  pendingSubjects: 1,
  riskAlertCount: 2,
}

const originalSemesterGPA = [
  { semester: 'HK1 2024-2025', semesterGPA: 3.10, cumulativeGPA: 3.10, credits: 18, year: 1 },
  { semester: 'HK2 2024-2025', semesterGPA: 3.28, cumulativeGPA: 3.19, credits: 20, year: 1 },
  { semester: 'HK1 2025-2026', semesterGPA: 3.35, cumulativeGPA: 3.24, credits: 22, year: 2 },
  { semester: 'HK2 2025-2026', semesterGPA: 3.55, cumulativeGPA: 3.42, credits: 26, year: 2 },
]

const originalSubjects = [
  {
    id: 's1',
    name: 'Lập trình Web',
    code: 'LTW301',
    credits: 4,
    semester: 'HK2 2025-2026',
    teacher: 'TS. Trần Thị Lan',
    processScore: 8.5,
    midtermScore: 9.0,
    finalScore: 9.0,
    gpa: 9.0,
    letterGrade: 'A',
    status: 'pass',
    statusLabel: 'Đạt',
    note: 'Hoàn thành bài tập lớn, đúng hạn và chất lượng tốt.',
    publishedAt: '10/05/2026',
  },
  {
    id: 's2',
    name: 'Cấu trúc dữ liệu & Giải thuật',
    code: 'CTDL101',
    credits: 3,
    semester: 'HK2 2025-2026',
    teacher: 'ThS. Nguyễn Minh Khoa',
    processScore: 8.0,
    midtermScore: 8.5,
    finalScore: null,
    gpa: 8.2,
    letterGrade: 'B+',
    status: 'in_progress',
    statusLabel: 'Đang học',
    note: 'Chưa thi cuối kỳ. Điểm GPA là ước tính tạm thời.',
    publishedAt: null,
  },
  {
    id: 's3',
    name: 'Toán rời rạc',
    code: 'TOAN201',
    credits: 3,
    semester: 'HK2 2025-2026',
    teacher: 'TS. Lê Văn Minh',
    processScore: 6.0,
    midtermScore: 5.5,
    finalScore: null,
    gpa: null,
    letterGrade: null,
    status: 'pending',
    statusLabel: 'Chờ điểm',
    note: 'Đang chờ giảng viên công bố điểm cuối kỳ.',
    publishedAt: null,
  },
  {
    id: 's4',
    name: 'Hệ quản trị CSDL',
    code: 'HQTCSDL401',
    credits: 3,
    semester: 'HK1 2025-2026',
    teacher: 'PGS. Hoàng Đức Bình',
    processScore: 8.5,
    midtermScore: 8.0,
    finalScore: 8.8,
    gpa: 8.6,
    letterGrade: 'A',
    status: 'pass',
    statusLabel: 'Đạt',
    note: 'Bài tập nhóm được đánh giá cao.',
    publishedAt: '15/01/2026',
  },
  {
    id: 's5',
    name: 'Mạng máy tính',
    code: 'MMT401',
    credits: 3,
    semester: 'HK1 2025-2026',
    teacher: 'PGS. Hoàng Đức Bình',
    processScore: 4.5,
    midtermScore: 4.0,
    finalScore: 3.8,
    gpa: 4.0,
    letterGrade: 'D',
    status: 'fail',
    statusLabel: 'Không đạt',
    note: 'Chưa đạt điểm qua môn. Sinh viên cần đăng ký học lại.',
    publishedAt: '15/01/2026',
  },
  {
    id: 's6',
    name: 'Phân tích & Thiết kế hệ thống',
    code: 'PTTKHT301',
    credits: 3,
    semester: 'HK1 2025-2026',
    teacher: 'ThS. Phạm Quốc Tuấn',
    processScore: 7.0,
    midtermScore: null,
    finalScore: null,
    gpa: null,
    letterGrade: null,
    status: 'incomplete',
    statusLabel: 'Chưa hoàn thành',
    note: 'Sinh viên cần hoàn thành bài thi bù trước ngày 30/06/2026.',
    publishedAt: null,
    incompleteReason: 'Vắng thi cuối kỳ có phép. Cần thi bù.',
  },
  {
    id: 's7',
    name: 'Giải tích',
    code: 'GIAI101',
    credits: 4,
    semester: 'HK1 2024-2025',
    teacher: 'GS. Nguyễn Văn Hùng',
    processScore: 7.5,
    midtermScore: 7.0,
    finalScore: 7.8,
    gpa: 7.6,
    letterGrade: 'B',
    status: 'pass',
    statusLabel: 'Đạt',
    note: '',
    publishedAt: '20/01/2026',
  },
  {
    id: 's8',
    name: 'Lập trình hướng đối tượng',
    code: 'OOP201',
    credits: 3,
    semester: 'HK2 2024-2025',
    teacher: 'TS. Đỗ Thị Mai',
    processScore: 8.5,
    midtermScore: 9.0,
    finalScore: 8.8,
    gpa: 8.8,
    letterGrade: 'A',
    status: 'pass',
    statusLabel: 'Đạt',
    note: 'Kết quả xuất sắc trong phần thực hành.',
    publishedAt: '05/06/2026',
  },
]

export const mockFailRiskAlerts = [
  {
    id: 'a1',
    level: 'danger',
    levelLabel: 'Nguy hiểm',
    icon: 'XCircle',
    title: 'Môn Mạng máy tính không đạt',
    description: 'Điểm tổng kết 4.0/10 – dưới ngưỡng qua môn. Cần đăng ký học lại trong kỳ tới.',
    action: 'Đăng ký học lại',
  },
  {
    id: 'a2',
    level: 'warning',
    levelLabel: 'Cảnh báo',
    icon: 'AlertTriangle',
    title: 'Môn PTTKHT301 chưa hoàn thành',
    description: 'Sinh viên vắng thi cuối kỳ có phép. Cần thi bù trước 30/06/2026.',
    action: 'Xem hướng dẫn',
  },
  {
    id: 'a3',
    level: 'info',
    levelLabel: 'Lưu ý',
    icon: 'TrendingDown',
    title: 'GPA Toán rời rạc có thể ảnh hưởng tích lũy',
    description: 'Điểm quá trình và giữa kỳ đang ở mức 5.5–6.0. Cần đạt ≥ 6.5 cuối kỳ để giữ GPA.',
    action: null,
  },
]

export const mockGradeRules = [
  { icon: 'ShieldCheck', text: 'Chỉ hiển thị điểm đã được công bố chính thức.' },
  { icon: 'Calculator', text: 'GPA được tính tự động theo thang điểm 4.0.' },
  { icon: 'Lock', text: 'Điểm đã khóa không thể chỉnh sửa trực tiếp.' },
  { icon: 'User', text: 'Sinh viên chỉ xem được điểm của chính mình.' },
  { icon: 'FileText', text: 'Yêu cầu sửa điểm cần gửi đơn qua phòng đào tạo.' },
]

// ==========================================
// 7. DASHBOARD TỔNG QUAN (studentDashboard)
// ==========================================
const originalStudentDashboard = {
  student: {
    name: 'Nguyễn Văn An',
    code: 'SV2026001',
    className: 'SE1601',
    semester: 'HK2 2025-2026',
  },
  weekProgress: 68,
  focusSummary: {
    classesToday: 3,
    assignmentsDue: 4,
    completedThisWeek: 11,
    nearestDeadline: '23:59 hôm nay',
    gpa: '8.2',
  },
  kpis: [
    {
      id: 'courses',
      label: 'Khóa học đang học',
      value: '6',
      trend: '+2 khóa mới kỳ này',
      tone: 'blue',
      route: '/student/courses',
    },
    {
      id: 'assignments',
      label: 'Bài tập cần xử lý',
      value: '4',
      trend: '2 bài sắp đến hạn',
      tone: 'amber',
      route: '/student/assignments',
    },
    {
      id: 'gpa',
      label: 'GPA học kỳ',
      value: '8.2',
      trend: '+0.4 so với kỳ trước',
      tone: 'violet',
      route: '/student/grades',
    },
    {
      id: 'attendance',
      label: 'Chuyên cần',
      value: '92%',
      trend: '2 buổi vắng',
      tone: 'teal',
      route: '/student/attendance',
    },
  ],
  courses: [
    {
      id: 'ctdl',
      name: 'Cấu trúc dữ liệu & Giải thuật',
      code: 'CTDL101',
      lecturer: 'TS. Nguyễn Minh Khoa',
      progress: 72,
      completed: 9,
      total: 12,
      status: 'Cần tiếp tục',
      statusVariant: 'warning',
    },
    {
      id: 'web',
      name: 'Lập trình Web nâng cao',
      code: 'LTW301',
      lecturer: 'ThS. Lê Phương Mai',
      progress: 86,
      completed: 12,
      total: 14,
      status: 'Sắp hoàn thành',
      statusVariant: 'success',
    },
    {
      id: 'db',
      name: 'Hệ quản trị CSDL',
      code: 'HQTCSDL401',
      lecturer: 'ThS. Trần Quốc Việt',
      progress: 54,
      completed: 7,
      total: 13,
      status: 'Đang học',
      statusVariant: 'info',
    },
    {
      id: 'math',
      name: 'Toán rời rạc',
      code: 'TRR201',
      lecturer: 'TS. Phạm Thu Hà',
      progress: 61,
      completed: 8,
      total: 13,
      status: 'Đang học',
      statusVariant: 'info',
    },
  ],
  assignments: [
    {
      id: 'a1',
      title: 'Phân tích độ phức tạp thuật toán',
      course: 'Cấu trúc dữ liệu',
      deadline: 'Hôm nay · 23:59',
      status: 'Sắp đến hạn',
      variant: 'warning',
      priority: 'high',
    },
    {
      id: 'a2',
      title: 'Thiết kế dashboard Vue 3',
      course: 'Lập trình Web',
      deadline: 'Mai · 20:00',
      status: 'Chưa nộp',
      variant: 'info',
      priority: 'medium',
    },
    {
      id: 'a3',
      title: 'Báo cáo normal form',
      course: 'Hệ quản trị CSDL',
      deadline: 'Đã nộp · chờ chấm',
      status: 'Đã nộp',
      variant: 'success',
      priority: 'low',
    },
    {
      id: 'a4',
      title: 'Bài luyện tập logic mệnh đề',
      course: 'Toán rời rạc',
      deadline: 'Quá hạn 1 ngày',
      status: 'Quá hạn',
      variant: 'danger',
      priority: 'high',
    },
  ],
  schedule: [
    {
      id: 's1',
      time: '07:30 - 09:30',
      subject: 'Cấu trúc dữ liệu & Giải thuật',
      room: 'P.302 · CS1',
      lecturer: 'TS. Nguyễn Minh Khoa',
      status: 'Đã xong',
      variant: 'success',
    },
    {
      id: 's2',
      time: '10:00 - 11:30',
      subject: 'Hệ quản trị CSDL',
      room: 'Lab 4 · Online backup',
      lecturer: 'ThS. Trần Quốc Việt',
      status: 'Đang diễn ra',
      variant: 'info',
      current: true,
    },
    {
      id: 's3',
      time: '13:30 - 15:30',
      subject: 'Toán rời rạc',
      room: 'P.105 · CS1',
      lecturer: 'TS. Phạm Thu Hà',
      status: 'Sắp tới',
      variant: 'warning',
    },
  ],
  grades: [
    {
      id: 'g1',
      course: 'Lập trình Web',
      type: 'Giữa kỳ',
      score: '9.0',
      note: 'Tăng tốt ở phần component hóa',
    },
    {
      id: 'g2',
      course: 'Cấu trúc dữ liệu',
      type: 'Bài tập tuần 9',
      score: '8.4',
      note: 'Cần bổ sung phân tích edge case',
    },
    {
      id: 'g3',
      course: 'Hệ quản trị CSDL',
      type: 'Quiz',
      score: '7.8',
      note: 'Ổn định, ôn thêm chuẩn hóa',
    },
    {
      id: 'g4',
      course: 'Toán rời rạc',
      type: 'Kiểm tra 15 phút',
      score: '8.1',
      note: 'Hoàn thành đúng hạn',
    },
  ],
  attendance: {
    rate: 92,
    absent: 2,
    late: 1,
    warning: 'Môn CSDL còn 1 buổi vắng trước ngưỡng cảnh báo.',
  },
  tuition: {
    total: '18.500.000đ',
    paid: '12.000.000đ',
    remaining: '6.500.000đ',
    dueDate: '25/05/2026',
    status: 'Còn công nợ',
  },
  registration: {
    title: 'Đợt đăng ký HK hè',
    status: 'Đang mở',
    registered: 4,
    waitlist: 1,
    closesIn: 'Còn 3 ngày',
  },
  notifications: [
    {
      id: 'n1',
      type: 'Bài tập',
      title: 'Bài tập CTDL mới được giao',
      time: '12 phút trước',
      unread: true,
      variant: 'warning',
    },
    {
      id: 'n2',
      type: 'Lịch đổi',
      title: 'CSDL chuyển sang Lab 4 sáng nay',
      time: '35 phút trước',
      unread: true,
      variant: 'info',
    },
    {
      id: 'n3',
      type: 'Điểm',
      title: 'Điểm giữa kỳ Lập trình Web đã công bố',
      time: '2 giờ trước',
      unread: true,
      variant: 'success',
    },
    {
      id: 'n4',
      type: 'Học phí',
      title: 'Hóa đơn HK2 cần thanh toán phần còn lại',
      time: 'Hôm qua',
      unread: false,
      variant: 'danger',
    },
    {
      id: 'n5',
      type: 'Học vụ',
      title: 'Mở khảo sát đánh giá giảng viên',
      time: '2 ngày trước',
      unread: false,
      variant: 'violet',
    },
  ],
}

// ==========================================
// 8. DỮ LIỆU BỔ SUNG CHO CÁC SINH VIÊN KHÁC
// ==========================================

const curriculumGD = {
  studentName: 'Nguyễn Thiết Kế',
  majorName: 'Thiết kế đồ họa',
  facultyName: 'Mỹ thuật ứng dụng',
  programCode: 'GD-FPT-2026',
  programVersion: 'Version 2026',
  currentSemesterIndex: 3,
  currentBlockIndex: 1,
  totalCredits: 48,
  completedCredits: 24,
  totalSubjects: 16,
  completedSubjects: 8,
  semesters: [
    {
      semesterIndex: 1,
      semesterName: 'Kỳ 1',
      blocks: [
        {
          blockIndex: 1,
          blockName: 'Block 1',
          subjects: [
            createSubject('gd101', 'GD101', 'Cơ sở tạo hình', 3, 'completed', 100, 8.5, false),
            createSubject('gd102', 'GD102', 'Luật xa gần', 3, 'completed', 100, 8.0, false),
            createSubject('gd103', 'GD103', 'Lịch sử mỹ thuật', 3, 'completed', 100, 7.5, false),
            createSubject('eng101', 'ENG101', 'Tiếng Anh cơ sở 1', 3, 'completed', 100, 8.0, false),
          ]
        }
      ]
    },
    {
      semesterIndex: 2,
      semesterName: 'Kỳ 2',
      blocks: [
        {
          blockIndex: 1,
          blockName: 'Block 1',
          subjects: [
            createSubject('gd201', 'GD201', 'Thiết kế Illustrator', 3, 'completed', 100, 8.2, false),
            createSubject('gd202', 'GD202', 'Xử lý ảnh Photoshop', 3, 'completed', 100, 7.8, false),
            createSubject('gd203', 'GD203', 'Ý tưởng sáng tạo', 3, 'completed', 100, 8.5, false),
            createSubject('eng102', 'ENG102', 'Tiếng Anh cơ sở 2', 3, 'completed', 100, 8.0, false),
          ]
        }
      ]
    },
    {
      semesterIndex: 3,
      semesterName: 'Kỳ 3',
      blocks: [
        {
          blockIndex: 1,
          blockName: 'Block 1',
          subjects: [
            createSubject('gd301', 'GD301', 'Thiết kế dàn trang', 3, 'current', 45, null, false, { quizScore: 8.0 }),
            createSubject('gd302', 'GD302', 'Màu sắc trong thiết kế', 3, 'current', 60, null, false, { quizScore: 8.5 }),
            createSubject('gd303', 'GD303', 'Thiết kế Web UI/UX', 3, 'current', 50, null, false, { quizScore: 7.5 }),
          ]
        },
        {
          blockIndex: 2,
          blockName: 'Block 2',
          subjects: [
            createSubject('gd304', 'GD304', 'Đồ họa vector nâng cao', 3, 'early_available', 0, null, true),
          ]
        }
      ]
    },
    {
      semesterIndex: 4,
      semesterName: 'Kỳ 4',
      blocks: [
        {
          blockIndex: 1,
          blockName: 'Block 1',
          subjects: [
            createSubject('gd401', 'GD401', 'Thiết kế bao bì', 3, 'future_locked', 0, null, false),
            createSubject('gd402', 'GD402', 'Đồ họa chuyển động', 3, 'future_locked', 0, null, false),
          ]
        },
        {
          blockIndex: 2,
          blockName: 'Block 2',
          subjects: [
            createSubject('gd403', 'GD403', 'Thực tập doanh nghiệp', 3, 'future_locked', 0, null, false),
            createSubject('gd404', 'GD404', 'Đồ án tốt nghiệp', 6, 'future_locked', 0, null, false),
          ]
        }
      ]
    }
  ]
}

const curriculumMKT = {
  studentName: 'Trần Thị Marketing',
  majorName: 'Marketing',
  facultyName: 'Quản trị kinh doanh',
  programCode: 'MR-FPT-2026',
  programVersion: 'Version 2026',
  currentSemesterIndex: 5,
  currentBlockIndex: 1,
  totalCredits: 48,
  completedCredits: 36,
  totalSubjects: 16,
  completedSubjects: 12,
  semesters: [
    {
      semesterIndex: 1,
      semesterName: 'Kỳ 1',
      blocks: [
        {
          blockIndex: 1,
          blockName: 'Block 1',
          subjects: [
            createSubject('mr101', 'MR101', 'Nhập môn Marketing', 3, 'completed', 100, 8.0, false),
            createSubject('tin101', 'TIN101', 'Tin học văn phòng', 3, 'completed', 100, 9.0, false),
            createSubject('eng101', 'ENG101', 'Tiếng Anh cơ sở 1', 3, 'completed', 100, 8.0, false),
          ]
        }
      ]
    },
    {
      semesterIndex: 2,
      semesterName: 'Kỳ 2',
      blocks: [
        {
          blockIndex: 1,
          blockName: 'Block 1',
          subjects: [
            createSubject('mr201', 'MR201', 'Hành vi người tiêu dùng', 3, 'completed', 100, 8.2, false),
            createSubject('mr202', 'MR202', 'Nghiên cứu Marketing', 3, 'completed', 100, 7.8, false),
          ]
        }
      ]
    },
    {
      semesterIndex: 5,
      semesterName: 'Kỳ 5',
      blocks: [
        {
          blockIndex: 1,
          blockName: 'Block 1',
          subjects: [
            createSubject('mr501', 'MR501', 'Digital Marketing', 3, 'current', 30, null, false, { quizScore: 7.8 }),
            createSubject('mr502', 'MR502', 'Quản trị thương hiệu', 3, 'current', 50, null, false, { quizScore: 8.2 }),
            createSubject('mr503', 'MR503', 'Marketing quan hệ khách hàng', 3, 'current', 40, null, false, { quizScore: 7.5 }),
          ]
        }
      ]
    }
  ]
}

const dashboardGD = {
  student: {
    name: 'Nguyễn Thiết Kế',
    code: 'SV2026002',
    className: 'GD1701',
    semester: 'HK3 2025-2026',
  },
  weekProgress: 75,
  focusSummary: {
    classesToday: 2,
    assignmentsDue: 3,
    completedThisWeek: 8,
    nearestDeadline: '23:59 ngày mai',
    gpa: '8.5',
  },
  kpis: [
    {
      id: 'courses',
      label: 'Khóa học đang học',
      value: '5',
      trend: '+1 khóa mới kỳ này',
      tone: 'blue',
      route: '/student/courses',
    },
    {
      id: 'assignments',
      label: 'Bài tập cần xử lý',
      value: '3',
      trend: '1 bài sắp đến hạn',
      tone: 'amber',
      route: '/student/assignments',
    },
    {
      id: 'gpa',
      label: 'GPA học kỳ',
      value: '8.5',
      trend: '+0.3 so với kỳ trước',
      tone: 'violet',
      route: '/student/grades',
    },
    {
      id: 'attendance',
      label: 'Tỉ lệ chuyên cần',
      value: '98%',
      trend: 'Không vắng buổi nào',
      tone: 'teal',
      route: '/student/attendance',
    },
  ],
  courses: [
    {
      id: 'gd301',
      name: 'Thiết kế dàn trang',
      code: 'GD301',
      lecturer: 'ThS. Trần Typography',
      progress: 45,
      completed: 5,
      total: 11,
      status: 'Cần tiếp tục',
      statusVariant: 'warning',
    },
    {
      id: 'gd302',
      name: 'Màu sắc trong thiết kế',
      code: 'GD302',
      lecturer: 'ThS. Phạm Phối Màu',
      progress: 60,
      completed: 6,
      total: 10,
      status: 'Sắp hoàn thành',
      statusVariant: 'success',
    }
  ],
  assignments: [
    {
      id: 'a_gd1',
      title: 'Thiết kế tạp chí 8 trang',
      course: 'Thiết kế dàn trang',
      deadline: 'Hôm nay · 23:59',
      status: 'Sắp đến hạn',
      variant: 'warning',
      priority: 'high',
    },
    {
      id: 'a_gd2',
      title: 'Phối màu cho poster sự kiện',
      course: 'Màu sắc thiết kế',
      deadline: 'Mai · 20:00',
      status: 'Chưa nộp',
      variant: 'info',
      priority: 'medium',
    }
  ],
  schedule: [
    {
      id: 's_gd1',
      time: '07:30 - 09:30',
      subject: 'Thiết kế dàn trang',
      room: 'P.402 · CS1',
      lecturer: 'ThS. Trần Typography',
      status: 'Đã xong',
      variant: 'success',
    },
    {
      id: 's_gd2',
      time: '10:00 - 11:30',
      subject: 'Màu sắc trong thiết kế',
      room: 'Lab 5 · CS1',
      lecturer: 'ThS. Phạm Phối Màu',
      status: 'Đang diễn ra',
      variant: 'info',
      current: true,
    }
  ],
  grades: [
    {
      id: 'g_gd1',
      course: 'Thiết kế dàn trang',
      type: 'Giữa kỳ',
      score: '8.5',
      note: 'Bố cục tốt, chữ hơi nhỏ',
    },
    {
      id: 'g_gd2',
      course: 'Màu sắc trong thiết kế',
      type: 'Bài tập 1',
      score: '8.0',
      note: 'Phối màu hài hòa',
    }
  ],
  attendance: {
    rate: 98,
    absent: 0,
    late: 0,
    warning: 'Không có cảnh báo học tập kỳ này.',
  },
  tuition: {
    total: '19.500.000đ',
    paid: '19.500.000đ',
    remaining: '0đ',
    dueDate: '25/05/2026',
    status: 'Đã hoàn thành',
  },
  registration: {
    title: 'Đăng ký học hè',
    status: 'Đang mở',
    registered: 3,
    waitlist: 0,
    closesIn: 'Còn 2 ngày',
  },
  notifications: [
    {
      id: 'n_gd1',
      type: 'Học vụ',
      title: 'Lịch thi cuối kỳ thiết kế dàn trang đã công bố',
      time: '1 giờ trước',
      unread: true,
      variant: 'success',
    }
  ]
}

const dashboardMKT = {
  student: {
    name: 'Trần Thị Marketing',
    code: 'SV2026003',
    className: 'MR1801',
    semester: 'HK5 2025-2026',
  },
  weekProgress: 60,
  focusSummary: {
    classesToday: 4,
    assignmentsDue: 2,
    completedThisWeek: 12,
    nearestDeadline: '18:00 hôm nay',
    gpa: '8.0',
  },
  kpis: [
    {
      id: 'courses',
      label: 'Khóa học đang học',
      value: '4',
      trend: 'Kỳ chuyên ngành',
      tone: 'blue',
      route: '/student/courses',
    },
    {
      id: 'assignments',
      label: 'Bài tập cần xử lý',
      value: '2',
      trend: 'Đã nộp hầu hết',
      tone: 'amber',
      route: '/student/assignments',
    },
    {
      id: 'gpa',
      label: 'GPA học kỳ',
      value: '8.0',
      trend: 'Giữ vững phong độ',
      tone: 'violet',
      route: '/student/grades',
    },
    {
      id: 'attendance',
      label: 'Tỉ lệ chuyên cần',
      value: '95%',
      trend: 'Vắng 1 buổi có phép',
      tone: 'teal',
      route: '/student/attendance',
    },
  ],
  courses: [
    {
      id: 'mr501',
      name: 'Digital Marketing',
      code: 'MR501',
      lecturer: 'ThS. Lê SEO',
      progress: 30,
      completed: 3,
      total: 10,
      status: 'Cần tiếp tục',
      statusVariant: 'warning',
    },
    {
      id: 'mr502',
      name: 'Quản trị thương hiệu',
      code: 'MR502',
      lecturer: 'TS. Trần Kim Hiệu',
      progress: 50,
      completed: 5,
      total: 10,
      status: 'Sắp hoàn thành',
      statusVariant: 'success',
    }
  ],
  assignments: [
    {
      id: 'a_mr1',
      title: 'Tối ưu hóa On-page SEO website',
      course: 'Digital Marketing',
      deadline: 'Hôm nay · 23:59',
      status: 'Sắp đến hạn',
      variant: 'warning',
      priority: 'high',
    },
    {
      id: 'a_mr2',
      title: 'Phân tích SWOT thương hiệu Nike',
      course: 'Quản trị thương hiệu',
      deadline: 'Mai · 20:00',
      status: 'Chưa nộp',
      variant: 'info',
      priority: 'medium',
    }
  ],
  schedule: [
    {
      id: 's_mr1',
      time: '07:30 - 09:30',
      subject: 'Digital Marketing',
      room: 'P.305 · CS1',
      lecturer: 'ThS. Lê SEO',
      status: 'Đã xong',
      variant: 'success',
    },
    {
      id: 's_mr2',
      time: '10:00 - 11:30',
      subject: 'Quản trị thương hiệu',
      room: 'P.202 · CS1',
      lecturer: 'TS. Trần Kim Hiệu',
      status: 'Đang diễn ra',
      variant: 'info',
      current: true,
    }
  ],
  grades: [
    {
      id: 'g_mr1',
      course: 'Digital Marketing',
      type: 'Giữa kỳ',
      score: '7.8',
      note: 'Từ khóa SEO chuẩn xác',
    },
    {
      id: 'g_mr2',
      course: 'Quản trị thương hiệu',
      type: 'Bài tập 1',
      score: '8.2',
      note: 'Phân tích SWOT chi tiết',
    }
  ],
  attendance: {
    rate: 95,
    absent: 1,
    late: 0,
    warning: 'Môn Digital Marketing vắng 1 buổi.',
  },
  tuition: {
    total: '18.000.000đ',
    paid: '18.000.000đ',
    remaining: '0đ',
    dueDate: '25/05/2026',
    status: 'Đã hoàn thành',
  },
  registration: {
    title: 'Đăng ký học hè',
    status: 'Đang mở',
    registered: 4,
    waitlist: 1,
    closesIn: 'Còn 2 ngày',
  },
  notifications: [
    {
      id: 'n_mr1',
      type: 'Bài tập',
      title: 'Giao bài tập SEO nâng cao mới',
      time: '30 phút trước',
      unread: true,
      variant: 'warning',
    }
  ]
}

const subjectsGD = [
  {
    id: 's_gd1',
    name: 'Cơ sở tạo hình',
    code: 'GD101',
    credits: 3,
    semester: 'HK1 2025-2026',
    teacher: 'ThS. Nguyễn Mỹ Thuật',
    processScore: 8.5,
    midtermScore: 9.0,
    finalScore: 8.5,
    gpa: 8.6,
    letterGrade: 'A',
    status: 'pass',
    statusLabel: 'Đạt',
    note: 'Bài vẽ tay rất sáng tạo.'
  },
  {
    id: 's_gd2',
    name: 'Luật xa gần',
    code: 'GD102',
    credits: 3,
    semester: 'HK1 2025-2026',
    teacher: 'ThS. Lê Bố Cục',
    processScore: 8.0,
    midtermScore: 8.0,
    finalScore: 8.0,
    gpa: 8.0,
    letterGrade: 'B+',
    status: 'pass',
    statusLabel: 'Đạt',
    note: 'Nắm vững luật phối cảnh.'
  },
  {
    id: 's_gd3',
    name: 'Lịch sử mỹ thuật',
    code: 'GD103',
    credits: 3,
    semester: 'HK1 2025-2026',
    teacher: 'TS. Phan Cổ Điển',
    processScore: 7.5,
    midtermScore: 7.0,
    finalScore: 7.8,
    gpa: 7.5,
    letterGrade: 'B',
    status: 'pass',
    statusLabel: 'Đạt',
    note: 'Hiểu sâu sắc các trường phái nghệ thuật cổ điển.'
  },
  {
    id: 's_gd4',
    name: 'Thiết kế Illustrator',
    code: 'GD201',
    credits: 3,
    semester: 'HK2 2025-2026',
    teacher: 'ThS. Lê Vector',
    processScore: 8.2,
    midtermScore: 8.0,
    finalScore: 8.4,
    gpa: 8.2,
    letterGrade: 'B+',
    status: 'pass',
    statusLabel: 'Đạt',
    note: 'Kỹ năng thiết kế logo vector rất tốt.'
  },
  {
    id: 's_gd5',
    name: 'Xử lý ảnh Photoshop',
    code: 'GD202',
    credits: 3,
    semester: 'HK2 2025-2026',
    teacher: 'ThS. Phạm Blend',
    processScore: 7.8,
    midtermScore: 8.2,
    finalScore: 7.5,
    gpa: 7.8,
    letterGrade: 'B',
    status: 'pass',
    statusLabel: 'Đạt',
    note: 'Khả năng retouch ảnh và ghép ảnh sáng tạo.'
  },
  {
    id: 's_gd6',
    name: 'Ý tưởng sáng tạo',
    code: 'GD203',
    credits: 3,
    semester: 'HK2 2025-2026',
    teacher: 'TS. Nguyễn Concept',
    processScore: 8.5,
    midtermScore: 8.5,
    finalScore: 8.5,
    gpa: 8.5,
    letterGrade: 'A',
    status: 'pass',
    statusLabel: 'Đạt',
    note: 'Phương pháp brainstorm ý tưởng độc đáo.'
  },
  {
    id: 's_gd7',
    name: 'Thiết kế dàn trang',
    code: 'GD301',
    credits: 3,
    semester: 'HK3 2025-2026',
    teacher: 'ThS. Trần Typography',
    processScore: 8.2,
    midtermScore: 8.5,
    finalScore: null,
    gpa: 8.3,
    letterGrade: 'B+',
    status: 'in_progress',
    statusLabel: 'Đang học',
    note: 'Chưa thi cuối kỳ.'
  },
  {
    id: 's_gd8',
    name: 'Màu sắc trong thiết kế',
    code: 'GD302',
    credits: 3,
    semester: 'HK3 2025-2026',
    teacher: 'ThS. Phạm Phối Màu',
    processScore: 8.8,
    midtermScore: 8.5,
    finalScore: null,
    gpa: 8.6,
    letterGrade: 'A',
    status: 'in_progress',
    statusLabel: 'Đang học',
    note: 'Khả năng cảm thụ màu sắc rất tốt.'
  },
  {
    id: 's_gd9',
    name: 'Thiết kế Web UI/UX',
    code: 'GD303',
    credits: 3,
    semester: 'HK3 2025-2026',
    teacher: 'ThS. Nguyễn Wireframe',
    processScore: 7.5,
    midtermScore: 7.8,
    finalScore: null,
    gpa: 7.6,
    letterGrade: 'B',
    status: 'in_progress',
    statusLabel: 'Đang học',
    note: 'Kỹ năng thiết kế wireframe trên Figma ổn.'
  }
]

const subjectsMKT = [
  {
    id: 's_mr1',
    name: 'Nhập môn Marketing',
    code: 'MR101',
    credits: 3,
    semester: 'HK1 2025-2026',
    teacher: 'TS. Nguyễn Thị Quảng Cáo',
    processScore: 8.0,
    midtermScore: 8.0,
    finalScore: 8.0,
    gpa: 8.0,
    letterGrade: 'B+',
    status: 'pass',
    statusLabel: 'Đạt',
    note: 'Hiểu rõ các khái niệm Marketing căn bản.'
  },
  {
    id: 's_mr2',
    name: 'Digital Marketing',
    code: 'MR501',
    credits: 3,
    semester: 'HK5 2025-2026',
    teacher: 'ThS. Lê SEO',
    processScore: 7.8,
    midtermScore: 8.0,
    finalScore: null,
    gpa: 7.9,
    letterGrade: 'B',
    status: 'in_progress',
    statusLabel: 'Đang học',
    note: 'Đang thực hiện bài tập nhóm SEO.'
  },
  {
    id: 's_mr3',
    name: 'Quản trị thương hiệu',
    code: 'MR502',
    credits: 3,
    semester: 'HK5 2025-2026',
    teacher: 'TS. Trần Kim Hiệu',
    processScore: 8.2,
    midtermScore: 8.2,
    finalScore: null,
    gpa: 8.2,
    letterGrade: 'B+',
    status: 'in_progress',
    statusLabel: 'Đang học',
    note: 'Bài thuyết trình thương hiệu tốt.'
  }
]

const gradeSummaryGD = {
  cumulativeGPA: 3.45,
  maxGPA: 4.0,
  currentSemesterGPA: 3.45,
  previousSemesterGPA: 3.30,
  gpaChange: +0.15,
  classification: 'Giỏi',
  totalCreditsEarned: 24,
  totalCreditsRequired: 120,
  totalSubjectsPassed: 8,
  totalSubjectsFailed: 0,
  incompleteSubjects: 0,
  pendingSubjects: 2,
  riskAlertCount: 0,
}

const gradeSummaryMKT = {
  cumulativeGPA: 3.25,
  maxGPA: 4.0,
  currentSemesterGPA: 3.25,
  previousSemesterGPA: 3.20,
  gpaChange: +0.05,
  classification: 'Khá',
  totalCreditsEarned: 36,
  totalCreditsRequired: 120,
  totalSubjectsPassed: 12,
  totalSubjectsFailed: 0,
  incompleteSubjects: 0,
  pendingSubjects: 2,
  riskAlertCount: 0,
}

const semesterGPAGD = [
  { semester: 'HK1 2024-2025', semesterGPA: 3.20, cumulativeGPA: 3.20, credits: 12, year: 1 },
  { semester: 'HK2 2024-2025', semesterGPA: 3.30, cumulativeGPA: 3.25, credits: 12, year: 1 },
  { semester: 'HK3 2025-2026', semesterGPA: 3.45, cumulativeGPA: 3.45, credits: 15, year: 2 },
]

const semesterGPAMKT = [
  { semester: 'HK1 2023-2024', semesterGPA: 3.10, cumulativeGPA: 3.10, credits: 12, year: 1 },
  { semester: 'HK2 2023-2024', semesterGPA: 3.20, cumulativeGPA: 3.15, credits: 12, year: 1 },
  { semester: 'HK1 2024-2025', semesterGPA: 3.20, cumulativeGPA: 3.17, credits: 12, year: 2 },
  { semester: 'HK2 2024-2025', semesterGPA: 3.25, cumulativeGPA: 3.20, credits: 12, year: 2 },
  { semester: 'HK1 2025-2026', semesterGPA: 3.25, cumulativeGPA: 3.25, credits: 15, year: 3 },
]

const examsGD = [
  {
    id: 'exam-gd-01',
    title: 'Thi cuối kỳ Thiết kế dàn trang',
    subject: 'Thiết kế dàn trang',
    subjectCode: 'GD301',
    date: 'Hôm nay',
    time: '13:30 - 15:30',
    duration: 120,
    room: 'Lab 5',
    status: 'Đang mở',
    statusVariant: 'success',
    allowTake: true,
  }
]

const examsMKT = [
  {
    id: 'exam-mr-01',
    title: 'Thi cuối kỳ Digital Marketing',
    subject: 'Digital Marketing',
    subjectCode: 'MR501',
    date: 'Ngày mai',
    time: '08:00 - 10:00',
    duration: 120,
    room: 'P.305',
    status: 'Sắp diễn ra',
    statusVariant: 'info',
    allowTake: false,
  }
]

// Dữ liệu chi tiết khóa học Thiết kế đồ họa
const gdCourse = {
  id: 'GD301',
  title: 'Thiết kế dàn trang',
  code: 'GD301',
  teacher: 'ThS. Trần Typography',
  semester: 'HK3 2025-2026',
  credits: 3,
  coverGradient: 'from-purple-700 via-indigo-600 to-blue-500',
  description: 'Môn học cung cấp kiến thức nền tảng và chuyên sâu về thiết kế dàn trang tạp chí, sách, báo chí sử dụng Adobe InDesign. Sinh viên sẽ nắm vững nguyên lý Grid, Typography, phân cấp thông tin và tối ưu hóa tệp tin in ấn.'
}

const gdStats = [
  { label: 'Tiến độ', value: '45', unit: '%', icon: 'Gauge', tone: 'purple', progress: 45, hint: '5/11 bài đã hoàn thành' },
  { label: 'Bài học', value: '5', unit: '/11', icon: 'BookOpenCheck', tone: 'indigo', progress: 45, hint: 'Đã hoàn thành 5 bài' },
  { label: 'Bài tập', value: '1', unit: 'mục', icon: 'ClipboardList', tone: 'amber', progress: 50, hint: '1 bài tập đang thực hiện' },
  { label: 'Tài liệu', value: '12', unit: 'file', icon: 'Files', tone: 'violet', progress: 50, hint: 'Tệp mẫu InDesign, PDF' },
]

export const gdLessons = [
  {
    id: 'gd_ch1',
    chapter: 'Chương 1',
    title: 'Tổng quan về Adobe InDesign & Workspace',
    description: 'Làm quen giao diện, thanh công cụ, cách tạo tài liệu chuẩn in ấn và thiết lập Grid/Guides.',
    status: 'completed',
    badge: 'Hoàn thành',
    tone: 'green',
    icon: 'CheckCircle2',
    meta: ['2 bài học', '1 thực hành'],
    progress: 100,
    lessons: [
      { id: 'gdl1-1', title: 'Giới thiệu về InDesign & Kích thước chuẩn', duration: '15:20', status: 'completed' },
      { id: 'gdl1-2', title: 'Thiết lập Master Pages và Grid System', duration: '20:15', status: 'completed' },
    ]
  },
  {
    id: 'gd_ch2',
    chapter: 'Chương 2',
    title: 'Làm việc với Typography & Text Flow',
    description: 'Cách nhập văn bản, quản lý Text Frame, thiết lập Character/Paragraph Styles và xử lý Overset Text.',
    status: 'active',
    badge: 'Đang học',
    tone: 'purple',
    icon: 'ListTree',
    meta: ['3 bài học', '1 bài tập'],
    progress: 50,
    lessons: [
      { id: 'gdl2-1', title: 'Paragraph Styles & Character Styles', duration: '25:10', status: 'completed' },
      { id: 'gdl2-2', title: 'Nguyên lý Grid và Layout trong InDesign', duration: '18:40', status: 'active' },
      { id: 'gdl2-3', title: 'Typography nâng cao và Căn chỉnh lưới', duration: '22:00', status: 'locked' },
      { id: 'gdl2-4', title: 'Bài tập thiết kế Layout tạp chí', duration: '–', status: 'locked', type: 'assignment' },
    ]
  },
  {
    id: 'gd_ch3',
    chapter: 'Chương 3',
    title: 'Quản lý Hình ảnh & Màu sắc',
    description: 'Cách import hình ảnh (Links Panel), quản lý độ phân giải chuẩn in ấn (DPI), hệ màu CMYK và hiệu ứng hình ảnh.',
    status: 'locked',
    badge: 'Chưa mở',
    tone: 'slate',
    icon: 'Lock',
    meta: ['3 bài học'],
    progress: 0,
    lessons: []
  }
]

const gdCurrentLesson = {
  id: 'gdl2-2',
  chapterId: 'gd_ch2',
  chapterTitle: 'Chương 2: Làm việc với Typography & Text Flow',
  title: 'Nguyên lý Grid và Layout trong InDesign',
  lessonType: 'video',
  videoUrl: '',
  duration: '18:40',
  watchedSeconds: 610,
  totalSeconds: 1120,
  durationSeconds: 1120,
  allowSeek: true,
  pauseOnBlur: false,
  minWatchPercentToComplete: 80,
  progressPercent: 54,
  maxWatchedSeconds: 610,
  completedAt: null,
  videoThumb: null,
  documentTitle: 'Slide Nguyên lý Grid và Layout.pdf',
  documentPages: 25,
  documentCurrentPage: 10,
}

const gdQuizQuestions = [
  {
    id: 'gd_q1',
    question: 'Hệ màu nào sau đây được sử dụng phổ biến nhất trong thiết kế in ấn tạp chí?',
    options: ['RGB', 'CMYK', 'Lab Color', 'Grayscale'],
    correctIndex: 1,
  },
  {
    id: 'gd_q2',
    question: 'Độ phân giải (DPI) tối thiểu khuyên dùng cho file thiết kế in ấn chất lượng cao là bao nhiêu?',
    options: ['72 DPI', '150 DPI', '300 DPI', '600 DPI'],
    correctIndex: 2,
  },
]

const gdComments = [
  {
    id: 'gd_c1',
    author: 'Lê Mỹ Thuật',
    initials: 'LMT',
    avatarColor: 'from-purple-500 to-indigo-400',
    content: 'Thầy ơi, khi xuất file in ấn tạp chí thì mình chọn định dạng PDF nào là chuẩn nhất ạ?',
    time: '3 giờ trước',
    likes: 5,
    replies: [
      {
        id: 'gd_r1',
        author: 'ThS. Trần Typography',
        initials: 'GV',
        avatarColor: 'from-pink-600 to-purple-500',
        content: 'Chào em, thông thường khi gửi nhà in, em nên xuất file chuẩn "PDF/X-1a:2001" hoặc "High Quality Print" kèm thiết lập Bleed & Slug nhé.',
        time: '2 giờ trước',
        isTeacher: true,
      }
    ]
  }
]

const gdTimeline = [
  {
    id: 'gd_t1',
    title: 'Tiếp tục bài đang học',
    description: 'Nguyên lý Grid và Layout – Chương 2',
    time: 'Đang học',
    tone: 'purple',
    icon: 'Play',
  },
  {
    id: 'gd_t2',
    title: 'Nộp bài tập dàn trang tạp chí 8 trang',
    description: 'Hạn cuối hôm nay · 23:59',
    time: 'Khẩn cấp',
    tone: 'orange',
    icon: 'ClipboardList',
  },
  {
    id: 'gd_t3',
    title: 'Thi cuối kỳ thực hành',
    description: 'Thiết kế Portfolio cá nhân tại phòng Lab 5',
    time: 'Dự kiến 18/06',
    tone: 'violet',
    icon: 'GraduationCap',
  }
]

const gdAISummary = {
  points: [
    'Thiết kế dàn trang bắt buộc phải dựa trên Grid System để đảm bảo sự thống nhất và cân bằng.',
    'Dùng Master Page để tự động đánh số trang và lặp lại các header/footer.',
    'Cần chừa vùng Bleed (thường là 2-3mm) để tránh bị cắt lẹm trắng khi in ấn.',
    'Sử dụng Character/Paragraph Style giúp thay đổi style font chữ hàng loạt nhanh chóng.',
    'Luôn kiểm tra Links Panel để chắc chắn không bị thiếu hình ảnh gốc độ phân giải cao.'
  ]
}

const gdAssignment = {
  id: 'BT-GD-02',
  title: 'Thiết kế tạp chí thời trang 8 trang',
  courseCode: 'GD301',
  courseTitle: 'Thiết kế dàn trang',
  teacher: 'ThS. Trần Typography',
  class: 'GD1701',
  assignedAt: '12/05/2026',
  deadline: '2026-05-31T23:59:00',
  deadlineDisplay: '23:59 – 31/05/2026',
  status: 'submitted',
  statusLabel: 'Đã nộp',
  description: `Sinh viên thiết kế một cuốn tạp chí thời trang mini gồm 8 trang trên Adobe InDesign. Yêu cầu bài làm:\n\n1. Thiết lập khổ trang A4, Bleed 3mm, Margins 15mm.\n2. Sử dụng Grid System (3 hoặc 4 cột) đồng nhất xuyên suốt các trang.\n3. Tạo và sử dụng Paragraph Styles cho Body Text, Headings và Sub-headings.\n4. Hình ảnh minh họa phải có độ phân giải tối thiểu 300 DPI, chuyển về hệ màu CMYK.\n5. Nộp tệp tin đóng gói (Package folder) chứa file .indd, file .pdf xuất bản, thư mục Fonts và Links.`
}

const gdSubmissionRules = {
  allowedFormats: ['.zip', '.rar'],
  maxSizeMB: 50,
  maxAttempts: 2,
  currentAttempt: 1,
  canSubmit: true,
  note: 'Sinh viên bắt buộc phải dùng tính năng Package trong InDesign để đóng gói toàn bộ font và link ảnh trước khi nén zip nộp bài.'
}

const gdAttachments = [
  { id: 'gd_a1', name: 'Tài liệu hướng dẫn Package InDesign.pdf', size: '1.2 MB', type: 'pdf' },
  { id: 'gd_a2', name: 'Text bài viết mẫu.docx', size: '32 KB', type: 'docx' },
  { id: 'gd_a3', name: 'Folder ảnh sản phẩm thời trang.zip', size: '18.5 MB', type: 'zip' }
]

const gdSubmissions = [
  {
    id: 'gd_s1', attempt: 1,
    submittedAt: '25/05/2026 – 14:20', status: 'graded', statusLabel: 'Đã chấm',
    onTime: true, timeLabel: 'Đúng hạn',
    file: 'SV2026002_NguyenThietKe_Magazine.zip', fileSize: '24.2 MB',
    note: 'Bài làm hoàn thiện lần 1.', isLatest: true
  }
]

const gdPlagiarismResult = {
  status: 'safe',
  percentage: 8,
  checkedAt: '25/05/2026 – 14:30',
  detail: 'Tỷ lệ trùng lặp hình ảnh/bố cục ở mức an toàn. Bài làm hợp lệ.'
}

const gdFeedback = {
  graded: true,
  score: 8.5,
  maxScore: 10,
  gradedAt: '28/05/2026',
  gradedBy: 'ThS. Trần Typography',
  comment: 'Bài làm rất tốt. Thiết lập Grid chuẩn xác, layout phân cấp rõ ràng và typography đẹp mắt. Cách phối màu sắc thời trang hiện đại. Lưu ý căn chỉnh lề (Margins) ở trang đôi (Facing Pages) cần rộng hơn chút nữa để khi đóng gáy không bị mất chữ nhé.',
  aiSuggestion: 'Thiết kế đạt chất lượng cao. Cần chú ý thêm khoảng trống (White space) ở các trang tiêu điểm để tăng tính thẩm mỹ cho tạp chí thời trang.',
  rubric: [
    { label: 'Thiết lập Layout & Grid System', score: 3.0, max: 3.0 },
    { label: 'Sử dụng Typography & Styles', score: 2.5, max: 3.0 },
    { label: 'Chất lượng hình ảnh & File Package', score: 2.0, max: 2.0 },
    { label: 'Tư duy thẩm mỹ & Phối màu', score: 1.0, max: 2.0 }
  ]
}

// Dữ liệu chi tiết khóa học Marketing
const mktCourse = {
  id: 'MR501',
  title: 'Digital Marketing',
  code: 'MR501',
  teacher: 'ThS. Lê SEO',
  semester: 'HK5 2025-2026',
  credits: 3,
  coverGradient: 'from-orange-600 via-red-500 to-yellow-400',
  description: 'Môn học giúp sinh viên nắm vững các công cụ và chiến lược tiếp thị kỹ thuật số hiện đại bao gồm SEO (Tối ưu hóa công cụ tìm kiếm), SEM, Social Media Marketing, Email Marketing và Web Analytics.'
}

const mktStats = [
  { label: 'Tiến độ', value: '30', unit: '%', icon: 'Gauge', tone: 'orange', progress: 30, hint: '3/10 bài đã hoàn thành' },
  { label: 'Bài học', value: '3', unit: '/10', icon: 'BookOpenCheck', tone: 'red', progress: 30, hint: 'Đã hoàn thành 3 bài' },
  { label: 'Bài tập', value: '1', unit: 'mục', icon: 'ClipboardList', tone: 'amber', progress: 50, hint: '1 bài tập đang thực hiện' },
  { label: 'Tài liệu', value: '10', unit: 'file', icon: 'Files', tone: 'violet', progress: 40, hint: 'Case studies, Slide bài giảng' }
]

export const mktLessons = [
  {
    id: 'mkt_ch1',
    chapter: 'Chương 1',
    title: 'Tổng quan về Digital Marketing & Strategy',
    description: 'Giới thiệu các kênh Digital Marketing, hành trình khách hàng số và lập kế hoạch chiến dịch.',
    status: 'completed',
    badge: 'Hoàn thành',
    tone: 'green',
    icon: 'CheckCircle2',
    meta: ['2 bài học'],
    progress: 100,
    lessons: [
      { id: 'mktl1-1', title: 'Tổng quan các kênh tiếp thị số', duration: '18:10', status: 'completed' },
      { id: 'mktl1-2', title: 'Xây dựng chân dung khách hàng số (Persona)', duration: '22:45', status: 'completed' },
    ]
  },
  {
    id: 'mkt_ch2',
    chapter: 'Chương 2',
    title: 'Tối ưu hóa Công cụ tìm kiếm (SEO)',
    description: 'Nguyên lý hoạt động của Google, nghiên cứu từ khóa (Keyword Research) và tối ưu hóa On-page SEO.',
    status: 'active',
    badge: 'Đang học',
    tone: 'orange',
    icon: 'ListTree',
    meta: ['3 bài học', '1 bài tập'],
    progress: 33,
    lessons: [
      { id: 'mktl2-1', title: 'Thuật toán tìm kiếm & Keyword Research', duration: '30:15', status: 'completed' },
      { id: 'mktl2-2', title: 'Tối ưu hóa On-page SEO website', duration: '24:20', status: 'active' },
      { id: 'mktl2-3', title: 'Chiến lược đi Backlink (Off-page SEO)', duration: '28:00', status: 'locked' },
      { id: 'mktl2-4', title: 'Thực hành tối ưu SEO On-page', duration: '–', status: 'locked', type: 'assignment' },
    ]
  }
]

const mktCurrentLesson = {
  id: 'mktl2-2',
  chapterId: 'mkt_ch2',
  chapterTitle: 'Chương 2: Tối ưu hóa Công cụ tìm kiếm (SEO)',
  title: 'Tối ưu hóa On-page SEO website',
  lessonType: 'video',
  videoUrl: '',
  duration: '24:20',
  watchedSeconds: 730,
  totalSeconds: 1460,
  durationSeconds: 1460,
  allowSeek: true,
  pauseOnBlur: true,
  minWatchPercentToComplete: 80,
  progressPercent: 50,
  maxWatchedSeconds: 730,
  completedAt: null,
  videoThumb: null,
  documentTitle: 'Checklist On-page SEO tối ưu.pdf',
  documentPages: 18,
  documentCurrentPage: 5,
}

const mktQuizQuestions = [
  {
    id: 'mkt_q1',
    question: 'Thẻ nào sau đây đóng vai trò quan trọng nhất đối với SEO On-page của một trang web?',
    options: ['Thẻ Title', 'Thẻ Meta Keywords', 'Thẻ Bold <b>', 'Thẻ Italic <i>'],
    correctIndex: 0,
  },
  {
    id: 'mkt_q2',
    question: 'Độ dài ký tự tiêu chuẩn được Google khuyến nghị hiển thị tốt nhất cho thẻ Meta Description là bao nhiêu?',
    options: ['50-60 ký tự', '150-160 ký tự', '250-300 ký tự', 'Không giới hạn'],
    correctIndex: 1,
  },
]

const mktComments = [
  {
    id: 'mkt_c1',
    author: 'Nguyễn Văn SEO',
    initials: 'NVS',
    avatarColor: 'from-orange-500 to-yellow-400',
    content: 'Thưa thầy, hiện tại công cụ nào là tốt nhất để nghiên cứu volume từ khóa miễn phí ạ?',
    time: '4 giờ trước',
    likes: 3,
    replies: [
      {
        id: 'mkt_r1',
        author: 'ThS. Lê SEO',
        initials: 'GV',
        avatarColor: 'from-red-600 to-orange-500',
        content: 'Chào em, em có thể sử dụng Google Keyword Planner (trong tài khoản Google Ads) hoặc Ubersuggest bản miễn phí để tra cứu volume từ khóa nhé.',
        time: '3 giờ trước',
        isTeacher: true,
      }
    ]
  }
]

const mktTimeline = [
  {
    id: 'mkt_t1',
    title: 'Tiếp tục bài học SEO On-page',
    description: 'Xem video chương 2 và hoàn thành checklist',
    time: 'Đang học',
    tone: 'orange',
    icon: 'Play',
  },
  {
    id: 'mkt_t2',
    title: 'Nộp bài thực hành tối ưu SEO On-page',
    description: 'Hạn cuối ngày mai · 23:59',
    time: 'Sắp đến hạn',
    tone: 'amber',
    icon: 'ClipboardList',
  }
]

const mktAISummary = {
  points: [
    'SEO On-page tập trung vào việc tối ưu nội dung và cấu trúc mã nguồn trên chính trang web.',
    'Thẻ Title cần chứa từ khóa chính và đặt ở ngay đầu thẻ.',
    'Tốc độ tải trang và tính thân thiện trên di động là các yếu tố xếp hạng quan trọng của Google.',
    'Nội dung cần hữu ích, tránh nhồi nhét từ khóa (Keyword stuffing).',
    'Hình ảnh cần có thẻ mô tả alt text giúp Google bot hiểu nội dung ảnh.'
  ]
}

const mktAssignment = {
  id: 'BT-MKT-01',
  title: 'Tối ưu hóa On-page SEO website',
  courseCode: 'MR501',
  courseTitle: 'Digital Marketing',
  teacher: 'ThS. Lê SEO',
  class: 'MR1801',
  assignedAt: '15/05/2026',
  deadline: '2026-05-31T23:59:00',
  deadlineDisplay: '23:59 – 31/05/2026',
  status: 'submitted',
  statusLabel: 'Đã nộp',
  description: `Sinh viên thực hiện tối ưu hóa SEO On-page cho một bài viết blog cụ thể trên website WordPress mẫu. Yêu cầu bài làm:\n\n1. Nghiên cứu bộ từ khóa chính và phụ bằng công cụ nghiên cứu từ khóa.\n2. Tối ưu tiêu đề Title tag (chứa từ khóa chính, dưới 60 ký tự) và thẻ Meta Description (dưới 160 ký tự).\n3. Tối ưu cấu trúc thẻ Heading (H1, H2, H3).\n4. Tối ưu hình ảnh (nén dung lượng, đổi tên file chuẩn SEO, thêm Alt text chứa từ khóa).\n5. Viết nội dung chuẩn SEO tối thiểu 800 từ và chèn liên kết nội bộ (Internal link).\n6. Chụp ảnh minh họa kết quả tối ưu từ RankMath SEO hoặc Yoast SEO đạt điểm xanh.`
}

const mktSubmissionRules = {
  allowedFormats: ['.pdf', '.docx'],
  maxSizeMB: 10,
  maxAttempts: 2,
  currentAttempt: 1,
  canSubmit: true,
  note: 'Nộp báo cáo chi tiết định dạng PDF hoặc Word trình bày các bước thực hiện và ảnh chụp minh chứng điểm SEO.'
}

const mktAttachments = [
  { id: 'mkt_a1', name: 'Checklist tối ưu SEO On-page chuẩn 2026.pdf', size: '420 KB', type: 'pdf' },
  { id: 'mkt_a2', name: 'Bài viết thô chưa tối ưu.docx', size: '25 KB', type: 'docx' }
]

const mktSubmissions = [
  {
    id: 'mkt_s1', attempt: 1,
    submittedAt: '26/05/2026 – 09:15', status: 'graded', statusLabel: 'Đã chấm',
    onTime: true, timeLabel: 'Đúng hạn',
    file: 'SV2026003_TranThiMarketing_SEO.pdf', fileSize: '1.5 MB',
    note: 'Bài nộp báo cáo SEO.', isLatest: true
  }
]

const mktPlagiarismResult = {
  status: 'safe',
  percentage: 5,
  checkedAt: '26/05/2026 – 09:20',
  detail: 'Nội dung bài viết tự viết, không trùng lặp đáng kể.'
}

const mktFeedback = {
  graded: true,
  score: 9.1,
  maxScore: 10,
  gradedAt: '29/05/2026',
  gradedBy: 'ThS. Lê SEO',
  comment: 'Bài làm rất tốt, tối ưu tiêu đề và thẻ mô tả rất thu hút, mật độ từ khóa phân bố tự nhiên và hợp lý. Kết quả chấm điểm trên plugin Yoast SEO đạt 88 điểm xanh. Cần cải thiện thêm cấu trúc internal links hướng tới các trang danh mục sản phẩm chính nhé.',
  aiSuggestion: 'Tối ưu hóa SEO tốt. Có thể nghiên cứu thêm cách tối ưu Schema markup cho bài viết để hiển thị Rich Snippets đẹp hơn trên Google Search.',
  rubric: [
    { label: 'Nghiên cứu từ khóa & Cấu trúc H-tag', score: 3.0, max: 3.0 },
    { label: 'Tối ưu Title, Meta Description & Content', score: 3.0, max: 3.0 },
    { label: 'Tối ưu hóa Hình ảnh & Alt text', score: 1.6, max: 2.0 },
    { label: 'Internal & External Links', score: 1.5, max: 2.0 }
  ]
}

export function syncActiveStudentData() {
  const email = getActiveEmail()
  let profile = originalProfile
  let awards = originalAwards
  let disciplines = originalDisciplines
  let parents = originalParents
  let curriculum = originalStudentCurriculum
  let curriculumVersion = originalCurriculumVersionData
  let dashboard = originalStudentDashboard
  let subjects = originalSubjects
  let gradeSummary = originalGradeSummary
  let semesterGPA = originalSemesterGPA
  let exams = originalExams

  let assignment = originalAssignment
  let submissionRules = originalSubmissionRules
  let attachments = originalAttachments
  let submissions = originalSubmissions
  let plagiarismResult = originalPlagiarismResult
  let feedback = originalFeedback
  let course = originalCourse
  let stats = originalStats
  let lessons = originalLessons
  let currentLesson = originalCurrentLesson
  let quizQuestions = originalQuizQuestions
  let comments = originalComments
  let timeline = originalTimeline
  let aiSummary = originalAISummary

  if (email.includes('gd')) {
    profile = profileGD
    awards = [
      { id: 'AW-GD-01', title: 'Giải Nhất Cuộc thi Sáng tạo Logo 2026', type: 'Thi đấu', gpa: 'N/A', date: '12/03/2026' }
    ]
    disciplines = []
    parents = [
      { 
        id: 'PR-02', name: 'Nguyễn Văn Thiết (Bố)', email: 'thiet.nv@gmail.com', status: 'Connected',
        permissions: { grades: true, attendance: true, finance: true, schedule: true }
      }
    ]
    curriculum = curriculumGD
    curriculumVersion = curriculumVersionDataGD
    dashboard = dashboardGD
    subjects = subjectsGD
    gradeSummary = gradeSummaryGD
    semesterGPA = semesterGPAGD
    exams = examsGD

    assignment = gdAssignment
    submissionRules = gdSubmissionRules
    attachments = gdAttachments
    submissions = gdSubmissions
    plagiarismResult = gdPlagiarismResult
    feedback = gdFeedback
    course = gdCourse
    stats = gdStats
    lessons = gdLessons
    currentLesson = gdCurrentLesson
    quizQuestions = gdQuizQuestions
    comments = gdComments
    timeline = gdTimeline
    aiSummary = gdAISummary
  } else if (email.includes('mkt')) {
    profile = profileMKT
    awards = [
      { id: 'AW-MKT-01', title: 'Chứng chỉ Digital Marketing xuất sắc', type: 'Học thuật', gpa: 'N/A', date: '10/04/2026' }
    ]
    disciplines = []
    parents = [
      { 
        id: 'PR-03', name: 'Trần Văn Thịnh (Bố)', email: 'thinh.tv@gmail.com', status: 'Connected',
        permissions: { grades: true, attendance: true, finance: true, schedule: true }
      }
    ]
    curriculum = curriculumMKT
    curriculumVersion = curriculumVersionDataMKT
    dashboard = dashboardMKT
    subjects = subjectsMKT
    gradeSummary = gradeSummaryMKT
    semesterGPA = semesterGPAMKT
    exams = examsMKT

    assignment = mktAssignment
    submissionRules = mktSubmissionRules
    attachments = mktAttachments
    submissions = mktSubmissions
    plagiarismResult = mktPlagiarismResult
    feedback = mktFeedback
    course = mktCourse
    stats = mktStats
    lessons = mktLessons
    currentLesson = mktCurrentLesson
    quizQuestions = mktQuizQuestions
    comments = mktComments
    timeline = mktTimeline
    aiSummary = mktAISummary
  }

  // Sync profile
  for (const key in mockProfile) delete mockProfile[key]
  Object.assign(mockProfile, profile)

  // Sync awards
  mockAwards.length = 0
  mockAwards.push(...awards)

  // Sync disciplines
  mockDisciplines.length = 0
  mockDisciplines.push(...disciplines)

  // Sync parents
  mockParents.length = 0
  mockParents.push(...parents)

  // Sync curriculum
  for (const key in mockStudentCurriculum) delete mockStudentCurriculum[key]
  Object.assign(mockStudentCurriculum, curriculum)

  // Sync curriculumVersion
  for (const key in mockCurriculumVersionData) delete mockCurriculumVersionData[key]
  Object.assign(mockCurriculumVersionData, curriculumVersion)

  // Sync dashboard
  for (const key in studentDashboardMock) delete studentDashboardMock[key]
  Object.assign(studentDashboardMock, dashboard)

  // Sync subjects
  mockSubjects.length = 0
  mockSubjects.push(...subjects)

  // Sync gradeSummary
  for (const key in mockGradeSummary) delete mockGradeSummary[key]
  Object.assign(mockGradeSummary, gradeSummary)

  // Sync semesterGPA
  mockSemesterGPA.length = 0
  mockSemesterGPA.push(...semesterGPA)

  // Sync exams
  mockExams.length = 0
  mockExams.push(...exams)

  // Sync assignment details
  for (const key in mockAssignment) delete mockAssignment[key]
  Object.assign(mockAssignment, assignment)

  for (const key in mockSubmissionRules) delete mockSubmissionRules[key]
  Object.assign(mockSubmissionRules, submissionRules)

  mockAttachments.length = 0
  mockAttachments.push(...attachments)

  mockSubmissions.length = 0
  mockSubmissions.push(...submissions)

  for (const key in mockPlagiarismResult) delete mockPlagiarismResult[key]
  Object.assign(mockPlagiarismResult, plagiarismResult)

  for (const key in mockFeedback) delete mockFeedback[key]
  Object.assign(mockFeedback, feedback)

  // Sync course details
  for (const key in mockCourse) delete mockCourse[key]
  Object.assign(mockCourse, course)

  mockStats.length = 0
  mockStats.push(...stats)

  mockLessons.length = 0
  mockLessons.push(...lessons)

  for (const key in mockCurrentLesson) delete mockCurrentLesson[key]
  Object.assign(mockCurrentLesson, currentLesson)

  mockQuizQuestions.length = 0
  mockQuizQuestions.push(...quizQuestions)

  mockComments.length = 0
  mockComments.push(...comments)

  mockTimeline.length = 0
  mockTimeline.push(...timeline)

  for (const key in mockAISummary) delete mockAISummary[key]
  Object.assign(mockAISummary, aiSummary)
}

syncActiveStudentData()

