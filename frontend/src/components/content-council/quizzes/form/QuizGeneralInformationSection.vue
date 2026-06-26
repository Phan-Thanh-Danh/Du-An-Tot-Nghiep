<script setup lang="ts">
import { ref, computed } from 'vue'
import { QuizFormData } from '@/types/content-council/quizForm'
import { mockSemesters } from '@/mocks/content-council/semesters.mock'

const props = defineProps<{
  modelValue: QuizFormData
  isReadOnly: boolean
  errors: Record<string, string>
  hasQuestions: boolean
}>()

const emit = defineEmits(['update:modelValue'])

const formData = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val)
})

const mockSubjects = [
  { id: 1, code: 'WEB201', name: 'Lập trình Web cơ bản', status: 'active' },
  { id: 2, code: 'COM101', name: 'Nhập môn CNTT', status: 'active' },
  { id: 3, code: 'PRO192', name: 'Lập trình Java', status: 'active' },
  { id: 4, code: 'DBI202', name: 'Hệ quản trị CSDL', status: 'active' }
]

const updateField = (field: keyof QuizFormData, value: any) => {
  if (props.isReadOnly) return
  const newData = { ...formData.value, [field]: value }
  emit('update:modelValue', newData)
}
</script>

<template>
  <div class="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden mb-6" :class="{'ring-1 ring-red-500': errors['subjectId'] || errors['title'] || errors['description']}">
    <div class="px-6 py-4 border-b border-slate-100 bg-slate-50 flex items-center justify-between">
      <div>
        <h2 class="text-base font-bold text-slate-800 flex items-center gap-2">
          1. Thông tin chung
        </h2>
        <p class="text-xs text-slate-500 mt-1">Cài đặt môn học và tên định danh của Quiz.</p>
      </div>
      <div v-if="errors['subjectId'] || errors['title'] || errors['description']" class="text-xs font-medium bg-red-100 text-red-700 px-2.5 py-1 rounded-full flex items-center gap-1" role="alert">
        <span class="w-1.5 h-1.5 rounded-full bg-red-600"></span>
        Có lỗi
      </div>
    </div>

    <div class="p-6 space-y-6">
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <!-- Môn học -->
        <div>
          <label class="block text-sm font-medium text-slate-700 mb-1.5">
            Môn học <span class="text-red-500">*</span>
          </label>
          <select
            :value="formData.subjectId || ''"
            @change="updateField('subjectId', Number(($event.target as HTMLSelectElement).value))"
            :disabled="isReadOnly || hasQuestions"
            class="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:outline-none transition-colors"
            :class="[
              errors['subjectId'] ? 'border-red-300 focus:ring-red-500' : 'border-slate-300 focus:border-blue-500 focus:ring-blue-500/20',
              (isReadOnly || hasQuestions) ? 'bg-slate-50 text-slate-500 cursor-not-allowed' : 'bg-white text-slate-700'
            ]"
          >
            <option value="" disabled>-- Chọn môn học --</option>
            <option v-for="sub in mockSubjects" :key="sub.id" :value="sub.id">
              {{ sub.code }} - {{ sub.name }}
            </option>
          </select>
          <p v-if="errors['subjectId']" class="mt-1.5 text-sm text-red-600">{{ errors['subjectId'] }}</p>
          <p v-else-if="hasQuestions" class="mt-1.5 text-xs text-amber-600">
            Không thể đổi môn học sau khi Quiz đã có câu hỏi.
          </p>
        </div>

        <!-- Học kỳ -->
        <div>
          <label class="block text-sm font-medium text-slate-700 mb-1.5">
            Học kỳ
          </label>
          <select
            :value="formData.semesterId || ''"
            @change="updateField('semesterId', Number(($event.target as HTMLSelectElement).value) || null)"
            :disabled="isReadOnly"
            class="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:outline-none transition-colors"
            :class="[
              isReadOnly ? 'bg-slate-50 text-slate-500 cursor-not-allowed' : 'bg-white text-slate-700 border-slate-300 focus:border-blue-500 focus:ring-blue-500/20'
            ]"
          >
            <option value="">Không gắn học kỳ</option>
            <option v-for="sem in mockSemesters" :key="sem.id" :value="sem.id">
              {{ sem.name }} ({{ sem.year }})
            </option>
          </select>
        </div>
      </div>

      <!-- Tiêu đề -->
      <div>
        <label class="block text-sm font-medium text-slate-700 mb-1.5">
          Tiêu đề Quiz / Đề kiểm tra <span class="text-red-500">*</span>
        </label>
        <input
          type="text"
          :value="formData.title"
          @input="updateField('title', ($event.target as HTMLInputElement).value)"
          :disabled="isReadOnly"
          placeholder="Ví dụ: Quiz chương 1 – Tổng quan LMS"
          class="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:outline-none transition-colors"
          :class="[
            errors['title'] ? 'border-red-300 focus:ring-red-500' : 'border-slate-300 focus:border-blue-500 focus:ring-blue-500/20',
            isReadOnly ? 'bg-slate-50 text-slate-500 cursor-not-allowed' : 'bg-white text-slate-700'
          ]"
        />
        <p v-if="errors['title']" class="mt-1.5 text-sm text-red-600">{{ errors['title'] }}</p>
      </div>

      <!-- Mô tả -->
      <div>
        <div class="flex justify-between items-end mb-1.5">
          <label class="block text-sm font-medium text-slate-700">
            Mô tả
          </label>
          <span class="text-xs" :class="(formData.description?.length || 0) > 1000 ? 'text-red-600' : 'text-slate-400'">
            {{ formData.description?.length || 0 }}/1000
          </span>
        </div>
        <textarea
          :value="formData.description"
          @input="updateField('description', ($event.target as HTMLTextAreaElement).value)"
          :disabled="isReadOnly"
          placeholder="Nhập hướng dẫn hoặc mô tả ngắn cho Quiz..."
          rows="3"
          class="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:outline-none transition-colors resize-y"
          :class="[
            errors['description'] ? 'border-red-300 focus:ring-red-500' : 'border-slate-300 focus:border-blue-500 focus:ring-blue-500/20',
            isReadOnly ? 'bg-slate-50 text-slate-500 cursor-not-allowed' : 'bg-white text-slate-700'
          ]"
        ></textarea>
        <p v-if="errors['description']" class="mt-1.5 text-sm text-red-600">{{ errors['description'] }}</p>
      </div>
    </div>
  </div>
</template>
