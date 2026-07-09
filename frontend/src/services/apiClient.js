function getApiBaseUrl() {
  return (import.meta.env.VITE_API_BASE_URL || '').replace(/\/$/, '')
}

const AUTH_REFRESH_PATHS = new Set([
  '/api/auth/login',
  '/api/auth/refresh-token',
  '/api/auth/logout',
])

export class ApiError extends Error {
  constructor(message, statusCode, details = null) {
    super(message)
    this.name = 'ApiError'
    this.statusCode = statusCode
    this.details = details
  }
}

export function unwrapApiData(response) {
  return response?.data ?? response?.Data ?? response
}

function getStoredAccessToken() {
  return localStorage.getItem('lms_access_token') || sessionStorage.getItem('lms_access_token') || ''
}

async function parseResponse(response) {
  if (response.status === 204) return null

  const contentType = response.headers.get('content-type') || ''

  if (contentType.includes('application/json')) {
    return response.json()
  }

  const text = await response.text()
  return text ? { message: text } : null
}

function buildApiError(data, statusCode) {
  const message =
    data?.message ||
    data?.Message ||
    data?.title ||
    data?.errors?.[0] ||
    'Không thể xử lý yêu cầu.'

  return new ApiError(message, statusCode, data)
}

async function tryRefreshToken() {
  try {
    const { useAuthStore } = await import('@/stores/auth')
    const authStore = useAuthStore()
    await authStore.refreshSession()
    return true
  } catch {
    try {
      const { useAuthStore } = await import('@/stores/auth')
      const authStore = useAuthStore()
      authStore.clearSession()
    } catch {
      // Không chặn lỗi gốc nếu Pinia chưa sẵn sàng.
    }
    return false
  }
}

async function sendRequest(path, options = {}, state = { retried: false }) {
  const headers = new Headers(options.headers || {})

  if (!headers.has('Content-Type') && options.body && !(options.body instanceof FormData)) {
    headers.set('Content-Type', 'application/json')
  }

  const token = options.token || getStoredAccessToken()
  if (token && !headers.has('Authorization')) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  const response = await fetch(`${getApiBaseUrl()}${path}`, {
    ...options,
    headers,
  })

  const data = await parseResponse(response)

  const shouldTryRefresh =
    response.status === 401 &&
    !options.skipAuthRefresh &&
    !state.retried &&
    !AUTH_REFRESH_PATHS.has(path)

  if (shouldTryRefresh) {
    const refreshed = await tryRefreshToken()
    if (refreshed) {
      return sendRequest(path, options, { retried: true })
    }
  }

  if (!response.ok) {
    throw buildApiError(data, response.status)
  }

  return data
}

export async function apiRequest(path, options = {}) {
  return sendRequest(path, options, { retried: false })
}

export const authApi = {
  login(payload) {
    return apiRequest('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify(payload),
      skipAuthRefresh: true,
    })
  },

  refreshToken(payload) {
    return apiRequest('/api/auth/refresh-token', {
      method: 'POST',
      body: JSON.stringify(payload),
      skipAuthRefresh: true,
    })
  },

  logout(payload) {
    return apiRequest('/api/auth/logout', {
      method: 'POST',
      body: JSON.stringify(payload),
      skipAuthRefresh: true,
    })
  },

  changePassword(payload) {
    return apiRequest('/api/auth/change-password', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },
}

export const storageApi = {
  upload(file, folder = 'general') {
    const formData = new FormData()
    formData.append('file', file)
    return apiRequest(`/api/storage/upload?folder=${folder}`, {
      method: 'POST',
      body: formData,
    })
  }
}
