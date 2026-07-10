<script setup>
import { computed, onMounted, ref } from 'vue'
import { AlertTriangle, BarChart3, CheckCircle2, Clock3, TrendingUp } from 'lucide-vue-next'

import EmptyState from '@/components/ui/EmptyState.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'
import TableShell from '@/components/ui/TableShell.vue'
import { applicationsApi } from '@/services/applicationsApi'
import { getStatusMeta } from '@/utils/statusLabels'

const icons = [BarChart3, Clock3, CheckCircle2, AlertTriangle]

const loading = ref(false)
const error = ref('')
const overview = ref({})
const typeRows = ref([])

const applicationReportCards = computed(() => [
  {
    label: 'Tổng đơn',
    value: overview.value.total ?? overview.value.tongSo ?? overview.value.totalApplications ?? 0,
    status: 'dang_xem_xet',
  },
  {
    label: 'Đang xử lý',
    value: overview.value.inProgress ?? overview.value.dangXuLy ?? overview.value.processing ?? 0,
    status: 'dang_xem_xet',
  },
  {
    label: 'Đã hoàn tất',
    value: overview.value.completed ?? overview.value.daHoanTat ?? overview.value.approved ?? 0,
    status: 'da_duyet',
  },
  {
    label: 'Quá hạn',
    value: overview.value.overdue ?? overview.value.quaHan ?? 0,
    status: 'tu_choi',
  },
])

function getApplicationTypeLabel(type) {
  return type || 'Không xác định'
}

function cardStatusMeta(status) {
  return getStatusMeta('applications', status)
}

function unwrapList(payload) {
  const data = payload?.data ?? payload?.Data ?? payload
  if (Array.isArray(data)) return data
  if (Array.isArray(data?.items)) return data.items
  if (Array.isArray(data?.Items)) return data.Items
  return []
}

function normalizeTypeRow(row) {
  const total = Number(row.total ?? row.tongSo ?? row.count ?? 0)
  const completed = Number(row.completed ?? row.daHoanTat ?? row.approved ?? 0)
  return {
    type: row.type ?? row.loaiDon ?? row.LoaiDon ?? row.tenLoaiDon ?? row.TenLoaiDon ?? 'Không xác định',
    total,
    completed,
    avgHours: Number(row.avgHours ?? row.gioXuLyTrungBinh ?? row.averageProcessingHours ?? 0),
  }
}

async function loadReports() {
  loading.value = true
  error.value = ''
  try {
    const [overviewResult, byTypeResult] = await Promise.all([
      applicationsApi.getApplicationReportOverview(),
      applicationsApi.getApplicationReportByType(),
    ])
    overview.value = overviewResult?.data ?? overviewResult?.Data ?? overviewResult ?? {}
    typeRows.value = unwrapList(byTypeResult).map(normalizeTypeRow)
  } catch (e) {
    error.value = e.message || 'Không tải được báo cáo đơn từ.'
    overview.value = {}
    typeRows.value = []
  } finally {
    loading.value = false
  }
}

onMounted(loadReports)
</script>

<template>
  <div class="application-reports-page mx-auto max-w-7xl space-y-5">
    <GlassPanel variant="flat" density="compact" class="page-header">
      <div class="header-copy">
        <p class="eyebrow">
          <BarChart3 :size="15" />
          Báo cáo đơn từ
        </p>
        <div>
          <h1>Báo cáo đơn từ</h1>
          <p>Tổng quan hiệu suất xử lý, cơ cấu loại đơn và cảnh báo SLA từ API báo cáo.</p>
        </div>
      </div>
      <GlassButton variant="secondary" :disabled="loading" @click="loadReports">Làm mới</GlassButton>
    </GlassPanel>

    <LoadingSkeleton v-if="loading" :lines="6" />

    <GlassPanel v-else-if="error" variant="flat" density="compact" class="error-panel">
      <div>
        <h2>Không tải được báo cáo</h2>
        <p>{{ error }}</p>
      </div>
      <GlassButton variant="secondary" @click="loadReports">Thử lại</GlassButton>
    </GlassPanel>

    <template v-else>
    <section class="summary-grid">
      <GlassPanel
        v-for="(card, index) in applicationReportCards"
        :key="card.label"
        variant="flat"
        density="compact"
        class="summary-card"
      >
        <span class="summary-icon">
          <component :is="icons[index]" :size="18" />
        </span>
        <span class="min-w-0">
          <p>{{ card.label }}</p>
          <strong>{{ card.value }}</strong>
        </span>
        <GlassBadge :variant="cardStatusMeta(card.status).variant">
          {{ cardStatusMeta(card.status).label }}
        </GlassBadge>
      </GlassPanel>
    </section>

    <section v-if="typeRows.length" class="report-grid">
      <GlassPanel variant="flat" density="compact" class="report-card">
        <div class="panel-heading">
          <div>
            <h2>Cơ cấu loại đơn</h2>
            <p>Tỷ trọng đơn đã tiếp nhận theo nhóm nghiệp vụ.</p>
          </div>
        </div>
        <div class="bar-list">
          <div v-for="row in typeRows" :key="row.type" class="bar-row">
            <span class="bar-label">{{ getApplicationTypeLabel(row.type) }}</span>
            <span class="bar-track">
            <span class="bar-fill" :style="{ width: `${Math.min(100, Math.round(row.total / Math.max(1, typeRows.reduce((max, item) => Math.max(max, item.total), 0)) * 100))}%` }" />
            </span>
            <strong>{{ row.total }}</strong>
          </div>
        </div>
      </GlassPanel>

      <GlassPanel variant="flat" density="compact" class="report-card">
        <div class="panel-heading">
          <div>
            <h2>Hiệu suất xử lý</h2>
            <p>Thời gian xử lý trung bình theo loại đơn.</p>
          </div>
        </div>
        <TableShell density="compact">
          <table>
            <thead>
              <tr>
                <th>Loại đơn</th>
                <th>Hoàn thành</th>
                <th>Giờ TB</th>
                <th>Tỷ lệ</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="row in typeRows" :key="row.type">
                <td><strong class="clamp-1">{{ getApplicationTypeLabel(row.type) }}</strong></td>
                <td>{{ row.completed }}</td>
                <td>{{ row.avgHours }} giờ</td>
                <td>
                  <GlassBadge variant="success">
                    {{ row.total ? Math.round((row.completed / row.total) * 100) : 0 }}%
                  </GlassBadge>
                </td>
              </tr>
            </tbody>
          </table>
        </TableShell>
      </GlassPanel>
    </section>

    <EmptyState
      v-else
      title="Chưa có dữ liệu báo cáo"
      description="API báo cáo hiện chưa trả về dòng thống kê theo loại đơn."
    />
    </template>
  </div>
</template>

<style scoped>
.application-reports-page {
  color: var(--text-body);
}

.page-header,
.summary-card,
.panel-heading,
.bar-row {
  display: flex;
  align-items: center;
}

.page-header,
.panel-heading {
  justify-content: space-between;
  gap: 1rem;
}

.header-copy,
.panel-heading > div {
  min-width: 0;
}

.eyebrow {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  width: fit-content;
  border: 1px solid var(--border-card);
  border-radius: 999px;
  background: var(--surface-input);
  color: var(--text-link);
  padding: 0.25rem 0.625rem;
  font-size: 0.71875rem;
  font-weight: 850;
}

h1,
h2 {
  margin: 0;
  color: var(--text-heading);
  font-weight: 900;
}

h1 {
  margin-top: 0.45rem;
  font-size: 1.5rem;
}

h2 {
  font-size: 1rem;
}

p {
  margin: 0.25rem 0 0;
  color: var(--text-muted);
  font-size: 0.84375rem;
}

.summary-grid,
.report-grid {
  display: grid;
  gap: 0.75rem;
  align-items: stretch;
}

.summary-grid {
  grid-template-columns: repeat(4, minmax(0, 1fr));
}

.report-grid {
  grid-template-columns: repeat(2, minmax(0, 1fr));
}

.summary-card {
  min-height: 7.5rem;
  align-items: flex-start;
  gap: 0.75rem;
}

.summary-card :deep(.lg-badge) {
  margin-left: auto;
}

.summary-icon {
  display: inline-flex;
  width: 2.25rem;
  height: 2.25rem;
  flex: none;
  align-items: center;
  justify-content: center;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-lg);
  background: var(--surface-input);
  color: var(--text-link);
}

.summary-card p {
  margin: 0;
  color: var(--text-muted);
  font-size: 0.75rem;
  font-weight: 750;
}

.summary-card strong {
  display: block;
  margin-top: 0.25rem;
  color: var(--text-heading);
  font-size: 1.45rem;
  font-weight: 950;
}

.report-card {
  display: grid;
  gap: 0.875rem;
}

.bar-list {
  display: grid;
  gap: 0.75rem;
}

.bar-row {
  min-height: 3rem;
  gap: 0.75rem;
}

.bar-label {
  min-width: 10rem;
  color: var(--text-heading);
  font-size: 0.84375rem;
  font-weight: 850;
}

.bar-track {
  height: 0.75rem;
  flex: 1;
  overflow: hidden;
  border: 1px solid var(--border-card);
  border-radius: 999px;
  background: var(--surface-input);
}

.bar-fill {
  display: block;
  height: 100%;
  border-radius: inherit;
  background: var(--text-link);
}

table {
  width: 100%;
  border-collapse: collapse;
  table-layout: fixed;
}

thead tr {
  border-bottom: 1px solid var(--border-table);
  background: var(--surface-table-header);
}

tbody tr {
  border-bottom: 1px solid var(--border-table);
  background: var(--surface-table);
}

tbody tr:hover {
  background: var(--surface-table-row-hover);
}

th,
td {
  padding: 0.625rem 0.75rem;
}

td strong {
  color: var(--text-heading);
  font-weight: 850;
}

.clamp-1 {
  display: -webkit-box;
  overflow: hidden;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 1;
}

@media (max-width: 1060px) {
  .summary-grid,
  .report-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 680px) {
  .page-header {
    flex-direction: column;
    align-items: stretch;
  }

  .summary-grid,
  .report-grid {
    grid-template-columns: 1fr;
  }

  .bar-row {
    display: grid;
    grid-template-columns: 1fr auto;
  }

  .bar-track {
    grid-column: 1 / -1;
  }
}
</style>
