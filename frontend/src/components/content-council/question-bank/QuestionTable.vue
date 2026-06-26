<script setup lang="ts">
import { computed } from 'vue'
import { QuestionBankItem } from '@/types/content-council/questionBank'
import { 
  MoreHorizontal, Eye, Edit3, Copy, Trash2, Power, PowerOff,
  ChevronLeft, ChevronRight, CheckSquare, Square
} from 'lucide-vue-next'

const props = defineProps<{
  questions: QuestionBankItem[]
  selectedIds: number[]
  currentPage: number
  pageSize: number
  totalItems: number
}>()

const emit = defineEmits([
  'update:selectedIds',
  'update:currentPage',
  'update:pageSize',
  'view',
  'edit',
  'duplicate',
  'delete',
  'toggleStatus',
  'bulkDelete',
  'bulkActivate',
  'bulkDeactivate'
])

const totalPages = computed(() => Math.ceil(props.totalItems / props.pageSize) || 1)

const isAllSelectedOnPage = computed(() => {
  if (props.questions.length === 0) return false
  return props.questions.every(q => props.selectedIds.includes(q.id))
})

const isSomeSelectedOnPage = computed(() => {
  return props.questions.some(q => props.selectedIds.includes(q.id)) && !isAllSelectedOnPage.value
})

const toggleSelectAll = () => {
  let newSelected = [...props.selectedIds]
  if (isAllSelectedOnPage.value) {
    newSelected = newSelected.filter(id => !props.questions.some(q => q.id === id))
  } else {
    const unselectedIds = props.questions.filter(q => !props.selectedIds.includes(q.id)).map(q => q.id)
    newSelected.push(...unselectedIds)
  }
  emit('update:selectedIds', newSelected)
}

const toggleSelect = (id: number) => {
  const newSelected = [...props.selectedIds]
  const index = newSelected.indexOf(id)
  if (index > -1) newSelected.splice(index, 1)
  else newSelected.push(id)
  emit('update:selectedIds', newSelected)
}

const getDifficultyColor = (diff: string) => {
  switch (diff) {
    case 'easy': return 'bg-green-100 text-green-700'
    case 'medium': return 'bg-amber-100 text-amber-700'
    case 'hard': return 'bg-red-100 text-red-700'
    default: return 'bg-slate-100 text-slate-700'
  }
}

const getDifficultyLabel = (diff: string) => {
  switch (diff) {
    case 'easy': return 'Dễ'
    case 'medium': return 'T.Bình'
    case 'hard': return 'Khó'
    default: return diff
  }
}

const getSelectionTypeLabel = (sel?: string) => {
  if (sel === 'single') return 'Chọn một'
  if (sel === 'multiple') return 'Chọn nhiều'
  return ''
}

const stripHtml = (html: string) => {
  const tmp = document.createElement('DIV')
  tmp.innerHTML = html
  return tmp.textContent || tmp.innerText || ''
}
</script>

<template>
  <div class="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden flex flex-col relative">
    
    <!-- Bulk Action Bar -->
    <div 
      v-if="selectedIds.length > 0"
      class="absolute top-0 inset-x-0 h-14 bg-blue-50 border-b border-blue-100 flex items-center justify-between px-4 z-20"
    >
      <div class="flex items-center gap-4">
        <span class="text-sm font-medium text-blue-700">Đã chọn {{ selectedIds.length }} câu hỏi</span>
        <button @click="emit('update:selectedIds', [])" class="text-xs text-blue-600 hover:underline">Bỏ chọn</button>
      </div>
      <div class="flex items-center gap-2">
        <button 
          @click="emit('bulkActivate')"
          class="px-3 py-1.5 text-xs font-medium bg-white text-slate-700 border border-slate-200 rounded hover:bg-slate-50 transition-colors"
        >
          Kích hoạt
        </button>
        <button 
          @click="emit('bulkDeactivate')"
          class="px-3 py-1.5 text-xs font-medium bg-white text-slate-700 border border-slate-200 rounded hover:bg-slate-50 transition-colors"
        >
          Vô hiệu hóa
        </button>
        <button 
          @click="emit('bulkDelete')"
          class="px-3 py-1.5 text-xs font-medium bg-white text-red-600 border border-slate-200 rounded hover:bg-red-50 transition-colors"
        >
          Xóa câu hỏi
        </button>
      </div>
    </div>

    <div class="overflow-x-auto min-h-[400px]">
      <table class="w-full text-left text-sm text-slate-600">
        <thead class="bg-slate-50 text-slate-500 font-medium border-b border-slate-200">
          <tr>
            <th class="p-4 w-12">
              <button @click="toggleSelectAll" class="text-slate-400 hover:text-blue-600 transition-colors">
                <CheckSquare v-if="isAllSelectedOnPage" class="w-5 h-5 text-blue-600" />
                <div v-else-if="isSomeSelectedOnPage" class="w-5 h-5 border-2 border-blue-600 bg-blue-600 rounded flex items-center justify-center">
                  <div class="w-2.5 h-0.5 bg-white"></div>
                </div>
                <Square v-else class="w-5 h-5" />
              </button>
            </th>
            <th class="p-4 w-32">Mã CH</th>
            <th class="p-4 min-w-[250px]">Nội dung</th>
            <th class="p-4">Môn học</th>
            <th class="p-4">Loại</th>
            <th class="p-4">Độ khó</th>
            <th class="p-4">Trạng thái</th>
            <th class="p-4 whitespace-nowrap">Sử dụng</th>
            <th class="p-4 w-12"></th>
          </tr>
        </thead>
        <tbody class="divide-y divide-slate-100">
          <tr 
            v-for="q in questions" 
            :key="q.id"
            class="hover:bg-slate-50 transition-colors group"
            :class="{ 'bg-blue-50/50 hover:bg-blue-50': selectedIds.includes(q.id) }"
          >
            <td class="p-4">
              <button @click="toggleSelect(q.id)" class="text-slate-400 hover:text-blue-600 transition-colors">
                <CheckSquare v-if="selectedIds.includes(q.id)" class="w-5 h-5 text-blue-600" />
                <Square v-else class="w-5 h-5" />
              </button>
            </td>
            <td class="p-4 font-medium text-slate-800">{{ q.code }}</td>
            <td class="p-4">
              <div class="line-clamp-2" :title="stripHtml(q.content)">
                {{ stripHtml(q.content) }}
              </div>
            </td>
            <td class="p-4" :title="q.subjectName">
              <span class="font-medium text-slate-700">{{ q.subjectCode }}</span>
            </td>
            <td class="p-4">
              <div class="flex flex-col gap-1 items-start">
                <span class="px-2 py-0.5 rounded text-[11px] font-medium tracking-wide bg-slate-100 text-slate-700 border border-slate-200">
                  {{ q.type === 'essay' ? 'Tự luận' : 'Trắc nghiệm' }}
                </span>
                <span v-if="q.type === 'multiple_choice'" class="text-[10px] text-slate-500 uppercase tracking-wider">
                  {{ getSelectionTypeLabel(q.selectionType) }}
                </span>
              </div>
            </td>
            <td class="p-4">
              <span class="px-2 py-0.5 rounded text-[11px] font-medium border border-transparent" :class="getDifficultyColor(q.difficulty)">
                {{ getDifficultyLabel(q.difficulty) }}
              </span>
            </td>
            <td class="p-4">
              <span 
                class="px-2 py-0.5 rounded-full text-[11px] font-medium flex items-center gap-1.5 w-max"
                :class="q.status === 'active' ? 'bg-green-50 text-green-700 border border-green-200' : 'bg-slate-100 text-slate-500 border border-slate-200'"
              >
                <span class="w-1.5 h-1.5 rounded-full" :class="q.status === 'active' ? 'bg-green-500' : 'bg-slate-400'"></span>
                {{ q.status === 'active' ? 'Hoạt động' : 'Vô hiệu hóa' }}
              </span>
            </td>
            <td class="p-4 text-xs">
              <span v-if="q.usageCount > 0" class="text-amber-600 font-medium">{{ q.usageCount }} Quiz</span>
              <span v-else class="text-slate-400">Chưa dùng</span>
            </td>
            <td class="p-4 text-right relative">
              <div class="flex items-center justify-end gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                <!-- Hover quick actions -->
                <button @click="emit('edit', q)" class="p-1.5 text-slate-400 hover:text-blue-600 hover:bg-blue-50 rounded" title="Sửa">
                  <Edit3 class="w-4 h-4" />
                </button>
                <div class="relative group/menu">
                  <button class="p-1.5 text-slate-400 hover:text-slate-600 hover:bg-slate-100 rounded">
                    <MoreHorizontal class="w-4 h-4" />
                  </button>
                  <!-- Dropdown Menu -->
                  <div class="absolute right-0 top-full mt-1 w-40 bg-white border border-slate-200 rounded-lg shadow-lg opacity-0 invisible group-hover/menu:opacity-100 group-hover/menu:visible transition-all z-10 flex flex-col py-1">
                    <button @click="emit('view', q)" class="px-3 py-2 text-sm text-left hover:bg-slate-50 flex items-center gap-2">
                      <Eye class="w-4 h-4 text-slate-400" /> Xem chi tiết
                    </button>
                    <button @click="emit('duplicate', q)" class="px-3 py-2 text-sm text-left hover:bg-slate-50 flex items-center gap-2">
                      <Copy class="w-4 h-4 text-slate-400" /> Nhân bản
                    </button>
                    <button 
                      @click="emit('toggleStatus', q)" 
                      class="px-3 py-2 text-sm text-left hover:bg-slate-50 flex items-center gap-2"
                    >
                      <Power v-if="q.status === 'inactive'" class="w-4 h-4 text-green-500" />
                      <PowerOff v-else class="w-4 h-4 text-slate-400" />
                      {{ q.status === 'active' ? 'Vô hiệu hóa' : 'Kích hoạt' }}
                    </button>
                    <div class="h-px bg-slate-100 my-1"></div>
                    <button 
                      @click="emit('delete', q)" 
                      class="px-3 py-2 text-sm text-left hover:bg-red-50 text-red-600 flex items-center gap-2"
                      :class="{'opacity-50 cursor-not-allowed hover:bg-transparent': q.usageCount > 0}"
                      :title="q.usageCount > 0 ? 'Câu hỏi đang được sử dụng, không thể xóa' : ''"
                      :disabled="q.usageCount > 0"
                    >
                      <Trash2 class="w-4 h-4" /> Xóa
                    </button>
                  </div>
                </div>
              </div>
            </td>
          </tr>
          
          <tr v-if="questions.length === 0">
            <td colspan="9" class="p-12 text-center text-slate-500">
              <div class="max-w-xs mx-auto">
                <p class="font-medium text-slate-700 mb-1">Không tìm thấy câu hỏi</p>
                <p class="text-sm">Không có dữ liệu phù hợp với bộ lọc hoặc ngân hàng câu hỏi đang trống.</p>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Pagination -->
    <div class="px-4 py-3 border-t border-slate-200 bg-slate-50 flex flex-col sm:flex-row items-center justify-between gap-4">
      <div class="text-sm text-slate-500">
        Hiển thị <span class="font-medium text-slate-800">{{ totalItems > 0 ? (currentPage - 1) * pageSize + 1 : 0 }}</span> – 
        <span class="font-medium text-slate-800">{{ Math.min(currentPage * pageSize, totalItems) }}</span> 
        trong tổng số <span class="font-medium text-slate-800">{{ totalItems }}</span> câu hỏi
      </div>
      
      <div class="flex items-center gap-4">
        <!-- Page Size -->
        <select 
          :value="pageSize" 
          @change="emit('update:pageSize', Number(($event.target as HTMLSelectElement).value))"
          class="text-sm border-slate-200 rounded-lg py-1.5 pl-3 pr-8 focus:ring-blue-500 focus:border-blue-500"
        >
          <option :value="10">10 / trang</option>
          <option :value="20">20 / trang</option>
          <option :value="50">50 / trang</option>
        </select>

        <!-- Nav -->
        <div class="flex items-center gap-1">
          <button 
            @click="emit('update:currentPage', currentPage - 1)"
            :disabled="currentPage <= 1"
            class="p-1.5 rounded-lg border border-slate-200 text-slate-500 hover:bg-white hover:text-blue-600 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            <ChevronLeft class="w-5 h-5" />
          </button>
          
          <div class="px-3 text-sm font-medium text-slate-700">
            Trang {{ currentPage }} / {{ totalPages }}
          </div>
          
          <button 
            @click="emit('update:currentPage', currentPage + 1)"
            :disabled="currentPage >= totalPages"
            class="p-1.5 rounded-lg border border-slate-200 text-slate-500 hover:bg-white hover:text-blue-600 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            <ChevronRight class="w-5 h-5" />
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
