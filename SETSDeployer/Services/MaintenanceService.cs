using System.IO;
using SETSDeployer.Models;

namespace SETSDeployer.Services
{
    public class MaintenanceService
    {
        private readonly Action<string, LogLevel> _log;
        private const string APP_OFFLINE = "app_offline.htm";

        public MaintenanceService(Action<string, LogLevel> log)
        {
            _log = log;
        }

        public async Task<bool> EnableAsync(BranchConfig branch, CancellationToken ct = default)
        {
            if (!IsPathValid(branch)) return false;

            var target = Path.Combine(branch.UncPath, APP_OFFLINE);
            try
            {
                await File.WriteAllTextAsync(target, BuildMaintenancePage(), ct);
                _log($"[{branch.Name}] ✓ Maintenance mode enabled.", LogLevel.Success);
                return true;
            }
            catch (Exception ex)
            {
                _log($"[{branch.Name}] Failed to enable maintenance: {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        public Task<bool> DisableAsync(BranchConfig branch)
        {
            if (!IsPathValid(branch)) return Task.FromResult(false);

            var target = Path.Combine(branch.UncPath, APP_OFFLINE);
            try
            {
                if (File.Exists(target))
                    File.Delete(target);
                _log($"[{branch.Name}] ✓ Maintenance mode disabled.", LogLevel.Success);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _log($"[{branch.Name}] Failed to disable maintenance: {ex.Message}", LogLevel.Error);
                return Task.FromResult(false);
            }
        }

        public static bool IsInMaintenance(BranchConfig branch)
        {
            if (string.IsNullOrWhiteSpace(branch.UncPath) ||
                branch.UncPath.Equals("NONE", StringComparison.OrdinalIgnoreCase))
                return false;

            try { return File.Exists(Path.Combine(branch.UncPath, APP_OFFLINE)); }
            catch { return false; }
        }

        private bool IsPathValid(BranchConfig branch)
        {
            if (string.IsNullOrWhiteSpace(branch.UncPath) ||
                branch.UncPath.Equals("NONE", StringComparison.OrdinalIgnoreCase))
            {
                _log($"[{branch.Name}] UNC path not configured — cannot toggle maintenance.", LogLevel.Error);
                return false;
            }
            if (!Directory.Exists(branch.UncPath))
            {
                _log($"[{branch.Name}] UNC path not reachable: {branch.UncPath}", LogLevel.Error);
                return false;
            }
            return true;
        }

        private static string BuildMaintenancePage() => """
            <!DOCTYPE html>
            <html>
            <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <meta http-equiv="refresh" content="60">
            <title>SETS — Under Maintenance</title>
            <style>
            *{box-sizing:border-box;margin:0;padding:0}
            body{min-height:100vh;background-color:#0f1117;display:flex;align-items:center;justify-content:center;font-family:'Segoe UI',system-ui,-apple-system,sans-serif;color:#e8ecf4}
            .container{text-align:center;padding:56px 32px;max-width:500px;width:100%}
            .brand{display:flex;flex-direction:column;align-items:center;gap:10px;margin-bottom:48px}
            .brand-top{display:flex;align-items:center;justify-content:center;gap:12px}
            .brand-name{font-size:22px;font-weight:500;letter-spacing:-0.03em;color:#7c4dff;line-height:1}
            .brand-sub{font-size:9px;font-weight:700;letter-spacing:0.12em;text-transform:uppercase;color:#5a6478;margin-top:2px;line-height:1.5;text-align:center}
            .badge{display:inline-flex;align-items:center;gap:7px;background:#1a1035;color:#7c4dff;font-size:11px;font-weight:700;letter-spacing:0.08em;text-transform:uppercase;padding:6px 14px;border-radius:20px;border:1px solid #2e1f6e;margin-bottom:28px}
            .badge-dot{width:6px;height:6px;background:#7c4dff;border-radius:50%;animation:pulse 2s infinite}
            @keyframes pulse{0%,100%{opacity:1}50%{opacity:0.25}}
            h1{font-size:26px;font-weight:700;color:#e8ecf4;margin-bottom:14px;line-height:1.3}
            .subtitle{font-size:14px;color:#8b95aa;line-height:1.75;margin-bottom:40px}
            .divider{width:40px;height:2px;background:#252d40;margin:0 auto 40px;border-radius:2px}
            .info-grid{display:grid;grid-template-columns:1fr 1fr;gap:10px;margin-bottom:40px}
            .info-card{background:#161b24;border:1px solid #252d40;border-radius:10px;padding:16px;text-align:left}
            .info-label{font-size:10px;font-weight:700;letter-spacing:0.1em;text-transform:uppercase;color:#5a6478;margin-bottom:6px}
            .info-value{font-size:13px;color:#c8d0e0;font-weight:500}
            .footer{font-size:12px;color:#5a6478;line-height:1.7}
            .footer strong{color:#8b95aa;font-weight:600}
            </style>
            </head>
            <body>
            <div class="container">
              <div class="brand">
                <div class="brand-top">
                  <svg width="40" height="40" viewBox="0 0 60 52" fill="none" xmlns="http://www.w3.org/2000/svg">
                    <path d="M4 8 L4 36 Q4 46 10 46 Q16 46 16 36 L16 8" stroke="#7c4dff" stroke-width="2" fill="none"/>
                    <line x1="2" y1="8" x2="18" y2="8" stroke="#7c4dff" stroke-width="2.5" stroke-linecap="round"/>
                    <path d="M4 28 L4 36 Q4 46 10 46 Q16 46 16 36 L16 28 Z" fill="#7c4dff" opacity="0.4"/>
                    <path d="M22 4 L22 36 Q22 46 28 46 Q34 46 34 36 L34 4" stroke="#7c4dff" stroke-width="2" fill="none"/>
                    <line x1="20" y1="4" x2="36" y2="4" stroke="#7c4dff" stroke-width="2.5" stroke-linecap="round"/>
                    <path d="M22 20 L22 36 Q22 46 28 46 Q34 46 34 36 L34 20 Z" fill="#7c4dff" opacity="0.4"/>
                    <circle cx="26" cy="30" r="1.5" fill="#7c4dff" opacity="0.7"/>
                    <circle cx="30" cy="35" r="1" fill="#7c4dff" opacity="0.5"/>
                    <path d="M40 10 L40 36 Q40 46 46 46 Q52 46 52 36 L52 10" stroke="#7c4dff" stroke-width="2" fill="none"/>
                    <line x1="38" y1="10" x2="54" y2="10" stroke="#7c4dff" stroke-width="2.5" stroke-linecap="round"/>
                    <path d="M40 22 L40 36 Q40 46 46 46 Q52 46 52 36 L52 22 Z" fill="#7c4dff" opacity="0.4"/>
                  </svg>
                  <div class="brand-name">SETS</div>
                </div>
                <div class="brand-sub">Specimen Endorsement &amp; Tracking System</div>
              </div>
              <div class="badge"><span class="badge-dot"></span>Scheduled Maintenance</div>
              <h1>System Under Maintenance</h1>
              <p class="subtitle">The Specimen Endorsement and Tracking System is currently undergoing scheduled maintenance. We&apos;ll be back shortly.</p>
              <div class="divider"></div>
              <div class="info-grid">
                <div class="info-card"><div class="info-label">Status</div><div class="info-value">🔧 In Progress</div></div>
                <div class="info-card"><div class="info-label">Affected System</div><div class="info-value">SETS Web Application</div></div>
              </div>
              <p class="footer">For urgent concerns, please contact the <strong>IT Department</strong> directly.<br>We apologize for the inconvenience.</p>
            </div>
            </body>
            </html>
            """;
    }
}
