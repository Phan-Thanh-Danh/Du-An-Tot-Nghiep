<script setup>
import { ref } from 'vue'
import { 
  ArrowLeft, Search, Download, ExternalLink, 
  MessageSquare, Star, Save, CheckCircle2, AlertCircle 
} from 'lucide-vue-next'

const submissions = ref([
  { id: 1, studentId: 'SV16001', name: 'Nguyễn Văn A', file: 'Asm1_NVA.zip', time: '18/05/2026 09:30', score: 8.5, comment: 'Tốt, giao diện sạch sẽ.', status: 'Graded' },
  { id: 2, studentId: 'SV16002', name: 'Trần Thị B', file: 'Asm1_Final_B.rar', time: '19/05/2026 14:15', score: null, comment: '', status: 'Pending' },
  { id: 3, studentId: 'SV16003', name: 'Lê Hoàng C', file: 'LHC_Asm1.pdf', time: '19/05/2026 23:55', score: 9.0, comment: 'Xuất sắc!', status: 'Graded' },
  { id: 4, studentId: 'SV16004', name: 'Phạm Minh D', file: 'asm_java_d.zip', time: '20/05/2026 01:20', score: null, comment: '', status: 'Late' },
])

const selectedSubmission = ref(null)

function selectGrading(sub) {
  selectedSubmission.value = { ...sub }
}

function saveGrade() {
  if (selectedSubmission.value) {
    const idx = submissions.value.findIndex(s => s.id === selectedSubmission.value.id)
    if (idx !== -1) {
      submissions.value[idx] = { ...selectedSubmission.value, status: 'Graded' }
    }
    selectedSubmission.value = null
    alert('Đã lưu điểm và nhận xét!')
  }
}
</script>

<template>
  <div class="space-y-6 pb-10">
    <!-- Header -->
    <div class="flex items-center gap-4">
      <router-link to="/teacher/assignments" class="p-2 rounded-xl bg-white border border-slate-100 text-slate-400 hover:text-indigo-600 transition-all shadow-sm">
        <ArrowLeft :size="20" />
      </router-link>
      <div>
        <h1 class="text-2xl font-bold text-slate-800 tracking-tight">Chấm bài: Assignment 1</h1>
        <p class="text-sm text-slate-500">Lớp SE1601 • Hạn nộp: 20/05/2026</p>
      </div>
    </div>

    <div class="grid grid-cols-1 xl:grid-cols-3 gap-6">
      <!-- Left: List of Submissions -->
      <div class="xl:col-span-2 space-y-4">
        <div class="rounded-[28px] border border-slate-100 bg-white shadow-sm overflow-hidden">
          <div class="p-6 border-b border-slate-50 flex items-center justify-between gap-4">
            <h2 class="text-lg font-bold text-slate-800">Danh sách bài nộp</h2>
            <div class="relative w-64">
              <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
              <input type="text" placeholder="Tìm sinh viên..." class="w-full rounded-xl border border-slate-100 bg-slate-50 pl-9 pr-4 py-2 text-xs outline-none" />
            </div>
          </div>
          
          <div class="overflow-x-auto">
            <table class="w-full text-left">
              <thead>
                <tr class="bg-slate-50/50 text-[11px] font-bold uppercase tracking-wider text-slate-400">
                  <th class="px-6 py-4">Sinh viên</th>
                  <th class="px-6 py-4">File bài nộp</th>
                  <th class="px-6 py-4">Thời gian</th>
                  <th class="px-6 py-4">Điểm</th>
                  <th class="px-6 py-4 text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-50">
                <tr v-for="sub in submissions" :key="sub.id" 
                    :class="['group hover:bg-slate-50/50 transition-colors cursor-pointer', selectedSubmission?.id === sub.id ? 'bg-indigo-50/50' : '']"
                    @click="selectGrading(sub)">
                  <td class="px-6 py-4">
                    <div class="flex items-center gap-3">
                      <div class="h-8 w-8 rounded-full bg-slate-100 flex items-center justify-center text-slate-400 font-bold text-[10px]">
                        {{ sub.name.split(' ').pop()[0] }}
                      </div>
                      <div class="min-w-0">
                        <p class="text-xs font-bold text-slate-800 truncate">{{ sub.name }}</p>
                        <p class="text-[9px] text-slate-400 uppercase">{{ sub.studentId }}</p>
                      </div>
                    </div>
                  </td>
                  <td class="px-6 py-4">
                    <div class="flex items-center gap-2 text-xs text-indigo-600 font-bold hover:underline">
                      <Download :size="14" /> {{ sub.file }}
                    </div>
                  </td>
                  <td class="px-6 py-4">
                    <p class="text-[10px] font-bold text-slate-500">{{ sub.time }}</p>
                    <span v-if="sub.status === 'Late'" class="text-[9px] font-black text-rose-500 uppercase tracking-tighter">Nộp trễ</span>
                  </td>
                  <td class="px-6 py-4">
                    <span v-if="sub.score !== null" class="text-sm font-black text-indigo-600">{{ sub.score }}</span>
                    <span v-else class="text-xs text-slate-300 italic">Chưa chấm</span>
                  </td>
                  <td class="px-6 py-4 text-right">
                    <button class="p-2 text-slate-400 hover:text-indigo-600 rounded-lg"><ChevronRight :size="18" /></button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- Right: Grading Panel -->
      <div class="xl:col-span-1">
        <div v-if="selectedSubmission" class="rounded-[28px] border border-slate-100 bg-white shadow-xl p-6 sticky top-6">
           <div class="flex items-center gap-4 mb-6 pb-6 border-b border-slate-50">
              <div class="h-14 w-14 rounded-2xl bg-indigo-50 flex items-center justify-center text-indigo-600">
                 <Star :size="32" />
              </div>
              <div>
                 <h2 class="text-lg font-bold text-slate-800">Chấm điểm</h2>
                 <p class="text-sm text-slate-400 font-medium">{{ selectedSubmission.name }}</p>
              </div>
           </div>

           <div class="space-y-6 text-slate-800">
              <div class="space-y-2">
                 <label class="text-xs font-bold text-slate-400 uppercase tracking-widest">Xem bài nộp</label>
                 <button class="w-full flex items-center justify-between rounded-xl bg-slate-50 border border-slate-100 p-4 text-sm font-bold text-slate-600 hover:bg-slate-100 transition-all">
                    <div class="flex items-center gap-3">
                       <Download :size="18" class="text-indigo-500" />
                       <span>Tải về file bài làm</span>
                    </div>
                    <ExternalLink :size="16" class="text-slate-300" />
                 </button>
              </div>

              <div class="space-y-2">
                 <label class="text-xs font-bold text-slate-400 uppercase tracking-widest">Nhập điểm (0-10)</label>
                 <input 
                    type="number" 
                    v-model="selectedSubmission.score"
                    max="10" min="0" step="0.1"
                    placeholder="Ví dụ: 8.5"
                    class="w-full rounded-2xl border border-slate-200 bg-white p-4 text-2xl font-black outline-none focus:border-indigo-400 focus:ring-4 focus:ring-indigo-50 transition-all"
                 />
              </div>

              <div class="space-y-2">
                 <label class="text-xs font-bold text-slate-400 uppercase tracking-widest">Nhận xét của giảng viên</label>
                 <textarea 
                    v-model="selectedSubmission.comment"
                    rows="5"
                    placeholder="Ghi chú cho sinh viên..."
                    class="w-full rounded-2xl border border-slate-200 bg-white p-4 text-sm outline-none focus:border-indigo-400 focus:ring-4 focus:ring-indigo-50 transition-all leading-relaxed"
                 ></textarea>
              </div>

              <div class="pt-4 flex gap-3">
                 <button @click="selectedSubmission = null" class="flex-1 rounded-2xl border border-slate-200 py-4 text-sm font-bold text-slate-500 hover:bg-slate-50 transition-all">Hủy</button>
                 <button @click="saveGrade" class="flex-2 rounded-2xl bg-indigo-600 py-4 px-8 text-sm font-bold text-white shadow-lg shadow-indigo-100 hover:bg-indigo-700 transition-all flex items-center justify-center gap-2">
                    <CheckCircle2 :size="18" /> Hoàn tất chấm điểm
                 </button>
              </div>
           </div>
        </div>

        <div v-else class="rounded-[28px] border border-dashed border-slate-200 bg-slate-50/50 p-12 text-center flex flex-col items-center justify-center min-h-[400px]">
           <div class="h-20 w-20 rounded-full bg-white flex items-center justify-center text-slate-200 mb-4 shadow-sm">
              <MessageSquare :size="40" />
           </div>
           <h3 class="text-lg font-bold text-slate-400">Chọn sinh viên</h3>
           <p class="text-sm text-slate-300 mt-1 max-w-[200px]">Chọn bài nộp từ danh sách bên trái để thực hiện chấm điểm và nhận xét.</p>
        </div>
      </div>
    </div>

    <!-- Instructions -->
    <div class="lg-alert lg-alert-warning flex gap-4">
       <AlertCircle :size="24" class="shrink-0" />
       <div>
          <p class="font-bold">Lưu ý về bài nộp trễ</p>
          <p class="text-xs opacity-80 mt-1">Hệ thống đánh dấu các bài nộp sau ngày 20/05/2026 là "Late". Giảng viên có thể xem xét trừ điểm tùy theo quy định của môn học.</p>
       </div>
    </div>
  </div>
</template>

<style scoped>
.flex-2 { flex: 2; }
</style>
