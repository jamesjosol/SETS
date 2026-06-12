<template>
  <div class="space-y-6">

    <!-- ═══════════════════════════════════════════════════
         SECTION 1 — TEST CODE MAPPING
    ════════════════════════════════════════════════════ -->
    <div class="rounded-2xl overflow-hidden"
         style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)">

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
              Map equivalent test codes between SETS and HCLAB (e.g. machine swap aliases).
            </p>
          </div>
        </div>
        <button class="flex items-center gap-2 px-4 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 shadow"
                style="background: var(--color-primary-gradient); color: #ffffff"
                @click="openAddMap">
          <span class="material-symbols-outlined text-sm">add</span>Add Mapping
        </button>
      </div>

      <div class="px-8 pt-6 pb-2">
        <div class="relative">
          <span class="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-base"
                style="color: var(--color-text-muted)">search</span>
          <input v-model="mapSearch"
                 class="w-full pl-10 pr-4 py-2.5 rounded-xl text-sm font-medium outline-none transition-all"
                 style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                 placeholder="Search by Code A, Code B, or Remarks..."
                 @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                 @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
        </div>
      </div>

      <div class="px-8 py-4">
        <AppTable :rows="filteredMaps" :columns="mapColumns" row-key="id"
                  :page-size="10" :loading="mapLoading"
                  loading-text="Loading mappings..."
                  empty-text="No test code mappings yet."
                  empty-icon="compare_arrows">

          <template #cell-codeA="{ value }">
            <span class="px-2.5 py-1 rounded-lg font-mono font-bold text-xs"
                  style="background-color: var(--color-primary-soft); color: var(--color-primary)">
              {{ value }}
            </span>
          </template>

          <template #cell-arrow>
            <span class="material-symbols-outlined text-base"
                  style="color: var(--color-text-muted)">sync_alt</span>
          </template>

          <template #cell-codeB="{ value }">
            <span class="px-2.5 py-1 rounded-lg font-mono font-bold text-xs"
                  style="background-color: var(--color-surface-high); color: var(--color-text)">
              {{ value }}
            </span>
          </template>

          <template #cell-isActive="{ row }">
            <button class="flex items-center gap-1.5 px-3 py-1 rounded-lg text-[10px] font-bold uppercase tracking-widest transition-all"
                    :style="row.isActive
                      ? 'background-color: var(--color-primary-soft); color: var(--color-primary);'
                      : 'background-color: var(--color-surface-low); color: var(--color-text-muted); border: 1px solid var(--color-border);'"
                    @click.stop="toggleMap(row)">
              <span class="material-symbols-outlined text-xs">{{ row.isActive ? 'check_circle' : 'cancel' }}</span>
              {{ row.isActive ? 'Active' : 'Inactive' }}
            </button>
          </template>

          <template #actions="{ row }">
            <div class="flex items-center gap-1 justify-end">
              <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                      title="Edit mapping" style="color: var(--color-text-muted)"
                      @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'var(--color-surface-low)')"
                      @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                      @click.stop="openEditMap(row)">
                <span class="material-symbols-outlined text-sm">edit</span>
              </button>
              <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                      title="Delete mapping" style="color: #ba1a1a"
                      @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'rgba(186,26,26,0.08)')"
                      @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                      @click.stop="openDeleteMap(row)">
                <span class="material-symbols-outlined text-sm">delete</span>
              </button>
            </div>
          </template>
        </AppTable>
      </div>
    </div>

    <!-- ═══════════════════════════════════════════════════
         SECTION 2 — TEST GROUP OVERRIDE
    ════════════════════════════════════════════════════ -->
    <div class="rounded-2xl overflow-hidden"
         style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)">

      <div class="px-8 py-5 flex items-center justify-between"
           style="border-bottom: 1px solid var(--color-border)">
        <div class="flex items-center gap-4">
          <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
               style="background-color: var(--color-primary-soft)">
            <span class="material-symbols-outlined text-base"
                  style="color: var(--color-primary)">rule_settings</span>
          </div>
          <div>
            <h2 class="text-base font-extrabold tracking-tight"
                style="color: var(--color-text)">
              Test Group Override
            </h2>
            <p class="text-xs mt-0.5" style="color: var(--color-text-muted)">
              Override the test group assigned to a test code in HCLAB for correct section routing.
            </p>
          </div>
        </div>
        <button class="flex items-center gap-2 px-4 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 shadow"
                style="background: var(--color-primary-gradient); color: #ffffff"
                @click="openAddOverride">
          <span class="material-symbols-outlined text-sm">add</span>Add Override
        </button>
      </div>

      <div class="px-8 pt-6 pb-2">
        <div class="relative">
          <span class="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-base"
                style="color: var(--color-text-muted)">search</span>
          <input v-model="overrideSearch"
                 class="w-full pl-10 pr-4 py-2.5 rounded-xl text-sm font-medium outline-none transition-all"
                 style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                 placeholder="Search by Test Code, Override Group, or Remarks..."
                 @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
               @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
        </div>
      </div>

      <div class="px-8 py-4">
        <AppTable :rows="filteredOverrides" :columns="overrideColumns" row-key="id"
                  :page-size="10" :loading="overrideLoading"
                  loading-text="Loading overrides..."
                  empty-text="No test group overrides yet."
                  empty-icon="rule_settings">

          <template #cell-testCode="{ value }">
            <span class="px-2.5 py-1 rounded-lg font-mono font-bold text-xs"
                  style="background-color: var(--color-primary-soft); color: var(--color-primary)">
              {{ value }}
            </span>
          </template>

          <template #cell-overrideGroup="{ value }">
            <span class="px-2.5 py-1 rounded-lg font-mono font-bold text-xs"
                  style="background-color: var(--color-surface-high); color: var(--color-text)">
              {{ value }}
            </span>
          </template>

          <template #cell-isActive="{ row }">
            <button class="flex items-center gap-1.5 px-3 py-1 rounded-lg text-[10px] font-bold uppercase tracking-widest transition-all"
                    :style="row.isActive
                      ? 'background-color: var(--color-primary-soft); color: var(--color-primary);'
                      : 'background-color: var(--color-surface-low); color: var(--color-text-muted); border: 1px solid var(--color-border);'"
                    @click.stop="toggleOverride(row)">
              <span class="material-symbols-outlined text-xs">{{ row.isActive ? 'check_circle' : 'cancel' }}</span>
              {{ row.isActive ? 'Active' : 'Inactive' }}
            </button>
          </template>

          <template #actions="{ row }">
            <div class="flex items-center gap-1 justify-end">
              <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                      title="Edit override" style="color: var(--color-text-muted)"
                      @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'var(--color-surface-low)')"
                      @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                      @click.stop="openEditOverride(row)">
                <span class="material-symbols-outlined text-sm">edit</span>
              </button>
              <button class="w-8 h-8 rounded-lg flex items-center justify-center transition-all"
                      title="Delete override" style="color: #ba1a1a"
                      @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'rgba(186,26,26,0.08)')"
                      @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                      @click.stop="openDeleteOverride(row)">
                <span class="material-symbols-outlined text-sm">delete</span>
              </button>
            </div>
          </template>
        </AppTable>
      </div>
    </div>
  </div>

  <!-- ═══════════════════════════════════════════════════
       TEST CODE MAP — Add/Edit Modal
  ════════════════════════════════════════════════════ -->
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="mapModal.visible"
           class="fixed inset-0 z-50 flex items-center justify-center"
           style="background-color: rgba(0,0,0,0.4)"
           @click.self="closeMapModal">
        <div class="w-full max-w-md rounded-2xl overflow-hidden"
             style="background-color: var(--color-surface); box-shadow: 0 8px 32px rgba(0,0,0,0.24)">

          <div class="px-6 py-4 flex items-center justify-between"
               style="border-bottom: 1px solid var(--color-border)">
            <div class="flex items-center gap-3">
              <div class="w-8 h-8 rounded-lg flex items-center justify-center"
                   style="background-color: var(--color-primary-soft)">
                <span class="material-symbols-outlined text-sm"
                      style="color: var(--color-primary)">compare_arrows</span>
              </div>
              <h3 class="text-sm font-extrabold tracking-tight" style="color: var(--color-text)">
                {{ mapModal.mode === 'add' ? 'Add Mapping' : 'Edit Mapping' }}
              </h3>
            </div>
            <button class="w-7 h-7 rounded-lg flex items-center justify-center"
                    style="color: var(--color-text-muted)" @click="closeMapModal">
              <span class="material-symbols-outlined text-sm">close</span>
            </button>
          </div>

          <div class="px-6 py-5 space-y-4">

            <!-- Code A -->
            <div class="flex flex-col gap-1.5">
              <label class="text-[10px] font-bold uppercase tracking-widest"
                     style="color: var(--color-text-muted)">Code A</label>
              <div class="relative">
                <input v-model="codeASearch"
                       ref="codeAInputRef"
                       placeholder="Search HCLAB test code..."
                       class="w-full px-4 py-2.5 pr-10 rounded-xl text-sm font-medium outline-none transition-all"
                       style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                       autocomplete="off"
                       @input="() => onTestSearch('codeA')"
                       @keydown.down.prevent="() => selectNext('codeA')"
                       @keydown.up.prevent="() => selectPrev('codeA')"
                       @keydown.enter.prevent="() => confirmSelected('codeA')"
                       @keydown.escape="searchState.codeA.open = false"
                       @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                       @blur="onFieldBlur"/>
                <div class="absolute right-3 top-1/2 -translate-y-1/2">
                  <span v-if="searchState.codeA.loading"
                        class="material-symbols-outlined text-sm animate-spin"
                        style="color: var(--color-text-muted)">progress_activity</span>
                  <span v-else-if="mapForm.codeA"
                        class="material-symbols-outlined text-sm"
                        style="color: var(--color-primary)">check_circle</span>
                  <span v-else class="material-symbols-outlined text-sm"
                        style="color: var(--color-text-muted)">biotech</span>
                </div>
              </div>
              <!-- Selected pill -->
              <div v-if="mapForm.codeA"
                   class="flex items-center gap-2 px-3 py-2 rounded-lg"
                   style="background-color: var(--color-primary-soft)">
                <span class="material-symbols-outlined text-sm"
                      style="color: var(--color-primary)">biotech</span>
                <div class="flex-1 min-w-0">
                  <p class="text-xs font-bold font-mono truncate"
                     style="color: var(--color-primary)">{{ mapForm.codeA }}</p>
                  <p class="text-[10px] truncate"
                     style="color: var(--color-text-muted)">{{ searchState.codeA.selectedName }}</p>
                </div>
                <button class="material-symbols-outlined text-sm"
                        style="color: var(--color-text-muted)"
                        @click="clearField('codeA')">
                  close
                </button>
              </div>
            </div>

            <!-- Code B -->
            <div class="flex flex-col gap-1.5">
              <label class="text-[10px] font-bold uppercase tracking-widest"
                     style="color: var(--color-text-muted)">Code B</label>
              <div class="relative">
                <input v-model="codeBSearch"
                       ref="codeBInputRef"
                       placeholder="Search HCLAB test code..."
                       class="w-full px-4 py-2.5 pr-10 rounded-xl text-sm font-medium outline-none transition-all"
                       style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                       autocomplete="off"
                       @input="() => onTestSearch('codeB')"
                       @keydown.down.prevent="() => selectNext('codeB')"
                       @keydown.up.prevent="() => selectPrev('codeB')"
                       @keydown.enter.prevent="() => confirmSelected('codeB')"
                       @keydown.escape="searchState.codeB.open = false"
                       @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                       @blur="onFieldBlur"/>
                <div class="absolute right-3 top-1/2 -translate-y-1/2">
                  <span v-if="searchState.codeB.loading"
                        class="material-symbols-outlined text-sm animate-spin"
                        style="color: var(--color-text-muted)">progress_activity</span>
                  <span v-else-if="mapForm.codeB"
                        class="material-symbols-outlined text-sm"
                        style="color: var(--color-primary)">check_circle</span>
                  <span v-else class="material-symbols-outlined text-sm"
                        style="color: var(--color-text-muted)">biotech</span>
                </div>
              </div>
              <!-- Selected pill -->
              <div v-if="mapForm.codeB"
                   class="flex items-center gap-2 px-3 py-2 rounded-lg"
                   style="background-color: var(--color-primary-soft)">
                <span class="material-symbols-outlined text-sm"
                      style="color: var(--color-primary)">biotech</span>
                <div class="flex-1 min-w-0">
                  <p class="text-xs font-bold font-mono truncate"
                     style="color: var(--color-primary)">{{ mapForm.codeB }}</p>
                  <p class="text-[10px] truncate"
                     style="color: var(--color-text-muted)">{{ searchState.codeB.selectedName }}</p>
                </div>
                <button class="material-symbols-outlined text-sm"
                        style="color: var(--color-text-muted)"
                        @click="clearField('codeB')">
                  close
                </button>
              </div>
            </div>

            <!-- Remarks -->
            <div class="flex flex-col gap-1.5">
              <label class="text-[10px] font-bold uppercase tracking-widest"
                     style="color: var(--color-text-muted)">
                Remarks
                <span style="font-weight: 400">(optional)</span>
              </label>
              <input v-model="mapForm.remarks" type="text" maxlength="200"
                     placeholder="e.g. Machine swap — analyzer offline"
                     class="w-full px-3 py-2.5 rounded-xl text-sm outline-none transition-all"
                     style="background-color: var(--color-surface-low); border: 1px solid var(--color-border); color: var(--color-text)"
                     @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                     @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
            </div>

            <p v-if="mapModal.error" class="text-xs font-bold" style="color: #ba1a1a">
              {{ mapModal.error }}
            </p>
          </div>

          <div class="px-6 py-4 flex items-center justify-end gap-3"
               style="border-top: 1px solid var(--color-border)">
            <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                    style="background-color: var(--color-surface-low); color: var(--color-text-muted)"
                    @click="closeMapModal">
              Cancel
            </button>
            <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow"
                    style="background: var(--color-primary-gradient); color: #ffffff"
                    :disabled="mapModal.saving" @click="saveMapModal">
              <span v-if="mapModal.saving"
                    class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
              <span v-else class="material-symbols-outlined text-sm">save</span>
              {{ mapModal.mode === 'add' ? 'Add' : 'Save Changes' }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>

  <!-- ═══════════════════════════════════════════════════
       TEST GROUP OVERRIDE — Add/Edit Modal
  ════════════════════════════════════════════════════ -->
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="overrideModal.visible"
           class="fixed inset-0 z-50 flex items-center justify-center"
           style="background-color: rgba(0,0,0,0.4)"
           @click.self="closeOverrideModal">
        <div class="w-full max-w-md rounded-2xl overflow-hidden"
             style="background-color: var(--color-surface); box-shadow: 0 8px 32px rgba(0,0,0,0.24)">

          <div class="px-6 py-4 flex items-center justify-between"
               style="border-bottom: 1px solid var(--color-border)">
            <div class="flex items-center gap-3">
              <div class="w-8 h-8 rounded-lg flex items-center justify-center"
                   style="background-color: var(--color-primary-soft)">
                <span class="material-symbols-outlined text-sm"
                      style="color: var(--color-primary)">rule_settings</span>
              </div>
              <h3 class="text-sm font-extrabold tracking-tight" style="color: var(--color-text)">
                {{ overrideModal.mode === 'add' ? 'Add Override' : 'Edit Override' }}
              </h3>
            </div>
            <button class="w-7 h-7 rounded-lg flex items-center justify-center"
                    style="color: var(--color-text-muted)" @click="closeOverrideModal">
              <span class="material-symbols-outlined text-sm">close</span>
            </button>
          </div>

          <div class="px-6 py-5 space-y-4">

            <!-- Test Code -->
            <div class="flex flex-col gap-1.5">
              <label class="text-[10px] font-bold uppercase tracking-widest"
                     style="color: var(--color-text-muted)">Test Code</label>
              <div class="relative">
                <input v-model="overrideTestSearch"
                       ref="overrideTestInputRef"
                       placeholder="Search HCLAB test code..."
                       class="w-full px-4 py-2.5 pr-10 rounded-xl text-sm font-medium outline-none transition-all"
                       style="background-color: var(--color-surface-low); color: var(--color-text); border: 1.5px solid var(--color-border);"
                       autocomplete="off"
                       @input="() => onTestSearch('override')"
                       @keydown.down.prevent="() => selectNext('override')"
                       @keydown.up.prevent="() => selectPrev('override')"
                       @keydown.enter.prevent="() => confirmSelected('override')"
                       @keydown.escape="searchState.override.open = false"
                       @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                       @blur="onFieldBlur" />
                <div class="absolute right-3 top-1/2 -translate-y-1/2">
                  <span v-if="searchState.override.loading"
                        class="material-symbols-outlined text-sm animate-spin"
                        style="color: var(--color-text-muted)">progress_activity</span>
                  <span v-else-if="overrideForm.testCode"
                        class="material-symbols-outlined text-sm"
                        style="color: var(--color-primary)">check_circle</span>
                  <span v-else class="material-symbols-outlined text-sm"
                        style="color: var(--color-text-muted)">biotech</span>
                </div>
              </div>
              <!-- Selected pill -->
              <div v-if="overrideForm.testCode"
                   class="flex items-center gap-2 px-3 py-2 rounded-lg"
                   style="background-color: var(--color-primary-soft)">
                <span class="material-symbols-outlined text-sm"
                      style="color: var(--color-primary)">biotech</span>
                <div class="flex-1 min-w-0">
                  <p class="text-xs font-bold font-mono truncate"
                     style="color: var(--color-primary)">{{ overrideForm.testCode }}</p>
                  <p class="text-[10px] truncate"
                     style="color: var(--color-text-muted)">{{ searchState.override.selectedName }}</p>
                </div>
                <button class="material-symbols-outlined text-sm"
                        style="color: var(--color-text-muted)"
                        @click="clearField('override')">
                  close
                </button>
              </div>
            </div>

            <!-- Override Group -->
            <div class="flex flex-col gap-1.5">
              <label class="text-[10px] font-bold uppercase tracking-widest"
                     style="color: var(--color-text-muted)">Override Group</label>
              <input v-model="overrideForm.overrideGroup" type="text" maxlength="20"
                     placeholder="e.g. CM"
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
              <input v-model="overrideForm.remarks" type="text" maxlength="200"
                     placeholder="e.g. Wrong test group in HCLAB"
                     class="w-full px-3 py-2.5 rounded-xl text-sm outline-none transition-all"
                     style="background-color: var(--color-surface-low); border: 1px solid var(--color-border); color: var(--color-text)"
                     @focus="(e) => (e.target.style.borderColor = 'var(--color-primary)')"
                     @blur="(e) => (e.target.style.borderColor = 'var(--color-border)')" />
            </div>

            <p v-if="overrideModal.error" class="text-xs font-bold" style="color: #ba1a1a">
              {{ overrideModal.error }}
            </p>
          </div>

          <div class="px-6 py-4 flex items-center justify-end gap-3"
               style="border-top: 1px solid var(--color-border)">
            <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                    style="background-color: var(--color-surface-low); color: var(--color-text-muted)"
                    @click="closeOverrideModal">
              Cancel
            </button>
            <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex items-center gap-2 shadow"
                    style="background: var(--color-primary-gradient); color: #ffffff"
                    :disabled="overrideModal.saving" @click="saveOverrideModal">
              <span v-if="overrideModal.saving"
                    class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
              <span v-else class="material-symbols-outlined text-sm">save</span>
              {{ overrideModal.mode === 'add' ? 'Add' : 'Save Changes' }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>

  <!-- Dropdowns — Teleported to body for correct z-index -->
  <Teleport to="body">
    <template v-for="field in ['codeA', 'codeB', 'override']" :key="field">
      <div v-if="searchState[field].open &&
                 (searchState[field].results.length || searchState[field].noResults)"
           class="fixed z-[60] rounded-xl overflow-hidden shadow-xl"
           :style="`top: ${searchState[field].pos.top}px;
                    left: ${searchState[field].pos.left}px;
                    width: ${searchState[field].pos.width}px;
                    background-color: var(--color-surface);
                    border: 1.5px solid var(--color-border);`">
        <div v-if="searchState[field].noResults"
             class="px-4 py-3 text-xs text-center"
             style="color: var(--color-text-muted)">
          No tests found.
        </div>
        <div v-else class="overflow-y-auto" style="max-height: 200px">
          <div v-for="(t, idx) in searchState[field].results" :key="t.testCode"
               class="flex items-center gap-3 px-4 py-2.5 cursor-pointer transition-all"
               :style="idx === searchState[field].activeIdx
                 ? 'background-color: var(--color-primary-soft);'
                 : 'border-bottom: 1px solid var(--color-surface-low);'"
               @mouseenter="searchState[field].activeIdx = idx"
               @mousedown.prevent="selectTest(field, t)">
            <div class="w-7 h-7 rounded-lg flex items-center justify-center flex-shrink-0"
                 style="background-color: var(--color-surface-low)">
              <span class="material-symbols-outlined text-sm"
                    style="color: var(--color-text-muted)">biotech</span>
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-xs font-bold font-mono truncate"
                 style="color: var(--color-text)">{{ t.testCode }}</p>
              <p class="text-[10px] truncate"
                 style="color: var(--color-text-muted)">{{ t.testName }}</p>
            </div>
            <span v-if="t.testGroup"
                  class="px-1.5 py-0.5 rounded text-[9px] font-bold uppercase tracking-wider flex-shrink-0"
                  style="background-color: var(--color-primary-soft); color: var(--color-primary)">
              {{ t.testGroup }}
            </span>
          </div>
        </div>
      </div>
    </template>
  </Teleport>

  <!-- Confirm Modals -->
  <ConfirmModal :isVisible="mapDeleteConfirm.visible"
                title="Remove Mapping"
                :message="`Remove mapping between '${mapDeleteConfirm.item?.codeA}' and '${mapDeleteConfirm.item?.codeB}'? This cannot be undone.`"
                confirmText="Remove" cancelText="Cancel"
                @confirm="executeDeleteMap"
                @close="mapDeleteConfirm.visible = false" />

  <ConfirmModal :isVisible="overrideDeleteConfirm.visible"
                title="Remove Override"
                :message="`Remove group override for test code '${overrideDeleteConfirm.item?.testCode}'? This cannot be undone.`"
                confirmText="Remove" cancelText="Cancel"
                @confirm="executeDeleteOverride"
                @close="overrideDeleteConfirm.visible = false" />
</template>

<script setup>
  import { ref, computed, onMounted, nextTick } from 'vue'
  import AppTable from '@/components/common/AppTable.vue'
  import ConfirmModal from '@/components/common/ConfirmModal.vue'
  import { testCodeMapApi } from '@/api/testCodeMapApi'
  import { testGroupOverrideApi } from '@/api/testGroupOverrideApi'
  import { testRunningDayApi } from '@/api/testRunningDayApi'

  const emit = defineEmits(['toast'])

  // ── Columns ────────────────────────────────────────────────────────────────
  const mapColumns = [
    { key: 'codeA', label: 'Code A' },
    { key: 'arrow', label: '' },
    { key: 'codeB', label: 'Code B' },
    { key: 'remarks', label: 'Remarks' },
    { key: 'isActive', label: 'Status' },
    { key: 'createdBy', label: 'Added By' },
  ]

  const overrideColumns = [
    { key: 'testCode', label: 'Test Code' },
    { key: 'overrideGroup', label: 'Override Group' },
    { key: 'remarks', label: 'Remarks' },
    { key: 'isActive', label: 'Status' },
    { key: 'createdBy', label: 'Added By' },
  ]

  // ── Input refs ─────────────────────────────────────────────────────────────
  const codeAInputRef = ref(null)
  const codeBInputRef = ref(null)
  const overrideTestInputRef = ref(null)

  // ── Search inputs (v-model for each input) ─────────────────────────────────
  const codeASearch = ref('')
  const codeBSearch = ref('')
  const overrideTestSearch = ref('')

  // ── Unified search state per field ────────────────────────────────────────
  const makeSearchState = () => ({
    results: [], loading: false, open: false,
    noResults: false, activeIdx: -1,
    selectedName: '',
    pos: { top: 0, left: 0, width: 0 }
  })

  const searchState = ref({
    codeA: makeSearchState(),
    codeB: makeSearchState(),
    override: makeSearchState(),
  })

  const searchTimers = { codeA: null, codeB: null, override: null }

  const inputRefs = computed(() => ({
    codeA: codeAInputRef,
    codeB: codeBInputRef,
    override: overrideTestInputRef,
  }))

  const searchInputs = computed(() => ({
    codeA: codeASearch,
    codeB: codeBSearch,
    override: overrideTestSearch,
  }))

  // ── Table state ────────────────────────────────────────────────────────────
  const mapItems = ref([])
  const mapLoading = ref(false)
  const mapSearch = ref('')

  const overrideItems = ref([])
  const overrideLoading = ref(false)
  const overrideSearch = ref('')

  // ── Modal state ────────────────────────────────────────────────────────────
  const mapModal = ref({ visible: false, mode: 'add', editId: null, saving: false, error: '' })
  const mapForm = ref({ codeA: '', codeB: '', remarks: '' })
  const mapDeleteConfirm = ref({ visible: false, item: null })

  const overrideModal = ref({ visible: false, mode: 'add', editId: null, saving: false, error: '' })
  const overrideForm = ref({ testCode: '', overrideGroup: '', remarks: '' })
  const overrideDeleteConfirm = ref({ visible: false, item: null })

  // ── Computed ───────────────────────────────────────────────────────────────
  const filteredMaps = computed(() => {
    const q = mapSearch.value.trim().toLowerCase()
    if (!q) return mapItems.value
    return mapItems.value.filter(i =>
      i.codeA.toLowerCase().includes(q) ||
      i.codeB.toLowerCase().includes(q) ||
      (i.remarks ?? '').toLowerCase().includes(q)
    )
  })

  const filteredOverrides = computed(() => {
    const q = overrideSearch.value.trim().toLowerCase()
    if (!q) return overrideItems.value
    return overrideItems.value.filter(i =>
      i.testCode.toLowerCase().includes(q) ||
      i.overrideGroup.toLowerCase().includes(q) ||
      (i.remarks ?? '').toLowerCase().includes(q)
    )
  })

  // ── Load ───────────────────────────────────────────────────────────────────
  async function loadMaps() {
    mapLoading.value = true
    try { mapItems.value = await testCodeMapApi.getAll() }
    catch { /* silent */ }
    finally { mapLoading.value = false }
  }

  async function loadOverrides() {
    overrideLoading.value = true
    try { overrideItems.value = await testGroupOverrideApi.getAll() }
    catch { /* silent */ }
    finally { overrideLoading.value = false }
  }

  onMounted(() => { loadMaps(); loadOverrides() })

  // ── HCLAB Search ───────────────────────────────────────────────────────────
  function computePos(field) {
    const el = inputRefs.value[field]?.value
    if (!el) return
    const rect = el.getBoundingClientRect()
    searchState.value[field].pos = { top: rect.bottom + 6, left: rect.left, width: rect.width }
  }

  function onTestSearch(field) {
    // clear selected value
    if (field === 'codeA') mapForm.value.codeA = ''
    if (field === 'codeB') mapForm.value.codeB = ''
    if (field === 'override') overrideForm.value.testCode = ''

    searchState.value[field].open = false
    searchState.value[field].noResults = false
    searchState.value[field].activeIdx = -1
    searchState.value[field].selectedName = ''

    window.clearTimeout(searchTimers[field])

    const q = searchInputs.value[field].value.trim()
    if (!q) { searchState.value[field].results = []; return }

    searchState.value[field].loading = true
    searchTimers[field] = window.setTimeout(async () => {
      try {
        const res = await testRunningDayApi.search(q)
        searchState.value[field].results = res.data ?? []
        searchState.value[field].noResults = searchState.value[field].results.length === 0
        searchState.value[field].open = true
        await nextTick()
        computePos(field)
      } catch {
        searchState.value[field].results = []
        searchState.value[field].noResults = true
      } finally {
        searchState.value[field].loading = false
      }
    }, 300)
  }

  function selectTest(field, t) {
    searchState.value[field].open = false
    searchState.value[field].activeIdx = -1
    searchState.value[field].selectedName = t.testName

    searchInputs.value[field].value = `${t.testCode} — ${t.testName}`

    if (field === 'codeA') mapForm.value.codeA = t.testCode
    if (field === 'codeB') mapForm.value.codeB = t.testCode
    if (field === 'override') overrideForm.value.testCode = t.testCode
  }

  function clearField(field) {
    if (field === 'codeA') { mapForm.value.codeA = ''; codeASearch.value = '' }
    if (field === 'codeB') { mapForm.value.codeB = ''; codeBSearch.value = '' }
    if (field === 'override') { overrideForm.value.testCode = ''; overrideTestSearch.value = '' }
    searchState.value[field] = makeSearchState()
    nextTick(() => inputRefs.value[field]?.value?.focus())
  }

  function selectNext(field) {
    const len = searchState.value[field].results.length
    if (!len) return
    searchState.value[field].activeIdx = (searchState.value[field].activeIdx + 1) % len
  }

  function selectPrev(field) {
    const len = searchState.value[field].results.length
    if (!len) return
    searchState.value[field].activeIdx =
      (searchState.value[field].activeIdx - 1 + len) % len
  }

  function confirmSelected(field) {
    const idx = searchState.value[field].activeIdx
    const results = searchState.value[field].results
    if (idx >= 0 && results[idx]) selectTest(field, results[idx])
  }

  // ── Code Map — CRUD ────────────────────────────────────────────────────────
  function resetSearchStates() {
    searchState.value.codeA = makeSearchState()
    searchState.value.codeB = makeSearchState()
    searchState.value.override = makeSearchState()
    codeASearch.value = ''
    codeBSearch.value = ''
    overrideTestSearch.value = ''
  }

  function openAddMap() {
    resetSearchStates()
    mapForm.value = { codeA: '', codeB: '', remarks: '' }
    mapModal.value = { visible: true, mode: 'add', editId: null, saving: false, error: '' }
  }

  function openEditMap(item) {
    resetSearchStates()
    // Pre-fill search inputs with existing codes
    codeASearch.value = item.codeA
    codeBSearch.value = item.codeB
    searchState.value.codeA.selectedName = ''
    searchState.value.codeB.selectedName = ''
    mapForm.value = { codeA: item.codeA, codeB: item.codeB, remarks: item.remarks ?? '' }
    mapModal.value = { visible: true, mode: 'edit', editId: item.id, saving: false, error: '' }
  }

  function closeMapModal() {
    if (mapModal.value.saving) return
    mapModal.value.visible = false
  }

  async function saveMapModal() {
    mapModal.value.error = ''
    if (!mapForm.value.codeA.trim() || !mapForm.value.codeB.trim()) {
      mapModal.value.error = 'Both Code A and Code B are required.'
      return
    }
    if (mapForm.value.codeA.trim().toUpperCase() === mapForm.value.codeB.trim().toUpperCase()) {
      mapModal.value.error = 'Code A and Code B must be different.'
      return
    }
    mapModal.value.saving = true
    try {
      const payload = {
        codeA: mapForm.value.codeA.trim(),
        codeB: mapForm.value.codeB.trim(),
        remarks: mapForm.value.remarks.trim() || null
      }
      if (mapModal.value.mode === 'add') {
        await testCodeMapApi.add(payload)
        emit('toast', 'Mapping added successfully.')
      } else {
        await testCodeMapApi.update(mapModal.value.editId, payload)
        emit('toast', 'Mapping updated successfully.')
      }
      mapModal.value.visible = false
      await loadMaps()
    } catch (err) {
      mapModal.value.error = err?.response?.data?.message ?? 'An error occurred.'
    } finally {
      mapModal.value.saving = false
    }
  }

  async function toggleMap(item) {
    try {
      await testCodeMapApi.toggle(item.id)
      item.isActive = !item.isActive
      emit('toast', `Mapping ${item.isActive ? 'activated' : 'deactivated'}.`)
    } catch (err) {
      emit('toast', err?.response?.data?.message ?? 'Failed to update status.')
    }
  }

  function openDeleteMap(item) { mapDeleteConfirm.value = { visible: true, item } }

  async function executeDeleteMap() {
    try {
      await testCodeMapApi.remove(mapDeleteConfirm.value.item.id)
      mapDeleteConfirm.value.visible = false
      await loadMaps()
      emit('toast', 'Mapping removed.')
    } catch (err) {
      emit('toast', err?.response?.data?.message ?? 'Failed to remove mapping.')
    }
  }

  function onFieldBlur(e) {
    window.setTimeout(() => {
      if (e.target) e.target.style.borderColor = 'var(--color-border)'
    }, 150)
  }

  // ── Group Override — CRUD ──────────────────────────────────────────────────
  function openAddOverride() {
    resetSearchStates()
    overrideForm.value = { testCode: '', overrideGroup: '', remarks: '' }
    overrideModal.value = { visible: true, mode: 'add', editId: null, saving: false, error: '' }
  }

  function openEditOverride(item) {
    resetSearchStates()
    overrideTestSearch.value = item.testCode
    searchState.value.override.selectedName = ''
    overrideForm.value = { testCode: item.testCode, overrideGroup: item.overrideGroup, remarks: item.remarks ?? '' }
    overrideModal.value = { visible: true, mode: 'edit', editId: item.id, saving: false, error: '' }
  }

  function closeOverrideModal() {
    if (overrideModal.value.saving) return
    overrideModal.value.visible = false
  }

  async function saveOverrideModal() {
    overrideModal.value.error = ''
    if (!overrideForm.value.testCode.trim()) {
      overrideModal.value.error = 'Test Code is required.'
      return
    }
    if (!overrideForm.value.overrideGroup.trim()) {
      overrideModal.value.error = 'Override Group is required.'
      return
    }
    overrideModal.value.saving = true
    try {
      const payload = {
        testCode: overrideForm.value.testCode.trim(),
        overrideGroup: overrideForm.value.overrideGroup.trim(),
        remarks: overrideForm.value.remarks.trim() || null
      }
      if (overrideModal.value.mode === 'add') {
        await testGroupOverrideApi.add(payload)
        emit('toast', 'Override added successfully.')
      } else {
        await testGroupOverrideApi.update(overrideModal.value.editId, payload)
        emit('toast', 'Override updated successfully.')
      }
      overrideModal.value.visible = false
      await loadOverrides()
    } catch (err) {
      overrideModal.value.error = err?.response?.data?.message ?? 'An error occurred.'
    } finally {
      overrideModal.value.saving = false
    }
  }

  async function toggleOverride(item) {
    try {
      await testGroupOverrideApi.toggle(item.id)
      item.isActive = !item.isActive
      emit('toast', `Override ${item.isActive ? 'activated' : 'deactivated'}.`)
    } catch (err) {
      emit('toast', err?.response?.data?.message ?? 'Failed to update status.')
    }
  }

  function openDeleteOverride(item) { overrideDeleteConfirm.value = { visible: true, item } }

  async function executeDeleteOverride() {
    try {
      await testGroupOverrideApi.remove(overrideDeleteConfirm.value.item.id)
      overrideDeleteConfirm.value.visible = false
      await loadOverrides()
      emit('toast', 'Override removed.')
    } catch (err) {
      emit('toast', err?.response?.data?.message ?? 'Failed to remove override.')
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
