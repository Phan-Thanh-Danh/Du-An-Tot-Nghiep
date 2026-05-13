<script setup>
import { ref } from 'vue'
import { 
  CheckCircle2, XCircle, Users, Clock, MapPin, 
  ChevronRight, Save, Calendar, Search, ArrowLeft 
} from 'lucide-vue-next'

const todayClasses = ref([
  { id: 1, code: 'SE1601', name: 'Lập trình Java', room: 'A201', time: '07:30 - 10:45', students: 45 },
  { id: 2, code: 'SS1402', name: 'Lập trình Web', room: 'B305', time: '12:30 - 15:45', students: 38 },
])

const selectedClass = ref(null)
const students = ref([
  { id: 'SV16001', name: 'Nguyễn Văn A', present: true },
  { id: 'SV16002', name: 'Trần Thị B', present: true },
  { id: 'SV16003', name: 'Lê Hoàng C', present: false },
  { id: 'SV16004', name: 'Phạm Minh D', present: true },
])

function selectClass(cls) {
  selectedClass.value = cls
}

function submitAttendance() {
  alert('Đã lưu điểm danh cho lớp ' + selectedClass.value.code)
  selectedClass.value = null
}
</script>

<template>
  <div class="space-y-6 pb-10">
    <!-- Header -->
    <div v-if="!selectedClass">
      <h1 class="text-3xl font-bold text-slate-800 tracking-tight">Điểm danh hôm nay</h1>
      <p class="text-slate-500 mt-1">Danh sách các lớp học có lịch giảng dạy trong ngày hôm nay: {{ new Date().toLocaleDateString('vi-VN') }}</p>
    </div>

    <!-- ── STEP 1: Select Class ── -->
    <div v-if="!selectedClass" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div v-for="cls in todayClasses" :key="cls.id" 
           @click="selectClass(cls)"
           class="lg-card-glass lg-card-hover group border-slate-100 p-6 cursor-pointer">
        <div class="flex justify-between items-start mb-6">
           <div class="h-12 w-12 rounded-2xl bg-indigo-50 flex items-center justify-center text-indigo-600 border border-indigo-100/50 shadow-sm group-hover:scale-110 transition-transform">
              <Calendar :size="24" />
           </div>
           <span class="rounded-full bg-emerald-50 px-3 py-1 text-[10px] font-black text-emerald-600 uppercase">Sắp diễn ra</span>
        </div>
        
        <h3 class="text-xl font-bold text-slate-800">{{ cls.code }}</h3>
        <p class="text-sm font-semibold text-slate-500 mt-1">{{ cls.name }}</p>
        
        <div class="mt-6 space-y-3">
           <div class="flex items-center gap-3 text-sm text-slate-600">
              <Clock :size="16" class="text-slate-400" />
              <span>{{ cls.time }}</span>
           </div>
           <div class="flex items-center gap-3 text-sm text-slate-600">
              <MapPin :size="16" class="text-slate-400" />
              <span>Phòng: <span class="font-bold text-slate-800">{{ cls.room }}</span></span>
           </div>
           <div class="flex items-center gap-3 text-sm text-slate-600">
              <Users :size="16" class="text-slate-400" />
              <span>Sĩ số: <span class="font-bold text-slate-800">{{ cls.students }} SV</span></span>
           </div>
        </div>

        <button class="mt-8 w-full rounded-xl bg-slate-900 py-3 text-xs font-bold text-white hover:bg-slate-800 transition-all flex items-center justify-center gap-2">
           Bắt đầu điểm danh <ChevronRight :size="14" />
        </button>
      </div>
    </div>

    <!-- ── STEP 2: Attendance Check ── -->
    <div v-else class="space-y-6">
       <div class="flex items-center gap-4">
          <button @click="selectedClass = null" class="p-2 rounded-xl bg-white border border-slate-100 text-slate-400 hover:text-indigo-600 shadow-sm transition-all">
             <ArrowLeft :size="20" />
          </button>
          <div>
             <h2 class="text-2xl font-bold text-slate-800">Điểm danh lớp {{ selectedClass.code }}</h2>
             <p class="text-sm text-slate-500">{{ selectedClass.name }} • {{ selectedClass.time }}</p>
          </div>
       </div>

       <div class="rounded-[28px] border border-slate-100 bg-white shadow-xl overflow-hidden">
          <div class="p-6 border-b border-slate-50 flex items-center justify-between gap-4">
             <div class="flex items-center gap-6">
                <div class="flex items-center gap-2">
                   <div class="h-3 w-3 rounded-full bg-emerald-500"></div>
                   <span class="text-xs font-bold text-slate-500">Có mặt: 42</span>
                </div>
                <div class="flex items-center gap-2">
                   <div class="h-3 w-3 rounded-full bg-rose-500"></div>
                   <span class="text-xs font-bold text-slate-500">Vắng: 3</span>
                </div>
             </div>
             <div class="relative w-64">
                <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
                <input type="text" placeholder="Tìm sinh viên..." class="w-full rounded-xl border border-slate-100 bg-slate-50 pl-9 pr-4 py-2 text-xs outline-none focus:border-indigo-300" />
             </div>
          </div>

          <div class="overflow-x-auto text-slate-800">
             <table class="w-full text-left">
                <thead>
                   <tr class="bg-slate-50/50 text-[11px] font-bold uppercase tracking-wider text-slate-400">
                      <th class="px-8 py-4">Sinh viên</th>
                      <th class="px-6 py-4">Mã số SV</th>
                      <th class="px-6 py-4 text-center">Trạng thái điểm danh</th>
                   </tr>
                </thead>
                <tbody class="divide-y divide-slate-50">
                   <tr v-for="sv in students" :key="sv.id" class="group hover:bg-slate-50/50 transition-colors">
                      <td class="px-8 py-4">
                         <div class="flex items-center gap-3">
                            <div class="h-10 w-10 rounded-full bg-slate-100 flex items-center justify-center text-slate-400 font-bold text-xs">
                               {{ sv.name.split(' ').pop()[0] }}
                            </div>
                            <p class="text-sm font-bold text-slate-700">{{ sv.name }}</p>
                         </div>
                      </td>
                      <td class="px-6 py-4 text-sm font-medium text-slate-500">{{ sv.id }}</td>
                      <td class="px-6 py-4">
                         <div class="flex items-center justify-center gap-8">
                            <label class="flex items-center gap-2 cursor-pointer group/check">
                               <input type="radio" :name="'att-'+sv.id" :checked="sv.present" @change="sv.present = true" class="hidden" />
                               <div :class="['h-6 w-6 rounded-lg border-2 flex items-center justify-center transition-all', sv.present ? 'bg-emerald-500 border-emerald-500 text-white shadow-lg shadow-emerald-100' : 'border-slate-200 bg-white text-transparent group-hover/check:border-emerald-300']">
                                  <CheckCircle2 :size="14" />
                               </div>
                               <span :class="['text-xs font-bold transition-colors', sv.present ? 'text-emerald-600' : 'text-slate-400']">Có mặt</span>
                            </label>

                            <label class="flex items-center gap-2 cursor-pointer group/check">
                               <input type="radio" :name="'att-'+sv.id" :checked="!sv.present" @change="sv.present = false" class="hidden" />
                               <div :class="['h-6 w-6 rounded-lg border-2 flex items-center justify-center transition-all', !sv.present ? 'bg-rose-500 border-rose-500 text-white shadow-lg shadow-rose-100' : 'border-slate-200 bg-white text-transparent group-hover/check:border-rose-300']">
                                  <XCircle :size="14" />
                               </div>
                               <span :class="['text-xs font-bold transition-colors', !sv.present ? 'text-rose-600' : 'text-slate-400']">Vắng</span>
                            </label>
                         </div>
                      </td>
                   </tr>
                </tbody>
             </table>
          </div>

          <div class="p-8 bg-slate-50/50 border-t border-slate-100 flex justify-end">
             <button @click="submitAttendance" class="rounded-2xl bg-indigo-600 px-10 py-4 text-sm font-black text-white shadow-xl shadow-indigo-100 hover:bg-indigo-700 transition-all active:scale-95 flex items-center gap-3">
                <Save :size="20" /> HOÀN TẤT ĐIỂM DANH
             </button>
          </div>
       </div>
    </div>
  </div>
</template>

<style scoped>
.shadow-emerald-100 { shadow-color: rgba(16, 185, 129, 0.2); }
.shadow-rose-100 { shadow-color: rgba(244, 63, 94, 0.2); }
</style>
