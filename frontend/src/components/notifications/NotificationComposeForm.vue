<script setup>
import { ref, computed } from 'vue'
import { Send, Eye, Users } from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassInput from '@/components/ui/GlassInput.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import { usePopupStore } from '@/stores/popup'

const props = defineProps({
  loading: { type: Boolean, default: false },
})
const emit = defineEmits(['preview', 'submit'])

const popupStore = usePopupStore()
const confirmAction = ref(null)

const form = ref({
  title: '',
  excerpt: '',
  body: '',
  category: 'hoc_vu',
  priority: 'thong_tin',
  scope: 'all'
})

const categories = [
  { value: 'hoc_vu', label: 'Học vụ' },
  { value: 'tai_chinh', label: 'Tài chính' },
  { value: 'he_thong', label: 'Hệ thống' }
]

const priorities = [
  { value: 'thong_tin', label: 'Bình thường' },
  { value: 'khan_cap', label: 'Khẩn cấp' }
]

const scopes = [
  { value: 'all', label: 'Tất cả sinh viên và giảng viên' },
  { value: 'students', label: 'Chỉ sinh viên' },
  { value: 'teachers', label: 'Chỉ giảng viên' }
]

const recipientScopeLabel = computed(() => scopes.find((item) => item.value === form.value.scope)?.label || 'Chưa chọn')

function buildPayload() {
  const scopeMap = {
    all: { phamViGui: 'toan_he_thong', roleCodes: [] },
    students: { phamViGui: 'vai_tro', roleCodes: ['hoc_sinh', 'Student'] },
    teachers: { phamViGui: 'vai_tro', roleCodes: ['giao_vien', 'Teacher'] },
  }
  const scope = scopeMap[form.value.scope] || scopeMap.all
  return {
    tieuDe: form.value.title.trim(),
    tomTat: form.value.excerpt.trim() || null,
    tomTatNoiDung: form.value.excerpt.trim() || null,
    noiDungText: form.value.body.trim(),
    noiDungJson: JSON.stringify({
      blocks: [{ type: 'paragraph', data: { text: form.value.body.trim() } }],
    }),
    mucDo: form.value.priority,
    loaiThongBao: form.value.category,
    phamViGui: scope.phamViGui,
    targetType: scope.phamViGui,
    roleCodes: scope.roleCodes,
    targetIds: [],
  }
}

function previewRecipients() {
  emit('preview', buildPayload())
}

const submitForm = () => {
  if (!form.value.title.trim()) {
    popupStore.error('Lỗi', 'Vui lòng nhập tiêu đề thông báo')
    return
  }
  if (!form.value.body.trim()) {
    popupStore.error('Lỗi', 'Vui lòng nhập nội dung thông báo')
    return
  }

  const payload = buildPayload()
  confirmAction.value = {
    title: 'Gửi thông báo?',
    message: `Thông báo "${form.value.title}" sẽ được gửi đến nhóm: ${recipientScopeLabel.value}.`,
    label: 'Gửi đi',
    variant: 'primary',
    run: () => {
      emit('submit', payload)
      confirmAction.value = null
      form.value.title = ''
      form.value.excerpt = ''
      form.value.body = ''
    }
  }
}
</script>

<template>
  <div class="notification-compose max-w-7xl mx-auto space-y-6">
    <div class="page-header mb-6">
      <h1 class="text-2xl font-bold text-(--text-heading)">Gửi thông báo</h1>
      <p class="text-(--text-body)">Soạn thảo và gửi thông báo hệ thống với chế độ xem trước.</p>
    </div>

    <div class="compose-layout">
      <div class="compose-form space-y-5">
        <GlassPanel variant="flat">
          <h2 class="text-lg font-semibold mb-4 text-(--text-heading)">Nội dung</h2>
          <div class="space-y-4">
            <GlassInput v-model="form.title" label="Tiêu đề" placeholder="Nhập tiêu đề thông báo" required />
            <GlassInput v-model="form.excerpt" label="Mô tả ngắn (Hiển thị ở inbox)" placeholder="Tóm tắt ngắn gọn 1-2 câu" />
            <label class="block">
              <span class="block text-sm font-medium text-(--text-label) mb-1">Nội dung chi tiết *</span>
              <textarea
                v-model="form.body"
                class="lg-control w-full min-h-[150px] resize-y"
                placeholder="Nhập nội dung thông báo (hỗ trợ xuống dòng, không dùng HTML)"
              ></textarea>
            </label>
            <div class="grid grid-cols-2 gap-4">
              <label class="block">
                <span class="block text-sm font-medium text-(--text-label) mb-1">Danh mục</span>
                <select v-model="form.category" class="lg-control w-full">
                  <option v-for="c in categories" :key="c.value" :value="c.value">{{ c.label }}</option>
                </select>
              </label>
              <label class="block">
                <span class="block text-sm font-medium text-(--text-label) mb-1">Độ ưu tiên</span>
                <select v-model="form.priority" class="lg-control w-full">
                  <option v-for="p in priorities" :key="p.value" :value="p.value">{{ p.label }}</option>
                </select>
              </label>
            </div>
          </div>
        </GlassPanel>

        <GlassPanel variant="flat">
          <h2 class="text-lg font-semibold mb-4 text-(--text-heading)">Phạm vi gửi</h2>
          <div class="space-y-4">
            <label class="block">
              <span class="block text-sm font-medium text-(--text-label) mb-1">Nhóm người nhận</span>
              <select v-model="form.scope" class="lg-control w-full">
                <option v-for="s in scopes" :key="s.value" :value="s.value">{{ s.label }}</option>
              </select>
            </label>
            <div class="p-4 bg-(--surface-modal) rounded-lg flex items-center justify-between">
              <span class="flex items-center gap-2 text-(--text-heading)">
                <Users :size="18" /> Phạm vi gửi
              </span>
              <strong class="text-lg">{{ recipientScopeLabel }}</strong>
            </div>
          </div>
        </GlassPanel>

        <div class="flex justify-end gap-3">
          <GlassButton variant="secondary" :disabled="props.loading" @click="previewRecipients">Xem trước người nhận</GlassButton>
          <GlassButton variant="primary" :disabled="props.loading" @click="submitForm">
            <template #leading><Send :size="16" /></template>
            {{ props.loading ? 'Đang gửi...' : 'Gửi thông báo' }}
          </GlassButton>
        </div>
      </div>

      <div class="preview-panel">
        <GlassPanel variant="readable" class="sticky top-6">
          <h2 class="text-lg font-semibold mb-4 text-(--text-heading) flex items-center gap-2">
            <Eye :size="18" /> Xem trước (Preview)
          </h2>
          <div class="preview-card border border-(--border-default) rounded-xl p-5 space-y-4 bg-(--surface-card)">
            <div class="flex gap-2">
              <GlassBadge v-if="form.priority === 'khan_cap'" variant="danger" size="sm">Khẩn cấp</GlassBadge>
              <GlassBadge variant="info" size="sm">{{ categories.find(c => c.value === form.category)?.label }}</GlassBadge>
            </div>
            <h3 class="text-xl font-bold text-(--text-heading) leading-tight">
              {{ form.title || 'Tiêu đề thông báo' }}
            </h3>
            <p class="text-(--text-muted) text-sm">Người gửi: Ban Giám Hiệu • Vừa xong</p>
            <div class="text-(--text-body) text-sm border-l-4 border-(--border-focus) pl-3 italic">
              {{ form.excerpt || 'Mô tả ngắn hiển thị ở inbox' }}
            </div>
            <div class="text-(--text-body) text-base whitespace-pre-wrap">
              {{ form.body || 'Nội dung chi tiết...' }}
            </div>
          </div>
        </GlassPanel>
      </div>
    </div>

    <ConfirmActionDialog
      v-if="confirmAction"
      :show="true"
      :title="confirmAction.title"
      :message="confirmAction.message"
      :confirmLabel="confirmAction.label"
      :variant="confirmAction.variant"
      @confirm="confirmAction.run"
      @cancel="confirmAction = null"
    />
  </div>
</template>

<style scoped>
.compose-layout {
  display: grid;
  grid-template-columns: 1fr 400px;
  gap: 1.5rem;
  align-items: start;
}
@media (max-width: 1024px) {
  .compose-layout {
    grid-template-columns: 1fr;
  }
}
</style>
