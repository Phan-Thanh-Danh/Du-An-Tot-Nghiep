function getApiBaseUrl() {
  return (import.meta.env.VITE_API_BASE_URL || '').replace(/\/$/, '')
}

export class ApiError extends Error {
  constructor(message, statusCode, details = null) {
    super(message)
    this.name = 'ApiError'
    this.statusCode = statusCode
    this.details = details
  }
}

async function parseResponse(response) {
  const contentType = response.headers.get('content-type') || ''

  if (contentType.includes('application/json')) {
    return response.json()
  }

  const text = await response.text()
  return text ? { message: text } : null
}

export async function apiRequest(path, options = {}) {
  const headers = new Headers(options.headers || {})

  if (!headers.has('Content-Type') && options.body) {
    headers.set('Content-Type', 'application/json')
  }

  const token =
    options.token ||
    localStorage.getItem('lms_access_token') ||
    sessionStorage.getItem('lms_access_token')
  if (token && !headers.has('Authorization')) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  const response = await fetch(`${getApiBaseUrl()}${path}`, {
    ...options,
    headers,
  })

  const data = await parseResponse(response)

  if (!response.ok) {
    throw new ApiError(data?.message || 'Không thể xử lý yêu cầu.', response.status, data)
  }

  return data
}

export const authApi = {
  login(payload) {
    return apiRequest('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  changePassword(payload) {
    return apiRequest('/api/auth/change-password', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },
}
