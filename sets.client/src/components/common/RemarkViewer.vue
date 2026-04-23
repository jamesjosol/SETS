<!-- RemarkViewer.vue -->
<template>
  <div v-if="isVisible" class="fixed inset-0 z-50 flex items-center justify-center p-4">
    <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"
         @click="emit('close')"></div>
    <div class="relative w-full max-w-sm rounded-2xl shadow-2xl p-6 flex flex-col gap-4 animate-viewer"
         style="background-color: var(--color-surface);">
      <div class="flex items-center gap-2">
        <div class="p-2 rounded-xl" style="background-color: rgba(217,119,6,0.1);">
          <span class="material-symbols-outlined text-sm"
                style="color: var(--color-warning);">chat_bubble</span>
        </div>
        <h3 class="text-sm font-bold" style="color: var(--color-text);">{{ title }}</h3>
      </div>
      <p class="text-sm rounded-xl p-4 whitespace-pre-wrap"
         style="background-color: var(--color-surface-low); color: var(--color-text);">
        {{ text }}
      </p>
      <button class="w-full py-3 rounded-xl text-xs font-bold uppercase tracking-widest"
              style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
              @click="emit('close')">
        Close
      </button>
    </div>
  </div>
</template>

<script setup>
defineProps({
  isVisible: { type: Boolean, default: false },
  title:     { type: String,  default: 'Remarks' },
  text:      { type: String,  default: '' },
})

const emit = defineEmits(['close'])
</script>

<style scoped>
  .animate-viewer {
    animation: viewerIn 0.2s ease;
  }

  @keyframes viewerIn {
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
