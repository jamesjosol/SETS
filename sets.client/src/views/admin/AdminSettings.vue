<template>
  <AppLayout>

    <!-- Page Header -->
    <div class="mb-6">
      <h1 class="text-2xl font-extrabold tracking-tight" style="color: var(--color-text);">
        Global Settings
      </h1>
      <p class="text-sm mt-1" style="color: var(--color-text-muted);">
        <span style="color: var(--color-primary); font-weight: 700;">ADMINISTRATOR</span>
        · System Configuration
      </p>
    </div>

    <!-- Settings Layout: Sidebar + Content -->
    <div class="flex gap-6">

      <!-- Left Nav Sidebar -->
      <aside class="w-56 flex-shrink-0">
        <div class="rounded-2xl overflow-hidden sticky top-6"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
          <div class="p-2 space-y-0.5">
            <button v-for="tab in settingsTabs"
                    :key="tab.key"
                    class="w-full flex items-center gap-3 px-4 py-3 rounded-xl text-xs font-bold uppercase tracking-widest transition-all text-left"
                    :style="activeTab === tab.key
                      ? 'background-color: var(--color-primary-soft); color: var(--color-primary); border-left: 3px solid var(--color-primary); padding-left: calc(1rem - 3px);'
                      : 'color: var(--color-text-muted); border-left: 3px solid transparent; padding-left: calc(1rem - 3px);'"
                    @click="activeTab = tab.key">
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
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">

          <!-- Header -->
          <div class="px-8 py-5 flex items-center justify-between" style="border-bottom: 1px solid var(--color-border);">
            <div class="flex items-center gap-4">
              <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
                   style="background-color: var(--color-primary-soft);">
                <span class="material-symbols-outlined text-base" style="color: var(--color-primary);">computer</span>
              </div>
              <div>
                <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text);">PC Registration</h2>
                <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">
                  Register workstations by IP address and assign which sections they can access.
                </p>
              </div>
            </div>
            <button class="flex items-center gap-2 px-4 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 shadow"
                    style="background: var(--color-primary-gradient); color: #ffffff;"
                    @click="openAddPC">
              <span class="material-symbols-outlined text-sm">add</span>
              Register PC
            </button>
          </div>

          <!-- PC Table -->
          <div class="px-8 py-6">
            <!-- Loading -->
            <div v-if="pcLoading" class="flex items-center justify-center py-12 gap-3"
                 style="color: var(--color-text-muted);">
              <span class="material-symbols-outlined text-xl animate-spin">progress_activity</span>
              <span class="text-sm font-medium">Loading registered PCs...</span>
            </div>

            <!-- Empty -->
            <div v-else-if="!pcList.length"
                 class="py-12 flex flex-col items-center gap-2" style="color: var(--color-text-muted);">
              <span class="material-symbols-outlined text-3xl">computer_cancel</span>
              <p class="text-sm font-medium">No PCs registered yet.</p>
            </div>

            <!-- Table -->
            <table v-else class="w-full text-sm">
              <thead>
                <tr style="border-bottom: 1px solid var(--color-border);">
                  <th class="text-left pb-3 text-[10px] font-bold uppercase tracking-widest pr-6"
                      style="color: var(--color-text-muted);">IP Address</th>
                  <th class="text-left pb-3 text-[10px] font-bold uppercase tracking-widest pr-6"
                      style="color: var(--color-text-muted);">Description</th>
                  <th class="text-left pb-3 text-[10px] font-bold uppercase tracking-widest pr-6"
                      style="color: var(--color-text-muted);">Allowed Sections</th>
                  <th class="text-left pb-3 text-[10px] font-bold uppercase tracking-widest pr-6"
                      style="color: var(--color-text-muted);">Status</th>
                  <th class="pb-3"></th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="pc in pcList" :key="pc.id"
                    style="border-bottom: 1px solid var(--color-surface-low);">
                  <!-- IP Address -->
                  <td class="py-3 pr-6">
                    <span class="font-mono font-bold text-xs" style="color: var(--color-text);">
                      {{ pc.ipAddress }}
                    </span>
                  </td>

                  <!-- Description -->
                  <td class="py-3 pr-6">
                    <span class="text-sm" style="color: var(--color-text-muted);">
                      {{ pc.description || '—' }}
                    </span>
                  </td>

                  <!-- Sections -->
                  <td class="py-3 pr-6">
                    <div class="flex flex-wrap gap-1">
                      <span v-if="!pc.sections.length"
                            class="text-xs font-bold" style="color: var(--color-text-muted);">No sections</span>
                      <span v-for="sec in pc.sections" :key="sec.id"
                            class="px-2 py-0.5 rounded-lg text-[10px] font-bold"
                            style="background-color: var(--color-primary-soft); color: var(--color-primary);">
                        {{ sec.sectionName }}
                      </span>
                    </div>
                  </td>

                  <!-- Status toggle -->
                  <td class="py-3 pr-6">
                    <button class="flex items-center gap-1.5 px-3 py-1 rounded-lg text-[10px] font-bold uppercase tracking-widest transition-all"
                            :style="pc.active
                      ? 'background-color: var(--color-primary-soft); color: var(--color-primary);'
                      : 'background-color: var(--color-surface-low); color: var(--color-text-muted); border: 1px solid var(--color-border);'"
                            @click="togglePC(pc)">
                      <span class="material-symbols-outlined text-xs">
                        {{ pc.active ? 'check_circle' : 'cancel' }}
                      </span>
                      {{ pc.active ? 'Active' : 'Inactive' }}
                    </button>
                  </td>

                  <!-- Actions -->
                  <td class="py-3">
                    <div class="flex items-center gap-1 justify-end">
                      <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                              style="color: var(--color-text-muted);"
                              title="Edit sections"
                              @mouseenter="(e) => e.currentTarget.style.backgroundColor = 'var(--color-surface-low)'"
                              @mouseleave="(e) => e.currentTarget.style.backgroundColor = 'transparent'"
                              @click="openEditSections(pc)">
                        <span class="material-symbols-outlined text-sm">edit</span>
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- ══════════════════════════════════════════════════════════
       ADD PC MODAL
  ══════════════════════════════════════════════════════════ -->
        <Teleport to="body">
          <Transition name="modal">
            <div v-if="pcModal.visible"
                 class="fixed inset-0 z-50 flex items-center justify-center"
                 style="background-color: rgba(0,0,0,0.4);"
                 @click.self="closePCModal">

              <div class="w-full max-w-md rounded-2xl overflow-hidden"
                   style="background-color: var(--color-surface); box-shadow: 0 8px 32px rgba(0,0,0,0.24);">

                <!-- Modal Header -->
                <div class="px-6 py-4 flex items-center justify-between"
                     style="border-bottom: 1px solid var(--color-border);">
                  <div class="flex items-center gap-3">
                    <div class="w-8 h-8 rounded-lg flex items-center justify-center"
                         style="background-color: var(--color-primary-soft);">
                      <span class="material-symbols-outlined text-sm" style="color: var(--color-primary);">computer</span>
                    </div>
                    <h3 class="text-sm font-extrabold tracking-tight" style="color: var(--color-text);">
                      {{ pcModal.mode === 'add' ? 'Register New PC' : 'Edit Section Assignments' }}
                    </h3>
                  </div>
                  <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                          style="color: var(--color-text-muted);"
                          @mouseenter="(e) => e.currentTarget.style.backgroundColor = 'var(--color-surface-low)'"
                          @mouseleave="(e) => e.currentTarget.style.backgroundColor = 'transparent'"
                          @click="closePCModal">
                    <span class="material-symbols-outlined text-base">close</span>
                  </button>
                </div>

                <!-- Modal Body -->
                <div class="px-6 py-5 space-y-4">

                  <!-- IP Address (add mode only) -->
                  <div v-if="pcModal.mode === 'add'">
                    <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5"
                           style="color: var(--color-text-muted);">IP Address *</label>
                    <input v-model="pcModal.form.ipAddress"
                           placeholder="e.g. 192.168.1.100"
                           class="w-full px-4 py-2.5 rounded-xl text-sm font-mono font-medium outline-none transition-all"
                           style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                           @focus="(e) => e.target.style.borderColor = 'var(--color-primary)'"
                           @blur="(e) => e.target.style.borderColor = 'var(--color-border)'" />
                  </div>

                  <!-- Description (add mode only) -->
                  <div v-if="pcModal.mode === 'add'">
                    <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5"
                           style="color: var(--color-text-muted);">Description</label>
                    <input v-model="pcModal.form.description"
                           placeholder="e.g. Hematology workstation"
                           class="w-full px-4 py-2.5 rounded-xl text-sm font-medium outline-none transition-all"
                           style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                           @focus="(e) => e.target.style.borderColor = 'var(--color-primary)'"
                           @blur="(e) => e.target.style.borderColor = 'var(--color-border)'" />
                  </div>

                  <!-- Section Assignment -->
                  <div>
                    <label class="block text-[10px] font-bold uppercase tracking-widest mb-2"
                           style="color: var(--color-text-muted);">Allowed Sections</label>
                    <div class="rounded-xl overflow-hidden"
                         style="border: 1.5px solid var(--color-border);">
                      <div v-for="sec in availableSections" :key="sec.code"
                           class="flex items-center justify-between px-4 py-2.5 transition-all cursor-pointer"
                           style="border-bottom: 1px solid var(--color-surface-low);"
                           :style="pcModal.form.sectionCodes.includes(sec.code)
                     ? 'background-color: var(--color-primary-soft);'
                     : ''"
                           @click="toggleSectionInModal(sec.code)">
                        <span class="text-sm font-medium" style="color: var(--color-text);">{{ sec.name }}</span>
                        <div class="w-5 h-5 rounded-md flex items-center justify-center transition-all"
                             :style="pcModal.form.sectionCodes.includes(sec.code)
                       ? 'background-color: var(--color-primary);'
                       : 'border: 1.5px solid var(--color-border);'">
                          <span v-if="pcModal.form.sectionCodes.includes(sec.code)"
                                class="material-symbols-outlined text-xs" style="color: #ffffff;">check</span>
                        </div>
                      </div>
                      <p v-if="!availableSections.length"
                         class="px-4 py-3 text-xs text-center" style="color: var(--color-text-muted);">
                        No sections available.
                      </p>
                    </div>
                  </div>

                  <!-- Error -->
                  <p v-if="pcModal.error" class="text-xs font-bold" style="color: #ba1a1a;">
                    {{ pcModal.error }}
                  </p>
                </div>

                <!-- Modal Footer -->
                <div class="px-6 py-4 flex justify-end gap-3"
                     style="border-top: 1px solid var(--color-border);">
                  <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                          style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                          @click="closePCModal">
                    Cancel
                  </button>
                  <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow"
                          style="background: var(--color-primary-gradient); color: #ffffff;"
                          :disabled="pcModal.saving"
                          @click="savePCModal">
                    <span v-if="pcModal.saving" class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
                    <span class="material-symbols-outlined text-sm" v-else>save</span>
                    {{ pcModal.mode === 'add' ? 'Register' : 'Save' }}
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
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
          <div class="px-8 py-5 flex items-center gap-4" style="border-bottom: 1px solid var(--color-border);">
            <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0" style="background-color: var(--color-primary-soft);">
              <span class="material-symbols-outlined text-base" style="color: var(--color-primary);">timer</span>
            </div>
            <div>
              <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text);">TAT Set-Up</h2>
              <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">Configure turnaround time thresholds for each workflow stage.</p>
            </div>
          </div>
          <div class="px-8 py-6 space-y-8">

            <div>
              <p class="text-[10px] font-bold uppercase tracking-widest mb-4" style="color: var(--color-text-muted);">Batch Endorsement (Endorsing Section)</p>
              <div class="flex items-center justify-between gap-6">
                <div>
                  <p class="text-sm font-bold" style="color: var(--color-text);">TAT Threshold</p>
                  <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">Time from branch dispatch to branch endorsement</p>
                </div>
                <div class="flex items-center gap-2">
                  <input v-model.number="settings.tat.batchEndorsement.hours" type="number" min="0"
                         class="w-16 px-3 py-2 rounded-xl text-sm font-bold text-center outline-none"
                         style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);" />
                  <span class="text-xs font-bold" style="color: var(--color-text-muted);">hr</span>
                  <input v-model.number="settings.tat.batchEndorsement.minutes" type="number" min="0" max="59"
                         class="w-16 px-3 py-2 rounded-xl text-sm font-bold text-center outline-none"
                         style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);" />
                  <span class="text-xs font-bold" style="color: var(--color-text-muted);">min</span>
                </div>
              </div>
            </div>

            <div>
              <p class="text-[10px] font-bold uppercase tracking-widest mb-4" style="color: var(--color-text-muted);">Batch Completion (Processing)</p>
              <div class="space-y-4">
                <div class="flex items-center justify-between gap-6">
                  <div>
                    <p class="text-sm font-bold" style="color: var(--color-text);">Within TAT Display Color</p>
                    <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">Color shown when batch is within TAT</p>
                  </div>
                  <div class="flex items-center gap-2">
                    <div class="w-4 h-4 rounded-full" style="background-color: #3b82f6;"></div>
                    <span class="text-xs font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Blue</span>
                    <span class="text-[10px] font-bold px-2 py-0.5 rounded-lg" style="background-color: var(--color-surface-low); color: var(--color-text-muted);">Fixed</span>
                  </div>
                </div>
                <div class="flex items-center justify-between gap-6">
                  <div>
                    <p class="text-sm font-bold" style="color: var(--color-text);">Outside TAT Display Color</p>
                    <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">Color shown when batch is outside TAT</p>
                  </div>
                  <div class="flex items-center gap-2">
                    <div class="w-4 h-4 rounded-full" style="background-color: #ef4444;"></div>
                    <span class="text-xs font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Red</span>
                    <span class="text-[10px] font-bold px-2 py-0.5 rounded-lg" style="background-color: var(--color-surface-low); color: var(--color-text-muted);">Fixed</span>
                  </div>
                </div>
                <div class="flex items-center justify-between gap-6">
                  <p class="text-sm font-bold" style="color: var(--color-text);">TAT Threshold</p>
                  <div class="flex items-center gap-2">
                    <input v-model.number="settings.tat.batchCompletion.hours" type="number" min="0"
                           class="w-16 px-3 py-2 rounded-xl text-sm font-bold text-center outline-none"
                           style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);" />
                    <span class="text-xs font-bold" style="color: var(--color-text-muted);">hr</span>
                    <input v-model.number="settings.tat.batchCompletion.minutes" type="number" min="0" max="59"
                           class="w-16 px-3 py-2 rounded-xl text-sm font-bold text-center outline-none"
                           style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);" />
                    <span class="text-xs font-bold" style="color: var(--color-text-muted);">min</span>
                  </div>
                </div>
              </div>
            </div>

            <div>
              <p class="text-[10px] font-bold uppercase tracking-widest mb-4" style="color: var(--color-text-muted);">Specimen Receipt (Section)</p>
              <div class="flex items-center justify-between gap-6">
                <div>
                  <p class="text-sm font-bold" style="color: var(--color-text);">TAT Threshold</p>
                  <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">Time from processing receipt to section receipt</p>
                </div>
                <div class="flex items-center gap-2">
                  <input v-model.number="settings.tat.specimenReceipt.hours" type="number" min="0"
                         class="w-16 px-3 py-2 rounded-xl text-sm font-bold text-center outline-none"
                         style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);" />
                  <span class="text-xs font-bold" style="color: var(--color-text-muted);">hr</span>
                  <input v-model.number="settings.tat.specimenReceipt.minutes" type="number" min="0" max="59"
                         class="w-16 px-3 py-2 rounded-xl text-sm font-bold text-center outline-none"
                         style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);" />
                  <span class="text-xs font-bold" style="color: var(--color-text-muted);">min</span>
                </div>
              </div>
            </div>

            <div class="flex justify-end pt-4" style="border-top: 1px solid var(--color-border);">
              <button class="px-6 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow-lg"
                      style="background: var(--color-primary-gradient); color: #ffffff;" @click="save('TAT')">
                <span class="material-symbols-outlined text-sm">save</span>Save Changes
              </button>
            </div>
          </div>
        </div>

        <!-- ══════════════════════════════════════════════════════════
       BRANCH
  ══════════════════════════════════════════════════════════ -->
        <div v-if="activeTab === 'branch'"
             class="rounded-2xl overflow-hidden"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
          <div class="px-8 py-5 flex items-center gap-4" style="border-bottom: 1px solid var(--color-border);">
            <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0" style="background-color: var(--color-primary-soft);">
              <span class="material-symbols-outlined text-base" style="color: var(--color-primary);">location_city</span>
            </div>
            <div>
              <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text);">Branch Management</h2>
              <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">Add and manage branch locations across the network.</p>
            </div>
          </div>
          <div class="px-8 py-6 space-y-8">

            <div>
              <p class="text-[10px] font-bold uppercase tracking-widest mb-4" style="color: var(--color-text-muted);">Add New Branch</p>
              <div class="flex gap-3">
                <div class="flex-1">
                  <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5" style="color: var(--color-text-muted);">Branch Code</label>
                  <input v-model="newBranch.code" class="w-full px-4 py-2.5 rounded-xl text-sm font-medium outline-none transition-all"
                         style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                         placeholder="e.g. WES" maxlength="10"
                         @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                         @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
                </div>
                <div class="flex-[2]">
                  <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5" style="color: var(--color-text-muted);">Branch Name</label>
                  <input v-model="newBranch.name" class="w-full px-4 py-2.5 rounded-xl text-sm font-medium outline-none transition-all"
                         style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                         placeholder="e.g. West Branch"
                         @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                         @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
                </div>
                <div class="flex items-end">
                  <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2"
                          style="background: var(--color-primary-gradient); color: #ffffff;" @click="addBranch">
                    <span class="material-symbols-outlined text-sm">add</span>Add
                  </button>
                </div>
              </div>
            </div>

            <div>
              <p class="text-[10px] font-bold uppercase tracking-widest mb-4" style="color: var(--color-text-muted);">Existing Branches</p>
              <div class="space-y-2">
                <div v-for="branch in settings.branches" :key="branch.code"
                     class="flex items-center gap-4 px-4 py-3 rounded-xl" style="background-color: var(--color-surface-low);">
                  <div class="w-10 h-10 rounded-xl flex items-center justify-center text-xs font-extrabold flex-shrink-0"
                       style="background-color: var(--color-primary-soft); color: var(--color-primary);">
                    {{ branch.code.substring(0, 3) }}
                  </div>
                  <div class="flex-1">
                    <p class="text-sm font-bold" style="color: var(--color-text);">{{ branch.name }}</p>
                    <p class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">{{ branch.code }}</p>
                  </div>
                  <button @click="branch.isActive = !branch.isActive"
                          class="flex items-center gap-2 px-3 py-1.5 rounded-xl text-[10px] font-bold uppercase tracking-widest transition-all"
                          :style="branch.isActive
                            ? 'background-color: var(--color-primary-soft); color: var(--color-primary);'
                            : 'background-color: var(--color-surface); color: var(--color-text-muted); border: 1px solid var(--color-border);'">
                    <span class="material-symbols-outlined text-xs">{{ branch.isActive ? 'check_circle' : 'cancel' }}</span>
                    {{ branch.isActive ? 'Active' : 'Inactive' }}
                  </button>
                </div>
                <p v-if="!settings.branches.length" class="py-8 text-center text-sm" style="color: var(--color-text-muted);">No branches configured yet.</p>
              </div>
            </div>

            <div class="flex justify-end pt-4" style="border-top: 1px solid var(--color-border);">
              <button class="px-6 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow-lg"
                      style="background: var(--color-primary-gradient); color: #ffffff;" @click="save('Branch')">
                <span class="material-symbols-outlined text-sm">save</span>Save Changes
              </button>
            </div>
          </div>
        </div>

        <!-- ══════════════════════════════════════════════════════════
       SECTION / LOCATION
  ══════════════════════════════════════════════════════════ -->
        <div v-if="activeTab === 'section'"
             class="rounded-2xl overflow-hidden"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
          <div class="px-8 py-5 flex items-center gap-4" style="border-bottom: 1px solid var(--color-border);">
            <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0" style="background-color: var(--color-primary-soft);">
              <span class="material-symbols-outlined text-base" style="color: var(--color-primary);">apartment</span>
            </div>
            <div>
              <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text);">Section / Location</h2>
              <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">Manage lab sections, cut-offs, and assignment eligibility.</p>
            </div>
          </div>
          <div class="px-8 py-6 space-y-8">

            <div>
              <p class="text-[10px] font-bold uppercase tracking-widest mb-4" style="color: var(--color-text-muted);">Add New Section</p>
              <div class="grid grid-cols-3 gap-3 mb-3">
                <div>
                  <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5" style="color: var(--color-text-muted);">Section Code</label>
                  <input v-model="newSection.code" class="w-full px-4 py-2.5 rounded-xl text-sm font-medium outline-none transition-all"
                         style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                         placeholder="e.g. HEM" maxlength="10"
                         @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                         @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
                </div>
                <div class="col-span-2">
                  <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5" style="color: var(--color-text-muted);">Section Name</label>
                  <input v-model="newSection.name" class="w-full px-4 py-2.5 rounded-xl text-sm font-medium outline-none transition-all"
                         style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                         placeholder="e.g. Hematology"
                         @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                         @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
                </div>
              </div>
              <div class="grid grid-cols-2 gap-3 mb-3">
                <div>
                  <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5" style="color: var(--color-text-muted);">Category</label>
                  <select v-model="newSection.category" class="w-full px-4 py-2.5 rounded-xl text-sm font-medium outline-none"
                          style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);">
                    <option value="1">Phlebo / Send-In</option>
                    <option value="2">Processing</option>
                    <option value="3">Laboratory</option>
                  </select>
                </div>
                <div>
                  <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5" style="color: var(--color-text-muted);">Cut-Off Time</label>
                  <input v-model="newSection.cutOff" type="time" class="w-full px-4 py-2.5 rounded-xl text-sm font-medium outline-none"
                         style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);" />
                </div>
              </div>
              <div class="flex gap-6 mb-4">
                <div class="flex items-center gap-2 cursor-pointer" @click="newSection.includeInAssignRMT = !newSection.includeInAssignRMT">
                  <div class="w-4 h-4 rounded flex items-center justify-center transition-all flex-shrink-0"
                       :style="newSection.includeInAssignRMT ? 'background-color: var(--color-primary);' : 'border: 1.5px solid var(--color-border);'">
                    <span v-if="newSection.includeInAssignRMT" class="material-symbols-outlined text-white" style="font-size: 11px;">check</span>
                  </div>
                  <span class="text-xs font-bold" style="color: var(--color-text-muted);">Include in Assign RMT</span>
                </div>
                <div class="flex items-center gap-2 cursor-pointer" @click="newSection.includeInReceiving = !newSection.includeInReceiving">
                  <div class="w-4 h-4 rounded flex items-center justify-center transition-all flex-shrink-0"
                       :style="newSection.includeInReceiving ? 'background-color: var(--color-primary);' : 'border: 1.5px solid var(--color-border);'">
                    <span v-if="newSection.includeInReceiving" class="material-symbols-outlined text-white" style="font-size: 11px;">check</span>
                  </div>
                  <span class="text-xs font-bold" style="color: var(--color-text-muted);">Include in Section Receiving</span>
                </div>
              </div>
              <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2"
                      style="background: var(--color-primary-gradient); color: #ffffff;" @click="addSection">
                <span class="material-symbols-outlined text-sm">add</span>Add Section
              </button>
            </div>

            <div>
              <p class="text-[10px] font-bold uppercase tracking-widest mb-4" style="color: var(--color-text-muted);">Existing Sections</p>
              <div class="space-y-2">
                <div v-for="sec in settings.sections" :key="sec.code"
                     class="flex items-center gap-4 px-4 py-3 rounded-xl" style="background-color: var(--color-surface-low);">
                  <div class="w-10 h-10 rounded-xl flex items-center justify-center text-xs font-extrabold flex-shrink-0"
                       style="background-color: var(--color-primary-soft); color: var(--color-primary);">
                    {{ sec.code.substring(0, 3) }}
                  </div>
                  <div class="flex-1">
                    <p class="text-sm font-bold" style="color: var(--color-text);">{{ sec.name }}</p>
                    <p class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">
                      {{ sec.code }} · Cut-off {{ sec.cutOff || 'N/A' }}
                    </p>
                  </div>
                  <div class="flex items-center gap-2">
                    <span v-if="sec.includeInAssignRMT" class="px-2 py-1 rounded-lg text-[10px] font-bold"
                          style="background-color: var(--color-primary-soft); color: var(--color-primary);">RMT</span>
                    <span v-if="sec.includeInReceiving" class="px-2 py-1 rounded-lg text-[10px] font-bold"
                          style="background-color: var(--color-primary-soft); color: var(--color-primary);">Receiving</span>
                  </div>
                  <button @click="sec.isActive = !sec.isActive"
                          class="flex items-center gap-2 px-3 py-1.5 rounded-xl text-[10px] font-bold uppercase tracking-widest transition-all"
                          :style="sec.isActive
                            ? 'background-color: var(--color-primary-soft); color: var(--color-primary);'
                            : 'background-color: var(--color-surface); color: var(--color-text-muted); border: 1px solid var(--color-border);'">
                    <span class="material-symbols-outlined text-xs">{{ sec.isActive ? 'check_circle' : 'cancel' }}</span>
                    {{ sec.isActive ? 'Active' : 'Inactive' }}
                  </button>
                </div>
                <p v-if="!settings.sections.length" class="py-8 text-center text-sm" style="color: var(--color-text-muted);">No sections configured yet.</p>
              </div>
            </div>

            <div class="flex justify-end pt-4" style="border-top: 1px solid var(--color-border);">
              <button class="px-6 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow-lg"
                      style="background: var(--color-primary-gradient); color: #ffffff;" @click="save('Section')">
                <span class="material-symbols-outlined text-sm">save</span>Save Changes
              </button>
            </div>
          </div>
        </div>

        <!-- ══════════════════════════════════════════════════════════
       USER MANAGEMENT
  ══════════════════════════════════════════════════════════ -->
        <div v-if="activeTab === 'users'"
             class="rounded-2xl overflow-hidden"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
          <div class="px-8 py-5 flex items-center gap-4" style="border-bottom: 1px solid var(--color-border);">
            <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0" style="background-color: var(--color-primary-soft);">
              <span class="material-symbols-outlined text-base" style="color: var(--color-primary);">manage_accounts</span>
            </div>
            <div>
              <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text);">User Management</h2>
              <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">Manage user accounts, roles, and section access.</p>
            </div>
          </div>
          <div class="px-8 py-6 space-y-6">

            <div class="relative">
              <span class="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-base" style="color: var(--color-text-muted);">search</span>
              <input v-model="userSearch" class="w-full pl-10 pr-4 py-2.5 rounded-xl text-sm font-medium outline-none transition-all"
                     style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                     placeholder="Search by User ID or Name..."
                     @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                     @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
            </div>

            <table class="w-full text-sm">
              <thead>
                <tr style="border-bottom: 1px solid var(--color-border);">
                  <th class="text-left pb-3 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">User ID</th>
                  <th class="text-left pb-3 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Name</th>
                  <th class="text-left pb-3 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Sections</th>
                  <th class="text-left pb-3 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Admin</th>
                  <th class="text-left pb-3 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Theme</th>
                  <th class="pb-3"></th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="user in filteredUsers" :key="user.userID" style="border-bottom: 1px solid var(--color-surface-low);">
                  <td class="py-3 font-bold text-xs" style="color: var(--color-text);">{{ user.userID }}</td>
                  <td class="py-3 font-medium" style="color: var(--color-text);">{{ user.userName }}</td>
                  <td class="py-3">
                    <div class="flex flex-wrap gap-1">
                      <span v-for="sec in user.sections" :key="sec" class="px-2 py-0.5 rounded-lg text-[10px] font-bold"
                            style="background-color: var(--color-primary-soft); color: var(--color-primary);">{{ sec }}</span>
                    </div>
                  </td>
                  <td class="py-3">
                    <span v-if="user.isAdmin" class="material-symbols-outlined text-base" style="color: var(--color-primary);">verified_user</span>
                    <span v-else class="text-[10px] font-bold" style="color: var(--color-text-muted);">—</span>
                  </td>
                  <td class="py-3 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">
                    {{ ['Light', 'Dark', 'Dim'][user.theme] ?? 'Light' }}
                  </td>
                  <td class="py-3">
                    <button class="px-3 py-1.5 rounded-lg text-[10px] font-bold uppercase tracking-widest transition-all active:scale-95"
                            style="background-color: var(--color-surface-low); color: var(--color-text-muted);" @click="openUserProfile(user)">
                      Edit
                    </button>
                  </td>
                </tr>
                <tr v-if="!filteredUsers.length">
                  <td colspan="6" class="py-8 text-center text-sm" style="color: var(--color-text-muted);">No users found.</td>
                </tr>
              </tbody>
            </table>

          </div>
        </div>

        <!-- ══════════════════════════════════════════════════════════
       ENDORSEMENT SET-UP
  ══════════════════════════════════════════════════════════ -->
        <div v-if="activeTab === 'endorsement'"
             class="rounded-2xl overflow-hidden"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
          <div class="px-8 py-5 flex items-center gap-4" style="border-bottom: 1px solid var(--color-border);">
            <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0" style="background-color: var(--color-primary-soft);">
              <span class="material-symbols-outlined text-base" style="color: var(--color-primary);">swap_horiz</span>
            </div>
            <div>
              <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text);">Endorsement Set-Up</h2>
              <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">Configure default and additional endorsed-to sites per branch and section.</p>
            </div>
          </div>
          <div class="px-8 py-6 space-y-8">

            <div>
              <p class="text-[10px] font-bold uppercase tracking-widest mb-2" style="color: var(--color-text-muted);">Default Endorsed-To Site</p>
              <p class="text-xs mb-4" style="color: var(--color-text-muted);">The default site shown when opening the endorsement form. One per branch/section combination.</p>
              <div class="space-y-3">
                <div v-for="(config, idx) in settings.endorsement.defaults" :key="idx"
                     class="grid grid-cols-3 gap-3 p-4 rounded-xl" style="background-color: var(--color-surface-low);">
                  <div>
                    <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5" style="color: var(--color-text-muted);">Branch</label>
                    <input v-model="config.branchCode" class="w-full px-3 py-2 rounded-xl text-sm outline-none"
                           style="background-color: var(--color-surface); color: var(--color-text); border: 1.5px solid var(--color-border);" placeholder="Branch Code" />
                  </div>
                  <div>
                    <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5" style="color: var(--color-text-muted);">Section</label>
                    <input v-model="config.sectionCode" class="w-full px-3 py-2 rounded-xl text-sm outline-none"
                           style="background-color: var(--color-surface); color: var(--color-text); border: 1.5px solid var(--color-border);" placeholder="Section Code" />
                  </div>
                  <div>
                    <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5" style="color: var(--color-text-muted);">Default Endorsed-To</label>
                    <input v-model="config.defaultSite" class="w-full px-3 py-2 rounded-xl text-sm outline-none"
                           style="background-color: var(--color-surface); color: var(--color-text); border: 1.5px solid var(--color-border);" placeholder="Site Code" />
                  </div>
                </div>
              </div>
              <button class="mt-3 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2"
                      style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                      @click="settings.endorsement.defaults.push({ branchCode: '', sectionCode: '', defaultSite: '' })">
                <span class="material-symbols-outlined text-sm">add</span>Add Row
              </button>
            </div>

            <div>
              <p class="text-[10px] font-bold uppercase tracking-widest mb-2" style="color: var(--color-text-muted);">Additional Endorsed-To Sites</p>
              <p class="text-xs mb-4" style="color: var(--color-text-muted);">Sites available in the dropdown. All other sites are hidden unless configured here.</p>
              <div class="space-y-3">
                <div v-for="(config, idx) in settings.endorsement.additional" :key="idx"
                     class="flex gap-3 items-end p-4 rounded-xl" style="background-color: var(--color-surface-low);">
                  <div class="flex-1">
                    <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5" style="color: var(--color-text-muted);">Branch / Section</label>
                    <input v-model="config.branchSection" class="w-full px-3 py-2 rounded-xl text-sm outline-none"
                           style="background-color: var(--color-surface); color: var(--color-text); border: 1.5px solid var(--color-border);" placeholder="e.g. WES / HEM" />
                  </div>
                  <div class="flex-[2]">
                    <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5" style="color: var(--color-text-muted);">Additional Sites (comma-separated)</label>
                    <input v-model="config.sites" class="w-full px-3 py-2 rounded-xl text-sm outline-none"
                           style="background-color: var(--color-surface); color: var(--color-text); border: 1.5px solid var(--color-border);" placeholder="e.g. MAIN, NAGA, TABUNOK" />
                  </div>
                  <button class="w-9 h-9 rounded-xl flex items-center justify-center transition-all active:scale-95 flex-shrink-0"
                          style="background-color: var(--color-error-soft); color: var(--color-error);"
                          @click="settings.endorsement.additional.splice(idx, 1)">
                    <span class="material-symbols-outlined text-sm">close</span>
                  </button>
                </div>
              </div>
              <button class="mt-3 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2"
                      style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                      @click="settings.endorsement.additional.push({ branchSection: '', sites: '' })">
                <span class="material-symbols-outlined text-sm">add</span>Add Row
              </button>
            </div>

            <div class="flex justify-end pt-4" style="border-top: 1px solid var(--color-border);">
              <button class="px-6 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow-lg"
                      style="background: var(--color-primary-gradient); color: #ffffff;" @click="save('Endorsement')">
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
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
          <div class="px-8 py-5 flex items-center gap-4" style="border-bottom: 1px solid var(--color-border);">
            <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0" style="background-color: var(--color-primary-soft);">
              <span class="material-symbols-outlined text-base" style="color: var(--color-primary);">archive</span>
            </div>
            <div>
              <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text);">Archive Settings</h2>
              <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">Set data retention period. Older batches will be archived and accessible only through the archive viewer.</p>
            </div>
          </div>
          <div class="px-8 py-6 space-y-6">

            <div>
              <p class="text-[10px] font-bold uppercase tracking-widest mb-4" style="color: var(--color-text-muted);">Data Retention</p>
              <div class="space-y-4">
                <div class="flex items-center justify-between gap-6">
                  <div>
                    <p class="text-sm font-bold" style="color: var(--color-text);">Active Batch Retention Period</p>
                    <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">Batches older than this will be moved to archive</p>
                  </div>
                  <div class="flex items-center gap-3">
                    <input v-model.number="settings.archive.retentionDays" type="number" min="7" max="365"
                           class="w-24 px-4 py-2.5 rounded-xl text-sm font-bold text-center outline-none transition-all"
                           style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                           @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                           @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
                    <span class="text-sm font-bold" style="color: var(--color-text-muted);">days</span>
                  </div>
                </div>
                <div class="flex items-center justify-between gap-6">
                  <div>
                    <p class="text-sm font-bold" style="color: var(--color-text);">Allow Archive Review</p>
                    <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">Users with appropriate rights can browse archived batches</p>
                  </div>
                  <button @click="settings.archive.allowReview = !settings.archive.allowReview"
                          class="w-12 h-6 rounded-full transition-all relative flex-shrink-0"
                          :style="settings.archive.allowReview ? 'background-color: var(--color-primary);' : 'background-color: var(--color-border);'">
                    <span class="absolute top-0.5 w-5 h-5 rounded-full bg-white shadow transition-all"
                          :style="settings.archive.allowReview ? 'left: calc(100% - 1.375rem);' : 'left: 0.125rem;'"></span>
                  </button>
                </div>
              </div>
            </div>

            <div class="px-5 py-4 rounded-xl flex items-start gap-3" style="background-color: #fffbeb; border: 1px solid #f59e0b;">
              <span class="material-symbols-outlined text-base mt-0.5" style="color: #f59e0b;">warning</span>
              <p class="text-xs font-medium" style="color: var(--color-text-muted);">
                Archived data is stored and can be retrieved, but will not appear in standard batch views or reports. Ensure the retention period aligns with your compliance requirements.
              </p>
            </div>

            <div class="flex justify-end pt-4" style="border-top: 1px solid var(--color-border);">
              <button class="px-6 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow-lg"
                      style="background: var(--color-primary-gradient); color: #ffffff;" @click="save('Archive')">
                <span class="material-symbols-outlined text-sm">save</span>Save Changes
              </button>
            </div>
          </div>
        </div>

        <!-- ══════════════════════════════════════════════════════════
       PROCESSING OPTIONS
  ══════════════════════════════════════════════════════════ -->
        <div v-if="activeTab === 'processing'"
             class="rounded-2xl overflow-hidden"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
          <div class="px-8 py-5 flex items-center gap-4" style="border-bottom: 1px solid var(--color-border);">
            <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0" style="background-color: var(--color-primary-soft);">
              <span class="material-symbols-outlined text-base" style="color: var(--color-primary);">tune</span>
            </div>
            <div>
              <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text);">Processing Options</h2>
              <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">Toggle optional fields shown during specimen receiving in Processing.</p>
            </div>
          </div>
          <div class="px-8 py-6 space-y-8">

            <div>
              <p class="text-[10px] font-bold uppercase tracking-widest mb-4" style="color: var(--color-text-muted);">Receive by Specimen — Optional Fields</p>
              <div class="space-y-4">
                <div v-for="field in processingFields" :key="field.key" class="flex items-center justify-between gap-6">
                  <div>
                    <p class="text-sm font-bold" style="color: var(--color-text);">{{ field.label }}</p>
                    <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">{{ field.hint }}</p>
                  </div>
                  <button @click="settings.processing[field.key] = !settings.processing[field.key]"
                          class="w-12 h-6 rounded-full transition-all relative flex-shrink-0"
                          :style="settings.processing[field.key] ? 'background-color: var(--color-primary);' : 'background-color: var(--color-border);'">
                    <span class="absolute top-0.5 w-5 h-5 rounded-full bg-white shadow transition-all"
                          :style="settings.processing[field.key] ? 'left: calc(100% - 1.375rem);' : 'left: 0.125rem;'"></span>
                  </button>
                </div>
              </div>
            </div>

            <div class="flex justify-end pt-4" style="border-top: 1px solid var(--color-border);">
              <button class="px-6 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow-lg"
                      style="background: var(--color-primary-gradient); color: #ffffff;" @click="save('Processing')">
                <span class="material-symbols-outlined text-sm">save</span>Save Changes
              </button>
            </div>
          </div>
        </div>

      </div>
    </div>

    <!-- Toast Notification -->
    <Transition name="toast">
      <div v-if="toast.visible"
           class="fixed bottom-6 right-6 flex items-center gap-3 px-5 py-4 rounded-2xl shadow-xl z-50"
           style="background-color: var(--color-surface); border: 1px solid var(--color-border);">
        <span class="material-symbols-outlined text-base" style="color: var(--color-primary);">check_circle</span>
        <p class="text-sm font-bold" style="color: var(--color-text);">{{ toast.message }}</p>
      </div>
    </Transition>

  </AppLayout>
</template>

<script setup>
  import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
  import { pcApi } from '@/api/pcApi'
  import { settingsApi } from '@/api/settingsApi'
  import AppLayout from '@/components/layout/AppLayout.vue'

  // ── Tabs ───────────────────────────────────────────────────────────────────

  const activeTab = ref('pc')

  const settingsTabs = [
    { key: 'pc', label: 'PC Registration', icon: 'computer' },
    { key: 'tat', label: 'TAT Set-Up', icon: 'timer' },
    { key: 'branch', label: 'Branch', icon: 'location_city' },
    { key: 'section', label: 'Section', icon: 'apartment' },
    { key: 'users', label: 'Users', icon: 'manage_accounts' },
    { key: 'endorsement', label: 'Endorsement', icon: 'swap_horiz' },
    { key: 'archive', label: 'Archive', icon: 'archive' },
    { key: 'processing', label: 'Processing', icon: 'tune' },
  ]

  const processingFields = [
    { key: 'showTemperature', label: 'Temperature Field', hint: 'Show temperature input during receiving' },
    { key: 'showTempRemarks', label: 'Temperature Remarks', hint: 'Show temperature remarks field' },
    { key: 'showBagNo', label: 'Bag Number', hint: 'Show bag number field during receiving' },
  ]

  onMounted(() => {
  loadPCs()
  loadSections()
})

  // ── Settings State ─────────────────────────────────────────────────────────

  const settings = ref({
    tat: {
      batchEndorsement: { hours: 2, minutes: 0 },
      batchCompletion: { hours: 4, minutes: 0 },
      specimenReceipt: { hours: 1, minutes: 30 },
    },
    branches: [
      { code: 'WES', name: 'West Branch', isActive: true },
      { code: 'NAGA', name: 'Naga Branch', isActive: true },
      { code: 'TABUNOK', name: 'Tabunok Branch', isActive: true },
      { code: 'LILOAN', name: 'Liloan Branch', isActive: false },
      { code: 'DIAMOND', name: 'Diamond Branch', isActive: true },
      { code: 'MACTAN', name: 'Mactan Branch', isActive: true },
      { code: 'CONSOLACION', name: 'Consolacion Branch', isActive: true },
    ],
    sections: [
      { code: 'HEM', name: 'Hematology', category: '3', cutOff: '18:00', isActive: true, includeInAssignRMT: true, includeInReceiving: true },
      { code: 'CHE', name: 'Chemistry', category: '3', cutOff: '17:30', isActive: true, includeInAssignRMT: true, includeInReceiving: true },
      { code: 'URI', name: 'Urinalysis', category: '3', cutOff: '18:00', isActive: true, includeInAssignRMT: false, includeInReceiving: true },
      { code: 'MIC', name: 'Microbiology', category: '3', cutOff: '16:00', isActive: true, includeInAssignRMT: true, includeInReceiving: true },
      { code: 'PRO', name: 'Processing', category: '2', cutOff: '', isActive: true, includeInAssignRMT: false, includeInReceiving: false },
      { code: 'END', name: 'Send-In / Phlebotomy', category: '1', cutOff: '', isActive: true, includeInAssignRMT: false, includeInReceiving: false },
    ],
    endorsement: {
      defaults: [{ branchCode: 'WES', sectionCode: 'HEM', defaultSite: 'MAIN' }],
      additional: [{ branchSection: 'WES / HEM', sites: 'NAGA, TABUNOK' }],
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
  })

  // ── Branch ─────────────────────────────────────────────────────────────────

  const newBranch = ref({ code: '', name: '' })

  function addBranch() {
    if (!newBranch.value.code.trim() || !newBranch.value.name.trim()) return
    settings.value.branches.push({
      code: newBranch.value.code.trim().toUpperCase(),
      name: newBranch.value.name.trim(),
      isActive: true,
    })
    newBranch.value = { code: '', name: '' }
  }

  // ── Section ────────────────────────────────────────────────────────────────

  const newSection = ref({ code: '', name: '', category: '3', cutOff: '', includeInAssignRMT: false, includeInReceiving: false })

  function addSection() {
    if (!newSection.value.code.trim() || !newSection.value.name.trim()) return
    settings.value.sections.push({
      ...newSection.value,
      code: newSection.value.code.trim().toUpperCase(),
      name: newSection.value.name.trim(),
      isActive: true,
    })
    newSection.value = { code: '', name: '', category: '3', cutOff: '', includeInAssignRMT: false, includeInReceiving: false }
  }

  // ── Users ──────────────────────────────────────────────────────────────────

  const userSearch = ref('')

  const users = ref([
    { userID: 'U001', userName: 'Juan Dela Cruz', sections: ['HEM', 'CHE'], isAdmin: false, theme: 0 },
    { userID: 'U002', userName: 'Maria Santos', sections: ['URI'], isAdmin: false, theme: 1 },
    { userID: 'U003', userName: 'Admin User', sections: [], isAdmin: true, theme: 0 },
    { userID: 'U004', userName: 'Pedro Reyes', sections: ['MIC', 'HEM'], isAdmin: false, theme: 2 },
  ])

  const filteredUsers = computed(() => {
    const q = userSearch.value.toLowerCase()
    if (!q) return users.value
    return users.value.filter(u =>
      u.userID.toLowerCase().includes(q) || u.userName.toLowerCase().includes(q)
    )
  })

  function openUserProfile(user) {
    // TODO: open user edit modal / drawer
    console.log('Edit user:', user)
  }

  // ── Toast ──────────────────────────────────────────────────────────────────

  const toast = ref({ visible: false, message: '' })

  function save(section) {
    toast.value = { visible: true, message: `${section} settings saved.` }
    setTimeout(() => { toast.value.visible = false }, 2500)
    // TODO: call API endpoint
  }

  function showToast(msg) {
    toast.value = { visible: true, message: msg }
    setTimeout(() => { toast.value.visible = false }, 2500)
    // TODO: call API endpoint
  }

  // ── PC Registration state ────────────────────────────────────────────
  const pcList = ref([])
  const pcLoading = ref(false)

  const availableSections = ref([])   // all Section_Master for the branch

  const pcModal = ref({
    visible: false,
    mode: 'add',          // 'add' | 'editSections'
    pcId: null,
    form: { ipAddress: '', description: '', sectionCodes: [] },
    saving: false,
    error: ''
  })

  async function loadPCs() {
    pcLoading.value = true
    try {
      const res = await pcApi.getAll()
      pcList.value = res.data
    } catch {
      pcList.value = []
    } finally {
      pcLoading.value = false
    }
  }

  async function loadSections() {
    try {
      // Reuse your existing settingsApi or sectionApi — adjust as needed
      const res = await settingsApi.getSections()
      availableSections.value = res.data.filter(s => s.active)
    } catch {
      availableSections.value = []
    }
  }

  // Called when 'pc' tab is activated — load data lazily
  watch(activeTab, (val) => {
    if (val === 'pc' && !pcList.value.length) {
      loadPCs()
      loadSections()
    }
  })

  function openAddPC() {
    pcModal.value = {
      visible: true, mode: 'add', pcId: null,
      form: { ipAddress: '', description: '', sectionCodes: [] },
      saving: false, error: ''
    }
  }

  function openEditSections(pc) {
    pcModal.value = {
      visible: true, mode: 'editSections', pcId: pc.id,
      form: {
        ipAddress: pc.ipAddress,
        description: pc.description,
        sectionCodes: pc.sections.map(s => s.sectionCode)
      },
      saving: false, error: ''
    }
  }

  function closePCModal() {
    pcModal.value.visible = false
  }

  function toggleSectionInModal(code) {
    const idx = pcModal.value.form.sectionCodes.indexOf(code)
    if (idx === -1) pcModal.value.form.sectionCodes.push(code)
    else pcModal.value.form.sectionCodes.splice(idx, 1)
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
        await pcApi.add({
          ipAddress: pcModal.value.form.ipAddress.trim(),
          description: pcModal.value.form.description.trim(),
          sectionCodes: pcModal.value.form.sectionCodes
        })
      } else {
        await pcApi.updateSections(pcModal.value.pcId, {
          sectionCodes: pcModal.value.form.sectionCodes
        })
      }
      closePCModal()

      await loadPCs()
      showToast(pcModal.value.mode === 'add'
        ? 'PC registered successfully.'
        : 'Section assignments updated.')

    } catch (err) {
      pcModal.value.error = err.response?.data?.message || 'An error occurred.'
    } finally {
      pcModal.value.saving = false
    }
  }

  async function togglePC(pc) {
    try {
      await pcApi.toggle(pc.id)
      pc.active = !pc.active
    } catch (err) {
      showToast(err.response?.data?.message || 'Failed to update status.')
    }
  }
</script>

<style scoped>
  .toast-enter-active,
  .toast-leave-active {
    transition: opacity 0.25s ease, transform 0.25s ease;
  }

  .toast-enter-from,
  .toast-leave-to {
    opacity: 0;
    transform: translateY(8px);
  }
</style>
