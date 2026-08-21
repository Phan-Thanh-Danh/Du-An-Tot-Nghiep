<script setup>
import { computed, defineAsyncComponent, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { FileText, Plus, Pencil, Power, Trash2, X } from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassInput from '@/components/ui/GlassInput.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
// Lazy-load để phá vỡ circular chunk giữa FormBuilder và FormRenderer, tránh lỗi TDZ trong production build
const ApplicationFormRenderer = defineAsyncComponent(() => import('@/components/applications/ApplicationFormRenderer.vue'))
const ApplicationFormBuilder = defineAsyncComponent(() => import('@/components/applications/ApplicationFormBuilder.vue'))
import { applicationsApi } from '@/services/applicationsApi'
import { formatDateTime } from '@/utils/dateFormat'
import { usePopupStore } from '@/stores/popup'


const router = useRouter()
const popupStore = usePopupStore()
const loading = ref(true)
const saving = ref(false)
const templates = ref([])
const modalOpen = ref(false)
const modalMode = ref('create')
const previewOpen = ref(false)
const builderMode = ref('builder')
const builderKey = ref(0)

const EMPTY_SCHEMA = {
  fields: [
    { key: 'student_info', type: 'studentInfo', label: 'Thông tin sinh viên', readonly: true },
    { key: 'ly_do', type: 'textarea', label: 'Lý do', required: true },
  ],
}

const form = ref({
  loaiDon: '',
  tenMau: '',
  cauHinhJson: JSON.stringify(EMPTY_SCHEMA, null, 2),
  batBuocMinhChung: false,
  soTepToiDa: 5,
  dungLuongTepToiDaByte: 10485760,
  tongDungLuongToiDaByte: 26214400,
  slaGio: 72,
  dangHoatDong: true,
})

const jsonError = ref('')
const formError = ref('')

const previewFields = computed(() => {
  try {
    const parsed = JSON.parse(form.value.cauHinhJson)
    return Array.isArray(parsed.fields) ? parsed.fields : []
  } catch {
    return []
  }
})

const builderModel = computed({
  get: () => {
    try {
      return JSON.parse(form.value.cauHinhJson)
    } catch {
      return { fields: [] }
    }
  },
  set: (value) => {
    form.value.cauHinhJson = JSON.stringify(value, null, 2)
  },
})

function setBuilderMode(mode) {
  builderMode.value = mode
  builderKey.value += 1
}

const parseCauHinh = () => {
  try {
    const parsed = JSON.parse(form.value.cauHinhJson)
    if (!parsed || !Array.isArray(parsed.fields)) {
      throw new Error('Cấu hình phải có root.fields là mảng.')
    }
    return parsed
  } catch (err) {
    jsonError.value = err.message || 'Cấu hình không phải JSON hợp lệ.'
    return null
  }
}

async function loadTemplates() {
  loading.value = true
  try {
    templates.value = await applicationsApi.getApplicationTemplates({ includeInactive: true })
  } catch {
    popupStore.error('Lỗi', 'Không tải được danh sách mẫu đơn.')
  } finally {
    loading.value = false
  }
}



function openCreate() {
  modalMode.value = 'create'
  previewOpen.value = false
  jsonError.value = ''
  formError.value = ''
  form.value = {
    loaiDon: '',
    tenMau: '',
    cauHinhJson: JSON.stringify(EMPTY_SCHEMA, null, 2),
    batBuocMinhChung: false,
    soTepToiDa: 5,
    dungLuongTepToiDaByte: 10485760,
    tongDungLuongToiDaByte: 26214400,
    slaGio: 72,
    dangHoatDong: true,
  }
  modalOpen.value = true
}

function openEdit(template) {
  router.push(`/super-admin/approvals/requests/${encodeURIComponent(template.loaiDon)}`)
}

function closeModal() {
  modalOpen.value = false
}

async function saveTemplate() {
  jsonError.value = ''
  formError.value = ''
  if (!form.value.tenMau.trim()) {
    formError.value = 'Tên mẫu đơn không được rỗng.'
    return
  }
  if (modalMode.value === 'create' && !form.value.loaiDon.trim()) {
    formError.value = 'Mã loại đơn không được rỗng.'
    return
  }
  if (!parseCauHinh()) return

  saving.value = true
  try {
    await applicationsApi.createApplicationTemplate(form.value)
    popupStore.success('Thành công', 'Tạo mẫu đơn thành công.')
    closeModal()
    await loadTemplates()
  } catch (err) {
    popupStore.error('Lỗi', err?.message || 'Lưu mẫu đơn thất bại.')
  } finally {
    saving.value = false
  }
}

async function toggleActive(template) {
  try {
    await applicationsApi.updateApplicationTemplate(template.loaiDon, {
      tenMau: template.tenMau,
      cauHinhJson: template.cauHinhJson,
      batBuocMinhChung: template.batBuocMinhChung,
      soTepToiDa: template.soTepToiDa,
      dungLuongTepToiDaByte: template.dungLuongTepToiDaByte,
      tongDungLuongToiDaByte: template.tongDungLuongToiDaByte,
      slaGio: template.slaGio,
      dangHoatDong: !template.dangHoatDong,
    })
    popupStore.success('Thành công', template.dangHoatDong ? 'Đã tạm ẩn mẫu đơn.' : 'Mẫu đơn đã hoạt động trở lại.')
    await loadTemplates()
  } catch (err) {
    popupStore.error('Lỗi', err?.message || 'Thay đổi trạng thái thất bại.')
  }
}

const confirmDelete = ref(null)

async function deleteTemplate(template) {
  confirmDelete.value = template
}

async function executeDelete() {
  const template = confirmDelete.value
  if (!template) return
  try {
    await applicationsApi.deleteApplicationTemplate(template.loaiDon)
    confirmDelete.value = null
    popupStore.success('Thành công', 'Đã xóa mẫu đơn.')
    await loadTemplates()
  } catch (err) {
    confirmDelete.value = null
    popupStore.error('Lỗi', err?.message || 'Xóa mẫu đơn thất bại.')
  }
}

function formatBytes(bytes) {
  if (!bytes) return '0 KB'
  return `${Math.round(bytes / 1024)} KB`
}

onMounted(async () => {
  await loadTemplates()
})
</script>

<template>
  <div>
    <div class="mb-5 flex flex-wrap items-center justify-between gap-3">
      <div>
        <h2 class="text-heading text-lg font-bold">Quản lý mẫu đơn từ</h2>
        <p class="text-label text-sm">Thiết kế biểu mẫu đơn hành chính và cấu hình trường nhập liệu cho sinh viên</p>
      </div>
      <GlassButton variant="primary" @click="openCreate">
        <template #leading><Plus :size="16" /></template>
        Tạo mẫu mới
      </GlassButton>
    </div>

    <GlassPanel>
      <LoadingSkeleton v-if="loading" :lines="6" />
      <EmptyState
        v-else-if="!templates.length"
        icon="FileText"
        title="Chưa có mẫu đơn nào"
        description="Tạo mẫu đơn đầu tiên để sinh viên bắt đầu nộp đơn."
      />
      <div v-else class="overflow-x-auto">
        <table class="min-w-full text-sm">
          <thead>
            <tr class="border-b border-(--border-card) text-left text-(--text-label)">
              <th class="px-4 py-3 font-semibold">Tên mẫu</th>
              <th class="px-4 py-3 font-semibold">Loại đơn</th>
              <th class="px-4 py-3 font-semibold">Phiên bản</th>
              <th class="px-4 py-3 font-semibold">SLA (giờ)</th>
              <th class="px-4 py-3 font-semibold">Minh chứng</th>
              <th class="px-4 py-3 font-semibold">Trạng thái</th>
              <th class="px-4 py-3 font-semibold">Cập nhật lúc</th>
              <th class="px-4 py-3 text-right font-semibold">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="template in templates" :key="template.maMauDon" class="border-b border-(--border-card) last:border-0">
              <td class="px-4 py-3">
                <div class="flex items-center gap-2.5">
                  <FileText class="h-4 w-4 shrink-0 text-(--text-label)" />
                  <span class="text-heading font-medium">{{ template.tenMau }}</span>
                </div>
              </td>
              <td class="px-4 py-3">
                <GlassBadge variant="secondary">{{ template.loaiDon }}</GlassBadge>
              </td>
              <td class="px-4 py-3 text-(--text-body)">v{{ template.phienBan }}</td>
              <td class="px-4 py-3 text-(--text-body)">{{ template.slaGio ?? '—' }}</td>
              <td class="px-4 py-3 text-(--text-body)">
                <template v-if="template.batBuocMinhChung">Bắt buộc</template>
                <template v-else>1-{{ template.soTepToiDa }} tệp</template>
                <span class="text-(--text-placeholder)"> ({{ formatBytes(template.dungLuongTepToiDaByte) }}/tệp)</span>
              </td>
              <td class="px-4 py-3">
                <GlassBadge :variant="template.dangHoatDong ? 'success' : 'secondary'">
                  {{ template.dangHoatDong ? 'Đang hoạt động' : 'Tạm ẩn' }}
                </GlassBadge>
              </td>
              <td class="px-4 py-3 text-(--text-body)">{{ formatDateTime(template.ngayCapNhat) }}</td>
              <td class="px-4 py-3">
                <div class="flex items-center justify-end gap-2">
                  <GlassButton variant="secondary" size="sm" @click="openEdit(template)">
                    <template #leading><Pencil :size="14" /></template>
                    Sửa
                  </GlassButton>
                  <GlassButton variant="secondary" size="sm" @click="toggleActive(template)">
                    <template #leading><Power :size="14" /></template>
                    {{ template.dangHoatDong ? 'Ẩn' : 'Kích hoạt' }}
                  </GlassButton>
                  <GlassButton variant="danger" size="sm" @click="deleteTemplate(template)">
                    <template #leading><Trash2 :size="14" /></template>
                    Xóa
                  </GlassButton>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </GlassPanel>

    <div
      v-if="modalOpen"
      class="fixed inset-0 z-[60] flex items-center justify-center bg-black/50 p-4"
      @click.self="closeModal"
    >
      <div class="lg-glass-strong border-card flex max-h-[92vh] w-full max-w-5xl flex-col rounded-2xl border shadow-2xl">
        <div class="flex items-start justify-between gap-4 border-b border-(--border-card) p-5">
          <div>
            <h3 class="text-heading text-lg font-bold">Tạo mẫu đơn mới</h3>
            <p class="text-label mt-1 text-sm">Cấu hình trường nhập liệu được lưu trong cột cau_hinh_json</p>
          </div>
          <button class="text-(--text-placeholder) hover:text-(--text-heading)" @click="closeModal">
            <X class="h-5 w-5" />
          </button>
        </div>

        <div class="overflow-y-auto p-5">
          <form class="space-y-4" @submit.prevent="saveTemplate">
            <div v-if="modalMode === 'create'" class="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <div>
                <label class="text-label mb-1 block text-sm font-medium">Mã loại đơn *</label>
                <GlassInput
                  v-model="form.loaiDon"
                  placeholder="VD: nghi_phep, bao_luu, doi_lop"
                />
                <p class="mt-1 text-xs text-(--text-placeholder)">Dùng chữ thường (a-z), số (0-9), dấu gạch dưới (_)</p>
              </div>
              <div>
                <label class="text-label mb-1 block text-sm font-medium">Tên mẫu đơn *</label>
                <GlassInput v-model="form.tenMau" placeholder="VD: Đơn xin nghỉ phép" />
              </div>
            </div>
            <div v-else>
              <label class="text-label mb-1 block text-sm font-medium">Tên mẫu đơn *</label>
              <GlassInput v-model="form.tenMau" placeholder="VD: Đơn xin nghỉ phép" />
            </div>

            <div class="grid grid-cols-2 gap-4 sm:grid-cols-4">
              <div>
                <label class="text-label mb-1 block text-sm font-medium">SLA (giờ)</label>
                <GlassInput v-model.number="form.slaGio" type="number" min="1" />
              </div>
              <div>
                <label class="text-label mb-1 block text-sm font-medium">Số tệp tối đa</label>
                <GlassInput v-model.number="form.soTepToiDa" type="number" min="1" max="10" />
              </div>
              <div>
                <label class="text-label mb-1 block text-sm font-medium">Dung lượng/tệp (KB)</label>
                <GlassInput v-model.number="form.dungLuongTepToiDaByte" type="number" min="1024" />
              </div>
              <div>
                <label class="text-label mb-1 block text-sm font-medium">Tổng dung lượng (KB)</label>
                <GlassInput v-model.number="form.tongDungLuongToiDaByte" type="number" min="1024" />
              </div>
            </div>

            <div class="flex flex-wrap gap-6">
              <label class="flex items-center gap-2 text-sm text-(--text-body)">
                <input v-model="form.batBuocMinhChung" type="checkbox" class="accent-(--lg-primary)" />
                Bắt buộc minh chứng
              </label>
              <label class="flex items-center gap-2 text-sm text-(--text-body)">
                <input v-model="form.dangHoatDong" type="checkbox" class="accent-(--lg-primary)" />
                Đang hoạt động (sinh viên có thể nộp)
              </label>
            </div>

            <div>
              <div class="mb-2 flex flex-wrap items-center justify-between gap-2">
                <label class="text-label text-sm font-medium">Cấu hình biểu mẫu *</label>
                <div class="flex items-center gap-3">
                  <div class="flex overflow-hidden rounded-lg border border-(--border-card)">
                    <button
                      :class="builderMode === 'builder' ? 'bg-(--color-info-bg) text-(--color-info-text)' : 'text-(--text-placeholder)'"
                      class="px-3 py-1.5 text-xs font-medium transition-colors"
                      @click="setBuilderMode('builder')"
                    >
                      Kéo thả
                    </button>
                    <button
                      :class="builderMode === 'json' ? 'bg-(--color-info-bg) text-(--color-info-text)' : 'text-(--text-placeholder)'"
                      class="border-(--border-card) border-l px-3 py-1.5 text-xs font-medium transition-colors"
                      @click="setBuilderMode('json')"
                    >
                      JSON
                    </button>
                  </div>
                  <GlassButton variant="ghost" size="sm" type="button" @click="previewOpen = !previewOpen">
                    {{ previewOpen ? 'Ẩn xem trước' : 'Xem trước biểu mẫu' }}
                  </GlassButton>
                </div>
              </div>

              <ApplicationFormBuilder
                v-if="builderMode === 'builder'"
                :key="builderKey"
                v-model="builderModel"
              />

              <template v-else>
                <textarea
                  v-model="form.cauHinhJson"
                  rows="12"
                  spellcheck="false"
                  class="w-full px-3 py-2 bg-(--surface-input) border border-(--border-input) rounded-lg focus:ring-2 focus:ring-(--lg-primary) outline-none text-(--text-body) transition-all resize-y font-mono text-xs"
                  :class="{ 'border-(--color-danger-border)': jsonError }"
                ></textarea>
                <p class="text-placeholder mt-1 text-xs">
                  Cấu trúc: root.fields là mảng, mỗi field có key, label, type (text/textarea/number/date/email/tel/select/multiselect/boolean/studentInfo).
                </p>
              </template>
              <p v-if="jsonError" class="mt-1 text-sm text-(--color-danger-text)">{{ jsonError }}</p>
            </div>

            <GlassPanel v-if="previewOpen && previewFields.length" variant="flat" density="compact">
              <p class="text-label mb-3 text-sm font-semibold">Xem trước:</p>
              <ApplicationFormRenderer :schema="previewFields" :model-value="{ student_info: 'Sinh viên (tự động)' }" readonly />
            </GlassPanel>

            <p v-if="formError" class="text-sm text-(--color-danger-text)">{{ formError }}</p>

            <div class="flex justify-end gap-3 pt-1">
              <GlassButton variant="secondary" @click="closeModal">Hủy</GlassButton>
              <GlassButton variant="primary" :disabled="saving" @click="saveTemplate">
                {{ saving ? 'Đang lưu...' : 'Lưu mẫu đơn' }}
              </GlassButton>
            </div>
          </form>
        </div>
      </div>
    </div>

    <ConfirmActionDialog
      v-if="confirmDelete"
      :model-value="true"
      title="Xóa mẫu đơn"
      :message="`Bạn có chắc muốn xóa mẫu &quot;${confirmDelete.tenMau}&quot; (${confirmDelete.loaiDon})? Mẫu đang được đơn từ sử dụng sẽ không thể xóa.`"
      confirm-label="Xóa"
      variant="danger"
      @confirm="executeDelete"
      @cancel="confirmDelete = null"
    />
  </div>
</template>
