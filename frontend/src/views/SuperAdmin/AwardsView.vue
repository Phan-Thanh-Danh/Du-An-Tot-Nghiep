<script setup>
import { ref, computed, onMounted } from 'vue'
import { Award, Search, Users, Loader2 } from 'lucide-vue-next'
import html2pdf from 'html2pdf.js'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import TableShell from '@/components/ui/TableShell.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import { rewardDisciplineApi } from '@/services/rewardDisciplineApi'
import { certificateTemplateApi } from '@/services/certificateTemplateApi'
import { unwrapApiData } from '@/services/apiClient'
import { usePopupStore } from '@/stores/popup'

const popupStore = usePopupStore()
const campaigns = ref([])
const loading = ref(false)
const confirmAction = ref(null)
const searchQuery = ref('')
const filter = ref('all')
const selectedCampaign = ref(null)
const candidates = ref([])
const genProgress = ref(null)

const mapCampaign = (item) => ({
  id: item.maDotKhenThuong ?? item.MaDotKhenThuong,
  maDot: `DKT-${item.maDotKhenThuong ?? item.MaDotKhenThuong}`,
  tenDot: item.tenDot ?? item.TenDot ?? 'Đợt khen thưởng',
  hocKy: item.tenHocKy ?? item.TenHocKy ?? 'Chưa có học kỳ',
  donVi: item.tenDonVi ?? item.TenDonVi,
  trangThai: normalizeCampaignStatus(item.trangThai ?? item.TrangThai ?? 'nhap'),
  maMauBangKhen: item.maMauBangKhen ?? item.MaMauBangKhen,
  tenMauBangKhen: item.tenMauBangKhen ?? item.TenMauBangKhen,
  tongUngVien: 0,
  daDuyet: 0,
})

function normalizeCampaignStatus(status) {
  if (['da_duyet', 'cho_duyet'].includes(status)) return 'approved'
  if (['da_cong_bo', 'completed'].includes(status)) return 'completed'
  return 'evaluating'
}

const mapCandidate = (item) => ({
  id: item.maUngVienKhenThuong ?? item.MaUngVienKhenThuong,
  rank: item.xepHang ?? item.XepHang ?? '-',
  name: item.hoTenSnapshot ?? item.HoTenSnapshot ?? 'Chưa có tên',
  rollNum: item.mssvSnapshot ?? item.MssvSnapshot ?? '',
  class: '',
  gpa: item.gpaHocKy ?? item.GpaHocKy ?? item.diemXet ?? item.DiemXet ?? '-',
  status: item.trangThai ?? item.TrangThai ?? '',
})

const fetchCampaigns = async () => {
  loading.value = true
  try {
    const res = await rewardDisciplineApi.getRewardCampaigns({ pageIndex: 1, pageSize: 50 })
    const data = unwrapApiData(res)
    campaigns.value = (data?.items ?? data?.Items ?? []).map(mapCampaign)
  } catch (err) {
    console.error(err)
    campaigns.value = []
    popupStore.error('Không thể tải dữ liệu', err?.message || 'Không thể tải danh sách đợt khen thưởng.')
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

const selectCampaign = async (cmp) => {
  selectedCampaign.value = cmp
  candidates.value = []
  try {
    const res = await rewardDisciplineApi.getRewardCampaignCandidates(cmp.id, { pageIndex: 1, pageSize: 3 })
    const data = unwrapApiData(res)
    candidates.value = (data?.items ?? data?.Items ?? []).map(mapCandidate)
  } catch (err) {
    popupStore.error('Không thể tải ứng viên', err?.message || 'Không thể tải danh sách ứng viên.')
  }
}

function parseConfig(json) {
  try {
    const value = typeof json === 'string' ? JSON.parse(json) : json
    return value || null
  } catch {
    return null
  }
}

function fillTokens(html, data) {
  return html.replace(/\{\{\s*([\w]+)\s*\}\}/g, (_, key) =>
    data[key] !== undefined && data[key] !== null ? String(data[key]) : `{{${key}}}`,
  )
}

function blobToBase64(blob) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.onloadend = () => resolve(String(reader.result).split(',')[1])
    reader.onerror = reject
    reader.readAsDataURL(blob)
  })
}

async function renderCertificatePdf(template, row, campaign) {
  const width = template.chieuRong || 1123
  const height = template.chieuCao || 794
  const rowData = {
    hoTen: row.hoTen ?? row.HoTen ?? 'Sinh viên',
    mssv: row.mssv ?? row.Mssv ?? '',
    tenHocKy: row.tenHocKy ?? row.TenHocKy ?? campaign.hocKy ?? '',
    danhHieu: row.danhHieu ?? row.DanhHieu ?? 'Top 100 học kỳ',
    xepHang: row.xepHang ?? row.XepHang ?? '',
    diemXet: row.diemXet ?? row.DiemXet ?? '',
    ngayCap: new Date().toISOString().slice(0, 10),
  }
  const doc = [
    '<!DOCTYPE html><html lang="vi"><head><meta charset="utf-8">',
    `<style>*{box-sizing:border-box;margin:0;padding:0}html,body{width:100%;height:100%}${template.css || ''}</style>`,
    `</head><body>${fillTokens(template.html || '', rowData)}</body></html>`,
  ].join('')

  const holder = document.createElement('div')
  holder.style.cssText = `position:absolute;left:-9999px;top:0;width:${width}px;height:${height}px;overflow:hidden;background:#fff`
  holder.innerHTML = doc
  document.body.appendChild(holder)

  const mmPerPx = 25.4 / 96
  try {
    const blob = await html2pdf()
      .set({
        margin: 0,
        filename: `bang-khen-${rowData.mssv || row.maKhenThuong || 'khong-ma'}.pdf`,
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: {
          scale: 2,
          useCORS: true,
          backgroundColor: '#ffffff',
          width,
          height,
          windowWidth: width,
          windowHeight: height,
        },
        jsPDF: {
          unit: 'mm',
          format: [Number((width * mmPerPx).toFixed(2)), Number((height * mmPerPx).toFixed(2))],
        },
      })
      .from(holder)
      .toPdf()
      .get('pdf')
      .outputPdf('blob')
    return blob
  } finally {
    holder.remove()
  }
}

async function generateCertificatesFrontend(campaign) {
  const template = await certificateTemplateApi.getTemplate(campaign.maMauBangKhen)
  const config = parseConfig(template?.cauHinhJson)
  if (!config || String(config.mode || '').toLowerCase() !== 'html') {
    await rewardDisciplineApi.generateRewardCertificates(campaign.id, {})
    return { mode: 'backend' }
  }

  const certRes = await rewardDisciplineApi.getRewardCertificates(campaign.id, {
    pageIndex: 1,
    pageSize: 500,
  })
  const certData = unwrapApiData(certRes)
  const rows = (certData?.items ?? certData?.Items ?? []).filter(
    (r) => !(r.urlPdfBangKhen ?? r.UrlPdfBangKhen),
  )
  if (rows.length === 0) {
    return { mode: 'frontend', total: 0, failed: [] }
  }

  const templateConfig = {
    html: config.html || '',
    css: config.css || '',
    chieuRong: template.chieuRong,
    chieuCao: template.chieuCao,
  }
  const failed = []
  genProgress.value = { total: rows.length, done: 0, current: '', failed: 0 }

  for (const row of rows) {
    const name = row.hoTen ?? row.HoTen ?? ''
    genProgress.value.current = name
    try {
      const blob = await renderCertificatePdf(templateConfig, row, campaign)
      const base64 = await blobToBase64(blob)
      await certificateTemplateApi.uploadRewardCertificatePdf(campaign.id, {
        MaKhenThuong: row.maKhenThuong ?? row.MaKhenThuong,
        MaMauBangKhen: campaign.maMauBangKhen,
        FileBase64: base64,
        GhiChu: 'FE render html2pdf',
      })
    } catch (err) {
      console.error(err)
      failed.push(`${name} (${row.mssv ?? row.Mssv ?? ''})`)
    }
    genProgress.value.done += 1
  }

  return { mode: 'frontend', total: rows.length, failed }
}

const generateCertificates = () => {
  if (!selectedCampaign.value) return
  const cmp = selectedCampaign.value
  const hasHtmlTemplate = Boolean(cmp.maMauBangKhen)
  confirmAction.value = {
    title: 'Phát sinh bằng khen',
    message: hasHtmlTemplate
      ? `Đợt này dùng mẫu giấy khen "${cmp.tenMauBangKhen || '—'}" (HTML/CSS). Bằng khen sẽ được render tại trình duyệt và tải lên hệ thống cho từng sinh viên.`
      : `Bạn muốn tạo bằng khen (PDF) cho đợt "${cmp.tenDot}"? Thao tác này sẽ xử lý toàn bộ ứng viên đã duyệt.`,
    label: 'Bắt đầu tạo',
    variant: 'primary',
    run: async () => {
      confirmAction.value = null
      try {
        const result = await generateCertificatesFrontend(cmp)
        if (result.mode === 'backend') {
          popupStore.success('Thành công', 'Đã phát sinh bằng khen.')
        } else if (result.total === 0) {
          popupStore.success('Thành công', 'Tất cả bằng khen đã có PDF, không có gì cần cấp phát.')
        } else {
          const okCount = result.total - result.failed.length
          if (result.failed.length === 0) {
            popupStore.success('Thành công', `Đã cấp phát ${okCount}/${result.total} bằng khen (render HTML tại trình duyệt).`)
          } else {
            popupStore.error(
              'Một số bằng khen thất bại',
              `Đã cấp ${okCount}/${result.total} bằng khen. Thất bại: ${result.failed.join('; ')}. Bạn có thể bấm lại để thử tiếp.`,
            )
          }
        }
        cmp.trangThai = 'completed'
      } catch (err) {
        console.error(err)
        popupStore.error('Lỗi', err?.message || 'Có lỗi xảy ra khi tạo bằng khen.')
      } finally {
        genProgress.value = null
      }
    },
  }
}

const approveCampaign = () => {
  if (!selectedCampaign.value) return
  confirmAction.value = {
    title: 'Chốt danh sách khen thưởng',
    message: `Xác nhận chốt danh sách ứng viên cho đợt "${selectedCampaign.value.tenDot}"? Bạn sẽ không thể thêm ứng viên sau khi chốt.`,
    label: 'Chốt danh sách',
    variant: 'primary',
    run: async () => {
      try {
        await rewardDisciplineApi.approveRewardCampaign(selectedCampaign.value.id)
        selectedCampaign.value.trangThai = 'approved'
        candidates.value.forEach(c => c.status = 'approved')
        confirmAction.value = null
        popupStore.success('Thành công', 'Đã chốt danh sách đợt khen thưởng.')
      } catch (err) {
        popupStore.error('Lỗi', err?.message || 'Không thể chốt danh sách khen thưởng.')
      }
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
                <div v-if="selectedCampaign.tenMauBangKhen" class="flex justify-between"><span class="text-(--text-muted)">Mẫu giấy khen</span><span class="font-medium text-(--text-body)">{{ selectedCampaign.tenMauBangKhen }}</span></div>
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

    <div v-if="genProgress" class="fixed inset-0 z-[70] flex items-center justify-center bg-black/50 p-4">
      <div class="lg-glass-strong w-full max-w-md rounded-2xl border border-(--border-card) p-6">
        <div class="mb-2 flex items-center gap-2">
          <Loader2 class="h-5 w-5 animate-spin text-(--color-info-text)" />
          <h3 class="text-heading font-bold">Đang cấp phát bằng khen...</h3>
        </div>
        <p class="text-label mb-3 text-sm">
          Đã xử lý {{ genProgress.done }}/{{ genProgress.total }} —
          {{ genProgress.current || 'Đang chuẩn bị' }}
        </p>
        <div class="h-2 w-full overflow-hidden rounded-full bg-(--surface-input)">
          <div
            class="h-full bg-(--color-info-text) transition-all duration-200"
            :style="{ width: `${genProgress.total ? (genProgress.done / genProgress.total) * 100 : 0}%` }"
          ></div>
        </div>
      </div>
    </div>
  </div>
</template>
