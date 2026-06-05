import api from './axiosInstance'

const BASE_URL = '/api/Tat/outbound'

export const tatOutboundApi = {
  // Settings
  getSettings: () => api.get(`${BASE_URL}/settings`).then(r => r.data),
  updateSettings: (data) => api.put(`${BASE_URL}/settings`, data).then(r => r.data),

  // Windows
  getWindows: () => api.get(`${BASE_URL}/windows`).then(r => r.data),
  upsertWindows: (items) => api.put(`${BASE_URL}/windows`, items).then(r => r.data),
  deleteWindow: (id) => api.delete(`${BASE_URL}/windows/${id}`).then(r => r.data),

  // Runtime (endorser)
  getCurrentWindow: () => api.get(`${BASE_URL}/current-window`).then(r => r.data),

  // Logs (audit trail)
  getLogs: (params) => api.get(`${BASE_URL}/logs`, { params }).then(r => r.data),
  appeal: (logId) => api.post(`${BASE_URL}/logs/${logId}/appeal`).then(r => r.data),
}
