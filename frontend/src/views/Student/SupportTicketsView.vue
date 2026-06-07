<script setup>
import { ref, computed } from 'vue'
import { useBodyScrollLock } from '@/composables/useBodyScrollLock'
import { usePopupStore } from '@/stores/popup'
import {
  LifeBuoy, Search, Filter, Plus, Send, Clock, 
  CheckCircle2, XCircle, AlertCircle, Paperclip,
  Star, ChevronRight, MessageSquare, Bot, FileText, X, ChevronDown
} from 'lucide-vue-next'

const popupStore = usePopupStore()

// Mock Data
const categories = ['Kỹ thuật', 'Học vụ', 'Tài chính', 'Khác']
const statuses = ['Open', 'In progress', 'Resolved', 'Closed']

const statusConfig = {
  'Open': { label: 'Mới tạo', cls: 'badge-blue', icon: AlertCircle },
  'In progress': { label: 'Đang xử lý', cls: 'badge-amber', icon: Clock },
  'Resolved': { label: 'Đã giải quyết', cls: 'badge-green', icon: CheckCircle2 },
  'Closed': { label: 'Đã đóng', cls: 'badge-slate', icon: XCircle }
}

const mockTickets = ref([
  {
    id: 'TCK-001', title: 'Lỗi không vào được trang làm bài thi', category: 'Kỹ thuật', status: 'In progress',
    assignedTo: 'Admin Nguyễn Văn A', createdAt: new Date(2026, 4, 20, 9, 30), deadline: new Date(2026, 4, 21, 9, 30),
    messages: [
      { sender: 'me', text: 'Chào Admin, em không thể bấm vào nút bắt đầu bài thi môn Cấu trúc dữ liệu.', time: '09:30 AM' },
      { sender: 'agent', text: 'Chào em, hệ thống đã ghi nhận lỗi. Vui lòng thử xóa cache trình duyệt và thử lại nhé.', time: '09:45 AM' }
    ],
    timeline: [
      { action: 'Ticket được tạo', time: '09:30 20/05/2026' },
      { action: 'AI Phân loại: Kỹ thuật', time: '09:31 20/05/2026' },
      { action: 'Gán cho: Nguyễn Văn A', time: '09:35 20/05/2026' }
    ]
  },
  {
    id: 'TCK-002', title: 'Xin gia hạn nộp học phí đợt 2', category: 'Tài chính', status: 'Resolved',
    assignedTo: 'Phòng Tài vụ', createdAt: new Date(2026, 4, 15, 14, 0), deadline: new Date(2026, 4, 18, 14, 0),
    messages: [
      { sender: 'me', text: 'Dạ trường cho em hỏi em có thể nộp trễ học phí 5 ngày được không ạ?', time: '14:00 PM' },
      { sender: 'agent', text: 'Chào em, yêu cầu của em đã được duyệt. Hệ thống đã cập nhật hạn nộp mới trên portal.', time: '08:30 AM (Hôm sau)' }
    ],
    timeline: [
      { action: 'Ticket được tạo', time: '14:00 15/05/2026' },
      { action: 'Đã giải quyết', time: '08:30 16/05/2026' }
    ]
  }
])

const faqs = [
  { question: 'Làm sao để đổi mật khẩu?', answer: 'Bạn vào mục Cá nhân > Đổi mật khẩu.' },
  { question: 'Lỗi không vào được trang thi', answer: 'Hãy thử xóa cache, dùng Chrome phiên bản mới nhất hoặc tắt các tiện ích chặn quảng cáo.' }
]

// State
const searchQuery = ref('')
const filterStatus = ref('Tất cả')
const filterCategory = ref('Tất cả')

const statusOpen = ref(false)
const categoryOpen = ref(false)

const activeTicket = ref(null)
const chatInput = ref('')

// Modals
const createModalOpen = ref(false)
useBodyScrollLock(createModalOpen)
const ratingModalOpen = ref(false)
const createStep = ref(1) // 1: Input & AI FAQ, 2: Form

const newTicket = ref({
  category: '',
  title: '',
  content: '',
  file: null
})

const aiSuggestions = ref([])
const rating = ref(0)
const ratingFeedback = ref('')

// Computed
const filteredTickets = computed(() => {
  return mockTickets.value.filter(t => {
    const matchStatus = filterStatus.value === 'Tất cả' || t.status === filterStatus.value
    const matchCat = filterCategory.value === 'Tất cả' || t.category === filterCategory.value
    const matchQuery = t.title.toLowerCase().includes(searchQuery.value.toLowerCase()) || t.id.toLowerCase().includes(searchQuery.value.toLowerCase())
    return matchStatus && matchCat && matchQuery
  })
})

// Actions
const selectTicket = (t) => {
  activeTicket.value = t
  // Reset chat input
  chatInput.value = ''
}

const sendMessage = () => {
  if (!chatInput.value.trim() || !activeTicket.value) return
  if (activeTicket.value.status === 'Closed' || activeTicket.value.status === 'Resolved') return

  const now = new Date()
  const timeStr = new Intl.DateTimeFormat('vi-VN', { hour: '2-digit', minute: '2-digit' }).format(now)
  
  activeTicket.value.messages.push({
    sender: 'me',
    text: chatInput.value,
    time: timeStr
  })
  chatInput.value = ''

  // Mock reply
  setTimeout(() => {
    activeTicket.value.messages.push({
      sender: 'agent',
      text: 'Đây là tin nhắn tự động từ hệ thống. Quản trị viên sẽ phản hồi bạn sớm nhất.',
      time: new Intl.DateTimeFormat('vi-VN', { hour: '2-digit', minute: '2-digit' }).format(new Date())
    })
  }, 1500)
}

const openCreateModal = () => {
  createStep.value = 1
  newTicket.value = { category: 'Kỹ thuật', title: '', content: '', file: null }
  aiSuggestions.value = []
  createModalOpen.value = true
}

const checkFAQ = () => {
  // Simulate AI fetching FAQ based on title
  const query = newTicket.value.title.toLowerCase()
  aiSuggestions.value = faqs.filter(f => f.question.toLowerCase().includes(query) || query.includes('lỗi'))
  if (aiSuggestions.value.length === 0) {
    createStep.value = 2 // Move to form if no FAQ
  }
}

const submitTicket = () => {
  const newId = `TCK-00${mockTickets.value.length + 1}`
  const now = new Date()
  const deadline = new Date(now.getTime() + 24 * 60 * 60 * 1000) // Default 24h
  
  mockTickets.value.unshift({
    id: newId,
    title: newTicket.value.title,
    category: newTicket.value.category,
    status: 'Open',
    assignedTo: 'Đang chờ phân công...',
    createdAt: now,
    deadline: deadline,
    messages: [
      { sender: 'me', text: newTicket.value.content, time: 'Vừa xong' }
    ],
    timeline: [
      { action: 'Ticket được tạo', time: 'Vừa xong' },
      { action: 'AI Phân loại: ' + newTicket.value.category, time: 'Vừa xong' }
    ]
  })
  
  createModalOpen.value = false
  selectTicket(mockTickets.value[0])
}

const closeTicket = () => {
  if (!activeTicket.value) return
  activeTicket.value.status = 'Closed'
  ratingModalOpen.value = true
}

const submitRating = () => {
  ratingModalOpen.value = false
  popupStore.success('Cảm ơn bạn', 'Đánh giá của bạn đã được ghi nhận.')
  rating.value = 0
  ratingFeedback.value = ''
}

const formatDate = (date) => new Intl.DateTimeFormat('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' }).format(date)
const setRating = (val) => rating.value = val

</script>

<template>
  <div class="support-page">
    <!-- Header -->
    <div class="page-header">
      <div>
        <div class="eyebrow"><LifeBuoy :size="15"/>Hỗ trợ sinh viên</div>
        <h1 class="page-title">Hỗ trợ & Ticket</h1>
        <p class="page-sub">Gửi yêu cầu hỗ trợ, theo dõi tiến độ xử lý với sự trợ giúp từ AI.</p>
      </div>
      <button class="btn-primary" @click="openCreateModal">
        <Plus :size="16"/> Tạo Ticket Mới
      </button>
    </div>

    <!-- Main Layout: Master Detail -->
    <div class="master-detail-layout">
      <!-- Left: Ticket List -->
      <div class="ticket-list-panel">
        <div class="toolbar">
          <div class="search-box">
            <Search :size="16" class="search-icon" />
            <input v-model="searchQuery" type="text" placeholder="Tìm tiêu đề hoặc mã ticket..." />
          </div>
          <div class="filter-group">
            <div v-if="statusOpen || categoryOpen" class="dropdown-backdrop" @click="statusOpen = false; categoryOpen = false"></div>
            
            <div class="custom-select">
              <div class="select-trigger" @click="statusOpen = !statusOpen; categoryOpen = false">
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

            <div class="custom-select">
              <div class="select-trigger" @click="categoryOpen = !categoryOpen; statusOpen = false">
                {{ filterCategory === 'Tất cả' ? 'Danh mục' : filterCategory }}
                <ChevronDown :size="14" class="ml-1" />
              </div>
              <Transition name="fade">
                <div class="select-menu" v-if="categoryOpen">
                  <div class="select-option" :class="{'selected': filterCategory === 'Tất cả'}" @click="filterCategory = 'Tất cả'; categoryOpen = false">Danh mục (Tất cả)</div>
                  <div v-for="c in categories" :key="c" class="select-option" :class="{'selected': filterCategory === c}" @click="filterCategory = c; categoryOpen = false">
                    {{ c }}
                  </div>
                </div>
              </Transition>
            </div>
          </div>
        </div>

        <div class="ticket-cards">
          <div v-for="t in filteredTickets" :key="t.id" 
               class="ticket-card" :class="{'active-card': activeTicket?.id === t.id}"
               @click="selectTicket(t)">
            <div class="tc-header">
              <span class="tc-id">{{ t.id }}</span>
              <span class="status-badge" :class="statusConfig[t.status].cls">
                <component :is="statusConfig[t.status].icon" :size="12" />
                {{ statusConfig[t.status].label }}
              </span>
            </div>
            <h3 class="tc-title">{{ t.title }}</h3>
            <div class="tc-meta">
              <span>{{ t.category }}</span>
              <span>•</span>
              <span>{{ formatDate(t.createdAt).split(' ')[1] }}</span>
            </div>
          </div>
          <div v-if="filteredTickets.length === 0" class="empty-list">
            Không tìm thấy ticket nào.
          </div>
        </div>
      </div>

      <!-- Right: Ticket Detail / Chat -->
      <div class="ticket-detail-panel">
        <template v-if="activeTicket">
          <!-- Detail Header -->
          <div class="detail-header">
            <div class="flex justify-between items-start mb-2">
              <div>
                <h2 class="detail-title">{{ activeTicket.title }}</h2>
                <div class="detail-subtitle">Mã: {{ activeTicket.id }} • Tạo lúc: {{ formatDate(activeTicket.createdAt) }}</div>
              </div>
              <span class="status-badge lg" :class="statusConfig[activeTicket.status].cls">
                {{ statusConfig[activeTicket.status].label }}
              </span>
            </div>
            
            <div class="detail-info-bar">
              <div class="info-item"><FileText :size="14"/> Danh mục: <strong>{{ activeTicket.category }}</strong></div>
              <div class="info-item"><Bot :size="14"/> Xử lý: <strong>{{ activeTicket.assignedTo }}</strong></div>
              <div class="info-item"><Clock :size="14"/> Deadline: <strong class="deadline-text">{{ formatDate(activeTicket.deadline) }}</strong></div>
            </div>
          </div>

          <!-- Detail Body (Chat + Timeline) -->
          <div class="detail-body">
            <div class="chat-area">
              <div v-for="(msg, i) in activeTicket.messages" :key="i" class="chat-msg" :class="msg.sender === 'me' ? 'msg-me' : 'msg-agent'">
                <div class="msg-bubble">
                  {{ msg.text }}
                </div>
                <div class="msg-time">{{ msg.time }}</div>
              </div>
            </div>

            <!-- Timeline Sidebar (Desktop only or hidden on mobile) -->
            <div class="timeline-sidebar">
              <h4 class="timeline-title"><Clock :size="14"/> Lịch sử xử lý</h4>
              <div class="timeline">
                <div v-for="(log, i) in activeTicket.timeline" :key="i" class="tl-item">
                  <div class="tl-dot"></div>
                  <div class="tl-content">
                    <div class="tl-action">{{ log.action }}</div>
                    <div class="tl-time">{{ log.time }}</div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Chat Input Footer -->
          <div class="detail-footer">
            <template v-if="['Closed', 'Resolved'].includes(activeTicket.status)">
              <div class="closed-ticket-note">
                Ticket này đã được đóng. Không thể gửi thêm tin nhắn.
                <button v-if="activeTicket.status === 'Resolved'" class="btn-primary mt-2 mx-auto" @click="closeTicket">
                  Xác nhận Đóng & Đánh giá
                </button>
              </div>
            </template>
            <template v-else>
              <button class="btn-icon" title="Đính kèm file"><Paperclip :size="18"/></button>
              <input v-model="chatInput" type="text" class="chat-input" placeholder="Nhập tin nhắn trao đổi..." @keyup.enter="sendMessage" />
              <button class="btn-primary" @click="sendMessage"><Send :size="16"/></button>
              <button class="btn-secondary" title="Kết thúc hỗ trợ" @click="closeTicket"><CheckCircle2 :size="16"/></button>
            </template>
          </div>
        </template>
        <div v-else class="empty-detail">
          <MessageSquare :size="48" class="empty-icon" />
          <h3>Chọn một ticket</h3>
          <p>Bấm vào một ticket ở danh sách bên trái để xem chi tiết hoặc tương tác với nhân viên hỗ trợ.</p>
        </div>
      </div>
    </div>

    <!-- Modals -->
    <Teleport to="body">
      <!-- Create Ticket Modal -->
      <Transition name="modal">
        <div v-if="createModalOpen" class="modal-overlay" @click.self="createModalOpen = false">
          <div class="modal-content lg">
            <div class="modal-header">
              <h3>Tạo yêu cầu hỗ trợ mới</h3>
              <button class="close-btn-sm" @click="createModalOpen = false"><X :size="20"/></button>
            </div>
            
            <div class="modal-body">
              <!-- Step 1: Input & AI FAQ -->
              <template v-if="createStep === 1">
                <div class="form-group mb-4">
                  <label>Mô tả ngắn gọn vấn đề của bạn</label>
                  <div class="flex gap-2">
                    <input v-model="newTicket.title" type="text" class="input-glass flex-1" placeholder="Ví dụ: Không đăng nhập được, muốn đổi lịch học..." @keyup.enter="checkFAQ" />
                    <button class="btn-primary" @click="checkFAQ" :disabled="!newTicket.title"><Sparkles :size="16"/> Gợi ý AI</button>
                  </div>
                </div>

                <div v-if="aiSuggestions.length > 0" class="faq-suggestions">
                  <div class="faq-heading">
                    <Bot :size="18"/> Có thể những bài viết này sẽ giúp ích cho bạn:
                  </div>
                  <div v-for="(faq, i) in aiSuggestions" :key="i" class="faq-item">
                    <h4>{{ faq.question }}</h4>
                    <p>{{ faq.answer }}</p>
                  </div>
                  <div class="mt-4 text-center">
                    <span class="modal-muted">Vẫn chưa giải quyết được?</span>
                    <button class="btn-outline ml-2" @click="createStep = 2">Tiếp tục tạo Ticket</button>
                  </div>
                </div>
              </template>

              <!-- Step 2: Full Form -->
              <template v-if="createStep === 2">
                <div class="ai-notice">
                  <Bot :size="16" class="shrink-0"/>
                  Hệ thống AI sẽ tự động phân tích nội dung và chuyển yêu cầu của bạn đến nhân sự/phòng ban phù hợp nhất.
                </div>
                
                <div class="form-group mt-3">
                  <label>Tiêu đề</label>
                  <input v-model="newTicket.title" type="text" class="input-glass" />
                </div>
                
                <div class="form-group mt-3">
                  <label>Danh mục (Tùy chọn)</label>
                  <select v-model="newTicket.category" class="input-glass">
                    <option v-for="c in categories" :key="c" :value="c">{{ c }}</option>
                  </select>
                </div>

                <div class="form-group mt-3">
                  <label>Mô tả chi tiết</label>
                  <textarea v-model="newTicket.content" class="input-glass" rows="4" placeholder="Cung cấp chi tiết lỗi, thời gian xảy ra, các bước bạn đã làm..."></textarea>
                </div>

                <div class="form-group mt-3">
                  <label>Tệp đính kèm (Hình ảnh lỗi minh chứng)</label>
                  <div class="upload-box">
                    <Paperclip :size="20" class="upload-icon"/>
                    <span>Kéo thả file hoặc nhấn để chọn (Tối đa 5MB)</span>
                  </div>
                </div>
              </template>
            </div>
            
            <div class="modal-footer" v-if="createStep === 2">
              <button class="btn-secondary" @click="createStep = 1">Quay lại</button>
              <button class="btn-primary" @click="submitTicket" :disabled="!newTicket.title || !newTicket.content">Gửi Yêu Cầu</button>
            </div>
          </div>
        </div>
      </Transition>

      <!-- Rating Modal -->
      <Transition name="modal">
        <div v-if="ratingModalOpen" class="modal-overlay" @click.self="ratingModalOpen = false">
          <div class="modal-content sm">
            <div class="modal-header">
              <h3>Đánh giá chất lượng hỗ trợ</h3>
              <button class="close-btn-sm" @click="ratingModalOpen = false"><X :size="20"/></button>
            </div>
            <div class="modal-body text-center">
              <p class="rating-copy">Bạn đánh giá thế nào về sự hỗ trợ của chuyên viên <strong>{{ activeTicket?.assignedTo }}</strong>?</p>
              <div class="stars-container">
                <Star v-for="i in 5" :key="i" :size="32" 
                      class="star-icon" :class="{'active': rating >= i}" 
                      @click="setRating(i)" />
              </div>
              <textarea v-model="ratingFeedback" class="input-glass mt-4" rows="3" placeholder="Góp ý thêm của bạn (Không bắt buộc)"></textarea>
            </div>
            <div class="modal-footer justify-center">
              <button class="btn-primary w-full" :disabled="rating === 0" @click="submitRating">Gửi Đánh Giá</button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<style scoped>
.support-page {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  width: 100%;
  min-height: min(46rem, calc(100vh - 8rem));
  color: var(--text-heading);
}

.page-header { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; flex-wrap: wrap; flex-shrink: 0; }
.eyebrow { display: inline-flex; align-items: center; gap: .35rem; width: fit-content; border: 1px solid var(--border-card); border-radius: 999px; background: var(--surface-input); color: var(--text-link); padding: .25rem .6rem; font-size: .7rem; font-weight: 850; text-transform: uppercase; }
.page-title { color: var(--text-heading); font-size: 1.35rem; font-weight: 900; margin: .45rem 0 .2rem; line-height: 1.15; }
.page-sub { font-size: .82rem; color: var(--text-body); margin: 0; }

/* Master Detail Layout */
.master-detail-layout {
  display: flex;
  gap: 1rem;
  flex: 1;
  min-height: 0; /* Important for scrollable children */
}

/* Left Panel */
.ticket-list-panel {
  width: 360px;
  background: var(--surface-card);
  border: 1px solid var(--border-card);
  border-radius: 18px;
  display: flex;
  flex-direction: column;
  box-shadow: var(--lg-shadow-sm);
  backdrop-filter: blur(calc(var(--glass-blur) - 4px)) saturate(130%);
  overflow: hidden;
}

.toolbar { padding: .8rem; border-bottom: 1px solid var(--border-card); display: flex; flex-direction: column; gap: .65rem; }
.search-box { display: flex; align-items: center; gap: .5rem; background: var(--surface-input); border-radius: 10px; padding: 0 .75rem; border: 1px solid var(--border-input); }
.search-icon { color: var(--text-placeholder); }
.search-box input { background: transparent; border: none; outline: none; padding: .6rem 0; width: 100%; color: var(--text-label); font-size: .85rem; }
.search-box input::placeholder { color: var(--text-placeholder); }
.filter-group { display: flex; gap: .5rem; position: relative; }

.dropdown-backdrop { position: fixed; inset: 0; z-index: 10; }
.custom-select { position: relative; z-index: 11; flex: 1; }
.select-trigger { display: flex; align-items: center; justify-content: space-between; padding: .6rem .75rem; border-radius: 10px; border: 1px solid var(--border-input); background: var(--surface-input); font-size: .8rem; font-weight: 750; color: var(--text-label); cursor: pointer; transition: all .2s; user-select: none; }
.select-trigger:hover { border-color: var(--border-input-focus); color: var(--text-link); }
.select-menu { position: absolute; top: calc(100% + .5rem); left: 0; width: 100%; min-width: 140px; background: var(--surface-dropdown); backdrop-filter: blur(var(--glass-blur)) saturate(140%); border: 1px solid var(--border-card); border-radius: 14px; padding: .35rem; box-shadow: var(--lg-shadow-md); display: flex; flex-direction: column; gap: .2rem; overflow: hidden; }
.select-option { padding: .55rem .65rem; border-radius: 10px; font-size: .8rem; font-weight: 650; color: var(--text-label); cursor: pointer; transition: all .15s; }
.select-option:hover { background: var(--surface-input); color: var(--text-link); }
.select-option.selected { background: var(--accent-primary-soft); color: var(--text-link); font-weight: 800; }

.ticket-cards { flex: 1; overflow-y: auto; padding: .75rem; display: flex; flex-direction: column; gap: .5rem; }
.ticket-card {
  background: var(--surface-input);
  border: 1px solid var(--border-card);
  border-radius: 12px; padding: .8rem; cursor: pointer;
  transition: all .2s;
}
.ticket-card:hover { border-color: var(--border-input-focus); }
.ticket-card.active-card { background: var(--accent-primary-soft); border-color: var(--border-input-focus); box-shadow: var(--lg-shadow-sm); }

.tc-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: .5rem; }
.tc-id { font-size: .72rem; font-weight: 800; color: var(--text-label); background: var(--surface-card); padding: .2rem .5rem; border-radius: 6px; }
.tc-title { color: var(--text-heading); font-size: .9rem; font-weight: 850; margin: 0 0 .5rem; line-height: 1.3; }
.tc-meta { display: flex; gap: .5rem; font-size: .72rem; color: var(--text-placeholder); font-weight: 700; }

.empty-list { text-align: center; padding: 2rem; color: var(--text-placeholder); font-size: .85rem; }

/* Right Panel */
.ticket-detail-panel {
  flex: 1;
  background: var(--surface-card);
  border: 1px solid var(--border-card);
  border-radius: 18px;
  display: flex;
  flex-direction: column;
  box-shadow: var(--lg-shadow-sm);
  backdrop-filter: blur(calc(var(--glass-blur) - 4px)) saturate(130%);
  overflow: hidden;
}

.empty-detail { flex: 1; display: flex; flex-direction: column; align-items: center; justify-content: center; color: var(--text-label); text-align: center; padding: 2rem; }
.empty-icon { color: var(--text-placeholder); margin-bottom: 1rem; }
.empty-detail h3 { font-size: 1.1rem; font-weight: 850; color: var(--text-heading); margin-bottom: .5rem; }

.detail-header { padding: 1rem; border-bottom: 1px solid var(--border-card); background: var(--surface-card); }
.detail-title { margin: 0; color: var(--text-heading); font-size: 1.05rem; font-weight: 900; }
.detail-subtitle { margin-top: .25rem; color: var(--text-placeholder); font-size: .8rem; font-weight: 650; }
.detail-info-bar { display: flex; gap: 1rem; margin-top: .8rem; padding: .65rem; background: var(--surface-input); border-radius: 10px; font-size: .78rem; border: 1px solid var(--border-card); flex-wrap: wrap; }
.info-item { display: flex; align-items: center; gap: .4rem; color: var(--text-label); }
.deadline-text { color: var(--color-warning-text); }

.detail-body { flex: 1; display: flex; overflow: hidden; }

/* Chat Area */
.chat-area { flex: 1; padding: 1rem; overflow-y: auto; display: flex; flex-direction: column; gap: .8rem; background: var(--surface-input); }
.chat-msg { display: flex; flex-direction: column; max-width: 80%; }
.msg-me { align-self: flex-end; align-items: flex-end; }
.msg-agent { align-self: flex-start; align-items: flex-start; }
.msg-bubble { padding: .7rem .85rem; border-radius: 14px; font-size: .86rem; line-height: 1.42; box-shadow: var(--lg-shadow-sm); }
.msg-me .msg-bubble { background: var(--accent-primary); color: var(--text-inverse); border-bottom-right-radius: 4px; }
.msg-agent .msg-bubble { background: var(--surface-card); color: var(--text-heading); border: 1px solid var(--border-card); border-bottom-left-radius: 4px; }
.msg-time { font-size: .68rem; color: var(--text-placeholder); margin-top: .25rem; font-weight: 650; }

/* Timeline Sidebar */
.timeline-sidebar { width: 250px; padding: 1rem; border-left: 1px solid var(--border-card); background: var(--surface-card); overflow-y: auto; }
.timeline-title { display: flex; align-items: center; gap: .4rem; margin: 0 0 .8rem; color: var(--text-label); font-size: .82rem; font-weight: 850; }
.timeline { position: relative; padding-left: 8px; }
.timeline::before { content: ''; position: absolute; left: 11px; top: 0; bottom: 0; width: 2px; background: var(--border-card); }
.tl-item { position: relative; margin-bottom: 1rem; }
.tl-dot { position: absolute; left: 0; top: 4px; width: 8px; height: 8px; border-radius: 50%; background: var(--accent-primary); border: 2px solid var(--surface-card); box-sizing: content-box; }
.tl-content { padding-left: 1.5rem; }
.tl-action { font-size: .8rem; font-weight: 760; color: var(--text-label); }
.tl-time { font-size: .68rem; color: var(--text-placeholder); margin-top: .1rem; }

/* Footer */
.detail-footer { padding: .85rem 1rem; border-top: 1px solid var(--border-card); background: var(--surface-card); display: flex; align-items: center; gap: .65rem; }
.closed-ticket-note { width: 100%; border: 1px solid var(--border-card); border-radius: 12px; background: var(--surface-input); color: var(--text-label); padding: .65rem; text-align: center; }
.chat-input { flex: 1; padding: .65rem 1rem; border-radius: 999px; border: 1px solid var(--border-input); background: var(--surface-input); color: var(--text-label); font-size: .88rem; outline: none; transition: border-color .2s; }
.chat-input:focus { border-color: var(--border-input-focus); background: var(--surface-card); }

/* Badges */
.status-badge { display: inline-flex; align-items: center; gap: .3rem; font-size: .65rem; font-weight: 700; padding: .15rem .5rem; border-radius: 99px; text-transform: uppercase; }
.status-badge.lg { padding: .3rem .75rem; font-size: .75rem; }
.badge-blue { background: var(--color-info-bg); color: var(--color-info-text); }
.badge-red { background: var(--color-danger-bg); color: var(--color-danger-text); }
.badge-green { background: var(--color-success-bg); color: var(--color-success-text); }
.badge-amber { background: var(--color-warning-bg); color: var(--color-warning-text); }
.badge-slate { background: var(--surface-input); color: var(--text-placeholder); }

/* Buttons */
.btn-primary, .btn-secondary, .btn-outline { display: inline-flex; align-items: center; justify-content: center; gap: .4rem; padding: .6rem 1.2rem; border-radius: 10px; font-size: .8125rem; font-weight: 700; cursor: pointer; border: none; transition: all .15s; outline: none; }
.btn-primary { background: var(--accent-primary); color: var(--text-inverse); box-shadow: var(--lg-shadow-sm); }
.btn-primary:hover:not(:disabled) { transform: translateY(-1px); }
.btn-primary:disabled { opacity: .6; cursor: not-allowed; }
.btn-secondary { background: var(--surface-input); color: var(--text-label); border: 1px solid var(--border-input); }
.btn-secondary:hover { border-color: var(--border-input-focus); color: var(--text-link); }
.btn-outline { background: var(--surface-input); color: var(--text-label); border: 1px solid var(--border-input); }
.btn-outline:hover { color: var(--text-link); border-color: var(--border-input-focus); }
.btn-icon { width: 36px; height: 36px; border-radius: 50%; background: transparent; border: none; color: var(--text-placeholder); cursor: pointer; display: flex; align-items: center; justify-content: center; transition: background .2s; }
.btn-icon:hover { background: var(--surface-input); color: var(--text-heading); }

/* Modals */
.modal-overlay { position: fixed; inset: 0; z-index: 9998; background: color-mix(in srgb, var(--lg-bg-mid) 58%, transparent); backdrop-filter: blur(6px); display: flex; align-items: center; justify-content: center; padding: 1rem; }
.modal-content { position: relative; z-index: 9999; background: var(--surface-modal); width: 100%; border-radius: 22px; box-shadow: var(--lg-shadow-lg); overflow: hidden; border: 1px solid var(--border-card); }
.modal-content.lg { max-width: 600px; }
.modal-content.sm { max-width: 400px; }
.modal-header { padding: 1rem; border-bottom: 1px solid var(--border-card); display: flex; justify-content: space-between; align-items: center; }
.modal-header h3 { margin: 0; font-size: 1rem; font-weight: 900; color: var(--text-heading); }
.close-btn-sm { background: transparent; border: none; color: var(--text-placeholder); cursor: pointer; display: flex; transition: color .15s; }
.close-btn-sm:hover { color: var(--color-danger-text); }
.modal-body { padding: 1rem; display: flex; flex-direction: column; }
.modal-footer { padding: 1rem; border-top: 1px solid var(--border-card); display: flex; justify-content: flex-end; gap: .75rem; background: var(--surface-input); }

.form-group label { display: block; color: var(--text-label); font-size: .8125rem; font-weight: 800; margin-bottom: .4rem; }
.input-glass { width: 100%; padding: .6rem 1rem; border-radius: 10px; border: 1px solid var(--border-input); background: var(--surface-input); color: var(--text-label); font-size: .875rem; outline: none; transition: border-color .2s; }
.input-glass:focus { border-color: var(--border-input-focus); }

.upload-box { border: 2px dashed var(--border-input); border-radius: 12px; padding: 1.5rem; text-align: center; background: var(--surface-input); cursor: pointer; display: flex; flex-direction: column; align-items: center; color: var(--text-label); font-size: .8125rem; transition: background .2s; }
.upload-box:hover { border-color: var(--border-input-focus); color: var(--text-link); }
.upload-icon { color: var(--text-placeholder); margin-bottom: .5rem; }

.ai-notice,
.faq-heading { display: flex; gap: .5rem; background: var(--accent-violet-soft); border: 1px solid color-mix(in srgb, var(--accent-violet) 18%, transparent); padding: .75rem 1rem; border-radius: 10px; color: var(--accent-violet); font-size: .8125rem; font-weight: 750; align-items: center; }

.faq-suggestions { background: var(--surface-input); border-radius: 12px; padding: 1rem; border: 1px solid var(--border-card); }
.faq-item { background: var(--surface-card); padding: .8rem; border-radius: 8px; margin-bottom: .5rem; border: 1px solid var(--border-card); }
.faq-item h4 { margin: 0 0 .25rem; font-size: .86rem; color: var(--text-heading); }
.faq-item p { margin: 0; font-size: .82rem; color: var(--text-label); }
.modal-muted,
.rating-copy { color: var(--text-label); font-size: .85rem; }

.stars-container { display: flex; justify-content: center; gap: .5rem; }
.star-icon { color: var(--text-placeholder); cursor: pointer; transition: color .2s, transform .2s; }
.star-icon:hover { transform: scale(1.1); }
.star-icon.active { color: var(--color-warning-text); fill: var(--color-warning-text); }

.modal-enter-active, .modal-leave-active { transition: all .3s cubic-bezier(0.16,1,.3,1); }
.modal-enter-from, .modal-leave-to { opacity: 0; transform: scale(0.95); }

@media (max-width: 1024px) {
  .master-detail-layout { flex-direction: column; }
  .ticket-list-panel { width: 100%; max-height: 400px; }
  .timeline-sidebar { display: none; }
}
</style>
