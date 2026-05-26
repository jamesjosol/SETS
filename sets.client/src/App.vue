<template>
  <RouterView />

  <OfflineOverlay :show="isOffline"
                  :is-updating="isUpdating"
                  :retrying="retrying"
                  :countdown="countdown" />
</template>

<script setup>
  import { RouterView } from 'vue-router'
  import { useTheme } from '@/composables/useTheme'
  import AlertModal from '@/components/common/AlertModal.vue'
  import { useGlobalAlert } from '@/composables/useGlobalAlert'
  import OfflineOverlay from '@/components/common/OfflineOverlay.vue'
  import { useOfflineDetector } from '@/composables/useOfflineDetector'

  const { isOffline, isUpdating, retrying, countdown } = useOfflineDetector()
  const { alert, hideAlert } = useGlobalAlert()
  const { initTheme } = useTheme()
  initTheme()
</script>


