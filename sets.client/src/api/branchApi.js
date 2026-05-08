import api from './axiosInstance'

const BASE_URL = '/api/Settings/branches'

export const branchApi = {
  getWithConfigStatus: () => api.get(`${BASE_URL}/config-status`).then(r => r.data),
  add: (code) => api.post(`${BASE_URL}`, { code }).then(r => r.data),
  toggle: (code) => api.patch(`${BASE_URL}/${code}/toggle`).then(r => r.data),
  checkConfig: (code) => api.get(`${BASE_URL}/check-config`, { params: { code } }).then(r => r.data),
  getAll: () => api.get('/api/Settings/branches').then(r => r.data),
}
