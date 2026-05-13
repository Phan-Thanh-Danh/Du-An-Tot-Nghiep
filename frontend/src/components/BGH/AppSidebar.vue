<script setup>
import { useRouter } from 'vue-router'
import {
  PanelLeftClose,
  PanelLeftOpen,
  HelpCircle,
  LogOut,
  GraduationCap,
} from 'lucide-vue-next'
import SidebarMenuGroup from '../SinhVien/SidebarMenuGroup.vue'
import { bghMenuGroups, mockBGH } from './data/menuData.js'
import { useAuthStore } from '@/stores/auth'

defineProps({
  collapsed: { type: Boolean, default: false },
})

const emit = defineEmits(['toggle'])

const router = useRouter()
const authStore = useAuthStore()

function logout() {
  authStore.logout()
  router.replace('/login')
}
</script>

<template>
  <aside
    :class="[
      'relative flex flex-col h-full bg-white/70 backdrop-blur-md border-r border-slate-200/50 transition-all duration-300 ease-in-out select-none',
      collapsed ? 'w-[64px]' : 'w-[248px]',
    ]"
  >
    <!-- ──────────── LOGO / BRAND ──────────── -->
    <div
      class="flex items-center gap-3 border-b border-slate-200/50 px-4 py-4"
      :class="collapsed ? 'justify-center px-2' : ''"
    >
      <div class="flex-shrink-0 flex h-9 w-9 items-center justify-center rounded-xl bg-gradient-to-br from-indigo-600 to-indigo-500 shadow-md shadow-indigo-200">
        <GraduationCap :size="20" color="white" :stroke-width="2.2" />
      </div>
      <Transition name="fade-slide">
        <div v-if="!collapsed" class="overflow-hidden">
          <p class="text-[15px] font-bold leading-tight text-slate-800">EduLMS</p>
          <p class="text-[11px] text-slate-500 leading-tight">Ban Giám Hiệu</p>
        </div>
      </Transition>
    </div>

    <!-- ──────────── MENU SCROLL AREA ──────────── -->
    <nav class="flex-1 overflow-y-auto overflow-x-visible px-2 py-3 space-y-0.5 scrollbar-thin">
      <SidebarMenuGroup
        v-for="group in bghMenuGroups"
        :key="group.id"
        :group="group"
        :collapsed="collapsed"
      />
    </nav>

    <!-- ──────────── BOTTOM: HELP + LOGOUT ──────────── -->
    <div class="border-t border-slate-200/50 px-2 py-3 space-y-0.5">
      <button
        :title="collapsed ? 'Trợ giúp' : ''"
        :class="[
          'group flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-sm text-slate-600 hover:bg-white/50 hover:text-slate-800 transition-colors',
          collapsed ? 'justify-center' : '',
        ]"
      >
        <HelpCircle :size="18" stroke-width="1.8" class="flex-shrink-0 text-slate-400 group-hover:text-slate-600 transition-colors" />
        <span v-if="!collapsed" class="text-sm font-medium">Trợ giúp</span>
      </button>

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
      class="border-t border-slate-200/50 p-3"
      :class="collapsed ? 'flex justify-center' : ''"
    >
      <div :class="['flex items-center gap-3', collapsed ? '' : '']">
        <div class="flex-shrink-0 flex h-8 w-8 items-center justify-center rounded-full bg-gradient-to-br from-indigo-500 to-purple-600 text-white text-xs font-bold shadow">
          {{ authStore.initials || mockBGH.initials }}
        </div>
        <Transition name="fade-slide">
          <div v-if="!collapsed" class="overflow-hidden min-w-0">
            <p class="text-[13px] font-semibold text-slate-800 truncate leading-tight">
              {{ authStore.displayName || mockBGH.name }}
            </p>
            <p class="text-[11px] text-slate-500 truncate leading-tight">
              {{ authStore.user?.email || mockBGH.email }}
            </p>
          </div>
        </Transition>
      </div>
    </div>

    <!-- ──────────── TOGGLE BUTTON ──────────── -->
    <button
      class="absolute -right-3 top-[60px] z-10 flex h-6 w-6 items-center justify-center rounded-full border border-slate-200 bg-white shadow-md text-slate-500 hover:text-indigo-600 hover:border-indigo-300 transition-all"
      :title="collapsed ? 'Mở rộng sidebar' : 'Thu gọn sidebar'"
      @click="emit('toggle')"
    >
      <component :is="collapsed ? PanelLeftOpen : PanelLeftClose" :size="13" stroke-width="2" />
    </button>
  </aside>
</template>

<style scoped>
.fade-slide-enter-active,
.fade-slide-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}
.fade-slide-enter-from,
.fade-slide-leave-to {
  opacity: 0;
  transform: translateX(-6px);
}

.scrollbar-thin::-webkit-scrollbar {
  width: 3px;
}
.scrollbar-thin::-webkit-scrollbar-track {
  background: transparent;
}
.scrollbar-thin::-webkit-scrollbar-thumb {
  background: rgba(226, 232, 240, 0.8);
  border-radius: 999px;
}
</style>
