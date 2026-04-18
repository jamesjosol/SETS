<template>
  <div ref="rootRef" class="relative">

    <!-- Trigger button -->
    <button type="button"
            class="flex items-center gap-2 px-4 py-1.5 rounded-xl text-xs font-bold uppercase tracking-widest outline-none border transition-all select-none"
            :style="[
              'background-color: var(--color-surface-low);',
              `border-color: ${open ? 'var(--color-primary)' : 'var(--color-border)'};`,
              `color: ${modelValue ? 'var(--color-text)' : 'var(--color-text-muted)'};`,
            ].join(' ')"
            @click.stop="toggle">
      <span v-if="icon"
            class="material-symbols-outlined text-sm"
            :style="`color: ${modelValue ? 'var(--color-primary)' : 'var(--color-text-muted)'}`">
        {{ icon }}
      </span>
      <span>{{ selectedLabel }}</span>
      <span class="material-symbols-outlined text-sm transition-transform duration-200 ml-1"
            :style="`color: var(--color-text-muted); transform: rotate(${open ? '180deg' : '0deg'})`">
        expand_more
      </span>
    </button>

    <!-- Menu -->
    <Transition enter-active-class="transition-all duration-150 ease-out"
                enter-from-class="opacity-0 scale-95 -translate-y-1"
                enter-to-class="opacity-100 scale-100 translate-y-0"
                leave-active-class="transition-all duration-100 ease-in"
                leave-from-class="opacity-100 scale-100 translate-y-0"
                leave-to-class="opacity-0 scale-95 -translate-y-1">
      <ul v-if="open"
          class="absolute left-0 top-full mt-1 z-50 rounded-xl shadow-xl overflow-hidden"
          style="
            background-color: var(--color-surface);
            border: 1px solid var(--color-border);
            min-width: 160px;
            padding: 4px;
          ">
        <li v-for="option in options" :key="option.value">
          <button type="button"
                  class="w-full flex items-center gap-2 px-4 py-2.5 rounded-lg text-xs font-bold uppercase tracking-widest transition-all text-left"
                  :style="modelValue === option.value
                    ? 'background-color: var(--color-primary-soft); color: var(--color-primary);'
                    : 'color: var(--color-text-muted);'"
                  @mouseenter="e => { if (modelValue !== option.value) e.currentTarget.style.backgroundColor = 'var(--color-surface-low)' }"
                  @mouseleave="e => { if (modelValue !== option.value) e.currentTarget.style.backgroundColor = 'transparent' }"
                  @click.stop="select(option.value)">
            <span class="material-symbols-outlined text-sm"
                  :style="`opacity: ${modelValue === option.value ? 1 : 0}; color: var(--color-primary); flex-shrink: 0;`">
              check
            </span>
            {{ option.label }}
          </button>
        </li>
      </ul>
    </Transition>

  </div>
</template>

<script setup>
  import { ref, computed, onMounted, onUnmounted } from 'vue'

  const props = defineProps({
    modelValue: {
      type: String,
      default: '',
    },
    options: {
      type: Array,
      required: true,
    },
    placeholder: {
      type: String,
      default: 'Select',
    },
    icon: {
      type: String,
      default: null,
    },
  })

  const emit = defineEmits(['update:modelValue', 'change'])

  const open = ref(false)
  const rootRef = ref(null)

  const selectedLabel = computed(() => {
    const match = props.options.find(o => o.value === props.modelValue)
    return match ? match.label : props.placeholder
  })

  function toggle() {
    open.value = !open.value
  }

  function select(value) {
    emit('update:modelValue', value)
    emit('change', value)
    open.value = false
  }

  // Close when clicking outside
  function onClickOutside(e) {
    if (rootRef.value && !rootRef.value.contains(e.target)) {
      open.value = false
    }
  }

  onMounted(() => document.addEventListener('click', onClickOutside))
  onUnmounted(() => document.removeEventListener('click', onClickOutside))
</script>
