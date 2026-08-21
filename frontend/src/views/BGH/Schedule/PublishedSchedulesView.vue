<script setup>
import { ref, computed, onMounted } from 'vue'
import { Eye, Search, Building2, CalendarDays, Clock, Users, AlertCircle, Loader2, X, Printer, CheckCircle2, User, MapPin, Layers, GraduationCap } from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import { bghApi } from '@/services/bghApi'
import { unwrapApiData } from '@/services/apiClient'

const loading = ref(false)
const error = ref(null)
const searchQuery = ref('')
const publishedData = ref([])
const showScheduleModal = ref(false)
const selectedGroup = ref(null)

const daysOfWeek = ['Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7', 'Chủ Nhật']
const shifts = computed(() => {
  const unique = new Map()
  publishedData.value.forEach(item => {
    if (!item.shiftId) return
    unique.set(Number(item.shiftId), {
      id: Number(item.shiftId),
      name: item.shift,
      time: [item.shiftStart?.slice(0, 5), item.shiftEnd?.slice(0, 5)].filter(Boolean).join(' - '),
    })
  })
  return [...unique.values()].sort((a, b) => a.id - b.id)
})

async function loadData() {
  loading.value = true
  error.value = null
  try {
    const res = await bghApi.getPendingSchedules({ status: 'published' })
    const rawData = unwrapApiData(res) || []
    publishedData.value = rawData
  } catch (e) {
    error.value = e?.message || 'Lỗi tải dữ liệu TKB đã xuất bản'
  } finally {
    loading.value = false
  }
}

// Logic Gộp TKB: Nhóm các bản ghi trùng Lịch (Ca, Thứ, Học kỳ, Cơ sở, Loại TKB)
const groupedSchedules = computed(() => {
  const groupsMap = new Map()

  publishedData.value.forEach(item => {
    const term = item.term || item.semester || 'Chưa xác định học kỳ'
    const campus = item.campus || 'Chưa xác định cơ sở'
    const type = item.type || 'Chưa xác định loại lịch'
    const shiftKey = item.shift || 'Chưa xác định ca'
    const dayKey = item.thuTrongTuan

    // Key để xét Lịch học giống nhau
    const groupKey = `${term}_${campus}_${type}_${shiftKey}_Thứ ${dayKey}`

    if (!groupsMap.has(groupKey)) {
      groupsMap.set(groupKey, {
        groupKey,
        term,
        campus,
        type,
        shiftName: shiftKey,
        dayOfWeek: `Thứ ${dayKey}`,
        dayIdx: Number(dayKey) - 2 >= 0 ? Number(dayKey) - 2 : 0,
        shiftId: Number(item.shiftId || 0),
        depts: new Set(),
        subjects: new Set(),
        classesList: new Set(),
        roomsList: new Set(),
        teachersList: new Set(),
        totalClassesCount: 0,
        totalHoursCount: 0,
        items: []
      })
    }

    const group = groupsMap.get(groupKey)
    if (item.dept || item.department) group.depts.add(item.dept || item.department)
    if (item.subject) group.subjects.add(item.subject)
    if (item.classCode) group.classesList.add(item.classCode)
    if (item.room) group.roomsList.add(item.room)
    if (item.teacher || item.submitter) group.teachersList.add(item.teacher || item.submitter)
    group.totalClassesCount += Number(item.classes ?? 0)
    group.totalHoursCount += Number(item.hours ?? 0)
    group.items.push(item)
  })

  // Format array của các nhóm
  return Array.from(groupsMap.values()).map(g => ({
    ...g,
    deptText: Array.from(g.depts).join(', ') || 'Đa ngành học',
    deptsArray: Array.from(g.depts),
    subjectsArray: Array.from(g.subjects),
    classesText: Array.from(g.classesList).join(', ') || `${g.totalClassesCount} Lớp`,
    classesArray: Array.from(g.classesList),
    roomsText: Array.from(g.roomsList).join(', ') || 'Chưa xếp phòng',
    roomsArray: Array.from(g.roomsList),
    teachersText: Array.from(g.teachersList).join(', ') || 'Đang cập nhật GV',
    teachersArray: Array.from(g.teachersList),
    displayHours: formatHours(g.totalHoursCount / Math.max(g.items.length, 1))
  }))
})

const filteredData = computed(() => {
  if (!searchQuery.value) return groupedSchedules.value
  const q = searchQuery.value.toLowerCase()
  return groupedSchedules.value.filter(g =>
    g.deptText.toLowerCase().includes(q) ||
    g.classesText.toLowerCase().includes(q) ||
    g.term.toLowerCase().includes(q) ||
    g.campus.toLowerCase().includes(q) ||
    g.roomsText.toLowerCase().includes(q)
  )
})

function formatHours(h) {
  const num = Number(h)
  if (isNaN(num)) return '0'
  return num % 1 === 0 ? num.toString() : num.toFixed(1)
}

function viewScheduleGroup(group) {
  selectedGroup.value = group
  showScheduleModal.value = true
}

function getGridItemForGroup(dayIdx, shiftId) {
  if (!selectedGroup.value) return null

  // Đơn giản hóa ma trận hiển thị môn học cho từng ca/thứ của nhóm đã chọn
  if (dayIdx === selectedGroup.value.dayIdx && shiftId === selectedGroup.value.shiftId) {
    return {
      subject: selectedGroup.value.subjectsArray.join(', ') || 'Chưa có tên môn học',
      code: selectedGroup.value.classesArray.join(', ') || 'Chưa có lớp học phần',
      teacher: selectedGroup.value.teachersArray.join(', ') || 'Chưa phân công',
      room: selectedGroup.value.roomsArray.join(', ') || 'Chưa xếp phòng',
      color: 'bg-emerald-500/10 border-emerald-500/30 text-emerald-700'
    }
  }

  // Lịch môn phụ tham khảo nếu có trong items
  const foundItem = selectedGroup.value.items.find(it => {
    const d = (it.thuTrongTuan || 2) - 2
    const s = Number(it.shiftId || 0)
    return d === dayIdx && s === shiftId
  })

  if (foundItem) {
    return {
      subject: foundItem.subject || foundItem.dept,
      code: foundItem.classCode || 'LOP-HP',
      teacher: foundItem.teacher || 'Giáo viên',
      room: foundItem.room || 'Phòng',
      color: 'bg-blue-500/10 border-blue-500/30 text-blue-700'
    }
  }

  return null
}

function printSchedule() {
  window.print()
}

onMounted(() => { loadData() })
</script>

<template>
  <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
    <!-- Loading State -->
    <div v-if="loading" class="col-span-full flex items-center justify-center py-20">
      <div class="flex flex-col items-center gap-3 text-muted">
        <Loader2 :size="32" class="animate-spin text-(--lg-primary)" />
        <p class="text-sm font-medium">Đang tổng hợp dữ liệu TKB theo nhóm lịch học...</p>
      </div>
    </div>
    <!-- Error State -->
    <div v-else-if="error" class="col-span-full flex items-center justify-center py-20">
      <div class="flex flex-col items-center gap-3">
        <AlertCircle :size="32" class="text-(--color-danger-text)" />
        <p class="text-sm text-(--color-danger-text) font-medium">{{ error }}</p>
        <button @click="loadData()" class="px-4 py-2 bg-(--lg-primary) text-white text-xs font-bold rounded-lg hover:bg-(--lg-primary-dark) transition-colors">Thử lại</button>
      </div>
    </div>
    <template v-else>
      <div class="col-span-full flex items-center justify-between mb-2">
        <div class="relative">
          <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-placeholder" />
          <input v-model="searchQuery" type="text" placeholder="Tìm ngành, lớp, phòng, cơ sở..." class="w-72 surface-input border border-input rounded-xl pl-9 pr-4 py-2 text-sm font-medium outline-none focus:ring-4 focus:ring-(--border-focus-ring)">
        </div>
        <div class="flex items-center gap-2">
          <GlassBadge variant="info" size="sm">
            <Layers :size="12" class="mr-1" /> Đã gộp {{ filteredData.length }} khung lịch học trùng nhau
          </GlassBadge>
        </div>
      </div>

      <!-- CARD THỜI KHÓA BIỂU ĐÃ GỘP THEO LỊCH HỌC TRÙNG NHAU -->
      <div v-for="group in filteredData" :key="group.groupKey" class="surface-card border border-card rounded-2xl p-5 hover:shadow-lg transition-all relative overflow-hidden group flex flex-col justify-between">
         <div class="absolute top-0 left-0 right-0 h-1 bg-gradient-to-r from-emerald-500 via-teal-500 to-cyan-500"></div>

         <div>
           <!-- Header Card -->
           <div class="flex justify-between items-start mb-3">
             <div class="flex items-center gap-2">
               <div class="p-2 rounded-xl bg-emerald-500/10 text-emerald-600 border border-emerald-500/20">
                 <GraduationCap :size="18" />
               </div>
               <div>
                 <h3 class="font-bold text-heading text-base leading-snug line-clamp-1" :title="group.deptText">{{ group.deptText }}</h3>
                 <p class="text-[11px] font-semibold text-emerald-600">{{ group.dayOfWeek }} · {{ group.shiftName }}</p>
               </div>
             </div>
             <GlassBadge variant="success" size="sm">Đã xuất bản</GlassBadge>
           </div>

           <!-- Term & Campus Badge -->
           <div class="flex items-center gap-2 text-xs text-muted mb-4 bg-surface-input p-2 rounded-xl border border-card">
             <CalendarDays :size="14" class="text-placeholder flex-shrink-0" />
             <span class="font-semibold truncate">{{ group.term }} · {{ group.campus }}</span>
           </div>

           <!-- ÁP DỤNG CHO DỮ LIỆU ĐA NGÀNH / PHÒNG / LỚP / GIẢNG VIÊN -->
           <div class="space-y-2.5 py-3 border-y border-default text-xs">
             <!-- Danh sách Lớp áp dụng -->
             <div class="flex items-start gap-2">
               <Users :size="14" class="text-placeholder mt-0.5 flex-shrink-0" />
               <div class="flex-1 min-w-0">
                 <p class="text-[10px] text-muted uppercase tracking-wider font-bold">Lớp áp dụng ({{ group.classesArray.length || group.totalClassesCount }})</p>
                 <div class="flex flex-wrap gap-1 mt-1">
                   <span v-for="cls in group.classesArray.slice(0, 3)" :key="cls" class="px-2 py-0.5 rounded-md bg-blue-500/10 text-blue-700 font-mono text-[10px] font-bold">
                     {{ cls }}
                   </span>
                   <span v-if="group.classesArray.length > 3" class="px-1.5 py-0.5 rounded-md bg-surface-input text-muted text-[10px]">
                     +{{ group.classesArray.length - 3 }} lớp
                   </span>
                 </div>
               </div>
             </div>

             <!-- Danh sách Phòng học áp dụng -->
             <div class="flex items-start gap-2">
               <MapPin :size="14" class="text-placeholder mt-0.5 flex-shrink-0" />
               <div class="flex-1 min-w-0">
                 <p class="text-[10px] text-muted uppercase tracking-wider font-bold">Phòng học áp dụng</p>
                 <p class="font-bold text-heading text-xs truncate" :title="group.roomsText">{{ group.roomsText }}</p>
               </div>
             </div>

             <!-- Danh sách Giảng viên phụ trách -->
             <div class="flex items-start gap-2">
               <User :size="14" class="text-placeholder mt-0.5 flex-shrink-0" />
               <div class="flex-1 min-w-0">
                 <p class="text-[10px] text-muted uppercase tracking-wider font-bold">Giảng viên phụ trách</p>
                 <p class="font-bold text-heading text-xs truncate" :title="group.teachersText">{{ group.teachersText }}</p>
               </div>
             </div>
           </div>
         </div>

         <!-- Footer Card -->
         <div class="mt-4 pt-3 flex items-center justify-between border-t border-card">
           <div class="flex items-center gap-1.5 text-xs text-muted">
             <Clock :size="14" />
             <span class="font-bold text-heading">{{ group.displayHours }} Giờ</span>
           </div>
           <GlassButton variant="primary" size="sm" @click="viewScheduleGroup(group)">
             <Eye :size="14" class="mr-1" /> Xem lịch chi tiết
           </GlassButton>
         </div>

      </div>

      <div v-if="filteredData.length === 0" class="col-span-full py-16 text-center surface-card border border-card rounded-2xl">
        <CalendarDays :size="48" class="text-placeholder mx-auto mb-3" />
        <p class="text-sm font-semibold text-muted">Không có thời khóa biểu phù hợp</p>
      </div>
    </template>

    <!-- FULL WEEKLY SCHEDULE MODAL VIEW FOR SELECTED GROUP -->
    <div v-if="showScheduleModal && selectedGroup" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
      <div class="surface-card border border-card rounded-3xl w-full max-w-5xl max-h-[90vh] flex flex-col shadow-2xl overflow-hidden animate-in fade-in zoom-in-95 duration-200">

        <!-- Modal Header -->
        <div class="p-5 border-b border-default flex items-center justify-between bg-surface-card">
          <div class="flex items-center gap-3">
            <div class="h-10 w-10 rounded-2xl bg-emerald-500/10 text-emerald-600 flex items-center justify-center border border-emerald-500/20">
              <CalendarDays :size="20" />
            </div>
            <div>
              <div class="flex items-center gap-2">
                <h2 class="text-lg font-bold text-heading">{{ selectedGroup?.deptText }}</h2>
                <span class="text-xs font-mono font-bold text-emerald-700 bg-emerald-500/10 px-2 py-0.5 rounded-md border border-emerald-500/20">Khung lịch: {{ selectedGroup?.dayOfWeek }} · {{ selectedGroup?.shiftName }}</span>
              </div>
              <p class="text-xs text-muted font-medium mt-0.5">{{ selectedGroup?.term }} · {{ selectedGroup?.campus }}</p>
            </div>
          </div>
          <div class="flex items-center gap-2">
            <GlassButton variant="secondary" size="sm" @click="printSchedule">
              <Printer :size="14" class="mr-1.5" /> In lịch biểu
            </GlassButton>
            <button @click="showScheduleModal = false" class="p-2 text-muted hover:text-heading hover:bg-surface-input rounded-xl transition-colors">
              <X :size="20" />
            </button>
          </div>
        </div>

        <!-- Schedule Grid Content -->
        <div class="p-6 overflow-y-auto flex-1 space-y-4">

          <!-- Detail Badges Summary -->
          <div class="grid grid-cols-1 md:grid-cols-4 gap-3 p-4 rounded-2xl surface-input border border-card text-xs">
            <div>
              <span class="text-[10px] uppercase font-bold text-muted block">Chuyên ngành áp dụng</span>
              <span class="font-bold text-heading text-xs line-clamp-1">{{ selectedGroup?.deptText }}</span>
            </div>
            <div>
              <span class="text-[10px] uppercase font-bold text-muted block">Danh sách Lớp học phần</span>
              <span class="font-bold text-heading text-xs line-clamp-1">{{ selectedGroup?.classesText }}</span>
            </div>
            <div>
              <span class="text-[10px] uppercase font-bold text-muted block">Phòng học xếp sẵn</span>
              <span class="font-bold text-heading text-xs line-clamp-1">{{ selectedGroup?.roomsText }}</span>
            </div>
            <div>
              <span class="text-[10px] uppercase font-bold text-muted block">Giảng viên giảng dạy</span>
              <span class="font-bold text-heading text-xs line-clamp-1">{{ selectedGroup?.teachersText }}</span>
            </div>
          </div>

          <!-- Weekly Schedule Table Grid -->
          <div class="border border-card rounded-2xl overflow-hidden shadow-sm">
            <table class="w-full border-collapse text-left text-xs">
              <thead>
                <tr class="surface-solid border-b border-card">
                  <th class="p-3 font-bold text-muted text-center w-28 border-r border-card">Ca học / Thứ</th>
                  <th v-for="day in daysOfWeek" :key="day" class="p-3 font-bold text-heading text-center border-r border-card last:border-r-0">
                    {{ day }}
                  </th>
                </tr>
              </thead>
              <tbody class="divide-y divide-card">
                <tr v-for="shift in shifts" :key="shift.id" class="hover:bg-surface-input/50 transition-colors">
                  <!-- Shift Header -->
                  <td class="p-3 border-r border-card font-semibold surface-solid">
                    <p class="font-bold text-heading text-xs">{{ shift.name }}</p>
                    <p class="text-[10px] text-muted">{{ shift.time }}</p>
                  </td>
                  <!-- Day Cells -->
                  <td v-for="(day, dayIdx) in daysOfWeek" :key="dayIdx" class="p-2 border-r border-card last:border-r-0 align-top h-24">
                    <div v-if="getGridItemForGroup(dayIdx, shift.id)"
                         :class="['p-2 rounded-xl border text-[11px] font-semibold h-full flex flex-col justify-between shadow-xs', getGridItemForGroup(dayIdx, shift.id).color]">
                      <div>
                        <p class="font-bold leading-tight line-clamp-2">{{ getGridItemForGroup(dayIdx, shift.id).subject }}</p>
                        <p class="text-[10px] opacity-80 mt-0.5 font-mono">{{ getGridItemForGroup(dayIdx, shift.id).code }}</p>
                      </div>
                      <div class="mt-2 text-[10px] opacity-90 flex flex-col gap-0.5 border-t stroke-current/20 pt-1">
                        <span class="flex items-center gap-1 truncate"><User :size="10" /> {{ getGridItemForGroup(dayIdx, shift.id).teacher }}</span>
                        <span class="flex items-center gap-1 font-bold truncate"><MapPin :size="10" /> {{ getGridItemForGroup(dayIdx, shift.id).room }}</span>
                      </div>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

        </div>

        <!-- Modal Footer -->
        <div class="p-4 border-t border-default flex justify-between items-center surface-card">
          <span class="text-xs font-bold text-muted">Trạng thái: <span class="text-emerald-600">Đã phê duyệt & Đã phát hành chính thức</span></span>
          <GlassButton variant="secondary" @click="showScheduleModal = false">
            Đóng cửa sổ
          </GlassButton>
        </div>

      </div>
    </div>

  </div>
</template>
