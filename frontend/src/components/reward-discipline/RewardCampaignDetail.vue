<script setup>
import { ref, onMounted } from 'vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import RewardStatusBadge from './RewardStatusBadge.vue'
import RewardCandidateTable from './RewardCandidateTable.vue'
import { ArrowLeft, Users, CheckCircle2, XCircle } from 'lucide-vue-next'
import { rewardDisciplineMockService } from '@/mocks/rewardDisciplineMockService'

const props = defineProps({
  campaign: { type: Object, required: true }
})
const emit = defineEmits(['back'])

const candidates = ref([])
const loading = ref(false)

const loadCandidates = async () => {
  loading.value = true
  try {
    const res = await rewardDisciplineMockService.getRewardCandidates(props.campaign.id)
    candidates.value = res.items
  } finally {
    loading.value = false
  }
}

onMounted(() => loadCandidates())
</script>

<template>
  <div class="space-y-6">
    <div class="flex items-center justify-between">
      <div class="flex items-center gap-4">
        <button @click="emit('back')" class="p-2 hover:bg-[var(--surface-hover)] rounded-full text-[var(--text-muted)]">
          <ArrowLeft class="w-5 h-5" />
        </button>
        <div>
          <h2 class="text-xl font-bold text-[var(--text-heading)] flex items-center gap-3">
            {{ campaign.tenDot }}
            <RewardStatusBadge :status="campaign.trangThai" />
          </h2>
          <p class="text-sm text-[var(--text-muted)] mt-1">Học kỳ: {{ campaign.hocKy }} | Đơn vị: {{ campaign.donVi }}</p>
        </div>
      </div>
      <div>
        <GlassButton variant="primary">Sinh bằng khen (Mock)</GlassButton>
      </div>
    </div>

    <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
      <GlassPanel padding="normal" class="text-center">
        <div class="text-sm text-[var(--text-muted)] mb-1">Tổng UV</div>
        <div class="text-2xl font-bold text-[var(--lg-primary)]">{{ campaign.tongUngVien }}</div>
      </GlassPanel>
      <GlassPanel padding="normal" class="text-center">
        <div class="text-sm text-[var(--text-muted)] mb-1">Đã duyệt</div>
        <div class="text-2xl font-bold text-[var(--color-success-text)]">{{ campaign.daDuyet }}</div>
      </GlassPanel>
      <GlassPanel padding="normal" class="text-center">
        <div class="text-sm text-[var(--text-muted)] mb-1">Bị loại</div>
        <div class="text-2xl font-bold text-[var(--color-danger-text)]">{{ campaign.biLoai }}</div>
      </GlassPanel>
      <GlassPanel padding="normal" class="text-center">
        <div class="text-sm text-[var(--text-muted)] mb-1">Bằng khen đã sinh</div>
        <div class="text-2xl font-bold text-amber-600">{{ campaign.certificateGenerated }}</div>
      </GlassPanel>
    </div>

    <div>
      <h3 class="text-lg font-semibold text-[var(--text-heading)] mb-4 flex items-center gap-2">
        <Users class="w-5 h-5" /> Danh sách ứng viên
      </h3>
      <div v-if="loading" class="text-[var(--text-muted)] py-4">Đang tải dữ liệu...</div>
      <RewardCandidateTable v-else :candidates="candidates" />
    </div>
  </div>
</template>
