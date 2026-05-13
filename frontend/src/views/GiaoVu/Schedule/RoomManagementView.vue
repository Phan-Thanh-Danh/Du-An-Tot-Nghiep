<script setup>
import { ref } from 'vue'
import { 
  Search, 
  Filter, 
  Plus, 
  MapPin, 
  Users, 
  Monitor, 
  Hammer,
  MoreVertical,
  CheckCircle,
  XCircle,
  History,
  Building
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Rooms ──────────────────────────────────────────────
const rooms = ref([
  { id: 'PH001', name: 'Phòng 302', campus: 'Cơ sở chính', capacity: 45, type: 'Lý thuyết', devices: ['Projector', 'Air Conditioner'], status: 'active' },
  { id: 'PH002', name: 'Lab 2', campus: 'Cơ sở chính', capacity: 30, type: 'Thực hành', devices: ['PCs', 'Server', 'Projector'], status: 'active' },
  { id: 'PH003', name: 'Phòng 105', campus: 'Cơ sở phụ', capacity: 60, type: 'Hội trường', devices: ['Sound System', 'Projector'], status: 'maintenance' },
  { id: 'PH004', name: 'Phòng 401', campus: 'Cơ sở chính', capacity: 40, type: 'Lý thuyết', devices: ['Whiteboard'], status: 'active' },
  { id: 'PH005', name: 'Studio 1', campus: 'Cơ sở chính', capacity: 15, type: 'Chuyên dụng', devices: ['Cameras', 'Green Screen'], status: 'inactive' },
])

const getStatusBadge = (status) => {
  switch (status) {
    case 'active': return 'bg-green-100 text-green-700'
    case 'maintenance': return 'bg-orange-100 text-orange-700'
    case 'inactive': return 'bg-red-100 text-red-700'
    default: return 'bg-slate-100 text-slate-700'
  }
}
</script>

<template>
  <PageContainer 
    title="Quản lý phòng học" 
    subtitle="Theo dõi sức chứa, thiết bị và tình trạng sử dụng của các phòng học."
  >
    <template #actions>
      <button class="lg-button-primary px-5 py-2.5 text-sm font-bold shadow-lg shadow-blue-500/20">
        <Plus :size="18" /> Thêm phòng mới
      </button>
    </template>

    <div class="space-y-6">
      <!-- ── Filter Bar ── -->
      <div class="lg-glass-strong p-4 rounded-[24px] flex flex-wrap items-center justify-between gap-4">
        <div class="flex-1 min-w-[280px] relative">
          <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
          <input 
            type="text" 
            placeholder="Tìm tên phòng hoặc mã phòng..." 
            class="w-full bg-white border border-slate-100 rounded-xl pl-11 pr-4 py-2.5 text-sm font-medium outline-none focus:ring-4 focus:ring-blue-500/10 transition-all"
          >
        </div>
        <div class="flex items-center gap-3">
          <select class="bg-white border border-slate-100 rounded-xl px-4 py-2.5 text-sm font-bold outline-none">
            <option>Tất cả cơ sở</option>
            <option>Cơ sở chính</option>
            <option>Cơ sở phụ</option>
          </select>
          <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold">
            <Filter :size="18" /> Lọc
          </button>
        </div>
      </div>

      <!-- ── Rooms Grid ── -->
      <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
        <div v-for="room in rooms" :key="room.id" class="lg-card-glass group p-6 transition-all hover:-translate-y-1 hover:shadow-xl relative overflow-hidden">
          <!-- Status Indicator -->
          <div class="absolute top-4 right-4 flex items-center gap-2">
            <span :class="['h-2 w-2 rounded-full', room.status === 'active' ? 'bg-green-500 animate-pulse' : 'bg-red-500']"></span>
            <span class="text-[10px] font-black uppercase tracking-widest text-slate-400">{{ room.status }}</span>
          </div>

          <div class="flex items-start justify-between">
            <div class="h-12 w-12 rounded-2xl bg-blue-50 flex items-center justify-center text-blue-600 border border-blue-100">
              <Building v-if="room.type !== 'Thực hành'" :size="24" />
              <Monitor v-else :size="24" />
            </div>
            <button class="p-1.5 hover:bg-slate-50 rounded-lg text-slate-400">
              <MoreVertical :size="18" />
            </button>
          </div>

          <div class="mt-4">
            <h3 class="text-lg font-black text-slate-800 leading-tight group-hover:text-blue-600 transition-colors">{{ room.name }}</h3>
            <div class="flex items-center gap-1.5 text-xs font-bold text-slate-400 mt-1 uppercase tracking-tighter">
              <MapPin :size="12" /> {{ room.campus }}
            </div>
          </div>

          <div class="mt-6 grid grid-cols-2 gap-4">
            <div class="bg-slate-50/50 rounded-xl p-3 border border-slate-100">
              <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest">Sức chứa</p>
              <div class="flex items-center gap-2 mt-1">
                <Users :size="14" class="text-blue-500" />
                <span class="text-sm font-black text-slate-700">{{ room.capacity }} SV</span>
              </div>
            </div>
            <div class="bg-slate-50/50 rounded-xl p-3 border border-slate-100">
              <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest">Loại phòng</p>
              <p class="text-sm font-black text-slate-700 mt-1">{{ room.type }}</p>
            </div>
          </div>

          <div class="mt-4">
            <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-2">Thiết bị</p>
            <div class="flex flex-wrap gap-2">
              <span v-for="device in room.devices" :key="device" class="px-2 py-0.5 rounded-lg bg-white/60 border border-slate-100 text-[10px] font-bold text-slate-500 shadow-sm">
                {{ device }}
              </span>
            </div>
          </div>

          <div class="mt-6 pt-4 border-t border-slate-100/50 flex items-center justify-between">
            <span :class="['px-2.5 py-1 rounded-full text-[10px] font-bold uppercase tracking-widest', getStatusBadge(room.status)]">
              {{ room.status }}
            </span>
            <div class="flex items-center gap-2">
              <button class="p-2 hover:bg-slate-50 rounded-lg text-slate-400 transition-all" title="Lịch sử dùng">
                <History :size="16" />
              </button>
              <button class="p-2 hover:bg-orange-50 hover:text-orange-600 rounded-lg text-slate-400 transition-all" title="Bảo trì">
                <Hammer :size="16" />
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </PageContainer>
</template>
