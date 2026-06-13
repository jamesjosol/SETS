import { ref, onMounted, onUnmounted } from 'vue'
import axios from 'axios'

const PING_URL = '/api/health/status'
const PING_INTERVAL = 10_000
const RETRY_COUNTDOWN = 10
const RELOAD_INTERVAL = 30_000
const FAILURE_THRESHOLD = 5  // must fail this many times in a row before showing overlay

export function useOfflineDetector() {
  const isOffline = ref(false)
  const isUpdating = ref(false)
  const retrying = ref(false)
  const countdown = ref(RETRY_COUNTDOWN)

  let pingTimer = null
  let countdownTimer = null
  let reloadTimer = null
  let failureCount = 0      // consecutive failure counter
  let isPinging = false     // in-flight guard — prevents concurrent ping calls

  async function ping() {
    // Skip if a ping is already in progress.
    // Without this, the 10s interval and the countdown timer can fire simultaneously,
    // stacking concurrent requests and inflating failureCount artificially.
    if (isPinging) return
    isPinging = true

    try {
      await axios.get(PING_URL, { timeout: 5000 })

      // Reset failure streak on any success
      failureCount = 0

      if (isOffline.value) {
        isOffline.value = false
        isUpdating.value = false
        window.location.reload()
      }
    } catch (err) {
      failureCount++

      // Only show overlay after FAILURE_THRESHOLD consecutive failures.
      // This prevents brief fluctuations from interrupting the user.
      if (failureCount < FAILURE_THRESHOLD) return

      const status = err?.response?.status
      const isHtmlResponse = err?.response?.headers?.['content-type']?.includes('text/html')

      isUpdating.value = status === 503 || isHtmlResponse === true

      // Only initialize the overlay state and timers on the first triggering failure.
      // Without this guard, every subsequent interval ping while offline would call
      // startCountdown() again — resetting the countdown to 10 every 10 seconds
      // and preventing it from ever reaching 0.
      if (!isOffline.value) {
        isOffline.value = true
        startCountdown()
        startReloadTimer()
      }
    } finally {
      isPinging = false
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
