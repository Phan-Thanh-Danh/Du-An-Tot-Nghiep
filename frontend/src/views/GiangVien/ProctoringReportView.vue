<template>
  <div class="proctor-report-page p-4 md:p-6 max-w-6xl mx-auto">
    <!-- Breadcrumb & Top Bar -->
    <div class="flex items-center justify-between mb-6 no-print">
      <div class="flex items-center gap-3">
        <button
          type="button"
          class="p-2 rounded-xl surface-card border-card text-heading hover:bg-black/5 dark:hover:bg-white/10 transition cursor-pointer"
          @click="goBack"
        >
          <ArrowLeft :size="20" />
        </button>
        <div>
          <h1 class="text-xl font-bold text-heading m-0">Biên bản ca thi #{{ sessionId }}</h1>
          <p class="text-xs text-label m-0">Xem và in báo cáo kết quả ca thi trực tuyến</p>
        </div>
      </div>

      <div class="flex items-center gap-2">
        <button
          type="button"
          class="px-4 py-2 rounded-xl bg-teal-600 text-white font-bold text-xs flex items-center gap-1.5 hover:bg-teal-700 transition cursor-pointer shadow-md"
          @click="printReport"
        >
          <Printer :size="16" /> In biên bản (PDF)
        </button>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="surface-card border-card rounded-2xl p-12 flex flex-col items-center justify-center gap-3 text-slate-400">
      <Loader2 :size="40" class="animate-spin text-teal-500" />
      <span class="text-sm font-medium">Đang tổng hợp dữ liệu biên bản ca thi...</span>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="surface-card border-card rounded-2xl p-12 flex flex-col items-center justify-center gap-4 text-center">
      <AlertCircle :size="48" class="text-rose-500" />
      <p class="text-rose-600 font-bold m-0">{{ error }}</p>
      <button type="button" class="btn-primary px-4 py-2 text-xs" @click="fetchReport">Thử lại</button>
    </div>

    <!-- Report Detail View (Printable Card) -->
    <section v-else-if="reportData" class="bien-ban-card lg-glass-strong surface-card border-card text-heading p-6 md:p-10 rounded-2xl shadow-xl">
      <!-- Title Header -->
      <div class="text-center border-b border-default pb-6 mb-6">
        <div class="flex items-center justify-center gap-2 text-teal-600 dark:text-teal-400 mb-1">
          <FileText :size="28" />
          <span class="text-xs uppercase tracking-wider font-extrabold">HỆ THỐNG QUẢN LÝ HỌC VỤ & THI TRỰC TUYẾN</span>
        </div>
        <h1 class="text-2xl md:text-3xl font-black text-heading uppercase tracking-tight m-0 mb-1">
          BIÊN BẢN GIÁM SÁT CA THI TRỰC TUYẾN
        </h1>
        <p class="text-xs text-label italic m-0">Mã ca thi: #{{ sessionId }} · Đã tự động lập và lưu trữ hệ thống</p>
      </div>

      <!-- General Info Grid -->
      <div class="grid grid-cols-2 md:grid-cols-4 gap-4 p-4 rounded-xl surface-input border-card mb-6 text-xs">
        <div>
          <span class="text-label block font-medium">Môn thi:</span>
          <strong class="text-heading text-sm">{{ reportData.tenMonHoc }} ({{ reportData.maCodeMonHoc }})</strong>
        </div>
        <div>
          <span class="text-label block font-medium">Phòng thi:</span>
          <strong class="text-heading text-sm">{{ reportData.tenPhong }}</strong>
        </div>
        <div>
          <span class="text-label block font-medium">Cán bộ giám sát (Giám thị):</span>
          <strong class="text-heading text-sm text-teal-600 dark:text-teal-400">{{ reportData.tenGiamThi }}</strong>
        </div>
        <div>
          <span class="text-label block font-medium">Ngày thi:</span>
          <strong class="text-heading text-sm">{{ reportData.ngayThi ? new Date(reportData.ngayThi).toLocaleDateString('vi-VN') : '2026-07-27' }}</strong>
        </div>
      </div>

      <!-- Statistics Summary Cards -->
      <div class="grid grid-cols-2 md:grid-cols-5 gap-3 mb-8 text-center">
        <div class="p-3.5 rounded-xl bg-slate-500/10 border border-slate-500/20">
          <span class="text-[11px] text-label block font-medium">Tổng thí sinh</span>
          <strong class="text-xl font-black text-heading">{{ reportData.tongSoThiSinh }}</strong>
        </div>
        <div class="p-3.5 rounded-xl bg-blue-500/10 border border-blue-500/20">
          <span class="text-[11px] text-blue-500 block font-medium">Có mặt</span>
          <strong class="text-xl font-black text-blue-600 dark:text-blue-400">{{ reportData.soCoMat }}</strong>
        </div>
        <div class="p-3.5 rounded-xl bg-emerald-500/10 border border-emerald-500/20">
          <span class="text-[11px] text-emerald-500 block font-medium">Đã nộp bài</span>
          <strong class="text-xl font-black text-emerald-600 dark:text-emerald-400">{{ reportData.soNopBai }}</strong>
        </div>
        <div class="p-3.5 rounded-xl bg-rose-500/10 border border-rose-500/20">
          <span class="text-[11px] text-rose-500 block font-medium">Bị đình chỉ</span>
          <strong class="text-xl font-black text-rose-600 dark:text-rose-400">{{ reportData.soDinhChi }}</strong>
        </div>
        <div class="p-3.5 rounded-xl bg-amber-500/10 border border-amber-500/20">
          <span class="text-[11px] text-amber-500 block font-medium">Tổng số vi phạm</span>
          <strong class="text-xl font-black text-amber-600 dark:text-amber-400">{{ reportData.tongSoViPham }}</strong>
        </div>
      </div>

      <!-- Candidate Results Table -->
      <div class="mb-8">
        <div class="flex items-center justify-between mb-3">
          <h2 class="text-base font-extrabold text-heading uppercase m-0 flex items-center gap-2">
            <span>DANH SÁCH THÍ SINH VÀ KẾT QUẢ</span>
          </h2>
          <span class="text-xs text-label">Tự động chấm trắc nghiệm (Thang điểm 10)</span>
        </div>

        <div class="table-print-container overflow-x-auto rounded-xl border border-card" style="max-height: 520px; overflow-y: auto;">
          <table class="w-full text-xs text-left">
            <thead class="bg-surface-input border-b border-card text-label font-semibold sticky top-0 z-10 backdrop-blur-md">
              <tr>
                <th class="p-3 text-center w-12">STT</th>
                <th class="p-3">Mã SV</th>
                <th class="p-3">Họ và tên</th>
                <th class="p-3">Trạng thái</th>
                <th class="p-3 text-center">Số câu đúng</th>
                <th class="p-3 text-center">Điểm số</th>
                <th class="p-3">Lỗi vi phạm phát hiện</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-card">
              <tr v-for="(stu, idx) in reportData.danhSachThiSinh" :key="stu.maHocSinh" class="hover:bg-surface-input/50">
                <td class="p-3 text-center font-medium text-label">{{ idx + 1 }}</td>
                <td class="p-3 font-bold text-heading">{{ stu.studentCode }}</td>
                <td class="p-3 font-medium text-heading">{{ stu.tenHocSinh }}</td>
                <td class="p-3">
                  <span v-if="stu.trangThai === 'da_nop'" class="px-2.5 py-1 rounded-lg text-[11px] font-bold bg-emerald-500/15 text-emerald-600 dark:text-emerald-400">Đã nộp bài</span>
                  <span v-else-if="stu.trangThai === 'dinh_chi'" class="px-2.5 py-1 rounded-lg text-[11px] font-bold bg-rose-500/15 text-rose-600 dark:text-rose-400">Bị đình chỉ</span>
                  <span v-else class="px-2.5 py-1 rounded-lg text-[11px] font-bold bg-slate-500/15 text-slate-500">Chưa nộp</span>
                </td>
                <td class="p-3 text-center font-semibold">
                  {{ stu.soCauDung != null ? `${stu.soCauDung} / ${stu.tongSoCau || 5}` : '-' }}
                </td>
                <td class="p-3 text-center">
                  <strong v-if="stu.diemSo != null" class="text-emerald-600 dark:text-emerald-400 text-sm font-black">{{ stu.diemSo }}đ</strong>
                  <span v-else class="text-label text-[11px]">(Chưa có)</span>
                </td>
                <td class="p-3">
                  <div v-if="stu.danhSachViPham && stu.danhSachViPham.length > 0" class="flex flex-col gap-1">
                    <span v-for="(v, vIdx) in stu.danhSachViPham" :key="vIdx" class="text-[11px] text-rose-500 flex items-center gap-1">
                      <AlertCircle :size="12" /> {{ v }}
                    </span>
                  </div>
                  <span v-else class="text-slate-400 text-[11px]">Không vi phạm</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Confirmation Signatures -->
      <div class="grid grid-cols-2 text-center text-xs mt-12 pt-6 border-t border-default">
        <div>
          <p class="font-bold text-heading m-0">ĐẠI DIỆN HỘI ĐỒNG THI</p>
          <p class="text-label text-[11px] italic mb-16">(Ký và ghi rõ họ tên)</p>
        </div>
        <div>
          <p class="font-bold text-heading m-0">CÁN BỘ GIÁM SÁT CA THI</p>
          <p class="text-label text-[11px] italic mb-16">(Ký và ghi rõ họ tên)</p>
          <strong class="text-teal-600 dark:text-teal-400 font-bold text-sm">{{ reportData.tenGiamThi }}</strong>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { FileText, Printer, ArrowLeft, Monitor, Loader2, AlertCircle } from 'lucide-vue-next'
import { teacherApi } from '@/services/teacherApi'

const route = useRoute()
const router = useRouter()

const sessionId = route.params.sessionId
const loading = ref(true)
const error = ref(null)
const reportData = ref(null)

const fetchReport = async () => {
  loading.value = true
  error.value = null
  try {
    const data = await teacherApi.getExamSessionReport(sessionId)
    reportData.value = data
  } catch (err) {
    console.error('Fetch report error:', err)
    error.value = err?.message || 'Không thể tải thông tin biên bản ca thi.'
  } finally {
    loading.value = false
  }
}

const printReport = () => {
  if (!reportData.value) return

  const rd = reportData.value

  const rows = (rd.danhSachThiSinh || []).map((stu, idx) => {
    const viPham = stu.danhSachViPham && stu.danhSachViPham.length > 0
      ? stu.danhSachViPham.join('; ')
      : 'Không vi phạm'
    const trangThai = stu.trangThai === 'da_nop' ? 'Đã nộp bài'
      : stu.trangThai === 'dinh_chi' ? 'Bị đình chỉ' : 'Chưa nộp'
    const diemSo = stu.diemSo != null ? `${stu.diemSo}đ` : '(Chưa có)'
    const soCau = stu.soCauDung != null ? `${stu.soCauDung} / ${stu.tongSoCau || 5}` : '-'
    return `
      <tr>
        <td>${idx + 1}</td>
        <td>${stu.studentCode || ''}</td>
        <td>${stu.tenHocSinh || ''}</td>
        <td>${trangThai}</td>
        <td style="text-align:center">${soCau}</td>
        <td style="text-align:center;font-weight:bold">${diemSo}</td>
        <td>${viPham}</td>
      </tr>`
  }).join('')

  const ngayThi = rd.ngayThi
    ? new Date(rd.ngayThi).toLocaleDateString('vi-VN')
    : new Date().toLocaleDateString('vi-VN')
  const scriptStart = '<' + 'script'
  const scriptEnd = '<' + '/script>'
  const html = `<!DOCTYPE html>
<html lang="vi">
<head>
  <meta charset="UTF-8" />
  <title>Biên bản ca thi #${sessionId}</title>
  <style>
    * { box-sizing: border-box; margin: 0; padding: 0; }
    body {
      font-family: 'Times New Roman', Times, serif;
      font-size: 12pt;
      color: #000;
      background: #fff;
      padding: 20mm 15mm;
    }
    .header { text-align: center; margin-bottom: 24px; border-bottom: 2px solid #000; padding-bottom: 16px; }
    .header .subtitle { font-size: 10pt; text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 4px; }
    .header h1 { font-size: 18pt; font-weight: 900; text-transform: uppercase; margin-bottom: 4px; }
    .header .meta { font-size: 9pt; color: #444; font-style: italic; }
    .info-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 6px 32px; margin-bottom: 20px; border: 1px solid #ccc; padding: 12px; border-radius: 4px; font-size: 11pt; }
    .info-grid .label { font-size: 9pt; color: #555; }
    .info-grid .value { font-weight: bold; }
    .stats { display: grid; grid-template-columns: repeat(5, 1fr); gap: 8px; margin-bottom: 24px; text-align: center; }
    .stat-box { border: 1px solid #ccc; padding: 8px 4px; border-radius: 4px; }
    .stat-box .stat-label { font-size: 8pt; color: #555; }
    .stat-box .stat-value { font-size: 16pt; font-weight: 900; }
    h2 { font-size: 12pt; text-transform: uppercase; margin-bottom: 10px; font-weight: 900; border-bottom: 1px solid #000; padding-bottom: 4px; }
    table { width: 100%; border-collapse: collapse; font-size: 10pt; margin-bottom: 24px; }
    thead { background-color: #f1f5f9; }
    th, td { border: 1px solid #94a3b8; padding: 6px 8px; vertical-align: top; }
    th { font-weight: 700; text-align: center; white-space: nowrap; }
    tr:nth-child(even) { background-color: #f8fafc; }
    .sign-section { display: grid; grid-template-columns: 1fr 1fr; text-align: center; margin-top: 40px; padding-top: 16px; border-top: 1px solid #000; font-size: 10pt; }
    .sign-section p { margin-bottom: 4px; }
    .sign-section .sign-blank { height: 60px; }
    .sign-section .name { font-weight: bold; }
    @page { size: A4; margin: 15mm; }
  </style>
</head>
<body>
  <div id="report-content">
  <div class="header">
    <div class="subtitle">Hệ thống quản lý học vụ &amp; thi trực tuyến</div>
    <h1>Biên bản giám sát ca thi trực tuyến</h1>
    <div class="meta">Mã ca thi: #${sessionId} &middot; Đã tự động lập và lưu trữ hệ thống</div>
  </div>

  <div class="info-grid">
    <div>
      <div class="label">Môn thi:</div>
      <div class="value">${rd.tenMonHoc || ''} (${rd.maCodeMonHoc || ''})</div>
    </div>
    <div>
      <div class="label">Phòng thi:</div>
      <div class="value">${rd.tenPhong || ''}</div>
    </div>
    <div>
      <div class="label">Cán bộ giám sát (Giám thị):</div>
      <div class="value">${rd.tenGiamThi || ''}</div>
    </div>
    <div>
      <div class="label">Ngày thi:</div>
      <div class="value">${ngayThi}</div>
    </div>
  </div>

  <div class="stats">
    <div class="stat-box">
      <div class="stat-label">Tổng thí sinh</div>
      <div class="stat-value">${rd.tongSoThiSinh ?? 0}</div>
    </div>
    <div class="stat-box">
      <div class="stat-label">Có mặt</div>
      <div class="stat-value">${rd.soCoMat ?? 0}</div>
    </div>
    <div class="stat-box">
      <div class="stat-label">Đã nộp bài</div>
      <div class="stat-value">${rd.soNopBai ?? 0}</div>
    </div>
    <div class="stat-box">
      <div class="stat-label">Bị đình chỉ</div>
      <div class="stat-value">${rd.soDinhChi ?? 0}</div>
    </div>
    <div class="stat-box">
      <div class="stat-label">Tổng vi phạm</div>
      <div class="stat-value">${rd.tongSoViPham ?? 0}</div>
    </div>
  </div>

  <h2>Danh sách thí sinh và kết quả</h2>
  <table>
    <thead>
      <tr>
        <th>STT</th>
        <th>Mã SV</th>
        <th>Họ và tên</th>
        <th>Trạng thái</th>
        <th>Số câu đúng</th>
        <th>Điểm số</th>
        <th>Lỗi vi phạm phát hiện</th>
      </tr>
    </thead>
    <tbody>${rows}</tbody>
  </table>

  <div class="sign-section">
    <div>
      <p><strong>ĐẠI DIỆN HỘI ĐỒNG THI</strong></p>
      <p style="font-size:9pt;font-style:italic">(Ký và ghi rõ họ tên)</p>
      <div class="sign-blank"></div>
    </div>
    <div>
      <p><strong>CÁN BỘ GIÁM SÁT CA THI</strong></p>
      <p style="font-size:9pt;font-style:italic">(Ký và ghi rõ họ tên)</p>
      <div class="sign-blank"></div>
      <p class="name">${rd.tenGiamThi || ''}</p>
    </div>
  </div>
  </div>

  <div id="loading-overlay" style="position:fixed;inset:0;background:rgba(255,255,255,0.92);display:flex;flex-direction:column;align-items:center;justify-content:center;z-index:9999;font-family:sans-serif;">
    <div style="width:48px;height:48px;border:5px solid #e2e8f0;border-top-color:#0d9488;border-radius:50%;animation:spin 0.8s linear infinite;margin-bottom:16px;"></div>
    <p style="color:#0d9488;font-weight:700;font-size:15px;margin:0;">Đang tạo file PDF...</p>
    <p style="color:#64748b;font-size:12px;margin:4px 0 0;">Vui lòng chờ, file sẽ tự động tải về</p>
  </div>
  <style>
    @keyframes spin { to { transform: rotate(360deg); } }
  </style>
  ${scriptStart} src="https://cdnjs.cloudflare.com/ajax/libs/html2pdf.js/0.10.1/html2pdf.bundle.min.js" crossorigin="anonymous">${scriptEnd}
  ${scriptStart}>
    window.onload = function() {
      var overlay = document.getElementById('loading-overlay');
      var content = document.getElementById('report-content');
      var filename = 'BienBan_CaThi_${sessionId}_${new Date().toISOString().slice(0,10)}.pdf';

      var opt = {
        margin: [10, 10, 10, 10],
        filename: filename,
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: {
          scale: 2,
          useCORS: true,
          logging: false,
        },
        jsPDF: {
          unit: 'mm',
          format: 'a4',
          orientation: 'portrait',
        },
        pagebreak: { mode: ['avoid-all', 'css', 'legacy'] }
      };

      html2pdf().set(opt).from(content).save().then(function() {
        overlay.innerHTML = '<p style="color:#0d9488;font-weight:700;font-size:16px;">✓ Tải xuống thành công!</p><p style="color:#64748b;font-size:12px;">Cửa sổ này sẽ tự đóng...</p>';
        setTimeout(function() { window.close(); }, 1500);
      }).catch(function(err) {
        overlay.innerHTML = '<p style="color:#e11d48;font-weight:700;font-size:14px;">Lỗi tạo PDF: ' + err.message + '</p><button onclick="window.print()" style="margin-top:12px;padding:8px 20px;background:#0d9488;color:#fff;border:none;border-radius:8px;cursor:pointer;font-size:13px;">In thay thế</button>';
      });
    };
  ${scriptEnd}
</body>
</html>`

  const printWindow = window.open('', '_blank', 'width=1000,height=750')
  if (printWindow) {
    printWindow.document.write(html)
    printWindow.document.close()
  }
}

const goBack = () => {
  router.push({ name: 'teacher-proctoring-sessions' })
}

onMounted(() => {
  fetchReport()
})
</script>

<style>
@media print {
  /* 1. Hide all non-printable UI elements */
  header,
  nav,
  aside,
  .app-sidebar,
  .app-topbar,
  .no-print,
  button,
  .ai-assistant-widget,
  [class*="ai-assistant"],
  [class*="topbar"],
  [class*="sidebar"],
  [class*="notice"] {
    display: none !important;
  }

  /* 2. Reset html & body layout */
  html, body {
    background: #ffffff !important;
    color: #000000 !important;
    margin: 0 !important;
    padding: 0 !important;
    width: 100% !important;
    height: auto !important;
    overflow: visible !important;
    -webkit-print-color-adjust: exact !important;
    print-color-adjust: exact !important;
  }

  /* 3. Report container full width */
  .proctor-report-page {
    max-width: 100% !important;
    width: 100% !important;
    padding: 0 !important;
    margin: 0 !important;
  }

  /* 4. Clean printable card styling */
  .bien-ban-card {
    border: none !important;
    box-shadow: none !important;
    background: #ffffff !important;
    color: #000000 !important;
    padding: 10mm 5mm !important;
    margin: 0 !important;
    border-radius: 0 !important;
  }

  /* 5. UNCLIP TABLE CONTAINER: allow full table to render across pages without scrollbar clipping */
  .table-print-container,
  .max-h-\[500px\],
  .overflow-y-auto,
  .overflow-x-auto {
    max-height: none !important;
    height: auto !important;
    overflow: visible !important;
    border: none !important;
  }

  /* 6. Print table formatting */
  table {
    width: 100% !important;
    border-collapse: collapse !important;
    font-size: 11px !important;
  }

  tr {
    page-break-inside: avoid !important;
  }

  th, td {
    border: 1px solid #94a3b8 !important;
    color: #000000 !important;
    padding: 6px 8px !important;
  }

  thead {
    display: table-header-group !important;
    background-color: #f1f5f9 !important;
  }

  /* Force all text colors to print clearly */
  h1, h2, h3, p, span, strong, td, th {
    color: #000000 !important;
  }
}
</style>
