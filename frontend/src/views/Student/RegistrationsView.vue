<script setup>
import { ref, computed } from 'vue'
import {
  BookOpen, Users, Clock, AlertTriangle, Sparkles,
  CheckCircle2, XCircle, Search, Filter,
  ArrowRightLeft, MinusCircle, UserPlus, Zap
} from 'lucide-vue-next'

// Mock Data
const metrics = [
  { label: 'Tín chỉ đã đăng ký', value: '15', max: '21', unit: 'TC', icon: BookOpen, tone: 'blue', hint: 'Tối đa 21 TC' },
  { label: 'Số lớp đã chọn', value: '5', unit: 'lớp', icon: CheckCircle2, tone: 'green', hint: 'Bao gồm lý thuyết & thực hành' },
  { label: 'Đang ở hàng chờ', value: '1', unit: 'môn', icon: Clock, tone: 'amber', hint: 'Waitlist (vị trí #2)' },
]

const subjects = ['Tất cả', 'Cấu trúc dữ liệu & Giải thuật', 'Toán rời rạc', 'Lập trình Web', 'Tiếng Anh 2']
const statuses = ['Tất cả', 'Open', 'Full', 'Enrolled', 'Waitlist']

const mockClasses = ref([
  { id: 101, code: 'CS301-A', subject: 'Cấu trúc dữ liệu & Giải thuật', credits: 3, teacher: 'TS. Nguyễn Minh Khoa', schedule: 'T2, 07:30 - 09:30 | P.302', slots: 40, maxSlots: 40, status: 'Full', aiTag: 'hot', prereq: 'Lập trình cơ bản' },
  { id: 102, code: 'CS301-B', subject: 'Cấu trúc dữ liệu & Giải thuật', credits: 3, teacher: 'ThS. Trần Thu Hà', schedule: 'T3, 13:30 - 15:30 | P.105', slots: 35, maxSlots: 40, status: 'Open', aiTag: 'optimal', prereq: 'Lập trình cơ bản' },
  { id: 103, code: 'MA201-A', subject: 'Toán rời rạc', credits: 3, teacher: 'TS. Lê Minh', schedule: 'T4, 07:30 - 09:30 | P.102', slots: 40, maxSlots: 40, status: 'Enrolled', aiTag: '' },
  { id: 104, code: 'CS402-C', subject: 'Lập trình Web', credits: 4, teacher: 'KS. Lê Văn Tâm', schedule: 'T5, 08:00 - 11:00 | Lab 04', slots: 30, maxSlots: 30, status: 'Waitlist', aiTag: 'hot', waitlistPos: 2 },
  { id: 105, code: 'ENG102-A', subject: 'Tiếng Anh 2', credits: 2, teacher: 'ThS. Nguyễn Lan', schedule: 'T6, 13:30 - 15:30 | P.205', slots: 15, maxSlots: 35, status: 'Open', aiTag: '' }
])

const statusConfig = {
  Open: { label: 'Đang mở', cls: 'badge-blue', icon: CheckCircle2 },
  Full: { label: 'Đã đầy', cls: 'badge-red', icon: Users },
  Enrolled: { label: 'Đã đăng ký', cls: 'badge-green', icon: CheckCircle2 },
  Waitlist: { label: 'Hàng chờ', cls: 'badge-amber', icon: Clock },
  Dropped: { label: 'Đã hủy', cls: 'badge-slate', icon: XCircle }
}

const aiBanner = {
  title: 'AI Gợi ý Lộ trình Tối ưu',
  message: 'Thuật toán đề xuất bạn nên chọn lớp CS301-B để tối ưu thời khóa biểu, tránh khoảng trống lịch vào chiều thứ 3. Lớp CS402-C đang có nguy cơ quá tải (hot), hãy giữ vị trí Waitlist.'
}

// State
const filterSubject = ref('Tất cả')
const filterStatus = ref('Tất cả')
const searchQuery = ref('')

// Modals
const modalMode = ref('') // 'enroll', 'waitlist', 'withdraw', 'swap'
const activeClass = ref(null)

const confirmChecks = ref({
  prereq: false,
  schedule: false,
  credits: false
})
const confirmLoading = ref(false)
const swapTarget = ref('')

// Computed
const filteredClasses = computed(() => {
  return mockClasses.value.filter(c => {
    const matchSub = filterSubject.value === 'Tất cả' || c.subject === filterSubject.value
    const matchStat = filterStatus.value === 'Tất cả' || c.status === filterStatus.value
    const matchQuery = !searchQuery.value || c.code.toLowerCase().includes(searchQuery.value.toLowerCase()) || c.subject.toLowerCase().includes(searchQuery.value.toLowerCase())
    return matchSub && matchStat && matchQuery
  })
})

const getProgressColor = (slots, max) => {
  const p = slots / max
  if (p >= 1) return 'bg-red-500'
  if (p >= 0.8) return 'bg-amber-500'
  return 'bg-blue-500'
}

// Actions
const openModal = (mode, cls) => {
  modalMode.value = mode
  activeClass.value = cls
  
  if (mode === 'enroll') {
    confirmChecks.value = { prereq: false, schedule: false, credits: false }
    // Simulate checking
    setTimeout(() => confirmChecks.value.prereq = true, 400)
    setTimeout(() => confirmChecks.value.schedule = true, 800)
    setTimeout(() => confirmChecks.value.credits = true, 1200)
  }
}

const closeModal = () => {
  modalMode.value = ''
  activeClass.value = null
  swapTarget.value = ''
}

const handleAction = () => {
  confirmLoading.value = true
  setTimeout(() => {
    // Mock action success
    if (modalMode.value === 'enroll') {
      const idx = mockClasses.value.findIndex(c => c.id === activeClass.value.id)
      if (idx !== -1) mockClasses.value[idx].status = 'Enrolled'
    } else if (modalMode.value === 'waitlist') {
      const idx = mockClasses.value.findIndex(c => c.id === activeClass.value.id)
      if (idx !== -1) mockClasses.value[idx].status = 'Waitlist'
    } else if (modalMode.value === 'withdraw') {
      const idx = mockClasses.value.findIndex(c => c.id === activeClass.value.id)
      if (idx !== -1) {
        mockClasses.value[idx].status = 'Open'
        mockClasses.value[idx].slots -= 1
      }
    }
    confirmLoading.value = false
    closeModal()
  }, 1000)
}

</script>

<template>
  <div class="registration-page">
    <!-- Header -->
    <div class="page-header">
      <div>
        <div class="eyebrow"><BookOpen :size="15"/>Đăng ký & Quản lý lớp</div>
        <h1 class="page-title">Đăng ký môn học</h1>
        <p class="page-sub">Xây dựng lộ trình học tập, theo dõi sĩ số và xếp hàng chờ lớp thời gian thực.</p>
      </div>
    </div>

    <!-- AI Suggestion Banner -->
    <div class="ai-banner banner-violet">
      <div class="banner-icon"><Sparkles :size="24" /></div>
      <div class="banner-content">
        <h3>{{ aiBanner.title }}</h3>
        <p>{{ aiBanner.message }}</p>
      </div>
    </div>

    <!-- Metrics -->
    <div class="metrics-grid">
      <div v-for="m in metrics" :key="m.label" class="metric-card" :class="`metric-${m.tone}`">
        <div class="metric-icon-wrap"><component :is="m.icon" :size="20"/></div>
        <div class="metric-body">
          <div class="metric-val">
            {{ m.value }}<span v-if="m.max" class="text-slate-400 text-base font-semibold">/{{ m.max }}</span>
            <span class="metric-unit">{{ m.unit }}</span>
          </div>
          <div class="metric-lbl">{{ m.label }}</div>
          <div class="metric-hint">{{ m.hint }}</div>
        </div>
      </div>
    </div>

    <!-- Toolbar -->
    <div class="toolbar">
      <div class="search-box">
        <Search :size="16" class="text-slate-400" />
        <input v-model="searchQuery" type="text" placeholder="Tìm tên môn, mã lớp..." />
      </div>
      <div class="filter-group">
        <Filter :size="16" class="text-slate-400" />
        <select v-model="filterSubject" class="filter-select">
          <option v-for="s in subjects" :key="s" :value="s">{{ s }}</option>
        </select>
        <select v-model="filterStatus" class="filter-select">
          <option v-for="st in statuses" :key="st" :value="st">{{ statusConfig[st]?.label || st }}</option>
        </select>
      </div>
    </div>

    <!-- Class Grid -->
    <div class="class-grid">
      <div v-for="cls in filteredClasses" :key="cls.id" class="class-card" :class="`card-${cls.status}`">
        
        <!-- Card Header -->
        <div class="card-header">
          <div class="flex items-center justify-between gap-2 w-full">
            <span class="class-code">{{ cls.code }}</span>
            <div class="flex items-center gap-2">
              <!-- AI Tags -->
              <span v-if="cls.aiTag === 'optimal'" class="ai-badge optimal" title="AI đề xuất tối ưu"><Sparkles :size="12"/>Tối ưu</span>
              <span v-if="cls.aiTag === 'hot'" class="ai-badge hot" title="Lớp sắp đầy"><Zap :size="12"/>Hot</span>
              <!-- Status Badge -->
              <span class="status-badge" :class="statusConfig[cls.status].cls">
                {{ statusConfig[cls.status].label }}
              </span>
            </div>
          </div>
          <h3 class="class-subject">{{ cls.subject }}</h3>
        </div>

        <!-- Card Body -->
        <div class="card-body">
          <div class="info-line"><Clock :size="14" class="info-icon"/> {{ cls.schedule }}</div>
          <div class="info-line"><Users :size="14" class="info-icon"/> {{ cls.teacher }}</div>
          <div class="info-line"><BookOpen :size="14" class="info-icon"/> {{ cls.credits }} Tín chỉ · Yêu cầu: {{ cls.prereq || 'Không' }}</div>
          
          <!-- Slot Counter -->
          <div class="slot-container mt-3">
            <div class="flex justify-between text-xs font-semibold mb-1">
              <span class="text-slate-600">Sĩ số (Thời gian thực)</span>
              <span class="text-slate-900">{{ cls.slots }}/{{ cls.maxSlots }}</span>
            </div>
            <div class="slot-track">
              <div class="slot-fill" :class="getProgressColor(cls.slots, cls.maxSlots)" :style="{ width: `${(cls.slots / cls.maxSlots) * 100}%` }"></div>
            </div>
          </div>
          
          <div v-if="cls.status === 'Waitlist'" class="waitlist-info">
            <AlertTriangle :size="14" />
            Bạn đang ở vị trí số #{{ cls.waitlistPos }} trong hàng chờ.
          </div>
        </div>

        <!-- Card Footer Actions -->
        <div class="card-footer">
          <button v-if="cls.status === 'Open'" class="btn-primary flex-1" @click="openModal('enroll', cls)">
            <UserPlus :size="15"/> Đăng ký
          </button>
          
          <button v-if="cls.status === 'Full'" class="btn-amber flex-1" @click="openModal('waitlist', cls)">
            <Clock :size="15"/> Vào Waitlist
          </button>

          <template v-if="['Enrolled', 'Waitlist'].includes(cls.status)">
            <button class="btn-outline flex-1" @click="openModal('withdraw', cls)">
              <MinusCircle :size="15"/> Hủy đăng ký
            </button>
            <button v-if="cls.status === 'Enrolled'" class="btn-ghost" @click="openModal('swap', cls)" title="Đổi lớp chéo">
              <ArrowRightLeft :size="16"/>
            </button>
          </template>
        </div>
      </div>

      <div v-if="filteredClasses.length === 0" class="empty-state">
        Không tìm thấy lớp học phần nào khớp với bộ lọc.
      </div>
    </div>

    <!-- Modals -->
    <Teleport to="body">
      <Transition name="modal">
        <div v-if="modalMode" class="modal-overlay" @click.self="closeModal">
          <div class="modal-content">
            
            <!-- ENROLL MODAL -->
            <template v-if="modalMode === 'enroll'">
              <div class="modal-header">
                <h3>Xác nhận đăng ký lớp</h3>
                <button class="close-btn-sm" @click="closeModal"><XCircle :size="20"/></button>
              </div>
              <div class="modal-body">
                <p>Bạn đang đăng ký lớp <strong>{{ activeClass.code }}</strong> - {{ activeClass.subject }}.</p>
                
                <div class="checklist">
                  <div class="check-item" :class="confirmChecks.prereq ? 'checked' : 'pending'">
                    <component :is="confirmChecks.prereq ? CheckCircle2 : Clock" :size="18" />
                    Điều kiện tiên quyết: {{ activeClass.prereq || 'Không' }}
                  </div>
                  <div class="check-item" :class="confirmChecks.schedule ? 'checked' : 'pending'">
                    <component :is="confirmChecks.schedule ? CheckCircle2 : Clock" :size="18" />
                    Kiểm tra xung đột lịch học
                  </div>
                  <div class="check-item" :class="confirmChecks.credits ? 'checked' : 'pending'">
                    <component :is="confirmChecks.credits ? CheckCircle2 : Clock" :size="18" />
                    Giới hạn tín chỉ (Còn phép đăng ký)
                  </div>
                </div>
              </div>
              <div class="modal-footer">
                <button class="btn-secondary" @click="closeModal">Hủy</button>
                <button class="btn-primary" :disabled="!confirmChecks.credits || confirmLoading" @click="handleAction">
                  {{ confirmLoading ? 'Đang xử lý...' : 'Xác nhận Đăng ký' }}
                </button>
              </div>
            </template>

            <!-- WAITLIST MODAL -->
            <template v-else-if="modalMode === 'waitlist'">
              <div class="modal-header">
                <h3>Vào hàng chờ (Waitlist)</h3>
                <button class="close-btn-sm" @click="closeModal"><XCircle :size="20"/></button>
              </div>
              <div class="modal-body">
                <p>Lớp <strong>{{ activeClass.code }}</strong> hiện đã đủ sĩ số.</p>
                <div class="warning-box amber">
                  <AlertTriangle :size="20" class="shrink-0" />
                  <p>Khi chọn vào Waitlist, hệ thống sẽ tự động xếp bạn vào lớp theo nguyên tắc FIFO nếu có sinh viên khác hủy môn. Bạn sẽ nhận được thông báo qua Email khi được đẩy vào danh sách chính thức.</p>
                </div>
              </div>
              <div class="modal-footer">
                <button class="btn-secondary" @click="closeModal">Hủy</button>
                <button class="btn-amber" :disabled="confirmLoading" @click="handleAction">
                  {{ confirmLoading ? 'Đang xử lý...' : 'Tham gia Waitlist' }}
                </button>
              </div>
            </template>

            <!-- WITHDRAW MODAL -->
            <template v-else-if="modalMode === 'withdraw'">
              <div class="modal-header">
                <h3>Hủy đăng ký (Withdraw)</h3>
                <button class="close-btn-sm" @click="closeModal"><XCircle :size="20"/></button>
              </div>
              <div class="modal-body">
                <p>Bạn chắc chắn muốn rút khỏi lớp <strong>{{ activeClass.code }}</strong> - {{ activeClass.subject }}?</p>
                <div class="warning-box red">
                  <AlertTriangle :size="20" class="shrink-0" />
                  <p>Hành động này không thể hoàn tác. Số tín chỉ sẽ được hoàn trả và hệ thống sẽ tự động nhường chỗ cho sinh viên đang ở hàng chờ.</p>
                </div>
              </div>
              <div class="modal-footer">
                <button class="btn-secondary" @click="closeModal">Thoát</button>
                <button class="btn-red" :disabled="confirmLoading" @click="handleAction">
                  {{ confirmLoading ? 'Đang xử lý...' : 'Xác nhận Hủy' }}
                </button>
              </div>
            </template>

            <!-- SWAP MODAL -->
            <template v-else-if="modalMode === 'swap'">
              <div class="modal-header">
                <h3>Đổi lớp chéo (Atomic Swap)</h3>
                <button class="close-btn-sm" @click="closeModal"><XCircle :size="20"/></button>
              </div>
              <div class="modal-body">
                <p>Nhập mã sinh viên hoặc mã giao dịch để đổi chỗ lớp <strong>{{ activeClass.code }}</strong>.</p>
                <div class="form-group mt-4">
                  <label>Mã Sinh viên đối tác</label>
                  <input v-model="swapTarget" type="text" class="input-glass" placeholder="VD: SV123456" />
                </div>
                <div class="warning-box blue mt-4">
                  <Sparkles :size="20" class="shrink-0" />
                  <p>Hệ thống sẽ gửi yêu cầu đổi chéo đến sinh viên này. Lớp chỉ được đổi khi sinh viên đối tác đồng ý và cả hai thỏa mãn điều kiện thời khóa biểu.</p>
                </div>
              </div>
              <div class="modal-footer">
                <button class="btn-secondary" @click="closeModal">Hủy</button>
                <button class="btn-primary" :disabled="!swapTarget || confirmLoading" @click="handleAction">
                  {{ confirmLoading ? 'Đang xử lý...' : 'Gửi yêu cầu đổi lớp' }}
                </button>
              </div>
            </template>

          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<style scoped>
.registration-page {
  padding: 2rem;
  max-width: 1400px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  color: #0f172a;
}

.page-header { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; }
.eyebrow { display: flex; align-items: center; gap: .375rem; font-size: .7rem; font-weight: 700; text-transform: uppercase; letter-spacing: .08em; color: #2563eb; margin-bottom: .4rem; }
.page-title { font-size: 1.875rem; font-weight: 800; margin: 0 0 .25rem; letter-spacing: -.02em; }
.page-sub { font-size: .875rem; color: #64748b; margin: 0; }

/* AI Banner */
.ai-banner {
  display: flex; align-items: center; gap: 1rem;
  padding: 1rem 1.5rem; border-radius: 16px;
  backdrop-filter: blur(12px);
}
.banner-violet { background: rgba(124,58,237,.1); border: 1px solid rgba(124,58,237,.2); color: #6d28d9; }
.banner-content h3 { font-size: 1rem; font-weight: 700; margin: 0 0 .25rem; }
.banner-content p { font-size: .875rem; margin: 0; opacity: 0.9; }

/* Metrics */
.metrics-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 1rem; }
.metric-card {
  display: flex; align-items: center; gap: 1rem;
  background: rgba(255,255,255,.72); border: 1px solid rgba(255,255,255,.5);
  border-radius: 20px; padding: 1.25rem;
  box-shadow: 0 4px 20px rgba(15,23,42,.05);
  backdrop-filter: saturate(160%) blur(16px);
}
.metric-icon-wrap { width: 44px; height: 44px; border-radius: 12px; display: flex; align-items: center; justify-content: center; }
.metric-blue .metric-icon-wrap { background: rgba(37,99,235,.1); color: #2563eb; }
.metric-green .metric-icon-wrap { background: rgba(22,163,74,.1); color: #16a34a; }
.metric-amber .metric-icon-wrap { background: rgba(217,119,6,.1); color: #d97706; }
.metric-val { font-size: 1.5rem; font-weight: 800; line-height: 1; }
.metric-unit { font-size: .8rem; font-weight: 500; color: #94a3b8; margin-left: 4px; }
.metric-lbl { font-size: .78rem; font-weight: 600; color: #475569; margin-top: .25rem; }
.metric-hint { font-size: .7rem; color: #94a3b8; margin-top: .1rem; }

/* Toolbar */
.toolbar { display: flex; flex-wrap: wrap; justify-content: space-between; gap: 1rem; background: rgba(255,255,255,.72); border-radius: 16px; padding: .75rem; border: 1px solid rgba(255,255,255,.5); backdrop-filter: blur(12px); }
.search-box { display: flex; align-items: center; gap: .5rem; background: rgba(248,250,252,.8); border-radius: 10px; padding: 0 1rem; flex: 1; min-width: 250px; border: 1px solid rgba(148,163,184,.2); }
.search-box input { background: transparent; border: none; outline: none; padding: .6rem 0; width: 100%; font-size: .875rem; }
.filter-group { display: flex; align-items: center; gap: .75rem; }
.filter-select { padding: .5rem .75rem; border-radius: 10px; border: 1px solid rgba(148,163,184,.3); background: rgba(248,250,252,.8); font-size: .85rem; outline: none; cursor: pointer; }

/* Class Grid */
.class-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(340px, 1fr)); gap: 1.25rem; }
.class-card {
  background: rgba(255,255,255,.72); border: 1px solid rgba(255,255,255,.6);
  border-radius: 20px; overflow: hidden; display: flex; flex-direction: column;
  box-shadow: 0 4px 20px rgba(15,23,42,.06), inset 0 1px 0 rgba(255,255,255,.3);
  backdrop-filter: saturate(160%) blur(16px);
  transition: transform .2s, box-shadow .2s;
}
.class-card:hover { transform: translateY(-3px); box-shadow: 0 12px 36px rgba(15,23,42,.12); }
.card-Open { border-color: rgba(37,99,235,.2); }
.card-Full { border-color: rgba(220,38,38,.2); }
.card-Enrolled { border-color: rgba(22,163,74,.25); background: rgba(240,253,244,.5); }
.card-Waitlist { border-color: rgba(217,119,6,.25); background: rgba(255,251,235,.5); }

.card-header { padding: 1.25rem 1.25rem .75rem; border-bottom: 1px solid rgba(148,163,184,.15); }
.class-code { font-size: .8125rem; font-weight: 800; color: #2563eb; background: rgba(37,99,235,.1); padding: .2rem .5rem; border-radius: 6px; letter-spacing: .02em; }
.class-subject { font-size: 1.05rem; font-weight: 800; margin: .75rem 0 0; color: #0f172a; line-height: 1.3; }

/* Tags & Badges */
.ai-badge { display: inline-flex; align-items: center; gap: .2rem; padding: .15rem .4rem; border-radius: 6px; font-size: .65rem; font-weight: 700; text-transform: uppercase; }
.ai-badge.optimal { background: rgba(124,58,237,.1); color: #7c3aed; }
.ai-badge.hot { background: rgba(239,68,68,.1); color: #ef4444; }
.status-badge { font-size: .65rem; font-weight: 700; padding: .15rem .5rem; border-radius: 99px; text-transform: uppercase; }
.badge-blue { background: rgba(37,99,235,.15); color: #1d4ed8; }
.badge-red { background: rgba(220,38,38,.15); color: #b91c1c; }
.badge-green { background: rgba(22,163,74,.15); color: #15803d; }
.badge-amber { background: rgba(217,119,6,.15); color: #b45309; }
.badge-slate { background: rgba(148,163,184,.15); color: #475569; }

.card-body { padding: 1.25rem; flex: 1; display: flex; flex-direction: column; gap: .6rem; }
.info-line { display: flex; align-items: center; gap: .5rem; font-size: .8125rem; color: #475569; }
.info-icon { color: #94a3b8; }

.slot-container { background: rgba(248,250,252,.7); padding: .75rem; border-radius: 12px; border: 1px solid rgba(148,163,184,.15); }
.slot-track { height: 6px; background: rgba(148,163,184,.2); border-radius: 99px; overflow: hidden; }
.slot-fill { height: 100%; border-radius: 99px; transition: width .3s; }
.waitlist-info { margin-top: .5rem; display: flex; align-items: center; gap: .4rem; font-size: .75rem; color: #b45309; font-weight: 600; background: rgba(217,119,6,.1); padding: .5rem; border-radius: 8px; }

.card-footer { padding: 1rem 1.25rem; border-top: 1px solid rgba(148,163,184,.15); display: flex; gap: .75rem; background: rgba(248,250,252,.4); }

/* Buttons */
.btn-primary, .btn-amber, .btn-outline, .btn-red, .btn-secondary, .btn-ghost {
  display: inline-flex; align-items: center; justify-content: center; gap: .375rem;
  padding: .5rem 1rem; border-radius: 10px; font-size: .8125rem; font-weight: 700;
  cursor: pointer; border: none; transition: all .15s; outline: none;
}
.btn-primary { background: #2563eb; color: #fff; box-shadow: 0 4px 14px rgba(37,99,235,.25); }
.btn-primary:hover:not(:disabled) { background: #1d4ed8; transform: translateY(-1px); }
.btn-primary:disabled { opacity: .6; cursor: not-allowed; }
.btn-amber { background: #f59e0b; color: #fff; box-shadow: 0 4px 14px rgba(245,158,11,.25); }
.btn-amber:hover { background: #d97706; transform: translateY(-1px); }
.btn-red { background: #ef4444; color: #fff; box-shadow: 0 4px 14px rgba(239,68,68,.25); }
.btn-red:hover { background: #dc2626; transform: translateY(-1px); }
.btn-outline { background: rgba(255,255,255,.8); color: #374151; border: 1px solid rgba(148,163,184,.3); }
.btn-outline:hover { border-color: #ef4444; color: #ef4444; }
.btn-secondary { background: rgba(248,250,252,.8); color: #475569; border: 1px solid rgba(148,163,184,.3); }
.btn-secondary:hover { color: #0f172a; border-color: #94a3b8; }
.btn-ghost { background: transparent; color: #64748b; padding: .5rem; }
.btn-ghost:hover { background: rgba(37,99,235,.1); color: #2563eb; }

/* Modals */
.modal-overlay { position: fixed; inset: 0; z-index: 1000; background: rgba(15,23,42,.4); backdrop-filter: blur(6px); display: flex; align-items: center; justify-content: center; padding: 1rem; }
.modal-content { background: rgba(255,255,255,.95); backdrop-filter: saturate(180%) blur(24px); width: 100%; max-width: 480px; border-radius: 24px; box-shadow: 0 24px 80px rgba(2,6,23,.32); overflow: hidden; border: 1px solid rgba(255,255,255,.5); }
.modal-header { padding: 1.25rem 1.5rem; border-bottom: 1px solid rgba(148,163,184,.15); display: flex; justify-content: space-between; align-items: center; }
.modal-header h3 { margin: 0; font-size: 1.1rem; font-weight: 800; color: #0f172a; }
.close-btn-sm { background: transparent; border: none; color: #94a3b8; cursor: pointer; display: flex; transition: color .15s; }
.close-btn-sm:hover { color: #ef4444; }
.modal-body { padding: 1.5rem; display: flex; flex-direction: column; gap: 1rem; font-size: .875rem; color: #374151; }

.checklist { background: rgba(248,250,252,.7); border: 1px solid rgba(148,163,184,.2); border-radius: 12px; padding: 1rem; display: flex; flex-direction: column; gap: .75rem; }
.check-item { display: flex; align-items: center; gap: .5rem; font-weight: 600; transition: color .3s; }
.check-item.pending { color: #94a3b8; }
.check-item.checked { color: #16a34a; }

.warning-box { display: flex; gap: .75rem; padding: 1rem; border-radius: 12px; font-weight: 500; font-size: .8125rem; }
.warning-box.amber { background: rgba(245,158,11,.1); color: #b45309; border: 1px solid rgba(245,158,11,.2); }
.warning-box.red { background: rgba(239,68,68,.1); color: #b91c1c; border: 1px solid rgba(239,68,68,.2); }
.warning-box.blue { background: rgba(37,99,235,.1); color: #1d4ed8; border: 1px solid rgba(37,99,235,.2); }

.form-group label { display: block; font-size: .8125rem; font-weight: 700; margin-bottom: .4rem; }
.input-glass { width: 100%; padding: .6rem 1rem; border-radius: 10px; border: 1px solid rgba(148,163,184,.4); background: rgba(255,255,255,.6); font-size: .875rem; outline: none; transition: border-color .2s; }
.input-glass:focus { border-color: #2563eb; }

.modal-footer { padding: 1.25rem 1.5rem; border-top: 1px solid rgba(148,163,184,.15); display: flex; justify-content: flex-end; gap: .75rem; background: rgba(248,250,252,.5); }

.empty-state { grid-column: 1 / -1; text-align: center; padding: 4rem; color: #64748b; font-style: italic; background: rgba(255,255,255,.5); border-radius: 20px; border: 1px dashed rgba(148,163,184,.4); }

.modal-enter-active, .modal-leave-active { transition: all .3s cubic-bezier(0.16,1,.3,1); }
.modal-enter-from, .modal-leave-to { opacity: 0; transform: scale(0.95); }

@media (max-width: 768px) {
  .registration-page { padding: 1rem; }
  .toolbar { flex-direction: column; }
  .search-box { width: 100%; }
}
</style>
