<template>
  <div class="flex-1 flex flex-col overflow-hidden">

    <!-- Header -->
    <div class="px-7 py-5 flex-shrink-0"
         style="border-bottom: 1px solid var(--color-border); background-color: var(--color-surface);">
      <h1 class="text-base font-bold" style="color: var(--color-text);">{{ label }}</h1>
      <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">{{ sublabel }}</p>
    </div>

    <!-- Placeholder -->
    <div class="flex-1 flex flex-col items-center justify-center gap-4">
      <span ref="iconRef"
            class="material-symbols-outlined text-6xl"
            style="color: var(--color-text-muted);">engineering</span>
      <p class="text-sm font-bold" style="color: var(--color-text);">This report is in the build queue</p>
      <p class="text-xs text-center max-w-xs" style="color: var(--color-text-muted);">
        Pending clarification or implementation. Check back once all requirements are confirmed.
      </p>
    </div>

  </div>
</template>

<script setup>
  import { ref, onMounted } from 'vue'
  import { gsap } from 'gsap'

  defineProps({
    label:    { type: String, default: '' },
    sublabel: { type: String, default: '' },
  })

  const iconRef = ref(null)

  onMounted(() => {
    if (!iconRef.value) return
    gsap.set(iconRef.value, { scale: 0.7, opacity: 0 })
    gsap.to(iconRef.value, {
      scale: 1,
      opacity: 1,
      duration: 0.38,
      ease: 'back.out(2)',
    })
  })
</script>
