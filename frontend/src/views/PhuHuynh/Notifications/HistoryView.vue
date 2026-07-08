<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  MessageSquare,
  ChevronDown,
  ChevronLeft,
  Search,
  CheckCircle,
  Eye,
  Calendar,
  User,
  School,
  X
} from 'lucide-vue-next'
import { parentApi } from '@/services/parentApi'
import { getStoredActiveChildId, setActiveChildId } from '@/components/PhuHuynh/data/parentState.js'
import { usePopupStore } from '@/stores/popup'

const route = useRoute()
const router = useRouter()
const popupStore = usePopupStore()

const activeChildId = ref(Number(route.query.studentId) || getStoredActiveChildId())
const dropdownOpen = ref(false)
const children = ref([])
const notifications = ref([])

// Tìm kiếm & Lọc
const searchQuery = ref('')
const selectedTab = ref('Tất cả') // 'Tất cả', 'Nhà trường', 'Giảng viên'

const currentChild = computed(() => {
  return children.value.find(c => c.id === activeChildId.value) || children.value[0] || {
    id: null,
    name: 'Chưa có học sinh liên kết',
    className: '',
  }
})

function selectChild(id) {
  activeChildId.value = id
  setActiveChildId(id)
  dropdownOpen.value = false
  router.replace({ query: { studentId: id } })
}

const allNotifications = computed(() => {
  return notifications.value.map(item => ({
    id: item.id,
    title: item.title || 'Thông báo',
    content: item.content || '',
    sender: 'Hệ thống LMS',
    date: item.readAt || item.createdAt || '',
    read: true,
    type: 'school',
  }))
})

// Lọc thông báo theo tab và ô tìm kiếm
const filteredNotifications = computed(() => {
  let list = allNotifications.value

  if (selectedTab.value === 'Nhà trường') {
    list = list.filter(n => n.type === 'school')
  } else if (selectedTab.value === 'Giảng viên') {
    list = list.filter(n => n.type === 'teacher')
  }

  if (searchQuery.value.trim()) {
    const query = searchQuery.value.toLowerCase()
    list = list.filter(
      n =>
        n.title.toLowerCase().includes(query) ||
        n.content.toLowerCase().includes(query) ||
        n.sender.toLowerCase().includes(query)
    )
  }

  return list
})


// Đánh dấu đã đọc
function markAsRead(notifId) {
  const item = notifications.value.find(n => n.id === notifId)
  if (item) item.read = true

  popupStore.success('Đã đọc', 'Đã đánh dấu thông báo này là đã đọc.')
}

// Đánh dấu đã đọc tất cả
function markAllAsRead() {
  let count = 0
  filteredNotifications.value.forEach(notif => {
    if (!notif.read) {
      notif.read = true
      count++
    }
  })

  if (count > 0) {
    popupStore.success('Thành công', 'Đã đánh dấu tất cả thông báo hiển thị là đã đọc.')
  } else {
    popupStore.info('Thông báo', 'Tất cả thông báo trong bộ lọc này đã được đọc từ trước.')
  }
}

// Chi tiết thông báo modal
const selectedNotif = ref(null)
const isDetailModalOpen = ref(false)

function openDetail(notif) {
  notif.read = true
  selectedNotif.value = notif
  isDetailModalOpen.value = true
}

function goBack() {
  router.push('/parent/dashboard')
}

async function loadData() {
  const [childrenRes, notificationsRes] = await Promise.all([
    parentApi.getChildren(),
    parentApi.getNotificationHistory(),
  ])
  children.value = childrenRes?.data || []
  notifications.value = notificationsRes?.data || []
  const firstChild = children.value.find(child => child.id === activeChildId.value) || children.value[0]
  if (firstChild) {
    activeChildId.value = firstChild.id
    setActiveChildId(firstChild.id)
  }
}

onMounted(loadData)
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
            <MessageSquare :size="20" class="text-orange-600" />
            Lịch sử thông báo & tin nhắn
          </h2>
          <p class="text-xs text-body">Theo dõi các tin tức chung từ nhà trường và trao đổi riêng từ giảng viên</p>
        </div>
      </div>

      <!-- Chọn học sinh nhanh -->
      <div class="relative min-w-[220px]">
        <button
          type="button"
          class="surface-input border-card flex w-full items-center justify-between gap-2.5 rounded-xl border px-3.5 py-1.8 text-xs font-semibold text-heading shadow-sm transition-all focus:outline-none"
          @click="dropdownOpen = !dropdownOpen"
        >
          <div class="flex items-center gap-2">
            <div class="h-5 w-5 flex items-center justify-center rounded-full bg-orange-600 text-[9px] font-bold text-white">
              {{ currentChild.name.split(' ').filter(Boolean).pop()?.charAt(0) || '-' }}
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
              v-for="child in children"
              :key="child.id"
              type="button"
              class="flex w-full items-center justify-between rounded-lg px-2.5 py-1.5 text-left text-xs font-medium text-label transition hover:bg-(--surface-card-hover)"
              @click="selectChild(child.id)"
            >
              <span>{{ child.name }} ({{ child.className || 'Chưa có lớp' }})</span>
            </button>
          </div>
        </Transition>
      </div>
    </div>

    <!-- ── THANH LỌC & TÌM KIẾM ── -->
    <div class="lg-card-glass p-4 flex flex-col md:flex-row md:items-center justify-between gap-4">
      
      <!-- Tabs filter -->
      <div class="flex items-center gap-1">
        <button
          v-for="tab in ['Tất cả', 'Nhà trường', 'Giảng viên']"
          :key="tab"
          @click="selectedTab = tab"
          class="px-3.5 py-1.5 text-xs rounded-xl font-bold border transition"
          :class="selectedTab === tab ? 'bg-orange-600 border-orange-600 text-white shadow-sm' : 'border-card text-label hover:text-orange-600'"
        >
          {{ tab }}
        </button>
      </div>

      <!-- Ô tìm kiếm và Đọc tất cả -->
      <div class="flex items-center gap-3 w-full md:w-auto">
        <div class="relative flex-1 md:w-64">
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Tìm kiếm tiêu đề, người gửi..."
            class="surface-input border-card w-full pl-8 pr-3 py-1.8 text-xs rounded-xl border focus:outline-none focus:ring-2 focus:ring-orange-500/20"
          />
          <Search :size="13" class="absolute left-3 top-2.5 text-muted" />
        </div>

        <button
          @click="markAllAsRead"
          class="lg-icon-button flex items-center gap-1.5 px-3 py-1.8 border border-card rounded-xl text-xs font-bold text-label hover:text-orange-600 transition"
          title="Đọc tất cả hiển thị"
        >
          <CheckCircle :size="13" />
          <span>Đọc tất cả</span>
        </button>
      </div>

    </div>

    <!-- ── DANH SÁCH THÔNG BÁO ── -->
    <div class="space-y-3">
      <div v-if="filteredNotifications.length === 0" class="lg-card-glass p-12 text-center text-muted text-xs">
        Không tìm thấy thông báo nào phù hợp với điều kiện tìm kiếm/lọc.
      </div>
      
      <div
        v-for="notif in filteredNotifications"
        :key="notif.id"
        class="lg-card-glass p-5 hover:scale-[1.005] transition flex gap-4 items-start"
        :class="[!notif.read ? 'border-orange-200 dark:border-orange-950/20 bg-orange-50/5' : 'opacity-85']"
      >
        <!-- Icon theo nhóm gửi -->
        <div class="flex-shrink-0 mt-0.5">
          <span
            v-if="notif.type === 'school'"
            class="h-8 w-8 rounded-xl bg-orange-50 dark:bg-orange-950/20 text-orange-600 flex items-center justify-center"
          >
            <School :size="15" />
          </span>
          <span
            v-else
            class="h-8 w-8 rounded-xl bg-blue-50 dark:bg-blue-950/20 text-blue-600 flex items-center justify-center"
          >
            <User :size="15" />
          </span>
        </div>

        <!-- Nội dung tin -->
        <div class="flex-1 min-w-0">
          <div class="flex items-start justify-between gap-3">
            <div>
              <div class="flex items-center gap-2 flex-wrap">
                <span
                  class="px-2 py-0.2 rounded text-[8px] font-extrabold uppercase tracking-wide"
                  :class="notif.type === 'school' ? 'bg-orange-100 text-orange-700 dark:bg-orange-950/30' : 'bg-blue-100 text-blue-700 dark:bg-blue-950/30'"
                >
                  {{ notif.type === 'school' ? 'BGH Nhà Trường' : 'Giảng viên' }}
                </span>
                
                <span v-if="!notif.read" class="px-1.5 py-0.2 bg-orange-600 text-white rounded text-[8px] font-extrabold uppercase">Mới</span>
              </div>
              
              <h4
                class="text-xs font-bold text-heading mt-1.5 leading-snug"
                :class="!notif.read ? 'font-extrabold text-heading' : 'font-semibold text-body'"
              >
                {{ notif.title }}
              </h4>

              <p class="text-[11px] text-body mt-1 leading-relaxed line-clamp-2">
                {{ notif.content }}
              </p>
            </div>

            <!-- Nút thao tác -->
            <div class="flex items-center gap-1.5 flex-shrink-0">
              <button
                @click="openDetail(notif)"
                class="p-1.5 border border-card rounded-lg hover:text-orange-600 transition"
                title="Đọc chi tiết"
              >
                <Eye :size="13" />
              </button>
              <button
                v-if="!notif.read"
                @click="markAsRead(notif.id)"
                class="p-1.5 border border-card rounded-lg text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-950/20 transition"
                title="Đánh dấu đã đọc"
              >
                <CheckCircle :size="13" />
              </button>
            </div>
          </div>

          <!-- Date footer -->
          <div class="flex items-center gap-1.5 mt-3 text-[10px] text-muted font-semibold">
            <Calendar :size="11" />
            <span>Ngày gửi: {{ notif.date }}</span>
            <span>•</span>
            <span>Người gửi: {{ notif.sender }}</span>
          </div>
        </div>
      </div>
    </div>

    <!-- ── MODAL CHI TIẾT THÔNG BÁO ── -->
    <div v-if="isDetailModalOpen && selectedNotif" class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <!-- Overlay -->
      <div @click="isDetailModalOpen = false" class="absolute inset-0 bg-slate-900/40 dark:bg-slate-950/60 backdrop-blur-sm" />

      <!-- Modal Content -->
      <div class="lg-modal w-full max-w-lg relative z-10 flex flex-col rounded-2xl shadow-xl overflow-hidden">
        
        <!-- Header -->
        <div class="flex items-center justify-between pb-3 border-b border-card">
          <h2 class="text-sm font-bold text-heading flex items-center gap-1.5">
            <MessageSquare :size="16" class="text-orange-600" />
            Nội dung thông báo chi tiết
          </h2>
          <button @click="isDetailModalOpen = false" class="text-muted hover:text-orange-600">
            <X :size="16" />
          </button>
        </div>

        <!-- Body -->
        <div class="p-5 space-y-4">
          <div class="space-y-1">
            <div class="flex items-center gap-2">
              <span
                class="px-2 py-0.2 rounded text-[8px] font-extrabold uppercase"
                :class="selectedNotif.type === 'school' ? 'bg-orange-100 text-orange-700' : 'bg-blue-100 text-blue-700'"
              >
                {{ selectedNotif.type === 'school' ? 'Thông báo nhà trường' : 'Lời nhắn giảng viên' }}
              </span>
            </div>
            
            <h3 class="text-xs font-bold text-heading leading-snug mt-1.5">{{ selectedNotif.title }}</h3>
            
            <div class="flex items-center gap-1 text-[10px] text-muted font-semibold mt-1">
              <Calendar :size="11" />
              <span>Ngày gửi: {{ selectedNotif.date }}</span>
              <span>•</span>
              <span>Người gửi: {{ selectedNotif.sender }}</span>
            </div>
          </div>

          <div class="p-4 bg-slate-50 dark:bg-slate-900/40 rounded-xl border border-card text-[11px] text-body leading-relaxed whitespace-pre-wrap">
            {{ selectedNotif.content }}
          </div>

          <div class="pt-3 border-t border-card flex justify-end">
            <button
              @click="isDetailModalOpen = false"
              class="lg-button-primary bg-orange-600 hover:bg-orange-700 text-white px-4 py-2 rounded-xl text-xs font-bold transition"
            >
              Đóng lại
            </button>
          </div>
        </div>

      </div>
    </div>

  </div>
</template>

<style scoped>
</style>
