<script setup>
import { computed, onMounted, ref } from 'vue'
import {
  AlertCircle,
  ArrowUpDown,
  Download,
  Eye,
  Info,
  Lock,
  Search,
  Unlock,
  Users,
  X,
} from 'lucide-vue-next'

import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import TableShell from '@/components/ui/TableShell.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import { useRoute, useRouter } from 'vue-router'
import { teacherApi } from '@/services/teacherApi'

const route = useRoute()
const router = useRouter()
const classId = route.params.classId

// ── Data ──
const gradeData = ref(null)
const gradeColumns = ref([])
const students = ref([])
const loading = ref(false)
const searchQuery = ref('')

// ── Detail modal ──
const showDetailModal = ref(false)
const detailLoading = ref(false)
const detailData = ref(null)
const detailStudentName = ref('')

// ── Lock confirm ──
const showLockConfirm = ref(false)
const lockTarget = ref(null)
const lockLoading = ref(false)

// ── Unlock request ──
const showUnlockModal = ref(false)
const unlockTarget = ref(null)
const unlockLyDo = ref('')
const unlockLoading = ref(false)
const unlockSuccess = ref(false)

// ── Computed ──
const filteredStudents = computed(() => {
  if (!searchQuery.value.trim()) return students.value
  const q = searchQuery.value.trim().toLowerCase()
  return students.value.filter(
    (sv) =>
      sv.studentName.toLowerCase().includes(q) ||
      String(sv.studentId).includes(q),
  )
})

const gradeSummary = computed(() => {
  const all = students.value
  const withGrade = all.filter((s) => s.gpaMonHoc != null)
  const passed = all.filter((s) => s.trangThai === 'dat').length
  const failed = all.filter((s) => s.trangThai === 'rot').length
  const avg =
    withGrade.length > 0
      ? (withGrade.reduce((sum, s) => sum + Number(s.gpaMonHoc), 0) / withGrade.length).toFixed(1)
      : '—'

  return [
    { label: 'Sinh viên', value: all.length, tone: 'primary' },
    { label: 'Đã có điểm', value: withGrade.length, tone: 'success' },
    { label: 'Đạt', value: passed, tone: 'success' },
    { label: 'Rớt', value: failed, tone: 'danger' },
    { label: 'TB lớp', value: avg, tone: 'neutral' },
  ]
})

// ── Helpers ──
function formatGrade(value) {
  if (value === null || value === undefined) return '—'
  return Number(value).toFixed(2)
}

function trangThaiLabel(trangThai) {
  if (trangThai === 'dat') return 'Đạt'
  if (trangThai === 'rot') return 'Rớt'
  return '—'
}

function trangThaiVariant(trangThai) {
  if (trangThai === 'dat') return 'success'
  if (trangThai === 'rot') return 'danger'
  return 'neutral'
}

// ── Load grades ──
async function loadGrades() {
  loading.value = true
  try {
    const res = await teacherApi.getClassGradesV2(classId)
    const data = res?.data ?? res?.Data ?? res
    gradeData.value = data
    gradeColumns.value = data?.gradeColumns ?? data?.GradeColumns ?? []
    students.value = data?.students ?? data?.Students ?? []
  } catch (error) {
    console.error('Lỗi khi tải bảng điểm:', error)
  } finally {
    loading.value = false
  }
}

onMounted(loadGrades)

// ── Detail modal ──
async function openDetail(sv) {
  detailStudentName.value = sv.studentName
  detailData.value = null
  showDetailModal.value = true
  detailLoading.value = true
  try {
    const res = await teacherApi.getStudentGradeDetail(classId, sv.studentId)
    detailData.value = res?.data ?? res?.Data ?? res
  } catch (error) {
    console.error('Lỗi khi tải chi tiết điểm:', error)
  } finally {
    detailLoading.value = false
  }
}

function closeDetail() {
  showDetailModal.value = false
  detailData.value = null
}

// ── Lock ──
function openLockConfirm(sv) {
  lockTarget.value = sv
  showLockConfirm.value = true
}

async function confirmLock() {
  if (!lockTarget.value) return
  lockLoading.value = true
  try {
    await teacherApi.lockStudentGrade(classId, lockTarget.value.studentId)
    showLockConfirm.value = false
    lockTarget.value = null
    await loadGrades()
  } catch (error) {
    console.error('Lỗi khi khoá điểm:', error)
    alert('Không thể khoá điểm: ' + (error?.message || 'Lỗi không xác định'))
  } finally {
    lockLoading.value = false
  }
}

// ── Unlock request ──
function openUnlockModal(sv) {
  unlockTarget.value = sv
  unlockLyDo.value = ''
  unlockSuccess.value = false
  showUnlockModal.value = true
}

async function submitUnlockRequest() {
  if (!unlockTarget.value || !unlockLyDo.value.trim()) return
  unlockLoading.value = true
  try {
    await teacherApi.requestUnlockStudentGrade(
      classId,
      unlockTarget.value.studentId,
      unlockLyDo.value.trim(),
    )
    unlockSuccess.value = true
  } catch (error) {
    console.error('Lỗi khi gửi yêu cầu mở khoá:', error)
    alert('Không thể gửi yêu cầu: ' + (error?.message || 'Lỗi không xác định'))
  } finally {
    unlockLoading.value = false
  }
}

function closeUnlockModal() {
  showUnlockModal.value = false
  unlockTarget.value = null
  if (unlockSuccess.value) {
    loadGrades()
  }
}
</script>

<template>
  <div class="grades-page lg-page-enter">
    <GlassPanel variant="flat" density="compact" class="grades-header">
      <div class="header-copy">
        <div class="eyebrow">
          <Users :size="15" />
          {{ gradeData?.className ?? gradeData?.ClassName ?? '' }} · {{ gradeData?.subjectName ?? gradeData?.SubjectName ?? '' }}
        </div>
        <div>
          <h1>Bảng điểm tổng hợp</h1>
          <p>Xem điểm thành phần, điểm tổng kết và trạng thái khoá/mở khoá bảng điểm.</p>
        </div>
      </div>

      <div class="header-actions">
        <GlassButton variant="secondary" size="sm" @click="router.push('/teacher/grading-input')">
          Quay lại danh sách
        </GlassButton>
        <GlassButton variant="secondary" size="sm">
          <template #leading>
            <Download :size="16" />
          </template>
          Xuất bảng điểm
        </GlassButton>
      </div>
    </GlassPanel>

    <GlassPanel variant="flat" density="compact" class="context-panel">
      <div class="filter-row">
        <label class="search-field">
          <Search :size="16" />
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Tìm sinh viên bằng tên hoặc MSSV..."
          />
        </label>
      </div>

      <div class="summary-strip">
        <div v-for="item in gradeSummary" :key="item.label" :class="['summary-pill', item.tone]">
          <span>{{ item.label }}</span>
          <strong>{{ item.value }}</strong>
        </div>
      </div>
    </GlassPanel>

    <GlassPanel variant="flat" density="compact" class="table-panel">
      <div class="table-title">
        <div>
          <h2>Điểm thành phần</h2>
          <p>Cột điểm hiển thị động theo cấu hình đầu điểm của môn học. Tất cả điểm do hệ thống tổng hợp.</p>
        </div>
      </div>

      <div v-if="loading" class="loading-state">
        Đang tải dữ liệu bảng điểm...
      </div>

      <TableShell v-else density="compact">
        <table>
          <thead>
            <tr>
              <th>Sinh viên</th>
              <th v-for="col in gradeColumns" :key="col.code ?? col.Code">
                <span class="sortable-label">
                  {{ col.name ?? col.Name }}
                  ({{ ((col.weight ?? col.Weight) * 100).toFixed(0) }}%)
                  <ArrowUpDown :size="12" />
                </span>
              </th>
              <th>Điểm QT</th>
              <th>Giữa kỳ</th>
              <th>Cuối kỳ</th>
              <th>
                <span class="sortable-label total-label">
                  Tổng kết
                  <ArrowUpDown :size="12" />
                </span>
              </th>
              <th>Trạng thái</th>
              <th class="text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="sv in filteredStudents" :key="sv.studentId ?? sv.StudentId">
              <td>
                <div class="student-cell">
                  <span class="student-avatar">{{ (sv.studentName ?? sv.StudentName ?? '').split(' ').pop()?.[0] ?? '?' }}</span>
                  <span>
                    <strong>{{ sv.studentName ?? sv.StudentName }}</strong>
                    <small>{{ sv.studentId ?? sv.StudentId }}</small>
                  </span>
                </div>
              </td>

              <td v-for="col in gradeColumns" :key="(col.code ?? col.Code) + '-' + (sv.studentId ?? sv.StudentId)">
                <span class="grade-value">{{ formatGrade((sv.typeGrades ?? sv.TypeGrades)?.[col.code ?? col.Code]) }}</span>
              </td>

              <td><span class="grade-value">{{ formatGrade(sv.diemQuaTrinh ?? sv.DiemQuaTrinh) }}</span></td>
              <td><span class="grade-value">{{ formatGrade(sv.diemGiuaKy ?? sv.DiemGiuaKy) }}</span></td>
              <td><span class="grade-value">{{ formatGrade(sv.diemCuoiKy ?? sv.DiemCuoiKy) }}</span></td>

              <td>
                <strong
                  :class="[
                    'total-score',
                    (sv.trangThai ?? sv.TrangThai) === 'rot' ? 'failed' : (sv.trangThai ?? sv.TrangThai) === 'dat' ? 'passed' : '',
                  ]"
                >
                  {{ formatGrade(sv.gpaMonHoc ?? sv.GpaMonHoc) }}
                </strong>
              </td>

              <td>
                <GlassBadge :variant="trangThaiVariant(sv.trangThai ?? sv.TrangThai)">
                  {{ trangThaiLabel(sv.trangThai ?? sv.TrangThai) }}
                </GlassBadge>
              </td>

              <td>
                <div class="row-actions">
                  <GlassButton variant="secondary" size="sm" @click="openDetail(sv)">
                    <template #leading>
                      <Eye :size="14" />
                    </template>
                    Chi tiết
                  </GlassButton>

                  <!-- Đã khoá -->
                  <template v-if="sv.daKhoa ?? sv.DaKhoa">
                    <GlassBadge variant="neutral" class="lock-badge">
                      <Lock :size="11" />
                      Đã khoá
                    </GlassBadge>
                    <GlassButton variant="secondary" size="sm" @click="openUnlockModal(sv)">
                      <template #leading>
                        <Unlock :size="14" />
                      </template>
                      Yêu cầu mở khoá
                    </GlassButton>
                  </template>

                  <!-- Chưa khoá, có điểm → cho khoá -->
                  <template v-else-if="(sv.trangThai ?? sv.TrangThai) != null">
                    <GlassButton variant="primary" size="sm" @click="openLockConfirm(sv)">
                      <template #leading>
                        <Lock :size="14" />
                      </template>
                      Khoá điểm
                    </GlassButton>
                  </template>

                  <!-- Chưa khoá, chưa có điểm → disable -->
                  <template v-else>
                    <span class="lock-disabled" title="Chưa đủ điểm tổng kết để khoá bảng điểm">
                      <GlassButton variant="secondary" size="sm" disabled>
                        <template #leading>
                          <Lock :size="14" />
                        </template>
                        Khoá điểm
                      </GlassButton>
                      <Info :size="12" class="lock-info-icon" />
                    </span>
                  </template>
                </div>
              </td>
            </tr>
            <tr v-if="!loading && filteredStudents.length === 0">
              <td :colspan="gradeColumns.length + 7" class="empty-row">
                <AlertCircle :size="18" />
                Không tìm thấy sinh viên nào.
              </td>
            </tr>
          </tbody>
        </table>
      </TableShell>
    </GlassPanel>

    <GlassPanel variant="flat" density="compact" class="note-panel">
      <div class="note-icon">
        <AlertCircle :size="18" />
      </div>
      <div>
        <h3>Thông tin bảng điểm</h3>
        <ul>
          <li>Điểm tổng kết được hệ thống tính tự động dựa trên cấu hình trọng số môn học.</li>
          <li>Sau khi khoá điểm, cần gửi yêu cầu mở khoá và chờ BGH duyệt để có thể chỉnh sửa.</li>
          <li>Dấu "—" biểu thị chưa có dữ liệu. Số "0" là điểm thực tế.</li>
        </ul>
      </div>
    </GlassPanel>

    <!-- ═══ MODAL: Xem chi tiết điểm ═══ -->
    <Teleport to="body">
      <div v-if="showDetailModal" class="modal-overlay" @click.self="closeDetail" @keydown.esc="closeDetail">
        <GlassPanel variant="readable" density="comfortable" :clip="false" class="detail-modal">
          <div class="detail-header">
            <div>
              <h2>Chi tiết điểm</h2>
              <p>{{ detailStudentName }}</p>
            </div>
            <button class="lg-icon-button detail-close" @click="closeDetail" aria-label="Đóng">
              <X :size="16" />
            </button>
          </div>

          <div v-if="detailLoading" class="loading-state">
            Đang tải chi tiết điểm...
          </div>

          <template v-else-if="detailData">
            <div class="detail-types">
              <GlassPanel
                v-for="gt in (detailData.gradeTypes ?? detailData.GradeTypes ?? [])"
                :key="gt.code ?? gt.Code"
                variant="flat"
                density="compact"
                class="detail-type-card"
              >
                <div class="detail-type-header">
                  <strong>{{ gt.name ?? gt.Name }}</strong>
                  <div class="detail-type-meta">
                    <GlassBadge variant="primary">
                      Trọng số: {{ ((gt.weight ?? gt.Weight) * 100).toFixed(0) }}%
                    </GlassBadge>
                    <GlassBadge :variant="(gt.averageGrade ?? gt.AverageGrade) != null ? 'success' : 'neutral'">
                      TB: {{ formatGrade(gt.averageGrade ?? gt.AverageGrade) }}
                    </GlassBadge>
                  </div>
                </div>
                <div v-if="(gt.items ?? gt.Items ?? []).length > 0" class="detail-items">
                  <div
                    v-for="item in (gt.items ?? gt.Items)"
                    :key="item.itemId ?? item.ItemId"
                    class="detail-item"
                  >
                    <span class="detail-item-name">{{ item.itemName ?? item.ItemName }}</span>
                    <span :class="['detail-item-grade', (item.grade ?? item.Grade) === null ? 'no-data' : '']">
                      {{ formatGrade(item.grade ?? item.Grade) }}
                    </span>
                  </div>
                </div>
                <p v-else class="detail-no-items">Không có mục chi tiết cho loại điểm này.</p>
              </GlassPanel>
            </div>

            <div class="detail-summary">
              <div class="detail-summary-row">
                <span>Điểm quá trình</span>
                <strong>{{ formatGrade(detailData.diemQuaTrinh ?? detailData.DiemQuaTrinh) }}</strong>
              </div>
              <div class="detail-summary-row">
                <span>Giữa kỳ</span>
                <strong>{{ formatGrade(detailData.diemGiuaKy ?? detailData.DiemGiuaKy) }}</strong>
              </div>
              <div class="detail-summary-row">
                <span>Cuối kỳ</span>
                <strong>{{ formatGrade(detailData.diemCuoiKy ?? detailData.DiemCuoiKy) }}</strong>
              </div>
              <div class="detail-summary-row highlight">
                <span>Tổng kết</span>
                <strong>{{ formatGrade(detailData.gpaMonHoc ?? detailData.GpaMonHoc) }}</strong>
              </div>
              <div class="detail-summary-row">
                <span>Trạng thái</span>
                <GlassBadge :variant="trangThaiVariant(detailData.trangThai ?? detailData.TrangThai)">
                  {{ trangThaiLabel(detailData.trangThai ?? detailData.TrangThai) }}
                </GlassBadge>
              </div>
            </div>
          </template>
        </GlassPanel>
      </div>
    </Teleport>

    <!-- ═══ DIALOG: Xác nhận khoá điểm ═══ -->
    <ConfirmActionDialog
      v-model="showLockConfirm"
      title="Khoá bảng điểm"
      :message="`Sau khi khoá, điểm của ${lockTarget?.studentName ?? lockTarget?.StudentName ?? 'sinh viên'} sẽ không thể chỉnh sửa. Bạn sẽ cần gửi yêu cầu mở khoá và chờ BGH duyệt nếu muốn chỉnh sửa lại.`"
      confirm-label="Khoá điểm"
      variant="primary"
      :loading="lockLoading"
      @confirm="confirmLock"
    />

    <!-- ═══ MODAL: Yêu cầu mở khoá ═══ -->
    <Teleport to="body">
      <div v-if="showUnlockModal" class="modal-overlay" @click.self="closeUnlockModal" @keydown.esc="closeUnlockModal">
        <GlassPanel variant="readable" density="comfortable" :clip="false" class="unlock-modal">
          <div class="detail-header">
            <div>
              <h2>Yêu cầu mở khoá bảng điểm</h2>
              <p>{{ unlockTarget?.studentName ?? unlockTarget?.StudentName }}</p>
            </div>
            <button class="lg-icon-button detail-close" @click="closeUnlockModal" aria-label="Đóng">
              <X :size="16" />
            </button>
          </div>

          <template v-if="!unlockSuccess">
            <div class="unlock-form">
              <label class="unlock-label">
                Lý do yêu cầu mở khoá <span class="required">*</span>
              </label>
              <textarea
                v-model="unlockLyDo"
                class="unlock-textarea"
                rows="3"
                placeholder="Nhập lý do cần mở khoá bảng điểm..."
              />
            </div>
            <div class="unlock-actions">
              <GlassButton variant="secondary" @click="closeUnlockModal" :disabled="unlockLoading">
                Huỷ
              </GlassButton>
              <GlassButton
                variant="primary"
                :loading="unlockLoading"
                :disabled="!unlockLyDo.trim()"
                @click="submitUnlockRequest"
              >
                <template #leading>
                  <Unlock :size="14" />
                </template>
                Gửi yêu cầu
              </GlassButton>
            </div>
          </template>

          <div v-else class="unlock-success">
            <div class="unlock-success-icon">
              <AlertCircle :size="22" />
            </div>
            <h3>Đã gửi yêu cầu thành công</h3>
            <p>Yêu cầu mở khoá bảng điểm đã được gửi và đang chờ BGH duyệt. Bạn sẽ nhận được thông báo khi yêu cầu được xử lý.</p>
            <GlassButton variant="primary" @click="closeUnlockModal">
              Đóng
            </GlassButton>
          </div>
        </GlassPanel>
      </div>
    </Teleport>
  </div>
</template>

<style scoped>
.grades-page {
  display: flex;
  flex-direction: column;
  gap: 0.875rem;
  padding-bottom: 2.5rem;
  color: var(--text-body);
}

.grades-header,
.context-panel {
  display: flex;
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

.header-copy h1 {
  margin: 0;
  color: var(--text-heading);
  font-size: 1.45rem;
  line-height: 1.15;
  font-weight: 900;
}

.header-copy p,
.table-title p,
.student-cell small,
.note-panel li,
.summary-pill span {
  color: var(--text-muted);
}

.header-copy p,
.table-title p {
  margin: 0.25rem 0 0;
  font-size: 0.84rem;
}

.header-actions,
.filter-row,
.summary-strip,
.row-actions,
.sortable-label,
.student-cell,
.note-panel {
  display: flex;
  align-items: center;
}

.header-actions,
.filter-row,
.summary-strip {
  gap: 0.55rem;
  flex-wrap: wrap;
}

.row-actions {
  gap: 0.4rem;
  justify-content: flex-end;
  flex-wrap: wrap;
}

.context-panel {
  align-items: center;
}

.filter-row {
  min-width: min(24rem, 100%);
}

.search-field {
  display: inline-flex;
  align-items: center;
  min-height: 2.25rem;
  width: min(22rem, 100%);
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

.search-field:focus-within {
  border-color: var(--border-input-focus);
  background: var(--surface-input-focus);
  box-shadow: 0 0 0 3px var(--border-focus-ring);
}

.search-field input {
  min-width: 0;
  flex: 1;
  border: 0;
  outline: 0;
  background: transparent;
  color: var(--text-heading);
  font-size: 0.82rem;
  font-weight: 750;
}

.search-field input::placeholder {
  color: var(--text-placeholder);
}

.summary-strip {
  justify-content: flex-end;
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

.summary-pill.primary {
  background: var(--accent-primary-soft);
}

.summary-pill.success {
  background: var(--color-success-bg);
}

.summary-pill.danger {
  background: var(--color-danger-bg);
}

.table-panel {
  display: flex;
  flex-direction: column;
  gap: 0.8rem;
}

.table-title {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  border-bottom: 1px solid var(--border-card);
  padding-bottom: 0.75rem;
}

.table-title h2 {
  margin: 0;
  color: var(--text-heading);
  font-size: 1rem;
  font-weight: 900;
}

.sortable-label {
  gap: 0.35rem;
  white-space: nowrap;
}

.total-label {
  color: var(--text-link);
}

.student-cell {
  min-width: 13rem;
  gap: 0.65rem;
}

.student-avatar {
  display: inline-flex;
  width: 2rem;
  height: 2rem;
  flex: none;
  align-items: center;
  justify-content: center;
  border: 1px solid var(--border-card);
  border-radius: 999px;
  background: var(--surface-input);
  color: var(--text-link);
  font-size: 0.75rem;
  font-weight: 900;
}

.student-cell strong {
  display: block;
  color: var(--text-heading);
  font-size: 0.86rem;
}

.student-cell small {
  display: block;
  margin-top: 0.1rem;
  font-size: 0.72rem;
  font-weight: 750;
}

.grade-value,
.total-score {
  color: var(--text-heading);
  font-size: 0.88rem;
  font-weight: 900;
}

.total-score {
  display: inline-flex;
  min-width: 2.3rem;
  justify-content: center;
}

.total-score.passed {
  color: var(--color-success-text);
}

.total-score.failed {
  color: var(--color-danger-text);
}

.lock-badge {
  gap: 0.25rem;
}

.lock-disabled {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  cursor: help;
}

.lock-info-icon {
  color: var(--text-muted);
}

.empty-row {
  text-align: center;
  padding: 2rem !important;
  color: var(--text-muted);
  font-size: 0.85rem;
}

.empty-row svg {
  vertical-align: middle;
  margin-right: 0.35rem;
}

.loading-state {
  text-align: center;
  padding: 2rem;
  color: var(--text-muted);
  font-size: 0.85rem;
}

.note-panel {
  align-items: flex-start;
  gap: 0.75rem;
  background: var(--color-info-bg);
}

.note-icon {
  display: inline-flex;
  width: 2rem;
  height: 2rem;
  flex: none;
  align-items: center;
  justify-content: center;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  color: var(--color-info-text);
}

.note-panel h3 {
  margin: 0;
  color: var(--text-heading);
  font-size: 0.9rem;
  font-weight: 900;
}

.note-panel ul {
  margin: 0.35rem 0 0;
  padding-left: 1rem;
}

.note-panel li {
  font-size: 0.78rem;
  line-height: 1.55;
}

/* ── Modal overlay ── */
.modal-overlay {
  position: fixed;
  inset: 0;
  z-index: var(--z-modal, 9999);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  background: color-mix(in srgb, var(--surface-app) 58%, transparent);
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
}

/* ── Detail modal ── */
.detail-modal {
  width: 100%;
  max-width: 40rem;
  max-height: 80vh;
  overflow-y: auto;
  box-shadow: var(--lg-shadow-lg);
}

.detail-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1rem;
}

.detail-header h2 {
  margin: 0;
  color: var(--text-heading);
  font-size: 1.1rem;
  font-weight: 900;
}

.detail-header p {
  margin: 0.15rem 0 0;
  color: var(--text-muted);
  font-size: 0.84rem;
}

.detail-close {
  display: flex;
  width: 2rem;
  height: 2rem;
  flex: none;
  align-items: center;
  justify-content: center;
}

.detail-types {
  display: flex;
  flex-direction: column;
  gap: 0.65rem;
  margin-bottom: 1rem;
}

.detail-type-card {
  border: 1px solid var(--border-card);
}

.detail-type-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
  margin-bottom: 0.5rem;
  flex-wrap: wrap;
}

.detail-type-header strong {
  color: var(--text-heading);
  font-size: 0.88rem;
  font-weight: 900;
}

.detail-type-meta {
  display: flex;
  gap: 0.35rem;
}

.detail-items {
  display: flex;
  flex-direction: column;
  gap: 0.3rem;
}

.detail-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.35rem 0.5rem;
  border-radius: var(--radius-sm);
  background: var(--surface-input);
  font-size: 0.82rem;
}

.detail-item-name {
  color: var(--text-body);
  font-weight: 750;
}

.detail-item-grade {
  color: var(--text-heading);
  font-weight: 900;
}

.detail-item-grade.no-data {
  color: var(--text-muted);
}

.detail-no-items {
  margin: 0;
  color: var(--text-muted);
  font-size: 0.8rem;
  font-style: italic;
}

.detail-summary {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  border-top: 1px solid var(--border-card);
  padding-top: 0.75rem;
}

.detail-summary-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.3rem 0;
  font-size: 0.84rem;
}

.detail-summary-row span {
  color: var(--text-muted);
  font-weight: 750;
}

.detail-summary-row strong {
  color: var(--text-heading);
  font-weight: 900;
}

.detail-summary-row.highlight {
  padding: 0.5rem 0.5rem;
  border-radius: var(--radius-sm);
  background: var(--accent-primary-soft);
}

.detail-summary-row.highlight span {
  color: var(--text-link);
}

.detail-summary-row.highlight strong {
  font-size: 1.1rem;
  color: var(--text-link);
}

/* ── Unlock modal ── */
.unlock-modal {
  width: 100%;
  max-width: 28rem;
  box-shadow: var(--lg-shadow-lg);
}

.unlock-form {
  margin-bottom: 1rem;
}

.unlock-label {
  display: block;
  margin-bottom: 0.35rem;
  color: var(--text-heading);
  font-size: 0.84rem;
  font-weight: 850;
}

.unlock-label .required {
  color: var(--color-danger-text);
}

.unlock-textarea {
  width: 100%;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  color: var(--text-heading);
  padding: 0.6rem 0.7rem;
  font-size: 0.84rem;
  font-weight: 750;
  font-family: inherit;
  outline: none;
  resize: vertical;
  transition:
    border-color 0.2s ease,
    box-shadow 0.2s ease;
}

.unlock-textarea:focus {
  border-color: var(--border-input-focus);
  box-shadow: 0 0 0 3px var(--border-focus-ring);
}

.unlock-textarea::placeholder {
  color: var(--text-placeholder);
}

.unlock-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
}

.unlock-success {
  text-align: center;
  padding: 1rem 0;
}

.unlock-success-icon {
  display: inline-flex;
  width: 3rem;
  height: 3rem;
  align-items: center;
  justify-content: center;
  border: 1px solid var(--border-card);
  border-radius: 999px;
  background: var(--color-success-bg);
  color: var(--color-success-text);
  margin-bottom: 0.75rem;
}

.unlock-success h3 {
  margin: 0;
  color: var(--text-heading);
  font-size: 1rem;
  font-weight: 900;
}

.unlock-success p {
  margin: 0.5rem 0 1rem;
  color: var(--text-muted);
  font-size: 0.84rem;
}

@media (max-width: 1024px) {
  .grades-header,
  .context-panel,
  .table-title {
    flex-direction: column;
    align-items: stretch;
  }

  .summary-strip {
    justify-content: flex-start;
  }
}

@media (max-width: 640px) {
  .header-actions,
  .filter-row,
  .summary-strip {
    display: grid;
    grid-template-columns: 1fr;
  }

  .search-field,
  .summary-pill {
    width: 100%;
  }

  .row-actions {
    justify-content: flex-start;
  }
}
</style>
