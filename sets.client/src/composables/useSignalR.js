// src/composables/useSignalR.js
//
// Two exports:
//
//   useSignalR              — for Vue components (auto lifecycle via onUnmounted)
//   createSignalRConnection — for Pinia stores (manual start/stop)
//
// ── Component usage ───────────────────────────────────────────────────────────
//
//   const { on, invoke, status } = useSignalR('/hubs/contingency', branchCode)
//   on('NewContingencyBatch', () => loadBatches())
//
// ── Store usage ───────────────────────────────────────────────────────────────
//
//   const conn = createSignalRConnection('/hubs/announcement', branchCode, () => this.fetch())
//   conn.on('AnnouncementUpdated', (data) => { ... })
//   await conn.start()
//   // later:
//   await conn.stop(branchCode)
//
// ─────────────────────────────────────────────────────────────────────────────

import { ref, onUnmounted } from 'vue'
import * as signalR from '@microsoft/signalr'

// ── Internal factory ──────────────────────────────────────────────────────────
// Builds the raw HubConnection and wires up group management + reconnect logic.
// Both exports share this so behaviour is identical.

function _buildConnection(hubUrl, branchCode, onReconnect = null) {
  const status = ref('disconnected')

  const connection = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl)
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Warning)
    .build()

  connection.onreconnecting(() => {
    status.value = 'reconnecting'
  })

  connection.onreconnected(async () => {
    status.value = 'connected'
    // withAutomaticReconnect() restores the transport but NOT group membership
    await connection.invoke('JoinBranch', branchCode).catch(() => { })
    if (onReconnect) await onReconnect()
  })

  connection.onclose(() => {
    status.value = 'disconnected'
  })

  async function start() {
    status.value = 'connecting'
    try {
      await connection.start()
      status.value = 'connected'
      await connection.invoke('JoinBranch', branchCode).catch(() => { })
    } catch (err) {
      status.value = 'disconnected'
      console.error(`[useSignalR] Failed to connect to ${hubUrl}:`, err)
    }
  }

  async function stop(branch = branchCode) {
    if (connection.state === signalR.HubConnectionState.Disconnected) return
    await connection.invoke('LeaveBranch', branch).catch(() => { })
    await connection.stop().catch(() => { })
    status.value = 'disconnected'
  }

  function on(event, handler) {
    connection.on(event, handler)
  }

  async function invoke(method, ...args) {
    try {
      return await connection.invoke(method, ...args)
    } catch (err) {
      console.warn(`[useSignalR] invoke('${method}') failed:`, err)
    }
  }

  return { on, invoke, start, stop, status }
}

// ── For Vue components ────────────────────────────────────────────────────────
// Starts automatically and cleans up on component unmount.

export function useSignalR(hubUrl, branchCode, onReconnect = null) {
  const conn = _buildConnection(hubUrl, branchCode, onReconnect)

  conn.start()
  onUnmounted(() => conn.stop())

  return { on: conn.on, invoke: conn.invoke, status: conn.status }
}

// ── For Pinia stores ──────────────────────────────────────────────────────────
// Returns the connection object with manual start/stop — the store controls
// the lifecycle via its own connectSignalR / disconnectSignalR actions.

export function createSignalRConnection(hubUrl, branchCode, onReconnect = null) {
  return _buildConnection(hubUrl, branchCode, onReconnect)
}
