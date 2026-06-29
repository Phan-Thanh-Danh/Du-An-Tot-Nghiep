<script setup>
import { ref, computed, onMounted } from 'vue'
import { Award, Search, Users } from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import TableShell from '@/components/ui/TableShell.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import { rewardDisciplineMockService } from '@/mocks/rewardDisciplineMockService'
import { usePopupStore } from '@/stores/popup'

const popupStore = usePopupStore()
const campaigns = ref([])
const loading = ref(false)
const confirmAction = ref(null)
const searchQuery = ref('')
const filter = ref('all')
const selectedCampaign = ref(null)
const candidates = ref([])

const fetchCampaigns = async () => {
  loading.value = true
  try {
    const res = await rewardDisciplineMockService.getRewardCampaigns()
    campaigns.value = res.items || []
  } catch (err) {
    console.error(err)
  } finally {
    loading.value = false
  }
}

onMounted(() => fetchCampaigns())

const filteredCampaigns = computed(() => {
  let list = campaigns.value
  if (filter.value !== 'all') {
    list = list.filter(c => c.trangThai === filter.value)
  }
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(c => c.tenDot?.toLowerCase().includes(q) || c.hocKy?.toLowerCase().includes(q))
  }
  return list
})

const selectCampaign = (cmp) => {
  selectedCampaign.value = cmp
  // Mock top candidates
  candidates.value = [
    { id: 1, rank: 1, name: 'Nguyễn Văn A', rollNum: 'SE15001', class: 'SE1501', gpa: 9.8, status: cmp.trangThai === 'approved' || cmp.trangThai === 'completed' ? 'approved' : 'evaluating' },
    { id: 2, rank: 2, name: 'Trần Thị B', rollNum: 'SE15002', class: 'SE1502', gpa: 9.7, status: cmp.trangThai === 'approved' || cmp.trangThai === 'completed' ? 'approved' : 'evaluating' },
    { id: 3, rank: 3, name: 'Lê Văn C', rollNum: 'SE15003', class: 'SE1503', gpa: 9.5, status: cmp.trangThai === 'approved' || cmp.trangThai === 'completed' ? 'approved' : 'evaluating' },
  ]
}

const generateCertificates = () => {
  if (!selectedCampaign.value) return
  confirmAction.value = {
    title: 'Phát sinh bằng khen',
    message: `Bạn muốn tạo bằng khen (PDF) cho đợt "${selectedCampaign.value.tenDot}"? Thao tác này sẽ xử lý ${selectedCampaign.value.daDuyet} ứng viên.`,
    label: 'Bắt đầu tạo',
    variant: 'primary',
    run: async () => {
      confirmAction.value = null
      try {
        await rewardDisciplineMockService.generateCertificates(selectedCampaign.value.id)
        popupStore.success('Thành công', 'Đã xếp hàng đợi phát sinh bằng khen.')
        selectedCampaign.value.trangThai = 'completed'
      } catch (_e) {
        console.error(_e)
        popupStore.error('Lỗi', 'Có lỗi xảy ra khi tạo bằng khen.')
      }
    }
  }
}

const approveCampaign = () => {
  if (!selectedCampaign.value) return
  confirmAction.value = {
    title: 'Chốt danh sách khen thưởng',
    message: `Xác nhận chốt danh sách ứng viên cho đợt "${selectedCampaign.value.tenDot}"? Bạn sẽ không thể thêm ứng viên sau khi chốt.`,
    label: 'Chốt danh sách',
    variant: 'primary',
    run: () => {
      selectedCampaign.value.trangThai = 'approved'
      candidates.value.forEach(c => c.status = 'approved')
      confirmAction.value = null
      popupStore.success('Thành công', 'Đã chốt danh sách đợt khen thưởng.')
    }
  }
}
</script>

<template>
  <div class="sa-awards max-w-7xl mx-auto space-y-6">
    <GlassPanel variant="flat" density="compact">
      <div class="flex items-center gap-3 mb-2">
        <Award class="text-amber-500" :size="24" />
        <h1 class="text-2xl font-bold text-(--text-heading)">Quản lý Khen Thưởng</h1>
      </div>
      <p class="text-(--text-body)">Quản lý các đợt khen thưởng, xét duyệt ứng viên và cấp phát chứng nhận.</p>
    </GlassPanel>

    <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
      <GlassPanel variant="flat" density="compact" class="flex flex-col justify-center min-h-[90px] border-l-4 border-(--border-default)">
        <p class="text-sm font-medium text-(--text-muted) mb-1">Tổng đợt khen thưởng</p>
        <strong class="text-2xl text-(--text-heading)">{{ campaigns.length }}</strong>
      </GlassPanel>
      <GlassPanel variant="flat" density="compact" class="flex flex-col justify-center min-h-[90px] border-l-4 border-blue-500">
        <p class="text-sm font-medium text-(--text-muted) mb-1">Chờ xét duyệt</p>
        <strong class="text-2xl text-(--text-heading)">{{ campaigns.filter(c => c.trangThai === 'evaluating').length }}</strong>
      </GlassPanel>
      <GlassPanel variant="flat" density="compact" class="flex flex-col justify-center min-h-[90px] border-l-4 border-emerald-500">
        <p class="text-sm font-medium text-(--text-muted) mb-1">Đã chốt danh sách</p>
        <strong class="text-2xl text-(--text-heading)">{{ campaigns.filter(c => c.trangThai === 'approved' || c.trangThai === 'completed').length }}</strong>
      </GlassPanel>
      <GlassPanel variant="flat" density="compact" class="flex flex-col justify-center min-h-[90px] border-l-4 border-amber-500">
        <p class="text-sm font-medium text-(--text-muted) mb-1">Bằng khen lỗi</p>
        <strong class="text-2xl text-(--text-heading)">0</strong>
      </GlassPanel>
    </div>

    <GlassPanel variant="flat" class="p-0 overflow-hidden">
      <div class="p-4 border-b border-(--border-default) flex flex-wrap gap-4 items-center">
        <label class="flex items-center gap-2 bg-(--surface-input) px-3 h-10 rounded-lg border border-(--border-input) flex-1 min-w-[200px] focus-within:ring-2 focus-within:ring-(--border-focus) transition-shadow">
          <Search :size="16" class="text-(--text-muted)" />
          <input v-model="searchQuery" type="text" placeholder="Tìm theo tên đợt, học kỳ..." class="bg-transparent border-none outline-none w-full text-sm text-(--text-body)" />
        </label>
        <select v-model="filter" class="h-10 px-3 py-0 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm focus:ring-2 focus:ring-(--border-focus) outline-none transition-shadow min-w-[150px]">
          <option value="all">Tất cả trạng thái</option>
          <option value="evaluating">Đang xét duyệt</option>
          <option value="approved">Đã duyệt (Chờ cấp bằng)</option>
          <option value="completed">Đã hoàn tất</option>
        </select>
        <GlassButton variant="primary" class="h-10">Tạo đợt mới</GlassButton>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 min-h-[500px]">
        <div class="lg:col-span-2 border-r border-(--border-default) overflow-x-auto relative">
          <TableShell v-if="filteredCampaigns.length > 0">
            <table>
              <thead>
                <tr>
                  <th>Mã đợt</th>
                  <th>Tên đợt</th>
                  <th>Học kỳ</th>
                  <th>Trạng thái</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="cmp in filteredCampaigns" :key="cmp.id" 
                    @click="selectCampaign(cmp)"
                    class="cursor-pointer transition-colors"
                    :class="selectedCampaign?.id === cmp.id ? 'bg-(--surface-hover)' : 'hover:bg-(--surface-hover)'">
                  <td class="whitespace-nowrap font-mono text-sm text-(--text-muted)">{{ cmp.maDot }}</td>
                  <td class="font-medium max-w-[200px] truncate" :title="cmp.tenDot">{{ cmp.tenDot }}</td>
                  <td class="text-sm">{{ cmp.hocKy }}</td>
                  <td>
                    <GlassBadge v-if="cmp.trangThai === 'approved'" variant="warning" size="sm">Đã duyệt</GlassBadge>
                    <GlassBadge v-else-if="cmp.trangThai === 'evaluating'" variant="info" size="sm">Đang xét</GlassBadge>
                    <GlassBadge v-else variant="success" size="sm">Hoàn tất</GlassBadge>
                  </td>
                </tr>
              </tbody>
            </table>
          </TableShell>
          <div v-else class="p-8">
            <EmptyState title="Không tìm thấy đợt khen thưởng" description="Hãy điều chỉnh bộ lọc hoặc tạo đợt mới." />
          </div>
        </div>

        <div class="lg:col-span-1 bg-(--surface-card)">
          <div v-if="!selectedCampaign" class="h-full flex items-center justify-center p-6 text-center text-(--text-muted) text-sm">
            Chọn một đợt khen thưởng bên trái để xem chi tiết
          </div>
          <div v-else class="flex flex-col h-full">
            <div class="p-5 border-b border-(--border-default)">
              <h3 class="font-bold text-lg text-(--text-heading) leading-tight mb-2">{{ selectedCampaign.tenDot }}</h3>
              <div class="flex items-center gap-2 mb-4">
                <GlassBadge v-if="selectedCampaign.trangThai === 'approved'" variant="warning" size="sm">Đã chốt danh sách</GlassBadge>
                <GlassBadge v-else-if="selectedCampaign.trangThai === 'evaluating'" variant="info" size="sm">Đang xét duyệt</GlassBadge>
                <GlassBadge v-else variant="success" size="sm">Hoàn tất bằng khen</GlassBadge>
                <span class="text-xs text-(--text-muted) font-mono">{{ selectedCampaign.maDot }}</span>
              </div>
              <div class="space-y-2 text-sm">
                <div class="flex justify-between"><span class="text-(--text-muted)">Học kỳ</span><span class="font-medium text-(--text-body)">{{ selectedCampaign.hocKy }}</span></div>
                <div class="flex justify-between"><span class="text-(--text-muted)">Đơn vị</span><span class="font-medium text-(--text-body)">{{ selectedCampaign.donVi || 'Toàn trường' }}</span></div>
              </div>
            </div>

            <div class="p-5 border-b border-(--border-default)">
              <h4 class="font-semibold text-sm text-(--text-heading) mb-3 flex items-center gap-2"><Users :size="16"/> Số liệu ứng viên</h4>
              <div class="grid grid-cols-2 gap-3 mb-4">
                <div class="bg-(--surface-input) p-3 rounded-lg border border-(--border-default) text-center">
                  <div class="text-2xl font-bold text-(--text-heading)">{{ selectedCampaign.tongUngVien }}</div>
                  <div class="text-xs text-(--text-muted)">Tổng ứng viên</div>
                </div>
                <div class="bg-(--surface-input) p-3 rounded-lg border border-(--border-default) text-center">
                  <div class="text-2xl font-bold text-emerald-600 dark:text-emerald-400">{{ selectedCampaign.daDuyet }}</div>
                  <div class="text-xs text-(--text-muted)">Đã duyệt</div>
                </div>
              </div>
              <div class="text-sm font-medium text-(--text-body) mb-2">Ứng viên nổi bật (Top 3)</div>
              <div class="space-y-2">
                <div v-for="c in candidates" :key="c.id" class="flex items-center justify-between bg-(--surface-modal) p-2 rounded border border-(--border-default) text-sm">
                  <div class="flex items-center gap-2">
                    <span class="flex items-center justify-center w-5 h-5 rounded-full bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-400 font-bold text-[10px]">{{ c.rank }}</span>
                    <div>
                      <div class="font-medium text-(--text-heading) line-clamp-1">{{ c.name }}</div>
                      <div class="text-[10px] text-(--text-muted)">{{ c.rollNum }} • {{ c.class }}</div>
                    </div>
                  </div>
                  <div class="text-right">
                    <div class="font-bold text-(--lg-primary)">{{ c.gpa }}</div>
                    <div class="text-[10px]" :class="c.status === 'approved' ? 'text-emerald-500' : 'text-blue-500'">{{ c.status === 'approved' ? 'Đã duyệt' : 'Đang xét' }}</div>
                  </div>
                </div>
              </div>
              <GlassButton variant="ghost" size="sm" class="w-full mt-3 text-sm justify-center">Xem toàn bộ danh sách</GlassButton>
            </div>

            <div class="p-5 mt-auto bg-(--surface-modal)">
              <div class="flex flex-col gap-2">
                <GlassButton v-if="selectedCampaign.trangThai === 'evaluating'" variant="primary" class="w-full justify-center" @click="approveCampaign">
                  Chốt danh sách khen thưởng
                </GlassButton>
                <GlassButton v-if="selectedCampaign.trangThai === 'approved'" variant="primary" class="w-full justify-center bg-amber-600 hover:bg-amber-700 text-white border-none" @click="generateCertificates">
                  Phát sinh bằng khen (PDF)
                </GlassButton>
                <GlassButton v-if="selectedCampaign.trangThai === 'completed'" variant="secondary" class="w-full justify-center opacity-70" disabled>
                  Đợt khen thưởng đã hoàn tất
                </GlassButton>
                <GlassButton v-if="selectedCampaign.trangThai === 'approved' || selectedCampaign.trangThai === 'completed'" variant="ghost" class="w-full justify-center">
                  Gửi thông báo cho sinh viên
                </GlassButton>
              </div>
            </div>
          </div>
        </div>
      </div>
    </GlassPanel>

    <ConfirmActionDialog
      v-if="confirmAction"
      :show="true"
      :title="confirmAction.title"
      :message="confirmAction.message"
      :confirmLabel="confirmAction.label"
      :variant="confirmAction.variant"
      @confirm="confirmAction.run"
      @cancel="confirmAction = null"
    />
  </div>
</template>
