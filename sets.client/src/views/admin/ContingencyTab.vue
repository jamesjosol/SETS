<template>
  <div class="space-y-6">
    <div class="rounded-2xl p-6 space-y-5" style="background-color: var(--color-surface); border: 1px solid var(--color-border)">
      <h2 class="text-xs font-bold uppercase tracking-widest" style="color: var(--color-text-muted)">Contingency Settings</h2>

      <!-- Enable toggle -->
      <div class="flex items-center justify-between">
        <div>
          <p class="text-sm font-bold" style="color: var(--color-text)">Enable Contingency Mode</p>
          <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">Allows staff to log in using the master password when HCLAB is offline</p>
        </div>
        <button @click="config.isEnabled = !config.isEnabled"
                class="w-12 h-6 rounded-full transition-all relative"
                :style="config.isEnabled ? 'background-color: var(--color-primary)' : 'background-color: var(--color-surface-low)'">
          <span class="absolute top-0.5 w-5 h-5 rounded-full bg-white shadow transition-all"
                :style="config.isEnabled ? 'left: 26px' : 'left: 2px'"></span>
        </button>
      </div>

      <!-- Master password -->
      <div>
        <label class="block text-xs font-bold uppercase tracking-widest mb-1.5" style="color: var(--color-text-muted)">
          Master Password
          <span v-if="config.hasPassword" class="ml-2 px-2 py-0.5 rounded-full text-xs normal-case"
                style="background-color: var(--color-success-soft); color: var(--color-success)">Set</span>
          <span v-else class="ml-2 px-2 py-0.5 rounded-full text-xs normal-case"
                style="background-color: var(--color-error-soft); color: var(--color-error)">Not set</span>
        </label>
        <div class="flex gap-3">
          <input v-model="newPassword"
                 type="password"
                 placeholder="Enter new master password (leave blank to keep current)"
                 class="flex-1 px-4 py-2.5 rounded-xl text-sm outline-none"
                 style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border)" />
        </div>
        <p class="text-xs mt-1.5" style="color: var(--color-text-muted)">
          Last updated: {{ config.updatedBy ? `${config.updatedBy} on ${formatDate(config.updatedAt)}` : 'Never' }}
        </p>
      </div>

      <div class="flex justify-end pt-2" style="border-top: 1px solid var(--color-border)">
        <button @click="save" :disabled="saving"
                class="px-6 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow-lg"
                style="background: var(--color-primary-gradient); color: #fff">
          <span class="material-symbols-outlined text-sm">save</span>
          {{ saving ? 'Saving...' : 'Save Changes' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { contingencyApi } from '@/api/contingencyApi'

const emit = defineEmits(['toast'])

const config = ref({ isEnabled: false, hasPassword: false, updatedBy: null, updatedAt: null })
const newPassword = ref('')
const saving = ref(false)

function formatDate(dt) {
  if (!dt) return '—'
  return new Date(dt).toLocaleDateString('en-PH', { year: 'numeric', month: 'short', day: '2-digit' })
}

onMounted(async () => {
  try { config.value = await contingencyApi.getConfig() } catch { /* non-fatal */ }
})

async function save() {
  saving.value = true
  try {
    await contingencyApi.upsertConfig({
      isEnabled: config.value.isEnabled,
      masterPassword: newPassword.value || null
    })
    newPassword.value = ''
    config.value = await contingencyApi.getConfig()
    emit('toast', 'Contingency settings saved.')
  } catch (err) {
    emit('toast', err.response?.data?.message ?? 'Failed to save.')
  } finally { saving.value = false }
}
</script>
