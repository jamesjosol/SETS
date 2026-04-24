import api from './axiosInstance'

export const pcApi = {
  // GET all PCs with their sections
  getAll: () =>
    api.get('/api/pc'),

  // POST register new PC
  add: (data) =>
    api.post('/api/pc', data),

  // PUT update PC info (description, IP, active)
  update: (id, data) =>
    api.put(`/api/pc/${id}`, data),

  // PATCH toggle active/inactive
  toggle: (id) =>
    api.patch(`/api/pc/${id}/toggle`),

  // PUT replace all section assignments for a PC
  updateSections: (id, data) =>
    api.put(`/api/pc/${id}/sections`, data),
}
