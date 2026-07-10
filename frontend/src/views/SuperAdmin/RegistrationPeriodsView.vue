<script setup>
import SuperAdminApiListView from './components/SuperAdminApiListView.vue'
import { registrationApi } from '@/services/registrationApi'

const columns = [
  { key: 'name', label: 'Đợt đăng ký' },
  { key: 'term', label: 'Học kỳ' },
  { key: 'dateRange', label: 'Thời gian' },
  { key: 'status', label: 'Trạng thái' },
]

function formatDate(value) {
  if (!value) return 'Chưa có dữ liệu'
  return new Date(value).toLocaleDateString('vi-VN')
}

function unwrapItems(response) {
  if (Array.isArray(response)) return response
  if (Array.isArray(response?.items)) return response.items
  if (Array.isArray(response?.Items)) return response.Items
  return []
}

async function loadRegistrationPeriods() {
  const periods = unwrapItems(await registrationApi.getPeriods())
  return periods.map((period) => ({
    id: period.maDotDangKy || period.MaDotDangKy || period.id || period.Id,
    name: period.tenDotDangKy || period.TenDotDangKy || period.name || period.Name || 'Chưa có dữ liệu',
    term: period.tenHocKy || period.TenHocKy || period.hocKy || period.HocKy || 'Chưa có dữ liệu',
    dateRange: `${formatDate(period.ngayBatDau || period.NgayBatDau || period.startDate || period.StartDate)} - ${formatDate(period.ngayKetThuc || period.NgayKetThuc || period.endDate || period.EndDate)}`,
    status: period.trangThai || period.TrangThai || period.status || period.Status || 'Chưa có dữ liệu',
  }))
}
</script>

<template>
  <SuperAdminApiListView
    title="Đợt đăng ký học phần"
    subtitle="Danh sách đợt đăng ký lấy từ API admin registrations."
    source-label="GET /api/admin/registration-periods"
    action-note="P16B.2 giữ màn ở trạng thái load API thật; thao tác mở/đóng/xóa đợt sẽ được kiểm riêng trước khi claim full action/API."
    :columns="columns"
    :loader="loadRegistrationPeriods"
  />
</template>
