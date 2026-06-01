<template>
  <div class="rounded-2xl overflow-hidden"
       style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)">
    <!-- Header -->
    <div class="px-8 py-5 flex items-center justify-between"
         style="border-bottom: 1px solid var(--color-border)">
      <div class="flex items-center gap-4">
        <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
             style="background-color: var(--color-primary-soft)">
          <span class="material-symbols-outlined text-base" style="color: var(--color-primary)">apartment</span>
        </div>
        <div>
          <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text)">Section Management</h2>
          <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">Manage sections by branch. One Receiver is allowed per branch.</p>
        </div>
      </div>
      <button class="flex items-center gap-2 px-4 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 shadow"
              style="background: var(--color-primary-gradient); color: #ffffff"
              @click="openAddSection">
        <span class="material-symbols-outlined text-sm">add</span>Add Section
      </button>
    </div>

    <div class="px-8 py-6 space-y-5">
      <!-- Branch filter + search -->
      <div class="flex items-center gap-3">
        <div class="flex gap-1.5 flex-wrap">
          <button class="px-3 py-1.5 rounded-lg text-[10px] font-bold uppercase tracking-widest transition-all"
                  :style="sectionBranchFilter === 'ALL'
                    ? 'background-color: var(--color-primary); color: #ffffff;'
                    : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'"
                  @click="sectionBranchFilter = 'ALL'">
            All
          </button>
          <button v-for="b in availableBranches" :key="b.code"
                  class="px-3 py-1.5 rounded-lg text-[10px] font-bold uppercase tracking-widest transition-all"
                  :style="sectionBranchFilter === b.code
                    ? 'background-color: var(--color-primary); color: #ffffff;'
                    : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'"
                  @click="sectionBranchFilter = b.code">
            {{ b.code }}
          </button>
        </div>
        <div class="relative flex-1 min-w-0">
          <span class="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-sm"
                style="color: var(--color-text-muted)">search</span>
          <input v-model="sectionSearch"
                 class="w-full pl-9 pr-4 py-2 rounded-xl text-sm font-medium outline-none transition-all"
                 style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                 placeholder="Search by code or name..."
                 @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                 @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
        </div>
      </div>

      <!-- Loading -->
      <div v-if="sectionsLoading" class="flex items-center justify-center py-12 gap-3" style="color: var(--color-text-muted)">
        <span class="material-symbols-outlined text-xl animate-spin">progress_activity</span>
        <span class="text-sm font-medium">Loading sections...</span>
      </div>

      <!-- Grouped by category -->
      <template v-else>
        <div v-for="cat in sectionCategories" :key="cat.id" class="space-y-2">
          <div class="flex items-center gap-2 pt-2">
            <div class="w-6 h-6 rounded-lg flex items-center justify-center" :style="`background-color: ${cat.softColor};`">
              <span class="material-symbols-outlined text-xs" :style="`color: ${cat.color};`">{{ cat.icon }}</span>
            </div>
            <span class="text-[10px] font-bold uppercase tracking-widest" :style="`color: ${cat.color};`">{{ cat.label }}</span>
            <span class="text-[10px] font-bold" style="color: var(--color-text-muted)">({{ filteredSectionsByCategory(cat.id).length }})</span>
          </div>

          <div v-if="!filteredSectionsByCategory(cat.id).length"
               class="px-4 py-3 rounded-xl text-xs text-center"
               style="background-color: var(--color-surface-low); color: var(--color-text-muted)">
            No {{ cat.label.toLowerCase() }} sections found.
          </div>

          <div v-else class="rounded-xl overflow-hidden" style="border: 1px solid var(--color-border)">
            <table class="w-full text-sm">
              <thead>
                <tr style="border-bottom: 1px solid var(--color-border)">
                  <th v-for="col in sectionTableHeaders(cat.id)" :key="col"
                      class="text-left px-4 py-2.5 text-[10px] font-bold uppercase tracking-widest"
                      style="color: var(--color-text-muted); background-color: var(--color-surface-low);">{{ col }}</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="sec in filteredSectionsByCategory(cat.id)" :key="sec.code"
                    style="border-bottom: 1px solid var(--color-surface-low)">
                  <td class="px-4 py-3">
                    <span class="font-mono font-bold text-xs" style="color: var(--color-text)">{{ sec.code }}</span>
                  </td>
                  <td class="px-4 py-3">
                    <span class="font-medium" style="color: var(--color-text)">{{ sec.name }}</span>
                  </td>
                  <td class="px-4 py-3">
                    <span class="px-2 py-0.5 rounded-lg text-[10px] font-bold"
                          style="background-color: var(--color-surface-low); color: var(--color-text-muted);">{{ sec.branchCode }}</span>
                  </td>
                  <!-- Auto No — endorser only -->
                  <td v-if="cat.id === '1'" class="px-4 py-3">
                    <span class="text-sm font-mono" style="color: var(--color-text-muted)">{{ sec.autoNo || "—" }}</span>
                  </td>
                  <!-- Cut-off — lab only -->
                  <td v-if="cat.id === '3'" class="px-4 py-3">
                    <span v-if="sec.cutOffTime"
                          class="flex items-center gap-1 text-xs font-bold font-mono"
                          style="color: var(--color-text)">
                      <span class="material-symbols-outlined text-sm" style="color: var(--color-primary)">schedule</span>
                      {{ sec.cutOffTime }}
                    </span>
                    <span v-else class="text-xs" style="color: var(--color-text-muted)">—</span>
                  </td>
                  <td class="px-4 py-3">
                    <button class="flex items-center gap-1.5 px-3 py-1 rounded-lg text-[10px] font-bold uppercase tracking-widest transition-all"
                            :style="sec.active
                              ? 'background-color: var(--color-primary-soft); color: var(--color-primary);'
                              : 'background-color: var(--color-surface-low); color: var(--color-text-muted); border: 1px solid var(--color-border);'"
                            @click="toggleSection(sec)">
                      <span class="material-symbols-outlined text-xs">{{ sec.active ? "check_circle" : "cancel" }}</span>
                      {{ sec.active ? "Active" : "Inactive" }}
                    </button>
                  </td>
                  <td class="px-4 py-3">
                    <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                            style="color: var(--color-text-muted)"
                            @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'var(--color-surface-low)')"
                            @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                            @click="openEditSection(sec)">
                      <span class="material-symbols-outlined text-sm">edit</span>
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <div v-if="!filteredSections.length" class="py-10 flex flex-col items-center gap-2" style="color: var(--color-text-muted)">
          <span class="material-symbols-outlined text-3xl">apartment</span>
          <p class="text-sm font-medium">No sections found.</p>
        </div>
      </template>
    </div>
  </div>

  <!-- Add/Edit Section Modal -->
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="sectionModal.visible"
           class="fixed inset-0 z-50 flex items-center justify-center"
           style="background-color: rgba(0,0,0,0.4)"
           @click.self="closeSectionModal">
        <div class="w-full max-w-md rounded-2xl overflow-hidden flex flex-col"
             style="background-color: var(--color-surface); box-shadow: 0 8px 32px rgba(0,0,0,0.24); max-height: 90vh;">
          <!-- Header -->
          <div class="px-6 py-4 flex items-center justify-between flex-shrink-0"
               style="border-bottom: 1px solid var(--color-border)">
            <div class="flex items-center gap-3">
              <div class="w-8 h-8 rounded-lg flex items-center justify-center" style="background-color: var(--color-primary-soft)">
                <span class="material-symbols-outlined text-sm" style="color: var(--color-primary)">apartment</span>
              </div>
              <h3 class="text-sm font-extrabold tracking-tight" style="color: var(--color-text)">
                {{ sectionModal.mode === "add" ? "Add New Section" : "Edit Section" }}
              </h3>
            </div>
            <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                    style="color: var(--color-text-muted)"
                    @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'var(--color-surface-low)')"
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click="closeSectionModal">
              <span class="material-symbols-outlined text-base">close</span>
            </button>
          </div>

          <!-- Body -->
          <div class="px-6 py-5 space-y-4 overflow-y-auto flex-1">
            <!-- Code (add only) -->
            <div v-if="sectionModal.mode === 'add'">
              <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5" style="color: var(--color-text-muted)">Section Code *</label>
              <div class="relative">
                <input v-model="sectionModal.form.code"
                       placeholder="e.g. HEM"
                       maxlength="20"
                       class="w-full px-4 py-2.5 pr-10 rounded-xl text-sm font-mono font-bold uppercase outline-none transition-all"
                       :style="`background-color: var(--color-surface-low); color: var(--color-text);
                         border: 1.5px solid ${sectionModal.codeStatus === 'taken' ? '#ba1a1a' : sectionModal.codeStatus === 'available' ? '#4caf50' : 'var(--color-border)'};`"
                       @input="onCodeInput"
                       @focus="(e) => { if (sectionModal.codeStatus === 'idle') e.target.style.borderColor = 'var(--color-primary)'; }"
                       @blur="(e) => { if (sectionModal.codeStatus === 'idle') e.target.style.borderColor = 'var(--color-border)'; }" />
                <div class="absolute right-3 top-1/2 -translate-y-1/2">
                  <span v-if="sectionModal.codeChecking" class="material-symbols-outlined text-sm animate-spin" style="color: var(--color-text-muted)">progress_activity</span>
                  <span v-else-if="sectionModal.codeStatus === 'available'" class="material-symbols-outlined text-sm" style="color: #4caf50">check_circle</span>
                  <span v-else-if="sectionModal.codeStatus === 'taken'" class="material-symbols-outlined text-sm" style="color: #ba1a1a">cancel</span>
                </div>
              </div>
              <p v-if="sectionModal.codeStatus === 'taken'" class="mt-1 text-xs font-bold" style="color: #ba1a1a">This section code already exists.</p>
              <p v-else-if="sectionModal.codeStatus === 'available'" class="mt-1 text-xs font-bold" style="color: #4caf50">Code is available.</p>
            </div>

            <!-- Code read-only (edit) -->
            <div v-else>
              <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5" style="color: var(--color-text-muted)">Section Code</label>
              <div class="px-4 py-2.5 rounded-xl text-sm font-mono font-bold"
                   style="background-color: var(--color-surface-low); color: var(--color-text-muted);">{{ sectionModal.form.code }}</div>
            </div>

            <!-- Name -->
            <div>
              <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5" style="color: var(--color-text-muted)">Section Name *</label>
              <input v-model="sectionModal.form.name"
                     placeholder="e.g. Hematology"
                     class="w-full px-4 py-2.5 rounded-xl text-sm font-medium outline-none transition-all"
                     style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                     @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                     @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
            </div>

            <!-- Branch (add only) -->
            <div v-if="sectionModal.mode === 'add'">
              <label class="block text-[10px] font-bold uppercase tracking-widest mb-2" style="color: var(--color-text-muted)">Branch *</label>
              <div class="rounded-xl overflow-hidden" style="border: 1.5px solid var(--color-border)">
                <div v-for="b in availableBranches" :key="b.code"
                     class="flex items-center justify-between px-4 py-2.5 cursor-pointer transition-all"
                     style="border-bottom: 1px solid var(--color-surface-low)"
                     :style="sectionModal.form.branchCode === b.code ? 'background-color: var(--color-primary-soft);' : ''"
                     @click="sectionModal.form.branchCode = b.code">
                  <span class="text-sm font-medium" style="color: var(--color-text)">{{ b.name || b.code }}</span>
                  <div class="w-5 h-5 rounded-full flex items-center justify-center"
                       :style="sectionModal.form.branchCode === b.code ? 'background-color: var(--color-primary);' : 'border: 1.5px solid var(--color-border);'">
                    <span v-if="sectionModal.form.branchCode === b.code" class="material-symbols-outlined text-xs" style="color: #ffffff">check</span>
                  </div>
                </div>
              </div>
            </div>

            <!-- Category (add only) -->
            <div v-if="sectionModal.mode === 'add'">
              <label class="block text-[10px] font-bold uppercase tracking-widest mb-2" style="color: var(--color-text-muted)">Category *</label>
              <div class="grid grid-cols-3 gap-2">
                <button v-for="cat in sectionCategories" :key="cat.id"
                        class="flex flex-col items-center gap-1.5 py-3 px-2 rounded-xl text-xs font-bold transition-all"
                        :style="sectionModal.form.category === cat.id
                          ? `background-color: ${cat.softColor}; color: ${cat.color}; border: 2px solid ${cat.color};`
                          : 'background-color: var(--color-surface-low); color: var(--color-text-muted); border: 2px solid transparent;'"
                        @click="sectionModal.form.category = cat.id">
                  <span class="material-symbols-outlined text-base">{{ cat.icon }}</span>
                  {{ cat.label }}
                </button>
              </div>
              <!-- Receiver warning -->
              <div v-if="sectionModal.form.category === '2' && sectionModal.form.branchCode && receiverExistsForBranch(sectionModal.form.branchCode)"
                   class="mt-2 flex items-start gap-2 px-3 py-2.5 rounded-xl"
                   style="background-color: rgba(186,26,26,0.08); border: 1px solid rgba(186,26,26,0.2);">
                <span class="material-symbols-outlined text-sm flex-shrink-0" style="color: #ba1a1a">warning</span>
                <p class="text-xs font-bold" style="color: #ba1a1a">A Receiver already exists for this branch. Only one Receiver is allowed per branch.</p>
              </div>
            </div>

            <!-- Auto No (endorser only) -->
            <div v-if="sectionModal.form.category === '1'">
              <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5" style="color: var(--color-text-muted)">Auto Number</label>
              <input v-model.number="sectionModal.form.autoNo"
                     type="number" min="0" placeholder="0"
                     class="w-full px-4 py-2.5 rounded-xl text-sm font-mono font-medium outline-none transition-all"
                     style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                     @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                     @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')"
                     disabled />
              <p class="mt-1 text-xs" style="color: var(--color-text-muted)">Used for batch number auto-incrementing.</p>
            </div>

            <!-- Test Groups (laboratory only) -->
            <div v-if="sectionModal.form.category === '3'">
              <div class="flex items-center justify-between mb-2">
                <label class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted)">Test Groups *</label>
                <span class="text-[10px] font-bold" style="color: var(--color-text-muted)">{{ sectionModal.form.testGroupCodes.length }} selected</span>
              </div>
              <p v-if="sectionModal.testGroupError" class="mb-2 text-xs font-bold" style="color: #ba1a1a">At least one test group must be assigned.</p>
              <div v-if="testGroupsLoading" class="flex items-center gap-2 px-4 py-3 rounded-xl"
                   style="background-color: var(--color-surface-low); color: var(--color-text-muted);">
                <span class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
                <span class="text-xs">Loading test groups...</span>
              </div>
              <div v-else class="rounded-xl overflow-hidden" style="border: 1.5px solid var(--color-border)">
                <div v-for="tg in allTestGroups" :key="tg.code">
                  <!-- Disabled (assigned elsewhere) -->
                  <div v-if="tg.assignedSectionCode && tg.assignedSectionCode !== sectionModal.originalCode"
                       class="flex items-center gap-3 px-4 py-2.5 cursor-not-allowed"
                       style="border-bottom: 1px solid var(--color-surface-low); opacity: 0.45">
                    <div class="w-5 h-5 rounded-md flex-shrink-0" style="border: 1.5px solid var(--color-border)"></div>
                    <div class="flex-1 min-w-0">
                      <span class="text-sm font-medium" style="color: var(--color-text)">{{ tg.name }}</span>
                      <span class="ml-2 text-[10px] font-bold" style="color: var(--color-text-muted)">— assigned to {{ tg.assignedSectionCode }}</span>
                    </div>
                    <span class="material-symbols-outlined text-xs" style="color: var(--color-text-muted)">lock</span>
                  </div>
                  <!-- Selectable -->
                  <div v-else
                       class="flex items-center gap-3 px-4 py-2.5 cursor-pointer transition-all"
                       style="border-bottom: 1px solid var(--color-surface-low)"
                       :style="sectionModal.form.testGroupCodes.includes(tg.code) ? 'background-color: var(--color-primary-soft);' : ''"
                       @click="toggleTestGroup(tg.code)">
                    <div class="w-5 h-5 rounded-md flex items-center justify-center transition-all flex-shrink-0"
                         :style="sectionModal.form.testGroupCodes.includes(tg.code)
                           ? 'background-color: var(--color-primary);'
                           : 'border: 1.5px solid var(--color-border);'">
                      <span v-if="sectionModal.form.testGroupCodes.includes(tg.code)"
                            class="material-symbols-outlined text-xs" style="color: #ffffff">check</span>
                    </div>
                    <span class="text-sm font-medium flex-1" style="color: var(--color-text)">{{ tg.name }}</span>
                    <span class="font-mono text-[10px] font-bold" style="color: var(--color-text-muted)">{{ tg.code }}</span>
                  </div>
                </div>
                <p v-if="!allTestGroups.length && !testGroupsLoading" class="px-4 py-3 text-xs text-center" style="color: var(--color-text-muted)">
                  No test groups available.
                </p>
              </div>
            </div>

            <!-- ── Cut-Off Time (lab sections, edit mode only) ────────────────── -->
            <div v-if="sectionModal.form.category === '3' && sectionModal.mode === 'edit'">
              <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5" style="color: var(--color-text-muted)">
                Cut-Off Time
              </label>
              <div class="flex items-center gap-3">
                <input v-model="sectionModal.form.cutOffTime"
                       type="time"
                       class="px-4 py-2.5 rounded-xl text-sm font-mono font-bold outline-none transition-all"
                       style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border); min-width: 140px;"
                       @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                       @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
                <button v-if="sectionModal.form.cutOffTime"
                        class="flex items-center gap-1.5 px-3 py-2 rounded-xl text-[10px] font-bold uppercase tracking-widest transition-all"
                        style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                        @click="sectionModal.form.cutOffTime = ''">
                  <span class="material-symbols-outlined text-xs">close</span>
                  Clear
                </button>
              </div>
              <p class="mt-1.5 text-xs" style="color: var(--color-text-muted)">
                Specimens scanned at or after this time will have all <strong>NOW</strong> tests automatically set to <strong>END</strong> (Endorsed Next Day).
                Leave blank to disable cut-off for this section.
              </p>
              <!-- Active indicator -->
              <div v-if="sectionModal.form.cutOffTime"
                   class="mt-2 flex items-center gap-2 px-3 py-2 rounded-xl"
                   style="background-color: rgba(70,21,153,0.06); border: 1px solid rgba(70,21,153,0.15);">
                <span class="material-symbols-outlined text-sm" style="color: var(--color-primary)">schedule</span>
                <span class="text-xs font-bold" style="color: var(--color-primary)">
                  Cut-off active at {{ sectionModal.form.cutOffTime }}
                </span>
              </div>
            </div>

            <!-- ── Auto Receive & Run (lab sections, edit mode only) ────────────── -->
            <div v-if="sectionModal.form.category === '3' && sectionModal.mode === 'edit'">
              <div class="flex items-center justify-between px-4 py-3 rounded-xl cursor-pointer transition-all"
                   :style="sectionModal.form.autoRun
                     ? 'background-color: rgba(46,125,79,0.08); border: 1px solid rgba(46,125,79,0.2);'
                     : 'background-color: var(--color-surface-low); border: 1px solid var(--color-border);'"
                   @click="sectionModal.form.autoRun = !sectionModal.form.autoRun">
                <div class="flex items-center gap-3">
                  <div class="w-8 h-8 rounded-lg flex items-center justify-center flex-shrink-0"
                       :style="sectionModal.form.autoRun
                         ? 'background-color: rgba(46,125,79,0.15);'
                         : 'background-color: var(--color-surface);'">
                    <span class="material-symbols-outlined text-sm"
                          :style="sectionModal.form.autoRun ? 'color: #2e7d4f;' : 'color: var(--color-text-muted);'">
                      bolt
                    </span>
                  </div>
                  <div>
                    <p class="text-xs font-bold" style="color: var(--color-text)">Auto Receive & Run</p>
                    <p class="text-[11px] mt-0.5" style="color: var(--color-text-muted)">
                      Specimens routed to this section will be automatically received and set to running.
                    </p>
                  </div>
                </div>
                <!-- Toggle pill -->
                <div class="relative flex-shrink-0 w-10 h-6 rounded-full transition-all ml-3"
                     :style="sectionModal.form.autoRun
                       ? 'background-color: #2e7d4f;'
                       : 'background-color: var(--color-border);'">
                  <div class="absolute top-0.5 w-5 h-5 rounded-full bg-white shadow transition-all"
                       :style="sectionModal.form.autoRun ? 'left: 20px;' : 'left: 2px;'"></div>
                </div>
              </div>
            </div>

            <p v-if="sectionModal.error" class="text-xs font-bold" style="color: #ba1a1a">{{ sectionModal.error }}</p>
          </div>

          <!-- Footer -->
          <div class="px-6 py-4 flex justify-end gap-3 flex-shrink-0" style="border-top: 1px solid var(--color-border)">
            <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                    style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                    @click="closeSectionModal">
              Cancel
            </button>
            <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow"
                    style="background: var(--color-primary-gradient); color: #ffffff"
                    :disabled="sectionModal.saving || sectionModal.codeStatus === 'taken' || sectionModal.codeChecking"
                    @click="saveSectionModal">
              <span v-if="sectionModal.saving" class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
              <span v-else class="material-symbols-outlined text-sm">save</span>
              {{ sectionModal.mode === "add" ? "Create Section" : "Save Changes" }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
  import { ref, computed, onMounted } from "vue";
  import { sectionApi } from "@/api/sectionApi";
  import { settingsApi } from "@/api/settingsApi";

  const emit = defineEmits(["toast"]);

  // ── Constants ──────────────────────────────────────────────────────────────
  const sectionCategories = [
    { id: "1", label: "Endorser", icon: "outbox", color: "#2e7d9a", softColor: "rgba(46,125,154,0.1)" },
    { id: "2", label: "Receiver", icon: "move_to_inbox", color: "#6a4c93", softColor: "rgba(106,76,147,0.1)" },
    { id: "3", label: "Laboratory", icon: "science", color: "#2e7d4f", softColor: "rgba(46,125,79,0.1)" },
  ];

  // Column headers per category — lab sections get cut-off column instead of auto-no
  function sectionTableHeaders(catId) {
    if (catId === "1") return ["Code", "Name", "Branch", "Auto No.", "Status", ""];
    if (catId === "3") return ["Code", "Name", "Branch", "Cut-Off", "Status", ""];
    return ["Code", "Name", "Branch", "Status", ""];
  }

  // ── State ──────────────────────────────────────────────────────────────────
  const sectionsList = ref([]);
  const sectionsLoading = ref(false);
  const sectionSearch = ref("");
  const sectionBranchFilter = ref("ALL");
  const availableBranches = ref([]);
  const allTestGroups = ref([]);
  const testGroupsLoading = ref(false);
  let codeCheckTimer = null;

  const filteredSections = computed(() => {
    let list = sectionsList.value;
    if (sectionBranchFilter.value !== "ALL") list = list.filter((s) => s.branchCode === sectionBranchFilter.value);
    const q = sectionSearch.value.toLowerCase();
    if (q) list = list.filter((s) => s.code.toLowerCase().includes(q) || s.name.toLowerCase().includes(q));
    return list;
  });

  function filteredSectionsByCategory(catId) {
    return filteredSections.value.filter((s) => s.category === catId);
  }

  function receiverExistsForBranch(branchCode) {
    return sectionsList.value.some((s) => s.category === "2" && s.branchCode === branchCode);
  }

  const sectionModal = ref({
    visible: false, mode: "add", originalCode: null,
    form: { code: "", name: "", branchCode: availableBranches.value[0]?.code ?? "", category: "1", autoNo: 0, testGroupCodes: [], cutOffTime: "", autoRun: false },
    codeStatus: "idle", codeChecking: false, testGroupError: false, saving: false, error: "",
  });

  // ── Data loading ───────────────────────────────────────────────────────────
  async function loadSectionsList() {
    sectionsLoading.value = true;
    try {
      const res = await sectionApi.getAll();
      sectionsList.value = res.data;
    } catch { sectionsList.value = []; }
    finally { sectionsLoading.value = false; }
  }

  async function loadBranches() {
    try {
      const res = await settingsApi.getBranches();
      availableBranches.value = res.data.filter((b) => b.active);
    } catch { availableBranches.value = []; }
  }

  async function loadTestGroups() {
    testGroupsLoading.value = true;
    try {
      const res = await sectionApi.getTestGroups();
      allTestGroups.value = res.data;
    } catch { allTestGroups.value = []; }
    finally { testGroupsLoading.value = false; }
  }

  onMounted(() => {
    loadSectionsList();
    loadBranches();
  });

  // ── Modal actions ──────────────────────────────────────────────────────────
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
      } catch { sectionModal.value.codeStatus = "idle"; }
      finally { sectionModal.value.codeChecking = false; }
    }, 400);
  }

  function openAddSection() {
    sectionModal.value = {
      visible: true, mode: "add", originalCode: null,
      form: { code: "", name: "", branchCode: availableBranches.value[0]?.code ?? "", category: "1", autoNo: 0, testGroupCodes: [], cutOffTime: "" },
      codeStatus: "idle", codeChecking: false, testGroupError: false, saving: false, error: "",
    };
    loadTestGroups();
  }

  function openEditSection(sec) {
    sectionModal.value = {
      visible: true, mode: "edit", originalCode: sec.code,
      form: {
        code: sec.code, name: sec.name, branchCode: sec.branchCode, category: sec.category,
        autoNo: sec.autoNo ?? 0,
        testGroupCodes: sec.testGroups?.filter((tg) => tg.active).map((tg) => tg.testGroupCode) ?? [],
        cutOffTime: sec.cutOffTime ?? "",    // "HH:mm" from API or empty string
        autoRun: sec.autoRun ?? false,
      },
      codeStatus: "idle", codeChecking: false, testGroupError: false, saving: false, error: "",
    };
    if (sec.category === "3") loadTestGroups();
  }

  function closeSectionModal() {
    clearTimeout(codeCheckTimer);
    sectionModal.value.visible = false;
  }

  function toggleTestGroup(code) {
    const idx = sectionModal.value.form.testGroupCodes.indexOf(code);
    if (idx === -1) sectionModal.value.form.testGroupCodes.push(code);
    else sectionModal.value.form.testGroupCodes.splice(idx, 1);
    sectionModal.value.testGroupError = false;
  }

  async function saveSectionModal() {
    sectionModal.value.error = "";
    sectionModal.value.testGroupError = false;
    const f = sectionModal.value.form;

    if (sectionModal.value.mode === "add") {
      if (!f.code.trim()) { sectionModal.value.error = "Section code is required."; return; }
      if (sectionModal.value.codeStatus === "taken") { sectionModal.value.error = "This section code already exists."; return; }
      if (sectionModal.value.codeChecking) return;
      if (!f.branchCode) { sectionModal.value.error = "Please select a branch."; return; }
      if (!f.category) { sectionModal.value.error = "Please select a category."; return; }
      if (f.category === "2" && receiverExistsForBranch(f.branchCode)) {
        sectionModal.value.error = "A Receiver already exists for this branch."; return;
      }
    }
    if (!f.name.trim()) { sectionModal.value.error = "Section name is required."; return; }
    if (f.category === "3" && !f.testGroupCodes.length) { sectionModal.value.testGroupError = true; return; }

    sectionModal.value.saving = true;
    try {
      if (sectionModal.value.mode === "add") {
        await sectionApi.add({
          code: f.code.trim().toUpperCase(), name: f.name.trim(), branchCode: f.branchCode,
          category: f.category, autoNo: f.category === "1" ? (f.autoNo ?? 0) : 0,
          testGroupCodes: f.category === "3" ? f.testGroupCodes : [],
        });
        emit("toast", "Section created successfully.");
      } else {
        await sectionApi.update(sectionModal.value.originalCode, {
          name: f.name.trim(),
          testGroupCodes: f.category === "3" ? f.testGroupCodes : [],
          cutOffTime: f.category === "3" ? (f.cutOffTime || null) : null,
          autoRun: f.category === "3" ? f.autoRun : false,
        });
        emit("toast", "Section updated successfully.");
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
      emit("toast", err.response?.data?.message || "Failed to update status.");
    }
  }
</script>

<style scoped>
  .modal-enter-active,
  .modal-leave-active {
    transition: opacity 0.2s ease;
  }

  .modal-enter-from,
  .modal-leave-to {
    opacity: 0;
  }
</style>
