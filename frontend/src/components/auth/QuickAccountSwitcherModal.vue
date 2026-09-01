<script setup>
import { ref, reactive, onMounted, onBeforeUnmount, watch } from 'vue'
import {
  Search,
  X,
  GripHorizontal,
  KeyRound,
  Sparkles,
  ArrowRight,
  Check,
  ChevronLeft,
  ChevronRight,
  Loader2
} from 'lucide-vue-next'
import { authApi } from '@/services/apiClient'

// eslint-disable-next-line no-unused-vars
const props = defineProps({
  isOpen: { type: Boolean, default: false }
})

const emit = defineEmits(['close', 'select-account', 'quick-login'])

const isVisible = ref(false)
const searchKeyword = ref('')
const selectedRole = ref('all')
const selectedCampus = ref('all')
const copiedEmail = ref('')

// ── Pagination & API State ──
const accounts = ref([])
const totalItems = ref(0)
const page = ref(1)
const pageSize = ref(10)
const totalPages = ref(1)
const isLoading = ref(false)
const isFiltersLoading = ref(false)

const rolesList = ref([
  { id: 'all', label: 'Tất cả vai trò' }
])

const campusesList = ref([
  { id: 'all', label: 'Tất cả cơ sở' }
])

// ── Drag & Drop State ──
const modalRef = ref(null)
const isDragging = ref(false)
const pos = reactive({
  x: Math.max(20, Math.round((window.innerWidth - 560) / 2)),
  y: Math.max(30, Math.round((window.innerHeight - 660) / 2))
})
const dragStart = reactive({ x: 0, y: 0, initialX: 0, initialY: 0 })

function startDrag(e) {
  if (e.target.closest('button') || e.target.closest('input') || e.target.closest('select')) return
  isDragging.value = true
  dragStart.x = e.clientX
  dragStart.y = e.clientY
  dragStart.initialX = pos.x
  dragStart.initialY = pos.y

  window.addEventListener('mousemove', onDrag)
  window.addEventListener('mouseup', stopDrag)
}

function onDrag(e) {
  if (!isDragging.value) return
  const dx = e.clientX - dragStart.x
  const dy = e.clientY - dragStart.y

  const modalWidth = 560
  const modalHeight = 650
  const maxX = window.innerWidth - modalWidth - 10
  const maxY = window.innerHeight - modalHeight - 10

  pos.x = Math.max(10, Math.min(maxX, dragStart.initialX + dx))
  pos.y = Math.max(10, Math.min(maxY, dragStart.initialY + dy))
}

function stopDrag() {
  isDragging.value = false
  window.removeEventListener('mousemove', onDrag)
  window.removeEventListener('mouseup', stopDrag)
}

// ── Fetch Filters from Database ──
async function fetchFilters() {
  isFiltersLoading.value = true
  try {
    const res = await authApi.getDemoFilters()
    if (res) {
      if (res.roles && Array.isArray(res.roles)) {
        rolesList.value = [
          { id: 'all', label: 'Tất cả vai trò' },
          ...res.roles.map(r => ({ id: r.id || r.Id, label: r.label || r.Label }))
        ]
      }
      if (res.campuses && Array.isArray(res.campuses)) {
        campusesList.value = [
          { id: 'all', label: 'Tất cả cơ sở' },
          ...res.campuses.map(c => ({ id: c.id || c.Id, label: c.label || c.Label }))
        ]
      }
    }
  } catch (error) {
    console.error('Lỗi khi tải danh mục vai trò/cơ sở:', error)
  } finally {
    isFiltersLoading.value = false
  }
}

// ── Fetch Accounts from Database ──
let searchTimeout = null

async function fetchAccounts() {
  isLoading.value = true
  try {
    const res = await authApi.getDemoAccounts({
      search: searchKeyword.value.trim(),
      role: selectedRole.value,
      campus: selectedCampus.value,
      page: page.value,
      pageSize: pageSize.value
    })

    if (res) {
      accounts.value = res.items || res.Items || []
      totalItems.value = res.totalItems ?? res.TotalItems ?? 0
      totalPages.value = res.totalPages ?? res.TotalPages ?? Math.max(1, Math.ceil(totalItems.value / pageSize.value))
    }
  } catch (error) {
    console.error('Lỗi khi tải danh sách tài khoản demo:', error)
    accounts.value = []
    totalItems.value = 0
    totalPages.value = 1
  } finally {
    isLoading.value = false
  }
}

function handleSearchChange() {
  clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    page.value = 1
    fetchAccounts()
  }, 300)
}

watch(selectedRole, () => {
  page.value = 1
  fetchAccounts()
})

watch(selectedCampus, () => {
  page.value = 1
  fetchAccounts()
})

function goToPage(p) {
  if (p < 1 || p > totalPages.value || p === page.value) return
  page.value = p
  fetchAccounts()
}

function getRoleBadgeColor(role) {
  const r = (role || '').toLowerCase()
  if (r.includes('sinh_vien') || r === 'student') return 'bg-blue-500/10 text-blue-400 border-blue-500/20'
  if (r.includes('giang_vien') || r === 'teacher') return 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20'
  if (r.includes('giao_vu') || r === 'staff' || r === 'academic_staff') return 'bg-amber-500/10 text-amber-400 border-amber-500/20'
  if (r.includes('hieu_truong') || r.includes('chu_tich') || r === 'bgh') return 'bg-purple-500/10 text-purple-400 border-purple-500/20'
  if (r.includes('phu_huynh') || r === 'parent') return 'bg-teal-500/10 text-teal-400 border-teal-500/20'
  if (r.includes('sieu_quan_tri') || r === 'super_admin' || r === 'super-admin' || r === 'admin') return 'bg-rose-500/10 text-rose-400 border-rose-500/20'
  if (r.includes('hoi_dong') || r === 'content-council' || r === 'content_council') return 'bg-cyan-500/10 text-cyan-400 border-cyan-500/20'
  return 'bg-slate-500/10 text-slate-400 border-slate-500/20'
}

function handleSelect(account, autoSubmit = false) {
  copiedEmail.value = account.email
  setTimeout(() => { copiedEmail.value = '' }, 1500)
  if (autoSubmit) {
    emit('quick-login', account)
  } else {
    emit('select-account', account)
  }
}

function handleKeyDown(e) {
  if ((e.ctrlKey && e.key.toLowerCase() === 'k') ||
      (e.ctrlKey && e.shiftKey && e.key.toLowerCase() === 'd') ||
      e.key === 'F2') {
    e.preventDefault()
    isVisible.value = !isVisible.value
    if (isVisible.value) {
      if (rolesList.value.length <= 1) fetchFilters()
      fetchAccounts()
    }
  } else if (e.key === 'Escape' && isVisible.value) {
    isVisible.value = false
    emit('close')
  }
}

onMounted(() => {
  window.addEventListener('keydown', handleKeyDown)
  pos.x = Math.max(20, Math.round((window.innerWidth - 560) / 2))
  pos.y = Math.max(20, Math.round((window.innerHeight - 660) / 2))
  fetchFilters()
  fetchAccounts()
})

onBeforeUnmount(() => {
  window.removeEventListener('keydown', handleKeyDown)
  stopDrag()
  clearTimeout(searchTimeout)
})

function toggleModal() {
  isVisible.value = !isVisible.value
  if (isVisible.value) {
    if (rolesList.value.length <= 1) fetchFilters()
    fetchAccounts()
  } else {
    emit('close')
  }
}

defineExpose({
  open() { 
    isVisible.value = true 
    if (rolesList.value.length <= 1) fetchFilters()
    fetchAccounts()
  },
  close() { isVisible.value = false }
})
</script>

<template>
  <div>
    <!-- ── NÚT PHÍM TẮT GỢI Ý Ở GÓC MÀN HÌNH ── -->
    <button
      type="button"
      @click="toggleModal"
      class="fixed bottom-4 right-4 z-40 flex items-center gap-2 px-3.5 py-2 rounded-full bg-slate-900/90 backdrop-blur-md border border-cyan-500/40 text-cyan-300 text-[12px] font-medium shadow-xl hover:border-cyan-400 hover:shadow-cyan-500/20 hover:scale-105 transition-all duration-200"
      title="Nhấn phím tắt Ctrl+K hoặc F2 để mở danh sách tài khoản demo"
    >
      <Sparkles class="w-4 h-4 text-cyan-400 animate-pulse" />
      <span>Tài khoản Demo</span>
      <kbd class="px-1.5 py-0.5 rounded bg-slate-800 border border-slate-700 text-[10px] text-slate-300 font-mono">Ctrl+K</kbd>
    </button>

    <!-- ── MODAL DI CHUYỂN ĐƯỢC (DRAGGABLE POPUP) ── -->
    <Teleport to="body">
      <Transition
        enter-active-class="transition duration-200 ease-out"
        enter-from-class="opacity-0 scale-95"
        enter-to-class="opacity-100 scale-100"
        leave-active-class="transition duration-150 ease-in"
        leave-from-class="opacity-100 scale-100"
        leave-to-class="opacity-0 scale-95"
      >
        <div
          v-if="isVisible"
          ref="modalRef"
          :style="{ left: `${pos.x}px`, top: `${pos.y}px` }"
          class="fixed z-50 w-[560px] max-w-[95vw] rounded-2xl bg-slate-900/95 text-slate-100 backdrop-blur-xl border border-slate-700/80 shadow-2xl shadow-black/80 flex flex-col max-h-[88vh] overflow-hidden select-none"
        >
          <!-- ── HEADER (VÙNG KÉO THẢ BẰNG CHUỘT) ── -->
          <div
            class="flex items-center justify-between px-4 py-3 border-b border-slate-800 bg-slate-950/70 cursor-grab active:cursor-grabbing"
            @mousedown="startDrag"
          >
            <div class="flex items-center gap-2.5">
              <GripHorizontal class="w-4 h-4 text-slate-500" />
              <div class="flex items-center gap-2">
                <Sparkles class="w-4 h-4 text-cyan-400" />
                <span class="text-[14px] font-semibold text-white">Danh Sách Tài Khoản Hệ Thống</span>
              </div>
              <span class="px-2 py-0.5 rounded-full text-[10px] bg-cyan-500/10 border border-cyan-500/20 text-cyan-400 font-mono">
                {{ totalItems.toLocaleString() }} tài khoản
              </span>
            </div>

            <button
              type="button"
              @click="toggleModal"
              class="p-1 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition-colors"
            >
              <X class="w-4 h-4" />
            </button>
          </div>

          <!-- ── SEARCH & FILTERS (LẤY TỪ BẢNG DATABASE) ── -->
          <div class="p-3.5 border-b border-slate-800/80 bg-slate-900/60 space-y-2.5">
            <!-- Search bar -->
            <div class="relative">
              <Search class="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
              <input
                v-model="searchKeyword"
                type="text"
                @input="handleSearchChange"
                placeholder="Tìm kiếm theo họ tên, email, số điện thoại..."
                class="w-full pl-9 pr-8 py-1.5 rounded-xl bg-slate-950/80 border border-slate-700/80 text-[13px] text-white placeholder-slate-500 focus:outline-none focus:border-cyan-500 transition-colors"
              />
              <button
                v-if="searchKeyword"
                type="button"
                @click="searchKeyword = ''; fetchAccounts()"
                class="absolute right-2.5 top-1/2 -translate-y-1/2 text-slate-400 hover:text-white"
              >
                <X class="w-3.5 h-3.5" />
              </button>
            </div>

            <!-- Role & Campus Filters (Database Dropdowns) -->
            <div class="grid grid-cols-2 gap-2 text-[12px]">
              <div>
                <select
                  v-model="selectedRole"
                  class="w-full px-2.5 py-1.5 rounded-lg bg-slate-950/80 border border-slate-700/80 text-slate-300 focus:outline-none focus:border-cyan-500"
                >
                  <option v-for="r in rolesList" :key="r.id" :value="r.id">{{ r.label }}</option>
                </select>
              </div>
              <div>
                <select
                  v-model="selectedCampus"
                  class="w-full px-2.5 py-1.5 rounded-lg bg-slate-950/80 border border-slate-700/80 text-slate-300 focus:outline-none focus:border-cyan-500"
                >
                  <option v-for="c in campusesList" :key="c.id" :value="c.id">{{ c.label }}</option>
                </select>
              </div>
            </div>
          </div>

          <!-- ── ACCOUNTS LIST ── -->
          <div class="flex-1 overflow-y-auto p-3 space-y-2 max-h-[380px] scrollbar-thin scrollbar-thumb-slate-700 relative min-h-[160px]">
            <!-- Loading overlay -->
            <div v-if="isLoading" class="absolute inset-0 bg-slate-950/60 backdrop-blur-xs flex items-center justify-center z-10">
              <div class="flex items-center gap-2 text-cyan-400 text-xs font-medium">
                <Loader2 class="w-4 h-4 animate-spin" />
                <span>Đang tải danh sách tài khoản...</span>
              </div>
            </div>

            <!-- Empty state -->
            <div
              v-if="!isLoading && accounts.length === 0"
              class="py-12 text-center text-slate-400 text-[13px]"
            >
              Không tìm thấy tài khoản phù hợp với bộ lọc tìm kiếm.
            </div>

            <!-- List items -->
            <div
              v-for="acc in accounts"
              :key="acc.email"
              class="group flex items-center justify-between p-2.5 rounded-xl border border-slate-800 hover:border-cyan-500/40 bg-slate-950/40 hover:bg-slate-800/60 transition-all duration-150"
            >
              <!-- Info -->
              <div class="flex items-center gap-3 min-w-0 flex-1">
                <div class="w-8 h-8 rounded-lg bg-slate-800 border border-slate-700 flex items-center justify-center flex-shrink-0 text-cyan-400 font-bold text-xs uppercase">
                  {{ (acc.name || 'U').charAt(0) }}
                </div>
                <div class="min-w-0 flex-1">
                  <div class="flex items-center gap-2 flex-wrap">
                    <span class="text-[13px] font-medium text-white truncate">{{ acc.name }}</span>
                    <span
                      class="px-1.5 py-0.5 rounded text-[10px] font-medium border"
                      :class="getRoleBadgeColor(acc.role)"
                    >
                      {{ acc.roleName }}
                    </span>
                    <span class="px-1.5 py-0.5 rounded text-[10px] bg-slate-800 text-slate-400 font-mono">
                      {{ acc.campusName }}
                    </span>
                  </div>
                  <div class="flex items-center gap-2 text-[11px] text-slate-400 mt-0.5">
                    <span class="text-cyan-300/90 font-mono">{{ acc.email }}</span>
                    <span v-if="acc.note">•</span>
                    <span v-if="acc.note" class="text-slate-400 truncate">{{ acc.note }}</span>
                  </div>
                </div>
              </div>

              <!-- Actions -->
              <div class="flex items-center gap-1.5 flex-shrink-0 ml-2">
                <button
                  type="button"
                  @click="handleSelect(acc, false)"
                  class="px-2.5 py-1.5 rounded-lg bg-slate-800 hover:bg-slate-700 text-slate-200 text-[11px] font-medium border border-slate-700 hover:border-slate-600 transition-colors"
                  title="Điền email & mật khẩu vào khung đăng nhập"
                >
                  <span v-if="copiedEmail === acc.email" class="text-emerald-400 flex items-center gap-1">
                    <Check class="w-3 h-3" /> Đã điền
                  </span>
                  <span v-else>Điền</span>
                </button>

                <button
                  type="button"
                  @click="handleSelect(acc, true)"
                  class="px-2.5 py-1.5 rounded-lg bg-cyan-600 hover:bg-cyan-500 text-white text-[11px] font-medium shadow shadow-cyan-600/30 flex items-center gap-1 transition-all"
                  title="Đăng nhập ngay lập tức vào hệ thống"
                >
                  <span>Vào</span>
                  <ArrowRight class="w-3 h-3" />
                </button>
              </div>
            </div>
          </div>

          <!-- ── PHÂN TRANG (PAGINATION CONTROLS) ── -->
          <div class="flex items-center justify-between px-4 py-2 border-t border-slate-800/80 bg-slate-950/80 text-[12px]">
            <div class="text-slate-400">
              Trang <span class="text-white font-medium">{{ page }}</span> / <span class="text-slate-300 font-medium">{{ totalPages }}</span>
            </div>

            <div class="flex items-center gap-1">
              <button
                type="button"
                :disabled="page <= 1 || isLoading"
                @click="goToPage(page - 1)"
                class="p-1.5 rounded-lg bg-slate-900 border border-slate-800 text-slate-300 hover:text-white hover:bg-slate-800 disabled:opacity-30 disabled:cursor-not-allowed transition-all"
                title="Trang trước"
              >
                <ChevronLeft class="w-3.5 h-3.5" />
              </button>

              <button
                type="button"
                :disabled="page >= totalPages || isLoading"
                @click="goToPage(page + 1)"
                class="p-1.5 rounded-lg bg-slate-900 border border-slate-800 text-slate-300 hover:text-white hover:bg-slate-800 disabled:opacity-30 disabled:cursor-not-allowed transition-all"
                title="Trang sau"
              >
                <ChevronRight class="w-3.5 h-3.5" />
              </button>
            </div>
          </div>

          <!-- ── FOOTER ── -->
          <div class="flex items-center justify-between px-4 py-2.5 border-t border-slate-800 bg-slate-950/90 text-[11px] text-slate-400">
            <div class="flex items-center gap-2">
              <KeyRound class="w-3.5 h-3.5 text-amber-400" />
              <span>Mật khẩu tất cả tài khoản: <strong class="text-amber-300 font-mono">123456</strong></span>
            </div>
            <div class="flex items-center gap-1 text-[10px] text-slate-500">
              <span>Kéo thả header để di chuyển</span>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>
