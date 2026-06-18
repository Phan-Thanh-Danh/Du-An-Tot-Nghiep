<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useRecentFavoritesStore } from '@/stores/recentFavorites'
import * as LucideIcons from 'lucide-vue-next'

const router = useRouter()
const authStore = useAuthStore()
const recentStore = useRecentFavoritesStore()

const open = ref(false)
const menuRef = ref(null)

const actions = computed(() => {
  if (authStore.hasRole('Principal')) return [
    { id: 'bgh-schedule', label: 'Duyệt thời khóa biểu', icon: 'CalendarCheck', route: '/bgh/schedule/pending' },
    { id: 'bgh-evaluations', label: 'Đánh giá giảng viên', icon: 'Star', route: '/bgh/evaluations' },
    { id: 'bgh-curriculum', label: 'Khung chương trình', icon: 'Library', route: '/bgh/curriculum' },
  ]
  if (authStore.hasRole('Teacher')) return [
    { id: 'teacher-assignment', label: 'Tạo bài tập', icon: 'FilePlus', route: '/teacher/assignments' },
    { id: 'teacher-exam', label: 'Tạo đề thi', icon: 'ScrollText', route: '/teacher/assignments' },
    { id: 'teacher-lesson', label: 'Soạn bài giảng', icon: 'BookOpen', route: '/teacher/lessons' },
  ]
  if (authStore.hasRole('AcademicStaff')) return [
    { id: 'staff-notice', label: 'Gửi thông báo', icon: 'Megaphone', route: '/staff/notices/send' },
    { id: 'staff-period', label: 'Tạo đợt đăng ký', icon: 'CalendarPlus', route: '/staff/registrations' },
    { id: 'staff-assign', label: 'Phân công giảng viên', icon: 'UserPlus', route: '/staff/assignments' },
  ]
  return [
    { id: 'student-request', label: 'Gửi đơn từ', icon: 'FilePlus', route: '/student/requests' },
    { id: 'student-ticket', label: 'Tạo ticket hỗ trợ', icon: 'MessageSquarePlus', route: '/student/support-tickets' },
    { id: 'student-register', label: 'Đăng ký môn học', icon: 'ClipboardPlus', route: '/student/registrations' },
  ]
})

function execute(item) {
  if (item.route) {
    recentStore.visitPage({ path: item.route, label: item.label, icon: item.icon })
    router.push(item.route)
  }
  open.value = false
}

function toggle() {
  open.value = !open.value
}
</script>

<template>
  <div ref="menuRef" class="relative">
    <button
      class="flex h-8 w-8 items-center justify-center rounded-xl bg-gradient-to-br from-blue-600 to-cyan-600 text-white shadow-lg shadow-blue-500/24 dark:shadow-blue-500/10 transition-all hover:scale-105 hover:shadow-xl active:scale-95 focus:outline-none"
      aria-label="Tạo nhanh"
      :title="open ? 'Đóng' : 'Tạo nhanh'"
      @click="toggle"
    >
      <LucideIcons.Plus :size="16" :class="open ? 'rotate-45' : ''" class="transition-transform duration-200" />
    </button>

    <Transition
      enter-active-class="transition-all duration-200 ease-out"
      enter-from-class="opacity-0 translate-y-2 scale-95"
      enter-to-class="opacity-100 translate-y-0 scale-100"
      leave-active-class="transition-all duration-150 ease-in"
      leave-from-class="opacity-100 translate-y-0 scale-100"
      leave-to-class="opacity-0 translate-y-2 scale-95"
    >
      <div
        v-if="open"
        class="absolute right-0 top-[calc(100%+0.5rem)] z-[80] w-64 origin-top-right overflow-hidden rounded-[20px] border border-white/60 dark:border-white/10 bg-white/90 dark:bg-slate-900/85 p-1.5 shadow-[0_20px_50px_rgba(15,23,42,0.15)] dark:shadow-[0_20px_50px_rgba(2,6,23,0.4)] backdrop-blur-2xl"
        role="menu"
        @click.stop
      >
        <div class="px-3 py-2 text-[10px] font-bold uppercase tracking-wider text-slate-400 dark:text-slate-500">Tạo nhanh</div>
        <div class="space-y-0.5">
          <button
            v-for="action in actions"
            :key="action.id"
            class="flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-left text-sm font-semibold text-slate-700 dark:text-slate-300 transition-all hover:bg-blue-50 dark:hover:bg-blue-600/15 hover:text-blue-700 dark:hover:text-blue-300 focus:outline-none"
            role="menuitem"
            @click="execute(action)"
          >
            <component :is="LucideIcons[action.icon]" :size="16" class="flex-shrink-0 text-slate-400 dark:text-slate-500" />
            <span>{{ action.label }}</span>
          </button>
        </div>
      </div>
    </Transition>
  </div>
</template>
