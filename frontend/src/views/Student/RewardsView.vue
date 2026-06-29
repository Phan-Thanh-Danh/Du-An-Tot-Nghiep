<script setup>
import { ref, computed, onMounted } from 'vue'
import { Trophy, Download, Star } from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'
import { rewardDisciplineMockService } from '@/mocks/rewardDisciplineMockService'
import { formatDate } from '@/utils/dateFormat'

const rewards = ref([])
const loading = ref(false)
const selectedReward = ref(null)
const filter = ref('all') // all, pending, generated
const searchQuery = ref('')

const fetchRewards = async () => {
  loading.value = true
  try {
    const res = await rewardDisciplineMockService.getMyRewards()
    rewards.value = res.items || []
  } catch (error) {
    console.error(error)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchRewards()
})

const filteredRewards = computed(() => {
  let list = rewards.value
  if (filter.value === 'generated') list = list.filter(r => r.certificateStatus === 'generated')
  if (filter.value === 'pending') list = list.filter(r => r.certificateStatus !== 'generated')

  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(r => r.tieuDe?.toLowerCase().includes(q) || r.moTa?.toLowerCase().includes(q))
  }
  return list
})

const totalGenerated = computed(() => rewards.value.filter(r => r.certificateStatus === 'generated').length)

const selectReward = (r) => {
  selectedReward.value = r
}
</script>

<template>
  <div class="student-rewards max-w-7xl mx-auto space-y-6">
    <div class="hero-panel bg-(--surface-card) rounded-xl p-8 border border-amber-300/50 flex items-center justify-between gap-6 flex-wrap relative overflow-hidden">
      <!-- Decorative background icon -->
      <div class="absolute -right-8 -top-8 text-amber-500/10 transform rotate-12 pointer-events-none">
        <Trophy :size="180" />
      </div>
      <!-- Subtle gradient overlay -->
      <div class="absolute inset-0 bg-gradient-to-br from-amber-500/5 to-orange-500/5 pointer-events-none"></div>
      
      <div class="flex items-center gap-5 relative z-10">
        <div class="w-16 h-16 rounded-2xl bg-gradient-to-br from-amber-400 to-orange-500 text-white shadow-lg shadow-amber-500/30 flex items-center justify-center transform -rotate-6">
          <Trophy :size="32" />
        </div>
        <div>
          <h1 class="text-3xl font-extrabold text-(--text-heading) tracking-tight mb-1">Thành tích của tôi</h1>
          <p class="text-(--text-muted) font-medium">Ghi nhận những nỗ lực và sự xuất sắc trong quá trình học tập.</p>
        </div>
      </div>
      <div class="flex gap-4">
        <div class="text-center px-4 relative z-10">
          <div class="text-3xl font-black text-amber-500">{{ rewards.length }}</div>
          <div class="text-sm font-semibold text-(--text-muted) uppercase tracking-wider mt-1">Tổng vinh danh</div>
        </div>
        <div class="w-px bg-(--border-default) relative z-10"></div>
        <div class="text-center px-4 relative z-10">
          <div class="text-3xl font-black text-orange-500">{{ totalGenerated }}</div>
          <div class="text-sm font-semibold text-(--text-muted) uppercase tracking-wider mt-1">Bằng khen PDF</div>
        </div>
      </div>
    </div>

    <div class="flex gap-4 items-center flex-wrap">
      <label class="flex-1 min-w-[200px] flex items-center gap-2 bg-(--surface-input) px-3 h-10 rounded border border-(--border-input)">
        <input v-model="searchQuery" type="text" placeholder="Tìm theo tên thành tích..." class="bg-transparent border-none outline-none w-full" />
      </label>
      <select v-model="filter" class="lg-control h-10 min-w-[150px]">
        <option value="all">Tất cả chứng nhận</option>
        <option value="generated">Đã cấp bản mềm (PDF)</option>
        <option value="pending">Đang chờ cấp</option>
      </select>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <div class="lg:col-span-2 space-y-4">
        <LoadingSkeleton v-if="loading" :lines="6" />
        <EmptyState v-else-if="filteredRewards.length === 0" title="Chưa có khen thưởng nào" description="Bạn chưa có khen thưởng hoặc không tìm thấy kết quả phù hợp." />

        <div v-else class="grid sm:grid-cols-2 gap-4 min-h-[450px] content-start">
          <GlassPanel
            v-for="r in filteredRewards" :key="r.id"
            variant="flat"
            class="flex flex-col cursor-pointer transition-all duration-300 hover:-translate-y-1 hover:shadow-lg hover:shadow-amber-500/10 border relative overflow-hidden"
            :class="[
              selectedReward?.id === r.id 
                ? 'ring-2 ring-amber-500 border-amber-500/50' 
                : 'border-(--border-default)'
            ]"
            @click="selectReward(r)"
          >
            <div v-if="selectedReward?.id === r.id" class="absolute inset-0 bg-gradient-to-b from-amber-500/10 to-transparent pointer-events-none"></div>
            <div class="relative z-10 flex flex-col h-full">
            <div class="flex justify-between items-start mb-2 gap-2">
              <GlassBadge :variant="r.loaiKhenThuong === 'Học tập' ? 'info' : 'success'" size="sm" class="shrink-0">{{ r.loaiKhenThuong }}</GlassBadge>
              <GlassBadge v-if="r.certificateStatus === 'generated'" variant="warning" size="sm" class="shrink-0">PDF Sẵn sàng</GlassBadge>
            </div>
            <h3 class="font-bold text-(--text-heading) text-lg line-clamp-2 mb-2 flex-1">{{ r.tieuDe }}</h3>
            <div class="text-xs text-(--text-muted) border-t border-(--border-default) pt-3 flex justify-between items-center mt-auto">
              <span>Cấp ngày: {{ formatDate(r.ngayCap) }}</span>
              <span>Học kỳ: {{ r.hocKy }}</span>
            </div>
            </div>
          </GlassPanel>
        </div>
      </div>

      <div class="lg:col-span-1">
        <div class="sticky top-6 flex flex-col min-h-[450px] lg:w-[380px] xl:w-[420px] bg-(--surface-card) rounded-2xl border border-(--border-default) shadow-lg shadow-amber-500/5 overflow-hidden mx-auto lg:mx-0 w-full">
          <div class="h-2 bg-gradient-to-r from-amber-400 via-orange-400 to-amber-500"></div>
          <div class="p-6 flex flex-col flex-1">
          <EmptyState v-if="!selectedReward" title="Chọn vinh danh" description="Nhấp vào thẻ bên trái để xem chi tiết Bằng khen." />
          <div v-else class="space-y-6 flex flex-col h-full relative">
            <div class="absolute top-0 right-0 text-amber-500/10">
              <Star :size="80" fill="currentColor" />
            </div>
            <div class="relative z-10">
              <div class="inline-flex items-center justify-center px-3 py-1 rounded-full bg-amber-500/10 text-amber-600 dark:text-amber-400 text-xs font-bold uppercase tracking-wider mb-3 border border-amber-500/20">
                {{ selectedReward.loaiKhenThuong }}
              </div>
              <h3 class="text-2xl font-black text-transparent bg-clip-text bg-gradient-to-br from-amber-600 to-orange-600 dark:from-amber-400 dark:to-orange-400 leading-tight mb-4">{{ selectedReward.tieuDe }}</h3>
              <div class="text-sm text-(--text-body) space-y-1 mb-4">
                <p><strong>Loại:</strong> {{ selectedReward.loaiKhenThuong }}</p>
                <p><strong>Xếp hạng:</strong> {{ selectedReward.xepHang || 'Không xếp hạng' }}</p>
                <p><strong>Học kỳ:</strong> {{ selectedReward.hocKy }}</p>
              </div>
              <div class="mt-4 p-4 rounded-xl bg-(--surface-input) border border-(--border-default) relative">
                <div class="absolute -left-2 -top-2 text-3xl text-amber-500/40">"</div>
                <p class="text-(--text-body) text-sm italic relative z-10 px-2 leading-relaxed">{{ selectedReward.moTa }}</p>
                <div class="absolute -right-2 -bottom-4 text-3xl text-amber-500/40 rotate-180">"</div>
              </div>
            </div>

            <div class="flex-1 relative z-10 mt-2">
              <div class="text-xs font-bold uppercase tracking-wider text-(--text-muted) mb-4 flex items-center gap-2">
                <div class="h-px bg-(--border-default) flex-1"></div>
                Tiến trình cấp phát
                <div class="h-px bg-(--border-default) flex-1"></div>
              </div>
              <div class="space-y-5 relative before:absolute before:inset-0 before:ml-[5px] before:-translate-x-px md:before:mx-auto md:before:translate-x-0 before:h-full before:w-0.5 before:bg-(--border-default)">
                <div v-for="(tl, i) in selectedReward.timeline" :key="i" class="relative flex items-center justify-between md:justify-normal md:odd:flex-row-reverse group is-active">
                  <div class="flex items-center justify-center w-3 h-3 rounded-full border-2 border-white dark:border-slate-900 bg-amber-500 shrink-0 md:order-1 md:group-odd:-translate-x-1/2 md:group-even:translate-x-1/2 shadow shadow-amber-500/50 z-10"></div>
                  <div class="w-[calc(100%-1.5rem)] md:w-[calc(50%-1.5rem)] pl-3 md:pl-0 md:group-odd:pr-3 md:group-even:pl-3">
                    <div class="font-bold text-(--text-heading) text-sm">{{ tl.tieuDe }}</div>
                    <div class="text-(--text-muted) text-xs">{{ formatDate(tl.thoiGian, 'DD/MM/YYYY HH:mm') }}</div>
                  </div>
                </div>
              </div>
            </div>

            <div class="pt-4 flex gap-2 relative z-10 mt-auto">
              <GlassButton v-if="selectedReward.certificateStatus === 'generated'" variant="primary" class="w-full justify-center !bg-amber-600 hover:!bg-amber-700 !text-white !border-none">
                <template #leading><Download :size="18" /></template>
                Tải bản PDF vinh danh
              </GlassButton>
              <GlassButton v-else variant="secondary" class="w-full justify-center opacity-70" disabled>
                Bản mềm chưa sẵn sàng
              </GlassButton>
            </div>
          </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
