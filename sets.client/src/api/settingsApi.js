import axios from 'axios'

export const settingsApi = {
  // GET all sections (all statuses)
  getSections: () =>
    axios.get('/api/settings/sections'),

  // GET all branches
  getBranches: () =>
    axios.get('/api/settings/branches'),
}
