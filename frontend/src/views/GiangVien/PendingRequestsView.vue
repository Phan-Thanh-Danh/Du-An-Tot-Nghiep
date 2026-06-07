<script setup>
import { computed, ref } from 'vue'
import { usePopupStore } from '@/stores/popup'
import {
  AlertCircle,
  CheckCircle,
  Clock,
  FileQuestion,
  FileText,
  Mail,
  Search,
  User,
  XCircle,
} from 'lucide-vue-next'
import EmptyState from '@/components/ui/EmptyState.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'

const popupStore = usePopupStore()

const requests = ref([
  { id: 1, student: 'Nguyễn Văn A', type: 'Xin vắng học', content: 'Em xin nghỉ buổi học ngày 20/05 do có việc gia đình.', status: 'Pending', time: '8:00 AM', tag: 'Vắng học', color: 'blue' },
  { id: 2, student: 'Trần Thị B', type: 'Phúc khảo điểm', content: 'Em xin phúc khảo lại điểm thi Lab 2, em thấy mình làm đúng hết.', status: 'Pending', time: '10:30 AM', tag: 'Khảo thí', color: 'cyan' },
  { id: 3, student: 'Lê Hoàng C', type: 'Đơn xin học bù', content: 'Em muốn xin học bù lớp SE1601 sang SE1602 vào thứ 5.', status: 'Pending', time: 'Hôm qua', tag: 'Học vụ', color: 'emerald' },
])

const selectedReq = ref(null)

const requestStats = computed(() => [
  { label: 'Tổng yêu cầu', value: requests.value.length, variant: 'neutral' },
  { label: 'Đang chờ', value: requests.value.filter(req => req.status === 'Pending').length, variant: 'warning' },
  { label: 'Hôm nay', value: 2, variant: 'info' },
  { label: 'Cần phản hồi', value: requests.value.length, variant: 'primary' },
])

function selectRequest(req) {
  selectedReq.value = req
}

function processRequest(action) {
  popupStore.success('Đã xử lý đơn', `Đã ${action.toLowerCase()} đơn của ${selectedReq.value.student}`)
  selectedReq.value = null
}

const getTagVariant = (color) => {
  const colors = {
    blue: 'primary',
    cyan: 'info',
    emerald: 'success',
  }
  return colors[color] || 'neutral'
}
</script>

<template>
  <div class="pending-requests-page">
    <GlassPanel variant="soft" density="compact" class="page-header" :clip="false">
      <div class="header-main">
        <span class="header-icon">
          <FileText :size="20" />
        </span>
        <div class="min-w-0">
          <div class="eyebrow">Request inbox</div>
          <h1 class="page-title">Yêu cầu đang chờ</h1>
          <p class="page-subtitle">
            Theo dõi các đơn từ mới, xem chi tiết và phản hồi theo quy trình học vụ.
          </p>
        </div>
      </div>

      <div class="header-actions">
        <GlassBadge variant="warning" size="md">
          <AlertCircle :size="13" />
          {{ requests.length }} đơn đang chờ
        </GlassBadge>
      </div>
    </GlassPanel>

    <GlassPanel variant="surface" density="compact" class="context-bar" :clip="false">
      <div class="mini-stats">
        <div v-for="item in requestStats" :key="item.label" class="mini-stat">
          <span class="stat-label">{{ item.label }}</span>
          <div class="stat-value-line">
            <strong>{{ item.value }}</strong>
            <GlassBadge :variant="item.variant" size="sm">{{ item.label }}</GlassBadge>
          </div>
        </div>
      </div>

      <label class="search-field">
        <Search :size="15" />
        <input type="text" placeholder="Tìm kiếm đơn từ..." />
      </label>
    </GlassPanel>

    <div class="requests-workspace">
      <GlassPanel variant="surface" density="none" class="request-list-panel">
        <template #header>
          <div class="panel-heading">
            <div>
              <h2>Yêu cầu mới nhất</h2>
              <p>{{ requests.length }} yêu cầu cần theo dõi</p>
            </div>
            <GlassBadge variant="warning" size="sm">Pending</GlassBadge>
          </div>
        </template>

        <div v-if="requests.length" class="request-list">
          <button
            v-for="req in requests"
            :key="req.id"
            type="button"
            :class="['request-row', selectedReq?.id === req.id && 'is-selected']"
            @click="selectRequest(req)"
          >
            <span class="request-icon">
              <FileQuestion :size="17" />
            </span>
            <span class="request-content">
              <span class="request-topline">
                <strong>{{ req.type }}</strong>
                <span class="time-chip">
                  <Clock :size="12" />
                  {{ req.time }}
                </span>
              </span>
              <span class="student-line">
                <User :size="12" />
                Sinh viên: <b>{{ req.student }}</b>
              </span>
              <span class="request-text">"{{ req.content }}"</span>
              <span class="row-meta">
                <GlassBadge :variant="getTagVariant(req.color)" size="sm">{{ req.tag }}</GlassBadge>
                <GlassBadge variant="warning" size="sm">Đang chờ</GlassBadge>
              </span>
            </span>
          </button>
        </div>

        <EmptyState
          v-else
          title="Không có yêu cầu đang chờ"
          description="Các đơn từ cần xử lý sẽ xuất hiện tại đây."
        >
          <template #icon>
            <FileQuestion :size="22" />
          </template>
        </EmptyState>
      </GlassPanel>

      <GlassPanel v-if="selectedReq" variant="readable" density="compact" class="detail-panel">
        <template #header>
          <div class="panel-heading">
            <div>
              <h2>Chi tiết yêu cầu</h2>
              <p>Mã yêu cầu #GV-REQ-{{ selectedReq.id }}</p>
            </div>
            <GlassBadge :variant="getTagVariant(selectedReq.color)" size="sm">
              {{ selectedReq.tag }}
            </GlassBadge>
          </div>
        </template>

        <div class="detail-summary">
          <span class="detail-icon">
            <FileText :size="22" />
          </span>
          <div>
            <h3>{{ selectedReq.type }}</h3>
            <p>
              <User :size="13" />
              {{ selectedReq.student }}
            </p>
          </div>
        </div>

        <div class="content-box">
          <Mail :size="15" />
          <div>
            <span>Nội dung chi tiết</span>
            <p>"{{ selectedReq.content }}"</p>
          </div>
        </div>

        <div class="timeline">
          <div class="timeline-item">
            <span />
            <div>
              <strong>Đã gửi yêu cầu</strong>
              <p>{{ selectedReq.time }} · Sinh viên gửi đơn lên hệ thống</p>
            </div>
          </div>
          <div class="timeline-item is-current">
            <span />
            <div>
              <strong>Đang chờ phản hồi</strong>
              <p>Giảng viên xem nội dung và cập nhật hướng xử lý.</p>
            </div>
          </div>
        </div>

        <div class="detail-actions">
          <GlassButton variant="success" size="sm" block @click="processRequest('Chấp nhận')">
            <template #leading>
              <CheckCircle :size="14" />
            </template>
            Phê duyệt đơn
          </GlassButton>
          <GlassButton variant="danger" size="sm" block @click="processRequest('Từ chối')">
            <template #leading>
              <XCircle :size="14" />
            </template>
            Từ chối
          </GlassButton>
          <GlassButton variant="ghost" size="sm" block @click="selectedReq = null">Đóng cửa sổ</GlassButton>
        </div>
      </GlassPanel>

      <GlassPanel v-else variant="surface" density="compact" class="detail-panel empty-detail">
        <div class="empty-detail-inner">
          <span class="detail-icon">
            <FileQuestion :size="24" />
          </span>
          <h2>Chưa chọn đơn nào</h2>
          <p>Chọn một đơn từ danh sách để xem chi tiết, timeline và thao tác xử lý.</p>
        </div>
      </GlassPanel>
    </div>
  </div>
</template>

<style scoped>
.pending-requests-page {
  display: grid;
  gap: 1rem;
  padding-bottom: 2rem;
  color: var(--text-body);
}

.page-header,
.context-bar,
.header-main,
.header-actions,
.panel-heading,
.stat-value-line,
.request-row,
.request-topline,
.student-line,
.row-meta,
.detail-summary,
.detail-summary p,
.content-box,
.timeline-item {
  display: flex;
  align-items: center;
}

.page-header,
.context-bar,
.panel-heading {
  justify-content: space-between;
  gap: 1rem;
}

.header-main {
  gap: 0.875rem;
}

.header-icon,
.request-icon,
.detail-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex: 0 0 auto;
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-link);
}

.header-icon {
  width: 2.5rem;
  height: 2.5rem;
  border-radius: var(--radius-lg);
}

.eyebrow,
.page-subtitle,
.panel-heading p,
.stat-label,
.time-chip,
.student-line,
.timeline-item p,
.empty-detail-inner p {
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

.header-actions {
  justify-content: flex-end;
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

.search-field {
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

.search-field input {
  width: 100%;
  min-width: 0;
  border: 0;
  outline: 0;
  background: transparent;
  color: var(--text-body);
  font-size: 0.8125rem;
  font-weight: 600;
}

.search-field input::placeholder {
  color: var(--text-placeholder);
}

.search-field:focus-within {
  border-color: var(--border-input-focus);
  box-shadow: 0 0 0 3px var(--border-focus-ring);
}

.requests-workspace {
  display: grid;
  grid-template-columns: minmax(0, 1.35fr) minmax(20rem, 0.65fr);
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

.request-list {
  display: grid;
  gap: 0.625rem;
  padding: 0.75rem;
}

.request-row {
  align-items: flex-start;
  gap: 0.75rem;
  width: 100%;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-lg);
  background: var(--surface-card);
  color: var(--text-body);
  cursor: pointer;
  padding: 0.75rem;
  text-align: left;
  transition: background 160ms ease, border-color 160ms ease, transform 160ms ease;
}

.request-row:hover,
.request-row.is-selected {
  border-color: var(--border-input-focus);
  background: var(--surface-input);
}

.request-row.is-selected {
  transform: translateY(-1px);
}

.request-icon,
.detail-icon {
  width: 2.25rem;
  height: 2.25rem;
  border-radius: var(--radius-md);
}

.request-content {
  min-width: 0;
  flex: 1;
}

.request-topline {
  justify-content: space-between;
  gap: 0.75rem;
}

.request-topline strong,
.detail-summary h3,
.empty-detail-inner h2 {
  color: var(--text-heading);
  font-weight: 900;
}

.request-topline strong {
  overflow: hidden;
  font-size: 0.875rem;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.time-chip,
.student-line {
  gap: 0.25rem;
  font-size: 0.75rem;
  font-weight: 700;
}

.student-line {
  margin-top: 0.25rem;
}

.student-line b {
  color: var(--text-body);
}

.request-text {
  display: -webkit-box;
  margin-top: 0.375rem;
  overflow: hidden;
  color: var(--text-body);
  font-size: 0.875rem;
  font-weight: 600;
  line-height: 1.55;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2;
}

.row-meta {
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-top: 0.625rem;
}

.detail-panel {
  position: sticky;
  top: 1rem;
  min-width: 0;
}

.detail-summary {
  align-items: flex-start;
  gap: 0.75rem;
}

.detail-summary h3,
.empty-detail-inner h2 {
  margin: 0;
  font-size: 1rem;
}

.detail-summary p {
  gap: 0.375rem;
  margin: 0.25rem 0 0;
  color: var(--text-muted);
  font-size: 0.8125rem;
  font-weight: 700;
}

.content-box {
  align-items: flex-start;
  gap: 0.625rem;
  margin-top: 1rem;
  border-radius: var(--radius-lg);
  border: 1px solid var(--border-card);
  background: var(--surface-card);
  padding: 0.875rem;
}

.content-box span {
  color: var(--text-label);
  font-size: 0.6875rem;
  font-weight: 900;
  letter-spacing: 0.06em;
  text-transform: uppercase;
}

.content-box p {
  margin: 0.375rem 0 0;
  color: var(--text-body);
  font-size: 0.875rem;
  font-weight: 600;
  line-height: 1.6;
}

.timeline {
  display: grid;
  gap: 0.75rem;
  margin-top: 1rem;
}

.timeline-item {
  align-items: flex-start;
  gap: 0.625rem;
}

.timeline-item > span {
  width: 0.625rem;
  height: 0.625rem;
  margin-top: 0.25rem;
  border-radius: 999px;
  background: var(--border-default);
}

.timeline-item.is-current > span {
  background: var(--color-warning-text);
}

.timeline-item strong {
  color: var(--text-heading);
  font-size: 0.8125rem;
  font-weight: 900;
}

.timeline-item p {
  margin: 0.125rem 0 0;
  font-size: 0.75rem;
  font-weight: 600;
  line-height: 1.45;
}

.detail-actions {
  display: grid;
  gap: 0.5rem;
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 1px solid var(--border-card);
}

.empty-detail-inner {
  display: grid;
  min-height: 20rem;
  place-items: center;
  align-content: center;
  gap: 0.625rem;
  text-align: center;
}

.empty-detail-inner p {
  margin: 0;
  max-width: 16rem;
  font-size: 0.8125rem;
  font-weight: 600;
  line-height: 1.55;
}

@media (max-width: 1024px) {
  .page-header,
  .context-bar {
    align-items: flex-start;
    flex-direction: column;
  }

  .mini-stats {
    width: 100%;
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .search-field {
    width: 100%;
  }

  .requests-workspace {
    grid-template-columns: 1fr;
  }

  .detail-panel {
    position: static;
  }
}

@media (max-width: 640px) {
  .mini-stats {
    grid-template-columns: 1fr;
  }

  .header-actions {
    width: 100%;
  }

  .request-topline {
    align-items: flex-start;
    flex-direction: column;
    gap: 0.25rem;
  }
}
</style>
