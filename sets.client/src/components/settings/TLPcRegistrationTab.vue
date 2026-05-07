<template>
  <div class="rounded-2xl overflow-hidden"
       style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)">

    <!-- Header -->
    <div class="px-8 py-5 flex items-center justify-between"
         style="border-bottom: 1px solid var(--color-border)">
      <div class="flex items-center gap-4">
        <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
             style="background-color: var(--color-primary-soft)">
          <span class="material-symbols-outlined text-base" style="color: var(--color-primary)">computer</span>
        </div>
        <div>
          <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text)">
            PC Registration
          </h2>
          <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">
            Workstations registered for
            <span class="font-bold" style="color: var(--color-primary)">{{ authStore.sectionName }}</span>.
          </p>
        </div>
      </div>
      <button class="flex items-center gap-2 px-4 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 shadow"
              style="background: var(--color-primary-gradient); color: #ffffff"
              @click="openAddPC">
        <span class="material-symbols-outlined text-sm">add</span>
        Register PC
      </button>
    </div>

    <!-- Table -->
    <div class="px-8 py-6 space-y-5">
      <div class="relative">
        <span class="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-base"
              style="color: var(--color-text-muted)">search</span>
        <input v-model="pcSearch"
               class="w-full pl-10 pr-4 py-2.5 rounded-xl text-sm font-medium outline-none transition-all"
               style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
               placeholder="Search by IP or description..."
               @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
               @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
      </div>

      <AppTable :rows="filteredPCs" :columns="pcColumns" row-key="id" :page-size="10"
                :loading="pcLoading" loading-text="Loading registered PCs..."
                empty-text="No PCs registered for your section yet." empty-icon="computer_off">
        <template #cell-ipAddress="{ value }">
          <span class="font-mono font-bold text-xs" style="color: var(--color-text)">{{ value }}</span>
        </template>
        <template #cell-description="{ value }">
          <span class="text-sm" style="color: var(--color-text-muted)">{{ value || '—' }}</span>
        </template>
        <template #cell-active="{ row }">
          <button class="flex items-center gap-1.5 px-3 py-1 rounded-lg text-[10px] font-bold uppercase tracking-widest transition-all"
                  :style="row.active
                    ? 'background-color: var(--color-primary-soft); color: var(--color-primary);'
                    : 'background-color: var(--color-surface-low); color: var(--color-text-muted); border: 1px solid var(--color-border);'"
                  @click.stop="togglePC(row)">
            <span class="material-symbols-outlined text-xs">{{ row.active ? 'check_circle' : 'cancel' }}</span>
            {{ row.active ? 'Active' : 'Inactive' }}
          </button>
        </template>
        <template #actions="{ row }">
          <div class="flex items-center gap-1 justify-end">
            <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                    title="Edit description"
                    style="color: var(--color-text-muted)"
                    @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'var(--color-surface-low)')"
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click.stop="openEditPC(row)">
              <span class="material-symbols-outlined text-sm">edit</span>
            </button>
            <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                    title="Remove from my section"
                    style="color: #ba1a1a"
                    @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'rgba(186,26,26,0.08)')"
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click.stop="confirmRemove(row)">
              <span class="material-symbols-outlined text-sm">link_off</span>
            </button>
          </div>
        </template>
      </AppTable>
    </div>
  </div>

  <!-- Add / Edit Modal -->
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="pcModal.visible"
           class="fixed inset-0 z-50 flex items-center justify-center"
           style="background-color: rgba(0,0,0,0.4)"
           @click.self="closePCModal">
        <div class="w-full max-w-md rounded-2xl overflow-hidden"
             style="background-color: var(--color-surface); box-shadow: 0 8px 32px rgba(0,0,0,0.24);">

          <!-- Modal Header -->
          <div class="px-6 py-4 flex items-center justify-between"
               style="border-bottom: 1px solid var(--color-border)">
            <div class="flex items-center gap-3">
              <div class="w-8 h-8 rounded-lg flex items-center justify-center"
                   style="background-color: var(--color-primary-soft)">
                <span class="material-symbols-outlined text-sm" style="color: var(--color-primary)">computer</span>
              </div>
              <h3 class="text-sm font-extrabold tracking-tight" style="color: var(--color-text)">
                {{ pcModal.mode === 'add' ? 'Register New PC' : 'Edit PC' }}
              </h3>
            </div>
            <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                    style="color: var(--color-text-muted)"
                    @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'var(--color-surface-low)')"
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click="closePCModal">
              <span class="material-symbols-outlined text-base">close</span>
            </button>
          </div>

          <!-- Modal Body -->
          <div class="px-6 py-5 space-y-4">
            <div>
              <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5"
                     style="color: var(--color-text-muted)">IP Address *</label>
              <input v-model="pcModal.form.ipAddress"
                     placeholder="e.g. 192.168.1.100"
                     :readonly="pcModal.mode === 'edit'"
                     class="w-full px-4 py-2.5 rounded-xl text-sm font-mono font-medium outline-none transition-all"
                     :style="pcModal.mode === 'edit'
                       ? 'background-color: var(--color-surface-low); color: var(--color-text-muted); border: 1.5px solid var(--color-border); cursor: not-allowed;'
                       : 'background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);'"
                     @focus="(e) => { if (pcModal.mode !== 'edit') e.target.style.borderColor = 'var(--color-primary)' }"
                     @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
            </div>

            <div>
              <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5"
                     style="color: var(--color-text-muted)">Description</label>
              <input v-model="pcModal.form.description"
                     placeholder="e.g. Hematology workstation"
                     class="w-full px-4 py-2.5 rounded-xl text-sm font-medium outline-none transition-all"
                     style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                     @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                     @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
            </div>

            <!-- Section lock indicator -->
            <div class="flex items-center gap-3 px-4 py-3 rounded-xl"
                 style="background-color: var(--color-primary-soft);">
              <span class="material-symbols-outlined text-sm" style="color: var(--color-primary)">lock</span>
              <div>
                <p class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-primary)">Section</p>
                <p class="text-sm font-bold mt-0.5" style="color: var(--color-text)">{{ authStore.sectionName }}</p>
              </div>
            </div>

            <p v-if="pcModal.error" class="text-xs font-bold" style="color: #ba1a1a">{{ pcModal.error }}</p>
          </div>

          <!-- Modal Footer -->
          <div class="px-6 py-4 flex justify-end gap-3"
               style="border-top: 1px solid var(--color-border)">
            <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                    style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                    @click="closePCModal">
              Cancel
            </button>
            <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow"
                    style="background: var(--color-primary-gradient); color: #ffffff"
                    :disabled="pcModal.saving"
                    @click="savePCModal">
              <span v-if="pcModal.saving" class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
              <span v-else class="material-symbols-outlined text-sm">save</span>
              {{ pcModal.mode === 'add' ? 'Register' : 'Save' }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>

  <!-- Confirm Remove Modal -->
  <ConfirmModal :isVisible="removeConfirm.visible"
                type="warning"
                title="Remove PC from Section"
                :message="`Remove ${removeConfirm.ipAddress} from your section? The PC itself will not be deleted.`"
                confirm-label="Remove"
                @close="removeConfirm.visible = false"
                @confirm="doRemove" />
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { pcApi } from '@/api/pcApi'
import { useAuthStore } from '@/stores/authStore'
import AppTable from '@/components/common/AppTable.vue'
import ConfirmModal from '@/components/common/ConfirmModal.vue'

const emit = defineEmits(['toast'])
const authStore = useAuthStore()

// ── State ──────────────────────────────────────────────────────────────────
const pcList    = ref([])
const pcLoading = ref(false)
const pcSearch  = ref('')

const pcColumns = [
  { key: 'ipAddress',   label: 'IP Address'  },
  { key: 'description', label: 'Description' },
  { key: 'active',      label: 'Status'      },
]

const pcModal = ref({
  visible: false,
  mode: 'add',   // 'add' | 'edit'
  pcId: null,
  form: { ipAddress: '', description: '', active: true },
  saving: false,
  error: '',
})

const removeConfirm = ref({ visible: false, pcId: null, ipAddress: '' })

const filteredPCs = computed(() => {
  const q = pcSearch.value.toLowerCase()
  if (!q) return pcList.value
  return pcList.value.filter(pc =>
    pc.ipAddress.toLowerCase().includes(q) ||
    (pc.description ?? '').toLowerCase().includes(q)
  )
})

// ── Data loading ───────────────────────────────────────────────────────────
async function loadPCs() {
  pcLoading.value = true
  try {
    const res = await pcApi.tlGetAll()
    pcList.value = res.data
  } catch {
    pcList.value = []
  } finally {
    pcLoading.value = false
  }
}

onMounted(loadPCs)

// ── Modal: Add ─────────────────────────────────────────────────────────────
function openAddPC() {
  pcModal.value = {
    visible: true, mode: 'add', pcId: null,
    form: { ipAddress: '', description: '', active: true },
    saving: false, error: '',
  }
}

// ── Modal: Edit ────────────────────────────────────────────────────────────
function openEditPC(pc) {
  pcModal.value = {
    visible: true, mode: 'edit', pcId: pc.id,
    form: { ipAddress: pc.ipAddress, description: pc.description, active: pc.active },
    saving: false, error: '',
  }
}

function closePCModal() {
  pcModal.value.visible = false
}

async function savePCModal() {
  pcModal.value.error = ''
  pcModal.value.saving = true
  try {
    if (pcModal.value.mode === 'add') {
      if (!pcModal.value.form.ipAddress.trim()) {
        pcModal.value.error = 'IP Address is required.'
        return
      }
      await pcApi.tlAdd({
        ipAddress: pcModal.value.form.ipAddress.trim(),
        description: pcModal.value.form.description.trim(),
      })
      emit('toast', 'PC registered successfully.')
    } else {
      await pcApi.tlUpdate(pcModal.value.pcId, {
        description: pcModal.value.form.description.trim(),
        active: pcModal.value.form.active,
      })
      emit('toast', 'PC updated successfully.')
    }
    closePCModal()
    await loadPCs()
  } catch (err) {
    pcModal.value.error = err.response?.data?.message || 'An error occurred.'
  } finally {
    pcModal.value.saving = false
  }
}

// ── Toggle ─────────────────────────────────────────────────────────────────
async function togglePC(pc) {
  try {
    await pcApi.tlToggle(pc.id)
    pc.active = !pc.active
  } catch (err) {
    emit('toast', err.response?.data?.message || 'Failed to update status.')
  }
}

// ── Remove from section ────────────────────────────────────────────────────
function confirmRemove(pc) {
  removeConfirm.value = { visible: true, pcId: pc.id, ipAddress: pc.ipAddress }
}

async function doRemove() {
  try {
    await pcApi.tlRemoveSection(removeConfirm.value.pcId)
    emit('toast', 'PC removed from your section.')
    removeConfirm.value.visible = false
    await loadPCs()
  } catch (err) {
    emit('toast', err.response?.data?.message || 'Failed to remove PC.')
    removeConfirm.value.visible = false
  }
}
</script>

<style scoped>
  .modal-enter-active {
    transition: opacity 0.15s ease, transform 0.15s ease;
  }

  .modal-leave-active {
    transition: opacity 0.1s ease, transform 0.1s ease;
  }

  .modal-enter-from {
    opacity: 0;
    transform: scale(0.97);
  }

  .modal-leave-to {
    opacity: 0;
    transform: scale(0.97);
  }

  .animate-spin {
    animation: spin 0.8s linear infinite;
  }

  @keyframes spin {
    to {
      transform: rotate(360deg);
    }
  }
</style>
