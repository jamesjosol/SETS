import api from './axiosInstance'

const BASE_URL = '/api/Tat'

export const tatApi = {
  getAll: () => api.get(`${BASE_URL}`).then(r => r.data),
  getBySection: (sectionCode) => api.get(`${BASE_URL}/${sectionCode}`).then(r => r.data),
  upsert: (items) => api.put(`${BASE_URL}`, items).then(r => r.data),
  getOpenCycle: (sectionCode) => api.get(`${BASE_URL}/${sectionCode}/cycle`).then(r => r.data),
  appeal: (sectionCode) => api.post(`${BASE_URL}/${sectionCode}/appeal`).then(r => r.data),
  getCycleLogs: (params) => api.get(`${BASE_URL}/cycle-logs`, { params }).then(r => r.data),
  getProcessing: () => api.get('/api/tat/processing'),
  upsertProcessing: (data) =>  api.put('/api/tat/processing', data),
}
