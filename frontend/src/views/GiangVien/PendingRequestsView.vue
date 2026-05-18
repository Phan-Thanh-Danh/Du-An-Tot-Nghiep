<script setup>
import { ref } from 'vue'
import { 
  FileText, User, Search, CheckCircle, XCircle, 
  Clock, Filter, MoreVertical, Eye, ArrowLeft,
  ChevronRight, AlertCircle, FileQuestion, Mail
} from 'lucide-vue-next'

const requests = ref([
  { id: 1, student: 'Nguyễn Văn A', type: 'Xin vắng học', content: 'Em xin nghỉ buổi học ngày 20/05 do có việc gia đình.', status: 'Pending', time: '8:00 AM', tag: 'Vắng học', color: 'blue' },
  { id: 2, student: 'Trần Thị B', type: 'Phúc khảo điểm', content: 'Em xin phúc khảo lại điểm thi Lab 2, em thấy mình làm đúng hết.', status: 'Pending', time: '10:30 AM', tag: 'Khảo thí', color: 'cyan' },
  { id: 3, student: 'Lê Hoàng C', type: 'Đơn xin học bù', content: 'Em muốn xin học bù lớp SE1601 sang SE1602 vào thứ 5.', status: 'Pending', time: 'Hôm qua', tag: 'Học vụ', color: 'emerald' },
])

const selectedReq = ref(null)

function selectRequest(req) {
  selectedReq.value = req
}

function processRequest(action) {
  alert(`Đã ${action.toLowerCase()} đơn của ${selectedReq.value.student}`)
  selectedReq.value = null
}

const getTagColor = (color) => {
  const colors = {
    blue: 'bg-blue-100 text-blue-700 border-blue-200',
    cyan: 'bg-cyan-100 text-cyan-700 border-cyan-200',
    emerald: 'bg-emerald-100 text-emerald-700 border-emerald-200',
  }
  return colors[color] || 'bg-slate-100 text-slate-700 border-slate-200'
}

const getIconBg = (color) => {
  const bgs = {
    blue: 'bg-gradient-to-br from-blue-100 to-blue-100 text-blue-600',
    cyan: 'bg-gradient-to-br from-cyan-100 to-cyan-100 text-cyan-600',
    emerald: 'bg-gradient-to-br from-emerald-100 to-teal-100 text-emerald-600',
  }
  return bgs[color] || 'bg-slate-100 text-slate-500'
}
</script>

<template>
  <div class="space-y-8 pb-12 animate-fade-in">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-white p-6 rounded-[24px] shadow-sm border border-slate-100/60 backdrop-blur-xl relative overflow-hidden">
      <!-- Decorative background glow -->
      <div class="absolute -top-10 -right-10 w-40 h-40 bg-blue-500/10 rounded-full blur-3xl pointer-events-none"></div>
      <div class="absolute -bottom-10 -left-10 w-40 h-40 bg-cyan-500/10 rounded-full blur-3xl pointer-events-none"></div>

      <div class="flex items-center gap-5 relative z-10">
        <div class="p-3 rounded-2xl bg-blue-50 text-blue-600 border border-blue-100 shadow-inner">
          <FileText :size="24" stroke-width="2" />
        </div>
        <div>
          <h1 class="text-3xl font-black text-slate-800 tracking-tight">Đơn cần xử lý</h1>
          <p class="text-sm text-slate-500 mt-1">Phê duyệt hoặc từ chối các đơn từ, yêu cầu của sinh viên.</p>
        </div>
      </div>

      <div class="flex items-center gap-3 relative z-10">
         <span class="inline-flex items-center gap-1.5 rounded-full bg-gradient-to-r from-amber-400 to-orange-400 px-4 py-2 text-xs font-bold text-white shadow-lg shadow-orange-500/20">
           <AlertCircle :size="14" />
           {{ requests.length }} đơn đang chờ
         </span>
      </div>
    </div>

    <div class="grid grid-cols-1 xl:grid-cols-3 gap-6">
      <!-- List Column -->
      <div class="xl:col-span-2">
         <div class="bg-white rounded-[32px] border border-slate-100 shadow-sm overflow-hidden flex flex-col h-full">
            <div class="p-6 md:p-8 border-b border-slate-100/80 flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-slate-50/30">
               <div>
                 <h2 class="text-xl font-bold text-slate-800">Yêu cầu mới nhất</h2>
               </div>
               <div class="relative w-full sm:w-64">
                  <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
                  <input type="text" placeholder="Tìm kiếm đơn từ..." class="w-full rounded-2xl border border-slate-200 bg-white pl-11 pr-4 py-2.5 text-sm outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-all shadow-sm" />
               </div>
            </div>

            <div class="divide-y divide-slate-100/80 flex-1 bg-white">
               <div v-for="req in requests" :key="req.id" 
                    @click="selectRequest(req)"
                    :class="['p-6 cursor-pointer transition-all duration-300 group hover:bg-slate-50/50 relative overflow-hidden', selectedReq?.id === req.id ? 'bg-blue-50/30' : '']">
                  
                  <!-- Active Indicator -->
                  <div class="absolute left-0 top-0 bottom-0 w-1.5 bg-blue-600 rounded-r-full transition-transform duration-300 origin-left" :class="selectedReq?.id === req.id ? 'scale-x-100' : 'scale-x-0 group-hover:scale-x-100 opacity-50'"></div>
                  
                  <div class="flex items-start gap-4">
                    <div class="h-12 w-12 rounded-2xl flex items-center justify-center shadow-inner shrink-0 group-hover:scale-105 transition-transform" :class="getIconBg(req.color)">
                      <FileQuestion :size="20" />
                    </div>
                    
                    <div class="flex-1 min-w-0">
                      <div class="flex items-center justify-between mb-1">
                        <div class="flex items-center gap-2">
                           <h3 class="text-sm font-bold text-slate-800 group-hover:text-blue-600 transition-colors truncate">{{ req.type }}</h3>
                           <span class="px-2 py-0.5 rounded-lg border text-[10px] font-bold uppercase tracking-wider hidden sm:inline-block" :class="getTagColor(req.color)">{{ req.tag }}</span>
                        </div>
                        <span class="text-xs font-bold text-slate-400 flex items-center gap-1 shrink-0"><Clock :size="12" /> {{ req.time }}</span>
                      </div>
                      <p class="text-xs text-slate-500 font-medium truncate mb-2">Sinh viên: <span class="text-slate-700">{{ req.student }}</span></p>
                      <p class="text-sm text-slate-600 line-clamp-2 leading-relaxed">"{{ req.content }}"</p>
                    </div>
                    
                    <div class="shrink-0 self-center opacity-0 group-hover:opacity-100 transition-opacity translate-x-2 group-hover:translate-x-0 hidden sm:block">
                      <div class="h-8 w-8 rounded-full bg-white shadow-sm border border-slate-200 flex items-center justify-center text-slate-400 group-hover:text-blue-600 group-hover:border-blue-200">
                        <ChevronRight :size="16" />
                      </div>
                    </div>
                  </div>
               </div>
            </div>
         </div>
      </div>

      <!-- Detail/Action Column -->
      <div class="xl:col-span-1">
         <div v-if="selectedReq" class="rounded-[32px] bg-white shadow-xl shadow-slate-200/50 p-8 sticky top-6 border border-slate-100 overflow-hidden relative animate-in fade-in slide-in-from-right-4 duration-500">
            <!-- Background glow -->
            <div class="absolute top-0 right-0 w-32 h-32 blur-3xl opacity-30 rounded-full -z-10" :class="selectedReq.color === 'blue' ? 'bg-blue-400' : selectedReq.color === 'cyan' ? 'bg-cyan-400' : 'bg-emerald-400'"></div>
            
            <div class="flex flex-col items-center text-center mb-8 relative z-10">
               <div class="h-20 w-20 rounded-[24px] flex items-center justify-center mb-5 shadow-inner" :class="getIconBg(selectedReq.color)">
                  <FileText :size="36" stroke-width="1.5" />
               </div>
               <span class="px-2.5 py-1 rounded-lg border text-[10px] font-bold uppercase tracking-wider mb-3" :class="getTagColor(selectedReq.color)">{{ selectedReq.tag }}</span>
               <h3 class="text-xl font-black text-slate-800">{{ selectedReq.type }}</h3>
               <div class="flex items-center gap-2 mt-2 text-slate-500 bg-slate-50 px-3 py-1.5 rounded-xl border border-slate-100">
                  <User :size="14" />
                  <span class="text-xs font-bold">{{ selectedReq.student }}</span>
               </div>
            </div>

            <div class="space-y-6 relative z-10">
               <div class="p-5 rounded-2xl bg-slate-50/80 border border-slate-100/80 relative">
                  <Mail :size="16" class="absolute top-5 right-5 text-slate-300" />
                  <label class="text-[10px] font-black text-slate-400 uppercase tracking-widest block mb-3">Nội dung chi tiết</label>
                  <p class="text-sm text-slate-700 leading-relaxed italic border-l-2 border-blue-200 pl-3">"{{ selectedReq.content }}"</p>
               </div>

               <div class="flex flex-col gap-3 pt-4 border-t border-slate-100">
                  <button @click="processRequest('Chấp nhận')" class="group relative w-full rounded-2xl bg-gradient-to-r from-emerald-500 to-teal-500 p-[1px] overflow-hidden active:scale-95 transition-transform">
                     <div class="absolute inset-0 bg-white/20 -translate-x-full group-hover:translate-x-full transition-transform duration-700 ease-out"></div>
                     <div class="w-full bg-gradient-to-r from-emerald-500 to-teal-500 py-3.5 px-4 rounded-[15px] flex items-center justify-center gap-2">
                       <CheckCircle :size="18" class="text-emerald-50" />
                       <span class="text-sm font-bold text-white tracking-wide">PHÊ DUYỆT ĐƠN</span>
                     </div>
                  </button>
                  
                  <button @click="processRequest('Từ chối')" class="w-full rounded-2xl bg-white border border-rose-200 py-3.5 text-sm font-bold text-rose-500 hover:bg-rose-50 hover:border-rose-300 active:scale-95 transition-all flex items-center justify-center gap-2 shadow-sm">
                     <XCircle :size="18" /> TỪ CHỐI
                  </button>
                  
                  <button @click="selectedReq = null" class="w-full py-2.5 text-xs font-bold text-slate-400 hover:text-slate-600 transition-colors mt-1">Đóng cửa sổ</button>
               </div>
            </div>
         </div>

         <div v-else class="h-full min-h-[400px] rounded-[32px] border border-dashed border-slate-200 bg-slate-50/50 p-12 text-center flex flex-col items-center justify-center animate-in fade-in duration-500">
            <div class="h-24 w-24 rounded-[28px] bg-white flex items-center justify-center text-slate-300 mb-6 shadow-sm border border-slate-100">
               <FileQuestion :size="48" stroke-width="1.5" />
            </div>
            <h3 class="text-lg font-bold text-slate-500">Chưa chọn đơn nào</h3>
            <p class="text-sm text-slate-400 mt-2 max-w-[200px]">Vui lòng chọn một đơn từ danh sách bên trái để xem chi tiết và xử lý.</p>
         </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.animate-fade-in {
  animation: fadeIn 0.6s cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
