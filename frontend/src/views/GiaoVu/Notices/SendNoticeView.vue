<script setup>
import { ref, computed } from 'vue'
import { usePopupStore } from '@/stores/popup'
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
  Plus,
  Search
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

const popupStore = usePopupStore()

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

// ── Add Section Modal ────────────────────────────────────────
const showAddModal = ref(false)
const sectionSearch = ref('')
const availableSections = ref([
  'SE1601 - Java Programming',
  'SE1602 - Data Structures',
  'SE1603 - Web Development',
  'SE1604 - Mobile App Development',
  'SE1605 - Artificial Intelligence',
  'SE1606 - Database Systems',
  'SE1607 - Software Engineering',
  'SE1608 - Computer Networks',
])

const filteredSections = computed(() =>
  availableSections.value.filter(
    s => s.toLowerCase().includes(sectionSearch.value.toLowerCase()) && !selectedSections.value.includes(s),
  ),
)

function toggleSection(section) {
  const idx = selectedSections.value.indexOf(section)
  if (idx >= 0) {
    selectedSections.value.splice(idx, 1)
  } else {
    selectedSections.value.push(section)
  }
  sectionSearch.value = ''
}

function removeSection(index) {
  selectedSections.value.splice(index, 1)
}

function sendNotice() {
  if (!title.value.trim()) {
    popupStore.warning('Thiếu thông tin', 'Vui lòng nhập tiêu đề thông báo.')
    return
  }
  if (!content.value.trim()) {
    popupStore.warning('Thiếu thông tin', 'Vui lòng nhập nội dung thông báo.')
    return
  }
  const selectedTargets = targets.value.filter(t => t.selected)
  if (!selectedTargets.length) {
    popupStore.warning('Thiếu thông tin', 'Vui lòng chọn ít nhất một đối tượng nhận.')
    return
  }
  const selectedChannels = channels.value.filter(c => c.selected)
  if (!selectedChannels.length) {
    popupStore.warning('Thiếu thông tin', 'Vui lòng chọn ít nhất một kênh gửi.')
    return
  }
  popupStore.success(
    'Đã gửi thông báo',
    sendMode.value === 'now'
      ? 'Thông báo đang được gửi đến các đối tượng đã chọn.'
      : `Thông báo đã được lên lịch gửi vào ${new Date(scheduleTime.value).toLocaleString('vi-VN')}.`,
  )
}

function saveDraft() {
  if (!title.value.trim()) {
    popupStore.warning('Thiếu thông tin', 'Vui lòng nhập tiêu đề để lưu nháp.')
    return
  }
  const draft = {
    title: title.value,
    content: content.value,
    sendMode: sendMode.value,
    scheduleTime: scheduleTime.value,
    targets: targets.value.map(t => ({ id: t.id, selected: t.selected })),
    channels: channels.value.map(c => ({ id: c.id, selected: c.selected })),
    selectedSections: [...selectedSections.value],
    savedAt: new Date().toISOString(),
  }
  localStorage.setItem('lms_notice_draft', JSON.stringify(draft))
  popupStore.success('Đã lưu nháp', 'Thông báo đã được lưu vào bản nháp.')
}
</script>

<template>
  <PageContainer 
    title="Gửi thông báo học vụ" 
    subtitle="Soạn thảo và gửi thông báo đến các đối tượng sinh viên, giảng viên theo nhu cầu."
  >
    <div class="grid grid-cols-1 xl:grid-cols-3 gap-6">
      
      <!-- ── Left: Composer ── -->
      <div class="xl:col-span-2 space-y-4">
        <div class="surface-card border border-card rounded-2xl p-6">
          <div class="space-y-4">
             <!-- Title -->
             <div>
                <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-2 block">Tiêu đề thông báo</label>
                <input 
                  v-model="title"
                  type="text" 
                  placeholder="Ví dụ: Thông báo thay đổi lịch học bù môn Java..." 
                  class="w-full lg-input rounded-2xl px-5 py-3.5 text-sm font-bold outline-none transition-all placeholder:text-placeholder"
                >
             </div>

             <!-- Content -->
             <div>
                 <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-2 block">Nội dung chi tiết</label>
                 <div class="border border-card rounded-2xl overflow-hidden surface-card transition-all">
                    <!-- Toolbar Mock -->
                    <div class="px-4 py-2 surface-solid border-b border-default flex items-center gap-2">
                       <button class="p-1.5 lg-button-ghost rounded-lg text-xs font-semibold">B</button>
                       <button class="p-1.5 lg-button-ghost rounded-lg text-xs italic font-serif">I</button>
                       <button class="p-1.5 lg-button-ghost rounded-lg text-xs underline">U</button>
                       <div class="w-px h-4 border-default mx-1"></div>
                      <button class="p-1.5 hover:bg-[var(--surface-input)] rounded-lg text-muted"><Plus :size="14" /></button>
                   </div>
                    <textarea 
                      v-model="content"
                      placeholder="Nhập nội dung thông báo tại đây..."
                      class="w-full surface-input p-4 text-sm font-medium text-body outline-none h-56 resize-none leading-relaxed"
                    ></textarea>
                </div>
             </div>
          </div>
        </div>

        <!-- Targeting Context -->
        <div class="surface-card border border-card rounded-2xl p-5">
            <h4 class="text-xs font-semibold text-heading uppercase tracking-widest mb-4 flex items-center gap-2">
               <Users :size="16" /> Đối tượng nhận thông báo
            </h4>
            
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-8">
               <label 
                 v-for="target in targets" 
                 :key="target.id"
                 class="flex items-center gap-3 p-4 surface-card border border-card rounded-2xl cursor-pointer hover:border-[var(--border-input-focus)] transition-all"
               >
                  <div class="relative flex items-center">
                     <input type="checkbox" v-model="target.selected" class="peer h-5 w-5 opacity-0 absolute">
                     <div class="h-5 w-5 border-2 border-default rounded-lg peer-checked:bg-[var(--lg-primary)] peer-checked:border-[var(--lg-primary)] flex items-center justify-center transition-all">
                        <CheckCircle2 v-if="target.selected" :size="14" class="text-white" />
                     </div>
                  </div>
                  <span class="text-sm font-bold text-heading">{{ target.label }}</span>
               </label>
            </div>

            <!-- Specific Targets Tags -->
            <div class="space-y-4">
               <p class="text-[10px] font-semibold text-label uppercase tracking-widest">Danh sách đã chọn</p>
               <div class="flex flex-wrap gap-2">
                  <div v-for="(sec, idx) in selectedSections" :key="sec" class="flex items-center gap-2 px-3 py-1.5 lg-badge-info rounded-xl text-xs font-bold shadow-sm">
                     {{ sec }}
                     <button @click="removeSection(idx)" class="hover:bg-[var(--surface-input)] rounded-full p-0.5"><X :size="12" /></button>
                  </div>
                   <button @click="showAddModal = true" class="px-3 py-1.5 border-2 border-dashed border-default text-placeholder rounded-xl text-xs font-bold hover:border-[var(--border-input-focus)] hover:text-link transition-all">
                     + Thêm lớp/SV
                  </button>
              </div>
           </div>
        </div>
      </div>

      <!-- ── Right: Delivery Settings ── -->
      <div class="space-y-4">
        
        <!-- Channels -->
        <div class="surface-card border border-card rounded-2xl p-5">
            <h4 class="text-xs font-semibold text-label uppercase tracking-widest mb-4">Kênh phân phối</h4>
            <div class="space-y-3">
               <div v-for="ch in channels" :key="ch.id" class="flex items-center justify-between p-4 surface-solid rounded-2xl border border-default group">
                  <div class="flex items-center gap-3">
                     <div :class="['h-9 w-9 rounded-xl flex items-center justify-center transition-colors', ch.selected ? 'bg-[var(--lg-primary)] text-white shadow-sm' : 'surface-solid text-placeholder']">
                        <component :is="ch.icon" :size="18" />
                     </div>
                     <span :class="['text-sm font-bold', ch.selected ? 'text-heading' : 'text-placeholder']">{{ ch.label }}</span>
                  </div>
                  <button @click="ch.selected = !ch.selected" :class="['h-5 w-10 rounded-full relative transition-colors p-1', ch.selected ? 'bg-[var(--lg-primary)]' : 'border-default border']">
                     <div :class="['h-3 w-3 bg-[var(--surface-modal)] rounded-full transition-transform', ch.selected ? 'translate-x-5' : 'translate-x-0']"></div>
                 </button>
              </div>
           </div>
        </div>

        <!-- Schedule -->
        <div class="surface-card border border-card rounded-2xl p-5">
            <h4 class="text-xs font-semibold text-label uppercase tracking-widest mb-4">Thời gian gửi</h4>
            <div class="flex flex-col gap-4">
               <label class="flex items-center gap-3 cursor-pointer group">
                  <div class="relative flex items-center">
                     <input type="radio" value="now" v-model="sendMode" class="peer h-5 w-5 opacity-0 absolute">
                     <div class="h-5 w-5 border-2 border-default rounded-full peer-checked:border-[var(--lg-primary)] peer-checked:border-[6px] transition-all"></div>
                  </div>
                  <span class="text-sm font-bold text-heading">Gửi ngay lập tức</span>
               </label>
               <label class="flex items-center gap-3 cursor-pointer group">
                  <div class="relative flex items-center">
                     <input type="radio" value="schedule" v-model="sendMode" class="peer h-5 w-5 opacity-0 absolute">
                     <div class="h-5 w-5 border-2 border-default rounded-full peer-checked:border-[var(--lg-primary)] peer-checked:border-[6px] transition-all"></div>
                  </div>
                  <span class="text-sm font-bold text-heading">Lên lịch gửi sau</span>
              </label>

              <div v-if="sendMode === 'schedule'" class="mt-2 pl-8 animate-in fade-in slide-in-from-top-2 duration-300">
                 <div class="relative">
                     <Clock :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-placeholder" />
                     <input 
                        v-model="scheduleTime"
                        type="datetime-local" 
                        class="w-full lg-input rounded-xl pl-10 pr-4 py-2.5 text-xs font-bold"
                      >
                 </div>
              </div>
           </div>
        </div>

        <!-- Main Actions -->
        <div class="space-y-3">
           <button @click="sendNotice" class="w-full lg-button-primary py-4 text-sm font-semibold flex items-center justify-center gap-2">
               <Send :size="20" /> GỬI THÔNG BÁO
            </button>
             <button @click="saveDraft" class="w-full lg-button-secondary py-3 text-sm font-bold flex items-center justify-center gap-2">
               <Save :size="18" /> LƯU BẢN NHÁP
            </button>
        </div>

        <!-- Help Info -->
         <div class="p-5 rounded-2xl bg-[var(--color-warning-bg)] border border-[var(--color-warning-text)]/20 flex items-start gap-4">
            <Info :size="18" class="text-[var(--color-warning-text)] shrink-0 mt-0.5" />
            <p class="text-xs text-heading leading-relaxed font-medium">
             <strong>Lưu ý:</strong> Thông báo sẽ được gửi đồng thời qua các kênh đã chọn. Hãy đảm bảo nội dung chính xác trước khi nhấn nút Gửi.
           </p>
        </div>

      </div>

    </div>
  </PageContainer>

  <!-- ── Add Section Modal ── -->
  <Teleport to="body">
    <div v-if="showAddModal" class="fixed inset-0 z-50 flex items-center justify-center" @click.self="showAddModal = false">
      <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"></div>
      <div class="relative surface-card border border-card rounded-2xl p-6 w-full max-w-lg mx-4 shadow-2xl animate-in fade-in zoom-in-95 duration-200">
        <div class="flex items-center justify-between mb-5">
          <h4 class="text-sm font-semibold text-heading uppercase tracking-widest">Thêm lớp học phần / Sinh viên</h4>
          <button @click="showAddModal = false" class="p-1.5 hover:surface-solid rounded-lg transition-all">
            <X :size="16" class="text-placeholder" />
          </button>
        </div>

        <div class="relative mb-4">
          <Search :size="16" class="absolute left-3.5 top-1/2 -translate-y-1/2 text-placeholder" />
          <input
            v-model="sectionSearch"
            type="text"
            placeholder="Tìm kiếm lớp học phần..."
            class="w-full lg-input rounded-xl pl-10 pr-4 py-2.5 text-sm font-medium outline-none"
          />
        </div>

        <ul class="space-y-1 max-h-64 overflow-y-auto">
          <li
            v-for="section in filteredSections"
            :key="section"
            @click="toggleSection(section)"
            class="flex items-center gap-3 px-3.5 py-2.5 rounded-xl cursor-pointer transition-all hover:surface-solid text-sm font-bold text-heading"
          >
            <div :class="['h-4 w-4 rounded border-2 flex items-center justify-center transition-all', selectedSections.includes(section) ? 'bg-[var(--lg-primary)] border-[var(--lg-primary)]' : 'border-default']">
              <CheckCircle2 v-if="selectedSections.includes(section)" :size="10" class="text-white" />
            </div>
            {{ section }}
          </li>
          <li v-if="!filteredSections.length" class="text-center py-6 text-xs text-placeholder font-medium">
            Không tìm thấy lớp học phần nào.
          </li>
        </ul>

        <div class="mt-5 flex justify-end gap-3">
          <button @click="showAddModal = false" class="lg-button-ghost px-5 py-2 text-xs font-bold rounded-xl">Đóng</button>
          <button @click="showAddModal = false" class="lg-button-primary px-5 py-2 text-xs font-bold rounded-xl">Xác nhận</button>
        </div>
      </div>
    </div>
  </Teleport>
</template>
