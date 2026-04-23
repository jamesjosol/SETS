import axios from 'axios'

export const pcApi = {
  // GET all PCs with their sections
  getAll: () =>
    axios.get('/api/pc'),

  // POST register new PC
  add: (data) =>
    axios.post('/api/pc', data),

  // PUT update PC info (description, IP, active)
  update: (id, data) =>
    axios.put(`/api/pc/${id}`, data),

  // PATCH toggle active/inactive
  toggle: (id) =>
    axios.patch(`/api/pc/${id}/toggle`),

  // PUT replace all section assignments for a PC
  updateSections: (id, data) =>
    axios.put(`/api/pc/${id}/sections`, data),
}
