<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import {
  Users,
  Search,
  UserCheck,
  ShieldCheck,
  ExternalLink,
  Calendar,
  CreditCard,
  AlertCircle
} from 'lucide-vue-next'
import { parentApi } from '@/services/parentApi'
import { setActiveChildId } from '@/components/PhuHuynh/data/parentState.js'

const router = useRouter()
const searchQuery = ref('')
const loading = ref(false)
const error = ref('')
const childrenList = ref([])

const filteredChildren = computed(() => {
  const query = searchQuery.value.trim().toLowerCase()
  if (!query) return childrenList.value
  return childrenList.value.filter(child =>
    child.name.toLowerCase().includes(query) ||
    child.studentId.toLowerCase().includes(query) ||
    child.class.toLowerCase().includes(query)
  )
})

function navigateToOverview(childId) {
  setActiveChildId(childId)
  router.push({
    path: '/parent/children/overview',
    query: { studentId: childId }
  })
}

function navigateToSchedule(childId) {
  setActiveChildId(childId)
  router.push('/parent/learning/schedule')
}

function navigateToFinance(childId) {
  setActiveChildId(childId)
  router.push('/parent/finance/tuition')
}

function getInitials(name = '') {
  return name.split(' ').filter(Boolean).slice(-2).map(part => part.charAt(0)).join('').toUpperCase() || '-'
}

async function loadChildren() {
  loading.value = true
  error.value = ''
  try {
    const res = await parentApi.getChildren()
    childrenList.value = (res?.data || []).map(child => ({
      id: child.id,
      name: child.name || '',
      studentId: child.email || `ID ${child.id}`,
      class: child.className || 'Chưa có lớp',
      status: child.status || '',
      linkedDate: '',
      permissions: [],
      avatarInitials: getInitials(child.name),
    }))
  } catch (e) {
    error.value = e?.message || 'Không thể tải danh sách học sinh.'
  } finally {
    loading.value = false
  }
}

onMounted(loadChildren)
</script>

<template>
  <div class="space-y-6">
    <!-- ── THÔNG TIN ĐẦU TRANG ── -->
    <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
      <div>
        <h2 class="text-lg font-bold text-heading flex items-center gap-2">
          <Users :size="20" class="text-orange-600" />
          Con của tôi
        </h2>
        <p class="text-xs text-body">Xem danh sách các con đang theo học và quản lý quyền truy cập thông tin</p>
      </div>

      <!-- Tìm kiếm -->
      <div class="relative w-full md:w-72">
        <span class="absolute inset-y-0 left-0 flex items-center pl-3 text-muted pointer-events-none">
          <Search :size="16" />
        </span>
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Tìm kiếm theo tên hoặc mã số..."
          class="surface-input border-card w-full pl-9 pr-4 py-2 text-xs rounded-xl border focus:outline-none focus:ring-2 focus:ring-orange-500/20"
        />
      </div>
    </div>

    <!-- ── QUY TẮC LIÊN KẾT (RULES BANNER) ── -->
    <div class="lg-alert lg-alert-info">
      <AlertCircle :size="20" class="flex-shrink-0 mt-0.5" />
      <div class="text-sm space-y-1.5">
        <p class="font-bold">Quy tắc liên kết Phụ huynh - Học sinh:</p>
        <ul class="list-disc list-inside space-y-1 opacity-90 text-xs">
          <li>Phụ huynh chỉ xem được dữ liệu học sinh khi trạng thái liên kết <strong>(ParentLink)</strong> đang hoạt động.</li>
          <li>Một học sinh được liên kết tối đa <strong>3 phụ huynh</strong> (Cha, Mẹ, Người giám hộ).</li>
          <li>Học sinh có quyền thu hồi hoặc chỉnh sửa các quyền truy cập thông tin bất cứ lúc nào. Khi đó, tài khoản phụ huynh sẽ mất quyền truy cập tương ứng ngay lập tức.</li>
        </ul>
      </div>
    </div>

    <!-- ── DANH SÁCH HỌC SINH ── -->
    <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
      <div
        v-for="child in filteredChildren"
        :key="child.id"
        class="lg-card-glass flex flex-col justify-between hover:border-orange-500/30 transition-all duration-300 relative group"
      >
        <!-- Background Glow -->
        <div class="absolute inset-0 bg-gradient-to-br from-orange-500/0 to-orange-500/5 opacity-0 group-hover:opacity-100 transition-opacity rounded-[22px] pointer-events-none" />

        <div class="relative z-10 flex flex-col h-full">
          <!-- Thông tin cơ bản -->
          <div class="flex items-center gap-4 mb-5">
            <div class="h-16 w-16 flex-shrink-0 flex items-center justify-center rounded-[18px] bg-gradient-to-br from-orange-400 to-orange-600 text-white font-bold text-xl shadow-lg shadow-orange-500/30">
              {{ child.avatarInitials }}
            </div>
            <div class="flex-1 min-w-0">
              <h3 class="text-lg font-bold text-heading truncate">{{ child.name }}</h3>
              <p class="text-sm font-medium text-muted mt-0.5 truncate">ID: {{ child.studentId }} <span class="mx-1.5">•</span> Lớp: <span class="text-body font-semibold">{{ child.class }}</span></p>
            </div>
          </div>
          
          <!-- Trạng thái liên kết -->
          <div class="flex items-center justify-between py-3 border-y border-card/60 mb-5">
            <span class="text-xs font-semibold text-muted">Trạng thái liên kết</span>
            <div class="flex items-center gap-2">
              <span v-if="child.linkedDate" class="text-[10px] font-semibold text-muted">Từ {{ child.linkedDate }}</span>
              <span class="lg-badge lg-badge-success py-1">
                <UserCheck :size="12" /> Đang liên kết
              </span>
            </div>
          </div>

          <!-- Chi tiết liên hệ -->
          <div class="flex-grow mb-5">
            <p class="text-xs font-semibold text-muted mb-2.5">Quyền hạn truy cập:</p>
            <div v-if="child.permissions.length" class="flex flex-wrap gap-1.5">
              <span
                v-for="perm in child.permissions"
                :key="perm"
                class="lg-badge bg-orange-50 dark:bg-orange-500/10 text-orange-700 dark:text-orange-400 border border-orange-200 dark:border-orange-500/20"
              >
                <ShieldCheck :size="12" />
                {{ perm }}
              </span>
            </div>
            <div v-else class="text-xs text-muted italic">Không có quyền cụ thể</div>
          </div>

          <!-- Các nút tác vụ nhanh -->
          <div class="grid grid-cols-2 gap-3 mt-auto relative z-10">
            <button
              @click="navigateToSchedule(child.id)"
              class="lg-btn-secondary text-sm font-bold shadow-sm h-[42px]"
              title="Xem thời khóa biểu"
            >
              <Calendar :size="16" class="text-muted" /> Lịch học
            </button>
            
            <button
              @click="navigateToFinance(child.id)"
              class="lg-btn-secondary text-sm font-bold shadow-sm h-[42px]"
              title="Đóng học phí"
            >
              <CreditCard :size="16" class="text-muted" /> Học phí
            </button>

            <button
              @click="navigateToOverview(child.id)"
              class="col-span-2 text-sm font-bold h-[44px] rounded-xl shadow-md transition-all flex justify-center items-center gap-2 bg-gradient-to-r from-orange-500 to-amber-500 hover:from-orange-600 hover:to-amber-600 text-white"
            >
              Tổng quan học tập <ExternalLink :size="16" />
            </button>
          </div>
        </div>
      </div>
    </div>
    <div v-if="loading" class="lg-card-glass p-8 text-center text-xs text-muted">Đang tải danh sách học sinh...</div>
    <div v-else-if="error" class="lg-card-glass p-8 text-center text-xs text-red-600">{{ error }}</div>
    <div v-else-if="filteredChildren.length === 0" class="lg-card-glass p-8 text-center text-xs text-muted">Không có học sinh liên kết.</div>
  </div>
</template>

<style scoped>
.text-heading {
  color: var(--text-heading);
}
.text-body {
  color: var(--text-body);
}
.text-muted {
  color: var(--text-muted);
}
.border-card {
  border-color: var(--border-card);
}
</style>
