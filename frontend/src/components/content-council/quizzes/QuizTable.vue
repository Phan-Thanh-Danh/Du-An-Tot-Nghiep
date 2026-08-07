<script setup lang="ts">
import { computed, ref, onMounted, onUnmounted } from 'vue'
import { ContentCouncilQuiz } from '@/types/content-council/quiz'
import QuizStatusBadge from './QuizStatusBadge.vue'
import TableShell from '@/components/ui/TableShell.vue'
import { 
  MoreHorizontal, Eye, Edit3, PenTool, Copy, Send, RotateCcw, 
  PlayCircle, Lock, Trash2, AlertTriangle, ChevronLeft, ChevronRight, CheckCircle 
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
const activeQuiz = ref<ContentCouncilQuiz | null>(null)
const menuPosition = ref<{ top: string; left: string }>({ top: '0px', left: '0px' })

const toggleMenu = (quiz: ContentCouncilQuiz, event: MouseEvent) => {
  if (activeMenuId.value === quiz.id) {
    closeMenu()
    return
  }

  const btn = event.currentTarget as HTMLElement
  const rect = btn.getBoundingClientRect()

  const menuWidth = 192
  const menuHeight = 240

  let top = rect.bottom + 4
  let left = rect.right - menuWidth

  if (rect.bottom + menuHeight > window.innerHeight) {
    top = Math.max(10, rect.top - menuHeight - 4)
  }

  menuPosition.value = {
    top: `${top}px`,
    left: `${left}px`
  }

  activeQuiz.value = quiz
  activeMenuId.value = quiz.id
}

const closeMenu = () => {
  activeMenuId.value = null
  activeQuiz.value = null
}

onMounted(() => {
  document.addEventListener('click', closeMenu)
  window.addEventListener('scroll', closeMenu, true)
  window.addEventListener('resize', closeMenu)
})
onUnmounted(() => {
  document.removeEventListener('click', closeMenu)
  window.removeEventListener('scroll', closeMenu, true)
  window.removeEventListener('resize', closeMenu)
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
  <div class="bg-white rounded-xl border border-slate-200 shadow-sm flex flex-col">
    <!-- Table -->
    <TableShell>
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

            <td class="p-4 text-xs">
              <div class="flex flex-col gap-0.5">
                <span class="font-medium text-slate-700">{{ getExamTypeLabel(quiz.examType) }}</span>
                <span class="text-slate-500">{{ getFormatLabel(quiz.format) }}</span>
                <span class="text-slate-400">{{ quiz.questionCount }} câu</span>
              </div>
            </td>

            <td class="p-4 text-xs">
              <div class="flex flex-col gap-0.5">
                <span class="font-medium text-slate-700">{{ quiz.durationMinutes }} phút</span>
                <span class="text-slate-500">{{ quiz.totalScore }} điểm</span>
              </div>
            </td>

            <td class="p-4">
              <QuizStatusBadge :status="quiz.status" :trang-thai-duyet="quiz.trangThaiDuyet" />
            </td>

            <td class="p-4 text-xs text-slate-500">
              {{ formatDate(quiz.updatedAt) }}
            </td>

            <td class="p-4 text-center">
              <button 
                @click.stop="toggleMenu(quiz, $event)"
                class="p-1.5 rounded-lg text-slate-400 hover:text-slate-600 hover:bg-slate-100 transition-colors"
              >
                <MoreHorizontal class="w-5 h-5" />
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </TableShell>

    <!-- Teleport Menu -->
    <Teleport to="body">
      <div 
        v-if="activeMenuId && activeQuiz" 
        class="fixed z-[99999] w-48 rounded-lg bg-white shadow-2xl ring-1 ring-black ring-opacity-10 focus:outline-none divide-y divide-slate-100 text-sm"
        :style="{ top: menuPosition.top, left: menuPosition.left }"
        @click.stop
      >
        <div class="p-1">
            <button 
              @click="handleAction('view', activeQuiz)"
              class="flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm text-slate-700 hover:text-blue-700 hover:bg-blue-50"
            >
              <Eye class="w-4 h-4" /> Xem chi tiết
            </button>
            <button 
              @click="handleAction('edit', activeQuiz)"
              class="flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm disabled:opacity-50 disabled:cursor-not-allowed text-slate-700 hover:text-blue-700 hover:bg-blue-50"
              :disabled="activeQuiz.status === 'open'"
              :title="activeQuiz.status === 'open' ? 'Không thể sửa khi Quiz đang mở' : ''"
            >
              <Edit3 class="w-4 h-4" /> Chỉnh sửa
            </button>
            <button 
              @click="handleAction('build', activeQuiz)"
              class="flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm disabled:opacity-50 disabled:cursor-not-allowed text-slate-700 hover:text-blue-700 hover:bg-blue-50"
              :disabled="activeQuiz.status === 'open'"
              :title="activeQuiz.status === 'open' ? 'Không thể sửa khi Quiz đang mở' : ''"
            >
              <PenTool class="w-4 h-4" /> Xây dựng đề
            </button>
            <button 
              @click="handleAction('duplicate', activeQuiz)"
              class="flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm text-slate-700 hover:text-blue-700 hover:bg-blue-50"
            >
              <Copy class="w-4 h-4" /> Nhân bản
            </button>
        </div>

        <div class="p-1">
            <button v-if="activeQuiz.status === 'draft' || activeQuiz.status === 'nhap'"
              @click="handleAction('publish', activeQuiz)"
              class="flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm text-slate-700 hover:text-blue-700 hover:bg-blue-50"
            >
              <Send class="w-4 h-4 text-blue-500" /> Xuất bản
            </button>

            <button v-if="activeQuiz.status === 'published' || activeQuiz.status === 'da_xuat_ban' || activeQuiz.status === 'closed' || activeQuiz.status === 'da_dong'"
              @click="handleAction('unpublish', activeQuiz)"
              class="flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm text-slate-700 hover:text-blue-700 hover:bg-blue-50"
            >
              <RotateCcw class="w-4 h-4" /> Chuyển về nháp
            </button>

            <button v-if="activeQuiz.status === 'published' || activeQuiz.status === 'da_xuat_ban'"
              @click="handleAction('open', activeQuiz)"
              class="flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm text-green-600 hover:text-green-700 hover:bg-green-50"
            >
              <PlayCircle class="w-4 h-4" /> Mở Quiz
            </button>

            <button v-if="activeQuiz.status === 'open' || activeQuiz.status === 'dang_mo'"
              @click="handleAction('close', activeQuiz)"
              class="flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm text-amber-600 hover:text-amber-700 hover:bg-amber-50"
            >
              <Lock class="w-4 h-4" /> Đóng Quiz
            </button>
        </div>

        <div class="p-1" v-if="activeQuiz.status === 'draft' || activeQuiz.status === 'nhap'">
            <button 
              @click="handleAction('delete', activeQuiz)"
              class="flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm disabled:opacity-50 disabled:cursor-not-allowed text-red-600 hover:text-red-700 hover:bg-red-50"
            >
              <Trash2 class="w-4 h-4" /> Xóa
            </button>
        </div>
      </div>
    </Teleport>

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
