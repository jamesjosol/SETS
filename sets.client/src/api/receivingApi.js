import api from './axiosInstance'

const BASE_URL = '/api/Receiving'

export const receivingApi = {
  receiveSpecimen: (payload) => api.post(`${BASE_URL}/specimen`, payload),
  receiveNonBarcoded: (payload) => api.post(`${BASE_URL}/nonbarcoded`, payload),
  getPendingNonBarcoded: (sectionCode) =>
    api.get(`${BASE_URL}/pending-nonbarcoded`, { params: { sectionCode } }).then(r => r.data),
  getDashboardSummary: (sectionCode) =>
    api.get(`${BASE_URL}/dashboard-summary`, { params: { sectionCode } }).then(r => r.data),
  getMonitoringDashboard: (sectionCode) =>
    api.get(`${BASE_URL}/monitoring`, { params: { sectionCode } }).then(r => r.data),
  getWeeklyFlow: (sectionCode) =>
    api.get(`${BASE_URL}/weekly-flow`, { params: { sectionCode } }).then(r => r.data),
}
