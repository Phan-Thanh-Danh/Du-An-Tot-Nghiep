<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { Filter, X, Search } from 'lucide-vue-next'

const router = useRouter()
const route = useRoute()

const emit = defineEmits(['filter-change'])

const keyword = ref((route.query.keyword as string) || '')
const questionType = ref((route.query.questionType as string) || '')
const selectionType = ref((route.query.selectionType as string) || '')
const difficulty = ref((route.query.difficulty as string) || '')

const hasFilters = computed(() => !!keyword.value || !!questionType.value || !!selectionType.value || !!difficulty.value)

const applyFilters = () => {
  const query = { ...route.query }
  
  if (keyword.value) query.keyword = keyword.value
  else delete query.keyword
  
  if (questionType.value) query.questionType = questionType.value
  else delete query.questionType
  
  if (selectionType.value) query.selectionType = selectionType.value
  else delete query.selectionType
  
  if (difficulty.value) query.difficulty = difficulty.value
  else delete query.difficulty

  // reset page
  delete query.page
  
  router.replace({ query })
  emit('filter-change', {
    keyword: keyword.value,
    questionType: questionType.value,
    selectionType: selectionType.value,
    difficulty: difficulty.value
  })
}

const clearFilters = () => {
  keyword.value = ''
  questionType.value = ''
  selectionType.value = ''
  difficulty.value = ''
  applyFilters()
}

// Watch questionType to reset selectionType if essay is selected
watch(questionType, (newVal) => {
  if (newVal === 'essay') {
    selectionType.value = ''
  }
})
</script>

<template>
  <div class="bg-white border-b border-slate-200 p-4 space-y-3 shrink-0">
    <!-- Search Bar -->
    <div class="relative">
      <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
        <Search class="h-4 w-4 text-slate-400" />
      </div>
      <input 
        v-model="keyword"
        @keyup.enter="applyFilters"
        type="text" 
        class="block w-full pl-10 pr-3 py-2 border border-slate-300 rounded-lg text-sm placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500" 
        placeholder="Tìm câu hỏi theo nội dung hoặc mã..."
      >
    </div>

    <!-- Filters -->
    <div class="flex flex-wrap items-center gap-2">
      <select 
        v-model="questionType" 
        @change="applyFilters"
        class="text-sm border border-slate-300 rounded-md px-2.5 py-1.5 focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
      >
        <option value="">Tất cả loại</option>
        <option value="multiple_choice">Trắc nghiệm</option>
        <option value="essay">Tự luận</option>
      </select>

      <select 
        v-model="selectionType" 
        @change="applyFilters"
        :disabled="questionType === 'essay'"
        class="text-sm border border-slate-300 rounded-md px-2.5 py-1.5 focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
        :class="{'opacity-50 cursor-not-allowed': questionType === 'essay'}"
      >
        <option value="">Kiểu lựa chọn</option>
        <option value="single">Chọn một</option>
        <option value="multiple">Chọn nhiều</option>
      </select>

      <select 
        v-model="difficulty" 
        @change="applyFilters"
        class="text-sm border border-slate-300 rounded-md px-2.5 py-1.5 focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
      >
        <option value="">Tất cả độ khó</option>
        <option value="easy">Dễ</option>
        <option value="medium">Trung bình</option>
        <option value="hard">Khó</option>
      </select>

      <button 
        v-if="hasFilters" 
        @click="clearFilters"
        class="text-xs text-slate-500 hover:text-red-600 flex items-center gap-1 px-2"
      >
        <X class="w-3 h-3" />
        Xóa lọc
      </button>
    </div>
  </div>
</template>
