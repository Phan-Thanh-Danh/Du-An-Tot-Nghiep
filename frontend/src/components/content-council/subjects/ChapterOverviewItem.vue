<script setup>
import { ref } from 'vue'
import { ChevronRight, ChevronDown, EyeOff } from 'lucide-vue-next'
import LessonOverviewItem from './LessonOverviewItem.vue'

const props = defineProps({
  chapter: {
    type: Object,
    required: true
  },
  isExpanded: {
    type: Boolean,
    default: true
  }
})

const emit = defineEmits(['toggle'])

const toggle = () => {
  emit('toggle', props.chapter.id)
}
</script>

<template>
  <div class="border-b border-card last:border-b-0 surface-card">
    <!-- Chapter Header -->
    <button 
      @click="toggle"
      class="w-full flex items-center justify-between py-2.5 px-4 transition-colors focus:outline-none"
      :aria-expanded="isExpanded"
    >
      <div class="flex items-center gap-3 flex-1 min-w-0">
        <div class="text-placeholder">
          <ChevronDown v-if="isExpanded" class="w-5 h-5" />
          <ChevronRight v-else class="w-5 h-5" />
        </div>
        <div class="flex items-center gap-2 flex-1 min-w-0">
          <span class="font-bold text-heading truncate text-left">
            Chương {{ chapter.order }}: {{ chapter.title }}
          </span>
          <span v-if="chapter.hidden" class="flex-shrink-0 flex items-center gap-1 text-xs font-medium text-purple-700 bg-purple-50 border border-purple-200 px-2 py-0.5 rounded-full">
            <EyeOff class="w-3 h-3" /> <span class="hidden sm:inline">Đang ẩn</span>
          </span>
        </div>
      </div>
      <div class="flex-shrink-0 text-sm font-medium text-body pl-4">
        {{ chapter.lessons?.length || 0 }} bài học
      </div>
    </button>

    <!-- Lessons List -->
    <div 
      v-show="isExpanded" 
      class="lg-glass-soft border-t border-card"
    >
      <div v-if="!chapter.lessons || chapter.lessons.length === 0" class="p-3 text-sm text-placeholder italic text-center">
        Chương này chưa có bài học nào.
      </div>
      <template v-else>
        <LessonOverviewItem 
          v-for="lesson in chapter.lessons" 
          :key="lesson.id" 
          :lesson="lesson" 
        />
      </template>
    </div>
  </div>
</template>
