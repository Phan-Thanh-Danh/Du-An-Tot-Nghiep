<script setup>
import { ref, computed } from 'vue'
import {
  UserCheck, AlertTriangle, UserX, Clock,
  FileText, UploadCloud, X, CheckCircle2,
  Filter, Sparkles, AlertCircle, ChevronDown,
  Info, Eye, FileSignature
} from 'lucide-vue-next'

// Mock Data
const subjects = ['Tất cả', 'Cấu trúc dữ liệu & Giải thuật', 'Toán rời rạc', 'Lập trình Web']
const filters = {
  status: ['Tất cả', 'Present', 'Absent', 'Late', 'Excused', 'Unconfirmed']
}

const mockAttendanceData = [
  { id: 1, date: new Date(2026, 4, 15, 7, 30), subject: 'Cấu trúc dữ liệu & Giải thuật', teacher: 'TS. Nguyễn Minh Khoa', status: 'Present', notes: 'Đúng giờ' },
  { id: 2, date: new Date(2026, 4, 17, 13, 30), subject: 'Toán rời rạc', teacher: 'ThS. Trần Thu Hà', status: 'Absent', notes: 'Không phép' },
  { id: 3, date: new Date(2026, 4, 20, 8, 0), subject: 'Lập trình Web', teacher: 'KS. Lê Văn Tâm', status: 'Late', notes: 'Đi trễ 15 phút' },
  { id: 4, date: new Date(2026, 4, 22, 7, 30), subject: 'Cấu trúc dữ liệu & Giải thuật', teacher: 'TS. Nguyễn Minh Khoa', status: 'Excused', notes: 'Có đơn xin phép bệnh' },
  { id: 5, date: new Date(2026, 4, 24, 13, 30), subject: 'Toán rời rạc', teacher: 'ThS. Trần Thu Hà', status: 'Unconfirmed', notes: 'Giảng viên chưa chốt' }
]

const quotas = [
  { subject: 'Cấu trúc dữ liệu & Giải thuật', absent: 2, max: 6 },
  { subject: 'Toán rời rạc', absent: 4, max: 5 }, // 80% warning
  { subject: 'Lập trình Web', absent: 0, max: 7 }
]

const statusConfig = {
  Present: { label: 'Có mặt', cls: 'badge-green', icon: CheckCircle2 },
  Absent: { label: 'Vắng mặt', cls: 'badge-red', icon: UserX },
  Late: { label: 'Đi muộn', cls: 'badge-amber', icon: Clock },
  Excused: { label: 'Có phép', cls: 'badge-blue', icon: FileText },
  Unconfirmed: { label: 'Chưa xác nhận', cls: 'badge-slate', icon: Info }
}

const metrics = [
  { label: 'Tỷ lệ có mặt', value: '85', unit: '%', icon: UserCheck, tone: 'green', hint: 'Trên mức an toàn' },
  { label: 'Tổng vắng', value: '2', unit: 'buổi', icon: UserX, tone: 'red', hint: '1 không phép, 1 có phép' },
  { label: 'Đi muộn', value: '1', unit: 'lần', icon: Clock, tone: 'amber', hint: 'Cần chú ý đi học đúng giờ' },
  { label: 'Chưa xác nhận', value: '1', unit: 'buổi', icon: AlertCircle, tone: 'slate', hint: 'Đang chờ giảng viên cập nhật' },
]

// State
const selectedSubject = ref('Tất cả')
const selectedStatus = ref('Tất cả')
const drawerOpen = ref(false)
const modalOpen = ref(false)
const selectedSession = ref(null)

const excuseForm = ref({
  reason: '',
  file: null
})

// Computed
const filteredData = computed(() => {
  return mockAttendanceData.filter(item => {
    const matchSubject = selectedSubject.value === 'Tất cả' || item.subject === selectedSubject.value
    const matchStatus = selectedStatus.value === 'Tất cả' || item.status === selectedStatus.value
    return matchSubject && matchStatus
  })
})

const getAiRisk = computed(() => {
  const highRisk = quotas.find(q => (q.absent / q.max) >= 0.8)
  if (highRisk) {
    return {
      risk: 'high',
      title: 'Cảnh báo rủi ro cao từ AI',
      message: `Môn ${highRisk.subject} đã vắng ${highRisk.absent}/${highRisk.max} buổi. Nếu vắng thêm ${highRisk.max - highRisk.absent} buổi nữa sẽ bị cấm thi. Hãy chú ý đi học đầy đủ!`
    }
  }
  const mediumRisk = quotas.find(q => (q.absent / q.max) >= 0.6)
  if (mediumRisk) {
    return {
      risk: 'medium',
      title: 'Dự đoán AI: Cần lưu ý',
      message: `Môn ${mediumRisk.subject} đã vắng ${mediumRisk.absent}/${mediumRisk.max} buổi. Bạn đang ở ngưỡng 60% quỹ vắng.`
    }
  }
  return {
    risk: 'low',
    title: 'Dự đoán AI: An toàn',
    message: 'Thói quen đi học của bạn đang rất tốt, không có nguy cơ vi phạm quỹ vắng ở các môn học hiện tại.'
  }
})

// Methods
const fmtDateTime = (date) => {
  return new Intl.DateTimeFormat('vi-VN', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit'
  }).format(date)
}

const getQuotaColor = (absent, max) => {
  const percent = absent / max
  if (percent >= 0.8) return 'bg-red-500'
  if (percent >= 0.6) return 'bg-amber-500'
  return 'bg-blue-500'
}

const openDrawer = (session) => {
  selectedSession.value = session
  drawerOpen.value = true
}

const openExcuseModal = (session) => {
  selectedSession.value = session
  excuseForm.value.reason = ''
  excuseForm.value.file = null
  drawerOpen.value = false
  modalOpen.value = true
}

const closeDrawer = () => drawerOpen.value = false
const closeModal = () => modalOpen.value = false

const handleFileChange = (e) => {
  excuseForm.value.file = e.target.files[0]
}

const submitExcuse = () => {
  // Mock submission
  if (!excuseForm.value.reason) return
  alert(`Đã gửi giải trình cho môn ${selectedSession.value.subject}`)
  modalOpen.value = false
}

</script>

<template>
  <div class="attendance-page">
    <!-- Header -->
    <div class="page-header">
      <div>
        <div class="eyebrow"><UserCheck :size="15"/>Quản lý học vụ</div>
        <h1 class="page-title">Điểm danh</h1>
        <p class="page-sub">Theo dõi lịch sử chuyên cần, nộp đơn giải trình và nhận cảnh báo từ AI.</p>
      </div>
    </div>

    <!-- AI Risk Banner -->
    <div class="ai-banner" :class="`banner-${getAiRisk.risk}`">
      <div class="banner-icon">
        <Sparkles v-if="getAiRisk.risk === 'low'" :size="24" />
        <AlertTriangle v-else :size="24" />
      </div>
      <div class="banner-content">
        <h3>{{ getAiRisk.title }}</h3>
        <p>{{ getAiRisk.message }}</p>
      </div>
    </div>

    <!-- Quota Bars -->
    <div class="quota-grid">
      <div v-for="q in quotas" :key="q.subject" class="quota-card">
        <div class="quota-header">
          <span class="quota-subject">{{ q.subject }}</span>
          <span class="quota-val">{{ q.absent }}/{{ q.max }} vắng</span>
        </div>
        <div class="quota-track">
          <div class="quota-fill" :class="getQuotaColor(q.absent, q.max)" :style="{ width: `${Math.min((q.absent / q.max) * 100, 100)}%` }"></div>
        </div>
      </div>
    </div>

    <!-- Metrics Grid -->
    <div class="metrics-grid">
      <div v-for="m in metrics" :key="m.label" class="metric-card" :class="`metric-${m.tone}`">
        <div class="metric-icon-wrap"><component :is="m.icon" :size="20"/></div>
        <div class="metric-body">
          <div class="metric-val">{{ m.value }}<span class="metric-unit">{{ m.unit }}</span></div>
          <div class="metric-lbl">{{ m.label }}</div>
        </div>
      </div>
    </div>

    <!-- Filters -->
    <div class="filter-bar">
      <div class="filter-group">
        <Filter :size="16" class="text-slate-400" />
        <select v-model="selectedSubject" class="filter-select">
          <option v-for="s in subjects" :key="s" :value="s">{{ s }}</option>
        </select>
        <select v-model="selectedStatus" class="filter-select">
          <option v-for="st in filters.status" :key="st" :value="st">{{ statusConfig[st]?.label || st }}</option>
        </select>
      </div>
    </div>

    <!-- Table -->
    <div class="table-container">
      <table class="data-table">
        <thead>
          <tr>
            <th>Thời gian</th>
            <th>Môn học</th>
            <th>Giảng viên</th>
            <th>Trạng thái</th>
            <th class="text-right">Thao tác</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in filteredData" :key="item.id">
            <td class="font-medium">{{ fmtDateTime(item.date) }}</td>
            <td>{{ item.subject }}</td>
            <td class="text-slate-500">{{ item.teacher }}</td>
            <td>
              <span class="status-badge" :class="statusConfig[item.status].cls">
                <component :is="statusConfig[item.status].icon" :size="12" />
                {{ statusConfig[item.status].label }}
              </span>
            </td>
            <td class="text-right">
              <button class="btn-ghost" @click="openDrawer(item)">
                <Eye :size="16" /> Chi tiết
              </button>
            </td>
          </tr>
          <tr v-if="filteredData.length === 0">
            <td colspan="5" class="empty-state">
              Không tìm thấy dữ liệu điểm danh phù hợp.
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Drawer for Details -->
    <Teleport to="body">
      <Transition name="fade">
        <div v-if="drawerOpen" class="drawer-overlay" @click.self="closeDrawer">
          <Transition name="slide-right">
          <div v-if="selectedSession" class="drawer">
            <div class="drawer-header">
              <div>
                <span class="status-badge lg" :class="statusConfig[selectedSession.status].cls">
                  <component :is="statusConfig[selectedSession.status].icon" :size="14" />
                  {{ statusConfig[selectedSession.status].label }}
                </span>
                <h2 class="drawer-title">{{ selectedSession.subject }}</h2>
              </div>
              <button class="close-btn" @click="closeDrawer"><X :size="20"/></button>
            </div>
            <div class="drawer-body">
              <div class="info-row"><Clock :size="16" class="info-icon"/><span>{{ fmtDateTime(selectedSession.date) }}</span></div>
              <div class="info-row"><UserCheck :size="16" class="info-icon"/><span>{{ selectedSession.teacher }}</span></div>
              
              <div class="info-box">
                <h4>Ghi chú từ hệ thống/giảng viên:</h4>
                <p>{{ selectedSession.notes || 'Không có ghi chú.' }}</p>
              </div>

              <div class="action-box" v-if="['Absent', 'Late', 'Unconfirmed'].includes(selectedSession.status)">
                <FileSignature :size="24" class="action-icon" />
                <div>
                  <h4>Cần giải trình?</h4>
                  <p>Nếu bạn có lý do chính đáng, hãy nộp minh chứng để được xem xét chuyển trạng thái thành "Có phép".</p>
                </div>
                <button class="btn-primary mt-3 w-full justify-center" @click="openExcuseModal(selectedSession)">
                  Nộp đơn giải trình
                </button>
              </div>
            </div>
          </div>
        </Transition>
      </div>
    </Transition>
    </Teleport>

    <!-- Modal for Excuses -->
    <Teleport to="body">
      <Transition name="modal">
        <div v-if="modalOpen" class="modal-overlay" @click.self="closeModal">
        <div class="modal-content">
          <div class="modal-header">
            <h3>Giải trình vắng mặt</h3>
            <button class="close-btn-sm" @click="closeModal"><X :size="18"/></button>
          </div>
          <div class="modal-body">
            <div class="form-group">
              <label>Môn học</label>
              <input type="text" :value="selectedSession?.subject" disabled class="input-disabled" />
            </div>
            <div class="form-group">
              <label>Lý do giải trình <span class="text-red-500">*</span></label>
              <textarea v-model="excuseForm.reason" rows="4" class="input-glass" placeholder="Nhập chi tiết lý do vắng mặt..."></textarea>
            </div>
            <div class="form-group">
              <label>Minh chứng đính kèm (Hình ảnh, PDF)</label>
              <label class="file-upload">
                <input type="file" class="hidden" @change="handleFileChange" accept="image/*,.pdf" />
                <div class="upload-area">
                  <UploadCloud :size="24" class="text-slate-400 mb-2" />
                  <span v-if="!excuseForm.file">Nhấn để chọn file tải lên</span>
                  <span v-else class="text-blue-600 font-medium">{{ excuseForm.file.name }}</span>
                </div>
              </label>
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn-secondary" @click="closeModal">Hủy</button>
            <button class="btn-primary" :disabled="!excuseForm.reason" @click="submitExcuse">Gửi yêu cầu</button>
          </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<style scoped>
.attendance-page {
  padding: 2rem;
  max-width: 1400px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  color: #0f172a;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 1rem;
}
.eyebrow { display: flex; align-items: center; gap: .375rem; font-size: .7rem; font-weight: 700; text-transform: uppercase; letter-spacing: .08em; color: #2563eb; margin-bottom: .4rem; }
.page-title { font-size: 1.875rem; font-weight: 800; margin: 0 0 .25rem; letter-spacing: -.02em; }
.page-sub { font-size: .875rem; color: #64748b; margin: 0; }

/* AI Banner */
.ai-banner {
  display: flex; align-items: center; gap: 1rem;
  padding: 1rem 1.5rem; border-radius: 16px;
  backdrop-filter: blur(12px);
}
.ai-banner.banner-low { background: rgba(124,58,237,.1); border: 1px solid rgba(124,58,237,.2); color: #6d28d9; }
.ai-banner.banner-medium { background: rgba(217,119,6,.1); border: 1px solid rgba(217,119,6,.2); color: #b45309; }
.ai-banner.banner-high { background: rgba(220,38,38,.1); border: 1px solid rgba(220,38,38,.2); color: #b91c1c; }
.banner-content h3 { font-size: 1rem; font-weight: 700; margin: 0 0 .25rem; }
.banner-content p { font-size: .875rem; margin: 0; opacity: 0.9; }

/* Quota Grid */
.quota-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 1rem; }
.quota-card {
  background: rgba(255,255,255,.72); border: 1px solid rgba(255,255,255,.5);
  border-radius: 16px; padding: 1rem;
  box-shadow: 0 4px 20px rgba(15,23,42,.05);
}
.quota-header { display: flex; justify-content: space-between; font-size: .8125rem; font-weight: 600; margin-bottom: .5rem; }
.quota-val { color: #64748b; }
.quota-track { height: 6px; background: rgba(148,163,184,.2); border-radius: 99px; overflow: hidden; }
.quota-fill { height: 100%; border-radius: 99px; transition: width .3s ease; }

/* Metrics */
.metrics-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 1rem; }
.metric-card {
  display: flex; align-items: center; gap: 1rem;
  background: rgba(255,255,255,.72); border: 1px solid rgba(255,255,255,.5);
  border-radius: 20px; padding: 1.25rem;
  box-shadow: 0 4px 20px rgba(15,23,42,.05);
}
.metric-icon-wrap { width: 44px; height: 44px; border-radius: 12px; display: flex; align-items: center; justify-content: center; }
.metric-green .metric-icon-wrap { background: rgba(22,163,74,.1); color: #16a34a; }
.metric-red .metric-icon-wrap { background: rgba(220,38,38,.1); color: #dc2626; }
.metric-amber .metric-icon-wrap { background: rgba(217,119,6,.1); color: #d97706; }
.metric-slate .metric-icon-wrap { background: rgba(100,116,139,.1); color: #475569; }
.metric-val { font-size: 1.5rem; font-weight: 800; line-height: 1; }
.metric-unit { font-size: .8rem; font-weight: 500; color: #94a3b8; margin-left: 4px; }
.metric-lbl { font-size: .78rem; font-weight: 600; color: #475569; margin-top: .25rem; }

/* Filters */
.filter-bar { display: flex; align-items: center; background: rgba(255,255,255,.72); border-radius: 12px; padding: .5rem; border: 1px solid rgba(255,255,255,.5); }
.filter-group { display: flex; align-items: center; gap: .75rem; padding-left: .5rem; }
.filter-select { padding: .4rem .75rem; border-radius: 8px; border: 1px solid rgba(148,163,184,.3); background: transparent; font-size: .85rem; outline: none; cursor: pointer; }
.filter-select:focus { border-color: #2563eb; }

/* Table */
.table-container {
  background: rgba(255,255,255,.88); border: 1px solid rgba(148,163,184,.28);
  border-radius: 20px; overflow-x: auto; box-shadow: 0 4px 20px rgba(15,23,42,.03);
}
.data-table { width: 100%; border-collapse: collapse; font-size: .875rem; }
.data-table th { text-align: left; padding: 1rem; background: rgba(248,250,252,.8); font-weight: 600; color: #475569; border-bottom: 1px solid rgba(148,163,184,.2); }
.data-table td { padding: 1rem; border-bottom: 1px solid rgba(148,163,184,.15); }
.data-table tbody tr:hover { background: rgba(248,250,252,.5); }
.empty-state { text-align: center; padding: 3rem !important; color: #64748b; font-style: italic; }

.status-badge { display: inline-flex; align-items: center; gap: .3rem; padding: .25rem .6rem; border-radius: 9999px; font-size: .75rem; font-weight: 600; }
.status-badge.lg { padding: .3rem .8rem; font-size: .8125rem; margin-bottom: .5rem; }
.badge-green { background: rgba(22,163,74,.1); color: #15803d; }
.badge-red { background: rgba(220,38,38,.1); color: #b91c1c; }
.badge-amber { background: rgba(217,119,6,.1); color: #b45309; }
.badge-blue { background: rgba(37,99,235,.1); color: #1d4ed8; }
.badge-slate { background: rgba(148,163,184,.15); color: #475569; }

/* Buttons */
.btn-ghost { background: transparent; border: none; color: #2563eb; font-size: .8125rem; font-weight: 600; cursor: pointer; display: inline-flex; align-items: center; gap: .3rem; padding: .4rem .75rem; border-radius: 8px; transition: background .2s; }
.btn-ghost:hover { background: rgba(37,99,235,.1); }
.btn-primary { background: #2563eb; color: #fff; border: none; padding: .6rem 1.25rem; border-radius: 10px; font-size: .8125rem; font-weight: 600; cursor: pointer; display: inline-flex; align-items: center; gap: .4rem; transition: background .2s; }
.btn-primary:hover:not(:disabled) { background: #1d4ed8; }
.btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }
.btn-secondary { background: rgba(248,250,252,.9); color: #374151; border: 1px solid rgba(148,163,184,.3); padding: .6rem 1.25rem; border-radius: 10px; font-size: .8125rem; font-weight: 600; cursor: pointer; transition: all .2s; }
.btn-secondary:hover { border-color: #2563eb; color: #2563eb; }

/* Drawer Overlay */
.drawer-overlay { position: fixed; inset: 0; z-index: 999; background: rgba(15,23,42,.3); backdrop-filter: blur(4px); display: flex; justify-content: flex-end; }
.drawer { width: 420px; max-width: 95vw; background: rgba(255,255,255,.92); backdrop-filter: saturate(180%) blur(24px); box-shadow: -8px 0 40px rgba(15,23,42,.18); display: flex; flex-direction: column; height: 100%; }
.drawer-header { padding: 1.5rem; border-bottom: 1px solid rgba(148,163,184,.15); display: flex; justify-content: space-between; align-items: flex-start; }
.drawer-title { font-size: 1.1rem; font-weight: 800; margin: 0; line-height: 1.3; }
.close-btn, .close-btn-sm { width: 36px; height: 36px; border-radius: 10px; border: 1px solid rgba(148,163,184,.3); background: rgba(248,250,252,.8); cursor: pointer; display: flex; align-items: center; justify-content: center; }
.close-btn:hover, .close-btn-sm:hover { color: #dc2626; border-color: #dc2626; }
.drawer-body { flex: 1; padding: 1.5rem; display: flex; flex-direction: column; gap: 1rem; overflow-y: auto; }
.info-row { display: flex; align-items: center; gap: .75rem; font-size: .875rem; color: #374151; }
.info-icon { color: #94a3b8; flex-shrink: 0; }
.info-box { background: rgba(248,250,252,.7); padding: 1rem; border-radius: 12px; border: 1px solid rgba(148,163,184,.2); }
.info-box h4 { margin: 0 0 .5rem; font-size: .8125rem; color: #64748b; }
.info-box p { margin: 0; font-size: .875rem; }

.action-box { background: rgba(37,99,235,.05); border: 1px dashed rgba(37,99,235,.3); padding: 1.25rem; border-radius: 12px; text-align: center; margin-top: 1rem; }
.action-icon { color: #2563eb; margin: 0 auto .5rem; }
.action-box h4 { margin: 0 0 .25rem; font-size: .9rem; color: #1e3a8a; }
.action-box p { margin: 0; font-size: .8125rem; color: #3b82f6; }

/* Modal Overlay */
.modal-overlay { position: fixed; inset: 0; z-index: 1000; background: rgba(15,23,42,.4); backdrop-filter: blur(4px); display: flex; align-items: center; justify-content: center; padding: 1rem; }
.modal-content { background: rgba(255,255,255,.95); backdrop-filter: saturate(180%) blur(24px); width: 100%; max-width: 500px; border-radius: 24px; box-shadow: 0 24px 80px rgba(2,6,23,.32); overflow: hidden; border: 1px solid rgba(255,255,255,.5); }
.modal-header { padding: 1.25rem 1.5rem; border-bottom: 1px solid rgba(148,163,184,.15); display: flex; justify-content: space-between; align-items: center; }
.modal-header h3 { margin: 0; font-size: 1.1rem; font-weight: 700; }
.close-btn-sm { width: 30px; height: 30px; }
.modal-body { padding: 1.5rem; display: flex; flex-direction: column; gap: 1.25rem; }
.form-group label { display: block; font-size: .8125rem; font-weight: 600; margin-bottom: .4rem; }
.input-disabled { width: 100%; padding: .6rem 1rem; border-radius: 10px; border: 1px solid rgba(148,163,184,.2); background: #f8fafc; color: #64748b; font-size: .875rem; }
.input-glass { width: 100%; padding: .6rem 1rem; border-radius: 10px; border: 1px solid rgba(148,163,184,.4); background: rgba(255,255,255,.6); font-size: .875rem; outline: none; transition: border-color .2s; }
.input-glass:focus { border-color: #2563eb; }
.upload-area { border: 2px dashed rgba(148,163,184,.4); border-radius: 12px; padding: 1.5rem; text-align: center; cursor: pointer; transition: background .2s; display: flex; flex-direction: column; align-items: center; font-size: .8125rem; color: #64748b; }
.upload-area:hover { background: rgba(248,250,252,.8); }
.modal-footer { padding: 1.25rem 1.5rem; border-top: 1px solid rgba(148,163,184,.15); display: flex; justify-content: flex-end; gap: .75rem; background: rgba(248,250,252,.5); }

/* Transitions */
.fade-enter-active, .fade-leave-active { transition: opacity .25s; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
.slide-right-enter-active, .slide-right-leave-active { transition: transform .3s cubic-bezier(0.16,1,.3,1); }
.slide-right-enter-from, .slide-right-leave-to { transform: translateX(100%); }
.modal-enter-active, .modal-leave-active { transition: all .3s cubic-bezier(0.16,1,.3,1); }
.modal-enter-from, .modal-leave-to { opacity: 0; transform: scale(0.95); }

@media (max-width: 768px) {
  .attendance-page { padding: 1rem; }
  .filter-group { flex-direction: column; align-items: stretch; }
}

/* ── Dark Mode ─────────────────────────────────────── */
:global(.dark) .attendance-page { color: #f1f5f9; }
:global(.dark) .page-title { color: #f1f5f9; }
:global(.dark) .page-sub { color: #94a3b8; }
:global(.dark) .eyebrow { color: #93c5fd; }

:global(.dark) .ai-banner.banner-low  { background: rgba(124,58,237,.18); border-color: rgba(124,58,237,.3); color: #c4b5fd; }
:global(.dark) .ai-banner.banner-medium { background: rgba(217,119,6,.18); border-color: rgba(217,119,6,.3); color: #fcd34d; }
:global(.dark) .ai-banner.banner-high  { background: rgba(220,38,38,.18); border-color: rgba(220,38,38,.3); color: #fca5a5; }

:global(.dark) .quota-card { background: rgba(15,23,42,.55); border-color: rgba(255,255,255,.1); box-shadow: 0 4px 20px rgba(2,6,23,.4); }
:global(.dark) .quota-val { color: #94a3b8; }
:global(.dark) .quota-track { background: rgba(255,255,255,.08); }

:global(.dark) .metric-card { background: rgba(15,23,42,.55); border-color: rgba(255,255,255,.1); box-shadow: 0 4px 20px rgba(2,6,23,.4); }
:global(.dark) .metric-val { color: #f1f5f9; }
:global(.dark) .metric-lbl { color: #94a3b8; }
:global(.dark) .metric-green .metric-icon-wrap { background: rgba(22,163,74,.2); color: #86efac; }
:global(.dark) .metric-red .metric-icon-wrap { background: rgba(220,38,38,.2); color: #fca5a5; }
:global(.dark) .metric-amber .metric-icon-wrap { background: rgba(217,119,6,.2); color: #fcd34d; }
:global(.dark) .metric-slate .metric-icon-wrap { background: rgba(100,116,139,.2); color: #94a3b8; }

:global(.dark) .filter-bar { background: rgba(15,23,42,.55); border-color: rgba(255,255,255,.1); }
:global(.dark) .filter-select { background: rgba(15,23,42,.6); border-color: rgba(255,255,255,.15); color: #e2e8f0; }

:global(.dark) .table-container { background: rgba(15,23,42,.55); border-color: rgba(255,255,255,.1); }
:global(.dark) .data-table th { background: rgba(2,6,23,.4); color: #94a3b8; border-bottom-color: rgba(255,255,255,.08); }
:global(.dark) .data-table td { border-bottom-color: rgba(255,255,255,.06); color: #e2e8f0; }
:global(.dark) .data-table td.text-slate-500 { color: #64748b; }
:global(.dark) .data-table td.font-medium { color: #cbd5e1; }
:global(.dark) .data-table tbody tr:hover { background: rgba(30,41,59,.5); }
:global(.dark) .empty-state { color: #64748b; }

:global(.dark) .badge-green  { background: rgba(22,163,74,.2); color: #86efac; }
:global(.dark) .badge-red    { background: rgba(220,38,38,.2); color: #fca5a5; }
:global(.dark) .badge-amber  { background: rgba(217,119,6,.2); color: #fcd34d; }
:global(.dark) .badge-blue   { background: rgba(37,99,235,.2); color: #93c5fd; }
:global(.dark) .badge-slate  { background: rgba(100,116,139,.2); color: #94a3b8; }

:global(.dark) .btn-ghost { color: #93c5fd; }
:global(.dark) .btn-ghost:hover { background: rgba(37,99,235,.18); }
:global(.dark) .btn-primary { background: #2563eb; box-shadow: 0 4px 14px rgba(37,99,235,.4); }
:global(.dark) .btn-secondary { background: rgba(30,41,59,.7); border-color: rgba(255,255,255,.12); color: #cbd5e1; }
:global(.dark) .btn-secondary:hover { border-color: #93c5fd; color: #93c5fd; }

:global(.dark) .drawer { background: rgba(10,16,32,.92); backdrop-filter: saturate(160%) blur(28px); box-shadow: -8px 0 48px rgba(2,6,23,.6); }
:global(.dark) .drawer-header { border-bottom-color: rgba(255,255,255,.08); }
:global(.dark) .drawer-title { color: #f1f5f9; }
:global(.dark) .close-btn, :global(.dark) .close-btn-sm { background: rgba(30,41,59,.8); border-color: rgba(255,255,255,.12); color: #94a3b8; }
:global(.dark) .info-row { color: #cbd5e1; }
:global(.dark) .info-icon { color: #475569; }
:global(.dark) .info-box { background: rgba(30,41,59,.6); border-color: rgba(255,255,255,.08); }
:global(.dark) .info-box h4 { color: #94a3b8; }
:global(.dark) .info-box p { color: #cbd5e1; }
:global(.dark) .action-box { background: rgba(37,99,235,.12); border-color: rgba(37,99,235,.3); }
:global(.dark) .action-box h4 { color: #93c5fd; }
:global(.dark) .action-box p { color: #60a5fa; }
:global(.dark) .action-icon { color: #93c5fd; }

:global(.dark) .modal-content { background: rgba(10,16,32,.95); border-color: rgba(255,255,255,.12); box-shadow: 0 24px 80px rgba(2,6,23,.6); }
:global(.dark) .modal-header { border-bottom-color: rgba(255,255,255,.08); }
:global(.dark) .modal-header h3 { color: #f1f5f9; }
:global(.dark) .modal-footer { background: rgba(2,6,23,.3); border-top-color: rgba(255,255,255,.08); }
:global(.dark) .form-group label { color: #cbd5e1; }
:global(.dark) .input-disabled { background: rgba(30,41,59,.6); border-color: rgba(255,255,255,.08); color: #64748b; }
:global(.dark) .input-glass { background: rgba(15,23,42,.6); border-color: rgba(255,255,255,.15); color: #e2e8f0; }
:global(.dark) .input-glass:focus { border-color: #3b82f6; }
:global(.dark) .upload-area { border-color: rgba(255,255,255,.15); color: #64748b; }
:global(.dark) .upload-area:hover { background: rgba(30,41,59,.5); }
</style>
