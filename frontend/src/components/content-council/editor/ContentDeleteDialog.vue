<script setup lang="ts">
import { inject, computed } from 'vue'
import { AlertTriangle, X } from 'lucide-vue-next'

const editor = inject<any>('curriculumEditor')
const isOpen = editor.isDeleteDialogOpen

const targetType = computed(() => editor.deleteTarget.value?.type)

const getTitle = () => {
  if (targetType.value === 'chapter') return 'Xóa chương'
  if (targetType.value === 'lesson') return 'Xóa bài học'
  if (targetType.value === 'content') return 'Xóa nội dung'
  return 'Xác nhận xóa'
}

const getMessage = () => {
  if (targetType.value === 'chapter') return 'Bạn có chắc chắn muốn xóa chương này? Tất cả bài học bên trong cũng sẽ bị xóa. Hành động này không thể hoàn tác.'
  if (targetType.value === 'lesson') return 'Bạn có chắc chắn muốn xóa bài học này? Tất cả nội dung bên trong cũng sẽ bị xóa. Hành động này không thể hoàn tác.'
  if (targetType.value === 'content') return 'Bạn có chắc chắn muốn xóa khối nội dung này? Hành động này không thể hoàn tác.'
  return 'Bạn có chắc chắn muốn thực hiện hành động này?'
}

const close = () => {
  isOpen.value = false
  editor.deleteTarget.value = null
}

const confirmDelete = () => {
  editor.deleteItem()
}
</script>

<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-sm">
    <div class="bg-white rounded-xl shadow-xl w-full max-w-md overflow-hidden flex flex-col" @click.stop>
      <div class="px-5 py-4 border-b border-slate-100 flex items-center justify-between">
        <div class="flex items-center gap-2 text-red-600">
          <AlertTriangle class="w-5 h-5" />
          <h3 class="text-lg font-bold">{{ getTitle() }}</h3>
        </div>
        <button @click="close" class="text-slate-400 hover:text-slate-600 transition-colors">
          <X class="w-5 h-5" />
        </button>
      </div>

      <div class="p-5">
        <p class="text-slate-600 text-sm leading-relaxed">{{ getMessage() }}</p>
      </div>

      <div class="px-5 py-4 border-t border-slate-100 bg-slate-50 flex items-center justify-end gap-3">
        <button @click="close" class="px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-200 rounded-lg transition-colors">
          Hủy
        </button>
        <button @click="confirmDelete" class="px-4 py-2 text-sm font-medium text-white bg-red-600 hover:bg-red-700 rounded-lg transition-colors">
          Xác nhận xóa
        </button>
      </div>
    </div>
  </div>
</template>
