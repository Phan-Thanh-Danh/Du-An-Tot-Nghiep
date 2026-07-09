<script setup lang="ts">
import { inject, ref, watch, computed } from 'vue'
import { X, Save, Loader2 } from 'lucide-vue-next'
import SlideHtmlEditor from './content/SlideHtmlEditor.vue'
import LmsSelect from '@/components/LmsSelect.vue'
import { storageApi } from '@/services/apiClient'

const editor = inject<any>('curriculumEditor')
const isOpen = editor.isContentDrawerOpen

const isEdit = ref(false)
const contentType = computed(() => editor.selectedContentType.value)

const formData = ref<any>({})
const isSaving = ref(false)

const statusOptions = [
  { value: 'draft', label: 'Nháp' },
  { value: 'published', label: 'Xuất bản' },
  { value: 'hidden', label: 'Đang ẩn' }
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
    case 'quiz': return `${prefix} Quiz`
    default: return `${prefix} nội dung`
  }
}

watch(() => isOpen.value, (val) => {
  if (val) {
    if (editor.editingContent.value) {
      isEdit.value = true
      formData.value = { ...editor.editingContent.value }
    } else {
      isEdit.value = false
      formData.value = {
        title: editor.selectedLesson.value?.title || '',
        status: 'draft',
        type: contentType.value,
        order: null,
        description: '',
        // fields for various types
        videoUrl: '',
        durationSeconds: 0,
        html: '',
        fileUrl: '',
        fileName: '',
        fileSize: 0,
        fileType: '',
        quizId: undefined,
        quizCompletionRule: 'pass',
        NoiDungJson: '',
        rawFile: null
      }
    }
  }
})

const close = () => {
  isOpen.value = false
}

const save = async () => {
  if (!formData.value.title?.trim()) {
    alert('Vui lòng nhập tiêu đề')
    return
  }
  if (contentType.value === 'slide_html') {
    if (!formData.value.NoiDungJson || formData.value.NoiDungJson === '{}') {
      alert('Nội dung slide không được để trống.')
      return
    }
  }
  
  isSaving.value = true
  try {
    // Upload if there's a new file
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

    const dataToSave = { ...formData.value }
    delete dataToSave.rawFile

    editor.saveContent(dataToSave)
  } catch (error: any) {
    alert('Lỗi upload file: ' + (error.message || 'Vui lòng thử lại sau'))
  } finally {
    isSaving.value = false
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

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-slate-700 mb-1">Trạng thái</label>
                <LmsSelect 
                  v-model="formData.status"
                  :options="statusOptions"
                  placeholder="Chọn trạng thái"
                />
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
              <SlideHtmlEditor v-if="isOpen && contentType === 'slide_html'" v-model="formData.NoiDungJson" />
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
            <h4 class="font-semibold text-slate-800 mb-2">Thiết lập Quiz</h4>
            
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-slate-700 mb-1">Tên Quiz</label>
                <input 
                  v-model="formData.quizTitle" 
                  type="text" 
                  class="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
              </div>
              <div>
                <label class="block text-sm font-medium text-slate-700 mb-1">Thời gian (phút)</label>
                <input 
                  v-model.number="formData.quizDurationMinutes" 
                  type="number" 
                  class="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
              </div>
              <div>
                <label class="block text-sm font-medium text-slate-700 mb-1">Số câu hỏi</label>
                <input 
                  v-model.number="formData.quizQuestionCount" 
                  type="number" 
                  class="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
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
