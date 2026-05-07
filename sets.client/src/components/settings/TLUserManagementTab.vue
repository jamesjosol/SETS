<template>
  <div class="rounded-2xl overflow-hidden"
       style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)">

    <!-- Header -->
    <div class="px-8 py-5 flex items-center justify-between"
         style="border-bottom: 1px solid var(--color-border)">
      <div class="flex items-center gap-4">
        <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
             style="background-color: var(--color-primary-soft)">
          <span class="material-symbols-outlined text-base" style="color: var(--color-primary)">manage_accounts</span>
        </div>
        <div>
          <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text)">User Management</h2>
          <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">
            Users with access to
            <span class="font-bold" style="color: var(--color-primary)">{{ authStore.sectionName }}</span>.
          </p>
        </div>
      </div>
      <button class="flex items-center gap-2 px-4 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 shadow"
              style="background: var(--color-primary-gradient); color: #ffffff"
              @click="openAddUser">
        <span class="material-symbols-outlined text-sm">person_add</span>Add User
      </button>
    </div>

    <div class="px-8 py-6 space-y-5">
      <div class="relative">
        <span class="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-base"
              style="color: var(--color-text-muted)">search</span>
        <input v-model="userSearch"
               class="w-full pl-10 pr-4 py-2.5 rounded-xl text-sm font-medium outline-none transition-all"
               style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
               placeholder="Search by User ID or Name..."
               @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
               @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
      </div>

      <AppTable :rows="filteredUsers" :columns="userColumns" row-key="userID" :page-size="10"
                :loading="usersLoading" loading-text="Loading users..."
                empty-text="No users found for your section." empty-icon="manage_accounts">
        <template #cell-userID="{ value }">
          <span class="font-mono font-bold text-xs" style="color: var(--color-text)">{{ value }}</span>
        </template>
        <template #cell-userName="{ value }">
          <span class="text-sm font-medium" style="color: var(--color-text)">{{ value }}</span>
        </template>
        <template #cell-role="{ row }">
          <span class="px-2 py-0.5 rounded-lg text-[10px] font-bold"
                :style="getSectionRole(row) === 2
                  ? 'background-color: var(--color-primary-soft); color: var(--color-primary);'
                  : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'">
            {{ getSectionRole(row) === 2 ? 'Team Lead' : 'Regular' }}
          </span>
        </template>
        <template #cell-active="{ row }">
          <button class="flex items-center gap-1.5 px-3 py-1 rounded-lg text-[10px] font-bold uppercase tracking-widest transition-all"
                  :style="row.active
                    ? 'background-color: var(--color-primary-soft); color: var(--color-primary);'
                    : 'background-color: var(--color-surface-low); color: var(--color-text-muted); border: 1px solid var(--color-border);'"
                  :disabled="row.userID === authStore.userID"
                  @click.stop="toggleUser(row)">
            <span class="material-symbols-outlined text-xs">{{ row.active ? 'check_circle' : 'cancel' }}</span>
            {{ row.active ? 'Active' : 'Inactive' }}
          </button>
        </template>
        <template #actions="{ row }">
          <div class="flex items-center gap-1 justify-end">
            <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                    title="Edit role"
                    style="color: var(--color-text-muted)"
                    @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'var(--color-surface-low)')"
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click.stop="openEditUser(row)">
              <span class="material-symbols-outlined text-sm">edit</span>
            </button>
            <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                    title="Remove from section"
                    :style="row.userID === authStore.userID
                      ? 'color: var(--color-text-muted); opacity: 0.4; cursor: not-allowed;'
                      : 'color: #ba1a1a;'"
                    :disabled="row.userID === authStore.userID"
                    @mouseenter="(e) => { if (row.userID !== authStore.userID) e.currentTarget.style.backgroundColor = 'rgba(186,26,26,0.08)' }"
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click.stop="row.userID !== authStore.userID && confirmRemoveUser(row)">
              <span class="material-symbols-outlined text-sm">person_remove</span>
            </button>
          </div>
        </template>
      </AppTable>
    </div>
  </div>

  <!-- Add / Edit Modal -->
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="userModal.visible"
           class="fixed inset-0 z-50 flex items-center justify-center"
           style="background-color: rgba(0,0,0,0.4)"
           @click.self="closeUserModal">
        <div class="w-full max-w-lg rounded-2xl overflow-hidden flex flex-col"
             style="background-color: var(--color-surface); box-shadow: 0 8px 32px rgba(0,0,0,0.24); max-height: 90vh;">

          <!-- Modal Header -->
          <div class="px-6 py-4 flex items-center justify-between flex-shrink-0"
               style="border-bottom: 1px solid var(--color-border)">
            <div class="flex items-center gap-3">
              <div class="w-8 h-8 rounded-lg flex items-center justify-center"
                   style="background-color: var(--color-primary-soft)">
                <span class="material-symbols-outlined text-sm" style="color: var(--color-primary)">
                  {{ userModal.mode === 'add' ? 'person_add' : 'manage_accounts' }}
                </span>
              </div>
              <h3 class="text-sm font-extrabold tracking-tight" style="color: var(--color-text)">
                {{ userModal.mode === 'add' ? 'Add New User' : 'Edit User' }}
              </h3>
            </div>
            <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                    style="color: var(--color-text-muted)"
                    @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'var(--color-surface-low)')"
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click="closeUserModal">
              <span class="material-symbols-outlined text-base">close</span>
            </button>
          </div>

          <!-- Modal Body -->
          <div class="px-6 py-5 space-y-5 overflow-y-auto flex-1">

            <!-- Add mode: HCLAB search (same pattern as admin UserManagementTab) -->
            <div v-if="userModal.mode === 'add'" class="relative">
              <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5"
                     style="color: var(--color-text-muted)">User ID *</label>
              <div class="relative">
                <input v-model="userModal.hclabSearch"
                       ref="userIdInputRef"
                       placeholder="Type 3+ characters to search..."
                       class="w-full px-4 py-2.5 pr-10 rounded-xl text-sm font-medium outline-none transition-all"
                       style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                       autocomplete="off"
                       @input="onHclabSearchInput"
                       @focus="onHclabSearchFocus"
                       @keydown.down.prevent="hclabSelectNext"
                       @keydown.up.prevent="hclabSelectPrev"
                       @keydown.enter.prevent="hclabConfirmSelected"
                       @keydown.escape="closeHclabDropdown"
                       @blur="onHclabSearchBlur" />
                <div class="absolute right-3 top-1/2 -translate-y-1/2 pointer-events-none">
                  <span v-if="userModal.hclabLoading"
                        class="material-symbols-outlined text-sm animate-spin" style="color: var(--color-text-muted)">progress_activity</span>
                  <span v-else-if="userModal.form.userID"
                        class="material-symbols-outlined text-sm pointer-events-auto cursor-pointer" style="color: var(--color-primary)"
                        @click.stop="clearHclabSelection">check_circle</span>
                  <span v-else class="material-symbols-outlined text-sm" style="color: var(--color-text-muted)">search</span>
                </div>
              </div>

              <!-- Selected user badge -->
              <div v-if="userModal.form.userID"
                   class="mt-1.5 flex items-center gap-2 px-3 py-2 rounded-lg"
                   style="background-color: var(--color-primary-soft)">
                <span class="material-symbols-outlined text-sm" style="color: var(--color-primary)">person</span>
                <div class="flex-1 min-w-0">
                  <p class="text-xs font-bold truncate" style="color: var(--color-primary)">{{ userModal.form.userID }}</p>
                  <p class="text-[10px] truncate" style="color: var(--color-text-muted)">{{ userModal.form.userName }}</p>
                </div>
                <button class="material-symbols-outlined text-sm flex-shrink-0 transition-all"
                        style="color: var(--color-text-muted)"
                        @click="clearHclabSelection">
                  close
                </button>
              </div>

              <p v-if="!userModal.form.userID && userModal.hclabSearch.length < 3 && !userModal.hclabResults.length"
                 class="mt-1 text-[10px]" style="color: var(--color-text-muted)">
                Search by HCLAB's User ID or name (min. 3 characters).
              </p>

              <!-- Dropdown -->
              <Teleport to="body">
                <div v-if="userModal.hclabDropdownOpen && (userModal.hclabResults.length || userModal.hclabNoResults)"
                     ref="hclabDropdownRef"
                     class="fixed z-[60] rounded-xl overflow-hidden shadow-xl"
                     :style="`top: ${hclabDropdownPos.top}px; left: ${hclabDropdownPos.left}px; width: ${hclabDropdownPos.width}px; background-color: var(--color-surface); border: 1.5px solid var(--color-border);`">
                  <div v-if="userModal.hclabNoResults" class="px-4 py-3 text-xs text-center"
                       style="color: var(--color-text-muted)">
                    No users found for "{{ userModal.hclabSearch }}".
                  </div>
                  <div v-else class="overflow-y-auto" style="max-height: 220px">
                    <div v-for="(u, idx) in userModal.hclabResults" :key="u.userID"
                         class="flex items-center gap-3 px-4 py-2.5 cursor-pointer transition-all"
                         :style="idx === userModal.hclabActiveIdx
                           ? 'background-color: var(--color-primary-soft);'
                           : 'border-bottom: 1px solid var(--color-surface-low);'"
                         @mouseenter="userModal.hclabActiveIdx = idx"
                         @mousedown.prevent="selectHclabUser(u)">
                      <span class="material-symbols-outlined text-sm" style="color: var(--color-primary)">person</span>
                      <div>
                        <p class="text-xs font-bold" style="color: var(--color-text)">{{ u.userID }}</p>
                        <p class="text-[10px]" style="color: var(--color-text-muted)">{{ u.userName }}</p>
                      </div>
                    </div>
                  </div>
                </div>
              </Teleport>
            </div>

            <!-- Edit mode: show user info read-only -->
            <div v-if="userModal.mode === 'edit'"
                 class="flex items-center gap-3 p-3 rounded-xl"
                 style="background-color: var(--color-surface-low)">
              <span class="material-symbols-outlined text-lg" style="color: var(--color-primary)">person</span>
              <div>
                <p class="text-xs font-bold font-mono" style="color: var(--color-text)">{{ userModal.form.userID }}</p>
                <p class="text-sm font-medium mt-0.5" style="color: var(--color-text-muted)">{{ userModal.form.userName }}</p>
              </div>
            </div>

            <!-- Role picker -->
            <div>
              <label class="block text-[10px] font-bold uppercase tracking-widest mb-2"
                     style="color: var(--color-text-muted)">Role in {{ authStore.sectionName }}</label>
              <div class="flex gap-2">
                <button v-for="role in roles" :key="role.id"
                        class="flex-1 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                        :style="userModal.form.roleID === role.id
                          ? 'background-color: var(--color-primary); color: #ffffff; border: 1.5px solid var(--color-primary);'
                          : 'background-color: var(--color-surface-low); color: var(--color-text-muted); border: 1.5px solid var(--color-border);'"
                        @click="userModal.form.roleID = role.id">
                  {{ role.label }}
                </button>
              </div>
            </div>

            <!-- Section lock -->
            <div class="flex items-center gap-3 px-4 py-3 rounded-xl"
                 style="background-color: var(--color-primary-soft);">
              <span class="material-symbols-outlined text-sm" style="color: var(--color-primary)">lock</span>
              <div>
                <p class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-primary)">Section</p>
                <p class="text-sm font-bold mt-0.5" style="color: var(--color-text)">{{ authStore.sectionName }}</p>
              </div>
            </div>

            <p v-if="userModal.error" class="text-xs font-bold" style="color: #ba1a1a">{{ userModal.error }}</p>
          </div>

          <!-- Modal Footer -->
          <div class="px-6 py-4 flex justify-end gap-3 flex-shrink-0"
               style="border-top: 1px solid var(--color-border)">
            <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                    style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                    @click="closeUserModal">
              Cancel
            </button>
            <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow"
                    style="background: var(--color-primary-gradient); color: #ffffff"
                    :disabled="userModal.saving"
                    @click="saveUserModal">
              <span v-if="userModal.saving" class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
              <span v-else class="material-symbols-outlined text-sm">save</span>
              {{ userModal.mode === 'add' ? 'Add User' : 'Save Changes' }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>

  <!-- Confirm Remove -->
  <ConfirmModal :isVisible="removeConfirm.visible"
                type="warning"
                title="Remove User from Section"
                :message="`Remove ${removeConfirm.userName} (${removeConfirm.userID}) from ${authStore.sectionName}?`"
                confirm-label="Remove"
                @close="removeConfirm.visible = false"
                @confirm="doRemoveUser" />
</template>

<script setup>
import { ref, computed, onMounted, nextTick } from 'vue'
import { userApi } from '@/api/userApi'
import { useAuthStore } from '@/stores/authStore'
import AppTable from '@/components/common/AppTable.vue'
import ConfirmModal from '@/components/common/ConfirmModal.vue'

const emit = defineEmits(['toast'])
const authStore = useAuthStore()

// ── Constants ──────────────────────────────────────────────────────────────
const roles = [
  { id: 1, label: 'Regular'   },
  { id: 2, label: 'Team Lead' },
]

const userColumns = [
  { key: 'userID',   label: 'User ID' },
  { key: 'userName', label: 'Name'    },
  { key: 'role',     label: 'Role'    },
  { key: 'active',   label: 'Status'  },
]

// ── State ──────────────────────────────────────────────────────────────────
const usersList    = ref([])
const usersLoading = ref(false)
const userSearch   = ref('')

const userIdInputRef   = ref(null)
const hclabDropdownRef = ref(null)
const hclabDropdownPos = ref({ top: 0, left: 0, width: 0 })
let   hclabSearchTimer = null

const userModal = ref({
  visible: false,
  mode: 'add',   // 'add' | 'edit'
  form: { userID: '', userName: '', roleID: 1 },
  hclabSearch: '', hclabResults: [], hclabLoading: false,
  hclabDropdownOpen: false, hclabNoResults: false, hclabActiveIdx: -1,
  saving: false, error: '',
})

const removeConfirm = ref({ visible: false, userID: '', userName: '' })

// ── Helpers ────────────────────────────────────────────────────────────────
function getSectionRole(user) {
  return user.sections?.find(s => s.sectionCode === authStore.sectionCode)?.roleID ?? 1
}

const filteredUsers = computed(() => {
  const q = userSearch.value.toLowerCase()
  if (!q) return usersList.value
  return usersList.value.filter(u =>
    u.userID.toLowerCase().includes(q) ||
    u.userName.toLowerCase().includes(q)
  )
})

// ── Data loading ───────────────────────────────────────────────────────────
async function loadUsers() {
  usersLoading.value = true
  try {
    const res = await userApi.tlGetAll()
    usersList.value = res.data
  } catch {
    usersList.value = []
  } finally {
    usersLoading.value = false
  }
}

onMounted(loadUsers)

// ── Modal: Add ─────────────────────────────────────────────────────────────
function openAddUser() {
  userModal.value = {
    visible: true, mode: 'add',
    form: { userID: '', userName: '', roleID: 1 },
    hclabSearch: '', hclabResults: [], hclabLoading: false,
    hclabDropdownOpen: false, hclabNoResults: false, hclabActiveIdx: -1,
    saving: false, error: '',
  }
}

// ── Modal: Edit ────────────────────────────────────────────────────────────
function openEditUser(user) {
  userModal.value = {
    visible: true, mode: 'edit',
    form: { userID: user.userID, userName: user.userName, roleID: getSectionRole(user) },
    saving: false, error: '',
  }
}

function closeUserModal() {
  userModal.value.visible = false
}

async function saveUserModal() {
  userModal.value.error = ''
  userModal.value.saving = true
  try {
    if (userModal.value.mode === 'add') {
      if (!userModal.value.form.userID.trim()) {
        userModal.value.error = 'Please select a user from the search results.'
        return
      }
      await userApi.tlAdd({
        userID:   userModal.value.form.userID.trim().toUpperCase(),
        userName: userModal.value.form.userName.trim(),
        roleID:   userModal.value.form.roleID,
      })
      emit('toast', 'User added successfully.')
    } else {
      await userApi.tlUpdateRole(userModal.value.form.userID, userModal.value.form.roleID)
      emit('toast', 'User role updated.')
    }
    closeUserModal()
    await loadUsers()
  } catch (err) {
    userModal.value.error = err.response?.data?.message || 'An error occurred.'
  } finally {
    userModal.value.saving = false
  }
}

// ── Toggle ─────────────────────────────────────────────────────────────────
async function toggleUser(user) {
  try {
    await userApi.tlToggle(user.userID)
    user.active = !user.active
  } catch (err) {
    emit('toast', err.response?.data?.message || 'Failed to update status.')
  }
}

// ── Remove from section ────────────────────────────────────────────────────
function confirmRemoveUser(user) {
  removeConfirm.value = { visible: true, userID: user.userID, userName: user.userName }
}

async function doRemoveUser() {
  try {
    await userApi.tlRemoveFromSection(removeConfirm.value.userID)
    emit('toast', 'User removed from section.')
    removeConfirm.value.visible = false
    await loadUsers()
  } catch (err) {
    emit('toast', err.response?.data?.message || 'Failed to remove user.')
    removeConfirm.value.visible = false
  }
}

// ── HCLAB search (identical pattern to admin UserManagementTab) ────────────
function computeDropdownPos() {
  const el = userIdInputRef.value
  if (!el) return
  const rect = el.getBoundingClientRect()
  hclabDropdownPos.value = { top: rect.bottom + 6, left: rect.left, width: rect.width }
}

function onHclabSearchFocus() {
  userIdInputRef.value?.style.setProperty('border-color', 'var(--color-primary)')
  if (userModal.value.hclabResults.length) {
    computeDropdownPos()
    userModal.value.hclabDropdownOpen = true
  }
}

function onHclabSearchBlur(e) {
  setTimeout(() => {
    e.target.style.borderColor = 'var(--color-border)'
    if (!userModal.value.hclabDropdownOpen) return
    if (!userModal.value.form.userID) closeHclabDropdown()
  }, 150)
}

function onHclabSearchInput() {
  userModal.value.form.userID = ''
  userModal.value.form.userName = ''
  userModal.value.hclabNoResults = false
  userModal.value.hclabDropdownOpen = false
  userModal.value.hclabActiveIdx = -1
  clearTimeout(hclabSearchTimer)

  const q = userModal.value.hclabSearch.trim()
  if (q.length < 3) { userModal.value.hclabResults = []; return }

  userModal.value.hclabLoading = true
  hclabSearchTimer = setTimeout(async () => {
    try {
      const res = await userApi.getHCLABUsers(q)
      userModal.value.hclabResults = res.data ?? []
      userModal.value.hclabNoResults = userModal.value.hclabResults.length === 0
      userModal.value.hclabDropdownOpen = true
      await nextTick()
      computeDropdownPos()
    } catch {
      userModal.value.hclabResults = []
      userModal.value.hclabNoResults = true
    } finally {
      userModal.value.hclabLoading = false
    }
  }, 350)
}

function selectHclabUser(u) {
  userModal.value.form.userID   = u.userID
  userModal.value.form.userName = u.userName
  userModal.value.hclabSearch   = `${u.userID} — ${u.userName}`
  userModal.value.hclabDropdownOpen = false
  userModal.value.hclabActiveIdx    = -1
}

function clearHclabSelection() {
  userModal.value.form.userID   = ''
  userModal.value.form.userName = ''
  userModal.value.hclabSearch   = ''
  userModal.value.hclabResults  = []
  userModal.value.hclabDropdownOpen = false
  userModal.value.hclabActiveIdx    = -1
  nextTick(() => userIdInputRef.value?.focus())
}

function closeHclabDropdown()   { userModal.value.hclabDropdownOpen = false }
function hclabSelectNext()      { if (!userModal.value.hclabResults.length) return; userModal.value.hclabActiveIdx = (userModal.value.hclabActiveIdx + 1) % userModal.value.hclabResults.length }
function hclabSelectPrev()      { if (!userModal.value.hclabResults.length) return; userModal.value.hclabActiveIdx = (userModal.value.hclabActiveIdx - 1 + userModal.value.hclabResults.length) % userModal.value.hclabResults.length }
function hclabConfirmSelected() { const idx = userModal.value.hclabActiveIdx; if (idx >= 0 && userModal.value.hclabResults[idx]) selectHclabUser(userModal.value.hclabResults[idx]) }
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

  .animate-pulse {
    animation: pulse 1.5s ease-in-out infinite;
  }

  @keyframes spin {
    to {
      transform: rotate(360deg);
    }
  }

  @keyframes pulse {
    0%, 100% {
      opacity: 1;
    }

    50% {
      opacity: 0.5;
    }
  }
</style>
