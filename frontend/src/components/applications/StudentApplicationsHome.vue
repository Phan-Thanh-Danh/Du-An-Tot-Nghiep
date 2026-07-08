<script setup>
import { computed, ref, watch } from 'vue'
import {
  CalendarDays,
  CheckCircle2,
  FilePenLine,
  FolderOpen,
  Paperclip,
  RotateCcw,
  Search,
  Send,
  XCircle,
} from 'lucide-vue-next'

import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'
import { usePopupStore } from '@/stores/popup'
import { formatDate, formatDateTime } from '@/utils/dateFormat'
import { getStatusMeta, getStatusOptions } from '@/utils/statusLabels'

const popupStore = usePopupStore()
const applicationTypes = []
const studentApplications = []

function getApplicationTypeLabel(type) {
  return applicationTypes.find((item) => item.value === type)?.label || type || 'Không xác định'
}

const loading = ref(false)
const error = ref('')
const applications = ref(studentApplications.map((item) => ({ ...item })))
const selectedId = ref(applications.value[0]?.id || '')
const searchQuery = ref('')
const statusFilter = ref('')
const typeFilter = ref('')
const mode = ref('list')
const wizardStep = ref(0)
const submitAttempted = ref(false)
const confirmAction = ref(null)

const draft = ref({
  type: applicationTypes[0]?.value || '',
  title: '',
  reason: '',
  evidenceName: '',
  reviewAccepted: false,
})

const statusOptions = getStatusOptions('applications').filter((option) =>
  ['nhap', 'da_nop', 'dang_xem_xet', 'yeu_cau_bo_sung', 'da_duyet', 'tu_choi', 'da_huy'].includes(option.value),
)

const filteredApplications = computed(() => {
  const query = searchQuery.value.trim().toLowerCase()
  return applications.value.filter((item) => {
    const matchesSearch =
      !query ||
      item.tieuDe.toLowerCase().includes(query) ||
      getApplicationTypeLabel(item.loaiDon).toLowerCase().includes(query) ||
      item.id.toLowerCase().includes(query)
    const matchesStatus = !statusFilter.value || item.trangThai === statusFilter.value
    const matchesType = !typeFilter.value || item.loaiDon === typeFilter.value
    return matchesSearch && matchesStatus && matchesType
  })
})

const selectedApplication = computed(() =>
  applications.value.find((item) => item.id === selectedId.value) || filteredApplications.value[0] || null,
)

watch(
  filteredApplications,
  (items) => {
    if (!items.length) {
      selectedId.value = ''
      return
    }
    if (!selectedId.value || !items.some((item) => item.id === selectedId.value)) {
      selectedId.value = items[0].id
    }
  },
  { immediate: true },
)

const summaryCards = computed(() => [
  { label: 'Tổng đơn', value: applications.value.length, icon: FolderOpen, variant: 'primary' },
  {
    label: 'Đang xử lý',
    value: applications.value.filter((item) => ['da_nop', 'dang_xem_xet'].includes(item.trangThai)).length,
    icon: CalendarDays,
    variant: 'info',
  },
  {
    label: 'Cần bổ sung',
    value: applications.value.filter((item) => item.trangThai === 'yeu_cau_bo_sung').length,
    icon: RotateCcw,
    variant: 'warning',
  },
  {
    label: 'Đã duyệt',
    value: applications.value.filter((item) => item.trangThai === 'da_duyet').length,
    icon: CheckCircle2,
    variant: 'success',
  },
])

const wizardSteps = ['Chọn loại đơn', 'Điền thông tin', 'Minh chứng', 'Xem lại']

const draftErrors = computed(() => {
  const errors = []
  if (!draft.value.type) errors.push('Vui lòng chọn loại đơn.')
  if (wizardStep.value >= 1 && !draft.value.title.trim()) errors.push('Vui lòng nhập tiêu đề đơn.')
  if (wizardStep.value >= 1 && !draft.value.reason.trim()) errors.push('Vui lòng nhập nội dung yêu cầu.')
  if (wizardStep.value >= 3 && !draft.value.reviewAccepted) errors.push('Vui lòng xác nhận đã kiểm tra thông tin.')
  return errors
})

function statusMeta(status) {
  return getStatusMeta('applications', status)
}

function selectApplication(id) {
  selectedId.value = id
  mode.value = 'list'
}

function resetFilters() {
  searchQuery.value = ''
  statusFilter.value = ''
  typeFilter.value = ''
}

function startCreate() {
  mode.value = 'create'
  wizardStep.value = 0
  submitAttempted.value = false
  draft.value = {
    type: applicationTypes[0]?.value || '',
    title: '',
    reason: '',
    evidenceName: '',
    reviewAccepted: false,
  }
}

function goNext() {
  submitAttempted.value = true
  if (draftErrors.value.length) return
  submitAttempted.value = false
  wizardStep.value = Math.min(wizardSteps.length - 1, wizardStep.value + 1)
}

function saveDraft() {
  const created = createApplicationFromDraft('nhap')
  popupStore.success('Đã lưu bản nháp', 'Đơn mới đã được thêm vào danh sách demo.')
  selectedId.value = created.id
  mode.value = 'list'
}

function submitDraft() {
  submitAttempted.value = true
  if (draftErrors.value.length) return
  confirmAction.value = {
    title: 'Nộp đơn mới?',
    message: 'Đơn sẽ được gửi đến hàng đợi xử lý của giáo vụ.',
    label: 'Nộp đơn',
    variant: 'success',
    run: () => {
      const created = createApplicationFromDraft('da_nop')
      selectedId.value = created.id
      mode.value = 'list'
      confirmAction.value = null
      popupStore.success('Đã nộp đơn', 'Đơn demo đã được gửi thành công.')
    },
  }
}

function createApplicationFromDraft(status) {
  const now = new Date()
  const item = {
    id: `APP-DEMO-${Date.now().toString().slice(-5)}`,
    tieuDe: draft.value.title.trim() || getApplicationTypeLabel(draft.value.type),
    loaiDon: draft.value.type,
    trangThai: status,
    ngayTao: now,
    ngayNop: status === 'nhap' ? null : now,
    hanXuLy: status === 'nhap' ? null : new Date(Date.now() + 3 * 24 * 60 * 60 * 1000),
    capNhatLanCuoi: now,
    moTaNgan: draft.value.reason.trim(),
    nguoiXuLy: '',
    noiDungYeuCauBoSung: '',
    lyDoTuChoi: '',
    formData: [
      { label: 'Loại đơn', value: getApplicationTypeLabel(draft.value.type) },
      { label: 'Nội dung yêu cầu', value: draft.value.reason.trim() },
    ],
    evidence: draft.value.evidenceName
      ? [{ id: 'ev-demo', name: draft.value.evidenceName, size: 'Demo file', uploadedAt: now }]
      : [],
    timeline: [
      {
        id: 'tl-demo',
        at: now,
        title: status === 'nhap' ? 'Lưu bản nháp' : 'Nộp đơn',
        description: status === 'nhap' ? 'Sinh viên lưu bản nháp.' : 'Sinh viên nộp đơn demo.',
        actor: 'Sinh viên',
      },
    ],
  }
  applications.value.unshift(item)
  return item
}

function requestCancel(application) {
  confirmAction.value = {
    title: 'Hủy đơn này?',
    message: `Đơn ${application.id} sẽ chuyển sang trạng thái đã hủy trong dữ liệu demo.`,
    label: 'Hủy đơn',
    variant: 'danger',
    run: () => {
      application.trangThai = 'da_huy'
      application.capNhatLanCuoi = new Date()
      confirmAction.value = null
      popupStore.warning('Đã hủy đơn', 'Đơn demo đã được cập nhật trạng thái.')
    },
  }
}

function resubmit(application) {
  confirmAction.value = {
    title: 'Nộp lại đơn?',
    message: 'Đơn sẽ quay lại hàng đợi xử lý sau khi bổ sung minh chứng.',
    label: 'Nộp lại',
    variant: 'success',
    run: () => {
      application.trangThai = 'da_nop'
      application.capNhatLanCuoi = new Date()
      confirmAction.value = null
      popupStore.success('Đã nộp lại đơn', 'Đơn demo đã được gửi lại.')
    },
  }
}

function canCancel(application) {
  return ['nhap', 'da_nop', 'dang_xem_xet', 'yeu_cau_bo_sung'].includes(application?.trangThai)
}

function retryLoad() {
  loading.value = true
  window.setTimeout(() => {
    applications.value = studentApplications.map((item) => ({ ...item }))
    loading.value = false
    error.value = ''
  }, 350)
}
</script>

<template>
  <div class="student-applications-page mx-auto max-w-7xl space-y-5">
    <GlassPanel variant="flat" density="compact" class="page-header">
      <div class="header-copy">
        <p class="eyebrow">
          <FolderOpen :size="15" />
          Trung tâm đơn từ
        </p>
        <div>
          <h1>Đơn từ của tôi</h1>
          <p>Quản lý bản nháp, đơn đã nộp, yêu cầu bổ sung và tiến độ xử lý.</p>
        </div>
      </div>
      <GlassButton variant="primary" @click="startCreate">
        <template #leading>
          <FilePenLine :size="16" />
        </template>
        Tạo đơn mới
      </GlassButton>
    </GlassPanel>

    <section class="summary-grid">
      <GlassPanel v-for="card in summaryCards" :key="card.label" variant="flat" density="compact" class="summary-card">
        <span class="summary-icon">
          <component :is="card.icon" :size="18" />
        </span>
        <span class="min-w-0">
          <p>{{ card.label }}</p>
          <strong>{{ card.value }}</strong>
        </span>
        <GlassBadge :variant="card.variant">{{ card.label }}</GlassBadge>
      </GlassPanel>
    </section>

    <GlassPanel v-if="mode === 'list'" variant="flat" density="compact" class="filter-panel">
      <div class="filter-grid">
        <label class="control-field">
          <span>Tìm kiếm</span>
          <span class="search-control">
            <Search :size="15" />
            <input v-model="searchQuery" type="text" placeholder="Tiêu đề, mã đơn, loại đơn" />
          </span>
        </label>
        <label class="control-field">
          <span>Trạng thái</span>
          <select v-model="statusFilter" class="lg-control">
            <option value="">Tất cả</option>
            <option v-for="option in statusOptions" :key="option.value" :value="option.value">
              {{ option.label }}
            </option>
          </select>
        </label>
        <label class="control-field">
          <span>Loại đơn</span>
          <select v-model="typeFilter" class="lg-control">
            <option value="">Tất cả</option>
            <option v-for="type in applicationTypes" :key="type.value" :value="type.value">
              {{ type.label }}
            </option>
          </select>
        </label>
        <div class="filter-actions">
          <GlassButton variant="secondary" @click="resetFilters">Xóa lọc</GlassButton>
        </div>
      </div>
    </GlassPanel>

    <LoadingSkeleton v-if="loading" :lines="6" />

    <GlassPanel v-else-if="error" variant="flat" density="compact" class="error-panel">
      <div>
        <h2>Không tải được danh sách đơn</h2>
        <p>{{ error }}</p>
      </div>
      <GlassButton variant="secondary" @click="retryLoad">Thử lại</GlassButton>
    </GlassPanel>

    <section v-else-if="mode === 'list'" class="requests-grid">
      <div class="request-list">
        <EmptyState
          v-if="!filteredApplications.length"
          title="Bạn chưa có đơn từ nào"
          description="Tạo đơn mới để gửi yêu cầu học vụ đến nhà trường."
        >
          <GlassButton variant="primary" @click="startCreate">Tạo đơn mới</GlassButton>
        </EmptyState>

        <button
          v-for="application in filteredApplications"
          :key="application.id"
          type="button"
          :class="['request-card', application.id === selectedApplication?.id ? 'is-active' : '']"
          @click="selectApplication(application.id)"
        >
          <span class="card-top">
            <GlassBadge :variant="statusMeta(application.trangThai).variant">
              {{ statusMeta(application.trangThai).label }}
            </GlassBadge>
            <span>{{ application.id }}</span>
          </span>
          <span class="request-title clamp-2">{{ application.tieuDe }}</span>
          <span class="request-type clamp-1">{{ getApplicationTypeLabel(application.loaiDon) }}</span>
          <span class="request-meta">
            <span>Tạo: {{ formatDate(application.ngayTao) }}</span>
            <span>Hạn: {{ formatDate(application.hanXuLy, 'Chưa có hạn') }}</span>
          </span>
          <span class="request-desc clamp-2">{{ application.moTaNgan }}</span>
          <span class="card-actions">
            <GlassButton variant="secondary" size="sm" @click.stop="selectApplication(application.id)">Xem chi tiết</GlassButton>
            <GlassButton v-if="application.trangThai === 'yeu_cau_bo_sung'" variant="primary" size="sm" @click.stop="resubmit(application)">
              Nộp lại
            </GlassButton>
            <GlassButton v-if="canCancel(application)" variant="ghost" size="sm" @click.stop="requestCancel(application)">
              Hủy
            </GlassButton>
          </span>
        </button>
      </div>

      <GlassPanel v-if="selectedApplication" variant="flat" density="compact" class="detail-panel">
        <div class="detail-header">
          <div>
            <h2 class="clamp-2">{{ selectedApplication.tieuDe }}</h2>
            <p>{{ getApplicationTypeLabel(selectedApplication.loaiDon) }} · {{ selectedApplication.id }}</p>
          </div>
          <GlassBadge :variant="statusMeta(selectedApplication.trangThai).variant" size="md">
            {{ statusMeta(selectedApplication.trangThai).label }}
          </GlassBadge>
        </div>

        <div v-if="selectedApplication.noiDungYeuCauBoSung" class="callout warning">
          <RotateCcw :size="16" />
          {{ selectedApplication.noiDungYeuCauBoSung }}
        </div>
        <div v-if="selectedApplication.lyDoTuChoi" class="callout danger">
          <XCircle :size="16" />
          {{ selectedApplication.lyDoTuChoi }}
        </div>

        <div class="info-grid">
          <div><span>Ngày tạo</span><strong>{{ formatDate(selectedApplication.ngayTao) }}</strong></div>
          <div><span>Ngày nộp</span><strong>{{ formatDate(selectedApplication.ngayNop, 'Chưa nộp') }}</strong></div>
          <div><span>Hạn xử lý</span><strong>{{ formatDate(selectedApplication.hanXuLy, 'Chưa có hạn') }}</strong></div>
          <div><span>Người xử lý</span><strong>{{ selectedApplication.nguoiXuLy || 'Chưa phân công' }}</strong></div>
        </div>

        <section class="detail-section">
          <h3>Thông tin đã khai</h3>
          <div class="readonly-list">
            <div v-for="field in selectedApplication.formData" :key="field.label">
              <span>{{ field.label }}</span>
              <strong>{{ field.value }}</strong>
            </div>
          </div>
        </section>

        <section class="detail-section">
          <h3>Minh chứng</h3>
          <div v-if="selectedApplication.evidence.length" class="evidence-list">
            <div v-for="file in selectedApplication.evidence" :key="file.id" class="evidence-item">
              <Paperclip :size="15" />
              <span class="min-w-0">
                <strong class="clamp-1">{{ file.name }}</strong>
                <small>{{ file.size }} · {{ formatDate(file.uploadedAt) }}</small>
              </span>
            </div>
          </div>
          <p v-else class="muted-text">Chưa có minh chứng đính kèm.</p>
        </section>

        <section class="detail-section">
          <h3>Timeline xử lý</h3>
          <div class="timeline">
            <p v-for="item in selectedApplication.timeline" :key="item.id">
              <span>{{ formatDateTime(item.at) }}</span>
              <strong>{{ item.title }}</strong>
              <small>{{ item.description }}</small>
            </p>
          </div>
        </section>
      </GlassPanel>
    </section>

    <GlassPanel v-else variant="flat" density="compact" class="wizard-panel">
      <div class="wizard-header">
        <div>
          <h2>Tạo đơn mới</h2>
          <p>Hoàn tất 4 bước để lưu nháp hoặc nộp đơn demo.</p>
        </div>
        <GlassButton variant="secondary" @click="mode = 'list'">Quay lại danh sách</GlassButton>
      </div>

      <div class="stepper">
        <span v-for="(step, index) in wizardSteps" :key="step" :class="['step', index === wizardStep ? 'active' : '', index < wizardStep ? 'done' : '']">
          <strong>{{ index + 1 }}</strong>
          {{ step }}
        </span>
      </div>

      <div v-if="submitAttempted && draftErrors.length" class="form-error-summary">
        <p v-for="errorItem in draftErrors" :key="errorItem">{{ errorItem }}</p>
      </div>

      <div class="wizard-body">
        <div v-if="wizardStep === 0" class="type-grid">
          <button
            v-for="type in applicationTypes"
            :key="type.value"
            type="button"
            :class="['type-card', draft.type === type.value ? 'active' : '']"
            @click="draft.type = type.value"
          >
            <strong>{{ type.label }}</strong>
            <span>Biểu mẫu demo cho {{ type.label.toLowerCase() }}.</span>
          </button>
        </div>

        <div v-else-if="wizardStep === 1" class="form-grid">
          <label class="form-field">
            <span>Tiêu đề đơn</span>
            <input v-model="draft.title" type="text" placeholder="Nhập tiêu đề rõ ràng" />
          </label>
          <label class="form-field">
            <span>Nội dung yêu cầu</span>
            <textarea v-model="draft.reason" rows="5" placeholder="Trình bày ngắn gọn lý do và thông tin cần xử lý" />
          </label>
        </div>

        <div v-else-if="wizardStep === 2" class="form-grid">
          <label class="form-field">
            <span>Tên minh chứng demo</span>
            <input v-model="draft.evidenceName" type="text" placeholder="VD: giay-xac-nhan.pdf" />
          </label>
          <p class="helper-text">Phase UI chỉ lưu tên file demo, không upload backend.</p>
        </div>

        <div v-else class="review-box">
          <div><span>Loại đơn</span><strong>{{ getApplicationTypeLabel(draft.type) }}</strong></div>
          <div><span>Tiêu đề</span><strong>{{ draft.title || 'Chưa nhập' }}</strong></div>
          <div><span>Nội dung</span><strong>{{ draft.reason || 'Chưa nhập' }}</strong></div>
          <div><span>Minh chứng</span><strong>{{ draft.evidenceName || 'Không đính kèm' }}</strong></div>
          <label class="review-check">
            <input v-model="draft.reviewAccepted" type="checkbox" />
            Tôi đã kiểm tra thông tin và sẵn sàng nộp đơn.
          </label>
        </div>
      </div>

      <div class="wizard-actions">
        <GlassButton variant="secondary" :disabled="wizardStep === 0" @click="wizardStep -= 1">Quay lại</GlassButton>
        <span class="action-spacer" />
        <GlassButton variant="ghost" @click="saveDraft">Lưu nháp</GlassButton>
        <GlassButton v-if="wizardStep < wizardSteps.length - 1" variant="primary" @click="goNext">Tiếp tục</GlassButton>
        <GlassButton v-else variant="success" @click="submitDraft">
          <template #leading><Send :size="16" /></template>
          Nộp đơn
        </GlassButton>
      </div>
    </GlassPanel>

    <ConfirmActionDialog
      :model-value="Boolean(confirmAction)"
      :title="confirmAction?.title || ''"
      :message="confirmAction?.message || ''"
      :confirm-label="confirmAction?.label || 'Xác nhận'"
      :variant="confirmAction?.variant || 'primary'"
      @update:model-value="(value) => { if (!value) confirmAction = null }"
      @confirm="confirmAction?.run?.()"
    />
  </div>
</template>

<style scoped>
.student-applications-page {
  color: var(--text-body);
}

.page-header,
.summary-card,
.filter-actions,
.card-top,
.card-actions,
.detail-header,
.evidence-item,
.wizard-header,
.wizard-actions {
  display: flex;
  align-items: center;
}

.page-header,
.detail-header,
.wizard-header {
  justify-content: space-between;
  gap: 1rem;
}

.header-copy,
.detail-header > div,
.wizard-header > div {
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
h2,
h3 {
  margin: 0;
  color: var(--text-heading);
  font-weight: 900;
}

h1 {
  margin-top: 0.45rem;
  font-size: 1.5rem;
  line-height: 1.15;
}

h2 {
  font-size: 1rem;
}

h3 {
  font-size: 0.875rem;
}

p {
  margin: 0.25rem 0 0;
  color: var(--text-muted);
  font-size: 0.84375rem;
  line-height: 1.55;
}

.summary-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 0.75rem;
  align-items: stretch;
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

.summary-card p,
.info-grid span,
.readonly-list span,
.review-box span {
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

.filter-grid {
  display: grid;
  grid-template-columns: minmax(0, 1.35fr) minmax(0, 1fr) minmax(0, 1fr) auto;
  gap: 0.75rem;
  align-items: end;
}

.control-field,
.form-field {
  display: flex;
  min-width: 0;
  flex-direction: column;
  gap: 0.375rem;
}

.control-field > span,
.form-field > span {
  color: var(--text-label);
  font-size: 0.75rem;
  font-weight: 850;
}

.search-control,
.lg-control,
.form-field input,
.form-field textarea {
  width: 100%;
  min-height: var(--control-height-lg);
  border: 1px solid var(--border-input);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  color: var(--text-heading);
  outline: 0;
  font-size: 0.84375rem;
  font-weight: 750;
}

.search-control {
  display: flex;
  align-items: center;
  gap: 0.45rem;
  padding: 0 0.75rem;
  color: var(--text-placeholder);
}

.search-control input {
  min-width: 0;
  flex: 1;
  border: 0;
  outline: 0;
  background: transparent;
  color: var(--text-heading);
}

.lg-control,
.form-field input {
  padding: 0 0.75rem;
}

.form-field textarea {
  padding: 0.75rem;
  resize: vertical;
}

.search-control:focus-within,
.lg-control:focus,
.form-field input:focus,
.form-field textarea:focus {
  border-color: var(--border-input-focus);
  background: var(--surface-input-focus);
  box-shadow: 0 0 0 3px var(--border-focus-ring);
}

.requests-grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 390px;
  gap: 0.875rem;
  align-items: start;
}

.request-list {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.75rem;
}

.request-card {
  display: flex;
  min-height: 17rem;
  flex-direction: column;
  justify-content: space-between;
  overflow: hidden;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-lg);
  background: var(--surface-card);
  padding: 0.875rem;
  text-align: left;
  color: var(--text-body);
}

.request-card.is-active,
.request-card:hover,
.type-card.active,
.type-card:hover {
  border-color: var(--border-input-focus);
  background: var(--surface-card-hover);
}

.request-card.is-active {
  box-shadow: inset 3px 0 0 var(--text-link);
}

.card-top {
  justify-content: space-between;
  gap: 0.75rem;
  color: var(--text-muted);
  font-size: 0.75rem;
  font-weight: 850;
}

.request-title {
  color: var(--text-heading);
  font-size: 1rem;
  font-weight: 900;
  line-height: 1.35;
}

.request-type,
.request-desc,
.request-meta,
.muted-text,
.timeline small,
.evidence-item small {
  color: var(--text-muted);
  font-size: 0.8125rem;
  font-weight: 750;
}

.request-meta {
  display: grid;
  gap: 0.25rem;
}

.card-actions {
  flex-wrap: wrap;
  gap: 0.5rem;
}

.detail-panel {
  position: sticky;
  top: 0.75rem;
  display: grid;
  gap: 0.875rem;
}

.callout {
  display: flex;
  gap: 0.5rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  padding: 0.625rem 0.75rem;
  font-size: 0.8125rem;
  font-weight: 800;
}

.callout.warning {
  background: var(--color-warning-bg);
  color: var(--color-warning-text);
}

.callout.danger {
  background: var(--color-danger-bg);
  color: var(--color-danger-text);
}

.info-grid,
.readonly-list,
.review-box {
  display: grid;
  gap: 0.5rem;
}

.info-grid {
  grid-template-columns: repeat(2, minmax(0, 1fr));
}

.info-grid div,
.readonly-list div,
.review-box div,
.timeline p,
.evidence-item {
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  padding: 0.625rem;
}

.info-grid strong,
.readonly-list strong,
.review-box strong,
.timeline strong,
.evidence-item strong {
  display: block;
  color: var(--text-heading);
  font-size: 0.84375rem;
  font-weight: 850;
}

.detail-section {
  display: grid;
  gap: 0.625rem;
}

.evidence-list,
.timeline {
  display: grid;
  gap: 0.5rem;
}

.evidence-item {
  gap: 0.5rem;
  color: var(--text-link);
}

.timeline p {
  margin: 0;
}

.timeline span {
  display: block;
  color: var(--text-link);
  font-size: 0.75rem;
  font-weight: 850;
}

.wizard-panel {
  display: grid;
  gap: 1rem;
}

.stepper {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 0.5rem;
}

.step {
  display: flex;
  min-height: 3rem;
  align-items: center;
  gap: 0.5rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  padding: 0.5rem;
  color: var(--text-muted);
  font-size: 0.78125rem;
  font-weight: 850;
}

.step strong {
  display: inline-flex;
  width: 1.5rem;
  height: 1.5rem;
  flex: none;
  align-items: center;
  justify-content: center;
  border-radius: 999px;
  background: var(--surface-card);
  color: var(--text-link);
}

.step.active,
.step.done {
  border-color: var(--border-input-focus);
  background: var(--accent-primary-soft);
  color: var(--text-heading);
}

.form-error-summary {
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--color-danger-bg);
  color: var(--color-danger-text);
  padding: 0.625rem 0.75rem;
  font-weight: 800;
}

.type-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.75rem;
}

.type-card {
  min-height: 6rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-lg);
  background: var(--surface-card);
  padding: 0.875rem;
  text-align: left;
  color: var(--text-body);
}

.type-card strong {
  display: block;
  color: var(--text-heading);
  font-weight: 900;
}

.type-card span,
.helper-text {
  color: var(--text-muted);
  font-size: 0.8125rem;
}

.form-grid {
  display: grid;
  gap: 0.875rem;
}

.review-check {
  display: flex;
  gap: 0.5rem;
  color: var(--text-label);
  font-weight: 800;
}

.wizard-actions {
  gap: 0.5rem;
}

.action-spacer {
  flex: 1;
}

.clamp-1,
.clamp-2 {
  display: -webkit-box;
  overflow: hidden;
  -webkit-box-orient: vertical;
}

.clamp-1 {
  -webkit-line-clamp: 1;
}

.clamp-2 {
  -webkit-line-clamp: 2;
}

@media (max-width: 1180px) {
  .summary-grid,
  .request-list {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .requests-grid {
    grid-template-columns: 1fr;
  }

  .detail-panel {
    position: static;
  }
}

@media (max-width: 820px) {
  .filter-grid,
  .type-grid,
  .stepper,
  .info-grid {
    grid-template-columns: 1fr;
  }

  .filter-actions,
  .wizard-actions {
    align-items: stretch;
  }
}

@media (max-width: 680px) {
  .page-header,
  .detail-header,
  .wizard-header {
    flex-direction: column;
    align-items: stretch;
  }

  .summary-grid,
  .request-list {
    grid-template-columns: 1fr;
  }

  .request-card {
    min-height: 15rem;
  }

  .wizard-actions,
  .card-actions {
    display: grid;
    grid-template-columns: 1fr;
  }

  .action-spacer {
    display: none;
  }
}
</style>
