// src/api/axiosInstance.js
import axios from 'axios'
import { useAuthStore } from '@/stores/authStore'
import { useGlobalAlert } from '@/composables/useGlobalAlert'
import router from '@/router'

const instance = axios.create({
  withCredentials: true
})

instance.interceptors.response.use(
  (response) => response,
  (error) => {
    const isLoginRequest = error.config?.url?.includes('/api/Auth/login')

    if (error.response?.status === 401 && !isLoginRequest) {
      const { showAlert } = useGlobalAlert()
      const authStore = useAuthStore()

      showAlert('error', 'Session Expired', 'Your session has expired. Please log in again.')

      // Small delay so the alert is visible before the page transitions
      setTimeout(() => {
        authStore.logout()
        router.push('/')
      }, 2000)
    }
    return Promise.reject(error)
  }
)

export default instance
