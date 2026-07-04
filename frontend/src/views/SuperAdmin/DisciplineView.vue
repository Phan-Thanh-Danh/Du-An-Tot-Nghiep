<script setup>
import { ref, computed, onMounted } from 'vue'
import { ShieldAlert, Search, Scale, FileText, UserX, CheckCircle } from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import TableShell from '@/components/ui/TableShell.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import { rewardDisciplineApi } from '@/services/rewardDisciplineApi'
import { unwrapApiData } from '@/services/apiClient'
import { formatDate } from '@/utils/dateFormat'
import { usePopupStore } from '@/stores/popup'

const popupStore = usePopupStore()
const records = ref([])
const appeals = ref([])
const loading = ref(false)
const confirmAction = ref(null)
const currentTab = ref('records') // records, appeals
const searchQuery = ref('')
const filterStatus = ref('all')
const filterSeverity = ref('all')

const selectedRecord = ref(null)
const selectedAppeal = ref(null)
const appealResolution = ref({ status: 'resolved', reason: '' })

const normalizeRecordStatus = (status) => ['dang_hieu_luc', 'active'].includes(status) ? 'active' : 'expired'
const normalizeAppealStatus = (status) => {
  if (['chap_nhan', 'resolved'].includes(status)) return 'resolved'
  if (['tu_choi', 'rejected'].includes(status)) return 'rejected'
  return 'pending'
}

const mapRecord = (item) => ({
  id: item.maHoSoKyLuat ?? item.MaHoSoKyLuat,
  studentName: item.hoTenHocSinh ?? item.HoTenHocSinh ?? '',
  studentRollNumber: item.mssv ?? item.Mssv ?? '',
  studentClass: item.tenDonVi ?? item.TenDonVi ?? '',
  tieuDe: item.tieuDe ?? item.TieuDe ?? 'Hồ sơ kỷ luật',
  mucDoKyLuat: item.mucDoKyLuat ?? item.MucDoKyLuat ?? '',
  hinhThucXuLy: item.hinhThucXuLy ?? item.HinhThucXuLy ?? '',
  trangThai: normalizeRecordStatus(item.trangThai ?? item.TrangThai),
  ngayViPham: item.ngayViPham ?? item.NgayViPham,
  moTaCongKhai: item.moTaCongKhai ?? item.MoTaCongKhai,
  moTaNoiBo: item.moTaNoiBo ?? item.MoTaNoiBo,
})

const mapAppeal = (item) => ({
  id: item.maKhieuNaiKyLuat ?? item.MaKhieuNaiKyLuat,
  maHoSo: item.maHoSoKyLuat ? `KL-${item.maHoSoKyLuat}` : `KL-${item.MaHoSoKyLuat}`,
  studentName: item.tenHocSinh ?? item.TenHocSinh ?? '',
  studentRollNumber: item.mssv ?? item.Mssv ?? '',
  trangThai: normalizeAppealStatus(item.trangThai ?? item.TrangThai),
  ngayKhieuNai: item.ngayTao ?? item.NgayTao,
  lyDoKhieuNai: item.lyDoKhieuNai ?? item.LyDoKhieuNai ?? 'Xem chi tiết khiếu nại trên backend.',
  ghiChuGiaiQuyet: item.ghiChuXuLy ?? item.GhiChuXuLy ?? item.lyDoXuLy ?? item.LyDoXuLy,
})

const fetchAll = async () => {
  loading.value = true
  try {
    const [recRes, appRes] = await Promise.all([
      rewardDisciplineApi.getDisciplineRecords({ pageIndex: 1, pageSize: 50 }),
      rewardDisciplineApi.getDisciplineAppeals({ pageIndex: 1, pageSize: 50 })
    ])
    const recData = unwrapApiData(recRes)
    const appData = unwrapApiData(appRes)
    records.value = (recData?.items ?? recData?.Items ?? []).map(mapRecord)
    appeals.value = (appData?.items ?? appData?.Items ?? []).map(mapAppeal)
  } catch (err) {
    console.error(err)
    records.value = []
    appeals.value = []
    popupStore.error('Không thể tải dữ liệu', err?.message || 'Không thể tải hồ sơ kỷ luật.')
  } finally {
    loading.value = false
  }
}

onMounted(() => fetchAll())

const filteredRecords = computed(() => {
  let list = records.value
  if (filterStatus.value !== 'all') {
    list = list.filter(r => r.trangThai === filterStatus.value)
  }
  if (filterSeverity.value !== 'all') {
    list = list.filter(r => r.mucDoKyLuat === filterSeverity.value)
  }
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(r => r.studentName?.toLowerCase().includes(q) || r.studentRollNumber?.toLowerCase().includes(q) || r.tieuDe?.toLowerCase().includes(q))
  }
  return list
})

const filteredAppeals = computed(() => {
  let list = appeals.value
  if (filterStatus.value !== 'all') {
    list = list.filter(a => a.trangThai === filterStatus.value)
  }
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(a => a.studentName?.toLowerCase().includes(q) || a.maHoSo?.toLowerCase().includes(q))
  }
  return list
})

const removeEffect = () => {
  if (!selectedRecord.value) return
  confirmAction.value = {
    title: 'Gỡ hiệu lực kỷ luật?',
    message: `Bạn muốn gỡ hiệu lực sớm của hồ sơ "${selectedRecord.value.tieuDe}" cho sinh viên ${selectedRecord.value.studentName}?`,
    label: 'Gỡ hiệu lực',
    variant: 'danger',
    run: async () => {
      try {
        await rewardDisciplineApi.removeDisciplineEffect(selectedRecord.value.id, {
          reason: 'Gỡ hiệu lực sớm theo thao tác quản trị.',
          removalNote: 'Thao tác từ giao diện SuperAdmin.',
        })
        selectedRecord.value.trangThai = 'expired'
        confirmAction.value = null
        popupStore.success('Thành công', 'Đã gỡ hiệu lực kỷ luật.')
      } catch (err) {
        popupStore.error('Lỗi', err?.message || 'Không thể gỡ hiệu lực kỷ luật.')
      }
    }
  }
}

const resolveAppeal = () => {
  if (!selectedAppeal.value) return
  if (!appealResolution.value.reason.trim()) {
    popupStore.error('Lỗi', 'Vui lòng nhập lý do giải quyết.')
    return
  }
  const isAccepting = appealResolution.value.status === 'resolved'
  confirmAction.value = {
    title: isAccepting ? 'Chấp nhận khiếu nại?' : 'Từ chối khiếu nại?',
    message: isAccepting ? `Chấp nhận khiếu nại của ${selectedAppeal.value.studentName} và gỡ kỷ luật liên quan?` : `Từ chối khiếu nại của ${selectedAppeal.value.studentName}?`,
    label: isAccepting ? 'Chấp nhận' : 'Từ chối',
    variant: isAccepting ? 'primary' : 'danger',
    run: async () => {
      try {
        await rewardDisciplineApi.resolveDisciplineAppeal(selectedAppeal.value.id, {
          decision: isAccepting ? 'chap_nhan' : 'tu_choi',
          reason: appealResolution.value.reason,
          resolutionNote: appealResolution.value.reason,
          removeEffect: isAccepting,
        })
        selectedAppeal.value.trangThai = appealResolution.value.status
        selectedAppeal.value.ghiChuGiaiQuyet = appealResolution.value.reason
        confirmAction.value = null
        popupStore.success('Thành công', 'Đã xử lý khiếu nại.')
        appealResolution.value.reason = ''
      } catch (err) {
        popupStore.error('Lỗi', err?.message || 'Không thể xử lý khiếu nại.')
      }
    }
  }
}
</script>

<template>
  <div class="sa-discipline max-w-7xl mx-auto space-y-6">
    <GlassPanel variant="flat" density="compact">
      <div class="flex items-center gap-3 mb-2">
        <ShieldAlert class="text-(--text-muted)" :size="24" />
        <h1 class="text-2xl font-bold text-(--text-heading)">Quản lý Kỷ luật</h1>
      </div>
      <p class="text-(--text-body)">Theo dõi hồ sơ vi phạm, ra quyết định và xử lý khiếu nại sinh viên một cách bảo mật.</p>
    </GlassPanel>

    <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
      <GlassPanel variant="flat" density="compact" class="flex flex-col justify-center min-h-[90px] border-l-4 border-[var(--color-danger-bg, #ef4444)]">
        <p class="text-sm font-medium text-(--text-muted) mb-1">Đang hiệu lực</p>
        <strong class="text-2xl text-(--text-heading)">{{ records.filter(r => r.trangThai === 'active').length }}</strong>
      </GlassPanel>
      <GlassPanel variant="flat" density="compact" class="flex flex-col justify-center min-h-[90px] border-l-4 border-amber-500">
        <p class="text-sm font-medium text-(--text-muted) mb-1">Chờ duyệt hồ sơ</p>
        <strong class="text-2xl text-(--text-heading)">0</strong>
      </GlassPanel>
      <GlassPanel variant="flat" density="compact" class="flex flex-col justify-center min-h-[90px] border-l-4 border-blue-500">
        <p class="text-sm font-medium text-(--text-muted) mb-1">Khiếu nại chờ xử lý</p>
        <strong class="text-2xl text-(--text-heading)">{{ appeals.filter(a => a.trangThai === 'pending').length }}</strong>
      </GlassPanel>
      <GlassPanel variant="flat" density="compact" class="flex flex-col justify-center min-h-[90px] border-l-4 border-(--border-default)">
        <p class="text-sm font-medium text-(--text-muted) mb-1">Đã gỡ / Hết hiệu lực</p>
        <strong class="text-2xl text-(--text-heading)">{{ records.filter(r => r.trangThai !== 'active').length }}</strong>
      </GlassPanel>
    </div>

    <GlassPanel variant="flat" class="p-0 overflow-hidden">
      <div class="p-4 border-b border-(--border-default) flex flex-wrap gap-4 items-center">
        <div class="flex bg-(--surface-modal) rounded-lg p-1 shrink-0 h-10 border border-(--border-default)">
          <button class="px-4 h-full rounded-md text-sm transition-colors flex items-center"
                  :class="currentTab === 'records' ? 'bg-(--surface-card) shadow-sm font-medium text-(--text-heading)' : 'text-(--text-muted) hover:text-(--text-body)'"
                  @click="currentTab = 'records'; selectedAppeal = null">Hồ sơ vi phạm</button>
          <button class="px-4 h-full rounded-md text-sm transition-colors flex items-center"
                  :class="currentTab === 'appeals' ? 'bg-(--surface-card) shadow-sm font-medium text-(--text-heading)' : 'text-(--text-muted) hover:text-(--text-body)'"
                  @click="currentTab = 'appeals'; selectedRecord = null">Đơn khiếu nại</button>
        </div>
        
        <label class="flex items-center gap-2 bg-(--surface-input) px-3 h-10 rounded-lg border border-(--border-input) flex-1 min-w-[200px] focus-within:ring-2 focus-within:ring-(--border-focus) transition-shadow">
          <Search :size="16" class="text-(--text-muted)" />
          <input v-model="searchQuery" type="text" placeholder="Tìm theo sinh viên, tiêu đề..." class="bg-transparent border-none outline-none w-full text-sm text-(--text-body)" />
        </label>
        
        <select v-if="currentTab === 'records'" v-model="filterStatus" class="h-10 px-3 py-0 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm focus:ring-2 focus:ring-(--border-focus) outline-none min-w-[130px]">
          <option value="all">Tất cả TT</option>
          <option value="active">Đang hiệu lực</option>
          <option value="expired">Đã gỡ/Hết hạn</option>
        </select>
        
        <select v-if="currentTab === 'records'" v-model="filterSeverity" class="h-10 px-3 py-0 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm focus:ring-2 focus:ring-(--border-focus) outline-none min-w-[130px]">
          <option value="all">Tất cả mức độ</option>
          <option value="Khiển trách">Khiển trách</option>
          <option value="Cảnh cáo">Cảnh cáo</option>
          <option value="Đình chỉ">Đình chỉ</option>
        </select>
        
        <select v-if="currentTab === 'appeals'" v-model="filterStatus" class="h-10 px-3 py-0 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm focus:ring-2 focus:ring-(--border-focus) outline-none min-w-[130px]">
          <option value="all">Tất cả TT</option>
          <option value="pending">Chờ xử lý</option>
          <option value="resolved">Đã chấp nhận</option>
          <option value="rejected">Bị từ chối</option>
        </select>

        <GlassButton v-if="currentTab === 'records'" variant="primary" class="h-10">Lập hồ sơ mới</GlassButton>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 min-h-[500px]">
        <!-- LIST -->
        <div class="lg:col-span-2 border-r border-(--border-default) overflow-x-auto relative">
          <!-- Records Table -->
          <TableShell v-if="currentTab === 'records'">
            <table>
              <thead>
                <tr>
                  <th>Sinh viên</th>
                  <th>Tiêu đề / Mức độ</th>
                  <th>Hình thức</th>
                  <th>Trạng thái</th>
                </tr>
              </thead>
              <tbody>
                <tr v-if="loading"><td colspan="4" class="text-center py-4">Đang tải...</td></tr>
                <tr v-else-if="filteredRecords.length === 0"><td colspan="4" class="text-center py-4 text-(--text-muted)">Không có dữ liệu.</td></tr>
                <tr v-for="r in filteredRecords" :key="r.id"
                    class="cursor-pointer transition-colors"
                    :class="selectedRecord?.id === r.id ? 'bg-(--surface-hover)' : 'hover:bg-(--surface-hover)'"
                    @click="selectedRecord = r">
                  <td>
                    <div class="font-medium text-sm">{{ r.studentName }}</div>
                    <div class="text-xs text-(--text-muted)">{{ r.studentRollNumber }}</div>
                  </td>
                  <td>
                    <div class="text-sm font-medium line-clamp-1" :title="r.tieuDe">{{ r.tieuDe }}</div>
                    <div class="text-xs text-(--text-muted)">{{ r.mucDoKyLuat }}</div>
                  </td>
                  <td class="text-sm">{{ r.hinhThucXuLy }}</td>
                  <td>
                    <GlassBadge :variant="r.trangThai === 'active' ? 'danger' : 'neutral'" size="sm">
                      {{ r.trangThai === 'active' ? 'Đang hiệu lực' : 'Đã hết/Gỡ' }}
                    </GlassBadge>
                  </td>
                </tr>
              </tbody>
            </table>
          </TableShell>

          <!-- Appeals Table -->
          <TableShell v-else>
            <table>
              <thead>
                <tr>
                  <th>Sinh viên</th>
                  <th>Mã hồ sơ</th>
                  <th>Ngày gửi</th>
                  <th>Trạng thái</th>
                </tr>
              </thead>
              <tbody>
                <tr v-if="loading"><td colspan="4" class="text-center py-4">Đang tải...</td></tr>
                <tr v-else-if="filteredAppeals.length === 0"><td colspan="4" class="text-center py-4 text-(--text-muted)">Không có dữ liệu.</td></tr>
                <tr v-for="a in filteredAppeals" :key="a.id"
                    class="cursor-pointer transition-colors"
                    :class="selectedAppeal?.id === a.id ? 'bg-(--surface-hover)' : 'hover:bg-(--surface-hover)'"
                    @click="selectedAppeal = a">
                  <td>
                    <div class="font-medium text-sm">{{ a.studentName }}</div>
                    <div class="text-xs text-(--text-muted)">{{ a.studentRollNumber }}</div>
                  </td>
                  <td class="text-sm font-mono text-(--text-muted)">{{ a.maHoSo }}</td>
                  <td class="text-sm">{{ formatDate(a.ngayKhieuNai) }}</td>
                  <td>
                    <GlassBadge v-if="a.trangThai === 'pending'" variant="warning" size="sm">Chờ xử lý</GlassBadge>
                    <GlassBadge v-else-if="a.trangThai === 'resolved'" variant="success" size="sm">Chấp nhận</GlassBadge>
                    <GlassBadge v-else variant="danger" size="sm">Từ chối</GlassBadge>
                  </td>
                </tr>
              </tbody>
            </table>
          </TableShell>
        </div>

        <!-- DETAILS PANEL -->
        <div class="lg:col-span-1 bg-(--surface-card)">
          <!-- Empty -->
          <div v-if="(currentTab === 'records' && !selectedRecord) || (currentTab === 'appeals' && !selectedAppeal)" 
               class="h-full flex items-center justify-center p-6 text-center text-(--text-muted) text-sm">
            Bấm vào danh sách bên trái để xem chi tiết
          </div>

          <!-- Record Details -->
          <div v-if="currentTab === 'records' && selectedRecord" class="flex flex-col h-full">
            <div class="p-5 border-b border-(--border-default) relative overflow-hidden">
              <div v-if="selectedRecord.trangThai === 'active'" class="absolute top-0 left-0 w-1 h-full bg-(--color-danger-bg)"></div>
              <h3 class="font-bold text-lg text-(--text-heading) leading-tight mb-3">{{ selectedRecord.tieuDe }}</h3>
              
              <div class="flex items-center gap-3 bg-(--surface-modal) p-3 rounded-lg border border-(--border-default)">
                <div class="w-10 h-10 rounded-full bg-(--surface-input) flex items-center justify-center text-(--text-muted) shrink-0">
                  <UserX :size="20"/>
                </div>
                <div>
                  <div class="font-semibold text-sm text-(--text-heading)">{{ selectedRecord.studentName }}</div>
                  <div class="text-xs text-(--text-muted)">{{ selectedRecord.studentRollNumber }} • {{ selectedRecord.studentClass || 'N/A' }}</div>
                </div>
              </div>
            </div>

            <div class="p-5 flex-1 overflow-y-auto space-y-4">
              <div class="grid grid-cols-2 gap-4">
                <div>
                  <div class="text-xs text-(--text-muted) mb-1">Mức độ</div>
                  <div class="font-medium text-sm text-(--text-heading)">{{ selectedRecord.mucDoKyLuat }}</div>
                </div>
                <div>
                  <div class="text-xs text-(--text-muted) mb-1">Hình thức xử lý</div>
                  <div class="font-medium text-sm text-(--text-heading)">{{ selectedRecord.hinhThucXuLy }}</div>
                </div>
                <div>
                  <div class="text-xs text-(--text-muted) mb-1">Ngày vi phạm</div>
                  <div class="text-sm text-(--text-body)">{{ formatDate(selectedRecord.ngayViPham) }}</div>
                </div>
                <div>
                  <div class="text-xs text-(--text-muted) mb-1">Trạng thái</div>
                  <GlassBadge :variant="selectedRecord.trangThai === 'active' ? 'danger' : 'neutral'" size="sm">
                    {{ selectedRecord.trangThai === 'active' ? 'Đang hiệu lực' : 'Hết hiệu lực' }}
                  </GlassBadge>
                </div>
              </div>

              <div>
                <div class="text-xs font-semibold text-(--text-muted) mb-2 uppercase tracking-wide">Mô tả vi phạm</div>
                <div class="bg-(--surface-input) p-3 rounded text-sm text-(--text-body) border border-(--border-default) font-mono leading-relaxed">
                  {{ selectedRecord.moTaCongKhai || selectedRecord.moTaNoiBo || 'Không có mô tả chi tiết.' }}
                </div>
              </div>
            </div>

            <div class="p-5 mt-auto bg-(--surface-modal) border-t border-(--border-default)">
              <GlassButton v-if="selectedRecord.trangThai === 'active'" variant="secondary" class="w-full justify-center !text-(--color-danger-bg) !border-(--color-danger-bg) hover:!bg-red-500/10" @click="removeEffect">
                Gỡ hiệu lực sớm
              </GlassButton>
              <GlassButton v-else variant="secondary" class="w-full justify-center opacity-70" disabled>
                Đã gỡ hiệu lực
              </GlassButton>
            </div>
          </div>

          <!-- Appeal Details -->
          <div v-if="currentTab === 'appeals' && selectedAppeal" class="flex flex-col h-full">
            <div class="p-5 border-b border-(--border-default)">
              <div class="flex items-center justify-between mb-3">
                <h3 class="font-bold text-lg text-(--text-heading)">Đơn khiếu nại</h3>
                <GlassBadge v-if="selectedAppeal.trangThai === 'pending'" variant="warning">Chờ xử lý</GlassBadge>
                <GlassBadge v-else-if="selectedAppeal.trangThai === 'resolved'" variant="success">Chấp nhận</GlassBadge>
                <GlassBadge v-else variant="danger">Từ chối</GlassBadge>
              </div>
              <div class="flex items-center gap-3">
                <div class="w-10 h-10 rounded-full bg-(--surface-input) border border-(--border-default) flex items-center justify-center shrink-0">
                  <Scale :size="18" class="text-(--lg-primary)" />
                </div>
                <div>
                  <div class="font-semibold text-sm">{{ selectedAppeal.studentName }}</div>
                  <div class="text-xs text-(--text-muted)">{{ selectedAppeal.studentRollNumber }}</div>
                </div>
              </div>
            </div>

            <div class="p-5 flex-1 overflow-y-auto space-y-5">
              <div>
                <div class="text-xs text-(--text-muted) mb-1">Mã hồ sơ liên quan</div>
                <div class="font-mono text-sm">{{ selectedAppeal.maHoSo }}</div>
              </div>
              
              <div>
                <div class="text-xs text-(--text-muted) mb-1">Lý do khiếu nại</div>
                <div class="bg-(--surface-input) p-3 rounded-lg border border-(--border-default) text-sm text-(--text-body) leading-relaxed italic">
                  "{{ selectedAppeal.lyDoKhieuNai }}"
                </div>
              </div>

              <!-- Resolve Form -->
              <div v-if="selectedAppeal.trangThai === 'pending'" class="mt-6 border-t border-dashed border-(--border-default) pt-4">
                <h4 class="font-medium text-sm text-(--text-heading) mb-3">Quyết định xử lý</h4>
                
                <div class="flex gap-2 mb-3 bg-(--surface-input) p-1 rounded-md border border-(--border-default)">
                  <button class="flex-1 py-1.5 rounded text-sm transition-colors"
                          :class="appealResolution.status === 'resolved' ? 'bg-emerald-500/20 text-emerald-600 dark:text-emerald-400 font-medium' : 'text-(--text-muted)'"
                          @click="appealResolution.status = 'resolved'">Chấp nhận</button>
                  <button class="flex-1 py-1.5 rounded text-sm transition-colors"
                          :class="appealResolution.status === 'rejected' ? 'bg-red-500/20 text-red-600 dark:text-red-400 font-medium' : 'text-(--text-muted)'"
                          @click="appealResolution.status = 'rejected'">Từ chối</button>
                </div>
                
                <textarea v-model="appealResolution.reason" 
                          class="w-full bg-(--surface-input) border border-(--border-default) rounded-md p-2.5 text-sm resize-none focus:outline-none focus:ring-2 focus:ring-(--border-focus) mb-3 min-h-[80px]" 
                          placeholder="Ghi chú lý do giải quyết..."></textarea>
                
                <GlassButton :variant="appealResolution.status === 'resolved' ? 'primary' : 'danger'" class="w-full justify-center" @click="resolveAppeal">
                  Xác nhận xử lý
                </GlassButton>
              </div>

              <!-- Solved State -->
              <div v-else class="mt-6 bg-(--surface-modal) rounded-lg p-4 border border-(--border-default)">
                <div class="flex items-center gap-2 mb-2">
                  <CheckCircle v-if="selectedAppeal.trangThai === 'resolved'" :size="16" class="text-emerald-500"/>
                  <FileText v-else :size="16" class="text-red-500"/>
                  <strong class="text-sm">Ghi chú giải quyết</strong>
                </div>
                <div class="text-sm text-(--text-body)">{{ selectedAppeal.ghiChuGiaiQuyet || 'Không có ghi chú.' }}</div>
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
