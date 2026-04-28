import api from './axiosInstance'

export const sectionApi = {
  // GET all sections
  getAll: () =>
    api.get('/api/section'),

  // POST create section
  add: (data) =>
    api.post('/api/section', data),

  // PUT update section name / autoNo
  update: (code, data) =>
    api.put(`/api/section/${code}`, data),

  // PATCH toggle active/inactive
  toggle: (code) =>
    api.patch(`/api/section/${code}/toggle`),

  // GET duplicate code check
  checkCode: (code) =>
    api.get(`/api/section/check-code/${code}`),

  getTestGroups: () => api.get('/api/section/test-groups'),
}
