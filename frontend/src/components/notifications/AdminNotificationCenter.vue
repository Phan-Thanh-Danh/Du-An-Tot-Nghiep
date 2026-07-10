<script setup>
import { ref } from 'vue'
import NotificationComposeForm from './NotificationComposeForm.vue'
import RecipientPreviewPanel from './RecipientPreviewPanel.vue'
import { notificationsApi } from '@/services/notificationsApi'



const previewData = ref(null)
const loadingPreview = ref(false)
const loadingSubmit = ref(false)
const successMessage = ref('')
const errorMessage = ref('')

const handlePreview = async (formPayload) => {
  loadingPreview.value = true
  errorMessage.value = ''
  successMessage.value = ''
  try {
    const data = await notificationsApi.previewRecipients(formPayload)
    previewData.value = data
  } catch (err) {
    errorMessage.value = err.message || 'Lỗi khi lấy dữ liệu preview'
  } finally {
    loadingPreview.value = false
  }
}

const handleSubmit = async (formPayload) => {
  if (previewData.value?.tongNguoiNhan === 0) {
    errorMessage.value = 'Không thể gửi thông báo cho 0 người nhận'
    return
  }

  if (!confirm('Bạn có chắc chắn muốn gửi thông báo này không? Thao tác này không thể hoàn tác hoàn toàn.')) {
    return
  }

  loadingSubmit.value = true
  errorMessage.value = ''
  successMessage.value = ''
  try {
    await notificationsApi.createNotification(formPayload)
    successMessage.value = 'Đã gửi thông báo thành công!'
    previewData.value = null // reset
    // In a real app, we might reset the form or redirect
  } catch (err) {
    errorMessage.value = err.message || 'Lỗi khi gửi thông báo'
  } finally {
    loadingSubmit.value = false
  }
}
</script>

<template>
  <div class="h-full flex flex-col p-4 md:p-6 bg-(--surface-page) min-h-[80vh]">
    <div class="mb-6">
      <h1 class="text-2xl font-bold text-(--text-heading)">Trung tâm thông báo Admin</h1>
      <p class="text-(--text-muted) mt-1">Gửi và quản lý thông báo toàn hệ thống hoặc theo cơ sở.</p>
    </div>

    <!-- Alerts -->
    <div v-if="errorMessage" class="mb-4 p-3 rounded-lg bg-(--color-danger-bg) text-(--color-danger-text) border border-(--color-danger-border)">
      {{ errorMessage }}
    </div>
    <div v-if="successMessage" class="mb-4 p-3 rounded-lg bg-(--color-success-bg) text-(--color-success-text) border border-(--color-success-border)">
      {{ successMessage }}
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 items-start">
      <div class="lg:col-span-2">
        <NotificationComposeForm
          :loading="loadingSubmit"
          @preview="handlePreview"
          @submit="handleSubmit"
        />
      </div>
      <div class="lg:col-span-1 sticky top-6">
        <RecipientPreviewPanel
          :previewData="previewData"
          :loading="loadingPreview"
        />
      </div>
    </div>
  </div>
</template>
