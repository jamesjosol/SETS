<template>
  <Transition name="fade">
    <div v-if="checking || statusBanner"
         class="flex items-center gap-2 px-3 py-1.5 rounded-xl"
         :style="checking
           ? 'background-color: var(--color-surface-low); border: 1px solid var(--color-border)'
           : `background-color: ${statusBanner.bg}; border: 1px solid ${statusBanner.border}`">

      <!-- Checking -->
      <template v-if="checking">
        <span class="material-symbols-outlined animate-spin"
              style="font-size: 14px; color: var(--color-text-muted)">progress_activity</span>
        <span class="text-xs font-bold" style="color: var(--color-text-muted)">Checking...</span>
      </template>

      <!-- Result -->
      <template v-else-if="statusBanner">
        <span class="material-symbols-outlined"
              style="font-size: 14px"
              :style="`color: ${statusBanner.color}`">
          {{ statusBanner.icon }}
        </span>

        <span class="text-xs font-bold" :style="`color: ${statusBanner.color}`">
          {{ statusBanner.text }}
        </span>

        <!-- Latency badge -->
        <span v-if="latencyBadge"
              class="text-[10px] font-bold px-2 py-0.5 rounded-full"
              :style="`background-color: ${latencyBadge.bg}; color: ${latencyBadge.color}`">
          {{ latencyBadge.text }}
        </span>

        <!-- Info icon with hover tooltip for errors -->
        <div v-if="statusBanner.errors?.length" class="relative group">
          <span class="material-symbols-outlined cursor-pointer"
                style="font-size: 14px"
                :style="`color: ${statusBanner.color}`">info</span>
          <div class="absolute left-0 top-full mt-1 z-50 rounded-xl p-3 space-y-1 hidden group-hover:block"
               style="background-color: var(--color-surface); border: 1px solid var(--color-border); min-width: 260px; box-shadow: 0 4px 12px var(--color-shadow)">
            <p v-for="(err, i) in statusBanner.errors"
               :key="i"
               class="text-[11px]"
               :style="`color: ${statusBanner.color}`">
              · {{ err }}
            </p>
          </div>
        </div>

        <!-- Retry -->
        <button v-if="statusBanner.showRetry"
                @click="$emit('retry')"
                class="flex items-center gap-1 px-2 py-1 rounded-lg text-[10px] font-bold uppercase tracking-widest transition-all active:scale-95"
                :style="`color: ${statusBanner.color}; border: 1px solid ${statusBanner.border}; background: transparent`">
          <span class="material-symbols-outlined" style="font-size: 12px">refresh</span>
          Retry
        </button>
      </template>

    </div>
  </Transition>
</template>

<script setup>
defineProps({
  checking: Boolean,
  statusBanner: Object,
  latencyBadge: Object
})
defineEmits(['retry'])
</script>

<style scoped>
.fade-enter-active, .fade-leave-active { transition: opacity 0.2s ease; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
</style>
