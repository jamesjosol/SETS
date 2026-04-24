import axios from 'axios'

export const sectionApi = {
  // GET all sections
  getAll: () =>
    axios.get('/api/section'),

  // POST create section
  add: (data) =>
    axios.post('/api/section', data),

  // PUT update section name / autoNo
  update: (code, data) =>
    axios.put(`/api/section/${code}`, data),

  // PATCH toggle active/inactive
  toggle: (code) =>
    axios.patch(`/api/section/${code}/toggle`),

  // GET duplicate code check
  checkCode: (code) =>
    axios.get(`/api/section/check-code/${code}`),

  getTestGroups: () => axios.get('/api/section/test-groups'),
}
