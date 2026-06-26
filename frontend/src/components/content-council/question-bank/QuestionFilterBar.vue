<script setup lang="ts">
import { computed } from 'vue'
import { Search, RotateCcw } from 'lucide-vue-next'
import LmsSelect from '@/components/LmsSelect.vue'

const props = defineProps<{
  filters: {
    keyword: string
    subjectId: string
    questionType: string
    selectionType: string
    difficulty: string
    status: string
  }
}>()

const emit = defineEmits(['update:filters', 'reset'])

// Hardcoded mock subjects (in a real app, fetched from API)
const mockSubjects = [
  { id: 1, code: 'WEB201', name: 'Lập trình Web cơ bản' },
  { id: 2, code: 'COM101', name: 'Nhập môn CNTT' },
  { id: 3, code: 'PRO192', name: 'Lập trình Java' },
  { id: 4, code: 'DBI202', name: 'Hệ quản trị CSDL' }
]

const subjectOptions = [
  { value: 'all', label: 'Tất cả môn học' },
  ...mockSubjects.map(s => ({ value: s.id.toString(), label: `${s.code} - ${s.name}` }))
]

const typeOptions = [
  { value: 'all', label: 'Tất cả loại' },
  { value: 'multiple_choice', label: 'Trắc nghiệm' },
  { value: 'essay', label: 'Tự luận' }
]

const selectionOptions = [
  { value: 'all', label: 'Tất cả kiểu' },
  { value: 'single', label: 'Chọn một' },
  { value: 'multiple', label: 'Chọn nhiều' }
]

const difficultyOptions = [
  { value: 'all', label: 'Tất cả độ khó' },
  { value: 'easy', label: 'Dễ' },
  { value: 'medium', label: 'Trung bình' },
  { value: 'hard', label: 'Khó' }
]

const statusOptions = [
  { value: 'all', label: 'Tất cả trạng thái' },
  { value: 'active', label: 'Hoạt động' },
  { value: 'inactive', label: 'Đã vô hiệu hóa' }
]

const updateFilter = (key: string, value: any) => {
  const newFilters = { ...props.filters, [key]: value }
  
  // Logic: disable/reset selectionType if type is essay
  if (key === 'questionType' && value === 'essay') {
    newFilters.selectionType = 'all'
  }
  
  emit('update:filters', newFilters)
}

const isSelectionDisabled = computed(() => props.filters.questionType === 'essay')

const hasActiveFilters = computed(() => {
  return props.filters.keyword !== '' ||
         props.filters.subjectId !== 'all' ||
         props.filters.questionType !== 'all' ||
         props.filters.selectionType !== 'all' ||
         props.filters.difficulty !== 'all' ||
         props.filters.status !== 'all'
})
</script>

<template>
  <div class="bg-white p-4 rounded-xl shadow-sm border border-slate-200 mb-6 space-y-4">
    <!-- Row 1: Search and Reset -->
    <div class="flex flex-col sm:flex-row gap-4 items-start sm:items-center justify-between">
      <div class="relative w-full sm:max-w-md">
        <Search class="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-slate-400" />
        <input 
          :value="filters.keyword"
          @input="updateFilter('keyword', ($event.target as HTMLInputElement).value)"
          type="text" 
          class="w-full pl-10 pr-4 py-2.5 bg-slate-50 border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 transition-shadow text-sm"
          placeholder="Tìm theo mã hoặc nội dung câu hỏi..."
        >
      </div>

      <button 
        v-if="hasActiveFilters"
        @click="emit('reset')"
        class="flex items-center gap-2 px-3 py-2 text-sm font-medium text-slate-600 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors shrink-0"
      >
        <RotateCcw class="w-4 h-4" />
        <span>Xóa bộ lọc</span>
      </button>
    </div>

    <!-- Row 2: Dropdowns -->
    <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4">
      <LmsSelect 
        :model-value="filters.subjectId"
        @update:model-value="updateFilter('subjectId', $event)"
        :options="subjectOptions"
        label="Môn học"
      />
      
      <LmsSelect 
        :model-value="filters.questionType"
        @update:model-value="updateFilter('questionType', $event)"
        :options="typeOptions"
        label="Loại câu hỏi"
      />

      <LmsSelect 
        :model-value="filters.selectionType"
        @update:model-value="updateFilter('selectionType', $event)"
        :options="selectionOptions"
        label="Kiểu lựa chọn"
        :disabled="isSelectionDisabled"
      />

      <LmsSelect 
        :model-value="filters.difficulty"
        @update:model-value="updateFilter('difficulty', $event)"
        :options="difficultyOptions"
        label="Độ khó"
      />

      <LmsSelect 
        :model-value="filters.status"
        @update:model-value="updateFilter('status', $event)"
        :options="statusOptions"
        label="Trạng thái"
      />
    </div>
  </div>
</template>
