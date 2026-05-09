import api from './axiosInstance'

const BASE_URL = '/api/Report'

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

  // R4 — Test Management
  getTestManagement: (payload) =>
    api.post(`${BASE_URL}/test-management`, payload).then(r => r.data),

  // R5 — Batch Summary (data preview)
  getBatchSummary: (payload) =>
    api.post(`${BASE_URL}/batch-summary`, payload).then(r => r.data),

  // R5 — Batch Summary Excel export (server-side ClosedXML, blob download)
  exportBatchSummaryExcel: async (payload) => {
    const response = await api.post(`${BASE_URL}/batch-summary/export`, payload, {
      responseType: 'blob',
    })
    const url = window.URL.createObjectURL(new Blob([response.data]))
    const link = document.createElement('a')
    const dateStr = new Date().toISOString().slice(0, 10).replace(/-/g, '')
    link.href = url
    link.setAttribute('download', `BatchSummaryReport_${dateStr}.xlsx`)
    document.body.appendChild(link)
    link.click()
    link.remove()
    window.URL.revokeObjectURL(url)
  },

  // R6 — Specimen Receipt (Section)
  getSpecimenReceiptSection: (payload) =>
    api.post(`${BASE_URL}/specimen-receipt-section`, payload).then(r => r.data),

  // R7 — Duplicate Endorsement
  getDuplicateEndorsements: (payload) =>
    api.post(`${BASE_URL}/duplicate-endorsement`, payload).then(r => r.data),

  // R8 — Transaction Beyond 14 Days
  getBeyond14Days: (payload) =>
    api.post(`${BASE_URL}/beyond-14-days`, payload).then(r => r.data),

  // R9 — Monthly Endorsement Summary
  getMonthlyEndorsementSummary: (payload) =>
    api.post(`${BASE_URL}/monthly-summary`, payload).then(r => r.data),
}
