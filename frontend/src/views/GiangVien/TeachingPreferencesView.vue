<template>
  <div class="space-y-6">
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div>
        <h1 class="text-2xl font-bold text-slate-800">Đăng ký nguyện vọng giảng dạy</h1>
        <p class="text-sm text-slate-500 mt-1">Cung cấp ưu tiên các ca dạy của bạn cho học kỳ sắp tới</p>
      </div>
      <div v-if="context" class="flex flex-wrap gap-2">
        <div class="px-3 py-1.5 rounded-lg text-sm font-medium border"
             :class="[
               context.isOpen ? 'bg-emerald-50 text-emerald-700 border-emerald-200' : 'bg-slate-50 text-slate-700 border-slate-200'
             ]">
          Trạng thái cửa sổ: {{ context.isOpen ? 'Đang mở' : 'Đã đóng' }}
        </div>
        <div class="px-3 py-1.5 rounded-lg text-sm font-medium border"
             :class="[
               currentStatus === 'submitted' ? 'bg-blue-50 text-blue-700 border-blue-200' : 
               currentStatus === 'draft' ? 'bg-amber-50 text-amber-700 border-amber-200' : 'bg-slate-50 text-slate-700 border-slate-200'
             ]">
          Trạng thái nộp: {{ statusText }}
        </div>
      </div>
    </div>

    <!-- Notifications -->
    <div v-if="successMessage" class="bg-emerald-50 text-emerald-700 p-4 rounded-lg border border-emerald-100 flex items-start gap-3">
      <CheckCircle2 class="w-5 h-5 shrink-0 mt-0.5" />
      <p>{{ successMessage }}</p>
      <button @click="successMessage = ''" class="ml-auto text-emerald-500 hover:text-emerald-700">
        <XIcon class="w-4 h-4" />
      </button>
    </div>

    <div v-if="errorMessage" class="bg-red-50 text-red-600 p-4 rounded-lg border border-red-100 flex items-start gap-3">
      <AlertCircle class="w-5 h-5 shrink-0 mt-0.5" />
      <p>{{ errorMessage }}</p>
      <button @click="errorMessage = ''" class="ml-auto text-red-500 hover:text-red-700">
        <XIcon class="w-4 h-4" />
      </button>
    </div>

    <div v-if="loading" class="flex justify-center py-12">
      <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
    </div>

    <div v-else-if="!context?.schedulingContext?.schedulableTerm" class="bg-amber-50 border border-amber-200 rounded-lg p-6 text-center">
      <CalendarX class="w-12 h-12 text-amber-400 mx-auto mb-4" />
      <h3 class="text-lg font-semibold text-amber-800 mb-2">Chưa có học kỳ chuẩn bị</h3>
      <p class="text-amber-600">{{ context?.reasonMessage || 'Hiện tại không có học kỳ chuẩn bị nào để đăng ký nguyện vọng.' }}</p>
    </div>

    <div v-else-if="!context.isOpen && context.reasonCode === 'NOT_OPEN_YET'" class="bg-blue-50 border border-blue-200 rounded-lg p-6 text-center">
      <Info class="w-12 h-12 text-blue-400 mx-auto mb-4" />
      <h3 class="text-lg font-semibold text-blue-800 mb-2">Chưa tới thời gian đăng ký</h3>
      <p class="text-blue-600">{{ context.reasonMessage }}</p>
    </div>

    <div v-else class="space-y-6">
      <div v-if="!context.isOpen && context.isPastDeadline" class="bg-blue-50 text-blue-800 p-4 rounded-lg flex items-start gap-3 mb-4">
        <Info class="w-5 h-5 shrink-0 mt-0.5 text-blue-600" />
        <div>
          <p class="font-medium">Thời hạn đăng ký nguyện vọng đã kết thúc</p>
          <p class="text-sm mt-1">Đã đóng lúc {{ formatDate(context.deadline) }}. Bạn chỉ có thể xem lại dữ liệu.</p>
        </div>
      </div>
      
      <div class="bg-white border border-slate-200 rounded-xl p-5 md:p-6 lg-glass-soft">
        <h2 class="text-lg font-semibold text-slate-800 mb-4">Thông tin chung ({{ context.schedulingContext.schedulableTerm.tenHocKy }})</h2>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label class="block text-sm font-medium text-slate-700 mb-1">Số lớp tối đa mong muốn (tùy chọn)</label>
            <input type="number" min="0" max="100" v-model="form.soLopToiDaMongMuon" :disabled="!context.canEdit"
                   class="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 disabled:bg-slate-50 disabled:text-slate-500">
          </div>
          <div>
            <label class="block text-sm font-medium text-slate-700 mb-1">Số ca tối đa mỗi tuần (tùy chọn)</label>
            <input type="number" min="0" max="100" v-model="form.soCaToiDaMoiTuan" :disabled="!context.canEdit"
                   class="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 disabled:bg-slate-50 disabled:text-slate-500">
          </div>
          <div class="md:col-span-2">
            <label class="block text-sm font-medium text-slate-700 mb-1">Ghi chú thêm (tùy chọn)</label>
            <textarea v-model="form.ghiChu" :disabled="!context.canEdit" rows="3"
                      class="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 disabled:bg-slate-50 disabled:text-slate-500"></textarea>
          </div>
        </div>
      </div>

      <div class="bg-white border border-slate-200 rounded-xl p-5 md:p-6 lg-glass-soft">
        <div class="flex flex-col sm:flex-row sm:items-center justify-between mb-4 gap-3">
          <h2 class="text-lg font-semibold text-slate-800">Ma trận ca dạy</h2>
          <div class="flex flex-wrap items-center gap-4 text-sm">
            <div class="flex items-center gap-1.5"><span class="w-3 h-3 rounded-full bg-emerald-500"></span> Ưu tiên</div>
            <div class="flex items-center gap-1.5"><span class="w-3 h-3 rounded-full bg-blue-500"></span> Có thể dạy</div>
            <div class="flex items-center gap-1.5"><span class="w-3 h-3 rounded-full bg-red-500"></span> Bận/Không thể</div>
            <div class="flex items-center gap-1.5"><span class="w-3 h-3 rounded-full border border-slate-300 bg-white"></span> Trung lập</div>
          </div>
        </div>

        <div class="overflow-x-auto">
          <table class="w-full min-w-[700px] border-collapse">
            <thead>
              <tr>
                <th class="border border-slate-200 bg-slate-50 p-3 text-left font-medium text-slate-700 w-32 whitespace-nowrap">Ca \ Thứ</th>
                <th v-for="day in context.supportedDays" :key="day" class="border border-slate-200 bg-slate-50 p-3 text-center font-medium text-slate-700">
                  Thứ {{ day }}
                </th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="shift in context.activeShifts" :key="shift.maCaHoc">
                <td class="border border-slate-200 bg-slate-50 p-2 text-center">
                  <div class="font-medium text-slate-800">{{ shift.tenCa }}</div>
                  <div class="text-xs text-slate-500">{{ shift.gioBatDau }} - {{ shift.gioKetThuc }}</div>
                </td>
                <td v-for="day in context.supportedDays" :key="`${shift.maCaHoc}-${day}`" 
                    class="border border-slate-200 p-2 text-center relative group" 
                    :class="[context.canEdit ? 'cursor-pointer hover:bg-slate-50' : 'cursor-default']"
                    @click="toggleSlot(day, shift.maCaHoc)">
                  <div class="w-full h-10 rounded-md flex items-center justify-center transition-colors duration-200"
                       :class="getSlotClass(day, shift.maCaHoc)">
                    <span v-if="getSlotValue(day, shift.maCaHoc) === 'preferred'" class="text-emerald-700"><CheckCircle2 class="w-5 h-5" /></span>
                    <span v-else-if="getSlotValue(day, shift.maCaHoc) === 'available'" class="text-blue-700"><Check class="w-5 h-5" /></span>
                    <span v-else-if="getSlotValue(day, shift.maCaHoc) === 'unavailable'" class="text-red-700"><XIcon class="w-5 h-5" /></span>
                    <span v-else class="text-slate-300" :class="[context.canEdit ? 'group-hover:text-slate-400' : '']">-</span>
                  </div>
                </td>
              </tr>
              <tr v-if="!context.activeShifts?.length">
                <td :colspan="context.supportedDays?.length + 1 || 7" class="p-8 text-center text-slate-500">
                  Không có ca học nào đang hoạt động.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <div v-if="context.canEdit" class="flex justify-end gap-3 pt-4 border-t border-slate-200">
        <button @click="saveDraft" :disabled="saving" class="px-5 py-2.5 rounded-lg font-medium border border-slate-300 text-slate-700 bg-white hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-indigo-500 disabled:opacity-50">
          <span v-if="saving && saveType === 'draft'" class="flex items-center gap-2"><Loader2 class="w-4 h-4 animate-spin" /> Đang lưu...</span>
          <span v-else>Lưu nháp</span>
        </button>
        <button @click="promptSubmit" :disabled="saving" class="px-5 py-2.5 rounded-lg font-medium bg-indigo-600 text-white hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 disabled:opacity-50">
          <span v-if="saving && saveType === 'submit'" class="flex items-center gap-2"><Loader2 class="w-4 h-4 animate-spin" /> Đang gửi...</span>
          <span v-else>Gửi nguyện vọng</span>
        </button>
      </div>
      <div v-else-if="currentStatus === 'submitted'" class="bg-emerald-50 text-emerald-800 p-4 rounded-lg flex items-start gap-3 mt-4">
        <CheckCircle2 class="w-5 h-5 shrink-0 mt-0.5 text-emerald-600" />
        <p>Bạn đã gửi nguyện vọng thành công. Cảm ơn sự hợp tác của bạn!</p>
      </div>
    </div>
    
    <ConfirmActionDialog
      v-model="isConfirmOpen"
      :title="confirmTitle"
      :message="confirmMessage"
      confirmLabel="Gửi ngay"
      cancelLabel="Hủy bỏ"
      variant="primary"
      :loading="saving"
      @confirm="executeSubmit"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { teacherApi } from '@/services/teacherApi'
import { AlertCircle, CalendarX, CheckCircle2, Check, X as XIcon, Loader2, Info } from 'lucide-vue-next'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'

const loading = ref(true)
const saving = ref(false)
const saveType = ref('')
const error = ref(null)
const successMessage = ref('')
const errorMessage = ref('')

const isConfirmOpen = ref(false)
const confirmTitle = ref('')
const confirmMessage = ref('')

const context = ref(null)
const currentStatus = ref('unstarted')
const form = ref({
  soLopToiDaMongMuon: null,
  soCaToiDaMoiTuan: null,
  ghiChu: '',
  slots: []
})

const statusText = computed(() => {
  if (currentStatus.value === 'submitted') return 'Đã gửi'
  if (currentStatus.value === 'draft') return 'Đang nháp'
  return 'Chưa làm'
})

const formatDate = (dateStr) => {
  if (!dateStr) return ''
  return new Date(dateStr).toLocaleString('vi-VN')
}

const getSlotValue = (thu, maCaHoc) => {
  const slot = form.value.slots.find(x => x.thuTrongTuan === thu && x.maCaHoc === maCaHoc)
  return slot ? slot.mucDo : 'neutral'
}

const getSlotClass = (thu, maCaHoc) => {
  const val = getSlotValue(thu, maCaHoc)
  if (val === 'preferred') return 'bg-emerald-100 border-emerald-300 border'
  if (val === 'available') return 'bg-blue-100 border-blue-300 border'
  if (val === 'unavailable') return 'bg-red-100 border-red-300 border'
  return 'bg-slate-50 border border-slate-200'
}

const toggleSlot = (thu, maCaHoc) => {
  if (!context.value?.canEdit) return
  
  const existingIdx = form.value.slots.findIndex(x => x.thuTrongTuan === thu && x.maCaHoc === maCaHoc)
  const currentVal = existingIdx >= 0 ? form.value.slots[existingIdx].mucDo : 'neutral'
  
  let nextVal = 'neutral'
  if (currentVal === 'neutral') nextVal = 'preferred'
  else if (currentVal === 'preferred') nextVal = 'available'
  else if (currentVal === 'available') nextVal = 'unavailable'
  else if (currentVal === 'unavailable') nextVal = 'neutral'

  if (nextVal === 'neutral') {
    if (existingIdx >= 0) form.value.slots.splice(existingIdx, 1)
  } else {
    if (existingIdx >= 0) {
      form.value.slots[existingIdx].mucDo = nextVal
    } else {
      form.value.slots.push({ thuTrongTuan: thu, maCaHoc: maCaHoc, mucDo: nextVal })
    }
  }
}

const fetchData = async () => {
  try {
    loading.value = true
    error.value = null
    const ctx = await teacherApi.getTeachingPreferenceContext()
    context.value = ctx
    
    if (ctx?.schedulingContext?.schedulableTerm) {
      currentStatus.value = ctx.currentStatus
      const termId = ctx.schedulingContext.schedulableTerm.maHocKy
      const formData = await teacherApi.getTeachingPreferenceForm(termId)
      if (formData) {
        form.value = {
          soLopToiDaMongMuon: formData.soLopToiDaMongMuon,
          soCaToiDaMoiTuan: formData.soCaToiDaMoiTuan,
          ghiChu: formData.ghiChu || '',
          slots: formData.slots || []
        }
      }
    }
  } catch (err) {
    error.value = err.message || 'Lỗi tải dữ liệu nguyện vọng'
  } finally {
    loading.value = false
  }
}

const saveDraft = async () => {
  if (!context.value?.canEdit) return
  try {
    saving.value = true
    saveType.value = 'draft'
    errorMessage.value = ''
    successMessage.value = ''
    
    const termId = context.value.schedulingContext.schedulableTerm.maHocKy
    await teacherApi.saveTeachingPreferenceDraft(termId, form.value)
    
    successMessage.value = 'Đã lưu bản nháp thành công.'
    currentStatus.value = 'draft'
  } catch (err) {
    errorMessage.value = 'Lỗi khi lưu nháp: ' + (err.message || 'Lỗi không xác định')
  } finally {
    saving.value = false
    saveType.value = ''
  }
}

const promptSubmit = () => {
  if (!context.value?.canEdit) return
  
  const hasPreferred = form.value.slots.some(x => x.mucDo === 'preferred')
  if (!hasPreferred) {
    confirmTitle.value = 'Xác nhận gửi nguyện vọng'
    confirmMessage.value = 'Bạn chưa chọn bất kỳ ca nào là ƯU TIÊN. Hệ thống sẽ không có dữ liệu để xếp ca tốt nhất cho bạn. Bạn có chắc chắn muốn gửi?'
  } else {
    confirmTitle.value = 'Xác nhận gửi nguyện vọng'
    confirmMessage.value = 'Sau khi gửi, bạn sẽ không thể chỉnh sửa nguyện vọng nữa. Bạn có chắc chắn muốn gửi?'
  }
  isConfirmOpen.value = true
}

const executeSubmit = async () => {
  try {
    saving.value = true
    saveType.value = 'submit'
    errorMessage.value = ''
    successMessage.value = ''
    
    const termId = context.value.schedulingContext.schedulableTerm.maHocKy
    const res = await teacherApi.submitTeachingPreference(termId, form.value)
    
    successMessage.value = 'Đã gửi nguyện vọng thành công.'
    currentStatus.value = 'submitted'
    context.value.canEdit = false
    context.value.canSubmit = false
    isConfirmOpen.value = false
  } catch (err) {
    errorMessage.value = 'Lỗi khi gửi nguyện vọng: ' + (err.message || 'Lỗi không xác định')
  } finally {
    saving.value = false
    saveType.value = ''
    isConfirmOpen.value = false
  }
}

onMounted(() => {
  fetchData()
})
</script>
