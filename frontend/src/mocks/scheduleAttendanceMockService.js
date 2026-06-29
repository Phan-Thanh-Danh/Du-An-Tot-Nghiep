import { scheduleAttendanceMockData } from './scheduleAttendanceMockData'

const delay = (ms = 300) => new Promise(resolve => setTimeout(resolve, ms))

export const scheduleAttendanceMockService = {
  // ── Student ──────────────────────────────────────────────
  async getStudentTodaySchedule() {
    await delay()
    return scheduleAttendanceMockData.sessions.filter(x => x.ngayHoc.startsWith('2025-10-15'))
  },
  async getStudentWeekSchedule(_query) {
    await delay()
    return { items: scheduleAttendanceMockData.sessions }
  },
  async getStudentAttendanceSummary(_query) {
    await delay()
    return { coMat: 45, diMuon: 3, coPhep: 1, vang: 2, tyLe: 90.0 }
  },
  async getStudentAttendanceHistory(_query) {
    await delay()
    const history = []
    scheduleAttendanceMockData.attendances.forEach(att => {
      const stu = att.danhSachSinhVien.find(x => x.maSinhVien === 'SE150001')
      const sess = scheduleAttendanceMockData.sessions.find(x => x.maBuoiHoc === att.maBuoiHoc)
      if(stu && sess) {
        history.push({ ...stu, maBuoiHoc: sess.maBuoiHoc, tenMon: sess.tenMon, ngayHoc: sess.ngayHoc })
      }
    })
    return { items: history }
  },

  // ── Teacher ──────────────────────────────────────────────
  async getTeacherTodayClasses() {
    await delay()
    return scheduleAttendanceMockData.sessions.filter(x => x.ngayHoc.startsWith('2025-10-15'))
  },
  async getTeacherWeekSchedule(_query) {
    await delay()
    return { items: scheduleAttendanceMockData.sessions }
  },
  async getAttendanceSession(sessionId) {
    await delay()
    const att = scheduleAttendanceMockData.attendances.find(x => x.maBuoiHoc === sessionId)
    const sess = scheduleAttendanceMockData.sessions.find(x => x.maBuoiHoc === sessionId)
    return { session: sess, attendance: att ? att.danhSachSinhVien : [] }
  },
  async updateAttendanceStudent(sessionId, studentId, _payload) {
    await delay(100)
    return { success: true }
  },
  async submitAttendance(_sessionId) {
    await delay(500)
    return { success: true }
  },
  async requestAttendanceUnlock(_sessionId, _payload) {
    await delay(500)
    return { success: true }
  },

  // ── Admin ────────────────────────────────────────────────
  async getShifts(_query) {
    await delay()
    return { items: scheduleAttendanceMockData.shifts }
  },
  async getSchedules(_query) {
    await delay()
    return { items: scheduleAttendanceMockData.schedules }
  },
  async checkScheduleConflict(_payload) {
    await delay(500)
    return { conflicts: [] }
  },
  async createScheduleMock(_payload) {
    await delay()
    return { success: true }
  },
  async getSessions(_query) {
    await delay()
    return { items: scheduleAttendanceMockData.sessions }
  },
  async changeSessionRoom(_sessionId, _payload) {
    await delay()
    return { success: true }
  },
  async changeSessionTeacher(_sessionId, _payload) {
    await delay()
    return { success: true }
  },
  async getAttendanceUnlockRequests(_query) {
    await delay()
    return { items: scheduleAttendanceMockData.unlockRequests }
  },
  async approveUnlockRequest(_id, _payload) {
    await delay()
    return { success: true }
  }
}
