<script setup lang="ts">
import { inject, ref, watch, computed } from 'vue'
import { X, Save, Loader2, HelpCircle } from 'lucide-vue-next'
import SlideHtmlEditor from './content/SlideHtmlEditor.vue'
import LmsSelect from '@/components/LmsSelect.vue'
import { storageApi } from '@/services/apiClient'
import { contentCouncilApi } from '@/services/contentCouncilApi'

const editor = inject<any>('curriculumEditor')
const isOpen = editor.isContentDrawerOpen

const isEdit = ref(false)
const contentType = computed(() => editor.selectedContentType.value)

const formData = ref<any>({})
const isSaving = ref(false)

const availableQuizzes = ref<any[]>([])
const isLoadingQuizzes = ref(false)
const selectedQuizDetail = ref<any>(null)

const statusOptions = [
  { value: 'nhap', label: 'Nháp' },
  { value: 'da_xuat_ban', label: 'Xuất bản' }
]

const quizCompletionOptions = [
  { value: 'pass', label: 'Phải đạt' },
  { value: 'submit', label: 'Chỉ cần nộp bài' }
]

const getDrawerTitle = () => {
  const prefix = isEdit.value ? 'Chỉnh sửa' : 'Thêm'
  switch (contentType.value) {
    case 'video': return `${prefix} Video`
    case 'slide_html': return `${prefix} Slide HTML`
    case 'document': return `${prefix} Tài liệu`
    case 'quiz': return `${prefix} Quiz / Đề kiểm tra`
    default: return `${prefix} nội dung`
  }
}

const loadAvailableQuizzes = async () => {
  const subjectId = editor.subjectId || editor.selectedChapter?.value?.subjectId
  if (!subjectId) return
  isLoadingQuizzes.value = true
  try {
    const res = await contentCouncilApi.getQuizzes({ maMonHoc: subjectId, pageSize: 100 })
    const items = res?.data?.items ?? res?.data?.Items ?? res?.items ?? res?.Items ?? (Array.isArray(res?.data) ? res.data : (Array.isArray(res) ? res : []))
    availableQuizzes.value = Array.isArray(items) ? items : []
    
    if (formData.value.quizId || formData.value.maDeKiemTra) {
      formData.value.quizId = formData.value.quizId || formData.value.maDeKiemTra
      onQuizSelect()
    }
  } catch (e) {
    console.error('Lỗi nạp danh sách Quiz:', e)
  } finally {
    isLoadingQuizzes.value = false
  }
}

const onQuizSelect = () => {
  const targetId = formData.value.quizId
  if (!targetId) {
    selectedQuizDetail.value = null
    return
  }
  const q = availableQuizzes.value.find((item: any) => (item.maDeKiemTra ?? item.MaDeKiemTra ?? item.id) === Number(targetId))
  if (q) {
    selectedQuizDetail.value = q
    const title = q.tieuDe ?? q.TieuDe ?? q.title ?? ''
    formData.value.title = title || formData.value.title
    formData.value.quizTitle = title
    formData.value.quizDurationMinutes = q.thoiGianPhut ?? q.ThoiGianPhut ?? 15
    formData.value.quizQuestionCount = q.soCauHoi ?? q.SoCauHoi ?? 0
    formData.value.maDeKiemTra = Number(targetId)
  }
}

watch(() => isOpen.value, (val) => {
  if (val) {
    if (editor.editingContent.value) {
      isEdit.value = true
      formData.value = { ...editor.editingContent.value }
      const currentSt = String(formData.value.status || '').toLowerCase()
      if (currentSt === 'published' || currentSt === 'da_xuat_ban') {
        formData.value.status = 'da_xuat_ban'
      } else if (currentSt === 'hidden' || currentSt === 'an') {
        formData.value.status = 'an'
      } else {
        formData.value.status = 'nhap'
      }
      
      let parsedJson: any = {}
      if (formData.value.NoiDungJson) {
        try {
          parsedJson = typeof formData.value.NoiDungJson === 'string'
            ? JSON.parse(formData.value.NoiDungJson)
            : formData.value.NoiDungJson
        } catch (e) {
          console.error('Lỗi parse NoiDungJson:', e)
        }
      } else if (formData.value.data) {
        parsedJson = formData.value.data
      }

      formData.value.description = formData.value.description || parsedJson.description || parsedJson.moTa || ''
      formData.value.quizCompletionRule = formData.value.quizCompletionRule || parsedJson.quizCompletionRule || 'pass'

      if (formData.value.maDeKiemTra && !formData.value.quizId) {
        formData.value.quizId = formData.value.maDeKiemTra
      }
      if (!formData.value.title) {
        formData.value.title = editor.selectedLesson.value?.title || ''
      }
      if (formData.value.type === 'video' && !formData.value.videoUrl) {
        formData.value.videoUrl = formData.value.fileUrl
      }
    } else {
      isEdit.value = false
      formData.value = {
        title: editor.selectedLesson.value?.title || '',
        status: 'nhap',
        type: contentType.value,
        order: null,
        description: '',
        videoUrl: '',
        durationSeconds: 0,
        html: '',
        fileUrl: '',
        fileName: '',
        fileSize: 0,
        fileType: '',
        quizId: undefined,
        maDeKiemTra: undefined,
        quizCompletionRule: 'pass',
        NoiDungJson: '',
        rawFile: null
      }
    }

    if (contentType.value === 'quiz') {
      loadAvailableQuizzes()
    }
  }
})

const slideEditorRef = ref<any>(null)

const close = () => {
  isOpen.value = false
}

const save = async () => {
  if (!formData.value.title?.trim()) {
    alert('Vui lòng nhập tiêu đề')
    return
  }

  if (contentType.value === 'slide_html') {
    if (slideEditorRef.value?.saveData) {
      const slideJson = await slideEditorRef.value.saveData()
      if (slideJson) {
        formData.value.NoiDungJson = slideJson
      }
    }
    if (!formData.value.NoiDungJson || formData.value.NoiDungJson === '{}' || formData.value.NoiDungJson === '{"blocks":[]}') {
      alert('Nội dung slide không được để trống. Vui lòng nhập thông tin vào trình soạn thảo.')
      return
    }
  } else {
    if (contentType.value === 'quiz') {
      if (!formData.value.quizId && !formData.value.maDeKiemTra) {
        alert('Vui lòng chọn một Quiz / Đề kiểm tra từ danh sách')
        return
      }
      formData.value.maDeKiemTra = Number(formData.value.quizId || formData.value.maDeKiemTra)
      formData.value.quizId = formData.value.maDeKiemTra
    }

    const jsonPayload = {
      description: formData.value.description || '',
      quizCompletionRule: formData.value.quizCompletionRule || 'pass',
      quizTitle: formData.value.quizTitle || formData.value.title || '',
      quizId: formData.value.quizId || formData.value.maDeKiemTra,
      title: formData.value.title || ''
    }
    formData.value.NoiDungJson = JSON.stringify(jsonPayload)
    formData.value.data = jsonPayload
  }
  
  isSaving.value = true
  try {
    if (formData.value.rawFile) {
      const folder = contentType.value === 'video' ? 'videos' : 'documents'
      const response = await storageApi.upload(formData.value.rawFile, folder)
      
      if (response && response.success && response.data) {
        const result = Array.isArray(response.data) ? response.data[0] : response.data
        if (contentType.value === 'video') {
          formData.value.videoUrl = result.url || result.Url
        } else if (contentType.value === 'document') {
          formData.value.fileUrl = result.url || result.Url
        }
      }
    }

    if (contentType.value === 'video') {
      formData.value.fileUrl = formData.value.videoUrl
    }

    const dataToSave = { ...formData.value }
    delete dataToSave.rawFile

    await editor.saveContent(dataToSave)
    isSaving.value = false
  } catch (error: any) {
    isSaving.value = false
    alert('Lỗi lưu nội dung: ' + (error.message || 'Vui lòng thử lại sau'))
  }
}

const onFileChange = (e: any) => {
  const file = e.target.files[0]
  if (file) {
    formData.value.rawFile = file
    formData.value.fileName = file.name
    formData.value.fileSize = file.size
    formData.value.fileType = file.type
    // Fake local object URL for preview
    formData.value.fileUrl = URL.createObjectURL(file)
  } else {
    formData.value.rawFile = null
  }
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
      class="fixed inset-y-0 right-0 z-50 w-full sm:w-[640px] md:w-[800px] bg-white shadow-2xl flex flex-col transform transition-transform duration-300"
      :class="isOpen ? 'translate-x-0' : 'translate-x-full'"
    >
      <!-- Header -->
      <div class="px-6 py-4 border-b border-slate-100 flex items-center justify-between bg-white">
        <h3 class="text-xl font-bold text-slate-800">{{ getDrawerTitle() }}</h3>
        <button @click="close" class="text-slate-400 hover:text-slate-600 transition-colors p-2 hover:bg-slate-100 rounded-full">
          <X class="w-5 h-5" />
        </button>
      </div>

      <!-- Body -->
      <div class="flex-1 overflow-y-auto p-6 bg-slate-50">
        <div class="space-y-6 max-w-3xl mx-auto">
          
          <!-- Common Fields -->
          <div class="bg-white p-5 rounded-xl border border-slate-200 space-y-4">
            <h4 class="font-semibold text-slate-800 mb-2">Thông tin chung</h4>
            
            <div>
              <label class="block text-sm font-medium text-slate-700 mb-1">Tiêu đề <span class="text-red-500">*</span></label>
              <input 
                v-model="formData.title" 
                type="text" 
                class="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Nhập tiêu đề..."
              >
            </div>

            <div>
              <label class="block text-sm font-medium text-slate-700 mb-1">Thứ tự</label>
              <input 
                v-model.number="formData.order" 
                type="number" 
                class="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Để trống để thêm vào cuối"
              >
            </div>

            <div>
              <label class="block text-sm font-medium text-slate-700 mb-1">Mô tả ngắn</label>
              <textarea 
                v-model="formData.description" 
                rows="2"
                class="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Nhập mô tả..."
              ></textarea>
            </div>
          </div>

          <!-- Video Specific Fields -->
          <div v-if="contentType === 'video'" class="bg-white p-5 rounded-xl border border-slate-200 space-y-4">
            <h4 class="font-semibold text-slate-800 mb-2">Nguồn Video</h4>
            
            <div>
              <label class="block text-sm font-medium text-slate-700 mb-1">Upload Video (MP4, WebM...)</label>
              <input 
                type="file" 
                accept="video/*"
                @change="onFileChange"
                class="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
              <p v-if="formData.fileName && contentType === 'video'" class="text-sm text-blue-600 mt-2 flex items-center gap-2">
                Đã chọn: {{ formData.fileName }} ({{ (formData.fileSize / 1024 / 1024).toFixed(2) }} MB)
              </p>
            </div>

            <div>
              <label class="block text-sm font-medium text-slate-700 mb-1">Hoặc URL Video (YouTube, Vimeo...)</label>
              <input 
                v-model="formData.videoUrl" 
                type="url" 
                class="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="https://..."
              >
            </div>
          </div>

          <!-- Slide HTML Specific Fields -->
          <div v-if="contentType === 'slide_html'" class="bg-white p-5 rounded-xl border border-slate-200 space-y-4">
            <h4 class="font-semibold text-slate-800 mb-2">Trình soạn thảo HTML</h4>
            <div class="min-h-[400px]">
              <!-- We will integrate Editor.js here -->
              <SlideHtmlEditor v-if="isOpen && contentType === 'slide_html'" ref="slideEditorRef" v-model="formData.NoiDungJson" />
            </div>
          </div>

          <!-- Document Specific Fields -->
          <div v-if="contentType === 'document'" class="bg-white p-5 rounded-xl border border-slate-200 space-y-4">
            <h4 class="font-semibold text-slate-800 mb-2">Tài liệu đính kèm</h4>
            
            <div>
              <label class="block text-sm font-medium text-slate-700 mb-1">Upload File (PDF, DOCX...)</label>
              <input 
                type="file" 
                @change="onFileChange"
                class="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
              <p v-if="formData.fileName && contentType === 'document'" class="text-sm text-blue-600 mt-2 flex items-center gap-2">
                Đã chọn: {{ formData.fileName }} ({{ (formData.fileSize / 1024 / 1024).toFixed(2) }} MB)
              </p>
            </div>
            
            <div>
              <label class="block text-sm font-medium text-slate-700 mb-1">Hoặc URL tài liệu</label>
              <input 
                v-model="formData.fileUrl" 
                type="url" 
                class="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="https://..."
              >
            </div>
          </div>

          <!-- Text Specific Fields -->
          <div v-if="contentType === 'text'" class="bg-white p-5 rounded-xl border border-slate-200 space-y-4">
            <h4 class="font-semibold text-slate-800 mb-2">Nội dung văn bản</h4>
            <div>
              <!-- Mock simple textarea instead of full rich text editor for basic text if requested -->
              <textarea 
                v-model="formData.html" 
                rows="8"
                class="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Nhập nội dung văn bản..."
              ></textarea>
            </div>
          </div>

          <!-- Quiz Specific Fields -->
          <div v-if="contentType === 'quiz'" class="bg-white p-5 rounded-xl border border-slate-200 space-y-4">
            <h4 class="font-semibold text-slate-800 mb-2">Chọn Quiz từ Ngân hàng Đề kiểm tra</h4>
            
            <div v-if="isLoadingQuizzes" class="py-4 text-center text-sm text-slate-500">
              <Loader2 class="w-5 h-5 animate-spin mx-auto mb-2 text-blue-600" />
              Đang tải danh sách Quiz của môn học...
            </div>
            
            <div v-else-if="availableQuizzes.length === 0" class="p-4 bg-amber-50 rounded-lg border border-amber-200 text-amber-800 text-sm">
              <p class="font-semibold mb-1">Môn học này chưa có Quiz nào được tạo.</p>
              <p class="text-xs text-amber-700">Vui lòng vào mục <strong>Quiz / Đề kiểm tra</strong> ở menu bên trái để tạo đề kiểm tra cho môn học trước khi đính kèm vào bài học.</p>
            </div>
            
            <div v-else class="space-y-4">
              <div>
                <label class="block text-sm font-medium text-slate-700 mb-1">Chọn Đề kiểm tra *</label>
                <select 
                  v-model="formData.quizId" 
                  @change="onQuizSelect"
                  class="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white text-slate-800"
                >
                  <option :value="undefined" disabled>-- Chọn đề kiểm tra từ danh sách --</option>
                  <option v-for="q in availableQuizzes" :key="q.maDeKiemTra || q.id" :value="q.maDeKiemTra || q.id">
                    [{{ q.code || `QZ-${q.maDeKiemTra || q.id}` }}] {{ q.tieuDe || q.title }} ({{ q.thoiGianPhut || q.durationMinutes || 15 }} phút, {{ q.soCauHoi || q.questionCount || 0 }} câu)
                  </option>
                </select>
              </div>

              <!-- Selected Quiz Preview Details -->
              <div v-if="selectedQuizDetail" class="bg-slate-50 p-4 rounded-lg border border-slate-200 space-y-2 text-sm">
                <div class="flex justify-between">
                  <span class="text-slate-500">Mã đề:</span>
                  <span class="font-mono text-xs font-semibold text-slate-700 bg-slate-200 px-2 py-0.5 rounded">{{ selectedQuizDetail.code || `QZ-${selectedQuizDetail.maDeKiemTra || selectedQuizDetail.id}` }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-slate-500">Tên đề thi:</span>
                  <span class="font-semibold text-slate-800">{{ selectedQuizDetail.title || selectedQuizDetail.tieuDe }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-slate-500">Thời gian làm bài:</span>
                  <span class="font-medium text-slate-800">{{ selectedQuizDetail.durationMinutes || selectedQuizDetail.thoiGianPhut }} phút</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-slate-500">Số câu hỏi:</span>
                  <span class="font-medium text-slate-800">{{ selectedQuizDetail.questionCount || selectedQuizDetail.soCauHoi || 0 }} câu</span>
                </div>
                <div class="flex justify-between border-t border-slate-200 pt-2">
                  <span class="text-slate-500">Tổng điểm:</span>
                  <span class="font-bold text-blue-600">{{ selectedQuizDetail.totalScore || selectedQuizDetail.tongDiem || 10 }} điểm</span>
                </div>
              </div>

              <div>
                <label class="block text-sm font-medium text-slate-700 mb-1">Điều kiện hoàn thành</label>
                <LmsSelect 
                  v-model="formData.quizCompletionRule"
                  :options="quizCompletionOptions"
                  placeholder="Chọn điều kiện"
                />
              </div>
            </div>
          </div>

        </div>
      </div>

      <!-- Footer -->
      <div class="px-6 py-4 border-t border-slate-200 bg-white flex items-center justify-end gap-3 z-10">
        <button @click="close" :disabled="isSaving" class="px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100 rounded-lg transition-colors disabled:opacity-50">
          Hủy
        </button>
        <button @click="save" :disabled="isSaving" class="px-5 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors flex items-center gap-2 disabled:opacity-50">
          <Loader2 v-if="isSaving" class="w-4 h-4 animate-spin" />
          <Save v-else class="w-4 h-4" />
          <span>{{ isSaving ? 'Đang lưu...' : 'Lưu nội dung' }}</span>
        </button>
      </div>
    </div>
  </div>
</template>
