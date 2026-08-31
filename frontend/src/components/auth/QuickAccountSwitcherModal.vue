<script setup>
import { computed, ref, reactive, onMounted, onBeforeUnmount } from 'vue'
import {
  Search,
  X,
  GripHorizontal,
  KeyRound,
  Sparkles,
  ArrowRight,
  Check
} from 'lucide-vue-next'

const props = defineProps({
  isOpen: { type: Boolean, default: false }
})

const emit = defineEmits(['close', 'select-account', 'quick-login'])

const isVisible = ref(false)
const searchKeyword = ref('')
const selectedRole = ref('all')
const selectedCampus = ref('all')
const copiedEmail = ref('')

// ── Drag & Drop State ──
const modalRef = ref(null)
const isDragging = ref(false)
const pos = reactive({
  x: Math.max(20, Math.round((window.innerWidth - 560) / 2)),
  y: Math.max(40, Math.round((window.innerHeight - 620) / 2))
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
  const modalHeight = 600
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

// ── Master Demo Accounts Dataset ──
const demoAccounts = [
  // Cơ sở 1
  { email: 'sv1.cs1@aet.local', password: '123456', name: 'Sinh viên 1 (SE)', role: 'student', roleName: 'Sinh viên', campus: 'cs1', campusName: 'Cơ sở 1', note: 'Lớp SE - Kỹ thuật phần mềm' },
  { email: 'sv2.cs1@aet.local', password: '123456', name: 'Sinh viên 2 (GD)', role: 'student', roleName: 'Sinh viên', campus: 'cs1', campusName: 'Cơ sở 1', note: 'Lớp GD - Thiết kế đồ họa' },
  { email: 'sv3.cs1@aet.local', password: '123456', name: 'Sinh viên 3 (DM)', role: 'student', roleName: 'Sinh viên', campus: 'cs1', campusName: 'Cơ sở 1', note: 'Lớp DM - Digital Marketing' },
  { email: 'gv1.cs1@aet.local', password: '123456', name: 'Giảng viên 1 (GD)', role: 'teacher', roleName: 'Giảng viên', campus: 'cs1', campusName: 'Cơ sở 1', note: 'Dạy môn UIX101 - Thiết kế UI/UX' },
  { email: 'gv2.cs1@aet.local', password: '123456', name: 'Giảng viên 2 (DM)', role: 'teacher', roleName: 'Giảng viên', campus: 'cs1', campusName: 'Cơ sở 1', note: 'Dạy môn MKT101 - Marketing căn bản' },
  { email: 'gv3.cs1@aet.local', password: '123456', name: 'Giảng viên 3 (SE)', role: 'teacher', roleName: 'Giảng viên', campus: 'cs1', campusName: 'Cơ sở 1', note: 'Dạy môn COM101 & DBI202' },
  { email: 'giaovu1.cs1@aet.local', password: '123456', name: 'Giáo vụ 1 (CS1)', role: 'staff', roleName: 'Giáo vụ', campus: 'cs1', campusName: 'Cơ sở 1', note: 'Quản lý lịch học & điểm số' },
  { email: 'hieupho.cs1@aet.local', password: '123456', name: 'Hiệu phó Đào tạo (CS1)', role: 'bgh', roleName: 'Ban giám hiệu', campus: 'cs1', campusName: 'Cơ sở 1', note: 'Duyệt TKB, mở điểm, xem báo cáo' },
  { email: 'ph1.cs1@aet.local', password: '123456', name: 'Phụ huynh 1 (CS1)', role: 'parent', roleName: 'Phụ huynh', campus: 'cs1', campusName: 'Cơ sở 1', note: 'Phụ huynh của Sinh viên 1' },

  // Cơ sở 2
  { email: 'sv1.cs2@aet.local', password: '123456', name: 'Sinh viên 1 (CS2)', role: 'student', roleName: 'Sinh viên', campus: 'cs2', campusName: 'Cơ sở 2', note: 'Lớp SE - Cơ sở TP.HCM' },
  { email: 'gv1.cs2@aet.local', password: '123456', name: 'Giảng viên 1 (CS2)', role: 'teacher', roleName: 'Giảng viên', campus: 'cs2', campusName: 'Cơ sở 2', note: 'Giảng viên cơ sở TP.HCM' },
  { email: 'giaovu1.cs2@aet.local', password: '123456', name: 'Giáo vụ 1 (CS2)', role: 'staff', roleName: 'Giáo vụ', campus: 'cs2', campusName: 'Cơ sở 2', note: 'Giáo vụ cơ sở TP.HCM' },
  { email: 'hieupho.cs2@aet.local', password: '123456', name: 'Hiệu phó (CS2)', role: 'bgh', roleName: 'Ban giám hiệu', campus: 'cs2', campusName: 'Cơ sở 2', note: 'Ban giám hiệu cơ sở 2' },
  { email: 'ph1.cs2@aet.local', password: '123456', name: 'Phụ huynh 1 (CS2)', role: 'parent', roleName: 'Phụ huynh', campus: 'cs2', campusName: 'Cơ sở 2', note: 'Phụ huynh cơ sở 2' },

  // Cơ sở 3
  { email: 'sv1.cs3@aet.local', password: '123456', name: 'Sinh viên 1 (CS3)', role: 'student', roleName: 'Sinh viên', campus: 'cs3', campusName: 'Cơ sở 3', note: 'Lớp SE - Cơ sở Đà Nẵng' },
  { email: 'gv1.cs3@aet.local', password: '123456', name: 'Giảng viên 1 (CS3)', role: 'teacher', roleName: 'Giảng viên', campus: 'cs3', campusName: 'Cơ sở 3', note: 'Giảng viên cơ sở Đà Nẵng' },
  { email: 'giaovu1.cs3@aet.local', password: '123456', name: 'Giáo vụ 1 (CS3)', role: 'staff', roleName: 'Giáo vụ', campus: 'cs3', campusName: 'Cơ sở 3', note: 'Giáo vụ cơ sở Đà Nẵng' },

  // Cấp cao toàn trường
  { email: 'hdqlnd@aet.local', password: '123456', name: 'Hội đồng Quản lý Nội dung', role: 'content-council', roleName: 'HĐQL Nội dung', campus: 'root', campusName: 'Toàn hệ thống', note: 'Duyệt khung CTĐT & đề cương môn học' },
  { email: 'superadmin@aet.local', password: '123456', name: 'Siêu Quản Trị Hệ Thống', role: 'super-admin', roleName: 'Siêu quản trị', campus: 'root', campusName: 'Toàn hệ thống', note: 'Toàn quyền cấu hình & phân quyền RBAC' }
]

const rolesList = [
  { id: 'all', label: 'Tất cả vai trò' },
  { id: 'student', label: 'Sinh viên' },
  { id: 'teacher', label: 'Giảng viên' },
  { id: 'staff', label: 'Giáo vụ' },
  { id: 'bgh', label: 'Ban giám hiệu' },
  { id: 'parent', label: 'Phụ huynh' },
  { id: 'content-council', label: 'HĐ Nội dung' },
  { id: 'super-admin', label: 'Quản trị' }
]

const campusesList = [
  { id: 'all', label: 'Tất cả cơ sở' },
  { id: 'cs1', label: 'Cơ sở 1 (Hà Nội)' },
  { id: 'cs2', label: 'Cơ sở 2 (TP.HCM)' },
  { id: 'cs3', label: 'Cơ sở 3 (Đà Nẵng)' },
  { id: 'root', label: 'Toàn trường' }
]

const filteredAccounts = computed(() => {
  let list = demoAccounts
  if (selectedRole.value !== 'all') {
    list = list.filter(a => a.role === selectedRole.value)
  }
  if (selectedCampus.value !== 'all') {
    list = list.filter(a => a.campus === selectedCampus.value)
  }
  if (searchKeyword.value.trim()) {
    const q = searchKeyword.value.trim().toLowerCase()
    list = list.filter(a =>
      a.email.toLowerCase().includes(q) ||
      a.name.toLowerCase().includes(q) ||
      a.roleName.toLowerCase().includes(q) ||
      a.note.toLowerCase().includes(q) ||
      a.campusName.toLowerCase().includes(q)
    )
  }
  return list
})

function getRoleBadgeColor(role) {
  switch (role) {
    case 'student': return 'bg-blue-500/10 text-blue-400 border-blue-500/20'
    case 'teacher': return 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20'
    case 'staff': return 'bg-amber-500/10 text-amber-400 border-amber-500/20'
    case 'bgh': return 'bg-purple-500/10 text-purple-400 border-purple-500/20'
    case 'parent': return 'bg-teal-500/10 text-teal-400 border-teal-500/20'
    case 'super-admin': return 'bg-rose-500/10 text-rose-400 border-rose-500/20'
    case 'content-council': return 'bg-cyan-500/10 text-cyan-400 border-cyan-500/20'
    default: return 'bg-slate-500/10 text-slate-400 border-slate-500/20'
  }
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
  // Phím tắt mở popup: Ctrl + K, hoặc Ctrl + Shift + D, hoặc F2
  if ((e.ctrlKey && e.key.toLowerCase() === 'k') ||
      (e.ctrlKey && e.shiftKey && e.key.toLowerCase() === 'd') ||
      e.key === 'F2') {
    e.preventDefault()
    isVisible.value = !isVisible.value
  } else if (e.key === 'Escape' && isVisible.value) {
    isVisible.value = false
    emit('close')
  }
}

onMounted(() => {
  window.addEventListener('keydown', handleKeyDown)
  pos.x = Math.max(20, Math.round((window.innerWidth - 560) / 2))
  pos.y = Math.max(30, Math.round((window.innerHeight - 620) / 2))
})

onBeforeUnmount(() => {
  window.removeEventListener('keydown', handleKeyDown)
  stopDrag()
})

function toggleModal() {
  isVisible.value = !isVisible.value
  if (!isVisible.value) emit('close')
}

defineExpose({
  open() { isVisible.value = true },
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
          class="fixed z-50 w-[540px] max-w-[95vw] rounded-2xl bg-slate-900/95 text-slate-100 backdrop-blur-xl border border-slate-700/80 shadow-2xl shadow-black/70 flex flex-col max-h-[85vh] overflow-hidden select-none"
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
                <span class="text-[14px] font-semibold text-white">Danh Sách Tài Khoản Demo (AET)</span>
              </div>
              <span class="px-2 py-0.5 rounded-full text-[10px] bg-cyan-500/10 border border-cyan-500/20 text-cyan-400 font-mono">
                {{ filteredAccounts.length }} tài khoản
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

          <!-- ── SEARCH & FILTERS ── -->
          <div class="p-3.5 border-b border-slate-800/80 bg-slate-900/60 space-y-2.5">
            <!-- Search bar -->
            <div class="relative">
              <Search class="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
              <input
                v-model="searchKeyword"
                type="text"
                placeholder="Tìm theo tên, email, chuyên ngành, môn học..."
                class="w-full pl-9 pr-3 py-1.5 rounded-xl bg-slate-950/80 border border-slate-700/80 text-[13px] text-white placeholder-slate-500 focus:outline-none focus:border-cyan-500 transition-colors"
              />
              <button
                v-if="searchKeyword"
                type="button"
                @click="searchKeyword = ''"
                class="absolute right-2.5 top-1/2 -translate-y-1/2 text-slate-400 hover:text-white"
              >
                <X class="w-3.5 h-3.5" />
              </button>
            </div>

            <!-- Role & Campus Filters -->
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
          <div class="flex-1 overflow-y-auto p-3 space-y-2 max-h-[380px] scrollbar-thin scrollbar-thumb-slate-700">
            <div
              v-if="filteredAccounts.length === 0"
              class="py-10 text-center text-slate-400 text-[13px]"
            >
              Không tìm thấy tài khoản phù hợp với bộ lọc.
            </div>

            <div
              v-for="acc in filteredAccounts"
              :key="acc.email"
              class="group flex items-center justify-between p-2.5 rounded-xl border border-slate-800 hover:border-cyan-500/40 bg-slate-950/40 hover:bg-slate-800/60 transition-all duration-150"
            >
              <!-- Info -->
              <div class="flex items-center gap-3 min-w-0 flex-1">
                <div class="w-8 h-8 rounded-lg bg-slate-800 border border-slate-700 flex items-center justify-center flex-shrink-0 text-cyan-400 font-bold text-xs">
                  {{ acc.name.charAt(0) }}
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
                    <span>•</span>
                    <span class="text-slate-400 truncate">{{ acc.note }}</span>
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

          <!-- ── FOOTER ── -->
          <div class="flex items-center justify-between px-4 py-2.5 border-t border-slate-800 bg-slate-950/60 text-[11px] text-slate-400">
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

