<script setup lang="ts">
import { ContentCouncilQuiz } from '@/types/content-council/quiz'
import QuizStatusBadge from './QuizStatusBadge.vue'
import { X, Edit3, PenTool, Calendar, BookOpen, Clock, ListOrdered, Settings, Activity } from 'lucide-vue-next'

const props = defineProps<{
  isOpen: boolean
  quiz: ContentCouncilQuiz | null
}>()

const emit = defineEmits(['update:isOpen', 'action'])

const getExamTypeLabel = (type: string) => {
  const map: Record<string, string> = {
    'lesson_quiz': 'Quiz bài học',
    'chapter_quiz': 'Quiz chương',
    'midterm': 'Kiểm tra giữa kỳ',
    'final': 'Kiểm tra cuối kỳ',
    'regular_test': 'Bài kiểm tra thường xuyên'
  }
  return map[type] || type
}

const getFormatLabel = (format: string) => {
  const map: Record<string, string> = {
    'multiple_choice': 'Trắc nghiệm',
    'essay': 'Tự luận',
    'mixed': 'Hỗn hợp'
  }
  return map[format] || format
}

const formatDate = (dateStr?: string) => {
  if (!dateStr) return '---'
  const d = new Date(dateStr)
  return d.toLocaleDateString('vi-VN') + ' ' + d.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })
}

const handleAction = (type: string) => {
  if (props.quiz) {
    emit('action', { type, quiz: props.quiz })
  }
}
</script>

<template>
  <div v-if="isOpen && quiz" class="fixed inset-0 z-50 flex justify-end">
    <!-- Backdrop -->
    <div 
      class="absolute inset-0 bg-slate-900/20 backdrop-blur-sm transition-opacity"
      @click="emit('update:isOpen', false)"
    ></div>

    <!-- Drawer Panel -->
    <div class="relative w-full max-w-[820px] bg-white h-full shadow-2xl flex flex-col animate-slide-in-right">
      <!-- Header -->
      <div class="flex items-start justify-between p-6 border-b border-slate-100 bg-slate-50/50 shrink-0">
        <div class="flex flex-col gap-2">
          <div class="flex items-center gap-3">
            <span class="font-mono text-sm font-medium text-slate-700 bg-slate-200/50 px-2 py-1 rounded">
              {{ quiz.code }}
            </span>
            <QuizStatusBadge :status="quiz.status" />
          </div>
          <h2 class="text-xl font-bold text-slate-800 leading-tight">
            {{ quiz.title }}
          </h2>
        </div>
        
        <button 
          @click="emit('update:isOpen', false)"
          class="p-2 text-slate-400 hover:text-slate-600 hover:bg-slate-100 rounded-full transition-colors"
        >
          <X class="w-5 h-5" />
        </button>
      </div>

      <!-- Content -->
      <div class="flex-1 overflow-y-auto p-6">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
          <!-- Column 1 -->
          <div class="space-y-8">
            <!-- Thông tin chung -->
            <section>
              <h3 class="flex items-center gap-2 text-sm font-bold text-slate-800 uppercase tracking-wider mb-4 border-b border-slate-100 pb-2">
                <BookOpen class="w-4 h-4 text-blue-600" /> Thông tin chung
              </h3>
              <div class="space-y-3">
                <div class="flex justify-between">
                  <span class="text-sm text-slate-500">Môn học</span>
                  <span class="text-sm font-medium text-slate-800">{{ quiz.subjectCode }} - {{ quiz.subjectName }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-sm text-slate-500">Học kỳ</span>
                  <span class="text-sm font-medium text-slate-800">{{ quiz.semesterName || '---' }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-sm text-slate-500">Loại đề</span>
                  <span class="text-sm font-medium text-slate-800">{{ getExamTypeLabel(quiz.examType) }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-sm text-slate-500">Hình thức</span>
                  <span class="text-sm font-medium text-slate-800">{{ getFormatLabel(quiz.format) }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-sm text-slate-500">Thời gian làm bài</span>
                  <span class="text-sm font-medium text-slate-800">{{ quiz.durationMinutes > 0 ? `${quiz.durationMinutes} phút` : 'Không giới hạn' }}</span>
                </div>
              </div>
            </section>

            <!-- Quy tắc làm bài -->
            <section>
              <h3 class="flex items-center gap-2 text-sm font-bold text-slate-800 uppercase tracking-wider mb-4 border-b border-slate-100 pb-2">
                <Settings class="w-4 h-4 text-orange-500" /> Quy tắc làm bài
              </h3>
              <div class="space-y-3">
                <div class="flex justify-between">
                  <span class="text-sm text-slate-500">Số lượt làm bài</span>
                  <span class="text-sm font-medium text-slate-800">
                    {{ quiz.unlimitedAttempts ? 'Không giới hạn' : `${quiz.maximumAttempts} lượt` }}
                  </span>
                </div>
                <div class="flex justify-between">
                  <span class="text-sm text-slate-500">Tính điểm cuối cùng</span>
                  <span class="text-sm font-medium text-slate-800">
                    {{ quiz.finalScoreMethod === 'highest' ? 'Lần cao nhất' : (quiz.finalScoreMethod === 'latest' ? 'Lần gần nhất' : 'Trung bình') }}
                  </span>
                </div>
                <div class="flex justify-between">
                  <span class="text-sm text-slate-500">Xáo trộn câu hỏi</span>
                  <span class="text-sm font-medium text-slate-800">{{ quiz.shuffleQuestions ? 'Có' : 'Không' }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-sm text-slate-500">Xáo trộn đáp án</span>
                  <span class="text-sm font-medium text-slate-800">{{ quiz.shuffleAnswers ? 'Có' : 'Không' }}</span>
                </div>
              </div>
            </section>
            
            <!-- Lịch mở đề -->
            <section>
              <h3 class="flex items-center gap-2 text-sm font-bold text-slate-800 uppercase tracking-wider mb-4 border-b border-slate-100 pb-2">
                <Calendar class="w-4 h-4 text-purple-600" /> Lịch mở đề
              </h3>
              <div class="space-y-3">
                <div class="flex justify-between">
                  <span class="text-sm text-slate-500">Mở lúc</span>
                  <span class="text-sm font-medium text-slate-800">{{ formatDate(quiz.openAt) }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-sm text-slate-500">Đóng lúc</span>
                  <span class="text-sm font-medium text-slate-800">{{ formatDate(quiz.closeAt) }}</span>
                </div>
              </div>
            </section>
          </div>

          <!-- Column 2 -->
          <div class="space-y-8">
            <!-- Cấu trúc câu hỏi -->
            <section>
              <h3 class="flex items-center gap-2 text-sm font-bold text-slate-800 uppercase tracking-wider mb-4 border-b border-slate-100 pb-2">
                <ListOrdered class="w-4 h-4 text-green-600" /> Cấu trúc câu hỏi
              </h3>
              
              <div class="bg-slate-50 rounded-xl p-4 mb-4 grid grid-cols-2 gap-4">
                <div>
                  <div class="text-xs text-slate-500 mb-1">Tổng số câu</div>
                  <div class="text-2xl font-bold text-slate-800">{{ quiz.questionCount }}</div>
                </div>
                <div>
                  <div class="text-xs text-slate-500 mb-1">Tổng điểm</div>
                  <div class="text-2xl font-bold text-blue-600">{{ quiz.totalScore }}</div>
                </div>
              </div>

              <div class="space-y-3">
                <div class="flex justify-between">
                  <span class="text-sm text-slate-500">Số câu trắc nghiệm</span>
                  <span class="text-sm font-medium text-slate-800">{{ quiz.multipleChoiceQuestionCount }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-sm text-slate-500">Số câu tự luận</span>
                  <span class="text-sm font-medium text-slate-800">{{ quiz.essayQuestionCount }}</span>
                </div>
                <div class="flex justify-between border-t border-slate-100 pt-3 mt-1">
                  <span class="text-sm text-slate-500">Điểm đạt tối thiểu</span>
                  <span class="text-sm font-bold text-green-600">{{ quiz.passingScore || 5 }} / {{ quiz.totalScore }}</span>
                </div>
              </div>
            </section>

            <!-- Hiển thị kết quả -->
            <section>
              <h3 class="flex items-center gap-2 text-sm font-bold text-slate-800 uppercase tracking-wider mb-4 border-b border-slate-100 pb-2">
                <Eye class="w-4 h-4 text-teal-600" /> Hiển thị kết quả
              </h3>
              <div class="space-y-3">
                <div class="flex justify-between">
                  <span class="text-sm text-slate-500">Hiển thị kết quả sau khi nộp</span>
                  <span class="text-sm font-medium text-slate-800">{{ quiz.showResultAfterSubmit ? 'Có' : 'Không' }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-sm text-slate-500">Hiển thị đáp án đúng</span>
                  <span class="text-sm font-medium text-slate-800">{{ quiz.showCorrectAnswerAfterSubmit ? 'Có' : 'Không' }}</span>
                </div>
              </div>
            </section>

            <!-- Mức độ sử dụng -->
            <section>
              <h3 class="flex items-center gap-2 text-sm font-bold text-slate-800 uppercase tracking-wider mb-4 border-b border-slate-100 pb-2">
                <Activity class="w-4 h-4 text-rose-500" /> Mức độ sử dụng
              </h3>
              <div class="bg-blue-50/50 rounded-xl border border-blue-100 p-4">
                <div class="flex items-center gap-3">
                  <div class="w-10 h-10 rounded-full bg-blue-100 flex items-center justify-center text-blue-600">
                    <BookOpen class="w-5 h-5" />
                  </div>
                  <div>
                    <div class="text-sm font-medium text-slate-800">
                      {{ quiz.usageCount > 0 ? `Đang được sử dụng trong ${quiz.usageCount} bài học` : 'Chưa được sử dụng' }}
                    </div>
                    <div class="text-xs text-slate-500">
                      {{ quiz.usageCount > 0 ? 'Quiz này đang hoạt động và có thể đã có dữ liệu làm bài.' : 'Có thể xóa hoặc chỉnh sửa an toàn.' }}
                    </div>
                  </div>
                </div>
              </div>
            </section>
          </div>
        </div>
      </div>

      <!-- Footer Actions -->
      <div class="p-6 border-t border-slate-100 bg-slate-50/50 flex items-center justify-end gap-3 shrink-0">
        <button 
          @click="emit('update:isOpen', false)"
          class="px-4 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50 transition-colors"
        >
          Đóng
        </button>
        <button 
          @click="handleAction('edit')"
          class="flex items-center gap-2 px-4 py-2 text-sm font-medium text-blue-700 bg-blue-50 border border-blue-200 rounded-lg hover:bg-blue-100 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          :disabled="quiz.status === 'open'"
          :title="quiz.status === 'open' ? 'Không thể chỉnh sửa khi Quiz đang mở' : ''"
        >
          <Edit3 class="w-4 h-4" />
          Chỉnh sửa
        </button>
        <button 
          @click="handleAction('build')"
          class="flex items-center gap-2 px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 transition-colors shadow-sm disabled:opacity-50 disabled:cursor-not-allowed"
          :disabled="quiz.status === 'open'"
          :title="quiz.status === 'open' ? 'Không thể chỉnh sửa khi Quiz đang mở' : ''"
        >
          <PenTool class="w-4 h-4" />
          Xây dựng đề
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.animate-slide-in-right {
  animation: slideInRight 0.3s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}

@keyframes slideInRight {
  from {
    transform: translateX(100%);
  }
  to {
    transform: translateX(0);
  }
}
</style>
