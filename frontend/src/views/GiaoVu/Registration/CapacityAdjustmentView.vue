<script setup>
import { ref } from 'vue'
import { usePopupStore } from '@/stores/popup'
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

const popupStore = usePopupStore()

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
    popupStore.success('Đã cập nhật', 'Sức chứa lớp học phần đã được điều chỉnh.')
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
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div class="lg-card-glass p-4 flex flex-col items-center text-center">
          <p class="text-[10px] font-black text-placeholder uppercase tracking-widest mb-2">Sức chứa hiện tại</p>
          <div class="h-10 w-10 rounded-full lg-glass-soft text-link flex items-center justify-center border-4 border-white shadow-sm mb-3">
             <span class="text-xl font-black">{{ selectedSection.capacity }}</span>
          </div>
          <p class="text-xs font-bold text-label">Maximum capacity</p>
        </div>
        <div class="lg-card-glass p-4 flex flex-col items-center text-center">
          <p class="text-[10px] font-black text-placeholder uppercase tracking-widest mb-2">Đã đăng ký</p>
          <div class="h-10 w-10 rounded-full lg-glass-soft text-link flex items-center justify-center border-4 border-white shadow-sm mb-3">
             <span class="text-xl font-black">{{ selectedSection.enrolled }}</span>
          </div>
          <p class="text-xs font-bold text-label">Currently enrolled</p>
        </div>
        <div class="lg-card-glass p-4 flex flex-col items-center text-center border border-[var(--color-warning-bg)]/30">
          <p class="text-[10px] font-black text-placeholder uppercase tracking-widest mb-2">Đang đợi (Waitlist)</p>
          <div class="h-10 w-10 rounded-full lg-glass-soft text-link flex items-center justify-center border-4 border-white shadow-sm mb-3">
             <span class="text-xl font-black">{{ selectedSection.waitlist }}</span>
          </div>
          <p class="text-xs font-bold text-[var(--lg-warning)]">Students in queue</p>
        </div>
      </div>

      <!-- ── Adjustment Form ── -->
      <div class="lg-card-glass p-8">
        <h3 class="text-lg font-black text-heading mb-4 flex items-center gap-2">
          <Layers :size="20" class="text-link" /> CẤU HÌNH SỨC CHỨA MỚI
        </h3>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
          <div class="space-y-4">
            <div class="space-y-2">
              <label class="text-[10px] font-black text-placeholder uppercase tracking-widest ml-1">Sức chứa mới</label>
              <div class="flex items-center gap-4">
                <input 
                  v-model="newCapacity" 
                  type="number" 
                  class="flex-1 lg-input px-4 py-3 text-lg font-black text-heading transition-all"
                >
                <div class="flex flex-col gap-1">
                  <button @click="newCapacity++" class="p-1 hover:bg-[var(--surface-solid)] rounded-lg text-placeholder"><TrendingUp :size="16" /></button>
                  <button @click="newCapacity--" class="p-1 hover:bg-[var(--surface-solid)] rounded-lg text-placeholder"><TrendingDown :size="16" /></button>
                </div>
              </div>
            </div>

            <div class="space-y-2">
              <label class="text-[10px] font-black text-placeholder uppercase tracking-widest ml-1">Lý do điều chỉnh</label>
              <textarea 
                v-model="reason" 
                placeholder="Nhập lý do (Ví dụ: Theo nhu cầu SV, Đổi phòng lớn hơn...)"
                class="w-full lg-input px-4 py-3 text-sm font-medium h-24 resize-none"
              ></textarea>
            </div>
          </div>

          <div class="surface-solid rounded-[24px] p-4 border border-default space-y-4">
             <h4 class="text-xs font-black text-label uppercase tracking-widest">Xem trước ảnh hưởng</h4>
             
             <div class="space-y-4">
                <div class="flex items-center justify-between p-3 surface-card rounded-xl border border-default">
                   <div class="flex items-center gap-3">
                      <Building :size="18" class="text-placeholder" />
                      <span class="text-xs font-bold text-label">Giới hạn phòng học</span>
                   </div>
                   <span class="text-xs font-black text-heading">{{ selectedSection.roomCapacity }} SV</span>
                </div>

                <div class="flex items-center justify-between p-3 surface-card rounded-xl border border-default">
                   <div class="flex items-center gap-3">
                      <TrendingUp :size="18" class="text-[var(--lg-success)]" />
                      <span class="text-xs font-bold text-label">Đẩy từ Waitlist</span>
                   </div>
                   <span class="text-xs font-black text-[var(--lg-success)]">{{ Math.min(newCapacity - selectedSection.capacity, selectedSection.waitlist) > 0 ? '+' + Math.min(newCapacity - selectedSection.capacity, selectedSection.waitlist) : 0 }} SV</span>
                </div>

                <div v-if="newCapacity > selectedSection.roomCapacity" class="p-4 bg-[var(--color-danger-bg)] rounded-xl border border-[var(--color-danger-bg)]/50 flex gap-3">
                   <AlertTriangle :size="18" class="text-[var(--lg-danger)] shrink-0" />
                   <p class="text-[11px] font-bold text-[var(--lg-danger)]">Sức chứa mới vượt quá giới hạn của phòng học hiện tại. Vui lòng đổi phòng sau khi cập nhật.</p>
                </div>
             </div>
          </div>
        </div>

        <div class="mt-8 flex items-center justify-end gap-4 border-t border-default pt-6">
           <button class="px-4 py-2.5 text-sm font-bold text-label hover:text-heading transition-colors">Hủy bỏ</button>
           <button 
             @click="handleAdjust"
             :disabled="isProcessing"
             class="lg-button-primary px-5 py-2.5 text-sm font-bold shadow-lg shadow-blue-500/20 disabled:opacity-50"
           >
              {{ isProcessing ? 'Đang xử lý...' : 'Áp dụng thay đổi' }}
           </button>
        </div>
      </div>

    </div>
  </PageContainer>
</template>
