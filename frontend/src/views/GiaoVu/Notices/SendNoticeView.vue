<script setup>
import { ref, computed } from 'vue'
import { usePopupStore } from '@/stores/popup'
import { useAuthStore } from '@/stores/auth'
import { noticeApi } from '@/services/noticeApi'
import {
  Send, Save, Clock, Users, Mail, Smartphone, Bell, Info,
  CheckCircle2, X, Plus, Search, Eye, Loader2, ListChecks, ChevronDown, ChevronUp
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

const popupStore = usePopupStore()
const authStore = useAuthStore()

// ── State ────────────────────────────────────────────────────
const title = ref('')
const content = ref('')
const sendMode = ref('now')
const scheduleTime = ref('')
const sending = ref(false)
const sendResult = ref(null)
const showPreview = ref(false)
const groupEmailMode = ref(true)

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

// Mock section data — in production these come from API
const sectionData = [
  { id: 1, code: 'SE1601', name: 'Java Programming', studentCount: 30 },
  { id: 2, code: 'SE1602', name: 'Data Structures', studentCount: 28 },
  { id: 3, code: 'SE1603', name: 'Web Development', studentCount: 32 },
  { id: 4, code: 'SE1604', name: 'Mobile App Development', studentCount: 26 },
  { id: 5, code: 'SE1605', name: 'Artificial Intelligence', studentCount: 24 },
  { id: 6, code: 'SE1606', name: 'Database Systems', studentCount: 30 },
  { id: 7, code: 'SE1607', name: 'Software Engineering', studentCount: 28 },
  { id: 8, code: 'SE1608', name: 'Computer Networks', studentCount: 22 },
]

const selectedSections = ref([sectionData[0], sectionData[1]])

const displaySection = (s) => `${s.code} - ${s.name}`

// ── Computed: Recipient Summary ────────────────────────────
const selectedTargets = computed(() => targets.value.filter(t => t.selected))
const hasEmailChannel = computed(() => channels.value.find(c => c.id === 'email')?.selected)

const recipientSummary = computed(() => {
  const totalSelected = selectedSections.value.length
  const totalStudents = selectedSections.value.reduce((sum, s) => sum + s.studentCount, 0)
  const groups = groupEmailMode.value && hasEmailChannel.value
    ? selectedSections.value.map(s => ({
        classCode: s.code,
        email: `nhom-${s.code.toLowerCase()}@lms-test.edu.vn`,
        count: s.studentCount,
      }))
    : []
  return { totalSelected, totalStudents, groups }
})

// ── Add Section Modal ────────────────────────────────────────
const showAddModal = ref(false)
const sectionSearch = ref('')

const availableSections = computed(() =>
  sectionData.filter(
    s => !selectedSections.value.find(sel => sel.id === s.id)
      && (s.code.toLowerCase().includes(sectionSearch.value.toLowerCase())
        || s.name.toLowerCase().includes(sectionSearch.value.toLowerCase())),
  ),
)

function toggleSection(section) {
  const idx = selectedSections.value.findIndex(s => s.id === section.id)
  if (idx >= 0) selectedSections.value.splice(idx, 1)
  else selectedSections.value.push(section)
  sectionSearch.value = ''
}

function removeSection(index) {
  selectedSections.value.splice(index, 1)
}

// ── Send Logic ─────────────────────────────────────────────
function buildRequest() {
  const selectedTargetsArr = targets.value.filter(t => t.selected)
  const isAllStudents = selectedTargetsArr.some(t => t.id === 'all-students')
  const isByClass = selectedTargetsArr.some(t => t.id === 'by-class')
  const isBySection = selectedTargetsArr.some(t => t.id === 'by-section')
  const isWaitlist = selectedTargetsArr.some(t => t.id === 'waitlist')

  // Build TargetType + TargetIds
  if (isAllStudents) {
    // Send to entire campus
    return {
      targetType: 'campus',
      targetIds: [authStore.user?.campusId || 1],
      useGroupEmail: groupEmailMode.value && hasEmailChannel.value,
    }
  }
  if (isBySection && selectedSections.value.length) {
    return {
      targetType: 'course',
      targetIds: selectedSections.value.map(s => s.id),
      useGroupEmail: groupEmailMode.value && hasEmailChannel.value,
    }
  }
  if (isByClass && selectedSections.value.length) {
    return {
      targetType: 'class',
      targetIds: selectedSections.value.map(s => s.id),
      useGroupEmail: groupEmailMode.value && hasEmailChannel.value,
    }
  }
  if (isWaitlist) {
    // Mock: send to specific users (IDs 1-10 as waitlist students)
    return {
      targetType: 'users',
      targetIds: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10],
      useGroupEmail: false,
    }
  }
  return null
}

async function doSend() {
  // ── Validate ──
  if (!title.value.trim()) {
    popupStore.warning('Thiếu thông tin', 'Vui lòng nhập tiêu đề thông báo.')
    return
  }
  if (!content.value.trim()) {
    popupStore.warning('Thiếu thông tin', 'Vui lòng nhập nội dung thông báo.')
    return
  }
  if (!selectedTargets.value.length) {
    popupStore.warning('Thiếu thông tin', 'Vui lòng chọn ít nhất một đối tượng nhận.')
    return
  }
  if (!channels.value.some(c => c.selected)) {
    popupStore.warning('Thiếu thông tin', 'Vui lòng chọn ít nhất một kênh gửi.')
    return
  }

  const request = buildRequest()
  if (!request) {
    popupStore.warning('Lỗi', 'Không thể xác định đối tượng gửi. Vui lòng kiểm tra lại.')
    return
  }

  sending.value = true
  sendResult.value = null

  try {
    const payload = {
      tieuDe: title.value.trim(),
      tomTat: content.value.trim().substring(0, 200),
      noiDungText: content.value.trim(),
      mucDo: 'info',
      targetType: request.targetType,
      targetIds: request.targetIds,
    }

    const res = await noticeApi.send(payload)

    // Build result display
    const totalRecipients = recipientSummary.value.totalStudents || (
      request.targetType === 'campus' ? 300
      : request.targetType === 'users' ? 10
      : selectedSections.value.reduce((s, sec) => s + sec.studentCount, 0)
    )

    sendResult.value = {
      success: true,
      title: title.value.trim(),
      totalRecipients,
      channels: channels.value.filter(c => c.selected).map(c => c.label),
      groupEmailUsed: request.useGroupEmail,
      groupEmailCount: request.useGroupEmail ? selectedSections.value.length : 0,
      groupEmails: request.useGroupEmail ? recipientSummary.value.groups : [],
      targetLabel: selectedTargets.value.map(t => t.label).join(', '),
      data: res,
    }

    popupStore.success(
      'Đã gửi thông báo',
      sendMode.value === 'now'
        ? `Thông báo đang được gửi đến ${totalRecipients} người.`
        : 'Đã lên lịch gửi.',
    )
  } catch (err) {
    popupStore.error('Gửi thất bại', err.message || 'Có lỗi xảy ra khi gửi thông báo.')
    sendResult.value = { success: false, error: err.message }
  } finally {
    sending.value = false
  }
}

// Fallback: if API is not available, use mock send
async function sendNotice() {
  if (!title.value.trim() || !content.value.trim() || !selectedTargets.value.length) {
    popupStore.warning('Thiếu thông tin', 'Vui lòng điền đầy đủ thông tin trước khi gửi.')
    return
  }
  if (!channels.value.some(c => c.selected)) {
    popupStore.warning('Thiếu thông tin', 'Vui lòng chọn ít nhất một kênh gửi.')
    return
  }

  const request = buildRequest()
  if (!request) {
    popupStore.warning('Lỗi', 'Không thể xác định đối tượng gửi.')
    return
  }

  sending.value = true
  sendResult.value = null

  try {
    await doSend()
  } catch {
    // Fallback mock
    await new Promise(r => setTimeout(r, 800))
    const total = recipientSummary.value.totalStudents || 30
    sendResult.value = {
      success: true,
      title: title.value.trim(),
      totalRecipients: total,
      channels: channels.value.filter(c => c.selected).map(c => c.label),
      groupEmailUsed: request.useGroupEmail && hasEmailChannel.value,
      groupEmailCount: request.useGroupEmail ? selectedSections.value.length : 0,
      groupEmails: request.useGroupEmail ? recipientSummary.value.groups : [],
      targetLabel: selectedTargets.value.map(t => t.label).join(', '),
    }
    popupStore.success('Đã gửi thông báo', `Thông báo đã gửi đến ${total} người (offline mode).`)
  } finally {
    sending.value = false
  }
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
    selectedSections: selectedSections.value.map(s => s.id),
    savedAt: new Date().toISOString(),
  }
  localStorage.setItem('lms_notice_draft', JSON.stringify(draft))
  popupStore.success('Đã lưu nháp', 'Thông báo đã được lưu vào bản nháp.')
}

function clearResult() {
  sendResult.value = null
}
</script>

<template>
  <PageContainer
    title="Gửi thông báo học vụ"
    subtitle="Soạn thảo và gửi thông báo đến các đối tượng sinh viên, giảng viên theo nhu cầu."
  >
    <div class="grid grid-cols-1 xl:grid-cols-3 gap-6">

      <!-- ═══ Left: Composer ═══ -->
      <div class="xl:col-span-2 space-y-4">

        <!-- Result Banner -->
        <div v-if="sendResult"
          :class="[
            'p-5 rounded-2xl border flex items-start gap-4 transition-all',
            sendResult.success
              ? 'bg-[var(--color-success-bg)] border-[var(--color-success-text)]/30'
              : 'bg-[var(--color-danger-bg)] border-[var(--lg-danger)]/30'
          ]"
        >
          <div :class="[
            'h-10 w-10 rounded-xl flex items-center justify-center shrink-0',
            sendResult.success ? 'bg-[var(--lg-success)] text-white' : 'bg-[var(--lg-danger)] text-white'
          ]">
            <CheckCircle2 v-if="sendResult.success" :size="22" />
            <X v-else :size="22" />
          </div>
          <div class="flex-1">
            <h4 class="text-sm font-bold" :class="sendResult.success ? 'text-[var(--color-success-text)]' : 'text-[var(--lg-danger)]'">
              {{ sendResult.success ? 'Đã gửi thông báo thành công' : 'Gửi thất bại' }}
            </h4>
            <div v-if="sendResult.success" class="mt-2 space-y-1 text-xs text-body">
              <p><strong>Tiêu đề:</strong> {{ sendResult.title }}</p>
              <p><strong>Đối tượng:</strong> {{ sendResult.targetLabel }}</p>
              <p><strong>Số người nhận:</strong> {{ sendResult.totalRecipients }}</p>
              <p><strong>Kênh:</strong> {{ sendResult.channels.join(', ') }}</p>
              <div v-if="sendResult.groupEmailUsed && sendResult.groupEmails.length" class="mt-2">
                <button @click="showPreview = !showPreview" class="inline-flex items-center gap-1 text-[11px] font-semibold text-link hover:underline">
                  <Eye :size="13" />
                  Xem danh sách email gộp ({{ sendResult.groupEmailCount }} nhóm)
                  <ChevronDown v-if="!showPreview" :size="13" />
                  <ChevronUp v-else :size="13" />
                </button>
                <div v-if="showPreview" class="mt-2 space-y-1">
                  <div v-for="g in sendResult.groupEmails" :key="g.classCode"
                    class="inline-flex items-center gap-2 px-2.5 py-1 mr-1 mb-1 rounded-lg bg-[var(--surface-input)] border border-default text-[10px] font-semibold">
                    <Mail :size="11" class="text-link" />
                    {{ g.email }}
                    <span class="text-placeholder">({{ g.count }} SV)</span>
                  </div>
                </div>
              </div>
            </div>
            <p v-else class="text-xs text-[var(--lg-danger)] mt-1">{{ sendResult.error }}</p>
            <button @click="clearResult" class="mt-3 text-xs font-bold text-link hover:underline">Soạn thông báo mới</button>
          </div>
          <button @click="clearResult" class="p-1 hover:bg-[var(--surface-input)] rounded-lg text-placeholder shrink-0">
            <X :size="16" />
          </button>
        </div>

        <!-- Compose Card -->
        <div class="surface-card border border-card rounded-2xl p-6">
          <div class="space-y-4">
            <div>
              <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-2 block">Tiêu đề thông báo</label>
              <input
                v-model="title"
                type="text"
                placeholder="Ví dụ: Thông báo thay đổi lịch học bù môn Java..."
                class="w-full lg-input rounded-2xl px-5 py-3.5 text-sm font-bold outline-none transition-all placeholder:text-placeholder"
              >
            </div>
            <div>
              <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-2 block">Nội dung chi tiết</label>
              <div class="border border-card rounded-2xl overflow-hidden surface-card transition-all">
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

          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-6">
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

          <!-- Recipient Summary -->
          <div v-if="selectedTargets.length" class="mb-4 p-3 rounded-xl surface-solid border border-default">
            <div class="flex items-center justify-between text-xs text-body">
              <span class="font-semibold flex items-center gap-1.5">
                <ListChecks :size="14" class="text-link" />
                {{ selectedTargets.map(t => t.label).join(', ') }}
              </span>
              <span class="font-bold text-heading">
                ~{{ recipientSummary.totalStudents || 30 }} người nhận
              </span>
            </div>
            <!-- Group email preview -->
            <div v-if="groupEmailMode && hasEmailChannel && recipientSummary.groups.length" class="mt-2 pt-2 border-t border-default">
              <p class="text-[10px] font-semibold text-placeholder uppercase tracking-widest mb-1.5 flex items-center gap-1">
                <Mail :size="12" /> Email gộp theo lớp ({{ recipientSummary.groups.length }} nhóm)
              </p>
              <div class="flex flex-wrap gap-1.5">
                <div v-for="g in recipientSummary.groups" :key="g.classCode"
                  class="inline-flex items-center gap-1.5 px-2 py-0.5 rounded-lg bg-[var(--color-info-bg)]/50 border border-[var(--color-info-text)]/20 text-[9px] font-semibold">
                  <Mail :size="10" class="text-link" />
                  <span class="text-label">{{ g.email }}</span>
                  <span class="text-placeholder">({{ g.count }})</span>
                </div>
              </div>
            </div>
          </div>

          <!-- Selected Sections -->
          <div class="space-y-3">
            <p class="text-[10px] font-semibold text-label uppercase tracking-widest">Danh sách lớp đã chọn</p>
            <div class="flex flex-wrap gap-2">
              <div v-for="(sec, idx) in selectedSections" :key="sec.id"
                class="flex items-center gap-2 px-3 py-1.5 lg-badge-info rounded-xl text-xs font-bold shadow-sm">
                {{ displaySection(sec) }}
                <span class="text-[9px] text-placeholder">({{ sec.studentCount }} SV)</span>
                <button @click="removeSection(idx)" class="hover:bg-[var(--surface-input)] rounded-full p-0.5"><X :size="12" /></button>
              </div>
              <button @click="showAddModal = true"
                class="px-3 py-1.5 border-2 border-dashed border-default text-placeholder rounded-xl text-xs font-bold hover:border-[var(--border-input-focus)] hover:text-link transition-all">
                + Thêm lớp/SV
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- ═══ Right: Delivery Settings ═══ -->
      <div class="space-y-4">

        <!-- Channels -->
        <div class="surface-card border border-card rounded-2xl p-5">
          <h4 class="text-xs font-semibold text-label uppercase tracking-widest mb-4">Kênh phân phối</h4>
          <div class="space-y-3">
            <div v-for="ch in channels" :key="ch.id"
              class="flex items-center justify-between p-4 surface-solid rounded-2xl border border-default group">
              <div class="flex items-center gap-3">
                <div :class="['h-9 w-9 rounded-xl flex items-center justify-center transition-colors', ch.selected ? 'bg-[var(--lg-primary)] text-white shadow-sm' : 'surface-solid text-placeholder']">
                  <component :is="ch.icon" :size="18" />
                </div>
                <span :class="['text-sm font-bold', ch.selected ? 'text-heading' : 'text-placeholder']">{{ ch.label }}</span>
              </div>
              <button @click="ch.selected = !ch.selected"
                :class="['h-5 w-10 rounded-full relative transition-colors p-1', ch.selected ? 'bg-[var(--lg-primary)]' : 'border-default border']">
                <div :class="['h-3 w-3 bg-[var(--surface-modal)] rounded-full transition-transform', ch.selected ? 'translate-x-5' : 'translate-x-0']"></div>
              </button>
            </div>
          </div>

          <!-- Group Email Toggle -->
          <div v-if="hasEmailChannel" class="mt-4 p-3 rounded-xl surface-solid border border-default">
            <label class="flex items-center justify-between cursor-pointer">
              <div class="flex items-center gap-2.5">
                <div class="h-7 w-7 rounded-lg bg-[var(--color-info-bg)] flex items-center justify-center text-link">
                  <Mail :size="14" />
                </div>
                <div>
                  <p class="text-xs font-bold text-heading">Gửi email gộp theo lớp</p>
                  <p class="text-[9px] text-placeholder">1 email đại diện cho 1 lớp</p>
                </div>
              </div>
              <button @click.stop="groupEmailMode = !groupEmailMode"
                :class="['h-5 w-10 rounded-full relative transition-colors p-1', groupEmailMode ? 'bg-[var(--lg-primary)]' : 'border-default border']">
                <div :class="['h-3 w-3 bg-[var(--surface-modal)] rounded-full transition-transform', groupEmailMode ? 'translate-x-5' : 'translate-x-0']"></div>
              </button>
            </label>
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
                <input v-model="scheduleTime" type="datetime-local"
                  class="w-full lg-input rounded-xl pl-10 pr-4 py-2.5 text-xs font-bold">
              </div>
            </div>
          </div>
        </div>

        <!-- Main Actions -->
        <div class="space-y-3">
          <button @click="sendNotice" :disabled="sending"
            class="w-full lg-button-primary py-4 text-sm font-semibold flex items-center justify-center gap-2 disabled:opacity-60">
            <Loader2 v-if="sending" :size="20" class="animate-spin" />
            <Send v-else :size="20" />
            {{ sending ? 'ĐANG GỬI...' : 'GỬI THÔNG BÁO' }}
          </button>
          <button @click="saveDraft" class="w-full lg-button-secondary py-3 text-sm font-bold flex items-center justify-center gap-2">
            <Save :size="18" /> LƯU BẢN NHÁP
          </button>
        </div>

        <!-- Help Info -->
        <div class="p-5 rounded-2xl bg-[var(--color-warning-bg)] border border-[var(--color-warning-text)]/20 flex items-start gap-4">
          <Info :size="18" class="text-[var(--color-warning-text)] shrink-0 mt-0.5" />
          <div class="text-xs text-heading leading-relaxed font-medium">
            <p><strong>Lưu ý:</strong> Thông báo sẽ được gửi đồng thời qua các kênh đã chọn.</p>
            <p class="mt-1">Chế độ email gộp: mỗi lớp dùng chung 1 email (<span class="font-mono text-link">nhom-{mã lớp}@lms-test.edu.vn</span>) để tiết kiệm tài nguyên. Phù hợp môi trường test.</p>
          </div>
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
          <input v-model="sectionSearch" type="text" placeholder="Tìm kiếm lớp học phần..."
            class="w-full lg-input rounded-xl pl-10 pr-4 py-2.5 text-sm font-medium outline-none" />
        </div>

        <ul class="space-y-1 max-h-64 overflow-y-auto">
          <li v-for="section in availableSections" :key="section.id"
            @click="toggleSection(section)"
            class="flex items-center justify-between px-3.5 py-2.5 rounded-xl cursor-pointer transition-all hover:surface-solid text-sm font-bold text-heading">
            <div class="flex items-center gap-3">
              <div :class="['h-4 w-4 rounded border-2 flex items-center justify-center transition-all', selectedSections.some(s => s.id === section.id) ? 'bg-[var(--lg-primary)] border-[var(--lg-primary)]' : 'border-default']">
                <CheckCircle2 v-if="selectedSections.some(s => s.id === section.id)" :size="10" class="text-white" />
              </div>
              {{ displaySection(section) }}
            </div>
            <span class="text-[10px] text-placeholder">{{ section.studentCount }} SV</span>
          </li>
          <li v-if="!availableSections.length" class="text-center py-6 text-xs text-placeholder font-medium">
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
