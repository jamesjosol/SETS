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
          <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">Manage user accounts, roles, and section access.</p>
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
                empty-text="No users found." empty-icon="manage_accounts">
        <template #cell-userID="{ value }">
          <span class="font-mono font-bold text-xs" style="color: var(--color-text)">{{ value }}</span>
        </template>
        <template #cell-sections="{ row }">
          <div class="flex flex-wrap gap-1">
            <span v-if="!row.sections.length" class="text-xs font-bold" style="color: var(--color-text-muted)">—</span>
            <span v-for="sec in row.sections" :key="sec.id"
                  class="flex items-center gap-1 px-2 py-0.5 rounded-lg text-[10px] font-bold"
                  style="background-color: var(--color-primary-soft); color: var(--color-primary)">
              {{ sec.sectionName }}<span class="opacity-60">·</span>{{ roleLabel(sec.roleID) }}
            </span>
          </div>
        </template>
        <template #cell-isAdmin="{ value }">
          <span v-if="value" class="material-symbols-outlined text-base" style="color: var(--color-primary)">verified_user</span>
          <span v-else class="text-[10px] font-bold" style="color: var(--color-text-muted)">—</span>
        </template>
        <template #cell-active="{ row }">
          <button class="flex items-center gap-1.5 px-3 py-1 rounded-lg text-[10px] font-bold uppercase tracking-widest transition-all"
                  :style="row.active
                    ? 'background-color: var(--color-primary-soft); color: var(--color-primary);'
                    : 'background-color: var(--color-surface-low); color: var(--color-text-muted); border: 1px solid var(--color-border);'"
                  @click.stop="toggleUser(row)">
            <span class="material-symbols-outlined text-xs">{{ row.active ? "check_circle" : "cancel" }}</span>
            {{ row.active ? "Active" : "Inactive" }}
          </button>
        </template>
        <template #actions="{ row }">
          <div class="flex items-center gap-1 justify-end">
            <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                    title="Edit user"
                    style="color: var(--color-text-muted)"
                    @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'var(--color-surface-low)')"
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click.stop="openEditUser(row)">
              <span class="material-symbols-outlined text-sm">edit</span>
            </button>
            <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                    title="Delete user"
                    style="color: #ba1a1a"
                    @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'rgba(186,26,26,0.08)')"
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click.stop="confirmDeleteUser(row)">
              <span class="material-symbols-outlined text-sm">delete</span>
            </button>
          </div>
        </template>
      </AppTable>
    </div>
  </div>

  <!-- Add/Edit User Modal -->
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
              <div class="w-8 h-8 rounded-lg flex items-center justify-center overflow-hidden flex-shrink-0"
                   style="background-color: var(--color-primary-soft)">
                <img v-if="userModal.mode === 'edit' && userModal.profilePicture"
                     :src="userModal.profilePicture"
                     class="w-full h-full object-cover cursor-zoom-in"
                     @mouseenter="showAvatarPreview"
                     @mouseleave="hideAvatarPreview" />
                <span v-else
                      class="material-symbols-outlined text-sm"
                      style="color: var(--color-primary)">
                  {{ userModal.mode === "add" ? "person_add" : "manage_accounts" }}
                </span>
              </div>
              <h3 class="text-sm font-extrabold tracking-tight" style="color: var(--color-text)">
                {{ userModal.mode === "add" ? "Add New User" : "Edit User" }}
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
            <!-- User ID — searchable dropdown (add mode only) -->
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
                  <div v-if="userModal.hclabNoResults" class="px-4 py-3 text-xs text-center" style="color: var(--color-text-muted)">
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
                      <div class="w-7 h-7 rounded-lg flex items-center justify-center flex-shrink-0"
                           style="background-color: var(--color-surface-low)">
                        <span class="material-symbols-outlined text-sm" style="color: var(--color-text-muted)">person</span>
                      </div>
                      <div class="min-w-0">
                        <p class="text-xs font-bold truncate" style="color: var(--color-text)">{{ u.userID }}</p>
                        <p class="text-[10px] truncate" style="color: var(--color-text-muted)">{{ u.userName }}</p>
                      </div>
                    </div>
                  </div>
                </div>
              </Teleport>
            </div>

            <!-- Name -->
            <div>
              <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5"
                     style="color: var(--color-text-muted)">Full Name *</label>
              <input v-model="userModal.form.userName"
                     placeholder="e.g. Juan Dela Cruz"
                     class="w-full px-4 py-2.5 rounded-xl text-sm font-medium outline-none transition-all"
                     style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                     @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                     @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
            </div>

            <!-- Is Admin toggle -->
            <div class="flex items-center justify-between py-1">
              <div>
                <p class="text-sm font-bold" style="color: var(--color-text)">Administrator</p>
                <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">Grants full access across all sections and settings.</p>
              </div>
              <button class="relative w-11 h-6 rounded-full transition-all flex-shrink-0"
                      :style="userModal.form.isAdmin ? 'background-color: var(--color-primary);' : 'background-color: var(--color-border);'"
                      @click="userModal.form.isAdmin = !userModal.form.isAdmin">
                <span class="absolute top-0.5 w-5 h-5 rounded-full bg-white shadow transition-all"
                      :style="userModal.form.isAdmin ? 'left: calc(100% - 1.375rem);' : 'left: 0.125rem;'"></span>
              </button>
            </div>

            <!-- Section Assignments -->
            <div>
              <div class="flex items-center justify-between mb-2">
                <label class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted)">Section Access *</label>
                <span class="text-[10px] font-bold" style="color: var(--color-text-muted)">{{ userModal.form.sections.length }} selected</span>
              </div>
              <p v-if="userModal.sectionError" class="mb-2 text-xs font-bold" style="color: #ba1a1a">
                At least one section must be assigned.
              </p>
              <div class="rounded-xl overflow-hidden" style="border: 1.5px solid var(--color-border)">
                <div v-for="sec in availableSections" :key="sec.code">
                  <div class="flex items-center gap-3 px-4 py-2.5 cursor-pointer transition-all"
                       :style="isUserSectionSelected(sec.code)
                         ? 'background-color: var(--color-primary-soft);'
                         : 'border-bottom: 1px solid var(--color-surface-low);'"
                       @click="toggleUserSection(sec.code)">
                    <div class="w-5 h-5 rounded-md flex items-center justify-center transition-all flex-shrink-0"
                         :style="isUserSectionSelected(sec.code)
                           ? 'background-color: var(--color-primary);'
                           : 'border: 1.5px solid var(--color-border);'">
                      <span v-if="isUserSectionSelected(sec.code)"
                            class="material-symbols-outlined text-xs" style="color: #ffffff">check</span>
                    </div>
                    <span class="text-sm font-medium flex-1" style="color: var(--color-text)">{{ sec.name }}</span>
                    <span class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted)">
                      {{ sec.category === "1" ? "Endorser" : sec.category === "2" ? "Receiver" : "Runner" }}
                    </span>
                  </div>
                  <!-- Role picker -->
                  <div v-if="isUserSectionSelected(sec.code)"
                       class="flex items-center gap-2 px-4 py-2.5"
                       style="background-color: var(--color-primary-soft); border-bottom: 1px solid rgba(0,0,0,0.06);"
                       @click.stop>
                    <span class="text-[10px] font-bold uppercase tracking-widest mr-1" style="color: var(--color-primary)">Role:</span>
                    <button v-for="role in roles" :key="role.id"
                            class="px-3 py-1 rounded-lg text-[10px] font-bold uppercase tracking-widest transition-all"
                            :style="getUserSectionRole(sec.code) === role.id
                              ? 'background-color: var(--color-primary); color: #ffffff;'
                              : 'background-color: rgba(255,255,255,0.6); color: var(--color-text-muted);'"
                            @click="setUserSectionRole(sec.code, role.id)">
                      {{ role.label }}
                    </button>
                  </div>
                </div>
                <p v-if="!availableSections.length" class="px-4 py-3 text-xs text-center" style="color: var(--color-text-muted)">
                  No sections available.
                </p>
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
              {{ userModal.mode === "add" ? "Add User" : "Save Changes" }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>

  <Teleport to="body">
    <div v-if="avatarPreview.visible"
         ref="avatarPreviewRef"
         class="fixed z-[9999] flex flex-col items-center gap-2 pointer-events-none"
         :style="`top: ${avatarPreview.y}px; left: ${avatarPreview.x}px; transform: translate(-50%, -50%);`">
      <div class="rounded-2xl overflow-hidden shadow-2xl"
           style="width: 160px; height: 160px; border: 2.5px solid var(--color-border);">
        <img :src="userModal.profilePicture" class="w-full h-full object-cover" />
      </div>
      <span class="text-xs font-bold px-3 py-1 rounded-full"
            style="background-color: var(--color-surface); color: var(--color-text); border: 1px solid var(--color-border); box-shadow: 0 2px 8px rgba(0,0,0,0.12);">
        {{ userModal.form.userName }}
      </span>
    </div>
  </Teleport>

  <!-- Delete User Confirm Modal -->
  <ConfirmModal :isVisible="deleteConfirm.visible"
                type="error"
                title="Delete User"
                :message="`Delete ${deleteConfirm.userName} (${deleteConfirm.userID})? This will deactivate the account and revoke all section access.`"
                confirm-label="Delete"
                @close="deleteConfirm.visible = false"
                @confirm="executeDeleteUser" />
</template>

<script setup>
  import { ref, computed, onMounted, nextTick } from "vue";
  import { gsap } from 'gsap'
  import { userApi } from "@/api/userApi";
  import { settingsApi } from "@/api/settingsApi";
  import AppTable from "@/components/common/AppTable.vue";
  import ConfirmModal from "@/components/common/ConfirmModal.vue";

  const emit = defineEmits(["toast"]);

  // ── Constants ──────────────────────────────────────────────────────────────
  const roles = [
    { id: 1, label: "Regular" },
    { id: 2, label: "Team Lead" },
  ];

  const userColumns = [
    { key: "userID", label: "User ID" },
    { key: "userName", label: "Name" },
    { key: "sections", label: "Sections & Roles" },
    { key: "isAdmin", label: "Admin" },
    { key: "active", label: "Status" },
  ];

  function roleLabel(roleID) {
    return roles.find((r) => r.id === roleID)?.label ?? "Regular";
  }

  // ── State ──────────────────────────────────────────────────────────────────
  const usersList = ref([]);
  const usersLoading = ref(false);
  const userSearch = ref("");
  const availableSections = ref([]);
  const userIdInputRef = ref(null);
  const hclabDropdownRef = ref(null);
  const hclabDropdownPos = ref({ top: 0, left: 0, width: 0 });
  let hclabSearchTimer = null;

  const filteredUsers = computed(() => {
    const q = userSearch.value.toLowerCase();
    if (!q) return usersList.value;
    return usersList.value.filter(
      (u) => u.userID.toLowerCase().includes(q) || u.userName.toLowerCase().includes(q),
    );
  });

  const userModal = ref({
    visible: false,
    mode: "add",
    userID: null,
    profilePicture: null,
    form: { userID: "", userName: "", isAdmin: false, sections: [] },
    hclabSearch: "",
    hclabResults: [],
    hclabLoading: false,
    hclabDropdownOpen: false,
    hclabNoResults: false,
    hclabActiveIdx: -1,
    saving: false,
    error: "",
    sectionError: false,
  });

  // ── Data loading ───────────────────────────────────────────────────────────
  async function loadUsers() {
    usersLoading.value = true;
    try {
      const res = await userApi.getAll();
      usersList.value = res.data;
    } catch {
      usersList.value = [];
    } finally {
      usersLoading.value = false;
    }
  }

  async function loadSections() {
    try {
      const res = await settingsApi.getSections();
      availableSections.value = res.data.filter((s) => s.active);
    } catch {
      availableSections.value = [];
    }
  }

  onMounted(() => {
    loadUsers();
    loadSections();
  });

  // ── Section selection helpers ──────────────────────────────────────────────
  function isUserSectionSelected(code) {
    return userModal.value.form.sections.some((s) => s.sectionCode === code);
  }
  function getUserSectionRole(code) {
    return userModal.value.form.sections.find((s) => s.sectionCode === code)?.roleID ?? 1;
  }
  function toggleUserSection(code) {
    const idx = userModal.value.form.sections.findIndex((s) => s.sectionCode === code);
    if (idx === -1) userModal.value.form.sections.push({ sectionCode: code, roleID: 1 });
    else userModal.value.form.sections.splice(idx, 1);
    userModal.value.sectionError = false;
  }
  function setUserSectionRole(code, roleID) {
    const entry = userModal.value.form.sections.find((s) => s.sectionCode === code);
    if (entry) entry.roleID = roleID;
  }

  // ── Modal actions ──────────────────────────────────────────────────────────
  function openAddUser() {
    userModal.value = {
      visible: true, mode: "add", userID: null, profilePicture: null,
      form: { userID: "", userName: "", isAdmin: false, sections: [] },
      hclabSearch: "", hclabResults: [], hclabLoading: false,
      hclabDropdownOpen: false, hclabNoResults: false, hclabActiveIdx: -1,
      saving: false, error: "", sectionError: false,
    };
  }

  function openEditUser(user) {
    userModal.value = {
      visible: true, mode: "edit", userID: user.userID,
      profilePicture: user.profilePicture ?? null,
      form: {
        userID: user.userID, userName: user.userName, isAdmin: user.isAdmin,
        sections: user.sections.map((s) => ({ sectionCode: s.sectionCode, roleID: s.roleID })),
      },
      saving: false, error: "", sectionError: false,
    };
  }

  function closeUserModal() {
    userModal.value.profilePicture = null
    userModal.value.visible = false;
  }

  async function saveUserModal() {
    userModal.value.error = "";
    userModal.value.sectionError = false;

    if (!userModal.value.form.sections.length) {
      userModal.value.sectionError = true;
      return;
    }

    userModal.value.saving = true;
    try {
      if (userModal.value.mode === "add") {
        if (!userModal.value.form.userID.trim()) { userModal.value.error = "User ID is required."; return; }
        if (!userModal.value.form.userName.trim()) { userModal.value.error = "Full name is required."; return; }
        await userApi.add({
          userID: userModal.value.form.userID.trim().toUpperCase(),
          userName: userModal.value.form.userName.trim(),
          isAdmin: userModal.value.form.isAdmin,
          sections: userModal.value.form.sections,
        });
        emit("toast", "User added successfully.");
      } else {
        await userApi.update(userModal.value.userID, {
          userName: userModal.value.form.userName.trim(),
          isAdmin: userModal.value.form.isAdmin,
          active: usersList.value.find((u) => u.userID === userModal.value.userID)?.active ?? true,
        });
        await userApi.updateSections(userModal.value.userID, { sections: userModal.value.form.sections });
        emit("toast", "User updated successfully.");
      }
      closeUserModal();
      await loadUsers();
    } catch (err) {
      userModal.value.error = err.response?.data?.message || "An error occurred.";
    } finally {
      userModal.value.saving = false;
    }
  }

  async function toggleUser(user) {
    try {
      await userApi.toggle(user.userID);
      user.active = !user.active;
    } catch (err) {
      emit("toast", err.response?.data?.message || "Failed to update status.");
    }
  }

  const avatarPreviewRef = ref(null)
  const avatarPreview = ref({ visible: false, x: 0, y: 0 })

  function showAvatarPreview(e) {
    const rect = e.currentTarget.getBoundingClientRect()
    const previewH = 200 // approx height of preview + label
    const spaceAbove = rect.top
    const spaceBelow = window.innerHeight - rect.bottom

    let y
    if (spaceAbove >= previewH + 12) {
      // enough space above
      y = rect.top - 12
    } else if (spaceBelow >= previewH + 12) {
      // flip to below
      y = rect.bottom + 12 + previewH / 2
    } else {
      // not enough space either way — center on viewport
      y = window.innerHeight / 2
    }

    avatarPreview.value = {
      visible: true,
      x: rect.left + rect.width / 2,
      y,
    }

    nextTick(() => {
      if (!avatarPreviewRef.value) return
      gsap.set(avatarPreviewRef.value, { scale: 0.6, opacity: 0 })
      gsap.to(avatarPreviewRef.value, {
        scale: 1, opacity: 1,
        duration: 0.25, ease: 'back.out(1.4)',
      })
    })
  }

  function hideAvatarPreview() {
    if (!avatarPreviewRef.value) return
    gsap.to(avatarPreviewRef.value, {
      scale: 0.6, opacity: 0,
      duration: 0.15, ease: 'power2.in',
      onComplete: () => { avatarPreview.value.visible = false }
    })
  }

  // ── Delete ─────────────────────────────────────────────────────────────────
  const deleteConfirm = ref({ visible: false, userID: "", userName: "" });

  function confirmDeleteUser(user) {
    deleteConfirm.value = { visible: true, userID: user.userID, userName: user.userName };
  }

  async function executeDeleteUser() {
    try {
      await userApi.delete(deleteConfirm.value.userID);
      usersList.value = usersList.value.filter((u) => u.userID !== deleteConfirm.value.userID);
      emit("toast", "User deleted successfully.");
    } catch (err) {
      emit("toast", err.response?.data?.message || "Failed to delete user.");
    } finally {
      deleteConfirm.value.visible = false;
    }
  }

  // ── HCLAB search ───────────────────────────────────────────────────────────
  function computeDropdownPos() {
    const el = userIdInputRef.value;
    if (!el) return;
    const rect = el.getBoundingClientRect();
    hclabDropdownPos.value = { top: rect.bottom + 6, left: rect.left, width: rect.width };
  }

  function onHclabSearchFocus() {
    userIdInputRef.value?.style.setProperty("border-color", "var(--color-primary)");
    if (userModal.value.hclabResults.length) {
      computeDropdownPos();
      userModal.value.hclabDropdownOpen = true;
    }
  }

  function onHclabSearchBlur(e) {
    setTimeout(() => {
      e.target.style.borderColor = "var(--color-border)";
      if (!userModal.value.hclabDropdownOpen) return;
      if (!userModal.value.form.userID) closeHclabDropdown();
    }, 150);
  }

  function onHclabSearchInput() {
    userModal.value.form.userID = "";
    userModal.value.form.userName = "";
    userModal.value.hclabNoResults = false;
    userModal.value.hclabDropdownOpen = false;
    userModal.value.hclabActiveIdx = -1;
    clearTimeout(hclabSearchTimer);

    const q = userModal.value.hclabSearch.trim();
    if (q.length < 3) { userModal.value.hclabResults = []; return; }

    userModal.value.hclabLoading = true;
    hclabSearchTimer = setTimeout(async () => {
      try {
        const res = await userApi.getHCLABUsers(q);
        userModal.value.hclabResults = res.data ?? [];
        userModal.value.hclabNoResults = userModal.value.hclabResults.length === 0;
        userModal.value.hclabDropdownOpen = true;
        await nextTick();
        computeDropdownPos();
      } catch {
        userModal.value.hclabResults = [];
        userModal.value.hclabNoResults = true;
      } finally {
        userModal.value.hclabLoading = false;
      }
    }, 350);
  }

  function selectHclabUser(u) {
    userModal.value.form.userID = u.userID;
    userModal.value.form.userName = u.userName;
    userModal.value.hclabSearch = `${u.userID} — ${u.userName}`;
    userModal.value.hclabDropdownOpen = false;
    userModal.value.hclabActiveIdx = -1;
  }

  function clearHclabSelection() {
    userModal.value.form.userID = "";
    userModal.value.form.userName = "";
    userModal.value.hclabSearch = "";
    userModal.value.hclabResults = [];
    userModal.value.hclabDropdownOpen = false;
    userModal.value.hclabActiveIdx = -1;
    nextTick(() => userIdInputRef.value?.focus());
  }

  function closeHclabDropdown() { userModal.value.hclabDropdownOpen = false; }

  function hclabSelectNext() {
    if (!userModal.value.hclabResults.length) return;
    userModal.value.hclabActiveIdx = (userModal.value.hclabActiveIdx + 1) % userModal.value.hclabResults.length;
  }
  function hclabSelectPrev() {
    if (!userModal.value.hclabResults.length) return;
    userModal.value.hclabActiveIdx =
      (userModal.value.hclabActiveIdx - 1 + userModal.value.hclabResults.length) % userModal.value.hclabResults.length;
  }
  function hclabConfirmSelected() {
    const idx = userModal.value.hclabActiveIdx;
    if (idx >= 0 && userModal.value.hclabResults[idx]) selectHclabUser(userModal.value.hclabResults[idx]);
  }
</script>
