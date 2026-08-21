<script setup lang="ts">
import { inject } from 'vue'
import { AlertCircle, X, FileEdit } from 'lucide-vue-next'

const editor = inject<any>('curriculumEditor')
const isOpen = editor?.isDraftConfirmOpen
const pendingType = editor?.pendingContentType

const close = () => {
  if (isOpen) isOpen.value = false
  if (pendingType) pendingType.value = null
}

const confirmSwitchDraft = async () => {
  if (editor?.confirmSwitchToDraft) {
    await editor.confirmSwitchToDraft()
  }
}
</script>

<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-sm">
    <div class="bg-white rounded-2xl shadow-2xl w-full max-w-md overflow-hidden flex flex-col border border-slate-100 animate-in fade-in zoom-in duration-150" @click.stop>
      <!-- Header -->
      <div class="px-5 py-4 border-b border-slate-100 flex items-center justify-between bg-amber-50/50">
        <div class="flex items-center gap-2.5 text-amber-700">
          <div class="p-2 bg-amber-100 rounded-lg text-amber-700 shrink-0">
            <AlertCircle class="w-5 h-5" />
          </div>
          <h3 class="text-base font-bold text-slate-800">Chuyển trạng thái về Nháp</h3>
        </div>
        <button @click="close" class="text-slate-400 hover:text-slate-600 transition-colors p-1 rounded-lg hover:bg-slate-100">
          <X class="w-5 h-5" />
        </button>
      </div>

      <!-- Body -->
      <div class="p-6 space-y-4">
        <div class="p-3.5 bg-amber-50 border border-amber-200 rounded-xl text-xs text-amber-900 leading-relaxed flex items-start gap-2.5">
          <FileEdit class="w-4 h-4 text-amber-600 shrink-0 mt-0.5" />
          <div>
            Bài học <strong class="text-amber-950 font-bold">"{{ editor?.selectedLesson?.value?.title || 'Bài học' }}"</strong> đang ở trạng thái <span class="font-bold text-green-700">ĐÃ XUẤT BẢN</span>.
          </div>
        </div>
        <p class="text-slate-600 text-sm leading-relaxed">
          Để thêm nội dung mới vào bài học này, hệ thống cần chuyển trạng thái bài học về <strong>Nháp</strong>. Bạn có đồng ý chuyển ngay bây giờ không?
        </p>
      </div>

      <!-- Footer -->
      <div class="px-6 py-4 border-t border-slate-100 bg-slate-50 flex items-center justify-end gap-3">
        <button @click="close" class="px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-200 rounded-lg transition-colors">
          Hủy
        </button>
        <button @click="confirmSwitchDraft" class="px-5 py-2 text-sm font-bold text-white bg-amber-600 hover:bg-amber-700 rounded-lg transition-colors flex items-center gap-2 shadow-sm">
          <FileEdit class="w-4 h-4" />
          <span>Chuyển về Nháp & Tiếp tục</span>
        </button>
      </div>
    </div>
  </div>
</template>
