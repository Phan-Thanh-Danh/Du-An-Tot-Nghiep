<script setup>
import { computed, defineAsyncComponent, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ArrowLeft, Eye, FileText, Save } from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassInput from '@/components/ui/GlassInput.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'

// Lazy-load để phá vỡ circular chunk giữa FormBuilder và FormRenderer, tránh lỗi TDZ trong production build
const ApplicationFormBuilder = defineAsyncComponent(() => import('@/components/applications/ApplicationFormBuilder.vue'))
const ApplicationFormRenderer = defineAsyncComponent(() => import('@/components/applications/ApplicationFormRenderer.vue'))
import { applicationsApi } from '@/services/applicationsApi'
import { usePopupStore } from '@/stores/popup'

const route = useRoute()
const router = useRouter()
const popupStore = usePopupStore()

const loading = ref(true)
const saving = ref(false)
const previewOpen = ref(true)
const template = ref(null)
const builderKey = ref(0)
const formError = ref('')

const loaiDon = computed(() => String(route.params.loaiDon || ''))

const form = ref({
  tenMau: '',
  cauHinhJson: '',
  batBuocMinhChung: false,
  soTepToiDa: 5,
  dungLuongTepToiDaByte: 10485760,
  tongDungLuongToiDaByte: 26214400,
  slaGio: 72,
  dangHoatDong: true,
})

const previewFields = computed(() => {
  try {
    const parsed = JSON.parse(form.value.cauHinhJson)
    return Array.isArray(parsed.fields) ? parsed.fields : []
  } catch {
    return []
  }
})

const previewModel = ref({})
const formBuilderRef = ref(null)

function handleEditFieldFromPreview(field) {
  // Tìm field tương ứng có __id để mở edit dialog của builder
  if (!formBuilderRef.value) return
  const fields = formBuilderRef.value.fields.value
  const target = fields.find(f => f.key === field.key)
  if (target) {
    formBuilderRef.value.openEditDialog(target)
  }
}

function handleDeleteFieldFromPreview(field) {
  if (!formBuilderRef.value) return
  const fields = formBuilderRef.value.fields.value
  const target = fields.find(f => f.key === field.key)
  if (target) {
    formBuilderRef.value.removeField(target)
  }
}

function handleAddFieldFromPreview() {
  if (!formBuilderRef.value) return
  formBuilderRef.value.openAddDialog()
}

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

async function loadTemplate() {
  loading.value = true
  try {
    const data = await applicationsApi.getApplicationTemplateDetail(loaiDon.value)
    if (!data) {
      popupStore.error('Lỗi', 'Không tìm thấy mẫu đơn.')
      router.replace('/super-admin/approvals/requests')
      return
    }
    template.value = data
    form.value = {
      tenMau: data.tenMau || '',
      cauHinhJson: data.cauHinhJson || '',
      batBuocMinhChung: Boolean(data.batBuocMinhChung),
      soTepToiDa: data.soTepToiDa ?? 5,
      dungLuongTepToiDaByte: data.dungLuongTepToiDaByte ?? 10485760,
      tongDungLuongToiDaByte: data.tongDungLuongToiDaByte ?? 26214400,
      slaGio: data.slaGio ?? 72,
      dangHoatDong: data.dangHoatDong !== false,
    }
    builderKey.value += 1
  } catch (err) {
    popupStore.error('Lỗi', err?.message || 'Không tải được mẫu đơn.')
  } finally {
    loading.value = false
  }
}

function goBack() {
  router.push('/super-admin/approvals/requests')
}

async function saveTemplate(andClose = false) {
  formError.value = ''
  if (!form.value.tenMau.trim()) {
    formError.value = 'Tên mẫu đơn không được rỗng.'
    return
  }
  try {
    const parsed = JSON.parse(form.value.cauHinhJson)
    if (!parsed || !Array.isArray(parsed.fields)) {
      throw new Error('Cấu hình phải có root.fields là mảng.')
    }
  } catch (err) {
    formError.value = err?.message || 'Cấu hình biểu mẫu không hợp lệ.'
    return
  }

  saving.value = true
  try {
    await applicationsApi.updateApplicationTemplate(loaiDon.value, form.value)
    popupStore.success('Thành công', 'Cập nhật mẫu đơn thành công.')
    if (andClose) goBack()
  } catch (err) {
    popupStore.error('Lỗi', err?.message || 'Lưu mẫu đơn thất bại.')
  } finally {
    saving.value = false
  }
}

onMounted(loadTemplate)
</script>

<template>
  <div class="space-y-4">
    <div class="flex flex-wrap items-center justify-between gap-3">
      <div class="flex items-center gap-3">
        <GlassButton variant="ghost" size="sm" @click="goBack">
          <template #leading><ArrowLeft :size="16" /></template>
          Quay lại
        </GlassButton>
        <div>
          <div class="flex flex-wrap items-center gap-2">
            <h2 class="text-heading text-lg font-bold">{{ form.tenMau || 'Sửa mẫu đơn' }}</h2>
            <GlassBadge variant="secondary">{{ loaiDon }}</GlassBadge>
            <GlassBadge :variant="form.dangHoatDong ? 'success' : 'secondary'">
              {{ form.dangHoatDong ? 'Đang hoạt động' : 'Tạm ẩn' }}
            </GlassBadge>
          </div>
          <p class="text-label mt-0.5 text-sm">
            {{ template ? `Phiên bản v${template.phienBan}` : 'Chỉnh sửa biểu mẫu đơn từ' }}
          </p>
        </div>
      </div>
      <div class="flex items-center gap-2">
        <GlassButton variant="secondary" @click="previewOpen = !previewOpen">
          <template #leading><Eye :size="16" /></template>
          {{ previewOpen ? 'Ẩn xem trước' : 'Xem trước biểu mẫu' }}
        </GlassButton>
        <GlassButton variant="primary" :disabled="saving" @click="saveTemplate(false)">
          <template #leading><Save :size="16" /></template>
          {{ saving ? 'Đang lưu...' : 'Lưu' }}
        </GlassButton>
        <GlassButton variant="secondary" :disabled="saving" @click="saveTemplate(true)">
          {{ saving ? 'Đang lưu...' : 'Lưu & quay lại' }}
        </GlassButton>
      </div>
    </div>

    <LoadingSkeleton v-if="loading" :lines="8" />

    <template v-else>
      <GlassPanel>
        <div class="grid grid-cols-2 gap-4 lg:grid-cols-5">
          <div class="lg:col-span-2">
            <label class="text-label mb-1 block text-sm font-medium">Tên mẫu đơn *</label>
            <GlassInput v-model="form.tenMau" placeholder="VD: Đơn xin nghỉ phép" />
          </div>
          <div>
            <label class="text-label mb-1 block text-sm font-medium">SLA (giờ)</label>
            <GlassInput v-model.number="form.slaGio" type="number" min="1" />
          </div>
          <div>
            <label class="text-label mb-1 block text-sm font-medium">Số tệp tối đa</label>
            <GlassInput v-model.number="form.soTepToiDa" type="number" min="0" max="10" />
          </div>
          <div>
            <label class="text-label mb-1 block text-sm font-medium">Tổng dung lượng (KB)</label>
            <GlassInput v-model.number="form.tongDungLuongToiDaByte" type="number" min="1024" />
          </div>
          <div>
            <label class="text-label mb-1 block text-sm font-medium">Dung lượng/tệp (KB)</label>
            <GlassInput v-model.number="form.dungLuongTepToiDaByte" type="number" min="1024" />
          </div>
          <div class="flex flex-wrap items-end gap-5 pb-2">
            <label class="flex items-center gap-2 text-sm text-(--text-body)">
              <input v-model="form.batBuocMinhChung" type="checkbox" class="accent-(--lg-primary)" />
              Bắt buộc minh chứng
            </label>
            <label class="flex items-center gap-2 text-sm text-(--text-body)">
              <input v-model="form.dangHoatDong" type="checkbox" class="accent-(--lg-primary)" />
              Đang hoạt động (sinh viên có thể nộp)
            </label>
          </div>
        </div>
      </GlassPanel>

      <GlassPanel>
        <div class="mb-3 flex flex-wrap items-center justify-between gap-2">
          <div class="flex items-center gap-2">
            <FileText class="h-4 w-4 text-(--text-label)" />
            <p class="text-label text-sm font-semibold">Cấu hình biểu mẫu</p>
            <span class="text-placeholder text-xs">
              Nhấn + hoặc kéo trường từ cột trái, chọn trường để chỉnh thuộc tính bên phải
            </span>
          </div>
          <span class="text-placeholder text-xs">{{ previewFields.length }} trường</span>
        </div>
        <ApplicationFormBuilder ref="formBuilderRef" :key="builderKey" v-model="builderModel" />
        <p v-if="formError" class="mt-2 text-sm text-(--color-danger-text)">{{ formError }}</p>
      </GlassPanel>

      <GlassPanel v-if="previewOpen" variant="flat">
        <div class="mb-3 flex flex-wrap items-center justify-between gap-2">
          <p class="text-label text-sm font-semibold">Xem trước biểu mẫu:</p>
          <span class="text-placeholder text-xs">Thử nhập dữ liệu để kiểm tra form (chưa lưu)</span>
        </div>
        <ApplicationFormRenderer
          v-if="previewFields.length"
          v-model="previewModel"
          :schema="previewFields"
          editable
          @edit-field="handleEditFieldFromPreview"
          @delete-field="handleDeleteFieldFromPreview"
          @add-field="handleAddFieldFromPreview"
        />
        <p v-else class="text-placeholder text-sm italic">Chưa có trường nào — thêm trường ở phần Cấu hình biểu mẫu.</p>
      </GlassPanel>
    </template>
  </div>
</template>
