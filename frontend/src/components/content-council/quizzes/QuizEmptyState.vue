<script setup lang="ts">
import { ClipboardList, FilterX } from 'lucide-vue-next'

defineProps<{
  isFilterEmpty?: boolean
}>()

const emit = defineEmits(['action'])
</script>

<template>
  <div class="flex flex-col items-center justify-center py-16 px-4 text-center bg-white rounded-xl border border-slate-200">
    <div class="w-16 h-16 rounded-full bg-slate-50 flex items-center justify-center mb-4 text-slate-400">
      <FilterX v-if="isFilterEmpty" class="w-8 h-8" />
      <ClipboardList v-else class="w-8 h-8" />
    </div>
    <h3 class="text-lg font-bold text-slate-800 mb-2">
      {{ isFilterEmpty ? 'Không tìm thấy Quiz phù hợp' : 'Chưa có Quiz hoặc đề kiểm tra' }}
    </h3>
    <p class="text-slate-500 max-w-sm mb-6 text-sm">
      {{ isFilterEmpty 
        ? 'Thử thay đổi từ khóa hoặc bộ lọc để tìm kiếm lại.' 
        : 'Tạo Quiz đầu tiên để sử dụng trong bài học và hoạt động đánh giá.' 
      }}
    </p>
    <button 
      @click="emit('action')"
      class="px-4 py-2 text-sm font-medium rounded-lg transition-colors"
      :class="isFilterEmpty 
        ? 'bg-slate-100 text-slate-700 hover:bg-slate-200' 
        : 'bg-blue-600 text-white hover:bg-blue-700'"
    >
      {{ isFilterEmpty ? 'Xóa bộ lọc' : 'Tạo Quiz đầu tiên' }}
    </button>
  </div>
</template>
