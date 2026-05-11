import api from './axiosInstance'

const BASE_URL = '/api/Report'

// ── Shared Excel blob download helper ────────────────────────────────────────
async function downloadExcel(endpoint, payload, filename) {
  const dateStr = new Date().toISOString().slice(0, 10).replace(/-/g, '')
  const response = await api.post(endpoint, payload, { responseType: 'blob' })
  const url = window.URL.createObjectURL(new Blob([response.data]))
  const link = document.createElement('a')
  link.href = url
  link.setAttribute('download', `${filename}_${dateStr}.xlsx`)
  document.body.appendChild(link)
  link.click()
  link.remove()
  window.URL.revokeObjectURL(url)
}

export const reportApi = {

  // R1 — Unprocessed Specimen
  getUnprocessedSpecimens: (payload) =>
    api.post(`${BASE_URL}/unprocessed-specimen`, payload).then(r => r.data),

  // R2 — Specimen Not Endorsed
  getSpecimensNotEndorsed: (payload) =>
    api.post(`${BASE_URL}/specimen-not-endorsed`, payload).then(r => r.data),

  // R3 — Specimen Not Received / Pending
  getSpecimensNotReceived: (payload) =>
    api.post(`${BASE_URL}/specimen-not-received`, payload).then(r => r.data),

  exportSpecimensNotReceivedExcel: (payload) =>
    downloadExcel(`${BASE_URL}/specimen-not-received/export`, payload, 'SpecimenNotReceivedReport'),

  // R4 — Test Management
  getTestManagement: (payload) =>
    api.post(`${BASE_URL}/test-management`, payload).then(r => r.data),

  // R5 — Batch Summary
  getBatchSummary: (payload) =>
    api.post(`${BASE_URL}/batch-summary`, payload).then(r => r.data),

  exportBatchSummaryExcel: (payload) =>
    downloadExcel(`${BASE_URL}/batch-summary/export`, payload, 'BatchSummaryReport'),

  // R6 — Specimen Receipt (Laboratory Section)
  getSpecimenReceiptSection: (payload) =>
    api.post(`${BASE_URL}/specimen-receipt-section`, payload).then(r => r.data),

  exportSpecimenReceiptSectionExcel: (payload) =>
    downloadExcel(`${BASE_URL}/specimen-receipt-section/export`, payload, 'SpecimenReceiptLabSection'),

  // R7 — Duplicate Endorsement
  getDuplicateEndorsements: (payload) =>
    api.post(`${BASE_URL}/duplicate-endorsement`, payload).then(r => r.data),

  exportDuplicateEndorsementsExcel: (payload) =>
    downloadExcel(`${BASE_URL}/duplicate-endorsement/export`, payload, 'DuplicateEndorsementReport'),

  // R8 — Transaction Beyond 14 Days
  getBeyond14Days: (payload) =>
    api.post(`${BASE_URL}/beyond-14-days`, payload).then(r => r.data),

  exportBeyond14DaysExcel: (payload) =>
    downloadExcel(`${BASE_URL}/beyond-14-days/export`, payload, 'Beyond14DaysReport'),

  // R9 — Monthly Endorsement Summary
  getMonthlyEndorsementSummary: (payload) =>
    api.post(`${BASE_URL}/monthly-summary`, payload).then(r => r.data),
}
