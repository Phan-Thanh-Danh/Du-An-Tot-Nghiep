<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import {
  AlertTriangle,
  CheckCircle2,
  ClipboardCheck,
  Eye,
  FileText,
  RotateCcw,
  Search,
  Send,
  UserCheck,
  XCircle,
} from 'lucide-vue-next'

import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'
import TableShell from '@/components/ui/TableShell.vue'
import { applicationsApi } from '@/services/applicationsApi'
import { usePopupStore } from '@/stores/popup'
import { formatDate, formatDateTime } from '@/utils/dateFormat'
import { getStatusMeta, getStatusOptions } from '@/utils/statusLabels'

const popupStore = usePopupStore()
const applicationTypes = ref([])
const assignableUsers = ref([])

function getApplicationTypeLabel(type) {
  return applicationTypes.value.find((item) => item.value === type)?.label || type || 'Không xác định'
}

const loading = ref(false)
const error = ref('')
const applications = ref([])
const selectedId = ref(applications.value[0]?.id || '')
const searchQuery = ref('')
const statusFilter = ref('')
const typeFilter = ref('')
const assigneeFilter = ref('')
const slaFilter = ref('')
const actionMode = ref('')
const actionText = ref('')
const assigneeId = ref('')
const actionSubmitted = ref(false)
const confirmAction = ref(null)

const statusOptions = getStatusOptions('applications').filter((option) =>
  ['da_nop', 'dang_xem_xet', 'yeu_cau_bo_sung', 'da_duyet', 'tu_choi', 'da_huy'].includes(option.value),
)

const filteredApplications = computed(() => {
  const query = searchQuery.value.trim().toLowerCase()
  return applications.value.filter((item) => {
    const matchesSearch =
      !query ||
      item.id.toLowerCase().includes(query) ||
      item.tieuDe.toLowerCase().includes(query) ||
      item.sinhVien.toLowerCase().includes(query) ||
      item.maSinhVien.toLowerCase().includes(query)
    const matchesStatus = !statusFilter.value || item.trangThai === statusFilter.value
    const matchesType = !typeFilter.value || item.loaiDon === typeFilter.value
    const matchesAssignee = !assigneeFilter.value || item.nguoiXuLyId === assigneeFilter.value
    const matchesSla = !slaFilter.value || item.sla === slaFilter.value
    return matchesSearch && matchesStatus && matchesType && matchesAssignee && matchesSla
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
  {
    label: 'Đang xử lý',
    value: applications.value.filter((item) => ['da_nop', 'dang_xem_xet'].includes(item.trangThai)).length,
    icon: ClipboardCheck,
    variant: 'info',
  },
  {
    label: 'Chờ tiếp nhận',
    value: applications.value.filter((item) => item.trangThai === 'da_nop').length,
    icon: FileText,
    variant: 'primary',
  },
  {
    label: 'Cần bổ sung',
    value: applications.value.filter((item) => item.trangThai === 'yeu_cau_bo_sung').length,
    icon: RotateCcw,
    variant: 'warning',
  },
  {
    label: 'Quá hạn',
    value: applications.value.filter((item) => item.sla === 'qua_han').length,
    icon: AlertTriangle,
    variant: 'danger',
  },
])

const actionError = computed(() => {
  if (!actionSubmitted.value || !actionMode.value) return ''
  if (['supplement', 'reject'].includes(actionMode.value) && !actionText.value.trim()) {
    return actionMode.value === 'reject' ? 'Vui lòng nhập lý do từ chối.' : 'Vui lòng nhập nội dung cần bổ sung.'
  }
  if (['assign', 'reassign'].includes(actionMode.value) && !assigneeId.value) {
    return 'Vui lòng chọn người xử lý.'
  }
  return ''
})

function statusMeta(status) {
  return getStatusMeta('applications', status)
}

function slaMeta(sla) {
  if (sla === 'qua_han') return { label: 'Quá hạn', variant: 'danger' }
  if (sla === 'sap_qua_han') return { label: 'Sắp quá hạn', variant: 'warning' }
  return { label: 'Đúng hạn', variant: 'success' }
}

function selectApplication(id) {
  selectedId.value = id
  closeActionForm()
}

function resetFilters() {
  searchQuery.value = ''
  statusFilter.value = ''
  typeFilter.value = ''
  assigneeFilter.value = ''
  slaFilter.value = ''
}

function closeActionForm() {
  actionMode.value = ''
  actionText.value = ''
  actionSubmitted.value = false
}

function openAction(mode) {
  actionMode.value = mode
  actionText.value = ''
  actionSubmitted.value = false
  assigneeId.value = selectedApplication.value?.nguoiXuLyId || assignableUsers.value[0]?.id || ''
}

async function receiveSelected() {
  if (!selectedApplication.value) return
  try {
    await applicationsApi.receiveApplication(selectedApplication.value.id, {
      rowVersion: selectedApplication.value.rowVersion || '',
    })
    await loadApplications()
    popupStore.success('Đã tiếp nhận đơn', `${selectedApplication.value.id} đã chuyển sang trạng thái đang xem xét.`)
  } catch (e) {
    popupStore.error('Lỗi', e.message || 'Không thể tiếp nhận đơn.')
  }
}

async function applyAction() {
  actionSubmitted.value = true
  if (actionError.value || !selectedApplication.value) return

  const application = selectedApplication.value
  const rowVersion = application.rowVersion || ''
  const assigneeName = assignableUsers.value.find((item) => item.id === assigneeId.value)?.name || ''

  try {
    if (actionMode.value === 'assign') {
      await applicationsApi.assignApplication(application.id, { assigneeId: Number(assigneeId.value), rowVersion })
      popupStore.success('Đã phân công', `Đơn được giao cho ${assigneeName}.`)
    } else if (actionMode.value === 'reassign') {
      await applicationsApi.reassignApplication(application.id, {
        assigneeId: Number(assigneeId.value),
        rowVersion,
        lyDo: actionText.value.trim() || 'Phân công lại từ màn hàng đợi.',
      })
      popupStore.success('Đã phân công lại', `Đơn được giao cho ${assigneeName}.`)
    } else if (actionMode.value === 'supplement') {
      await applicationsApi.requestSupplement(application.id, {
        request: actionText.value.trim(),
        rowVersion,
      })
      popupStore.warning('Đã yêu cầu bổ sung', 'Nội dung yêu cầu đã được cập nhật.')
    } else if (actionMode.value === 'reject') {
      confirmAction.value = {
        title: 'Từ chối đơn này?',
        message: 'Sinh viên sẽ thấy lý do từ chối trong phần chi tiết đơn.',
        label: 'Từ chối đơn',
        variant: 'danger',
        run: async () => {
          try {
            await applicationsApi.rejectApplication(application.id, {
              reason: actionText.value.trim(),
              rowVersion,
            })
            confirmAction.value = null
            closeActionForm()
            await loadApplications()
            popupStore.error('Đã từ chối đơn', 'Trạng thái đơn đã được cập nhật.')
          } catch (e) {
            popupStore.error('Lỗi', e.message || 'Không thể từ chối đơn.')
          }
        },
      }
      return
    }
    closeActionForm()
    await loadApplications()
  } catch (e) {
    popupStore.error('Lỗi', e.message || 'Không thể lưu thao tác.')
  }
}

function approveSelected() {
  if (!selectedApplication.value) return
  const application = selectedApplication.value
  confirmAction.value = {
    title: 'Duyệt đơn này?',
    message: 'Đơn sẽ chuyển sang trạng thái đã duyệt trên hệ thống.',
    label: 'Duyệt đơn',
    variant: 'success',
    run: async () => {
      try {
        await applicationsApi.approveApplication(application.id, { rowVersion: application.rowVersion || '' })
        confirmAction.value = null
        await loadApplications()
        popupStore.success('Đã duyệt đơn', 'Đơn đã được phê duyệt.')
      } catch (e) {
        popupStore.error('Lỗi', e.message || 'Không thể duyệt đơn.')
      }
    },
  }
}

async function processAfterApproval() {
  if (!selectedApplication.value) return
  try {
    await applicationsApi.processApprovedApplication(selectedApplication.value.id, {
      rowVersion: selectedApplication.value.rowVersion || '',
    })
    await loadApplications()
    popupStore.info('Xử lý sau duyệt', 'Đã gửi yêu cầu xử lý sau duyệt.')
  } catch (e) {
    popupStore.error('Lỗi', e.message || 'Không thể xử lý sau duyệt.')
  }
}

function isFinal(application) {
  return ['da_duyet', 'tu_choi', 'da_huy'].includes(application?.trangThai)
}

function retryLoad() {
  loadApplications()
}

function unwrapList(payload) {
  const data = payload?.data ?? payload?.Data ?? payload
  if (Array.isArray(data)) return data
  if (Array.isArray(data?.items)) return data.items
  if (Array.isArray(data?.Items)) return data.Items
  return []
}

function getFormValue(data, key) {
  return data?.[key] ?? data?.[key.charAt(0).toUpperCase() + key.slice(1)] ?? ''
}

function mapApplication(item) {
  const id = String(item.maDonTu ?? item.MaDonTu ?? item.id ?? '')
  const formData = item.duLieuBieuMau ?? item.DuLieuBieuMau ?? {}
  const attachments = item.attachments ?? item.Attachments ?? []
  return {
    id,
    rowVersion: item.rowVersion ?? item.RowVersion ?? '',
    tieuDe: item.tieuDe ?? item.TieuDe ?? '',
    loaiDon: item.loaiDon ?? item.LoaiDon ?? '',
    trangThai: item.trangThai ?? item.TrangThai ?? '',
    sinhVien: item.tenSinhVien ?? item.TenSinhVien ?? item.sinhVien ?? item.SinhVien ?? 'Sinh viên',
    maSinhVien: String(item.maSinhVien ?? item.MaSinhVien ?? ''),
    nguoiXuLy: item.tenNguoiXuLy ?? item.TenNguoiXuLy ?? item.nguoiXuLy ?? item.NguoiXuLy ?? '',
    nguoiXuLyId: item.nguoiXuLyId ?? item.NguoiXuLyId ?? '',
    ngayNop: item.ngayNop ?? item.NgayNop,
    hanXuLy: item.hanXuLyLuc ?? item.HanXuLyLuc,
    capNhatLanCuoi: item.ngayCapNhat ?? item.NgayCapNhat,
    sla: item.slaTrangThai ?? item.SlaTrangThai ?? 'dung_han',
    noiDungYeuCauBoSung: item.noiDungYeuCauBoSung ?? item.NoiDungYeuCauBoSung ?? '',
    lyDoTuChoi: item.lyDoTuChoi ?? item.LyDoTuChoi ?? '',
    formData: [
      { label: 'Loại đơn', value: item.tenLoaiDon ?? item.TenLoaiDon ?? getApplicationTypeLabel(item.loaiDon ?? item.LoaiDon) },
      { label: 'Nội dung yêu cầu', value: getFormValue(formData, 'noiDungYeuCau') || '—' },
    ],
    evidence: attachments.map((file) => ({
      id: file.maTep ?? file.MaTep,
      name: file.tenFileGoc ?? file.TenFileGoc ?? '',
      size: file.kichThuocByte ? `${Math.round(file.kichThuocByte / 1024)} KB` : '',
      uploadedAt: file.ngayTao ?? file.NgayTao,
    })),
  }
}

async function loadApplications() {
  loading.value = true
  error.value = ''
  try {
    const [templates, users, queue] = await Promise.all([
      applicationsApi.getApplicationTemplates({ pageSize: 50 }).catch(() => []),
      applicationsApi.getAssignableUsers({ pageSize: 100 }).catch(() => []),
      applicationsApi.getAdminApplications({ pageSize: 50 }),
    ])
    applicationTypes.value = unwrapList(templates)
      .map((item) => ({
        value: item.loaiDon ?? item.LoaiDon,
        label: item.tenLoaiDon ?? item.TenLoaiDon ?? item.tenMau ?? item.TenMau,
      }))
      .filter((item) => item.value)
    assignableUsers.value = unwrapList(users)
      .map((item) => ({
        id: String(item.id ?? item.maNguoiDung ?? item.MaNguoiDung ?? ''),
        name: item.name ?? item.hoTen ?? item.HoTen ?? item.email ?? item.Email ?? '',
      }))
      .filter((item) => item.id)
    applications.value = unwrapList(queue).map(mapApplication)
    selectedId.value = applications.value[0]?.id || ''
  } catch (e) {
    error.value = e.message || 'Không tải được hàng đợi.'
    applications.value = []
  } finally {
    loading.value = false
  }
}

onMounted(loadApplications)
</script>

<template>
  <div class="admin-applications-page mx-auto max-w-7xl space-y-5">
    <GlassPanel variant="flat" density="compact" class="page-header">
      <div class="header-copy">
        <p class="eyebrow">
          <ClipboardCheck :size="15" />
          Hàng đợi xử lý
        </p>
        <div>
          <h1>Hàng đợi đơn từ</h1>
          <p>Tiếp nhận, phân công, yêu cầu bổ sung, duyệt hoặc từ chối đơn theo phạm vi quản trị.</p>
        </div>
      </div>
      <GlassBadge variant="info" size="md">API-backed</GlassBadge>
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

    <GlassPanel variant="flat" density="compact" class="filter-panel">
      <div class="filter-grid">
        <label class="control-field">
          <span>Tìm kiếm</span>
          <span class="search-control">
            <Search :size="15" />
            <input v-model="searchQuery" type="text" placeholder="Sinh viên, mã đơn, tiêu đề" />
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
        <label class="control-field">
          <span>Người xử lý</span>
          <select v-model="assigneeFilter" class="lg-control">
            <option value="">Tất cả</option>
            <option v-for="user in assignableUsers" :key="user.id" :value="user.id">{{ user.name }}</option>
          </select>
        </label>
        <label class="control-field">
          <span>SLA</span>
          <select v-model="slaFilter" class="lg-control">
            <option value="">Tất cả</option>
            <option value="dung_han">Đúng hạn</option>
            <option value="sap_qua_han">Sắp quá hạn</option>
            <option value="qua_han">Quá hạn</option>
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
        <h2>Không tải được hàng đợi</h2>
        <p>{{ error }}</p>
      </div>
      <GlassButton variant="secondary" @click="retryLoad">Thử lại</GlassButton>
    </GlassPanel>

    <section v-else class="queue-grid">
      <GlassPanel variant="flat" density="compact" class="queue-panel">
        <div class="panel-heading">
          <div>
            <h2>Danh sách đơn cần xử lý</h2>
            <p>Hiển thị {{ filteredApplications.length }} đơn theo bộ lọc hiện tại.</p>
          </div>
          <GlassBadge variant="primary">Queue</GlassBadge>
        </div>

        <TableShell v-if="filteredApplications.length" density="compact" class="queue-table">
          <table>
            <thead>
              <tr>
                <th>Mã đơn</th>
                <th>Sinh viên</th>
                <th>Loại đơn</th>
                <th>Tiêu đề</th>
                <th>Trạng thái</th>
                <th>Người xử lý</th>
                <th>SLA / hạn</th>
                <th>Ngày nộp</th>
                <th>Thao tác</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="application in filteredApplications"
                :key="application.id"
                :class="application.id === selectedApplication?.id ? 'is-selected' : ''"
              >
                <td class="code-cell">{{ application.id }}</td>
                <td>
                  <div class="student-cell">
                    <strong class="clamp-1">{{ application.sinhVien }}</strong>
                    <small>{{ application.maSinhVien }}</small>
                  </div>
                </td>
                <td><span class="muted-cell clamp-1">{{ getApplicationTypeLabel(application.loaiDon) }}</span></td>
                <td><strong class="title-cell clamp-2">{{ application.tieuDe }}</strong></td>
                <td>
                  <GlassBadge :variant="statusMeta(application.trangThai).variant">
                    {{ statusMeta(application.trangThai).label }}
                  </GlassBadge>
                </td>
                <td><span class="muted-cell clamp-1">{{ application.nguoiXuLy || 'Chưa phân công' }}</span></td>
                <td>
                  <span class="sla-cell">
                    <GlassBadge :variant="slaMeta(application.sla).variant">{{ slaMeta(application.sla).label }}</GlassBadge>
                    <small>{{ formatDate(application.hanXuLy, 'Chưa có hạn') }}</small>
                  </span>
                </td>
                <td>{{ formatDate(application.ngayNop, 'Chưa nộp') }}</td>
                <td>
                  <GlassButton variant="secondary" size="sm" @click="selectApplication(application.id)">
                    <template #leading><Eye :size="14" /></template>
                    Xem
                  </GlassButton>
                </td>
              </tr>
            </tbody>
          </table>
        </TableShell>

        <div v-if="filteredApplications.length" class="queue-mobile-list">
          <button
            v-for="application in filteredApplications"
            :key="application.id"
            type="button"
            :class="['queue-mobile-card', application.id === selectedApplication?.id ? 'is-selected' : '']"
            @click="selectApplication(application.id)"
          >
            <span class="mobile-card-top">
              <span>{{ application.id }}</span>
              <GlassBadge :variant="statusMeta(application.trangThai).variant">
                {{ statusMeta(application.trangThai).label }}
              </GlassBadge>
            </span>
            <strong class="clamp-2">{{ application.tieuDe }}</strong>
            <span class="muted-cell clamp-1">{{ application.sinhVien }} · {{ getApplicationTypeLabel(application.loaiDon) }}</span>
            <span class="mobile-card-top">
              <GlassBadge :variant="slaMeta(application.sla).variant">{{ slaMeta(application.sla).label }}</GlassBadge>
              <span>{{ formatDate(application.ngayNop, 'Chưa nộp') }}</span>
            </span>
          </button>
        </div>

        <EmptyState
          v-else
          title="Không có đơn phù hợp"
          description="Thử xóa bộ lọc hoặc đổi trạng thái cần xem."
        >
          <GlassButton variant="secondary" @click="resetFilters">Xóa bộ lọc</GlassButton>
        </EmptyState>
      </GlassPanel>

      <GlassPanel v-if="selectedApplication" variant="flat" density="compact" class="detail-panel">
        <div class="detail-header">
          <div>
            <h2 class="clamp-2">{{ selectedApplication.tieuDe }}</h2>
            <p>{{ selectedApplication.id }} · {{ selectedApplication.sinhVien }} · {{ selectedApplication.maSinhVien }}</p>
          </div>
          <GlassBadge :variant="statusMeta(selectedApplication.trangThai).variant" size="md">
            {{ statusMeta(selectedApplication.trangThai).label }}
          </GlassBadge>
        </div>

        <div class="detail-actions">
          <GlassButton variant="primary" :disabled="selectedApplication.trangThai !== 'da_nop'" @click="receiveSelected">
            <template #leading><UserCheck :size="16" /></template>
            Tiếp nhận
          </GlassButton>
          <GlassButton variant="secondary" :disabled="isFinal(selectedApplication)" @click="openAction(selectedApplication.nguoiXuLy ? 'reassign' : 'assign')">
            Phân công
          </GlassButton>
          <GlassButton variant="secondary" :disabled="isFinal(selectedApplication)" @click="openAction('supplement')">
            Bổ sung
          </GlassButton>
          <GlassButton variant="success" :disabled="isFinal(selectedApplication)" @click="approveSelected">
            <template #leading><CheckCircle2 :size="16" /></template>
            Duyệt
          </GlassButton>
          <GlassButton variant="danger" :disabled="isFinal(selectedApplication)" @click="openAction('reject')">
            <template #leading><XCircle :size="16" /></template>
            Từ chối
          </GlassButton>
          <GlassButton variant="ghost" :disabled="selectedApplication.trangThai !== 'da_duyet'" @click="processAfterApproval">
            Sau duyệt
          </GlassButton>
        </div>

        <div v-if="actionMode" class="action-form">
          <div class="action-form-header">
            <strong>
              {{
                actionMode === 'supplement'
                  ? 'Yêu cầu bổ sung'
                  : actionMode === 'reject'
                    ? 'Từ chối đơn'
                    : 'Phân công xử lý'
              }}
            </strong>
            <button type="button" @click="closeActionForm">Đóng</button>
          </div>
          <div v-if="actionError" class="form-error-summary">{{ actionError }}</div>
          <label v-if="['assign', 'reassign'].includes(actionMode)" class="form-field">
            <span>Người xử lý</span>
            <select v-model="assigneeId" class="lg-control">
              <option v-for="user in assignableUsers" :key="user.id" :value="user.id">{{ user.name }}</option>
            </select>
          </label>
          <label v-if="['supplement', 'reject'].includes(actionMode)" class="form-field">
            <span>{{ actionMode === 'reject' ? 'Lý do từ chối' : 'Nội dung cần bổ sung' }}</span>
            <textarea v-model="actionText" rows="4" placeholder="Nhập nội dung xử lý..." />
          </label>
          <GlassButton variant="primary" @click="applyAction">
            <template #leading><Send :size="16" /></template>
            Lưu thao tác
          </GlassButton>
        </div>

        <div class="info-grid">
          <div><span>Loại đơn</span><strong>{{ getApplicationTypeLabel(selectedApplication.loaiDon) }}</strong></div>
          <div><span>Ngày nộp</span><strong>{{ formatDate(selectedApplication.ngayNop, 'Chưa nộp') }}</strong></div>
          <div><span>Hạn xử lý</span><strong>{{ formatDate(selectedApplication.hanXuLy, 'Chưa có hạn') }}</strong></div>
          <div><span>Người xử lý</span><strong>{{ selectedApplication.nguoiXuLy || 'Chưa phân công' }}</strong></div>
        </div>

        <section class="detail-section">
          <h3>Thông tin đơn</h3>
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
              <FileText :size="15" />
              <span class="min-w-0">
                <strong class="clamp-1">{{ file.name }}</strong>
                <small>{{ file.size }} · {{ formatDate(file.uploadedAt) }}</small>
              </span>
            </div>
          </div>
          <p v-else>Không có minh chứng đính kèm.</p>
        </section>

        <section class="detail-section">
          <h3>Timeline</h3>
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
.admin-applications-page {
  color: var(--text-body);
}

.page-header,
.summary-card,
.panel-heading,
.detail-header,
.detail-actions,
.mobile-card-top,
.evidence-item,
.action-form-header,
.error-panel {
  display: flex;
  align-items: center;
}

.page-header,
.panel-heading,
.detail-header,
.action-form-header,
.error-panel {
  justify-content: space-between;
  gap: 1rem;
}

.header-copy,
.panel-heading > div,
.detail-header > div {
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
.readonly-list span {
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
  grid-template-columns: minmax(0, 1.35fr) repeat(4, minmax(0, 0.85fr)) auto;
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

.lg-control {
  padding: 0 0.75rem;
}

.form-field textarea {
  padding: 0.75rem;
  resize: vertical;
}

.search-control:focus-within,
.lg-control:focus,
.form-field textarea:focus {
  border-color: var(--border-input-focus);
  background: var(--surface-input-focus);
  box-shadow: 0 0 0 3px var(--border-focus-ring);
}

.queue-grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr);
  gap: 0.875rem;
}

.queue-panel,
.detail-panel {
  min-width: 0;
}

.queue-table {
  margin-top: 0.875rem;
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

tbody tr:hover,
tbody tr.is-selected {
  background: var(--surface-table-row-hover);
}

tbody tr.is-selected {
  box-shadow: inset 3px 0 0 var(--text-link);
}

tbody td {
  height: 4.875rem;
  overflow: hidden;
}

th,
td {
  padding: 0.625rem 0.75rem;
  vertical-align: middle;
}

.code-cell,
.title-cell,
.student-cell strong,
.info-grid strong,
.readonly-list strong,
.timeline strong,
.evidence-item strong {
  color: var(--text-heading);
  font-size: 0.84375rem;
  font-weight: 850;
}

.student-cell small,
.muted-cell,
.sla-cell small,
.timeline small,
.evidence-item small {
  color: var(--text-muted);
  font-size: 0.78125rem;
  font-weight: 750;
}

.sla-cell {
  display: grid;
  gap: 0.25rem;
}

.queue-mobile-list {
  display: none;
}

.queue-mobile-card {
  display: grid;
  min-height: 9.5rem;
  gap: 0.5rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-lg);
  background: var(--surface-card);
  padding: 0.75rem;
  text-align: left;
}

.queue-mobile-card.is-selected {
  border-color: var(--border-input-focus);
  box-shadow: inset 3px 0 0 var(--text-link);
}

.mobile-card-top {
  justify-content: space-between;
  gap: 0.5rem;
  color: var(--text-muted);
  font-size: 0.78125rem;
  font-weight: 850;
}

.queue-mobile-card strong {
  color: var(--text-heading);
  font-size: 0.9375rem;
  font-weight: 900;
  line-height: 1.35;
}

.detail-panel {
  display: grid;
  gap: 0.875rem;
}

.detail-actions {
  flex-wrap: wrap;
  gap: 0.5rem;
}

.action-form,
.info-grid,
.readonly-list,
.timeline,
.evidence-list {
  display: grid;
  gap: 0.5rem;
}

.action-form,
.info-grid div,
.readonly-list div,
.timeline p,
.evidence-item {
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  padding: 0.625rem;
}

.action-form-header button {
  border: 0;
  background: transparent;
  color: var(--text-link);
  font-weight: 850;
}

.form-error-summary {
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--color-danger-bg);
  color: var(--color-danger-text);
  padding: 0.625rem 0.75rem;
  font-weight: 800;
}

.info-grid {
  grid-template-columns: repeat(4, minmax(0, 1fr));
}

.detail-section {
  display: grid;
  gap: 0.625rem;
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
  .info-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .filter-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 760px) {
  .page-header,
  .panel-heading,
  .detail-header,
  .error-panel {
    flex-direction: column;
    align-items: stretch;
  }

  .summary-grid,
  .filter-grid,
  .info-grid {
    grid-template-columns: 1fr;
  }

  .queue-table {
    display: none;
  }

  .queue-mobile-list {
    display: grid;
    gap: 0.625rem;
    margin-top: 0.875rem;
  }

  .detail-actions {
    display: grid;
    grid-template-columns: 1fr;
  }
}
</style>
