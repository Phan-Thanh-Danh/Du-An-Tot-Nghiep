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
