import api from './axiosInstance'

const BASE_URL = '/api/TestGroupOverride'

export const testGroupOverrideApi = {
  getAll: () =>
    api.get(`${BASE_URL}`).then(r => r.data),

  add: (data) =>
    api.post(`${BASE_URL}`, data).then(r => r.data),

  update: (id, data) =>
    api.put(`${BASE_URL}/${id}`, data).then(r => r.data),

  toggle: (id) =>
    api.patch(`${BASE_URL}/${id}/toggle`).then(r => r.data),

  remove: (id) =>
    api.delete(`${BASE_URL}/${id}`).then(r => r.data),
}
