import api from './axiosInstance'

const BASE_URL = '/api/Announcement'

export const announcementApi = {
  getActive: () => api.get(`${BASE_URL}/active`).then(r => r.data),
  getAll: () => api.get(`${BASE_URL}`).then(r => r.data),
  create: (data) => api.post(`${BASE_URL}`, data).then(r => r.data),
  deactivate: (id) => api.patch(`${BASE_URL}/${id}/deactivate`).then(r => r.data),
}
