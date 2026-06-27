<script setup>
import { ref, computed } from 'vue'
import ChapterOverviewItem from './ChapterOverviewItem.vue'

const props = defineProps({
  chapters: {
    type: Array,
    required: true
  }
})

// Initialize expanded state for all chapters.
// Initially, open the first chapter, close the rest.
const expandedChapters = ref(
  props.chapters.reduce((acc, chapter, index) => {
    acc[chapter.id] = index === 0
    return acc
  }, {})
)

const isAllExpanded = computed(() => {
  return props.chapters.every(chapter => expandedChapters.value[chapter.id])
})

const toggleAll = () => {
  const targetState = !isAllExpanded.value
  props.chapters.forEach(chapter => {
    expandedChapters.value[chapter.id] = targetState
  })
}

const toggleChapter = (chapterId) => {
  expandedChapters.value[chapterId] = !expandedChapters.value[chapterId]
}
</script>

<template>
  <div class="flex flex-col h-full">
    <!-- Header -->
    <div class="p-4 border-b border-card flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-lg font-bold text-heading">Cấu trúc nội dung môn học</h2>
        <p class="text-sm text-body mt-1">Xem toàn bộ chương và bài học đã được xây dựng cho môn học này.</p>
      </div>
      <button 
        @click="toggleAll"
        class="text-sm font-medium text-blue-600 hover:text-blue-800 transition-colors whitespace-nowrap self-start sm:self-auto focus:outline-none"
      >
        {{ isAllExpanded ? 'Thu gọn tất cả' : 'Mở tất cả' }}
      </button>
    </div>

    <!-- Accordion Items -->
    <div class="flex-1 flex flex-col lg-glass-soft">
      <ChapterOverviewItem 
        v-for="chapter in chapters" 
        :key="chapter.id" 
        :chapter="chapter"
        :isExpanded="expandedChapters[chapter.id]"
        @toggle="toggleChapter"
      />
    </div>
  </div>
</template>
