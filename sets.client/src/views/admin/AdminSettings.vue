<template>
  <AppLayout>
    <!-- Page Header -->
    <div class="mb-6">
      <h1 class="text-2xl font-extrabold tracking-tight" style="color: var(--color-text)">
        Global Settings
      </h1>
      <p class="text-sm mt-1" style="color: var(--color-text-muted)">
        <span style="color: var(--color-primary); font-weight: 700">ADMINISTRATOR</span>
        · System Configuration
      </p>
    </div>

    <!-- Settings Layout: Sidebar + Content -->
    <div class="flex gap-6">
      <!-- Left Nav Sidebar -->
      <aside class="w-56 flex-shrink-0">
        <div
          class="rounded-2xl overflow-hidden sticky top-6"
          style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)"
        >
          <div class="p-2 space-y-0.5">
            <button
              v-for="tab in settingsTabs"
              :key="tab.key"
              class="w-full flex items-center gap-3 px-4 py-3 rounded-xl text-xs font-bold uppercase tracking-widest transition-all text-left"
              :style="
                activeTab === tab.key
                  ? 'background-color: var(--color-primary-soft); color: var(--color-primary); border-left: 3px solid var(--color-primary); padding-left: calc(1rem - 3px);'
                  : 'color: var(--color-text-muted); border-left: 3px solid transparent; padding-left: calc(1rem - 3px);'
              "
              @click="activeTab = tab.key"
            >
              <span class="material-symbols-outlined text-base">{{ tab.icon }}</span>
              {{ tab.label }}
            </button>
          </div>
        </div>
      </aside>

      <!-- Right Content Panel -->
      <div class="flex-1 min-w-0">
        <!-- ══════════════════════════════════════════════════════════
  PC REGISTRATION
  ══════════════════════════════════════════════════════════ -->
        <div v-if="activeTab === 'pc'"
             class="rounded-2xl overflow-hidden"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)">
          <!-- Header -->
          <div class="px-8 py-5 flex items-center justify-between"
               style="border-bottom: 1px solid var(--color-border)">
            <div class="flex items-center gap-4">
              <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
                   style="background-color: var(--color-primary-soft)">
                <span class="material-symbols-outlined text-base"
                      style="color: var(--color-primary)">computer</span>
              </div>
              <div>
                <h2 class="text-base font-extrabold tracking-tight"
                    style="color: var(--color-text)">
                  PC Registration
                </h2>
                <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">
                  Register workstations by IP address and assign which sections they can access.
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

          <!-- PC Table -->
          <div class="px-8 py-6 space-y-5">
            <div class="relative">
              <span class="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-base"
                    style="color: var(--color-text-muted)">search</span>
              <input v-model="pcSearch"
                     class="w-full pl-10 pr-4 py-2.5 rounded-xl text-sm font-medium outline-none transition-all"
                     style="
                  background-color: var(--color-surface-low);
                  color: var(--color-text);
                  border: 1.5px solid var(--color-border);
                "
                     placeholder="Search by IP, description, or section..."
                     @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                     @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
            </div>
            <AppTable :rows="filteredPCs"
                      :columns="pcColumns"
                      row-key="id"
                      :page-size="10"
                      :loading="pcLoading"
                      loading-text="Loading registered PCs..."
                      empty-text="No PCs registered yet."
                      empty-icon="computer_off">
              <!-- IP Address -->
              <template #cell-ipAddress="{ value }">
                <span class="font-mono font-bold text-xs" style="color: var(--color-text)">
                  {{
                  value
                  }}
                </span>
              </template>

              <!-- Description -->
              <template #cell-description="{ value }">
                <span class="text-sm" style="color: var(--color-text-muted)">
                  {{
                  value || "—"
                  }}
                </span>
              </template>

              <!-- Sections -->
              <template #cell-sections="{ row }">
                <div class="flex flex-wrap gap-1">
                  <span v-if="!row.sections.length"
                        class="text-xs font-bold"
                        style="color: var(--color-text-muted)">No sections</span>
                  <span v-for="sec in row.sections"
                        :key="sec.id"
                        class="px-2 py-0.5 rounded-lg text-[10px] font-bold"
                        style="background-color: var(--color-primary-soft); color: var(--color-primary)">
                    {{ sec.sectionName }}
                  </span>
                </div>
              </template>

              <!-- Status -->
              <template #cell-active="{ row }">
                <button class="flex items-center gap-1.5 px-3 py-1 rounded-lg text-[10px] font-bold uppercase tracking-widest transition-all"
                        :style="
                    row.active
                      ? 'background-color: var(--color-primary-soft); color: var(--color-primary);'
                      : 'background-color: var(--color-surface-low); color: var(--color-text-muted); border: 1px solid var(--color-border);'
                  "
                        @click.stop="togglePC(row)">
                  <span class="material-symbols-outlined text-xs">
                    {{
                    row.active ? "check_circle" : "cancel"
                    }}
                  </span>
                  {{ row.active ? "Active" : "Inactive" }}
                </button>
              </template>

              <!-- Actions -->
              <template #actions="{ row }">
                <div class="flex items-center gap-1 justify-end">
                  <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                          style="color: var(--color-text-muted)"
                          @mouseenter="
                          (e)=>
                    (e.currentTarget.style.backgroundColor = 'var(--color-surface-low)')
                    "
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click.stop="openEditSections(row)"
                    >
                    <span class="material-symbols-outlined text-sm">edit</span>
                  </button>
                </div>
              </template>
            </AppTable>
          </div>
        </div>

        <!-- ══════════════════════════════════════════════════════════
  ADD PC MODAL
   ══════════════════════════════════════════════════════════ -->
        <Teleport to="body">
          <Transition name="modal">
            <div v-if="pcModal.visible"
                 class="fixed inset-0 z-50 flex items-center justify-center"
                 style="background-color: rgba(0, 0, 0, 0.4)"
                 @click.self="closePCModal">
              <div class="w-full max-w-md rounded-2xl overflow-hidden"
                   style="
                  background-color: var(--color-surface);
                  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.24);
                ">
                <!-- Modal Header -->
                <div class="px-6 py-4 flex items-center justify-between"
                     style="border-bottom: 1px solid var(--color-border)">
                  <div class="flex items-center gap-3">
                    <div class="w-8 h-8 rounded-lg flex items-center justify-center"
                         style="background-color: var(--color-primary-soft)">
                      <span class="material-symbols-outlined text-sm"
                            style="color: var(--color-primary)">computer</span>
                    </div>
                    <h3 class="text-sm font-extrabold tracking-tight"
                        style="color: var(--color-text)">
                      {{ pcModal.mode === "add" ? "Register New PC" : "Edit Section Assignments" }}
                    </h3>
                  </div>
                  <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                          style="color: var(--color-text-muted)"
                          @mouseenter="
                          (e)=>
                    (e.currentTarget.style.backgroundColor = 'var(--color-surface-low)')
                    "
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click="closePCModal"
                    >
                    <span class="material-symbols-outlined text-base">close</span>
                  </button>
                </div>

                <!-- Modal Body -->
                <div class="px-6 py-5 space-y-4">
                  <!-- IP Address (add mode only) -->
                  <div v-if="pcModal.mode === 'add'">
                    <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5"
                           style="color: var(--color-text-muted)">IP Address *</label>
                    <input v-model="pcModal.form.ipAddress"
                           placeholder="e.g. 192.168.1.100"
                           class="w-full px-4 py-2.5 rounded-xl text-sm font-mono font-medium outline-none transition-all"
                           style="
                        background-color: var(--color-surface-low);
                        color: var(--color-text);
                        border: 1.5px solid var(--color-border);
                      "
                           @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                           @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
                  </div>

                  <!-- Description (add mode only) -->
                  <div v-if="pcModal.mode === 'add'">
                    <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5"
                           style="color: var(--color-text-muted)">Description</label>
                    <input v-model="pcModal.form.description"
                           placeholder="e.g. Hematology workstation"
                           class="w-full px-4 py-2.5 rounded-xl text-sm font-medium outline-none transition-all"
                           style="
                        background-color: var(--color-surface-low);
                        color: var(--color-text);
                        border: 1.5px solid var(--color-border);
                      "
                           @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                           @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
                  </div>

                  <!-- Section Assignment -->
                  <div>
                    <label class="block text-[10px] font-bold uppercase tracking-widest mb-2"
                           style="color: var(--color-text-muted)">Allowed Sections</label>
                    <div class="rounded-xl overflow-hidden"
                         style="border: 1.5px solid var(--color-border)">
                      <div v-for="sec in availableSections"
                           :key="sec.code"
                           class="flex items-center justify-between px-4 py-2.5 transition-all cursor-pointer"
                           style="border-bottom: 1px solid var(--color-surface-low)"
                           :style="
                          pcModal.form.sectionCodes.includes(sec.code)
                            ? 'background-color: var(--color-primary-soft);'
                            : ''
                        "
                           @click="toggleSectionInModal(sec.code)">
                        <span class="text-sm font-medium" style="color: var(--color-text)">
                          {{
                          sec.name
                          }}
                        </span>
                        <div class="w-5 h-5 rounded-md flex items-center justify-center transition-all"
                             :style="
                            pcModal.form.sectionCodes.includes(sec.code)
                              ? 'background-color: var(--color-primary);'
                              : 'border: 1.5px solid var(--color-border);'
                          ">
                          <span v-if="pcModal.form.sectionCodes.includes(sec.code)"
                                class="material-symbols-outlined text-xs"
                                style="color: #ffffff">check</span>
                        </div>
                      </div>
                      <p v-if="!availableSections.length"
                         class="px-4 py-3 text-xs text-center"
                         style="color: var(--color-text-muted)">
                        No sections available.
                      </p>
                    </div>
                  </div>

                  <!-- Error -->
                  <p v-if="pcModal.error" class="text-xs font-bold" style="color: #ba1a1a">
                    {{ pcModal.error }}
                  </p>
                </div>

                <!-- Modal Footer -->
                <div class="px-6 py-4 flex justify-end gap-3"
                     style="border-top: 1px solid var(--color-border)">
                  <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                          style="
                      background-color: var(--color-surface-low);
                      color: var(--color-text-muted);
                    "
                          @click="closePCModal">
                    Cancel
                  </button>
                  <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow"
                          style="background: var(--color-primary-gradient); color: #ffffff"
                          :disabled="pcModal.saving"
                          @click="savePCModal">
                    <span v-if="pcModal.saving"
                          class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
                    <span class="material-symbols-outlined text-sm" v-else>save</span>
                    {{ pcModal.mode === "add" ? "Register" : "Save" }}
                  </button>
                </div>
              </div>
            </div>
          </Transition>
        </Teleport>

        <!-- ══════════════════════════════════════════════════════════
       USER MANAGEMENT TAB
  ══════════════════════════════════════════════════════════ -->
        <div v-if="activeTab === 'users'"
             class="rounded-2xl overflow-hidden"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)">
          <!-- Header -->
          <div class="px-8 py-5 flex items-center justify-between"
               style="border-bottom: 1px solid var(--color-border)">
            <div class="flex items-center gap-4">
              <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
                   style="background-color: var(--color-primary-soft)">
                <span class="material-symbols-outlined text-base"
                      style="color: var(--color-primary)">manage_accounts</span>
              </div>
              <div>
                <h2 class="text-base font-extrabold tracking-tight"
                    style="color: var(--color-text)">
                  User Management
                </h2>
                <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">
                  Manage user accounts, roles, and section access.
                </p>
              </div>
            </div>
            <button class="flex items-center gap-2 px-4 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 shadow"
                    style="background: var(--color-primary-gradient); color: #ffffff"
                    @click="openAddUser">
              <span class="material-symbols-outlined text-sm">person_add</span>
              Add User
            </button>
          </div>

          <div class="px-8 py-6 space-y-5">
            <!-- Search (unchanged) -->
            <div class="relative">
              <span class="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-base"
                    style="color: var(--color-text-muted)">search</span>
              <input v-model="userSearch"
                     class="w-full pl-10 pr-4 py-2.5 rounded-xl text-sm font-medium outline-none transition-all"
                     style="
                  background-color: var(--color-surface-low);
                  color: var(--color-text);
                  border: 1.5px solid var(--color-border);
                "
                     placeholder="Search by User ID or Name..."
                     @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                     @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
            </div>

            <AppTable :rows="filteredUsers"
                      :columns="userColumns"
                      row-key="userID"
                      :page-size="10"
                      :loading="usersLoading"
                      loading-text="Loading users..."
                      empty-text="No users found."
                      empty-icon="manage_accounts">
              <!-- User ID -->
              <template #cell-userID="{ value }">
                <span class="font-mono font-bold text-xs" style="color: var(--color-text)">
                  {{
                  value
                  }}
                </span>
              </template>

              <!-- Sections & Roles -->
              <template #cell-sections="{ row }">
                <div class="flex flex-wrap gap-1">
                  <span v-if="!row.sections.length"
                        class="text-xs font-bold"
                        style="color: var(--color-text-muted)">—</span>
                  <span v-for="sec in row.sections"
                        :key="sec.id"
                        class="flex items-center gap-1 px-2 py-0.5 rounded-lg text-[10px] font-bold"
                        style="background-color: var(--color-primary-soft); color: var(--color-primary)">
                    {{ sec.sectionName }}
                    <span class="opacity-60">·</span>
                    {{ roleLabel(sec.roleID) }}
                  </span>
                </div>
              </template>

              <!-- Admin badge -->
              <template #cell-isAdmin="{ value }">
                <span v-if="value"
                      class="material-symbols-outlined text-base"
                      style="color: var(--color-primary)">verified_user</span>
                <span v-else class="text-[10px] font-bold" style="color: var(--color-text-muted)">—</span>
              </template>

              <!-- Status -->
              <template #cell-active="{ row }">
                <button class="flex items-center gap-1.5 px-3 py-1 rounded-lg text-[10px] font-bold uppercase tracking-widest transition-all"
                        :style="
                    row.active
                      ? 'background-color: var(--color-primary-soft); color: var(--color-primary);'
                      : 'background-color: var(--color-surface-low); color: var(--color-text-muted); border: 1px solid var(--color-border);'
                  "
                        @click.stop="toggleUser(row)">
                  <span class="material-symbols-outlined text-xs">
                    {{
                    row.active ? "check_circle" : "cancel"
                    }}
                  </span>
                  {{ row.active ? "Active" : "Inactive" }}
                </button>
              </template>

              <!-- Actions -->
              <template #actions="{ row }">
                <div class="flex items-center gap-1 justify-end">
                  <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                          style="color: var(--color-text-muted)"
                          @mouseenter="
                          (e)=>
                    (e.currentTarget.style.backgroundColor = 'var(--color-surface-low)')
                    "
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click.stop="openEditUser(row)"
                    >
                    <span class="material-symbols-outlined text-sm">edit</span>
                  </button>
                </div>
              </template>
            </AppTable>
          </div>
        </div>

        <!-- ══════════════════════════════════════════════════════════
       ADD / EDIT USER MODAL
  ══════════════════════════════════════════════════════════ -->
        <Teleport to="body">
          <Transition name="modal">
            <div v-if="userModal.visible"
                 class="fixed inset-0 z-50 flex items-center justify-center"
                 style="background-color: rgba(0, 0, 0, 0.4)"
                 @click.self="closeUserModal">
              <div class="w-full max-w-lg rounded-2xl overflow-hidden flex flex-col"
                   style="
                  background-color: var(--color-surface);
                  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.24);
                  max-height: 90vh;
                ">
                <!-- Modal Header -->
                <div class="px-6 py-4 flex items-center justify-between flex-shrink-0"
                     style="border-bottom: 1px solid var(--color-border)">
                  <div class="flex items-center gap-3">
                    <div class="w-8 h-8 rounded-lg flex items-center justify-center"
                         style="background-color: var(--color-primary-soft)">
                      <span class="material-symbols-outlined text-sm"
                            style="color: var(--color-primary)">
                        {{ userModal.mode === "add" ? "person_add" : "manage_accounts" }}
                      </span>
                    </div>
                    <h3 class="text-sm font-extrabold tracking-tight"
                        style="color: var(--color-text)">
                      {{ userModal.mode === "add" ? "Add New User" : "Edit User" }}
                    </h3>
                  </div>
                  <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                          style="color: var(--color-text-muted)"
                          @mouseenter="
                          (e)=>
                    (e.currentTarget.style.backgroundColor = 'var(--color-surface-low)')
                    "
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click="closeUserModal"
                    >
                    <span class="material-symbols-outlined text-base">close</span>
                  </button>
                </div>

                <!-- Modal Body -->
                <div class="px-6 py-5 space-y-5 overflow-y-auto flex-1">
                  <!-- User ID — searchable dropdown (add mode only) -->
                  <div v-if="userModal.mode === 'add'" class="relative">
                    <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5"
                           style="color: var(--color-text-muted)">User ID *</label>

                    <!-- Input wrapper -->
                    <div class="relative">
                      <input v-model="userModal.hclabSearch"
                             ref="userIdInputRef"
                             placeholder="Type 3+ characters to search..."
                             class="w-full px-4 py-2.5 pr-10 rounded-xl text-sm font-medium outline-none transition-all"
                             style="
                          background-color: var(--color-surface-low);
                          color: var(--color-text);
                          border: 1.5px solid var(--color-border);
                        "
                             autocomplete="off"
                             @input="onHclabSearchInput"
                             @focus="onHclabSearchFocus"
                             @keydown.down.prevent="hclabSelectNext"
                             @keydown.up.prevent="hclabSelectPrev"
                             @keydown.enter.prevent="hclabConfirmSelected"
                             @keydown.escape="closeHclabDropdown"
                             @focus.once="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                             @blur="onHclabSearchBlur" />

                      <!-- Right icon: spinner / clear / search -->
                      <div class="absolute right-3 top-1/2 -translate-y-1/2 pointer-events-none">
                        <span v-if="userModal.hclabLoading"
                              class="material-symbols-outlined text-sm animate-spin"
                              style="color: var(--color-text-muted)">progress_activity</span>
                        <span v-else-if="userModal.form.userID"
                              class="material-symbols-outlined text-sm pointer-events-auto cursor-pointer"
                              style="color: var(--color-primary)"
                              @click.stop="clearHclabSelection">check_circle</span>
                        <span v-else
                              class="material-symbols-outlined text-sm"
                              style="color: var(--color-text-muted)">search</span>
                      </div>
                    </div>

                    <!-- Selected user badge -->
                    <div v-if="userModal.form.userID"
                         class="mt-1.5 flex items-center gap-2 px-3 py-2 rounded-lg"
                         style="background-color: var(--color-primary-soft)">
                      <span class="material-symbols-outlined text-sm"
                            style="color: var(--color-primary)">person</span>
                      <div class="flex-1 min-w-0">
                        <p class="text-xs font-bold truncate" style="color: var(--color-primary)">
                          {{ userModal.form.userID }}
                        </p>
                        <p class="text-[10px] truncate" style="color: var(--color-text-muted)">
                          {{ userModal.form.userName }}
                        </p>
                      </div>
                      <button class="material-symbols-outlined text-sm flex-shrink-0 transition-all"
                              style="color: var(--color-text-muted)"
                              @click="clearHclabSelection">
                        close
                      </button>
                    </div>

                    <!-- Hint -->
                    <p v-if="
                       !userModal.form.userID &&
                       userModal.hclabSearch.length < 3 &&
                        !userModal.hclabResults.length
                      "
                      class="mt-1 text-[10px]"
                      style="color: var(--color-text-muted)"
                    >
                      Search by HCLAB's' User ID or name (min. 3 characters).
                    </p>

                    <!-- Dropdown -->
                    <Teleport to="body">
                      <div v-if="
                          userModal.hclabDropdownOpen &&
                          (userModal.hclabResults.length || userModal.hclabNoResults)
                        "
                           ref="hclabDropdownRef"
                           class="fixed z-[60] rounded-xl overflow-hidden shadow-xl"
                           :style="`top: ${hclabDropdownPos.top}px; left: ${hclabDropdownPos.left}px; width: ${hclabDropdownPos.width}px;
                  background-color: var(--color-surface); border: 1.5px solid var(--color-border);`">
                        <!-- No results -->
                        <div v-if="userModal.hclabNoResults"
                             class="px-4 py-3 text-xs text-center"
                             style="color: var(--color-text-muted)">
                          No users found for "{{ userModal.hclabSearch }}".
                        </div>

                        <!-- Result rows -->
                        <div v-else class="overflow-y-auto" style="max-height: 220px">
                          <div v-for="(u, idx) in userModal.hclabResults"
                               :key="u.userID"
                               class="flex items-center gap-3 px-4 py-2.5 cursor-pointer transition-all"
                               :style="
                              idx === userModal.hclabActiveIdx
                                ? 'background-color: var(--color-primary-soft);'
                                : 'border-bottom: 1px solid var(--color-surface-low);'
                            "
                               @mouseenter="userModal.hclabActiveIdx = idx"
                               @mousedown.prevent="selectHclabUser(u)">
                            <div class="w-7 h-7 rounded-lg flex items-center justify-center flex-shrink-0"
                                 style="background-color: var(--color-surface-low)">
                              <span class="material-symbols-outlined text-sm"
                                    style="color: var(--color-text-muted)">person</span>
                            </div>
                            <div class="min-w-0">
                              <p class="text-xs font-bold truncate"
                                 style="color: var(--color-text)">
                                {{ u.userID }}
                              </p>
                              <p class="text-[10px] truncate"
                                 style="color: var(--color-text-muted)">
                                {{ u.userName }}
                              </p>
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
                           style="
                        background-color: var(--color-surface-low);
                        color: var(--color-text);
                        border: 1.5px solid var(--color-border);
                      "
                           @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                           @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
                  </div>

                  <!-- Is Admin toggle -->
                  <div class="flex items-center justify-between py-1">
                    <div>
                      <p class="text-sm font-bold" style="color: var(--color-text)">
                        Administrator
                      </p>
                      <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">
                        Grants full access across all sections and settings.
                      </p>
                    </div>
                    <button class="relative w-11 h-6 rounded-full transition-all flex-shrink-0"
                            :style="
                        userModal.form.isAdmin
                          ? 'background-color: var(--color-primary);'
                          : 'background-color: var(--color-border);'
                      "
                            @click="userModal.form.isAdmin = !userModal.form.isAdmin">
                      <span class="absolute top-0.5 w-5 h-5 rounded-full bg-white shadow transition-all"
                            :style="
                          userModal.form.isAdmin
                            ? 'left: calc(100% - 1.375rem);'
                            : 'left: 0.125rem;'
                        "></span>
                    </button>
                  </div>

                  <!-- Section Assignments -->
                  <div>
                    <div class="flex items-center justify-between mb-2">
                      <label class="text-[10px] font-bold uppercase tracking-widest"
                             style="color: var(--color-text-muted)">Section Access *</label>
                      <span class="text-[10px] font-bold" style="color: var(--color-text-muted)">
                        {{ userModal.form.sections.length }} selected
                      </span>
                    </div>

                    <!-- Validation hint -->
                    <p v-if="userModal.sectionError"
                       class="mb-2 text-xs font-bold"
                       style="color: #ba1a1a">
                      At least one section must be assigned.
                    </p>

                    <div class="rounded-xl overflow-hidden"
                         style="border: 1.5px solid var(--color-border)">
                      <div v-for="sec in availableSections" :key="sec.code">
                        <!-- Section row header (click to toggle) -->
                        <div class="flex items-center gap-3 px-4 py-2.5 cursor-pointer transition-all"
                             :style="
                            isUserSectionSelected(sec.code)
                              ? 'background-color: var(--color-primary-soft);'
                              : 'border-bottom: 1px solid var(--color-surface-low);'
                          "
                             @click="toggleUserSection(sec.code)">
                          <div class="w-5 h-5 rounded-md flex items-center justify-center transition-all flex-shrink-0"
                               :style="
                              isUserSectionSelected(sec.code)
                                ? 'background-color: var(--color-primary);'
                                : 'border: 1.5px solid var(--color-border);'
                            ">
                            <span v-if="isUserSectionSelected(sec.code)"
                                  class="material-symbols-outlined text-xs"
                                  style="color: #ffffff">check</span>
                          </div>
                          <span class="text-sm font-medium flex-1" style="color: var(--color-text)">
                            {{ sec.name }}
                          </span>
                          <span class="text-[10px] font-bold uppercase tracking-widest"
                                style="color: var(--color-text-muted)">
                            {{
                              sec.category === "1"
                                ? "Endorser"
                                : sec.category === "2"
                                  ? "Receiver"
                                  : "Runner"
                            }}
                          </span>
                        </div>

                        <!-- Role picker (shown when section is selected) -->
                        <div v-if="isUserSectionSelected(sec.code)"
                             class="flex items-center gap-2 px-4 py-2.5"
                             style="
                            background-color: var(--color-primary-soft);
                            border-bottom: 1px solid rgba(0, 0, 0, 0.06);
                          "
                             @click.stop>
                          <span class="text-[10px] font-bold uppercase tracking-widest mr-1"
                                style="color: var(--color-primary)">Role:</span>
                          <button v-for="role in roles"
                                  :key="role.id"
                                  class="px-3 py-1 rounded-lg text-[10px] font-bold uppercase tracking-widest transition-all"
                                  :style="
                              getUserSectionRole(sec.code) === role.id
                                ? 'background-color: var(--color-primary); color: #ffffff;'
                                : 'background-color: rgba(255,255,255,0.6); color: var(--color-text-muted);'
                            "
                                  @click="setUserSectionRole(sec.code, role.id)">
                            {{ role.label }}
                          </button>
                        </div>
                      </div>

                      <p v-if="!availableSections.length"
                         class="px-4 py-3 text-xs text-center"
                         style="color: var(--color-text-muted)">
                        No sections available.
                      </p>
                    </div>
                  </div>

                  <!-- Error -->
                  <p v-if="userModal.error" class="text-xs font-bold" style="color: #ba1a1a">
                    {{ userModal.error }}
                  </p>
                </div>

                <!-- Modal Footer -->
                <div class="px-6 py-4 flex justify-end gap-3 flex-shrink-0"
                     style="border-top: 1px solid var(--color-border)">
                  <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                          style="
                      background-color: var(--color-surface-low);
                      color: var(--color-text-muted);
                    "
                          @click="closeUserModal">
                    Cancel
                  </button>
                  <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow"
                          style="background: var(--color-primary-gradient); color: #ffffff"
                          :disabled="userModal.saving"
                          @click="saveUserModal">
                    <span v-if="userModal.saving"
                          class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
                    <span class="material-symbols-outlined text-sm" v-else>save</span>
                    {{ userModal.mode === "add" ? "Add User" : "Save Changes" }}
                  </button>
                </div>
              </div>
            </div>
          </Transition>
        </Teleport>

        <!-- ══════════════════════════════════════════════════════════
       SECTION MANAGEMENT TAB
  ══════════════════════════════════════════════════════════ -->
        <div v-if="activeTab === 'sections'"
             class="rounded-2xl overflow-hidden"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)">
          <!-- Header -->
          <div class="px-8 py-5 flex items-center justify-between"
               style="border-bottom: 1px solid var(--color-border)">
            <div class="flex items-center gap-4">
              <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
                   style="background-color: var(--color-primary-soft)">
                <span class="material-symbols-outlined text-base"
                      style="color: var(--color-primary)">apartment</span>
              </div>
              <div>
                <h2 class="text-base font-extrabold tracking-tight"
                    style="color: var(--color-text)">
                  Section Management
                </h2>
                <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">
                  Manage sections by branch. One Receiver is allowed per branch.
                </p>
              </div>
            </div>
            <button class="flex items-center gap-2 px-4 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 shadow"
                    style="background: var(--color-primary-gradient); color: #ffffff"
                    @click="openAddSection">
              <span class="material-symbols-outlined text-sm">add</span>
              Add Section
            </button>
          </div>

          <div class="px-8 py-6 space-y-5">
            <!-- Branch filter + search row -->
            <div class="flex items-center gap-3">
              <!-- Branch filter -->
              <div class="flex gap-1.5 flex-wrap">
                <button class="px-3 py-1.5 rounded-lg text-[10px] font-bold uppercase tracking-widest transition-all"
                        :style="
                    sectionBranchFilter === 'ALL'
                      ? 'background-color: var(--color-primary); color: #ffffff;'
                      : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
                  "
                        @click="sectionBranchFilter = 'ALL'">
                  All
                </button>
                <button v-for="b in availableBranches"
                        :key="b.code"
                        class="px-3 py-1.5 rounded-lg text-[10px] font-bold uppercase tracking-widest transition-all"
                        :style="
                    sectionBranchFilter === b.code
                      ? 'background-color: var(--color-primary); color: #ffffff;'
                      : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
                  "
                        @click="sectionBranchFilter = b.code">
                  {{ b.code }}
                </button>
              </div>

              <!-- Search -->
              <div class="relative flex-1 min-w-0">
                <span class="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-sm"
                      style="color: var(--color-text-muted)">search</span>
                <input v-model="sectionSearch"
                       class="w-full pl-9 pr-4 py-2 rounded-xl text-sm font-medium outline-none transition-all"
                       style="
                    background-color: var(--color-surface-low);
                    color: var(--color-text);
                    border: 1.5px solid var(--color-border);
                  "
                       placeholder="Search by code or name..."
                       @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                       @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
              </div>
            </div>

            <!-- Loading -->
            <div v-if="sectionsLoading"
                 class="flex items-center justify-center py-12 gap-3"
                 style="color: var(--color-text-muted)">
              <span class="material-symbols-outlined text-xl animate-spin">progress_activity</span>
              <span class="text-sm font-medium">Loading sections...</span>
            </div>

            <!-- Grouped by category -->
            <template v-else>
              <div v-for="cat in sectionCategories" :key="cat.id" class="space-y-2">
                <!-- Category header -->
                <div class="flex items-center gap-2 pt-2">
                  <div class="w-6 h-6 rounded-lg flex items-center justify-center"
                       :style="`background-color: ${cat.softColor};`">
                    <span class="material-symbols-outlined text-xs" :style="`color: ${cat.color};`">
                      {{ cat.icon }}
                    </span>
                  </div>
                  <span class="text-[10px] font-bold uppercase tracking-widest"
                        :style="`color: ${cat.color};`">
                    {{ cat.label }}
                  </span>
                  <span class="text-[10px] font-bold" style="color: var(--color-text-muted)">
                    ({{ filteredSectionsByCategory(cat.id).length }})
                  </span>
                </div>

                <!-- Empty state per category -->
                <div v-if="!filteredSectionsByCategory(cat.id).length"
                     class="px-4 py-3 rounded-xl text-xs text-center"
                     style="background-color: var(--color-surface-low); color: var(--color-text-muted)">
                  No {{ cat.label.toLowerCase() }} sections found.
                </div>

                <!-- Sections table -->
                <div v-else
                     class="rounded-xl overflow-hidden"
                     style="border: 1px solid var(--color-border)">
                  <table class="w-full text-sm">
                    <thead>
                      <tr style="border-bottom: 1px solid var(--color-border)">
                        <th class="text-left px-4 py-2.5 text-[10px] font-bold uppercase tracking-widest"
                            style="
                            color: var(--color-text-muted);
                            background-color: var(--color-surface-low);
                          ">
                          Code
                        </th>
                        <th class="text-left px-4 py-2.5 text-[10px] font-bold uppercase tracking-widest"
                            style="
                            color: var(--color-text-muted);
                            background-color: var(--color-surface-low);
                          ">
                          Name
                        </th>
                        <th class="text-left px-4 py-2.5 text-[10px] font-bold uppercase tracking-widest"
                            style="
                            color: var(--color-text-muted);
                            background-color: var(--color-surface-low);
                          ">
                          Branch
                        </th>
                        <th class="text-left px-4 py-2.5 text-[10px] font-bold uppercase tracking-widest"
                            style="
                            color: var(--color-text-muted);
                            background-color: var(--color-surface-low);
                          ">
                          Auto No.
                        </th>
                        <th class="text-left px-4 py-2.5 text-[10px] font-bold uppercase tracking-widest"
                            style="
                            color: var(--color-text-muted);
                            background-color: var(--color-surface-low);
                          ">
                          Status
                        </th>
                        <th style="background-color: var(--color-surface-low)"></th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="sec in filteredSectionsByCategory(cat.id)"
                          :key="sec.code"
                          style="border-bottom: 1px solid var(--color-surface-low)">
                        <!-- Code -->
                        <td class="px-4 py-3">
                          <span class="font-mono font-bold text-xs"
                                style="color: var(--color-text)">
                            {{ sec.code }}
                          </span>
                        </td>

                        <!-- Name -->
                        <td class="px-4 py-3">
                          <span class="font-medium" style="color: var(--color-text)">
                            {{
                            sec.name
                            }}
                          </span>
                        </td>

                        <!-- Branch -->
                        <td class="px-4 py-3">
                          <span class="px-2 py-0.5 rounded-lg text-[10px] font-bold"
                                style="
                              background-color: var(--color-surface-low);
                              color: var(--color-text-muted);
                            ">
                            {{ sec.branchCode }}
                          </span>
                        </td>

                        <!-- AutoNo -->
                        <td class="px-4 py-3">
                          <span class="text-sm font-mono" style="color: var(--color-text-muted)">
                            {{ sec.autoNo || "—" }}
                          </span>
                        </td>

                        <!-- Status toggle -->
                        <td class="px-4 py-3">
                          <button class="flex items-center gap-1.5 px-3 py-1 rounded-lg text-[10px] font-bold uppercase tracking-widest transition-all"
                                  :style="
                              sec.active
                                ? 'background-color: var(--color-primary-soft); color: var(--color-primary);'
                                : 'background-color: var(--color-surface-low); color: var(--color-text-muted); border: 1px solid var(--color-border);'
                            "
                                  @click="toggleSection(sec)">
                            <span class="material-symbols-outlined text-xs">
                              {{ sec.active ? "check_circle" : "cancel" }}
                            </span>
                            {{ sec.active ? "Active" : "Inactive" }}
                          </button>
                        </td>

                        <!-- Edit -->
                        <td class="px-4 py-3">
                          <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                                  style="color: var(--color-text-muted)"
                                  title="Edit section"
                                  @mouseenter="
                                  (e)=>
                            (e.currentTarget.style.backgroundColor = 'var(--color-surface-low)')
                            "
                            @mouseleave="
                            (e) => (e.currentTarget.style.backgroundColor = 'transparent')
                            "
                            @click="openEditSection(sec)"
                            >
                            <span class="material-symbols-outlined text-sm">edit</span>
                          </button>
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>

              <!-- Empty overall -->
              <div v-if="!filteredSections.length && !sectionsLoading"
                   class="py-10 flex flex-col items-center gap-2"
                   style="color: var(--color-text-muted)">
                <span class="material-symbols-outlined text-3xl">apartment</span>
                <p class="text-sm font-medium">No sections found.</p>
              </div>
            </template>
          </div>
        </div>

        <!-- ══════════════════════════════════════════════════════════
       ADD / EDIT SECTION MODAL
  ══════════════════════════════════════════════════════════ -->
        <Teleport to="body">
          <Transition name="modal">
            <div v-if="sectionModal.visible"
                 class="fixed inset-0 z-50 flex items-center justify-center"
                 style="background-color: rgba(0, 0, 0, 0.4)"
                 @click.self="closeSectionModal">
              <div class="w-full max-w-md rounded-2xl overflow-hidden flex flex-col"
                   style="
                  background-color: var(--color-surface);
                  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.24);
                  max-height: 90vh;
                ">
                <!-- Header -->
                <div class="px-6 py-4 flex items-center justify-between flex-shrink-0"
                     style="border-bottom: 1px solid var(--color-border)">
                  <div class="flex items-center gap-3">
                    <div class="w-8 h-8 rounded-lg flex items-center justify-center"
                         style="background-color: var(--color-primary-soft)">
                      <span class="material-symbols-outlined text-sm"
                            style="color: var(--color-primary)">apartment</span>
                    </div>
                    <h3 class="text-sm font-extrabold tracking-tight"
                        style="color: var(--color-text)">
                      {{ sectionModal.mode === "add" ? "Add New Section" : "Edit Section" }}
                    </h3>
                  </div>
                  <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                          style="color: var(--color-text-muted)"
                          @mouseenter="
                          (e)=>
                    (e.currentTarget.style.backgroundColor = 'var(--color-surface-low)')
                    "
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click="closeSectionModal"
                    >
                    <span class="material-symbols-outlined text-base">close</span>
                  </button>
                </div>

                <!-- Body -->
                <div class="px-6 py-5 space-y-4 overflow-y-auto flex-1">
                  <!-- Section Code (add only) -->
                  <div v-if="sectionModal.mode === 'add'">
                    <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5"
                           style="color: var(--color-text-muted)">Section Code *</label>
                    <div class="relative">
                      <input v-model="sectionModal.form.code"
                             placeholder="e.g. HEM"
                             maxlength="20"
                             class="w-full px-4 py-2.5 pr-10 rounded-xl text-sm font-mono font-bold uppercase outline-none transition-all"
                             :style="`background-color: var(--color-surface-low); color: var(--color-text);
                       border: 1.5px solid ${
                         sectionModal.codeStatus === 'taken'
                           ? '#ba1a1a'
                           : sectionModal.codeStatus === 'available'
                             ? '#4caf50'
                             : 'var(--color-border)'
                       };`"
                             @input="onCodeInput"
                             @focus="
                             (e)=> {
                      if (sectionModal.codeStatus === 'idle')
                      e.target.style.borderColor = 'var(--color-primary)';
                      }
                      "
                      @blur="
                      (e) => {
                      if (sectionModal.codeStatus === 'idle')
                      e.target.style.borderColor = 'var(--color-border)';
                      }
                      "
                      />
                      <div class="absolute right-3 top-1/2 -translate-y-1/2">
                        <span v-if="sectionModal.codeChecking"
                              class="material-symbols-outlined text-sm animate-spin"
                              style="color: var(--color-text-muted)">progress_activity</span>
                        <span v-else-if="sectionModal.codeStatus === 'available'"
                              class="material-symbols-outlined text-sm"
                              style="color: #4caf50">check_circle</span>
                        <span v-else-if="sectionModal.codeStatus === 'taken'"
                              class="material-symbols-outlined text-sm"
                              style="color: #ba1a1a">cancel</span>
                      </div>
                    </div>
                    <p v-if="sectionModal.codeStatus === 'taken'"
                       class="mt-1 text-xs font-bold"
                       style="color: #ba1a1a">
                      This section code already exists.
                    </p>
                    <p v-else-if="sectionModal.codeStatus === 'available'"
                       class="mt-1 text-xs font-bold"
                       style="color: #4caf50">
                      Code is available.
                    </p>
                  </div>

                  <!-- Section Code read-only (edit mode) -->
                  <div v-else>
                    <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5"
                           style="color: var(--color-text-muted)">Section Code</label>
                    <div class="px-4 py-2.5 rounded-xl text-sm font-mono font-bold"
                         style="
                        background-color: var(--color-surface-low);
                        color: var(--color-text-muted);
                      ">
                      {{ sectionModal.form.code }}
                    </div>
                  </div>

                  <!-- Section Name -->
                  <div>
                    <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5"
                           style="color: var(--color-text-muted)">Section Name *</label>
                    <input v-model="sectionModal.form.name"
                           placeholder="e.g. Hematology"
                           class="w-full px-4 py-2.5 rounded-xl text-sm font-medium outline-none transition-all"
                           style="
                        background-color: var(--color-surface-low);
                        color: var(--color-text);
                        border: 1.5px solid var(--color-border);
                      "
                           @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                           @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
                  </div>

                  <!-- Branch (add only) -->
                  <div v-if="sectionModal.mode === 'add'">
                    <label class="block text-[10px] font-bold uppercase tracking-widest mb-2"
                           style="color: var(--color-text-muted)">Branch *</label>
                    <div class="rounded-xl overflow-hidden"
                         style="border: 1.5px solid var(--color-border)">
                      <div v-for="b in availableBranches"
                           :key="b.code"
                           class="flex items-center justify-between px-4 py-2.5 cursor-pointer transition-all"
                           style="border-bottom: 1px solid var(--color-surface-low)"
                           :style="
                          sectionModal.form.branchCode === b.code
                            ? 'background-color: var(--color-primary-soft);'
                            : ''
                        "
                           @click="sectionModal.form.branchCode = b.code">
                        <span class="text-sm font-medium" style="color: var(--color-text)">
                          {{
                          b.name || b.code
                          }}
                        </span>
                        <div class="w-5 h-5 rounded-full flex items-center justify-center"
                             :style="
                            sectionModal.form.branchCode === b.code
                              ? 'background-color: var(--color-primary);'
                              : 'border: 1.5px solid var(--color-border);'
                          ">
                          <span v-if="sectionModal.form.branchCode === b.code"
                                class="material-symbols-outlined text-xs"
                                style="color: #ffffff">check</span>
                        </div>
                      </div>
                    </div>
                  </div>

                  <!-- Category (add only) -->
                  <div v-if="sectionModal.mode === 'add'">
                    <label class="block text-[10px] font-bold uppercase tracking-widest mb-2"
                           style="color: var(--color-text-muted)">Category *</label>
                    <div class="grid grid-cols-3 gap-2">
                      <button v-for="cat in sectionCategories"
                              :key="cat.id"
                              class="flex flex-col items-center gap-1.5 py-3 px-2 rounded-xl text-xs font-bold transition-all"
                              :style="
                          sectionModal.form.category === cat.id
                            ? `background-color: ${cat.softColor}; color: ${cat.color}; border: 2px solid ${cat.color};`
                            : 'background-color: var(--color-surface-low); color: var(--color-text-muted); border: 2px solid transparent;'
                        "
                              @click="sectionModal.form.category = cat.id">
                        <span class="material-symbols-outlined text-base">{{ cat.icon }}</span>
                        {{ cat.label }}
                      </button>
                    </div>

                    <!-- Receiver warning -->
                    <div v-if="
                        sectionModal.form.category === '2' &&
                        sectionModal.form.branchCode &&
                        receiverExistsForBranch(sectionModal.form.branchCode)
                      "
                         class="mt-2 flex items-start gap-2 px-3 py-2.5 rounded-xl"
                         style="
                        background-color: rgba(186, 26, 26, 0.08);
                        border: 1px solid rgba(186, 26, 26, 0.2);
                      ">
                      <span class="material-symbols-outlined text-sm flex-shrink-0"
                            style="color: #ba1a1a">warning</span>
                      <p class="text-xs font-bold" style="color: #ba1a1a">
                        A Receiver already exists for this branch. Only one Receiver is allowed per
                        branch.
                      </p>
                    </div>
                  </div>

                  <!-- ── Auto No. — Endorser only ─────────────────────────────────── -->
                  <div v-if="sectionModal.form.category === '1'">
                    <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5"
                           style="color: var(--color-text-muted)">Auto Number</label>
                    <input v-model.number="sectionModal.form.autoNo"
                           type="number"
                           min="0"
                           placeholder="0"
                           class="w-full px-4 py-2.5 rounded-xl text-sm font-mono font-medium outline-none transition-all"
                           style="
                        background-color: var(--color-surface-low);
                        color: var(--color-text);
                        border: 1.5px solid var(--color-border);
                      "
                           @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                           @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')"
                           disabled />
                    <p class="mt-1 text-xs" style="color: var(--color-text-muted)">
                      Used for batch number auto-incrementing.
                    </p>
                  </div>

                  <!-- ── Test Groups — Laboratory only ────────────────────────────── -->
                  <div v-if="sectionModal.form.category === '3'">
                    <div class="flex items-center justify-between mb-2">
                      <label class="text-[10px] font-bold uppercase tracking-widest"
                             style="color: var(--color-text-muted)">Test Groups *</label>
                      <span class="text-[10px] font-bold" style="color: var(--color-text-muted)">
                        {{ sectionModal.form.testGroupCodes.length }} selected
                      </span>
                    </div>

                    <!-- Validation hint -->
                    <p v-if="sectionModal.testGroupError"
                       class="mb-2 text-xs font-bold"
                       style="color: #ba1a1a">
                      At least one test group must be assigned.
                    </p>

                    <!-- Loading -->
                    <div v-if="testGroupsLoading"
                         class="flex items-center gap-2 px-4 py-3 rounded-xl"
                         style="
                        background-color: var(--color-surface-low);
                        color: var(--color-text-muted);
                      ">
                      <span class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
                      <span class="text-xs">Loading test groups...</span>
                    </div>

                    <div v-else
                         class="rounded-xl overflow-hidden"
                         style="border: 1.5px solid var(--color-border)">
                      <div v-for="tg in allTestGroups" :key="tg.code">
                        <!-- Disabled: already assigned to a different section -->
                        <div v-if="
                            tg.assignedSectionCode &&
                            tg.assignedSectionCode !== sectionModal.originalCode
                          "
                             class="flex items-center gap-3 px-4 py-2.5 cursor-not-allowed"
                             style="border-bottom: 1px solid var(--color-surface-low); opacity: 0.45">
                          <div class="w-5 h-5 rounded-md flex-shrink-0"
                               style="border: 1.5px solid var(--color-border)"></div>
                          <div class="flex-1 min-w-0">
                            <span class="text-sm font-medium" style="color: var(--color-text)">
                              {{
                              tg.name
                              }}
                            </span>
                            <span class="ml-2 text-[10px] font-bold"
                                  style="color: var(--color-text-muted)">
                              — assigned to {{ tg.assignedSectionCode }}
                            </span>
                          </div>
                          <span class="material-symbols-outlined text-xs"
                                style="color: var(--color-text-muted)">lock</span>
                        </div>

                        <!-- Selectable -->
                        <div v-else
                             class="flex items-center gap-3 px-4 py-2.5 cursor-pointer transition-all"
                             style="border-bottom: 1px solid var(--color-surface-low)"
                             :style="
                            sectionModal.form.testGroupCodes.includes(tg.code)
                              ? 'background-color: var(--color-primary-soft);'
                              : ''
                          "
                             @click="toggleTestGroup(tg.code)">
                          <div class="w-5 h-5 rounded-md flex items-center justify-center transition-all flex-shrink-0"
                               :style="
                              sectionModal.form.testGroupCodes.includes(tg.code)
                                ? 'background-color: var(--color-primary);'
                                : 'border: 1.5px solid var(--color-border);'
                            ">
                            <span v-if="sectionModal.form.testGroupCodes.includes(tg.code)"
                                  class="material-symbols-outlined text-xs"
                                  style="color: #ffffff">check</span>
                          </div>
                          <span class="text-sm font-medium flex-1"
                                style="color: var(--color-text)">{{ tg.name }}</span>
                          <span class="font-mono text-[10px] font-bold"
                                style="color: var(--color-text-muted)">
                            {{ tg.code }}
                          </span>
                        </div>
                      </div>

                      <p v-if="!allTestGroups.length && !testGroupsLoading"
                         class="px-4 py-3 text-xs text-center"
                         style="color: var(--color-text-muted)">
                        No test groups available.
                      </p>
                    </div>
                  </div>

                  <!-- Error -->
                  <p v-if="sectionModal.error" class="text-xs font-bold" style="color: #ba1a1a">
                    {{ sectionModal.error }}
                  </p>
                </div>

                <!-- Footer -->
                <div class="px-6 py-4 flex justify-end gap-3 flex-shrink-0"
                     style="border-top: 1px solid var(--color-border)">
                  <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                          style="
                      background-color: var(--color-surface-low);
                      color: var(--color-text-muted);
                    "
                          @click="closeSectionModal">
                    Cancel
                  </button>
                  <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow"
                          style="background: var(--color-primary-gradient); color: #ffffff"
                          :disabled="
                      sectionModal.saving ||
                      sectionModal.codeStatus === 'taken' ||
                      sectionModal.codeChecking
                    "
                          @click="saveSectionModal">
                    <span v-if="sectionModal.saving"
                          class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
                    <span class="material-symbols-outlined text-sm" v-else>save</span>
                    {{ sectionModal.mode === "add" ? "Create Section" : "Save Changes" }}
                  </button>
                </div>
              </div>
            </div>
          </Transition>
        </Teleport>

        <!-- ══════════════════════════════════════════════════════════
       RUNNING DAYS TAB
  ══════════════════════════════════════════════════════════ -->

        <div v-if="activeTab === 'runningDays'"
             class="rounded-2xl overflow-hidden"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)">
          <!-- Header -->
          <div class="px-8 py-5 flex items-center justify-between"
               style="border-bottom: 1px solid var(--color-border)">
            <div class="flex items-center gap-4">
              <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
                   style="background-color: var(--color-primary-soft)">
                <span class="material-symbols-outlined text-base"
                      style="color: var(--color-primary)">calendar_month</span>
              </div>
              <div>
                <h2 class="text-base font-extrabold tracking-tight"
                    style="color: var(--color-text)">
                  Running Days
                </h2>
                <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">
                  Configure which days of the week each SRD-tagged test is scheduled to run.
                </p>
              </div>
            </div>
            <button class="flex items-center gap-2 px-4 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 shadow"
                    style="background: var(--color-primary-gradient); color: #ffffff"
                    @click="openAddRunningDay">
              <span class="material-symbols-outlined text-sm">add</span>
              Add Setup
            </button>
          </div>

          <div class="px-8 py-6 space-y-4">
            <!-- Search -->
            <div class="relative">
              <span class="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-sm"
                    style="color: var(--color-text-muted)">search</span>
              <input v-model="rdSearch"
                     placeholder="Search by test code or name..."
                     class="w-full pl-10 pr-4 py-2.5 rounded-xl text-sm font-medium outline-none transition-all"
                     style="
                  background-color: var(--color-surface-low);
                  color: var(--color-text);
                  border: 1.5px solid var(--color-border);
                "
                     @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                     @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
            </div>

            <AppTable :rows="filteredRd"
                      :columns="rdColumns"
                      row-key="id"
                      :page-size="10"
                      :loading="rdLoading"
                      loading-text="Loading setups..."
                      empty-text="No running day setups yet."
                      empty-icon="calendar_month">
              <!-- Test -->
              <template #cell-testName="{ row }">
                <div>
                  <p class="font-bold text-sm" style="color: var(--color-text)">
                    {{ row.testName }}
                  </p>
                  <p class="font-mono text-[10px] mt-0.5" style="color: var(--color-text-muted)">
                    {{ row.testCode }}
                  </p>
                </div>
              </template>

              <!-- Day chips -->
              <template #cell-runningDays="{ row }">
                <div class="flex flex-wrap gap-1.5">
                  <span v-for="day in allDays"
                        :key="day.value"
                        class="px-2.5 py-0.5 rounded-lg text-[10px] font-bold transition-all"
                        :style="
                      (row.dayList ?? []).includes(day.value)
                        ? `background-color: ${day.softColor}; color: ${day.color};`
                        : 'background-color: var(--color-surface-low); color: var(--color-text-muted); opacity: 0.4;'
                    ">
                    {{ day.short }}
                  </span>
                </div>
              </template>

              <!-- Actions -->
              <template #actions="{ row }">
                <div class="flex items-center gap-1 justify-end">
                  <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                          style="color: var(--color-text-muted)"
                          @mouseenter="
                          (e)=>
                    (e.currentTarget.style.backgroundColor = 'var(--color-surface-low)')
                    "
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click="openEditRunningDay(row)"
                    >
                    <span class="material-symbols-outlined text-sm">edit</span>
                  </button>
                  <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                          style="color: #ba1a1a"
                          @mouseenter="
                          (e)=>
                    (e.currentTarget.style.backgroundColor = 'rgba(186,26,26,0.08)')
                    "
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click="confirmDeleteRd(row)"
                    >
                    <span class="material-symbols-outlined text-sm">delete</span>
                  </button>
                </div>
              </template>
            </AppTable>
          </div>
        </div>

        <!-- ══════════════════════════════════════════════════════════
       ADD / EDIT RUNNING DAY MODAL
  ══════════════════════════════════════════════════════════ -->
        <Teleport to="body">
          <Transition name="modal">
            <div v-if="rdModal.visible"
                 class="fixed inset-0 z-50 flex items-center justify-center"
                 style="background-color: rgba(0, 0, 0, 0.4)"
                 @click.self="closeRdModal">
              <div class="w-full max-w-md rounded-2xl overflow-hidden"
                   style="
                  background-color: var(--color-surface);
                  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.24);
                ">
                <!-- Header -->
                <div class="px-6 py-4 flex items-center justify-between"
                     style="border-bottom: 1px solid var(--color-border)">
                  <div class="flex items-center gap-3">
                    <div class="w-8 h-8 rounded-lg flex items-center justify-center"
                         style="background-color: var(--color-primary-soft)">
                      <span class="material-symbols-outlined text-sm"
                            style="color: var(--color-primary)">calendar_month</span>
                    </div>
                    <h3 class="text-sm font-extrabold tracking-tight"
                        style="color: var(--color-text)">
                      {{ rdModal.mode === "add" ? "Add Running Day Setup" : "Edit Running Days" }}
                    </h3>
                  </div>
                  <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                          style="color: var(--color-text-muted)"
                          @mouseenter="
                          (e)=>
                    (e.currentTarget.style.backgroundColor = 'var(--color-surface-low)')
                    "
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click="closeRdModal"
                    >
                    <span class="material-symbols-outlined text-base">close</span>
                  </button>
                </div>

                <!-- Body -->
                <div class="px-6 py-5 space-y-5">
                  <!-- Test search (add only) -->
                  <div v-if="rdModal.mode === 'add'">
                    <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5"
                           style="color: var(--color-text-muted)">Test *</label>

                    <div class="relative">
                      <input v-model="rdModal.testSearch"
                             ref="rdTestInputRef"
                             placeholder="Type 3+ characters to search HCLAB..."
                             class="w-full px-4 py-2.5 pr-10 rounded-xl text-sm font-medium outline-none transition-all"
                             style="
                          background-color: var(--color-surface-low);
                          color: var(--color-text);
                          border: 1.5px solid var(--color-border);
                        "
                             autocomplete="off"
                             @input="onRdTestInput"
                             @keydown.down.prevent="rdSelectNext"
                             @keydown.up.prevent="rdSelectPrev"
                             @keydown.enter.prevent="rdConfirmSelected"
                             @keydown.escape="rdModal.dropdownOpen = false"
                             @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                             @blur="onRdTestBlur" />
                      <div class="absolute right-3 top-1/2 -translate-y-1/2">
                        <span v-if="rdModal.testLoading"
                              class="material-symbols-outlined text-sm animate-spin"
                              style="color: var(--color-text-muted)">progress_activity</span>
                        <span v-else-if="rdModal.form.testCode"
                              class="material-symbols-outlined text-sm"
                              style="color: var(--color-primary)">check_circle</span>
                        <span v-else
                              class="material-symbols-outlined text-sm"
                              style="color: var(--color-text-muted)">biotech</span>
                      </div>
                    </div>

                    <!-- Selected test badge -->
                    <div v-if="rdModal.form.testCode"
                         class="mt-1.5 flex items-center gap-2 px-3 py-2 rounded-lg"
                         style="background-color: var(--color-primary-soft)">
                      <span class="material-symbols-outlined text-sm"
                            style="color: var(--color-primary)">biotech</span>
                      <div class="flex-1 min-w-0">
                        <p class="text-xs font-bold truncate" style="color: var(--color-primary)">
                          {{ rdModal.form.testCode }}
                        </p>
                        <p class="text-[10px] truncate" style="color: var(--color-text-muted)">
                          {{ rdModal.form.testName }}
                        </p>
                      </div>
                      <button class="material-symbols-outlined text-sm"
                              style="color: var(--color-text-muted)"
                              @click="clearRdTest">
                        close
                      </button>
                    </div>

                    <p v-else-if="rdModal.testSearch.length < 3 && !rdModal.form.testCode"
                       class="mt-1 text-[10px]"
                       style="color: var(--color-text-muted)">
                      Search by test code or name (min. 3 characters).
                    </p>

                    <!-- Dropdown -->
                    <Teleport to="body">
                      <div v-if="
                          rdModal.dropdownOpen &&
                          (rdModal.testResults.length || rdModal.testNoResults)
                        "
                           class="fixed z-[60] rounded-xl overflow-hidden shadow-xl"
                           :style="`top: ${rdDropdownPos.top}px; left: ${rdDropdownPos.left}px; width: ${rdDropdownPos.width}px;
                            background-color: var(--color-surface); border: 1.5px solid var(--color-border);`">
                        <div v-if="rdModal.testNoResults"
                             class="px-4 py-3 text-xs text-center"
                             style="color: var(--color-text-muted)">
                          No tests found for "{{ rdModal.testSearch }}".
                        </div>
                        <div v-else class="overflow-y-auto" style="max-height: 200px">
                          <div v-for="(t, idx) in rdModal.testResults"
                               :key="t.testCode"
                               class="flex items-center gap-3 px-4 py-2.5 cursor-pointer transition-all"
                               :style="
                              idx === rdModal.activeIdx
                                ? 'background-color: var(--color-primary-soft);'
                                : 'border-bottom: 1px solid var(--color-surface-low);'
                            "
                               @mouseenter="rdModal.activeIdx = idx"
                               @mousedown.prevent="selectRdTest(t)">
                            <div class="w-7 h-7 rounded-lg flex items-center justify-center flex-shrink-0"
                                 style="background-color: var(--color-surface-low)">
                              <span class="material-symbols-outlined text-sm"
                                    style="color: var(--color-text-muted)">biotech</span>
                            </div>
                            <div class="flex-1 min-w-0">
                              <div class="flex items-center gap-2">
                                <p class="text-xs font-bold truncate"
                                   style="color: var(--color-text)">
                                  {{ t.testCode }}
                                </p>
                                <!-- Already configured badge -->
                                <span v-if="t.hasSetup"
                                      class="px-1.5 py-0.5 rounded text-[9px] font-bold uppercase"
                                      style="background-color: rgba(186, 26, 26, 0.1); color: #ba1a1a">
                                  Already set
                                </span>
                              </div>
                              <p class="text-[10px] truncate"
                                 style="color: var(--color-text-muted)">
                                {{ t.testName }}
                              </p>
                            </div>
                            <span class="text-[10px] font-mono font-bold flex-shrink-0"
                                  style="color: var(--color-text-muted)">{{ t.testGroup }}</span>
                          </div>
                        </div>
                      </div>
                    </Teleport>
                  </div>

                  <!-- Test name read-only (edit mode) -->
                  <div v-else>
                    <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5"
                           style="color: var(--color-text-muted)">Test</label>
                    <div class="flex items-center gap-3 px-4 py-2.5 rounded-xl"
                         style="background-color: var(--color-surface-low)">
                      <span class="material-symbols-outlined text-sm"
                            style="color: var(--color-primary)">biotech</span>
                      <div>
                        <p class="text-sm font-bold" style="color: var(--color-text)">
                          {{ rdModal.form.testName }}
                        </p>
                        <p class="text-[10px] font-mono" style="color: var(--color-text-muted)">
                          {{ rdModal.form.testCode }}
                        </p>
                      </div>
                    </div>
                  </div>

                  <!-- Day picker -->
                  <div>
                    <div class="flex items-center justify-between mb-3">
                      <label class="text-[10px] font-bold uppercase tracking-widest"
                             style="color: var(--color-text-muted)">Running Days *</label>
                      <span class="text-[10px] font-bold" style="color: var(--color-text-muted)">
                        {{ rdModal.form.days.length }} / 7
                      </span>
                    </div>

                    <p v-if="rdModal.dayError"
                       class="mb-2 text-xs font-bold"
                       style="color: #ba1a1a">
                      At least one running day must be selected.
                    </p>

                    <!-- Day grid — week starts Monday -->
                    <div class="grid grid-cols-7 gap-1.5">
                      <button v-for="day in allDays"
                              :key="day.value"
                              class="flex flex-col items-center py-3 rounded-xl text-[10px] font-bold uppercase tracking-widest transition-all"
                              :style="
                          rdModal.form.days.includes(day.value)
                            ? `background-color: ${day.color}; color: #ffffff;`
                            : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
                        "
                              @click="toggleRdDay(day.value)">
                        {{ day.short }}
                      </button>
                    </div>

                    <!-- Preview row -->
                    <div v-if="rdModal.form.days.length"
                         class="mt-3 px-3 py-2 rounded-xl text-xs"
                         style="
                        background-color: var(--color-surface-low);
                        color: var(--color-text-muted);
                      ">
                      <span class="font-bold" style="color: var(--color-text)">Will run on: </span>
                      {{ rdModal.form.days.join(" · ") }}
                    </div>
                  </div>

                  <!-- Error -->
                  <p v-if="rdModal.error" class="text-xs font-bold" style="color: #ba1a1a">
                    {{ rdModal.error }}
                  </p>
                </div>

                <!-- Footer -->
                <div class="px-6 py-4 flex justify-end gap-3"
                     style="border-top: 1px solid var(--color-border)">
                  <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                          style="
                      background-color: var(--color-surface-low);
                      color: var(--color-text-muted);
                    "
                          @click="closeRdModal">
                    Cancel
                  </button>
                  <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow"
                          style="background: var(--color-primary-gradient); color: #ffffff"
                          :disabled="rdModal.saving"
                          @click="saveRdModal">
                    <span v-if="rdModal.saving"
                          class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
                    <span class="material-symbols-outlined text-sm" v-else>save</span>
                    {{ rdModal.mode === "add" ? "Save Setup" : "Update" }}
                  </button>
                </div>
              </div>
            </div>
          </Transition>
        </Teleport>

        <!-- ══════════════════════════════════════════════════════════
   TAT SET-UP
  ══════════════════════════════════════════════════════════ -->
        <div v-if="activeTab === 'tat'"
             class="rounded-2xl overflow-hidden"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)">
          <!-- Header -->
          <div class="px-8 py-5 flex items-center gap-4"
               style="border-bottom: 1px solid var(--color-border)">
            <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
                 style="background-color: var(--color-primary-soft)">
              <span class="material-symbols-outlined text-base" style="color: var(--color-primary)">timer</span>
            </div>
            <div>
              <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text)">
                TAT Set-Up
              </h2>
              <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">
                Configure turnaround time thresholds for endorsing sections and processing.
              </p>
            </div>
          </div>

          <!-- Body -->
          <!-- Body -->
          <div class="px-8 py-6 space-y-8">
            <!-- ── Endorsement TAT ─────────────────────────────────── -->
            <div>
              <div class="flex items-center gap-2 mb-4">
                <span class="material-symbols-outlined text-sm" style="color: var(--color-primary)">arrow_upward</span>
                <p class="text-[10px] font-bold uppercase tracking-widest"
                   style="color: var(--color-text-muted)">
                  Batch Endorsement — Per Endorsing Section
                </p>
              </div>

              <!-- Loading -->
              <div v-if="tatLoading" class="flex items-center justify-center py-10 gap-3">
                <span class="material-symbols-outlined animate-spin text-xl"
                      style="color: var(--color-primary)">progress_activity</span>
                <p class="text-xs font-bold uppercase tracking-widest"
                   style="color: var(--color-text-muted)">
                  Loading...
                </p>
              </div>

              <!-- Empty -->
              <div v-else-if="!tatList.length"
                   class="flex flex-col items-center justify-center py-10 gap-2">
                <span class="material-symbols-outlined text-3xl"
                      style="color: var(--color-text-muted)">timer_off</span>
                <p class="text-xs font-bold uppercase tracking-widest"
                   style="color: var(--color-text-muted)">
                  No active endorsing sections found.
                </p>
              </div>

              <!-- Table -->
              <template v-else>
                <div class="rounded-xl overflow-hidden"
                     style="border: 1px solid var(--color-border)">
                  <div class="grid grid-cols-[1fr_160px_160px_180px] px-5 py-3 text-[10px] font-bold uppercase tracking-widest"
                       style="
                      background-color: var(--color-surface-low);
                      color: var(--color-text-muted);
                      border-bottom: 1px solid var(--color-border);
                    ">
                    <span>Section</span>
                    <span class="text-center">TAT Threshold</span>
                    <span class="text-center">Appeal Window</span>
                    <span></span>
                  </div>
                  <div v-for="(row, idx) in tatList"
                       :key="row.sectionCode"
                       class="grid grid-cols-[1fr_160px_160px_180px] items-center px-5 py-4 gap-4"
                       :style="
                       idx < tatList.length - 1
                        ? 'border-bottom: 1px solid var(--color-border);'
                        : ''
                    "
                  >
                    <div>
                      <p class="text-sm font-bold" style="color: var(--color-text)">
                        {{ row.name }}
                      </p>
                      <p
                        class="text-[10px] mt-0.5 font-mono"
                        style="color: var(--color-text-muted)"
                      >
                        {{ row.sectionCode }}
                      </p>
                    </div>
                    <div class="flex items-center justify-center gap-1.5">
                      <input
                        v-model.number="row.hours"
                        type="number"
                        min="0"
                        max="23"
                        class="w-14 px-2 py-2 rounded-xl text-sm font-bold text-center outline-none"
                        style="
                          background-color: var(--color-surface-low);
                          color: var(--color-text);
                          border: 1.5px solid var(--color-border);
                        "
                      />
                      <span class="text-[10px] font-bold" style="color: var(--color-text-muted)"
                        >hr</span
                      >
                      <input
                        v-model.number="row.minutes"
                        type="number"
                        min="0"
                        max="59"
                        class="w-14 px-2 py-2 rounded-xl text-sm font-bold text-center outline-none"
                        style="
                          background-color: var(--color-surface-low);
                          color: var(--color-text);
                          border: 1.5px solid var(--color-border);
                        "
                      />
                      <span class="text-[10px] font-bold" style="color: var(--color-text-muted)"
                        >min</span
                      >
                    </div>
                    <div class="flex items-center justify-center">
                      <div
                        class="flex rounded-xl overflow-hidden"
                        style="border: 1.5px solid var(--color-border)"
                      >
                        <button
                          class="px-3 py-1.5 text-[10px] font-bold uppercase tracking-widest transition-all"
                          :style="
                            row.appealWindow === 'Before'
                              ? 'background-color: var(--color-primary); color: #ffffff;'
                              : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
                          "
                          @click="row.appealWindow = 'Before'"
                        >
                          Before
                        </button>
                        <button
                          class="px-3 py-1.5 text-[10px] font-bold uppercase tracking-widest transition-all"
                          :style="
                            row.appealWindow === 'After'
                              ? 'background-color: var(--color-primary); color: #ffffff;'
                              : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
                          "
                          @click="row.appealWindow = 'After'"
                        >
                          After
                        </button>
                      </div>
                    </div>
                    <div>
                      <p class="text-[10px]" style="color: var(--color-text-muted)">
                        <template v-if="row.appealWindow === 'Before'">
                          Endorser can appeal
                          <span class="font-bold" style="color: var(--color-text)">anytime</span>
                          while the timer is still running.
                        </template>
                        <template v-else>
                          Endorser can only appeal
                          <span class="font-bold" style="color: var(--color-text)">after</span> the
                          TAT has already been breached.
                        </template>
                      </p>
                    </div>
                  </div>
                </div>
                <div class="flex justify-end pt-4">
                  <button class="px-6 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow"
                          :class="tatSaving ? 'opacity-60 pointer-events-none' : ''"
                          style="background: var(--color-primary-gradient); color: #ffffff"
                          @click="saveTat">
                    <span class="material-symbols-outlined text-sm">
                      {{
                      tatSaving ? "progress_activity" : "save"
                      }}
                    </span>
                    {{ tatSaving ? "Saving..." : "Save Endorsement TAT" }}
                  </button>
                </div>
              </template>
            </div>

            <!-- ── Processing TAT ──────────────────────────────────── -->
            <div style="border-top: 1px solid var(--color-border); padding-top: 2rem">
              <div class="flex items-center gap-2 mb-4">
                <span class="material-symbols-outlined text-sm" style="color: var(--color-primary)">arrow_downward</span>
                <p class="text-[10px] font-bold uppercase tracking-widest"
                   style="color: var(--color-text-muted)">
                  Batch Completion — Processing
                </p>
              </div>
              <p class="text-xs mb-5" style="color: var(--color-text-muted)">
                Time allowed from first specimen received to batch fully completed. Batches
                exceeding this threshold will be flagged and colored
                <span class="font-bold" style="color: #ef4444">red</span> on the monitoring
                dashboard; batches within TAT will be
                <span class="font-bold" style="color: #3b82f6">blue</span>.
              </p>

              <div v-if="procTatLoading" class="flex items-center justify-center py-10 gap-3">
                <span class="material-symbols-outlined animate-spin text-xl"
                      style="color: var(--color-primary)">progress_activity</span>
                <p class="text-xs font-bold uppercase tracking-widest"
                   style="color: var(--color-text-muted)">
                  Loading...
                </p>
              </div>

              <template v-else>
                <div class="flex items-center gap-4 p-5 rounded-xl"
                     style="
                    background-color: var(--color-surface-low);
                    border: 1px solid var(--color-border);
                  ">
                  <span class="material-symbols-outlined text-2xl flex-shrink-0"
                        style="color: var(--color-primary)">hourglass_bottom</span>
                  <div class="flex-1">
                    <p class="text-sm font-bold mb-0.5" style="color: var(--color-text)">
                      Completion Threshold
                    </p>
                    <p class="text-[10px]" style="color: var(--color-text-muted)">
                      Applied branch-wide to all incoming batches.
                    </p>
                  </div>
                  <div class="flex items-center gap-2 flex-shrink-0">
                    <input v-model.number="procTat.hours"
                           type="number"
                           min="0"
                           max="23"
                           class="w-16 px-2 py-2 rounded-xl text-sm font-bold text-center outline-none"
                           style="
                        background-color: var(--color-surface);
                        color: var(--color-text);
                        border: 1.5px solid var(--color-border);
                      "
                           @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                           @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
                    <span class="text-[10px] font-bold" style="color: var(--color-text-muted)">hr</span>
                    <input v-model.number="procTat.minutes"
                           type="number"
                           min="0"
                           max="59"
                           class="w-16 px-2 py-2 rounded-xl text-sm font-bold text-center outline-none"
                           style="
                        background-color: var(--color-surface);
                        color: var(--color-text);
                        border: 1.5px solid var(--color-border);
                      "
                           @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                           @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
                    <span class="text-[10px] font-bold" style="color: var(--color-text-muted)">min</span>
                  </div>
                </div>

                <div class="flex justify-end pt-4">
                  <button class="px-6 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow"
                          :class="procTatSaving ? 'opacity-60 pointer-events-none' : ''"
                          style="background: var(--color-primary-gradient); color: #ffffff"
                          @click="saveProcTat">
                    <span class="material-symbols-outlined text-sm">
                      {{
                      procTatSaving ? "progress_activity" : "save"
                      }}
                    </span>
                    {{ procTatSaving ? "Saving..." : "Save Processing TAT" }}
                  </button>
                </div>
              </template>
            </div>
          </div>
        </div>

        <!-- ══════════════════════════════════════════════════════════
      ON-SITE
    ══════════════════════════════════════════════════════════ -->
        <div v-if="activeTab === 'onsite'"
             class="rounded-2xl overflow-hidden"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)">
          <!-- Header -->
          <div class="px-8 py-5 flex items-center justify-between"
               style="border-bottom: 1px solid var(--color-border)">
            <div class="flex items-center gap-4">
              <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
                   style="background-color: var(--color-primary-soft)">
                <span class="material-symbols-outlined text-base"
                      style="color: var(--color-primary)">location_on</span>
              </div>
              <div>
                <h2 class="text-base font-extrabold tracking-tight"
                    style="color: var(--color-text)">
                  On-Site / Mission
                </h2>
                <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">
                  Manage On-Site specimen scanning and allowed lab number prefixes.
                </p>
              </div>
            </div>
          </div>

          <div class="px-8 py-6 space-y-8">
            <!-- Global Toggle -->
            <div class="flex items-center justify-between p-5 rounded-2xl"
                 style="
                background-color: var(--color-surface-low);
                border: 1.5px solid var(--color-border);
              ">
              <div>
                <p class="text-sm font-bold" style="color: var(--color-text)">
                  Enable On-Site Scanning
                </p>
                <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">
                  When enabled, lab section users can scan On-Site / Mission specimens on the Assign
                  RMT page.
                </p>
              </div>
              <button class="relative inline-flex h-6 w-11 items-center rounded-full transition-colors flex-shrink-0 ml-6"
                      :style="
                  onSiteEnabled
                    ? 'background-color: var(--color-primary);'
                    : 'background-color: var(--color-border);'
                "
                      :disabled="onSiteToggling"
                      @click="toggleOnSite">
                <span class="inline-block h-4 w-4 transform rounded-full bg-white shadow transition-transform"
                      :style="
                    onSiteEnabled ? 'transform: translateX(23px);' : 'transform: translateX(4px);'
                  " />
              </button>
            </div>

            <!-- Allowed Lab No. Prefixes -->
            <div>
              <div class="flex items-center justify-between mb-4">
                <div>
                  <p class="text-sm font-extrabold" style="color: var(--color-text)">
                    Allowed Lab No. Prefixes
                  </p>
                  <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">
                    Characters 3–4 of the lab number must match an active prefix to be allowed for
                    scanning.
                  </p>
                </div>
              </div>

              <!-- Add new prefix form -->
              <div class="flex items-start gap-3 mb-5 p-4 rounded-xl"
                   style="
                  background-color: var(--color-surface-low);
                  border: 1.5px solid var(--color-border);
                ">
                <div class="flex flex-col gap-1">
                  <label class="text-[10px] font-bold uppercase tracking-widest"
                         style="color: var(--color-text-muted)">Prefix (2 digits)</label>
                  <input v-model="onSiteNewPrefix"
                         maxlength="2"
                         placeholder="e.g. 52"
                         class="px-3 py-2 rounded-lg text-sm font-mono font-bold outline-none w-24 transition-all"
                         style="
                      background-color: var(--color-surface);
                      border: 1.5px solid var(--color-border);
                      color: var(--color-text);
                    "
                         @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                         @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')"
                         @keyup.enter="addLabNo" />
                </div>
                <div class="flex flex-col gap-1 flex-1">
                  <label class="text-[10px] font-bold uppercase tracking-widest"
                         style="color: var(--color-text-muted)">Description (optional)</label>
                  <input v-model="onSiteNewDescription"
                         placeholder="e.g. On-Site Mission A"
                         class="px-3 py-2 rounded-lg text-sm outline-none transition-all"
                         style="
                      background-color: var(--color-surface);
                      border: 1.5px solid var(--color-border);
                      color: var(--color-text);
                    "
                         @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                         @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')"
                         @keyup.enter="addLabNo" />
                </div>
                <div class="flex flex-col gap-1 justify-end">
                  <label class="text-[10px] font-bold uppercase tracking-widest opacity-0">Add</label>
                  <button class="flex items-center gap-1.5 px-4 py-2 rounded-lg text-xs font-bold uppercase tracking-widest transition-all active:scale-95"
                          style="background: var(--color-primary-gradient); color: #fff"
                          :disabled="onSiteAdding"
                          @click="addLabNo">
                    <span class="material-symbols-outlined text-sm">add</span>
                    Add
                  </button>
                </div>
              </div>

              <!-- Error -->
              <p v-if="onSiteAddError"
                 class="text-xs mb-3 font-semibold"
                 style="color: var(--color-error)">
                {{ onSiteAddError }}
              </p>

              <!-- Prefix table -->
              <div v-if="onSiteLoading"
                   class="py-10 text-center text-sm"
                   style="color: var(--color-text-muted)">
                Loading...
              </div>
              <div v-else-if="!onSiteLabNos.length"
                   class="py-10 text-center text-sm"
                   style="color: var(--color-text-muted)">
                No prefixes configured yet.
              </div>
              <table v-else class="w-full text-sm">
                <thead>
                  <tr style="border-bottom: 1px solid var(--color-border)">
                    <th class="text-left px-4 py-2.5 text-[11px] font-bold uppercase tracking-widest"
                        style="color: var(--color-text-muted)">
                      Prefix
                    </th>
                    <th class="text-left px-4 py-2.5 text-[11px] font-bold uppercase tracking-widest"
                        style="color: var(--color-text-muted)">
                      Description
                    </th>
                    <th class="text-left px-4 py-2.5 text-[11px] font-bold uppercase tracking-widest"
                        style="color: var(--color-text-muted)">
                      Status
                    </th>
                    <th class="text-left px-4 py-2.5 text-[11px] font-bold uppercase tracking-widest"
                        style="color: var(--color-text-muted)">
                      Added By
                    </th>
                    <th class="px-4 py-2.5"></th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="entry in onSiteLabNos"
                      :key="entry.id"
                      style="border-bottom: 1px solid var(--color-border)">
                    <td class="px-4 py-3">
                      <span class="font-mono font-extrabold text-base"
                            style="color: var(--color-primary)">{{ entry.prefix }}</span>
                    </td>
                    <td class="px-4 py-3" style="color: var(--color-text)">
                      {{ entry.description || "—" }}
                    </td>
                    <td class="px-4 py-3">
                      <span class="px-2.5 py-1 rounded-full text-[10px] font-bold uppercase tracking-widest"
                            :style="
                          entry.active
                            ? 'background-color: rgba(5,150,105,0.1); color: var(--color-success, #059669);'
                            : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
                        ">
                        {{ entry.active ? "Active" : "Inactive" }}
                      </span>
                    </td>
                    <td class="px-4 py-3 text-xs" style="color: var(--color-text-muted)">
                      {{ entry.createdBy }}
                    </td>
                    <td class="px-4 py-3">
                      <div class="flex items-center gap-2 justify-end">
                        <button class="text-xs font-bold px-3 py-1.5 rounded-lg transition-all"
                                :style="
                            entry.active
                              ? 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
                              : 'background-color: rgba(5,150,105,0.1); color: var(--color-success, #059669);'
                          "
                                @click="toggleLabNo(entry)">
                          {{ entry.active ? "Deactivate" : "Activate" }}
                        </button>
                        <button class="text-xs font-bold px-3 py-1.5 rounded-lg transition-all"
                                style="
                            background-color: rgba(239, 68, 68, 0.1);
                            color: var(--color-error, #ef4444);
                          "
                                @click="confirmDeleteLabNo(entry)">
                          Remove
                        </button>
                      </div>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <!-- ══════════════════════════════════════════════════════════
       PROCESSING OPTIONS
  ══════════════════════════════════════════════════════════ -->
        <div v-if="activeTab === 'processing'"
             class="rounded-2xl overflow-hidden"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)">
          <div class="px-8 py-5 flex items-center gap-4"
               style="border-bottom: 1px solid var(--color-border)">
            <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
                 style="background-color: var(--color-primary-soft)">
              <span class="material-symbols-outlined text-base" style="color: var(--color-primary)">tune</span>
            </div>
            <div>
              <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text)">
                Processing Options
              </h2>
              <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">
                Toggle optional fields shown during specimen receiving in Processing.
              </p>
            </div>
          </div>
          <div class="px-8 py-6 space-y-8">

            <!-- Loading -->
            <div v-if="procOptionsLoading" class="flex items-center justify-center py-10 gap-3">
              <span class="material-symbols-outlined animate-spin text-xl" style="color: var(--color-primary);">progress_activity</span>
              <p class="text-xs font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Loading...</p>
            </div>

            <template v-else>
              <div>
                <p class="text-[10px] font-bold uppercase tracking-widest mb-1" style="color: var(--color-text-muted);">
                  Receive by Specimen — Optional Fields
                </p>
                <p class="text-xs mb-5" style="color: var(--color-text-muted);">
                  Toggle which fields are visible during specimen receiving. Disabling a field hides it from the interface but does not delete any existing data.
                </p>

                <div class="rounded-xl overflow-hidden" style="border: 1px solid var(--color-border);">

                  <!-- Temperature -->
                  <div class="flex items-center justify-between px-5 py-4"
                       style="border-bottom: 1px solid var(--color-border);">
                    <div class="flex items-center gap-3">
                      <span class="material-symbols-outlined text-base" style="color: var(--color-primary);">thermostat</span>
                      <div>
                        <p class="text-sm font-bold" style="color: var(--color-text);">Temperature Field</p>
                        <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">Show temperature input during receiving</p>
                      </div>
                    </div>
                    <button class="w-12 h-6 rounded-full transition-all relative flex-shrink-0"
                            :style="procOptions.showTemperature ? 'background-color: var(--color-primary);' : 'background-color: var(--color-border);'"
                            @click="procOptions.showTemperature = !procOptions.showTemperature">
                      <span class="absolute top-0.5 w-5 h-5 rounded-full bg-white shadow transition-all"
                            :style="procOptions.showTemperature ? 'left: calc(100% - 1.375rem);' : 'left: 0.125rem;'"></span>
                    </button>
                  </div>

                  <!-- Temp Remarks -->
                  <div class="flex items-center justify-between px-5 py-4"
                       style="border-bottom: 1px solid var(--color-border);">
                    <div class="flex items-center gap-3">
                      <span class="material-symbols-outlined text-base" style="color: var(--color-primary);">comment</span>
                      <div>
                        <p class="text-sm font-bold" style="color: var(--color-text);">Temperature Remarks</p>
                        <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">
                          Show temperature remarks field. If existing data is present, the field will remain visible regardless of this setting.
                        </p>
                      </div>
                    </div>
                    <button class="w-12 h-6 rounded-full transition-all relative flex-shrink-0"
                            :style="procOptions.showTempRemarks ? 'background-color: var(--color-primary);' : 'background-color: var(--color-border);'"
                            @click="procOptions.showTempRemarks = !procOptions.showTempRemarks">
                      <span class="absolute top-0.5 w-5 h-5 rounded-full bg-white shadow transition-all"
                            :style="procOptions.showTempRemarks ? 'left: calc(100% - 1.375rem);' : 'left: 0.125rem;'"></span>
                    </button>
                  </div>

                  <!-- Bag No -->
                  <div class="flex items-center justify-between px-5 py-4">
                    <div class="flex items-center gap-3">
                      <span class="material-symbols-outlined text-base" style="color: var(--color-primary);">shopping_bag</span>
                      <div>
                        <p class="text-sm font-bold" style="color: var(--color-text);">Bag Number</p>
                        <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">Show bag number field during receiving</p>
                      </div>
                    </div>
                    <button class="w-12 h-6 rounded-full transition-all relative flex-shrink-0"
                            :style="procOptions.showBagNo ? 'background-color: var(--color-primary);' : 'background-color: var(--color-border);'"
                            @click="procOptions.showBagNo = !procOptions.showBagNo">
                      <span class="absolute top-0.5 w-5 h-5 rounded-full bg-white shadow transition-all"
                            :style="procOptions.showBagNo ? 'left: calc(100% - 1.375rem);' : 'left: 0.125rem;'"></span>
                    </button>
                  </div>

                </div>
              </div>

              <div class="flex justify-end pt-4" style="border-top: 1px solid var(--color-border);">
                <button class="px-6 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow-lg"
                        :class="procOptionsSaving ? 'opacity-60 pointer-events-none' : ''"
                        style="background: var(--color-primary-gradient); color: #ffffff;"
                        @click="saveProcessingOptions">
                  <span class="material-symbols-outlined text-sm">{{ procOptionsSaving ? 'progress_activity' : 'save' }}</span>
                  {{ procOptionsSaving ? 'Saving...' : 'Save Changes' }}
                </button>
              </div>
            </template>

          </div>
        </div>

        <!-- ══════════════════════════════════════════════════════════
  BRANCH
    ══════════════════════════════════════════════════════════ -->
        <div v-if="activeTab === 'branch'"
             class="rounded-2xl overflow-hidden"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)">
          <div class="px-8 py-5 flex items-center gap-4"
               style="border-bottom: 1px solid var(--color-border)">
            <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
                 style="background-color: var(--color-primary-soft)">
              <span class="material-symbols-outlined text-base" style="color: var(--color-primary)">location_city</span>
            </div>
            <div>
              <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text)">
                Branch Management
              </h2>
              <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">
                Add and manage branch locations across the network.
              </p>
            </div>
          </div>
          <div class="px-8 py-6 space-y-8">
            <div>
              <p class="text-[10px] font-bold uppercase tracking-widest mb-4"
                 style="color: var(--color-text-muted)">
                Add New Branch
              </p>
              <div class="flex gap-3">
                <div class="flex-1">
                  <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5"
                         style="color: var(--color-text-muted)">Branch Code</label>
                  <input v-model="newBranch.code"
                         class="w-full px-4 py-2.5 rounded-xl text-sm font-medium outline-none transition-all"
                         style="
                      background-color: var(--color-surface-low);
                      color: var(--color-text);
                      border: 1.5px solid var(--color-border);
                    "
                         placeholder="e.g. WES"
                         maxlength="10"
                         @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                         @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
                </div>
                <div class="flex-[2]">
                  <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5"
                         style="color: var(--color-text-muted)">Branch Name</label>
                  <input v-model="newBranch.name"
                         class="w-full px-4 py-2.5 rounded-xl text-sm font-medium outline-none transition-all"
                         style="
                      background-color: var(--color-surface-low);
                      color: var(--color-text);
                      border: 1.5px solid var(--color-border);
                    "
                         placeholder="e.g. West Branch"
                         @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                         @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
                </div>
                <div class="flex items-end">
                  <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2"
                          style="background: var(--color-primary-gradient); color: #ffffff"
                          @click="addBranch">
                    <span class="material-symbols-outlined text-sm">add</span>Add
                  </button>
                </div>
              </div>
            </div>

            <div>
              <p class="text-[10px] font-bold uppercase tracking-widest mb-4"
                 style="color: var(--color-text-muted)">
                Existing Branches
              </p>
              <div class="space-y-2">
                <div v-for="branch in settings.branches"
                     :key="branch.code"
                     class="flex items-center gap-4 px-4 py-3 rounded-xl"
                     style="background-color: var(--color-surface-low)">
                  <div class="w-10 h-10 rounded-xl flex items-center justify-center text-xs font-extrabold flex-shrink-0"
                       style="background-color: var(--color-primary-soft); color: var(--color-primary)">
                    {{ branch.code.substring(0, 3) }}
                  </div>
                  <div class="flex-1">
                    <p class="text-sm font-bold" style="color: var(--color-text)">
                      {{ branch.name }}
                    </p>
                    <p class="text-[10px] font-bold uppercase tracking-widest"
                       style="color: var(--color-text-muted)">
                      {{ branch.code }}
                    </p>
                  </div>
                  <button @click="branch.isActive = !branch.isActive"
                          class="flex items-center gap-2 px-3 py-1.5 rounded-xl text-[10px] font-bold uppercase tracking-widest transition-all"
                          :style="
                      branch.isActive
                        ? 'background-color: var(--color-primary-soft); color: var(--color-primary);'
                        : 'background-color: var(--color-surface); color: var(--color-text-muted); border: 1px solid var(--color-border);'
                    ">
                    <span class="material-symbols-outlined text-xs">
                      {{
                      branch.isActive ? "check_circle" : "cancel"
                      }}
                    </span>
                    {{ branch.isActive ? "Active" : "Inactive" }}
                  </button>
                </div>
                <p v-if="!settings.branches.length"
                   class="py-8 text-center text-sm"
                   style="color: var(--color-text-muted)">
                  No branches configured yet.
                </p>
              </div>
            </div>

            <div class="flex justify-end pt-4" style="border-top: 1px solid var(--color-border)">
              <button class="px-6 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow-lg"
                      style="background: var(--color-primary-gradient); color: #ffffff"
                      @click="save('Branch')">
                <span class="material-symbols-outlined text-sm">save</span>Save Changes
              </button>
            </div>
          </div>
        </div>

        <!-- ══════════════════════════════════════════════════════════
       ENDORSEMENT SET-UP
  ══════════════════════════════════════════════════════════ -->
        <div v-if="activeTab === 'endorsement'"
             class="rounded-2xl overflow-hidden"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)">
          <div class="px-8 py-5 flex items-center gap-4"
               style="border-bottom: 1px solid var(--color-border)">
            <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
                 style="background-color: var(--color-primary-soft)">
              <span class="material-symbols-outlined text-base" style="color: var(--color-primary)">swap_horiz</span>
            </div>
            <div>
              <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text)">
                Endorsement Set-Up
              </h2>
              <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">
                Configure default and additional endorsed-to sites per branch and section.
              </p>
            </div>
          </div>
          <div class="px-8 py-6 space-y-8">
            <div>
              <p class="text-[10px] font-bold uppercase tracking-widest mb-2"
                 style="color: var(--color-text-muted)">
                Default Endorsed-To Site
              </p>
              <p class="text-xs mb-4" style="color: var(--color-text-muted)">
                The default site shown when opening the endorsement form. One per branch/section
                combination.
              </p>
              <div class="space-y-3">
                <div v-for="(config, idx) in settings.endorsement.defaults"
                     :key="idx"
                     class="grid grid-cols-3 gap-3 p-4 rounded-xl"
                     style="background-color: var(--color-surface-low)">
                  <div>
                    <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5"
                           style="color: var(--color-text-muted)">Branch</label>
                    <input v-model="config.branchCode"
                           class="w-full px-3 py-2 rounded-xl text-sm outline-none"
                           style="
                        background-color: var(--color-surface);
                        color: var(--color-text);
                        border: 1.5px solid var(--color-border);
                      "
                           placeholder="Branch Code" />
                  </div>
                  <div>
                    <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5"
                           style="color: var(--color-text-muted)">Section</label>
                    <input v-model="config.sectionCode"
                           class="w-full px-3 py-2 rounded-xl text-sm outline-none"
                           style="
                        background-color: var(--color-surface);
                        color: var(--color-text);
                        border: 1.5px solid var(--color-border);
                      "
                           placeholder="Section Code" />
                  </div>
                  <div>
                    <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5"
                           style="color: var(--color-text-muted)">Default Endorsed-To</label>
                    <input v-model="config.defaultSite"
                           class="w-full px-3 py-2 rounded-xl text-sm outline-none"
                           style="
                        background-color: var(--color-surface);
                        color: var(--color-text);
                        border: 1.5px solid var(--color-border);
                      "
                           placeholder="Site Code" />
                  </div>
                </div>
              </div>
              <button class="mt-3 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2"
                      style="background-color: var(--color-surface-low); color: var(--color-text-muted)"
                      @click="
                  settings.endorsement.defaults.push({
                    branchCode: '',
                    sectionCode: '',
                    defaultSite: '',
                  })
                ">
                <span class="material-symbols-outlined text-sm">add</span>Add Row
              </button>
            </div>

            <div>
              <p class="text-[10px] font-bold uppercase tracking-widest mb-2"
                 style="color: var(--color-text-muted)">
                Additional Endorsed-To Sites
              </p>
              <p class="text-xs mb-4" style="color: var(--color-text-muted)">
                Sites available in the dropdown. All other sites are hidden unless configured here.
              </p>
              <div class="space-y-3">
                <div v-for="(config, idx) in settings.endorsement.additional"
                     :key="idx"
                     class="flex gap-3 items-end p-4 rounded-xl"
                     style="background-color: var(--color-surface-low)">
                  <div class="flex-1">
                    <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5"
                           style="color: var(--color-text-muted)">Branch / Section</label>
                    <input v-model="config.branchSection"
                           class="w-full px-3 py-2 rounded-xl text-sm outline-none"
                           style="
                        background-color: var(--color-surface);
                        color: var(--color-text);
                        border: 1.5px solid var(--color-border);
                      "
                           placeholder="e.g. WES / HEM" />
                  </div>
                  <div class="flex-[2]">
                    <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5"
                           style="color: var(--color-text-muted)">Additional Sites (comma-separated)</label>
                    <input v-model="config.sites"
                           class="w-full px-3 py-2 rounded-xl text-sm outline-none"
                           style="
                        background-color: var(--color-surface);
                        color: var(--color-text);
                        border: 1.5px solid var(--color-border);
                      "
                           placeholder="e.g. MAIN, NAGA, TABUNOK" />
                  </div>
                  <button class="w-9 h-9 rounded-xl flex items-center justify-center transition-all active:scale-95 flex-shrink-0"
                          style="background-color: var(--color-error-soft); color: var(--color-error)"
                          @click="settings.endorsement.additional.splice(idx, 1)">
                    <span class="material-symbols-outlined text-sm">close</span>
                  </button>
                </div>
              </div>
              <button class="mt-3 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2"
                      style="background-color: var(--color-surface-low); color: var(--color-text-muted)"
                      @click="settings.endorsement.additional.push({ branchSection: '', sites: '' })">
                <span class="material-symbols-outlined text-sm">add</span>Add Row
              </button>
            </div>

            <div class="flex justify-end pt-4" style="border-top: 1px solid var(--color-border)">
              <button class="px-6 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow-lg"
                      style="background: var(--color-primary-gradient); color: #ffffff"
                      @click="save('Endorsement')">
                <span class="material-symbols-outlined text-sm">save</span>Save Changes
              </button>
            </div>
          </div>
        </div>

        <!-- ══════════════════════════════════════════════════════════
       ARCHIVE
  ══════════════════════════════════════════════════════════ -->
        <div v-if="activeTab === 'archive'"
             class="rounded-2xl overflow-hidden"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)">
          <div class="px-8 py-5 flex items-center gap-4"
               style="border-bottom: 1px solid var(--color-border)">
            <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
                 style="background-color: var(--color-primary-soft)">
              <span class="material-symbols-outlined text-base" style="color: var(--color-primary)">archive</span>
            </div>
            <div>
              <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text)">
                Archive Settings
              </h2>
              <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">
                Set data retention period. Older batches will be archived and accessible only
                through the archive viewer.
              </p>
            </div>
          </div>
          <div class="px-8 py-6 space-y-6">
            <div>
              <p class="text-[10px] font-bold uppercase tracking-widest mb-4"
                 style="color: var(--color-text-muted)">
                Data Retention
              </p>
              <div class="space-y-4">
                <div class="flex items-center justify-between gap-6">
                  <div>
                    <p class="text-sm font-bold" style="color: var(--color-text)">
                      Active Batch Retention Period
                    </p>
                    <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">
                      Batches older than this will be moved to archive
                    </p>
                  </div>
                  <div class="flex items-center gap-3">
                    <input v-model.number="settings.archive.retentionDays"
                           type="number"
                           min="7"
                           max="365"
                           class="w-24 px-4 py-2.5 rounded-xl text-sm font-bold text-center outline-none transition-all"
                           style="
                        background-color: var(--color-surface-low);
                        color: var(--color-text);
                        border: 1.5px solid var(--color-border);
                      "
                           @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                           @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
                    <span class="text-sm font-bold" style="color: var(--color-text-muted)">days</span>
                  </div>
                </div>
                <div class="flex items-center justify-between gap-6">
                  <div>
                    <p class="text-sm font-bold" style="color: var(--color-text)">
                      Allow Archive Review
                    </p>
                    <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">
                      Users with appropriate rights can browse archived batches
                    </p>
                  </div>
                  <button @click="settings.archive.allowReview = !settings.archive.allowReview"
                          class="w-12 h-6 rounded-full transition-all relative flex-shrink-0"
                          :style="
                      settings.archive.allowReview
                        ? 'background-color: var(--color-primary);'
                        : 'background-color: var(--color-border);'
                    ">
                    <span class="absolute top-0.5 w-5 h-5 rounded-full bg-white shadow transition-all"
                          :style="
                        settings.archive.allowReview
                          ? 'left: calc(100% - 1.375rem);'
                          : 'left: 0.125rem;'
                      "></span>
                  </button>
                </div>
              </div>
            </div>

            <div class="px-5 py-4 rounded-xl flex items-start gap-3"
                 style="background-color: #fffbeb; border: 1px solid #f59e0b">
              <span class="material-symbols-outlined text-base mt-0.5" style="color: #f59e0b">warning</span>
              <p class="text-xs font-medium" style="color: var(--color-text-muted)">
                Archived data is stored and can be retrieved, but will not appear in standard batch
                views or reports. Ensure the retention period aligns with your compliance
                requirements.
              </p>
            </div>

            <div class="flex justify-end pt-4" style="border-top: 1px solid var(--color-border)">
              <button class="px-6 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow-lg"
                      style="background: var(--color-primary-gradient); color: #ffffff"
                      @click="save('Archive')">
                <span class="material-symbols-outlined text-sm">save</span>Save Changes
              </button>
            </div>
          </div>
        </div>


      </div>
    </div>

    <!-- Toast Notification -->
    <Transition name="toast">
      <div
        v-if="toast.visible"
        class="fixed bottom-6 right-6 flex items-center gap-3 px-5 py-4 rounded-2xl shadow-xl z-50"
        style="background-color: var(--color-surface); border: 1px solid var(--color-border)"
      >
        <span class="material-symbols-outlined text-base" style="color: var(--color-primary)"
          >check_circle</span
        >
        <p class="text-sm font-bold" style="color: var(--color-text)">{{ toast.message }}</p>
      </div>
    </Transition>

    <ConfirmModal
      :isVisible="rdDeleteConfirm.visible"
      type="error"
      icon="delete"
      title="Remove Setup"
      :message="`Remove running day setup for '${rdDeleteConfirm.itemName}'? Tests tagged SRD with this code will no longer resolve a running date.`"
      confirmText="Yes, Remove"
      cancelText="No"
      @confirm="executeDeleteRd"
      @close="rdDeleteConfirm.visible = false"
    />
  </AppLayout>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch, nextTick } from "vue";
import { pcApi } from "@/api/pcApi";
import { settingsApi } from "@/api/settingsApi";
import { userApi } from "@/api/userApi";
import { sectionApi } from "@/api/sectionApi";
import { testRunningDayApi } from "@/api/testRunningDayApi";
import { tatApi } from "@/api/tatApi";
import { runnerApi } from "@/api/runnerApi";
import { processingOptionsApi } from '@/api/processingOptionsApi'
import AppLayout from "@/components/layout/AppLayout.vue";
import ConfirmModal from "@/components/common/ConfirmModal.vue";
import AppTable from "@/components/common/AppTable.vue";

// ── Tabs ───────────────────────────────────────────────────────────────────

const activeTab = ref("pc");
const settingsTabs = [
  { key: "pc", label: "PC Registration", icon: "computer" },
  { key: "users", label: "Users", icon: "manage_accounts" },
  { key: "sections", label: "Section", icon: "apartment" },
  { key: "runningDays", label: "Running Days", icon: "calendar_month" },
  { key: "tat", label: "TAT Set-Up", icon: "timer" },
  { key: "onsite", label: "On-Site", icon: "location_on" },
  { key: "processing", label: "Processing", icon: "tune" },
  { key: "branch", label: "Branch", icon: "location_city" },
  { key: "endorsement", label: "Endorsement", icon: "swap_horiz" },
  { key: "archive", label: "Archive", icon: "archive" },
];

const processingFields = [
  {
    key: "showTemperature",
    label: "Temperature Field",
    hint: "Show temperature input during receiving",
  },
  { key: "showTempRemarks", label: "Temperature Remarks", hint: "Show temperature remarks field" },
  { key: "showBagNo", label: "Bag Number", hint: "Show bag number field during receiving" },
];

onMounted(() => {
  loadPCs();
  loadSections();
});

// ── Settings State ─────────────────────────────────────────────────────────

const settings = ref({
  tat: {
    batchEndorsement: { hours: 2, minutes: 0 },
    batchCompletion: { hours: 4, minutes: 0 },
    specimenReceipt: { hours: 1, minutes: 30 },
  },
  branches: [
    { code: "WES", name: "West Branch", isActive: true },
    { code: "NAGA", name: "Naga Branch", isActive: true },
    { code: "TABUNOK", name: "Tabunok Branch", isActive: true },
    { code: "LILOAN", name: "Liloan Branch", isActive: false },
    { code: "DIAMOND", name: "Diamond Branch", isActive: true },
    { code: "MACTAN", name: "Mactan Branch", isActive: true },
    { code: "CONSOLACION", name: "Consolacion Branch", isActive: true },
  ],
  sections: [
    {
      code: "HEM",
      name: "Hematology",
      category: "3",
      cutOff: "18:00",
      isActive: true,
      includeInAssignRMT: true,
      includeInReceiving: true,
    },
    {
      code: "CHE",
      name: "Chemistry",
      category: "3",
      cutOff: "17:30",
      isActive: true,
      includeInAssignRMT: true,
      includeInReceiving: true,
    },
    {
      code: "URI",
      name: "Urinalysis",
      category: "3",
      cutOff: "18:00",
      isActive: true,
      includeInAssignRMT: false,
      includeInReceiving: true,
    },
    {
      code: "MIC",
      name: "Microbiology",
      category: "3",
      cutOff: "16:00",
      isActive: true,
      includeInAssignRMT: true,
      includeInReceiving: true,
    },
    {
      code: "PRO",
      name: "Processing",
      category: "2",
      cutOff: "",
      isActive: true,
      includeInAssignRMT: false,
      includeInReceiving: false,
    },
    {
      code: "END",
      name: "Send-In / Phlebotomy",
      category: "1",
      cutOff: "",
      isActive: true,
      includeInAssignRMT: false,
      includeInReceiving: false,
    },
  ],
  endorsement: {
    defaults: [{ branchCode: "WES", sectionCode: "HEM", defaultSite: "MAIN" }],
    additional: [{ branchSection: "WES / HEM", sites: "NAGA, TABUNOK" }],
  },
  archive: {
    retentionDays: 30,
    allowReview: true,
  },
  processing: {
    showTemperature: true,
    showTempRemarks: true,
    showBagNo: true,
  },
});

// ── Branch ─────────────────────────────────────────────────────────────────

const newBranch = ref({ code: "", name: "" });

function addBranch() {
  if (!newBranch.value.code.trim() || !newBranch.value.name.trim()) return;
  settings.value.branches.push({
    code: newBranch.value.code.trim().toUpperCase(),
    name: newBranch.value.name.trim(),
    isActive: true,
  });
  newBranch.value = { code: "", name: "" };
}

// ── Section ────────────────────────────────────────────────────────────────

const newSection = ref({
  code: "",
  name: "",
  category: "3",
  cutOff: "",
  includeInAssignRMT: false,
  includeInReceiving: false,
});

function addSection() {
  if (!newSection.value.code.trim() || !newSection.value.name.trim()) return;
  settings.value.sections.push({
    ...newSection.value,
    code: newSection.value.code.trim().toUpperCase(),
    name: newSection.value.name.trim(),
    isActive: true,
  });
  newSection.value = {
    code: "",
    name: "",
    category: "3",
    cutOff: "",
    includeInAssignRMT: false,
    includeInReceiving: false,
  };
}

// ── Toast ──────────────────────────────────────────────────────────────────

const toast = ref({ visible: false, message: "" });

function save(section) {
  toast.value = { visible: true, message: `${section} settings saved.` };
  setTimeout(() => {
    toast.value.visible = false;
  }, 2500);
  // TODO: call API endpoint
}

function showToast(msg) {
  toast.value = { visible: true, message: msg };
  setTimeout(() => {
    toast.value.visible = false;
  }, 2500);
  // TODO: call API endpoint
}

// ── PC Registration state ────────────────────────────────────────────  PC
const pcList = ref([]);
const pcLoading = ref(false);
const availableSections = ref([]); // all Section_Master for the branch
const pcSearch = ref("");

const filteredPCs = computed(() => {
  const q = pcSearch.value.toLowerCase();
  if (!q) return pcList.value;
  return pcList.value.filter(
    (pc) =>
      pc.ipAddress.toLowerCase().includes(q) ||
      (pc.description ?? "").toLowerCase().includes(q) ||
      pc.sections.some((s) => s.sectionName.toLowerCase().includes(q)),
  );
});

const pcColumns = [
  { key: "ipAddress", label: "IP Address" },
  { key: "description", label: "Description" },
  { key: "sections", label: "Allowed Sections" },
  { key: "active", label: "Status" },
];

const pcModal = ref({
  visible: false,
  mode: "add", // 'add' | 'editSections'
  pcId: null,
  form: { ipAddress: "", description: "", sectionCodes: [] },
  saving: false,
  error: "",
});

async function loadPCs() {
  pcLoading.value = true;
  try {
    const res = await pcApi.getAll();
    pcList.value = res.data;
  } catch {
    pcList.value = [];
  } finally {
    pcLoading.value = false;
  }
}

async function loadSections() {
  try {
    // Reuse your existing settingsApi or sectionApi — adjust as needed
    const res = await settingsApi.getSections();
    availableSections.value = res.data.filter((s) => s.active);
  } catch {
    availableSections.value = [];
  }
}

// Called when 'pc' tab is activated — load data lazily
watch(activeTab, (val) => {
  if (val === "pc" && !pcList.value.length) {
    loadPCs();
    loadSections();
  }
});

function openAddPC() {
  pcModal.value = {
    visible: true,
    mode: "add",
    pcId: null,
    form: { ipAddress: "", description: "", sectionCodes: [] },
    saving: false,
    error: "",
  };
}

function openEditSections(pc) {
  pcModal.value = {
    visible: true,
    mode: "editSections",
    pcId: pc.id,
    form: {
      ipAddress: pc.ipAddress,
      description: pc.description,
      sectionCodes: pc.sections.map((s) => s.sectionCode),
    },
    saving: false,
    error: "",
  };
}

function closePCModal() {
  pcModal.value.visible = false;
}

function toggleSectionInModal(code) {
  const idx = pcModal.value.form.sectionCodes.indexOf(code);
  if (idx === -1) pcModal.value.form.sectionCodes.push(code);
  else pcModal.value.form.sectionCodes.splice(idx, 1);
}

async function savePCModal() {
  pcModal.value.error = "";
  pcModal.value.saving = true;
  try {
    if (pcModal.value.mode === "add") {
      if (!pcModal.value.form.ipAddress.trim()) {
        pcModal.value.error = "IP Address is required.";
        return;
      }
      await pcApi.add({
        ipAddress: pcModal.value.form.ipAddress.trim(),
        description: pcModal.value.form.description.trim(),
        sectionCodes: pcModal.value.form.sectionCodes,
      });
    } else {
      await pcApi.updateSections(pcModal.value.pcId, {
        sectionCodes: pcModal.value.form.sectionCodes,
      });
    }
    closePCModal();

    await loadPCs();
    showToast(
      pcModal.value.mode === "add" ? "PC registered successfully." : "Section assignments updated.",
    );
  } catch (err) {
    pcModal.value.error = err.response?.data?.message || "An error occurred.";
  } finally {
    pcModal.value.saving = false;
  }
}

async function togglePC(pc) {
  try {
    await pcApi.toggle(pc.id);
    pc.active = !pc.active;
  } catch (err) {
    showToast(err.response?.data?.message || "Failed to update status.");
  }
}

// ── Constants ──────────────────────────────────────────────────────── USER
const roles = [
  { id: 1, label: "Regular" },
  { id: 2, label: "Team Lead" },
  /* { id: 3, label: 'Admin' },*/
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

// ── Users state ──────────────────────────────────────────────────────
const usersList = ref([]);
const usersLoading = ref(false);
const userSearch = ref("");
const userIdInputRef = ref(null);
const hclabDropdownRef = ref(null);
const hclabDropdownPos = ref({ top: 0, left: 0, width: 0 });

const filteredUsers = computed(() => {
  const q = userSearch.value.toLowerCase();
  if (!q) return usersList.value;
  return usersList.value.filter(
    (u) => u.userID.toLowerCase().includes(q) || u.userName.toLowerCase().includes(q),
  );
});

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

// Load on mount (if users is default tab) or on tab switch
watch(activeTab, (val) => {
  if (val === "users" && !usersList.value.length) {
    loadUsers();
    loadSections(); // reuses availableSections from PC tab
  }
});

// ── User modal ───────────────────────────────────────────────────────
const userModal = ref({
  visible: false,
  mode: "add", // 'add' | 'edit'
  userID: null,
  form: {
    userID: "",
    userName: "",
    isAdmin: false,
    sections: [], // [{ sectionCode, roleID }]
  },
  saving: false,
  error: "",
  sectionError: false,
});

function isUserSectionSelected(code) {
  return userModal.value.form.sections.some((s) => s.sectionCode === code);
}

function getUserSectionRole(code) {
  return userModal.value.form.sections.find((s) => s.sectionCode === code)?.roleID ?? 1;
}

function toggleUserSection(code) {
  const idx = userModal.value.form.sections.findIndex((s) => s.sectionCode === code);
  if (idx === -1) {
    userModal.value.form.sections.push({ sectionCode: code, roleID: 1 });
  } else {
    userModal.value.form.sections.splice(idx, 1);
  }
  userModal.value.sectionError = false;
}

function setUserSectionRole(code, roleID) {
  const entry = userModal.value.form.sections.find((s) => s.sectionCode === code);
  if (entry) entry.roleID = roleID;
}

function openAddUser() {
  userModal.value = {
    visible: true,
    mode: "add",
    userID: null,
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
  };
}

function openEditUser(user) {
  userModal.value = {
    visible: true,
    mode: "edit",
    userID: user.userID,
    form: {
      userID: user.userID,
      userName: user.userName,
      isAdmin: user.isAdmin,
      sections: user.sections.map((s) => ({ sectionCode: s.sectionCode, roleID: s.roleID })),
    },
    saving: false,
    error: "",
    sectionError: false,
  };
}

function closeUserModal() {
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
      if (!userModal.value.form.userID.trim()) {
        userModal.value.error = "User ID is required.";
        return;
      }
      if (!userModal.value.form.userName.trim()) {
        userModal.value.error = "Full name is required.";
        return;
      }
      await userApi.add({
        userID: userModal.value.form.userID.trim().toUpperCase(),
        userName: userModal.value.form.userName.trim(),
        isAdmin: userModal.value.form.isAdmin,
        sections: userModal.value.form.sections,
      });
      showToast("User added successfully.");
    } else {
      // Update info
      await userApi.update(userModal.value.userID, {
        userName: userModal.value.form.userName.trim(),
        isAdmin: userModal.value.form.isAdmin,
        active: usersList.value.find((u) => u.userID === userModal.value.userID)?.active ?? true,
      });
      // Update sections
      await userApi.updateSections(userModal.value.userID, {
        sections: userModal.value.form.sections,
      });
      showToast("User updated successfully.");
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
    showToast(err.response?.data?.message || "Failed to update status.");
  }
}

function onHclabSearchBlur(e) {
  setTimeout(() => {
    e.target.style.borderColor = "var(--color-border)";
    if (!userModal.value.hclabDropdownOpen) return;
    if (!userModal.value.form.userID) closeHclabDropdown();
  }, 150);
}

// ── Extend userModal with HCLAB-specific state fields ────────────────

let hclabSearchTimer = null;

function computeDropdownPos() {
  const el = userIdInputRef.value;
  if (!el) return;
  const rect = el.getBoundingClientRect();
  hclabDropdownPos.value = {
    top: rect.bottom + 6,
    left: rect.left,
    width: rect.width,
  };
}

function onHclabSearchFocus() {
  userIdInputRef.value?.style.setProperty("border-color", "var(--color-primary)");
  if (userModal.value.hclabResults.length) {
    computeDropdownPos();
    userModal.value.hclabDropdownOpen = true;
  }
}

function onHclabSearchInput() {
  userModal.value.form.userID = ""; // clear selection on new input
  userModal.value.form.userName = "";
  userModal.value.hclabNoResults = false;
  userModal.value.hclabDropdownOpen = false;
  userModal.value.hclabActiveIdx = -1;
  clearTimeout(hclabSearchTimer);

  const q = userModal.value.hclabSearch.trim();
  if (q.length < 3) {
    userModal.value.hclabResults = [];
    return;
  }

  userModal.value.hclabLoading = true;
  hclabSearchTimer = setTimeout(async () => {
    try {
      const res = await userApi.getHCLABUsers(q);
      userModal.value.hclabResults = res.data ?? [];
      userModal.value.hclabNoResults = userModal.value.hclabResults.length === 0;
      userModal.value.hclabDropdownOpen = true;
      await nextTick();
      computeDropdownPos();
      console.log(res);
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

function closeHclabDropdown() {
  userModal.value.hclabDropdownOpen = false;
}

// Keyboard navigation
function hclabSelectNext() {
  if (!userModal.value.hclabResults.length) return;
  userModal.value.hclabActiveIdx =
    (userModal.value.hclabActiveIdx + 1) % userModal.value.hclabResults.length;
}

function hclabSelectPrev() {
  if (!userModal.value.hclabResults.length) return;
  userModal.value.hclabActiveIdx =
    (userModal.value.hclabActiveIdx - 1 + userModal.value.hclabResults.length) %
    userModal.value.hclabResults.length;
}

function hclabConfirmSelected() {
  const idx = userModal.value.hclabActiveIdx;
  if (idx >= 0 && userModal.value.hclabResults[idx]) {
    selectHclabUser(userModal.value.hclabResults[idx]);
  }
}

// ── Section categories definition ───────────────────────────────────
const sectionCategories = [
  {
    id: "1",
    label: "Endorser",
    icon: "outbox",
    color: "#2e7d9a",
    softColor: "rgba(46,125,154,0.1)",
  },
  {
    id: "2",
    label: "Receiver",
    icon: "move_to_inbox",
    color: "#6a4c93",
    softColor: "rgba(106,76,147,0.1)",
  },
  {
    id: "3",
    label: "Laboratory",
    icon: "science",
    color: "#2e7d4f",
    softColor: "rgba(46,125,79,0.1)",
  },
];

// ── Test groups state ────────────────────────────────────────────────
const allTestGroups = ref([]); // [{ code, name, assignedSectionCode }]
const testGroupsLoading = ref(false);

async function loadTestGroups() {
  testGroupsLoading.value = true;
  try {
    const res = await sectionApi.getTestGroups();
    allTestGroups.value = res.data;
  } catch (e) {
    allTestGroups.value = [];
  } finally {
    testGroupsLoading.value = false;
  }
}

// ── Sections state ───────────────────────────────────────────────────
const sectionsList = ref([]);
const sectionsLoading = ref(false);
const sectionSearch = ref("");
const sectionBranchFilter = ref("ALL");
const availableBranches = ref([]); // loaded from settingsApi.getBranches()

const filteredSections = computed(() => {
  let list = sectionsList.value;
  if (sectionBranchFilter.value !== "ALL")
    list = list.filter((s) => s.branchCode === sectionBranchFilter.value);
  const q = sectionSearch.value.toLowerCase();
  if (q)
    list = list.filter((s) => s.code.toLowerCase().includes(q) || s.name.toLowerCase().includes(q));
  return list;
});

function filteredSectionsByCategory(catId) {
  return filteredSections.value.filter((s) => s.category === catId);
}

function receiverExistsForBranch(branchCode) {
  return sectionsList.value.some((s) => s.category === "2" && s.branchCode === branchCode);
}

async function loadSectionsList() {
  sectionsLoading.value = true;
  try {
    const res = await sectionApi.getAll();
    sectionsList.value = res.data;
  } catch {
    sectionsList.value = [];
  } finally {
    sectionsLoading.value = false;
  }
}

async function loadBranches() {
  try {
    const res = await settingsApi.getBranches();
    availableBranches.value = res.data.filter((b) => b.active);
  } catch {
    availableBranches.value = [];
  }
}

watch(activeTab, (val) => {
  if (val === "sections" && !sectionsList.value.length) {
    loadSectionsList();
    loadBranches();
  }
});

// ── Section modal ────────────────────────────────────────────────────
let codeCheckTimer = null;

const sectionModal = ref({
  visible: false,
  mode: "add", // 'add' | 'edit'
  originalCode: null,
  form: { code: "", name: "", branchCode: "", category: "1", autoNo: 0 },
  codeStatus: "idle", // 'idle' | 'checking' | 'available' | 'taken'
  codeChecking: false,
  saving: false,
  error: "",
});

function onCodeInput() {
  sectionModal.value.form.code = sectionModal.value.form.code.toUpperCase();
  sectionModal.value.codeStatus = "idle";
  clearTimeout(codeCheckTimer);

  const code = sectionModal.value.form.code.trim();
  if (!code) return;

  sectionModal.value.codeChecking = true;
  codeCheckTimer = setTimeout(async () => {
    try {
      const res = await sectionApi.checkCode(code);
      sectionModal.value.codeStatus = res.data.exists ? "taken" : "available";
    } catch {
      sectionModal.value.codeStatus = "idle";
    } finally {
      sectionModal.value.codeChecking = false;
    }
  }, 400);
}

function openAddSection() {
  sectionModal.value = {
    visible: true,
    mode: "add",
    originalCode: null,
    form: {
      code: "",
      name: "",
      branchCode: availableBranches.value[0]?.code ?? "",
      category: "1",
      autoNo: 0,
      testGroupCodes: [],
    },
    codeStatus: "idle",
    codeChecking: false,
    testGroupError: false,
    saving: false,
    error: "",
  };
  // Always load fresh test group assignments when modal opens
  loadTestGroups();
}

function openEditSection(sec) {
  sectionModal.value = {
    visible: true,
    mode: "edit",
    originalCode: sec.code,
    form: {
      code: sec.code,
      name: sec.name,
      branchCode: sec.branchCode,
      category: sec.category,
      autoNo: sec.autoNo ?? 0,
      testGroupCodes: sec.testGroups?.filter((tg) => tg.active).map((tg) => tg.testGroupCode) ?? [],
    },
    codeStatus: "idle",
    codeChecking: false,
    testGroupError: false,
    saving: false,
    error: "",
  };
  if (sec.category === "3") loadTestGroups();
}

function closeSectionModal() {
  clearTimeout(codeCheckTimer);
  sectionModal.value.visible = false;
}

async function saveSectionModal() {
  sectionModal.value.error = "";
  sectionModal.value.testGroupError = false;

  const f = sectionModal.value.form;

  if (sectionModal.value.mode === "add") {
    if (!f.code.trim()) {
      sectionModal.value.error = "Section code is required.";
      return;
    }
    if (sectionModal.value.codeStatus === "taken") {
      sectionModal.value.error = "This section code already exists.";
      return;
    }
    if (sectionModal.value.codeChecking) return;
    if (!f.branchCode) {
      sectionModal.value.error = "Please select a branch.";
      return;
    }
    if (!f.category) {
      sectionModal.value.error = "Please select a category.";
      return;
    }
    if (f.category === "2" && receiverExistsForBranch(f.branchCode)) {
      sectionModal.value.error = "A Receiver already exists for this branch.";
      return;
    }
  }

  if (!f.name.trim()) {
    sectionModal.value.error = "Section name is required.";
    return;
  }

  if (f.category === "3" && !f.testGroupCodes.length) {
    sectionModal.value.testGroupError = true;
    return;
  }

  sectionModal.value.saving = true;
  try {
    if (sectionModal.value.mode === "add") {
      await sectionApi.add({
        code: f.code.trim().toUpperCase(),
        name: f.name.trim(),
        branchCode: f.branchCode,
        category: f.category,
        autoNo: f.category === "1" ? (f.autoNo ?? 0) : 0,
        testGroupCodes: f.category === "3" ? f.testGroupCodes : [],
      });
      showToast("Section created successfully.");
    } else {
      await sectionApi.update(sectionModal.value.originalCode, {
        name: f.name.trim(),
        /* autoNo: f.category === '1' ? (f.autoNo ?? 0) : 0,*/
        testGroupCodes: f.category === "3" ? f.testGroupCodes : [],
      });
      showToast("Section updated successfully.");
    }
    closeSectionModal();
    await loadSectionsList();
  } catch (err) {
    sectionModal.value.error = err.response?.data?.message || "An error occurred.";
  } finally {
    sectionModal.value.saving = false;
  }
}

async function toggleSection(sec) {
  try {
    await sectionApi.toggle(sec.code);
    sec.active = !sec.active;
  } catch (err) {
    showToast(err.response?.data?.message || "Failed to update status.");
  }
}

function toggleTestGroup(code) {
  const idx = sectionModal.value.form.testGroupCodes.indexOf(code);
  if (idx === -1) sectionModal.value.form.testGroupCodes.push(code);
  else sectionModal.value.form.testGroupCodes.splice(idx, 1);
  sectionModal.value.testGroupError = false;
}

// ──────────────────────────────────────────────────────────────────────────── RUNNING DAYS
// ── Day definitions ──────────────────────────────────────────────────
// Week displayed Mon→Sun; color per day for visual distinction
const allDays = [
  { value: "Monday", short: "Mon", color: "#1565c0", softColor: "rgba(21,101,192,0.1)" },
  { value: "Tuesday", short: "Tue", color: "#6a1b9a", softColor: "rgba(106,27,154,0.1)" },
  { value: "Wednesday", short: "Wed", color: "#2e7d32", softColor: "rgba(46,125,50,0.1)" },
  { value: "Thursday", short: "Thu", color: "#e65100", softColor: "rgba(230,81,0,0.1)" },
  { value: "Friday", short: "Fri", color: "#c62828", softColor: "rgba(198,40,40,0.1)" },
  { value: "Saturday", short: "Sat", color: "#558b2f", softColor: "rgba(85,139,47,0.1)" },
  { value: "Sunday", short: "Sun", color: "#4e342e", softColor: "rgba(78,52,46,0.1)" },
];

// ── Running days state ───────────────────────────────────────────────
const rdList = ref([]);
const rdLoading = ref(false);
const rdSearch = ref("");

const rdColumns = [
  { key: "testName", label: "Test" },
  { key: "runningDays", label: "Running Days" },
];

const filteredRd = computed(() => {
  const q = rdSearch.value.toLowerCase();
  if (!q) return rdList.value;
  return rdList.value.filter(
    (r) => r.testCode.toLowerCase().includes(q) || r.testName.toLowerCase().includes(q),
  );
});

async function loadRunningDays() {
  rdLoading.value = true;
  try {
    const res = await testRunningDayApi.getAll();
    rdList.value = res.data;
  } catch (e) {
    console.log(e);
    rdList.value = [];
  } finally {
    rdLoading.value = false;
  }
}

watch(activeTab, (val) => {
  if (val === "runningDays" && !rdList.value.length) loadRunningDays();
});

// ── Modal state ───────────────────────────────────────────────────────
const rdTestInputRef = ref(null);
const rdDropdownPos = ref({ top: 0, left: 0, width: 0 });
let rdSearchTimer = null;

const rdModal = ref({
  visible: false,
  mode: "add",
  id: null,
  form: { testCode: "", testName: "", days: [] },
  testSearch: "",
  testResults: [],
  testLoading: false,
  dropdownOpen: false,
  testNoResults: false,
  activeIdx: -1,
  dayError: false,
  saving: false,
  error: "",
});

function openAddRunningDay() {
  rdModal.value = {
    visible: true,
    mode: "add",
    id: null,
    form: { testCode: "", testName: "", days: [] },
    testSearch: "",
    testResults: [],
    testLoading: false,
    dropdownOpen: false,
    testNoResults: false,
    activeIdx: -1,
    dayError: false,
    saving: false,
    error: "",
  };
}

function openEditRunningDay(item) {
  rdModal.value = {
    visible: true,
    mode: "edit",
    id: item.id,
    form: { testCode: item.testCode, testName: item.testName, days: [...item.dayList] },
    testSearch: "",
    testResults: [],
    testLoading: false,
    dropdownOpen: false,
    testNoResults: false,
    activeIdx: -1,
    dayError: false,
    saving: false,
    error: "",
  };
}

function closeRdModal() {
  clearTimeout(rdSearchTimer);
  rdModal.value.visible = false;
}

function toggleRdDay(val) {
  const idx = rdModal.value.form.days.indexOf(val);
  if (idx === -1) rdModal.value.form.days.push(val);
  else rdModal.value.form.days.splice(idx, 1);
  rdModal.value.dayError = false;
}

function computeRdDropdownPos() {
  const el = rdTestInputRef.value;
  if (!el) return;
  const rect = el.getBoundingClientRect();
  rdDropdownPos.value = { top: rect.bottom + 6, left: rect.left, width: rect.width };
}

function onRdTestInput() {
  rdModal.value.form.testCode = "";
  rdModal.value.form.testName = "";
  rdModal.value.dropdownOpen = false;
  rdModal.value.testNoResults = false;
  rdModal.value.activeIdx = -1;
  clearTimeout(rdSearchTimer);

  const q = rdModal.value.testSearch.trim();
  if (q.length < 3) {
    rdModal.value.testResults = [];
    return;
  }

  rdModal.value.testLoading = true;
  rdSearchTimer = setTimeout(async () => {
    try {
      const res = await testRunningDayApi.search(q);
      rdModal.value.testResults = res.data ?? [];
      rdModal.value.testNoResults = rdModal.value.testResults.length === 0;
      rdModal.value.dropdownOpen = true;
      await nextTick();
      computeRdDropdownPos();
    } catch {
      rdModal.value.testResults = [];
      rdModal.value.testNoResults = true;
    } finally {
      rdModal.value.testLoading = false;
    }
  }, 350);
}

function selectRdTest(t) {
  rdModal.value.form.testCode = t.testCode;
  rdModal.value.form.testName = t.testName;
  rdModal.value.testSearch = `${t.testCode} — ${t.testName}`;
  rdModal.value.dropdownOpen = false;
  rdModal.value.activeIdx = -1;
}

function clearRdTest() {
  rdModal.value.form.testCode = "";
  rdModal.value.form.testName = "";
  rdModal.value.testSearch = "";
  rdModal.value.testResults = [];
  rdModal.value.dropdownOpen = false;
  nextTick(() => rdTestInputRef.value?.focus());
}

function rdSelectNext() {
  if (!rdModal.value.testResults.length) return;
  rdModal.value.activeIdx = (rdModal.value.activeIdx + 1) % rdModal.value.testResults.length;
}

function rdSelectPrev() {
  if (!rdModal.value.testResults.length) return;
  rdModal.value.activeIdx =
    (rdModal.value.activeIdx - 1 + rdModal.value.testResults.length) %
    rdModal.value.testResults.length;
}

function rdConfirmSelected() {
  const idx = rdModal.value.activeIdx;
  if (idx >= 0 && rdModal.value.testResults[idx]) selectRdTest(rdModal.value.testResults[idx]);
}

async function saveRdModal() {
  rdModal.value.error = "";
  rdModal.value.dayError = false;

  if (rdModal.value.mode === "add" && !rdModal.value.form.testCode) {
    rdModal.value.error = "Please select a test.";
    return;
  }

  if (!rdModal.value.form.days.length) {
    rdModal.value.dayError = true;
    return;
  }

  rdModal.value.saving = true;
  try {
    if (rdModal.value.mode === "add") {
      await testRunningDayApi.add({
        testCode: rdModal.value.form.testCode,
        testName: rdModal.value.form.testName,
        days: rdModal.value.form.days,
      });
      showToast("Running day setup saved.");
    } else {
      await testRunningDayApi.update(rdModal.value.id, {
        days: rdModal.value.form.days,
      });
      showToast("Running days updated.");
    }
    closeRdModal();
    await loadRunningDays();
  } catch (err) {
    rdModal.value.error = err.response?.data?.message || "An error occurred.";
  } finally {
    rdModal.value.saving = false;
  }
}

const rdDeleteConfirm = ref({
  visible: false,
  itemId: null,
  itemName: "",
});

function confirmDeleteRd(item) {
  rdDeleteConfirm.value = {
    visible: true,
    itemId: item.id,
    itemName: item.testName,
  };
}

async function executeDeleteRd() {
  try {
    await testRunningDayApi.delete(rdDeleteConfirm.value.itemId);
    rdList.value = rdList.value.filter((r) => r.id !== rdDeleteConfirm.value.itemId);
    showToast("Running day setup removed.");
  } catch (err) {
    showToast(err.response?.data?.message || "Failed to remove setup.");
  }
}

function onRdTestBlur(e) {
  setTimeout(() => {
    e.target.style.borderColor = "var(--color-border)";
  }, 150);
}

// ── TAT Set-Up ────────────────────────────────────────────────────────────

// Endorsement TAT
const tatList = ref([]);
const tatLoading = ref(false);
const tatSaving = ref(false);

async function loadTat() {
  tatLoading.value = true;
  try {
    const [sectionsRes, tatRes] = await Promise.all([sectionApi.getAll(), tatApi.getAll()]);
    const endorsers = sectionsRes.data.filter((s) => s.category === "1" && s.active);
    tatList.value = endorsers.map((s) => {
      const existing = tatRes.find((t) => t.sectionCode === s.code);
      return {
        sectionCode: s.code,
        name: s.name,
        hours: existing?.hours ?? 0,
        minutes: existing?.minutes ?? 30,
        appealWindow: existing?.appealWindow ?? "Before",
      };
    });
  } catch {
    tatList.value = [];
  } finally {
    tatLoading.value = false;
  }
}

async function saveTat() {
  tatSaving.value = true;
  try {
    const payload = tatList.value.map((t) => ({
      sectionCode: t.sectionCode,
      hours: t.hours ?? 0,
      minutes: t.minutes ?? 0,
      appealWindow: t.appealWindow ?? "Before",
    }));
    await tatApi.upsert(payload);
    showToast("Endorsement TAT settings saved.");
  } catch (err) {
    showToast(err.response?.data?.message || "Failed to save endorsement TAT settings.");
  } finally {
    tatSaving.value = false;
  }
}

// Processing TAT
const procTat = ref({ hours: 0, minutes: 30 });
const procTatLoading = ref(false);
const procTatSaving = ref(false);

async function loadProcTat() {
  procTatLoading.value = true;
  try {
    const data = await tatApi.getProcessing();
    procTat.value = { hours: data.hours ?? 0, minutes: data.minutes ?? 30 };
  } catch {
    procTat.value = { hours: 0, minutes: 30 };
  } finally {
    procTatLoading.value = false;
  }
}

async function saveProcTat() {
  procTatSaving.value = true;
  try {
    await tatApi.upsertProcessing({
      hours: procTat.value.hours ?? 0,
      minutes: procTat.value.minutes ?? 0,
    });
    showToast("Processing TAT saved.");
  } catch (err) {
    showToast(err.response?.data?.message || "Failed to save processing TAT.");
  } finally {
    procTatSaving.value = false;
  }
}

watch(activeTab, (val) => {
  if (val === "tat") {
    if (!tatList.value.length) loadTat();
    loadProcTat();
  }
});

// ── On-Site Settings state ─────────────────────────────────────────── ONSITE
const onSiteEnabled = ref(false);
const onSiteLabNos = ref([]);
const onSiteLoading = ref(false);
const onSiteToggling = ref(false);
const onSiteNewPrefix = ref("");
const onSiteNewDescription = ref("");
const onSiteAddError = ref("");
const onSiteAdding = ref(false);
const onSiteDeleteConfirm = ref({ visible: false, id: null, prefix: "" });

async function loadOnSiteSettings() {
  onSiteLoading.value = true;
  try {
    const data = await runnerApi.getOnSiteSettings();
    onSiteEnabled.value = data.isEnabled;
    onSiteLabNos.value = data.allowedLabNos ?? [];
  } catch {
    onSiteLabNos.value = [];
  } finally {
    onSiteLoading.value = false;
  }
}

async function toggleOnSite() {
  onSiteToggling.value = true;
  try {
    await runnerApi.toggleOnSite(!onSiteEnabled.value);
    onSiteEnabled.value = !onSiteEnabled.value;
    showToast(`On-Site scanning ${onSiteEnabled.value ? "enabled" : "disabled"}.`);
  } catch (err) {
    showToast(err.response?.data?.message || "Failed to update On-Site setting.");
  } finally {
    onSiteToggling.value = false;
  }
}

async function addLabNo() {
  onSiteAddError.value = "";
  const prefix = onSiteNewPrefix.value.trim();
  if (!prefix) {
    onSiteAddError.value = "Prefix is required.";
    return;
  }
  if (!/^\d{2}$/.test(prefix)) {
    onSiteAddError.value = "Prefix must be exactly 2 digits.";
    return;
  }

  onSiteAdding.value = true;
  try {
    await runnerApi.addAllowedLabNo({
      prefix,
      description: onSiteNewDescription.value.trim() || null,
    });
    onSiteNewPrefix.value = "";
    onSiteNewDescription.value = "";
    await loadOnSiteSettings();
    showToast(`Prefix '${prefix}' added.`);
  } catch (err) {
    onSiteAddError.value = err.response?.data?.message || "Failed to add prefix.";
  } finally {
    onSiteAdding.value = false;
  }
}

async function toggleLabNo(entry) {
  try {
    await runnerApi.toggleAllowedLabNo(entry.id);
    entry.active = !entry.active;
  } catch (err) {
    showToast(err.response?.data?.message || "Failed to update status.");
  }
}

function confirmDeleteLabNo(entry) {
  onSiteDeleteConfirm.value = { visible: true, id: entry.id, prefix: entry.prefix };
}

async function executeDeleteLabNo() {
  try {
    await runnerApi.deleteAllowedLabNo(onSiteDeleteConfirm.value.id);
    onSiteLabNos.value = onSiteLabNos.value.filter((l) => l.id !== onSiteDeleteConfirm.value.id);
    showToast(`Prefix '${onSiteDeleteConfirm.value.prefix}' removed.`);
  } catch (err) {
    showToast(err.response?.data?.message || "Failed to delete.");
  } finally {
    onSiteDeleteConfirm.value.visible = false;
  }
}

watch(activeTab, (val) => {
  if (val === "onsite") loadOnSiteSettings();
  if (val === 'processing') loadProcessingOptions()
});

  // ── Processing Options ────────────────────────────────────────────────────

  const procOptions = ref({ showTemperature: true, showTempRemarks: true, showBagNo: true })
  const procOptionsLoading = ref(false)
  const procOptionsSaving = ref(false)

  async function loadProcessingOptions() {
    procOptionsLoading.value = true
    try {
      const data = await processingOptionsApi.get()
      procOptions.value = {
        showTemperature: data.showTemperature,
        showTempRemarks: data.showTempRemarks,
        showBagNo: data.showBagNo
      }
    } catch {
      procOptions.value = { showTemperature: true, showTempRemarks: true, showBagNo: true }
    } finally {
      procOptionsLoading.value = false
    }
  }

  async function saveProcessingOptions() {
    procOptionsSaving.value = true
    try {
      await processingOptionsApi.upsert({
        showTemperature: procOptions.value.showTemperature,
        showTempRemarks: procOptions.value.showTempRemarks,
        showBagNo: procOptions.value.showBagNo
      })
      showToast('Processing options saved.')
    } catch (err) {
      showToast(err.response?.data?.message || 'Failed to save processing options.')
    } finally {
      procOptionsSaving.value = false
    }
  }
</script>

<style scoped>
.toast-enter-active,
.toast-leave-active {
  transition:
    opacity 0.25s ease,
    transform 0.25s ease;
}

.toast-enter-from,
.toast-leave-to {
  opacity: 0;
  transform: translateY(8px);
}
</style>
