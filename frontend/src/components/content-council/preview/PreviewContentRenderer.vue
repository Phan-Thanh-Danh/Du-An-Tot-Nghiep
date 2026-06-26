<script setup>
import { ref, onErrorCaptured } from 'vue'
import VideoPreviewContent from './VideoPreviewContent.vue'
import SlideHtmlPreviewContent from './SlideHtmlPreviewContent.vue'
import DocumentPreviewContent from './DocumentPreviewContent.vue'
import TextPreviewContent from './TextPreviewContent.vue'
import QuizPreviewContent from './QuizPreviewContent.vue'
import { AlertCircle } from 'lucide-vue-next'

const props = defineProps({
  lesson: {
    type: Object,
    required: true
  },
  showDrafts: {
    type: Boolean,
    default: true
  }
})

const renderError = ref(null)

// Bắt lỗi render từ các component con để không làm crash toàn trang
onErrorCaptured((err, instance, info) => {
  console.error('Preview Renderer Error:', err, info)
  renderError.value = 'Đã có lỗi xảy ra khi tải nội dung bài học. Định dạng dữ liệu có thể không tương thích.'
  return false // Ngăn chặn lỗi lan rộng
})

const getComponentForType = (type) => {
  switch (type) {
    case 'video': return VideoPreviewContent
    case 'slide_html': return SlideHtmlPreviewContent
    case 'document': return DocumentPreviewContent
    case 'text': return TextPreviewContent
    case 'quiz': return QuizPreviewContent
    default: return null
  }
}

const visibleContents = computed(() => {
  if (!props.lesson || !props.lesson.contents) return []
  return props.lesson.contents.filter(c => {
    if (c.status === 'draft' && !props.showDrafts) return false
    return true
  }).sort((a, b) => a.order - b.order)
})
</script>

<script>
// Cần import computed riêng vì setup block bên trên dùng ref, onErrorCaptured
import { computed } from 'vue'
</script>

<template>
  <div class="preview-content-renderer max-w-4xl mx-auto w-full">
    <!-- Header bài học -->
    <div class="mb-8 pb-6 border-b border-slate-200">
      <div class="flex items-center gap-3 mb-2 text-sm text-slate-500 font-medium">
        <span>Bài {{ lesson.order }}</span>
        <span v-if="lesson.status === 'draft'" class="px-1.5 py-0.5 rounded bg-amber-100 text-amber-700 text-[10px] uppercase tracking-wider">
          Bài học nháp
        </span>
      </div>
      <h1 class="text-3xl font-bold text-slate-900">{{ lesson.title }}</h1>
    </div>

    <!-- Error State -->
    <div v-if="renderError" class="p-6 bg-red-50 border border-red-200 rounded-xl flex items-start gap-4">
      <AlertCircle class="w-6 h-6 text-red-500 shrink-0 mt-0.5" />
      <div>
        <h3 class="font-bold text-red-800 mb-1">Lỗi hiển thị nội dung</h3>
        <p class="text-red-600 text-sm">{{ renderError }}</p>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else-if="visibleContents.length === 0" class="py-12 text-center text-slate-500 bg-slate-50 rounded-xl border border-dashed border-slate-200">
      <div class="max-w-sm mx-auto">
        <p class="font-medium text-slate-700 mb-1">Không có nội dung</p>
        <p class="text-sm">Bài học này chưa có nội dung nào được thêm hoặc nội dung đang ở dạng nháp.</p>
      </div>
    </div>

    <!-- Content Blocks -->
    <div v-else class="space-y-12">
      <template v-for="(content, index) in visibleContents" :key="content.id">
        <div class="relative group">
          <component 
            v-if="getComponentForType(content.type)"
            :is="getComponentForType(content.type)"
            :content="content"
          />
          <div v-else class="p-4 bg-slate-50 border border-slate-200 rounded-xl text-sm text-slate-500">
            Nội dung loại <span class="font-bold text-slate-700">"{{ content.type }}"</span> hiện chưa được hỗ trợ xem trước.
          </div>
          
          <!-- Divider between blocks -->
          <div v-if="index < visibleContents.length - 1" class="absolute -bottom-6 left-1/2 -translate-x-1/2 w-8 h-px bg-slate-300"></div>
        </div>
      </template>
    </div>
  </div>
</template>
