<script setup>
import SuperAdminEndpointPendingView from './components/SuperAdminEndpointPendingView.vue'
</script>

<template>
  <SuperAdminEndpointPendingView
    title="Lịch sử phê duyệt"
    subtitle="Cần API lịch sử phê duyệt tổng hợp trước khi bật claim."
    required-api="Cần bổ sung API approval history: danh sách, bộ lọc, chi tiết và audit người duyệt."
  />
</template>
