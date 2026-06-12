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
        <div class="flex-1 overflow-y-auto p-2" ref="folderListRef">
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
                  class="folder-item w-full flex items-center gap-3 px-3 py-3 rounded-xl text-left transition-all mb-1 cursor-pointer"
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
             class="flex-1 flex flex-col items-center justify-center gap-3"
             ref="emptyStateRef">
          <span class="material-symbols-outlined text-5xl"
                style="color: var(--color-text-muted);">folder_open</span>
          <p class="text-sm font-bold" style="color: var(--color-text-muted);">
            Select a folder to get started
          </p>
        </div>

        <template v-else>
          <!-- Right Panel Header + Breadcrumb -->
          <div class="flex-shrink-0 px-6 py-4 flex items-center justify-between"
               ref="rightHeaderRef"
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

            <!-- Folder-level actions (only shown when not drilled into a sub-category) -->
            <div v-if="!selectedSubCategory"
                 class="flex items-center gap-2 flex-shrink-0">

              <!-- Export Excel (all users) -->
              <button class="flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95"
                      :class="isExporting ? 'opacity-60 pointer-events-none' : ''"
                      style="background-color: var(--color-success-soft); color: var(--color-success);"
                      :title="`Export '${selectedFolder.name}' to Excel`"
                      @click="exportFolder">
                <span class="material-symbols-outlined text-sm"
                      :class="isExporting ? 'animate-spin' : ''">
                  {{ isExporting ? 'progress_activity' : 'download' }}
                </span>
                {{ isExporting ? 'Exporting...' : 'Export Excel' }}
              </button>

              <!-- Activate / Deactivate (TL/Admin only) -->
              <button v-if="isTLOrAdmin"
                      class="flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95"
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
                 class="flex-1 overflow-y-auto p-6"
                 ref="subCatPanelRef">

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

              <div v-else class="flex flex-col gap-2" ref="subCatListRef">
                <div v-for="sub in subCategories.filter(s => s.isActive || isTLOrAdmin)" :key="sub.id"
                     class="subcat-item rounded-xl px-4 py-3 flex items-center gap-3 transition-all"
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
                 class="flex-1 overflow-y-auto p-6 flex flex-col gap-4"
                 ref="commentsPanelRef">

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
              <div v-else class="flex flex-col gap-3" ref="commentListRef">
                <div v-for="comment in comments" :key="comment.id"
                     class="comment-item rounded-xl p-4 flex flex-col gap-2"
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
          <div v-else class="flex-1 flex flex-col overflow-hidden" ref="drillInPanelRef">

            <!-- Tags Row -->
            <div class="drill-in-tags flex-shrink-0 px-6 py-2 flex items-center gap-1.5 flex-wrap"
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
            <div class="flex-1 overflow-hidden p-6 flex flex-col gap-4">

              <!-- Scan Input -->
              <div class="drill-in-scan flex-shrink-0 rounded-xl p-4 flex items-center gap-3"
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

              <template v-else>

                <!-- Search bar -->
                <div class="flex-shrink-0 flex items-center gap-2 rounded-xl px-4 py-2.5"
                     style="background-color: var(--color-surface); border: 1px solid var(--color-border);">
                  <span class="material-symbols-outlined text-base flex-shrink-0"
                        style="color: var(--color-text-muted);">search</span>
                  <input v-model="entrySearch"
                         type="text"
                         placeholder="Search by specimen no., lab no., PID, or patient name..."
                         class="flex-1 border-none outline-none text-xs"
                         style="background-color: transparent; color: var(--color-text);" />
                  <button v-if="entrySearch"
                          class="flex-shrink-0 transition-opacity hover:opacity-70"
                          style="color: var(--color-text-muted);"
                          @click="entrySearch = ''">
                    <span class="material-symbols-outlined text-sm">close</span>
                  </button>
                </div>

                <!-- Empty (no entries at all) -->
                <div v-if="!labEntries.length"
                     class="drill-in-empty flex flex-col items-center justify-center py-12 gap-2">
                  <span class="material-symbols-outlined text-4xl"
                        style="color: var(--color-text-muted);">science_off</span>
                  <p class="text-sm font-bold" style="color: var(--color-text-muted);">No entries yet</p>
                  <p class="text-xs" style="color: var(--color-text-muted);">
                    Scan a specimen barcode to log it here.
                  </p>
                </div>

                <!-- No search results -->
                <div v-else-if="!filteredEntries.length"
                     class="flex flex-col items-center justify-center py-12 gap-2">
                  <span class="material-symbols-outlined text-4xl"
                        style="color: var(--color-text-muted);">search_off</span>
                  <p class="text-sm font-bold" style="color: var(--color-text-muted);">No matches found</p>
                  <p class="text-xs" style="color: var(--color-text-muted);">Try a different search term.</p>
                </div>

                <!-- Entries Table -->
                <div v-else class="drill-in-table flex-1 overflow-hidden rounded-xl flex flex-col"
                     style="background-color: var(--color-surface); border: 1px solid var(--color-border);">

                  <!-- Scrollable table body -->
                  <div class="flex-1 overflow-y-auto">
                    <table class="w-full text-sm">
                      <thead class="sticky top-0 z-10">
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
                          <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                              style="color: var(--color-text-muted);"></th>
                          <th v-if="isTLOrAdmin"
                              class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                              style="color: var(--color-text-muted);"></th>
                        </tr>
                      </thead>
                      <tbody ref="entryTbodyRef">
                        <tr v-for="entry in paginatedEntries" :key="entry.id"
                            class="entry-row"
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
                          <td class="px-4 py-3">
                            <button class="w-7 h-7 rounded-lg flex items-center justify-center transition-all active:scale-95"
                                    :style="entry.remarks
            ? 'background-color: var(--color-primary-soft); color: var(--color-primary);'
            : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'"
                                    :title="entry.remarks ? 'View / edit remark' : 'Add remark'"
                                    @click="openRemarkModal(entry)">
                              <span class="material-symbols-outlined text-sm">
                                {{ entry.remarks ? 'chat_bubble' : 'chat_bubble_outline' }}
                              </span>
                            </button>
                          </td>
                          <td v-if="isTLOrAdmin" class="px-4 py-3">
                            <button class="w-7 h-7 rounded-lg flex items-center justify-center transition-all active:scale-95"
                                    style="background-color: var(--color-error-soft); color: var(--color-error);"
                                    title="Delete entry"
                                    @click="promptDeleteEntry(entry)">
                              <span class="material-symbols-outlined text-sm">delete</span>
                            </button>
                          </td>
                        </tr>
                      </tbody>
                    </table>
                  </div>

                  <!-- Pagination footer -->
                  <div class="flex-shrink-0 flex items-center justify-between px-4 py-2.5"
                       style="border-top: 1px solid var(--color-border); background-color: var(--color-surface-low);">
                    <p class="text-[10px] font-bold uppercase tracking-widest"
                       style="color: var(--color-text-muted);">
                      {{ filteredEntries.length }} result{{ filteredEntries.length !== 1 ? 's' : '' }}
                      <template v-if="entrySearch">
                        &nbsp;&middot; filtered
                      </template>
                    </p>
                    <div class="flex items-center gap-1.5">
                      <button :disabled="entryPage === 1"
                              class="w-7 h-7 rounded-lg flex items-center justify-center transition-all disabled:opacity-30"
                              style="background-color: var(--color-surface); color: var(--color-text-muted);"
                              @click="entryPage--">
                        <span class="material-symbols-outlined text-sm">chevron_left</span>
                      </button>
                      <button v-for="p in entryTotalPages" :key="p"
                              class="w-7 h-7 rounded-lg text-[11px] font-bold transition-all"
                              :style="entryPage === p
                                ? 'background-color: var(--color-primary); color: #ffffff;'
                                : 'background-color: var(--color-surface); color: var(--color-text-muted);'"
                              @click="entryPage = p">
                        {{ p }}
                      </button>
                      <button :disabled="entryPage === entryTotalPages"
                              class="w-7 h-7 rounded-lg flex items-center justify-center transition-all disabled:opacity-30"
                              style="background-color: var(--color-surface); color: var(--color-text-muted);"
                              @click="entryPage++">
                        <span class="material-symbols-outlined text-sm">chevron_right</span>
                      </button>
                    </div>
                  </div>

                </div>

              </template>
            </div>
          </div>
        </template>
      </div>
    </div>

    <!-- ── Create Folder Modal ─────────────────────────────────────────────── -->
    <div v-if="folderModal.visible"
         class="fixed inset-0 z-50 flex items-center justify-center p-4"
         ref="folderModalOverlayRef">
      <div class="absolute inset-0 bg-black/40 backdrop-blur-sm modal-backdrop"
           @click="folderModal.visible = false"></div>
      <div class="modal-card relative w-full max-w-sm rounded-2xl shadow-2xl p-6 flex flex-col gap-4"
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
                  @click="closeModal('folder')">
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
         class="fixed inset-0 z-50 flex items-center justify-center p-4"
         ref="subCatModalOverlayRef">
      <div class="absolute inset-0 bg-black/40 backdrop-blur-sm modal-backdrop"
           @click="subCatModal.visible = false"></div>
      <div class="modal-card relative w-full max-w-sm rounded-2xl shadow-2xl p-6 flex flex-col gap-4"
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
                  @click="closeModal('subcat')">
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

    <!-- Remark Modal -->
    <RemarkModal :isVisible="remarkModal.visible"
                 title="Entry Remark"
                 :initialText="remarkModal.entry?.remarks ?? ''"
                 @save="saveRemark"
                 @close="closeRemarkModal" />

  </AppLayout>
</template>

<script setup>
  import { ref, computed, onMounted, nextTick, watch } from 'vue'
  import gsap from 'gsap'
  import AppLayout from '@/components/layout/AppLayout.vue'
  import AlertModal from '@/components/common/AlertModal.vue'
  import ConfirmModal from '@/components/common/ConfirmModal.vue'
  import RemarkModal from '@/components/common/RemarkModal.vue'
  import { useAuthStore } from '@/stores/authStore'
  import { specimenIssueApi } from '@/api/specimenIssueApi'

  const authStore = useAuthStore()

  const isTLOrAdmin = computed(() =>
    authStore.isAdmin || authStore.roleID === 2
  )

  // ── Template Refs ──────────────────────────────────────────────────────────

  const folderListRef = ref(null)
  const rightHeaderRef = ref(null)
  const subCatListRef = ref(null)
  const subCatPanelRef = ref(null)
  const commentListRef = ref(null)
  const commentsPanelRef = ref(null)
  const drillInPanelRef = ref(null)
  const entryTbodyRef = ref(null)
  const emptyStateRef = ref(null)
  const folderModalOverlayRef = ref(null)
  const subCatModalOverlayRef = ref(null)

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
    onConfirm: () => { }
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
      await nextTick()
      animateFolderList()
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
    nextTick(() => animateModalIn(folderModalOverlayRef.value))
  }

  function closeModal(which) {
    const ref = which === 'folder' ? folderModalOverlayRef.value : subCatModalOverlayRef.value
    animateModalOut(ref, () => {
      if (which === 'folder') folderModal.value.visible = false
      else subCatModal.value.visible = false
    })
  }

  async function submitCreateFolder() {
    if (!folderModal.value.name.trim()) return
    folderModal.value.saving = true
    try {
      await specimenIssueApi.createIncidentType({ name: folderModal.value.name.trim() })
      animateModalOut(folderModalOverlayRef.value, async () => {
        folderModal.value.visible = false
        await loadFolders()
      })
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

  // Animate tab panel switch
  watch(rightTab, async (newTab) => {
    await nextTick()
    const panel = newTab === 'subcategories' ? subCatPanelRef.value : commentsPanelRef.value
    if (!panel) return
    gsap.set(panel, { opacity: 0, y: 10 })
    gsap.to(panel, { opacity: 1, y: 0, duration: 0.25, ease: 'power2.out' })
  })

  // ── Sub-Categories ─────────────────────────────────────────────────────────

  const subCategories = ref([])
  const subCatLoading = ref(false)
  const selectedSubCategory = ref(null)

  async function loadSubCategories(incidentTypeId) {
    subCatLoading.value = true
    try {
      subCategories.value = await specimenIssueApi.getSubCategories(incidentTypeId)
      await nextTick()
      animateSubCatList()
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
    nextTick(() => {
      animateDrillIn()
      scanInput.value?.focus()
    })
  }

  function backToFolder() {
    // Slide out drill-in, restore sub-cat view
    if (drillInPanelRef.value) {
      gsap.to(drillInPanelRef.value, {
        opacity: 0,
        x: 30,
        duration: 0.18,
        ease: 'power2.in',
        onComplete: () => {
          selectedSubCategory.value = null
          nextTick(() => animateSubCatList())
        }
      })
    } else {
      selectedSubCategory.value = null
    }
  }

  // ── Sub-Category Modal ─────────────────────────────────────────────────────

  const subCatModal = ref({ visible: false, name: '', saving: false })

  function openCreateSubCatModal() {
    subCatModal.value = { visible: true, name: '', saving: false }
    nextTick(() => animateModalIn(subCatModalOverlayRef.value))
  }

  async function submitCreateSubCat() {
    if (!subCatModal.value.name.trim()) return
    subCatModal.value.saving = true
    try {
      await specimenIssueApi.createSubCategory({
        incidentTypeId: selectedFolder.value.id,
        name: subCatModal.value.name.trim()
      })
      animateModalOut(subCatModalOverlayRef.value, async () => {
        subCatModal.value.visible = false
        await loadSubCategories(selectedFolder.value.id)
        await loadFolders()
      })
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
  const lastAddedEntryId = ref(null)
  const ENTRY_PAGE_SIZE = 15
  const entrySearch = ref('')
  const entryPage = ref(1)

  const filteredEntries = computed(() => {
    const q = entrySearch.value.trim().toLowerCase()
    if (!q) return labEntries.value
    return labEntries.value.filter(e => {
      const labNo = e.specimenNo?.substring(0, 10).toLowerCase() ?? ''
      return (
        e.specimenNo?.toLowerCase().includes(q) ||
        labNo.includes(q) ||
        e.pid?.toLowerCase().includes(q) ||
        e.patientName?.toLowerCase().includes(q)
      )
    })
  })

  const entryTotalPages = computed(() =>
    Math.max(1, Math.ceil(filteredEntries.value.length / ENTRY_PAGE_SIZE))
  )

  const paginatedEntries = computed(() => {
    const start = (entryPage.value - 1) * ENTRY_PAGE_SIZE
    return filteredEntries.value.slice(start, start + ENTRY_PAGE_SIZE)
  })

  // Reset to page 1 when search changes
  watch(entrySearch, () => { entryPage.value = 1 })


  async function loadLabEntries(subCategoryId) {
    entriesLoading.value = true
    entrySearch.value = ''   
    entryPage.value = 1     
    try {
      labEntries.value = await specimenIssueApi.getLabEntries(subCategoryId)
      await nextTick()
      animateEntryRows()
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
      const result = await specimenIssueApi.addLabEntry({
        subCategoryId: selectedSubCategory.value.id,
        specimenNo: val
      })
      // Track newly added ID so we can animate only it
      lastAddedEntryId.value = result?.id ?? null
      scanValue.value = ''
      await loadLabEntries(selectedSubCategory.value.id)
      await loadSubCategories(selectedFolder.value.id)
      selectedSubCategory.value = subCategories.value.find(s => s.id === selectedSubCategory.value.id)
      nextTick(() => {
        scanInput.value?.focus()
        animateNewEntryRow()
      })
    } catch (e) {
      showAlert('error', 'Error', e.response?.data?.message ?? 'Failed to add entry.')
    } finally {
      entrySaving.value = false
    }
  }

  function promptDeleteEntry(entry) {
    openConfirm(
      'Delete Entry',
      `Remove specimen "${entry.specimenNo}" from this log? This cannot be undone.`,
      async () => {
        try {
          await specimenIssueApi.deleteLabEntry(entry.id)
          await loadLabEntries(selectedSubCategory.value.id)
          await loadSubCategories(selectedFolder.value.id)
          selectedSubCategory.value = subCategories.value.find(s => s.id === selectedSubCategory.value.id)
        } catch (e) {
          showAlert('error', 'Error', e.response?.data?.message ?? 'Failed to delete entry.')
        }
      }
    )
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
      await nextTick()
      animateCommentList()
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
      // Animate newest comment (first in list after reload)
      await nextTick()
      const firstComment = commentListRef.value?.querySelector('.comment-item')
      if (firstComment) {
        gsap.set(firstComment, { opacity: 0, y: -12, scale: 0.97 })
        gsap.to(firstComment, { opacity: 1, y: 0, scale: 1, duration: 0.35, ease: 'back.out(1.4)' })
      }
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

  // ── Remark Modal ───────────────────────────────────────────────────────────

  const remarkModal = ref({ visible: false, entry: null, saving: false })

  function openRemarkModal(entry) {
    remarkModal.value = { visible: true, entry, saving: false }
  }

  function closeRemarkModal() {
    remarkModal.value.visible = false
  }

  async function saveRemark(text) {
    if (remarkModal.value.saving) return
    remarkModal.value.saving = true
    try {
      await specimenIssueApi.updateLabEntryRemark(remarkModal.value.entry.id, { remarks: text })
      // Update in-place — no full reload needed
      const entry = labEntries.value.find(e => e.id === remarkModal.value.entry.id)
      if (entry) entry.remarks = text.trim() || null
      closeRemarkModal()
    } catch (e) {
      showAlert('error', 'Error', e.response?.data?.message ?? 'Failed to save remark.')
    } finally {
      remarkModal.value.saving = false
    }
  }

  // ── Export ─────────────────────────────────────────────────────────────────

  const isExporting = ref(false)

  async function exportFolder() {
    if (!selectedFolder.value || isExporting.value) return
    isExporting.value = true
    try {
      await specimenIssueApi.exportIncidentTypeExcel(
        selectedFolder.value.id,
        selectedFolder.value.name
      )
    } catch (e) {
      showAlert('error', 'Export Failed', e.response?.data?.message ?? 'Could not generate the Excel file.')
    } finally {
      isExporting.value = false
    }
  }

  // ── GSAP Animation Helpers ─────────────────────────────────────────────────

  /**
   * Stagger-in the folder list items on mount / reload.
   * Uses set+to pattern to avoid "from" flash bug.
   */
  function animateFolderList() {
    const items = folderListRef.value?.querySelectorAll('.folder-item')
    if (!items?.length) return
    gsap.set(items, { opacity: 0, x: -16 })
    gsap.to(items, {
      opacity: 1,
      x: 0,
      duration: 0.32,
      stagger: 0.055,
      ease: 'power3.out',
      clearProps: 'opacity,x'
    })
  }

  /**
   * Animate right panel header sliding down when a folder is selected.
   */
  watch(selectedFolder, async (newVal) => {
    if (!newVal) return
    await nextTick()
    if (rightHeaderRef.value) {
      gsap.set(rightHeaderRef.value, { opacity: 0, y: -10 })
      gsap.to(rightHeaderRef.value, { opacity: 1, y: 0, duration: 0.28, ease: 'power2.out', clearProps: 'opacity,y' })
    }
    // Also animate initial sub-cat panel
    if (subCatPanelRef.value) {
      gsap.set(subCatPanelRef.value, { opacity: 0, y: 12 })
      gsap.to(subCatPanelRef.value, { opacity: 1, y: 0, duration: 0.3, ease: 'power2.out', clearProps: 'opacity,y' })
    }
  })

  /**
   * Stagger-in sub-category cards.
   */
  function animateSubCatList() {
    const items = subCatListRef.value?.querySelectorAll('.subcat-item')
    if (!items?.length) return
    gsap.set(items, { opacity: 0, y: 14, scale: 0.98 })
    gsap.to(items, {
      opacity: 1,
      y: 0,
      scale: 1,
      duration: 0.3,
      stagger: 0.06,
      ease: 'power3.out',
      clearProps: 'opacity,y,scale'
    })
  }

  /**
   * Slide the drill-in panel in from the right.
   */
  function animateDrillIn() {
    if (!drillInPanelRef.value) return
    gsap.set(drillInPanelRef.value, { opacity: 0, x: 24 })
    gsap.to(drillInPanelRef.value, { opacity: 1, x: 0, duration: 0.3, ease: 'power3.out', clearProps: 'opacity,x' })

    // Cascade: tags row → scan bar → table/empty
    const tagsRow = drillInPanelRef.value.querySelector('.drill-in-tags')
    const scanBar = drillInPanelRef.value.querySelector('.drill-in-scan')
    const table = drillInPanelRef.value.querySelector('.drill-in-table, .drill-in-empty')

    const cascade = [tagsRow, scanBar, table].filter(Boolean)
    gsap.set(cascade, { opacity: 0, y: 10 })
    gsap.to(cascade, {
      opacity: 1,
      y: 0,
      duration: 0.28,
      stagger: 0.07,
      ease: 'power2.out',
      delay: 0.1,
      clearProps: 'opacity,y'
    })
  }

  /**
   * Stagger-in all entry rows when lab entries first load.
   */
  function animateEntryRows() {
    const rows = entryTbodyRef.value?.querySelectorAll('.entry-row')
    if (!rows?.length) return
    gsap.set(rows, { opacity: 0, y: 8 })
    gsap.to(rows, {
      opacity: 1,
      y: 0,
      duration: 0.25,
      stagger: 0.04,
      ease: 'power2.out',
      clearProps: 'opacity,y'
    })
  }

  /**
   * Pop-in just the first row (newest entry) after adding.
   */
  function animateNewEntryRow() {
    const firstRow = entryTbodyRef.value?.querySelector('.entry-row')
    if (!firstRow) return
    gsap.set(firstRow, { opacity: 0, scale: 0.96, backgroundColor: 'var(--color-primary-soft)' })
    gsap.to(firstRow, {
      opacity: 1,
      scale: 1,
      duration: 0.3,
      ease: 'back.out(1.6)',
      clearProps: 'opacity,scale'
    })
    // Fade the highlight back out
    gsap.to(firstRow, {
      backgroundColor: 'transparent',
      duration: 0.8,
      delay: 0.4,
      ease: 'power1.out',
      clearProps: 'backgroundColor'
    })
  }

  /**
   * Stagger-in comment cards.
   */
  function animateCommentList() {
    const items = commentListRef.value?.querySelectorAll('.comment-item')
    if (!items?.length) return
    gsap.set(items, { opacity: 0, y: 10 })
    gsap.to(items, {
      opacity: 1,
      y: 0,
      duration: 0.28,
      stagger: 0.06,
      ease: 'power2.out',
      clearProps: 'opacity,y'
    })
  }

  /**
   * Modal open: backdrop fade + card scale-in.
   */
  function animateModalIn(overlayEl) {
    if (!overlayEl) return
    const backdrop = overlayEl.querySelector('.modal-backdrop')
    const card = overlayEl.querySelector('.modal-card')

    if (backdrop) {
      gsap.set(backdrop, { opacity: 0 })
      gsap.to(backdrop, { opacity: 1, duration: 0.2, ease: 'power1.out' })
    }
    if (card) {
      gsap.set(card, { opacity: 0, scale: 0.88, y: 16 })
      gsap.to(card, { opacity: 1, scale: 1, y: 0, duration: 0.28, ease: 'back.out(1.5)', clearProps: 'opacity,scale,y' })
    }
  }

  /**
   * Modal close: card scale-out + backdrop fade, then callback.
   */
  function animateModalOut(overlayEl, onComplete) {
    if (!overlayEl) { onComplete?.(); return }
    const backdrop = overlayEl.querySelector('.modal-backdrop')
    const card = overlayEl.querySelector('.modal-card')

    const tl = gsap.timeline({ onComplete })
    if (card) tl.to(card, { opacity: 0, scale: 0.9, y: 10, duration: 0.18, ease: 'power2.in' }, 0)
    if (backdrop) tl.to(backdrop, { opacity: 0, duration: 0.2, ease: 'power1.in' }, 0)
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
    const [y, m, d] = String(val).split('-')
    return new Date(y, m - 1, d).toLocaleDateString('en-US', {
      month: 'short', day: 'numeric', year: 'numeric'
    })
  }

  // ── Init ───────────────────────────────────────────────────────────────────

  onMounted(async () => {
    // Animate the empty state on first render
    await nextTick()
    if (emptyStateRef.value) {
      gsap.set(emptyStateRef.value, { opacity: 0, y: 16 })
      gsap.to(emptyStateRef.value, { opacity: 1, y: 0, duration: 0.4, ease: 'power2.out', delay: 0.1, clearProps: 'opacity,y' })
    }
    loadFolders()
  })
</script>

<style scoped>
  /* Entry row hover highlight */
  .entry-row {
    transition: background-color 0.15s ease;
  }

    .entry-row:hover {
      background-color: var(--color-surface-low);
    }
</style>
