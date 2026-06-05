<template>
  <div class="space-y-6">

    <!-- Header -->
    <div>
      <p class="text-[10px] font-bold uppercase tracking-widest mb-1"
         style="color: var(--color-text-muted)">Outbound TAT</p>
      <p class="text-xs" style="color: var(--color-text-muted)">
        Configure time windows for outbound batch endorsements and appeal settings.
      </p>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex items-center gap-3 py-8">
      <span class="material-symbols-outlined animate-spin" style="color: var(--color-text-muted)">
        progress_activity
      </span>
      <span class="text-sm font-bold uppercase tracking-widest"
            style="color: var(--color-text-muted)">Loading...</span>
    </div>

    <template v-else>

      <!-- ── Toggle: Outbound TAT Enabled ───────────────────────────────── -->
      <div class="flex items-center justify-between px-5 py-4 rounded-2xl"
           style="background-color: var(--color-surface); border: 1px solid var(--color-border)">
        <div class="flex items-center gap-3">
          <div class="w-9 h-9 rounded-xl flex items-center justify-center flex-shrink-0"
               :style="settings.outboundTatEnabled
                 ? 'background-color: var(--color-success-soft)'
                 : 'background-color: var(--color-surface-low)'">
            <span class="material-symbols-outlined text-base"
                  :style="settings.outboundTatEnabled
                    ? 'color: var(--color-success)'
                    : 'color: var(--color-text-muted)'">schedule</span>
          </div>
          <div>
            <p class="text-sm font-bold" style="color: var(--color-text)">Outbound TAT</p>
            <p class="text-xs" style="color: var(--color-text-muted)">
              {{
 settings.outboundTatEnabled
                ? 'Enabled — outbound batches are evaluated against configured time windows'
                : 'Disabled — no outbound TAT tracking'
              }}
            </p>
          </div>
        </div>
        <button @click="toggleOutboundTat"
                :disabled="savingSettings"
                class="flex items-center gap-2 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95"
                :style="settings.outboundTatEnabled
                  ? 'background-color: var(--color-success-soft); color: var(--color-success)'
                  : 'background-color: var(--color-surface-low); color: var(--color-text-muted); border: 1px solid var(--color-border)'">
          <span class="material-symbols-outlined" style="font-size: 14px">
            {{ savingSettings ? 'progress_activity' : settings.outboundTatEnabled ? 'toggle_on' : 'toggle_off' }}
          </span>
          {{ settings.outboundTatEnabled ? 'Enabled' : 'Disabled' }}
        </button>
      </div>

      <!-- ── Toggle: Appeal Enabled ──────────────────────────────────────── -->
      <div class="flex items-center justify-between px-5 py-4 rounded-2xl"
           style="background-color: var(--color-surface); border: 1px solid var(--color-border)">
        <div class="flex items-center gap-3">
          <div class="w-9 h-9 rounded-xl flex items-center justify-center flex-shrink-0"
               :style="settings.outboundTatAppealEnabled
                 ? 'background-color: var(--color-warning-soft)'
                 : 'background-color: var(--color-surface-low)'">
            <span class="material-symbols-outlined text-base"
                  :style="settings.outboundTatAppealEnabled
                    ? 'color: var(--color-warning)'
                    : 'color: var(--color-text-muted)'">do_not_disturb_on</span>
          </div>
          <div>
            <p class="text-sm font-bold" style="color: var(--color-text)">Allow Appeals</p>
            <p class="text-xs" style="color: var(--color-text-muted)">
              {{
 settings.outboundTatAppealEnabled
                ? 'Enabled — endorsers can appeal missed outbound TAT windows'
                : 'Disabled — missed windows cannot be appealed'
              }}
            </p>
          </div>
        </div>
        <button @click="toggleAppeal"
                :disabled="savingSettings"
                class="flex items-center gap-2 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95"
                :style="settings.outboundTatAppealEnabled
                  ? 'background-color: var(--color-warning-soft); color: var(--color-warning)'
                  : 'background-color: var(--color-surface-low); color: var(--color-text-muted); border: 1px solid var(--color-border)'">
          <span class="material-symbols-outlined" style="font-size: 14px">
            {{ savingSettings ? 'progress_activity' : settings.outboundTatAppealEnabled ? 'toggle_on' : 'toggle_off' }}
          </span>
          {{ settings.outboundTatAppealEnabled ? 'Enabled' : 'Disabled' }}
        </button>
      </div>

      <!-- ── Time Windows ────────────────────────────────────────────────── -->
      <div class="rounded-2xl overflow-hidden"
           style="background-color: var(--color-surface); border: 1px solid var(--color-border)">

        <!-- Header -->
        <div class="px-6 py-4 flex items-center justify-between"
             style="border-bottom: 1px solid var(--color-surface-low)">
          <div>
            <p class="text-sm font-bold" style="color: var(--color-text)">Time Windows</p>
            <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">
              Define the time slots within which outbound batches must be endorsed.
            </p>
          </div>
          <button @click="addWindow"
                  class="flex items-center gap-2 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95"
                  style="background: var(--color-primary-gradient); color: #ffffff;">
            <span class="material-symbols-outlined text-sm">add</span>
            Add Window
          </button>
        </div>

        <!-- Schedule type tabs -->
        <div class="flex px-6 pt-4 gap-2">
          <button v-for="tab in scheduleTabs"
                  :key="tab.key"
                  @click="activeScheduleTab = tab.key"
                  class="px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                  :style="activeScheduleTab === tab.key
                    ? 'background-color: var(--color-primary-soft); color: var(--color-primary);'
                    : 'color: var(--color-text-muted); background-color: var(--color-surface-low);'">
            {{ tab.label }}
          </button>
        </div>

        <!-- Window rows -->
        <div class="p-6 space-y-3">
          <div v-if="filteredWindows.length === 0"
               class="py-8 text-center text-xs font-bold uppercase tracking-widest"
               style="color: var(--color-text-muted)">
            No windows configured for {{ activeScheduleTab === 'Weekday' ? 'Mon–Sat' : 'Sunday' }}.
            Click "Add Window" to create one.
          </div>

          <div v-for="(win, idx) in filteredWindows"
               :key="win._key"
               class="flex items-center gap-4 px-5 py-4 rounded-xl"
               style="background-color: var(--color-surface-low)">

            <!-- Active toggle dot -->
            <button @click="win.isActive = !win.isActive"
                    class="w-2.5 h-2.5 rounded-full flex-shrink-0 transition-all self-center"
                    :style="win.isActive
                      ? 'background-color: var(--color-success)'
                      : 'background-color: var(--color-text-muted)'"
                    :title="win.isActive ? 'Active — click to deactivate' : 'Inactive — click to activate'" />

            <!-- Start time -->
            <div class="flex flex-col gap-1">
              <label class="text-[10px] font-bold uppercase tracking-widest"
                     style="color: var(--color-text-muted)">Start</label>
              <input v-model="win.windowStart"
                     type="time"
                     class="px-3 py-2 rounded-lg text-sm font-mono font-bold outline-none border transition-all"
                     style="background-color: var(--color-surface); color: var(--color-text);
                            border-color: var(--color-border); width: 160px;"
                     @focus="e => e.target.style.borderColor = 'var(--color-primary)'"
                     @blur="e => e.target.style.borderColor = 'var(--color-border)'" />
            </div>

            <!-- Arrow -->
            <span class="material-symbols-outlined text-base flex-shrink-0 self-end mb-2"
                  style="color: var(--color-text-muted)">arrow_forward</span>

            <!-- End time -->
            <div class="flex flex-col gap-1">
              <label class="text-[10px] font-bold uppercase tracking-widest"
                     style="color: var(--color-text-muted)">End</label>
              <input v-model="win.windowEnd"
                     type="time"
                     class="px-3 py-2 rounded-lg text-sm font-mono font-bold outline-none border transition-all"
                     style="background-color: var(--color-surface); color: var(--color-text);
                            border-color: var(--color-border); width: 160px;"
                     @focus="e => e.target.style.borderColor = 'var(--color-primary)'"
                     @blur="e => e.target.style.borderColor = 'var(--color-border)'" />
            </div>

            <!-- Schedule type badge + Inactive badge — self-end aligns to input bottom -->
            <div class="flex items-center gap-2 self-end mb-1.5">
              <span class="px-3 py-1 rounded-lg text-[10px] font-bold uppercase tracking-widest"
                    :style="win.scheduleType === 'Sunday'
                      ? 'background-color: var(--color-warning-soft); color: var(--color-warning)'
                      : 'background-color: var(--color-primary-soft); color: var(--color-primary)'">
                {{ win.scheduleType === 'Sunday' ? 'Sunday' : 'Mon–Sat' }}
              </span>
              <span v-if="!win.isActive"
                    class="px-3 py-1 rounded-lg text-[10px] font-bold uppercase tracking-widest"
                    style="background-color: var(--color-surface); color: var(--color-text-muted);
                           border: 1px solid var(--color-border)">
                Inactive
              </span>
            </div>

            <!-- Delete -->
            <button @click="removeWindow(win)"
                    class="ml-auto p-1.5 rounded-lg transition-all self-center"
                    style="color: var(--color-text-muted)"
                    @mouseenter="e => e.currentTarget.style.color = 'var(--color-error)'"
                    @mouseleave="e => e.currentTarget.style.color = 'var(--color-text-muted)'">
              <span class="material-symbols-outlined text-base">delete</span>
            </button>

          </div>
        </div>

        <!-- Save footer -->
        <div class="px-6 py-4 flex items-center justify-between"
             style="border-top: 1px solid var(--color-surface-low)">
          <p v-if="windowError" class="text-xs font-bold" style="color: var(--color-error)">
            {{ windowError }}
          </p>
          <span v-else />
          <button @click="saveWindows"
                  :disabled="savingWindows"
                  class="flex items-center gap-2 px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-[0.98]"
                  style="background: var(--color-primary-gradient); color: #ffffff;">
            <span class="material-symbols-outlined text-sm"
                  :class="savingWindows ? 'animate-spin' : ''">
              {{ savingWindows ? 'progress_activity' : 'save' }}
            </span>
            Save Windows
          </button>
        </div>

      </div>

    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { tatOutboundApi } from '@/api/tatOutboundApi'

const emit = defineEmits(['toast'])

// ── State ──────────────────────────────────────────────────────────────────

const loading       = ref(true)
const savingSettings = ref(false)
const savingWindows  = ref(false)
const windowError    = ref(null)

const settings = ref({
  outboundTatEnabled: false,
  outboundTatAppealEnabled: true,
})

const windows = ref([])   // working copy — includes unsaved new rows
let   windowKeyCounter = 0

const scheduleTabs = [
  { key: 'Weekday', label: 'Mon–Sat' },
  { key: 'Sunday',  label: 'Sunday'  },
]
const activeScheduleTab = ref('Weekday')

const filteredWindows = computed(() =>
  windows.value.filter(w => w.scheduleType === activeScheduleTab.value)
)

// ── Load ───────────────────────────────────────────────────────────────────

onMounted(async () => {
  await Promise.all([loadSettings(), loadWindows()])
  loading.value = false
})

async function loadSettings() {
  try {
    settings.value = await tatOutboundApi.getSettings()
  } catch {
    settings.value = { outboundTatEnabled: false, outboundTatAppealEnabled: true }
  }
}

async function loadWindows() {
  try {
    const data = await tatOutboundApi.getWindows()
    windows.value = data.map(w => ({
      ...w,
      _key: ++windowKeyCounter,   // stable local key for v-for
    }))
  } catch {
    windows.value = []
  }
}

// ── Settings toggles ───────────────────────────────────────────────────────

async function toggleOutboundTat() {
  savingSettings.value = true
  try {
    settings.value.outboundTatEnabled = !settings.value.outboundTatEnabled
    await tatOutboundApi.updateSettings({
      outboundTatEnabled: settings.value.outboundTatEnabled,
      outboundTatAppealEnabled: settings.value.outboundTatAppealEnabled,
    })
    emit('toast', `Outbound TAT ${settings.value.outboundTatEnabled ? 'enabled' : 'disabled'}.`)
  } catch {
    // Revert on failure
    settings.value.outboundTatEnabled = !settings.value.outboundTatEnabled
    emit('toast', 'Failed to update setting.')
  } finally {
    savingSettings.value = false
  }
}

async function toggleAppeal() {
  savingSettings.value = true
  try {
    settings.value.outboundTatAppealEnabled = !settings.value.outboundTatAppealEnabled
    await tatOutboundApi.updateSettings({
      outboundTatEnabled: settings.value.outboundTatEnabled,
      outboundTatAppealEnabled: settings.value.outboundTatAppealEnabled,
    })
    emit('toast', `Appeals ${settings.value.outboundTatAppealEnabled ? 'enabled' : 'disabled'}.`)
  } catch {
    settings.value.outboundTatAppealEnabled = !settings.value.outboundTatAppealEnabled
    emit('toast', 'Failed to update setting.')
  } finally {
    savingSettings.value = false
  }
}

// ── Window management ──────────────────────────────────────────────────────

function addWindow() {
  windows.value.push({
    id: 0,
    windowStart: '08:00',
    windowEnd: '09:00',
    scheduleType: activeScheduleTab.value,
    isActive: true,
    _key: ++windowKeyCounter,
  })
}

function removeWindow(win) {
  // If it's a persisted window, delete from server immediately
  if (win.id !== 0) {
    tatOutboundApi.deleteWindow(win.id).catch(() => {})
  }
  windows.value = windows.value.filter(w => w._key !== win._key)
}

async function saveWindows() {
  windowError.value = null

  // Validate
  for (const win of windows.value) {
    if (!win.windowStart || !win.windowEnd) {
      windowError.value = 'All windows must have a start and end time.'
      return
    }
    if (win.windowEnd <= win.windowStart) {
      windowError.value = `Window end must be after start (${win.windowStart}–${win.windowEnd}).`
      return
    }
  }

  savingWindows.value = true
  try {
    await tatOutboundApi.upsertWindows(windows.value.map(w => ({
      id: w.id,
      windowStart: w.windowStart,
      windowEnd: w.windowEnd,
      scheduleType: w.scheduleType,
      isActive: w.isActive,
    })))
    // Reload to get server-assigned IDs for new rows
    await loadWindows()
    emit('toast', 'Outbound TAT windows saved.')
  } catch (err) {
    windowError.value = err.response?.data?.message ?? 'Failed to save windows.'
  } finally {
    savingWindows.value = false
  }
}
</script>
