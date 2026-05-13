import { ref, computed } from 'vue'
import endorsementSetupApi from '@/api/endorsementSetupApi'

export function usePartnerCheck() {
  const checking = ref(false)
  const checked = ref(false)
  const failed = ref(false)
  const checkResult = ref(null)

  // Latency badge — null means no badge shown
  const latencyBadge = computed(() => {
    if (!checkResult.value || !checkResult.value.reachable) return null
    const ms = checkResult.value.latencyMs
    if (ms < 200) return null
    if (ms < 400) return { text: 'Slight delay', color: '#d97706', bg: 'rgba(253,230,138,0.3)' }
    if (ms < 600) return { text: 'Delayed', color: '#b45309', bg: 'rgba(253,211,77,0.25)' }
    return { text: 'Severe delay', color: '#ea580c', bg: 'rgba(254,215,170,0.3)' }
  })

  // Overall pass/fail — all three checks must pass
  const passed = computed(() => {
    if (!checkResult.value) return false
    return checkResult.value.reachable &&
      checkResult.value.localSectionExistsOnRemote &&
      checkResult.value.remoteProcessingSectionExistsLocally
  })

  const blocked = computed(() => checked.value && !passed.value)

  const statusBanner = computed(() => {
    if (!checked.value) return null

    if (!checkResult.value?.reachable) {
      return {
        type: 'error',
        icon: 'wifi_off',
        text: 'Failed to connect',
        color: '#ef4444',
        bg: 'rgba(239,68,68,0.08)',
        border: 'rgba(239,68,68,0.3)',
        errors: checkResult.value?.errors ?? [],
        showRetry: true
      }
    }

    if (!passed.value) {
      return {
        type: 'error',
        icon: 'link_off',
        text: 'Setup incomplete',
        color: '#ef4444',
        bg: 'rgba(239,68,68,0.08)',
        border: 'rgba(239,68,68,0.3)',
        errors: checkResult.value?.errors ?? [],
        showRetry: true
      }
    }

    return {
      type: 'success',
      icon: 'check_circle',
      text: 'Connected',
      color: 'var(--color-success)',
      bg: 'var(--color-success-soft)',
      border: 'var(--color-success)',
      errors: [],
      showRetry: false
    }
  })

  async function runCheck(branchCode, sectionCode) {
    checking.value = true
    checked.value = false
    failed.value = false
    checkResult.value = null

    try {
      checkResult.value = await endorsementSetupApi.checkPartner(branchCode, sectionCode)
      checked.value = true
      failed.value = !passed.value
    } catch {
      checkResult.value = {
        reachable: false,
        latencyMs: 0,
        localSectionExistsOnRemote: false,
        remoteProcessingSectionExistsLocally: false,
        remoteProcessingSectionCode: null,
        errors: ['Unexpected error during connection check.']
      }
      checked.value = true
      failed.value = true
    } finally {
      checking.value = false
    }
  }

  function reset() {
    checking.value = false
    checked.value = false
    failed.value = false
    checkResult.value = null
  }

  return {
    checking,
    checked,
    failed,
    checkResult,
    latencyBadge,
    passed,
    blocked,
    statusBanner,
    runCheck,
    reset
  }
}
