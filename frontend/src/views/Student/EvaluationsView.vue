<script setup>
import { ref, computed } from 'vue'
import { useBodyScrollLock } from '@/composables/useBodyScrollLock'
import {
  MessageSquareHeart, AlertTriangle, ShieldCheck, 
  CheckCircle2, Edit3, Star, User, BookOpen, Send,
  X, AlertCircle, Sparkles, ChevronDown, Bot
} from 'lucide-vue-next'

// Mock Data
const semesters = ['Kỳ Spring 2026', 'Kỳ Fall 2025', 'Kỳ Summer 2025']
const selectedSemester = ref('Kỳ Spring 2026')
const semesterOpen = ref(false)

const mockEvaluations = ref([
  { 
    id: 'EVAL-001', subject: 'Cấu trúc dữ liệu & Giải thuật', teacher: 'TS. Nguyễn Minh Khoa', 
    status: 'Pending', editsLeft: 2, 
    ratings: { r1: 0, r2: 0, r3: 0 }, feedback: '' 
  },
  { 
    id: 'EVAL-002', subject: 'Toán rời rạc', teacher: 'ThS. Trần Thu Hà', 
    status: 'Completed', editsLeft: 1, 
    ratings: { r1: 5, r2: 4, r3: 5 }, feedback: 'Cô giảng bài rất nhiệt tình và dễ hiểu.' 
  },
  { 
    id: 'EVAL-003', subject: 'Lập trình Web', teacher: 'KS. Lê Văn Tâm', 
    status: 'Pending', editsLeft: 2, 
    ratings: { r1: 0, r2: 0, r3: 0 }, feedback: '' 
  }
])

const criteriaList = [
  { key: 'r1', label: '1. Đảm bảo thời gian và nội dung môn học', desc: 'Giảng viên đến lớp đúng giờ, dạy đủ thời lượng và bám sát đề cương.' },
  { key: 'r2', label: '2. Kỹ năng sư phạm và truyền đạt', desc: 'Phương pháp giảng dạy lôi cuốn, dễ hiểu, kết hợp lý thuyết và thực hành.' },
  { key: 'r3', label: '3. Thái độ và Hỗ trợ sinh viên', desc: 'Nhiệt tình giải đáp thắc mắc, tôn trọng và công bằng với sinh viên.' }
]

// State
const activeEval = ref(null)
const evalModalOpen = ref(false)
const confirmModalOpen = ref(false)
const anyModalOpen = computed(() => evalModalOpen.value || confirmModalOpen.value)
useBodyScrollLock(anyModalOpen)
const isSubmitting = ref(false)

// Computed
const filteredEvals = computed(() => mockEvaluations.value) // In real app, filter by selectedSemester
const pendingCount = computed(() => mockEvaluations.value.filter(e => e.status === 'Pending').length)

// Actions
const openEvalModal = (ev) => {
  activeEval.value = JSON.parse(JSON.stringify(ev)) // Deep copy to isolate changes
  evalModalOpen.value = true
}

const closeEvalModal = () => {
  evalModalOpen.value = false
  activeEval.value = null
}

const setRating = (key, val) => {
  if (activeEval.value) {
    activeEval.value.ratings[key] = val
  }
}

const submitEvaluation = () => {
  // Open confirm modal first to guarantee anonymity
  confirmModalOpen.value = true
}

const confirmSubmit = () => {
  isSubmitting.value = true
  
  setTimeout(() => {
    // Find and update the original evaluation
    const idx = mockEvaluations.value.findIndex(e => e.id === activeEval.value.id)
    if (idx !== -1) {
      const original = mockEvaluations.value[idx]
      original.status = 'Completed'
      if (original.feedback !== '') { // If editing
        original.editsLeft -= 1
      }
      original.ratings = { ...activeEval.value.ratings }
      original.feedback = activeEval.value.feedback
    }
    
    isSubmitting.value = false
    confirmModalOpen.value = false
    evalModalOpen.value = false
    activeEval.value = null
  }, 1000)
}

const getCompletionColor = (status) => status === 'Completed' ? 'completion-dot--done' : 'completion-dot--pending'

</script>

<template>
  <div class="eval-page">
    <!-- Header -->
    <div class="page-header">
      <div>
        <div class="eyebrow"><MessageSquareHeart :size="15"/>Đảm bảo chất lượng</div>
        <h1 class="page-title">Đánh giá Giảng viên</h1>
        <p class="page-sub">Phản hồi ẩn danh về chất lượng giảng dạy. Góp ý của bạn giúp cải thiện chất lượng đào tạo.</p>
      </div>
      
      <div class="custom-select-wrapper">
        <div v-if="semesterOpen" class="dropdown-backdrop" @click="semesterOpen = false"></div>
        <div class="custom-select">
          <div class="select-trigger" @click="semesterOpen = !semesterOpen">
            {{ selectedSemester }}
            <ChevronDown :size="14" class="ml-2" />
          </div>
          <Transition name="fade">
            <div class="select-menu" v-if="semesterOpen">
              <div v-for="s in semesters" :key="s" class="select-option" :class="{'selected': selectedSemester === s}" @click="selectedSemester = s; semesterOpen = false">
                {{ s }}
              </div>
            </div>
          </Transition>
        </div>
      </div>
    </div>

    <!-- Blocker Warning -->
    <div v-if="pendingCount > 0" class="warning-banner blocker-warning">
      <div class="warning-icon"><AlertTriangle :size="24"/></div>
      <div class="warning-content">
        <h3>Bắt buộc hoàn thành đánh giá (Còn {{ pendingCount }} môn)</h3>
        <p>Hệ thống sẽ <strong>tạm khóa chức năng Xem điểm thi và Đăng ký môn học</strong> kỳ tiếp theo cho đến khi bạn hoàn thành toàn bộ phiếu đánh giá trong kỳ hiện tại.</p>
      </div>
    </div>

    <!-- Privacy Guarantee -->
    <div class="privacy-banner">
      <ShieldCheck :size="20" class="icon-privacy shrink-0"/>
      <div class="text-sm text-privacy">
        <strong>Cam kết Ẩn danh tuyệt đối:</strong> Dữ liệu đánh giá của bạn được mã hóa một chiều. Giảng viên chỉ nhận được báo cáo tổng hợp từ hệ thống khi lớp có từ 5 lượt đánh giá trở lên. Hệ thống không lưu ID sinh viên kèm theo nội dung đánh giá.
      </div>
    </div>

    <!-- Evaluation Cards Grid -->
    <div class="eval-grid">
      <div v-for="ev in filteredEvals" :key="ev.id" class="eval-card" :class="{'card-completed': ev.status === 'Completed'}">
        <div class="ec-header">
          <span class="ec-subject">{{ ev.subject }}</span>
          <div class="status-indicator" :class="ev.status === 'Completed' ? 'status-done' : 'status-pending'">
            <CheckCircle2 v-if="ev.status === 'Completed'" :size="16"/>
            <AlertCircle v-else :size="16"/>
            <span class="text-xs font-bold uppercase">{{ ev.status === 'Completed' ? 'Hoàn thành' : 'Chưa đánh giá' }}</span>
          </div>
        </div>
        
        <div class="ec-body">
            <div class="flex items-center gap-2 text-sm font-semibold mb-2 text-label">
            <User :size="16" class="icon-teacher"/>
            {{ ev.teacher }}
          </div>
          <p class="text-xs edits-remaining italic" v-if="ev.status === 'Completed'">Bạn còn {{ ev.editsLeft }} lần chỉnh sửa.</p>
        </div>

        <div class="ec-footer">
          <button v-if="ev.status === 'Pending'" class="btn-primary w-full justify-center" @click="openEvalModal(ev)">
            <Edit3 :size="15"/> Thực hiện đánh giá
          </button>
          <button v-else-if="ev.editsLeft > 0" class="btn-outline w-full justify-center" @click="openEvalModal(ev)">
            <Edit3 :size="15"/> Chỉnh sửa đánh giá
          </button>
          <button v-else class="btn-secondary w-full justify-center" disabled>
            Hết lượt chỉnh sửa
          </button>
        </div>
      </div>
    </div>

    <!-- Modals via Teleport -->
    <Teleport to="body">
      
      <!-- Evaluation Form Modal -->
      <Transition name="modal">
        <div v-if="evalModalOpen" class="modal-overlay" @click.self="closeEvalModal">
          <div class="modal-content lg">
            <div class="modal-header">
              <h3>Phiếu đánh giá Giảng viên</h3>
              <button class="close-btn-sm" @click="closeEvalModal"><X :size="20"/></button>
            </div>
            
            <div class="modal-body">
              <div class="eval-target-info">
                <div class="font-semibold text-lg text-heading">{{ activeEval.subject }}</div>
                <div class="text-sm text-muted flex items-center gap-1 mt-1"><User :size="14"/> Giảng viên: <strong>{{ activeEval.teacher }}</strong></div>
              </div>

              <div class="criteria-list mt-6">
                <div v-for="crit in criteriaList" :key="crit.key" class="criterion-item">
                  <div class="crit-text">
                    <h4 class="crit-label">{{ crit.label }}</h4>
                    <p class="crit-desc">{{ crit.desc }}</p>
                  </div>
                  <div class="crit-stars">
                    <Star v-for="i in 5" :key="i" :size="28" 
                          class="star-btn" :class="{'active': activeEval.ratings[crit.key] >= i}" 
                          @click="setRating(crit.key, i)" />
                  </div>
                </div>
              </div>

              <div class="feedback-section mt-6">
                <h4 class="crit-label mb-2">Nhận xét chi tiết (Feedback)</h4>
                <textarea v-model="activeEval.feedback" class="input-glass" rows="4" placeholder="Nhập những góp ý, nhận xét tự do của bạn. Giảng viên sẽ đọc được nội dung này (ẩn danh)."></textarea>
                
                <div class="ai-notice mt-3">
                  <Bot :size="16" class="shrink-0"/>
                  Hệ thống AI (Sentiment Analysis & Topic Modeling) sẽ tự động phân tích cảm xúc và phân loại ý kiến của bạn để báo cáo tổng hợp lên Ban Giám Hiệu.
                </div>
              </div>
            </div>
            
            <div class="modal-footer">
              <button class="btn-secondary" @click="closeEvalModal">Hủy</button>
              <button class="btn-primary" @click="submitEvaluation" :disabled="!activeEval.ratings.r1 || !activeEval.ratings.r2 || !activeEval.ratings.r3">
                <Send :size="15"/> Tiếp tục
              </button>
            </div>
          </div>
        </div>
      </Transition>

      <!-- Confirmation / Anonymous Guarantee Modal -->
      <Transition name="modal">
        <div v-if="confirmModalOpen" class="modal-overlay" @click.self="confirmModalOpen = false">
          <div class="modal-content sm">
            <div class="modal-header">
              <h3>Xác nhận Gửi Đánh Giá</h3>
              <button class="close-btn-sm" @click="confirmModalOpen = false"><X :size="20"/></button>
            </div>
            <div class="modal-body text-center">
              <ShieldCheck :size="48" class="icon-privacy-lg mx-auto mb-4" />
              <h4 class="text-lg font-semibold text-heading mb-2">Danh tính của bạn được bảo mật</h4>
              <p class="text-sm text-muted mb-4">Bạn có chắc chắn muốn gửi đánh giá này không? Nội dung đánh giá sẽ được gửi ẩn danh hoàn toàn đến hệ thống và phân tích tự động bởi AI.</p>
              
              <div class="text-xs note-box text-left">
                <strong>Lưu ý:</strong> Bạn chỉ có thể chỉnh sửa lại đánh giá tối đa 2 lần trước khi kết thúc kỳ đánh giá.
              </div>
            </div>
            <div class="modal-footer justify-center">
              <button class="btn-secondary w-full" @click="confirmModalOpen = false" :disabled="isSubmitting">Quay lại</button>
              <button class="btn-primary w-full" @click="confirmSubmit" :disabled="isSubmitting">
                {{ isSubmitting ? 'Đang gửi mã hóa...' : 'Xác nhận Gửi' }}
              </button>
            </div>
          </div>
        </div>
      </Transition>

    </Teleport>
  </div>
</template>

<style scoped>
.eval-page {
  padding: 2rem;
  max-width: 1200px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  color: var(--text-heading);
}

.page-header { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; flex-wrap: wrap; }
.eyebrow { display: flex; align-items: center; gap: .375rem; font-size: .7rem; font-weight: 700; text-transform: uppercase; letter-spacing: .08em; color: var(--text-link); margin-bottom: .4rem; }
.page-title { font-size: 1.875rem; font-weight: 800; margin: 0 0 .25rem; letter-spacing: -.02em; color: var(--text-heading); }
.page-sub { font-size: .875rem; color: var(--text-muted); margin: 0; }

/* Custom Select Dropdown */
.custom-select-wrapper { position: relative; width: 220px; }
.dropdown-backdrop { position: fixed; inset: 0; z-index: 10; }
.custom-select { position: relative; z-index: 11; width: 100%; }
.select-trigger { display: flex; align-items: center; justify-content: space-between; padding: .6rem 1rem; border-radius: 10px; border: 1px solid var(--border-input); background: var(--surface-card); font-size: .875rem; font-weight: 700; color: var(--text-heading); cursor: pointer; transition: all .2s; box-shadow: var(--lg-shadow-sm); backdrop-filter: blur(12px); }
.select-trigger:hover { background: var(--surface-card-strong); border-color: var(--text-link); color: var(--text-link); }
.select-menu { position: absolute; top: calc(100% + .5rem); right: 0; width: 100%; background: var(--surface-modal); backdrop-filter: saturate(180%) blur(24px); border: 1px solid var(--border-default); border-radius: 16px; padding: .4rem; box-shadow: 0 10px 30px color-mix(in srgb, var(--text-heading) 12%, transparent); display: flex; flex-direction: column; gap: .2rem; overflow: hidden; }
.select-option { padding: .6rem .75rem; border-radius: 10px; font-size: .8125rem; font-weight: 500; color: var(--text-body); cursor: pointer; transition: all .15s; }
.select-option:hover { background: var(--surface-solid); color: var(--text-link); padding-left: 1rem; }
.select-option.selected { background: var(--accent-primary-soft); color: var(--lg-primary-dark); font-weight: 700; }

/* Banners */
.warning-banner { display: flex; align-items: flex-start; gap: 1rem; padding: 1.25rem 1.5rem; border-radius: 16px; backdrop-filter: blur(12px); box-shadow: 0 4px 20px color-mix(in srgb, var(--color-danger-text) 10%, transparent); }
.blocker-warning { background: var(--color-danger-bg); border: 1px solid color-mix(in srgb, var(--color-danger-text) 20%, transparent); color: var(--color-danger-text); }
.warning-icon { padding-top: .1rem; }
.warning-content h3 { font-size: 1rem; font-weight: 800; margin: 0 0 .25rem; color: var(--text-heading); }
.warning-content p { font-size: .875rem; margin: 0; opacity: 0.9; }

.privacy-banner { display: flex; align-items: center; gap: .75rem; background: var(--color-success-bg); border: 1px solid color-mix(in srgb, var(--color-success-text) 20%, transparent); padding: .75rem 1rem; border-radius: 12px; }

/* Cards Grid */
.eval-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 1.25rem; }
.eval-card {
  background: var(--surface-card); border: 1px solid var(--border-card);
  border-radius: 20px; overflow: hidden; display: flex; flex-direction: column;
  box-shadow: var(--lg-shadow-sm); backdrop-filter: saturate(160%) blur(16px);
  transition: transform .2s, box-shadow .2s; border-left: 4px solid var(--color-danger-text);
}
.eval-card:hover { transform: translateY(-3px); box-shadow: var(--lg-shadow-md); }
.card-completed { border-left-color: var(--color-success-text); background: color-mix(in srgb, var(--color-success-bg) 60%, var(--surface-card)); }

.ec-header { padding: 1.25rem 1.25rem .75rem; border-bottom: 1px dashed var(--border-default); display: flex; flex-direction: column; gap: .5rem; }
.ec-subject { font-size: 1.1rem; font-weight: 800; color: var(--text-heading); line-height: 1.3; }
.status-indicator { display: flex; align-items: center; gap: .375rem; }
.status-done { color: var(--color-success-text); }
.status-pending { color: var(--text-placeholder); }

.ec-body { padding: 1rem 1.25rem; flex: 1; display: flex; flex-direction: column; gap: .25rem; }
.ec-footer { padding: 1rem 1.25rem; border-top: 1px solid var(--border-default); background: var(--surface-solid); }

/* Buttons */
.btn-primary, .btn-secondary, .btn-outline { display: inline-flex; align-items: center; gap: .4rem; padding: .6rem 1.2rem; border-radius: 10px; font-size: .8125rem; font-weight: 700; cursor: pointer; border: none; transition: all .15s; outline: none; text-decoration: none; }
.btn-primary { background: var(--text-link); color: var(--text-inverse); box-shadow: 0 4px 14px color-mix(in srgb, var(--text-link) 25%, transparent); }
.btn-primary:hover:not(:disabled) { background: var(--lg-primary-dark); transform: translateY(-1px); }
.btn-primary:disabled { opacity: .6; cursor: not-allowed; }
.btn-secondary { background: var(--surface-solid); color: var(--text-label); border: 1px solid var(--border-input); }
.btn-secondary:hover:not(:disabled) { border-color: var(--text-placeholder); color: var(--text-heading); }
.btn-secondary:disabled { opacity: .6; cursor: not-allowed; }
.btn-outline { background: var(--surface-card); color: var(--text-body); border: 1px solid var(--border-input); }
.btn-outline:hover { color: var(--text-link); border-color: var(--text-link); }

/* Modals */
.modal-overlay { position: fixed; inset: 0; z-index: 9998; background: color-mix(in srgb, var(--text-heading) 40%, transparent); backdrop-filter: blur(6px); display: flex; align-items: center; justify-content: center; padding: 1rem; }
.modal-content { position: relative; z-index: 9999; background: var(--surface-modal); backdrop-filter: saturate(180%) blur(24px); width: 100%; border-radius: 24px; box-shadow: 0 24px 80px color-mix(in srgb, var(--text-heading) 32%, transparent); overflow: hidden; border: 1px solid var(--border-card); display: flex; flex-direction: column; max-height: 90vh; }
.modal-content.lg { max-width: 680px; }
.modal-content.sm { max-width: 400px; }
.modal-header { padding: 1.25rem 1.5rem; border-bottom: 1px solid var(--border-default); display: flex; justify-content: space-between; align-items: center; flex-shrink: 0; }
.modal-header h3 { margin: 0; font-size: 1.1rem; font-weight: 800; color: var(--text-heading); }
.close-btn-sm { background: transparent; border: none; color: var(--text-placeholder); cursor: pointer; display: flex; transition: color .15s; }
.close-btn-sm:hover { color: var(--color-danger-text); }
.modal-body { padding: 1.5rem; flex: 1; overflow-y: auto; }
.modal-footer { padding: 1.25rem 1.5rem; border-top: 1px solid var(--border-default); display: flex; justify-content: flex-end; gap: .75rem; background: var(--surface-solid); flex-shrink: 0; }

/* Form Elements */
.eval-target-info { background: var(--accent-primary-soft); border: 1px solid color-mix(in srgb, var(--text-link) 20%, transparent); padding: 1rem 1.25rem; border-radius: 12px; }
.criterion-item { border-bottom: 1px dashed var(--border-default); padding: 1.25rem 0; display: flex; flex-direction: column; gap: .75rem; }
.criterion-item:last-child { border-bottom: none; }
.crit-label { font-size: .95rem; font-weight: 700; color: var(--text-heading); margin: 0 0 .25rem; }
.crit-desc { font-size: .8125rem; color: var(--text-muted); margin: 0; }
.crit-stars { display: flex; gap: .5rem; }
.star-btn { color: var(--border-default); cursor: pointer; transition: all .2s; }
.star-btn:hover { transform: scale(1.1); }
.star-btn.active { color: var(--color-warning-text); fill: var(--color-warning-text); }

.input-glass { width: 100%; padding: .75rem 1rem; border-radius: 12px; border: 1px solid var(--border-input); background: var(--surface-input); font-size: .875rem; outline: none; transition: border-color .2s; color: var(--text-body); }
.input-glass:focus { border-color: var(--border-input-focus); }

.ai-notice { display: flex; gap: .5rem; background: var(--accent-violet-soft); border: 1px solid color-mix(in srgb, var(--accent-violet) 20%, transparent); padding: .75rem 1rem; border-radius: 10px; color: var(--accent-violet); font-size: .8125rem; font-weight: 500; align-items: flex-start; }

/* Icon helper classes */
.icon-privacy { color: var(--color-success-text); }
.icon-privacy-lg { color: var(--color-success-text); }
.text-privacy { color: var(--color-success-text); }
.icon-teacher { color: var(--text-placeholder); }
.edits-remaining { color: var(--text-muted); }
.text-heading { color: var(--text-heading); }
.text-muted { color: var(--text-muted); }
.note-box { color: var(--color-warning-text); background: var(--color-warning-bg); border: 1px solid color-mix(in srgb, var(--color-warning-text) 20%, transparent); border-radius: 0.5rem; padding: 0.5rem; }

.completion-dot--done { background: var(--color-success-text); }
.completion-dot--pending { background: var(--text-placeholder); }

.modal-enter-active, .modal-leave-active { transition: all .3s cubic-bezier(0.16,1,.3,1); }
.modal-enter-from, .modal-leave-to { opacity: 0; transform: scale(0.95); }

@media (max-width: 640px) {
  .eval-page { padding: 1rem; }
  .custom-select-wrapper { width: 100%; }
}
</style>
