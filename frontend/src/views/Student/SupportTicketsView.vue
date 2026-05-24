<script setup>
import { ref, computed } from 'vue'
import {
  LifeBuoy, Search, Filter, Plus, Send, Clock, 
  CheckCircle2, XCircle, AlertCircle, Paperclip,
  Star, ChevronRight, MessageSquare, Bot, FileText, X, ChevronDown
} from 'lucide-vue-next'

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
  alert('Cảm ơn bạn đã đánh giá chất lượng phục vụ!')
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
            <Search :size="16" class="text-slate-400" />
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
                <h2 class="text-xl font-bold text-slate-900">{{ activeTicket.title }}</h2>
                <div class="text-sm text-slate-500 mt-1">Mã: {{ activeTicket.id }} • Tạo lúc: {{ formatDate(activeTicket.createdAt) }}</div>
              </div>
              <span class="status-badge lg" :class="statusConfig[activeTicket.status].cls">
                {{ statusConfig[activeTicket.status].label }}
              </span>
            </div>
            
            <div class="detail-info-bar">
              <div class="info-item"><FileText :size="14"/> Danh mục: <strong>{{ activeTicket.category }}</strong></div>
              <div class="info-item"><Bot :size="14"/> Xử lý: <strong>{{ activeTicket.assignedTo }}</strong></div>
              <div class="info-item"><Clock :size="14"/> Deadline: <strong class="text-amber-600">{{ formatDate(activeTicket.deadline) }}</strong></div>
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
              <h4 class="text-sm font-bold text-slate-700 mb-3 flex items-center gap-2"><Clock :size="14"/> Lịch sử xử lý</h4>
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
              <div class="w-full text-center py-2 text-slate-500 bg-slate-50 rounded-xl border border-slate-200">
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
          <MessageSquare :size="48" class="text-slate-300 mb-4" />
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
                  <div class="flex items-center gap-2 text-violet-700 font-semibold mb-2">
                    <Bot :size="18"/> Có thể những bài viết này sẽ giúp ích cho bạn:
                  </div>
                  <div v-for="(faq, i) in aiSuggestions" :key="i" class="faq-item">
                    <h4>{{ faq.question }}</h4>
                    <p>{{ faq.answer }}</p>
                  </div>
                  <div class="mt-4 text-center">
                    <span class="text-slate-500 text-sm">Vẫn chưa giải quyết được?</span>
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
                    <Paperclip :size="20" class="text-slate-400 mb-2"/>
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
              <p class="text-slate-600 mb-4">Bạn đánh giá thế nào về sự hỗ trợ của chuyên viên <strong>{{ activeTicket?.assignedTo }}</strong>?</p>
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
  padding: 1.5rem 2rem;
  max-width: 1500px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  color: #0f172a;
  height: calc(100vh - 4rem); /* Ensure full height for master-detail */
}

.page-header { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; flex-wrap: wrap; flex-shrink: 0; }
.eyebrow { display: flex; align-items: center; gap: .375rem; font-size: .7rem; font-weight: 700; text-transform: uppercase; letter-spacing: .08em; color: #2563eb; margin-bottom: .4rem; }
.page-title { font-size: 1.875rem; font-weight: 800; margin: 0 0 .25rem; letter-spacing: -.02em; }
.page-sub { font-size: .875rem; color: #64748b; margin: 0; }

/* Master Detail Layout */
.master-detail-layout {
  display: flex;
  gap: 1.5rem;
  flex: 1;
  min-height: 0; /* Important for scrollable children */
}

/* Left Panel */
.ticket-list-panel {
  width: 380px;
  background: rgba(255,255,255,.72);
  border: 1px solid rgba(255,255,255,.5);
  border-radius: 20px;
  display: flex;
  flex-direction: column;
  box-shadow: 0 4px 20px rgba(15,23,42,.05);
  backdrop-filter: saturate(160%) blur(16px);
  overflow: hidden;
}

.toolbar { padding: 1rem; border-bottom: 1px solid rgba(148,163,184,.15); display: flex; flex-direction: column; gap: .75rem; }
.search-box { display: flex; align-items: center; gap: .5rem; background: rgba(248,250,252,.8); border-radius: 10px; padding: 0 1rem; border: 1px solid rgba(148,163,184,.2); }
.search-box input { background: transparent; border: none; outline: none; padding: .6rem 0; width: 100%; font-size: .875rem; }
.filter-group { display: flex; gap: .5rem; position: relative; }

.dropdown-backdrop { position: fixed; inset: 0; z-index: 10; }
.custom-select { position: relative; z-index: 11; flex: 1; }
.select-trigger { display: flex; align-items: center; justify-content: space-between; padding: .6rem .75rem; border-radius: 10px; border: 1px solid rgba(148,163,184,.3); background: rgba(248,250,252,.8); font-size: .8125rem; font-weight: 600; color: #475569; cursor: pointer; transition: all .2s; user-select: none; }
.select-trigger:hover { background: #fff; border-color: #2563eb; color: #2563eb; }
.select-menu { position: absolute; top: calc(100% + .5rem); left: 0; width: 100%; min-width: 140px; background: rgba(255,255,255,.95); backdrop-filter: saturate(180%) blur(24px); border: 1px solid rgba(148,163,184,.2); border-radius: 16px; padding: .4rem; box-shadow: 0 10px 30px rgba(15,23,42,.12); display: flex; flex-direction: column; gap: .2rem; overflow: hidden; }
.select-option { padding: .6rem .75rem; border-radius: 10px; font-size: .8125rem; font-weight: 500; color: #374151; cursor: pointer; transition: all .15s; }
.select-option:hover { background: rgba(248,250,252,.9); color: #2563eb; padding-left: 1rem; }
.select-option.selected { background: rgba(37,99,235,.08); color: #1d4ed8; font-weight: 700; }

.ticket-cards { flex: 1; overflow-y: auto; padding: .75rem; display: flex; flex-direction: column; gap: .5rem; }
.ticket-card {
  background: rgba(255,255,255,.6);
  border: 1px solid rgba(148,163,184,.2);
  border-radius: 12px; padding: 1rem; cursor: pointer;
  transition: all .2s;
}
.ticket-card:hover { background: #fff; border-color: rgba(37,99,235,.3); }
.ticket-card.active-card { background: #fff; border-color: #2563eb; box-shadow: 0 4px 12px rgba(37,99,235,.1); }

.tc-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: .5rem; }
.tc-id { font-size: .75rem; font-weight: 700; color: #64748b; background: rgba(248,250,252,.9); padding: .2rem .5rem; border-radius: 6px; }
.tc-title { font-size: .95rem; font-weight: 700; margin: 0 0 .5rem; line-height: 1.3; }
.tc-meta { display: flex; gap: .5rem; font-size: .75rem; color: #94a3b8; font-weight: 500; }

.empty-list { text-align: center; padding: 2rem; color: #64748b; font-size: .875rem; font-style: italic; }

/* Right Panel */
.ticket-detail-panel {
  flex: 1;
  background: rgba(255,255,255,.85);
  border: 1px solid rgba(255,255,255,.6);
  border-radius: 20px;
  display: flex;
  flex-direction: column;
  box-shadow: 0 4px 20px rgba(15,23,42,.05);
  backdrop-filter: saturate(160%) blur(16px);
  overflow: hidden;
}

.empty-detail { flex: 1; display: flex; flex-direction: column; align-items: center; justify-content: center; color: #64748b; text-align: center; padding: 2rem; }
.empty-detail h3 { font-size: 1.25rem; font-weight: 700; color: #0f172a; margin-bottom: .5rem; }

.detail-header { padding: 1.25rem 1.5rem; border-bottom: 1px solid rgba(148,163,184,.15); background: #fff; }
.detail-info-bar { display: flex; gap: 1.5rem; margin-top: 1rem; padding: .75rem; background: rgba(248,250,252,.8); border-radius: 10px; font-size: .8125rem; border: 1px solid rgba(148,163,184,.15); flex-wrap: wrap; }
.info-item { display: flex; align-items: center; gap: .4rem; color: #475569; }

.detail-body { flex: 1; display: flex; overflow: hidden; }

/* Chat Area */
.chat-area { flex: 1; padding: 1.5rem; overflow-y: auto; display: flex; flex-direction: column; gap: 1rem; background: rgba(248,250,252,.3); }
.chat-msg { display: flex; flex-direction: column; max-width: 80%; }
.msg-me { align-self: flex-end; align-items: flex-end; }
.msg-agent { align-self: flex-start; align-items: flex-start; }
.msg-bubble { padding: .75rem 1rem; border-radius: 16px; font-size: .9rem; line-height: 1.4; box-shadow: 0 2px 4px rgba(15,23,42,.03); }
.msg-me .msg-bubble { background: #2563eb; color: #fff; border-bottom-right-radius: 4px; }
.msg-agent .msg-bubble { background: #fff; color: #0f172a; border: 1px solid rgba(148,163,184,.2); border-bottom-left-radius: 4px; }
.msg-time { font-size: .7rem; color: #94a3b8; margin-top: .25rem; font-weight: 500; }

/* Timeline Sidebar */
.timeline-sidebar { width: 260px; padding: 1.5rem; border-left: 1px solid rgba(148,163,184,.15); background: #fff; overflow-y: auto; }
.timeline { position: relative; padding-left: 8px; }
.timeline::before { content: ''; position: absolute; left: 11px; top: 0; bottom: 0; width: 2px; background: rgba(148,163,184,.2); }
.tl-item { position: relative; margin-bottom: 1rem; }
.tl-dot { position: absolute; left: 0; top: 4px; width: 8px; height: 8px; border-radius: 50%; background: #2563eb; border: 2px solid #fff; box-sizing: content-box; }
.tl-content { padding-left: 1.5rem; }
.tl-action { font-size: .8125rem; font-weight: 600; color: #374151; }
.tl-time { font-size: .7rem; color: #94a3b8; margin-top: .1rem; }

/* Footer */
.detail-footer { padding: 1rem 1.5rem; border-top: 1px solid rgba(148,163,184,.15); background: #fff; display: flex; align-items: center; gap: .75rem; }
.chat-input { flex: 1; padding: .7rem 1rem; border-radius: 99px; border: 1px solid rgba(148,163,184,.3); background: rgba(248,250,252,.8); font-size: .9rem; outline: none; transition: border-color .2s; }
.chat-input:focus { border-color: #2563eb; background: #fff; }

/* Badges */
.status-badge { display: inline-flex; align-items: center; gap: .3rem; font-size: .65rem; font-weight: 700; padding: .15rem .5rem; border-radius: 99px; text-transform: uppercase; }
.status-badge.lg { padding: .3rem .75rem; font-size: .75rem; }
.badge-blue { background: rgba(37,99,235,.15); color: #1d4ed8; }
.badge-red { background: rgba(220,38,38,.15); color: #b91c1c; }
.badge-green { background: rgba(22,163,74,.15); color: #15803d; }
.badge-amber { background: rgba(217,119,6,.15); color: #b45309; }
.badge-slate { background: rgba(148,163,184,.15); color: #475569; }

/* Buttons */
.btn-primary, .btn-secondary, .btn-outline { display: inline-flex; align-items: center; justify-content: center; gap: .4rem; padding: .6rem 1.2rem; border-radius: 10px; font-size: .8125rem; font-weight: 700; cursor: pointer; border: none; transition: all .15s; outline: none; }
.btn-primary { background: #2563eb; color: #fff; box-shadow: 0 4px 14px rgba(37,99,235,.25); }
.btn-primary:hover:not(:disabled) { background: #1d4ed8; transform: translateY(-1px); }
.btn-primary:disabled { opacity: .6; cursor: not-allowed; }
.btn-secondary { background: rgba(248,250,252,.9); color: #475569; border: 1px solid rgba(148,163,184,.3); }
.btn-secondary:hover { border-color: #94a3b8; color: #0f172a; }
.btn-outline { background: rgba(255,255,255,.7); color: #374151; border: 1px solid rgba(148,163,184,.3); }
.btn-outline:hover { color: #2563eb; border-color: #2563eb; }
.btn-icon { width: 36px; height: 36px; border-radius: 50%; background: transparent; border: none; color: #64748b; cursor: pointer; display: flex; align-items: center; justify-content: center; transition: background .2s; }
.btn-icon:hover { background: rgba(148,163,184,.15); color: #0f172a; }

/* Modals */
.modal-overlay { position: fixed; inset: 0; z-index: 1000; background: rgba(15,23,42,.4); backdrop-filter: blur(6px); display: flex; align-items: center; justify-content: center; padding: 1rem; }
.modal-content { background: rgba(255,255,255,.95); backdrop-filter: saturate(180%) blur(24px); width: 100%; border-radius: 24px; box-shadow: 0 24px 80px rgba(2,6,23,.32); overflow: hidden; border: 1px solid rgba(255,255,255,.5); }
.modal-content.lg { max-width: 600px; }
.modal-content.sm { max-width: 400px; }
.modal-header { padding: 1.25rem 1.5rem; border-bottom: 1px solid rgba(148,163,184,.15); display: flex; justify-content: space-between; align-items: center; }
.modal-header h3 { margin: 0; font-size: 1.1rem; font-weight: 800; color: #0f172a; }
.close-btn-sm { background: transparent; border: none; color: #94a3b8; cursor: pointer; display: flex; transition: color .15s; }
.close-btn-sm:hover { color: #ef4444; }
.modal-body { padding: 1.5rem; display: flex; flex-direction: column; }
.modal-footer { padding: 1.25rem 1.5rem; border-top: 1px solid rgba(148,163,184,.15); display: flex; justify-content: flex-end; gap: .75rem; background: rgba(248,250,252,.5); }

.form-group label { display: block; font-size: .8125rem; font-weight: 700; margin-bottom: .4rem; }
.input-glass { width: 100%; padding: .6rem 1rem; border-radius: 10px; border: 1px solid rgba(148,163,184,.4); background: rgba(255,255,255,.6); font-size: .875rem; outline: none; transition: border-color .2s; }
.input-glass:focus { border-color: #2563eb; }

.upload-box { border: 2px dashed rgba(148,163,184,.4); border-radius: 12px; padding: 2rem; text-align: center; background: rgba(248,250,252,.5); cursor: pointer; display: flex; flex-direction: column; align-items: center; color: #64748b; font-size: .8125rem; transition: background .2s; }
.upload-box:hover { background: rgba(248,250,252,.9); border-color: #2563eb; color: #2563eb; }

.ai-notice { display: flex; gap: .5rem; background: rgba(124,58,237,.1); border: 1px solid rgba(124,58,237,.2); padding: .75rem 1rem; border-radius: 10px; color: #6d28d9; font-size: .8125rem; font-weight: 600; align-items: center; }

.faq-suggestions { background: rgba(248,250,252,.8); border-radius: 12px; padding: 1rem; border: 1px solid rgba(148,163,184,.2); }
.faq-item { background: #fff; padding: 1rem; border-radius: 8px; margin-bottom: .5rem; border: 1px solid rgba(148,163,184,.2); }
.faq-item h4 { margin: 0 0 .25rem; font-size: .9rem; color: #0f172a; }
.faq-item p { margin: 0; font-size: .85rem; color: #475569; }

.stars-container { display: flex; justify-content: center; gap: .5rem; }
.star-icon { color: #cbd5e1; cursor: pointer; transition: color .2s, transform .2s; }
.star-icon:hover { transform: scale(1.1); }
.star-icon.active { color: #f59e0b; fill: #f59e0b; }

.modal-enter-active, .modal-leave-active { transition: all .3s cubic-bezier(0.16,1,.3,1); }
.modal-enter-from, .modal-leave-to { opacity: 0; transform: scale(0.95); }

@media (max-width: 1024px) {
  .master-detail-layout { flex-direction: column; }
  .ticket-list-panel { width: 100%; max-height: 400px; }
  .timeline-sidebar { display: none; }
}
</style>
