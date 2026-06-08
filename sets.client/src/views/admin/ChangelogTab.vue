<script setup>
import { ref, onMounted } from 'vue'
import { changelogApi } from '@/api/changelogApi'

const emit = defineEmits(['toast'])

// ── State ──────────────────────────────────────────────────────────────────

const entries  = ref([])
const loading  = ref(false)
const saving   = ref(false)
const error    = ref('')

// ── Form ───────────────────────────────────────────────────────────────────

const VALID_TAGS = ['NEW', 'IMPROVED', 'FIXED', 'REMOVED']

const TAG_CONFIG = {
  NEW:      { label: 'New',      icon: 'add_circle',    color: '#16a34a', bg: 'rgba(22,163,74,0.10)'  },
  IMPROVED: { label: 'Improved', icon: 'trending_up',   color: '#2563eb', bg: 'rgba(37,99,235,0.10)'  },
  FIXED:    { label: 'Fixed',    icon: 'build_circle',  color: '#d97706', bg: 'rgba(217,119,6,0.10)'  },
  REMOVED:  { label: 'Removed',  icon: 'remove_circle', color: '#dc2626', bg: 'rgba(220,38,38,0.10)'  },
}

function emptyForm() {
  return {
    version:    '',
    title:      '',
    releasedAt: new Date().toISOString().slice(0, 10),
    items:      [{ tag: 'NEW', description: '' }],
  }
}

const form = ref(emptyForm())

function addItem() {
  form.value.items.push({ tag: 'NEW', description: '' })
}

function removeItem(index) {
  if (form.value.items.length === 1) return
  form.value.items.splice(index, 1)
}

// ── Delete confirm ─────────────────────────────────────────────────────────

const deleteConfirm = ref({ visible: false, id: null, version: '' })

function openDeleteConfirm(entry) {
  deleteConfirm.value = { visible: true, id: entry.id, version: entry.version }
}

// ── Load ───────────────────────────────────────────────────────────────────

async function load() {
  loading.value = true
  try {
    entries.value = await changelogApi.getAll()
  } catch {
    error.value = 'Failed to load changelog entries.'
  } finally {
    loading.value = false
  }
}

onMounted(load)

// ── Save ───────────────────────────────────────────────────────────────────

async function save() {
  error.value = ''

  if (!form.value.version.trim()) {
    error.value = 'Version is required.'
    return
  }
  if (!form.value.title.trim()) {
    error.value = 'Title is required.'
    return
  }

  const filledItems = form.value.items.filter(i => i.description.trim())
  if (filledItems.length === 0) {
    error.value = 'At least one item with a description is required.'
    return
  }

  saving.value = true
  try {
    await changelogApi.create({
      version:    form.value.version.trim(),
      title:      form.value.title.trim(),
      releasedAt: new Date(form.value.releasedAt).toISOString(),
      items:      filledItems.map((item, i) => ({
        tag:         item.tag,
        description: item.description.trim(),
        sortOrder:   i,
      })),
    })

    emit('toast', { type: 'success', message: `v${form.value.version} changelog published.` })
    form.value = emptyForm()
    await load()
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to save changelog.'
  } finally {
    saving.value = false
  }
}

// ── Delete ─────────────────────────────────────────────────────────────────

async function confirmDelete() {
  try {
    await changelogApi.delete(deleteConfirm.value.id)
    emit('toast', { type: 'success', message: `v${deleteConfirm.value.version} entry deleted.` })
    deleteConfirm.value.visible = false
    await load()
  } catch {
    emit('toast', { type: 'error', message: 'Failed to delete entry.' })
  }
}

// ── Helpers ────────────────────────────────────────────────────────────────

function formatDate(iso) {
  if (!iso) return ''
  return new Date(iso).toLocaleDateString('en-PH', {
    year: 'numeric', month: 'short', day: 'numeric'
  })
}
</script>

<template>
  <div class="space-y-6">

    <!-- ── Create New Entry ──────────────────────────────────────────────── -->
    <div class="rounded-2xl overflow-hidden"
         style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">

      <!-- Header -->
      <div class="px-8 py-5 flex items-center gap-4"
           style="border-bottom: 1px solid var(--color-border);">
        <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
             style="background-color: var(--color-primary-soft);">
          <span class="material-symbols-outlined text-base"
                style="color: var(--color-primary); font-variation-settings: 'FILL' 1;">
            new_releases
          </span>
        </div>
        <div>
          <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text);">
            Publish Changelog
          </h2>
          <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">
            Create a new version entry. All users will see it once on their next login.
          </p>
        </div>
      </div>

      <!-- Form -->
      <div class="px-8 py-6 space-y-5">

        <!-- Version + Title row -->
        <div class="grid grid-cols-3 gap-4">
          <div>
            <label class="text-[10px] font-bold uppercase tracking-widest mb-1.5 block"
                   style="color: var(--color-text-muted);">Version *</label>
            <input v-model="form.version"
                   type="text"
                   placeholder="e.g. 1.4.0"
                   class="w-full px-4 py-2.5 rounded-xl text-sm font-mono outline-none transition-all"
                   style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                   @focus="e => e.target.style.borderColor = 'var(--color-primary)'"
                   @blur="e => e.target.style.borderColor = 'var(--color-border)'" />
          </div>
          <div class="col-span-2">
            <label class="text-[10px] font-bold uppercase tracking-widest mb-1.5 block"
                   style="color: var(--color-text-muted);">Title *</label>
            <input v-model="form.title"
                   type="text"
                   placeholder="e.g. Outbound TAT & Performance Fixes"
                   class="w-full px-4 py-2.5 rounded-xl text-sm outline-none transition-all"
                   style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                   @focus="e => e.target.style.borderColor = 'var(--color-primary)'"
                   @blur="e => e.target.style.borderColor = 'var(--color-border)'" />
          </div>
        </div>

        <!-- Release date -->
        <div class="w-48">
          <label class="text-[10px] font-bold uppercase tracking-widest mb-1.5 block"
                 style="color: var(--color-text-muted);">Release Date *</label>
          <input v-model="form.releasedAt"
                 type="date"
                 class="w-full px-4 py-2.5 rounded-xl text-sm outline-none transition-all"
                 style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                 @focus="e => e.target.style.borderColor = 'var(--color-primary)'"
                 @blur="e => e.target.style.borderColor = 'var(--color-border)'" />
        </div>

        <!-- Changelog Items -->
        <div>
          <div class="flex items-center justify-between mb-3">
            <label class="text-[10px] font-bold uppercase tracking-widest"
                   style="color: var(--color-text-muted);">Changes *</label>
            <button class="flex items-center gap-1.5 text-[11px] font-bold px-3 py-1.5 rounded-xl transition-all active:scale-95"
                    style="background-color: var(--color-primary-soft); color: var(--color-primary);"
                    type="button"
                    @click="addItem">
              <span class="material-symbols-outlined text-sm">add</span>
              Add Item
            </button>
          </div>

          <div class="space-y-2">
            <div v-for="(item, index) in form.items"
                 :key="index"
                 class="flex items-center gap-3">

              <!-- Tag selector -->
              <select v-model="item.tag"
                      class="flex-shrink-0 px-3 py-2.5 rounded-xl text-xs font-bold uppercase outline-none transition-all cursor-pointer"
                      :style="`background-color: ${TAG_CONFIG[item.tag]?.bg ?? 'var(--color-surface-low)'}; color: ${TAG_CONFIG[item.tag]?.color ?? 'var(--color-text)'}; border: 1.5px solid ${TAG_CONFIG[item.tag]?.color ?? 'var(--color-border)'}44;`">
                <option v-for="t in VALID_TAGS" :key="t" :value="t">{{ t }}</option>
              </select>

              <!-- Description -->
              <input v-model="item.description"
                     type="text"
                     placeholder="Describe the change..."
                     class="flex-1 px-4 py-2.5 rounded-xl text-sm outline-none transition-all"
                     style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                     @focus="e => e.target.style.borderColor = 'var(--color-primary)'"
                     @blur="e => e.target.style.borderColor = 'var(--color-border)'" />

              <!-- Remove -->
              <button class="w-8 h-8 rounded-xl flex items-center justify-center flex-shrink-0 transition-all active:scale-90 disabled:opacity-30"
                      style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                      type="button"
                      :disabled="form.items.length === 1"
                      @click="removeItem(index)">
                <span class="material-symbols-outlined text-sm">remove</span>
              </button>
            </div>
          </div>
        </div>

        <!-- Error -->
        <p v-if="error"
           class="text-xs font-bold px-4 py-2.5 rounded-xl"
           style="background-color: rgba(186,26,26,0.08); color: #ba1a1a;">
          {{ error }}
        </p>

        <!-- Submit -->
        <div class="flex justify-end pt-1">
          <button class="px-6 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 disabled:opacity-50"
                  style="background-color: var(--color-primary); color: #ffffff;"
                  :disabled="saving"
                  @click="save">
            <span v-if="saving"
                  class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
            <span v-else class="material-symbols-outlined text-sm">publish</span>
            {{ saving ? 'Publishing...' : 'Publish' }}
          </button>
        </div>

      </div>
    </div>

    <!-- ── Existing Entries ──────────────────────────────────────────────── -->
    <div class="rounded-2xl overflow-hidden"
         style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">

      <div class="px-8 py-5 flex items-center gap-4"
           style="border-bottom: 1px solid var(--color-border);">
        <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
             style="background-color: var(--color-primary-soft);">
          <span class="material-symbols-outlined text-base" style="color: var(--color-primary);">history</span>
        </div>
        <div>
          <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text);">
            Published Entries
          </h2>
          <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">
            {{ entries.length }} version{{ entries.length !== 1 ? 's' : '' }} on record
          </p>
        </div>
      </div>

      <!-- Loading -->
      <div v-if="loading" class="px-8 py-10 text-center">
        <span class="material-symbols-outlined animate-spin text-2xl"
              style="color: var(--color-text-muted);">progress_activity</span>
      </div>

      <!-- Empty -->
      <div v-else-if="entries.length === 0" class="px-8 py-10 text-center">
        <span class="material-symbols-outlined text-3xl mb-2 block"
              style="color: var(--color-text-muted);">history_toggle_off</span>
        <p class="text-sm" style="color: var(--color-text-muted);">No changelog entries yet.</p>
      </div>

      <!-- List -->
      <div v-else class="divide-y" style="border-color: var(--color-border);">
        <div v-for="entry in entries"
             :key="entry.id"
             class="px-8 py-5">

          <!-- Entry header -->
          <div class="flex items-start justify-between gap-4 mb-3">
            <div class="flex items-center gap-3">
              <span class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-[11px] font-bold"
                    style="background-color: var(--color-primary); color: #ffffff;">
                <span class="material-symbols-outlined text-[12px]">tag</span>
                v{{ entry.version }}
              </span>
              <div>
                <p class="text-sm font-bold" style="color: var(--color-text);">{{ entry.title }}</p>
                <p class="text-[11px] mt-0.5" style="color: var(--color-text-muted);">
                  {{ formatDate(entry.releasedAt) }} · Posted by {{ entry.createdBy }}
                </p>
              </div>
            </div>

            <!-- Delete -->
            <button class="w-8 h-8 rounded-xl flex items-center justify-center flex-shrink-0 transition-all active:scale-90"
                    style="background-color: rgba(186,26,26,0.08); color: #ba1a1a;"
                    @click="openDeleteConfirm(entry)">
              <span class="material-symbols-outlined text-sm">delete</span>
            </button>
          </div>

          <!-- Items -->
          <div class="space-y-1.5 pl-1">
            <div v-for="item in entry.items"
                 :key="item.id"
                 class="flex items-start gap-2.5">
              <span class="material-symbols-outlined text-sm mt-0.5 flex-shrink-0"
                    :style="`color: ${TAG_CONFIG[item.tag]?.color ?? '#888'}; font-variation-settings: 'FILL' 1;`">
                {{ TAG_CONFIG[item.tag]?.icon ?? 'circle' }}
              </span>
              <div class="flex items-baseline gap-2">
                <span class="text-[9px] font-extrabold uppercase tracking-widest px-1.5 py-0.5 rounded"
                      :style="`color: ${TAG_CONFIG[item.tag]?.color ?? '#888'}; background-color: ${TAG_CONFIG[item.tag]?.bg ?? 'transparent'};`">
                  {{ TAG_CONFIG[item.tag]?.label ?? item.tag }}
                </span>
                <p class="text-xs" style="color: var(--color-text);">{{ item.description }}</p>
              </div>
            </div>
          </div>

        </div>
      </div>
    </div>

  </div>

  <!-- ── Delete Confirm Modal ──────────────────────────────────────────────── -->
  <div v-if="deleteConfirm.visible"
       class="fixed inset-0 z-[90] flex items-center justify-center p-4">
    <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"
         @click="deleteConfirm.visible = false" />
    <div class="relative w-full max-w-sm rounded-2xl shadow-2xl p-6 flex flex-col gap-4 animate-modal"
         style="background-color: var(--color-surface);">

      <div class="flex items-center gap-3">
        <span class="material-symbols-outlined text-2xl"
              style="color: #ba1a1a; font-variation-settings: 'FILL' 1;">warning</span>
        <div>
          <p class="font-bold text-sm" style="color: var(--color-text);">Delete Changelog Entry</p>
          <p class="text-xs mt-0.5 font-mono" style="color: var(--color-text-muted);">
            v{{ deleteConfirm.version }}
          </p>
        </div>
      </div>

      <p class="text-xs" style="color: var(--color-text-muted);">
        This will permanently delete this changelog entry and all its items. This cannot be undone.
      </p>

      <div class="flex gap-3 justify-end">
        <button class="px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest"
                style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                @click="deleteConfirm.visible = false">
          Cancel
        </button>
        <button class="px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest active:scale-95"
                style="background-color: #ba1a1a; color: #ffffff;"
                @click="confirmDelete">
          Delete
        </button>
      </div>
    </div>
  </div>
</template>
