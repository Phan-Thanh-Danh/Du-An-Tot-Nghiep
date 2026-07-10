<script setup>
import SuperAdminApiListView from './components/SuperAdminApiListView.vue'
import { apiRequest, unwrapApiData } from '@/services/apiClient'

const columns = [
  { key: 'time', label: 'Thời gian' },
  { key: 'actor', label: 'Người thao tác' },
  { key: 'action', label: 'Hành động' },
  { key: 'entity', label: 'Đối tượng' },
  { key: 'description', label: 'Mô tả' },
]

function unwrapItems(response) {
  const data = unwrapApiData(response)
  if (Array.isArray(data)) return data
  if (Array.isArray(data?.items)) return data.items
  if (Array.isArray(data?.Items)) return data.Items
  return []
}

function formatDateTime(value) {
  if (!value) return 'Chưa có dữ liệu'
  return new Date(value).toLocaleString('vi-VN')
}

async function loadAuditLogs() {
  const response = await apiRequest('/api/audit-logs?pageSize=100')
  return unwrapItems(response).map((log) => ({
    id: log.id || log.Id,
    time: formatDateTime(log.changedAt || log.ChangedAt),
    actor: log.changedByName || log.ChangedByName || String(log.changedBy || log.ChangedBy || 'Hệ thống'),
    action: log.action || log.Action || 'Chưa có dữ liệu',
    entity: `${log.entityType || log.EntityType || 'Entity'} #${log.entityId || log.EntityId || ''}`,
    description: log.description || log.Description || 'Chưa có dữ liệu',
  }))
}
</script>

<template>
  <SuperAdminApiListView
    title="Nhật ký hệ thống"
    subtitle="Audit log lấy trực tiếp từ backend."
    source-label="GET /api/audit-logs"
    action-note="Bộ lọc nâng cao và xuất file sẽ được kiểm ở P16 action coverage; P16B.2 chỉ giữ dữ liệu đọc từ audit API thật."
    :columns="columns"
    :loader="loadAuditLogs"
  />
</template>
