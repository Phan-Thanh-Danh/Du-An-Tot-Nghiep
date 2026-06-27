<script setup>
import { BookX, SearchX } from 'lucide-vue-next'

defineProps({
  type: {
    type: String,
    default: 'empty', // 'empty' or 'no-results'
    validator: (value) => ['empty', 'no-results'].includes(value)
  }
})

const emit = defineEmits(['clearFilters'])
</script>

<template>
  <div class="flex flex-col items-center justify-center py-16 px-4 text-center bg-white rounded-xl border border-slate-200">
    <div class="w-16 h-16 bg-slate-50 rounded-full flex items-center justify-center mb-4">
      <BookX v-if="type === 'empty'" class="w-8 h-8 text-slate-400" />
      <SearchX v-else class="w-8 h-8 text-slate-400" />
    </div>
    
    <h3 class="text-lg font-bold text-slate-900 mb-2">
      {{ type === 'empty' ? 'Chưa có môn học' : 'Không tìm thấy môn học phù hợp' }}
    </h3>
    
    <p class="text-slate-500 max-w-sm mb-6">
      {{ type === 'empty' 
        ? 'Hiện chưa có môn học nào để biên soạn nội dung.' 
        : 'Thử thay đổi từ khóa hoặc bộ lọc để tìm môn học bạn cần.' 
      }}
    </p>

    <button
      v-if="type === 'no-results'"
      @click="emit('clearFilters')"
      class="inline-flex items-center justify-center px-4 py-2 border border-slate-300 rounded-lg text-sm font-medium text-slate-700 bg-white hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors"
    >
      Xóa bộ lọc
    </button>
  </div>
</template>
