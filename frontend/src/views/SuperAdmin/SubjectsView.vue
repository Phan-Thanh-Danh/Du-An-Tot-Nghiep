<script setup>
import SuperAdminApiListView from './components/SuperAdminApiListView.vue'
import { subjectApi } from '@/services/subjectApi'

const columns = [
  { key: 'code', label: 'Mã môn' },
  { key: 'name', label: 'Tên môn học' },
  { key: 'credits', label: 'Tín chỉ' },
  { key: 'department', label: 'Đơn vị' },
  { key: 'status', label: 'Trạng thái' },
]

async function loadSubjects() {
  const subjects = await subjectApi.list({ pageSize: 100 })
  return subjects.map((subject) => ({
    id: subject.maMonHoc || subject.MaMonHoc,
    code: subject.maCodeMonHoc || subject.MaCodeMonHoc || String(subject.maMonHoc || subject.MaMonHoc || ''),
    name: subject.tenMonHoc || subject.TenMonHoc || 'Chưa có dữ liệu',
    credits: subject.soTinChi || subject.SoTinChi || 'Chưa có dữ liệu',
    department: subject.tenDonVi || subject.TenDonVi || 'Chưa có dữ liệu',
    status: (subject.conHoatDong ?? subject.ConHoatDong) === false ? 'Ngừng hoạt động' : 'Hoạt động',
  }))
}
</script>

<template>
  <SuperAdminApiListView
    title="Danh mục môn học"
    subtitle="Danh sách môn học lấy từ API master-data."
    source-label="GET /api/master-data/subjects"
    action-note="Tạo, sửa, kích hoạt môn học đã có service nhưng sẽ được kiểm action/API riêng để tránh claim quá phạm vi P16B.2."
    :columns="columns"
    :loader="loadSubjects"
  />
</template>
