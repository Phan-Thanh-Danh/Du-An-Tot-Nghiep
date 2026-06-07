<script setup>
import { ref, computed } from 'vue'
import { useBodyScrollLock } from '@/composables/useBodyScrollLock'
import {
  FileText, Search, Filter, Plus, Clock, 
  CheckCircle2, XCircle, AlertCircle, Paperclip,
  ChevronDown, X, FileSignature, Check, FileCheck, Info
} from 'lucide-vue-next'

// Mock Data
const requestTypes = ['Xin nghỉ học / Vắng thi', 'Phúc khảo điểm', 'Cấp bảng điểm', 'Rút môn học', 'Bảo lưu / Thôi học']
const statuses = ['Pending', 'Processing', 'Approved', 'Rejected']

const statusConfig = {
  'Pending': { label: 'Chờ tiếp nhận', cls: 'badge-slate', icon: Clock },
  'Processing': { label: 'Đang xử lý', cls: 'badge-blue', icon: AlertCircle },
  'Approved': { label: 'Đã duyệt', cls: 'badge-green', icon: CheckCircle2 },
  'Rejected': { label: 'Từ chối', cls: 'badge-red', icon: XCircle }
}

const mockRequests = ref([
  {
    id: 'REQ-26-001', type: 'Xin nghỉ học / Vắng thi', status: 'Approved',
    createdAt: new Date(2026, 4, 15, 8, 30),
    subject: 'Toán rời rạc', reason: 'Nghỉ ốm có giấy khám bệnh',
    processor: 'Phòng Đào tạo', response: 'Đã duyệt cho phép vắng mặt có lý do chính đáng.',
    timeline: [
      { action: 'Nộp đơn thành công', time: '08:30 15/05/2026' },
      { action: 'Đang xử lý', time: '10:00 15/05/2026' },
      { action: 'Đã duyệt', time: '14:30 16/05/2026' }
    ]
  },
  {
    id: 'REQ-26-002', type: 'Phúc khảo điểm', status: 'Pending',
    createdAt: new Date(2026, 4, 23, 10, 15),
    subject: 'Cấu trúc dữ liệu & Giải thuật', reason: 'Em thấy câu 3 phần code em làm đúng nhưng không có điểm.',
    processor: 'Chờ phân công', response: '',
    timeline: [
      { action: 'Nộp đơn thành công', time: '10:15 23/05/2026' }
    ]
  }
])

// State
const searchQuery = ref('')
const filterStatus = ref('Tất cả')
const filterType = ref('Tất cả')

const statusOpen = ref(false)
const typeOpen = ref(false)

const activeRequest = ref(null)

// Modals
const createModalOpen = ref(false)
useBodyScrollLock(createModalOpen)
const newReq = ref({
  type: 'Xin nghỉ học / Vắng thi',
  subject: '',
  reason: '',
  file: null
})

// Computed
const filteredRequests = computed(() => {
  return mockRequests.value.filter(r => {
    const matchStatus = filterStatus.value === 'Tất cả' || r.status === filterStatus.value
    const matchType = filterType.value === 'Tất cả' || r.type === filterType.value
    const matchQuery = r.type.toLowerCase().includes(searchQuery.value.toLowerCase()) || r.id.toLowerCase().includes(searchQuery.value.toLowerCase())
    return matchStatus && matchType && matchQuery
  })
})

// Actions
const selectRequest = (req) => {
  activeRequest.value = req
}

const openCreateModal = () => {
  newReq.value = { type: 'Xin nghỉ học / Vắng thi', subject: '', reason: '', file: null }
  createModalOpen.value = true
}

const submitRequest = () => {
  const newId = `REQ-26-00${mockRequests.value.length + 1}`
  const now = new Date()
  
  mockRequests.value.unshift({
    id: newId,
    type: newReq.value.type,
    status: 'Pending',
    createdAt: now,
    subject: newReq.value.subject,
    reason: newReq.value.reason,
    processor: 'Chờ phân công',
    response: '',
    timeline: [
      { action: 'Nộp đơn thành công', time: formatDate(now) }
    ]
  })
  
  createModalOpen.value = false
  selectRequest(mockRequests.value[0])
}

const formatDate = (date) => new Intl.DateTimeFormat('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' }).format(date)

</script>

<template>
  <div class="requests-page">
    <!-- Header -->
    <div class="page-header">
      <div>
        <div class="eyebrow"><FileSignature :size="15"/>Dịch vụ hành chính</div>
        <h1 class="page-title">Đơn từ & Yêu cầu</h1>
        <p class="page-sub">Quản lý và nộp các loại đơn từ học vụ (Nghỉ học, phúc khảo, cấp bảng điểm,...)</p>
      </div>
      <button class="btn-primary" @click="openCreateModal">
        <Plus :size="16"/> Nộp đơn mới
      </button>
    </div>

    <!-- Main Layout: Master Detail -->
    <div class="master-detail-layout">
      <!-- Left: Request List -->
      <div class="list-panel">
        <div class="toolbar">
          <div class="search-box">
            <Search :size="16" class="text-placeholder" />
            <input v-model="searchQuery" type="text" placeholder="Tìm mã đơn hoặc loại đơn..." />
          </div>
          <div class="filter-group">
            <div v-if="statusOpen || typeOpen" class="dropdown-backdrop" @click="statusOpen = false; typeOpen = false"></div>
            
            <!-- Status Dropdown -->
            <div class="custom-select">
              <div class="select-trigger" @click="statusOpen = !statusOpen; typeOpen = false">
                {{ filterStatus === 'Tất cả' ? 'Trạng thái' : statusConfig[filterStatus]?.label || filterStatus }}
                <ChevronDown :size="14" class="ml-1" />
              </div>
              <Transition name="fade">
                <div class="select-menu" v-if="statusOpen">
                  <div class="select-option" :class="{'selected': filterStatus === 'Tất cả'}" @click="filterStatus = 'Tất cả'; statusOpen = false">Trạng thái (Tất cả)</div>
                  <div v-for="s in statuses" :key="s" class="select-option" :class="{'selected': filterStatus === s}" @click="filterStatus = s; statusOpen = false">
                    {{ statusConfig[s]?.label }}
                  </div>
                </div>
              </Transition>
            </div>

            <!-- Type Dropdown -->
            <div class="custom-select">
              <div class="select-trigger" @click="typeOpen = !typeOpen; statusOpen = false">
                {{ filterType === 'Tất cả' ? 'Loại đơn' : filterType }}
                <ChevronDown :size="14" class="ml-1" />
              </div>
              <Transition name="fade">
                <div class="select-menu" v-if="typeOpen">
                  <div class="select-option" :class="{'selected': filterType === 'Tất cả'}" @click="filterType = 'Tất cả'; typeOpen = false">Loại đơn (Tất cả)</div>
                  <div v-for="t in requestTypes" :key="t" class="select-option" :class="{'selected': filterType === t}" @click="filterType = t; typeOpen = false">
                    {{ t }}
                  </div>
                </div>
              </Transition>
            </div>
          </div>
        </div>

        <div class="list-cards">
          <div v-for="req in filteredRequests" :key="req.id" 
               class="list-card" :class="{'active-card': activeRequest?.id === req.id}"
               @click="selectRequest(req)">
            <div class="lc-header">
              <span class="lc-id">{{ req.id }}</span>
              <span class="status-badge" :class="statusConfig[req.status].cls">
                <component :is="statusConfig[req.status].icon" :size="12" />
                {{ statusConfig[req.status].label }}
              </span>
            </div>
            <h3 class="lc-title">{{ req.type }}</h3>
            <div class="lc-meta">
              <span><Clock :size="12"/> {{ formatDate(req.createdAt).split(' ')[1] }}</span>
            </div>
          </div>
          <div v-if="filteredRequests.length === 0" class="empty-list">
            Không tìm thấy đơn từ nào.
          </div>
        </div>
      </div>

      <!-- Right: Detail Panel -->
      <div class="detail-panel">
        <template v-if="activeRequest">
          <!-- Detail Header -->
          <div class="detail-header">
            <div class="flex justify-between items-start mb-4">
              <div>
                <span class="status-badge lg mb-2" :class="statusConfig[activeRequest.status].cls">
                  <component :is="statusConfig[activeRequest.status].icon" :size="14" />
                  {{ statusConfig[activeRequest.status].label }}
                </span>
                <h2 class="text-xl font-semibold text-heading">{{ activeRequest.type }}</h2>
                <div class="text-sm text-muted mt-1">Mã đơn: {{ activeRequest.id }} • Nộp lúc: {{ formatDate(activeRequest.createdAt) }}</div>
              </div>
            </div>
          </div>

          <!-- Detail Body -->
          <div class="detail-body">
            <div class="info-section">
              <h3 class="section-title">Thông tin chi tiết</h3>
              <div class="info-grid">
                <div class="info-row" v-if="activeRequest.subject">
                  <span class="lbl">Môn học liên quan:</span>
                  <span class="val">{{ activeRequest.subject }}</span>
                </div>
                <div class="info-row">
                  <span class="lbl">Nội dung / Lý do:</span>
                  <span class="val">{{ activeRequest.reason }}</span>
                </div>
                <div class="info-row">
                  <span class="lbl">File đính kèm:</span>
                  <span class="val text-link flex items-center gap-1 cursor-pointer hover:underline"><Paperclip :size="14"/> minh_chung.pdf</span>
                </div>
              </div>
            </div>

            <div class="info-section mt-6">
              <h3 class="section-title">Kết quả xử lý</h3>
              <div class="response-box" :class="`resp-${activeRequest.status}`">
                <div class="flex items-center gap-2 font-semibold mb-2">
                  <Info :size="16"/>
                  Người tiếp nhận: {{ activeRequest.processor }}
                </div>
                <p v-if="activeRequest.response">{{ activeRequest.response }}</p>
                <p v-else class="italic text-muted">Đang chờ cán bộ xử lý và phản hồi...</p>
              </div>
            </div>

            <div class="info-section mt-6">
              <h3 class="section-title">Tiến trình (Timeline)</h3>
              <div class="timeline">
                <div v-for="(log, i) in activeRequest.timeline" :key="i" class="tl-item">
                  <div class="tl-dot"></div>
                  <div class="tl-content">
                    <div class="tl-action">{{ log.action }}</div>
                    <div class="tl-time">{{ log.time }}</div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </template>
        <div v-else class="empty-detail">
          <FileCheck :size="48" class="text-muted mb-4" />
          <h3>Chọn một đơn từ</h3>
          <p>Bấm vào một đơn từ ở danh sách bên trái để xem tiến độ và phản hồi từ nhà trường.</p>
        </div>
      </div>
    </div>

    <!-- Modals -->
    <Teleport to="body">
      <!-- Create Request Modal -->
      <Transition name="modal">
        <div v-if="createModalOpen" class="modal-overlay" @click.self="createModalOpen = false">
          <div class="modal-content lg">
            <div class="modal-header">
              <h3>Nộp đơn / Yêu cầu mới</h3>
              <button class="close-btn-sm" @click="createModalOpen = false"><X :size="20"/></button>
            </div>
            
            <div class="modal-body">
              <div class="form-group mb-4">
                <label>Loại đơn</label>
                <select v-model="newReq.type" class="input-glass">
                  <option v-for="t in requestTypes" :key="t" :value="t">{{ t }}</option>
                </select>
              </div>

              <div class="form-group mb-4" v-if="['Xin nghỉ học / Vắng thi', 'Phúc khảo điểm', 'Rút môn học'].includes(newReq.type)">
                <label>Môn học liên quan <span class="text-[var(--color-danger-text)]">*</span></label>
                <input v-model="newReq.subject" type="text" class="input-glass" placeholder="Nhập tên môn học..." />
              </div>

              <div class="form-group mb-4">
                <label>Nội dung / Lý do chi tiết <span class="text-[var(--color-danger-text)]">*</span></label>
                <textarea v-model="newReq.reason" class="input-glass" rows="4" placeholder="Trình bày rõ lý do của bạn..."></textarea>
              </div>

              <div class="form-group">
                <label>Tệp đính kèm (Bắt buộc với đơn xin nghỉ ốm)</label>
                <div class="upload-box">
                  <Paperclip :size="20" class="text-muted mb-2"/>
                  <span>Kéo thả minh chứng hoặc nhấn để chọn</span>
                </div>
              </div>
            </div>
            
            <div class="modal-footer">
              <button class="btn-secondary" @click="createModalOpen = false">Hủy</button>
              <button class="btn-primary" @click="submitRequest" :disabled="!newReq.reason || (!newReq.subject && ['Xin nghỉ học / Vắng thi', 'Phúc khảo điểm', 'Rút môn học'].includes(newReq.type))">
                <Check :size="16"/> Nộp Đơn
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<style scoped>
.requests-page {
  padding: 1.5rem 2rem;
  max-width: 1500px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  color: var(--text-body);
  height: calc(100vh - 4rem);
}

.page-header { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; flex-wrap: wrap; flex-shrink: 0; }
.eyebrow { display: flex; align-items: center; gap: .375rem; font-size: .7rem; font-weight: 700; text-transform: uppercase; letter-spacing: .08em; color: var(--text-link); margin-bottom: .4rem; }
.page-title { font-size: 1.875rem; font-weight: 800; margin: 0 0 .25rem; letter-spacing: -.02em; color: var(--text-heading); }
.page-sub { font-size: .875rem; color: var(--text-muted); margin: 0; }

/* Master Detail Layout */
.master-detail-layout { display: flex; gap: 1.5rem; flex: 1; min-height: 0; }

/* Left Panel */
.list-panel {
  width: 380px;
  background: var(--surface-card); border: 1px solid var(--border-card);
  border-radius: 20px; display: flex; flex-direction: column;
  box-shadow: var(--lg-shadow-sm); backdrop-filter: saturate(160%) blur(16px);
  overflow: hidden;
}

.toolbar { padding: 1rem; border-bottom: 1px solid var(--border-default); display: flex; flex-direction: column; gap: .75rem; }
.search-box { display: flex; align-items: center; gap: .5rem; background: var(--surface-input); border-radius: 10px; padding: 0 1rem; border: 1px solid var(--border-input); }
.search-box input { background: transparent; border: none; outline: none; padding: .6rem 0; width: 100%; font-size: .875rem; }
.filter-group { display: flex; gap: .5rem; position: relative; }

.dropdown-backdrop { position: fixed; inset: 0; z-index: 10; }
.custom-select { position: relative; z-index: 11; flex: 1; }
.select-trigger { display: flex; align-items: center; justify-content: space-between; padding: .6rem .75rem; border-radius: 10px; border: 1px solid var(--border-input); background: var(--surface-input); font-size: .8125rem; font-weight: 600; color: var(--text-label); cursor: pointer; transition: all .2s; user-select: none; }
.select-trigger:hover { border-color: var(--border-input-focus); color: var(--text-link); }
.select-menu { position: absolute; top: calc(100% + .5rem); left: 0; width: 100%; min-width: 140px; background: var(--surface-dropdown); backdrop-filter: saturate(180%) blur(24px); border: 1px solid var(--border-card); border-radius: 16px; padding: .4rem; box-shadow: var(--lg-shadow-md); display: flex; flex-direction: column; gap: .2rem; overflow: hidden; }
.select-option { padding: .6rem .75rem; border-radius: 10px; font-size: .8125rem; font-weight: 500; color: var(--text-body); cursor: pointer; transition: all .15s; }
.select-option:hover { background: var(--surface-input); color: var(--text-link); padding-left: 1rem; }
.select-option.selected { background: var(--color-info-bg); color: var(--color-info-text); font-weight: 700; }

.list-cards { flex: 1; overflow-y: auto; padding: .75rem; display: flex; flex-direction: column; gap: .5rem; }
.list-card { background: var(--surface-solid); border: 1px solid var(--border-default); border-radius: 12px; padding: 1rem; cursor: pointer; transition: all .2s; }
.list-card:hover { border-color: var(--border-input-focus); }
.list-card.active-card { background: var(--surface-card); border-color: var(--border-input-focus); box-shadow: 0 4px 12px var(--accent-primary-soft); }

.lc-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: .5rem; }
.lc-id { font-size: .75rem; font-weight: 700; color: var(--text-muted); background: var(--surface-input); padding: .2rem .5rem; border-radius: 6px; }
.lc-title { font-size: .95rem; font-weight: 700; margin: 0 0 .5rem; line-height: 1.3; color: var(--text-heading); }
.lc-meta { display: flex; gap: .5rem; font-size: .75rem; color: var(--text-muted); font-weight: 500; align-items: center; }

.empty-list { text-align: center; padding: 2rem; color: var(--text-muted); font-size: .875rem; font-style: italic; }

/* Right Panel */
.detail-panel {
  flex: 1; background: var(--surface-card); border: 1px solid var(--border-card);
  border-radius: 20px; display: flex; flex-direction: column;
  box-shadow: var(--lg-shadow-sm); backdrop-filter: saturate(160%) blur(16px);
  overflow: hidden;
}

.empty-detail { flex: 1; display: flex; flex-direction: column; align-items: center; justify-content: center; color: var(--text-muted); text-align: center; padding: 2rem; }
.empty-detail h3 { font-size: 1.25rem; font-weight: 700; color: var(--text-heading); margin-bottom: .5rem; }

.detail-header { padding: 1.5rem; border-bottom: 1px solid var(--border-default); background: var(--surface-solid); }
.detail-body { flex: 1; padding: 1.5rem; overflow-y: auto; }

.section-title { font-size: 1rem; font-weight: 800; color: var(--text-heading); margin: 0 0 1rem; border-left: 3px solid var(--text-link); padding-left: .5rem; }
.info-grid { background: var(--surface-solid); border: 1px solid var(--border-default); border-radius: 12px; padding: 1.25rem; display: flex; flex-direction: column; gap: .75rem; }
.info-row { display: flex; flex-direction: column; gap: .25rem; }
.info-row .lbl { font-size: .8125rem; font-weight: 600; color: var(--text-muted); }
.info-row .val { font-size: .9rem; font-weight: 500; color: var(--text-heading); }

.response-box { padding: 1.25rem; border-radius: 12px; font-size: .9rem; }
.resp-Pending { background: var(--surface-solid); border: 1px solid var(--border-default); color: var(--text-body); }
.resp-Processing { background: var(--color-info-bg); border: 1px solid color-mix(in srgb, var(--color-info-text) 22%, transparent); color: var(--color-info-text); }
.resp-Approved { background: var(--color-success-bg); border: 1px solid color-mix(in srgb, var(--color-success-text) 22%, transparent); color: var(--color-success-text); }
.resp-Rejected { background: var(--color-danger-bg); border: 1px solid color-mix(in srgb, var(--color-danger-text) 22%, transparent); color: var(--color-danger-text); }

/* Timeline Sidebar */
.timeline { position: relative; padding-left: 8px; margin-top: .5rem; }
.timeline::before { content: ''; position: absolute; left: 11px; top: 0; bottom: 0; width: 2px; background: var(--border-default); }
.tl-item { position: relative; margin-bottom: 1.25rem; }
.tl-dot { position: absolute; left: 0; top: 4px; width: 8px; height: 8px; border-radius: 50%; background: var(--text-link); border: 2px solid var(--surface-card); box-sizing: content-box; }
.tl-content { padding-left: 1.5rem; }
.tl-action { font-size: .875rem; font-weight: 600; color: var(--text-label); }
.tl-time { font-size: .75rem; color: var(--text-muted); margin-top: .1rem; }

/* Badges */
.status-badge { display: inline-flex; align-items: center; gap: .3rem; font-size: .65rem; font-weight: 700; padding: .15rem .5rem; border-radius: 99px; text-transform: uppercase; }
.status-badge.lg { padding: .3rem .75rem; font-size: .75rem; }
.badge-blue { background: var(--color-info-bg); color: var(--color-info-text); }
.badge-red { background: var(--color-danger-bg); color: var(--color-danger-text); }
.badge-green { background: var(--color-success-bg); color: var(--color-success-text); }
.badge-slate { background: var(--surface-solid); color: var(--text-muted); }

/* Buttons */
.btn-primary, .btn-secondary { display: inline-flex; align-items: center; justify-content: center; gap: .4rem; padding: .6rem 1.2rem; border-radius: 10px; font-size: .8125rem; font-weight: 700; cursor: pointer; border: none; transition: all .15s; outline: none; }
.btn-primary { background: var(--text-link); color: var(--text-inverse); box-shadow: 0 4px 14px color-mix(in srgb, var(--text-link) 25%, transparent); }
.btn-primary:hover:not(:disabled) { background: var(--lg-primary-dark); transform: translateY(-1px); }
.btn-primary:disabled { opacity: .6; cursor: not-allowed; }
.btn-secondary { background: var(--surface-input); color: var(--text-label); border: 1px solid var(--border-input); }
.btn-secondary:hover { border-color: var(--border-input-focus); color: var(--text-heading); }

/* Modals */
.modal-overlay { position: fixed; inset: 0; z-index: 9998; background: color-mix(in srgb, var(--text-heading) 40%, transparent); backdrop-filter: blur(6px); display: flex; align-items: center; justify-content: center; padding: 1rem; }
.modal-content { position: relative; z-index: 9999; background: var(--surface-modal); backdrop-filter: saturate(180%) blur(24px); width: 100%; border-radius: 24px; box-shadow: 0 24px 80px color-mix(in srgb, var(--text-heading) 32%, transparent); overflow: hidden; border: 1px solid var(--border-card); }
.modal-content.lg { max-width: 600px; }
.modal-header { padding: 1.25rem 1.5rem; border-bottom: 1px solid var(--border-default); display: flex; justify-content: space-between; align-items: center; }
.modal-header h3 { margin: 0; font-size: 1.1rem; font-weight: 800; color: var(--text-heading); }
.close-btn-sm { background: transparent; border: none; color: var(--text-placeholder); cursor: pointer; display: flex; transition: color .15s; }
.close-btn-sm:hover { color: var(--color-danger-text); }
.modal-body { padding: 1.5rem; display: flex; flex-direction: column; }
.modal-footer { padding: 1.25rem 1.5rem; border-top: 1px solid var(--border-default); display: flex; justify-content: flex-end; gap: .75rem; background: var(--surface-solid); }

.form-group label { display: block; font-size: .8125rem; font-weight: 700; margin-bottom: .4rem; }
.input-glass { width: 100%; padding: .6rem 1rem; border-radius: 10px; border: 1px solid var(--border-input); background: var(--surface-input); color: var(--text-body); font-size: .875rem; outline: none; transition: border-color .2s; }
.input-glass:focus { border-color: var(--border-input-focus); }

.upload-box { border: 2px dashed var(--border-input); border-radius: 12px; padding: 2rem; text-align: center; background: var(--surface-solid); cursor: pointer; display: flex; flex-direction: column; align-items: center; color: var(--text-muted); font-size: .8125rem; transition: background .2s; }
.upload-box:hover { border-color: var(--border-input-focus); color: var(--text-link); }

@media (max-width: 1024px) {
  .master-detail-layout { flex-direction: column; }
  .list-panel { width: 100%; max-height: 400px; }
}
</style>
