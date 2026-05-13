<script setup>
import { ref } from 'vue'
import { 
  AlertTriangle, 
  CheckCircle, 
  Search, 
  RefreshCw, 
  Calendar, 
  User, 
  Building,
  ArrowRight,
  ShieldAlert,
  Info
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Form State ──────────────────────────────────────────────
const isChecking = ref(false)
const checkResult = ref(null)

const form = ref({
  teacherId: '',
  roomId: '',
  classId: '',
  day: 'Thứ 2',
  startTime: '07:30',
  endTime: '09:30'
})

function performCheck() {
  isChecking.value = true
  // Mock check logic
  setTimeout(() => {
    isChecking.value = false
    checkResult.value = {
      hasConflict: true,
      conflicts: [
        { type: 'teacher', message: 'Giảng viên Nguyễn Văn A đã có lịch dạy lớp SE1601 tại P.302 cùng thời điểm.', severity: 'high' },
        { type: 'room', message: 'Phòng Lab 2 đang được sử dụng bởi lớp IT202.', severity: 'medium' }
      ]
    }
  }, 1000)
}
</script>

<template>
  <PageContainer 
    title="Kiểm tra xung đột" 
    subtitle="Công cụ kiểm tra trùng lịch giảng viên, phòng học và lớp học trước khi xuất bản TKB."
  >
    <div class="max-w-4xl mx-auto space-y-8">
      
      <!-- ── Input Form ── -->
      <div class="lg-glass-strong p-8 rounded-[32px] border border-slate-100 shadow-xl">
        <h3 class="text-lg font-black text-slate-800 mb-6 flex items-center gap-2">
          <Search :size="20" class="text-blue-600" /> THÔNG TIN KIỂM TRA
        </h3>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div class="space-y-1.5">
            <label class="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">Giảng viên</label>
            <div class="relative">
              <User :size="16" class="absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400" />
              <input v-model="form.teacherId" type="text" placeholder="Nhập tên hoặc mã GV..." class="w-full bg-white border border-slate-200 rounded-xl pl-10 pr-4 py-2.5 text-sm font-medium outline-none focus:ring-4 focus:ring-blue-500/10 transition-all">
            </div>
          </div>

          <div class="space-y-1.5">
            <label class="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">Phòng học</label>
            <div class="relative">
              <Building :size="16" class="absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400" />
              <input v-model="form.roomId" type="text" placeholder="Nhập tên phòng..." class="w-full bg-white border border-slate-200 rounded-xl pl-10 pr-4 py-2.5 text-sm font-medium outline-none focus:ring-4 focus:ring-blue-500/10 transition-all">
            </div>
          </div>

          <div class="space-y-1.5">
            <label class="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">Ngày trong tuần</label>
            <select v-model="form.day" class="w-full bg-white border border-slate-200 rounded-xl px-4 py-2.5 text-sm font-medium outline-none focus:ring-4 focus:ring-blue-500/10 transition-all appearance-none">
              <option>Thứ 2</option><option>Thứ 3</option><option>Thứ 4</option>
              <option>Thứ 5</option><option>Thứ 6</option><option>Thứ 7</option>
            </select>
          </div>

          <div class="grid grid-cols-2 gap-4">
             <div class="space-y-1.5">
               <label class="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">Bắt đầu</label>
               <input v-model="form.startTime" type="time" class="w-full bg-white border border-slate-200 rounded-xl px-4 py-2.5 text-sm font-medium outline-none">
             </div>
             <div class="space-y-1.5">
               <label class="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">Kết thúc</label>
               <input v-model="form.endTime" type="time" class="w-full bg-white border border-slate-200 rounded-xl px-4 py-2.5 text-sm font-medium outline-none">
             </div>
          </div>
        </div>

        <div class="mt-8 flex items-center justify-end gap-4 border-t border-slate-50 pt-6">
          <button class="px-6 py-2.5 text-sm font-bold text-slate-500 hover:text-slate-800 transition-colors">Xóa form</button>
          <button 
            @click="performCheck"
            :disabled="isChecking"
            class="lg-button-primary px-8 py-2.5 text-sm font-bold shadow-lg shadow-blue-500/20 disabled:opacity-50"
          >
            <RefreshCw v-if="isChecking" :size="18" class="animate-spin" />
            <span v-else>Kiểm tra xung đột</span>
          </button>
        </div>
      </div>

      <!-- ── Results ── -->
      <Transition
        enter-active-class="transition-all duration-300 ease-out"
        enter-from-class="opacity-0 translate-y-4"
        enter-to-class="opacity-100 translate-y-0"
      >
        <div v-if="checkResult" class="space-y-4">
          <div :class="['p-6 rounded-[28px] border flex items-start gap-4 shadow-sm', checkResult.hasConflict ? 'bg-rose-50 border-rose-100' : 'bg-emerald-50 border-emerald-100']">
            <div :class="['h-12 w-12 rounded-2xl flex items-center justify-center shrink-0', checkResult.hasConflict ? 'bg-rose-500 text-white shadow-lg shadow-rose-200' : 'bg-emerald-500 text-white shadow-lg shadow-emerald-200']">
              <ShieldAlert v-if="checkResult.hasConflict" :size="24" />
              <CheckCircle v-else :size="24" />
            </div>
            <div class="flex-1">
              <h4 :class="['text-lg font-black', checkResult.hasConflict ? 'text-rose-900' : 'text-emerald-900']">
                {{ checkResult.hasConflict ? 'Phát hiện xung đột!' : 'Hợp lệ' }}
              </h4>
              <p :class="['text-sm font-medium mt-1', checkResult.hasConflict ? 'text-rose-600' : 'text-emerald-600']">
                {{ checkResult.hasConflict ? `Có ${checkResult.conflicts.length} xung đột được phát hiện cho các tiêu chí này.` : 'Không phát hiện bất kỳ xung đột nào. Bạn có thể xếp lịch này.' }}
              </p>
            </div>
          </div>

          <!-- Conflict List -->
          <div v-if="checkResult.hasConflict" class="grid grid-cols-1 gap-3">
             <div v-for="(conflict, idx) in checkResult.conflicts" :key="idx" class="lg-glass-strong p-4 rounded-2xl border border-slate-100 flex items-center gap-4 group hover:border-rose-200 transition-colors">
                <div class="h-10 w-10 rounded-xl bg-slate-50 flex items-center justify-center text-rose-500 border border-slate-100">
                  <AlertTriangle :size="20" />
                </div>
                <p class="flex-1 text-sm font-bold text-slate-700">{{ conflict.message }}</p>
                <button class="p-2 hover:bg-rose-50 hover:text-rose-600 rounded-lg text-slate-400 transition-all">
                   <ArrowRight :size="18" />
                </button>
             </div>
          </div>
        </div>
      </Transition>

      <!-- ── Information Info ── -->
      <div class="lg-glass-strong p-6 rounded-[24px] bg-blue-50/50 border-blue-100">
        <div class="flex gap-3">
          <Info :size="20" class="text-blue-600 shrink-0 mt-0.5" />
          <div>
            <h5 class="text-sm font-black text-blue-900">Mẹo kiểm tra nhanh</h5>
            <p class="text-xs text-blue-700 mt-1 leading-relaxed">
              Bạn có thể kiểm tra một tiêu chí duy nhất (ví dụ: chỉ phòng) bằng cách để trống các trường khác. Hệ thống sẽ tự động quét toàn bộ cơ sở dữ liệu đã công bố và các bản nháp đang chờ duyệt.
            </p>
          </div>
        </div>
      </div>

    </div>
  </PageContainer>
</template>
