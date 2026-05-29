import axios from 'axios'

// Dedicated axios instance — no credentials, no session cookie,
// no 401 interceptor redirect. Kiosk is fully public.
const kioskHttp = axios.create()

const BASE_URL = '/api/Kiosk'

export const kioskApi = {
  getSummary: () => kioskHttp.get(`${BASE_URL}/summary`).then(r => r.data),
  getMonitoring: () => kioskHttp.get(`${BASE_URL}/monitoring`).then(r => r.data),
}
