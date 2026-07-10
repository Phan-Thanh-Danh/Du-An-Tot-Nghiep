<script setup>
import SuperAdminApiListView from './components/SuperAdminApiListView.vue'
import { academicTermApi } from '@/services/academicTermApi'

const columns = [
  { key: 'code', label: 'Mã học kỳ' },
  { key: 'name', label: 'Tên học kỳ' },
  { key: 'year', label: 'Năm học' },
  { key: 'dateRange', label: 'Thời gian' },
  { key: 'status', label: 'Trạng thái' },
]

function formatDate(value) {
  if (!value) return 'Chưa có dữ liệu'
  return new Date(value).toLocaleDateString('vi-VN')
}

async function loadSemesters() {
  const terms = await academicTermApi.list({ pageSize: 100 })
  return terms.map((term) => ({
    id: term.maHocKy || term.MaHocKy,
    code: term.maCodeHocKy || term.MaCodeHocKy || String(term.maHocKy || term.MaHocKy || ''),
    name: term.tenHocKy || term.TenHocKy || 'Chưa có dữ liệu',
    year: term.namHoc || term.NamHoc || 'Chưa có dữ liệu',
    dateRange: `${formatDate(term.ngayBatDau || term.NgayBatDau)} - ${formatDate(term.ngayKetThuc || term.NgayKetThuc)}`,
    status: (term.daKhoa || term.DaKhoa) ? 'Đã khóa' : (term.trangThai || term.TrangThai || 'Đang mở'),
  }))
}
</script>

<template>
  <SuperAdminApiListView
    title="Quản lý học kỳ"
    subtitle="Danh sách học kỳ lấy từ API master-data."
    source-label="GET /api/master-data/academic-terms"
    action-note="P16B.2 chỉ bật tải dữ liệu thật. Tạo, sửa, khóa học kỳ cần audit action riêng trước khi đưa vào claim full action/API."
    :columns="columns"
    :loader="loadSemesters"
  />
</template>
