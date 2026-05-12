import api from './axiosInstance'

const BASE_URL = '/api/EndorsementSetup'

export default {
  getPartners() {
    return api.get(`${BASE_URL}/partners`).then(r => r.data)
  },

  getEligibleBranches() {
    return api.get(`${BASE_URL}/eligible-branches`).then(r => r.data)
  },

  registerPartner(branchCode) {
    return api.post(`${BASE_URL}/partners`, { branchCode }).then(r => r.data)
  },

  togglePartner(branchCode) {
    return api.patch(`${BASE_URL}/partners/${branchCode}/toggle`).then(r => r.data)
  },

  unregisterPartner(branchCode) {
    return api.delete(`${BASE_URL}/partners/${branchCode}`).then(r => r.data)
  },

  lookupSection(branchCode, sectionCode) {
    return api.get(`${BASE_URL}/partners/${branchCode}/lookup-section`, {
      params: { code: sectionCode }
    }).then(r => r.data)
  },

  addSection(branchCode, payload) {
    return api.post(`${BASE_URL}/partners/${branchCode}/sections`, payload).then(r => r.data)
  },

  removeSection(branchCode, sectionCode) {
    return api.delete(`${BASE_URL}/partners/${branchCode}/sections/${sectionCode}`).then(r => r.data)
  },

  checkPartner(branchCode, sectionCode) {
    return api.get(`${BASE_URL}/partners/${branchCode}/check`, {
      params: { sectionCode }
    }).then(r => r.data)
  },

  getActivePartners() {
    return api.get(`${BASE_URL}/active-partners`).then(r => r.data)
  },

  getSettings() {
    return api.get(`${BASE_URL}/settings`).then(r => r.data)
  },

  setOutbound(enabled) {
    return api.patch(`${BASE_URL}/settings/outbound`, { enabled }).then(r => r.data)
  },
}
