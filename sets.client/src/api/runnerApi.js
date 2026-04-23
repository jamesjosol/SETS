import api from './axiosInstance'

const BASE_URL = '/api/Runner'

export const runnerApi = {
  // Pending Specimens
  getPendingSpecimens: (sectionCode) => api.get(`${BASE_URL}/pending`, { params: { sectionCode } }).then(r => r.data),
  getTestsByHeader: (headerId) => api.get(`${BASE_URL}/tests/${headerId}`).then(r => r.data),
  // Scheduled Specimens
  getScheduledSpecimens: (sectionCode) => api.get(`${BASE_URL}/scheduled`, { params: { sectionCode } }).then(r => r.data),
  rescheduleTest: (payload) => api.patch(`${BASE_URL}/reschedule`, payload),
  getRunningSpecimens: (sectionCode, allUsers = false) => api.get(`${BASE_URL}/running`, { params: { sectionCode, allUsers } }).then(r => r.data),
  // Assign RMT / Section Receiving
  scanSpecimen: (payload) => api.post(`${BASE_URL}/scan`, payload).then(r => r.data),
  saveAssignments: (payload) => api.post(`${BASE_URL}/assign`, payload),

  getDashboardSummary: (sectionCode) => api.get(`${BASE_URL}/dashboard-summary`, { params: { sectionCode } }).then(r => r.data),
  getAllSectionsSummary: () => api.get(`${BASE_URL}/dashboard-summary/all`).then(r => r.data),// Admin endpoints
  getAdminRunning: () => api.get(`${BASE_URL}/admin/running`).then(r => r.data),
  getAdminRecentlyRouted: () => api.get(`${BASE_URL}/admin/recently-routed`).then(r => r.data),
  getAdminDueToday: () => api.get(`${BASE_URL}/admin/due-today`).then(r => r.data),
  getAdminCompletedToday: () => api.get(`${BASE_URL}/admin/completed-today`).then(r => r.data),
  getAdminPending: () => api.get(`${BASE_URL}/admin/pending`).then(r => r.data),
  getAdminScheduled: () => api.get(`${BASE_URL}/admin/scheduled`).then(r => r.data),
}
