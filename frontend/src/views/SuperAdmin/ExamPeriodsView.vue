<script setup>
import SuperAdminEndpointPendingView from './components/SuperAdminEndpointPendingView.vue'
</script>

<template>
  <SuperAdminEndpointPendingView
    title="Cấu hình kỳ thi"
    subtitle="Cần endpoint quản trị kỳ thi trước khi bật dữ liệu nghiệp vụ."
    required-api="Cần bổ sung API quản trị kỳ thi: danh sách, tạo, cập nhật, mở/đóng kỳ thi."
  />
</template>
