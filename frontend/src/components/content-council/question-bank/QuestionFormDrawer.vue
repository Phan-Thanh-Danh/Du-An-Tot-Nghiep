<script setup lang="ts">
import { ref, watch, computed, onMounted } from 'vue'
import { QuestionBankItem, QuestionType, SelectionType, QuestionDifficulty, QuestionStatus } from '@/types/content-council/questionBank'
import { X, Save, Copy } from 'lucide-vue-next'
import SafeHtmlRenderer from '@/components/common/SafeHtmlRenderer.vue'
import LmsSelect from '@/components/LmsSelect.vue'
import QuestionChoiceEditor from './QuestionChoiceEditor.vue'
import { useSubjectStore } from '@/stores/content-council/subjectStore'

const props = defineProps<{
  isOpen: boolean
  mode: 'create' | 'edit' | 'duplicate'
  questionData?: QuestionBankItem | null
}>()

const emit = defineEmits(['update:isOpen', 'save'])

const generateId = () => Math.random().toString(36).substr(2, 9)
const subjectStore = useSubjectStore()

const subjectOptions = computed(() =>
  subjectStore.subjects.map(s => ({
    value: s.id,
    label: `${s.code} - ${s.name}`,
    code: s.code,
    name: s.name
  }))
)

const typeOptions = [
  { value: 'multiple_choice', label: 'Trắc nghiệm' },
  { value: 'essay', label: 'Tự luận' }
]

const selectionOptions = [
  { value: 'single', label: 'Chọn một đáp án' },
  { value: 'multiple', label: 'Chọn nhiều đáp án' }
]

const difficultyOptions = [
  { value: 'easy', label: 'Dễ' },
  { value: 'medium', label: 'Trung bình' },
  { value: 'hard', label: 'Khó' }
]

const statusOptions = [
  { value: 'active', label: 'Hoạt động' },
  { value: 'inactive', label: 'Vô hiệu hóa' }
]

// Form State
const formData = ref<Partial<QuestionBankItem>>({})
const isPreviewAnswerVisible = ref(false)

const getDrawerTitle = () => {
  if (props.mode === 'create') return 'Tạo câu hỏi mới'
  if (props.mode === 'edit') return 'Chỉnh sửa câu hỏi'
  return 'Nhân bản câu hỏi'
}

const getSaveButtonText = () => {
  if (props.mode === 'create') return 'Tạo câu hỏi'
  if (props.mode === 'edit') return 'Lưu thay đổi'
  return 'Tạo bản sao'
}

const getSaveButtonIcon = () => {
  if (props.mode === 'duplicate') return Copy
  return Save
}

watch(() => props.isOpen, (val) => {
  if (val) {
    isPreviewAnswerVisible.value = false
    if (props.mode === 'create' || !props.questionData) {
      formData.value = {
        subjectId: undefined,
        type: 'multiple_choice',
        selectionType: 'single',
        content: '',
        choices: [
          { id: generateId(), content: '' },
          { id: generateId(), content: '' },
          { id: generateId(), content: '' },
          { id: generateId(), content: '' }
        ],
        correctAnswerIds: [],
        difficulty: 'medium',
        status: 'active',
        answerExplanation: '',
        sampleAnswer: ''
      }
    } else {
      // Clone data
      const cloned = JSON.parse(JSON.stringify(props.questionData))
      if (props.mode === 'duplicate') {
        cloned.id = undefined // Will be assigned by parent
        cloned.code = '' // Will be generated
        cloned.status = 'active'
        cloned.usageCount = 0
        // cloned.content = cloned.content // maybe add "(Bản sao)" later
      }
      formData.value = cloned
    }
  }
})

const handleTypeChange = (newType: QuestionType) => {
  if (formData.value.type === newType) return
  
  if (formData.value.content || (formData.value.choices && formData.value.choices[0].content)) {
    if (!confirm('Chuyển đổi loại câu hỏi sẽ làm mất một số dữ liệu định dạng riêng. Bạn có chắc muốn tiếp tục?')) {
      // Revert select visually by not updating formData.type, but since v-model updates immediately, it's tricky.
      // We will just let it change and reset data.
    }
  }

  formData.value.type = newType
  if (newType === 'essay') {
    formData.value.selectionType = undefined
    formData.value.choices = []
    formData.value.correctAnswerIds = []
  } else {
    formData.value.selectionType = 'single'
    formData.value.choices = [
      { id: generateId(), content: '' },
      { id: generateId(), content: '' },
      { id: generateId(), content: '' },
      { id: generateId(), content: '' }
    ]
    formData.value.correctAnswerIds = []
  }
}

const handleSelectionTypeChange = (newSelection: SelectionType) => {
  if (formData.value.selectionType === newSelection) return
  
  if (newSelection === 'single' && formData.value.correctAnswerIds && formData.value.correctAnswerIds.length > 1) {
    alert('Đã giữ lại đáp án đúng đầu tiên để phù hợp với kiểu chọn một.')
    formData.value.correctAnswerIds = [formData.value.correctAnswerIds[0]]
  }
  formData.value.selectionType = newSelection
}

const close = () => {
  if (formData.value.content) {
    if (!confirm('Bỏ các thay đổi? Các nội dung chưa lưu sẽ bị mất.')) return
  }
  emit('update:isOpen', false)
}

const validateAndSave = () => {
  const d = formData.value
  if (!d.subjectId) return alert('Vui lòng chọn môn học.')
  if (!d.content || !d.content.trim()) return alert('Nội dung câu hỏi không được để trống.')
  
  if (d.type === 'multiple_choice') {
    if (!d.choices || d.choices.length < 2) return alert('Câu hỏi trắc nghiệm phải có ít nhất 2 lựa chọn.')
    const emptyChoice = d.choices.find(c => !c.content.trim())
    if (emptyChoice) return alert('Nội dung các lựa chọn không được để trống.')
    
    // Check duplicates
    const contents = d.choices.map(c => c.content.trim().toLowerCase())
    if (new Set(contents).size !== contents.length) {
      return alert('Các lựa chọn không được trùng lặp nội dung.')
    }

    if (!d.correctAnswerIds || d.correctAnswerIds.length === 0) {
      return alert('Vui lòng chọn ít nhất 1 đáp án đúng.')
    }

    if (d.selectionType === 'multiple' && d.correctAnswerIds.length < 2) {
      return alert('Câu chọn nhiều phải có ít nhất 2 đáp án đúng.')
    }
  }

  const subj = subjectOptions.value.find(s => s.value === d.subjectId)
  if (subj) {
    d.subjectName = subj.name
    d.subjectCode = subj.code
  }

  emit('save', { ...d })
}

onMounted(() => {
  subjectStore.init()
})
</script>

<template>
  <div>
    <!-- Backdrop -->
    <div 
      v-if="isOpen" 
      class="fixed inset-0 bg-slate-900/50 backdrop-blur-sm z-40 transition-opacity"
      @click="close"
    ></div>

    <div 
      class="fixed inset-y-0 right-0 z-50 w-full sm:w-[640px] md:w-[800px] lg:w-[1000px] xl:w-[1200px] 2xl:w-[1400px] bg-slate-50 shadow-2xl flex flex-col transform transition-transform duration-300"
      :class="isOpen ? 'translate-x-0' : 'translate-x-full'"
    >
      <!-- Header -->
      <div class="px-6 py-4 border-b border-slate-200 bg-white flex items-center justify-between shrink-0">
        <h3 class="text-xl font-bold text-slate-800">{{ getDrawerTitle() }}</h3>
        <button @click="close" class="text-slate-400 hover:text-slate-600 transition-colors p-2 hover:bg-slate-100 rounded-full">
          <X class="w-5 h-5" />
        </button>
      </div>

      <!-- Body -->
      <div class="flex-1 overflow-y-auto p-6 space-y-6">
        
        <!-- Warning when editing used question -->
        <div v-if="mode === 'edit' && questionData?.usageCount && questionData.usageCount > 0" class="bg-amber-50 border border-amber-200 p-4 rounded-xl flex gap-3 text-sm text-amber-800">
          <span class="text-xl leading-none">⚠️</span>
          <div>
            <span class="font-bold">Cảnh báo: </span>
            Câu hỏi này đang được sử dụng trong {{ questionData.usageCount }} Quiz. Thay đổi nội dung có thể ảnh hưởng đến các đề thi hiện tại.
          </div>
        </div>

        <div class="grid grid-cols-1 lg:grid-cols-12 gap-6">
          <!-- Cột 1: Thông tin phân loại -->
          <div class="lg:col-span-4 bg-white p-5 rounded-xl shadow-sm border border-slate-200 space-y-5 h-max">
            <h4 class="font-bold text-slate-800 text-lg mb-2">Phân loại & Thuộc tính</h4>
            
            <div>
              <LmsSelect 
                :model-value="formData.subjectId"
                @update:model-value="formData.subjectId = $event"
                :options="subjectOptions"
                label="Môn học"
                required
              />
            </div>

            <div class="space-y-4">
              <div>
                <label class="block text-sm font-medium text-slate-700 mb-1">Loại câu hỏi <span class="text-red-500">*</span></label>
                <LmsSelect 
                  :model-value="formData.type"
                  @update:model-value="handleTypeChange"
                  :options="typeOptions"
                />
              </div>
              <div v-if="formData.type === 'multiple_choice'">
                <label class="block text-sm font-medium text-slate-700 mb-1">Kiểu lựa chọn <span class="text-red-500">*</span></label>
                <LmsSelect 
                  :model-value="formData.selectionType"
                  @update:model-value="handleSelectionTypeChange"
                  :options="selectionOptions"
                />
              </div>
            </div>

            <div class="grid grid-cols-2 gap-4">
              <div>
                <LmsSelect 
                  :model-value="formData.difficulty"
                  @update:model-value="formData.difficulty = $event"
                  :options="difficultyOptions"
                  label="Độ khó"
                  required
                />
              </div>
              <div v-if="mode !== 'create'">
                <LmsSelect 
                  :model-value="formData.status"
                  @update:model-value="formData.status = $event"
                  :options="statusOptions"
                  label="Trạng thái"
                />
              </div>
            </div>
            
            <p class="text-xs text-slate-500 mt-1" v-if="formData.difficulty === 'easy'">
              Dễ: Kiểm tra khả năng ghi nhớ hoặc nhận biết.
            </p>
            <p class="text-xs text-slate-500 mt-1" v-if="formData.difficulty === 'medium'">
              Trung bình: Kiểm tra khả năng hiểu và áp dụng.
            </p>
            <p class="text-xs text-slate-500 mt-1" v-if="formData.difficulty === 'hard'">
              Khó: Kiểm tra khả năng phân tích hoặc giải quyết vấn đề.
            </p>
          </div>

          <!-- Cột 2: Nội dung câu hỏi -->
          <div class="lg:col-span-8 bg-white p-5 rounded-xl shadow-sm border border-slate-200 space-y-6 h-max">
            <h4 class="font-bold text-slate-800 text-lg">Nội dung câu hỏi</h4>
            
            <div>
              <label class="block text-sm font-medium text-slate-700 mb-2">Đề bài <span class="text-red-500">*</span></label>
              <textarea 
                v-model="formData.content"
                rows="4"
                class="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 resize-y"
                placeholder="Nhập nội dung câu hỏi..."
              ></textarea>
              <div class="flex justify-end mt-1 text-xs text-slate-400">
                {{ formData.content?.length || 0 }} ký tự
              </div>
            </div>

            <hr class="border-slate-100" />

            <!-- Editor Lựa chọn cho Trắc nghiệm -->
            <div v-if="formData.type === 'multiple_choice'">
              <QuestionChoiceEditor 
                :choices="formData.choices || []"
                @update:choices="formData.choices = $event"
                :correct-answer-ids="formData.correctAnswerIds || []"
                @update:correct-answer-ids="formData.correctAnswerIds = $event"
                :selection-type="(formData.selectionType as 'single'|'multiple')"
              />
            </div>

            <!-- Đáp án mẫu cho Tự luận -->
            <div v-if="formData.type === 'essay'">
              <label class="block text-sm font-medium text-slate-700 mb-2">Đáp án mẫu (Tùy chọn)</label>
              <p class="text-xs text-slate-500 mb-2">Đáp án này giúp giảng viên tham khảo khi chấm bài.</p>
              <textarea 
                v-model="formData.sampleAnswer"
                rows="4"
                class="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 resize-y"
                placeholder="Nhập đáp án mẫu..."
              ></textarea>
            </div>

            <hr class="border-slate-100" />

            <div>
              <label class="block text-sm font-medium text-slate-700 mb-2">
                {{ formData.type === 'multiple_choice' ? 'Giải thích đáp án' : 'Hướng dẫn chấm' }} (Tùy chọn)
              </label>
              <textarea 
                v-model="formData.answerExplanation"
                rows="3"
                class="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 resize-y"
                :placeholder="formData.type === 'multiple_choice' ? 'Giải thích vì sao đáp án trên là đúng...' : 'Các tiêu chí để chấm điểm...'"
              ></textarea>
            </div>

          </div>
        </div>

        <!-- Preview Section -->
        <div class="bg-slate-800 text-slate-100 p-5 rounded-xl shadow-inner mt-6">
          <div class="flex items-center justify-between mb-4 border-b border-slate-700 pb-3">
            <h4 class="font-bold text-lg flex items-center gap-2">
              <span class="text-xl">👁️</span> Xem trước câu hỏi
            </h4>
            <div class="flex items-center gap-2">
              <span class="text-sm text-slate-400">Hiển thị đáp án đúng</span>
              <button 
                @click="isPreviewAnswerVisible = !isPreviewAnswerVisible"
                class="w-10 h-5 rounded-full relative transition-colors focus:outline-none"
                :class="isPreviewAnswerVisible ? 'bg-blue-500' : 'bg-slate-600'"
              >
                <div 
                  class="absolute top-0.5 left-0.5 w-4 h-4 bg-white rounded-full transition-transform shadow"
                  :class="isPreviewAnswerVisible ? 'translate-x-5' : 'translate-x-0'"
                ></div>
              </button>
            </div>
          </div>
          
          <div class="bg-white text-slate-800 rounded-lg p-6 font-sans">
            <!-- Đề bài -->
            <SafeHtmlRenderer class="text-base leading-relaxed mb-6 whitespace-pre-wrap" :html="formData.content || `<span class='text-slate-400 italic'>Chưa có đề bài...</span>`" />
            
            <!-- Lựa chọn (Trắc nghiệm) -->
            <div v-if="formData.type === 'multiple_choice'" class="space-y-3">
              <div 
                v-for="(choice, index) in formData.choices" 
                :key="choice.id"
                class="flex items-start gap-3 p-3 rounded-lg border transition-colors"
                :class="{
                  'bg-green-50 border-green-200': isPreviewAnswerVisible && (formData.correctAnswerIds || []).includes(choice.id),
                  'bg-slate-50 border-slate-200': !isPreviewAnswerVisible || !(formData.correctAnswerIds || []).includes(choice.id)
                }"
              >
                <div class="w-6 h-6 shrink-0 rounded-full bg-white border border-slate-300 flex items-center justify-center text-xs font-bold text-slate-500"
                  :class="{'border-green-500 text-green-600': isPreviewAnswerVisible && (formData.correctAnswerIds || []).includes(choice.id)}"
                >
                  {{ String.fromCharCode(65 + index) }}
                </div>
                <div class="pt-0.5" :class="{'font-medium text-green-800': isPreviewAnswerVisible && (formData.correctAnswerIds || []).includes(choice.id)}">
                  {{ choice.content || '(Trống)' }}
                </div>
              </div>
            </div>

            <!-- Tự luận -->
            <div v-if="formData.type === 'essay'" class="mt-4">
              <textarea disabled rows="4" class="w-full bg-slate-50 border border-slate-200 rounded p-3 text-slate-400" placeholder="Khu vực sinh viên nhập câu trả lời..."></textarea>
              <div v-if="isPreviewAnswerVisible && formData.sampleAnswer" class="mt-4 p-4 bg-amber-50 border border-amber-200 rounded-lg text-amber-900 text-sm whitespace-pre-wrap">
                <div class="font-bold mb-1">Đáp án mẫu:</div>
                {{ formData.sampleAnswer }}
              </div>
            </div>

            <!-- Giải thích -->
            <div v-if="isPreviewAnswerVisible && formData.answerExplanation" class="mt-6 p-4 bg-blue-50 border border-blue-200 rounded-lg text-blue-900 text-sm whitespace-pre-wrap">
              <div class="font-bold mb-1">{{ formData.type === 'multiple_choice' ? 'Giải thích đáp án:' : 'Hướng dẫn chấm:' }}</div>
              {{ formData.answerExplanation }}
            </div>
          </div>
        </div>

      </div>

      <!-- Footer -->
      <div class="px-6 py-4 border-t border-slate-200 bg-white flex items-center justify-between shrink-0">
        <div class="text-xs text-slate-500 italic">
          Dữ liệu sẽ được đặt lại khi tải lại trang.
        </div>
        <div class="flex items-center gap-3">
          <button @click="close" class="px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100 rounded-lg transition-colors">
            Hủy
          </button>
          <button @click="validateAndSave" class="px-5 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors flex items-center gap-2">
            <component :is="getSaveButtonIcon()" class="w-4 h-4" />
            <span>{{ getSaveButtonText() }}</span>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
