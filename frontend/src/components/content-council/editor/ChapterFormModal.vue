<script setup lang="ts">
import { inject, ref, watch } from 'vue'
import { X } from 'lucide-vue-next'

const editor = inject<any>('curriculumEditor')
const isOpen = editor.isChapterModalOpen

const formData = ref({
  title: '',
  order: null as number | null,
  hidden: false
})

const isEdit = ref(false)

watch(() => isOpen.value, (val) => {
  if (val) {
    if (editor.editingChapter.value) {
      isEdit.value = true
      formData.value = {
        title: editor.editingChapter.value.title,
        order: editor.editingChapter.value.order,
        hidden: editor.editingChapter.value.hidden
      }
    } else {
      isEdit.value = false
      formData.value = { title: '', order: null, hidden: false }
    }
  }
})

const close = () => {
  isOpen.value = false
}

const save = () => {
  if (!formData.value.title.trim()) {
    alert('Vui lòng nhập tên chương')
    return
  }
  editor.saveChapter(formData.value)
}
</script>

<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-sm">
    <div class="bg-white rounded-xl shadow-xl w-full max-w-md overflow-hidden flex flex-col" @click.stop>
      <div class="px-5 py-4 border-b border-slate-100 flex items-center justify-between">
        <h3 class="text-lg font-bold text-slate-800">{{ isEdit ? 'Sửa thông tin chương' : 'Thêm chương mới' }}</h3>
        <button @click="close" class="text-slate-400 hover:text-slate-600 transition-colors">
          <X class="w-5 h-5" />
        </button>
      </div>

      <div class="p-5 space-y-4">
        <div>
          <label class="block text-sm font-medium text-slate-700 mb-1">Tên chương <span class="text-red-500">*</span></label>
          <input 
            v-model="formData.title" 
            type="text" 
            placeholder="Ví dụ: Chương 1 – Tổng quan"
            class="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            @keyup.enter="save"
            autofocus
          >
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

        <div v-if="isEdit" class="flex items-center justify-between pt-2">
          <div>
            <div class="text-sm font-medium text-slate-700">Trạng thái hiển thị</div>
            <div class="text-xs text-slate-500">Ẩn chương sẽ ẩn luôn cả các bài học bên trong.</div>
          </div>
          <label class="relative inline-flex items-center cursor-pointer">
            <input type="checkbox" v-model="formData.hidden" class="sr-only peer">
            <div class="w-11 h-6 bg-slate-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-blue-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-slate-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-blue-600"></div>
          </label>
        </div>
      </div>

      <div class="px-5 py-4 border-t border-slate-100 bg-slate-50 flex items-center justify-end gap-3">
        <button @click="close" class="px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-200 rounded-lg transition-colors">
          Hủy
        </button>
        <button @click="save" class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors">
          {{ isEdit ? 'Lưu thay đổi' : 'Tạo chương' }}
        </button>
      </div>
    </div>
  </div>
</template>
