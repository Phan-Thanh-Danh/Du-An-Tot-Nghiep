import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { authApi } from '@/services/apiClient'

const ACCESS_TOKEN_KEY = 'lms_access_token'
const AUTH_USER_KEY = 'lms_auth_user'
const TOKEN_EXPIRES_AT_KEY = 'lms_token_expires_at'
const REQUIRES_PASSWORD_CHANGE_KEY = 'lms_requires_password_change'
const REMEMBER_LOGIN_KEY = 'lms_remember_login'

function readJson(key) {
  try {
    const value = localStorage.getItem(key) || sessionStorage.getItem(key)
    return value ? JSON.parse(value) : null
  } catch {
    return null
  }
}

function readStoredValue(key) {
  return localStorage.getItem(key) || sessionStorage.getItem(key) || ''
}

function clearStoredSession() {
  const storages = [localStorage, sessionStorage]

  for (const storage of storages) {
    storage.removeItem(ACCESS_TOKEN_KEY)
    storage.removeItem(AUTH_USER_KEY)
    storage.removeItem(TOKEN_EXPIRES_AT_KEY)
    storage.removeItem(REQUIRES_PASSWORD_CHANGE_KEY)
  }
}

function isExpired(expiresAt) {
  if (!expiresAt) return true

  const expiresAtMs = new Date(expiresAt).getTime()
  if (Number.isNaN(expiresAtMs)) return true

  return expiresAtMs <= Date.now()
}

function normalizeRole(role) {
  return String(role || '').trim().toLowerCase()
}

export const useAuthStore = defineStore('auth', () => {
  const accessToken = ref(readStoredValue(ACCESS_TOKEN_KEY))
  const user = ref(readJson(AUTH_USER_KEY))
  const expiresAt = ref(readStoredValue(TOKEN_EXPIRES_AT_KEY))
  const requiresPasswordChange = ref(
    readStoredValue(REQUIRES_PASSWORD_CHANGE_KEY) === 'true',
  )
  const rememberLogin = ref(localStorage.getItem(REMEMBER_LOGIN_KEY) === 'true')
  const loading = ref(false)
  const error = ref('')

  const isAuthenticated = computed(() => Boolean(accessToken.value) && !isExpired(expiresAt.value))
  const role = computed(() => user.value?.role || '')
  const displayName = computed(() => user.value?.fullName || user.value?.email || 'Người dùng')
  const initials = computed(() => {
    const name = displayName.value.trim()
    if (!name) return 'U'

    return name
      .split(/\s+/)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase())
      .join('')
  })

  function persistSession(response, remember = false) {
    accessToken.value = response.accessToken || ''
    user.value = response.user || null
    expiresAt.value = response.expiresAt || ''
    requiresPasswordChange.value = Boolean(response.requiresPasswordChange)
    rememberLogin.value = Boolean(remember)

    clearStoredSession()
    localStorage.setItem(REMEMBER_LOGIN_KEY, String(rememberLogin.value))

    const storage = rememberLogin.value ? localStorage : sessionStorage
    storage.setItem(ACCESS_TOKEN_KEY, accessToken.value)
    storage.setItem(AUTH_USER_KEY, JSON.stringify(user.value))
    storage.setItem(TOKEN_EXPIRES_AT_KEY, expiresAt.value)
    storage.setItem(REQUIRES_PASSWORD_CHANGE_KEY, String(requiresPasswordChange.value))
  }

  function clearSession() {
    accessToken.value = ''
    user.value = null
    expiresAt.value = ''
    requiresPasswordChange.value = false
    error.value = ''

    clearStoredSession()
  }

  async function login(credentials, options = {}) {
    loading.value = true
    error.value = ''

    // ── MOCK LOGIN CHO QUÁ TRÌNH PHÁT TRIỂN ──
    const email = credentials.email.trim().toLowerCase()
    
    if (email === 'student' || email === 'teacher' || email === 'staff' || email === 'bgh') {
      let mockUser = {}
      
      if (email === 'student') {
        mockUser = {
          userId: 999, email: 'student@mock.local', fullName: 'Sinh Viên Demo',
          role: 'Student', campusId: 1, status: 'Active'
        }
      } else if (email === 'teacher') {
        mockUser = {
          userId: 888, email: 'teacher@mock.local', fullName: 'TS. Nguyễn Minh Khoa',
          role: 'Teacher', campusId: 1, status: 'Active'
        }
      } else if (email === 'staff') {
        mockUser = {
          userId: 777, email: 'staff@mock.local', fullName: 'Trần Thị Giáo Vụ',
          role: 'AcademicStaff', campusId: 1, status: 'Active'
        }
      } else if (email === 'bgh') {
        mockUser = {
          userId: 666, email: 'principal@mock.local', fullName: 'Nguyễn Văn Hiệu Trưởng',
          role: 'Principal', campusId: 1, status: 'Active'
        }
      }

      const mockResponse = {
        accessToken: `mock_token_${email}_${Date.now()}`,
        user: mockUser,
        expiresAt: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(),
        requiresPasswordChange: false
      }

      persistSession(mockResponse, Boolean(options.remember))
      loading.value = false
      return mockResponse
    }

    try {
      const response = await authApi.login(credentials)
      persistSession(response, Boolean(options.remember))
      return response
    } catch (err) {
      clearSession()
      error.value = err?.message || 'Đăng nhập không thành công.'
      throw err
    } finally {
      loading.value = false
    }
  }

  function logout() {
    clearSession()
  }

  function hasRole(expectedRole) {
    if (!expectedRole) return true

    const expectedRoles = Array.isArray(expectedRole) ? expectedRole : [expectedRole]
    const currentRole = normalizeRole(role.value)

    return expectedRoles.some((item) => normalizeRole(item) === currentRole)
  }

  function ensureFreshSession() {
    if (accessToken.value && isExpired(expiresAt.value)) {
      clearSession()
    }
  }

  return {
    accessToken,
    user,
    expiresAt,
    requiresPasswordChange,
    rememberLogin,
    loading,
    error,
    isAuthenticated,
    role,
    displayName,
    initials,
    login,
    logout,
    hasRole,
    ensureFreshSession,
  }
})
