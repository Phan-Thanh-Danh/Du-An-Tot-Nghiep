<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  ArrowLeft, Loader2, AlertCircle, FileText, Download, Paperclip, Inbox, MessageSquareText,
  CheckCircle2, XCircle, Zap, ClipboardList, User, Building2, Clock, ShieldCheck
} from 'lucide-vue-next'
import { applicationsApi } from '@/services/applicationsApi'
import { getStoredAccessToken } from '@/services/apiClient'
import { usePopupStore } from '@/stores/popup'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'

const route = useRoute()
const router = useRouter()
const popupStore = usePopupStore()

const applicationId = Number(route.params.id)
const detail = ref(null)
const loading = ref(true)
const apiError = ref('')
const actionKey = ref('')
const modal = ref('')

const modalForm = ref({
  request: '',
  internalNote: '',
  publicNote: '',
  reason: '',
  outcome: 'da_ghi_nhan',
})

const OUTCOME_OPTIONS = [
  { value: 'da_ghi_nhan', label: 'Đã ghi nhận' },
  { value: 'xu_ly_thanh_cong', label: 'Xử lý thành công' },
  { value: 'xu_ly_that_bai', label: 'Xử lý thất bại' },
]

const FIELD_LABELS = {
  ngay_bat_dau: 'Ngày bắt đầu',
  ngay_ket_thuc: 'Ngày kết thúc',
  ly_do: 'Lý do',
  noi_dung: 'Nội dung',
  lien_he_khan_cap: 'Liên hệ khẩn cấp',
  ma_hoc_ky: 'Học kỳ',
  ma_mon_hoc: 'Môn học',
  ma_diem_so: 'Điểm số',
  ma_lop: 'Lớp',
  so_buoi: 'Số buổi',
  so_tiet: 'Số tiết',
}

const getStatusMeta = (status) => {
  const map = {
    da_nop: { label: 'Chờ tiếp nhận', variant: 'warning' },
    dang_xem_xet: { label: 'Đang xem xét', variant: 'info' },
    yeu_cau_bo_sung: { label: 'Yêu cầu bổ sung', variant: 'warning' },
    da_duyet: { label: 'Đã duyệt', variant: 'success' },
    tu_choi: { label: 'Từ chối', variant: 'danger' },
    da_huy: { label: 'Đã hủy', variant: 'neutral' },
    nhap: { label: 'Nháp', variant: 'neutral' },
  }
  return map[status] || { label: status, variant: 'neutral' }
}

const getProcessingMeta = (code) => {
  const map = {
    chua_xu_ly: { label: 'Chưa xử lý', variant: 'neutral' },
    cho_xu_ly: { label: 'Chờ xử lý', variant: 'warning' },
    da_ghi_nhan: { label: 'Đã ghi nhận', variant: 'info' },
    xu_ly_thanh_cong: { label: 'Xử lý thành công', variant: 'success' },
    xu_ly_that_bai: { label: 'Xử lý thất bại', variant: 'danger' },
    can_xu_ly_thu_cong: { label: 'Cần xử lý thủ công', variant: 'warning' },
  }
  return map[code] || { label: code, variant: 'neutral' }
}

const getSlaMeta = (sla) => {
  const status = sla?.status ?? 'none'
  const map = {
    on_track: { label: 'Đúng hạn', variant: 'success' },
    due_soon: { label: 'Sắp hết hạn', variant: 'warning' },
    overdue: { label: 'Quá hạn', variant: 'danger' },
    paused: { label: 'Tạm dừng', variant: 'neutral' },
    none: { label: 'Không giới hạn', variant: 'neutral' },
  }
  const meta = map[status] || { label: status, variant: 'neutral' }
  if (status === 'due_soon' && sla?.remainingMinutes != null) {
    meta.label += ` (${formatMinutes(sla.remainingMinutes)})`
  }
  return meta
}

function formatMinutes(totalMinutes) {
  if (totalMinutes < 60) return `${totalMinutes}p`
  const h = Math.floor(totalMinutes / 60)
  const m = totalMinutes % 60
  return m > 0 ? `${h}h${m}m` : `${h}h`
}

function formatDateTime(value) {
  if (!value) return '—'
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return '—'
  const pad = (n) => String(n).padStart(2, '0')
  return `${pad(d.getDate())}/${pad(d.getMonth() + 1)}/${d.getFullYear()} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}

function formatBytes(bytes) {
  if (bytes == null) return '—'
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(2)} MB`
}

function formatFieldValue(value) {
  if (value === null || value === undefined || value === '') return '—'
  if (Array.isArray(value)) return value.map((v) => formatFieldValue(v)).join(', ')
  if (typeof value === 'object') {
    const pick = value.ten ?? value.label ?? value.name ?? value.hoTen ?? value.tenMonHoc ?? value.tenHocKy ?? value.tenLop
    return pick != null ? String(pick) : JSON.stringify(value)
  }
  if (typeof value === 'boolean') return value ? 'Có' : 'Không'
  return String(value)
}

function fieldLabel(key) {
  return FIELD_LABELS[key] || String(key).replaceAll('_', ' ').replace(/\b\w/g, (c) => c.toUpperCase())
}

const formEntries = computed(() => {
  const data = detail.value?.duLieuBieuMau
  if (!data || typeof data !== 'object' || Array.isArray(data)) return []
  return Object.entries(data)
})

const actions = computed(() => detail.value?.allowedActions || {})
const hasAnyAction = computed(() => Object.values(actions.value).some(Boolean))
const rowVersion = computed(() => detail.value?.rowVersion || '')

async function loadData() {
  loading.value = true
  apiError.value = ''
  try {
    detail.value = await applicationsApi.getAdminApplicationDetail(applicationId)
  } catch (e) {
    console.error(e)
    apiError.value = e?.message || 'Không thể tải chi tiết đơn.'
  } finally {
    loading.value = false
  }
}

async function downloadFile(url, filename) {
  try {
    const token = getStoredAccessToken()
    const res = await fetch(url, {
      headers: token ? { Authorization: `Bearer ${token}` } : {},
    })
    if (!res.ok) {
      throw new Error(`HTTP ${res.status}`)
    }
    const blob = await res.blob()
    const objectUrl = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = objectUrl
    a.download = filename || 'file'
    document.body.appendChild(a)
    a.click()
    window.URL.revokeObjectURL(objectUrl)
    document.body.removeChild(a)
  } catch (e) {
    console.error(e)
    popupStore.error('Không thể tải tệp đính kèm. Vui lòng thử lại.')
  }
}

function downloadAttachment(attachment) {
  const url = applicationsApi.downloadAdminEvidence(applicationId, attachment.maTep)
  downloadFile(url, attachment.tenFileGoc)
}

function openModal(name) {
  modalForm.value = { request: '', internalNote: '', publicNote: '', reason: '', outcome: 'da_ghi_nhan' }
  modal.value = name
}

const modalTitle = computed(() => {
  const titles = {
    receive: 'Tiếp nhận đơn',
    requestSupplement: 'Yêu cầu bổ sung',
    approve: 'Duyệt đơn',
    reject: 'Từ chối đơn',
    record: 'Ghi nhận kết quả xử lý',
  }
  return titles[modal.value] || ''
})

function submitModal() {
  const rv = rowVersion.value
  switch (modal.value) {
    case 'receive':
      runAction('receive', { rowVersion: rv })
      break
    case 'requestSupplement':
      if (!modalForm.value.request.trim()) {
        popupStore.error('Vui lòng nhập nội dung yêu cầu bổ sung.')
        return
      }
      runAction('requestSupplement', {
        request: modalForm.value.request.trim(),
        internalNote: modalForm.value.internalNote || undefined,
        rowVersion: rv,
      })
      break
    case 'approve':
      runAction('approve', {
        publicNote: modalForm.value.publicNote || undefined,
        internalNote: modalForm.value.internalNote || undefined,
        rowVersion: rv,
      })
      break
    case 'reject':
      if (!modalForm.value.reason.trim()) {
        popupStore.error('Vui lòng nhập lý do từ chối.')
        return
      }
      runAction('reject', {
        reason: modalForm.value.reason.trim(),
        internalNote: modalForm.value.internalNote || undefined,
        rowVersion: rv,
      })
      break
    case 'record':
      runAction('record', {
        outcome: modalForm.value.outcome,
        publicNote: modalForm.value.publicNote || undefined,
        internalNote: modalForm.value.internalNote || undefined,
        rowVersion: rv,
      })
      break
  }
}

function closeModal() {
  modal.value = ''
  actionKey.value = ''
}

async function runAction(name, payload) {
  actionKey.value = name
  try {
    switch (name) {
      case 'receive':
        await applicationsApi.receiveApplication(applicationId, payload)
        popupStore.success('Đơn đã được tiếp nhận.')
        break
      case 'requestSupplement':
        await applicationsApi.requestSupplement(applicationId, payload)
        popupStore.success('Đã gửi yêu cầu bổ sung cho sinh viên.')
        break
      case 'approve':
        await applicationsApi.approveApplication(applicationId, payload)
        popupStore.success('Đơn đã được duyệt.')
        break
      case 'reject':
        await applicationsApi.rejectApplication(applicationId, payload)
        popupStore.success('Đơn đã bị từ chối.')
        break
      case 'process':
        await applicationsApi.processApprovedApplication(applicationId, payload)
        popupStore.success('Đơn đã được xử lý tự động.')
        break
      case 'record':
        await applicationsApi.recordProcessingResult(applicationId, payload)
        popupStore.success('Đã ghi nhận kết quả xử lý.')
        break
    }
    closeModal()
    await loadData()
  } catch (e) {
    console.error(e)
    popupStore.error(e?.message || 'Thao tác không thành công. Vui lòng thử lại.')
  } finally {
    actionKey.value = ''
  }
}

onMounted(() => { loadData() })
</script>

<template>
  <div class="h-full flex flex-col space-y-4 max-w-5xl mx-auto w-full">
    <div class="flex items-start justify-between flex-wrap gap-4">
      <div class="flex items-center gap-3">
        <button @click="router.push('/staff/requests')" class="p-2 rounded-xl border border-(--border-input) text-(--text-muted) hover:bg-(--surface-hover) transition-colors">
          <ArrowLeft :size="18" />
        </button>
        <div>
          <div class="flex items-center gap-2">
            <FileText class="text-(--lg-primary)" :size="24" />
            <h1 class="text-xl font-bold text-(--text-heading)">Chi tiết đơn</h1>
            <span class="font-mono text-xs font-bold text-(--text-muted)">ĐT-{{ String(applicationId).padStart(4, '0') }}</span>
          </div>
          <p class="text-sm text-(--text-muted) mt-0.5">{{ detail?.tenLoaiDon || 'Đơn từ sinh viên' }}</p>
        </div>
      </div>
    </div>

    <div v-if="loading" class="flex flex-col items-center justify-center py-20 gap-3">
      <Loader2 class="animate-spin text-(--text-muted)" :size="28" />
      <p class="text-sm text-(--text-muted)">Đang tải chi tiết...</p>
    </div>

    <div v-else-if="apiError" class="surface-card border border-(--border-card) rounded-2xl p-6 flex flex-col items-center justify-center gap-3">
      <AlertCircle :size="32" class="text-(--color-danger-text)" />
      <p class="text-sm font-bold text-(--text-heading)">Không thể tải dữ liệu</p>
      <p class="text-xs text-(--text-muted)">{{ apiError }}</p>
      <button @click="loadData" class="lg-button-primary px-4 py-2 text-xs font-bold rounded-xl mt-2">Thử lại</button>
    </div>

    <template v-else-if="detail">
      <div class="surface-card border border-(--border-card) rounded-2xl p-5 flex flex-wrap items-center gap-3">
        <GlassBadge :variant="getStatusMeta(detail.trangThai).variant" size="sm">{{ getStatusMeta(detail.trangThai).label }}</GlassBadge>
        <GlassBadge :variant="getProcessingMeta(detail.trangThaiXuLyNghiepVu).variant" size="sm">
          {{ detail.tenTrangThaiXuLyNghiepVu || getProcessingMeta(detail.trangThaiXuLyNghiepVu).label }}
        </GlassBadge>
        <GlassBadge :variant="getSlaMeta(detail.sla).variant" size="sm">{{ getSlaMeta(detail.sla).label }}</GlassBadge>
        <div class="ml-auto flex flex-col items-end text-right gap-0.5">
          <p class="text-xs text-(--text-muted) flex items-center gap-1"><Clock :size="12" /> Nộp lúc {{ formatDateTime(detail.ngayNop) }}</p>
          <p class="text-xs text-(--text-muted)">Hạn xử lý: {{ formatDateTime(detail.hanXuLyLuc) }}</p>
        </div>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div class="surface-card border border-(--border-card) rounded-2xl p-5">
          <div class="flex items-center gap-2 mb-3">
            <User :size="16" class="text-(--lg-primary)" />
            <h2 class="text-sm font-bold text-(--text-heading)">Sinh viên</h2>
          </div>
          <dl class="space-y-2 text-sm">
            <div class="flex justify-between gap-3">
              <dt class="text-(--text-muted)">Họ tên</dt>
              <dd class="font-semibold text-(--text-heading) text-right">{{ detail.hocSinh?.hoTen || '—' }}</dd>
            </div>
            <div class="flex justify-between gap-3">
              <dt class="text-(--text-muted)">Email</dt>
              <dd class="text-(--text-heading) text-right break-all">{{ detail.hocSinh?.email || '—' }}</dd>
            </div>
            <div class="flex justify-between gap-3">
              <dt class="text-(--text-muted)">Vai trò</dt>
              <dd class="text-(--text-heading) text-right">{{ detail.hocSinh?.vaiTro || '—' }}</dd>
            </div>
            <div class="flex justify-between gap-3">
              <dt class="text-(--text-muted)">Cơ sở</dt>
              <dd class="text-(--text-heading) text-right flex items-center gap-1 justify-end"><Building2 :size="13" class="text-(--text-muted)" /> {{ detail.donVi?.tenDonVi || '—' }}</dd>
            </div>
            <div class="flex justify-between gap-3">
              <dt class="text-(--text-muted)">Tiêu đề</dt>
              <dd class="text-(--text-heading) text-right">{{ detail.tieuDe || '—' }}</dd>
            </div>
          </dl>
        </div>

        <div class="surface-card border border-(--border-card) rounded-2xl p-5">
          <div class="flex items-center gap-2 mb-3">
            <ClipboardList :size="16" class="text-(--lg-primary)" />
            <h2 class="text-sm font-bold text-(--text-heading)">Nội dung đơn</h2>
          </div>
          <div v-if="formEntries.length > 0" class="space-y-2 text-sm">
            <div v-for="[key, value] in formEntries" :key="key" class="flex justify-between gap-3 border-b border-(--border-default) pb-2">
              <dt class="text-(--text-muted) shrink-0">{{ fieldLabel(key) }}</dt>
              <dd class="font-semibold text-(--text-heading) text-right break-words min-w-0">{{ formatFieldValue(value) }}</dd>
            </div>
          </div>
          <p v-else-if="!detail.duLieuBieuMauHopLe" class="text-sm text-(--text-muted) flex items-center gap-2">
            <AlertCircle :size="15" class="text-(--color-warning-text)" /> Dữ liệu biểu mẫu không hợp lệ hoặc trống.
          </p>
          <p v-else class="text-sm text-(--text-muted)">Đơn không có nội dung biểu mẫu.</p>
        </div>
      </div>

      <div v-if="detail.noiDungYeuCauBoSung" class="surface-card border border-(--color-warning-bg) rounded-2xl p-4 flex gap-3 bg-(--color-warning-bg)">
        <MessageSquareText :size="18" class="text-(--color-warning-text) shrink-0 mt-0.5" />
        <div>
          <p class="text-sm font-bold text-(--text-heading)">Yêu cầu bổ sung đang chờ sinh viên phản hồi</p>
          <p class="text-sm text-(--text-muted) mt-1">{{ detail.noiDungYeuCauBoSung }}</p>
        </div>
      </div>

      <div v-if="detail.lyDoTuChoi" class="surface-card border border-(--color-danger-bg) rounded-2xl p-4 flex gap-3 bg-(--color-danger-bg)">
        <XCircle :size="18" class="text-(--color-danger-text) shrink-0 mt-0.5" />
        <div>
          <p class="text-sm font-bold text-(--text-heading)">Lý do từ chối</p>
          <p class="text-sm text-(--text-muted) mt-1">{{ detail.lyDoTuChoi }}</p>
        </div>
      </div>

      <div class="surface-card border border-(--border-card) rounded-2xl p-5">
        <div class="flex items-center gap-2 mb-3">
          <Paperclip :size="16" class="text-(--lg-primary)" />
          <h2 class="text-sm font-bold text-(--text-heading)">Tệp đính kèm ({{ detail.attachments?.length || 0 }})</h2>
        </div>
        <div v-if="detail.attachments && detail.attachments.length > 0" class="flex flex-col gap-2">
          <div v-for="att in detail.attachments" :key="att.maTep" class="flex items-center gap-3 border border-(--border-default) rounded-xl p-3">
            <FileText :size="18" class="text-(--text-muted) shrink-0" />
            <div class="min-w-0 flex-1">
              <p class="text-sm font-semibold text-(--text-heading) truncate">{{ att.tenFileGoc }}</p>
              <p class="text-xs text-(--text-muted)">{{ formatBytes(att.kichThuocByte) }} · {{ formatDateTime(att.ngayTao) }}</p>
            </div>
            <GlassButton variant="secondary" size="sm" @click="downloadAttachment(att)">
              <Download :size="14" class="mr-1" /> Tải xuống
            </GlassButton>
          </div>
        </div>
        <p v-else class="text-sm text-(--text-muted)">Không có tệp đính kèm.</p>
      </div>

      <div class="surface-card border border-(--border-card) rounded-2xl p-5">
        <div class="flex items-center gap-2 mb-4">
          <ShieldCheck :size="16" class="text-(--lg-primary)" />
          <h2 class="text-sm font-bold text-(--text-heading)">Lịch sử xử lý</h2>
        </div>
        <div v-if="detail.timeline && detail.timeline.length > 0" class="relative flex flex-col gap-4 pl-5 before:absolute before:left-[5px] before:top-1 before:bottom-1 before:w-px before:bg-(--border-default)">
          <div v-for="entry in detail.timeline" :key="entry.maNkDuyet" class="relative">
            <span class="absolute -left-[21px] top-1.5 w-2.5 h-2.5 rounded-full bg-(--lg-primary) ring-4 ring-(--surface-card)"></span>
            <div class="flex items-center gap-2 flex-wrap">
              <p class="text-sm font-bold text-(--text-heading)">{{ entry.hanhDong }}</p>
              <template v-if="entry.trangThaiCu">
                <GlassBadge size="sm" variant="neutral">{{ entry.trangThaiCu }}</GlassBadge>
                <span class="text-(--text-muted) text-xs">→</span>
                <GlassBadge size="sm" variant="info">{{ entry.trangThaiMoi || entry.trangThaiCu }}</GlassBadge>
              </template>
            </div>
            <p v-if="entry.ghiChuCongKhai" class="text-sm text-(--text-muted) mt-1">{{ entry.ghiChuCongKhai }}</p>
            <p class="text-xs text-(--text-muted) mt-1">{{ entry.nguoiThucHien?.hoTen || entry.nguonThucHien }} · {{ formatDateTime(entry.ngayTao) }}</p>
          </div>
        </div>
        <p v-else class="text-sm text-(--text-muted)">Chưa có hoạt động xử lý.</p>
      </div>

      <div v-if="hasAnyAction" class="surface-card border border-(--border-card) rounded-2xl p-4 flex flex-wrap gap-2 sticky bottom-4 lg-glass-strong">
        <GlassButton v-if="actions.canReceive" variant="success" :loading="actionKey === 'receive'" @click="openModal('receive')">
          <Inbox :size="16" class="mr-1" /> Tiếp nhận đơn
        </GlassButton>
        <GlassButton v-if="actions.canRequestSupplement" variant="secondary" :loading="actionKey === 'requestSupplement'" @click="openModal('requestSupplement')">
          <MessageSquareText :size="16" class="mr-1" /> Yêu cầu bổ sung
        </GlassButton>
        <GlassButton v-if="actions.canApprove" variant="success" :loading="actionKey === 'approve'" @click="openModal('approve')">
          <CheckCircle2 :size="16" class="mr-1" /> Duyệt đơn
        </GlassButton>
        <GlassButton v-if="actions.canReject" variant="danger" :loading="actionKey === 'reject'" @click="openModal('reject')">
          <XCircle :size="16" class="mr-1" /> Từ chối
        </GlassButton>
        <GlassButton v-if="actions.canProcessAutomatically" variant="primary" :loading="actionKey === 'process'" @click="runAction('process', { rowVersion: rowVersion })">
          <Zap :size="16" class="mr-1" /> Xử lý tự động
        </GlassButton>
        <GlassButton v-if="actions.canRecordProcessingResult" variant="primary" :loading="actionKey === 'record'" @click="openModal('record')">
          <ClipboardList :size="16" class="mr-1" /> Ghi nhận kết quả
        </GlassButton>
      </div>

      <div v-if="modal" class="fixed inset-0 z-50 flex items-center justify-center p-4" role="dialog" aria-modal="true">
        <div class="absolute inset-0 bg-black/40 backdrop-blur-sm" @click="closeModal"></div>
        <div class="relative surface-card border border-(--border-card) rounded-2xl p-6 w-full max-w-lg lg-glass-strong">
          <h2 class="text-base font-bold text-(--text-heading) mb-4">{{ modalTitle }}</h2>

          <div v-if="modal === 'receive'" class="space-y-4">
            <p class="text-sm text-(--text-muted)">Xác nhận tiếp nhận đơn để bắt đầu xử lý?</p>
          </div>

          <div v-else-if="modal === 'requestSupplement'" class="space-y-4">
            <div>
              <label class="block text-xs font-semibold text-(--text-muted) mb-1.5">Nội dung yêu cầu <span class="text-(--color-danger-text)">*</span></label>
              <textarea v-model="modalForm.request" rows="4" class="w-full px-3 py-2.5 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)" placeholder="Sinh viên cần bổ sung giấy tờ..." />
            </div>
            <div>
              <label class="block text-xs font-semibold text-(--text-muted) mb-1.5">Ghi chú nội bộ</label>
              <textarea v-model="modalForm.internalNote" rows="2" class="w-full px-3 py-2.5 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)" placeholder="Chỉ nhân viên nhìn thấy" />
            </div>
          </div>

          <div v-else-if="modal === 'approve'" class="space-y-4">
            <div>
              <label class="block text-xs font-semibold text-(--text-muted) mb-1.5">Ghi chú công khai</label>
              <textarea v-model="modalForm.publicNote" rows="3" class="w-full px-3 py-2.5 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)" placeholder="Nội dung sinh viên có thể nhìn thấy" />
            </div>
            <div>
              <label class="block text-xs font-semibold text-(--text-muted) mb-1.5">Ghi chú nội bộ</label>
              <textarea v-model="modalForm.internalNote" rows="2" class="w-full px-3 py-2.5 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)" placeholder="Chỉ nhân viên nhìn thấy" />
            </div>
          </div>

          <div v-else-if="modal === 'reject'" class="space-y-4">
            <div>
              <label class="block text-xs font-semibold text-(--text-muted) mb-1.5">Lý do từ chối <span class="text-(--color-danger-text)">*</span></label>
              <textarea v-model="modalForm.reason" rows="3" class="w-full px-3 py-2.5 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--color-danger-text)" placeholder="Nêu rõ lý do sinh viên có thể nhìn thấy" />
            </div>
            <div>
              <label class="block text-xs font-semibold text-(--text-muted) mb-1.5">Ghi chú nội bộ</label>
              <textarea v-model="modalForm.internalNote" rows="2" class="w-full px-3 py-2.5 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)" placeholder="Chỉ nhân viên nhìn thấy" />
            </div>
          </div>

          <div v-else-if="modal === 'record'" class="space-y-4">
            <div>
              <label class="block text-xs font-semibold text-(--text-muted) mb-1.5">Kết quả <span class="text-(--color-danger-text)">*</span></label>
              <select v-model="modalForm.outcome" class="w-full px-3 py-2.5 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)">
                <option v-for="opt in OUTCOME_OPTIONS" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
              </select>
            </div>
            <div>
              <label class="block text-xs font-semibold text-(--text-muted) mb-1.5">Ghi chú công khai</label>
              <textarea v-model="modalForm.publicNote" rows="2" class="w-full px-3 py-2.5 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)" placeholder="Nội dung sinh viên có thể nhìn thấy" />
            </div>
            <div>
              <label class="block text-xs font-semibold text-(--text-muted) mb-1.5">Ghi chú nội bộ</label>
              <textarea v-model="modalForm.internalNote" rows="2" class="w-full px-3 py-2.5 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)" placeholder="Chỉ nhân viên nhìn thấy" />
            </div>
          </div>

          <div class="flex justify-end gap-2 mt-6">
            <GlassButton variant="secondary" @click="closeModal">Hủy</GlassButton>
            <GlassButton
              variant="primary"
              :loading="!!actionKey"
              @click="submitModal"
            >
              Xác nhận
            </GlassButton>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>



