<template>
  <div class="space-y-4 pb-10">

    <div v-if="loading" class="flex flex-col items-center justify-center py-20 gap-3">
      <Loader2 class="animate-spin text-muted" :size="28" />
      <p class="text-sm text-muted">Đang tải dữ liệu...</p>
    </div>

    <div v-else-if="apiError" class="surface-card border border-card rounded-2xl p-6 flex flex-col items-center justify-center gap-3">
      <AlertCircle :size="32" class="text-[var(--color-danger-text)]" />
      <p class="text-sm font-bold text-heading">Không thể tải dữ liệu</p>
      <p class="text-xs text-muted">{{ apiError }}</p>
      <button @click="initData" class="lg-button-primary px-4 py-2 text-xs font-bold rounded-xl mt-2">Thử lại</button>
    </div>

    <template v-else>

      <!-- ── Welcome Hero ── -->
      <div class="rounded-2xl surface-card border border-card p-4">
        <div class="flex flex-col md:flex-row items-center justify-between gap-4">
          <div class="max-w-xl text-center md:text-left">
            <h1 class="text-lg md:text-2xl font-semibold leading-tight tracking-tight text-heading">
              Tổng quan giáo vụ
            </h1>
            <p class="mt-2 text-muted text-sm">
              3 xung đột lịch &bull; 28 đơn chờ xử lý
            </p>
            <div class="mt-4 flex flex-wrap justify-center md:justify-start gap-2">
              <router-link to="/staff/conflicts" class="lg-button-primary rounded-lg px-3 py-2 text-xs font-bold transition-all active:scale-95 inline-flex items-center gap-1.5">
                <AlertTriangle :size="14" />
                Xử lý xung đột
              </router-link>
              <router-link to="/staff/requests" class="lg-button-secondary rounded-lg px-3 py-2 text-xs font-bold transition-all inline-flex items-center gap-1.5">
                <FileStack :size="14" />
                Duyệt đơn từ
              </router-link>
            </div>
          </div>
          <div class="hidden lg:block">
            <div class="flex h-12 w-12 items-center justify-center rounded-2xl bg-[var(--color-info-bg)]/70 border border-[var(--color-info-text)]/20">
              <ShieldCheck :size="22" class="text-[var(--color-info-text)]/70" />
            </div>
          </div>
        </div>
      </div>

      <!-- ── Stats Grid ── -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <div v-for="item in stats" :key="item.id" 
             class="group relative overflow-hidden rounded-2xl border border-card surface-card p-4 shadow-sm transition-all">
          <div class="flex items-center justify-between">
            <div :class="['flex h-10 w-10 items-center justify-center rounded-2xl transition-transform group-hover:scale-110', item.bgColor, item.iconColor]">
              <component :is="item.icon" :size="24" stroke-width="2.2" />
            </div>
            <div :class="['flex items-center gap-1 rounded-full px-2.5 py-1 text-[11px] font-bold', item.isWarning ? 'bg-[var(--color-danger-bg)] text-[var(--color-danger-text)]' : 'bg-[var(--color-success-bg)] text-[var(--color-success-text)]']">
              {{ item.trend }}
              <ArrowUpRight v-if="!item.isWarning" :size="12" />
              <AlertCircle v-else :size="12" />
            </div>
          </div>
          <div class="mt-5">
            <p class="text-sm font-medium text-muted">{{ item.label }}</p>
            <p class="mt-1 text-xl font-semibold text-heading">{{ item.value }}</p>
          </div>
        </div>
      </div>

      <!-- ── Main Layout ── -->
      <div class="grid grid-cols-1 xl:grid-cols-3 gap-4">
        
        <!-- Left: Schedule & Classes -->
        <div class="xl:col-span-2 space-y-4">
          
          <!-- Schedule Tasks -->
          <div class="rounded-2xl border border-card surface-card shadow-sm overflow-hidden">
            <div class="flex items-center justify-between border-b border-default px-4 py-4">
              <div>
                <h2 class="text-lg font-bold text-heading">Thời khóa biểu cần xử lý</h2>
                <p class="text-xs text-muted mt-0.5">Các vấn đề phát sinh trong việc xếp lịch</p>
              </div>
              <router-link to="/staff/schedule" class="text-xs font-bold text-link hover:underline">Xem tất cả</router-link>
            </div>
            <div class="p-3 space-y-3">
              <div v-for="(item, idx) in scheduleTasks" :key="item.id" 
                   class="group flex flex-col sm:flex-row items-start sm:items-center gap-3 rounded-2xl p-4 transition-all surface-solid border border-default">
                <div :class="['flex h-9 w-9 flex-shrink-0 items-center justify-center rounded-xl font-bold', item.alert ? 'bg-[var(--color-danger-bg)] text-[var(--color-danger-text)]' : 'bg-[var(--color-info-bg)] text-[var(--color-info-text)]']">
                  <component :is="item.alert ? AlertCircle : Clock" :size="20" />
                </div>
                <div class="flex-1 min-w-0">
                  <div class="flex items-center gap-2">
                    <h3 class="text-base font-bold text-heading truncate">{{ item.title }}</h3>
                    <span v-if="item.alert" class="rounded-full bg-[var(--color-danger-bg)] px-2 py-0.5 text-[10px] font-bold text-[var(--color-danger-text)] uppercase tracking-wider">Khẩn cấp</span>
                  </div>
                  <p class="mt-0.5 text-xs text-muted">{{ item.desc }}</p>
                </div>
                <div class="flex items-center gap-2 mt-2 sm:mt-0">
                  <router-link :to="item.link" class="lg-button-primary h-8 rounded-lg px-3 text-[10px] font-bold">Xử lý ngay</router-link>
                </div>
              </div>
            </div>
          </div>

          <!-- Class Sections Summary -->
          <div class="rounded-2xl border border-card surface-card shadow-sm overflow-hidden">
            <div class="flex items-center justify-between border-b border-default px-4 py-4">
              <h2 class="text-lg font-bold text-heading">Tình trạng lớp học phần</h2>
            </div>
            <div class="grid grid-cols-1 md:grid-cols-2 md:divide-x divide-default">
              <div class="p-4">
                <div class="flex items-center justify-between mb-3">
                  <h3 class="text-xs font-bold text-heading">Lớp sắp đầy (&gt;90%)</h3>
                  <span class="rounded-full bg-[var(--color-warning-bg)] px-2 py-0.5 text-[10px] font-bold text-[var(--color-warning-text)]">8 lớp</span>
                </div>
                <div class="space-y-3">
                  <div v-for="cls in nearFullClasses" :key="cls.name" class="flex items-center justify-between">
                    <div class="flex items-center gap-2">
                      <div class="h-1.5 w-1.5 rounded-full bg-[var(--color-warning-text)]"></div>
                      <span class="text-xs text-body">{{ cls.name }}</span>
                    </div>
                    <span class="text-xs font-bold text-heading">{{ cls.enrolled }}/{{ cls.capacity }}</span>
                  </div>
                </div>
                <button class="mt-4 text-xs font-bold text-link hover:underline">Mở thêm sức chứa →</button>
              </div>
              <div class="p-4">
                <div class="flex items-center justify-between mb-3">
                  <h3 class="text-xs font-bold text-heading">Waitlist cần duyệt</h3>
                  <span class="rounded-full bg-[var(--color-info-bg)] px-2 py-0.5 text-[10px] font-bold text-[var(--color-info-text)]">45 SV</span>
                </div>
                <div class="space-y-3">
                  <div v-for="cls in waitlistClasses" :key="cls.name" class="flex items-center justify-between">
                    <div class="flex items-center gap-2">
                      <div class="h-1.5 w-1.5 rounded-full bg-[var(--color-info-text)]"></div>
                      <span class="text-xs text-body">{{ cls.name }}</span>
                    </div>
                    <span class="text-xs font-bold text-heading">{{ cls.count }} SV chờ</span>
                  </div>
                </div>
                <button class="mt-4 text-xs font-bold text-link hover:underline">Xử lý danh sách chờ →</button>
              </div>
            </div>
          </div>

        </div>

        <!-- Right sidebar -->
        <div class="space-y-4">
          
          <!-- Urgent Requests -->
          <div class="rounded-2xl border border-card surface-card shadow-sm p-4">
            <div class="mb-3 flex items-center justify-between">
              <h3 class="text-base font-bold text-heading">Đơn từ khẩn</h3>
              <span class="flex h-5 w-5 items-center justify-center rounded-full bg-[var(--color-danger-bg)] text-[10px] font-bold text-[var(--color-danger-text)]">3</span>
            </div>
            <div class="space-y-3">
              <div v-for="req in urgentRequests" :key="req.id" 
                   class="flex items-start gap-3 rounded-xl p-3 transition-all surface-solid border border-default">
                <div class="mt-0.5 h-8 w-8 shrink-0 rounded-lg bg-[var(--color-danger-bg)] flex items-center justify-center text-[var(--color-danger-text)]">
                  <FileStack :size="16" />
                </div>
                <div class="flex-1 min-w-0">
                  <div class="flex justify-between items-start">
                    <p class="text-xs font-bold text-heading leading-tight">{{ req.type }}</p>
                    <span class="text-[10px] font-bold text-[var(--color-danger-text)] uppercase">{{ req.time }}</span>
                  </div>
                  <p class="mt-0.5 text-[11px] text-muted truncate">{{ req.studentName }}</p>
                </div>
              </div>
            </div>
            <button class="mt-4 w-full lg-button-primary h-9 rounded-lg text-[11px] font-bold">Xử lý toàn bộ đơn</button>
          </div>

          <!-- Semester Overview -->
          <div class="rounded-2xl border border-card surface-card shadow-sm p-4">
            <h3 class="text-base font-bold text-heading">Thống kê Học kỳ</h3>
            <p class="text-xs text-muted mt-1">Đã hoàn thành xếp lịch cho 85% các khoa.</p>
            
            <div class="mt-4 flex items-end gap-2 h-20">
              <div v-for="(h, i) in [60, 40, 80, 50, 70, 95, 65]" :key="i" 
                   class="flex-1 bg-[var(--color-info-bg)] rounded-t-lg transition-all hover:bg-[var(--surface-input)] cursor-help"
                   :style="{ height: h + '%' }" />
            </div>
            
            <div class="mt-4 grid grid-cols-2 gap-3">
              <div class="rounded-xl surface-solid p-3 border border-default">
                <p class="text-[10px] uppercase font-bold text-muted tracking-wider">Lớp học phần</p>
                <p class="text-base font-semibold mt-0.5 text-heading">1.240</p>
              </div>
              <div class="rounded-xl surface-solid p-3 border border-default">
                <p class="text-[10px] uppercase font-bold text-muted tracking-wider">Phòng trống</p>
                <p class="text-base font-semibold mt-0.5 text-heading">12%</p>
              </div>
            </div>
          </div>

          <!-- Announcements -->
          <div class="rounded-2xl border border-card surface-card shadow-sm p-4">
            <div class="mb-3 flex items-center justify-between">
              <h3 class="text-base font-bold text-heading">Thông báo</h3>
              <Bell :size="14" class="text-muted" />
            </div>
            <div class="space-y-3">
              <div v-for="notif in announcements" :key="notif.title" class="flex gap-3 p-3 rounded-xl surface-solid border border-default">
                <div :class="['h-8 w-8 rounded-full flex items-center justify-center shrink-0', notif.bg, notif.iconColor]">
                  <Bell :size="14" />
                </div>
                <div>
                  <p class="text-xs font-bold text-heading">{{ notif.title }}</p>
                  <p class="text-[11px] text-muted mt-0.5">{{ notif.desc }}</p>
                </div>
              </div>
            </div>
            <button class="mt-4 w-full lg-button-secondary h-9 rounded-lg text-[11px] font-bold">Tất cả thông báo</button>
          </div>

        </div>

      </div>
    </template>
  </div>
</template>

<script setup>
import { 
  Layers, FileStack, Calendar, Clock, AlertCircle, 
  ArrowUpRight, ShieldCheck, Bell, AlertTriangle, Loader2
} from 'lucide-vue-next'
import { onMounted, ref } from 'vue'

const loading = ref(true)
const apiError = ref('')

const stats = ref([])
const scheduleTasks = ref([])
const urgentRequests = ref([])
const nearFullClasses = ref([])
const waitlistClasses = ref([])
const announcements = ref([])

function initData() {
  loading.value = true
  apiError.value = ''
  setTimeout(() => {
    stats.value = [
      { id: 1, label: 'Lịch học hôm nay', value: '142', trend: '+12', bgColor: 'bg-[var(--color-info-bg)]', iconColor: 'text-[var(--color-info-text)]', icon: Calendar },
      { id: 2, label: 'Xung đột lịch', value: '3', trend: 'Cần xử lý', isWarning: true, bgColor: 'bg-[var(--color-danger-bg)]', iconColor: 'text-[var(--color-danger-text)]', icon: AlertTriangle },
      { id: 3, label: 'Lớp đang mở', value: '86', trend: '8 lớp đầy', isWarning: true, bgColor: 'bg-[var(--color-warning-bg)]', iconColor: 'text-[var(--color-warning-text)]', icon: Layers },
      { id: 4, label: 'Đơn từ chờ duyệt', value: '28', trend: '3 quá hạn', isWarning: true, bgColor: 'bg-[var(--color-info-bg)]', iconColor: 'text-[var(--color-info-text)]', icon: FileStack },
    ]
    scheduleTasks.value = [
      { id: 1, title: '2 Lịch trùng phòng học', desc: 'Phòng A201 (Tiết 1-3) đang có 2 lớp xếp chồng.', alert: true, link: '/staff/conflicts' },
      { id: 2, title: 'TKB Khoa CNTT đang chờ duyệt', desc: 'Đã submit ngày hôm qua, chưa có phản hồi từ BGH.', alert: false, link: '/staff/schedule' },
      { id: 3, title: 'Giảng viên nghỉ đột xuất', desc: 'TS. Nguyễn Văn A xin nghỉ chiều nay. Cần dạy thay.', alert: true, link: '/staff/assignments' },
    ]
    urgentRequests.value = [
      { id: 101, type: 'Xin chuyển lớp', studentName: 'Trần Bình', studentId: 'SE150212', time: '-2 NGÀY' },
      { id: 102, type: 'Xin thi lại', studentName: 'Lê Hoàng', studentId: 'SS140023', time: '-1 NGÀY' },
      { id: 103, type: 'Xin giấy xác nhận', studentName: 'Phạm Thu', studentId: 'SA160199', time: 'TRỄ 4H' },
    ]
    nearFullClasses.value = [
      { name: 'CT101 - Nhóm 1', enrolled: 47, capacity: 50 },
      { name: 'ITA201 - Nhóm 3', enrolled: 49, capacity: 50 },
      { name: 'MAT101 - Nhóm 2', enrolled: 46, capacity: 50 },
    ]
    waitlistClasses.value = [
      { name: 'ENG102 - Nhóm 1', count: 12 },
      { name: 'PHY101 - Nhóm 2', count: 8 },
      { name: 'CS102 - Nhóm 1', count: 15 },
    ]
    announcements.value = [
      { title: 'Mở đăng ký kỳ Thu 2026', desc: 'Hệ thống đã sẵn sàng cho đợt đăng ký tới.', bg: 'bg-[var(--color-info-bg)]', iconColor: 'text-[var(--color-info-text)]' },
      { title: 'Giảng viên cần nộp điểm', desc: 'Còn 12 lớp chưa nộp điểm giữa kỳ.', bg: 'bg-[var(--color-warning-bg)]', iconColor: 'text-[var(--color-warning-text)]' },
    ]
    loading.value = false
  }, 400)
}

onMounted(() => { initData() })
</script>

<style scoped>
.transition-all {
  transition-duration: 300ms;
}
</style>
