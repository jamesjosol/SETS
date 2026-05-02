<!-- sets.client/src/views/receiver/SpecimenIssuesLog.vue -->
<template>
  <AppLayout>
    <div class="flex h-[calc(100vh-64px)] overflow-hidden">

      <!-- ── Left Panel: Incident Type Folders ──────────────────────────── -->
      <div class="w-72 flex-shrink-0 flex flex-col border-r overflow-hidden"
           style="background-color: var(--color-surface); border-color: var(--color-border);">

        <!-- Header -->
        <div class="px-5 py-4 flex items-center justify-between flex-shrink-0"
             style="border-bottom: 1px solid var(--color-border);">
          <div>
            <p class="text-xs font-bold uppercase tracking-widest"
               style="color: var(--color-text-muted);">Incident Types</p>
            <p class="text-[10px] mt-0.5" style="color: var(--color-text-muted);">
              {{ incidentTypes.length }} folder{{ incidentTypes.length !== 1 ? 's' : '' }}
            </p>
          </div>
          <button class="w-8 h-8 rounded-xl flex items-center justify-center transition-all active:scale-95"
                  style="background-color: var(--color-primary-soft); color: var(--color-primary);"
                  title="New Folder"
                  @click="openCreateFolderModal">
            <span class="material-symbols-outlined text-sm">create_new_folder</span>
          </button>
        </div>

        <!-- Folder List -->
        <div class="flex-1 overflow-y-auto p-2">
          <div v-if="foldersLoading" class="flex flex-col gap-1">
            <div v-for="i in 4" :key="i"
                 class="h-14 rounded-xl animate-pulse"
                 style="background-color: var(--color-surface-low);"></div>
          </div>

          <div v-else-if="!incidentTypes.length"
               class="flex flex-col items-center justify-center py-12 gap-2">
            <span class="material-symbols-outlined text-3xl"
                  style="color: var(--color-text-muted);">folder_open</span>
            <p class="text-xs text-center" style="color: var(--color-text-muted);">
              No folders yet.<br>Create one to get started.
            </p>
          </div>

          <button v-for="folder in incidentTypes.filter(f => f.isActive || isTLOrAdmin)"
                  :key="folder.id"
                  class="w-full flex items-center gap-3 px-3 py-3 rounded-xl text-left transition-all mb-1 cursor-pointer"
                  :style="selectedFolder?.id === folder.id
                    ? 'background-color: var(--color-primary-soft); border-left: 3px solid var(--color-primary); padding-left: calc(0.75rem - 3px);'
                    : 'border-left: 3px solid transparent;'"
                  @click="selectFolder(folder)">
            <span class="material-symbols-outlined text-base flex-shrink-0"
                  :style="selectedFolder?.id === folder.id
                    ? 'color: var(--color-primary);'
                    : 'color: var(--color-text-muted);'">
              {{ folder.isActive ? 'folder' : 'folder_off' }}
            </span>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-bold truncate"
                 :style="selectedFolder?.id === folder.id
                   ? 'color: var(--color-primary);'
                   : folder.isActive ? 'color: var(--color-text);' : 'color: var(--color-text-muted);'">
                {{ folder.name }}
              </p>
              <p class="text-[10px]" style="color: var(--color-text-muted);">
                {{ folder.subCategoryCount }} sub-categor{{ folder.subCategoryCount !== 1 ? 'ies' : 'y' }}
              </p>
            </div>
            <span v-if="!folder.isActive"
                  class="text-[9px] font-bold px-1.5 py-0.5 rounded-full flex-shrink-0"
                  style="background-color: var(--color-surface-low); color: var(--color-text-muted);">
              OFF
            </span>
          </button>
        </div>
      </div>

      <!-- ── Right Panel ─────────────────────────────────────────────────── -->
      <div class="flex-1 flex flex-col overflow-hidden"
           style="background-color: var(--color-bg);">

        <!-- Empty State -->
        <div v-if="!selectedFolder"
             class="flex-1 flex flex-col items-center justify-center gap-3">
          <span class="material-symbols-outlined text-5xl"
                style="color: var(--color-text-muted);">folder_open</span>
          <p class="text-sm font-bold" style="color: var(--color-text-muted);">
            Select a folder to get started
          </p>
        </div>

        <template v-else>
          <!-- Right Panel Header + Breadcrumb -->
          <div class="flex-shrink-0 px-6 py-4 flex items-center justify-between"
               style="border-bottom: 1px solid var(--color-border); background-color: var(--color-surface);">
            <div class="flex items-center gap-2 min-w-0">
              <!-- Breadcrumb -->
              <button v-if="selectedSubCategory"
                      class="text-sm font-bold transition-all hover:underline flex-shrink-0"
                      style="color: var(--color-primary);"
                      @click="backToFolder">
                {{ selectedFolder.name }}
              </button>
              <span v-if="selectedSubCategory"
                    class="material-symbols-outlined text-sm flex-shrink-0"
                    style="color: var(--color-text-muted);">
                chevron_right
              </span>
              <p class="text-sm font-bold truncate"
                 style="color: var(--color-text);">
                {{ selectedSubCategory ? selectedSubCategory.name : selectedFolder.name }}
              </p>
            </div>

            <!-- Folder actions (TL/Admin only) -->
            <div v-if="!selectedSubCategory && isTLOrAdmin"
                 class="flex items-center gap-2 flex-shrink-0">
              <button class="flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95"
                      :style="selectedFolder.isActive
                        ? 'background-color: var(--color-warning-soft); color: var(--color-warning);'
                        : 'background-color: var(--color-primary-soft); color: var(--color-primary);'"
                      @click="promptToggleFolder">
                <span class="material-symbols-outlined text-sm">
                  {{ selectedFolder.isActive ? 'folder_off' : 'folder' }}
                </span>
                {{ selectedFolder.isActive ? 'Deactivate' : 'Activate' }}
              </button>
            </div>
          </div>

          <!-- Sub-Category View -->
          <div v-if="!selectedSubCategory" class="flex-1 flex flex-col overflow-hidden">

            <!-- Tabs -->
            <div class="flex-shrink-0 flex gap-0 px-6 pt-4"
                 style="border-bottom: 1px solid var(--color-border);">
              <button v-for="tab in rightTabs" :key="tab.key"
                      class="flex items-center gap-2 px-4 py-2.5 text-xs font-bold uppercase tracking-widest transition-all relative"
                      :style="rightTab === tab.key
            ? 'color: var(--color-primary);'
            : 'color: var(--color-text-muted);'"
                      @click="rightTab = tab.key">
                {{ tab.label }}
                <span v-if="tab.key === 'comments' && comments.length"
                      class="text-[10px] font-bold px-1.5 py-0.5 rounded-full"
                      :style="rightTab === 'comments'
            ? 'background-color: var(--color-primary); color: #ffffff;'
            : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'">
                  {{ comments.length }}
                </span>
                <!-- Active underline -->
                <span v-if="rightTab === tab.key"
                      class="absolute bottom-0 left-0 right-0 h-0.5 rounded-full"
                      style="background-color: var(--color-primary);"></span>
              </button>
            </div>

            <!-- Tab: Sub-Categories -->
            <div v-if="rightTab === 'subcategories'"
                 class="flex-1 overflow-y-auto p-6">

              <div class="flex items-center justify-between mb-4">
                <p class="text-xs font-bold uppercase tracking-widest"
                   style="color: var(--color-text-muted);">
                  Sub-Categories
                </p>
                <button class="flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95"
                        style="background-color: var(--color-primary); color: #ffffff;"
                        @click="openCreateSubCatModal">
                  <span class="material-symbols-outlined text-sm">add</span>
                  Add
                </button>
              </div>

              <div v-if="subCatLoading" class="flex flex-col gap-2">
                <div v-for="i in 3" :key="i"
                     class="h-16 rounded-xl animate-pulse"
                     style="background-color: var(--color-surface);"></div>
              </div>

              <div v-else-if="!subCategories.length"
                   class="flex flex-col items-center justify-center py-16 gap-2">
                <span class="material-symbols-outlined text-4xl"
                      style="color: var(--color-text-muted);">category</span>
                <p class="text-sm font-bold" style="color: var(--color-text-muted);">No sub-categories yet</p>
              </div>

              <div v-else class="flex flex-col gap-2">
                  <div v-for="sub in subCategories.filter(s => s.isActive || isTLOrAdmin)" :key="sub.id"
                     class="rounded-xl px-4 py-3 flex items-center gap-3 transition-all"
                     style="background-color: var(--color-surface); border: 1px solid var(--color-border);">
                  <!-- Open button -->
                  <button class="flex-1 flex items-center gap-3 text-left min-w-0 cursor-pointer"
                          @click="selectSubCategory(sub)">
                    <span class="material-symbols-outlined text-base flex-shrink-0"
                          style="color: var(--color-primary);">
                      {{ sub.isActive ? 'label' : 'label_off' }}
                    </span>
                    <div class="min-w-0">
                      <p class="text-sm font-bold truncate"
                         :style="sub.isActive ? 'color: var(--color-text);' : 'color: var(--color-text-muted);'">
                        {{ sub.name }}
                      </p>
                      <div class="flex items-center gap-2 flex-wrap mt-0.5">
                        <span class="text-[10px]" style="color: var(--color-text-muted);">
                          {{ sub.entryCount }} entr{{ sub.entryCount !== 1 ? 'ies' : 'y' }}
                        </span>
                        <span v-for="tag in sub.tags" :key="tag.id"
                              class="text-[9px] font-bold px-1.5 py-0.5 rounded-full"
                              style="background-color: var(--color-primary-soft); color: var(--color-primary);">
                          {{ tag.tagName }}
                        </span>
                      </div>
                    </div>
                  </button>

                  <!-- Toggle (TL/Admin) -->
                  <button v-if="isTLOrAdmin"
                          class="w-7 h-7 rounded-lg flex items-center justify-center flex-shrink-0 transition-all active:scale-95"
                          :style="sub.isActive
                            ? 'background-color: var(--color-primary-soft); color: var(--color-primary);'
                            : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'"
                          :title="sub.isActive ? 'Deactivate' : 'Activate'"
                          @click="promptToggleSubCat(sub)">
                    <span class="material-symbols-outlined text-sm">
                      {{ sub.isActive ? 'toggle_on' : 'toggle_off' }}
                    </span>
                  </button>

                  <!-- Open arrow -->
                  <span class="material-symbols-outlined text-sm flex-shrink-0"
                        style="color: var(--color-text-muted);">
                    chevron_right
                  </span>
                </div>
              </div>
            </div>

            <!-- Tab: Comments -->
            <div v-if="rightTab === 'comments'"
                 class="flex-1 overflow-y-auto p-6 flex flex-col gap-4">

              <!-- Add Comment -->
              <div class="rounded-xl p-4 flex flex-col gap-3"
                   style="background-color: var(--color-surface); border: 1px solid var(--color-border);">
                <textarea v-model="newComment"
                          rows="3"
                          placeholder="Add a comment or note..."
                          class="w-full border-none outline-none rounded-xl p-3 text-sm resize-none"
                          style="background-color: var(--color-surface-low); color: var(--color-text);"></textarea>
                <div class="flex justify-end">
                  <button class="flex items-center gap-1.5 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95"
                          :class="commentSaving ? 'opacity-60 pointer-events-none' : ''"
                          style="background-color: var(--color-primary); color: #ffffff;"
                          @click="submitComment">
                    <span class="material-symbols-outlined text-sm">
                      {{ commentSaving ? 'progress_activity' : 'send' }}
                    </span>
                    {{ commentSaving ? 'Saving...' : 'Add Comment' }}
                  </button>
                </div>
              </div>

              <!-- Comments Loading -->
              <div v-if="commentsLoading" class="flex flex-col gap-3">
                <div v-for="i in 3" :key="i"
                     class="h-20 rounded-xl animate-pulse"
                     style="background-color: var(--color-surface);"></div>
              </div>

              <!-- Empty -->
              <div v-else-if="!comments.length"
                   class="flex flex-col items-center justify-center py-12 gap-2">
                <span class="material-symbols-outlined text-4xl"
                      style="color: var(--color-text-muted);">chat_bubble_outline</span>
                <p class="text-sm font-bold" style="color: var(--color-text-muted);">No comments yet</p>
              </div>

              <!-- Comment List -->
              <div v-else class="flex flex-col gap-3">
                <div v-for="comment in comments" :key="comment.id"
                     class="rounded-xl p-4 flex flex-col gap-2"
                     style="background-color: var(--color-surface); border: 1px solid var(--color-border);">

                  <div class="flex items-start justify-between gap-3">
                    <div v-if="editingCommentId !== comment.id"
                         class="text-sm flex-1"
                         style="color: var(--color-text); white-space: pre-wrap;">
                      {{ comment.commentText }}
                    </div>
                    <textarea v-else
                              v-model="editCommentText"
                              rows="3"
                              class="flex-1 border-none outline-none rounded-xl p-3 text-sm resize-none"
                              style="background-color: var(--color-surface-low); color: var(--color-text);"></textarea>

                    <div class="flex items-center gap-1 flex-shrink-0">
                      <template v-if="editingCommentId !== comment.id">
                        <button class="w-7 h-7 rounded-lg flex items-center justify-center transition-all"
                                style="color: var(--color-text-muted);"
                                title="Edit"
                                @click="startEditComment(comment)">
                          <span class="material-symbols-outlined text-sm">edit</span>
                        </button>
                      </template>
                      <template v-else>
                        <button class="px-3 py-1 rounded-lg text-xs font-bold transition-all"
                                style="background-color: var(--color-primary); color: #ffffff;"
                                @click="saveEditComment(comment.id)">
                          Save
                        </button>
                        <button class="px-3 py-1 rounded-lg text-xs font-bold transition-all"
                                style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                                @click="cancelEditComment">
                          Cancel
                        </button>
                      </template>
                    </div>
                  </div>

                  <div class="flex items-center gap-2">
                    <span class="text-[10px] font-bold"
                          style="color: var(--color-primary);">{{ comment.createdBy }}</span>
                    <span class="text-[10px]" style="color: var(--color-text-muted);">
                      {{ formatDate(comment.createdAt) }}
                    </span>
                    <span v-if="comment.revisedAt"
                          class="text-[10px] italic"
                          style="color: var(--color-text-muted);">
                      · edited {{ formatDate(comment.revisedAt) }} by {{ comment.revisedBy }}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- ── Sub-Category Drill-In View ──────────────────────────────── -->
          <div v-else class="flex-1 flex flex-col overflow-hidden">

            <!-- Tags Row -->
            <div class="flex-shrink-0 px-6 py-2 flex items-center gap-1.5 flex-wrap"
                 style="border-bottom: 1px solid var(--color-border); background-color: var(--color-surface);">
              <span class="text-[10px] font-bold uppercase tracking-widest flex-shrink-0 mr-1"
                    style="color: var(--color-text-muted);">Tags:</span>

              <span v-for="tag in selectedSubCategory.tags" :key="tag.id"
                    class="flex items-center gap-1 text-[10px] font-bold px-2 py-0.5 rounded-full"
                    style="background-color: var(--color-primary-soft); color: var(--color-primary);">
                {{ tag.tagName }}
                <button class="hover:opacity-70 transition-opacity leading-none"
                        @click="removeTag(tag.id)">
                  <span class="material-symbols-outlined" style="font-size: 10px;">close</span>
                </button>
              </span>

              <!-- Add Tag Input -->
              <div class="relative">
                <input v-model="tagInput"
                       type="text"
                       placeholder="+ Add tag"
                       class="border-none outline-none text-xs font-bold px-2.5 py-1 rounded-full transition-all w-24 focus:w-36"
                       style="background-color: var(--color-surface-low); color: var(--color-text);"
                       @keydown.enter="submitTag"
                       @input="filterTagSuggestions"
                       @focus="showTagSuggestions = true"
                       @blur="hideTagSuggestions" />
                <!-- Suggestions -->
                <div v-if="showTagSuggestions && filteredTagSuggestions.length"
                     class="absolute top-full left-0 mt-1 rounded-xl shadow-xl z-10 min-w-36 overflow-hidden"
                     style="background-color: var(--color-surface); border: 1px solid var(--color-border);">
                  <button v-for="suggestion in filteredTagSuggestions"
                          :key="suggestion"
                          class="w-full text-left px-3 py-2 text-xs font-bold transition-all"
                          style="color: var(--color-text);"
                          @mousedown.prevent="selectTagSuggestion(suggestion)">
                    {{ suggestion }}
                  </button>
                </div>
              </div>
            </div>

            <!-- Lab Entries -->
            <div class="flex-1 overflow-y-auto p-6 flex flex-col gap-4">

              <!-- Scan Input -->
              <div class="rounded-xl p-4 flex items-center gap-3"
                   style="background-color: var(--color-surface); border: 1px solid var(--color-border);">
                <span class="material-symbols-outlined text-base flex-shrink-0"
                      style="color: var(--color-primary);">barcode_scanner</span>
                <input ref="scanInput"
                       v-model="scanValue"
                       type="text"
                       placeholder="Scan or enter specimen / lab number..."
                       class="flex-1 border-none outline-none text-sm"
                       style="background-color: transparent; color: var(--color-text);"
                       @keydown.enter="submitLabEntry" />
                <button class="flex items-center gap-1.5 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 flex-shrink-0"
                        :class="entrySaving ? 'opacity-60 pointer-events-none' : ''"
                        style="background-color: var(--color-primary); color: #ffffff;"
                        @click="submitLabEntry">
                  <span class="material-symbols-outlined text-sm">
                    {{ entrySaving ? 'progress_activity' : 'add' }}
                  </span>
                  {{ entrySaving ? 'Adding...' : 'Add' }}
                </button>
              </div>

              <!-- Entries Loading -->
              <div v-if="entriesLoading" class="flex flex-col gap-2">
                <div v-for="i in 4" :key="i"
                     class="h-12 rounded-xl animate-pulse"
                     style="background-color: var(--color-surface);"></div>
              </div>

              <!-- Empty -->
              <div v-else-if="!labEntries.length"
                   class="flex flex-col items-center justify-center py-12 gap-2">
                <span class="material-symbols-outlined text-4xl"
                      style="color: var(--color-text-muted);">science_off</span>
                <p class="text-sm font-bold" style="color: var(--color-text-muted);">No entries yet</p>
                <p class="text-xs" style="color: var(--color-text-muted);">
                  Scan a specimen barcode to log it here.
                </p>
              </div>

              <!-- Entries Table -->
              <div v-else class="rounded-xl overflow-hidden"
                   style="background-color: var(--color-surface); border: 1px solid var(--color-border);">
                <table class="w-full text-sm">
                  <thead>
                    <tr style="background-color: var(--color-surface-low); border-bottom: 1px solid var(--color-border);">
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                          style="color: var(--color-text-muted);">Specimen No.</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                          style="color: var(--color-text-muted);">PID</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                          style="color: var(--color-text-muted);">Patient Name</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                          style="color: var(--color-text-muted);">Sample Type</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                          style="color: var(--color-text-muted);">Date</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                          style="color: var(--color-text-muted);">Logged By</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="entry in labEntries" :key="entry.id"
                        style="border-bottom: 1px solid var(--color-border);">
                      <td class="px-4 py-3 font-bold text-xs"
                          style="color: var(--color-primary);">
                        {{ entry.specimenNo }}
                      </td>
                      <td class="px-4 py-3 text-xs"
                          style="color: var(--color-text-muted);">
                        {{ entry.pid ?? '—' }}
                      </td>
                      <td class="px-4 py-3 text-xs font-bold"
                          style="color: var(--color-text);">
                        {{ entry.patientName ?? '—' }}
                      </td>
                      <td class="px-4 py-3 text-xs"
                          style="color: var(--color-text-muted);">
                        {{ entry.sampleTypeName ?? entry.sampleTypeCode ?? '—' }}
                      </td>
                      <td class="px-4 py-3 text-xs"
                          style="color: var(--color-text-muted);">
                        {{ formatEntryDate(entry.entryDate) }}
                      </td>
                      <td class="px-4 py-3 text-xs"
                          style="color: var(--color-text-muted);">
                        {{ entry.loggedBy }}
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </template>
      </div>
    </div>

    <!-- ── Create Folder Modal ─────────────────────────────────────────────── -->
    <div v-if="folderModal.visible"
         class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"
           @click="folderModal.visible = false"></div>
      <div class="relative w-full max-w-sm rounded-2xl shadow-2xl p-6 flex flex-col gap-4 animate-modal"
           style="background-color: var(--color-surface);">
        <h3 class="text-base font-bold" style="color: var(--color-text);">New Incident Type</h3>
        <input v-model="folderModal.name"
               type="text"
               placeholder="e.g. Hemolyzed Samples"
               class="w-full border-none outline-none rounded-xl px-4 py-3 text-sm"
               style="background-color: var(--color-surface-low); color: var(--color-text);"
               @keydown.enter="submitCreateFolder" />
        <div class="flex gap-3">
          <button class="flex-1 py-3 rounded-xl text-xs font-bold uppercase tracking-widest"
                  style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                  @click="folderModal.visible = false">
            Cancel
          </button>
          <button class="flex-1 py-3 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95"
                  :class="folderModal.saving ? 'opacity-60 pointer-events-none' : ''"
                  style="background-color: var(--color-primary); color: #ffffff;"
                  @click="submitCreateFolder">
            {{ folderModal.saving ? 'Creating...' : 'Create' }}
          </button>
        </div>
      </div>
    </div>

    <!-- ── Create Sub-Category Modal ──────────────────────────────────────── -->
    <div v-if="subCatModal.visible"
         class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"
           @click="subCatModal.visible = false"></div>
      <div class="relative w-full max-w-sm rounded-2xl shadow-2xl p-6 flex flex-col gap-4 animate-modal"
           style="background-color: var(--color-surface);">
        <h3 class="text-base font-bold" style="color: var(--color-text);">New Sub-Category</h3>
        <p class="text-xs" style="color: var(--color-text-muted);">
          Under: <span class="font-bold" style="color: var(--color-primary);">{{ selectedFolder?.name }}</span>
        </p>
        <input v-model="subCatModal.name"
               type="text"
               placeholder="e.g. WES Branch"
               class="w-full border-none outline-none rounded-xl px-4 py-3 text-sm"
               style="background-color: var(--color-surface-low); color: var(--color-text);"
               @keydown.enter="submitCreateSubCat" />
        <div class="flex gap-3">
          <button class="flex-1 py-3 rounded-xl text-xs font-bold uppercase tracking-widest"
                  style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                  @click="subCatModal.visible = false">
            Cancel
          </button>
          <button class="flex-1 py-3 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95"
                  :class="subCatModal.saving ? 'opacity-60 pointer-events-none' : ''"
                  style="background-color: var(--color-primary); color: #ffffff;"
                  @click="submitCreateSubCat">
            {{ subCatModal.saving ? 'Creating...' : 'Create' }}
          </button>
        </div>
      </div>
    </div>

    <!-- ── Confirm Modal ───────────────────────────────────────────────────── -->
    <ConfirmModal :isVisible="confirmModal.visible"
                  :title="confirmModal.title"
                  :message="confirmModal.message"
                  type="warning"
                  icon="warning"
                  @confirm="confirmModal.onConfirm"
                  @cancel="confirmModal.visible = false"
                  @close="confirmModal.visible = false" />

    <!-- ── Alert Modal ─────────────────────────────────────────────────────── -->
    <AlertModal :isVisible="alert.isVisible"
                :type="alert.type"
                :title="alert.title"
                :message="alert.message"
                @close="alert.isVisible = false"
                @confirm="alert.isVisible = false" />

  </AppLayout>
</template>

<script setup>
import { ref, computed, onMounted, nextTick } from 'vue'
import AppLayout from '@/components/layout/AppLayout.vue'
import AlertModal from '@/components/common/AlertModal.vue'
import ConfirmModal from '@/components/common/ConfirmModal.vue'
import { useAuthStore } from '@/stores/authStore'
import { specimenIssueApi } from '@/api/specimenIssueApi'

const authStore = useAuthStore()

const isTLOrAdmin = computed(() =>
  authStore.isAdmin || authStore.roleID === 2
)

// ── Alert ──────────────────────────────────────────────────────────────────

const alert = ref({ isVisible: false, type: 'error', title: '', message: '' })

function showAlert(type, title, message) {
  alert.value = { isVisible: true, type, title, message }
}

// ── Confirm Modal ──────────────────────────────────────────────────────────

const confirmModal = ref({
  visible: false,
  title: '',
  message: '',
  onConfirm: () => {}
})

function openConfirm(title, message, onConfirm) {
  confirmModal.value = { visible: true, title, message, onConfirm }
}

// ── Incident Types ─────────────────────────────────────────────────────────

const incidentTypes = ref([])
const foldersLoading = ref(false)
const selectedFolder = ref(null)

async function loadFolders() {
  foldersLoading.value = true
  try {
    incidentTypes.value = await specimenIssueApi.getIncidentTypes()
  } catch {
    showAlert('error', 'Error', 'Failed to load incident types.')
  } finally {
    foldersLoading.value = false
  }
}

function selectFolder(folder) {
  selectedFolder.value = folder
  selectedSubCategory.value = null
  rightTab.value = 'subcategories'
  loadSubCategories(folder.id)
  loadComments(folder.id)
}

// ── Folder Modal ───────────────────────────────────────────────────────────

const folderModal = ref({ visible: false, name: '', saving: false })

function openCreateFolderModal() {
  folderModal.value = { visible: true, name: '', saving: false }
}

async function submitCreateFolder() {
  if (!folderModal.value.name.trim()) return
  folderModal.value.saving = true
  try {
    await specimenIssueApi.createIncidentType({ name: folderModal.value.name.trim() })
    folderModal.value.visible = false
    await loadFolders()
  } catch (e) {
    showAlert('error', 'Error', e.response?.data?.message ?? 'Failed to create folder.')
  } finally {
    folderModal.value.saving = false
  }
}

function promptToggleFolder() {
  const action = selectedFolder.value.isActive ? 'deactivate' : 'activate'
  openConfirm(
    `${selectedFolder.value.isActive ? 'Deactivate' : 'Activate'} Folder`,
    `Are you sure you want to ${action} "${selectedFolder.value.name}"?`,
    async () => {
      try {
        await specimenIssueApi.toggleIncidentType(selectedFolder.value.id)
        await loadFolders()
        // Refresh selected folder state
        selectedFolder.value = incidentTypes.value.find(f => f.id === selectedFolder.value.id) ?? null
      } catch (e) {
        showAlert('error', 'Error', e.response?.data?.message ?? 'Failed to update folder.')
      }
    }
  )
}

// ── Right Panel Tabs ───────────────────────────────────────────────────────

const rightTab = ref('subcategories')
const rightTabs = [
  { key: 'subcategories', label: 'Sub-Categories' },
  { key: 'comments', label: 'Comments' },
]

// ── Sub-Categories ─────────────────────────────────────────────────────────

const subCategories = ref([])
const subCatLoading = ref(false)
const selectedSubCategory = ref(null)

async function loadSubCategories(incidentTypeId) {
  subCatLoading.value = true
  try {
    subCategories.value = await specimenIssueApi.getSubCategories(incidentTypeId)
  } catch {
    showAlert('error', 'Error', 'Failed to load sub-categories.')
  } finally {
    subCatLoading.value = false
  }
}

function selectSubCategory(sub) {
  selectedSubCategory.value = sub
  loadLabEntries(sub.id)
  loadTagSuggestions()
  nextTick(() => scanInput.value?.focus())
}

function backToFolder() {
  selectedSubCategory.value = null
}

// ── Sub-Category Modal ─────────────────────────────────────────────────────

const subCatModal = ref({ visible: false, name: '', saving: false })

function openCreateSubCatModal() {
  subCatModal.value = { visible: true, name: '', saving: false }
}

async function submitCreateSubCat() {
  if (!subCatModal.value.name.trim()) return
  subCatModal.value.saving = true
  try {
    await specimenIssueApi.createSubCategory({
      incidentTypeId: selectedFolder.value.id,
      name: subCatModal.value.name.trim()
    })
    subCatModal.value.visible = false
    await loadSubCategories(selectedFolder.value.id)
    await loadFolders() // refresh count
  } catch (e) {
    showAlert('error', 'Error', e.response?.data?.message ?? 'Failed to create sub-category.')
  } finally {
    subCatModal.value.saving = false
  }
}

function promptToggleSubCat(sub) {
  const action = sub.isActive ? 'deactivate' : 'activate'
  openConfirm(
    `${sub.isActive ? 'Deactivate' : 'Activate'} Sub-Category`,
    `Are you sure you want to ${action} "${sub.name}"?`,
    async () => {
      try {
        await specimenIssueApi.toggleSubCategory(sub.id)
        await loadSubCategories(selectedFolder.value.id)
      } catch (e) {
        showAlert('error', 'Error', e.response?.data?.message ?? 'Failed to update sub-category.')
      }
    }
  )
}

// ── Tags ───────────────────────────────────────────────────────────────────

const tagInput = ref('')
const showTagSuggestions = ref(false)
const allTagSuggestions = ref([])
const filteredTagSuggestions = computed(() =>
  tagInput.value.trim()
    ? allTagSuggestions.value.filter(t =>
        t.toLowerCase().includes(tagInput.value.toLowerCase()) &&
        !selectedSubCategory.value?.tags.some(st => st.tagName === t)
      )
    : []
)

async function loadTagSuggestions() {
  try {
    allTagSuggestions.value = await specimenIssueApi.getTagSuggestions()
  } catch { /* non-fatal */ }
}

function filterTagSuggestions() {
  showTagSuggestions.value = true
}

function hideTagSuggestions() {
  setTimeout(() => { showTagSuggestions.value = false }, 150)
}

function selectTagSuggestion(tag) {
  tagInput.value = tag
  submitTag()
}

async function submitTag() {
  const name = tagInput.value.trim()
  if (!name || !selectedSubCategory.value) return
  try {
    await specimenIssueApi.addTag({
      subCategoryId: selectedSubCategory.value.id,
      tagName: name
    })
    tagInput.value = ''
    await loadSubCategories(selectedFolder.value.id)
    // Sync selectedSubCategory tags
    selectedSubCategory.value = subCategories.value.find(s => s.id === selectedSubCategory.value.id)
    await loadTagSuggestions()
  } catch (e) {
    showAlert('error', 'Error', e.response?.data?.message ?? 'Failed to add tag.')
  }
}

async function removeTag(tagId) {
  try {
    await specimenIssueApi.deleteTag(tagId)
    await loadSubCategories(selectedFolder.value.id)
    selectedSubCategory.value = subCategories.value.find(s => s.id === selectedSubCategory.value.id)
  } catch (e) {
    showAlert('error', 'Error', e.response?.data?.message ?? 'Failed to remove tag.')
  }
}

// ── Lab Entries ────────────────────────────────────────────────────────────

const labEntries = ref([])
const entriesLoading = ref(false)
const scanValue = ref('')
const entrySaving = ref(false)
const scanInput = ref(null)

async function loadLabEntries(subCategoryId) {
  entriesLoading.value = true
  try {
    labEntries.value = await specimenIssueApi.getLabEntries(subCategoryId)
  } catch {
    showAlert('error', 'Error', 'Failed to load entries.')
  } finally {
    entriesLoading.value = false
  }
}

async function submitLabEntry() {
  const val = scanValue.value.trim()
  if (!val || !selectedSubCategory.value) return
  entrySaving.value = true
  try {
    await specimenIssueApi.addLabEntry({
      subCategoryId: selectedSubCategory.value.id,
      specimenNo: val
    })
    scanValue.value = ''
    await loadLabEntries(selectedSubCategory.value.id)
    // Refresh entry count on sub-category list
    await loadSubCategories(selectedFolder.value.id)
    selectedSubCategory.value = subCategories.value.find(s => s.id === selectedSubCategory.value.id)
    nextTick(() => scanInput.value?.focus())
  } catch (e) {
    showAlert('error', 'Error', e.response?.data?.message ?? 'Failed to add entry.')
  } finally {
    entrySaving.value = false
  }
}

// ── Comments ───────────────────────────────────────────────────────────────

const comments = ref([])
const commentsLoading = ref(false)
const newComment = ref('')
const commentSaving = ref(false)
const editingCommentId = ref(null)
const editCommentText = ref('')

async function loadComments(incidentTypeId) {
  commentsLoading.value = true
  try {
    comments.value = await specimenIssueApi.getComments(incidentTypeId)
  } catch {
    showAlert('error', 'Error', 'Failed to load comments.')
  } finally {
    commentsLoading.value = false
  }
}

async function submitComment() {
  if (!newComment.value.trim() || !selectedFolder.value) return
  commentSaving.value = true
  try {
    await specimenIssueApi.addComment({
      incidentTypeId: selectedFolder.value.id,
      commentText: newComment.value.trim()
    })
    newComment.value = ''
    await loadComments(selectedFolder.value.id)
  } catch (e) {
    showAlert('error', 'Error', e.response?.data?.message ?? 'Failed to add comment.')
  } finally {
    commentSaving.value = false
  }
}

function startEditComment(comment) {
  editingCommentId.value = comment.id
  editCommentText.value = comment.commentText
}

function cancelEditComment() {
  editingCommentId.value = null
  editCommentText.value = ''
}

async function saveEditComment(id) {
  if (!editCommentText.value.trim()) return
  try {
    await specimenIssueApi.editComment(id, { commentText: editCommentText.value.trim() })
    editingCommentId.value = null
    await loadComments(selectedFolder.value.id)
  } catch (e) {
    showAlert('error', 'Error', e.response?.data?.message ?? 'Failed to update comment.')
  }
}

// ── Formatters ─────────────────────────────────────────────────────────────

function formatDate(val) {
  if (!val) return '—'
  return new Date(val).toLocaleString('en-US', {
    month: 'short', day: 'numeric', year: 'numeric',
    hour: 'numeric', minute: '2-digit', hour12: true
  })
}

function formatEntryDate(val) {
  if (!val) return '—'
  // DateOnly from backend comes as "2026-05-02"
  const [y, m, d] = String(val).split('-')
  return new Date(y, m - 1, d).toLocaleDateString('en-US', {
    month: 'short', day: 'numeric', year: 'numeric'
  })
}

// ── Init ───────────────────────────────────────────────────────────────────

onMounted(() => {
  loadFolders()
})
</script>

<style scoped>
  .animate-modal {
    animation: modalIn 0.2s ease;
  }

  @keyframes modalIn {
    from {
      opacity: 0;
      transform: scale(0.9) translateY(10px);
    }

    to {
      opacity: 1;
      transform: scale(1) translateY(0);
    }
  }
</style>
