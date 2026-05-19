<template>
  <div v-if="isVisible" class="fixed inset-0 z-50 flex items-center justify-center p-4">
    <!-- Backdrop -->
    <div class="absolute inset-0 bg-black/40 backdrop-blur-sm" @click="emit('close')"></div>

    <!-- Modal -->
    <div class="relative w-full max-w-md rounded-2xl shadow-2xl p-6 flex flex-col gap-4 animate-modal"
         style="background-color: var(--color-surface);">

      <!-- Header -->
      <div class="flex items-center gap-3">
        <span class="material-symbols-outlined text-2xl" style="color: var(--color-error, #dc2626);">cancel</span>
        <div>
          <p class="font-bold text-sm" style="color: var(--color-text);">Cancel Specimen</p>
          <p class="text-xs font-mono mt-0.5" style="color: var(--color-text-muted);">{{ specimenNo }}</p>
        </div>
      </div>

      <!-- Warning text -->
      <p class="text-xs" style="color: var(--color-text-muted);">
        This will cancel the specimen and all its pending tests in this section. Released tests will not be affected. This action cannot be undone.
      </p>

      <!-- Reason field -->
      <div class="flex flex-col gap-1.5">
        <label class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">
          Reason <span style="color: var(--color-error, #dc2626);">*</span>
        </label>
        <textarea v-model="localReason"
                  rows="3"
                  placeholder="Enter cancellation reason..."
                  class="w-full px-3 py-2 rounded-xl text-sm outline-none resize-none transition-all"
                  style="background-color: var(--color-surface-low); border: 1.5px solid var(--color-border); color: var(--color-text);" />
      </div>

      <!-- Actions -->
      <div class="flex gap-3 justify-end">
        <button class="px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                @click="emit('close')">
          Cancel
        </button>
        <button class="px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 disabled:opacity-40"
                style="background-color: var(--color-error, #dc2626); color: #fff;"
                :disabled="!localReason.trim() || loading"
                @click="handleConfirm">
          <span v-if="loading" class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
          <span v-else>Confirm Cancel</span>
        </button>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'

const props = defineProps({
  isVisible:  { type: Boolean, default: false },
  specimenNo: { type: String,  default: '' },
  loading:    { type: Boolean, default: false },
})

const emit = defineEmits(['confirm', 'close'])

const localReason = ref('')

// Reset reason each time the modal opens
watch(() => props.isVisible, (val) => {
  if (val) localReason.value = ''
})

function handleConfirm() {
  if (!localReason.value.trim() || props.loading) return
  emit('confirm', localReason.value.trim())
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
