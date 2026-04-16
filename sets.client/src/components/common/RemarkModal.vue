<template>
  <div v-if="isVisible" class="fixed inset-0 z-50 flex items-center justify-center p-4">
    <div class="absolute inset-0 bg-black/40 backdrop-blur-sm" @click="handleCancel"></div>
    <div class="relative w-full max-w-sm rounded-2xl shadow-2xl p-6 flex flex-col gap-4 animate-modal"
         style="background-color: var(--color-surface);">

      <div class="flex items-center gap-2">
        <h3 class="text-base font-bold" style="color: var(--color-text);">
          {{ required ? 'Reason Required' : title }}
        </h3>
        <span v-if="required"
              class="text-[10px] font-bold px-2 py-0.5 rounded-full uppercase tracking-widest"
              style="background-color: var(--color-error-soft); color: var(--color-error);">
          Required
        </span>
      </div>

      <textarea v-model="localText"
                rows="4"
                :placeholder="required ? 'Complete the reason above...' : 'Enter remark here...'"
                class="w-full border-none outline-none rounded-xl p-4 text-sm resize-none transition-colors"
                style="background-color: var(--color-surface-low); color: var(--color-text);"></textarea>

      <div class="flex gap-3">
        <button class="flex-1 py-3 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                @click="handleCancel">
          Cancel
        </button>
        <button class="flex-1 py-3 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                style="background-color: var(--color-primary); color: #ffffff;"
                @click="handleSave">
          Save
        </button>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'

const props = defineProps({
  isVisible:    { type: Boolean, default: false },
  title:        { type: String,  default: 'Add Remark' },
  required:     { type: Boolean, default: false },
  initialText:  { type: String,  default: '' },
})

const emit = defineEmits(['save', 'cancel', 'close'])

const localText = ref(props.initialText)

// Sync initialText when modal opens
watch(() => props.isVisible, (val) => {
  if (val) localText.value = props.initialText
})

function handleSave() {
  emit('save', localText.value)
}

function handleCancel() {
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
