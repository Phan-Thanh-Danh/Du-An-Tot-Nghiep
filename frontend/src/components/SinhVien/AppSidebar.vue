<script setup>
import { useRouter } from 'vue-router'
import {
  PanelLeftClose,
  PanelLeftOpen,
  HelpCircle,
  LogOut,
  GraduationCap,
} from 'lucide-vue-next'
import SidebarMenuGroup from './SidebarMenuGroup.vue'
import { sinhVienMenuGroups, mockUser } from './data/menuData.js'

defineProps({
  collapsed: { type: Boolean, default: false },
})

const emit = defineEmits(['toggle'])

const router = useRouter()

function logout() {
  // TODO: gọi auth store logout
  router.push('/login')
}
</script>

<template>
  <aside
    :class="[
      'relative flex flex-col h-full bg-white border-r border-slate-100 transition-all duration-300 ease-in-out select-none',
      collapsed ? 'w-[64px]' : 'w-[248px]',
    ]"
  >
    <!-- ──────────── LOGO / BRAND ──────────── -->
    <div
      class="flex items-center gap-3 border-b border-slate-100 px-4 py-4"
      :class="collapsed ? 'justify-center px-2' : ''"
    >
      <div class="flex-shrink-0 flex h-9 w-9 items-center justify-center rounded-xl bg-gradient-to-br from-blue-600 to-blue-500 shadow-md shadow-blue-200">
        <GraduationCap :size="20" color="white" :stroke-width="2.2" />
      </div>
      <Transition name="fade-slide">
        <div v-if="!collapsed" class="overflow-hidden">
          <p class="text-[15px] font-bold leading-tight text-slate-800">EduLMS</p>
          <p class="text-[11px] text-slate-400 leading-tight">Cổng học sinh</p>
        </div>
      </Transition>
    </div>

    <!-- ──────────── MENU SCROLL AREA ──────────── -->
    <nav class="flex-1 overflow-y-auto overflow-x-visible px-2 py-3 space-y-0.5 scrollbar-thin">
      <SidebarMenuGroup
        v-for="group in sinhVienMenuGroups"
        :key="group.id"
        :group="group"
        :collapsed="collapsed"
      />
    </nav>

    <!-- ──────────── BOTTOM: HELP + LOGOUT ──────────── -->
    <div class="border-t border-slate-100 px-2 py-3 space-y-0.5">
      <!-- Help -->
      <button
        :title="collapsed ? 'Trợ giúp' : ''"
        :class="[
          'group flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-sm text-slate-500 hover:bg-slate-100 hover:text-slate-700 transition-colors',
          collapsed ? 'justify-center' : '',
        ]"
      >
        <HelpCircle :size="18" stroke-width="1.8" class="flex-shrink-0 text-slate-400 group-hover:text-slate-600 transition-colors" />
        <span v-if="!collapsed" class="text-sm font-medium">Trợ giúp</span>
      </button>

      <!-- Logout -->
      <button
        :title="collapsed ? 'Đăng xuất' : ''"
        :class="[
          'group flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-sm text-red-500 hover:bg-red-50 hover:text-red-600 transition-colors',
          collapsed ? 'justify-center' : '',
        ]"
        @click="logout"
      >
        <LogOut :size="18" stroke-width="1.8" class="flex-shrink-0 transition-colors" />
        <span v-if="!collapsed" class="text-sm font-medium">Đăng xuất</span>
      </button>
    </div>

    <!-- ──────────── USER CARD ──────────── -->
    <div
      class="border-t border-slate-100 p-3"
      :class="collapsed ? 'flex justify-center' : ''"
    >
      <div :class="['flex items-center gap-3', collapsed ? '' : '']">
        <!-- Avatar -->
        <div class="flex-shrink-0 flex h-8 w-8 items-center justify-center rounded-full bg-gradient-to-br from-blue-500 to-indigo-600 text-white text-xs font-bold shadow">
          {{ mockUser.initials }}
        </div>
        <Transition name="fade-slide">
          <div v-if="!collapsed" class="overflow-hidden min-w-0">
            <p class="text-[13px] font-semibold text-slate-700 truncate leading-tight">{{ mockUser.name }}</p>
            <p class="text-[11px] text-slate-400 truncate leading-tight">{{ mockUser.studentId }}</p>
          </div>
        </Transition>
      </div>
    </div>

    <!-- ──────────── TOGGLE BUTTON ──────────── -->
    <button
      class="absolute -right-3 top-[60px] z-10 flex h-6 w-6 items-center justify-center rounded-full border border-slate-200 bg-white shadow-md text-slate-500 hover:text-blue-600 hover:border-blue-300 transition-all"
      :title="collapsed ? 'Mở rộng sidebar' : 'Thu gọn sidebar'"
      @click="emit('toggle')"
    >
      <component :is="collapsed ? PanelLeftOpen : PanelLeftClose" :size="13" stroke-width="2" />
    </button>
  </aside>
</template>

<style scoped>
/* Smooth fade + slide cho label text */
.fade-slide-enter-active,
.fade-slide-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}
.fade-slide-enter-from,
.fade-slide-leave-to {
  opacity: 0;
  transform: translateX(-6px);
}

/* Scrollbar nhỏ cho nav */
.scrollbar-thin::-webkit-scrollbar {
  width: 3px;
}
.scrollbar-thin::-webkit-scrollbar-track {
  background: transparent;
}
.scrollbar-thin::-webkit-scrollbar-thumb {
  background: #e2e8f0;
  border-radius: 999px;
}
</style>
