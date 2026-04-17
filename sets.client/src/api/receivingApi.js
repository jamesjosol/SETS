import api from './axiosInstance'

const BASE_URL = '/api/Receiving'

export const receivingApi = {
  receiveSpecimen: (payload) => api.post(`${BASE_URL}/specimen`, payload),
  updateSpecimenRemarks: (payload) => api.patch(`${BASE_URL}/specimen-remarks`, payload),
  receiveNonBarcoded: (payload) => api.post(`${BASE_URL}/nonbarcoded`, payload),
  updateNonBarcodedRemarks: (payload) => api.patch(`${BASE_URL}/nonbarcoded-remarks`, payload),
  getPendingNonBarcoded: (sectionCode) => api.get(`${BASE_URL}/pending-nonbarcoded`, { params: { sectionCode } }).then(r => r.data),
  getDashboardSummary: (sectionCode) => api.get(`${BASE_URL}/dashboard-summary`, { params: { sectionCode } }).then(r => r.data),
  getMonitoringDashboard: (sectionCode) => api.get(`${BASE_URL}/monitoring`, { params: { sectionCode } }).then(r => r.data),
  getWeeklyFlow: (sectionCode) => api.get(`${BASE_URL}/weekly-flow`, { params: { sectionCode } }).then(r => r.data),
  getHourlyFlow: (sectionCode) => api.get(`${BASE_URL}/hourly-flow`, { params: { sectionCode } }).then(r => r.data),
  getIncomingBatches: (sectionCode) => api.get(`${BASE_URL}/incoming-batches`, { params: { sectionCode } }).then(r => r.data),
  getIncomingSpecimens: (sectionCode) => api.get(`${BASE_URL}/incoming-specimens`, { params: { sectionCode } }).then(r => r.data),
}
