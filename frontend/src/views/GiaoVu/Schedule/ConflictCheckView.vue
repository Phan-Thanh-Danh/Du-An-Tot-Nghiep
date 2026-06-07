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
      <div class="lg-glass-soft p-5 rounded-2xl border-default shadow-sm">
        <h3 class="text-lg font-black text-heading mb-4 flex items-center gap-2">
          <Search :size="20" class="text-[var(--lg-primary)]" /> THÔNG TIN KIỂM TRA
        </h3>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div class="space-y-1.5">
            <label class="text-[10px] font-black text-placeholder uppercase tracking-widest ml-1">Giảng viên</label>
            <div class="relative">
              <User :size="16" class="absolute left-3.5 top-1/2 -translate-y-1/2 text-placeholder" />
              <input v-model="form.teacherId" type="text" placeholder="Nhập tên hoặc mã GV..." class="w-full lg-input pl-10 pr-4 py-2.5 text-sm font-medium">
            </div>
          </div>

          <div class="space-y-1.5">
            <label class="text-[10px] font-black text-placeholder uppercase tracking-widest ml-1">Phòng học</label>
            <div class="relative">
              <Building :size="16" class="absolute left-3.5 top-1/2 -translate-y-1/2 text-placeholder" />
              <input v-model="form.roomId" type="text" placeholder="Nhập tên phòng..." class="w-full lg-input pl-10 pr-4 py-2.5 text-sm font-medium">
            </div>
          </div>

          <div class="space-y-1.5">
            <label class="text-[10px] font-black text-placeholder uppercase tracking-widest ml-1">Ngày trong tuần</label>
            <select v-model="form.day" class="w-full lg-input px-4 py-2.5 text-sm font-medium appearance-none">
              <option>Thứ 2</option><option>Thứ 3</option><option>Thứ 4</option>
              <option>Thứ 5</option><option>Thứ 6</option><option>Thứ 7</option>
            </select>
          </div>

          <div class="grid grid-cols-2 gap-4">
             <div class="space-y-1.5">
               <label class="text-[10px] font-black text-placeholder uppercase tracking-widest ml-1">Bắt đầu</label>
               <input v-model="form.startTime" type="time" class="w-full lg-input px-4 py-2.5 text-sm font-medium">
             </div>
             <div class="space-y-1.5">
               <label class="text-[10px] font-black text-placeholder uppercase tracking-widest ml-1">Kết thúc</label>
               <input v-model="form.endTime" type="time" class="w-full lg-input px-4 py-2.5 text-sm font-medium">
             </div>
          </div>
        </div>

        <div class="mt-8 flex items-center justify-end gap-4 border-t border-default pt-6">
          <button class="px-4 py-2.5 text-sm font-bold text-label hover:text-heading transition-colors">Xóa form</button>
          <button 
            @click="performCheck"
            :disabled="isChecking"
            class="lg-button-primary px-5 py-2.5 text-sm font-bold shadow-lg shadow-[var(--lg-primary)]/20 disabled:opacity-50"
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
          <div :class="['p-4 rounded-2xl border flex items-start gap-4 shadow-sm', checkResult.hasConflict ? 'bg-[var(--color-danger-bg)] border-[var(--color-danger-bg)]' : 'bg-[var(--color-success-bg)] border-[var(--color-success-bg)]']">
            <div :class="['h-10 w-10 rounded-2xl flex items-center justify-center shrink-0', checkResult.hasConflict ? 'bg-[var(--lg-danger)] text-white shadow-lg shadow-[var(--lg-danger)]/20' : 'bg-[var(--lg-success)] text-white shadow-lg shadow-[var(--lg-success)]/20']">
              <ShieldAlert v-if="checkResult.hasConflict" :size="24" />
              <CheckCircle v-else :size="24" />
            </div>
            <div class="flex-1">
              <h4 :class="['text-lg font-black', checkResult.hasConflict ? 'text-[var(--lg-danger)]' : 'text-[var(--lg-success)]']">
                {{ checkResult.hasConflict ? 'Phát hiện xung đột!' : 'Hợp lệ' }}
              </h4>
              <p :class="['text-sm font-medium mt-1', checkResult.hasConflict ? 'text-[var(--color-danger-text)]' : 'text-[var(--color-success-text)]']">
                {{ checkResult.hasConflict ? `Có ${checkResult.conflicts.length} xung đột được phát hiện cho các tiêu chí này.` : 'Không phát hiện bất kỳ xung đột nào. Bạn có thể xếp lịch này.' }}
              </p>
            </div>
          </div>

          <!-- Conflict List -->
          <div v-if="checkResult.hasConflict" class="grid grid-cols-1 gap-3">
             <div v-for="(conflict, idx) in checkResult.conflicts" :key="idx" class="lg-glass-strong p-4 rounded-2xl border-default flex items-center gap-4 group hover:border-[var(--lg-danger)]/20 transition-colors">
                <div class="h-10 w-10 rounded-xl bg-[var(--surface-input)] flex items-center justify-center text-[var(--lg-danger)] border-default">
                  <AlertTriangle :size="20" />
                </div>
                <p class="flex-1 text-sm font-bold text-heading">{{ conflict.message }}</p>
                <button class="p-2 hover:bg-[var(--color-danger-bg)] hover:text-[var(--color-danger-text)] rounded-lg text-placeholder transition-all">
                   <ArrowRight :size="18" />
                </button>
             </div>
          </div>
        </div>
      </Transition>

      <!-- ── Information Info ── -->
      <div class="lg-glass-strong p-4 rounded-[24px] bg-[var(--color-info-bg)]/50 border-[var(--color-info-bg)]">
        <div class="flex gap-3">
          <Info :size="20" class="text-[var(--color-info-text)] shrink-0 mt-0.5" />
          <div>
            <h5 class="text-sm font-black text-[var(--color-info-text)]">Mẹo kiểm tra nhanh</h5>
            <p class="text-xs text-[var(--color-info-text)] mt-1 leading-relaxed">
              Bạn có thể kiểm tra một tiêu chí duy nhất (ví dụ: chỉ phòng) bằng cách để trống các trường khác. Hệ thống sẽ tự động quét toàn bộ cơ sở dữ liệu đã công bố và các bản nháp đang chờ duyệt.
            </p>
          </div>
        </div>
      </div>

    </div>
  </PageContainer>
</template>
