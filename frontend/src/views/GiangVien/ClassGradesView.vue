<script setup>
import { computed, ref } from 'vue'
import {
  AlertCircle,
  ArrowUpDown,
  CheckCircle,
  Download,
  Edit3,
  Filter,
  Save,
  Search,
  Users,
} from 'lucide-vue-next'

import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import TableShell from '@/components/ui/TableShell.vue'

const gradesData = ref([
  { id: 'SV16001', name: 'Nguyễn Văn A', assignment: 8.5, exam: 7.5, total: 7.9, isEditing: false },
  { id: 'SV16002', name: 'Trần Thị B', assignment: 7.0, exam: 8.0, total: 7.6, isEditing: false },
  { id: 'SV16003', name: 'Lê Hoàng C', assignment: 9.5, exam: 9.0, total: 9.2, isEditing: false },
  { id: 'SV16004', name: 'Phạm Minh D', assignment: 5.0, exam: 4.5, total: 4.7, isEditing: false },
])

const gradeSummary = computed(() => {
  const entered = gradesData.value.filter((sv) => sv.assignment !== null && sv.exam !== null).length
  const passed = gradesData.value.filter((sv) => Number(sv.total) >= 5).length
  const failed = gradesData.value.filter((sv) => Number(sv.total) < 5).length
  const average =
    gradesData.value.reduce((sum, sv) => sum + Number(sv.total), 0) / gradesData.value.length

  return [
    { label: 'Sinh viên', value: gradesData.value.length, tone: 'primary' },
    { label: 'Đã nhập', value: entered, tone: 'success' },
    { label: 'Đạt', value: passed, tone: 'success' },
    { label: 'Rớt', value: failed, tone: 'danger' },
    { label: 'TB lớp', value: average.toFixed(1), tone: 'neutral' },
  ]
})

function toggleEdit(sv) {
  sv.isEditing = !sv.isEditing
}

function calculateTotal(sv) {
  // Mock formula: 40% assignment, 60% exam
  sv.total = (sv.assignment * 0.4 + sv.exam * 0.6).toFixed(1)
}

function gradeStatusVariant(total) {
  return Number(total) >= 5 ? 'success' : 'danger'
}

function gradeStatusLabel(total) {
  return Number(total) >= 5 ? 'Đạt' : 'Rớt'
}
</script>

<template>
  <div class="grades-page lg-page-enter">
    <GlassPanel variant="flat" density="compact" class="grades-header">
      <div class="header-copy">
        <div class="eyebrow">
          <Users :size="15" />
          SE1601 · Lập trình Java
        </div>
        <div>
          <h1>Bảng điểm lớp học</h1>
          <p>Quản lý điểm thành phần và điểm thi kết thúc môn của lớp SE1601.</p>
        </div>
      </div>

      <div class="header-actions">
        <GlassButton variant="secondary" size="sm">
          <template #leading>
            <Download :size="16" />
          </template>
          Xuất bảng điểm
        </GlassButton>
        <GlassButton variant="primary" size="sm">
          <template #leading>
            <Save :size="16" />
          </template>
          Lưu toàn bộ
        </GlassButton>
      </div>
    </GlassPanel>

    <GlassPanel variant="flat" density="compact" class="context-panel">
      <div class="filter-row">
        <label class="search-field">
          <Search :size="16" />
          <input type="text" placeholder="Tìm sinh viên bằng tên hoặc MSSV..." />
        </label>
        <GlassButton variant="secondary" size="sm">
          <template #leading>
            <Filter :size="15" />
          </template>
          Lọc nâng cao
        </GlassButton>
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
          <p>Điểm tổng kết tự tính theo trọng số: Assignment 40%, điểm thi 60%.</p>
        </div>
        <GlassBadge variant="info">Học kỳ Spring 2026</GlassBadge>
      </div>

      <TableShell density="compact">
        <table>
          <thead>
            <tr>
              <th>Sinh viên</th>
              <th>
                <span class="sortable-label">
                  Assignment (40%)
                  <ArrowUpDown :size="12" />
                </span>
              </th>
              <th>
                <span class="sortable-label">
                  Điểm thi (60%)
                  <ArrowUpDown :size="12" />
                </span>
              </th>
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
            <tr v-for="sv in gradesData" :key="sv.id">
              <td>
                <div class="student-cell">
                  <span class="student-avatar">{{ sv.name.split(' ').pop()[0] }}</span>
                  <span>
                    <strong>{{ sv.name }}</strong>
                    <small>{{ sv.id }}</small>
                  </span>
                </div>
              </td>

              <td>
                <input
                  v-if="sv.isEditing"
                  v-model="sv.assignment"
                  type="number"
                  max="10"
                  min="0"
                  step="0.1"
                  class="grade-input"
                  @input="calculateTotal(sv)"
                />
                <span v-else class="grade-value">{{ sv.assignment }}</span>
              </td>

              <td>
                <input
                  v-if="sv.isEditing"
                  v-model="sv.exam"
                  type="number"
                  max="10"
                  min="0"
                  step="0.1"
                  class="grade-input"
                  @input="calculateTotal(sv)"
                />
                <span v-else class="grade-value">{{ sv.exam }}</span>
              </td>

              <td>
                <strong :class="['total-score', Number(sv.total) < 5 ? 'failed' : 'passed']">
                  {{ sv.total }}
                </strong>
              </td>

              <td>
                <GlassBadge :variant="gradeStatusVariant(sv.total)">
                  {{ gradeStatusLabel(sv.total) }}
                </GlassBadge>
              </td>

              <td>
                <div class="row-actions">
                  <GlassButton
                    :variant="sv.isEditing ? 'success' : 'secondary'"
                    size="sm"
                    @click="toggleEdit(sv)"
                  >
                    <template #leading>
                      <component :is="sv.isEditing ? CheckCircle : Edit3" :size="14" />
                    </template>
                    {{ sv.isEditing ? 'Xong' : 'Sửa điểm' }}
                  </GlassButton>
                </div>
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
        <h3>Hướng dẫn nhập điểm</h3>
        <ul>
          <li>Điểm tổng kết được tính tự động dựa trên trọng số môn học.</li>
          <li>Sau khi sửa điểm, hãy nhấn "Lưu toàn bộ" để cập nhật vào hệ thống chính thức.</li>
          <li>Các trường hợp điểm dưới 5.0 sẽ được đánh dấu rớt tự động.</li>
        </ul>
      </div>
    </GlassPanel>
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

.grade-input {
  width: 4.5rem;
  min-height: 2rem;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-sm);
  background: var(--surface-input);
  color: var(--text-heading);
  padding: 0 0.55rem;
  outline: none;
  font-size: 0.84rem;
  font-weight: 850;
  transition:
    border-color 0.2s ease,
    background 0.2s ease,
    box-shadow 0.2s ease;
}

.grade-input:focus {
  border-color: var(--border-input-focus);
  background: var(--surface-input-focus);
  box-shadow: 0 0 0 3px var(--border-focus-ring);
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

.row-actions {
  justify-content: flex-end;
}

.note-panel {
  align-items: flex-start;
  gap: 0.75rem;
  background: var(--color-warning-bg);
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
  color: var(--color-warning-text);
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
