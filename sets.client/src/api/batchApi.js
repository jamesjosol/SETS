import api from './axiosInstance'

const BATCH_URL = '/api/Batch'

export const batchApi = {
  // Regular / TL — summary for own section
  getDashboardSummary: (sectionCode) =>
    api.get(`${BATCH_URL}/dashboard-summary`, { params: { sectionCode } }).then(r => r.data),

  // Admin — summary for all sections in current branch
  getAllSectionsSummary: () =>
    api.get(`${BATCH_URL}/dashboard-summary/all`).then(r => r.data),

  // Regular / TL — top 5 recent batches for own section
  getRecentBatches: (sectionCode) =>
    api.get(`${BATCH_URL}/recent`, { params: { sectionCode } }).then(r => r.data),

  // Admin — top 5 consolidated recent batches across all endorser sections
  getAllSectionsRecentBatches: () =>
    api.get(`${BATCH_URL}/recent/all`).then(r => r.data),

  // Batch detail for drawer
  getBatchDetail: (batchNo) =>
    api.get(`${BATCH_URL}/${batchNo}/detail`).then(r => r.data),

  // Regular / TL — weekly batch flow for own section
  getWeeklyFlow: (sectionCode) =>
    api.get(`${BATCH_URL}/weekly-flow`, { params: { sectionCode } }).then(r => r.data),

  // Admin — consolidated weekly flow across all endorser sections
  getAllSectionsWeeklyFlow: () =>
    api.get(`${BATCH_URL}/weekly-flow/all`).then(r => r.data),

  // Regular / TL — all endorsements for own section (archive)
  getEndorsements: (sectionCode, dateFrom, dateTo) => api.get(`${BATCH_URL}/endorsements`, { params: { sectionCode, dateFrom, dateTo } }).then(r => r.data),

  getAllEndorsements: (dateFrom, dateTo) => api.get(`${BATCH_URL}/endorsements/all`, { params: { dateFrom, dateTo } }).then(r => r.data),
}
