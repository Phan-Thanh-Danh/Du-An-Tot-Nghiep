<script setup>
import SuperAdminEndpointPendingView from './components/SuperAdminEndpointPendingView.vue'
</script>

<template>
  <SuperAdminEndpointPendingView
    title="Chính sách điểm danh"
    subtitle="Không hiển thị dữ liệu cấu hình khi backend policy endpoint chưa có."
    required-api="Cần bổ sung API chính sách điểm danh: danh sách rule, cập nhật rule, audit thay đổi."
  />
</template>
