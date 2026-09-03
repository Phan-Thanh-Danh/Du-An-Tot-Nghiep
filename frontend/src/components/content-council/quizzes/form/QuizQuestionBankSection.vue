<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { contentCouncilApi } from '@/services/contentCouncilApi'
import { mapBeToFeQuestion } from '@/stores/content-council/questionStore'
import { QuestionBankItem, QuestionType } from '@/types/content-council/questionBank'
import {
  Search, X, ChevronLeft, ChevronRight,
  CheckSquare, Square, BookOpen, FileText, Layers,
  CheckCircle, AlertCircle
} from 'lucide-vue-next'

const props = defineProps<{
  subjectId: number | null
  format: 'multiple_choice' | 'essay' | 'mixed'
  isReadOnly: boolean
  selectedQuestionIds: number[]
}>()

const emit = defineEmits<{
  (e: 'update:selectedQuestionIds', ids: number[]): void
}>()

const questions = ref<QuestionBankItem[]>([])
const isLoading = ref(false)
const errorMsg = ref('')
const keyword = ref('')
const currentPage = ref(1)
const pageSize = ref(10)
const totalItems = ref(0)

const totalPages = computed(() => Math.max(1, Math.ceil(totalItems.value / pageSize.value)))

const isAllOnPageSelected = computed(() =>
  questions.value.length > 0 &&
  questions.value.every(q => props.selectedQuestionIds.includes(q.id))
)

const isSomeOnPageSelected = computed(() =>
  questions.value.some(q => props.selectedQuestionIds.includes(q.id)) &&
  !isAllOnPageSelected.value
)

const selectedCount = computed(() => props.selectedQuestionIds.length)

const formatLabel = computed(() => {
  if (props.format === 'multiple_choice') return 'Trắc nghiệm'
  if (props.format === 'essay') return 'Tự luận'
  return 'Hỗn hợp (Trắc nghiệm + Tự luận)'
})

const formatIcon = computed(() => {
  if (props.format === 'multiple_choice') return BookOpen
  if (props.format === 'essay') return FileText
  return Layers
})

/** Map FE format value to BE loaiCauHoi value */
const formatToBeType = (_fmt: string) => {
  return 'trac_nghiem' // Quiz only supports multiple choice (single / multiple select)
}

const fetchQuestions = async () => {
  if (!props.subjectId) {
    questions.value = []
    totalItems.value = 0
    return
  }

  isLoading.value = true
  errorMsg.value = ''

  try {
    const params: Record<string, any> = {
      subjectId: props.subjectId,
      pageNumber: currentPage.value,
      pageSize: pageSize.value,
      conHoatDong: true,
    }

    if (keyword.value.trim()) params.keyword = keyword.value.trim()

    // Use BE type value (trac_nghiem / tu_luan); for mixed = no filter
    const beType = formatToBeType(props.format)
    if (beType) {
      params.loaiCauHoi = beType
    }

    const res = await contentCouncilApi.getQuestions(params)

    const items: any[] =
      res?.data?.items ??
      res?.data?.Items ??
      res?.items ??
      res?.Items ??
      (Array.isArray(res?.data) ? res.data : Array.isArray(res) ? res : [])

    const rawTotal: number =
      res?.data?.totalItems ??
      res?.data?.TotalItems ??
      res?.totalItems ??
      res?.TotalItems ??
      res?.data?.totalCount ??
      res?.data?.TotalCount ??
      res?.totalCount ??
      res?.TotalCount ??
      items.length

    // Reuse the same mapping function as questionStore
    questions.value = items.map(mapBeToFeQuestion)
    totalItems.value = rawTotal
  } catch (err) {
    console.error('Failed to fetch questions for quiz builder:', err)
    errorMsg.value = 'Không thể tải danh sách câu hỏi. Vui lòng thử lại.'
    questions.value = []
    totalItems.value = 0
  } finally {
    isLoading.value = false
  }
}

watch(() => props.subjectId, () => { currentPage.value = 1; fetchQuestions() })
watch(() => props.format, () => { currentPage.value = 1; fetchQuestions() })
watch(currentPage, fetchQuestions)
watch(pageSize, () => { currentPage.value = 1; fetchQuestions() })

let searchTimer: ReturnType<typeof setTimeout>
const onSearch = () => {
  clearTimeout(searchTimer)
  searchTimer = setTimeout(() => { currentPage.value = 1; fetchQuestions() }, 400)
}

onMounted(fetchQuestions)

const toggleSelect = (id: number) => {
  if (props.isReadOnly) return
  const newIds = [...props.selectedQuestionIds]
  const idx = newIds.indexOf(id)
  if (idx > -1) newIds.splice(idx, 1)
  else newIds.push(id)
  emit('update:selectedQuestionIds', newIds)
}

const toggleSelectAll = () => {
  if (props.isReadOnly) return
  let newIds = [...props.selectedQuestionIds]
  if (isAllOnPageSelected.value) {
    newIds = newIds.filter(id => !questions.value.some(q => q.id === id))
  } else {
    const toAdd = questions.value.filter(q => !newIds.includes(q.id)).map(q => q.id)
    newIds.push(...toAdd)
  }
  emit('update:selectedQuestionIds', newIds)
}

const removeSelected = (id: number) => {
  emit('update:selectedQuestionIds', props.selectedQuestionIds.filter(x => x !== id))
}

const typeLabel = (t: QuestionType) => t === 'multiple_choice' ? 'Trắc nghiệm' : 'Tự luận'
const typeBadgeClass = (t: QuestionType) =>
  t === 'multiple_choice' ? 'bg-blue-100 text-blue-700' : 'bg-amber-100 text-amber-700'

const diffLabel = (d: string) => {
  if (d === 'easy') return 'Dễ'
  if (d === 'hard') return 'Khó'
  return 'TB'
}
const diffClass = (d: string) => {
  if (d === 'easy') return 'bg-green-100 text-green-700'
  if (d === 'hard') return 'bg-red-100 text-red-700'
  return 'bg-orange-100 text-orange-700'
}
</script>

<template>
  <div class="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden mb-6">
    <div class="px-6 py-4 border-b border-slate-100 bg-slate-50 flex items-center justify-between">
      <div class="flex items-center gap-3">
        <component :is="formatIcon" class="w-5 h-5 text-blue-600" />
        <div>
          <h2 class="text-base font-bold text-slate-800">3. Chọn câu hỏi từ Ngân hàng câu hỏi</h2>
          <p class="text-xs text-slate-500 mt-0.5">
            Hình thức đề: <span class="font-medium text-blue-600">Trắc nghiệm</span>
            <span v-if="subjectId" class="ml-2">&middot;
              Đã chọn <span class="font-medium text-green-600">{{ selectedCount }}</span> câu hỏi
            </span>
          </p>
        </div>
      </div>
      <div
        v-if="selectedCount > 0"
        class="flex items-center gap-1.5 text-xs font-medium bg-green-100 text-green-700 px-2.5 py-1 rounded-full"
      >
        <CheckCircle class="w-3.5 h-3.5" />
        {{ selectedCount }} đã chọn
      </div>
    </div>

    <div class="p-6">
      <div
        v-if="!subjectId"
        class="flex items-center gap-3 bg-amber-50 border border-amber-200 rounded-lg px-4 py-3 text-sm text-amber-700"
      >
        <AlertCircle class="w-4 h-4 shrink-0" />
        Vui lòng chọn <strong class="mx-1">Môn học</strong> ở phần Thông tin chung trước khi chọn câu hỏi.
      </div>

      <template v-else>
        <div v-if="selectedCount > 0" class="flex flex-wrap gap-2 mb-4 p-3 bg-green-50 border border-green-100 rounded-lg">
          <span class="text-xs font-semibold text-green-700 self-center mr-1">Đã chọn:</span>
          <span
            v-for="id in selectedQuestionIds"
            :key="id"
            class="inline-flex items-center gap-1 bg-white border border-green-200 text-green-700 text-xs rounded-full px-2.5 py-0.5"
          >
            #{{ id }}
            <button
              v-if="!isReadOnly"
              @click="removeSelected(id)"
              class="ml-0.5 hover:text-red-500 transition-colors"
            >
              <X class="w-3 h-3" />
            </button>
          </span>
        </div>

        <div class="flex items-center gap-3 mb-4">
          <div class="relative flex-1">
            <Search class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
            <input
              v-model="keyword"
              @input="onSearch"
              type="text"
              placeholder="Tìm kiếm câu hỏi theo nội dung..."
              class="w-full pl-9 pr-4 py-2 border border-slate-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-500 bg-white"
            />
          </div>
          <button
            @click="fetchQuestions"
            class="px-3 py-2 bg-slate-100 hover:bg-slate-200 text-slate-600 rounded-lg text-sm transition-colors"
          >
            Làm mới
          </button>
        </div>

        <div v-if="isLoading" class="space-y-3">
          <div v-for="i in 4" :key="i" class="h-14 bg-slate-100 rounded-lg animate-pulse" />
        </div>

        <div v-else-if="errorMsg" class="flex items-center gap-2 text-sm text-red-600 bg-red-50 border border-red-100 rounded-lg px-4 py-3">
          <AlertCircle class="w-4 h-4 shrink-0" />
          {{ errorMsg }}
        </div>

        <div v-else-if="questions.length === 0" class="text-center py-10 text-slate-500">
          <BookOpen class="w-10 h-10 mx-auto mb-3 text-slate-300" />
          <p class="text-sm font-medium">Không tìm thấy câu hỏi phù hợp</p>
          <p class="text-xs text-slate-400 mt-1">Thử thay đổi từ khóa hoặc kiểm tra lại dữ liệu ngân hàng câu hỏi.</p>
        </div>

        <template v-else>
          <div class="border border-slate-200 rounded-lg overflow-hidden">
            <div class="grid grid-cols-[2.5rem_1fr_7rem_5rem_4.5rem] gap-x-4 bg-slate-50 border-b border-slate-200 px-3 py-2 text-xs font-semibold text-slate-600 uppercase tracking-wide">
              <div class="flex items-center justify-center">
                <button
                  @click="toggleSelectAll"
                  :disabled="isReadOnly"
                  class="text-slate-500 hover:text-blue-600 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                >
                  <CheckSquare v-if="isAllOnPageSelected" class="w-4 h-4 text-blue-600" />
                  <Square v-else class="w-4 h-4" />
                </button>
              </div>
              <div>Nội dung câu hỏi</div>
              <div class="text-center">Kiểu chọn</div>
              <div class="text-center">Mức độ</div>
              <div class="text-center">Dùng</div>
            </div>

            <div
              v-for="q in questions"
              :key="q.id"
              class="grid grid-cols-[2.5rem_1fr_7rem_5rem_4.5rem] gap-x-4 items-center px-3 py-3 border-b border-slate-100 last:border-b-0 hover:bg-slate-50 transition-colors cursor-pointer"
              :class="{ 'bg-blue-50/60 ring-1 ring-inset ring-blue-200': selectedQuestionIds.includes(q.id) }"
              @click="toggleSelect(q.id)"
            >
              <div class="flex items-center justify-center" @click.stop>
                <button
                  @click="toggleSelect(q.id)"
                  :disabled="isReadOnly"
                  class="text-slate-400 hover:text-blue-600 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                >
                  <CheckSquare
                    v-if="selectedQuestionIds.includes(q.id)"
                    class="w-4 h-4 text-blue-600"
                  />
                  <Square v-else class="w-4 h-4" />
                </button>
              </div>

              <div class="min-w-0">
                <p class="text-sm text-slate-700 line-clamp-2 leading-snug">{{ q.content }}</p>
                <p class="text-xs text-slate-400 mt-0.5">#{{ q.id }} &middot; {{ q.code || '–' }}</p>
              </div>

              <div class="flex justify-center">
                <span class="inline-block text-xs font-medium px-2 py-0.5 rounded-full bg-blue-50 text-blue-700 border border-blue-200">
                  {{ q.selectionType === 'multiple' ? 'Chọn nhiều' : 'Chọn 1' }}
                </span>
              </div>

              <div class="flex justify-center">
                <span class="inline-block text-xs font-medium px-2 py-0.5 rounded-full" :class="diffClass(q.difficulty)">
                  {{ diffLabel(q.difficulty) }}
                </span>
              </div>

              <div class="text-center text-xs text-slate-500 tabular-nums">{{ q.usageCount }}</div>
            </div>
          </div>

          <div class="flex flex-col sm:flex-row items-center justify-between gap-3 mt-4 pt-4 border-t border-slate-100">
            <div class="flex items-center gap-3">
              <p class="text-xs text-slate-500">
                Hiển thị {{ totalItems > 0 ? (currentPage - 1) * pageSize + 1 : 0 }}–{{ Math.min(currentPage * pageSize, totalItems) }} / {{ totalItems }} câu hỏi
              </p>
              <div class="flex items-center gap-1.5 text-xs text-slate-500">
                <span>Số câu/trang:</span>
                <select
                  v-model.number="pageSize"
                  class="border border-slate-200 rounded px-2 py-0.5 text-xs bg-white text-slate-700 focus:outline-none focus:ring-1 focus:ring-blue-500"
                >
                  <option :value="10">10</option>
                  <option :value="20">20</option>
                  <option :value="50">50</option>
                  <option :value="100">100</option>
                  <option :value="1000">Tất cả</option>
                </select>
              </div>
            </div>

            <div class="flex items-center gap-2">
              <button
                @click="currentPage--"
                :disabled="currentPage <= 1"
                class="p-1.5 rounded-md border border-slate-200 text-slate-600 hover:bg-slate-100 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
                title="Trang trước"
              >
                <ChevronLeft class="w-4 h-4" />
              </button>
              <span class="text-xs text-slate-600 tabular-nums px-1">Trang {{ currentPage }} / {{ totalPages }}</span>
              <button
                @click="currentPage++"
                :disabled="currentPage >= totalPages"
                class="p-1.5 rounded-md border border-slate-200 text-slate-600 hover:bg-slate-100 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
                title="Trang sau"
              >
                <ChevronRight class="w-4 h-4" />
              </button>
            </div>
          </div>
        </template>
      </template>
    </div>
  </div>
</template>