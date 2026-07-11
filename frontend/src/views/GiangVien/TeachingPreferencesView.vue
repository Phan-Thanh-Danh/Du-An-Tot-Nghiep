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
          Trạng thái: {{ context.isOpen ? 'Đang mở' : 'Đã đóng' }}
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

    <div v-if="loading" class="flex justify-center py-12">
      <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
    </div>

    <div v-else-if="error" class="bg-red-50 text-red-600 p-4 rounded-lg border border-red-100 flex items-start gap-3">
      <AlertCircle class="w-5 h-5 shrink-0 mt-0.5" />
      <p>{{ error }}</p>
    </div>

    <div v-else-if="!context?.schedulableTerm" class="bg-amber-50 border border-amber-200 rounded-lg p-6 text-center">
      <CalendarX class="w-12 h-12 text-amber-400 mx-auto mb-4" />
      <h3 class="text-lg font-semibold text-amber-800 mb-2">Không có học kỳ nào đang mở</h3>
      <p class="text-amber-600">Hiện tại không có đợt thu thập nguyện vọng nào được mở. Vui lòng quay lại sau.</p>
    </div>

    <div v-else class="space-y-6">
      <div class="bg-white border border-slate-200 rounded-xl p-5 md:p-6 lg-glass-soft">
        <h2 class="text-lg font-semibold text-slate-800 mb-4">Thông tin chung</h2>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label class="block text-sm font-medium text-slate-700 mb-1">Số lớp tối đa mong muốn (tùy chọn)</label>
            <input type="number" min="0" max="100" v-model="form.soLopToiDaMongMuon" :disabled="isReadonly"
                   class="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 disabled:bg-slate-50 disabled:text-slate-500">
          </div>
          <div>
            <label class="block text-sm font-medium text-slate-700 mb-1">Số ca tối đa mỗi tuần (tùy chọn)</label>
            <input type="number" min="0" max="100" v-model="form.soCaToiDaMoiTuan" :disabled="isReadonly"
                   class="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 disabled:bg-slate-50 disabled:text-slate-500">
          </div>
          <div class="md:col-span-2">
            <label class="block text-sm font-medium text-slate-700 mb-1">Ghi chú thêm (tùy chọn)</label>
            <textarea v-model="form.ghiChu" :disabled="isReadonly" rows="3"
                      class="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 disabled:bg-slate-50 disabled:text-slate-500"></textarea>
          </div>
        </div>
      </div>

      <div class="bg-white border border-slate-200 rounded-xl p-5 md:p-6 lg-glass-soft">
        <div class="flex items-center justify-between mb-4">
          <h2 class="text-lg font-semibold text-slate-800">Ma trận ca dạy</h2>
          <div class="flex items-center gap-4 text-sm">
            <div class="flex items-center gap-1.5"><span class="w-3 h-3 rounded-full bg-emerald-500"></span> Ưu tiên</div>
            <div class="flex items-center gap-1.5"><span class="w-3 h-3 rounded-full bg-blue-500"></span> Có thể dạy</div>
            <div class="flex items-center gap-1.5"><span class="w-3 h-3 rounded-full bg-red-500"></span> Bận/Không thể</div>
            <div class="flex items-center gap-1.5"><span class="w-3 h-3 rounded-full bg-slate-300"></span> Trung lập</div>
          </div>
        </div>

        <div class="overflow-x-auto">
          <table class="w-full min-w-[600px] border-collapse">
            <thead>
              <tr>
                <th class="border border-slate-200 bg-slate-50 p-3 text-left font-medium text-slate-700 w-24">Ca \ Thứ</th>
                <th v-for="day in 6" :key="day" class="border border-slate-200 bg-slate-50 p-3 text-center font-medium text-slate-700">
                  Thứ {{ day + 1 }}
                </th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="ca in 12" :key="ca">
                <td class="border border-slate-200 bg-slate-50 p-3 text-center font-medium text-slate-700">Ca {{ ca }}</td>
                <td v-for="day in 6" :key="`${ca}-${day}`" class="border border-slate-200 p-2 text-center relative group cursor-pointer hover:bg-slate-50" @click="toggleSlot(day + 1, ca)">
                  <div class="w-full h-10 rounded-md flex items-center justify-center transition-colors duration-200"
                       :class="getSlotClass(day + 1, ca)">
                    <span v-if="getSlotValue(day + 1, ca) === 'preferred'" class="text-emerald-700"><CheckCircle2 class="w-5 h-5" /></span>
                    <span v-else-if="getSlotValue(day + 1, ca) === 'available'" class="text-blue-700"><Check class="w-5 h-5" /></span>
                    <span v-else-if="getSlotValue(day + 1, ca) === 'unavailable'" class="text-red-700"><XIcon class="w-5 h-5" /></span>
                    <span v-else class="text-slate-400 group-hover:text-slate-600">-</span>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <div v-if="!isReadonly" class="flex justify-end gap-3 pt-4 border-t border-slate-200">
        <button @click="saveDraft" :disabled="saving" class="px-5 py-2.5 rounded-lg font-medium border border-slate-300 text-slate-700 bg-white hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-indigo-500 disabled:opacity-50">
          <span v-if="saving" class="flex items-center gap-2"><Loader2 class="w-4 h-4 animate-spin" /> Đang lưu...</span>
          <span v-else>Lưu nháp</span>
        </button>
        <button @click="confirmSubmit" :disabled="saving" class="px-5 py-2.5 rounded-lg font-medium bg-indigo-600 text-white hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 disabled:opacity-50">
          Gửi nguyện vọng
        </button>
      </div>
      <div v-else class="bg-blue-50 text-blue-800 p-4 rounded-lg flex items-start gap-3">
        <Info class="w-5 h-5 shrink-0 mt-0.5 text-blue-600" />
        <p>Biểu mẫu này chỉ xem được do bạn đã gửi nguyện vọng hoặc thời hạn đăng ký đã kết thúc.</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, nextTick } from 'vue'
import { teacherApi } from '@/services/teacherApi'
import { AlertCircle, CalendarX, CheckCircle2, Check, X as XIcon, Loader2, Info } from 'lucide-vue-next'

const loading = ref(true)
const saving = ref(false)
const error = ref(null)

const context = ref(null)
const currentStatus = ref('unstarted')
const form = ref({
  soLopToiDaMongMuon: null,
  soCaToiDaMoiTuan: null,
  ghiChu: '',
  slots: []
})

const isReadonly = computed(() => {
  if (!context.value) return true
  return context.value.isPastDeadline || currentStatus.value === 'submitted'
})

const statusText = computed(() => {
  if (currentStatus.value === 'submitted') return 'Đã gửi'
  if (currentStatus.value === 'draft') return 'Đang nháp'
  return 'Chưa làm'
})

const getSlotValue = (thu, ca) => {
  const slot = form.value.slots.find(x => x.thuTrongTuan === thu && x.maCaHoc === ca)
  return slot ? slot.mucDo : 'neutral'
}

const getSlotClass = (thu, ca) => {
  const val = getSlotValue(thu, ca)
  if (val === 'preferred') return 'bg-emerald-100 border-emerald-300'
  if (val === 'available') return 'bg-blue-100 border-blue-300'
  if (val === 'unavailable') return 'bg-red-100 border-red-300'
  return 'bg-transparent'
}

const toggleSlot = (thu, ca) => {
  if (isReadonly.value) return
  
  const existingIdx = form.value.slots.findIndex(x => x.thuTrongTuan === thu && x.maCaHoc === ca)
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
      form.value.slots.push({ thuTrongTuan: thu, maCaHoc: ca, mucDo: nextVal })
    }
  }
  
}

const fetchData = async () => {
  try {
    loading.value = true
    error.value = null
    const ctx = await teacherApi.getTeachingPreferenceContext()
    context.value = ctx
    
    if (ctx.schedulableTerm) {
      currentStatus.value = ctx.currentStatus
      const formData = await teacherApi.getTeachingPreferenceForm(ctx.schedulableTerm.maHocKy)
      if (formData) {
        form.value = {
          soLopToiDaMongMuon: formData.soLopToiDaMongMuon,
          soCaToiDaMoiTuan: formData.soCaToiDaMoiTuan,
          ghiChu: formData.ghiChu,
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
  if (isReadonly.value) return
  try {
    saving.value = true
    const termId = context.value.schedulableTerm.maHocKy
    const res = await teacherApi.saveTeachingPreferenceDraft(termId, form.value)
    alert('Đã lưu nháp thành công')
    currentStatus.value = 'draft'
  } catch (err) {
    alert('Lỗi khi lưu nháp: ' + (err.message || ''))
  } finally {
    saving.value = false
  }
}

const confirmSubmit = async () => {
  if (isReadonly.value) return
  
  // Warning for zero slots
  const hasPreferred = form.value.slots.some(x => x.mucDo === 'preferred')
  if (!hasPreferred) {
    if (!confirm('Bạn chưa chọn bất kỳ ca nào là ƯU TIÊN. Hệ thống sẽ không có dữ liệu để xếp ca tốt nhất cho bạn. Bạn vẫn muốn tiếp tục gửi?')) return
  } else {
    if (!confirm('Sau khi gửi, bạn sẽ không thể chỉnh sửa nguyện vọng nữa. Bạn có chắc chắn muốn gửi?')) return
  }

  try {
    saving.value = true
    const termId = context.value.schedulableTerm.maHocKy
    await teacherApi.submitTeachingPreference(termId, form.value)
    alert('Đã gửi nguyện vọng thành công')
    currentStatus.value = 'submitted'
  } catch (err) {
    alert('Lỗi khi nộp: ' + (err.message || ''))
  } finally {
    saving.value = false
  }
}

onMounted(() => {
  fetchData()
})
</script>
