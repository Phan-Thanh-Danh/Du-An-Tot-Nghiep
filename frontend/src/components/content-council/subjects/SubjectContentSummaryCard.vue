<script setup>
import { computed } from 'vue'

const props = defineProps({
  chapters: {
    type: Array,
    required: true
  }
})

// Calculate summary from chapters and lessons
const summary = computed(() => {
  let totalChapters = props.chapters.length
  let totalLessons = 0
  let totalContents = 0
  
  let publishedLessons = 0
  let draftLessons = 0
  let emptyLessons = 0
  let hiddenChapters = 0

  let videoCount = 0
  let slideCount = 0
  let pdfCount = 0
  let textCount = 0
  let quizCount = 0

  props.chapters.forEach(chapter => {
    if (chapter.hidden) hiddenChapters++
    
    totalLessons += chapter.lessons.length
    
    chapter.lessons.forEach(lesson => {
      totalContents += lesson.contentCount
      
      if (lesson.status === 'published') publishedLessons++
      else if (lesson.status === 'draft') draftLessons++
      else if (lesson.status === 'empty') emptyLessons++
      
      if (lesson.type === 'video') videoCount++
      else if (lesson.type === 'slide') slideCount++
      else if (lesson.type === 'pdf') pdfCount++
      else if (lesson.type === 'text') textCount++
      else if (lesson.type === 'quiz') quizCount++
    })
  })

  return {
    totalChapters,
    totalLessons,
    totalContents,
    publishedLessons,
    draftLessons,
    emptyLessons,
    hiddenChapters,
    videoCount,
    slideCount,
    pdfCount,
    textCount,
    quizCount
  }
})
</script>

<template>
  <div class="surface-card rounded-xl border border-card overflow-hidden shadow-sm">
    <div class="p-3 border-b border-card lg-glass-soft">
      <h3 class="font-bold text-heading text-sm sm:text-base">Tổng quan tình trạng nội dung</h3>
    </div>
    
    <div class="grid grid-cols-1 lg:grid-cols-3 divide-y lg:divide-y-0 lg:divide-x divide-card">
      
      <!-- Tổng quan (Chương, Bài, Nội dung) -->
      <div class="p-3 md:p-4 flex flex-col justify-center">
        <div class="flex justify-around items-center text-center">
          <div class="flex flex-col">
            <span class="text-2xl font-bold text-heading">{{ summary.totalChapters }}</span>
            <span class="text-xs text-body font-medium mt-1">Chương</span>
          </div>
          <div class="w-px h-8 border-l border-card mx-2"></div>
          <div class="flex flex-col">
            <span class="text-2xl font-bold text-heading">{{ summary.totalLessons }}</span>
            <span class="text-xs text-body font-medium mt-1">Bài học</span>
          </div>
          <div class="w-px h-8 border-l border-card mx-2"></div>
          <div class="flex flex-col">
            <span class="text-2xl font-bold text-heading">{{ summary.totalContents }}</span>
            <span class="text-xs text-body font-medium mt-1">Nội dung</span>
          </div>
        </div>
      </div>

      <!-- Chi tiết loại nội dung -->
      <div class="p-3 md:p-4 flex flex-col justify-center">
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-x-4 gap-y-2">
          <div class="flex items-center justify-between text-sm">
            <span class="text-body flex items-center gap-2">
              <span class="w-2 h-2 rounded-full bg-red-500"></span>
              <span>Video</span>
            </span>
            <span class="font-medium text-heading">{{ summary.videoCount }}</span>
          </div>
          <div class="flex items-center justify-between text-sm">
            <span class="text-body flex items-center gap-2">
              <span class="w-2 h-2 rounded-full bg-blue-500"></span>
              <span>Slide</span>
            </span>
            <span class="font-medium text-heading">{{ summary.slideCount }}</span>
          </div>
          <div class="flex items-center justify-between text-sm">
            <span class="text-body flex items-center gap-2">
              <span class="w-2 h-2 rounded-full bg-orange-500"></span>
              <span>Tài liệu (PDF)</span>
            </span>
            <span class="font-medium text-heading">{{ summary.pdfCount }}</span>
          </div>
          <div class="flex items-center justify-between text-sm">
            <span class="text-body flex items-center gap-2">
              <span class="w-2 h-2 rounded-full" style="background-color: #64748b;"></span>
              <span>Văn bản</span>
            </span>
            <span class="font-medium text-heading">{{ summary.textCount }}</span>
          </div>
          <div class="flex items-center justify-between text-sm">
            <span class="text-body flex items-center gap-2">
              <span class="w-2 h-2 rounded-full" style="background-color: #a855f7;"></span>
              <span>Quiz</span>
            </span>
            <span class="font-medium text-heading">{{ summary.quizCount }}</span>
          </div>
        </div>
      </div>

      <!-- Tình trạng biên soạn -->
      <div class="p-3 md:p-4 flex flex-col justify-center">
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-x-4 gap-y-2">
          <div class="flex items-center justify-between text-sm">
            <span class="text-body">Đã xuất bản</span>
            <span class="font-medium px-2 py-0.5 rounded-full bg-emerald-50 text-emerald-700 border border-emerald-200">
              {{ summary.publishedLessons }}
            </span>
          </div>
          <div class="flex items-center justify-between text-sm">
            <span class="text-body">Đang nháp</span>
            <span class="font-medium px-2 py-0.5 rounded-full bg-slate-100 text-slate-700 border border-slate-200">
              {{ summary.draftLessons }}
            </span>
          </div>
          <div class="flex items-center justify-between text-sm">
            <span class="text-body">Chưa có nội dung</span>
            <span class="font-medium px-2 py-0.5 rounded-full bg-orange-50 text-orange-700 border border-orange-200">
              {{ summary.emptyLessons }}
            </span>
          </div>
          <div v-if="summary.hiddenChapters > 0" class="flex items-center justify-between text-sm">
            <span class="text-body">Chương đang ẩn</span>
            <span class="font-medium px-2 py-0.5 rounded-full bg-purple-50 text-purple-700 border border-purple-200">
              {{ summary.hiddenChapters }}
            </span>
          </div>
        </div>
      </div>

    </div>
  </div>
</template>
