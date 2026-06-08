<template>
  <div v-if="isVisible" class="fixed inset-0 z-50 flex items-center justify-center p-4">
    <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"></div>
    <div class="relative w-full max-w-sm rounded-2xl shadow-2xl p-6 flex flex-col gap-4 animate-modal"
         style="background-color: var(--color-surface);">

      <!-- Header -->
      <div class="flex items-center gap-3">
        <div class="p-2 rounded-xl" style="background-color: var(--color-primary-soft)">
          <span class="material-symbols-outlined" style="color: var(--color-primary)">
            {{ isOutbound ? 'alt_route' : 'arrow_forward' }}
          </span>
        </div>
        <h3 class="text-base font-bold" style="color: var(--color-text);">Confirm Endorsement</h3>
      </div>

      <!-- Body -->
      <div class="flex flex-col gap-3">
        <p class="text-sm" style="color: var(--color-text-muted);">
          You are about to endorse to the following destination. Please confirm it is correct.
        </p>

        <!-- Destination highlight -->
        <div class="flex flex-col gap-1 px-4 py-3 rounded-xl"
             style="background-color: var(--color-surface-low); border: 1.5px solid var(--color-primary);">
          <p class="text-[10px] font-bold uppercase tracking-widest"
             style="color: var(--color-text-muted);">
            {{ isOutbound ? 'Destination Branch' : 'Destination' }}
          </p>
          <p class="text-lg font-extrabold uppercase tracking-wide"
             style="color: var(--color-primary);">
            {{ destination }}
          </p>
        </div>

        <!-- Outbound warning note -->
        <p v-if="isOutbound"
           class="text-xs flex items-center gap-1.5"
           style="color: var(--color-text-muted);">
          <span class="material-symbols-outlined text-sm" style="color: var(--color-warning);">
            info
          </span>
          This batch will be sent to an external branch.
        </p>
      </div>

      <!-- Buttons -->
      <div class="flex gap-3">
        <button class="flex-1 py-3 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                @click="handleNo">
          Cancel
        </button>
        <button class="flex-1 py-3 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-[0.98]"
                style="background: var(--color-primary-gradient); color: #ffffff;"
                @click="handleYes">
          Yes, Endorse
        </button>
      </div>

    </div>
  </div>
</template>

<script setup>
defineProps({
  isVisible:   { type: Boolean, default: false },
  destination: { type: String,  default: '' },   // already formatted, will be shown ALL CAPS via CSS
  isOutbound:  { type: Boolean, default: false },
})

const emit = defineEmits(['confirm', 'cancel', 'close'])

function handleYes() {
  emit('confirm')
  emit('close')
}

function handleNo() {
  emit('cancel')
  emit('close')
}
</script>

<style scoped>
  .animate-modal {
    animation: modalIn 0.2s ease;
  }

  @keyframes modalIn {
    from {
      opacity: 0;
      transform: scale(0.9) translateY(10px);
    }

    to {
      opacity: 1;
      transform: scale(1) translateY(0);
    }
  }
</style>
