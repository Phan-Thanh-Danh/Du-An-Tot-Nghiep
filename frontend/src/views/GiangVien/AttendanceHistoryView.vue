<script setup>
import { ref, computed } from 'vue'
import {
  Search,
  Calendar,
  Filter,
  Download,
  ChevronRight,
  ArrowUpDown,
  Clock,
  Users,
  CheckCircle2,
  AlertTriangle,
  BookOpen,
  X,
  Edit3,
  Save,
  AlertCircle,
} from 'lucide-vue-next'

import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import TableShell from '@/components/ui/TableShell.vue'

const attendanceHistory = ref([
  { id: 1, date: '12/05/2026', className: 'SE1601 - Java', absences: 3, total: 30, time: '07:30', room: 'A201' },
  { id: 2, date: '11/05/2026', className: 'SS1402 - Web', absences: 5, total: 32, time: '12:30', room: 'B305' },
  { id: 3, date: '10/05/2026', className: 'SE1601 - Java', absences: 1, total: 30, time: '07:30', room: 'A201' },
  { id: 4, date: '08/05/2026', className: 'SA1709 - DB', absences: 0, total: 25, time: '09:45', room: 'C102' },
])

// Mock data for student list per session
const sessionStudentsMock = ref({
  1: [
    { id: 'SV16001', name: 'Nguyễn Văn Anh', status: 'Present', time: '07:25', note: '' },
    { id: 'SV16002', name: 'Trần Thị Bình', status: 'Present', time: '07:28', note: '' },
    { id: 'SV16003', name: 'Lê Hoàng Cường', status: 'Late', time: '07:42', note: 'Kẹt xe đường Cộng Hòa' },
    { id: 'SV16004', name: 'Phạm Minh Danh', status: 'Absent', time: '--', note: 'Không lý do' },
    { id: 'SV16005', name: 'Đỗ Thùy Dương', status: 'Present', time: '07:18', note: '' },
    { id: 'SV16006', name: 'Nguyễn Tiến Đạt', status: 'Present', time: '07:22', note: '' },
    { id: 'SV16007', name: 'Vũ Thị Giang', status: 'Absent', time: '--', note: 'Có phép (Bị ốm sốt)' },
    { id: 'SV16008', name: 'Lê Minh Hải', status: 'Present', time: '07:29', note: '' },
    { id: 'SV16009', name: 'Phạm Thanh Hương', status: 'Absent', time: '--', note: 'Nghỉ không phép' },
    { id: 'SV16010', name: 'Nguyễn Hữu Khánh', status: 'Present', time: '07:24', note: '' }
  ],
  2: [
    { id: 'SV14001', name: 'Nguyễn Văn Anh', status: 'Present', time: '12:20', note: '' },
    { id: 'SV14002', name: 'Trần Thị Bình', status: 'Absent', time: '--', note: 'Nghỉ có phép' },
    { id: 'SV14003', name: 'Lê Hoàng Cường', status: 'Present', time: '12:25', note: '' },
    { id: 'SV14004', name: 'Phạm Minh Danh', status: 'Absent', time: '--', note: 'Nghỉ không phép' },
    { id: 'SV14005', name: 'Đỗ Thùy Dương', status: 'Late', time: '12:45', note: 'Hỏng xe giữa đường' },
    { id: 'SV14006', name: 'Nguyễn Tiến Đạt', status: 'Absent', time: '--', note: 'Không phép' },
    { id: 'SV14007', name: 'Vũ Thị Giang', status: 'Absent', time: '--', note: 'Không phép' },
    { id: 'SV14008', name: 'Lê Minh Hải', status: 'Present', time: '12:15', note: '' },
    { id: 'SV14009', name: 'Phạm Thanh Hương', status: 'Absent', time: '--', note: 'Không phép' },
    { id: 'SV14010', name: 'Nguyễn Hữu Khánh', status: 'Present', time: '12:22', note: '' }
  ],
  3: [
    { id: 'SV16001', name: 'Nguyễn Văn Anh', status: 'Present', time: '07:22', note: '' },
    { id: 'SV16002', name: 'Trần Thị Bình', status: 'Present', time: '07:25', note: '' },
    { id: 'SV16003', name: 'Lê Hoàng Cường', status: 'Present', time: '07:27', note: '' },
    { id: 'SV16004', name: 'Phạm Minh Danh', status: 'Present', time: '07:29', note: '' },
    { id: 'SV16005', name: 'Đỗ Thùy Dương', status: 'Present', time: '07:15', note: '' },
    { id: 'SV16006', name: 'Nguyễn Tiến Đạt', status: 'Present', time: '07:20', note: '' },
    { id: 'SV16007', name: 'Vũ Thị Giang', status: 'Present', time: '07:22', note: '' },
    { id: 'SV16008', name: 'Lê Minh Hải', status: 'Present', time: '07:24', note: '' },
    { id: 'SV16009', name: 'Phạm Thanh Hương', status: 'Absent', time: '--', note: 'Đi muộn quá giờ' },
    { id: 'SV16010', name: 'Nguyễn Hữu Khánh', status: 'Present', time: '07:21', note: '' }
  ],
  4: [
    { id: 'SV17001', name: 'Nguyễn Văn Anh', status: 'Present', time: '09:35', note: '' },
    { id: 'SV17002', name: 'Trần Thị Bình', status: 'Present', time: '09:40', note: '' },
    { id: 'SV17003', name: 'Lê Hoàng Cường', status: 'Present', time: '09:42', note: '' },
    { id: 'SV17004', name: 'Phạm Minh Danh', status: 'Present', time: '09:45', note: '' },
    { id: 'SV17005', name: 'Đỗ Thùy Dương', status: 'Present', time: '09:30', note: '' },
    { id: 'SV17006', name: 'Nguyễn Tiến Đạt', status: 'Present', time: '09:33', note: '' },
    { id: 'SV17007', name: 'Vũ Thị Giang', status: 'Present', time: '09:38', note: '' },
    { id: 'SV17008', name: 'Lê Minh Hải', status: 'Present', time: '09:41', note: '' },
    { id: 'SV17009', name: 'Phạm Thanh Hương', status: 'Present', time: '09:43', note: '' },
    { id: 'SV17010', name: 'Nguyễn Hữu Khánh', status: 'Present', time: '09:44', note: '' }
  ]
})

// Search & Filter History
const historySearch = ref('')
const historyDate = ref('')

const filteredHistory = computed(() => {
  return attendanceHistory.value.filter(item => {
    const matchSearch = item.className.toLowerCase().includes(historySearch.value.toLowerCase()) ||
                        item.room.toLowerCase().includes(historySearch.value.toLowerCase())

    let matchDate = true
    if (historyDate.value) {
      const parts = historyDate.value.split('-')
      const formattedDate = `${parts[2]}/${parts[1]}/${parts[0]}`
      matchDate = item.date === formattedDate
    }

    return matchSearch && matchDate
  })
})

// Selected Session Detail state
const selectedSession = ref(null)
const isDetailModalOpen = ref(false)
const activeStudents = ref([])
const activeSearchQuery = ref('')
const activeFilterStatus = ref('')
const isEditing = ref(false)

const toast = ref({
  show: false,
  message: '',
  type: 'success'
})

const historySummary = computed(() => {
  const totalSessions = attendanceHistory.value.length
  const totalStudents = attendanceHistory.value.reduce((sum, item) => sum + item.total, 0)
  const totalAbsences = attendanceHistory.value.reduce((sum, item) => sum + item.absences, 0)
  const attendanceRate = totalStudents
    ? Math.round(((totalStudents - totalAbsences) / totalStudents) * 100)
    : 0

  return [
    { label: 'Tổng buổi', value: totalSessions, tone: 'primary' },
    { label: 'Đã xác nhận', value: totalSessions, tone: 'success' },
    { label: 'Chưa xác nhận', value: 0, tone: 'warning' },
    { label: 'Tỷ lệ CC', value: `${attendanceRate}%`, tone: 'success' },
    { label: 'Lượt vắng', value: totalAbsences, tone: 'danger' },
    { label: 'Đi muộn', value: countAllByStatus('Late'), tone: 'warning' },
  ]
})

function countAllByStatus(status) {
  return Object.values(sessionStudentsMock.value)
    .flat()
    .filter((student) => student.status === status).length
}

function triggerToast(msg, type = 'success') {
  toast.value.message = msg
  toast.value.type = type
  toast.value.show = true
  setTimeout(() => {
    toast.value.show = false
  }, 4000)
}

function openSessionDetails(session) {
  selectedSession.value = { ...session }
  const list = sessionStudentsMock.value[session.id] || []
  activeStudents.value = JSON.parse(JSON.stringify(list))
  activeSearchQuery.value = ''
  activeFilterStatus.value = ''
  isEditing.value = false
  isDetailModalOpen.value = true
}

const filteredStudents = computed(() => {
  return activeStudents.value.filter(sv => {
    const matchSearch = sv.name.toLowerCase().includes(activeSearchQuery.value.toLowerCase()) ||
                        sv.id.toLowerCase().includes(activeSearchQuery.value.toLowerCase())
    const matchStatus = !activeFilterStatus.value || sv.status === activeFilterStatus.value
    return matchSearch && matchStatus
  })
})

const sessionStats = computed(() => {
  const total = activeStudents.value.length
  const present = activeStudents.value.filter(s => s.status === 'Present').length
  const late = activeStudents.value.filter(s => s.status === 'Late').length
  const absent = activeStudents.value.filter(s => s.status === 'Absent').length

  return { total, present, late, absent }
})

function changeStatus(index, status) {
  if (!isEditing.value) return
  activeStudents.value[index].status = status
  if (status === 'Absent') {
    activeStudents.value[index].time = '--'
  } else if (status === 'Present' && activeStudents.value[index].time === '--') {
    activeStudents.value[index].time = '07:30' // Fallback checkin time
  }
}

function saveAttendanceChanges() {
  if (selectedSession.value) {
    const idx = attendanceHistory.value.findIndex(h => h.id === selectedSession.value.id)
    if (idx !== -1) {
      const absencesCount = activeStudents.value.filter(s => s.status === 'Absent').length
      attendanceHistory.value[idx].absences = absencesCount
      attendanceHistory.value[idx].total = activeStudents.value.length
    }

    sessionStudentsMock.value[selectedSession.value.id] = JSON.parse(JSON.stringify(activeStudents.value))
    isEditing.value = false
    triggerToast('Đã cập nhật lịch sử điểm danh thành công!', 'success')
  }
}

function studentStatusVariant(status) {
  if (status === 'Present') return 'success'
  if (status === 'Late') return 'warning'
  return 'danger'
}

function studentStatusLabel(status) {
  if (status === 'Present') return 'Có mặt'
  if (status === 'Late') return 'Đi muộn'
  return 'Vắng mặt'
}
</script>

<template>
  <div class="attendance-history-page lg-page-enter">
    <Transition name="toast-slide">
      <div v-if="toast.show" :class="['toast', toast.type === 'success' ? 'success' : 'danger']">
        <CheckCircle2 v-if="toast.type === 'success'" :size="18" />
        <AlertCircle v-else :size="18" />
        <p>{{ toast.message }}</p>
      </div>
    </Transition>

    <GlassPanel variant="flat" density="compact" class="page-header">
      <div class="header-copy">
        <div class="eyebrow">
          <Calendar :size="15" />
          Spring 2026 · Block 2
        </div>
        <div>
          <h1>Lịch sử điểm danh</h1>
          <p>Xem lại nhật ký điểm danh, tỷ lệ chuyên cần và chi tiết từng buổi học.</p>
        </div>
      </div>

      <GlassButton variant="primary" size="sm">
        <template #leading>
          <Download :size="16" />
        </template>
        Xuất báo cáo
      </GlassButton>
    </GlassPanel>

    <GlassPanel variant="flat" density="compact" class="context-panel">
      <div class="summary-strip">
        <div v-for="item in historySummary" :key="item.label" :class="['summary-pill', item.tone]">
          <span>{{ item.label }}</span>
          <strong>{{ item.value }}</strong>
        </div>
      </div>

      <div class="filters">
        <label class="input-shell">
          <Search :size="16" />
          <input v-model="historySearch" type="text" placeholder="Tìm lớp học, mã phòng..." />
        </label>
        <label class="input-shell date-shell">
          <Calendar :size="16" />
          <input v-model="historyDate" type="date" />
        </label>
      </div>
    </GlassPanel>

    <GlassPanel variant="flat" density="compact" class="history-panel">
      <div class="panel-title">
        <div>
          <h2>
            <BookOpen :size="17" />
            Bảng lịch sử buổi học
          </h2>
          <p>Hiển thị {{ filteredHistory.length }} bản ghi theo bộ lọc hiện tại.</p>
        </div>
        <GlassBadge variant="success">Đã xác nhận</GlassBadge>
      </div>

      <TableShell v-if="filteredHistory.length" density="compact">
        <table>
          <thead>
            <tr>
              <th>
                <span class="sortable-label">
                  Ngày
                  <ArrowUpDown :size="12" />
                </span>
              </th>
              <th>Lớp / Môn</th>
              <th>Ca học / Phòng</th>
              <th>Tổng SV</th>
              <th>Có mặt</th>
              <th>Vắng</th>
              <th>Đi muộn</th>
              <th>Có phép</th>
              <th>Trạng thái</th>
              <th class="text-right">Hành động</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in filteredHistory" :key="item.id">
              <td>
                <div class="date-cell">
                  <span class="date-icon">
                    <Calendar :size="16" />
                  </span>
                  <span>
                    <strong>{{ item.date }}</strong>
                    <small>Học kỳ Fall</small>
                  </span>
                </div>
              </td>
              <td>
                <div class="class-cell">
                  <strong>{{ item.className }}</strong>
                  <small>Phòng {{ item.room }}</small>
                </div>
              </td>
              <td>
                <span class="time-cell">
                  <Clock :size="13" />
                  {{ item.time }} · {{ item.room }}
                </span>
              </td>
              <td class="number-cell">{{ item.total }}</td>
              <td class="number-cell success">{{ item.total - item.absences }}</td>
              <td class="number-cell danger">{{ item.absences }}</td>
              <td class="number-cell warning">0</td>
              <td class="number-cell info">0</td>
              <td>
                <GlassBadge :variant="item.absences === 0 ? 'success' : 'warning'">
                  {{ item.absences === 0 ? 'Đã xác nhận' : 'Có vắng' }}
                </GlassBadge>
              </td>
              <td>
                <div class="row-actions">
                  <GlassButton variant="ghost" size="sm">
                    <template #leading>
                      <Download :size="13" />
                    </template>
                    Xuất
                  </GlassButton>
                  <GlassButton variant="secondary" size="sm" @click="openSessionDetails(item)">
                    Chi tiết
                    <template #trailing>
                      <ChevronRight :size="13" />
                    </template>
                  </GlassButton>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </TableShell>

      <div v-else class="empty-block">
        <Calendar :size="32" />
        <h3>Không tìm thấy lịch sử điểm danh</h3>
        <p>Vui lòng thử lại với từ khóa hoặc bộ lọc khác.</p>
      </div>
    </GlassPanel>

    <Teleport to="body">
      <div v-if="isDetailModalOpen" class="modal-root">
        <button
          type="button"
          class="modal-backdrop"
          aria-label="Đóng chi tiết điểm danh"
          @click="isDetailModalOpen = false"
        />

        <GlassPanel variant="flat" density="none" class="detail-modal">
          <div class="modal-header">
            <div class="modal-title">
              <span class="modal-icon">
                <Calendar :size="20" />
              </span>
              <span v-if="selectedSession">
                <h3>Chi tiết điểm danh</h3>
                <p>
                  {{ selectedSession.className }} · {{ selectedSession.date }} ·
                  {{ selectedSession.time }} · Phòng {{ selectedSession.room }}
                </p>
              </span>
            </div>
            <button type="button" class="icon-button" @click="isDetailModalOpen = false">
              <X :size="18" />
            </button>
          </div>

          <div class="modal-summary">
            <div class="summary-pill primary">
              <span>Tổng số</span>
              <strong>{{ sessionStats.total }}</strong>
            </div>
            <div class="summary-pill success">
              <span>Có mặt</span>
              <strong>{{ sessionStats.present }}</strong>
            </div>
            <div class="summary-pill warning">
              <span>Đi muộn</span>
              <strong>{{ sessionStats.late }}</strong>
            </div>
            <div class="summary-pill danger">
              <span>Vắng</span>
              <strong>{{ sessionStats.absent }}</strong>
            </div>
          </div>

          <div class="modal-toolbar">
            <label class="input-shell">
              <Search :size="15" />
              <input v-model="activeSearchQuery" type="text" placeholder="Tìm sinh viên, MSSV..." />
            </label>

            <div class="filter-chips">
              <span>Lọc:</span>
              <button type="button" :class="['chip', !activeFilterStatus ? 'active' : '']" @click="activeFilterStatus = ''">
                Tất cả
              </button>
              <button type="button" :class="['chip success', activeFilterStatus === 'Present' ? 'active' : '']" @click="activeFilterStatus = 'Present'">
                Có mặt
              </button>
              <button type="button" :class="['chip warning', activeFilterStatus === 'Late' ? 'active' : '']" @click="activeFilterStatus = 'Late'">
                Muộn
              </button>
              <button type="button" :class="['chip danger', activeFilterStatus === 'Absent' ? 'active' : '']" @click="activeFilterStatus = 'Absent'">
                Vắng
              </button>
            </div>
          </div>

          <div class="modal-body">
            <TableShell v-if="filteredStudents.length" density="compact" sticky-header>
              <table>
                <thead>
                  <tr>
                    <th>Sinh viên</th>
                    <th>MSSV</th>
                    <th>Giờ điểm danh</th>
                    <th>Trạng thái</th>
                    <th>Ghi chú</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="(sv, idx) in filteredStudents" :key="sv.id">
                    <td>
                      <div class="student-cell">
                        <span class="student-avatar">{{ sv.name.split(' ').pop()[0] }}</span>
                        <strong>{{ sv.name }}</strong>
                      </div>
                    </td>
                    <td class="student-code">{{ sv.id }}</td>
                    <td>
                      <span class="time-cell">
                        <Clock :size="12" />
                        {{ sv.time }}
                      </span>
                    </td>
                    <td>
                      <GlassBadge v-if="!isEditing" :variant="studentStatusVariant(sv.status)">
                        {{ studentStatusLabel(sv.status) }}
                      </GlassBadge>
                      <div v-else class="status-actions">
                        <button
                          type="button"
                          :class="['status-button success', sv.status === 'Present' ? 'active' : '']"
                          @click="changeStatus(idx, 'Present')"
                        >
                          Có
                        </button>
                        <button
                          type="button"
                          :class="['status-button warning', sv.status === 'Late' ? 'active' : '']"
                          @click="changeStatus(idx, 'Late')"
                        >
                          Muộn
                        </button>
                        <button
                          type="button"
                          :class="['status-button danger', sv.status === 'Absent' ? 'active' : '']"
                          @click="changeStatus(idx, 'Absent')"
                        >
                          Vắng
                        </button>
                      </div>
                    </td>
                    <td>
                      <span v-if="!isEditing" class="note-text" :title="sv.note">{{ sv.note || '--' }}</span>
                      <input v-else v-model="sv.note" type="text" placeholder="Nhập lý do..." class="note-input" />
                    </td>
                  </tr>
                </tbody>
              </table>
            </TableShell>

            <div v-else class="empty-block compact">
              <Users :size="28" />
              <h3>Không tìm thấy sinh viên</h3>
            </div>
          </div>

          <div class="modal-footer">
            <GlassButton variant="secondary" size="sm">
              <template #leading>
                <Download :size="15" />
              </template>
              Xuất file buổi này
            </GlassButton>

            <div class="modal-actions">
              <GlassButton v-if="!isEditing" variant="primary" size="sm" @click="isEditing = true">
                <template #leading>
                  <Edit3 :size="15" />
                </template>
                Chỉnh sửa
              </GlassButton>

              <template v-else>
                <GlassButton variant="secondary" size="sm" @click="isEditing = false">Hủy</GlassButton>
                <GlassButton variant="primary" size="sm" @click="saveAttendanceChanges">
                  <template #leading>
                    <Save :size="15" />
                  </template>
                  Lưu thay đổi
                </GlassButton>
              </template>
            </div>
          </div>
        </GlassPanel>
      </div>
    </Teleport>
  </div>
</template>

<style scoped>
.attendance-history-page {
  position: relative;
  display: flex;
  flex-direction: column;
  gap: 0.875rem;
  padding-bottom: 2.5rem;
  color: var(--text-body);
}

.toast {
  position: fixed;
  top: 1rem;
  right: 1.5rem;
  z-index: 1000;
  display: flex;
  align-items: center;
  gap: 0.6rem;
  max-width: min(24rem, calc(100vw - 2rem));
  border: 1px solid var(--border-card);
  border-radius: var(--radius-lg);
  background: var(--surface-modal);
  padding: 0.8rem 1rem;
  color: var(--text-heading);
  box-shadow: var(--lg-shadow-md);
}

.toast.success {
  background: var(--color-success-bg);
  color: var(--color-success-text);
}

.toast.danger {
  background: var(--color-danger-bg);
  color: var(--color-danger-text);
}

.toast p {
  margin: 0;
  font-size: 0.84rem;
  font-weight: 800;
}

.page-header,
.context-panel,
.panel-title,
.modal-header,
.modal-toolbar,
.modal-footer,
.modal-title,
.summary-strip,
.filters,
.row-actions,
.date-cell,
.time-cell,
.student-cell,
.status-actions,
.filter-chips,
.modal-actions {
  display: flex;
  align-items: center;
}

.page-header,
.context-panel,
.panel-title,
.modal-header,
.modal-toolbar,
.modal-footer {
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
}

.header-copy {
  display: flex;
  min-width: 0;
  flex-direction: column;
  gap: 0.5rem;
}

.eyebrow {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  width: fit-content;
  border: 1px solid var(--border-card);
  border-radius: 999px;
  background: var(--surface-input);
  color: var(--text-link);
  padding: 0.25rem 0.6rem;
  font-size: 0.7rem;
  font-weight: 850;
  text-transform: uppercase;
}

.header-copy h1,
.panel-title h2,
.modal-title h3,
.empty-block h3 {
  margin: 0;
  color: var(--text-heading);
  font-weight: 900;
}

.header-copy h1 {
  font-size: 1.45rem;
  line-height: 1.15;
}

.panel-title h2 {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 1rem;
}

.modal-title h3,
.empty-block h3 {
  font-size: 1rem;
}

.header-copy p,
.panel-title p,
.summary-pill span,
.date-cell small,
.class-cell small,
.student-code,
.note-text,
.empty-block p {
  color: var(--text-muted);
}

.header-copy p,
.panel-title p,
.modal-title p,
.empty-block p {
  margin: 0.25rem 0 0;
  font-size: 0.84rem;
}

.context-panel {
  align-items: center;
}

.summary-strip,
.filters,
.row-actions,
.status-actions,
.filter-chips,
.modal-actions {
  gap: 0.45rem;
  flex-wrap: wrap;
}

.summary-pill {
  display: grid;
  min-width: 5rem;
  gap: 0.05rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  padding: 0.45rem 0.6rem;
}

.summary-pill strong {
  color: var(--text-heading);
  font-size: 1rem;
  font-weight: 900;
}

.summary-pill span {
  font-size: 0.68rem;
  font-weight: 800;
}

.summary-pill.primary,
.summary-pill.info {
  background: var(--accent-primary-soft);
}

.summary-pill.success {
  background: var(--color-success-bg);
}

.summary-pill.warning {
  background: var(--color-warning-bg);
}

.summary-pill.danger {
  background: var(--color-danger-bg);
}

.input-shell {
  display: inline-flex;
  align-items: center;
  min-height: 2.25rem;
  width: min(20rem, 100%);
  gap: 0.45rem;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  padding: 0 0.7rem;
  color: var(--text-placeholder);
  transition:
    border-color 0.2s ease,
    background 0.2s ease,
    box-shadow 0.2s ease;
}

.date-shell {
  width: min(13rem, 100%);
}

.input-shell:focus-within,
.note-input:focus {
  border-color: var(--border-input-focus);
  background: var(--surface-input-focus);
  box-shadow: 0 0 0 3px var(--border-focus-ring);
}

.input-shell input {
  min-width: 0;
  width: 100%;
  border: 0;
  outline: 0;
  background: transparent;
  color: var(--text-heading);
  font-size: 0.82rem;
  font-weight: 750;
}

.input-shell input::placeholder,
.note-input::placeholder {
  color: var(--text-placeholder);
}

.history-panel {
  display: flex;
  flex-direction: column;
  gap: 0.8rem;
}

.panel-title {
  border-bottom: 1px solid var(--border-card);
  padding-bottom: 0.75rem;
}

.sortable-label {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  color: var(--text-link);
  white-space: nowrap;
}

.date-cell {
  min-width: 9rem;
  gap: 0.65rem;
}

.date-icon,
.modal-icon,
.student-avatar {
  display: inline-flex;
  flex: none;
  align-items: center;
  justify-content: center;
  border: 1px solid var(--border-card);
  background: var(--surface-input);
}

.date-icon {
  width: 2rem;
  height: 2rem;
  border-radius: var(--radius-md);
  color: var(--text-link);
}

.date-cell strong,
.class-cell strong,
.student-cell strong {
  display: block;
  color: var(--text-heading);
  font-size: 0.86rem;
  font-weight: 850;
}

.date-cell small,
.class-cell small {
  display: block;
  margin-top: 0.1rem;
  font-size: 0.72rem;
  font-weight: 750;
}

.time-cell {
  gap: 0.35rem;
  min-width: 6.5rem;
  color: var(--text-muted);
  font-size: 0.8rem;
  font-weight: 750;
}

.number-cell {
  color: var(--text-heading);
  font-size: 0.82rem;
  font-weight: 900;
}

.number-cell.success {
  color: var(--color-success-text);
}

.number-cell.danger {
  color: var(--color-danger-text);
}

.number-cell.warning {
  color: var(--color-warning-text);
}

.number-cell.info {
  color: var(--text-link);
}

.row-actions {
  justify-content: flex-end;
  min-width: 9rem;
}

.empty-block {
  border: 1px solid var(--border-card);
  border-radius: var(--radius-lg);
  background: var(--surface-input);
  padding: 2rem;
  text-align: center;
  color: var(--text-placeholder);
}

.empty-block.compact {
  padding: 1.5rem;
}

.modal-root {
  position: fixed;
  inset: 0;
  z-index: 999;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
}

.modal-backdrop {
  position: absolute;
  inset: 0;
  border: 0;
  background: var(--surface-modal);
  cursor: pointer;
}

.detail-modal {
  position: relative;
  z-index: 1;
  display: flex;
  width: min(64rem, 100%);
  max-height: 88vh;
  flex-direction: column;
  animation: modal-in 0.22s ease-out both;
}

.modal-header,
.modal-toolbar,
.modal-footer,
.modal-summary {
  padding: 1rem;
  border-bottom: 1px solid var(--border-card);
  background: var(--surface-card);
}

.modal-footer {
  border-top: 1px solid var(--border-card);
  border-bottom: 0;
}

.modal-title {
  gap: 0.7rem;
}

.modal-title p {
  color: var(--text-muted);
}

.modal-icon {
  width: 2.35rem;
  height: 2.35rem;
  border-radius: var(--radius-lg);
  color: var(--text-link);
}

.icon-button {
  display: inline-flex;
  width: 2.25rem;
  height: 2.25rem;
  align-items: center;
  justify-content: center;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  color: var(--text-label);
  transition:
    border-color 0.2s ease,
    background 0.2s ease,
    color 0.2s ease;
}

.icon-button:hover {
  border-color: var(--border-input-focus);
  background: var(--surface-input-focus);
  color: var(--text-link);
}

.modal-summary {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 0.5rem;
}

.modal-toolbar {
  align-items: center;
}

.filter-chips span {
  color: var(--text-label);
  font-size: 0.72rem;
  font-weight: 850;
  text-transform: uppercase;
}

.chip,
.status-button {
  min-height: 1.85rem;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-sm);
  background: var(--surface-input);
  color: var(--text-label);
  padding: 0 0.65rem;
  font-size: 0.72rem;
  font-weight: 850;
  transition:
    border-color 0.2s ease,
    background 0.2s ease,
    color 0.2s ease;
}

.chip:hover,
.status-button:hover,
.chip.active,
.status-button.active {
  border-color: var(--border-input-focus);
  background: var(--accent-primary-soft);
  color: var(--text-link);
}

.chip.success.active,
.status-button.success.active {
  background: var(--color-success-bg);
  color: var(--color-success-text);
}

.chip.warning.active,
.status-button.warning.active {
  background: var(--color-warning-bg);
  color: var(--color-warning-text);
}

.chip.danger.active,
.status-button.danger.active {
  background: var(--color-danger-bg);
  color: var(--color-danger-text);
}

.modal-body {
  flex: 1;
  min-height: 0;
  overflow-y: auto;
  padding: 1rem;
  background: var(--surface-input);
}

.student-cell {
  min-width: 13rem;
  gap: 0.65rem;
}

.student-avatar {
  width: 2rem;
  height: 2rem;
  border-radius: var(--radius-md);
  color: var(--text-link);
  font-size: 0.75rem;
  font-weight: 900;
}

.student-code {
  color: var(--text-muted);
  font-size: 0.8rem;
  font-weight: 750;
}

.note-text {
  display: inline-block;
  max-width: 14rem;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  font-size: 0.8rem;
  font-style: italic;
}

.note-input {
  width: 100%;
  min-height: 2rem;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-sm);
  background: var(--surface-input);
  color: var(--text-heading);
  padding: 0 0.65rem;
  outline: none;
  font-size: 0.78rem;
  font-weight: 650;
  transition:
    border-color 0.2s ease,
    background 0.2s ease,
    box-shadow 0.2s ease;
}

.toast-slide-enter-active,
.toast-slide-leave-active {
  transition: all 0.25s ease;
}

.toast-slide-enter-from {
  transform: translateY(-0.75rem);
  opacity: 0;
}

.toast-slide-leave-to {
  transform: translateY(0.75rem);
  opacity: 0;
}

@keyframes modal-in {
  from {
    opacity: 0;
    transform: translateY(0.75rem) scale(0.98);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}

@media (max-width: 1024px) {
  .page-header,
  .context-panel,
  .panel-title,
  .modal-toolbar,
  .modal-footer {
    flex-direction: column;
    align-items: stretch;
  }

  .summary-strip,
  .filters,
  .row-actions,
  .modal-actions {
    justify-content: flex-start;
  }

  .modal-summary {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 640px) {
  .summary-strip,
  .filters,
  .filter-chips,
  .modal-actions,
  .row-actions {
    display: grid;
    grid-template-columns: 1fr;
  }

  .summary-pill,
  .input-shell {
    width: 100%;
  }

  .modal-root {
    align-items: stretch;
    padding: 0.5rem;
  }

  .detail-modal {
    max-height: calc(100vh - 1rem);
  }
}
</style>
