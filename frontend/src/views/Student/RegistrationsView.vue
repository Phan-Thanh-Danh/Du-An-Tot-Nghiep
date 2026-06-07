<script setup>
import { ref, computed } from 'vue'
import { useBodyScrollLock } from '@/composables/useBodyScrollLock'
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
const isModalOpen = computed(() => !!modalMode.value)
useBodyScrollLock(isModalOpen)
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
  if (p >= 1) return 'slot-fill--full'
  if (p >= 0.8) return 'slot-fill--warn'
  return 'slot-fill--ok'
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
            {{ m.value }}<span v-if="m.max" class="text-muted text-base font-semibold">/{{ m.max }}</span>
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
        <Search :size="16" class="text-placeholder" />
        <input v-model="searchQuery" type="text" placeholder="Tìm tên môn, mã lớp..." />
      </div>
      <div class="filter-group">
        <Filter :size="16" class="text-placeholder" />
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
              <span class="text-muted">Sĩ số (Thời gian thực)</span>
              <span class="text-heading">{{ cls.slots }}/{{ cls.maxSlots }}</span>
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
  color: var(--text-body);
}

.page-header { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; }
.eyebrow { display: flex; align-items: center; gap: .375rem; font-size: .7rem; font-weight: 700; text-transform: uppercase; letter-spacing: .08em; color: var(--text-link); margin-bottom: .4rem; }
.page-title { font-size: 1.875rem; font-weight: 800; margin: 0 0 .25rem; letter-spacing: -.02em; color: var(--text-heading); }
.page-sub { font-size: .875rem; color: var(--text-muted); margin: 0; }

/* AI Banner */
.ai-banner {
  display: flex; align-items: center; gap: 1rem;
  padding: 1rem 1.5rem; border-radius: 16px;
  backdrop-filter: blur(12px);
}
.banner-violet { background: var(--color-info-bg); border: 1px solid color-mix(in srgb, var(--color-info-text) 22%, transparent); color: var(--color-info-text); }
.banner-content h3 { font-size: 1rem; font-weight: 700; margin: 0 0 .25rem; }
.banner-content p { font-size: .875rem; margin: 0; opacity: 0.9; }

/* Metrics */
.metrics-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 1rem; }
.metric-card {
  display: flex; align-items: center; gap: 1rem;
  background: var(--surface-card); border: 1px solid var(--border-card);
  border-radius: 20px; padding: 1.25rem;
  box-shadow: var(--lg-shadow-sm);
  backdrop-filter: saturate(160%) blur(16px);
}
.metric-icon-wrap { width: 44px; height: 44px; border-radius: 12px; display: flex; align-items: center; justify-content: center; }
.metric-blue .metric-icon-wrap { background: var(--color-info-bg); color: var(--color-info-text); }
.metric-green .metric-icon-wrap { background: var(--color-success-bg); color: var(--color-success-text); }
.metric-amber .metric-icon-wrap { background: var(--color-warning-bg); color: var(--color-warning-text); }
.metric-val { font-size: 1.5rem; font-weight: 800; line-height: 1; color: var(--text-heading); }
.metric-unit { font-size: .8rem; font-weight: 500; color: var(--text-muted); margin-left: 4px; }
.metric-lbl { font-size: .78rem; font-weight: 600; color: var(--text-label); margin-top: .25rem; }
.metric-hint { font-size: .7rem; color: var(--text-muted); margin-top: .1rem; }

/* Toolbar */
.toolbar { display: flex; flex-wrap: wrap; justify-content: space-between; gap: 1rem; background: var(--surface-card); border-radius: 16px; padding: .75rem; border: 1px solid var(--border-card); backdrop-filter: blur(12px); }
.search-box { display: flex; align-items: center; gap: .5rem; background: var(--surface-input); border-radius: 10px; padding: 0 1rem; flex: 1; min-width: 250px; border: 1px solid var(--border-input); }
.search-box input { background: transparent; border: none; outline: none; padding: .6rem 0; width: 100%; font-size: .875rem; color: var(--text-body); }
.filter-group { display: flex; align-items: center; gap: .75rem; }
.filter-select { padding: .5rem .75rem; border-radius: 10px; border: 1px solid var(--border-input); background: var(--surface-input); color: var(--text-label); font-size: .85rem; outline: none; cursor: pointer; }

/* Class Grid */
.class-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(340px, 1fr)); gap: 1.25rem; }
.class-card {
  background: var(--surface-card); border: 1px solid var(--border-card);
  border-radius: 20px; overflow: hidden; display: flex; flex-direction: column;
  box-shadow: var(--lg-shadow-sm), inset 0 1px 0 var(--glass-highlight);
  backdrop-filter: saturate(160%) blur(16px);
  transition: transform .2s, box-shadow .2s;
}
.class-card:hover { transform: translateY(-2px); box-shadow: var(--lg-shadow-md); }
.card-Open { border-color: color-mix(in srgb, var(--color-info-text) 24%, var(--border-card)); }
.card-Full { border-color: color-mix(in srgb, var(--color-danger-text) 24%, var(--border-card)); }
.card-Enrolled { border-color: color-mix(in srgb, var(--color-success-text) 28%, var(--border-card)); background: var(--surface-card); }
.card-Waitlist { border-color: color-mix(in srgb, var(--color-warning-text) 28%, var(--border-card)); background: var(--surface-card); }

.card-header { padding: 1.25rem 1.25rem .75rem; border-bottom: 1px solid var(--border-default); }
.class-code { font-size: .8125rem; font-weight: 800; color: var(--text-link); background: var(--color-info-bg); padding: .2rem .5rem; border-radius: 6px; letter-spacing: .02em; }
.class-subject { font-size: 1.05rem; font-weight: 800; margin: .75rem 0 0; color: var(--text-heading); line-height: 1.3; }

/* Tags & Badges */
.ai-badge { display: inline-flex; align-items: center; gap: .2rem; padding: .15rem .4rem; border-radius: 6px; font-size: .65rem; font-weight: 700; text-transform: uppercase; }
.ai-badge.optimal { background: var(--color-info-bg); color: var(--color-info-text); }
.ai-badge.hot { background: var(--color-danger-bg); color: var(--color-danger-text); }
.status-badge { font-size: .65rem; font-weight: 700; padding: .15rem .5rem; border-radius: 99px; text-transform: uppercase; }
.badge-blue { background: var(--color-info-bg); color: var(--color-info-text); }
.badge-red { background: var(--color-danger-bg); color: var(--color-danger-text); }
.badge-green { background: var(--color-success-bg); color: var(--color-success-text); }
.badge-amber { background: var(--color-warning-bg); color: var(--color-warning-text); }
.badge-slate { background: var(--surface-solid); color: var(--text-muted); }

.card-body { padding: 1.25rem; flex: 1; display: flex; flex-direction: column; gap: .6rem; }
.info-line { display: flex; align-items: center; gap: .5rem; font-size: .8125rem; color: var(--text-muted); }
.info-icon { color: var(--text-placeholder); }

.slot-container { background: var(--surface-solid); padding: .75rem; border-radius: 12px; border: 1px solid var(--border-default); }
.slot-track { height: 6px; background: var(--border-default); border-radius: 99px; overflow: hidden; }
.slot-fill { height: 100%; border-radius: 99px; transition: width .3s; }
.slot-fill--ok { background: var(--text-link); }
.slot-fill--warn { background: var(--color-warning-text); }
.slot-fill--full { background: var(--color-danger-text); }
.waitlist-info { margin-top: .5rem; display: flex; align-items: center; gap: .4rem; font-size: .75rem; color: var(--color-warning-text); font-weight: 600; background: var(--color-warning-bg); padding: .5rem; border-radius: 8px; }

.card-footer { padding: 1rem 1.25rem; border-top: 1px solid var(--border-default); display: flex; gap: .75rem; background: var(--surface-solid); }

/* Buttons */
.btn-primary, .btn-amber, .btn-outline, .btn-red, .btn-secondary, .btn-ghost {
  display: inline-flex; align-items: center; justify-content: center; gap: .375rem;
  padding: .5rem 1rem; border-radius: 10px; font-size: .8125rem; font-weight: 700;
  cursor: pointer; border: none; transition: all .15s; outline: none;
}
.btn-primary { background: var(--text-link); color: var(--text-inverse); box-shadow: 0 4px 14px color-mix(in srgb, var(--text-link) 25%, transparent); }
.btn-primary:hover:not(:disabled) { background: var(--lg-primary-dark); transform: translateY(-1px); }
.btn-primary:disabled { opacity: .6; cursor: not-allowed; }
.btn-amber { background: var(--color-warning-text); color: var(--text-inverse); box-shadow: 0 4px 14px color-mix(in srgb, var(--color-warning-text) 25%, transparent); }
.btn-amber:hover { background: color-mix(in srgb, var(--color-warning-text) 80%, #000); transform: translateY(-1px); }
.btn-red { background: var(--color-danger-text); color: var(--text-inverse); box-shadow: 0 4px 14px color-mix(in srgb, var(--color-danger-text) 25%, transparent); }
.btn-red:hover { background: color-mix(in srgb, var(--color-danger-text) 85%, #000); transform: translateY(-1px); }
.btn-outline { background: var(--surface-card); color: var(--text-body); border: 1px solid var(--border-input); }
.btn-outline:hover { border-color: var(--color-danger-text); color: var(--color-danger-text); }
.btn-secondary { background: var(--surface-solid); color: var(--text-label); border: 1px solid var(--border-input); }
.btn-secondary:hover { color: var(--text-heading); border-color: var(--text-placeholder); }
.btn-ghost { background: transparent; color: var(--text-muted); padding: .5rem; }
.btn-ghost:hover { background: var(--accent-primary-soft); color: var(--text-link); }

/* Modals */
.modal-overlay { position: fixed; inset: 0; z-index: 9998; background: color-mix(in srgb, var(--text-heading) 40%, transparent); backdrop-filter: blur(6px); display: flex; align-items: center; justify-content: center; padding: 1rem; }
.modal-content { position: relative; z-index: 9999; background: var(--surface-modal); backdrop-filter: saturate(180%) blur(24px); width: 100%; max-width: 480px; border-radius: 24px; box-shadow: 0 24px 80px color-mix(in srgb, var(--text-heading) 32%, transparent); overflow: hidden; border: 1px solid var(--border-card); }
.modal-header { padding: 1.25rem 1.5rem; border-bottom: 1px solid var(--border-default); display: flex; justify-content: space-between; align-items: center; }
.modal-header h3 { margin: 0; font-size: 1.1rem; font-weight: 800; color: var(--text-heading); }
.close-btn-sm { background: transparent; border: none; color: var(--text-placeholder); cursor: pointer; display: flex; transition: color .15s; }
.close-btn-sm:hover { color: var(--color-danger-text); }
.modal-body { padding: 1.5rem; display: flex; flex-direction: column; gap: 1rem; font-size: .875rem; color: var(--text-body); }

.checklist { background: var(--surface-solid); border: 1px solid var(--border-default); border-radius: 12px; padding: 1rem; display: flex; flex-direction: column; gap: .75rem; }
.check-item { display: flex; align-items: center; gap: .5rem; font-weight: 600; transition: color .3s; }
.check-item.pending { color: var(--text-placeholder); }
.check-item.checked { color: var(--color-success-text); }

.warning-box { display: flex; gap: .75rem; padding: 1rem; border-radius: 12px; font-weight: 500; font-size: .8125rem; }
.warning-box.amber { background: var(--color-warning-bg); color: var(--color-warning-text); border: 1px solid color-mix(in srgb, var(--color-warning-text) 20%, transparent); }
.warning-box.red { background: var(--color-danger-bg); color: var(--color-danger-text); border: 1px solid color-mix(in srgb, var(--color-danger-text) 20%, transparent); }
.warning-box.blue { background: var(--color-info-bg); color: var(--color-info-text); border: 1px solid color-mix(in srgb, var(--color-info-text) 20%, transparent); }

.form-group label { display: block; font-size: .8125rem; font-weight: 700; margin-bottom: .4rem; color: var(--text-label); }
.input-glass { width: 100%; padding: .6rem 1rem; border-radius: 10px; border: 1px solid var(--border-input); background: var(--surface-input); font-size: .875rem; outline: none; transition: border-color .2s; color: var(--text-body); }
.input-glass:focus { border-color: var(--border-input-focus); }

.modal-footer { padding: 1.25rem 1.5rem; border-top: 1px solid var(--border-default); display: flex; justify-content: flex-end; gap: .75rem; background: var(--surface-solid); }

.empty-state { grid-column: 1 / -1; text-align: center; padding: 4rem; color: var(--text-muted); font-style: italic; background: var(--surface-card); border-radius: 20px; border: 1px dashed var(--border-default); }

.modal-enter-active, .modal-leave-active { transition: all .3s cubic-bezier(0.16,1,.3,1); }
.modal-enter-from, .modal-leave-to { opacity: 0; transform: scale(0.95); }

@media (max-width: 768px) {
  .registration-page { padding: 1rem; }
  .toolbar { flex-direction: column; }
  .search-box { width: 100%; }
}
</style>
