import api from './axiosInstance'

const BASE_URL = '/api/Contingency'

export const contingencyApi = {
  // Config (admin)
  getConfig: () => api.get(`${BASE_URL}/config`).then(r => r.data),
  upsertConfig: (data) => api.put(`${BASE_URL}/config`, data).then(r => r.data),

  // Sample types
  getSampleTypes: () => api.get(`${BASE_URL}/sample-types`).then(r => r.data),

  // Endorser
  endorse: (data) => api.post(`${BASE_URL}/endorse`, data).then(r => r.data),
  exportExcel: (batchNo) => api.get(`${BASE_URL}/export/${batchNo}`, { responseType: 'blob' }),
  getEndorsedBatches: () => api.get(`${BASE_URL}/endorsed-batches`).then(r => r.data),

  // Receiver
  getBatches: () => api.get(`${BASE_URL}/batches`).then(r => r.data),
  getBatchDetail: (id) => api.get(`${BASE_URL}/batches/${id}`).then(r => r.data),
  scanSpecimen: (data) => api.post(`${BASE_URL}/scan`, data).then(r => r.data),
  importExcel: (formData) => api.post(`${BASE_URL}/import`, formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  }).then(r => r.data),
}
