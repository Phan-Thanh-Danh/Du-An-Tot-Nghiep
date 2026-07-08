<script setup lang="ts">
import { computed } from 'vue'
import { Search, RotateCcw } from 'lucide-vue-next'
import LmsSelect from '@/components/LmsSelect.vue'

const props = defineProps<{
  filters: {
    keyword: string
    subjectId: string
    semesterId: string
    status: string
    examType: string
    format: string
  }
  sort: string
}>()

const emit = defineEmits(['update:filters', 'update:sort', 'reset'])

const subjects = []
const semesters = []

const subjectOptions = [
  { value: 'all', label: 'Tất cả môn học' },
  ...subjects.map(s => ({ value: s.id.toString(), label: `${s.code} - ${s.name}` }))
]

const semesterOptions = [
  { value: 'all', label: 'Tất cả học kỳ' },
  ...semesters.map(s => ({ value: s.id.toString(), label: s.name }))
]

const statusOptions = [
  { value: 'all', label: 'Tất cả trạng thái' },
  { value: 'draft', label: 'Bản nháp' },
  { value: 'published', label: 'Đã xuất bản' },
  { value: 'open', label: 'Đang mở' },
  { value: 'closed', label: 'Đã đóng' }
]

const examTypeOptions = [
  { value: 'all', label: 'Tất cả loại đề' },
  { value: 'lesson_quiz', label: 'Quiz bài học' },
  { value: 'chapter_quiz', label: 'Quiz chương' },
  { value: 'midterm', label: 'Kiểm tra giữa kỳ' },
  { value: 'final', label: 'Kiểm tra cuối kỳ' },
  { value: 'regular_test', label: 'Bài kiểm tra thường xuyên' }
]

const formatOptions = [
  { value: 'all', label: 'Tất cả hình thức' },
  { value: 'multiple_choice', label: 'Trắc nghiệm' },
  { value: 'essay', label: 'Tự luận' },
  { value: 'mixed', label: 'Hỗn hợp' }
]

const sortOptions = [
  { value: 'updated_desc', label: 'Cập nhật gần nhất' },
  { value: 'created_desc', label: 'Tạo gần nhất' },
  { value: 'title_asc', label: 'Tên A-Z' },
  { value: 'title_desc', label: 'Tên Z-A' },
  { value: 'question_desc', label: 'Nhiều câu hỏi nhất' },
  { value: 'duration_desc', label: 'Thời gian dài nhất' }
]

const updateFilter = (key: keyof typeof props.filters, value: string) => {
  const newFilters = { ...props.filters, [key]: value }
  emit('update:filters', newFilters)
}

let timeoutId: any
const handleKeywordInput = (e: Event) => {
  const val = (e.target as HTMLInputElement).value
  clearTimeout(timeoutId)
  timeoutId = setTimeout(() => {
    updateFilter('keyword', val)
  }, 300)
}

const hasActiveFilters = computed(() => {
  return props.filters.keyword !== '' ||
         props.filters.subjectId !== 'all' ||
         props.filters.semesterId !== 'all' ||
         props.filters.status !== 'all' ||
         props.filters.examType !== 'all' ||
         props.filters.format !== 'all'
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
          @input="handleKeywordInput"
          type="text" 
          class="w-full pl-10 pr-4 py-2.5 bg-slate-50 border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 transition-shadow text-sm"
          placeholder="Tìm theo tên hoặc mã Quiz..."
        >
      </div>

      <div class="flex items-center gap-4 w-full sm:w-auto">
        <LmsSelect 
          :model-value="sort"
          @update:model-value="emit('update:sort', $event)"
          :options="sortOptions"
          class="w-full sm:w-48"
        />

        <button 
          v-if="hasActiveFilters"
          @click="emit('reset')"
          class="flex items-center gap-2 px-3 py-2 text-sm font-medium text-slate-600 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors shrink-0"
        >
          <RotateCcw class="w-4 h-4" />
          <span>Xóa bộ lọc</span>
        </button>
      </div>
    </div>

    <!-- Row 2: Dropdowns -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-4">
      <LmsSelect 
        :model-value="filters.subjectId"
        @update:model-value="updateFilter('subjectId', $event)"
        :options="subjectOptions"
      />
      
      <LmsSelect 
        :model-value="filters.semesterId"
        @update:model-value="updateFilter('semesterId', $event)"
        :options="semesterOptions"
      />

      <LmsSelect 
        :model-value="filters.status"
        @update:model-value="updateFilter('status', $event)"
        :options="statusOptions"
      />

      <LmsSelect 
        :model-value="filters.examType"
        @update:model-value="updateFilter('examType', $event)"
        :options="examTypeOptions"
      />

      <LmsSelect 
        :model-value="filters.format"
        @update:model-value="updateFilter('format', $event)"
        :options="formatOptions"
      />
    </div>
  </div>
</template>
