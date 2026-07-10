<script setup>
import SuperAdminEndpointPendingView from './components/SuperAdminEndpointPendingView.vue'
</script>

<template>
  <SuperAdminEndpointPendingView
    title="Quy tắc đạt/rớt"
    subtitle="Quy tắc học vụ cần được lấy từ backend trước khi claim full API."
    required-api="Cần bổ sung API cấu hình pass/fail: danh sách rule, cập nhật rule, phiên bản hiệu lực."
  />
</template>
