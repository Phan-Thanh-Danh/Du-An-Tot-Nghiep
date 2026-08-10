<script setup>
import { computed, ref, watch, onMounted } from 'vue'
import { HelpCircle, Clock, Target, CheckCircle, Loader2 } from 'lucide-vue-next'
import { contentCouncilApi } from '@/services/contentCouncilApi'

const props = defineProps({
  content: {
    type: Object,
    required: true
  }
})

const showAnswers = ref(false)
const isLoading = ref(false)
const quizData = ref(null)

const loadQuizData = async () => {
  const quizId = props.content?.quizId || props.content?.maDeKiemTra
  if (!quizId) return
  isLoading.value = true
  try {
    const res = await contentCouncilApi.getQuizById(quizId)
    const raw = res?.data ?? res?.Data ?? res
    if (raw) {
      const cfg = raw.CauHinh ?? raw.cauHinh ?? {}
      quizData.value = {
        title: raw.TieuDe ?? raw.tieuDe ?? props.content?.title ?? '',
        duration: raw.ThoiGianPhut ?? raw.thoiGianPhut ?? 15,
        passScore: cfg.DiemDat ?? cfg.diemDat ?? 5,
        totalScore: cfg.TongDiem ?? cfg.tongDiem ?? 10,
        completionRule: (cfg.CachTinhDat ?? cfg.cachTinhDat) === 'theo_so_cau_dung' ? 'Theo số câu đúng' : 'Phải đạt',
        questions: (raw.DanhSachCauHoi ?? raw.danhSachCauHoi ?? []).map(q => {
          let rawOpts = q.LuaChon ?? q.luaChon ?? []
          if (typeof rawOpts === 'string') {
            try { rawOpts = JSON.parse(rawOpts) } catch { rawOpts = [] }
          }
          let rawAns = q.DapAnDung ?? q.dapAnDung ?? []
          if (typeof rawAns === 'string') {
            try { rawAns = JSON.parse(rawAns) } catch { rawAns = [rawAns] }
          }

          const parsedOptions = (Array.isArray(rawOpts) ? rawOpts : []).map((opt, idx) => {
            const letter = String.fromCharCode(65 + idx)
            let optId = letter
            let optText = ''
            if (typeof opt === 'string') {
              optText = opt
            } else if (opt && typeof opt === 'object') {
              optId = opt.id || opt.Id || opt.label || letter
              optText = opt.content || opt.Content || opt.text || opt.Text || ''
            }
            
            const isCorrect = isOptionCorrect(optId, rawAns)
            return {
              id: optId,
              text: optText,
              isCorrect
            }
          })

          const kieu = String(q.KieuLuaChon ?? q.kieuLuaChon ?? '').toLowerCase()
          const loai = String(q.LoaiCauHoi ?? q.loaiCauHoi ?? '').toLowerCase()
          const isMultiple = kieu === 'chon_nhieu' || kieu === 'nhieu_dap_an' || kieu === 'multiple' || (Array.isArray(rawAns) && rawAns.length > 1)
          const isEssay = loai === 'tu_luan' || loai === 'essay'

          return {
            id: q.MaCauHoi ?? q.maCauHoi ?? q.id,
            text: q.NoiDung ?? q.noiDung ?? '',
            type: isEssay ? 'essay' : (isMultiple ? 'multiple' : 'single'),
            options: parsedOptions
          }
        })
      }
    }
  } catch (e) {
    console.error('Lỗi tải thông tin chi tiết Quiz:', e)
  } finally {
    isLoading.value = false
  }
}

const isOptionCorrect = (optId, dapAn) => {
  if (dapAn === null || dapAn === undefined) return false
  if (Array.isArray(dapAn)) {
    return dapAn.some(a => String(a).trim().toUpperCase() === String(optId).trim().toUpperCase())
  }
  if (typeof dapAn === 'object') {
    return Object.values(dapAn).some(a => String(a).trim().toUpperCase() === String(optId).trim().toUpperCase())
  }
  return String(dapAn).trim().toUpperCase() === String(optId).trim().toUpperCase()
}

onMounted(() => {
  loadQuizData()
})

watch(() => props.content?.quizId || props.content?.maDeKiemTra, () => {
  loadQuizData()
})

const durationMinutes = computed(() => quizData.value?.duration ?? props.content?.duration ?? 15)
const passScore = computed(() => quizData.value?.passScore ?? 5)
const totalScore = computed(() => quizData.value?.totalScore ?? 10)
const completionRule = computed(() => quizData.value?.completionRule ?? 'Phải đạt')
const questions = computed(() => quizData.value?.questions ?? [])
</script>

<template>
  <div class="mb-8">
    <div class="flex items-start gap-3 mb-6">
      <div class="p-2 bg-green-50 text-green-600 rounded-lg shrink-0">
        <HelpCircle class="w-5 h-5" />
      </div>
      <div class="flex-1 min-w-0">
        <h3 class="text-lg font-bold text-slate-800">{{ quizData?.title || content.title }}</h3>
        <p v-if="content.description" class="text-sm text-slate-600 mt-1">{{ content.description }}</p>
      </div>
      <div v-if="content.status === 'draft'" class="shrink-0">
        <span class="text-[10px] font-medium px-2 py-1 rounded bg-amber-100 text-amber-700 border border-amber-200 uppercase tracking-wider">
          Bản nháp
        </span>
      </div>
    </div>

    <!-- Quiz Info Card -->
    <div class="bg-white border border-slate-200 rounded-xl overflow-hidden mb-6">
      <div v-if="isLoading" class="p-6 text-center text-sm text-slate-500 flex items-center justify-center gap-2">
        <Loader2 class="w-4 h-4 animate-spin text-blue-600" />
        <span>Đang nạp dữ liệu Quiz từ CSDL...</span>
      </div>

      <div v-else class="grid grid-cols-2 sm:grid-cols-4 divide-x divide-y sm:divide-y-0 divide-slate-100">
        <div class="p-4 text-center">
          <p class="text-xs font-medium text-slate-500 uppercase tracking-wider mb-1">Thời gian</p>
          <div class="flex items-center justify-center gap-1.5 text-slate-800 font-semibold">
            <Clock class="w-4 h-4 text-blue-500" />
            <span>{{ durationMinutes }} phút</span>
          </div>
        </div>
        <div class="p-4 text-center">
          <p class="text-xs font-medium text-slate-500 uppercase tracking-wider mb-1">Số câu hỏi</p>
          <div class="flex items-center justify-center gap-1.5 text-slate-800 font-semibold">
            <HelpCircle class="w-4 h-4 text-indigo-500" />
            <span>{{ questions.length }} câu</span>
          </div>
        </div>
        <div class="p-4 text-center">
          <p class="text-xs font-medium text-slate-500 uppercase tracking-wider mb-1">Điểm đạt</p>
          <div class="flex items-center justify-center gap-1.5 text-slate-800 font-semibold">
            <Target class="w-4 h-4 text-green-500" />
            <span>{{ passScore }}/{{ totalScore }}</span>
          </div>
        </div>
        <div class="p-4 text-center">
          <p class="text-xs font-medium text-slate-500 uppercase tracking-wider mb-1">Quy tắc</p>
          <div class="flex items-center justify-center gap-1.5 text-slate-800 font-semibold">
            <CheckCircle class="w-4 h-4 text-orange-500" />
            <span>{{ completionRule }}</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Council specific answers toggle -->
    <div class="flex items-center justify-between mb-4 bg-slate-50 p-3 rounded-lg border border-slate-200">
      <div class="flex items-center gap-2">
        <span class="text-xs font-bold px-2 py-1 bg-indigo-100 text-indigo-700 rounded uppercase">Quyền Hội Đồng</span>
        <span class="text-sm font-medium text-slate-600">Xem trước danh sách câu hỏi ({{ questions.length }} câu)</span>
      </div>
      <label class="flex items-center gap-2 text-sm text-slate-700 cursor-pointer select-none">
        <input 
          type="checkbox" 
          v-model="showAnswers"
          class="rounded border-slate-300 text-indigo-600 focus:ring-indigo-500 w-4 h-4 cursor-pointer"
        >
        <span class="font-medium">Hiển thị đáp án đúng</span>
      </label>
    </div>

    <!-- Questions Preview -->
    <div v-if="questions.length === 0" class="p-6 bg-slate-50 border border-slate-200 rounded-xl text-center text-sm text-slate-500">
      Quiz này chưa được xây dựng danh sách câu hỏi trong Ngân hàng đề.
    </div>

    <div v-else class="space-y-4">
      <div 
        v-for="(q, index) in questions"
        :key="q.id"
        class="bg-white border border-slate-200 rounded-xl p-5"
      >
        <h4 class="font-semibold text-slate-800 mb-4">
          <span class="text-slate-500 mr-2">Câu {{ index + 1 }}:</span>
          {{ q.text }}
          <span class="text-xs font-normal text-slate-400 ml-2">({{ q.type === 'single' ? 'Chọn 1 đáp án' : (q.type === 'multiple' ? 'Chọn nhiều đáp án' : 'Tự luận') }})</span>
        </h4>
        
        <div class="space-y-2">
          <div 
            v-for="opt in q.options" 
            :key="opt.id"
            class="flex items-start gap-3 p-3 rounded-lg border transition-colors"
            :class="[
              showAnswers && opt.isCorrect 
                ? 'bg-green-50 border-green-300 shadow-sm' 
                : 'bg-slate-50 border-slate-100'
            ]"
          >
            <div 
              class="w-5 h-5 rounded flex items-center justify-center text-xs font-bold shrink-0 mt-0.5"
              :class="[
                showAnswers && opt.isCorrect 
                  ? 'bg-green-600 text-white' 
                  : 'bg-white border border-slate-300 text-slate-500'
              ]"
            >
              {{ opt.id }}
            </div>
            <div class="text-sm flex-1" :class="showAnswers && opt.isCorrect ? 'text-green-900 font-semibold' : 'text-slate-700'">
              {{ opt.text }}
            </div>
            <span v-if="showAnswers && opt.isCorrect" class="inline-flex items-center gap-1 text-xs font-bold text-green-700 bg-green-100 px-2 py-0.5 rounded border border-green-200">
              <CheckCircle class="w-3.5 h-3.5" />
              Đáp án đúng
            </span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
