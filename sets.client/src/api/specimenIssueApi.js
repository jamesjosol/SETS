import api from './axiosInstance'

const BASE_URL = '/api/SpecimenIssue'

// ── Shared blob download helper (mirrors reportApi pattern) ──────────────────
async function downloadExcel(endpoint, filename) {
  const dateStr = new Date().toISOString().slice(0, 10).replace(/-/g, '')
  const response = await api.get(endpoint, { responseType: 'blob' })
  const url = window.URL.createObjectURL(new Blob([response.data]))
  const link = document.createElement('a')
  link.href = url
  link.setAttribute('download', `${filename}_${dateStr}.xlsx`)
  document.body.appendChild(link)
  link.click()
  link.remove()
  window.URL.revokeObjectURL(url)
}

export const specimenIssueApi = {
  // Incident Types
  getIncidentTypes: () => api.get(`${BASE_URL}/incident-types`).then(r => r.data),
  createIncidentType: (data) => api.post(`${BASE_URL}/incident-types`, data),
  toggleIncidentType: (id) => api.patch(`${BASE_URL}/incident-types/${id}/toggle`),

  // Sub-Categories
  getSubCategories: (incidentTypeId) => api.get(`${BASE_URL}/incident-types/${incidentTypeId}/subcategories`).then(r => r.data),
  createSubCategory: (data) => api.post(`${BASE_URL}/subcategories`, data),
  toggleSubCategory: (id) => api.patch(`${BASE_URL}/subcategories/${id}/toggle`),

  // Tags
  addTag: (data) => api.post(`${BASE_URL}/tags`, data),
  deleteTag: (id) => api.delete(`${BASE_URL}/tags/${id}`),
  getTagSuggestions: () => api.get(`${BASE_URL}/tags/suggestions`).then(r => r.data),

  // Lab Entries
  getLabEntries: (subCategoryId) => api.get(`${BASE_URL}/subcategories/${subCategoryId}/entries`).then(r => r.data),
  addLabEntry: (data) => api.post(`${BASE_URL}/entries`, data),
  deleteLabEntry: (id) => api.delete(`${BASE_URL}/entries/${id}`),
  updateLabEntryRemark: (id, data) => api.patch(`${BASE_URL}/entries/${id}/remark`, data),
  exportIncidentTypeExcel: (incidentTypeId, folderName) =>
    downloadExcel(
      `${BASE_URL}/incident-types/${incidentTypeId}/export`,
      `IssuesLog_${folderName.replace(/[^a-zA-Z0-9]/g, '_')}`
    ),

  // Comments
  getComments: (incidentTypeId) => api.get(`${BASE_URL}/incident-types/${incidentTypeId}/comments`).then(r => r.data),
  addComment: (data) => api.post(`${BASE_URL}/comments`, data),
  editComment: (id, data) => api.patch(`${BASE_URL}/comments/${id}`, data),
}
