<script setup lang="ts">
import { computed, ref, onMounted, onUnmounted } from 'vue'
import { ContentCouncilQuiz } from '@/types/content-council/quiz'
import QuizStatusBadge from './QuizStatusBadge.vue'
import { 
  MoreHorizontal, Eye, Edit3, PenTool, Copy, Send, RotateCcw, 
  PlayCircle, Lock, Trash2, AlertTriangle, ChevronLeft, ChevronRight 
} from 'lucide-vue-next'

const props = defineProps<{
  quizzes: ContentCouncilQuiz[]
  currentPage: number
  pageSize: number
  totalItems: number
}>()

const emit = defineEmits(['action', 'update:page', 'update:pageSize'])

const totalPages = computed(() => Math.max(1, Math.ceil(props.totalItems / props.pageSize)))

const formatDate = (dateStr: string) => {
  const d = new Date(dateStr)
  return d.toLocaleDateString('vi-VN') + ' ' + d.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })
}

const getExamTypeLabel = (type: string) => {
  const map: Record<string, string> = {
    'lesson_quiz': 'Quiz bài học',
    'chapter_quiz': 'Quiz chương',
    'midterm': 'Giữa kỳ',
    'final': 'Cuối kỳ',
    'regular_test': 'Thường xuyên'
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

const activeMenuId = ref<number | null>(null)
const toggleMenu = (id: number) => {
  activeMenuId.value = activeMenuId.value === id ? null : id
}
const closeMenu = () => {
  activeMenuId.value = null
}

onMounted(() => {
  document.addEventListener('click', closeMenu)
})
onUnmounted(() => {
  document.removeEventListener('click', closeMenu)
})

const handleAction = (actionType: string, quiz: ContentCouncilQuiz) => {
  closeMenu()
  emit('action', { type: actionType, quiz })
}

const displayPages = computed(() => {
  const maxPages = 5;
  let start = Math.max(1, props.currentPage - Math.floor(maxPages / 2));
  let end = Math.min(totalPages.value, start + maxPages - 1);
  if (end - start + 1 < maxPages) {
    start = Math.max(1, end - maxPages + 1);
  }
  return Array.from({ length: end - start + 1 }, (_, i) => start + i);
})
</script>

<template>
  <div class="bg-white rounded-xl border border-slate-200 overflow-hidden shadow-sm flex flex-col">
    <!-- Table -->
    <div class="overflow-x-auto flex-1">
      <table class="w-full text-left border-collapse min-w-[1000px]">
        <thead class="bg-slate-50 border-b border-slate-200 text-xs uppercase text-slate-500 font-semibold">
          <tr>
            <th class="p-4 w-12 text-center">STT</th>
            <th class="p-4 w-32">Mã Quiz</th>
            <th class="p-4">Tiêu đề & Môn học</th>
            <th class="p-4 w-32">Cấu trúc</th>
            <th class="p-4 w-32">Thời gian & Điểm</th>
            <th class="p-4 w-36">Trạng thái</th>
            <th class="p-4 w-36">Cập nhật</th>
            <th class="p-4 w-16 text-center"></th>
          </tr>
        </thead>
        <tbody class="divide-y divide-slate-100 text-sm">
          <tr 
            v-for="(quiz, index) in quizzes" 
            :key="quiz.id"
            class="hover:bg-slate-50 transition-colors"
          >
            <td class="p-4 text-center text-slate-500">
              {{ (currentPage - 1) * pageSize + index + 1 }}
            </td>
            
            <td class="p-4">
              <span class="font-mono text-xs font-medium text-slate-700 bg-slate-100 px-2 py-1 rounded">
                {{ quiz.code }}
              </span>
            </td>

            <td class="p-4">
              <div class="flex flex-col gap-1">
                <span class="font-semibold text-slate-800 line-clamp-2" :title="quiz.title">
                  {{ quiz.title }}
                </span>
                <span class="text-xs text-slate-500 truncate" :title="quiz.subjectName">
                  {{ quiz.subjectCode }} • {{ quiz.semesterName || '---' }}
                </span>
              </div>
            </td>

            <td class="p-4">
              <div class="flex flex-col gap-1 items-start">
                <span class="text-xs font-medium text-slate-600 bg-slate-100 px-2 py-0.5 rounded">
                  {{ getExamTypeLabel(quiz.examType) }}
                </span>
                <span class="text-xs text-slate-500 mt-1">
                  {{ getFormatLabel(quiz.format) }}
                </span>
                <span v-if="quiz.questionCount === 0" class="text-[11px] text-red-600 font-medium flex items-center gap-1 mt-0.5">
                  <AlertTriangle class="w-3 h-3" />
                  Chưa có câu hỏi
                </span>
                <span v-else class="text-[11px] text-slate-500 font-medium mt-0.5">
                  {{ quiz.questionCount }} câu
                </span>
              </div>
            </td>

            <td class="p-4">
              <div class="flex flex-col gap-1 text-slate-600">
                <span class="font-medium">{{ quiz.durationMinutes > 0 ? `${quiz.durationMinutes} phút` : '---' }}</span>
                <span class="text-[11px]">{{ quiz.totalScore }} điểm</span>
              </div>
            </td>

            <td class="p-4">
              <QuizStatusBadge :status="quiz.status" />
            </td>

            <td class="p-4 text-xs text-slate-500">
              {{ formatDate(quiz.updatedAt) }}
            </td>

            <td class="p-4 text-center">
              <div class="relative inline-block text-left" @click.stop>
                <button 
                  @click="toggleMenu(quiz.id)"
                  class="p-1.5 rounded-lg text-slate-400 hover:text-slate-600 hover:bg-slate-100 transition-colors"
                >
                  <MoreHorizontal class="w-5 h-5" />
                </button>
                
                <transition
                  enter-active-class="transition ease-out duration-100"
                  enter-from-class="transform opacity-0 scale-95"
                  enter-to-class="transform opacity-100 scale-100"
                  leave-active-class="transition ease-in duration-75"
                  leave-from-class="transform opacity-100 scale-100"
                  leave-to-class="transform opacity-0 scale-95"
                >
                  <div v-if="activeMenuId === quiz.id" class="absolute right-0 z-50 mt-1 w-48 origin-top-right rounded-lg bg-white shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none divide-y divide-slate-100">
                    <div class="p-1">
                        <button 
                          @click="handleAction('view', quiz)"
                          class="flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm text-slate-700 hover:text-blue-700 hover:bg-blue-50"
                        >
                          <Eye class="w-4 h-4" /> Xem chi tiết
                        </button>
                        <button 
                          @click="handleAction('edit', quiz)"
                          class="flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm disabled:opacity-50 disabled:cursor-not-allowed text-slate-700 hover:text-blue-700 hover:bg-blue-50"
                          :disabled="quiz.status === 'open'"
                          :title="quiz.status === 'open' ? 'Không thể sửa khi Quiz đang mở' : ''"
                        >
                          <Edit3 class="w-4 h-4" /> Chỉnh sửa
                        </button>
                        <button 
                          @click="handleAction('build', quiz)"
                          class="flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm disabled:opacity-50 disabled:cursor-not-allowed text-slate-700 hover:text-blue-700 hover:bg-blue-50"
                          :disabled="quiz.status === 'open'"
                          :title="quiz.status === 'open' ? 'Không thể sửa khi Quiz đang mở' : ''"
                        >
                          <PenTool class="w-4 h-4" /> Xây dựng đề
                        </button>
                        <button 
                          @click="handleAction('duplicate', quiz)"
                          class="flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm text-slate-700 hover:text-blue-700 hover:bg-blue-50"
                        >
                          <Copy class="w-4 h-4" /> Nhân bản
                        </button>
                    </div>

                    <div class="p-1">
                        <button v-if="quiz.status === 'draft'"
                          @click="handleAction('publish', quiz)"
                          class="flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm text-slate-700 hover:text-blue-700 hover:bg-blue-50"
                        >
                          <Send class="w-4 h-4" /> Xuất bản
                        </button>

                        <button v-if="quiz.status === 'published' || quiz.status === 'closed'"
                          @click="handleAction('unpublish', quiz)"
                          class="flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm text-slate-700 hover:text-blue-700 hover:bg-blue-50"
                        >
                          <RotateCcw class="w-4 h-4" /> Chuyển về nháp
                        </button>

                        <button v-if="quiz.status === 'published'"
                          @click="handleAction('open', quiz)"
                          class="flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm text-green-600 hover:text-green-700 hover:bg-green-50"
                        >
                          <PlayCircle class="w-4 h-4" /> Mở Quiz
                        </button>

                        <button v-if="quiz.status === 'open'"
                          @click="handleAction('close', quiz)"
                          class="flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm text-amber-600 hover:text-amber-700 hover:bg-amber-50"
                        >
                          <Lock class="w-4 h-4" /> Đóng Quiz
                        </button>
                    </div>

                    <div class="p-1" v-if="quiz.status === 'draft'">
                        <button 
                          @click="handleAction('delete', quiz)"
                          class="flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm disabled:opacity-50 disabled:cursor-not-allowed text-red-600 hover:text-red-700 hover:bg-red-50"
                        >
                          <Trash2 class="w-4 h-4" /> Xóa
                        </button>
                    </div>
                  </div>
                </transition>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Pagination -->
    <div class="border-t border-slate-200 bg-slate-50 px-4 py-3 flex flex-col sm:flex-row items-center justify-between gap-4">
      <div class="text-sm text-slate-600 text-center sm:text-left">
        Hiển thị <span class="font-medium">{{ Math.min((currentPage - 1) * pageSize + 1, totalItems) }}</span> 
        đến <span class="font-medium">{{ Math.min(currentPage * pageSize, totalItems) }}</span> 
        trong tổng số <span class="font-medium">{{ totalItems }}</span> Quiz
      </div>

      <div class="flex flex-col sm:flex-row items-center gap-4">
        <div class="flex items-center gap-2 text-sm">
          <span class="text-slate-600">Hiển thị</span>
          <select 
            :value="pageSize" 
            @change="emit('update:pageSize', Number(($event.target as HTMLSelectElement).value))"
            class="bg-white border border-slate-200 rounded px-2 py-1 focus:outline-none focus:ring-2 focus:ring-blue-500 text-slate-700"
          >
            <option :value="10">10</option>
            <option :value="20">20</option>
            <option :value="50">50</option>
          </select>
          <span class="text-slate-600">/ trang</span>
        </div>

        <div class="flex items-center gap-1">
          <button 
            @click="emit('update:page', currentPage - 1)"
            :disabled="currentPage === 1"
            class="p-1.5 rounded text-slate-500 hover:bg-slate-200 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            <ChevronLeft class="w-5 h-5" />
          </button>
          
          <button 
            v-for="p in displayPages" :key="p"
            @click="emit('update:page', p)"
            class="w-8 h-8 rounded text-sm font-medium transition-colors"
            :class="p === currentPage ? 'bg-blue-600 text-white' : 'text-slate-600 hover:bg-slate-200'"
          >
            {{ p }}
          </button>

          <button 
            @click="emit('update:page', currentPage + 1)"
            :disabled="currentPage === totalPages"
            class="p-1.5 rounded text-slate-500 hover:bg-slate-200 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            <ChevronRight class="w-5 h-5" />
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
