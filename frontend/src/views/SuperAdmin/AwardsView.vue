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
import { organizationApi } from '@/services/organizationService'
import { certificateTemplateApi } from '@/services/certificateTemplateApi'
import { academicTermApi } from '@/services/academicTermApi'
import { unwrapApiData } from '@/services/apiClient'
import { usePopupStore } from '@/stores/popup'

const popupStore = usePopupStore()
const showCreateModal = ref(false)
const isSubmitting = ref(false)
const terms = ref([])
const campuses = ref([])
const templates = ref([])
const createForm = ref({
  tenDot: '',
  maHocKy: null,
  maDonVi: null,
  soLuongToiDa: 100,
  maMauBangKhen: null,
  ghiChu: ''
})

const resetCreateForm = () => {
  createForm.value = {
    tenDot: '',
    maHocKy: null,
    maDonVi: null,
    soLuongToiDa: 100,
    maMauBangKhen: null,
    ghiChu: ''
  }
}

const filteredTerms = computed(() => {
  if (!createForm.value.maDonVi) return []
  return terms.value.filter(t => t.maDonVi === createForm.value.maDonVi || t.MaDonVi === createForm.value.maDonVi)
})
const campaigns = ref([])
const loading = ref(false)
const confirmAction = ref(null)
const searchQuery = ref('')
const filter = ref('all')
const selectedCampaign = ref(null)
const candidates = ref([])
const genProgress = ref(null)

const showCandidatesModal = ref(false)
const fullCandidates = ref([])
const isLoadingCandidates = ref(false)

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
  if (['da_huy', 'cancelled'].includes(status)) return 'cancelled'
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

onMounted(async () => {
  fetchCampaigns()
  try {
    const [termsRes, templatesRes, orgRes] = await Promise.all([
      academicTermApi.list({ pageIndex: 1, pageSize: 1000 }),
      certificateTemplateApi.getTemplates({ pageIndex: 1, pageSize: 100 }),
      organizationApi.getAll().catch(() => null)
    ])
    terms.value = termsRes || []
    
    // Filter campuses
    const orgs = unwrapApiData(orgRes) || []
    const campusList = orgs.filter(o => o.loaiDonVi === 'Campus' || o.LoaiDonVi === 'Campus')
    campuses.value = campusList.length ? campusList : orgs

    const tplData = unwrapApiData(templatesRes)
    templates.value = tplData?.items ?? tplData?.Items ?? []
  } catch (err) {
    console.error('Lỗi tải danh mục:', err)
  }
})

const submitCreateForm = async () => {
  if (!createForm.value.tenDot || !createForm.value.maHocKy || !createForm.value.maDonVi) {
    popupStore.error('Thiếu thông tin', 'Vui lòng nhập tên đợt, chọn cơ sở và học kỳ.')
    return
  }
  isSubmitting.value = true
  try {
    await rewardDisciplineApi.createTop100Campaign({
      MaDonVi: createForm.value.maDonVi,
      MaHocKy: createForm.value.maHocKy,
      TenDot: createForm.value.tenDot,
      SoLuongToiDa: createForm.value.soLuongToiDa,
      MaMauBangKhen: createForm.value.maMauBangKhen,
      GhiChu: createForm.value.ghiChu
    })
    popupStore.success('Thành công', 'Đã tạo đợt khen thưởng mới.')
    showCreateModal.value = false
    resetCreateForm()
    fetchCampaigns()
  } catch (err) {
    popupStore.error('Lỗi', err?.message || 'Không thể tạo đợt khen thưởng.')
  } finally {
    isSubmitting.value = false
  }
}

const evaluateCampaignAction = async () => {
  if (!selectedCampaign.value) return
  const cmp = selectedCampaign.value
  try {
    popupStore.success('Đang xử lý', 'Hệ thống đang quét dữ liệu điểm số...')
    await rewardDisciplineApi.evaluateCampaign(cmp.id, { isDryRun: false })
    popupStore.success('Thành công', 'Tính toán xếp hạng thành công.')
    selectCampaign(cmp)
  } catch (err) {
    popupStore.error('Lỗi', err?.message || 'Không thể tính toán xếp hạng.')
  }
}

const fetchFullCandidates = async () => {
  if (!selectedCampaign.value) return
  isLoadingCandidates.value = true
  showCandidatesModal.value = true
  try {
    const res = await rewardDisciplineApi.getRewardCampaignCandidates(selectedCampaign.value.id, { pageIndex: 1, pageSize: 500 })
    const data = unwrapApiData(res)
    fullCandidates.value = (data?.items ?? data?.Items ?? []).map(mapCandidate)
  } catch (err) {
    popupStore.error('Lỗi', err?.message || 'Không thể tải danh sách ứng viên.')
  } finally {
    isLoadingCandidates.value = false
  }
}

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
    const [candidatesRes, summaryRes] = await Promise.all([
      rewardDisciplineApi.getRewardCampaignCandidates(cmp.id, { pageIndex: 1, pageSize: 3 }),
      rewardDisciplineApi.getApprovalSummary(cmp.id).catch(() => null)
    ])
    
    const data = unwrapApiData(candidatesRes)
    candidates.value = (data?.items ?? data?.Items ?? []).map(mapCandidate)

    if (summaryRes) {
      const summary = unwrapApiData(summaryRes)
      if (summary) {
        cmp.tongUngVien = summary.totalCandidates ?? summary.TotalCandidates ?? 0
        cmp.daDuyet = summary.selectedCount ?? summary.SelectedCount ?? summary.approvedCandidateCount ?? summary.ApprovedCandidateCount ?? 0
      }
    }
  } catch (err) {
    popupStore.error('Không thể tải thông tin', err?.message || 'Không thể tải thông tin đợt khen thưởng.')
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
  const cleanHtml = (template.html || '').replace(/<link[^>]*>/gi, '')
  const doc = [
    '<!DOCTYPE html><html lang="vi"><head><meta charset="utf-8">',
    `<style>*{box-sizing:border-box;margin:0;padding:0}html,body{width:${width}px;height:${height}px;overflow:hidden;}${template.css || ''}</style>`,
    `</head><body><div id="pdf-wrapper" style="width:${width}px;height:${height}px;position:relative;background:white;">${fillTokens(cleanHtml, rowData)}</div></body></html>`,
  ].join('')

  const iframe = document.createElement('iframe')
  iframe.style.cssText = `position:fixed;left:0;top:0;width:${width}px;height:${height}px;border:none;opacity:0;pointer-events:none;z-index:-9999;`
  document.body.appendChild(iframe)

  const iframeDoc = iframe.contentWindow.document
  iframeDoc.open()
  iframeDoc.write(doc)
  iframeDoc.close()

  const mmPerPx = 25.4 / 96
  try {
    await new Promise(r => setTimeout(r, 500)) // give it a bit more time to render CSS/images
    const wrapper = iframeDoc.getElementById('pdf-wrapper')
    const blob = await html2pdf()
      .set({
        margin: 0,
        filename: `bang-khen-${rowData.mssv || row.maKhenThuong || 'khong-ma'}.pdf`,
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: {
          scale: 2,
          useCORS: true,
          backgroundColor: '#ffffff',
          logging: false
        },
        jsPDF: {
          unit: 'mm',
          format: [Number((width * mmPerPx).toFixed(2)), Number((height * mmPerPx).toFixed(2))],
          orientation: width > height ? 'l' : 'p',
        },
      })
      .from(wrapper)
      .toPdf()
      .get('pdf')
      .outputPdf('blob')
    return blob
  } finally {
    iframe.remove()
  }
}

async function generateCertificatesFrontend(campaign) {
  let template = null
  if (campaign.maMauBangKhen) {
    template = await certificateTemplateApi.getTemplate(campaign.maMauBangKhen).catch(() => null)
  }
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

const cancelCampaignAction = () => {
  if (!selectedCampaign.value) return
  confirmAction.value = {
    title: 'Hủy đợt khen thưởng',
    message: `Bạn có chắc chắn muốn hủy đợt khen thưởng "${selectedCampaign.value.tenDot}"? Thao tác này không thể hoàn tác.`,
    label: 'Xác nhận hủy',
    variant: 'danger',
    run: async () => {
      try {
        await rewardDisciplineApi.cancelCampaign(selectedCampaign.value.id, { LyDoHuy: 'Hủy bởi Super Admin' })
        selectedCampaign.value = null
        confirmAction.value = null
        popupStore.success('Thành công', 'Đã hủy đợt khen thưởng.')
        fetchCampaigns()
      } catch (err) {
        popupStore.error('Lỗi', err?.message || 'Không thể hủy đợt khen thưởng.')
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
          <option value="cancelled">Đã hủy</option>
        </select>
        <GlassButton @click="showCreateModal = true" variant="primary" class="h-10">Tạo đợt mới</GlassButton>
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
                    <GlassBadge v-else-if="cmp.trangThai === 'cancelled'" variant="danger" size="sm">Đã hủy</GlassBadge>
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
                <GlassBadge v-else-if="selectedCampaign.trangThai === 'cancelled'" variant="danger" size="sm">Đã hủy</GlassBadge>
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
              <GlassButton @click="fetchFullCandidates" variant="ghost" size="sm" class="w-full mt-3 text-sm justify-center">Xem toàn bộ danh sách</GlassButton>
            </div>

            <div class="p-5 mt-auto bg-(--surface-modal)">
              <div class="flex flex-col gap-2">
                <GlassButton v-if="selectedCampaign.trangThai === 'evaluating'" variant="primary" class="w-full justify-center bg-indigo-600 hover:bg-indigo-700 text-white border-none" @click="evaluateCampaignAction">
                  Bắt đầu tính toán xếp hạng
                </GlassButton>
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
                <GlassButton v-if="['evaluating', 'approved'].includes(selectedCampaign.trangThai)" variant="ghost" class="w-full justify-center text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20" @click="cancelCampaignAction">
                  Hủy đợt khen thưởng
                </GlassButton>
              </div>
            </div>
          </div>
        </div>
      </div>
    </GlassPanel>

    <ConfirmActionDialog
      v-if="confirmAction"
      :model-value="true"
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

    <div v-if="showCreateModal" class="fixed inset-0 z-[70] flex items-center justify-center bg-black/50 p-4">
      <div class="lg-glass-strong w-full max-w-lg rounded-2xl border border-(--border-card) p-6">
        <h3 class="text-xl font-bold text-(--text-heading) mb-4">Tạo đợt khen thưởng mới (Top 100)</h3>
        
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-(--text-label) mb-1">Tên đợt khen thưởng <span class="text-red-500">*</span></label>
            <input v-model="createForm.tenDot" type="text" class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg focus:ring-2 focus:ring-(--border-focus) outline-none transition-shadow text-sm" placeholder="VD: Khen thưởng Top 100 Học kỳ..." />
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div class="flex flex-col gap-1.5">
              <label class="text-sm font-medium text-(--text-body)">Cơ sở <span class="text-red-500">*</span></label>
              <select v-model="createForm.maDonVi" @change="createForm.maHocKy = null" class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg focus:ring-2 focus:ring-(--border-focus) outline-none transition-shadow text-sm">
                <option :value="null">-- Chọn cơ sở --</option>
                <option v-for="c in campuses" :key="c.id || c.Id || c.maDonVi" :value="c.id || c.Id || c.maDonVi">
                  {{ c.tenDonVi || c.TenDonVi || c.name }}
                </option>
              </select>
            </div>
            
            <div class="flex flex-col gap-1.5">
              <label class="text-sm font-medium text-(--text-body)">Học kỳ <span class="text-red-500">*</span></label>
              <select v-model="createForm.maHocKy" :disabled="!createForm.maDonVi" class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg focus:ring-2 focus:ring-(--border-focus) outline-none transition-shadow text-sm disabled:opacity-50 disabled:bg-gray-100 dark:disabled:bg-gray-800">
                <option :value="null">-- Chọn học kỳ --</option>
                <option v-for="t in filteredTerms" :key="t.maHocKy || t.MaHocKy" :value="t.maHocKy || t.MaHocKy">
                  {{ t.tenHocKy || t.TenHocKy }}
                </option>
              </select>
            </div>
            <div>
              <label class="block text-sm font-medium text-(--text-label) mb-1">Số lượng tối đa</label>
              <input v-model.number="createForm.soLuongToiDa" type="number" min="1" max="1000" class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg focus:ring-2 focus:ring-(--border-focus) outline-none transition-shadow text-sm" />
            </div>
          </div>

          <div>
            <label class="block text-sm font-medium text-(--text-label) mb-1">Mẫu bằng khen</label>
            <select v-model="createForm.maMauBangKhen" class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg focus:ring-2 focus:ring-(--border-focus) outline-none transition-shadow text-sm">
              <option :value="null">-- Mặc định --</option>
              <option v-for="tpl in templates" :key="tpl.maMauBangKhen || tpl.MaMauBangKhen" :value="tpl.maMauBangKhen || tpl.MaMauBangKhen">
                {{ tpl.tenMau || tpl.TenMau }}
              </option>
            </select>
          </div>

          <div>
            <label class="block text-sm font-medium text-(--text-label) mb-1">Ghi chú</label>
            <textarea v-model="createForm.ghiChu" class="w-full p-3 bg-(--surface-input) border border-(--border-input) rounded-lg focus:ring-2 focus:ring-(--border-focus) outline-none transition-shadow text-sm resize-none" rows="3" placeholder="Nhập ghi chú..."></textarea>
          </div>
        </div>

        <div class="mt-6 flex justify-end gap-3">
          <GlassButton @click="showCreateModal = false" variant="ghost">Hủy</GlassButton>
          <GlassButton @click="submitCreateForm" variant="primary" :disabled="isSubmitting">
            <Loader2 v-if="isSubmitting" class="animate-spin mr-2 h-4 w-4" />
            Tạo đợt
          </GlassButton>
        </div>
      </div>
    </div>

    <!-- Candidates Modal -->
    <div v-if="showCandidatesModal" class="fixed inset-0 z-[80] flex items-center justify-center bg-black/60 p-4">
      <div class="lg-glass-strong w-full max-w-4xl max-h-[85vh] rounded-2xl border border-(--border-card) flex flex-col overflow-hidden">
        <div class="p-5 border-b border-(--border-default) flex justify-between items-center bg-(--surface-card)">
          <h3 class="text-xl font-bold text-(--text-heading)">Danh sách ứng viên khen thưởng</h3>
          <button @click="showCandidatesModal = false" class="text-(--text-muted) hover:text-(--text-heading) transition-colors">
            ✕
          </button>
        </div>
        <div class="p-0 overflow-y-auto flex-1 bg-(--surface-card)">
          <div v-if="isLoadingCandidates" class="p-8 flex justify-center items-center">
            <Loader2 class="animate-spin text-(--color-info-text) h-8 w-8" />
          </div>
          <TableShell v-else-if="fullCandidates.length > 0">
            <table>
              <thead class="sticky top-0 bg-(--surface-card) z-10 shadow-sm">
                <tr>
                  <th class="w-16 text-center">Hạng</th>
                  <th>MSSV</th>
                  <th>Họ Tên</th>
                  <th class="text-right">Điểm xét</th>
                  <th class="text-center">Trạng thái</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="c in fullCandidates" :key="c.id" class="hover:bg-(--surface-hover) transition-colors">
                  <td class="text-center font-bold text-amber-600">{{ c.rank }}</td>
                  <td class="font-mono text-sm">{{ c.rollNum }}</td>
                  <td class="font-medium text-(--text-heading)">{{ c.name }}</td>
                  <td class="text-right font-bold text-(--lg-primary)">{{ c.gpa }}</td>
                  <td class="text-center">
                    <GlassBadge v-if="c.status === 'approved'" variant="success" size="sm">Đã duyệt</GlassBadge>
                    <GlassBadge v-else variant="info" size="sm">Đang xét</GlassBadge>
                  </td>
                </tr>
              </tbody>
            </table>
          </TableShell>
          <div v-else class="p-12 text-center text-(--text-muted)">
            Không có ứng viên nào trong danh sách.
          </div>
        </div>
        <div class="p-4 border-t border-(--border-default) bg-(--surface-card) flex justify-end">
          <GlassButton @click="showCandidatesModal = false" variant="ghost">Đóng</GlassButton>
        </div>
      </div>
    </div>
  </div>
</template>
