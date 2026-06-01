<script setup>
import { ref } from 'vue'
import { 
  Send, 
  Save, 
  Clock, 
  Users, 
  Mail, 
  Smartphone, 
  Bell, 
  Info,
  CheckCircle2,
  X,
  Plus
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── State ────────────────────────────────────────────────────
const title = ref('')
const content = ref('')
const sendMode = ref('now') // now, schedule
const scheduleTime = ref('')

const targets = ref([
  { id: 'all-students', label: 'Tất cả sinh viên', selected: false },
  { id: 'by-class', label: 'Theo lớp sinh viên', selected: false },
  { id: 'by-section', label: 'Theo lớp học phần', selected: true },
  { id: 'waitlist', label: 'Sinh viên trong Waitlist', selected: false },
])

const channels = ref([
  { id: 'in-app', label: 'In-app Notification', icon: Bell, selected: true },
  { id: 'email', label: 'Email Service', icon: Mail, selected: true },
  { id: 'push', label: 'Mobile Push', icon: Smartphone, selected: false },
])

const selectedSections = ref(['SE1601 - Java Programming', 'SE1602 - Data Structures'])

function removeSection(index) {
  selectedSections.value.splice(index, 1)
}
</script>

<template>
  <PageContainer 
    title="Gửi thông báo học vụ" 
    subtitle="Soạn thảo và gửi thông báo đến các đối tượng sinh viên, giảng viên theo nhu cầu."
  >
    <div class="grid grid-cols-1 xl:grid-cols-3 gap-8">
      
      <!-- ── Left: Composer ── -->
      <div class="xl:col-span-2 space-y-4">
        <div class="lg-card-glass p-8">
          <div class="space-y-4">
             <!-- Title -->
             <div>
                <label class="text-[11px] font-black text-label uppercase tracking-widest mb-2 block">Tiêu đề thông báo</label>
                <input 
                  v-model="title"
                  type="text" 
                  placeholder="Ví dụ: Thông báo thay đổi lịch học bù môn Java..." 
                  class="w-full lg-input rounded-2xl px-5 py-3.5 text-sm font-bold outline-none transition-all placeholder:text-placeholder"
                >
             </div>

             <!-- Content -->
             <div>
                 <label class="text-[11px] font-black text-label uppercase tracking-widest mb-2 block">Nội dung chi tiết</label>
                 <div class="border-default rounded-3xl overflow-hidden lg-card-glass transition-all">
                    <!-- Toolbar Mock -->
                    <div class="px-4 py-2 surface-solid border-default flex items-center gap-2">
                       <button class="p-1.5 lg-button-ghost rounded-lg text-xs font-black">B</button>
                       <button class="p-1.5 lg-button-ghost rounded-lg text-xs italic font-serif">I</button>
                       <button class="p-1.5 lg-button-ghost rounded-lg text-xs underline">U</button>
                       <div class="w-px h-4 border-default mx-1"></div>
                      <button class="p-1.5 hover:bg-white rounded-lg text-slate-400"><Plus :size="14" /></button>
                   </div>
                    <textarea 
                      v-model="content"
                      placeholder="Nhập nội dung thông báo tại đây..."
                      class="w-full p-4 text-sm font-medium text-body outline-none h-64 resize-none leading-relaxed"
                    ></textarea>
                </div>
             </div>
          </div>
        </div>

        <!-- Targeting Context -->
        <div class="lg-card-glass p-5 surface-solid border-default">
            <h4 class="text-xs font-black text-heading uppercase tracking-widest mb-4 flex items-center gap-2">
               <Users :size="16" /> Đối tượng nhận thông báo
            </h4>
            
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-8">
               <label 
                 v-for="target in targets" 
                 :key="target.id"
                 class="flex items-center gap-3 p-4 lg-glass-soft border-default cursor-pointer hover:border-blue-300 transition-all"
               >
                  <div class="relative flex items-center">
                     <input type="checkbox" v-model="target.selected" class="peer h-5 w-5 opacity-0 absolute">
                     <div class="h-5 w-5 border-2 border-default rounded-lg peer-checked:bg-link peer-checked:border-link flex items-center justify-center transition-all">
                        <CheckCircle2 v-if="target.selected" :size="14" class="text-white" />
                     </div>
                  </div>
                  <span class="text-sm font-bold text-heading">{{ target.label }}</span>
               </label>
            </div>

            <!-- Specific Targets Tags -->
            <div class="space-y-4">
               <p class="text-[10px] font-black text-label uppercase tracking-widest">Danh sách đã chọn</p>
               <div class="flex flex-wrap gap-2">
                  <div v-for="(sec, idx) in selectedSections" :key="sec" class="flex items-center gap-2 px-3 py-1.5 lg-badge-info rounded-xl text-xs font-bold shadow-sm">
                     {{ sec }}
                     <button @click="removeSection(idx)" class="hover:bg-white/10 rounded-full p-0.5"><X :size="12" /></button>
                  </div>
                  <button class="px-3 py-1.5 border-2 border-dashed border-default text-placeholder rounded-xl text-xs font-bold hover:border-blue-300 hover:text-link transition-all">
                    + Thêm lớp/SV
                 </button>
              </div>
           </div>
        </div>
      </div>

      <!-- ── Right: Delivery Settings ── -->
      <div class="space-y-4">
        
        <!-- Channels -->
        <div class="lg-card-glass p-8">
            <h4 class="text-xs font-black text-label uppercase tracking-widest mb-4">Kênh phân phối</h4>
            <div class="space-y-3">
               <div v-for="ch in channels" :key="ch.id" class="flex items-center justify-between p-4 surface-solid rounded-2xl border-default group">
                  <div class="flex items-center gap-3">
                     <div :class="['h-9 w-9 rounded-xl flex items-center justify-center transition-colors', ch.selected ? 'bg-link text-white shadow-md' : 'surface-solid text-placeholder']">
                        <component :is="ch.icon" :size="18" />
                     </div>
                     <span :class="['text-sm font-bold', ch.selected ? 'text-heading' : 'text-placeholder']">{{ ch.label }}</span>
                  </div>
                  <button @click="ch.selected = !ch.selected" :class="['h-5 w-10 rounded-full relative transition-colors p-1', ch.selected ? 'bg-link' : 'border-default border']">
                     <div :class="['h-3 w-3 bg-white rounded-full transition-transform', ch.selected ? 'translate-x-5' : 'translate-x-0']"></div>
                 </button>
              </div>
           </div>
        </div>

        <!-- Schedule -->
        <div class="lg-card-glass p-8">
            <h4 class="text-xs font-black text-label uppercase tracking-widest mb-4">Thời gian gửi</h4>
            <div class="flex flex-col gap-4">
               <label class="flex items-center gap-3 cursor-pointer group">
                  <div class="relative flex items-center">
                     <input type="radio" value="now" v-model="sendMode" class="peer h-5 w-5 opacity-0 absolute">
                     <div class="h-5 w-5 border-2 border-default rounded-full peer-checked:border-link peer-checked:border-[6px] transition-all"></div>
                  </div>
                  <span class="text-sm font-bold text-heading">Gửi ngay lập tức</span>
               </label>
               <label class="flex items-center gap-3 cursor-pointer group">
                  <div class="relative flex items-center">
                     <input type="radio" value="schedule" v-model="sendMode" class="peer h-5 w-5 opacity-0 absolute">
                     <div class="h-5 w-5 border-2 border-default rounded-full peer-checked:border-link peer-checked:border-[6px] transition-all"></div>
                  </div>
                  <span class="text-sm font-bold text-heading">Lên lịch gửi sau</span>
              </label>

              <div v-if="sendMode === 'schedule'" class="mt-2 pl-8 animate-in fade-in slide-in-from-top-2 duration-300">
                 <div class="relative">
                     <Clock :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-placeholder" />
                     <input 
                       type="datetime-local" 
                       class="w-full lg-input rounded-xl pl-10 pr-4 py-2.5 text-xs font-bold"
                     >
                 </div>
              </div>
           </div>
        </div>

        <!-- Main Actions -->
        <div class="space-y-3">
           <button class="w-full lg-button-primary py-4 text-sm font-black shadow-lg shadow-blue-500/20 flex items-center justify-center gap-2">
              <Send :size="20" /> GỬI THÔNG BÁO
           </button>
            <button class="w-full lg-button-secondary py-3 text-sm font-bold flex items-center justify-center gap-2">
              <Save :size="18" /> LƯU BẢN NHÁP
           </button>
        </div>

        <!-- Help Info -->
         <div class="p-5 rounded-3xl bg-warning/10 border-warning flex items-start gap-4">
            <Info :size="18" class="text-warning shrink-0 mt-0.5" />
            <p class="text-xs text-heading leading-relaxed font-medium">
             <strong>Lưu ý:</strong> Thông báo sẽ được gửi đồng thời qua các kênh đã chọn. Hãy đảm bảo nội dung chính xác trước khi nhấn nút Gửi.
           </p>
        </div>

      </div>

    </div>
  </PageContainer>
</template>
