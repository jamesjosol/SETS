import { ref, onMounted, onUnmounted } from 'vue'
import axios from 'axios'

const PING_URL = '/api/health/status'
const PING_INTERVAL = 10_000
const RETRY_COUNTDOWN = 10
const RELOAD_INTERVAL = 10_000  // hard reload every 20s while offline

export function useOfflineDetector() {
  const isOffline = ref(false)
  const isUpdating = ref(false)
  const retrying = ref(false)
  const countdown = ref(RETRY_COUNTDOWN)

  let pingTimer = null
  let countdownTimer = null
  let reloadTimer = null  // fires while offline to catch system coming back

  async function ping() {
    try {
      await axios.get(PING_URL, { timeout: 5000 })
      if (isOffline.value) {
        isOffline.value = false
        isUpdating.value = false
        window.location.reload()
      }
    } catch (err) {
      const status = err?.response?.status
      const isHtmlResponse = err?.response?.headers?.['content-type']?.includes('text/html')

      isUpdating.value = status === 503 || isHtmlResponse === true
      isOffline.value = true

      startCountdown()
      startReloadTimer()  // begin hard reload cycle
    }
  }

  function startCountdown() {
    clearInterval(countdownTimer)
    countdown.value = RETRY_COUNTDOWN
    retrying.value = false

    countdownTimer = setInterval(() => {
      countdown.value--
      if (countdown.value <= 0) {
        clearInterval(countdownTimer)
        retrying.value = true
        ping()
      }
    }, 1000)
  }

  function startReloadTimer() {
    // Only start once — don't stack multiple reload timers
    if (reloadTimer) return
    reloadTimer = setInterval(() => {
      if (isOffline.value) {
        window.location.reload()
      } else {
        clearInterval(reloadTimer)
        reloadTimer = null
      }
    }, RELOAD_INTERVAL)
  }

  onMounted(() => {
    pingTimer = setInterval(ping, PING_INTERVAL)
  })

  onUnmounted(() => {
    clearInterval(pingTimer)
    clearInterval(countdownTimer)
    clearInterval(reloadTimer)
  })

  return { isOffline, isUpdating, retrying, countdown }
}
