<template>
  <!-- Notice Banner -->
  <div class="flex items-start gap-3 px-5 py-4 rounded-2xl mb-4"
       style="background-color: rgba(234,179,8,0.08); border: 1.5px solid rgba(234,179,8,0.35);">
    <span class="material-symbols-outlined text-base flex-shrink-0 mt-0.5" style="color: #ca8a04;">warning</span>
    <div>
      <p class="text-xs font-extrabold" style="color: #92400e;">Heads up before making changes</p>
      <p class="text-xs mt-0.5" style="color: #92400e; opacity: 0.85;">
        Adding or modifying a branch requires corresponding updates to
        <span class="font-bold">appsettings.json</span> and the
        <span class="font-bold">HCLAB config</span> on the server.
        Please notify the IT or developer before touching this module.
      </p>
    </div>
  </div>

  <div class="rounded-2xl overflow-hidden"
       style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)">
    <!-- Header -->
    <div class="px-8 py-5 flex items-center gap-4" style="border-bottom: 1px solid var(--color-border)">
      <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
           style="background-color: var(--color-primary-soft)">
        <span class="material-symbols-outlined text-base" style="color: var(--color-primary)">location_city</span>
      </div>
      <div>
        <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text)">Branch Management</h2>
        <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">Add and manage branch locations. Branch code is used as the unique identifier.</p>
      </div>
    </div>

    <div class="px-8 py-6 space-y-8">

      <!-- Add New Branch -->
      <div>
        <p class="text-[10px] font-bold uppercase tracking-widest mb-3" style="color: var(--color-text-muted)">Add New Branch</p>
        <div class="flex items-start gap-3">
          <div class="flex-1">
            <input v-model="newCode"
                   class="w-full px-4 py-2.5 rounded-xl text-sm font-mono font-bold outline-none transition-all"
                   style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                   placeholder="e.g. WES"
                   maxlength="20"
                   :disabled="adding"
                   @input="onNewCodeInput"
                   @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                   @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')"
                   @keyup.enter="addBranch" />

            <!-- Config check feedback -->
            <div v-if="newCode.length >= 3" class="mt-2 flex items-center gap-3">
              <!-- Spinner while checking -->
              <template v-if="configChecking">
                <span class="flex items-center gap-1 text-[10px] font-bold" style="color: var(--color-text-muted)">
                  <span class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
                  Checking configs...
                </span>
              </template>

              <!-- Results -->
              <template v-else-if="configResult">
                <span class="flex items-center gap-1 text-[10px] font-bold"
                      :style="configResult.inSets ? 'color: #059669' : 'color: #f59e0b'">
                  <span class="material-symbols-outlined text-sm">
                    {{ configResult.inSets ? 'check_circle' : 'warning' }}
                  </span>
                  {{ configResult.inSets ? 'In appsettings.json' : 'Not in appsettings.json' }}
                </span>
                <span class="flex items-center gap-1 text-[10px] font-bold"
                      :style="configResult.inHclab ? 'color: #059669' : 'color: #f59e0b'">
                  <span class="material-symbols-outlined text-sm">
                    {{ configResult.inHclab ? 'check_circle' : 'warning' }}
                  </span>
                  {{ configResult.inHclab ? 'In HCLAB config' : 'Not in HCLAB config' }}
                </span>
              </template>
            </div>

            <p v-if="addError" class="mt-2 text-xs font-bold" style="color: #ba1a1a">{{ addError }}</p>
          </div>

          <button class="flex items-center gap-2 px-4 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex-shrink-0"
                  style="background: var(--color-primary-gradient); color: #ffffff"
                  :disabled="adding || !newCode.trim()"
                  @click="addBranch">
            <span v-if="adding" class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
            <span v-else class="material-symbols-outlined text-sm">add</span>
            Add
          </button>
        </div>
      </div>

      <!-- Branch List -->
      <div>
        <p class="text-[10px] font-bold uppercase tracking-widest mb-3" style="color: var(--color-text-muted)">Configured Branches</p>

        <!-- Loading -->
        <div v-if="loading" class="flex items-center justify-center py-12 gap-3" style="color: var(--color-text-muted)">
          <span class="material-symbols-outlined animate-spin text-xl">progress_activity</span>
          <span class="text-sm font-medium">Loading branches...</span>
        </div>

        <!-- Empty -->
        <div v-else-if="!branches.length" class="py-10 flex flex-col items-center gap-2" style="color: var(--color-text-muted)">
          <span class="material-symbols-outlined text-3xl">location_city</span>
          <p class="text-sm font-medium">No branches configured yet.</p>
        </div>

        <!-- Table -->
        <div v-else class="rounded-xl overflow-hidden" style="border: 1px solid var(--color-border)">
          <table class="w-full text-sm">
            <thead>
              <tr style="border-bottom: 1px solid var(--color-border)">
                <th class="text-left px-5 py-3 text-[10px] font-bold uppercase tracking-widest"
                    style="background-color: var(--color-surface-low); color: var(--color-text-muted)">Branch</th>
                <th class="text-left px-5 py-3 text-[10px] font-bold uppercase tracking-widest"
                    style="background-color: var(--color-surface-low); color: var(--color-text-muted)">Config Status</th>
                <th class="text-left px-5 py-3 text-[10px] font-bold uppercase tracking-widest"
                    style="background-color: var(--color-surface-low); color: var(--color-text-muted)">Added By</th>
                <th class="px-5 py-3" style="background-color: var(--color-surface-low)"></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(branch, idx) in branches" :key="branch.code"
                  :style="idx < branches.length - 1 ? 'border-bottom: 1px solid var(--color-border);' : ''">

                <!-- Branch code -->
                <td class="px-5 py-4">
                  <span class="font-mono font-extrabold text-sm" style="color: var(--color-primary)">{{ branch.code }}</span>
                </td>

                <!-- Config status badges -->
                <td class="px-5 py-4">
                  <div class="flex items-center gap-2">
                    <span class="flex items-center gap-1 px-2 py-1 rounded-lg text-[10px] font-bold"
                          :style="branch.inSets
                            ? 'background-color: rgba(5,150,105,0.1); color: #059669;'
                            : 'background-color: rgba(245,158,11,0.1); color: #f59e0b;'">
                      <span class="material-symbols-outlined text-xs">{{ branch.inSets ? 'check_circle' : 'warning' }}</span>
                      appsettings
                    </span>
                    <span class="flex items-center gap-1 px-2 py-1 rounded-lg text-[10px] font-bold"
                          :style="branch.inHclab
                            ? 'background-color: rgba(5,150,105,0.1); color: #059669;'
                            : 'background-color: rgba(245,158,11,0.1); color: #f59e0b;'">
                      <span class="material-symbols-outlined text-xs">{{ branch.inHclab ? 'check_circle' : 'warning' }}</span>
                      HCLAB
                    </span>
                  </div>
                </td>

                <!-- Added by -->
                <td class="px-5 py-4">
                  <p class="text-xs font-bold" style="color: var(--color-text)">{{ branch.createdBy }}</p>
                  <p class="text-[10px] mt-0.5" style="color: var(--color-text-muted)">
                    {{ new Date(branch.created).toLocaleDateString() }}
                  </p>
                </td>

                <!-- Status toggle -->
                <td class="px-5 py-4">
                  <button class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-[10px] font-bold uppercase tracking-widest transition-all"
                          :style="branch.active
                            ? 'background-color: var(--color-primary-soft); color: var(--color-primary);'
                            : 'background-color: var(--color-surface-low); color: var(--color-text-muted); border: 1px solid var(--color-border);'"
                          @click="toggleBranch(branch)">
                    <span class="material-symbols-outlined text-xs">{{ branch.active ? 'check_circle' : 'cancel' }}</span>
                    {{ branch.active ? 'Active' : 'Inactive' }}
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Config legend -->
        <div v-if="branches.length" class="mt-4">
          <p class="text-[10px]" style="color: var(--color-text-muted)">
            <span class="font-bold" style="color: #f59e0b">⚠ Warning</span>
            — branch exists in the DB but is missing from the indicated config file.
            Connections or HCLAB queries for this branch will fail until the config is updated.
          </p>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup>
  import { ref, onMounted } from 'vue'
  import { branchApi } from '@/api/branchApi'

  const emit = defineEmits(['toast'])

  // ── State ──────────────────────────────────────────────────────────────────
  const branches = ref([])
  const loading = ref(false)
  const adding = ref(false)
  const addError = ref('')
  const newCode = ref('')
  const configChecking = ref(false)
  const configResult = ref(null)   // { inSets, inHclab } | null
  let configCheckTimer = null

  // ── Data loading ───────────────────────────────────────────────────────────
  async function load() {
    loading.value = true
    try {
      branches.value = await branchApi.getWithConfigStatus()
    } catch {
      branches.value = []
    } finally {
      loading.value = false
    }
  }

  onMounted(() => load())

  // ── Input handler — debounced live config check ────────────────────────────
  function onNewCodeInput(e) {
    newCode.value = e.target.value.toUpperCase()
    addError.value = ''
    configResult.value = null
    clearTimeout(configCheckTimer)

    const code = newCode.value.trim()
    if (code.length < 3) {
      configChecking.value = false
      return
    }

    configChecking.value = true
    configCheckTimer = setTimeout(async () => {
      try {
        configResult.value = await branchApi.checkConfig(code)
      } catch {
        configResult.value = null
      } finally {
        configChecking.value = false
      }
    }, 350)
  }

  // ── Actions ────────────────────────────────────────────────────────────────
  async function addBranch() {
    addError.value = ''
    const code = newCode.value.trim().toUpperCase()
    if (!code) return

    adding.value = true
    try {
      await branchApi.add(code)
      newCode.value = ''
      configResult.value = null
      emit('toast', `Branch '${code}' added.`)
      await load()
    } catch (err) {
      addError.value = err.response?.data?.message || 'Failed to add branch.'
    } finally {
      adding.value = false
    }
  }

  async function toggleBranch(branch) {
    try {
      await branchApi.toggle(branch.code)
      branch.active = !branch.active
      emit('toast', `Branch '${branch.code}' ${branch.active ? 'activated' : 'deactivated'}.`)
    } catch (err) {
      emit('toast', err.response?.data?.message || 'Failed to update branch.')
    }
  }
</script>
