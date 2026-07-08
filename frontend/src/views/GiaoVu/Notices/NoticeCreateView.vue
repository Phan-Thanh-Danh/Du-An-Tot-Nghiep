<script setup>
import { ref } from 'vue'
import { Send, AlertCircle } from 'lucide-vue-next'
import { staffApi } from '@/services/staffApi'
import { usePopupStore } from '@/stores/popup'


const popupStore = usePopupStore()
const loading = ref(false)
const apiError = ref('')

const form = ref({
  title: '',
  content: '',
  priority: 'normal',
  recipients: 'all',
})

async function handleSubmit() {
  if (!form.value.title.trim()) {
    popupStore.warning('Thiếu thông tin', 'Vui lòng nhập tiêu đề thông báo.')
    return
  }
  if (!form.value.content.trim()) {
    popupStore.warning('Thiếu thông tin', 'Vui lòng nhập nội dung thông báo.')
    return
  }

  loading.value = true
  apiError.value = ''
  try {
    await staffApi.createNotice({ ...form.value })
    popupStore.success('Thành công', 'Thông báo đã được gửi.')
    form.value = { title: '', content: '', priority: 'normal', recipients: 'all' }
  } catch (e) {
    console.error(e)
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="notice-create max-w-4xl mx-auto space-y-6">
    <div class="lg-glass-soft p-6 rounded-2xl">
      <h1 class="text-2xl font-bold text-(--text-heading)">Tạo thông báo mới</h1>
      <p class="text-(--text-body) mt-1">Gửi thông báo đến sinh viên và giảng viên.</p>
    </div>

    <div v-if="apiError" class="surface-card border border-card rounded-2xl p-6 flex items-center gap-3">
      <AlertCircle :size="24" class="text-(--color-danger-text)" />
      <p class="text-sm text-label">{{ apiError }}</p>
    </div>

    <div class="lg-glass-soft p-6 rounded-2xl space-y-5">
      <div>
        <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">
          Tiêu đề <span class="text-(--lg-danger)">*</span>
        </label>
        <input
          v-model="form.title"
          type="text"
          class="lg-input w-full px-4 py-2.5 text-sm font-bold"
          placeholder="Nhập tiêu đề thông báo..."
        />
      </div>

      <div>
        <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">
          Nội dung <span class="text-(--lg-danger)">*</span>
        </label>
        <textarea
          v-model="form.content"
          rows="6"
          class="lg-input w-full px-4 py-2.5 text-sm font-medium resize-none"
          placeholder="Nhập nội dung thông báo..."
        ></textarea>
      </div>

      <div class="grid grid-cols-2 gap-4">
        <div>
          <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Mức độ ưu tiên</label>
          <select v-model="form.priority" class="lg-input w-full px-4 py-2.5 text-sm font-bold">
            <option value="normal">Bình thường</option>
            <option value="high">Cao</option>
            <option value="urgent">Khẩn cấp</option>
          </select>
        </div>
        <div>
          <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Đối tượng nhận</label>
          <select v-model="form.recipients" class="lg-input w-full px-4 py-2.5 text-sm font-bold">
            <option value="all">Tất cả</option>
            <option value="students">Sinh viên</option>
            <option value="teachers">Giảng viên</option>
          </select>
        </div>
      </div>

      <div class="flex items-center justify-end gap-3 pt-4 border-t border-default">
        <button
          class="lg-button-secondary px-5 py-2.5 text-sm font-bold"
          @click="form = { title: '', content: '', priority: 'normal', recipients: 'all' }"
        >Hủy</button>
        <button
          :class="[
            'lg-button-primary px-6 py-2.5 text-sm font-bold flex items-center gap-2',
            (!form.title || !form.content || loading) ? 'opacity-45 pointer-events-none' : ''
          ]"
          :disabled="!form.title || !form.content || loading"
          @click="handleSubmit"
        >
          <span v-if="loading" class="h-4 w-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
          <Send v-else :size="16" />
          {{ loading ? 'Đang gửi...' : 'Gửi thông báo' }}
        </button>
      </div>
    </div>
  </div>
</template>
