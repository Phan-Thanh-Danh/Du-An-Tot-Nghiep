<script setup>
import SuperAdminEndpointPendingView from './components/SuperAdminEndpointPendingView.vue'
</script>

<template>
  <SuperAdminEndpointPendingView
    title="Cấu hình đánh giá"
    subtitle="Không dùng dữ liệu biểu mẫu cục bộ khi chưa có API cấu hình."
    required-api="Cần bổ sung API evaluation config: danh sách biểu mẫu, tiêu chí, cập nhật và xuất bản."
  />
</template>
