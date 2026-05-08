<template>
  <div class="space-y-6">

    <!-- ── Create / Replace Announcement ────────────────────────────────── -->
    <div class="rounded-2xl overflow-hidden"
         style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)">

      <!-- Header -->
      <div class="px-8 py-5 flex items-center gap-4"
           style="border-bottom: 1px solid var(--color-border)">
        <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
             style="background-color: var(--color-primary-soft)">
          <span class="material-symbols-outlined text-base" style="color: var(--color-primary)">campaign</span>
        </div>
        <div>
          <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text)">
            Post Announcement
          </h2>
          <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">
            Broadcasts a message to all or specific user roles. Only one active announcement at a time. (Experimental feature)
          </p>
        </div>
      </div>

      <!-- Form -->
      <div class="px-8 py-6 space-y-5">

        <!-- Title -->
        <div>
          <label class="text-[10px] font-bold uppercase tracking-widest mb-1.5 block"
                 style="color: var(--color-text-muted)">Title <span style="color: var(--color-text-muted); font-weight: 400">(optional)</span></label>
          <input v-model="form.title"
                 type="text"
                 maxlength="120"
                 placeholder="e.g. System Maintenance Notice"
                 class="w-full px-4 py-2.5 rounded-xl text-sm font-medium outline-none transition-all"
                 style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                 @focus="e => e.target.style.borderColor = 'var(--color-primary)'"
                 @blur="e => e.target.style.borderColor = 'var(--color-border)'" />
        </div>

        <!-- Message -->
        <div>
          <label class="text-[10px] font-bold uppercase tracking-widest mb-1.5 block"
                 style="color: var(--color-text-muted)">Message *</label>
          <textarea v-model="form.message"
                    rows="3"
                    maxlength="1000"
                    placeholder="Enter the announcement message..."
                    class="w-full px-4 py-2.5 rounded-xl text-sm font-medium outline-none transition-all resize-none"
                    style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                    @focus="e => e.target.style.borderColor = 'var(--color-primary)'"
                    @blur="e => e.target.style.borderColor = 'var(--color-border)'" />
          <p class="text-[10px] mt-1 text-right" style="color: var(--color-text-muted)">
            {{ form.message.length }} / 1000
          </p>
        </div>

        <!-- Type + Visibility row -->
        <div class="grid grid-cols-2 gap-4">

          <!-- Type -->
          <div>
            <label class="text-[10px] font-bold uppercase tracking-widest mb-1.5 block"
                   style="color: var(--color-text-muted)">Type</label>
            <div class="flex gap-2">
              <button v-for="t in types" :key="t.value"
                      class="flex-1 flex items-center justify-center gap-1.5 px-3 py-2 rounded-xl text-xs font-bold transition-all"
                      :style="form.type === t.value
                        ? `background-color: ${t.bg}; color: ${t.color}; border: 1.5px solid ${t.color};`
                        : 'background-color: var(--color-surface-low); color: var(--color-text-muted); border: 1.5px solid var(--color-border);'"
                      @click="form.type = t.value">
                <span class="material-symbols-outlined" style="font-size: 14px">{{ t.icon }}</span>
                {{ t.label }}
              </button>
            </div>
          </div>

          <!-- Visibility -->
          <div>
            <label class="text-[10px] font-bold uppercase tracking-widest mb-1.5 block"
                   style="color: var(--color-text-muted)">Visible To</label>
            <div class="flex gap-2 flex-wrap">
              <button v-for="r in roles" :key="r.value"
                      class="flex items-center gap-1.5 px-3 py-2 rounded-xl text-xs font-bold transition-all"
                      :style="isRoleSelected(r.value)
                        ? 'background-color: var(--color-primary-soft); color: var(--color-primary); border: 1.5px solid var(--color-primary);'
                        : 'background-color: var(--color-surface-low); color: var(--color-text-muted); border: 1.5px solid var(--color-border);'"
                      @click="toggleRole(r.value)">
                <span class="material-symbols-outlined" style="font-size: 14px">{{ r.icon }}</span>
                {{ r.label }}
              </button>
            </div>
          </div>

        </div>

        <!-- Duration -->
        <div>
          <label class="text-[10px] font-bold uppercase tracking-widest mb-1.5 block"
                 style="color: var(--color-text-muted)">Hide After</label>
          <div class="flex gap-2 flex-wrap">
            <button v-for="m in durationModes" :key="m.value"
                    class="px-4 py-2 rounded-xl text-xs font-bold transition-all"
                    :style="form.durationMode === m.value
                      ? 'background-color: var(--color-primary-soft); color: var(--color-primary); border: 1.5px solid var(--color-primary);'
                      : 'background-color: var(--color-surface-low); color: var(--color-text-muted); border: 1.5px solid var(--color-border);'"
                    @click="form.durationMode = m.value">
              {{ m.label }}
            </button>
          </div>

          <!-- Datetime picker -->
          <div v-if="form.durationMode === 'datetime'" class="mt-3">
            <input v-model="form.expiresAt"
                   type="datetime-local"
                   class="px-4 py-2.5 rounded-xl text-sm font-medium outline-none transition-all"
                   style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border); min-width: 220px;"
                   @focus="e => e.target.style.borderColor = 'var(--color-primary)'"
                   @blur="e => e.target.style.borderColor = 'var(--color-border)'" />
          </div>

          <!-- Numeric duration -->
          <div v-else class="mt-3 flex items-center gap-3">
            <input v-model.number="form.durationValue"
                   type="number"
                   min="1"
                   class="px-4 py-2.5 rounded-xl text-sm font-medium outline-none transition-all w-32 text-center"
                   style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                   @focus="e => e.target.style.borderColor = 'var(--color-primary)'"
                   @blur="e => e.target.style.borderColor = 'var(--color-border)'" />
            <span class="text-sm font-bold" style="color: var(--color-text-muted)">
              {{ durationModes.find(m => m.value === form.durationMode)?.unit }}
            </span>
          </div>
        </div>

        <!-- Error -->
        <p v-if="formError" class="text-xs font-bold" style="color: var(--color-error)">
          {{ formError }}
        </p>

        <!-- Submit -->
        <div class="flex justify-end pt-2" style="border-top: 1px solid var(--color-border)">
          <button class="px-6 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow-lg"
                  :class="saving ? 'opacity-60 pointer-events-none' : ''"
                  style="background: var(--color-primary-gradient); color: #ffffff"
                  @click="post">
            <span class="material-symbols-outlined text-sm">{{ saving ? 'progress_activity' : 'send' }}</span>
            {{ saving ? 'Posting...' : 'Post Announcement' }}
          </button>
        </div>

      </div>
    </div>

    <!-- ── History ────────────────────────────────────────────────────────── -->
    <div class="rounded-2xl overflow-hidden"
         style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)">

      <div class="px-8 py-5 flex items-center justify-between"
           style="border-bottom: 1px solid var(--color-border)">
        <div class="flex items-center gap-4">
          <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
               style="background-color: var(--color-primary-soft)">
            <span class="material-symbols-outlined text-base" style="color: var(--color-primary)">history</span>
          </div>
          <div>
            <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text)">History</h2>
            <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">Past and current announcements.</p>
          </div>
        </div>
      </div>

      <!-- Loading -->
      <div v-if="listLoading" class="px-8 py-10 flex justify-center">
        <span class="material-symbols-outlined animate-spin" style="color: var(--color-primary)">progress_activity</span>
      </div>

      <!-- Empty -->
      <div v-else-if="!list.length" class="px-8 py-10 text-center">
        <span class="material-symbols-outlined text-3xl mb-2 block" style="color: var(--color-text-muted)">campaign</span>
        <p class="text-sm font-bold" style="color: var(--color-text-muted)">No announcements yet.</p>
      </div>

      <!-- List -->
      <div v-else class="divide-y" style="border-color: var(--color-border)">
        <div v-for="item in list" :key="item.id"
             class="px-8 py-4 flex items-start gap-4">

          <!-- Type indicator -->
          <div class="w-8 h-8 rounded-lg flex items-center justify-center flex-shrink-0 mt-0.5"
               :style="`background-color: ${typeMap[item.type]?.bg ?? 'var(--color-surface-low)'}`">
            <span class="material-symbols-outlined"
                  :style="`font-size: 15px; color: ${typeMap[item.type]?.color ?? 'var(--color-text-muted)'}`">
              {{ typeMap[item.type]?.icon ?? 'info' }}
            </span>
          </div>

          <!-- Content -->
          <div class="flex-1 min-w-0">
            <div class="flex items-center gap-2 flex-wrap mb-0.5">
              <p class="text-sm font-extrabold" style="color: var(--color-text)">
                {{ item.title || '(No title)' }}
              </p>
              <!-- Active badge -->
              <span v-if="item.isActive"
                    class="px-2 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-widest"
                    style="background-color: var(--color-success-soft); color: var(--color-success)">
                Active
              </span>
              <!-- Expired badge -->
              <span v-else-if="new Date(item.expiresAt) < new Date()"
                    class="px-2 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-widest"
                    style="background-color: var(--color-surface-low); color: var(--color-text-muted)">
                Expired
              </span>
              <!-- Deactivated badge -->
              <span v-else
                    class="px-2 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-widest"
                    style="background-color: rgba(186,26,26,0.08); color: #ba1a1a">
                Deactivated
              </span>
              <!-- Visibility chips -->
              <span v-for="r in resolveRoleLabels(item.targetRoles)" :key="r"
                    class="px-2 py-0.5 rounded-full text-[10px] font-bold"
                    style="background-color: var(--color-primary-soft); color: var(--color-primary)">
                {{ r }}
              </span>
            </div>
            <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">{{ item.message }}</p>
            <div class="flex items-center gap-3 mt-1.5 flex-wrap">
              <span class="text-[10px] font-bold" style="color: var(--color-text-muted)">
                Posted by {{ item.createdBy }} · {{ formatDate(item.createdAt) }}
              </span>
              <span class="text-[10px] font-bold" style="color: var(--color-text-muted)">
                Expires {{ formatDate(item.expiresAt) }}
              </span>
            </div>
          </div>

          <!-- Deactivate button (only if active) -->
          <button v-if="item.isActive"
                  class="flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-[10px] font-bold uppercase tracking-widest transition-all flex-shrink-0"
                  style="background-color: rgba(186,26,26,0.08); color: #ba1a1a; border: 1px solid rgba(186,26,26,0.2);"
                  @click="deactivate(item)">
            <span class="material-symbols-outlined" style="font-size: 13px">cancel</span>
            Stop
          </button>

        </div>
      </div>

    </div>

  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { announcementApi } from '@/api/announcementApi'
import { useAuthStore } from '@/stores/authStore'

const emit = defineEmits(['toast'])
const authStore = useAuthStore()

// ── Constants ─────────────────────────────────────────────────────────────

const types = [
  { value: 'info',     label: 'Info',     icon: 'info',    bg: 'rgba(59,130,246,0.08)',  color: '#3b82f6' },
  { value: 'warning',  label: 'Warning',  icon: 'warning', bg: 'rgba(234,179,8,0.08)',   color: '#ca8a04' },
  { value: 'critical', label: 'Critical', icon: 'error',   bg: 'rgba(186,26,26,0.08)',   color: '#ba1a1a' },
]

const typeMap = Object.fromEntries(types.map(t => [t.value, t]))

const roles = [
  { value: 'all', label: 'All',         icon: 'groups'       },
  { value: '1',   label: 'Endorser',    icon: 'upload_file'  },
  { value: '2',   label: 'Receiver',    icon: 'move_to_inbox'},
  { value: '3',   label: 'Lab Section', icon: 'biotech'      },
]

const durationModes = [
  { value: 'datetime', label: 'By Date & Time', unit: ''        },
  { value: 'hours',    label: 'Hours',           unit: 'hours'   },
  { value: 'minutes',  label: 'Minutes',         unit: 'minutes' },
  { value: 'seconds',  label: 'Seconds',         unit: 'seconds' },
]

// ── Form state ────────────────────────────────────────────────────────────

const defaultForm = () => ({
  title:         '',
  message:       '',
  type:          'info',
  targetRoles:   ['all'],
  durationMode:  'hours',
  durationValue: 8,
  expiresAt:     '',
})

const form      = ref(defaultForm())
const formError = ref('')
const saving    = ref(false)

// ── Role toggle ───────────────────────────────────────────────────────────

function isRoleSelected(val) {
  return form.value.targetRoles.includes(val)
}

function toggleRole(val) {
  if (val === 'all') {
    form.value.targetRoles = ['all']
    return
  }
  // Remove 'all' if a specific role is picked
  form.value.targetRoles = form.value.targetRoles.filter(r => r !== 'all')
  if (form.value.targetRoles.includes(val)) {
    form.value.targetRoles = form.value.targetRoles.filter(r => r !== val)
    if (!form.value.targetRoles.length) form.value.targetRoles = ['all']
  } else {
    form.value.targetRoles.push(val)
  }
}

// ── Post ──────────────────────────────────────────────────────────────────

async function post() {
  formError.value = ''

  if (!form.value.message.trim()) {
    formError.value = 'Message is required.'
    return
  }

  if (form.value.durationMode === 'datetime' && !form.value.expiresAt) {
    formError.value = 'Please select an expiry date and time.'
    return
  }

  if (form.value.durationMode !== 'datetime' && (!form.value.durationValue || form.value.durationValue < 1)) {
    formError.value = 'Please enter a valid duration.'
    return
  }

  saving.value = true
  try {
    await announcementApi.create({
      title:         form.value.title.trim() || null,
      message:       form.value.message.trim(),
      type:          form.value.type,
      targetRoles:   form.value.targetRoles.join(','),
      durationMode:  form.value.durationMode,
      expiresAt:     form.value.durationMode === 'datetime' ? form.value.expiresAt : null,
      durationValue: form.value.durationMode !== 'datetime' ? form.value.durationValue : null,
    })
    form.value = defaultForm()
    emit('toast', 'Announcement posted.')
    await loadList()
  } catch (err) {
    formError.value = err.response?.data?.message || 'Failed to post announcement.'
  } finally {
    saving.value = false
  }
}

// ── List ──────────────────────────────────────────────────────────────────

const list        = ref([])
const listLoading = ref(false)

async function loadList() {
  listLoading.value = true
  try {
    list.value = await announcementApi.getAll()
  } catch {
    list.value = []
  } finally {
    listLoading.value = false
  }
}

async function deactivate(item) {
  try {
    await announcementApi.deactivate(item.id)
    emit('toast', 'Announcement stopped.')
    await loadList()
  } catch (err) {
    emit('toast', err.response?.data?.message || 'Failed to stop announcement.')
  }
}

// ── Helpers ───────────────────────────────────────────────────────────────

function resolveRoleLabels(targetRoles) {
  if (!targetRoles || targetRoles === 'all') return ['All Users']
  return targetRoles.split(',').map(v => roles.find(r => r.value === v)?.label ?? v)
}

function formatDate(val) {
  if (!val) return '—'
  return new Date(val).toLocaleString('en-PH', {
    month: 'short', day: 'numeric', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  })
}

onMounted(() => loadList())
</script>
