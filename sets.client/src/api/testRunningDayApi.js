import api from './axiosInstance'

const BASE_URL = '/api/TestRunningDay'
export const testRunningDayApi = {
  getAll: () =>
    api.get(`${BASE_URL}`),
  search: (param) =>
    api.get(`${BASE_URL}/search`, { params: { param } }),

  add: (data) =>
    api.post(`${BASE_URL}`, data),

  update: (id, data) =>
    api.put(`${BASE_URL}/${id}`, data),

  delete: (id) =>
    api.delete(`${BASE_URL}/${id}`),
}
