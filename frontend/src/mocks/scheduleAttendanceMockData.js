import dayjs from 'dayjs'
import { studentDashboardMock } from '@/data/studentData.mock.js'

const shiftCatalog = [
  { id: 1, label: 'Ca 1', start: '07:30', end: '09:00' },
  { id: 2, label: 'Ca 2', start: '09:05', end: '12:00' },
  { id: 3, label: 'Ca 3', start: '13:00', end: '14:30' },
  { id: 4, label: 'Ca 4', start: '14:35', end: '16:05' },
]

const sessionChanges = [
  { status: 'binh_thuong' },
  { status: 'doi_phong', reason: 'Phòng cũ bảo trì máy chiếu, chuyển sang phòng dự phòng.' },
  { status: 'day_thay', substituteTeacher: 'ThS. Lê Hoàng Nam', reason: 'Giảng viên chính tham gia hội đồng bảo vệ.' },
  { status: 'doi_ca', reason: 'Điều chỉnh để tránh trùng lịch thực hành.' },
  { status: 'huy', reason: 'Buổi học được hủy và sẽ có lịch bù sau.' },
]

const attendanceStatuses = ['co_mat', 'di_muon', 'co_phep', 'vang', 'co_mat', 'chua_diem_danh']

const teacherCourses = [
  {
    id: 'java-se1601',
    code: 'PRO192',
    subject: 'Lập trình Java',
    className: 'SE1601',
    room: 'P.302',
    shift: shiftCatalog[0],
  },
  {
    id: 'web-ss1402',
    code: 'WEB201c',
    subject: 'Lập trình Web',
    className: 'SS1402',
    room: 'P.410',
    shift: shiftCatalog[1],
    changeStatus: 'doi_phong',
    changeNote: 'Chuyển từ P.305 sang P.410 do bảo trì máy chiếu.',
  },
  {
    id: 'db-sa1709',
    code: 'DBI202',
    subject: 'Cơ sở dữ liệu',
    className: 'SA1709',
    room: 'P.204',
    shift: shiftCatalog[2],
  },
  {
    id: 'algo-se1710',
    code: 'CSD201',
    subject: 'Cấu trúc dữ liệu và giải thuật',
    className: 'SE1710',
    room: 'P.501',
    shift: shiftCatalog[3],
    changeStatus: 'doi_ca',
    changeNote: 'Điều chỉnh ca học theo lịch thực hành phòng máy.',
  },
]

const teacherSessionStatuses = ['chua_mo', 'dang_diem_danh', 'da_gui', 'da_khoa']

const studentNames = [
  'Nguyễn Văn Anh',
  'Trần Thị Bình',
  'Lê Hoàng Cường',
  'Phạm Minh Danh',
  'Đỗ Thùy Dương',
  'Nguyễn Tiến Đạt',
  'Vũ Thị Giang',
  'Lê Minh Hải',
  'Phạm Thanh Hương',
  'Nguyễn Hữu Khánh',
  'Bùi Quang Long',
  'Mai Ngọc Linh',
]

const teacherAttendancePattern = [
  'co_mat',
  'co_mat',
  'di_muon',
  'vang',
  'co_phep',
  'co_mat',
  'co_mat',
  'vang',
  'co_mat',
  'co_mat',
  'chua_diem_danh',
  'co_mat',
]

function buildTeacherStudents(sessionId, className, offset = 0) {
  return studentNames.map((name, index) => {
    const status = teacherAttendancePattern[(index + offset) % teacherAttendancePattern.length]

    return {
      id: `${sessionId}-sv-${index + 1}`,
      studentCode: `SV${String(16000 + offset * 100 + index + 1).padStart(5, '0')}`,
      name,
      className,
      status,
      note:
        status === 'vang'
          ? 'Cần xác nhận lý do vắng'
          : status === 'co_phep'
            ? 'Đã báo phép trước giờ học'
            : status === 'di_muon'
              ? 'Đi muộn 12 phút'
              : '',
    }
  })
}

function getCourses() {
  return studentDashboardMock.courses?.length
    ? studentDashboardMock.courses
    : [
        {
          id: 'fallback',
          name: 'Học phần mẫu',
          code: 'MON101',
          lecturer: 'Giảng viên phụ trách',
        },
      ]
}

function weekStart(date = dayjs()) {
  const day = date.day()
  return date.startOf('day').subtract(day === 0 ? 6 : day - 1, 'day')
}

function buildDate(base, dayOffset, time) {
  const [hour, minute] = time.split(':').map(Number)
  return base.add(dayOffset, 'day').hour(hour).minute(minute).second(0).millisecond(0).toDate()
}

export function getStudentScheduleSessions(anchorDate = new Date()) {
  const base = weekStart(dayjs(anchorDate))

  return getCourses().flatMap((course, courseIndex) => {
    const primaryShift = shiftCatalog[courseIndex % shiftCatalog.length]
    const secondaryShift = shiftCatalog[(courseIndex + 2) % shiftCatalog.length]
    const firstDayOffset = courseIndex % 5
    const secondDayOffset = Math.min(6, firstDayOffset + 2)
    const firstChange = sessionChanges[courseIndex % sessionChanges.length]
    const secondChange = sessionChanges[(courseIndex + 2) % sessionChanges.length]

    return [
      {
        id: `week-${course.id}-1`,
        courseId: course.id,
        courseCode: course.code,
        subject: course.name,
        className: studentDashboardMock.student?.className || 'Lớp học phần',
        teacher: course.lecturer || 'Giảng viên phụ trách',
        substituteTeacher: firstChange.substituteTeacher || null,
        room: firstChange.status === 'doi_phong' ? `P.${410 + courseIndex}` : `P.${302 + courseIndex}`,
        shift: primaryShift,
        startAt: buildDate(base, firstDayOffset, primaryShift.start),
        endAt: buildDate(base, firstDayOffset, primaryShift.end),
        status: firstChange.status,
        reason: firstChange.reason || '',
      },
      {
        id: `week-${course.id}-2`,
        courseId: course.id,
        courseCode: course.code,
        subject: course.name,
        className: studentDashboardMock.student?.className || 'Lớp học phần',
        teacher: course.lecturer || 'Giảng viên phụ trách',
        substituteTeacher: secondChange.substituteTeacher || null,
        room: secondChange.status === 'doi_phong' ? `P.${510 + courseIndex}` : `P.${305 + courseIndex}`,
        shift: secondaryShift,
        startAt: buildDate(base, secondDayOffset, secondaryShift.start),
        endAt: buildDate(base, secondDayOffset, secondaryShift.end),
        status: secondChange.status,
        reason: secondChange.reason || '',
      },
    ]
  })
}

export function getStudentAttendanceHistory() {
  const base = dayjs().startOf('day').subtract(28, 'day')

  return getCourses()
    .flatMap((course, courseIndex) => {
      const teacher = course.lecturer || 'Giảng viên phụ trách'

      return Array.from({ length: 4 }, (_, sessionIndex) => {
        const status = attendanceStatuses[(courseIndex + sessionIndex) % attendanceStatuses.length]
        const shift = shiftCatalog[(courseIndex + sessionIndex) % shiftCatalog.length]

        return {
          id: `att-${course.id}-${sessionIndex}`,
          courseId: course.id,
          subject: course.name,
          courseCode: course.code,
          teacher,
          room: `P.${302 + courseIndex}`,
          shift,
          attendedAt: buildDate(base, courseIndex * 2 + sessionIndex * 5, shift.start),
          endedAt: buildDate(base, courseIndex * 2 + sessionIndex * 5, shift.end),
          status,
          note:
            status === 'vang'
              ? 'Vắng không phép, có thể tạo đơn giải trình nếu có minh chứng.'
              : status === 'co_phep'
                ? 'Đã được ghi nhận nghỉ có phép.'
                : status === 'di_muon'
                  ? 'Đi muộn 12 phút.'
                  : status === 'chua_diem_danh'
                    ? 'Giảng viên chưa chốt điểm danh.'
                    : 'Đi học đúng giờ.',
        }
      })
    })
    .sort((left, right) => dayjs(right.attendedAt).valueOf() - dayjs(left.attendedAt).valueOf())
}

export function getAttendanceBySubject() {
  const history = getStudentAttendanceHistory()

  return getCourses().map((course) => {
    const rows = history.filter((item) => item.courseId === course.id)
    const counted = rows.filter((item) => item.status !== 'chua_diem_danh')
    const positive = counted.filter((item) => ['co_mat', 'di_muon', 'co_phep'].includes(item.status))
    const rate = counted.length ? Math.round((positive.length / counted.length) * 100) : 100

    return {
      courseId: course.id,
      subject: course.name,
      rate,
      absent: rows.filter((item) => item.status === 'vang').length,
      total: counted.length,
    }
  })
}

export function getTeacherTodaySessions(anchorDate = new Date()) {
  const today = dayjs(anchorDate).startOf('day')

  return teacherCourses.map((course, index) => ({
    id: `teacher-today-${course.id}`,
    courseId: course.id,
    courseCode: course.code,
    subject: course.subject,
    className: course.className,
    room: course.room,
    shift: course.shift,
    startAt: buildDate(today, 0, course.shift.start),
    endAt: buildDate(today, 0, course.shift.end),
    status: teacherSessionStatuses[index % teacherSessionStatuses.length],
    changeStatus: course.changeStatus || null,
    changeNote: course.changeNote || '',
    lockedReason: index === 3 ? 'Đã quá thời hạn chỉnh sửa sau khi gửi điểm danh.' : '',
    unlockEligible: index >= 2,
    students: buildTeacherStudents(`teacher-today-${course.id}`, course.className, index),
  }))
}

export function getTeacherAttendanceHistory() {
  const base = dayjs().startOf('day').subtract(18, 'day')
  const historyStatuses = ['da_gui', 'da_khoa', 'da_gui', 'da_khoa', 'da_gui', 'da_huy']

  return teacherCourses.flatMap((course, courseIndex) =>
    Array.from({ length: 2 }, (_, sessionIndex) => {
      const shift = shiftCatalog[(courseIndex + sessionIndex) % shiftCatalog.length]
      const status = historyStatuses[(courseIndex + sessionIndex) % historyStatuses.length]
      const students = buildTeacherStudents(
        `teacher-history-${course.id}-${sessionIndex}`,
        course.className,
        courseIndex + sessionIndex,
      ).map((student) => ({
        ...student,
        status: student.status === 'chua_diem_danh' ? 'co_mat' : student.status,
      }))
      const absent = students.filter((student) => student.status === 'vang').length
      const present = students.filter((student) => student.status === 'co_mat').length
      const late = students.filter((student) => student.status === 'di_muon').length
      const excused = students.filter((student) => student.status === 'co_phep').length

      return {
        id: `teacher-history-${course.id}-${sessionIndex}`,
        courseId: course.id,
        courseCode: course.code,
        subject: course.subject,
        className: course.className,
        room: sessionIndex === 0 ? course.room : `P.${305 + courseIndex}`,
        shift,
        date: buildDate(base, courseIndex * 3 + sessionIndex * 5, shift.start),
        endAt: buildDate(base, courseIndex * 3 + sessionIndex * 5, shift.end),
        status,
        total: students.length,
        present,
        late,
        excused,
        absent,
        submittedAt: buildDate(base, courseIndex * 3 + sessionIndex * 5, shift.end),
        lockedAt: status === 'da_khoa' ? buildDate(base, courseIndex * 3 + sessionIndex * 5 + 1, '08:00') : null,
        lockReason: status === 'da_khoa' ? 'Đã hết hạn chỉnh sửa điểm danh.' : '',
        students,
      }
    }),
  ).sort((left, right) => dayjs(right.date).valueOf() - dayjs(left.date).valueOf())
}

export function getTeacherUnlockRequests() {
  const history = getTeacherAttendanceHistory()

  return [
    {
      id: 'unlock-1',
      sessionId: history[1]?.id,
      subject: history[1]?.subject || 'Lập trình Java',
      className: history[1]?.className || 'SE1601',
      status: 'cho_duyet',
      reason: 'Cần bổ sung minh chứng sinh viên có phép sau khi buổi đã khóa.',
      createdAt: dayjs().subtract(1, 'day').toDate(),
    },
    {
      id: 'unlock-2',
      sessionId: history[3]?.id,
      subject: history[3]?.subject || 'Cơ sở dữ liệu',
      className: history[3]?.className || 'SA1709',
      status: 'da_duyet',
      reason: 'Mở khóa để cập nhật trạng thái đi muộn theo biên bản lớp.',
      createdAt: dayjs().subtract(5, 'day').toDate(),
    },
  ]
}

// ─────────────────────────────────────────────────────────────────────────────
// PHASE 7 — Staff/BGH Schedule mock data
// ─────────────────────────────────────────────────────────────────────────────

export const caHocCatalog = [
  { id: 'ca1', maCaHoc: 'CA1', tenCa: 'Ca 1', buoi: 'sang', gioBatDau: '07:30', gioKetThuc: '09:00', thuTu: 1, conHoatDong: true },
  { id: 'ca2', maCaHoc: 'CA2', tenCa: 'Ca 2', buoi: 'sang', gioBatDau: '09:10', gioKetThuc: '10:40', thuTu: 2, conHoatDong: true },
  { id: 'ca3', maCaHoc: 'CA3', tenCa: 'Ca 3', buoi: 'sang', gioBatDau: '10:50', gioKetThuc: '12:20', thuTu: 3, conHoatDong: true },
  { id: 'ca4', maCaHoc: 'CA4', tenCa: 'Ca 4', buoi: 'chieu', gioBatDau: '13:00', gioKetThuc: '14:30', thuTu: 4, conHoatDong: true },
  { id: 'ca5', maCaHoc: 'CA5', tenCa: 'Ca 5', buoi: 'chieu', gioBatDau: '14:40', gioKetThuc: '16:10', thuTu: 5, conHoatDong: true },
  { id: 'ca6', maCaHoc: 'CA6', tenCa: 'Ca 6', buoi: 'toi', gioBatDau: '18:00', gioKetThuc: '19:30', thuTu: 6, conHoatDong: true },
]

export const thuTrongTuanOptions = [
  { value: 2, label: 'Thứ 2' },
  { value: 3, label: 'Thứ 3' },
  { value: 4, label: 'Thứ 4' },
  { value: 5, label: 'Thứ 5' },
  { value: 6, label: 'Thứ 6' },
  { value: 7, label: 'Thứ 7' },
]

export const staffScheduleRows = [
  {
    id: 'TKB-001', maTkb: 'TKB-001',
    hocKy: { ma: 'SP2026', ten: 'Spring 2026' },
    lop: { ma: 'SE1601', ten: 'SE1601 - Kỹ thuật phần mềm' },
    monHoc: { ma: 'PRO192', ten: 'Lập trình Java' },
    giaoVien: { ma: 'GV001', ten: 'TS. Nguyễn Văn An', email: 'an.nv@fpt.edu.vn' },
    thuTrongTuan: 2, caHoc: { id: 'ca1', tenCa: 'Ca 1', gioBatDau: '07:30', gioKetThuc: '09:00' },
    phongHoc: { ma: 'P302', ten: 'P.302' },
    trangThai: 'da_xuat_ban',
    ngayBatDau: '2026-01-06', ngayKetThuc: '2026-05-30', ghiChu: '',
  },
  {
    id: 'TKB-002', maTkb: 'TKB-002',
    hocKy: { ma: 'SP2026', ten: 'Spring 2026' },
    lop: { ma: 'SE1601', ten: 'SE1601 - Kỹ thuật phần mềm' },
    monHoc: { ma: 'CSD201', ten: 'Cấu trúc dữ liệu & Giải thuật' },
    giaoVien: { ma: 'GV002', ten: 'ThS. Trần Thị Bình', email: 'binh.tt@fpt.edu.vn' },
    thuTrongTuan: 4, caHoc: { id: 'ca3', tenCa: 'Ca 3', gioBatDau: '10:50', gioKetThuc: '12:20' },
    phongHoc: { ma: 'LAB2', ten: 'Lab 2' },
    trangThai: 'da_xuat_ban',
    ngayBatDau: '2026-01-06', ngayKetThuc: '2026-05-30', ghiChu: '',
  },
  {
    id: 'TKB-003', maTkb: 'TKB-003',
    hocKy: { ma: 'SP2026', ten: 'Spring 2026' },
    lop: { ma: 'SE1602', ten: 'SE1602 - Kỹ thuật phần mềm' },
    monHoc: { ma: 'WEB201', ten: 'Lập trình Web' },
    giaoVien: { ma: 'GV003', ten: 'ThS. Lê Văn Cường', email: 'cuong.lv@fpt.edu.vn' },
    thuTrongTuan: 3, caHoc: { id: 'ca2', tenCa: 'Ca 2', gioBatDau: '09:10', gioKetThuc: '10:40' },
    phongHoc: { ma: 'P105', ten: 'P.105' },
    trangThai: 'nhap',
    ngayBatDau: '2026-01-06', ngayKetThuc: '2026-05-30', ghiChu: 'Chờ xác nhận phòng',
  },
  {
    id: 'TKB-004', maTkb: 'TKB-004',
    hocKy: { ma: 'SP2026', ten: 'Spring 2026' },
    lop: { ma: 'SA1709', ten: 'SA1709 - Quản trị doanh nghiệp' },
    monHoc: { ma: 'DBI202', ten: 'Cơ sở dữ liệu' },
    giaoVien: { ma: 'GV001', ten: 'TS. Nguyễn Văn An', email: 'an.nv@fpt.edu.vn' },
    thuTrongTuan: 5, caHoc: { id: 'ca4', tenCa: 'Ca 4', gioBatDau: '13:00', gioKetThuc: '14:30' },
    phongHoc: { ma: 'P204', ten: 'P.204' },
    trangThai: 'nhap',
    ngayBatDau: '2026-01-06', ngayKetThuc: '2026-05-30', ghiChu: '',
  },
  {
    id: 'TKB-005', maTkb: 'TKB-005',
    hocKy: { ma: 'SU2026', ten: 'Summer 2026' },
    lop: { ma: 'SE1610', ten: 'SE1610 - Kỹ thuật phần mềm' },
    monHoc: { ma: 'NET101', ten: 'Mạng máy tính' },
    giaoVien: { ma: 'GV004', ten: 'TS. Phạm Minh Đức', email: 'duc.pm@fpt.edu.vn' },
    thuTrongTuan: 6, caHoc: { id: 'ca5', tenCa: 'Ca 5', gioBatDau: '14:40', gioKetThuc: '16:10' },
    phongHoc: { ma: 'LAB1', ten: 'Lab 1' },
    trangThai: 'da_huy',
    ngayBatDau: '2026-06-01', ngayKetThuc: '2026-08-31', ghiChu: 'Hủy do số lượng sinh viên không đủ',
  },
]

export const scheduleConflictRows = [
  {
    id: 'CFL-001', loai: 'giang_vien',
    doiTuong: 'TS. Nguyen Van An (GV001)',
    lichHienTai: 'TKB-001 · Thứ 2 · Ca 1 · P.302',
    lichMoi: 'TKB-004 · Thứ 2 · Ca 1 · P.204 (đề xuất)',
    moTa: 'Giảng viên TS. Nguyễn Văn An đã có lịch TKB-001 vào Thứ 2 Ca 1. Lịch TKB-004 trùng cùng thứ và ca.',
    mucDo: 'critical',
    soTietAnhHuong: 18,
    trangThaiXuLy: 'chua_xu_ly',
    deXuat: 'Đổi sang Ca 2 (09:10-10:40) hoặc chọn giảng viên khác',
  },
  {
    id: 'CFL-002', loai: 'phong_hoc',
    doiTuong: 'P.302',
    lichHienTai: 'TKB-001 · Thứ 2 · Ca 1 · SE1601',
    lichMoi: 'Request Thứ 2 Ca 1 P.302 từ lớp IT201 (đề xuất)',
    moTa: 'Phòng P.302 đã được dùng bởi TKB-001 vào Thứ 2 Ca 1. Yêu cầu mới cũng cần phòng này cùng ca.',
    mucDo: 'critical',
    soTietAnhHuong: 10,
    trangThaiXuLy: 'chua_xu_ly',
    deXuat: 'Đổi sang phòng P.303 (trống) hoặc Lab 3 (trống)',
  },
  {
    id: 'CFL-003', loai: 'lop_hoc',
    doiTuong: 'SE1602',
    lichHienTai: 'TKB-003 · Thứ 3 · Ca 2 · WEB201',
    lichMoi: 'Request Thứ 3 Ca 2 cho SE1602 môn CSD201 (đề xuất)',
    moTa: 'Lớp SE1602 đã có lịch Thứ 3 Ca 2 môn Lập trình Web. Không thể xếp thêm môn khác cùng thứ cùng ca.',
    mucDo: 'major',
    soTietAnhHuong: 6,
    trangThaiXuLy: 'dang_xu_ly',
    deXuat: 'Đổi sang Thứ 4 Ca 3 (trống cho lớp SE1602)',
  },
]

export const bghPendingSchedules = [
  {
    id: 'BTKT-001', maTkb: 'BTKT-001',
    tenHocKy: 'Spring 2026',
    tenDonVi: 'Khoa Kỹ thuật phần mềm',
    soLop: 12, soGiaoVien: 8, tongSoTiet: 96,
    ngayTao: '15/12/2025',
    xungDot: 2,
    doUrgent: 'cao',
    nguoiTao: 'Phòng Đào tạo',
    conflicts: [
      'GV Nguyễn Văn An trùng lịch Thứ 2 Ca 1',
      'Phòng P.302 bị đặt 2 lịch cùng lúc Thứ 4 Ca 3',
    ],
    trangThai: 'cho_duyet',
  },
  {
    id: 'BTKT-002', maTkb: 'BTKT-002',
    tenHocKy: 'Spring 2026',
    tenDonVi: 'Khoa Kinh tế',
    soLop: 8, soGiaoVien: 5, tongSoTiet: 64,
    ngayTao: '16/12/2025',
    xungDot: 0,
    doUrgent: 'trung_binh',
    nguoiTao: 'Phòng Đào tạo',
    conflicts: [],
    trangThai: 'cho_duyet',
  },
  {
    id: 'BTKT-003', maTkb: 'BTKT-003',
    tenHocKy: 'Summer 2026',
    tenDonVi: 'Khoa Thiết kế đồ họa',
    soLop: 5, soGiaoVien: 4, tongSoTiet: 40,
    ngayTao: '17/12/2025',
    xungDot: 1,
    doUrgent: 'thap',
    nguoiTao: 'Phòng Đào tạo',
    conflicts: ['Lớp DG1401 trùng lịch Thứ 5 Ca 2'],
    trangThai: 'tra_ve',
  },
]

export const bghPublishedSchedules = [
  {
    id: 'BTKB-001', maTkb: 'BTKB-001',
    tenHocKy: 'Spring 2026',
    tenDonVi: 'Khoa CNTT',
    soLop: 15, soGiaoVien: 10, tongSoTiet: 120,
    ngayXuatBan: '2026-01-02',
    thayDoiPhatSinh: 3,
    buoiHuy: 1,
    trangThai: 'da_xuat_ban',
  },
  {
    id: 'BTKB-002', maTkb: 'BTKB-002',
    tenHocKy: 'Spring 2026',
    tenDonVi: 'Khoa Kinh tế',
    soLop: 8, soGiaoVien: 6, tongSoTiet: 64,
    ngayXuatBan: '2026-01-03',
    thayDoiPhatSinh: 1,
    buoiHuy: 0,
    trangThai: 'da_xuat_ban',
  },
  {
    id: 'BTKB-003', maTkb: 'BTKB-003',
    tenHocKy: 'Fall 2025',
    tenDonVi: 'Khoa Thiết kế',
    soLop: 6, soGiaoVien: 4, tongSoTiet: 48,
    ngayXuatBan: '2025-08-01',
    thayDoiPhatSinh: 0,
    buoiHuy: 2,
    trangThai: 'da_xuat_ban',
  },
]

export const bghScheduleChanges = [
  {
    id: 'CHG-201',
    ngayHoc: '2026-05-12',
    lop: 'SE1601',
    monHoc: 'Lập trình Java',
    giaoVien: 'TS. Nguyễn Văn An',
    loaiThayDoi: 'day_thay',
    truoc: 'GV: TS. Nguyễn Văn An · Ca 1 · P.302',
    sau: 'GV: ThS. Lê Hoàng Nam · Ca 1 · P.302',
    lyDo: 'Giảng viên chính tham gia hội đồng bảo vệ luận văn.',
    nguoiCapNhat: 'Nguyễn Thị Hoa (GV)',
    ngayCapNhat: '2026-05-10',
    trangThai: 'da_xac_nhan',
  },
  {
    id: 'CHG-202',
    ngayHoc: '2026-05-13',
    lop: 'SE1602',
    monHoc: 'Lập trình Web',
    giaoVien: 'ThS. Lê Văn Cường',
    loaiThayDoi: 'doi_phong',
    truoc: 'Ca 2 · P.105',
    sau: 'Ca 2 · P.210 (thay thế)',
    lyDo: 'P.105 bảo trì máy chiếu đột xuất.',
    nguoiCapNhat: 'Trần Hữu Hùng (GV)',
    ngayCapNhat: '2026-05-12',
    trangThai: 'da_xac_nhan',
  },
  {
    id: 'CHG-203',
    ngayHoc: '2026-05-14',
    lop: 'SA1709',
    monHoc: 'Cơ sở dữ liệu',
    giaoVien: 'TS. Nguyễn Văn An',
    loaiThayDoi: 'huy_buoi',
    truoc: 'Ca 4 · P.204',
    sau: '(Buổi bị hủy — sẽ có lịch bù)',
    lyDo: 'Cả lớp tham gia đợt thi sát hạch kỹ năng nghề.',
    nguoiCapNhat: 'Phạm Thị Mai (GV)',
    ngayCapNhat: '2026-05-11',
    trangThai: 'cho_xac_nhan',
  },
  {
    id: 'CHG-204',
    ngayHoc: '2026-05-16',
    lop: 'SE1610',
    monHoc: 'Mạng máy tính',
    giaoVien: 'TS. Phạm Minh Đức',
    loaiThayDoi: 'doi_ca',
    truoc: 'Ca 5 · Lab 1',
    sau: 'Ca 2 · Lab 1 (đổi ca)',
    lyDo: 'Điều chỉnh để tránh trùng lịch thi giữa kỳ.',
    nguoiCapNhat: 'Nguyễn Hữu Tài (GV)',
    ngayCapNhat: '2026-05-14',
    trangThai: 'da_xac_nhan',
  },
]
