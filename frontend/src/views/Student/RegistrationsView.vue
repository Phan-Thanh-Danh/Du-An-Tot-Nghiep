<script setup>
import { ref, computed, onMounted } from 'vue'
import {
  BookOpen, Users, Clock, AlertTriangle, Sparkles,
  CheckCircle2, XCircle, Search, Filter,
  ArrowRightLeft, MinusCircle, UserPlus, Zap, ShieldAlert
} from 'lucide-vue-next'
import { registrationApi } from '@/services/registrationApi'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassInput from '@/components/ui/GlassInput.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import { usePopupStore } from '@/stores/popup'

const popup = usePopupStore()

const loading = ref(true)
const forbidden = ref(false)
const error = ref(null)
const confirmLoading = ref(false)
const classes = ref([])

function mapSection(section) {
  const status = section.registrationStatus || section.status
  return {
    id: section.id,
    registrationId: section.registrationId,
    code: section.code,
    subject: section.subject,
    credits: section.credits,
    teacher: section.teacher,
    schedule: section.schedule,
    slots: section.enrolled,
    maxSlots: section.capacity,
    status: status === 'full' ? 'Full' : status === 'closed' || status === 'cancelled' ? 'Closed' : status || 'Open',
    aiTag: section.status === 'full' ? 'hot' : '',
    prereq: 'Theo chương trình đào tạo',
    waitlistPos: section.waitlistPosition,
  }
}

async function loadRegistrations() {
  loading.value = true
  error.value = null
  forbidden.value = false
  try {
    const data = await registrationApi.getAvailableSections()
    const raw = data?.items ?? data?.data ?? data ?? []
    classes.value = Array.isArray(raw) ? raw.map(mapSection) : []
  } catch (err) {
    console.error(err)
    if (err?.statusCode === 403) {
      forbidden.value = true
    } else {
      error.value = err?.message || 'Không thể tải danh sách lớp có thể đăng ký.'
    }
  } finally {
    loading.value = false
  }
}

const metrics = computed(() => {
  const list = classes.value
  const enrolledList = list.filter(c => c.status === 'Enrolled')
  const waitlistList = list.filter(c => c.status === 'Waitlist')
  const totalCredits = enrolledList.reduce((sum, c) => sum + c.credits, 0)
  
  return [
    { label: 'Tín chỉ đã đăng ký', value: String(totalCredits), max: '21', unit: 'TC', icon: BookOpen, tone: 'blue', hint: 'Tối đa 21 TC' },
    { label: 'Số lớp đã chọn', value: String(enrolledList.length), unit: 'lớp', icon: CheckCircle2, tone: 'green', hint: 'Bao gồm lý thuyết & thực hành' },
    { label: 'Đang ở hàng chờ', value: String(waitlistList.length), unit: 'môn', icon: Clock, tone: 'amber', hint: waitlistList.length ? `Waitlist (vị trí #${waitlistList[0].waitlistPos || 1})` : 'Không ở hàng chờ' },
  ]
})

const subjects = computed(() => {
  const list = classes.value.map(c => c.subject)
  return ['Tất cả', ...new Set(list)]
})

const statuses = ['Tất cả', 'Open', 'Full', 'Enrolled', 'Waitlist', 'Closed']

const statusConfig = {
  Open: { label: 'Đang mở', variant: 'info', icon: CheckCircle2 },
  Full: { label: 'Đã đầy', variant: 'danger', icon: Users },
  Enrolled: { label: 'Đã đăng ký', variant: 'success', icon: CheckCircle2 },
  Waitlist: { label: 'Hàng chờ', variant: 'warning', icon: Clock },
  Dropped: { label: 'Đã hủy', variant: 'neutral', icon: XCircle },
  Closed: { label: 'Đã đóng', variant: 'neutral', icon: XCircle }
}

const filterSubject = ref('Tất cả')
const filterStatus = ref('Tất cả')
const searchQuery = ref('')

const filteredClasses = computed(() => {
  return classes.value.filter(c => {
    const matchSub = filterSubject.value === 'Tất cả' || c.subject === filterSubject.value
    const matchStat = filterStatus.value === 'Tất cả' || c.status === filterStatus.value
    const matchQuery = !searchQuery.value || c.code.toLowerCase().includes(searchQuery.value.toLowerCase()) || c.subject.toLowerCase().includes(searchQuery.value.toLowerCase())
    return matchSub && matchStat && matchQuery
  })
})

const getProgressColor = (slots, max) => {
  const p = slots / max
  if (p >= 1) return 'bg-red-500'
  if (p >= 0.8) return 'bg-yellow-500'
  return 'bg-blue-500'
}

// Dialog control
const isDialogShow = ref(false)
const dialogTitle = ref('')
const dialogMessage = ref('')
const dialogConfirmLabel = ref('')
const dialogVariant = ref('primary')
const dialogActionType = ref('')
const dialogTargetClass = ref(null)

const openConfirmDialog = (type, cls) => {
  dialogTargetClass.value = cls
  dialogActionType.value = type
  isDialogShow.value = true
  
  if (type === 'enroll') {
    dialogTitle.value = 'Xác nhận Đăng ký học phần'
    dialogMessage.value = `Bạn đang đăng ký lớp ${cls.code} - ${cls.subject} (${cls.credits} tín chỉ). Hệ thống sẽ kiểm tra điều kiện tiên quyết, giới hạn tín chỉ và xung đột lịch học trước khi ghi nhận. Bạn có chắc muốn tiếp tục?`
    dialogConfirmLabel.value = 'Đăng ký'
    dialogVariant.value = 'primary'
  } else if (type === 'waitlist') {
    dialogTitle.value = 'Xác nhận Vào danh sách chờ'
    dialogMessage.value = `Lớp ${cls.code} hiện đã hết chỗ. Bạn có muốn tham gia danh sách chờ (Waitlist)? Bạn sẽ tự động được xếp vào lớp theo thứ tự khi có sinh viên khác hủy lớp.`
    dialogConfirmLabel.value = 'Vào hàng chờ'
    dialogVariant.value = 'success'
  } else if (type === 'withdraw') {
    dialogTitle.value = 'Xác nhận Hủy đăng ký'
    dialogMessage.value = `Bạn có chắc chắn muốn hủy đăng ký lớp ${cls.code} - ${cls.subject}? Chỗ của bạn sẽ được hoàn trả và nhường lại cho các sinh viên đang ở hàng chờ.`
    dialogConfirmLabel.value = 'Hủy đăng ký'
    dialogVariant.value = 'danger'
  }
}

const handleDialogConfirm = async () => {
  const actionType = dialogActionType.value
  const targetClass = dialogTargetClass.value
  isDialogShow.value = false
  
  confirmLoading.value = true
  try {
    if (actionType === 'enroll' || actionType === 'waitlist') {
      await registrationApi.enroll(targetClass.id)
      popup.success('Thành công', `Đăng ký học phần ${targetClass.code} thành công.`)
    } else if (actionType === 'withdraw') {
      await registrationApi.withdraw(targetClass.registrationId)
      popup.success('Thành công', `Đã hủy đăng ký học phần ${targetClass.code}.`)
    }
    await loadRegistrations()
  } catch (err) {
    console.error(err)
    popup.error('Không thể thực hiện', err?.message || 'Có lỗi xảy ra trong quá trình xử lý.')
  } finally {
    confirmLoading.value = false
  }
}

onMounted(() => {
  loadRegistrations()
})
</script>

<template>
  <div class="registration-page max-w-7xl mx-auto space-y-6">
    <!-- Header -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
      <div>
        <div class="flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-(--text-link) mb-1">
          <BookOpen :size="14"/>
          Đăng ký & Quản lý lớp
        </div>
        <h1 class="text-3xl font-extrabold text-(--text-heading) tracking-tight">Đăng ký môn học</h1>
        <p class="text-sm text-(--text-muted) mt-1">Xây dựng lộ trình học tập, theo dõi sĩ số và xếp hàng chờ lớp thời gian thực.</p>
      </div>
    </div>

    <!-- 5 States container -->
    <div v-if="loading" class="space-y-4 py-8">
      <LoadingSkeleton :lines="5" />
    </div>

    <!-- Forbidden State -->
    <div v-else-if="forbidden" class="flex flex-col items-center justify-center py-16 bg-(--surface-card) border border-(--border-default) rounded-xl">
      <ShieldAlert :size="48" class="text-(--color-danger-bg, #ef4444) mb-4" />
      <h3 class="text-lg font-bold text-(--text-heading)">Không có quyền truy cập</h3>
      <p class="text-sm text-(--text-muted) mt-1">Chức năng đăng ký học phần không khả dụng đối với tài khoản của bạn hoặc chưa được mở.</p>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="flex flex-col items-center justify-center py-16 bg-(--surface-card) border border-(--border-default) rounded-xl">
      <AlertTriangle :size="48" class="text-(--color-danger-bg, #ef4444) mb-4" />
      <h3 class="text-lg font-bold text-(--text-heading)">Đã xảy ra lỗi</h3>
      <p class="text-sm text-(--text-muted) mt-1 mb-4">{{ error }}</p>
      <GlassButton variant="secondary" @click="loadRegistrations">Thử lại</GlassButton>
    </div>

    <!-- Success State -->
    <template v-else>
      <!-- Suggestion Banner -->
      <GlassPanel variant="readable" class="p-4 bg-gradient-to-br from-indigo-500/5 to-purple-500/5 border-indigo-500/20 relative overflow-hidden flex items-start gap-4">
        <div class="p-2.5 bg-indigo-500/10 text-indigo-500 rounded-xl">
          <Sparkles :size="20" />
        </div>
        <div>
          <h3 class="font-bold text-(--text-heading) text-sm mb-0.5">Gợi ý đăng ký</h3>
          <p class="text-xs text-(--text-muted) leading-relaxed">Hệ thống tự động kiểm tra đợt đăng ký, sĩ số, trùng môn, trùng lịch và giới hạn tín chỉ trước khi ghi nhận đăng ký học phần.</p>
        </div>
      </GlassPanel>

      <!-- Metrics -->
      <div class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <GlassPanel v-for="m in metrics" :key="m.label" variant="flat" density="compact" class="flex items-center gap-4 border-l-4" :class="{
          'border-l-blue-500': m.tone === 'blue',
          'border-l-green-500': m.tone === 'green',
          'border-l-yellow-500': m.tone === 'amber'
        }">
          <div class="w-10 h-10 rounded-xl flex items-center justify-center text-xs shrink-0" :class="{
            'bg-blue-500/10 text-blue-500': m.tone === 'blue',
            'bg-green-500/10 text-green-500': m.tone === 'green',
            'bg-yellow-500/10 text-yellow-500': m.tone === 'amber'
          }">
            <component :is="m.icon" :size="20" />
          </div>
          <div>
            <div class="text-lg font-black text-(--text-heading)">
              {{ m.value }}<span v-if="m.max" class="text-xs text-(--text-muted) font-semibold">/{{ m.max }}</span>
              <span class="text-xs font-semibold text-(--text-muted) ml-1">{{ m.unit }}</span>
            </div>
            <div class="text-xs font-bold text-(--text-muted) mt-0.5">{{ m.label }}</div>
            <div class="text-[10px] text-(--text-muted) mt-0.5 italic">{{ m.hint }}</div>
          </div>
        </GlassPanel>
      </div>

      <!-- Toolbar -->
      <GlassPanel variant="flat" density="compact" class="flex flex-col md:flex-row gap-3 justify-between items-stretch md:items-center">
        <div class="flex-1 max-w-md">
          <GlassInput v-model="searchQuery" placeholder="Tìm tên môn, mã lớp..." class="w-full">
            <template #prefix><Search :size="16" class="text-placeholder" /></template>
          </GlassInput>
        </div>
        <div class="flex flex-wrap items-center gap-2">
          <div class="flex items-center gap-2 text-xs font-bold text-(--text-muted) shrink-0">
            <Filter :size="14" />
            Lọc môn học:
          </div>
          <select v-model="filterSubject" class="text-xs bg-(--surface-input) border border-(--border-input) text-(--text-body) rounded px-2.5 py-1.5 outline-none focus:ring-1 focus:ring-(--border-focus)">
            <option v-for="s in subjects" :key="s" :value="s">{{ s }}</option>
          </select>
          <select v-model="filterStatus" class="text-xs bg-(--surface-input) border border-(--border-input) text-(--text-body) rounded px-2.5 py-1.5 outline-none focus:ring-1 focus:ring-(--border-focus)">
            <option v-for="st in statuses" :key="st" :value="st">{{ statusConfig[st]?.label || st }}</option>
          </select>
        </div>
      </GlassPanel>

      <!-- Class Cards Grid -->
      <div v-if="filteredClasses.length === 0" class="py-12">
        <EmptyState title="Không tìm thấy lớp" description="Không tìm thấy lớp học phần nào khớp với bộ lọc." />
      </div>
      
      <div v-else class="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        <GlassPanel
          v-for="cls in filteredClasses"
          :key="cls.id"
          variant="flat"
          interactive
          class="flex flex-col h-full border relative overflow-hidden transition-all duration-300"
          :class="{
            'border-blue-500/20 hover:shadow-blue-500/5': cls.status === 'Open',
            'border-red-500/20 hover:shadow-red-500/5': cls.status === 'Full',
            'border-green-500/30 bg-green-500/[0.02]': cls.status === 'Enrolled',
            'border-yellow-500/30 bg-yellow-500/[0.02]': cls.status === 'Waitlist'
          }"
        >
          <!-- Card Header -->
          <div class="p-4 border-b border-(--border-default) flex justify-between items-start gap-2">
            <div>
              <span class="text-[10px] font-black text-blue-500 bg-blue-500/10 px-2 py-0.5 rounded uppercase tracking-wider">
                {{ cls.code }}
              </span>
              <h3 class="font-bold text-sm text-(--text-heading) mt-2 line-clamp-1">
                {{ cls.subject }}
              </h3>
            </div>
            <div class="flex flex-col items-end gap-1.5 shrink-0">
              <GlassBadge :variant="statusConfig[cls.status]?.variant || 'neutral'" size="sm">
                {{ statusConfig[cls.status]?.label || cls.status }}
              </GlassBadge>
              <GlassBadge v-if="cls.aiTag === 'hot'" variant="danger" size="sm">Hot</GlassBadge>
            </div>
          </div>

          <!-- Card Body -->
          <div class="p-4 flex-1 space-y-2 text-xs text-(--text-muted)">
            <div class="flex items-center gap-2">
              <Clock :size="14" class="text-placeholder shrink-0" />
              <span class="truncate">{{ cls.schedule }}</span>
            </div>
            <div class="flex items-center gap-2">
              <Users :size="14" class="text-placeholder shrink-0" />
              <span>GV: {{ cls.teacher }}</span>
            </div>
            <div class="flex items-center gap-2">
              <BookOpen :size="14" class="text-placeholder shrink-0" />
              <span>Số tín chỉ: <strong>{{ cls.credits }} TC</strong></span>
            </div>

            <!-- Waitlist rank -->
            <div v-if="cls.status === 'Waitlist'" class="flex items-center gap-2 p-2 bg-yellow-500/10 text-yellow-600 dark:text-yellow-400 rounded-lg text-[10px] font-bold">
              <AlertTriangle :size="12" class="shrink-0" />
              <span>Bạn đang ở hàng chờ vị trí số #{{ cls.waitlistPos || 1 }}</span>
            </div>

            <!-- Real-time capacity bar -->
            <div class="p-3 bg-(--surface-input) border border-(--border-default) rounded-xl mt-3 space-y-1.5">
              <div class="flex justify-between items-center text-[10px] font-bold">
                <span>Sĩ số (Thời gian thực)</span>
                <span class="text-(--text-heading)">{{ cls.slots }}/{{ cls.maxSlots }}</span>
              </div>
              <div class="w-full h-1.5 bg-(--border-default) rounded-full overflow-hidden">
                <div class="h-full rounded-full transition-all duration-300" :class="getProgressColor(cls.slots, cls.maxSlots)" :style="{ width: `${(cls.slots / cls.maxSlots) * 100}%` }"></div>
              </div>
            </div>
          </div>

          <!-- Card Footer -->
          <div class="p-4 border-t border-(--border-default) bg-(--surface-solid) flex gap-2">
            <!-- Action Button based on status -->
            <GlassButton v-if="cls.status === 'Open'" variant="primary" class="w-full text-xs justify-center" @click="openConfirmDialog('enroll', cls)">
              <template #leading><UserPlus :size="14" /></template>
              Đăng ký
            </GlassButton>
            
            <GlassButton v-else-if="cls.status === 'Full'" variant="primary" class="w-full text-xs justify-center bg-yellow-600 hover:bg-yellow-700 text-white border-none" @click="openConfirmDialog('waitlist', cls)">
              <template #leading><Clock :size="14" /></template>
              Vào hàng chờ
            </GlassButton>

            <template v-else-if="['Enrolled', 'Waitlist'].includes(cls.status)">
              <GlassButton variant="danger" class="w-full text-xs justify-center" @click="openConfirmDialog('withdraw', cls)">
                <template #leading><MinusCircle :size="14" /></template>
                Hủy đăng ký
              </GlassButton>
              
              <GlassButton v-if="cls.status === 'Enrolled'" variant="secondary" disabled class="shrink-0 opacity-40 cursor-not-allowed" title="Tính năng đổi lớp chéo đang được phát triển">
                <ArrowRightLeft :size="14" />
              </GlassButton>
            </template>

            <GlassButton v-else disabled variant="secondary" class="w-full text-xs justify-center opacity-50 cursor-not-allowed">
              <XCircle :size="14" />
              Đã đóng đăng ký
            </GlassButton>
          </div>
        </GlassPanel>
      </div>
    </template>

    <!-- Modal dialogs using ConfirmActionDialog -->
    <ConfirmActionDialog
      :modelValue="isDialogShow"
      @update:modelValue="isDialogShow = $event"
      :title="dialogTitle"
      :message="dialogMessage"
      :confirmLabel="dialogConfirmLabel"
      :variant="dialogVariant"
      :loading="confirmLoading"
      @confirm="handleDialogConfirm"
    />
  </div>
</template>

<style scoped>
.registration-page {
  padding: 1rem;
}
@media (min-width: 768px) {
  .registration-page {
    padding: 2rem;
  }
}
</style>
