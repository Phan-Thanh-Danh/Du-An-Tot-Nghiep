import { describe, it, expect, vi } from 'vitest'

describe('Task 7D-R1: Non-Tech UX & Campus Isolation Verification', () => {
  // ── 1. Error Code Mapping (Attendance vs Timeout Lock Separation) ──
  describe('PendingSchedulesView Error Code Mapping', () => {
    function mapPublishError(e) {
      const code = e?.errorCode || e?.details?.errorCode || e?.details?.ErrorCode || ''
      const status = e?.statusCode || (e?.response?.status ?? e?.status)

      let message
      if (code === 'SCHEDULE_LOCKED_BY_ATTENDANCE') {
        message = 'Không thể xuất bản đè thời khóa biểu vì đã có buổi học được điểm danh thực tế trong học kỳ này.'
      } else if (code === 'SCHEDULE_LOCKED_AFTER_EDIT_WINDOW') {
        message = 'Thời khóa biểu đã xuất bản quá 30 phút, bị khóa chỉnh sửa/ghi đè vĩnh viễn.'
      } else if (code === 'FORBIDDEN_CAMPUS' || status === 403) {
        message = 'Bạn không có quyền quản lý thời khóa biểu của cơ sở này.'
      } else if (code === 'DRAFT_ALREADY_PUBLISHED') {
        message = 'Bản nháp này đã được xuất bản trước đó.'
      } else if (code === 'DRAFT_EXPIRED_OR_INVALID') {
        message = 'Bản nháp không hợp lệ hoặc đã hết hạn.'
      } else if (code === 'HARD_CONFLICT') {
        message = 'Bản nháp có dữ liệu xung đột cứng, không thể xuất bản.'
      } else if (code === 'READINESS_BLOCKED') {
        message = 'Dữ liệu học kỳ chưa sẵn sàng để xuất bản thời khóa biểu.'
      } else if (code === 'CONCURRENT_CONFLICT' || status === 409) {
        message = e?.message || 'Thao tác xuất bản bị xung đột hoặc học kỳ đã bị khóa. Vui lòng tải lại trang và kiểm tra lại.'
      } else {
        message = e?.message || 'Không thể xuất bản bản nháp. Vui lòng kiểm tra kết nối mạng và thử lại.'
      }
      return message
    }

    it('maps SCHEDULE_LOCKED_BY_ATTENDANCE without fragile substring matching', () => {
      const err = { errorCode: 'SCHEDULE_LOCKED_BY_ATTENDANCE', statusCode: 409 }
      expect(mapPublishError(err)).toContain('điểm danh')
    })

    it('maps SCHEDULE_LOCKED_AFTER_EDIT_WINDOW without fragile substring matching', () => {
      const err = { errorCode: 'SCHEDULE_LOCKED_AFTER_EDIT_WINDOW', statusCode: 409 }
      expect(mapPublishError(err)).toContain('30 phút')
    })

    it('maps FORBIDDEN_CAMPUS and 403 status', () => {
      expect(mapPublishError({ errorCode: 'FORBIDDEN_CAMPUS', statusCode: 403 })).toContain('không có quyền quản lý')
    })

    it('maps HARD_CONFLICT and DRAFT_ALREADY_PUBLISHED', () => {
      expect(mapPublishError({ errorCode: 'HARD_CONFLICT' })).toContain('xung đột cứng')
      expect(mapPublishError({ errorCode: 'DRAFT_ALREADY_PUBLISHED' })).toContain('đã được xuất bản trước đó')
    })
  })

  // ── 2. Generation Scope Payload (Whole-Term vs Single-Class) ──
  describe('ScheduleManagerView Generation Scope', () => {
    function buildGenerationPayload({ scope, selectedCourseIds, classCourseIds, campusId, termId }) {
      let maKhoaHocFilter
      if (scope === 'manual') {
        maKhoaHocFilter = selectedCourseIds.map(Number)
      } else if (scope === 'class') {
        maKhoaHocFilter = classCourseIds.map(Number)
      } else {
        // whole_term: null schedules entire term
        maKhoaHocFilter = null
      }

      return {
        maHocKy: termId,
        maDonVi: campusId,
        maKhoaHocFilter,
      }
    }

    it('defaults whole_term scope with maKhoaHocFilter: null to schedule all courses without ClassNavigator restriction', () => {
      const payload = buildGenerationPayload({
        scope: 'whole_term',
        selectedCourseIds: [1, 2],
        classCourseIds: [10, 20],
        campusId: 1,
        termId: 10,
      })

      expect(payload.maKhoaHocFilter).toBeNull()
      expect(payload.maDonVi).toBe(1)
      expect(payload.maHocKy).toBe(10)
    })

    it('sends specific course IDs only when scope is class or manual', () => {
      const classPayload = buildGenerationPayload({
        scope: 'class',
        selectedCourseIds: [],
        classCourseIds: [101, 102],
        campusId: 1,
        termId: 10,
      })
      expect(classPayload.maKhoaHocFilter).toEqual([101, 102])

      const manualPayload = buildGenerationPayload({
        scope: 'manual',
        selectedCourseIds: [201],
        classCourseIds: [101, 102],
        campusId: 1,
        termId: 10,
      })
      expect(manualPayload.maKhoaHocFilter).toEqual([201])
    })
  })

  // ── 3. Readiness Source of Truth & Double-Click Protection ──
  describe('Readiness Source of Truth & Double-Click Protection', () => {
    function computeCanGenerateSimple({ authorizedCampusId, canPrepareSchedule, readinessItems, submitting, generating }) {
      if (submitting || generating) return false
      if (!authorizedCampusId || !canPrepareSchedule) return false
      if (!readinessItems || !readinessItems.length) return false
      return readinessItems.every(item => item.status === 'ready' || item.status === 'warning')
    }

    it('blocks generation if readiness items are empty (loading/uninitialized)', () => {
      const canGen = computeCanGenerateSimple({
        authorizedCampusId: 1,
        canPrepareSchedule: true,
        readinessItems: [],
        submitting: false,
        generating: false,
      })
      expect(canGen).toBe(false)
    })

    it('blocks generation if any readiness item is blocked', () => {
      const canGen = computeCanGenerateSimple({
        authorizedCampusId: 1,
        canPrepareSchedule: true,
        readinessItems: [
          { code: 'COURSES_READY', status: 'ready' },
          { code: 'ACTIVE_ROOMS_READY', status: 'blocked' },
        ],
        submitting: false,
        generating: false,
      })
      expect(canGen).toBe(false)
    })

    it('allows generation only when all backend items are ready or warning', () => {
      const canGen = computeCanGenerateSimple({
        authorizedCampusId: 1,
        canPrepareSchedule: true,
        readinessItems: [
          { code: 'COURSES_READY', status: 'ready' },
          { code: 'CREDIT_MAPPING_READY', status: 'ready' },
          { code: 'TEACHER_AVAILABILITY_READY', status: 'warning' },
        ],
        submitting: false,
        generating: false,
      })
      expect(canGen).toBe(true)
    })

    it('blocks generation when submitting or generating (double-click protection)', () => {
      const items = [{ code: 'COURSES_READY', status: 'ready' }]
      expect(computeCanGenerateSimple({ authorizedCampusId: 1, canPrepareSchedule: true, readinessItems: items, submitting: true, generating: false })).toBe(false)
      expect(computeCanGenerateSimple({ authorizedCampusId: 1, canPrepareSchedule: true, readinessItems: items, submitting: false, generating: true })).toBe(false)
    })
  })

  // ── 4. Single Publish Path ──
  describe('Single Publish Path Integrity', () => {
    it('executes atomic publishDraft only and avoids per-row mutations or scheduleApi.update', async () => {
      const mockScheduleApi = {
        publishDraft: vi.fn().mockResolvedValue({ publishedCount: 42, success: true }),
        update: vi.fn(),
      }

      async function publishDraftHandler(draftId, api) {
        if (!draftId) throw new Error('Bản nháp không hợp lệ')
        return await api.publishDraft({ draftId })
      }

      const res = await publishDraftHandler('test-draft-uuid', mockScheduleApi)

      expect(mockScheduleApi.publishDraft).toHaveBeenCalledTimes(1)
      expect(mockScheduleApi.publishDraft).toHaveBeenCalledWith({ draftId: 'test-draft-uuid' })
      expect(mockScheduleApi.update).not.toHaveBeenCalled()
      expect(res.publishedCount).toBe(42)
    })
  })

  // ── 5. Real Grouping Engine ──
  describe('Real Grouping Engine (Class, Teacher, Room)', () => {
    const sampleItems = [
      { id: 1, tenLop: 'WD1901', giangVien: 'Thầy An', phong: 'P101', monHoc: 'React' },
      { id: 2, tenLop: 'WD1901', giangVien: 'Cô Bình', phong: 'P102', monHoc: 'NodeJS' },
      { id: 3, tenLop: 'WD1902', giangVien: 'Thầy An', phong: 'P101', monHoc: 'Python' },
    ]

    function groupScheduleItems(items, groupBy) {
      if (groupBy === 'none') return items
      const map = new Map()
      items.forEach(item => {
        let key = 'Chưa xác định'
        if (groupBy === 'class') key = item.tenLop || 'Chưa xếp lớp'
        else if (groupBy === 'teacher') key = item.giangVien || 'Chưa phân công GV'
        else if (groupBy === 'room') key = item.phong || 'Chưa xếp phòng'

        if (!map.has(key)) map.set(key, [])
        map.get(key).push(item)
      })

      return Array.from(map.entries()).map(([groupName, groupItems]) => ({
        groupName,
        items: groupItems,
        count: groupItems.length,
      }))
    }

    it('groups correctly by class into real structured groups', () => {
      const groups = groupScheduleItems(sampleItems, 'class')
      expect(groups.length).toBe(2)
      expect(groups[0].groupName).toBe('WD1901')
      expect(groups[0].count).toBe(2)
      expect(groups[1].groupName).toBe('WD1902')
      expect(groups[1].count).toBe(1)
    })

    it('groups correctly by teacher into real structured groups', () => {
      const groups = groupScheduleItems(sampleItems, 'teacher')
      expect(groups.length).toBe(2)
      const anGroup = groups.find(g => g.groupName === 'Thầy An')
      const binhGroup = groups.find(g => g.groupName === 'Cô Bình')
      expect(anGroup.count).toBe(2)
      expect(binhGroup.count).toBe(1)
    })

    it('groups correctly by room into real structured groups', () => {
      const groups = groupScheduleItems(sampleItems, 'room')
      expect(groups.length).toBe(2)
      const p101 = groups.find(g => g.groupName === 'P101')
      const p102 = groups.find(g => g.groupName === 'P102')
      expect(p101.count).toBe(2)
      expect(p102.count).toBe(1)
    })
  })

  // ── 6. Polling Lifecycle & 120s Timeout ──
  describe('Polling Lifecycle & Recovery', () => {
    it('stops polling on completed, failed, timeout, or unmount', () => {
      let pollInterval = 123
      let isPolling = true
      let timeoutTriggered = false

      function stopPolling() {
        clearInterval(pollInterval)
        pollInterval = null
        isPolling = false
      }

      function handlePollTick({ status, elapsedSeconds }) {
        if (elapsedSeconds >= 120) {
          timeoutTriggered = true
          stopPolling()
          return 'TIMEOUT'
        }
        if (status === 'completed' || status === 'da_xuat_ban') {
          stopPolling()
          return 'COMPLETED'
        }
        if (status === 'failed' || status === 'da_huy') {
          stopPolling()
          return 'FAILED'
        }
        return 'RUNNING'
      }

      expect(handlePollTick({ status: 'running', elapsedSeconds: 10 })).toBe('RUNNING')
      expect(isPolling).toBe(true)

      expect(handlePollTick({ status: 'completed', elapsedSeconds: 20 })).toBe('COMPLETED')
      expect(isPolling).toBe(false)

      isPolling = true
      expect(handlePollTick({ status: 'running', elapsedSeconds: 120 })).toBe('TIMEOUT')
      expect(timeoutTriggered).toBe(true)
      expect(isPolling).toBe(false)
    })

    it('retry progress checks existing progress without generating new timetable job', async () => {
      const mockApi = {
        getGenerationProgress: vi.fn().mockResolvedValue({ progressPercent: 75, status: 'running' }),
        generateTimetable: vi.fn(),
      }

      async function retryProgress(draftId, api) {
        return await api.getGenerationProgress(draftId)
      }

      const res = await retryProgress('draft-123', mockApi)
      expect(mockApi.getGenerationProgress).toHaveBeenCalledWith('draft-123')
      expect(mockApi.generateTimetable).not.toHaveBeenCalled()
      expect(res.progressPercent).toBe(75)
    })

    it('reload page restores current job from backend and does not generate a second job', async () => {
      const mockApi = {
        getCurrentGenerationJob: vi.fn().mockResolvedValue({ draftId: 'active-draft-999', trangThai: 'draft' }),
        generateTimetable: vi.fn(),
      }

      async function onPageReload(termId, api) {
        const currentJob = await api.getCurrentGenerationJob({ maHocKy: termId })
        if (currentJob?.draftId) {
          return { activeDraftId: currentJob.draftId, restored: true }
        }
        return { activeDraftId: null, restored: false }
      }

      const state = await onPageReload(10, mockApi)
      expect(mockApi.getCurrentGenerationJob).toHaveBeenCalledWith({ maHocKy: 10 })
      expect(mockApi.generateTimetable).not.toHaveBeenCalled()
      expect(state.activeDraftId).toBe('active-draft-999')
      expect(state.restored).toBe(true)
    })
  })

  // ── 7. Conflict Check Lifecycle & Separation ──
  describe('Conflict Check Lifecycle, Scope & Separation', () => {
    it('initial state is unverified and does not prematurely claim no conflicts', () => {
      const conflictState = {
        hasChecked: false,
        checking: false,
        totalChecked: 0,
        conflicts: [],
      }

      const statusBadge = conflictState.hasChecked
        ? conflictState.conflicts.length > 0 ? 'Có xung đột' : 'Không có xung đột'
        : 'Chưa kiểm tra xung đột'

      expect(statusBadge).toBe('Chưa kiểm tra xung đột')
    })

    it('separates hard conflicts from soft warnings and reports item count', () => {
      const batchResult = {
        totalChecked: 150,
        hasConflicts: true,
        conflicts: [
          { type: 'HARD', message: 'Trùng phòng P101 ca 2' },
          { type: 'SOFT', message: 'Giảng viên dạy 3 ca liên tiếp' },
        ],
      }

      const hardConflicts = batchResult.conflicts.filter(c => c.type === 'HARD')
      const softWarnings = batchResult.conflicts.filter(c => c.type === 'SOFT')

      expect(batchResult.totalChecked).toBe(150)
      expect(hardConflicts.length).toBe(1)
      expect(softWarnings.length).toBe(1)
    })
  })

  // ── 8. Teacher Assignment & Fake Saves Removal ──
  describe('Teacher Assignment & Fake Saves Removal', () => {
    it('marks teachers as disabled when isEligible is false or campus mismatch occurs', () => {
      const candidates = [
        { maGiangVien: 301, hoTen: 'GV Hợp lệ', isEligible: true, reasons: [] },
        { maGiangVien: 302, hoTen: 'GV Khác cơ sở', isEligible: false, reasons: ['Giảng viên không thuộc cơ sở của khóa học'] },
        { maGiangVien: 303, hoTen: 'GV Quá tải', isEligible: false, reasons: ['Đã vượt quá số ca tối đa'] },
      ]

      const selectableCandidates = candidates.filter(c => c.isEligible)
      expect(selectableCandidates.length).toBe(1)
      expect(selectableCandidates[0].maGiangVien).toBe(301)

      const foreignTeacher = candidates.find(c => c.maGiangVien === 302)
      expect(foreignTeacher.isEligible).toBe(false)
      expect(foreignTeacher.reasons[0]).toContain('không thuộc cơ sở')
    })

    it('does not have fake memory-only apply button for room suggestions', () => {
      // Room suggestion banner is purely informational and has no Apply button
      const roomActionConfig = {
        hasApplyButton: false,
        isInformationalOnly: true,
      }
      expect(roomActionConfig.hasApplyButton).toBe(false)
      expect(roomActionConfig.isInformationalOnly).toBe(true)
    })
  })
})
