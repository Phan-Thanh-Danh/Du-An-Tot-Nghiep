<script setup lang="ts">
import { ref } from 'vue'
import { X, UploadCloud, Download, FileSpreadsheet, CheckCircle2, AlertCircle } from 'lucide-vue-next'
import { contentCouncilApi } from '@/services/contentCouncilApi'
import ConfirmModal from '@/components/common/ConfirmModal.vue'

const props = defineProps<{
  isOpen: boolean
}>()

const emit = defineEmits(['update:isOpen', 'import'])

const fileInput = ref<HTMLInputElement | null>(null)
const selectedFile = ref<File | null>(null)
const isChecking = ref(false)
const checkResult = ref<{
  valid: number
  invalid: number
  total: number
  errors: Array<{row: number, col: string, msg: string}>
} | null>(null)

const modalState = ref({
  isOpen: false,
  title: 'Thông báo',
  message: '',
  variant: 'warning' as 'warning' | 'danger' | 'info' | 'success',
  confirmText: 'Đóng',
  cancelText: 'Hủy',
  isAlert: true
})

const showAlert = (msg: string, title = 'Thông báo', variant: 'warning' | 'danger' | 'info' | 'success' = 'warning') => {
  modalState.value = {
    isOpen: true,
    title,
    message: msg,
    variant,
    confirmText: 'Đóng',
    cancelText: 'Hủy',
    isAlert: true
  }
}

const close = () => {
  selectedFile.value = null
  checkResult.value = null
  emit('update:isOpen', false)
}

const onFileSelect = (e: Event) => {
  const target = e.target as HTMLInputElement
  if (target.files && target.files.length > 0) {
    const file = target.files[0]
    if (!file.name.endsWith('.xlsx') && !file.name.endsWith('.csv')) {
      showAlert('Vui lòng chọn file Excel (.xlsx) hoặc CSV.', 'Định dạng file không hỗ trợ', 'warning')
      return
    }
    if (file.size > 10 * 1024 * 1024) {
      showAlert('Dung lượng file không được vượt quá 10MB.', 'File quá lớn', 'warning')
      return
    }
    selectedFile.value = file
    checkResult.value = null
  }
}

const triggerFileSelect = () => {
  fileInput.value?.click()
}

const removeFile = () => {
  selectedFile.value = null
  checkResult.value = null
  if (fileInput.value) fileInput.value.value = ''
}

import * as XLSX from 'xlsx'

const isUploading = ref(false)

const checkFile = async () => {
  if (!selectedFile.value) return
  isChecking.value = true
  
  try {
    const arrayBuffer = await selectedFile.value.arrayBuffer()
    const workbook = XLSX.read(arrayBuffer, { type: 'array' })
    const sheetName = workbook.SheetNames.includes('Questions') ? 'Questions' : workbook.SheetNames[0]
    const worksheet = sheetName ? workbook.Sheets[sheetName] : null
    
    if (!worksheet) {
      isChecking.value = false
      checkResult.value = {
        total: 0,
        valid: 0,
        invalid: 1,
        errors: [{ row: 1, col: 'File', msg: 'Không tìm thấy sheet dữ liệu trong file Excel' }]
      }
      return
    }

    const jsonData: any[] = XLSX.utils.sheet_to_json(worksheet, { defval: '' })
    const errors: Array<{row: number, col: string, msg: string}> = []
    let validCount = 0

    jsonData.forEach((row: any, idx: number) => {
      const rowNum = idx + 2 // Row 1 is header
      const maMonHoc = String(row['MaCodeMonHoc'] || row['Mã môn'] || row['MaMonHoc'] || '').trim()
      const loai = String(row['LoaiCauHoi'] || row['Loại câu hỏi'] || '').trim()
      const noiDung = String(row['NoiDung'] || row['Nội dung'] || '').trim()

      if (!maMonHoc) {
        errors.push({ row: rowNum, col: 'MaCodeMonHoc', msg: 'Thiếu mã môn học' })
      } else if (!noiDung) {
        errors.push({ row: rowNum, col: 'NoiDung', msg: 'Thiếu nội dung câu hỏi' })
      } else if (loai === 'trac_nghiem') {
        const choiceA = String(row['LuaChonA'] || row['Lựa chọn A'] || row['Đáp án A'] || '').trim()
        const choiceB = String(row['LuaChonB'] || row['Lựa chọn B'] || row['Đáp án B'] || '').trim()
        const legacyChoices = String(row['LuaChon'] || row['Lựa chọn'] || '').trim()
        const dapAnDung = String(row['DapAnDung'] || row['Đáp án đúng'] || '').trim()

        const hasOptionCols = choiceA || choiceB
        const hasLegacyChoice = legacyChoices

        if (!hasOptionCols && !hasLegacyChoice) {
          errors.push({ row: rowNum, col: 'LuaChonA', msg: 'Câu trắc nghiệm phải có ít nhất các lựa chọn A và B' })
        } else if (!dapAnDung) {
          errors.push({ row: rowNum, col: 'DapAnDung', msg: 'Câu trắc nghiệm phải có đáp án đúng' })
        } else {
          validCount++
        }
      } else {
        validCount++
      }
    })

    checkResult.value = {
      total: jsonData.length,
      valid: validCount,
      invalid: jsonData.length - validCount,
      errors
    }
  } catch (err: any) {
    showAlert('Không thể đọc dữ liệu file Excel. Vui lòng kiểm tra định dạng file.', 'Lỗi đọc file', 'danger')
    checkResult.value = { total: 0, valid: 0, invalid: 0, errors: [] }
  } finally {
    isChecking.value = false
  }
}

const downloadTemplate = async () => {
  try {
    const token = localStorage.getItem('lms_access_token') || sessionStorage.getItem('lms_access_token') || ''
    const response = await fetch('/api/question-bank/questions/import-template', {
      headers: { Authorization: `Bearer ${token}` }
    })
    if (!response.ok) throw new Error('Không thể tải file mẫu')
    const blob = await response.blob()
    const url = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = 'QuestionImportTemplate.xlsx'
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
    window.URL.revokeObjectURL(url)
  } catch (err: any) {
    showAlert(err?.message || 'Không thể tải file mẫu.', 'Lỗi tải file mẫu', 'danger')
  }
}

const importData = async () => {
  if (!selectedFile.value) return
  isUploading.value = true
  try {
    const formData = new FormData()
    formData.append('file', selectedFile.value)
    const res = await contentCouncilApi.importQuestions(formData)
    const count = res?.count ?? 1
    emit('import', count)
    close()
  } catch (err: any) {
    showAlert(err?.message || 'Có lỗi xảy ra khi import câu hỏi từ file Excel.', 'Lỗi import', 'danger')
  } finally {
    isUploading.value = false
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

    <!-- Modal -->
    <div 
      v-if="isOpen"
      class="fixed inset-0 z-50 flex items-center justify-center p-4 sm:p-0"
    >
      <div class="bg-white rounded-xl shadow-2xl w-full max-w-2xl overflow-hidden flex flex-col max-h-[90vh]">
        <!-- Header -->
        <div class="px-6 py-4 border-b border-slate-100 flex items-center justify-between shrink-0">
          <h3 class="text-xl font-bold text-slate-800">Import câu hỏi từ Excel</h3>
          <button @click="close" class="text-slate-400 hover:text-slate-600 transition-colors p-1.5 hover:bg-slate-100 rounded-full">
            <X class="w-5 h-5" />
          </button>
        </div>

        <!-- Body -->
        <div class="flex-1 overflow-y-auto p-6 space-y-8">
          
          <!-- Step 1 -->
          <div>
            <div class="flex items-center gap-3 mb-2">
              <div class="w-6 h-6 rounded-full bg-blue-100 text-blue-700 flex items-center justify-center text-sm font-bold">1</div>
              <h4 class="font-bold text-slate-800">Tải file mẫu</h4>
            </div>
            <p class="text-sm text-slate-500 pl-9 mb-3">Sử dụng file mẫu để đảm bảo cấu trúc dữ liệu chính xác trước khi upload.</p>
            <div class="pl-9">
              <button @click="downloadTemplate" class="px-4 py-2 border border-slate-300 rounded-lg text-sm font-medium text-slate-700 hover:bg-slate-50 flex items-center gap-2 transition-colors">
                <Download class="w-4 h-4" /> Tải file mẫu (.xlsx)
              </button>
            </div>
          </div>

          <!-- Step 2 -->
          <div>
            <div class="flex items-center gap-3 mb-2">
              <div class="w-6 h-6 rounded-full bg-blue-100 text-blue-700 flex items-center justify-center text-sm font-bold">2</div>
              <h4 class="font-bold text-slate-800">Tải file lên</h4>
            </div>
            <p class="text-sm text-slate-500 pl-9 mb-3">Upload file Excel đã điền dữ liệu. Tối đa 10MB.</p>
            <div class="pl-9">
              
              <div v-if="!selectedFile" 
                class="border-2 border-dashed border-slate-300 rounded-xl p-8 flex flex-col items-center justify-center text-center hover:bg-slate-50 hover:border-blue-400 cursor-pointer transition-colors"
                @click="triggerFileSelect"
              >
                <div class="w-12 h-12 bg-blue-50 text-blue-500 rounded-full flex items-center justify-center mb-3">
                  <UploadCloud class="w-6 h-6" />
                </div>
                <div class="text-slate-700 font-medium mb-1">Kéo thả file vào đây hoặc bấm để chọn</div>
                <div class="text-xs text-slate-500">Hỗ trợ .xlsx, .csv</div>
                <input type="file" class="hidden" ref="fileInput" accept=".xlsx,.csv" @change="onFileSelect">
              </div>

              <div v-else class="bg-slate-50 border border-slate-200 rounded-xl p-4 flex items-center justify-between">
                <div class="flex items-center gap-3">
                  <div class="p-2 bg-green-100 text-green-700 rounded-lg">
                    <FileSpreadsheet class="w-6 h-6" />
                  </div>
                  <div>
                    <div class="font-medium text-slate-800">{{ selectedFile.name }}</div>
                    <div class="text-xs text-slate-500">{{ (selectedFile.size / 1024).toFixed(2) }} KB</div>
                  </div>
                </div>
                <button @click="removeFile" class="p-2 text-slate-400 hover:text-red-500 hover:bg-red-50 rounded-lg transition-colors">
                  <X class="w-5 h-5" />
                </button>
              </div>

            </div>
          </div>

          <!-- Step 3: Check Results -->
          <div v-if="isChecking || checkResult">
            <div class="flex items-center gap-3 mb-2">
              <div class="w-6 h-6 rounded-full bg-blue-100 text-blue-700 flex items-center justify-center text-sm font-bold">3</div>
              <h4 class="font-bold text-slate-800">Kiểm tra dữ liệu</h4>
            </div>
            <div class="pl-9">
              
              <!-- Loading -->
              <div v-if="isChecking" class="flex flex-col items-center justify-center py-6 bg-slate-50 rounded-xl border border-slate-200">
                <div class="w-8 h-8 border-4 border-blue-200 border-t-blue-600 rounded-full animate-spin mb-3"></div>
                <div class="text-sm font-medium text-slate-600">Đang phân tích file...</div>
              </div>

              <!-- Result -->
              <div v-if="checkResult" class="space-y-4">
                <div class="grid grid-cols-3 gap-3">
                  <div class="bg-slate-50 p-3 rounded-lg border border-slate-200 text-center">
                    <div class="text-2xl font-bold text-slate-700">{{ checkResult.total }}</div>
                    <div class="text-xs text-slate-500">Tổng số dòng</div>
                  </div>
                  <div class="bg-green-50 p-3 rounded-lg border border-green-200 text-center">
                    <div class="text-2xl font-bold text-green-700">{{ checkResult.valid }}</div>
                    <div class="text-xs text-green-600">Dòng hợp lệ</div>
                  </div>
                  <div class="bg-red-50 p-3 rounded-lg border border-red-200 text-center">
                    <div class="text-2xl font-bold text-red-700">{{ checkResult.invalid }}</div>
                    <div class="text-xs text-red-600">Dòng lỗi</div>
                  </div>
                </div>

                <!-- Errors Table -->
                <div v-if="checkResult.invalid > 0" class="border border-red-200 rounded-lg overflow-hidden">
                  <div class="bg-red-50 px-3 py-2 text-sm font-medium text-red-800 flex items-center gap-2">
                    <AlertCircle class="w-4 h-4" /> Chi tiết lỗi cần sửa
                  </div>
                  <table class="w-full text-sm text-left">
                    <thead class="bg-slate-50 border-y border-red-100 text-slate-600">
                      <tr>
                        <th class="px-3 py-2 font-medium w-16">Dòng</th>
                        <th class="px-3 py-2 font-medium w-32">Cột</th>
                        <th class="px-3 py-2 font-medium">Lỗi</th>
                      </tr>
                    </thead>
                    <tbody class="divide-y divide-slate-100 bg-white">
                      <tr v-for="(err, idx) in checkResult.errors" :key="idx">
                        <td class="px-3 py-2 font-medium text-slate-700">{{ err.row }}</td>
                        <td class="px-3 py-2 text-slate-600">{{ err.col }}</td>
                        <td class="px-3 py-2 text-red-600">{{ err.msg }}</td>
                      </tr>
                    </tbody>
                  </table>
                </div>

              </div>
            </div>
          </div>

        </div>

        <!-- Footer -->
        <div class="px-6 py-4 border-t border-slate-100 bg-slate-50 flex items-center justify-between shrink-0">
          <div class="text-xs text-slate-500 italic">
            File sẽ được kiểm tra và lưu trực tiếp vào cơ sở dữ liệu.
          </div>
          <div class="flex items-center gap-3">
            <button @click="close" class="px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-200 rounded-lg transition-colors">
              Hủy
            </button>
            <button 
              v-if="selectedFile && !checkResult && !isChecking"
              @click="checkFile" 
              class="px-5 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
            >
              Kiểm tra file
            </button>
            <button 
              v-if="checkResult && checkResult.valid > 0"
              @click="importData" 
              class="px-5 py-2 text-sm font-medium text-white bg-green-600 hover:bg-green-700 rounded-lg transition-colors flex items-center gap-2"
            >
              <CheckCircle2 class="w-4 h-4" /> Import {{ checkResult.valid }} câu hợp lệ
            </button>
          </div>
        </div>

      </div>
    </div>

    <!-- Confirm / Alert Modal Popup -->
    <ConfirmModal 
      v-model:is-open="modalState.isOpen"
      :title="modalState.title"
      :message="modalState.message"
      :variant="modalState.variant"
      :confirm-text="modalState.confirmText"
      :cancel-text="modalState.cancelText"
      :is-alert="modalState.isAlert"
    />
  </div>
</template>
