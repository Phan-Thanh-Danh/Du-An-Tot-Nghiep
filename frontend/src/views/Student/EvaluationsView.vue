<script setup>
import { ref, computed, onMounted } from 'vue'
import { useBodyScrollLock } from '@/composables/useBodyScrollLock'
import { usePopupStore } from '@/stores/popup'
import { studentApi } from '@/services/studentApi'
import { unwrapApiData } from '@/services/apiClient'
import {
  MessageSquareHeart, AlertTriangle, ShieldCheck,
  CheckCircle2, Edit3, Star, User, Send,
  X, AlertCircle, ChevronDown, Bot
} from 'lucide-vue-next'

const popupStore = usePopupStore()

const semesters = ['Kỳ Spring 2026', 'Kỳ Fall 2025', 'Kỳ Summer 2025']
const selectedSemester = ref('Kỳ Spring 2026')
const semesterOpen = ref(false)

const evaluations = ref([])
const loading = ref(false)
const error = ref('')

// 6 Tiêu chí đánh giá chuẩn được cấu hình sẵn trong hệ thống (Đồng bộ 100% với Giảng viên)
const criteriaList = [
  { key: 'r1', label: '1. Đảm bảo thời gian và nội dung môn học', desc: 'Giảng viên đến lớp đúng giờ, dạy đủ thời lượng và bám sát đề cương.' },
  { key: 'r2', label: '2. Kỹ năng sư phạm và truyền đạt', desc: 'Phương pháp giảng dạy lôi cuốn, dễ hiểu, kết hợp lý thuyết và thực hành.' },
  { key: 'r3', label: '3. Thái độ và hỗ trợ sinh viên', desc: 'Nhiệt tình giải đáp thắc mắc, tôn trọng và công bằng với sinh viên.' },
  { key: 'r4', label: '4. Tài liệu học tập và chuẩn bị bài giảng', desc: 'Cung cấp slide, giáo trình, bài tập thực hành đầy đủ và rõ ràng.' },
  { key: 'r5', label: '5. Đánh giá và chấm điểm công bằng', desc: 'Tiêu chí chấm điểm rõ ràng, minh bạch, phản hồi kết quả kịp thời.' },
  { key: 'r6', label: '6. Tương tác và khuyến khích thảo luận', desc: 'Tạo không khí học tập tích cực, khuyến khích sinh viên đặt câu hỏi và phản biện.' }
]

const activeEval = ref(null)
const evalModalOpen = ref(false)
const confirmModalOpen = ref(false)
const anyModalOpen = computed(() => evalModalOpen.value || confirmModalOpen.value)
useBodyScrollLock(anyModalOpen)
const isSubmitting = ref(false)

const filteredEvals = computed(() => evaluations.value)
const pendingCount = computed(() => evaluations.value.filter(e => e.status === 'Pending').length)

const mapEvaluation = (item) => ({
  id: item.id ?? item.Id ?? item.maDanhGia,
  enrollmentId: item.enrollmentId ?? item.EnrollmentId,
  subject: item.tenMonHoc ?? item.subject ?? item.Subject ?? item.tenMon ?? item.course,
  teacher: item.giangVien ?? item.teacher ?? item.Teacher ?? item.tenGiangVien,
  status: item.trangThai ?? item.status ?? item.Status ?? 'Pending',
  editsLeft: item.soLanSua ?? item.editsLeft ?? item.EditsLeft ?? 2,
  ratings: {
    r1: item.diem1 ?? item.r1 ?? 0,
    r2: item.diem2 ?? item.r2 ?? 0,
    r3: item.diem3 ?? item.r3 ?? 0,
    r4: item.diem4 ?? item.r4 ?? 0,
    r5: item.diem5 ?? item.r5 ?? 0,
    r6: item.diem6 ?? item.r6 ?? 0,
  },
  feedback: item.nhanXet ?? item.feedback ?? item.Feedback ?? ''
})

const fetchEvaluations = async () => {
  loading.value = true
  error.value = ''
  try {
    const response = await studentApi.getEvaluations()
    const data = unwrapApiData(response) || []
    evaluations.value = (data.items ?? data.Items ?? data).map(mapEvaluation)
  } catch (err) {
    error.value = err?.message || 'Không thể tải danh sách đánh giá.'
    evaluations.value = []
  } finally {
    loading.value = false
  }
}

onMounted(fetchEvaluations)

const openEvalModal = (ev) => {
  activeEval.value = JSON.parse(JSON.stringify(ev))
  if (!activeEval.value.ratings) {
    activeEval.value.ratings = { r1: 0, r2: 0, r3: 0, r4: 0, r5: 0, r6: 0 }
  }
  evalModalOpen.value = true
}

const closeEvalModal = () => {
  evalModalOpen.value = false
  activeEval.value = null
}

const setRating = (key, val) => {
  if (activeEval.value) activeEval.value.ratings[key] = val
}

const isAllStandardRated = computed(() => {
  if (!activeEval.value || !activeEval.value.ratings) return false
  const r = activeEval.value.ratings
  return r.r1 > 0 && r.r2 > 0 && r.r3 > 0 && r.r4 > 0 && r.r5 > 0 && r.r6 > 0
})

const submitEvaluation = () => {
  confirmModalOpen.value = true
}

const confirmSubmit = async () => {
  isSubmitting.value = true
  try {
    await studentApi.submitEvaluation(activeEval.value.id, {
      id: activeEval.value.id,
      enrollmentId: activeEval.value.enrollmentId,
      ratings: activeEval.value.ratings,
      feedback: activeEval.value.feedback,
    })
    const idx = evaluations.value.findIndex(e => e.id === activeEval.value.id)
    if (idx !== -1) {
      evaluations.value.splice(idx, 1)
    }
    confirmModalOpen.value = false
    evalModalOpen.value = false
    activeEval.value = null
    popupStore.success('Thành công', 'Đánh giá giảng viên của bạn đã được ghi nhận ẩn danh vào hệ thống.')
  } catch (err) {
    popupStore.error('Lỗi', err?.message || 'Không thể gửi đánh giá.')
  } finally {
    isSubmitting.value = false
  }
}
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

    <!-- Loading State -->
    <div v-if="loading" class="text-center py-12 text-muted">
      <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-(--text-link) mb-3"></div>
      <p>Đang tải danh sách đánh giá giảng viên...</p>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="text-center py-12 text-red-500">
      <AlertCircle :size="32" class="mx-auto mb-2" />
      <p>{{ error }}</p>
    </div>

    <!-- Empty State (Completed All) -->
    <div v-else-if="filteredEvals.length === 0" class="text-center py-16 surface-card rounded-2xl border border-default p-8">
      <div class="flex h-16 w-16 items-center justify-center rounded-full bg-emerald-500/15 text-emerald-600 mx-auto mb-4">
        <CheckCircle2 :size="36" />
      </div>
      <h3 class="text-xl font-bold text-heading mb-2">Bạn đã hoàn thành tất cả đánh giá giảng viên!</h3>
      <p class="text-sm text-muted max-w-md mx-auto">
        Cảm ơn bạn đã đóng góp ý kiến xây dựng chất lượng đào tạo. Các môn học trong kỳ hiện tại đã được bạn đánh giá đầy đủ.
      </p>
    </div>

    <!-- Evaluation Cards Grid -->
    <div v-else class="eval-grid">
      <div v-for="ev in filteredEvals" :key="ev.id" class="eval-card">
        <div class="ec-header">
          <h3 class="subject-title">{{ ev.subject }}</h3>
          <span class="status-badge-sm badge-pending">
            Chưa đánh giá
          </span>
        </div>
        
        <div class="ec-body">
          <div class="flex items-center gap-2 text-sm font-semibold mb-2 text-label">
            <User :size="16" class="icon-teacher"/>
            {{ ev.teacher }}
          </div>
        </div>

        <div class="ec-footer">
          <button class="btn-primary w-full justify-center" @click="openEvalModal(ev)">
            <Edit3 :size="15"/> Thực hiện đánh giá
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
            
            <div class="modal-body max-h-[70vh] overflow-y-auto pr-1">
              <div class="eval-target-info">
                <div class="font-semibold text-lg text-heading">{{ activeEval?.subject }}</div>
                <div class="text-sm text-muted flex items-center gap-1 mt-1"><User :size="14"/> Giảng viên: <strong>{{ activeEval?.teacher }}</strong></div>
              </div>

              <!-- Standard Configured Criteria List (6 mục chuẩn) -->
              <div class="criteria-list mt-6 space-y-4">
                <div v-for="crit in criteriaList" :key="crit.key" class="criterion-item p-4 rounded-xl surface-card border border-default">
                  <div class="crit-text mb-2">
                    <h4 class="crit-label font-bold text-sm text-heading">{{ crit.label }}</h4>
                    <p class="crit-desc text-xs text-muted mt-0.5">{{ crit.desc }}</p>
                  </div>
                  <div class="crit-stars flex items-center gap-1 mt-2">
                    <Star v-for="i in 5" :key="i" :size="26" 
                          class="star-btn cursor-pointer transition-transform hover:scale-110" 
                          :class="activeEval?.ratings[crit.key] >= i ? 'fill-amber-400 text-amber-400' : 'text-slate-300 dark:text-slate-600'" 
                          @click="setRating(crit.key, i)" />
                    <span class="text-xs font-bold text-heading ml-2">
                      {{ activeEval?.ratings[crit.key] > 0 ? `${activeEval.ratings[crit.key]}/5 sao` : 'Chưa chấm' }}
                    </span>
                  </div>
                </div>
              </div>

              <div class="feedback-section mt-6">
                <h4 class="crit-label mb-2 font-bold text-sm text-heading">Nhận xét chi tiết (Feedback)</h4>
                <textarea v-model="activeEval.feedback" class="input-glass w-full p-3 rounded-xl border border-default" rows="3" placeholder="Nhập những góp ý, nhận xét tự do của bạn. Giảng viên sẽ đọc được nội dung này (ẩn danh)."></textarea>
              </div>
            </div>
            
            <div class="modal-footer flex items-center justify-between p-4 border-t border-default bg-slate-50/50 dark:bg-slate-900/30">
              <span class="text-xs text-muted" v-if="!isAllStandardRated">
                * Vui lòng chấm điểm đủ 6 mục để hoàn thành đánh giá
              </span>
              <span class="text-xs text-emerald-600 font-medium" v-else>
                ✓ Đã hoàn thành đủ các mục tiêu chí
              </span>
              <div class="flex items-center gap-2">
                <button class="btn-secondary px-4 py-2 text-xs" @click="closeEvalModal">Hủy</button>
                <button class="btn-primary px-4 py-2 text-xs flex items-center gap-1.5" @click="submitEvaluation" :disabled="!isAllStandardRated">
                  <Send :size="14"/> Tiếp tục
                </button>
              </div>
            </div>
          </div>
        </div>
      </Transition>

      <!-- Confirmation Modal -->
      <Transition name="modal">
        <div v-if="confirmModalOpen" class="modal-overlay" @click.self="confirmModalOpen = false">
          <div class="modal-content sm">
            <div class="modal-header">
              <h3>Xác nhận Gửi Đánh Giá</h3>
              <button class="close-btn-sm" @click="confirmModalOpen = false"><X :size="20"/></button>
            </div>
            <div class="modal-body text-center py-6">
              <ShieldCheck :size="48" class="icon-privacy-lg mx-auto mb-3" />
              <h4 class="text-lg font-semibold text-heading mb-2">Xác nhận gửi đánh giá</h4>
              <p class="text-base text-body font-medium">Bạn có chắc chắn muốn gửi đánh giá này không?</p>
            </div>
            <div class="modal-footer justify-center gap-3">
              <button class="btn-secondary flex-1" @click="confirmModalOpen = false" :disabled="isSubmitting">Quay lại</button>
              <button class="btn-primary flex-1" @click="confirmSubmit" :disabled="isSubmitting">
                {{ isSubmitting ? 'Đang gửi...' : 'Xác nhận Gửi' }}
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

.warning-banner { display: flex; align-items: flex-start; gap: 1rem; padding: 1.25rem 1.5rem; border-radius: 16px; backdrop-filter: blur(12px); box-shadow: 0 4px 20px color-mix(in srgb, var(--color-warning-text) 10%, transparent); }
.blocker-warning { background: var(--color-warning-bg); border: 1px solid color-mix(in srgb, var(--color-warning-text) 20%, transparent); color: var(--color-warning-text); }
.warning-icon { padding-top: .1rem; }
.warning-content h3 { font-size: 1rem; font-weight: 800; margin: 0 0 .25rem; color: var(--text-heading); }
.warning-content p { font-size: .875rem; margin: 0; opacity: 0.9; }

.privacy-banner { display: flex; align-items: center; gap: .875rem; padding: 1rem 1.25rem; border-radius: 12px; background: color-mix(in srgb, var(--text-link) 8%, transparent); border: 1px dashed color-mix(in srgb, var(--text-link) 30%, transparent); }
.icon-privacy { color: var(--text-link); }
.text-privacy { color: var(--text-heading); opacity: 0.9; }

.eval-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 1.5rem; }
.eval-card { background: var(--surface-card-strong); border: 1px solid var(--border-default); border-radius: 16px; padding: 1.25rem; display: flex; flex-direction: column; justify-content: space-between; transition: all .2s; }
.eval-card:hover { border-color: var(--border-focus); transform: translateY(-2px); box-shadow: var(--lg-shadow-sm); }
.ec-header { display: flex; justify-content: space-between; align-items: flex-start; gap: .5rem; margin-bottom: 1rem; }
.subject-title { font-size: 1.1rem; font-weight: 700; color: var(--text-heading); margin: 0; line-height: 1.3; }

.status-badge-sm { font-size: .75rem; font-weight: 700; padding: .25rem .6rem; border-radius: 6px; }
.badge-pending { background: var(--color-warning-bg); color: var(--color-warning-text); }

.icon-teacher { color: var(--text-link); }

.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.6); backdrop-filter: blur(4px); display: flex; align-items: center; justify-content: center; z-index: 999; padding: 1rem; }
.modal-content { background: var(--surface-card-strong); border: 1px solid var(--border-card); border-radius: 20px; width: 100%; box-shadow: 0 20px 40px rgba(0,0,0,0.3); overflow: hidden; display: flex; flex-direction: column; }
.modal-content.lg { max-width: 650px; }
.modal-content.sm { max-width: 400px; }
.modal-header { padding: 1.25rem 1.5rem; border-bottom: 1px solid var(--border-default); display: flex; justify-content: space-between; align-items: center; }
.modal-header h3 { font-size: 1.15rem; font-weight: 700; margin: 0; color: var(--text-heading); }
.close-btn-sm { background: transparent; border: none; cursor: pointer; color: var(--text-placeholder); border-radius: 6px; padding: .2rem; display: flex; }
.close-btn-sm:hover { color: var(--text-heading); background: var(--surface-solid); }

.modal-body { padding: 1.5rem; }
.modal-footer { padding: 1.25rem 1.5rem; border-top: 1px solid var(--border-default); display: flex; }

.eval-target-info { background: var(--surface-solid); padding: 1rem; border-radius: 12px; border: 1px solid var(--border-default); }

.btn-primary { display: inline-flex; align-items: center; gap: .4rem; padding: .75rem 1.25rem; border-radius: 10px; font-size: .875rem; font-weight: 700; cursor: pointer; border: none; background: var(--text-link); color: var(--text-inverse); box-shadow: 0 4px 14px color-mix(in srgb, var(--text-link) 25%, transparent); transition: all .15s; }
.btn-primary:hover:not(:disabled) { background: var(--lg-primary-dark); transform: translateY(-1px); }
.btn-primary:disabled { opacity: .6; cursor: not-allowed; }

.btn-secondary { display: inline-flex; align-items: center; gap: .4rem; padding: .75rem 1.25rem; border-radius: 10px; font-size: .875rem; font-weight: 600; cursor: pointer; border: 1px solid var(--border-default); background: var(--surface-card); color: var(--text-heading); transition: all .15s; }
.btn-secondary:hover:not(:disabled) { background: var(--surface-solid); }

.input-glass { width: 100%; border-radius: 10px; border: 1px solid var(--border-input); background: var(--surface-input); font-size: .9rem; outline: none; transition: border-color .2s; color: var(--text-body); }
.input-glass:focus { border-color: var(--border-input-focus); background: var(--surface-input-focus); }

.custom-select-wrapper { position: relative; }
.custom-select { position: relative; width: 180px; }
.select-trigger { display: flex; justify-content: space-between; align-items: center; padding: .6rem 1rem; background: var(--surface-card); border: 1px solid var(--border-card); border-radius: 10px; font-size: .875rem; font-weight: 600; cursor: pointer; color: var(--text-heading); }
.dropdown-backdrop { position: fixed; inset: 0; z-index: 10; }
.select-menu { position: absolute; top: calc(100% + 5px); right: 0; width: 100%; background: var(--surface-card); border: 1px solid var(--border-card); border-radius: 10px; box-shadow: var(--lg-shadow-sm); z-index: 20; overflow: hidden; padding: .3rem 0; }
.select-option { padding: .6rem 1rem; font-size: .85rem; font-weight: 500; cursor: pointer; color: var(--text-body); transition: background .15s; }
.select-option:hover { background: var(--surface-solid); }
.select-option.selected { background: color-mix(in srgb, var(--text-link) 10%, transparent); color: var(--text-link); font-weight: 700; }
</style>
