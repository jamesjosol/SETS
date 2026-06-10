<template>
  <div class="rounded-2xl overflow-hidden"
       style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)">

    <!-- Header -->
    <div class="px-8 py-5 flex items-center justify-between"
         style="border-bottom: 1px solid var(--color-border)">
      <div class="flex items-center gap-4">
        <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
             style="background-color: var(--color-primary-soft)">
          <span class="material-symbols-outlined text-base"
                style="color: var(--color-primary)">compare_arrows</span>
        </div>
        <div>
          <h2 class="text-base font-extrabold tracking-tight"
              style="color: var(--color-text)">
            Test Code Mapping
          </h2>
          <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">
            Map equivalent test codes between SETS and HCLAB.
          </p>
        </div>
      </div>
      <button class="flex items-center gap-2 px-4 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 shadow"
              style="background: var(--color-primary-gradient); color: #ffffff"
              @click="openAdd">
        <span class="material-symbols-outlined text-sm">add</span>Add Mapping
      </button>
    </div>

    <!-- Search -->
    <div class="px-8 pt-6 pb-2">
      <div class="relative">
        <span class="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-base"
              style="color: var(--color-text-muted)">search</span>
        <input v-model="search"
               class="w-full pl-10 pr-4 py-2.5 rounded-xl text-sm font-medium outline-none transition-all"
               style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
               placeholder="Search by Code A, Code B, or Remarks..."
               @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
               @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
      </div>
    </div>

    <!-- Table -->
    <div class="px-8 py-4">
      <AppTable :rows="filteredItems"
                :columns="columns"
                row-key="id"
                :page-size="10"
                :loading="loading"
                loading-text="Loading mappings..."
                empty-text="No test code mappings yet."
                empty-icon="compare_arrows">

        <!-- Code A -->
        <template #cell-codeA="{ value }">
          <span class="px-2.5 py-1 rounded-lg font-mono font-bold text-xs"
                style="background-color: var(--color-primary-soft); color: var(--color-primary)">
            {{ value }}
          </span>
        </template>

        <!-- Arrow -->
        <template #cell-arrow>
          <span class="material-symbols-outlined text-base"
                style="color: var(--color-text-muted)">sync_alt</span>
        </template>

        <!-- Code B -->
        <template #cell-codeB="{ value }">
          <span class="px-2.5 py-1 rounded-lg font-mono font-bold text-xs"
                style="background-color: var(--color-surface-high); color: var(--color-text)">
            {{ value }}
          </span>
        </template>

        <!-- Status -->
        <template #cell-isActive="{ row }">
          <button class="flex items-center gap-1.5 px-3 py-1 rounded-lg text-[10px] font-bold uppercase tracking-widest transition-all"
                  :style="row.isActive
                    ? 'background-color: var(--color-primary-soft); color: var(--color-primary);'
                    : 'background-color: var(--color-surface-low); color: var(--color-text-muted); border: 1px solid var(--color-border);'"
                  @click.stop="toggleItem(row)">
            <span class="material-symbols-outlined text-xs">{{ row.isActive ? 'check_circle' : 'cancel' }}</span>
            {{ row.isActive ? 'Active' : 'Inactive' }}
          </button>
        </template>

        <!-- Actions -->
        <template #actions="{ row }">
          <div class="flex items-center gap-1 justify-end">
            <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                    title="Edit mapping"
                    style="color: var(--color-text-muted)"
                    @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'var(--color-surface-low)')"
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click.stop="openEdit(row)">
              <span class="material-symbols-outlined text-sm">edit</span>
            </button>
            <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                    title="Delete mapping"
                    style="color: #ba1a1a"
                    @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'rgba(186,26,26,0.08)')"
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click.stop="openDelete(row)">
              <span class="material-symbols-outlined text-sm">delete</span>
            </button>
          </div>
        </template>

      </AppTable>
    </div>
  </div>

  <!-- Add/Edit Modal -->
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="modal.visible"
           class="fixed inset-0 z-50 flex items-center justify-center"
           style="background-color: rgba(0,0,0,0.4)"
           @click.self="closeModal">
        <div class="w-full max-w-md rounded-2xl overflow-hidden"
             style="background-color: var(--color-surface); box-shadow: 0 8px 32px rgba(0,0,0,0.24)">

          <!-- Modal Header -->
          <div class="px-6 py-4 flex items-center justify-between"
               style="border-bottom: 1px solid var(--color-border)">
            <div class="flex items-center gap-3">
              <div class="w-8 h-8 rounded-lg flex items-center justify-center"
                   style="background-color: var(--color-primary-soft)">
                <span class="material-symbols-outlined text-sm"
                      style="color: var(--color-primary)">compare_arrows</span>
              </div>
              <h3 class="text-sm font-extrabold tracking-tight" style="color: var(--color-text)">
                {{ modal.mode === 'add' ? 'Add Mapping' : 'Edit Mapping' }}
              </h3>
            </div>
            <button class="w-7 h-7 rounded-lg flex items-center justify-center transition-all"
                    style="color: var(--color-text-muted)"
                    @click="closeModal">
              <span class="material-symbols-outlined text-sm">close</span>
            </button>
          </div>

          <!-- Modal Body -->
          <div class="px-6 py-5 space-y-4">

            <!-- Code A -->
            <div class="flex flex-col gap-1.5">
              <label class="text-[10px] font-bold uppercase tracking-widest"
                     style="color: var(--color-text-muted)">Code A</label>
              <input v-model="form.codeA"
                     type="text"
                     maxlength="20"
                     placeholder="e.g. CRPHS"
                     class="w-full px-3 py-2.5 rounded-xl text-sm font-mono outline-none transition-all"
                     style="background-color: var(--color-surface-low); border: 1px solid var(--color-border); color: var(--color-text)"
                     @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                     @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
            </div>

            <!-- Code B -->
            <div class="flex flex-col gap-1.5">
              <label class="text-[10px] font-bold uppercase tracking-widest"
                     style="color: var(--color-text-muted)">Code B</label>
              <input v-model="form.codeB"
                     type="text"
                     maxlength="20"
                     placeholder="e.g. CRPHS2"
                     class="w-full px-3 py-2.5 rounded-xl text-sm font-mono outline-none transition-all"
                     style="background-color: var(--color-surface-low); border: 1px solid var(--color-border); color: var(--color-text)"
                     @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                     @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
            </div>

            <!-- Remarks -->
            <div class="flex flex-col gap-1.5">
              <label class="text-[10px] font-bold uppercase tracking-widest"
                     style="color: var(--color-text-muted)">
                Remarks
                <span style="font-weight: 400">(optional)</span>
              </label>
              <input v-model="form.remarks"
                     type="text"
                     maxlength="200"
                     placeholder="e.g. Machine swap — analyzer offline"
                     class="w-full px-3 py-2.5 rounded-xl text-sm outline-none transition-all"
                     style="background-color: var(--color-surface-low); border: 1px solid var(--color-border); color: var(--color-text)"
                     @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                     @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
            </div>

            <!-- Error -->
            <p v-if="modal.error" class="text-xs font-bold" style="color: #ba1a1a">
              {{ modal.error }}
            </p>

          </div>

          <!-- Modal Footer -->
          <div class="px-6 py-4 flex items-center justify-end gap-3"
               style="border-top: 1px solid var(--color-border)">
            <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                    style="background-color: var(--color-surface-low); color: var(--color-text-muted)"
                    @click="closeModal">
              Cancel
            </button>
            <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow"
                    style="background: var(--color-primary-gradient); color: #ffffff"
                    :disabled="modal.saving"
                    @click="saveModal">
              <span v-if="modal.saving"
                    class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
              <span v-else class="material-symbols-outlined text-sm">save</span>
              {{ modal.mode === 'add' ? 'Add' : 'Save Changes' }}
            </button>
          </div>

        </div>
      </div>
    </Transition>
  </Teleport>

  <!-- Delete Confirm -->
  <ConfirmModal :isVisible="deleteConfirm.visible"
                :title="`Remove Mapping`"
                :message="`Remove mapping between '${deleteConfirm.item?.codeA}' and '${deleteConfirm.item?.codeB}'? This cannot be undone.`"
                confirmText="Remove"
                cancelText="Cancel"
                @confirm="executeDelete"
                @close="deleteConfirm.visible = false" />
</template>

<script setup>
  import { ref, computed, onMounted } from 'vue'
  import AppTable from '@/components/common/AppTable.vue'
  import ConfirmModal from '@/components/common/ConfirmModal.vue'
  import { testCodeMapApi } from '@/api/testCodeMapApi'

  const emit = defineEmits(['toast'])

  // ── Columns ────────────────────────────────────────────────────────────────
  const columns = [
    { key: 'codeA', label: 'Code A' },
    { key: 'arrow', label: '' },
    { key: 'codeB', label: 'Code B' },
    { key: 'remarks', label: 'Remarks' },
    { key: 'isActive', label: 'Status' },
    { key: 'createdBy', label: 'Added By' },
  ]

  // ── State ──────────────────────────────────────────────────────────────────
  const items = ref([])
  const loading = ref(false)
  const search = ref('')

  const modal = ref({
    visible: false,
    mode: 'add',
    editId: null,
    saving: false,
    error: ''
  })

  const form = ref({ codeA: '', codeB: '', remarks: '' })
  const deleteConfirm = ref({ visible: false, item: null })

  // ── Computed ───────────────────────────────────────────────────────────────
  const filteredItems = computed(() => {
    const q = search.value.trim().toLowerCase()
    if (!q) return items.value
    return items.value.filter(i =>
      i.codeA.toLowerCase().includes(q) ||
      i.codeB.toLowerCase().includes(q) ||
      (i.remarks ?? '').toLowerCase().includes(q)
    )
  })

  // ── Load ───────────────────────────────────────────────────────────────────
  async function load() {
    loading.value = true
    try {
      items.value = await testCodeMapApi.getAll()
    } catch {
      // silent
    } finally {
      loading.value = false
    }
  }

  onMounted(load)

  // ── Add / Edit ─────────────────────────────────────────────────────────────
  function openAdd() {
    form.value = { codeA: '', codeB: '', remarks: '' }
    modal.value = { visible: true, mode: 'add', editId: null, saving: false, error: '' }
  }

  function openEdit(item) {
    form.value = { codeA: item.codeA, codeB: item.codeB, remarks: item.remarks ?? '' }
    modal.value = { visible: true, mode: 'edit', editId: item.id, saving: false, error: '' }
  }

  function closeModal() {
    if (modal.value.saving) return
    modal.value.visible = false
  }

  async function saveModal() {
    modal.value.error = ''

    if (!form.value.codeA.trim() || !form.value.codeB.trim()) {
      modal.value.error = 'Both Code A and Code B are required.'
      return
    }

    if (form.value.codeA.trim().toUpperCase() === form.value.codeB.trim().toUpperCase()) {
      modal.value.error = 'Code A and Code B must be different.'
      return
    }

    modal.value.saving = true
    try {
      const payload = {
        codeA: form.value.codeA.trim(),
        codeB: form.value.codeB.trim(),
        remarks: form.value.remarks.trim() || null
      }

      if (modal.value.mode === 'add') {
        await testCodeMapApi.add(payload)
        emit('toast', 'Mapping added successfully.')
      } else {
        await testCodeMapApi.update(modal.value.editId, payload)
        emit('toast', 'Mapping updated successfully.')
      }

      modal.value.visible = false
      await load()
    } catch (err) {
      modal.value.error = err?.response?.data?.message ?? 'An error occurred. Please try again.'
    } finally {
      modal.value.saving = false
    }
  }

  // ── Toggle ─────────────────────────────────────────────────────────────────
  async function toggleItem(item) {
    try {
      await testCodeMapApi.toggle(item.id)
      item.isActive = !item.isActive
      emit('toast', `Mapping ${item.isActive ? 'activated' : 'deactivated'}.`)
    } catch (err) {
      emit('toast', err?.response?.data?.message ?? 'Failed to update status.')
    }
  }

  // ── Delete ─────────────────────────────────────────────────────────────────
  function openDelete(item) {
    deleteConfirm.value = { visible: true, item }
  }

  async function executeDelete() {
    try {
      await testCodeMapApi.remove(deleteConfirm.value.item.id)
      deleteConfirm.value.visible = false
      await load()
      emit('toast', 'Mapping removed.')
    } catch (err) {
      emit('toast', err?.response?.data?.message ?? 'Failed to remove mapping.')
    }
  }
</script>

<style scoped>
  .modal-enter-active {
    transition: opacity 0.2s ease;
  }

  .modal-leave-active {
    transition: opacity 0.2s ease;
  }

  .modal-enter-from {
    opacity: 0;
  }

  .modal-leave-to {
    opacity: 0;
  }
</style>
