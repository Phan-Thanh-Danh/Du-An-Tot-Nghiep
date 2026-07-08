<script setup>
import { computed, ref, onMounted } from 'vue'
import {
  AlertCircle,
  ArrowUpDown,
  Calendar,
  CheckCircle,
  CheckSquare,
  Clock,
  Download,
  Filter,
  History,
  Loader2,
  Search,
  XCircle,
  XSquare,
} from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import TableShell from '@/components/ui/TableShell.vue'
import { teacherApi } from '@/services/teacherApi'

const loading = ref(false)
const error = ref('')
const history = ref([])

const historyStats = computed(() => [
  { label: 'Tổng yêu cầu', value: history.value.length, variant: 'neutral' },
  { label: 'Đã duyệt', value: history.value.filter(item => (item.ketQua || item.result) === 'Approved').length, variant: 'success' },
  { label: 'Từ chối', value: history.value.filter(item => (item.ketQua || item.result) === 'Rejected').length, variant: 'danger' },
  { label: 'Tháng này', value: 15, variant: 'info' },
])

const typeStats = computed(() => [
  { label: 'Xin vắng học', value: '45%' },
  { label: 'Phúc khảo', value: '30%' },
])

async function loadHistory() {
  loading.value = true
  error.value = ''
  try {
    const data = await teacherApi.getTeacherRequestHistory()
    history.value = Array.isArray(data) ? data : (data?.items ?? data?.data ?? [])
  } catch (e) {
    error.value = e?.message || 'Không thể tải lịch sử yêu cầu.'
    history.value = []
  } finally {
    loading.value = false
  }
}

const getStatusText = (status) => {
  return (status || '') === 'Approved' ? 'Đã duyệt' : 'Từ chối'
}

const getStatusVariant = (status) => {
  return (status || '') === 'Approved' ? 'success' : 'danger'
}

const getStatusIcon = (status) => {
  return (status || '') === 'Approved' ? CheckCircle : XCircle
}

onMounted(() => { loadHistory() })
</script>

<template>
  <div class="requests-history-page">
    <GlassPanel variant="soft" density="compact" class="page-header" :clip="false">
      <div class="header-main">
        <span class="header-icon">
          <History :size="20" />
        </span>
        <div class="min-w-0">
          <div class="eyebrow">Request archive</div>
          <h1 class="page-title">Lịch sử yêu cầu</h1>
          <p class="page-subtitle">
            Tra cứu các đơn từ đã xử lý, kết quả phản hồi và thời gian hoàn tất.
          </p>
        </div>
      </div>

      <div class="header-actions">
        <GlassButton size="sm" variant="secondary">
          <template #leading>
            <Download :size="14" />
          </template>
          Xuất báo cáo
        </GlassButton>
      </div>
    </GlassPanel>

    <GlassPanel variant="surface" density="compact" class="context-bar" :clip="false">
      <div class="mini-stats">
        <div v-for="item in historyStats" :key="item.label" class="mini-stat">
          <span class="stat-label">{{ item.label }}</span>
          <div class="stat-value-line">
            <strong>{{ item.value }}</strong>
            <GlassBadge :variant="item.variant" size="sm">{{ item.label }}</GlassBadge>
          </div>
        </div>
      </div>

      <div class="filters">
        <label class="select-field">
          <Filter :size="15" />
          <select>
            <option>Tất cả trạng thái</option>
            <option>Đã phê duyệt</option>
            <option>Đã từ chối</option>
          </select>
        </label>
        <label class="search-field">
          <Search :size="15" />
          <input type="text" placeholder="Tìm sinh viên hoặc loại đơn..." />
        </label>
      </div>
    </GlassPanel>

    <div class="history-layout">
      <GlassPanel variant="surface" density="compact" class="insight-panel">
        <template #header>
          <div class="panel-heading">
            <div>
              <h2>Tổng quan tháng</h2>
              <p>15 đơn đã ghi nhận</p>
            </div>
            <Calendar :size="18" />
          </div>
        </template>

        <div class="approval-summary">
          <div class="summary-row">
            <span class="summary-icon success">
              <CheckSquare :size="16" />
            </span>
            <span>Đã duyệt</span>
            <strong>12</strong>
          </div>
          <div class="summary-row">
            <span class="summary-icon danger">
              <XSquare :size="16" />
            </span>
            <span>Từ chối</span>
            <strong>3</strong>
          </div>
        </div>

        <div class="type-stats">
          <h3>Loại đơn phổ biến</h3>
          <div v-for="item in typeStats" :key="item.label" class="type-row">
            <div class="type-label">
              <span>{{ item.label }}</span>
              <strong>{{ item.value }}</strong>
            </div>
            <div class="progress-line">
              <span :style="{ width: item.value }" />
            </div>
          </div>
        </div>
      </GlassPanel>

      <GlassPanel variant="surface" density="none" class="history-table-panel">
        <template #header>
          <div class="panel-heading">
            <div>
              <h2>Bảng lịch sử</h2>
              <p>Hiển thị 1-{{ history.length }} trong số 15 kết quả</p>
            </div>
            <GlassBadge variant="neutral" size="sm">Archive</GlassBadge>
          </div>
        </template>

        <div v-if="loading" class="flex items-center justify-center py-12">
          <Loader2 :size="20" class="animate-spin text-muted" />
          <span class="ml-2 text-xs font-semibold text-muted">Đang tải lịch sử...</span>
        </div>

        <div v-else-if="error" class="flex flex-col items-center py-12">
          <AlertCircle :size="28" class="text-rose-400 mb-2" />
          <p class="text-xs font-semibold text-muted">{{ error }}</p>
        </div>

        <TableShell v-else density="compact" sticky-header>
          <table>
            <thead>
              <tr>
                <th>
                  <span class="sortable-heading">
                    Ngày xử lý
                    <ArrowUpDown :size="13" />
                  </span>
                </th>
                <th>Sinh viên</th>
                <th>Loại đơn</th>
                <th class="text-right">Kết quả</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in history" :key="item.id || item.maYeuCau">
                <td>
                  <div class="date-cell">
                    <strong>{{ item.ngayXuLy || item.date || '--' }}</strong>
                    <span>
                      <Clock :size="11" />
                      {{ item.gioXuLy || item.time || '--' }}
                    </span>
                  </div>
                </td>
                <td>
                  <div class="student-cell">
                    <span class="avatar">{{ (item.hoTen || item.student || '').split(' ').pop()[0] || '?' }}</span>
                    <strong>{{ item.hoTen || item.student }}</strong>
                  </div>
                </td>
                <td>
                  <span class="type-chip">{{ item.loaiYeuCau || item.type }}</span>
                </td>
                <td class="text-right">
                  <GlassBadge :variant="getStatusVariant(item.ketQua || item.result)" size="sm">
                    <component :is="getStatusIcon(item.ketQua || item.result)" :size="12" />
                    {{ getStatusText(item.ketQua || item.result) }}
                  </GlassBadge>
                </td>
              </tr>
            </tbody>
          </table>
        </TableShell>

        <div class="table-footer">
          <span>Hiển thị 1-{{ history.length }} trong số 15 kết quả</span>
          <div class="pager">
            <button type="button">Trước</button>
            <button type="button" class="is-active">1</button>
            <button type="button">2</button>
            <button type="button">Sau</button>
          </div>
        </div>
      </GlassPanel>
    </div>
  </div>
</template>

<style scoped>
.requests-history-page {
  display: grid;
  gap: 1rem;
  padding-bottom: 2rem;
  color: var(--text-body);
}

.page-header,
.context-bar,
.header-main,
.header-actions,
.filters,
.panel-heading,
.stat-value-line,
.summary-row,
.type-label,
.sortable-heading,
.date-cell span,
.student-cell,
.table-footer,
.pager {
  display: flex;
  align-items: center;
}

.page-header,
.context-bar,
.panel-heading,
.table-footer {
  justify-content: space-between;
  gap: 1rem;
}

.header-main {
  gap: 0.875rem;
}

.header-icon,
.summary-icon,
.avatar {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex: 0 0 auto;
  border: 1px solid var(--border-card);
  background: var(--surface-input);
}

.header-icon {
  width: 2.5rem;
  height: 2.5rem;
  border-radius: var(--radius-lg);
  color: var(--text-link);
}

.eyebrow,
.page-subtitle,
.panel-heading p,
.stat-label,
.date-cell span,
.table-footer,
.type-stats h3 {
  color: var(--text-muted);
}

.eyebrow {
  font-size: 0.6875rem;
  font-weight: 800;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

.page-title {
  margin: 0;
  color: var(--text-heading);
  font-size: clamp(1.125rem, 2vw, 1.5rem);
  font-weight: 900;
}

.page-subtitle {
  margin: 0.25rem 0 0;
  max-width: 42rem;
  font-size: 0.875rem;
  line-height: 1.5;
}

.filters {
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 0.625rem;
}

.context-bar {
  align-items: stretch;
}

.mini-stats {
  display: grid;
  grid-template-columns: repeat(4, minmax(7rem, 1fr));
  gap: 0.625rem;
  flex: 1;
}

.mini-stat {
  min-width: 0;
  border-radius: var(--radius-lg);
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  padding: 0.625rem 0.75rem;
}

.stat-label {
  display: block;
  font-size: 0.6875rem;
  font-weight: 700;
}

.stat-value-line {
  justify-content: space-between;
  gap: 0.5rem;
  margin-top: 0.375rem;
}

.stat-value-line strong {
  color: var(--text-heading);
  font-size: 1.125rem;
  font-weight: 900;
}

.search-field,
.select-field {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  min-width: min(18rem, 100%);
  height: 2.25rem;
  border-radius: var(--radius-md);
  border: 1px solid var(--border-input);
  background: var(--surface-input);
  color: var(--text-muted);
  padding: 0 0.75rem;
}

.select-field {
  min-width: 11rem;
}

.search-field input,
.select-field select {
  width: 100%;
  min-width: 0;
  border: 0;
  outline: 0;
  background: transparent;
  color: var(--text-body);
  font-size: 0.8125rem;
  font-weight: 600;
}

.select-field select {
  appearance: none;
  cursor: pointer;
}

.search-field input::placeholder {
  color: var(--text-placeholder);
}

.search-field:focus-within,
.select-field:focus-within {
  border-color: var(--border-input-focus);
  box-shadow: 0 0 0 3px var(--border-focus-ring);
}

.history-layout {
  display: grid;
  grid-template-columns: minmax(16rem, 0.35fr) minmax(0, 1fr);
  gap: 1rem;
  align-items: start;
}

.panel-heading h2 {
  margin: 0;
  color: var(--text-heading);
  font-size: 0.9375rem;
  font-weight: 900;
}

.panel-heading p {
  margin: 0.125rem 0 0;
  font-size: 0.75rem;
  font-weight: 600;
}

.approval-summary {
  display: grid;
  gap: 0.625rem;
}

.summary-row {
  gap: 0.625rem;
  border-radius: var(--radius-lg);
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  padding: 0.75rem;
}

.summary-row span:nth-child(2) {
  flex: 1;
  color: var(--text-body);
  font-size: 0.8125rem;
  font-weight: 800;
}

.summary-row strong {
  color: var(--text-heading);
  font-size: 1rem;
  font-weight: 900;
}

.summary-icon {
  width: 2rem;
  height: 2rem;
  border-radius: var(--radius-md);
}

.summary-icon.success {
  color: var(--color-success-text);
}

.summary-icon.danger {
  color: var(--color-danger-text);
}

.type-stats {
  display: grid;
  gap: 0.75rem;
  margin-top: 1rem;
}

.type-stats h3 {
  margin: 0;
  font-size: 0.6875rem;
  font-weight: 900;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

.type-row {
  display: grid;
  gap: 0.375rem;
}

.type-label {
  justify-content: space-between;
  gap: 0.75rem;
  color: var(--text-body);
  font-size: 0.8125rem;
  font-weight: 700;
}

.type-label strong {
  color: var(--text-heading);
}

.progress-line {
  height: 0.5rem;
  overflow: hidden;
  border-radius: 999px;
  border: 1px solid var(--border-card);
  background: var(--surface-input);
}

.progress-line span {
  display: block;
  height: 100%;
  border-radius: inherit;
  background: var(--text-link);
}

table {
  border-collapse: collapse;
  width: 100%;
}

th {
  background: var(--surface-input);
  text-align: left;
  white-space: nowrap;
}

td {
  border-top: 1px solid var(--border-card);
  vertical-align: middle;
}

tbody tr:hover {
  background: var(--surface-input);
}

.sortable-heading {
  gap: 0.375rem;
}

.date-cell {
  display: grid;
  gap: 0.1875rem;
}

.date-cell strong,
.student-cell strong {
  color: var(--text-heading);
  font-size: 0.875rem;
  font-weight: 900;
}

.date-cell span {
  gap: 0.25rem;
  font-size: 0.75rem;
  font-weight: 650;
}

.student-cell {
  gap: 0.625rem;
  min-width: 12rem;
}

.avatar {
  width: 2rem;
  height: 2rem;
  border-radius: var(--radius-md);
  color: var(--text-link);
  font-size: 0.75rem;
  font-weight: 900;
}

.type-chip {
  display: inline-flex;
  width: max-content;
  border-radius: var(--radius-md);
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-body);
  padding: 0.25rem 0.5rem;
  font-size: 0.75rem;
  font-weight: 800;
}

.text-right {
  text-align: right;
}

.table-footer {
  border-top: 1px solid var(--border-card);
  padding: 0.75rem 1rem;
  font-size: 0.75rem;
  font-weight: 700;
}

.pager {
  gap: 0.25rem;
}

.pager button {
  min-height: 2rem;
  border-radius: var(--radius-md);
  border: 1px solid var(--border-card);
  background: var(--surface-card);
  color: var(--text-body);
  cursor: pointer;
  padding: 0 0.625rem;
  font-size: 0.75rem;
  font-weight: 800;
}

.pager button.is-active {
  border-color: var(--border-input-focus);
  color: var(--text-link);
}

@media (max-width: 1100px) {
  .page-header,
  .context-bar {
    align-items: flex-start;
    flex-direction: column;
  }

  .mini-stats {
    width: 100%;
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .filters,
  .search-field {
    width: 100%;
  }

  .history-layout {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 640px) {
  .mini-stats {
    grid-template-columns: 1fr;
  }

  .select-field,
  .table-footer,
  .pager {
    width: 100%;
  }

  .table-footer {
    align-items: flex-start;
    flex-direction: column;
  }
}
</style>
