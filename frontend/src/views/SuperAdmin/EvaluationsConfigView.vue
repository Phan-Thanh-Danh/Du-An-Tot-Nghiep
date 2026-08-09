<script setup>
import { computed, onMounted, ref } from 'vue'
import { Eye, FileText, Save } from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassInput from '@/components/ui/GlassInput.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'
import ApplicationFormBuilder from '@/components/applications/ApplicationFormBuilder.vue'
import ApplicationFormRenderer from '@/components/applications/ApplicationFormRenderer.vue'
import { evaluationsApi } from '@/services/evaluationsApi'
import { usePopupStore } from '@/stores/popup'

const popupStore = usePopupStore()

const EMPTY_SCHEMA = {
  fields: [
    { key: 'giao_vien', type: 'text', label: 'Tên giảng viên', required: true },
    { key: 'tieu_chi_1', type: 'select', label: 'Tiêu chí đánh giá 1', options: [
      { value: '1', label: '1 - Kém' },
      { value: '2', label: '2 - Yếu' },
      { value: '3', label: '3 - Trung bình' },
      { value: '4', label: '4 - Khá' },
      { value: '5', label: '5 - Tốt' },
    ], required: true },
    { key: 'nhan_xet', type: 'textarea', label: 'Nhận xét', required: true },
  ],
}

const loading = ref(true)
const saving = ref(false)
const previewOpen = ref(false)
const builderKey = ref(0)
const formError = ref('')

const form = ref({
  tenMau: 'Biểu mẫu đánh giá giảng viên',
  cauHinhJson: JSON.stringify(EMPTY_SCHEMA, null, 2),
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

async function loadConfig() {
  loading.value = true
  try {
    const data = await evaluationsApi.getConfig()
    if (data) {
      form.value.tenMau = data.tenMau || form.value.tenMau
      form.value.cauHinhJson = data.cauHinhJson || JSON.stringify(EMPTY_SCHEMA, null, 2)
      form.value.dangHoatDong = data.dangHoatDong !== false
    }
    builderKey.value += 1
  } catch (err) {
    popupStore.error('Lỗi', err?.message || 'Không tải được cấu hình đánh giá.')
  } finally {
    loading.value = false
  }
}

async function saveConfig() {
  formError.value = ''
  if (!form.value.tenMau.trim()) {
    formError.value = 'Tên biểu mẫu không được rỗng.'
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
    await evaluationsApi.saveConfig(form.value)
    popupStore.success('Thành công', 'Đã lưu cấu hình biểu mẫu đánh giá.')
  } catch (err) {
    popupStore.error('Lỗi', err?.message || 'Lưu cấu hình thất bại.')
  } finally {
    saving.value = false
  }
}

onMounted(loadConfig)
</script>

<template>
  <div class="space-y-4">
    <div class="flex flex-wrap items-center justify-between gap-3">
      <div>
        <div class="flex flex-wrap items-center gap-2">
          <h2 class="text-heading text-lg font-bold">Cấu hình biểu mẫu đánh giá GV</h2>
          <GlassBadge variant="secondary">đánh giá giảng viên</GlassBadge>
          <GlassBadge :variant="form.dangHoatDong ? 'success' : 'secondary'">
            {{ form.dangHoatDong ? 'Đang hoạt động' : 'Tạm ẩn' }}
          </GlassBadge>
        </div>
        <p class="text-label mt-0.5 text-sm">Kéo thả trường để thiết kế phiếu khảo sát, cấu hình được lưu dưới dạng JSON</p>
      </div>
      <div class="flex items-center gap-2">
        <GlassButton variant="secondary" @click="previewOpen = !previewOpen">
          <template #leading><Eye :size="16" /></template>
          {{ previewOpen ? 'Ẩn xem trước' : 'Xem trước biểu mẫu' }}
        </GlassButton>
        <GlassButton variant="primary" :disabled="saving" @click="saveConfig">
          <template #leading><Save :size="16" /></template>
          {{ saving ? 'Đang lưu...' : 'Lưu cấu hình' }}
        </GlassButton>
      </div>
    </div>

    <LoadingSkeleton v-if="loading" :lines="8" />

    <template v-else>
      <GlassPanel>
        <div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <label class="text-label mb-1 block text-sm font-medium">Tên biểu mẫu *</label>
            <GlassInput v-model="form.tenMau" placeholder="VD: Phiếu khảo sát đánh giá giảng viên" />
          </div>
          <div class="flex items-end pb-1">
            <label class="flex items-center gap-2 text-sm text-(--text-body)">
              <input v-model="form.dangHoatDong" type="checkbox" class="accent-(--lg-primary)" />
              Đang hoạt động (sinh viên có thể đánh giá)
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
        <ApplicationFormBuilder :key="builderKey" v-model="builderModel" />
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
        />
        <p v-else class="text-placeholder text-sm italic">Chưa có trường nào — thêm trường ở phần Cấu hình biểu mẫu.</p>
      </GlassPanel>
    </template>
  </div>
</template>
