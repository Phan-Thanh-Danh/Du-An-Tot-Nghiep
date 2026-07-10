<script setup>
import SuperAdminApiListView from './components/SuperAdminApiListView.vue'
import { apiRequest, unwrapApiData } from '@/services/apiClient'

const columns = [
  { key: 'code', label: 'Mã CTĐT' },
  { key: 'name', label: 'Tên chương trình' },
  { key: 'major', label: 'Ngành/Chuyên ngành' },
  { key: 'version', label: 'Phiên bản' },
  { key: 'credits', label: 'Tín chỉ' },
  { key: 'status', label: 'Trạng thái' },
]

function unwrapItems(response) {
  const data = unwrapApiData(response)
  if (Array.isArray(data)) return data
  if (Array.isArray(data?.items)) return data.items
  if (Array.isArray(data?.Items)) return data.Items
  return []
}

async function loadPrograms() {
  const response = await apiRequest('/api/master-data/training-programs?pageSize=100')
  return unwrapItems(response).map((program) => ({
    id: program.maChuongTrinh || program.MaChuongTrinh,
    code: program.maCodeChuongTrinh || program.MaCodeChuongTrinh || 'Chưa có dữ liệu',
    name: program.tenChuongTrinh || program.TenChuongTrinh || 'Chưa có dữ liệu',
    major: program.tenChuyenNganh || program.TenChuyenNganh || program.tenNganh || program.TenNganh || 'Chưa có dữ liệu',
    version: program.version || program.Version || 'Chưa có dữ liệu',
    credits: program.tongTinChiYeuCau || program.TongTinChiYeuCau || 'Chưa có dữ liệu',
    status: program.trangThai || program.TrangThai || 'Chưa có dữ liệu',
  }))
}
</script>

<template>
  <SuperAdminApiListView
    title="Khung chương trình đào tạo"
    subtitle="Danh sách chương trình đào tạo lấy từ API master-data."
    source-label="GET /api/master-data/training-programs"
    action-note="P16B.2 loại bỏ dữ liệu cục bộ. Các thao tác clone/submit/approve môn trong khung sẽ được audit action/API ở bước riêng."
    :columns="columns"
    :loader="loadPrograms"
  />
</template>
