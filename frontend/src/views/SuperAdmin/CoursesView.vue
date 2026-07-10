<script setup>
import SuperAdminApiListView from './components/SuperAdminApiListView.vue'
import { courseApi } from '@/services/courseApi'
import { unwrapApiData } from '@/services/apiClient'

const columns = [
  { key: 'code', label: 'Mã khóa học' },
  { key: 'title', label: 'Tên khóa học' },
  { key: 'subject', label: 'Môn học' },
  { key: 'teacher', label: 'Giảng viên' },
  { key: 'status', label: 'Trạng thái' },
]

function unwrapItems(response) {
  const data = unwrapApiData(response)
  if (Array.isArray(data)) return data
  if (Array.isArray(data?.items)) return data.items
  if (Array.isArray(data?.Items)) return data.Items
  return []
}

async function loadCourses() {
  const response = await courseApi.getCourses({ pageSize: 100 })
  return unwrapItems(response).map((course) => ({
    id: course.maKhoaHoc || course.MaKhoaHoc,
    code: course.maCodeKhoaHoc || course.MaCodeKhoaHoc || String(course.maKhoaHoc || course.MaKhoaHoc || ''),
    title: course.tieuDe || course.TieuDe || course.tenKhoaHoc || course.TenKhoaHoc || 'Chưa có dữ liệu',
    subject: course.tenMonHoc || course.TenMonHoc || course.maCodeMonHoc || course.MaCodeMonHoc || 'Chưa có dữ liệu',
    teacher: course.tenGiaoVien || course.TenGiaoVien || 'Chưa phân công',
    status: course.trangThai || course.TrangThai || 'Chưa có dữ liệu',
  }))
}
</script>

<template>
  <SuperAdminApiListView
    title="Quản lý khóa học"
    subtitle="Danh sách khóa học lấy từ API Courses."
    source-label="GET /api/courses"
    action-note="Các thao tác phân công, công bố và lưu trữ sẽ được kiểm riêng theo quyền và payload thật trước khi bật trong claim full action/API."
    :columns="columns"
    :loader="loadCourses"
  />
</template>
