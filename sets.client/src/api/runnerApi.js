import api from './axiosInstance'

const BASE_URL = '/api/Runner'

export const runnerApi = {
  // Pending Specimens
  getPendingSpecimens: (sectionCode) =>
    api.get(`${BASE_URL}/pending`, { params: { sectionCode } }).then(r => r.data),
  getTestsByHeader: (headerId) =>
    api.get(`${BASE_URL}/tests/${headerId}`).then(r => r.data),

  // Saved Specimens
  getSavedSpecimens: (sectionCode) =>
    api.get(`${BASE_URL}/saved`, { params: { sectionCode } }).then(r => r.data),
  rescheduleTest: (payload) =>
    api.patch(`${BASE_URL}/reschedule`, payload),

  // Assign RMT / Section Receiving
  scanSpecimen: (payload) =>
    api.post(`${BASE_URL}/scan`, payload).then(r => r.data),
  saveAssignments: (payload) =>
    api.post(`${BASE_URL}/assign`, payload),

}
