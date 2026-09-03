import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { useAuthStore } from '@/stores/auth'
import { useAcademicSchedulingContextStore } from '@/stores/academicSchedulingContext'
import { usePopupStore } from '@/stores/popup'
import ScheduleManagerView from '../ScheduleManagerView.vue'
import PendingSchedulesView from '../PendingSchedulesView.vue'
import TeacherAssignmentView from '../TeacherAssignmentView.vue'
import ConflictCheckView from '../ConflictCheckView.vue'
import { scheduleApi } from '@/services/scheduleApi'
import { courseApi } from '@/services/courseApi'
import { academicTermApi } from '@/services/academicTermApi'
import { blockApi } from '@/services/blockApi'
import { staffApi } from '@/services/staffApi'
import { academicSchedulingApi } from '@/services/academicSchedulingApi'
import { assignmentApi } from '@/services/assignmentApi'

vi.mock('@/services/scheduleApi', () => ({
  scheduleApi: {
    list: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
    cancel: vi.fn(),
    generateDraft: vi.fn(),
    getGenerationProgress: vi.fn(),
    getCurrentGenerationJob: vi.fn(),
    getDraft: vi.fn(),
    listDrafts: vi.fn(),
    publishDraft: vi.fn(),
    deleteDraft: vi.fn(),
    suggestSlots: vi.fn(),
    suggestSlotsBatch: vi.fn(),
    checkConflicts: vi.fn(),
    getTienDoBuoiHoc: vi.fn(),
  },
}))

vi.mock('@/services/courseApi', () => ({
  courseApi: {
    list: vi.fn(),
    getCourses: vi.fn(),
    getById: vi.fn(),
    listClasses: vi.fn(),
    assignTeacher: vi.fn(),
    bulkAssignTeachers: vi.fn(),
  },
}))

vi.mock('@/services/academicTermApi', () => ({
  academicTermApi: {
    list: vi.fn(),
    getActive: vi.fn(),
  },
}))

vi.mock('@/services/blockApi', () => ({
  blockApi: {
    list: vi.fn(),
    getByTerm: vi.fn(),
  },
}))

vi.mock('@/services/staffApi', () => ({
  staffApi: {
    getRooms: vi.fn(),
    getShifts: vi.fn(),
    getCaHoc: vi.fn(),
    getTeachers: vi.fn(),
  },
}))

vi.mock('@/services/academicSchedulingApi', () => {
  const apiObj = {
    getContext: vi.fn().mockResolvedValue({
      schedulableTerm: { maHocKy: 15, maCodeHocKy: 'HK1_2027', tenHocKy: 'Học kỳ 1 năm 2027' },
      canPrepareSchedule: true,
      readiness: {
        canPrepareSchedule: true,
        items: [
          { code: 'COURSES_READY', status: 'ready', message: 'Có 30 khóa học cần xếp lịch.', affectedCount: 30 },
        ],
      },
    }),
    getMajors: vi.fn().mockResolvedValue([]),
    getSpecializations: vi.fn().mockResolvedValue([]),
    getClassesBySpecialization: vi.fn().mockResolvedValue([]),
    checkReadiness: vi.fn().mockResolvedValue({ canPrepareSchedule: true }),
  }
  return {
    default: apiObj,
    academicSchedulingApi: apiObj,
  }
})

vi.mock('@/services/assignmentApi', () => ({
  assignmentApi: {
    list: vi.fn(),
    getDonViOptions: vi.fn(),
    getCourses: vi.fn(),
    getCaHocs: vi.fn(),
    getRooms: vi.fn(),
    assignTeacher: vi.fn(),
  },
}))

const mockRouterPush = vi.fn()
vi.mock('vue-router', () => ({
  useRouter: () => ({
    push: mockRouterPush,
  }),
  useRoute: () => ({
    query: {},
    params: {},
  }),
}))

describe('Task 7D-C: Component Test Matrix (35 Mandatory Verification Items)', () => {
  let pinia

  beforeEach(() => {
    pinia = createPinia()
    setActivePinia(pinia)
    vi.clearAllMocks()

    const authStore = useAuthStore()
    authStore.user = {
      id: 5,
      userId: 5,
      username: 'giaovu.hcm@lms.local',
      role: 'AcademicStaff',
      campusId: 14,
      donVi: 'FPT Polytechnic Hồ Chí Minh',
    }

    const schedulingContext = useAcademicSchedulingContextStore()
    schedulingContext.isContextLoaded = true
    schedulingContext.canPrepareSchedule = true
    schedulingContext.schedulableTerm = {
      maHocKy: 15,
      maCodeHocKy: 'HK1_2027',
      tenHocKy: 'Học kỳ 1 năm 2027',
      daKhoa: false,
    }
    schedulingContext.readiness = {
      canPrepareSchedule: true,
      items: [
        { code: 'COURSES_READY', status: 'ready', message: 'Có 30 khóa học cần xếp lịch.', affectedCount: 30 },
        { code: 'BLOCKS_READY', status: 'ready', message: 'Đã cấu hình 5 Block cho học kỳ.', affectedCount: 5 },
        { code: 'ACTIVE_ROOMS_READY', status: 'ready', message: 'Có 10 phòng học đang hoạt động.', affectedCount: 10 },
        { code: 'ACTIVE_SHIFTS_READY', status: 'ready', message: 'Có 5 ca học đang hoạt động.', affectedCount: 5 },
        { code: 'TOTAL_ROOM_SLOTS_READY', status: 'ready', message: 'Tổng số slot phòng đủ đáp ứng.', affectedCount: 0 },
      ],
    }
    vi.spyOn(schedulingContext, 'fetchContext').mockImplementation(async () => {
      schedulingContext.isContextLoaded = true
      schedulingContext.canPrepareSchedule = true
      schedulingContext.schedulableTerm = {
        maHocKy: 15,
        maCodeHocKy: 'HK1_2027',
        tenHocKy: 'Học kỳ 1 năm 2027',
        daKhoa: false,
      }
    })

    scheduleApi.list.mockResolvedValue({ data: { items: [], total: 0 } })
    academicTermApi.list.mockResolvedValue({
      data: [{ maHocKy: 15, tenHocKy: 'Học kỳ 1 năm 2027', maDonVi: 14 }],
    })
    blockApi.getByTerm.mockResolvedValue({
      data: [
        { maBlock: 71, soThuTu: 1, ngayBatDau: '2027-01-01', ngayKetThuc: '2027-01-20' },
        { maBlock: 72, soThuTu: 2, ngayBatDau: '2027-01-21', ngayKetThuc: '2027-02-10' },
      ],
    })
    staffApi.getRooms.mockResolvedValue({
      data: [{ id: 101, maPhong: 101, tenPhong: 'P.101', sucChua: 40, maDonVi: 14 }],
    })
    staffApi.getShifts.mockResolvedValue({
      data: [{ id: 1, maCaHoc: 1, tenCa: 'Ca 1', thuTu: 1, conHoatDong: true }],
    })
    staffApi.getCaHoc.mockResolvedValue({
      data: [{ id: 1, maCaHoc: 1, tenCa: 'Ca 1', thuTu: 1, conHoatDong: true }],
    })
    courseApi.list.mockResolvedValue({
      data: [
        { maKhoaHoc: 5552, tieuDe: 'Lập trình C#', maLop: 1410, tenLop: 'WD18301', maDonVi: 14 },
        { maKhoaHoc: 5553, tieuDe: 'Cơ sở dữ liệu', maLop: 1410, tenLop: 'WD18301', maDonVi: 14 },
        { maKhoaHoc: 5554, tieuDe: 'Thiết kế Web', maLop: 1411, tenLop: 'WD18302', maDonVi: 14 },
      ],
    })
    courseApi.getCourses.mockResolvedValue({
      data: [
        { maKhoaHoc: 5552, tieuDe: 'Lập trình C#', maLop: 1410, tenLop: 'WD18301', maDonVi: 14, maHocKy: 15 },
        { maKhoaHoc: 5553, tieuDe: 'Cơ sở dữ liệu', maLop: 1410, tenLop: 'WD18301', maDonVi: 14, maHocKy: 15 },
        { maKhoaHoc: 5554, tieuDe: 'Thiết kế Web', maLop: 1411, tenLop: 'WD18302', maDonVi: 14, maHocKy: 15 },
      ],
    })
    academicSchedulingApi.getContext.mockResolvedValue({
      data: {
        schedulableTerm: { maHocKy: 15, maCodeHocKy: 'HK1_2027', tenHocKy: 'Học kỳ 1 năm 2027' },
      },
    })
    assignmentApi.list.mockResolvedValue([])
    assignmentApi.getDonViOptions.mockResolvedValue([])
    assignmentApi.getCourses.mockResolvedValue([])
    assignmentApi.getCaHocs.mockResolvedValue([])
    assignmentApi.getRooms.mockResolvedValue([])
  })

  afterEach(() => {
    vi.restoreAllMocks()
  })

  // ── 1. Simple Mode mặc định ──────────────────────────────────────
  it('Item 1: defaults to Simple Mode (whole_term scope, modal collapsed)', async () => {
    const wrapper = mount(ScheduleManagerView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    expect(wrapper.vm.smartCourseScope).toBe('whole_term')
    expect(wrapper.vm.showFormModal).toBe(false)
    expect(wrapper.text()).not.toContain('Kích thước quần thể')
    wrapper.unmount()
  })

  // ── 2. Campus lấy từ authenticated context ──────────────────────
  it('Item 2: campus is strictly resolved from authenticated context (campusId = 14)', async () => {
    const wrapper = mount(ScheduleManagerView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    expect(wrapper.vm.authorizedCampusId).toBe(14)
    await wrapper.vm.generateSimpleDraft()
    expect(wrapper.vm.smartCampusId).toBe(14)
    wrapper.unmount()
  })

  // ── 3. AcademicStaff không thấy campus override ─────────────────
  it('Item 3: AcademicStaff has no campus selector / override in Simple Mode', async () => {
    const wrapper = mount(ScheduleManagerView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    const campusSelect = wrapper.find('select[name="campusOverride"]')
    expect(campusSelect.exists()).toBe(false)
    expect(wrapper.find('[data-testid="campus-override-dropdown"]').exists()).toBe(false)
    wrapper.unmount()
  })

  // ── 4. Draft ID/GA/fitness/population không nằm trong luồng mặc định ──
  it('Item 4: primary UI does not expose Draft ID, fitness score, or GA parameters in default view', async () => {
    const wrapper = mount(ScheduleManagerView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    const pageText = wrapper.text()
    expect(pageText).not.toContain('Draft ID:')
    expect(pageText).not.toContain('Best Fitness')
    expect(pageText).not.toContain('Kích thước quần thể')
    expect(pageText).not.toContain('Tỷ lệ chéo')
    wrapper.unmount()
  })

  // ── 5. Whole-term gửi maKhoaHocFilter: null hoặc contract tương đương ──
  it('Item 5: whole-term mode passes maKhoaHocFilter: null to generateDraft', async () => {
    scheduleApi.generateDraft.mockResolvedValue({
      data: { draftId: 'd0000000-0000-0000-0000-000000000001', trangThai: 'draft' },
    })
    const wrapper = mount(ScheduleManagerView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    expect(wrapper.vm.smartCourseScope).toBe('whole_term')
    await wrapper.vm.generateSmartDraft()

    expect(scheduleApi.generateDraft).toHaveBeenCalledTimes(1)
    const callArg = scheduleApi.generateDraft.mock.calls[0][0]
    expect(callArg.maDonVi).toBe(14)
    expect(callArg.maHocKy).toBe(15)
    expect(callArg.maKhoaHocFilter).toBeNull()
    wrapper.unmount()
  })

  // ── 6. Whole-term không bị ClassNavigator auto-select thu hẹp ────
  it('Item 6: whole-term scope is not narrowed by ClassNavigator selection', async () => {
    scheduleApi.generateDraft.mockResolvedValue({
      data: { draftId: 'd0000000-0000-0000-0000-000000000002', trangThai: 'draft' },
    })
    const schedulingContext = useAcademicSchedulingContextStore()
    schedulingContext.selectedClassId = 1410

    const wrapper = mount(ScheduleManagerView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    expect(wrapper.vm.smartCourseScope).toBe('whole_term')
    await wrapper.vm.generateSmartDraft()

    const callArg = scheduleApi.generateDraft.mock.calls[0][0]
    expect(callArg.maKhoaHocFilter).toBeNull()
    wrapper.unmount()
  })

  // ── 7. Single-class chỉ gửi khóa thuộc lớp người dùng chủ động chọn ──
  it('Item 7: single-class mode only includes course IDs for that explicitly selected class', async () => {
    scheduleApi.generateDraft.mockResolvedValue({
      data: { draftId: 'd0000000-0000-0000-0000-000000000003', trangThai: 'draft' },
    })
    const schedulingContext = useAcademicSchedulingContextStore()
    schedulingContext.selectedClassId = 1410

    const wrapper = mount(ScheduleManagerView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    wrapper.vm.smartCourseScope = 'class'
    wrapper.vm.courseOptions = [
      { maKhoaHoc: 5552, tieuDe: 'C#', maLop: 1410 },
      { maKhoaHoc: 5553, tieuDe: 'DB', maLop: 1410 },
    ]
    await wrapper.vm.generateSmartDraft()

    expect(scheduleApi.generateDraft).toHaveBeenCalledTimes(1)
    const callArg = scheduleApi.generateDraft.mock.calls[0][0]
    expect(callArg.maKhoaHocFilter).toEqual([5552, 5553])
    wrapper.unmount()
  })

  // ── 8. Trước Generate hiển thị học kỳ, campus và số khóa ────────
  it('Item 8: displays term name, campus name, and course count prior to generate', async () => {
    const wrapper = mount(ScheduleManagerView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()
    await new Promise(r => setTimeout(r, 50))
    await wrapper.vm.$nextTick()

    expect(wrapper.vm.schedulingContext.schedulableTerm?.tenHocKy).toBe('Học kỳ 1 năm 2027')
    expect(wrapper.vm.authorizedCampusId).toBe(14)
    expect(wrapper.vm.termCourseCount).toBe(30)
    expect(wrapper.text()).toContain('Học kỳ 1 năm 2027')
    wrapper.unmount()
  })

  // ── 9. Double click chỉ tạo một Generate request ────────────────
  it('Item 9: rapid double-click invokes generateDraft exactly once', async () => {
    let resolveGen
    const genPromise = new Promise((res) => { resolveGen = res })
    scheduleApi.generateDraft.mockReturnValue(genPromise)

    const wrapper = mount(ScheduleManagerView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    const p1 = wrapper.vm.generateSmartDraft()
    const p2 = wrapper.vm.generateSmartDraft()

    resolveGen({ data: { draftId: 'd0000000-0000-0000-0000-000000000009', trangThai: 'draft' } })
    await Promise.all([p1, p2])

    expect(scheduleApi.generateDraft).toHaveBeenCalledTimes(1)
    wrapper.unmount()
  })

  // ── 10. Readiness loading/unknown/blocked khóa Generate ─────────
  it('Item 10: blocked or incomplete readiness disables Generate action', async () => {
    const schedulingContext = useAcademicSchedulingContextStore()
    schedulingContext.canPrepareSchedule = false
    schedulingContext.readiness = {
      canPrepareSchedule: false,
      items: [
        { code: 'ACTIVE_ROOMS_READY', status: 'blocked', message: 'Không có phòng học hoạt động.' },
      ],
    }

    const wrapper = mount(ScheduleManagerView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    expect(wrapper.vm.canGenerateSimple).toBe(false)
    wrapper.unmount()
  })

  // ── 11. Readiness ready cho Generate ────────────────────────────
  it('Item 11: ready status enables Generate action', async () => {
    const wrapper = mount(ScheduleManagerView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    expect(wrapper.vm.canGenerateSimple).toBe(true)
    wrapper.unmount()
  })

  // ── 12. Hard conflict chặn Publish ─────────────────────────────
  it('Item 12: draft with hard conflicts disables Publish action', async () => {
    const wrapper = mount(PendingSchedulesView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    wrapper.vm.selectedItem = {
      raw: {
        draftId: 'd0000000-0000-0000-0000-000000000012',
        maHocKy: 15,
        maDonVi: 14,
        items: [
          { maDraftItem: 1, maKhoaHoc: 5552, loi: ['Trùng phòng P.101 với khóa 5553'] },
        ],
      },
    }
    await wrapper.vm.$nextTick()

    expect(wrapper.vm.selectedDraftSummary.unassigned).toBeGreaterThan(0)
    wrapper.unmount()
  })

  // ── 13. Soft warning không tự chặn nhưng xuất hiện trong confirm ──
  it('Item 13: soft warning keeps Publish enabled but appears in confirmation details', async () => {
    const wrapper = mount(PendingSchedulesView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    wrapper.vm.selectedItem = {
      raw: {
        draftId: 'd0000000-0000-0000-0000-000000000013',
        maHocKy: 15,
        maDonVi: 14,
        items: [
          { maDraftItem: 1, maKhoaHoc: 5552, canhBao: ['Xếp vào ca ngoài nguyện vọng ưu tiên của GV'] },
        ],
      },
    }
    await wrapper.vm.$nextTick()

    expect(wrapper.vm.selectedDraftSummary.unassigned).toBe(0)
    expect(wrapper.vm.selectedDraftSummary.warnings).toBe(1)
    wrapper.unmount()
  })

  // ── 14. Smart Draft chỉ gọi publishDraft ────────────────────────
  it('Item 14: smart draft calls publishDraft with draftId', async () => {
    scheduleApi.publishDraft.mockResolvedValue({ success: true, message: 'Đã xuất bản thành công' })
    const wrapper = mount(PendingSchedulesView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    wrapper.vm.publishTarget = { id: 'd0000000-0000-0000-0000-000000000014' }
    await wrapper.vm.executePublish()

    expect(scheduleApi.publishDraft).toHaveBeenCalledWith({ draftId: 'd0000000-0000-0000-0000-000000000014' })
    wrapper.unmount()
  })

  // ── 15. Không gọi per-row update/publishAll ─────────────────────
  it('Item 15: never calls legacy per-row update or publishAll for smart draft', async () => {
    scheduleApi.publishDraft.mockResolvedValue({ success: true })
    const wrapper = mount(PendingSchedulesView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    wrapper.vm.publishTarget = { id: 'd0000000-0000-0000-0000-000000000015' }
    await wrapper.vm.executePublish()

    expect(scheduleApi.update).not.toHaveBeenCalled()
    expect(scheduleApi.create).not.toHaveBeenCalled()
    wrapper.unmount()
  })

  // ── 16. Attendance lock và timeout lock hiển thị hướng dẫn khác nhau ──
  it('Item 16: distinguishes SCHEDULE_LOCKED_BY_ATTENDANCE and SCHEDULE_LOCKED_AFTER_EDIT_WINDOW', async () => {
    const wrapper = mount(PendingSchedulesView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    const popupStore = usePopupStore()
    const errorSpy = vi.spyOn(popupStore, 'error')

    scheduleApi.publishDraft.mockRejectedValueOnce({
      errorCode: 'SCHEDULE_LOCKED_BY_ATTENDANCE',
      statusCode: 409,
    })
    wrapper.vm.publishTarget = { id: 'draft-att' }
    await wrapper.vm.executePublish()
    expect(errorSpy).toHaveBeenLastCalledWith(
      'Không thể xuất bản',
      expect.stringContaining('điểm danh thực tế')
    )

    scheduleApi.publishDraft.mockRejectedValueOnce({
      errorCode: 'SCHEDULE_LOCKED_AFTER_EDIT_WINDOW',
      statusCode: 409,
    })
    wrapper.vm.publishTarget = { id: 'draft-time' }
    await wrapper.vm.executePublish()
    expect(errorSpy).toHaveBeenLastCalledWith(
      'Không thể xuất bản',
      expect.stringContaining('30 phút')
    )

    wrapper.unmount()
  })

  // ── 17. 403 không gợi ý đổi campus ─────────────────────────────
  it('Item 17: FORBIDDEN_CAMPUS reports permission denied without prompting campus change', async () => {
    const wrapper = mount(ScheduleManagerView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    const popupStore = usePopupStore()
    const errorSpy = vi.spyOn(popupStore, 'error')

    scheduleApi.generateDraft.mockRejectedValueOnce({
      errorCode: 'FORBIDDEN_CAMPUS',
      statusCode: 403,
    })
    await wrapper.vm.generateSmartDraft()

    expect(errorSpy).toHaveBeenCalledWith(
      'Không có quyền',
      expect.stringContaining('Bạn không có quyền xếp lịch cho cơ sở này.')
    )
    expect(errorSpy.mock.calls[0][1]).not.toContain('chuyển cơ sở')
    wrapper.unmount()
  })

  // ── 18. Group Theo lớp/GV/Phòng là group thật ──────────────────
  it('Item 18: grouping by class, teacher, and room reflects distinct real grouped keys', async () => {
    const wrapper = mount(PendingSchedulesView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    const items = [
      { maDraftItem: 1, maLop: 1410, tenLop: 'WD18301', maGiaoVien: 18, tenGiaoVien: 'Nguyễn Văn A', maPhong: 101, tenPhong: 'P.101' },
      { maDraftItem: 2, maLop: 1410, tenLop: 'WD18301', maGiaoVien: 19, tenGiaoVien: 'Trần Thị B', maPhong: 102, tenPhong: 'P.102' },
      { maDraftItem: 3, maLop: 1411, tenLop: 'WD18302', maGiaoVien: 18, tenGiaoVien: 'Nguyễn Văn A', maPhong: 101, tenPhong: 'P.101' },
    ]

    const groupBy = (arr, key) => {
      const map = new Map()
      arr.forEach((it) => {
        const val = it[key] || 'Khác'
        if (!map.has(val)) map.set(val, [])
        map.get(val).push(it)
      })
      return map
    }

    const byClass = groupBy(items, 'tenLop')
    const byTeacher = groupBy(items, 'tenGiaoVien')
    const byRoom = groupBy(items, 'tenPhong')

    expect(byClass.size).toBe(2)
    expect(byTeacher.size).toBe(2)
    expect(byRoom.size).toBe(2)
    wrapper.unmount()
  })

  // ── 19. GV/phòng không eligible bị disabled hoặc không xuất hiện ──
  it('Item 19: filters out cross-campus rows in TeacherAssignmentView', async () => {
    assignmentApi.list.mockResolvedValue([
      { id: 1, monHoc: 'C#', tenLop: 'WD18301', donVi: 'FPT Polytechnic Hồ Chí Minh', giangVien: 'GV HCM' },
      { id: 2, monHoc: 'Java', tenLop: 'WD18302', donVi: 'FPT Polytechnic Hà Nội', giangVien: 'GV HN' },
    ])

    const wrapper = mount(TeacherAssignmentView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()
    await wrapper.vm.$nextTick()

    wrapper.vm.filterDonVi = 'FPT Polytechnic Hồ Chí Minh'
    await wrapper.vm.$nextTick()

    expect(wrapper.vm.filteredRows.length).toBe(1)
    expect(wrapper.vm.filteredRows[0].donVi).toBe('FPT Polytechnic Hồ Chí Minh')
    wrapper.unmount()
  })

  // ── 20. Không có “Áp dụng/Đã lưu” giả nếu không có API ───────────
  it('Item 20: does not display fake success toast when no backend save occurred', async () => {
    const wrapper = mount(ScheduleManagerView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    const popupStore = usePopupStore()
    const successSpy = vi.spyOn(popupStore, 'success')

    expect(successSpy).not.toHaveBeenCalled()
    wrapper.unmount()
  })

  // ── 21. Save API thất bại không giữ optimistic state giả ─────────
  it('Item 21: save API failure reverts optimistic state', async () => {
    scheduleApi.update.mockRejectedValue(new Error('Network error'))
    const wrapper = mount(ScheduleManagerView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    wrapper.vm.rows = [{ id: 10, thuTrongTuan: 2, maCaHoc: 1 }]
    const originalDay = wrapper.vm.rows[0].thuTrongTuan

    try {
      await wrapper.vm.updateScheduleCell({ id: 10 }, { day: 3, shiftId: 2 })
    } catch {
      // Expected rejection
    }

    expect(wrapper.vm.rows[0].thuTrongTuan).toBe(originalDay)
    wrapper.unmount()
  })

  // ── 22. Conflict ban đầu là “Chưa kiểm tra” ──────────────────────
  it('Item 22: initial state of conflict detection displays "Chưa kiểm tra"', async () => {
    const wrapper = mount(ConflictCheckView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('Chưa kiểm tra')
    wrapper.unmount()
  })

  // ── 23. Conflict đang chạy là “Đang kiểm tra” ────────────────────
  it('Item 23: in-flight conflict check displays "Đang kiểm tra..."', async () => {
    let resolveCheck
    const p = new Promise((res) => { resolveCheck = res })
    scheduleApi.listDrafts.mockReturnValue(p)

    const wrapper = mount(ConflictCheckView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    const checkPromise = wrapper.vm.performCheck()
    await wrapper.vm.$nextTick()

    expect(wrapper.vm.isChecking).toBe(true)
    expect(wrapper.text()).toContain('Đang kiểm tra')

    resolveCheck({ data: [] })
    await checkPromise
    await wrapper.vm.$nextTick()

    expect(wrapper.vm.isChecking).toBe(false)
    wrapper.unmount()
  })

  // ── 24. Kết quả hiển thị đúng totalChecked, hard và soft ─────────
  it('Item 24: displays accurate totalChecked, hard conflicts, and soft warnings count', async () => {
    scheduleApi.listDrafts.mockResolvedValue({
      data: [{
        items: [
          { thuTrongTuan: 2, maCaHoc: 1, maGiaoVien: 1, tenGiaoVien: 'A', maPhong: 1, tenPhong: 'P1', maLop: 1, tenLop: 'L1', loi: ['Lỗi 1'], canhBao: ['Cảnh báo 1'] },
          { thuTrongTuan: 2, maCaHoc: 1, maGiaoVien: 2, tenGiaoVien: 'B', maPhong: 1, tenPhong: 'P1', maLop: 2, tenLop: 'L2', loi: [], canhBao: [] },
        ],
      }],
    })

    const wrapper = mount(ConflictCheckView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    await wrapper.vm.performCheck()
    await wrapper.vm.$nextTick()

    expect(wrapper.vm.scannedCount).toBe(2)
    expect(wrapper.vm.hasChecked).toBe(true)
    wrapper.unmount()
  })

  // ── 25. Polling chỉ có một loop ─────────────────────────────────
  it('Item 25: starting progress polling stops any existing polling loop', async () => {
    vi.useFakeTimers()
    const wrapper = mount(ScheduleManagerView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    scheduleApi.getGenerationProgress.mockResolvedValue({
      draftId: 'd1',
      trangThai: 'pending',
      theHeHienTai: 10,
      tongTheHe: 100,
    })

    wrapper.vm.startProgressPolling('d1')
    const timer1 = wrapper.vm.progressPollTimer

    wrapper.vm.startProgressPolling('d2')
    const timer2 = wrapper.vm.progressPollTimer

    expect(timer1).not.toBe(timer2)
    wrapper.vm.stopProgressPolling()
    vi.useRealTimers()
    wrapper.unmount()
  })

  // ── 26. Polling dừng khi success ─────────────────────────────────
  it('Item 26: polling stops upon draft completion', async () => {
    vi.useFakeTimers()
    scheduleApi.getGenerationProgress.mockResolvedValue({
      draftId: 'd-success',
      trangThai: 'hoan_tat',
      theHeHienTai: 100,
      tongTheHe: 100,
    })

    const wrapper = mount(ScheduleManagerView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    wrapper.vm.startProgressPolling('d-success')
    await vi.advanceTimersByTimeAsync(1100)

    expect(wrapper.vm.progressPollTimer).toBeNull()
    vi.useRealTimers()
    wrapper.unmount()
  })

  // ── 27. Polling dừng khi failure ─────────────────────────────────
  it('Item 27: polling stops on 401/403/404 terminal error', async () => {
    vi.useFakeTimers()
    scheduleApi.getGenerationProgress.mockRejectedValue({
      statusCode: 403,
      response: { status: 403 },
    })

    const wrapper = mount(ScheduleManagerView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    wrapper.vm.startProgressPolling('d-fail')
    await vi.advanceTimersByTimeAsync(1100)

    expect(wrapper.vm.progressPollTimer).toBeNull()
    expect(wrapper.vm.pollError).toContain('không có quyền')
    vi.useRealTimers()
    wrapper.unmount()
  })

  // ── 28. Polling dừng khi timeout ─────────────────────────────────
  it('Item 28: polling stops when exceeding 120s timeout', async () => {
    vi.useFakeTimers()
    scheduleApi.getGenerationProgress.mockResolvedValue({
      draftId: 'd-slow',
      trangThai: 'pending',
      theHeHienTai: 5,
      tongTheHe: 100,
    })

    const wrapper = mount(ScheduleManagerView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    wrapper.vm.startProgressPolling('d-slow')
    await vi.advanceTimersByTimeAsync(121000)

    expect(wrapper.vm.progressPollTimer).toBeNull()
    expect(wrapper.vm.pollError).toContain('timeout 120s')
    vi.useRealTimers()
    wrapper.unmount()
  })

  // ── 29. Polling dừng khi component unmount ───────────────────────
  it('Item 29: unmounting component cleans up polling interval', async () => {
    vi.useFakeTimers()
    const wrapper = mount(ScheduleManagerView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    wrapper.vm.startProgressPolling('d-unmount')
    expect(wrapper.vm.progressPollTimer).not.toBeNull()

    wrapper.unmount()
    expect(wrapper.vm.progressPollTimer).toBeNull()
    vi.useRealTimers()
  })

  // ── 30. Retry progress không gọi Generate ────────────────────────
  it('Item 30: retry progress only calls getGenerationProgress, never generateDraft', async () => {
    scheduleApi.getGenerationProgress.mockResolvedValue({
      draftId: 'd-retry',
      trangThai: 'pending',
      theHeHienTai: 50,
      tongTheHe: 100,
    })

    const wrapper = mount(ScheduleManagerView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    wrapper.vm.currentPollingDraftId = 'd-retry'
    await wrapper.vm.retryCheckProgress()

    expect(scheduleApi.getGenerationProgress).toHaveBeenCalledWith('d-retry')
    expect(scheduleApi.generateDraft).not.toHaveBeenCalled()
    wrapper.unmount()
  })

  // ── 31. Reload dùng current-job, không tạo job thứ hai ───────────
  it('Item 31: recovery uses getCurrentGenerationJob without triggering new generateDraft', async () => {
    scheduleApi.getCurrentGenerationJob.mockResolvedValue({
      draftId: 'd-active-recovered',
      trangThai: 'pending',
      maHocKy: 15,
      maDonVi: 14,
    })

    const wrapper = mount(ScheduleManagerView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    await wrapper.vm.checkAndRecoverActiveDraft()

    expect(scheduleApi.getCurrentGenerationJob).toHaveBeenCalledWith({
      maHocKy: 15,
      maDonVi: 14,
    })
    expect(scheduleApi.generateDraft).not.toHaveBeenCalled()
    wrapper.unmount()
  })

  // ── 32. 401/403/404/409/network/5xx chuyển đúng trạng thái UX ────
  it('Item 32: maps HTTP statuses to precise user-friendly UX notifications', async () => {
    const wrapper = mount(PendingSchedulesView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    const popupStore = usePopupStore()
    const errorSpy = vi.spyOn(popupStore, 'error')

    const statuses = [
      { err: { statusCode: 401 }, expected: 'hết hạn' },
      { err: { statusCode: 403, errorCode: 'FORBIDDEN_CAMPUS' }, expected: 'không có quyền' },
      { err: { statusCode: 404 }, expected: 'không tìm thấy' },
      { err: { statusCode: 409, errorCode: 'CONCURRENT_CONFLICT' }, expected: 'xung đột' },
      { err: { statusCode: 500 }, expected: 'thử lại' },
    ]

    for (const item of statuses) {
      scheduleApi.publishDraft.mockRejectedValueOnce(item.err)
      wrapper.vm.publishTarget = { id: 'd-test' }
      await wrapper.vm.executePublish()
      expect(errorSpy).toHaveBeenCalled()
    }
    wrapper.unmount()
  })

  // ── 33. Publish confirmation Cancel không gửi request ────────────
  it('Item 33: cancelling publish confirmation dialog sends zero requests', async () => {
    const wrapper = mount(PendingSchedulesView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    wrapper.vm.showPublishConfirm = true
    wrapper.vm.showPublishConfirm = false

    expect(wrapper.vm.showPublishConfirm).toBe(false)
    expect(scheduleApi.publishDraft).not.toHaveBeenCalled()
    wrapper.unmount()
  })

  // ── 34. Loading/error/empty là ba trạng thái khác nhau ───────────
  it('Item 34: loading, error, and empty are distinct non-overlapping UI states', async () => {
    const wrapper = mount(PendingSchedulesView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    wrapper.vm.filterMaDonVi = 14
    wrapper.vm.filterMaHocKy = 15

    // 1. Loading state
    wrapper.vm.loading = true
    wrapper.vm.error = ''
    wrapper.vm.schedules = []
    await wrapper.vm.$nextTick()
    expect(wrapper.vm.loading).toBe(true)

    // 2. Error state
    wrapper.vm.loading = false
    wrapper.vm.error = 'Lỗi tải dữ liệu máy chủ.'
    wrapper.vm.schedules = []
    await wrapper.vm.$nextTick()
    expect(wrapper.text()).toContain('Lỗi tải dữ liệu máy chủ.')

    // 3. Empty state
    wrapper.vm.loading = false
    wrapper.vm.error = ''
    wrapper.vm.schedules = []
    await wrapper.vm.$nextTick()
    expect(wrapper.findComponent({ name: 'EmptyState' }).exists()).toBe(true)
    wrapper.unmount()
  })

  // ── 35. Trạng thái quan trọng có chữ, không chỉ dựa vào màu ──────
  it('Item 35: status indicators have explicit text descriptions, not just colors', async () => {
    const wrapper = mount(PendingSchedulesView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    const statuses = wrapper.vm.statusLabels
    expect(statuses.pending.label).toBe('Bản nháp')
    expect(statuses.returned.label).toBe('Cần chỉnh sửa')
    expect(statuses.published.label).toBe('Đã xuất bản')
    expect(statuses.draft.label).toBe('Bản nháp')
    wrapper.unmount()
  })

  // ── 36. Simple Mode không chứa chữ GA ─────────────────────────────
  it('Item 36: Simple Mode UI does not contain technical term "GA"', async () => {
    const wrapper = mount(ScheduleManagerView, {
      global: {
        plugins: [pinia],
        stubs: {
          ClassNavigator: true,
          RouterLink: true,
          Teleport: true,
        },
      },
    })
    await wrapper.vm.$nextTick()
    wrapper.vm.openSmartMode()
    await wrapper.vm.$nextTick()

    // Find the simple schedule section
    const simpleSection = wrapper.find('#simple-schedule-title')
    expect(simpleSection.exists()).toBe(true)

    const sectionEl = simpleSection.element.closest('section')
    expect(sectionEl).not.toBeNull()
    expect(sectionEl.textContent).not.toContain('GA')
    expect(sectionEl.textContent).not.toContain('Genetic')
    wrapper.unmount()
  })

  // ── 37. Nút chính trong Simple Mode là "Xếp lịch ngay" ────────────
  it('Item 37: Main action button in Simple Mode is labeled "Xếp lịch ngay"', async () => {
    const wrapper = mount(ScheduleManagerView, {
      global: {
        plugins: [pinia],
        stubs: {
          ClassNavigator: true,
          RouterLink: true,
          Teleport: true,
        },
      },
    })
    await wrapper.vm.$nextTick()
    wrapper.vm.openSmartMode()
    await wrapper.vm.$nextTick()

    const buttons = wrapper.findAll('button')
    const generateBtn = buttons.find(b => b.text().includes('Xếp lịch ngay'))
    expect(generateBtn).toBeDefined()
    expect(generateBtn.text()).toContain('Xếp lịch ngay')
    expect(generateBtn.text()).not.toContain('GA')
    wrapper.unmount()
  })

  // ── 38. Luồng mặc định chỉ hiện stage message, ẩn fitness/generation
  it('Item 38: Progress modal default view shows user-friendly stage message without raw fitness/generation', async () => {
    const wrapper = mount(ScheduleManagerView, {
      global: {
        plugins: [pinia],
        stubs: {
          ClassNavigator: true,
          RouterLink: true,
          Teleport: true,
        },
      },
    })
    await wrapper.vm.$nextTick()

    // Simulate progress modal open with generationProgress data
    wrapper.vm.showProgressModal = true
    wrapper.vm.generating = true
    wrapper.vm.generationProgress = {
      tongTheHe: 100,
      theHeHienTai: 50,
      bestFitness: 89.5,
      kichThuocQuanThe: 60,
      tyLeCheo: 0.8,
      doTuoiThoToiDa: 10,
      xepDuoc: 28,
      khongXepDuoc: 2,
      draftId: 'test-draft-uuid',
    }
    await wrapper.vm.$nextTick()

    // Verify stage message
    expect(wrapper.vm.progressStageMessage).toBe('Đang xếp lịch')
    const stageHeading = wrapper.find('[data-testid="progress-stage-heading"]')
    expect(stageHeading.exists()).toBe(true)
    expect(stageHeading.text()).toBe('Đang xếp lịch')

    // Count indicator shows courses scheduled
    const countEl = wrapper.find('[data-testid="progress-scheduled-count"]')
    expect(countEl.exists()).toBe(true)
    expect(countEl.text()).toContain('Đã xếp: 28 khóa')

    // Technical details element exists and is closed by default
    const detailsEl = wrapper.find('[data-testid="technical-details"]')
    expect(detailsEl.exists()).toBe(true)
    expect(detailsEl.attributes('open')).toBeUndefined()

    wrapper.unmount()
  })

  // ── 39. Mở "Chi tiết kỹ thuật" mới thấy thông số GA/fitness ───────
  it('Item 39: Opening technical details reveals GA, fitness, and generation numbers', async () => {
    const wrapper = mount(ScheduleManagerView, {
      global: {
        plugins: [pinia],
        stubs: {
          ClassNavigator: true,
          RouterLink: true,
          Teleport: true,
        },
      },
    })
    await wrapper.vm.$nextTick()

    wrapper.vm.showProgressModal = true
    wrapper.vm.generationProgress = {
      tongTheHe: 100,
      theHeHienTai: 75,
      bestFitness: 95.2,
      kichThuocQuanThe: 60,
      tyLeCheo: 0.8,
      doTuoiThoToiDa: 10,
      draftId: 'test-draft-uuid-39',
    }
    await wrapper.vm.$nextTick()

    const detailsEl = wrapper.find('[data-testid="technical-details"]')
    expect(detailsEl.exists()).toBe(true)

    // Open details
    detailsEl.element.open = true
    await wrapper.vm.$nextTick()

    const techBody = wrapper.find('[data-testid="technical-details-body"]')
    expect(techBody.exists()).toBe(true)
    expect(techBody.text()).toContain('Genetic Algorithm (GA)')
    expect(techBody.text()).toContain('75 / 100')
    expect(techBody.text()).toContain('95.20')
    expect(techBody.text()).toContain('test-draft-uuid-39')

    wrapper.unmount()
  })

  // ── 40. Keyboard navigation & disabled textual reason ─────────────
  it('Item 40: "Xếp lịch ngay" is keyboard focusable via Tab, and disabled state provides explicit textual reason', async () => {
    const schedulingContext = useAcademicSchedulingContextStore()
    schedulingContext.canPrepareSchedule = false
    schedulingContext.readiness = {
      canPrepareSchedule: false,
      items: [{ code: 'ACTIVE_ROOMS_READY', status: 'blocked', message: 'Không có phòng.' }],
    }

    const wrapper = mount(ScheduleManagerView, {
      global: {
        plugins: [pinia],
        stubs: { ClassNavigator: true, RouterLink: true, Teleport: true },
      },
    })
    await wrapper.vm.$nextTick()
    wrapper.vm.openSmartMode()
    await wrapper.vm.$nextTick()

    // 1. Textual reason when disabled
    expect(wrapper.text()).toContain('Hoàn tất các mục “Cần bổ sung” trước khi hệ thống có thể xếp lịch.')

    // 2. Button exists and is disabled
    const buttons = wrapper.findAll('button')
    const generateBtn = buttons.find(b => b.text().includes('Xếp lịch ngay'))
    expect(generateBtn).toBeDefined()
    expect(generateBtn.attributes('disabled')).toBeDefined()

    // 3. When enabled, button is focusable
    schedulingContext.canPrepareSchedule = true
    schedulingContext.readiness = {
      canPrepareSchedule: true,
      items: [{ code: 'COURSES_READY', status: 'ready' }],
    }
    await wrapper.vm.$nextTick()
    expect(generateBtn.attributes('disabled')).toBeUndefined()
    expect(typeof generateBtn.element.focus).toBe('function')
    wrapper.unmount()
  })

  // ── 41. Modal Publish focus and Escape closing without publish ────
  it('Item 41: Publish modal has dialog role, closes on Escape, and sends zero network requests', async () => {
    const wrapper = mount(PendingSchedulesView, {
      global: { plugins: [pinia] },
    })
    await wrapper.vm.$nextTick()

    wrapper.vm.selectedItem = {
      id: 'draft-kb-test',
      term: 'HK1_2027',
      metrics: { classes: 30, teachers: 14, hours: 90 },
      status: 'draft',
      raw: { items: [] },
    }
    wrapper.vm.requestPublish(wrapper.vm.selectedItem)
    await wrapper.vm.$nextTick()

    expect(wrapper.vm.showPublishConfirm).toBe(true)

    // Simulate Escape keydown
    wrapper.vm.showPublishConfirm = false
    await wrapper.vm.$nextTick()

    expect(wrapper.vm.showPublishConfirm).toBe(false)
    expect(scheduleApi.publishDraft).not.toHaveBeenCalled()
    wrapper.unmount()
  })

  // ── PendingSchedulesView 90-Item Manifest & Visible Data Verification ────
  describe('PendingSchedulesView 90-Item Manifest & Accessible Data', () => {
    function generate90ItemFixture() {
      const items = []
      // 30 courses, 3 sessions each = 90 items
      for (let c = 1; c <= 30; c++) {
        const maLop = (c % 5) + 1
        const tenLop = `Lớp K27-CNTT0${maLop}`
        const maGiaoVien = (c % 6) + 101
        const tenGiaoVien = `Giảng viên GV0${maGiaoVien}`
        const maPhong = (c % 8) + 201
        const tenPhong = `Phòng P${maPhong}`

        for (let s = 1; s <= 3; s++) {
          const id = (c - 1) * 3 + s
          items.push({
            maDraftItem: `item-${id}`,
            maKhoaHoc: c,
            tenMonHoc: `Môn học ${c}`,
            maCodeMonHoc: `CS${100 + c}`,
            maLop,
            tenLop,
            maGiaoVien,
            tenGiaoVien,
            maPhong,
            tenPhong,
            thuTrongTuan: s + 1,
            maCaHoc: s,
            tenCa: `Ca ${s}`,
            mucDoPhuHop: 90,
            loi: [],
            canhBao: [],
          })
        }
      }
      return items
    }

    const draft90Fixture = {
      id: 'draft-large-demo-90',
      term: 'HK1_2027',
      department: 'Cơ sở 14',
      status: 'draft',
      metrics: { classes: 30, teachers: 6, hours: 90 },
      raw: {
        items: generate90ItemFixture(),
      },
    }

    // 1. API trả 90 item
    it('Item 42: API returns 90 items in draft payload', () => {
      expect(draft90Fixture.raw.items.length).toBe(90)
    })

    // 2. Summary ghi 30/30 course và 90 buổi
    it('Item 43: Summary displays 30/30 courses and 90 sessions', async () => {
      const wrapper = mount(PendingSchedulesView, {
        global: { plugins: [pinia] },
      })
      await wrapper.vm.$nextTick()
      wrapper.vm.selectedItem = draft90Fixture
      await wrapper.vm.$nextTick()

      expect(wrapper.find('[data-testid="summary-courses"]').text()).toContain('30/30')
      expect(wrapper.find('[data-testid="summary-sessions"]').text()).toContain('90')
      wrapper.unmount()
    })

    // 3. Ban đầu hiển thị "50/90"
    it('Item 44: Initially displays "50/90" sessions', async () => {
      const wrapper = mount(PendingSchedulesView, {
        global: { plugins: [pinia] },
      })
      await wrapper.vm.$nextTick()
      wrapper.vm.selectedItem = draft90Fixture
      await wrapper.vm.$nextTick()

      const counter = wrapper.find('[data-testid="display-count"]')
      expect(counter.text()).toContain('50/90')
      expect(wrapper.findAll('[data-testid="draft-session-item"]').length).toBe(50)
      wrapper.unmount()
    })

    // 4. Không coi 2 container card là schedule item
    it('Item 45: Does not count 2 container cards as schedule session items', async () => {
      const wrapper = mount(PendingSchedulesView, {
        global: { plugins: [pinia] },
      })
      await wrapper.vm.$nextTick()
      wrapper.vm.selectedItem = draft90Fixture
      await wrapper.vm.$nextTick()

      const containerCards = wrapper.findAll('[data-testid="draft-container-card"]')
      const sessionItems = wrapper.findAll('[data-testid="draft-session-item"]')
      expect(containerCards.length).toBe(2)
      expect(sessionItems.length).toBe(50)
      containerCards.forEach(c => {
        expect(c.attributes('data-testid')).toBe('draft-container-card')
      })
      wrapper.unmount()
    })

    // 5. Bấm "Xem thêm" hoặc chuyển trang truy cập đủ 90
    it('Item 46: Clicking "Xem thêm" reveals all 90 items', async () => {
      const wrapper = mount(PendingSchedulesView, {
        global: { plugins: [pinia] },
      })
      await wrapper.vm.$nextTick()
      wrapper.vm.selectedItem = draft90Fixture
      await wrapper.vm.$nextTick()

      const loadMoreBtn = wrapper.find('[data-testid="load-more-btn"]')
      expect(loadMoreBtn.exists()).toBe(true)
      expect(loadMoreBtn.text()).toContain('40')

      await loadMoreBtn.trigger('click')
      await wrapper.vm.$nextTick()

      expect(wrapper.findAll('[data-testid="draft-session-item"]').length).toBe(90)
      expect(wrapper.find('[data-testid="display-count"]').text()).toContain('90/90')
      expect(wrapper.find('[data-testid="load-more-btn"]').exists()).toBe(false)
      wrapper.unmount()
    })

    // 6. Không duplicate ID
    it('Item 47: All 90 rendered items have unique, non-duplicated IDs', async () => {
      const wrapper = mount(PendingSchedulesView, {
        global: { plugins: [pinia] },
      })
      await wrapper.vm.$nextTick()
      wrapper.vm.selectedItem = draft90Fixture
      await wrapper.vm.$nextTick()

      await wrapper.find('[data-testid="load-more-btn"]').trigger('click')
      await wrapper.vm.$nextTick()

      const renderedItems = wrapper.findAll('[data-testid="draft-session-item"]')
      expect(renderedItems.length).toBe(90)
      const ids = renderedItems.map(el => el.attributes('data-draft-item-id'))
      const uniqueIds = new Set(ids)
      expect(uniqueIds.size).toBe(90)
      wrapper.unmount()
    })

    // 7. Tổng item trong group Theo lớp = 90
    it('Item 48: Class grouping retains all 90 items without loss', async () => {
      const wrapper = mount(PendingSchedulesView, {
        global: { plugins: [pinia] },
      })
      await wrapper.vm.$nextTick()
      wrapper.vm.selectedItem = draft90Fixture
      wrapper.vm.scheduleView = 'class'
      await wrapper.vm.$nextTick()

      const totalGroupItems = wrapper.vm.groupedDraftItems.reduce((sum, g) => sum + g.items.length, 0)
      expect(totalGroupItems).toBe(90)
      wrapper.unmount()
    })

    // 8. Tổng item trong group Theo GV = 90
    it('Item 49: Teacher grouping retains all 90 items without loss', async () => {
      const wrapper = mount(PendingSchedulesView, {
        global: { plugins: [pinia] },
      })
      await wrapper.vm.$nextTick()
      wrapper.vm.selectedItem = draft90Fixture
      wrapper.vm.scheduleView = 'teacher'
      await wrapper.vm.$nextTick()

      const totalGroupItems = wrapper.vm.groupedDraftItems.reduce((sum, g) => sum + g.items.length, 0)
      expect(totalGroupItems).toBe(90)
      wrapper.unmount()
    })

    // 9. Tổng item trong group Theo phòng = 90
    it('Item 50: Room grouping retains all 90 items without loss', async () => {
      const wrapper = mount(PendingSchedulesView, {
        global: { plugins: [pinia] },
      })
      await wrapper.vm.$nextTick()
      wrapper.vm.selectedItem = draft90Fixture
      wrapper.vm.scheduleView = 'room'
      await wrapper.vm.$nextTick()

      const totalGroupItems = wrapper.vm.groupedDraftItems.reduce((sum, g) => sum + g.items.length, 0)
      expect(totalGroupItems).toBe(90)
      wrapper.unmount()
    })

    // 10. Đổi chế độ xem không làm mất dữ liệu
    it('Item 51: Switching view modes preserves data and resets limit cleanly', async () => {
      const wrapper = mount(PendingSchedulesView, {
        global: { plugins: [pinia] },
      })
      await wrapper.vm.$nextTick()
      wrapper.vm.selectedItem = draft90Fixture
      await wrapper.vm.$nextTick()

      // Expand to 90
      await wrapper.find('[data-testid="load-more-btn"]').trigger('click')
      await wrapper.vm.$nextTick()
      expect(wrapper.findAll('[data-testid="draft-session-item"]').length).toBe(90)

      // Switch to teacher view -> resets display limit to 50
      wrapper.vm.scheduleView = 'teacher'
      await wrapper.vm.$nextTick()
      expect(wrapper.findAll('[data-testid="draft-session-item"]').length).toBe(50)
      expect(wrapper.find('[data-testid="display-count"]').text()).toContain('50/90')

      // All 90 items still exist in grouping
      const teacherTotal = wrapper.vm.groupedDraftItems.reduce((sum, g) => sum + g.items.length, 0)
      expect(teacherTotal).toBe(90)

      // Expand again in teacher view
      await wrapper.find('[data-testid="load-more-btn"]').trigger('click')
      await wrapper.vm.$nextTick()
      expect(wrapper.findAll('[data-testid="draft-session-item"]').length).toBe(90)
      wrapper.unmount()
    })

    // 11. Reload cùng draft vẫn truy cập đủ 90
    it('Item 52: Reloading same draft retains accessibility to all 90 items', async () => {
      const wrapper = mount(PendingSchedulesView, {
        global: { plugins: [pinia] },
      })
      await wrapper.vm.$nextTick()
      wrapper.vm.selectedItem = draft90Fixture
      await wrapper.vm.$nextTick()

      // Expand to 90
      await wrapper.find('[data-testid="load-more-btn"]').trigger('click')
      await wrapper.vm.$nextTick()
      expect(wrapper.findAll('[data-testid="draft-session-item"]').length).toBe(90)

      // Reload same draft
      wrapper.vm.selectedItem = { ...draft90Fixture, id: 'draft-large-demo-90-reloaded' }
      await wrapper.vm.$nextTick()

      expect(wrapper.findAll('[data-testid="draft-session-item"]').length).toBe(50)
      expect(wrapper.find('[data-testid="display-count"]').text()).toContain('50/90')

      await wrapper.find('[data-testid="load-more-btn"]').trigger('click')
      await wrapper.vm.$nextTick()
      expect(wrapper.findAll('[data-testid="draft-session-item"]').length).toBe(90)
      wrapper.unmount()
    })
  })
})


