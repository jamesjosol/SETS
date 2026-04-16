<template>
  <div v-if="isVisible" class="fixed inset-0 z-50 flex items-center justify-center p-4">
    <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"></div>
    <div class="relative w-full max-w-sm rounded-2xl shadow-2xl p-6 flex flex-col gap-4 animate-modal"
         style="background-color: var(--color-surface);">

      <div class="flex items-center gap-3">
        <div class="p-2 rounded-xl" :style="{ backgroundColor: `var(--color-${type}-soft)` }">
          <span class="material-symbols-outlined" :style="{ color: `var(--color-${type})` }">
            {{ icon }}
          </span>
        </div>
        <h3 class="text-base font-bold" style="color: var(--color-text);">{{ title }}</h3>
      </div>

      <p class="text-sm" style="color: var(--color-text-muted);">{{ message }}</p>

      <div class="flex gap-3">
        <button class="flex-1 py-3 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                @click="handleNo">
          {{ cancelText }}
        </button>
        <button class="flex-1 py-3 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                :style="{ backgroundColor: `var(--color-${type})`, color: '#ffffff' }"
                @click="handleYes">
          {{ confirmText }}
        </button>
      </div>

    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  isVisible:   { type: Boolean, default: false },
  title:       { type: String,  default: 'Confirm' },
  message:     { type: String,  default: '' },
  type:        { type: String,  default: 'warning' }, // 'warning' | 'error' | 'primary'
  icon:        { type: String,  default: 'warning' }, // material symbol name
  confirmText: { type: String,  default: 'Yes, Continue' },
  cancelText:  { type: String,  default: 'No' },
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
