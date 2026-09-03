<script setup lang="ts">
import { ref, computed } from 'vue'
import { X, UploadCloud, Download, FileSpreadsheet, CheckCircle2, AlertCircle, ChevronLeft, ChevronRight } from 'lucide-vue-next'
import { contentCouncilApi } from '@/services/contentCouncilApi'
import ConfirmModal from '@/components/common/ConfirmModal.vue'

const props = defineProps<{
  isOpen: boolean
}>()

const emit = defineEmits<{
  (e: 'update:isOpen', val: boolean): void
  (e: 'import', count: number): void
}>()

const fileInput = ref<HTMLInputElement | null>(null)
const selectedFile = ref<File | null>(null)
const isChecking = ref(false)
const checkResult = ref<{
  valid: number
  invalid: number
  total: number
  errors: Array<{row: number, col: string, msg: string}>
} | null>(null)

interface ParsedImportRow {
  rowNum: number
  maMonHoc: string
  loai: string
  kieuLuaChon: string
  doKho: string
  noiDung: string
  dapAnDung: string
  isValid: boolean
  errorMsg?: string
}

const parsedRows = ref<ParsedImportRow[]>([])
const previewPage = ref(1)
const previewPageSize = ref(10)
const previewTab = ref<'all' | 'valid' | 'invalid'>('all')

const filteredPreviewRows = computed(() => {
  if (previewTab.value === 'valid') return parsedRows.value.filter(r => r.isValid)
  if (previewTab.value === 'invalid') return parsedRows.value.filter(r => !r.isValid)
  return parsedRows.value
})

const totalPreviewPages = computed(() => Math.max(1, Math.ceil(filteredPreviewRows.value.length / previewPageSize.value)))

const paginatedPreviewRows = computed(() => {
  if (previewPageSize.value >= 1000) return filteredPreviewRows.value
  const start = (previewPage.value - 1) * previewPageSize.value
  return filteredPreviewRows.value.slice(start, start + previewPageSize.value)
})

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
  parsedRows.value = []
  previewPage.value = 1
  previewTab.value = 'all'
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
    parsedRows.value = []
    previewPage.value = 1
  }
}

const triggerFileSelect = () => {
  fileInput.value?.click()
}

const removeFile = () => {
  selectedFile.value = null
  checkResult.value = null
  parsedRows.value = []
  previewPage.value = 1
  previewTab.value = 'all'
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
      parsedRows.value = []
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
    const rowsList: ParsedImportRow[] = []
    let validCount = 0

    jsonData.forEach((row: any, idx: number) => {
      const rowNum = idx + 2 // Row 1 is header
      const maMonHoc = String(row['MaCodeMonHoc'] || row['Mã môn'] || row['MaMonHoc'] || '').trim()
      const loai = String(row['LoaiCauHoi'] || row['Loại câu hỏi'] || '').trim()
      const noiDung = String(row['NoiDung'] || row['Nội dung'] || '').trim()
      const doKho = String(row['DoKho'] || row['Độ khó'] || 'trung_binh').trim()
      const kieuLuaChon = String(row['KieuLuaChon'] || row['Kiểu lựa chọn'] || '').trim()
      const dapAnDung = String(row['DapAnDung'] || row['Đáp án đúng'] || '').trim()

      let rowError = ''
      if (!maMonHoc) {
        rowError = 'Thiếu mã môn học'
        errors.push({ row: rowNum, col: 'MaCodeMonHoc', msg: rowError })
      } else if (!noiDung) {
        rowError = 'Thiếu nội dung câu hỏi'
        errors.push({ row: rowNum, col: 'NoiDung', msg: rowError })
      } else if (loai === 'trac_nghiem') {
        const choiceA = String(row['LuaChonA'] || row['Lựa chọn A'] || row['Đáp án A'] || '').trim()
        const choiceB = String(row['LuaChonB'] || row['Lựa chọn B'] || row['Đáp án B'] || '').trim()
        const legacyChoices = String(row['LuaChon'] || row['Lựa chọn'] || '').trim()

        const hasOptionCols = choiceA || choiceB
        const hasLegacyChoice = legacyChoices

        if (!hasOptionCols && !hasLegacyChoice) {
          rowError = 'Câu trắc nghiệm phải có ít nhất các lựa chọn A và B'
          errors.push({ row: rowNum, col: 'LuaChonA', msg: rowError })
        } else if (!dapAnDung) {
          rowError = 'Câu trắc nghiệm phải có đáp án đúng'
          errors.push({ row: rowNum, col: 'DapAnDung', msg: rowError })
        } else {
          validCount++
        }
      } else {
        validCount++
      }

      rowsList.push({
        rowNum,
        maMonHoc,
        loai: loai || 'trac_nghiem',
        kieuLuaChon,
        doKho,
        noiDung,
        dapAnDung,
        isValid: !rowError,
        errorMsg: rowError
      })
    })

    parsedRows.value = rowsList
    previewPage.value = 1
    previewTab.value = 'all'

    checkResult.value = {
      total: jsonData.length,
      valid: validCount,
      invalid: jsonData.length - validCount,
      errors
    }
  } catch (err: any) {
    showAlert('Không thể đọc dữ liệu file Excel. Vui lòng kiểm tra định dạng file.', 'Lỗi đọc file', 'danger')
    parsedRows.value = []
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
      <div class="bg-white rounded-xl shadow-2xl w-full max-w-4xl overflow-hidden flex flex-col max-h-[90vh]">
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
                    <AlertCircle class="w-4 h-4" /> Chi tiết lỗi cần sửa ({{ checkResult.invalid }} dòng)
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

                <!-- Questions Preview Section -->
                <div v-if="parsedRows.length > 0" class="space-y-3 pt-2">
                  <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-2 border-b border-slate-200 pb-3">
                    <div class="flex items-center gap-2">
                      <span class="text-xs font-bold text-slate-700 uppercase tracking-wider">Xem trước:</span>
                      <button
                        type="button"
                        @click="previewTab = 'all'; previewPage = 1"
                        class="px-2.5 py-1 text-xs font-semibold rounded-lg transition-colors"
                        :class="previewTab === 'all' ? 'bg-blue-600 text-white' : 'bg-slate-100 text-slate-600 hover:bg-slate-200'"
                      >
                        Tất cả ({{ checkResult.total }})
                      </button>
                      <button
                        type="button"
                        @click="previewTab = 'valid'; previewPage = 1"
                        class="px-2.5 py-1 text-xs font-semibold rounded-lg transition-colors"
                        :class="previewTab === 'valid' ? 'bg-green-600 text-white' : 'bg-green-50 text-green-700 hover:bg-green-100'"
                      >
                        Hợp lệ ({{ checkResult.valid }})
                      </button>
                      <button
                        v-if="checkResult.invalid > 0"
                        type="button"
                        @click="previewTab = 'invalid'; previewPage = 1"
                        class="px-2.5 py-1 text-xs font-semibold rounded-lg transition-colors"
                        :class="previewTab === 'invalid' ? 'bg-red-600 text-white' : 'bg-red-50 text-red-700 hover:bg-red-100'"
                      >
                        Có lỗi ({{ checkResult.invalid }})
                      </button>
                    </div>

                    <div class="flex items-center gap-2 text-xs text-slate-500">
                      <span>Hiển thị:</span>
                      <select
                        v-model.number="previewPageSize"
                        @change="previewPage = 1"
                        class="border border-slate-200 rounded px-2 py-1 text-xs bg-white text-slate-700 focus:outline-none focus:ring-1 focus:ring-blue-500"
                      >
                        <option :value="10">10 / trang</option>
                        <option :value="20">20 / trang</option>
                        <option :value="50">50 / trang</option>
                        <option :value="100">100 / trang</option>
                        <option :value="1000">Tất cả</option>
                      </select>
                    </div>
                  </div>

                  <!-- Preview Table -->
                  <div class="border border-slate-200 rounded-lg overflow-hidden bg-white shadow-sm">
                    <div class="overflow-x-auto max-h-80">
                      <table class="w-full text-xs text-left">
                        <thead class="bg-slate-50 text-slate-700 font-semibold sticky top-0 z-10 border-b border-slate-200">
                          <tr>
                            <th class="px-3 py-2.5 w-14 text-center">Dòng</th>
                            <th class="px-3 py-2.5 w-28">Mã môn</th>
                            <th class="px-3 py-2.5 w-24">Loại</th>
                            <th class="px-3 py-2.5 min-w-[240px]">Nội dung câu hỏi</th>
                            <th class="px-3 py-2.5 w-24 text-center">Độ khó</th>
                            <th class="px-3 py-2.5 w-28 text-center">Trạng thái</th>
                          </tr>
                        </thead>
                        <tbody class="divide-y divide-slate-100">
                          <tr v-for="row in paginatedPreviewRows" :key="row.rowNum" :class="{'bg-red-50/40': !row.isValid}">
                            <td class="px-3 py-2.5 text-center font-medium text-slate-500">{{ row.rowNum }}</td>
                            <td class="px-3 py-2.5 font-mono font-semibold text-slate-800">{{ row.maMonHoc }}</td>
                            <td class="px-3 py-2.5 text-slate-600">
                              <span class="px-2 py-0.5 rounded text-[11px] font-medium" :class="row.loai === 'tu_luan' ? 'bg-amber-50 text-amber-700 border border-amber-200' : 'bg-blue-50 text-blue-700 border border-blue-200'">
                                {{ row.loai === 'tu_luan' ? 'Tự luận' : 'Trắc nghiệm' }}
                              </span>
                            </td>
                            <td class="px-3 py-2.5 text-slate-800">
                              <div class="line-clamp-2 max-w-sm" :title="row.noiDung">{{ row.noiDung }}</div>
                              <div v-if="!row.isValid && row.errorMsg" class="text-red-600 text-[11px] mt-0.5 font-medium">
                                {{ row.errorMsg }}
                              </div>
                            </td>
                            <td class="px-3 py-2.5 text-center text-slate-600 capitalize">
                              <span class="px-2 py-0.5 rounded text-[11px] font-medium" :class="{
                                'bg-green-50 text-green-700': row.doKho === 'de',
                                'bg-yellow-50 text-yellow-700': row.doKho === 'trung_binh',
                                'bg-red-50 text-red-700': row.doKho === 'kho'
                              }">
                                {{ row.doKho === 'de' ? 'Dễ' : row.doKho === 'kho' ? 'Khó' : 'TB' }}
                              </span>
                            </td>
                            <td class="px-3 py-2.5 text-center">
                              <span v-if="row.isValid" class="inline-flex items-center px-2 py-0.5 rounded-full text-[11px] font-medium bg-green-100 text-green-700">
                                Hợp lệ
                              </span>
                              <span v-else class="inline-flex items-center px-2 py-0.5 rounded-full text-[11px] font-medium bg-red-100 text-red-700" :title="row.errorMsg">
                                Lỗi
                              </span>
                            </td>
                          </tr>
                        </tbody>
                      </table>
                    </div>

                    <!-- Preview Pagination Controls -->
                    <div class="px-4 py-2.5 bg-slate-50 border-t border-slate-200 flex flex-col sm:flex-row items-center justify-between gap-2 text-xs text-slate-600">
                      <div>
                        Hiển thị <span class="font-semibold text-slate-800">{{ filteredPreviewRows.length > 0 ? (previewPage - 1) * previewPageSize + 1 : 0 }}</span>–<span class="font-semibold text-slate-800">{{ Math.min(previewPage * previewPageSize, filteredPreviewRows.length) }}</span> / <span class="font-semibold text-slate-800">{{ filteredPreviewRows.length }}</span> câu hỏi
                      </div>
                      <div class="flex items-center gap-1.5">
                        <button
                          type="button"
                          @click="previewPage--"
                          :disabled="previewPage <= 1"
                          class="p-1 rounded border border-slate-200 text-slate-600 hover:bg-white disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
                          title="Trang trước"
                        >
                          <ChevronLeft class="w-4 h-4" />
                        </button>
                        <span class="px-2 font-medium">Trang {{ previewPage }} / {{ totalPreviewPages }}</span>
                        <button
                          type="button"
                          @click="previewPage++"
                          :disabled="previewPage >= totalPreviewPages"
                          class="p-1 rounded border border-slate-200 text-slate-600 hover:bg-white disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
                          title="Trang sau"
                        >
                          <ChevronRight class="w-4 h-4" />
                        </button>
                      </div>
                    </div>
                  </div>
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
