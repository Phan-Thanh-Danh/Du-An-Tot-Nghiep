import * as XLSX from 'xlsx'

export async function exportBghToExcel(data, filename, sheetName) {
  const { exportToExcel } = await import('@/services/exportService.js')
  return exportToExcel(data, filename, sheetName)
}

/**
 * Xuất báo cáo Tổng quan kết quả học tập ra Excel với định dạng chuyên nghiệp.
 */
export async function exportAcademicOverviewToExcelAdvanced(opts = {}) {
  const {
    kpis = [],
    distribution = [],
    chartData = [],
    topSubjects = [],
    totalTeachers = 0,
    totalClasses = 0,
    semesterLabel = 'Tất cả học kỳ',
    campusLabel = 'Tất cả cơ sở',
  } = opts

  const now = new Date()
  const dateStr = now.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
  const timeStr = now.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })

  // Tạo mảng dữ liệu (Array of Arrays)
  const aoa = []

  // --- HEADER ---
  aoa.push(['BÁO CÁO HỌC VỤ — HỆ THỐNG LMS'])
  aoa.push(['Tổng quan kết quả học tập'])
  aoa.push(['Ngày xuất:', `${dateStr} lúc ${timeStr}`])
  aoa.push(['Học kỳ:', semesterLabel])
  aoa.push(['Cơ sở:', campusLabel])
  aoa.push([])

  // --- TỔNG QUAN KPI ---
  aoa.push(['1. CHỈ TIÊU ĐÁNH GIÁ CHUNG'])
  aoa.push(['Chỉ tiêu', 'Giá trị', 'Đánh giá/Xu hướng'])
  kpis.forEach(k => aoa.push([k.label, k.value, k.trend || '']))
  aoa.push([])

  // --- NHÂN SỰ ---
  const abPct = distribution
    .filter(d => d.range?.startsWith('A') || d.range?.startsWith('B'))
    .reduce((s, d) => s + (d.percent || 0), 0)
    .toFixed(0)

  aoa.push(['2. NHÂN SỰ & GIẢNG DẠY'])
  aoa.push(['Giảng viên đang dạy', 'Lớp học phần đang mở', 'Tỷ lệ sinh viên đạt điểm A/B'])
  aoa.push([totalTeachers, totalClasses, `${abPct}%`])
  aoa.push([])

  // --- PHÂN PHỐI ĐIỂM ---
  aoa.push(['3. PHÂN PHỐI ĐIỂM SỐ'])
  aoa.push(['Xếp loại', 'Số lượng sinh viên', 'Tỷ lệ phần trăm'])
  distribution.forEach(d => aoa.push([d.range, d.count, `${d.percent}%`]))
  aoa.push([])

  // --- XU HƯỚNG GPA ---
  aoa.push(['4. XU HƯỚNG GPA THEO HỌC KỲ'])
  aoa.push(['Học kỳ', 'GPA Trung bình toàn trường'])
  chartData.forEach(d => aoa.push([d.k, Number(d.toanTruong)]))
  aoa.push([])

  // --- XẾP HẠNG MÔN HỌC ---
  aoa.push(['5. TOP CÁC MÔN HỌC THEO TỶ LỆ PASS'])
  aoa.push(['Tên môn học', 'Sĩ số', 'Pass', 'Tỷ lệ rớt'])
  topSubjects.forEach(s => aoa.push([s.name, s.total, s.pass, `${s.failRate}%`]))
  aoa.push([])

  const ws = XLSX.utils.aoa_to_sheet(aoa)

  // Chỉnh độ rộng cột
  ws['!cols'] = [
    { wch: 35 }, // Cột 1
    { wch: 20 }, // Cột 2
    { wch: 25 }, // Cột 3
    { wch: 15 }  // Cột 4
  ]

  const wb = XLSX.utils.book_new()
  XLSX.utils.book_append_sheet(wb, ws, 'Tổng quan')

  const semShort = semesterLabel.replace(/[^a-zA-Z0-9\u00C0-\u024F]/g, '_').slice(0, 30)
  const filename = `BaoCao-TongQuan-${semShort}-${now.getFullYear()}.xlsx`
  
  XLSX.writeFile(wb, filename)
}

export function printBghPage() {
  window.print()
}

/**
 * Xuất báo cáo Tổng quan kết quả học tập ra file PDF.
 * Dùng <table> layout thuần — KHÔNG dùng flex/gap để tránh lỗi trắng trang với html2canvas.
 */
export async function exportAcademicOverviewToPdf(opts = {}) {
  const html2pdf = (await import('html2pdf.js')).default

  const {
    kpis = [],
    distribution = [],
    chartData = [],
    topSubjects = [],
    totalTeachers = 0,
    totalClasses = 0,
    semesterLabel = 'Tất cả học kỳ',
    campusLabel = 'Tất cả cơ sở',
  } = opts

  const now = new Date()
  const dateStr = now.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
  const timeStr = now.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })

  // ── KPI cells (table-based) ──────────────────────────────────────
  const kpiColors = [
    { bg: '#EFF6FF', text: '#1D4ED8', border: '#BFDBFE' },
    { bg: '#F0FDF4', text: '#15803D', border: '#BBF7D0' },
    { bg: '#FFFBEB', text: '#B45309', border: '#FDE68A' },
    { bg: '#FEF2F2', text: '#B91C1C', border: '#FECACA' },
  ]
  const kpiCells = kpis.map((kpi, i) => {
    const c = kpiColors[i % kpiColors.length]
    return `<td style="width:25%;padding:4px;">
      <div style="background:${c.bg};border:1.5px solid ${c.border};border-radius:10px;padding:14px 10px;text-align:center;">
        <p style="font-size:9px;font-weight:700;color:#64748B;text-transform:uppercase;letter-spacing:0.4px;margin:0 0 6px;">${kpi.label}</p>
        <p style="font-size:20px;font-weight:800;color:${c.text};margin:0;">${kpi.value}</p>
      </div>
    </td>`
  }).join('')

  // ── Nhân sự cells ────────────────────────────────────────────────
  const abPct = distribution
    .filter(d => d.range?.startsWith('A') || d.range?.startsWith('B'))
    .reduce((s, d) => s + (d.percent || 0), 0)
    .toFixed(0)

  const staffCells = `
    <td style="width:33.33%;padding:4px;">
      <div style="background:#F1F5F9;border:1.5px solid #E2E8F0;border-radius:10px;padding:12px;text-align:center;">
        <p style="font-size:9px;font-weight:700;color:#64748B;text-transform:uppercase;margin:0 0 5px;">Giảng viên đang dạy</p>
        <p style="font-size:18px;font-weight:800;color:#1D4ED8;margin:0;">${totalTeachers.toLocaleString()}</p>
      </div>
    </td>
    <td style="width:33.33%;padding:4px;">
      <div style="background:#F1F5F9;border:1.5px solid #E2E8F0;border-radius:10px;padding:12px;text-align:center;">
        <p style="font-size:9px;font-weight:700;color:#64748B;text-transform:uppercase;margin:0 0 5px;">Lớp học phần đang mở</p>
        <p style="font-size:18px;font-weight:800;color:#7C3AED;margin:0;">${totalClasses.toLocaleString()}</p>
      </div>
    </td>
    <td style="width:33.33%;padding:4px;">
      <div style="background:#EFF6FF;border:1.5px solid #BFDBFE;border-radius:10px;padding:12px;text-align:center;">
        <p style="font-size:9px;font-weight:700;color:#64748B;text-transform:uppercase;margin:0 0 5px;">Điểm A/B (chất lượng)</p>
        <p style="font-size:18px;font-weight:800;color:#15803D;margin:0;">${abPct}%</p>
      </div>
    </td>`

  // ── Phân phối điểm rows ──────────────────────────────────────────
  const distRows = distribution.map(d => {
    const barColor = d.range?.startsWith('A') ? '#16A34A'
      : d.range?.startsWith('B') ? '#2563EB'
      : d.range?.startsWith('C') ? '#7C3AED'
      : d.range?.startsWith('D') ? '#D97706' : '#DC2626'
    const barW = Math.min(d.percent || 0, 100)
    return `<tr>
      <td style="padding:7px 10px;font-weight:700;color:#334155;font-size:11px;">${d.range}</td>
      <td style="padding:7px 10px;text-align:right;color:#334155;font-size:11px;">${d.count} SV</td>
      <td style="padding:7px 10px;text-align:right;font-size:11px;">
        <table style="border-collapse:collapse;margin-left:auto;">
          <tr>
            <td style="padding:0;vertical-align:middle;">
              <div style="width:48px;height:5px;background:#E2E8F0;border-radius:3px;">
                <div style="width:${barW}%;height:5px;background:${barColor};border-radius:3px;"></div>
              </div>
            </td>
            <td style="padding:0 0 0 5px;vertical-align:middle;font-weight:700;color:${barColor};font-size:10px;">${d.percent}%</td>
          </tr>
        </table>
      </td>
    </tr>`
  }).join('')

  // ── GPA trend rows ───────────────────────────────────────────────
  const gpaRows = chartData.map(item => {
    const gpa = Number(item.toanTruong) || 0
    const badge = gpa >= 7.5
      ? { bg: '#F0FDF4', text: '#15803D' }
      : gpa >= 5.5
        ? { bg: '#FFFBEB', text: '#B45309' }
        : { bg: '#FEF2F2', text: '#B91C1C' }
    return `<tr>
      <td style="padding:7px 10px;font-weight:600;color:#334155;font-size:11px;">${item.k}</td>
      <td style="padding:7px 10px;text-align:right;">
        <span style="background:${badge.bg};color:${badge.text};font-weight:800;padding:2px 8px;border-radius:20px;font-size:10px;">${gpa.toFixed(2)}</span>
      </td>
    </tr>`
  }).join('')

  // ── Subject rows ─────────────────────────────────────────────────
  const subjectRows = topSubjects.map((s, idx) => {
    const failBadge = s.failRate >= 20
      ? { bg: '#FEF2F2', text: '#B91C1C' }
      : s.failRate >= 10
        ? { bg: '#FFFBEB', text: '#B45309' }
        : { bg: '#F0FDF4', text: '#15803D' }
    return `<tr style="background:${idx % 2 === 0 ? '#F8FAFC' : '#FFFFFF'};">
      <td style="padding:7px 10px;font-weight:600;color:#334155;font-size:11px;">${s.name}</td>
      <td style="padding:7px 10px;text-align:center;color:#64748B;font-size:11px;">${s.total}</td>
      <td style="padding:7px 10px;text-align:center;color:#15803D;font-weight:700;font-size:11px;">${s.pass}</td>
      <td style="padding:7px 10px;text-align:center;">
        <span style="background:${failBadge.bg};color:${failBadge.text};font-weight:700;padding:2px 8px;border-radius:20px;font-size:10px;">${s.failRate}%</span>
      </td>
    </tr>`
  }).join('')

  // ── Full HTML — chỉ dùng <table> layout ─────────────────────────
  const html = `
<style>
  *, *::before, *::after { box-sizing: border-box; }
  html, body { margin: 0; padding: 0; width: 100%; background: #fff; }
</style>
<div style="font-family:'Segoe UI',Arial,sans-serif;color:#1E293B;background:#fff;width:100%;max-width:794px;margin:0 auto;padding:0;">

  <!-- HEADER -->
  <div style="background:linear-gradient(135deg,#1E3A8A 0%,#2563EB 60%,#0EA5E9 100%);padding:28px 36px 24px;">
    <table style="width:100%;border-collapse:collapse;">
      <tr>
        <td style="vertical-align:top;">
          <p style="font-size:10px;font-weight:600;color:#93C5FD;text-transform:uppercase;letter-spacing:1px;margin:0 0 5px;">BÁO CÁO HỌC VỤ — HỆ THỐNG LMS</p>
          <p style="font-size:20px;font-weight:800;color:#fff;margin:0 0 3px;">Tổng quan kết quả học tập</p>
          <p style="font-size:11px;color:#BAE6FD;margin:0;">Phân tích chất lượng học tập và hiệu quả giảng dạy</p>
        </td>
        <td style="vertical-align:top;text-align:right;white-space:nowrap;">
          <p style="font-size:10px;color:#93C5FD;margin:0 0 3px;">Ngày xuất: ${dateStr} lúc ${timeStr}</p>
          <p style="font-size:10px;color:#BAE6FD;margin:0 0 2px;">Học kỳ: ${semesterLabel}</p>
          <p style="font-size:10px;color:#BAE6FD;margin:0;">Cơ sở: ${campusLabel}</p>
        </td>
      </tr>
    </table>
  </div>

  <div style="padding:24px 36px;">

    <!-- KPI CARDS -->
    <table style="width:100%;border-collapse:collapse;margin-bottom:16px;"><tr>${kpiCells}</tr></table>

    <!-- NHÂN SỰ -->
    <table style="width:100%;border-collapse:collapse;margin-bottom:20px;"><tr>${staffCells}</tr></table>

    <!-- PHÂN PHỐI + GPA TREND (2 cột bằng table) -->
    <table style="width:100%;border-collapse:collapse;margin-bottom:20px;">
      <tr>
        <td style="width:50%;vertical-align:top;padding-right:10px;">
          <div style="border:1.5px solid #E2E8F0;border-radius:10px;overflow:hidden;">
            <div style="background:#F8FAFC;padding:10px 14px;border-bottom:1px solid #E2E8F0;">
              <p style="font-size:11px;font-weight:700;color:#334155;text-transform:uppercase;letter-spacing:0.4px;margin:0;">Phân phối điểm số</p>
            </div>
            <table style="width:100%;border-collapse:collapse;">
              <thead><tr style="background:#F1F5F9;">
                <th style="padding:7px 10px;text-align:left;color:#64748B;font-size:9px;font-weight:700;text-transform:uppercase;">Xếp loại</th>
                <th style="padding:7px 10px;text-align:right;color:#64748B;font-size:9px;font-weight:700;text-transform:uppercase;">Số SV</th>
                <th style="padding:7px 10px;text-align:right;color:#64748B;font-size:9px;font-weight:700;text-transform:uppercase;">Tỷ lệ</th>
              </tr></thead>
              <tbody>${distRows || '<tr><td colspan="3" style="text-align:center;padding:14px;color:#94A3B8;font-size:11px;">Chưa có dữ liệu</td></tr>'}</tbody>
            </table>
          </div>
        </td>
        <td style="width:50%;vertical-align:top;padding-left:10px;">
          <div style="border:1.5px solid #E2E8F0;border-radius:10px;overflow:hidden;">
            <div style="background:#F8FAFC;padding:10px 14px;border-bottom:1px solid #E2E8F0;">
              <p style="font-size:11px;font-weight:700;color:#334155;text-transform:uppercase;letter-spacing:0.4px;margin:0;">Xu hướng GPA theo học kỳ</p>
            </div>
            <table style="width:100%;border-collapse:collapse;">
              <thead><tr style="background:#F1F5F9;">
                <th style="padding:7px 10px;text-align:left;color:#64748B;font-size:9px;font-weight:700;text-transform:uppercase;">Học kỳ</th>
                <th style="padding:7px 10px;text-align:right;color:#64748B;font-size:9px;font-weight:700;text-transform:uppercase;">GPA TB</th>
              </tr></thead>
              <tbody>${gpaRows || '<tr><td colspan="2" style="text-align:center;padding:14px;color:#94A3B8;font-size:11px;">Chưa có dữ liệu</td></tr>'}</tbody>
            </table>
          </div>
        </td>
      </tr>
    </table>

    ${topSubjects.length > 0 ? `<!-- BẢNG MÔN HỌC -->
    <div style="border:1.5px solid #E2E8F0;border-radius:10px;overflow:hidden;margin-bottom:20px;">
      <div style="background:#F8FAFC;padding:10px 14px;border-bottom:1px solid #E2E8F0;">
        <p style="font-size:11px;font-weight:700;color:#334155;text-transform:uppercase;letter-spacing:0.4px;margin:0;">Xếp hạng môn học theo tỷ lệ Pass</p>
      </div>
      <table style="width:100%;border-collapse:collapse;">
        <thead><tr style="background:#F1F5F9;">
          <th style="padding:7px 10px;text-align:left;color:#64748B;font-size:9px;font-weight:700;text-transform:uppercase;">Môn học</th>
          <th style="padding:7px 10px;text-align:center;color:#64748B;font-size:9px;font-weight:700;text-transform:uppercase;">Sĩ số</th>
          <th style="padding:7px 10px;text-align:center;color:#64748B;font-size:9px;font-weight:700;text-transform:uppercase;">Pass</th>
          <th style="padding:7px 10px;text-align:center;color:#64748B;font-size:9px;font-weight:700;text-transform:uppercase;">Tỷ lệ rớt</th>
        </tr></thead>
        <tbody>${subjectRows}</tbody>
      </table>
    </div>` : ''}

    <!-- FOOTER -->
    <table style="width:100%;border-collapse:collapse;margin-top:8px;border-top:1.5px solid #E2E8F0;">
      <tr>
        <td style="vertical-align:top;padding-top:12px;">
          <p style="font-size:9px;color:#94A3B8;margin:0;">Tài liệu được tạo tự động bởi Hệ thống LMS Academic Management</p>
          <p style="font-size:9px;color:#94A3B8;margin:3px 0 0;">Dữ liệu phản ánh tình trạng tại thời điểm xuất · ${dateStr} ${timeStr}</p>
        </td>
        <td style="text-align:right;vertical-align:top;padding-top:12px;white-space:nowrap;">
          <p style="font-size:9px;font-weight:700;color:#1D4ED8;margin:0;">LMS · Ban Giám Hiệu</p>
          <p style="font-size:9px;color:#94A3B8;margin:3px 0 0;">Báo cáo bảo mật — Lưu hành nội bộ</p>
        </td>
      </tr>
    </table>

  </div>
</div>`

  const semShort = semesterLabel.replace(/[^a-zA-Z0-9\u00C0-\u024F]/g, '_').slice(0, 30)
  const filename = `BaoCao-TongQuan-${semShort}-${now.getFullYear()}.pdf`

  await html2pdf()
    .set({
      margin: [10, 5, 10, 5],
      filename,
      image: { type: 'jpeg', quality: 0.98 },
      html2canvas: { scale: 2, useCORS: true, logging: false },
      jsPDF: { unit: 'mm', format: 'a4', orientation: 'portrait' }
    })
    .from(html)
    .save()
}