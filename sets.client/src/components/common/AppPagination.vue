<template>
  <div v-if="totalPages > 1"
       class="px-6 py-3 flex items-center justify-between"
       style="border-top: 1px solid var(--color-surface-low);">

    <!-- Prev -->
    <button :disabled="modelValue === 1"
            class="flex items-center gap-1 px-3 py-1.5 rounded-lg text-xs font-bold uppercase tracking-widest transition-all disabled:opacity-40"
            style="color: var(--color-text-muted); background-color: var(--color-surface-low);"
            @click="$emit('update:modelValue', modelValue - 1)">
      <span class="material-symbols-outlined text-sm">chevron_left</span>
      Prev
    </button>

    <!-- Page numbers -->
    <div class="flex items-center gap-1">
      <button v-for="p in pages" :key="p"
              class="w-7 h-7 rounded-lg text-xs font-bold transition-all"
              :style="p === modelValue
                ? 'background-color: var(--color-primary); color: #ffffff;'
                : 'color: var(--color-text-muted); background-color: var(--color-surface-low);'"
              @click="$emit('update:modelValue', p)">
        {{ p }}
      </button>
    </div>

    <!-- Next -->
    <button :disabled="modelValue === totalPages"
            class="flex items-center gap-1 px-3 py-1.5 rounded-lg text-xs font-bold uppercase tracking-widest transition-all disabled:opacity-40"
            style="color: var(--color-text-muted); background-color: var(--color-surface-low);"
            @click="$emit('update:modelValue', modelValue + 1)">
      Next
      <span class="material-symbols-outlined text-sm">chevron_right</span>
    </button>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  modelValue: {   // current page (v-model)
    type: Number,
    required: true,
  },
  total: {        // total number of items
    type: Number,
    required: true,
  },
  pageSize: {     // items per page
    type: Number,
    default: 10,
  },
})

defineEmits(['update:modelValue'])

const totalPages = computed(() =>
  Math.max(1, Math.ceil(props.total / props.pageSize))
)

// Show at most 7 page buttons; if more, show first, last, and surrounding current
const pages = computed(() => {
  const total = totalPages.value
  if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1)

  const cur = props.modelValue
  const set = new Set([1, total, cur])
  if (cur > 1) set.add(cur - 1)
  if (cur < total) set.add(cur + 1)

  return [...set].sort((a, b) => a - b)
})
</script>
