<script setup lang="ts">
import { QuestionBankItem } from '@/types/content-council/questionBank'
import { X, Edit3, Copy, Trash2, Power, PowerOff } from 'lucide-vue-next'
import SafeHtmlRenderer from '@/components/common/SafeHtmlRenderer.vue'

const props = defineProps<{
  isOpen: boolean
  questionData: QuestionBankItem | null
}>()

const emit = defineEmits(['update:isOpen', 'edit', 'duplicate', 'delete', 'toggleStatus'])

const close = () => {
  emit('update:isOpen', false)
}

const getDifficultyLabel = (diff: string) => {
  switch (diff) {
    case 'easy': return 'Dễ'
    case 'medium': return 'Trung bình'
    case 'hard': return 'Khó'
    default: return diff
  }
}

const getSelectionTypeLabel = (sel?: string) => {
  if (sel === 'single') return 'Chọn một đáp án'
  if (sel === 'multiple') return 'Chọn nhiều đáp án'
  return ''
}

const formatDate = (dateString?: string) => {
  if (!dateString) return ''
  const d = new Date(dateString)
  return d.toLocaleString('vi-VN', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit'
  })
}
</script>

<template>
  <div>
    <!-- Backdrop -->
    <div 
      v-if="isOpen" 
      class="fixed inset-0 bg-slate-900/50 backdrop-blur-sm z-40 transition-opacity"
      @click="close"
    ></div>

    <!-- Drawer -->
    <div 
      class="fixed inset-y-0 right-0 z-50 w-full sm:w-[500px] md:w-[680px] bg-slate-50 shadow-2xl flex flex-col transform transition-transform duration-300"
      :class="isOpen ? 'translate-x-0' : 'translate-x-full'"
    >
      <template v-if="questionData">
        <!-- Header -->
        <div class="px-6 py-4 border-b border-slate-200 bg-white flex flex-col gap-2 shrink-0">
          <div class="flex items-start justify-between">
            <div class="flex items-center gap-3">
              <h3 class="text-xl font-bold text-slate-800">{{ questionData.code }}</h3>
              <span 
                class="px-2 py-0.5 rounded-full text-xs font-medium"
                :class="questionData.status === 'active' ? 'bg-green-100 text-green-700' : 'bg-slate-200 text-slate-600'"
              >
                {{ questionData.status === 'active' ? 'Hoạt động' : 'Vô hiệu hóa' }}
              </span>
            </div>
            <button @click="close" class="text-slate-400 hover:text-slate-600 transition-colors p-1.5 hover:bg-slate-100 rounded-full">
              <X class="w-5 h-5" />
            </button>
          </div>
          
          <div class="flex flex-wrap items-center gap-4 text-sm text-slate-600">
            <div class="flex items-center gap-1.5 font-medium">
              <span class="text-slate-400">Môn học:</span>
              <span class="text-slate-800">{{ questionData.subjectCode }}</span>
            </div>
            <div class="w-1 h-1 rounded-full bg-slate-300"></div>
            <div class="flex items-center gap-1.5 font-medium">
              <span class="text-slate-400">Độ khó:</span>
              <span class="text-slate-800">{{ getDifficultyLabel(questionData.difficulty) }}</span>
            </div>
            <div class="w-1 h-1 rounded-full bg-slate-300"></div>
            <div class="flex items-center gap-1.5 font-medium">
              <span class="text-slate-400">Sử dụng:</span>
              <span :class="questionData.usageCount > 0 ? 'text-amber-600' : 'text-slate-800'">
                {{ questionData.usageCount > 0 ? `${questionData.usageCount} lần` : 'Chưa sử dụng' }}
              </span>
            </div>
          </div>
        </div>

        <!-- Body -->
        <div class="flex-1 overflow-y-auto p-6 space-y-6">
          <div class="bg-white rounded-xl shadow-sm border border-slate-200 p-6 space-y-6">
            <!-- Đề bài -->
            <div>
              <div class="flex items-center gap-2 mb-3">
                <span class="px-2 py-0.5 rounded text-[11px] font-bold uppercase tracking-wider bg-slate-100 text-slate-600 border border-slate-200">
                  {{ questionData.type === 'essay' ? 'Tự luận' : 'Trắc nghiệm' }}
                </span>
                <span v-if="questionData.type === 'multiple_choice'" class="text-[11px] text-slate-500 font-medium">
                  • {{ getSelectionTypeLabel(questionData.selectionType) }}
                </span>
              </div>
              <SafeHtmlRenderer class="text-base text-slate-800 leading-relaxed whitespace-pre-wrap" :html="questionData.content" />
            </div>

            <!-- Các lựa chọn cho trắc nghiệm -->
            <div v-if="questionData.type === 'multiple_choice'" class="space-y-3 pt-4 border-t border-slate-100">
              <div class="text-sm font-bold text-slate-800 mb-2">Các lựa chọn:</div>
              <div 
                v-for="(choice, index) in questionData.choices" 
                :key="choice.id"
                class="flex items-start gap-3 p-3 rounded-lg border"
                :class="{
                  'bg-green-50 border-green-200': (questionData.correctAnswerIds || []).includes(choice.id),
                  'bg-slate-50 border-slate-200': !(questionData.correctAnswerIds || []).includes(choice.id)
                }"
              >
                <div class="w-6 h-6 shrink-0 rounded-full bg-white border flex items-center justify-center text-xs font-bold"
                  :class="(questionData.correctAnswerIds || []).includes(choice.id) ? 'border-green-500 text-green-600' : 'border-slate-300 text-slate-500'"
                >
                  {{ String.fromCharCode(65 + index) }}
                </div>
                <div class="pt-0.5" :class="{'font-medium text-green-800': (questionData.correctAnswerIds || []).includes(choice.id)}">
                  {{ choice.content }}
                </div>
              </div>
            </div>

            <!-- Đáp án mẫu cho tự luận -->
            <div v-if="questionData.type === 'essay' && questionData.sampleAnswer" class="pt-4 border-t border-slate-100">
              <div class="text-sm font-bold text-amber-800 mb-2">Đáp án mẫu:</div>
              <div class="p-4 bg-amber-50 rounded-lg text-amber-900 text-sm whitespace-pre-wrap border border-amber-100">
                {{ questionData.sampleAnswer }}
              </div>
            </div>

            <!-- Giải thích đáp án -->
            <div v-if="questionData.answerExplanation" class="pt-4 border-t border-slate-100">
              <div class="text-sm font-bold text-blue-800 mb-2">{{ questionData.type === 'multiple_choice' ? 'Giải thích đáp án:' : 'Hướng dẫn chấm:' }}</div>
              <div class="p-4 bg-blue-50 rounded-lg text-blue-900 text-sm whitespace-pre-wrap border border-blue-100">
                {{ questionData.answerExplanation }}
              </div>
            </div>
          </div>

          <!-- Meta Info -->
          <div class="flex items-center gap-6 text-xs text-slate-500 px-2">
            <div>
              <span class="font-medium text-slate-700">Ngày tạo:</span> {{ formatDate(questionData.createdAt) }}
            </div>
            <div>
              <span class="font-medium text-slate-700">Cập nhật lần cuối:</span> {{ formatDate(questionData.updatedAt) }}
            </div>
          </div>
        </div>

        <!-- Footer Actions -->
        <div class="px-6 py-4 border-t border-slate-200 bg-slate-50 flex items-center justify-between shrink-0">
          <button 
            @click="emit('delete', questionData)"
            class="p-2 text-red-500 hover:bg-red-50 rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            title="Xóa câu hỏi"
            :disabled="questionData.usageCount > 0"
          >
            <Trash2 class="w-5 h-5" />
          </button>
          
          <div class="flex items-center gap-3">
            <button 
              @click="emit('toggleStatus', questionData)"
              class="px-4 py-2 text-sm font-medium border border-slate-200 rounded-lg hover:bg-white transition-colors flex items-center gap-2"
              :class="questionData.status === 'active' ? 'text-amber-600' : 'text-green-600'"
            >
              <PowerOff v-if="questionData.status === 'active'" class="w-4 h-4" />
              <Power v-else class="w-4 h-4" />
              <span>{{ questionData.status === 'active' ? 'Vô hiệu hóa' : 'Kích hoạt' }}</span>
            </button>
            <button 
              @click="emit('duplicate', questionData)"
              class="px-4 py-2 text-sm font-medium border border-slate-200 rounded-lg hover:bg-white text-slate-700 transition-colors flex items-center gap-2"
            >
              <Copy class="w-4 h-4" /> Nhân bản
            </button>
            <button 
              @click="emit('edit', questionData)"
              class="px-4 py-2 text-sm font-medium bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition-colors flex items-center gap-2"
            >
              <Edit3 class="w-4 h-4" /> Chỉnh sửa
            </button>
          </div>
        </div>
      </template>
    </div>
  </div>
</template>
