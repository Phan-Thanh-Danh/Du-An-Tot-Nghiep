<script setup>
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  ShieldAlert,
  ChevronDown,
  ChevronLeft,
  CheckCircle,
  Eye,
  AlertTriangle,
  Info,
  Calendar,
  X
} from 'lucide-vue-next'
import { childrenData, setActiveChildId } from '@/components/PhuHuynh/data/parentData.js'
import { usePopupStore } from '@/stores/popup'

const route = useRoute()
const router = useRouter()
const popupStore = usePopupStore()

// Lấy studentId từ query URL hoặc local storage, mặc định là 1
const activeChildId = ref(Number(route.query.studentId) || Number(localStorage.getItem('parent_active_student_id')) || 1)
const dropdownOpen = ref(false)

const currentChild = computed(() => {
  return childrenData.find(c => c.id === activeChildId.value) || childrenData[0]
})

function selectChild(id) {
  activeChildId.value = id
  setActiveChildId(id)
  dropdownOpen.value = false
  router.replace({ query: { studentId: id } })
}

// Lấy danh sách thông báo hệ thống
const systemNotifications = computed(() => {
  return currentChild.value.systemNotifications || []
})

// Số thông báo chưa đọc
const unreadCount = computed(() => {
  return systemNotifications.value.filter(n => !n.read).length
})

// Đánh dấu đã đọc một thông báo
function markAsRead(notification) {
  notification.read = true
  popupStore.success('Đã đọc', 'Đã đánh dấu thông báo là đã đọc.')
}

// Đánh dấu tất cả đã đọc
function markAllAsRead() {
  const unreadList = systemNotifications.value.filter(n => !n.read)
  if (unreadList.length === 0) {
    popupStore.info('Thông báo', 'Tất cả các thông báo hệ thống đã được đọc.')
    return
  }
  unreadList.forEach(n => {
    n.read = true
  })
  popupStore.success('Thành công', 'Đã đánh dấu tất cả các thông báo hệ thống là đã đọc.')
}

// State mở modal xem chi tiết
const selectedNotif = ref(null)
const isDetailModalOpen = ref(false)

function openDetail(notif) {
  notif.read = true // tự động đánh dấu đã đọc khi mở xem
  selectedNotif.value = notif
  isDetailModalOpen.value = true
}

function goBack() {
  router.push('/parent/dashboard')
}
</script>

<template>
  <div class="space-y-6">
    <!-- ── THANH TIÊU ĐỀ & CHỌN HỌC SINH ── -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
      <div class="flex items-center gap-2">
        <button
          @click="goBack"
          class="lg-icon-button flex h-8 w-8 text-muted hover:text-orange-600 border border-card surface-card rounded-lg"
          title="Quay lại"
        >
          <ChevronLeft :size="18" />
        </button>
        <div>
          <h2 class="text-lg font-bold text-heading flex items-center gap-2">
            <ShieldAlert :size="20" class="text-orange-600" />
            Cảnh báo từ hệ thống
          </h2>
          <p class="text-xs text-body">Xem các cảnh báo tự động về điểm danh, điểm số và học phí</p>
        </div>
      </div>

      <!-- Chọn học sinh nhanh -->
      <div class="relative min-w-[220px]">
        <button
          type="button"
          class="surface-input border-card flex w-full items-center justify-between gap-2.5 rounded-xl border px-3.5 py-2 text-xs font-semibold text-heading shadow-sm transition-all focus:outline-none"
          @click="dropdownOpen = !dropdownOpen"
        >
          <div class="flex items-center gap-2">
            <div class="h-5 w-5 flex items-center justify-center rounded-full bg-orange-600 text-[9px] font-bold text-white">
              {{ currentChild.name.split(' ').pop().charAt(0) }}
            </div>
            <span>{{ currentChild.name }}</span>
          </div>
          <ChevronDown :size="14" class="text-muted transition-transform" :class="dropdownOpen ? 'rotate-180' : ''" />
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
            v-if="dropdownOpen"
            class="surface-dropdown absolute right-0 top-[calc(100%+0.5rem)] z-50 w-full rounded-xl border border-card p-1 shadow-(--lg-shadow-md)"
          >
            <button
              v-for="child in childrenData"
              :key="child.id"
              type="button"
              class="flex w-full items-center justify-between rounded-lg px-2.5 py-1.5 text-left text-xs font-medium text-label transition hover:bg-(--surface-card-hover)"
              @click="selectChild(child.id)"
            >
              <span>{{ child.name }} ({{ child.class }})</span>
            </button>
          </div>
        </Transition>
      </div>
    </div>

    <!-- ── DANH SÁCH CẢNH BÁO HỆ THỐNG ── -->
    <div class="lg-card-glass p-5 space-y-4">
      <div class="flex items-center justify-between pb-3 border-b border-card">
        <div class="flex items-center gap-2">
          <h3 class="text-xs font-bold text-heading uppercase tracking-wide">
            Danh sách cảnh báo tự động
          </h3>
          <span
            v-if="unreadCount > 0"
            class="px-2 py-0.5 rounded-full text-[9px] font-bold bg-orange-100 text-orange-700 animate-pulse"
          >
            {{ unreadCount }} chưa đọc
          </span>
        </div>

        <button
          @click="markAllAsRead"
          class="text-xs font-bold text-orange-600 hover:text-orange-700 transition"
        >
          Đánh dấu đã đọc tất cả
        </button>
      </div>

      <div v-if="systemNotifications.length === 0" class="text-center py-12 text-muted text-xs">
        Tuyệt vời! Hiện tại không có cảnh báo hệ thống nào cho học sinh này.
      </div>
      
      <div v-else class="space-y-3">
        <div
          v-for="notif in systemNotifications"
          :key="notif.id"
          class="p-4 rounded-xl border transition flex gap-3.5 items-start"
          :class="[
            notif.read ? 'border-card opacity-60' : 'border-orange-200 bg-orange-50/5 dark:border-orange-950/20 dark:bg-orange-950/5',
          ]"
        >
          <!-- Icon theo loại cảnh báo -->
          <div class="flex-shrink-0 mt-0.5">
            <span
              v-if="notif.type === 'danger'"
              class="h-7 w-7 rounded-lg bg-red-100 text-red-700 dark:bg-red-950/30 dark:text-red-400 flex items-center justify-center"
            >
              <AlertTriangle :size="14" />
            </span>
            <span
              v-else-if="notif.type === 'warning'"
              class="h-7 w-7 rounded-lg bg-amber-100 text-amber-700 dark:bg-amber-950/30 dark:text-amber-400 flex items-center justify-center"
            >
              <AlertTriangle :size="14" />
            </span>
            <span
              v-else
              class="h-7 w-7 rounded-lg bg-blue-100 text-blue-700 dark:bg-blue-950/30 dark:text-blue-400 flex items-center justify-center"
            >
              <Info :size="14" />
            </span>
          </div>

          <!-- Nội dung tin -->
          <div class="flex-1 min-w-0">
            <div class="flex items-start justify-between gap-3">
              <div>
                <h4
                  class="text-xs font-bold text-heading leading-snug flex items-center gap-1.5"
                  :class="notif.read ? 'font-medium text-body' : 'font-extrabold'"
                >
                  {{ notif.title }}
                  <span
                    v-if="!notif.read"
                    class="h-1.5 w-1.5 rounded-full bg-orange-600 flex-shrink-0"
                  ></span>
                </h4>
                <p class="text-[11px] text-body mt-1 leading-relaxed line-clamp-2">
                  {{ notif.content }}
                </p>
              </div>

              <!-- Action button desktop -->
              <div class="flex items-center gap-1.5 flex-shrink-0">
                <button
                  @click="openDetail(notif)"
                  class="p-1 border border-card rounded-lg hover:text-orange-600 transition"
                  title="Xem chi tiết"
                >
                  <Eye :size="12" />
                </button>
                <button
                  v-if="!notif.read"
                  @click="markAsRead(notif)"
                  class="p-1 border border-card rounded-lg text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-950/20 transition"
                  title="Đánh dấu đã đọc"
                >
                  <CheckCircle :size="12" />
                </button>
              </div>
            </div>

            <!-- Date time footer -->
            <div class="flex items-center gap-1.5 mt-2.5 text-[10px] text-muted font-semibold">
              <Calendar :size="11" />
              <span>{{ notif.date }}</span>
              <span>•</span>
              <span>Gửi tự động từ hệ thống</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- ── MODAL CHI TIẾT CẢNH BÁO ── -->
    <div v-if="isDetailModalOpen && selectedNotif" class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <!-- Overlay -->
      <div @click="isDetailModalOpen = false" class="absolute inset-0 bg-slate-900/40 dark:bg-slate-950/60 backdrop-blur-sm" />

      <!-- Modal Content -->
      <div class="lg-modal w-full max-w-md relative z-10 flex flex-col rounded-2xl shadow-xl overflow-hidden">
        
        <!-- Header -->
        <div class="flex items-center justify-between pb-3 border-b border-card">
          <h2 class="text-sm font-bold text-heading flex items-center gap-1.5">
            <ShieldAlert :size="16" class="text-orange-600" />
            Chi tiết cảnh báo
          </h2>
          <button @click="isDetailModalOpen = false" class="text-muted hover:text-orange-600">
            <X :size="16" />
          </button>
        </div>

        <!-- Body -->
        <div class="p-4 space-y-4">
          <div class="space-y-1">
            <span
              class="inline-block px-2 py-0.5 rounded text-[8px] font-extrabold uppercase"
              :class="
                selectedNotif.type === 'danger' ? 'bg-red-100 text-red-700 dark:bg-red-950/30' :
                selectedNotif.type === 'warning' ? 'bg-amber-100 text-amber-700 dark:bg-amber-950/30' :
                'bg-blue-100 text-blue-700 dark:bg-blue-950/30'
              "
            >
              {{ selectedNotif.type === 'danger' ? 'Khẩn cấp' : selectedNotif.type === 'warning' ? 'Cảnh báo' : 'Thông tin' }}
            </span>
            <h3 class="text-xs font-bold text-heading leading-snug">{{ selectedNotif.title }}</h3>
            <p class="text-[10px] text-muted">{{ selectedNotif.date }} • Hệ thống LMS tự động gửi</p>
          </div>

          <div class="p-3.5 surface-input rounded-xl border border-card text-[11px] text-body leading-relaxed">
            {{ selectedNotif.content }}
          </div>

          <p class="text-[10px] text-muted italic leading-relaxed">
            * Cảnh báo này được thiết lập tự động dựa trên dữ liệu học tập/chuyên cần thực tế của học sinh tại trường. Đề nghị phụ huynh theo dõi sát sao.
          </p>

          <!-- Action -->
          <div class="pt-3 border-t border-card flex justify-end">
            <button
              @click="isDetailModalOpen = false"
              class="lg-button-primary bg-orange-600 hover:bg-orange-700 text-white px-4 py-2 rounded-xl text-xs font-bold transition"
            >
              Đã hiểu
            </button>
          </div>
        </div>

      </div>
    </div>

  </div>
</template>

<style scoped>
</style>
