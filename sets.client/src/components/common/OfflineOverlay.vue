<template>
  <Teleport to="body">
    <Transition name="offline-fade">
      <div v-if="show" class="offline-overlay">
        <div class="container">

          <div class="brand">
            <div class="brand-top">
              <svg width="40" height="40" viewBox="0 0 60 52" fill="none">
                <path d="M4 8 L4 36 Q4 46 10 46 Q16 46 16 36 L16 8" stroke="#7c4dff" stroke-width="2" fill="none" />
                <line x1="2" y1="8" x2="18" y2="8" stroke="#7c4dff" stroke-width="2.5" stroke-linecap="round" />
                <path d="M4 28 L4 36 Q4 46 10 46 Q16 46 16 36 L16 28 Z" fill="#7c4dff" opacity="0.4" />
                <path d="M22 4 L22 36 Q22 46 28 46 Q34 46 34 36 L34 4" stroke="#7c4dff" stroke-width="2" fill="none" />
                <line x1="20" y1="4" x2="36" y2="4" stroke="#7c4dff" stroke-width="2.5" stroke-linecap="round" />
                <path d="M22 20 L22 36 Q22 46 28 46 Q34 46 34 36 L34 20 Z" fill="#7c4dff" opacity="0.4" />
                <circle cx="26" cy="30" r="1.5" fill="#7c4dff" opacity="0.7" />
                <circle cx="30" cy="35" r="1" fill="#7c4dff" opacity="0.5" />
                <path d="M40 10 L40 36 Q40 46 46 46 Q52 46 52 36 L52 10" stroke="#7c4dff" stroke-width="2" fill="none" />
                <line x1="38" y1="10" x2="54" y2="10" stroke="#7c4dff" stroke-width="2.5" stroke-linecap="round" />
                <path d="M40 22 L40 36 Q40 46 46 46 Q52 46 52 36 L52 22 Z" fill="#7c4dff" opacity="0.4" />
              </svg>
              <span class="brand-name">SETS</span>
            </div>
            <span class="brand-sub">Specimen Endorsement &amp; Tracking System</span>
          </div>

          <div class="badge">
            <span class="badge-dot" />
            {{ isUpdating ? 'Deploying Update' : 'Server Unavailable' }}
          </div>

          <h1>{{ isUpdating ? 'System is Being Updated' : 'Unable to Reach Server' }}</h1>

          <p class="subtitle">
            {{
 isUpdating
              ? 'A new version of SETS is being deployed. This will only take a moment.'
              : 'The connection to SETS was lost. Attempting to reconnect automatically.'
            }}
          </p>

          <div class="divider" />

          <div v-if="isUpdating" class="progress-wrap">
            <div class="progress-bar" />
          </div>

          <div v-else class="retry-info">
            <span class="retry-dot" :class="{ pulsing: retrying }" />
            <span>{{ retrying ? 'Reconnecting...' : `Retrying in ${countdown}s` }}</span>
          </div>

          <p class="footer">
            {{
 isUpdating
              ? 'You will be redirected automatically once the update is complete.'
              : 'If this persists, please contact the IT Department.'
            }}
          </p>

        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
defineProps({
  show:       { type: Boolean, default: false },
  isUpdating: { type: Boolean, default: false },
  retrying:   { type: Boolean, default: false },
  countdown:  { type: Number,  default: 10    },
})
</script>

<style scoped>
  .offline-overlay {
    position: fixed;
    inset: 0;
    z-index: 99999;
    background-color: #0f1117;
    display: flex;
    align-items: center;
    justify-content: center;
    font-family: 'Segoe UI', system-ui, -apple-system, sans-serif;
  }

  .container {
    text-align: center;
    padding: 56px 32px;
    max-width: 500px;
    width: 100%;
  }

  /* Brand */
  .brand {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 10px;
    margin-bottom: 48px;
  }

  .brand-top {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 12px;
  }

  .brand-name {
    font-size: 22px;
    font-weight: 500;
    letter-spacing: -0.03em;
    color: #7c4dff;
    line-height: 1;
  }

  .brand-sub {
    font-size: 9px;
    font-weight: 700;
    letter-spacing: 0.12em;
    text-transform: uppercase;
    color: #5a6478;
    line-height: 1.5;
    text-align: center;
  }

  /* Badge */
  .badge {
    display: inline-flex;
    align-items: center;
    gap: 7px;
    background: #0f2027;
    color: #4f8ef7;
    font-size: 11px;
    font-weight: 700;
    letter-spacing: 0.08em;
    text-transform: uppercase;
    padding: 6px 14px;
    border-radius: 20px;
    border: 1px solid #1e3a6e;
    margin-bottom: 28px;
  }

  .badge-dot {
    width: 6px;
    height: 6px;
    background: #4f8ef7;
    border-radius: 50%;
    animation: pulse 1.2s infinite;
  }

  /* Text */
  h1 {
    font-size: 26px;
    font-weight: 700;
    color: #e8ecf4;
    margin-bottom: 14px;
    line-height: 1.3;
  }

  .subtitle {
    font-size: 14px;
    color: #8b95aa;
    line-height: 1.75;
    margin-bottom: 40px;
  }

  .divider {
    width: 40px;
    height: 2px;
    background: #252d40;
    margin: 0 auto 40px;
    border-radius: 2px;
  }

  .footer {
    font-size: 12px;
    color: #5a6478;
    line-height: 1.7;
    margin-top: 24px;
  }

  /* Progress bar (updating state) */
  .progress-wrap {
    background: #161b24;
    border: 1px solid #252d40;
    border-radius: 100px;
    height: 6px;
    overflow: hidden;
    margin-bottom: 40px;
  }

  .progress-bar {
    height: 100%;
    width: 60%;
    background: linear-gradient(90deg, #4f8ef7, #7c4dff);
    border-radius: 100px;
    animation: progress 2s ease-in-out infinite alternate;
  }

  /* Retry indicator (connection lost state) */
  .retry-info {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    background: #161b24;
    border: 1px solid #252d40;
    border-radius: 20px;
    padding: 8px 18px;
    font-size: 12px;
    color: #8b95aa;
    margin-bottom: 8px;
  }

  .retry-dot {
    width: 7px;
    height: 7px;
    background: #5a6478;
    border-radius: 50%;
    transition: background 0.3s;
  }

    .retry-dot.pulsing {
      background: #4f8ef7;
      animation: pulse 0.8s infinite;
    }

  /* Animations */
  @keyframes pulse {
    0%,100% {
      opacity: 1
    }

    50% {
      opacity: 0.2
    }
  }

  @keyframes progress {
    0% {
      width: 30%
    }

    100% {
      width: 85%
    }
  }

  /* Transition */
  .offline-fade-enter-active, .offline-fade-leave-active {
    transition: opacity 0.3s ease;
  }

  .offline-fade-enter-from, .offline-fade-leave-to {
    opacity: 0;
  }
</style>
