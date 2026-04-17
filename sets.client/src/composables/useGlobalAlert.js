import { ref } from 'vue'

const alert = ref({
  isVisible: false,
  type: 'error',
  title: '',
  message: ''
})

export function useGlobalAlert() {
  function showAlert(type, title, message) {
    alert.value = { isVisible: true, type, title, message }
  }

  function hideAlert() {
    alert.value.isVisible = false
  }

  return { alert, showAlert, hideAlert }
}
