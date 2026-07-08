<script setup lang="ts">
import { inject, ref, watch } from 'vue'
import { X } from 'lucide-vue-next'
import LmsSelect from '@/components/LmsSelect.vue'

const editor = inject<any>('curriculumEditor')
const isOpen = editor.isLessonModalOpen

const formData = ref({
  title: '',
  type: 'video',
  order: null as number | null,
  status: 'draft'
})

const isEdit = ref(false)
const isSaving = ref(false)
const saveError = ref('')

const contentTypes = [
  { value: 'video', label: 'Video' },
  { value: 'slide_html', label: 'Slide HTML' },
  { value: 'pdf', label: 'Tài liệu PDF' },
  { value: 'van_ban', label: 'Văn bản' },
  { value: 'trac_nghiem', label: 'Quiz' }
]

const statusOptions = [
  { value: 'draft', label: 'Nháp' },
  { value: 'published', label: 'Xuất bản' },
  { value: 'hidden', label: 'Đang ẩn' }
]

watch(() => isOpen.value, (val) => {
  if (val) {
    saveError.value = ''
    if (editor.editingLesson.value) {
      isEdit.value = true
      formData.value = {
        title: editor.editingLesson.value.title,
        type: editor.editingLesson.value.type,
        order: editor.editingLesson.value.order,
        status: editor.editingLesson.value.status
      }
    } else {
      isEdit.value = false
      formData.value = { title: '', type: 'video', order: null, status: 'draft' }
    }
  }
})

const close = () => {
  if (isSaving.value) return
  isOpen.value = false
}

const save = async () => {
  if (!formData.value.title.trim()) {
    alert('Vui lòng nhập tên bài học')
    return
  }
  isSaving.value = true
  saveError.value = ''
  try {
    await editor.saveLesson(formData.value)
  } catch (e: any) {
    saveError.value = e?.message || 'Không thể lưu thông tin bài học'
  } finally {
    isSaving.value = false
  }
}
</script>

<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-sm">
    <div class="bg-white rounded-xl shadow-xl w-full max-w-md flex flex-col" @click.stop>
      <div class="px-5 py-4 border-b border-slate-100 flex items-center justify-between rounded-t-xl">
        <h3 class="text-lg font-bold text-slate-800">{{ isEdit ? 'Sửa thông tin bài học' : 'Thêm bài học mới' }}</h3>
        <button @click="close" class="text-slate-400 hover:text-slate-600 transition-colors">
          <X class="w-5 h-5" />
        </button>
      </div>

      <div class="p-5 space-y-4">
        <div>
          <label class="block text-sm font-medium text-slate-700 mb-1">Tên bài học <span class="text-red-500">*</span></label>
          <input 
            v-model="formData.title" 
            type="text" 
            placeholder="Ví dụ: Bài 1 – Hệ thống LMS là gì?"
            class="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            @keyup.enter="save"
            autofocus
          >
        </div>

        <div>
          <label class="block text-sm font-medium text-slate-700 mb-1">Loại nội dung chính</label>
          <LmsSelect 
            v-model="formData.type"
            :options="contentTypes"
            placeholder="Chọn loại nội dung"
          />
        </div>

        <div v-if="isEdit">
          <label class="block text-sm font-medium text-slate-700 mb-1">Trạng thái</label>
          <LmsSelect 
            v-model="formData.status"
            :options="statusOptions"
            placeholder="Chọn trạng thái"
          />
        </div>

        <div>
          <label class="block text-sm font-medium text-slate-700 mb-1">Thứ tự</label>
          <input 
            v-model.number="formData.order" 
            type="number" 
            placeholder="Để trống để thêm vào cuối"
            class="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          >
        </div>

        <p v-if="saveError" class="text-sm text-red-600">
          {{ saveError }}
        </p>
      </div>

      <div class="px-5 py-4 border-t border-slate-100 bg-slate-50 flex items-center justify-end gap-3 rounded-b-xl">
        <button @click="close" :disabled="isSaving" class="px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-200 rounded-lg transition-colors disabled:opacity-60 disabled:cursor-not-allowed">
          Hủy
        </button>
        <button @click="save" :disabled="isSaving" class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors disabled:opacity-60 disabled:cursor-not-allowed">
          {{ isSaving ? 'Đang lưu...' : (isEdit ? 'Lưu thay đổi' : 'Tạo bài học') }}
        </button>
      </div>
    </div>
  </div>
</template>
