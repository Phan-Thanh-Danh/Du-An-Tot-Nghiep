<script setup>
import { ref } from 'vue'
import { 
  FileText, User, Search, CheckCircle, XCircle, 
  Clock, Filter, MoreVertical, Eye, ArrowLeft 
} from 'lucide-vue-next'

const requests = ref([
  { id: 1, student: 'Nguyễn Văn A', type: 'Xin vắng học', content: 'Em xin nghỉ buổi học ngày 20/05 do có việc gia đình.', status: 'Pending', time: '8:00 AM' },
  { id: 2, student: 'Trần Thị B', type: 'Phúc khảo điểm', content: 'Em xin phúc khảo lại điểm thi Lab 2, em thấy mình làm đúng hết.', status: 'Pending', time: '10:30 AM' },
  { id: 3, student: 'Lê Hoàng C', type: 'Đơn xin học bù', content: 'Em muốn xin học bù lớp SE1601 sang SE1602 vào thứ 5.', status: 'Pending', time: 'Hôm qua' },
])

const selectedReq = ref(null)

function selectRequest(req) {
  selectedReq.value = req
}

function processRequest(action) {
  alert(`Đã ${action} đơn của ${selectedReq.value.student}`)
  selectedReq.value = null
}
</script>

<template>
  <div class="space-y-6 pb-10 text-slate-800">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div>
        <h1 class="text-3xl font-bold text-slate-800 tracking-tight">Đơn cần xử lý</h1>
        <p class="text-slate-500 mt-1">Phê duyệt hoặc từ chối các đơn từ, yêu cầu hành chính của sinh viên.</p>
      </div>
      <div class="flex gap-2">
         <span class="rounded-full bg-amber-100 px-4 py-2 text-xs font-bold text-amber-600 border border-amber-200">
           3 đơn đang chờ
         </span>
      </div>
    </div>

    <div class="grid grid-cols-1 xl:grid-cols-3 gap-6">
      <!-- List -->
      <div class="xl:col-span-2">
         <div class="rounded-[28px] border border-slate-100 bg-white shadow-sm overflow-hidden">
            <div class="p-6 border-b border-slate-50 flex items-center justify-between">
               <h2 class="text-lg font-bold">Yêu cầu mới nhất</h2>
               <div class="relative w-64">
                  <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
                  <input type="text" placeholder="Tìm kiếm..." class="w-full rounded-xl border border-slate-100 bg-slate-50 pl-9 pr-4 py-2 text-xs outline-none focus:border-indigo-300" />
               </div>
            </div>

            <div class="divide-y divide-slate-50">
               <div v-for="req in requests" :key="req.id" 
                    @click="selectRequest(req)"
                    :class="['p-6 cursor-pointer transition-all hover:bg-slate-50 group', selectedReq?.id === req.id ? 'bg-indigo-50/50 border-l-4 border-indigo-600' : '']">
                  <div class="flex items-center justify-between mb-2">
                     <div class="flex items-center gap-2">
                        <FileText :size="16" class="text-indigo-500" />
                        <span class="text-sm font-black text-slate-800">{{ req.type }}</span>
                     </div>
                     <span class="text-[10px] font-bold text-slate-400">{{ req.time }}</span>
                  </div>
                  <div class="flex items-center gap-3">
                     <div class="h-8 w-8 rounded-full bg-slate-100 flex items-center justify-center text-[10px] font-bold text-slate-400">
                        {{ req.student.split(' ').pop()[0] }}
                     </div>
                     <p class="text-xs text-slate-600 truncate flex-1">{{ req.student }}: {{ req.content }}</p>
                     <ChevronRight :size="16" class="text-slate-300 group-hover:text-indigo-600 transition-colors" />
                  </div>
               </div>
            </div>
         </div>
      </div>

      <!-- Detail/Action -->
      <div class="xl:col-span-1">
         <div v-if="selectedReq" class="rounded-[32px] border border-slate-100 bg-white shadow-xl p-8 sticky top-6">
            <div class="flex flex-col items-center text-center mb-8">
               <div class="h-20 w-20 rounded-3xl bg-indigo-50 flex items-center justify-center text-indigo-600 mb-4 border border-indigo-100 shadow-lg shadow-indigo-100/20">
                  <FileText :size="40" />
               </div>
               <h3 class="text-lg font-black text-slate-800">{{ selectedReq.type }}</h3>
               <p class="text-xs font-bold text-slate-400 uppercase tracking-widest mt-1">{{ selectedReq.student }}</p>
            </div>

            <div class="space-y-6">
               <div class="p-4 rounded-2xl bg-slate-50 border border-slate-100">
                  <label class="text-[10px] font-black text-slate-400 uppercase tracking-widest block mb-2">Nội dung đơn</label>
                  <p class="text-sm text-slate-700 leading-relaxed italic">"{{ selectedReq.content }}"</p>
               </div>

               <div class="flex flex-col gap-3">
                  <button @click="processRequest('Phê duyệt')" class="w-full rounded-2xl bg-indigo-600 py-4 text-sm font-black text-white shadow-xl shadow-indigo-100 hover:bg-indigo-700 transition-all flex items-center justify-center gap-3">
                     <CheckCircle :size="20" /> CHẤP NHẬN
                  </button>
                  <button @click="processRequest('Từ chối')" class="w-full rounded-2xl bg-white border-2 border-rose-100 py-4 text-sm font-black text-rose-500 hover:bg-rose-50 transition-all flex items-center justify-center gap-3">
                     <XCircle :size="20" /> TỪ CHỐI
                  </button>
                  <button @click="selectedReq = null" class="w-full py-2 text-xs font-bold text-slate-400 hover:text-slate-600">Đóng cửa sổ</button>
               </div>
            </div>
         </div>

         <div v-else class="h-full min-h-[400px] rounded-[32px] border border-dashed border-slate-200 bg-slate-50/50 p-12 text-center flex flex-col items-center justify-center">
            <div class="h-24 w-24 rounded-full bg-white flex items-center justify-center text-slate-200 mb-6 shadow-sm">
               <FileText :size="48" />
            </div>
            <h3 class="text-lg font-bold text-slate-400">Chọn đơn để xử lý</h3>
            <p class="text-sm text-slate-300 mt-2">Nội dung chi tiết và các nút phê duyệt sẽ hiển thị tại đây.</p>
         </div>
      </div>
    </div>
  </div>
</template>
