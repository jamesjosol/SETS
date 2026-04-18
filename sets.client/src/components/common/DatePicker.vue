<template>
  <div class="relative">
    <span class="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-base pointer-events-none"
          style="color: var(--color-text-muted);">calendar_month</span>
    <input ref="inputRef"
           type="text"
           :placeholder="placeholder"
           readonly
           class="pl-9 pr-4 py-2.5 rounded-xl text-xs font-bold outline-none border transition-all cursor-pointer w-full"
           :style="[
             'background-color: var(--color-surface-low);',
             'color: var(--color-text-muted);',
             `border-color: ${focused ? 'var(--color-primary)' : 'var(--color-border)'};`,
           ].join(' ')" />
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, watch } from 'vue'
import Pikaday from 'pikaday'

const props = defineProps({
  modelValue: {
    type: String,
    default: '',
  },
  placeholder: {
    type: String,
    default: 'Select date',
  },
  minDate: {
    type: String,
    default: null,
  },
  maxDate: {
    type: String,
    default: null,
  },
})

const emit = defineEmits(['update:modelValue', 'change'])

const inputRef = ref(null)
const focused  = ref(false)

let picker = null

// yyyy-MM-dd → Date (local, no timezone shift)
function parseDate(str) {
  if (!str) return null
  const [y, m, d] = str.split('-').map(Number)
  return new Date(y, m - 1, d)
}

// Date → yyyy-MM-dd
function toInputDate(date) {
  if (!date) return ''
  const y = date.getFullYear()
  const m = String(date.getMonth() + 1).padStart(2, '0')
  const d = String(date.getDate()).padStart(2, '0')
  return `${y}-${m}-${d}`
}

onMounted(() => {
  picker = new Pikaday({
    field:       inputRef.value,
    toString:    date => toInputDate(date),
    parse:       str  => parseDate(str),
    defaultDate: parseDate(props.modelValue) ?? undefined,
    setDefaultDate: !!props.modelValue,
    minDate:     parseDate(props.minDate) ?? undefined,
    maxDate:     parseDate(props.maxDate) ?? undefined,
    onOpen()  { focused.value = true  },
    onClose() { focused.value = false },
    onSelect(date) {
      const val = toInputDate(date)
      emit('update:modelValue', val)
      emit('change', val)
    },
  })
})

onUnmounted(() => {
  picker?.destroy()
})

// Keep picker in sync if parent changes the value programmatically
watch(() => props.modelValue, val => {
  const date = parseDate(val)
  if (date) {
    picker?.setDate(date, true)   // true = silent (no onSelect fire)
  } else {
    picker?.setDate(null, true)
    if (inputRef.value) inputRef.value.value = ''
  }
})

// Keep min/max in sync
watch(() => props.minDate, val => {
  picker?.setMinDate(parseDate(val) ?? null)
})

watch(() => props.maxDate, val => {
  picker?.setMaxDate(parseDate(val) ?? null)
})
</script>
