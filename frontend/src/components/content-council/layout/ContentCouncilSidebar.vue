<script setup>
import { ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import {
  BookOpen,
  CircleHelp,
  ClipboardCheck,
  LogOut,
  ChevronLeft,
  ChevronRight,
  User,
  GraduationCap
} from 'lucide-vue-next'

const props = defineProps({
  isCollapsed: {
    type: Boolean,
    default: false
  },
  isMobileDrawerOpen: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['update:isCollapsed', 'update:isMobileDrawerOpen'])

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const menuItems = [
  {
    name: 'Nội dung môn học',
    icon: BookOpen,
    route: 'content-council-subjects',
    path: '/content-council/subjects',
    disabled: false,
    tooltip: ''
  },
  {
    name: 'Ngân hàng câu hỏi',
    icon: CircleHelp,
    route: 'content-council-question-bank',
    path: '/content-council/question-bank',
    disabled: false,
    tooltip: ''
  },
  {
    name: 'Quiz / Đề kiểm tra',
    icon: ClipboardCheck,
    route: 'content-council-quizzes',
    path: '/content-council/quizzes',
    disabled: false,
    tooltip: ''
  }
]

const toggleCollapse = () => {
  emit('update:isCollapsed', !props.isCollapsed)
}

const closeDrawer = () => {
  emit('update:isMobileDrawerOpen', false)
}

const navigate = (item) => {
  if (item.disabled) return
  router.push(item.path)
  closeDrawer()
}

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}
</script>

<template>


  <!-- Sidebar Container -->
  <aside 
    class="lg-sidebar h-full flex flex-col bg-white border-r border-slate-200 transition-all duration-300 ease-in-out"
    :class="[
      isCollapsed ? 'w-[72px]' : 'w-[240px]'
    ]"
    :style="{
      '--sidebar-accent': '#2563eb',
      '--sidebar-indicator': '#2563eb',
      '--active-start': '#1e40af',
      '--active-mid': '#2563eb',
      '--active-end': '#3b82f6',
    }"
  >
    <!-- Sidebar Header -->
    <div class="h-16 flex items-center px-4 border-b border-slate-100 shrink-0">
      <div class="flex items-center gap-3 overflow-hidden whitespace-nowrap">
        <div class="w-10 h-10 rounded-lg bg-blue-600 text-white flex items-center justify-center shrink-0">
          <GraduationCap class="w-6 h-6" />
        </div>
        <div v-show="!isCollapsed" class="flex flex-col opacity-100 transition-opacity duration-300">
          <span class="font-bold text-slate-800 leading-tight">LMS System</span>
          <span class="text-xs text-slate-500 font-medium">Quản lý nội dung</span>
        </div>
      </div>
    </div>

    <!-- Collapse Toggle (Desktop only) -->
    <button 
      @click="toggleCollapse"
      class="hidden md:flex absolute -right-3 top-20 w-6 h-6 bg-white border border-slate-200 rounded-full items-center justify-center text-slate-500 hover:text-blue-600 hover:border-blue-300 shadow-sm transition-colors z-10"
      :aria-label="isCollapsed ? 'Mở rộng menu' : 'Thu gọn menu'"
    >
      <ChevronRight v-if="isCollapsed" class="w-4 h-4" />
      <ChevronLeft v-else class="w-4 h-4" />
    </button>

    <!-- Navigation Menu -->
    <nav class="flex-1 overflow-y-auto py-4 px-3 space-y-1 flex flex-col gap-1">
      <button
        v-for="item in menuItems"
        :key="item.name"
        @click="navigate(item)"
        class="w-full flex items-center gap-3 px-3 py-2.5 rounded-lg transition-all duration-200 group relative text-left"
        :class="[
          route.name === item.route 
            ? 'bg-blue-50 text-blue-700 font-medium' 
            : 'text-slate-600 hover:bg-slate-50',
          item.disabled ? 'opacity-50 cursor-not-allowed hover:bg-transparent' : 'cursor-pointer'
        ]"
        :title="isCollapsed ? (item.tooltip || item.name) : item.tooltip"
      >
        <!-- Active Indicator -->
        <div 
          v-if="route.name === item.route"
          class="absolute left-0 top-1/2 -translate-y-1/2 w-1 h-5 bg-blue-600 rounded-r-full"
        ></div>

        <component 
          :is="item.icon" 
          class="w-5 h-5 shrink-0 transition-colors"
          :class="route.name === item.route ? 'text-blue-600' : 'text-slate-400 group-hover:text-slate-600'" 
        />
        
        <span v-show="!isCollapsed" class="truncate transition-opacity duration-200">
          {{ item.name }}
        </span>
      </button>
    </nav>

    <!-- User Profile & Logout -->
    <div class="p-4 border-t border-slate-100 shrink-0">
      <div 
        class="flex items-center gap-3 overflow-hidden whitespace-nowrap mb-4"
        :class="isCollapsed ? 'justify-center' : ''"
      >
        <div class="w-10 h-10 rounded-full bg-slate-100 border border-slate-200 flex items-center justify-center shrink-0 text-slate-600">
          <span v-if="authStore.initials" class="font-medium text-sm">{{ authStore.initials }}</span>
          <User v-else class="w-5 h-5" />
        </div>
        <div v-show="!isCollapsed" class="flex flex-col">
          <span class="text-sm font-medium text-slate-800 truncate">{{ authStore.displayName }}</span>
          <span class="text-xs text-slate-500 truncate">Hội đồng quản lý nội dung</span>
        </div>
      </div>
      
      <button 
        @click="handleLogout"
        class="w-full flex items-center gap-3 px-3 py-2 text-slate-600 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors group"
        :class="isCollapsed ? 'justify-center' : ''"
        :title="isCollapsed ? 'Đăng xuất' : ''"
      >
        <LogOut class="w-5 h-5 shrink-0" />
        <span v-show="!isCollapsed" class="font-medium">Đăng xuất</span>
      </button>
    </div>
  </aside>
</template>
