<template>
  <RouterView />

</template>

<script setup>
  import { RouterView } from 'vue-router'
  import { useTheme } from '@/composables/useTheme'
  import AlertModal from '@/components/common/AlertModal.vue'
  import { useGlobalAlert } from '@/composables/useGlobalAlert'

  const { alert, hideAlert } = useGlobalAlert()
  const { initTheme } = useTheme()
  initTheme()
</script>


