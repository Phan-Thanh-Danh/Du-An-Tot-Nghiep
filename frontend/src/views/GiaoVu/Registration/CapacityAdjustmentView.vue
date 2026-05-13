<script setup>
import { ref } from 'vue'
import { 
  Users, 
  ArrowRight, 
  TrendingUp, 
  TrendingDown, 
  AlertTriangle, 
  CheckCircle,
  Building,
  Info,
  Layers
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── State ────────────────────────────────────────────────────
const selectedSection = ref({
  id: 'LHP001',
  subject: 'Lập trình Java',
  capacity: 45,
  enrolled: 45,
  waitlist: 12,
  roomCapacity: 60
})

const newCapacity = ref(selectedSection.value.capacity)
const reason = ref('')
const isProcessing = ref(false)

function handleAdjust() {
  isProcessing.value = true
  setTimeout(() => {
    isProcessing.value = false
    alert('Đã cập nhật sức chứa thành công!')
  }, 1000)
}
</script>

<template>
  <PageContainer 
    title="Điều chỉnh sức chứa" 
    subtitle="Tăng hoặc giảm số lượng chỗ trống của lớp học phần."
  >
    <div class="max-w-4xl mx-auto space-y-8">
      
      <!-- ── Current Status ── -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div class="lg-card-glass p-6 flex flex-col items-center text-center">
          <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">Sức chứa hiện tại</p>
          <div class="h-16 w-16 rounded-full bg-blue-50 text-blue-600 flex items-center justify-center border-4 border-white shadow-sm mb-3">
             <span class="text-xl font-black">{{ selectedSection.capacity }}</span>
          </div>
          <p class="text-xs font-bold text-slate-500">Maximum capacity</p>
        </div>
        <div class="lg-card-glass p-6 flex flex-col items-center text-center">
          <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">Đã đăng ký</p>
          <div class="h-16 w-16 rounded-full bg-emerald-50 text-emerald-600 flex items-center justify-center border-4 border-white shadow-sm mb-3">
             <span class="text-xl font-black">{{ selectedSection.enrolled }}</span>
          </div>
          <p class="text-xs font-bold text-slate-500">Currently enrolled</p>
        </div>
        <div class="lg-card-glass p-6 flex flex-col items-center text-center border-amber-100 bg-amber-50/20">
          <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">Đang đợi (Waitlist)</p>
          <div class="h-16 w-16 rounded-full bg-amber-100 text-amber-600 flex items-center justify-center border-4 border-white shadow-sm mb-3">
             <span class="text-xl font-black">{{ selectedSection.waitlist }}</span>
          </div>
          <p class="text-xs font-bold text-amber-600">Students in queue</p>
        </div>
      </div>

      <!-- ── Adjustment Form ── -->
      <div class="lg-card-glass p-8">
        <h3 class="text-lg font-black text-slate-800 mb-6 flex items-center gap-2">
          <Layers :size="20" class="text-blue-600" /> CẤU HÌNH SỨC CHỨA MỚI
        </h3>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
          <div class="space-y-6">
            <div class="space-y-2">
              <label class="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">Sức chứa mới</label>
              <div class="flex items-center gap-4">
                <input 
                  v-model="newCapacity" 
                  type="number" 
                  class="flex-1 bg-white border border-slate-200 rounded-xl px-4 py-3 text-lg font-black text-slate-800 outline-none focus:ring-4 focus:ring-blue-500/10 transition-all"
                >
                <div class="flex flex-col gap-1">
                  <button @click="newCapacity++" class="p-1 hover:bg-slate-100 rounded-lg text-slate-400"><TrendingUp :size="16" /></button>
                  <button @click="newCapacity--" class="p-1 hover:bg-slate-100 rounded-lg text-slate-400"><TrendingDown :size="16" /></button>
                </div>
              </div>
            </div>

            <div class="space-y-2">
              <label class="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">Lý do điều chỉnh</label>
              <textarea 
                v-model="reason" 
                placeholder="Nhập lý do (Ví dụ: Theo nhu cầu SV, Đổi phòng lớn hơn...)"
                class="w-full bg-white border border-slate-200 rounded-xl px-4 py-3 text-sm font-medium outline-none h-24 resize-none"
              ></textarea>
            </div>
          </div>

          <div class="bg-slate-50/50 rounded-[24px] p-6 border border-slate-100 space-y-6">
             <h4 class="text-xs font-black text-slate-500 uppercase tracking-widest">Xem trước ảnh hưởng</h4>
             
             <div class="space-y-4">
                <div class="flex items-center justify-between p-3 bg-white rounded-xl border border-slate-100">
                   <div class="flex items-center gap-3">
                      <Building :size="18" class="text-slate-400" />
                      <span class="text-xs font-bold text-slate-600">Giới hạn phòng học</span>
                   </div>
                   <span class="text-xs font-black text-slate-800">{{ selectedSection.roomCapacity }} SV</span>
                </div>

                <div class="flex items-center justify-between p-3 bg-white rounded-xl border border-slate-100">
                   <div class="flex items-center gap-3">
                      <TrendingUp :size="18" class="text-emerald-500" />
                      <span class="text-xs font-bold text-slate-600">Đẩy từ Waitlist</span>
                   </div>
                   <span class="text-xs font-black text-emerald-600">{{ Math.min(newCapacity - selectedSection.capacity, selectedSection.waitlist) > 0 ? '+' + Math.min(newCapacity - selectedSection.capacity, selectedSection.waitlist) : 0 }} SV</span>
                </div>

                <div v-if="newCapacity > selectedSection.roomCapacity" class="p-4 bg-rose-50 rounded-xl border border-rose-100 flex gap-3">
                   <AlertTriangle :size="18" class="text-rose-500 shrink-0" />
                   <p class="text-[11px] font-bold text-rose-700">Sức chứa mới vượt quá giới hạn của phòng học hiện tại. Vui lòng đổi phòng sau khi cập nhật.</p>
                </div>
             </div>
          </div>
        </div>

        <div class="mt-8 flex items-center justify-end gap-4 border-t border-slate-50 pt-6">
           <button class="px-6 py-2.5 text-sm font-bold text-slate-500 hover:text-slate-800 transition-colors">Hủy bỏ</button>
           <button 
             @click="handleAdjust"
             :disabled="isProcessing"
             class="lg-button-primary px-8 py-2.5 text-sm font-bold shadow-lg shadow-blue-500/20 disabled:opacity-50"
           >
              {{ isProcessing ? 'Đang xử lý...' : 'Áp dụng thay đổi' }}
           </button>
        </div>
      </div>

    </div>
  </PageContainer>
</template>
