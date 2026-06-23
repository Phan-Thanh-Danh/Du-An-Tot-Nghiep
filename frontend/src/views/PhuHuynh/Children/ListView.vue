<script setup>
import { ref } from 'vue'
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

const router = useRouter()
const searchQuery = ref('')

// Mock danh sách học sinh liên kết
const childrenList = ref([
  {
    id: 1,
    name: 'Nguyễn Minh Quân',
    studentId: 'SV2024001',
    class: 'CNTT K26A',
    email: 'quan.nm2024@student.edu.vn',
    status: 'Active',
    linkedDate: '10/01/2026',
    permissions: ['Xem bảng điểm', 'Xem điểm danh', 'Xem thời khóa biểu', 'Xem học phí'],
    avatarInitials: 'MQ'
  },
  {
    id: 2,
    name: 'Nguyễn Khánh Linh',
    studentId: 'SV2024045',
    class: 'CNTT K26B',
    email: 'linh.nk2024@student.edu.vn',
    status: 'Active',
    linkedDate: '12/01/2026',
    permissions: ['Xem bảng điểm', 'Xem điểm danh', 'Xem thời khóa biểu'],
    avatarInitials: 'KL'
  }
])

function navigateToOverview(childId) {
  router.push({
    path: '/parent/children/overview',
    query: { studentId: childId }
  })
}

function navigateToSchedule() {
  router.push('/parent/learning/schedule')
}

function navigateToFinance() {
  router.push('/parent/finance/tuition')
}
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
    <div class="p-4 rounded-xl border border-orange-200 dark:border-orange-950/20 bg-orange-50/40 dark:bg-orange-950/5 flex gap-3">
      <AlertCircle :size="18" class="text-orange-600 flex-shrink-0 mt-0.5" />
      <div class="text-xs text-body space-y-1">
        <p class="font-bold text-slate-800 dark:text-slate-200">Quy tắc liên kết Phụ huynh - Học sinh:</p>
        <ul class="list-disc list-inside space-y-1 text-slate-600 dark:text-slate-400">
          <li>Phụ huynh chỉ xem được dữ liệu học sinh khi trạng thái liên kết <strong>(ParentLink)</strong> đang hoạt động.</li>
          <li>Một học sinh được liên kết tối đa <strong>3 phụ huynh</strong> (Cha, Mẹ, Người giám hộ).</li>
          <li>Học sinh có quyền thu hồi hoặc chỉnh sửa các quyền truy cập thông tin bất cứ lúc nào. Khi đó, tài khoản phụ huynh sẽ mất quyền truy cập tương ứng ngay lập tức.</li>
        </ul>
      </div>
    </div>

    <!-- ── DANH SÁCH HỌC SINH ── -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <div
        v-for="child in childrenList"
        :key="child.id"
        class="lg-card-glass p-5 flex flex-col justify-between hover:border-orange-500/30 transition-all duration-300 relative group"
      >
        <!-- Background Glow -->
        <div class="absolute inset-0 bg-gradient-to-tr from-orange-500/0 to-orange-500/[0.02] opacity-0 group-hover:opacity-100 transition-opacity rounded-[var(--radius-xl)] pointer-events-none" />

        <div class="relative z-10">
          <!-- Thông tin cơ bản -->
          <div class="flex items-start justify-between border-b border-card pb-4 mb-4">
            <div class="flex items-center gap-3">
              <div class="h-12 w-12 flex items-center justify-center rounded-2xl bg-orange-100 dark:bg-orange-950/30 text-orange-700 font-extrabold text-sm">
                {{ child.avatarInitials }}
              </div>
              <div>
                <h3 class="text-sm font-bold text-heading">{{ child.name }}</h3>
                <p class="text-[11px] text-muted">{{ child.studentId }} | Lớp: {{ child.class }}</p>
              </div>
            </div>
            
            <div class="flex flex-col items-end gap-1.5">
              <span class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-bold bg-emerald-100 text-emerald-700 dark:bg-emerald-950/30 dark:text-emerald-400">
                <UserCheck :size="10" /> Đang liên kết
              </span>
              <span class="text-[9px] text-muted">Từ ngày {{ child.linkedDate }}</span>
            </div>
          </div>

          <!-- Chi tiết liên hệ -->
          <div class="space-y-1.5 mb-4 text-xs">
            <p class="flex items-center gap-2 text-body">
              <span class="text-muted w-16">Email:</span>
              <span class="font-medium truncate">{{ child.email }}</span>
            </p>
            <div class="flex items-start gap-2 text-body">
              <span class="text-muted w-16 flex-shrink-0 mt-0.5">Quyền hạn:</span>
              <div class="flex flex-wrap gap-1">
                <span
                  v-for="perm in child.permissions"
                  :key="perm"
                  class="inline-flex items-center gap-1 px-2 py-0.5 rounded bg-slate-100 dark:bg-slate-800 text-[10px] font-semibold text-slate-700 dark:text-slate-300"
                >
                  <ShieldCheck :size="9" class="text-orange-600" />
                  {{ perm }}
                </span>
              </div>
            </div>
          </div>
        </div>

        <!-- Các nút tác vụ nhanh -->
        <div class="border-t border-card pt-4 mt-auto flex flex-wrap gap-2 justify-end relative z-10">
          <button
            @click="navigateToSchedule(child.id)"
            class="lg-button-secondary text-xs px-3 py-1.5 rounded-xl border border-card flex items-center gap-1.5 font-semibold text-label hover:text-orange-600"
            title="Xem thời khóa biểu"
          >
            <Calendar :size="13" /> Lịch học
          </button>
          
          <button
            @click="navigateToFinance(child.id)"
            class="lg-button-secondary text-xs px-3 py-1.5 rounded-xl border border-card flex items-center gap-1.5 font-semibold text-label hover:text-orange-600"
            title="Đóng học phí"
          >
            <CreditCard :size="13" /> Học phí
          </button>

          <button
            @click="navigateToOverview(child.id)"
            class="lg-button-primary bg-orange-600 hover:bg-orange-700 text-white text-xs px-4 py-1.5 rounded-xl flex items-center gap-1.5 font-semibold"
          >
            Tổng quan học tập <ExternalLink :size="12" />
          </button>
        </div>
      </div>
    </div>
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
