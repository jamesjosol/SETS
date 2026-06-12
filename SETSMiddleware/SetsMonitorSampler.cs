using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace SETSMiddleware.Monitoring
{
    /// <summary>
    /// SETS-only resource sampler.
    ///
    /// Every N seconds (default 2) it samples:
    ///   - CPU %, RAM (working set / private bytes), and disk I/O rates from the
    ///     w3wp.exe worker process belonging to the SETS app pool
    ///   - Network sent/received rates, current connections, and requests/sec
    ///     from the IIS per-site performance counters: "Web Service(SETS)"
    ///
    /// Keeps a rolling history window (default 60 samples = 2 minutes) so the
    /// dashboard can draw Task Manager-style charts immediately on load.
    ///
    /// Active in-flight requests and the IIS site state are fetched ON DEMAND
    /// (when the /monitor/sets endpoint is hit), via appcmd — not every tick.
    ///
    /// Fully tolerant of the app pool being stopped or idle-recycled:
    /// no w3wp simply means status = "idle" and zeroed process metrics.
    /// Not a TaskBase on purpose — a 2-second tick would flood the live log.
    /// </summary>
    public class SetsMonitorSampler : IDisposable
    {
        private const string APPCMD = @"C:\Windows\System32\inetsrv\appcmd.exe";

        private readonly string _appPoolName;
        private readonly string _siteName;
        private readonly int _sampleSeconds;
        private readonly int _historySize;

        private readonly object _lock = new();
        private readonly List<MonitorSample> _history = new();

        private System.Threading.Timer? _timer;
        private bool _disposed;

        // ── w3wp process tracking ─────────────────────────────────────────────
        private Process? _proc;
        private DateTime _lastPidResolve = DateTime.MinValue;
        private TimeSpan _lastCpuTime = TimeSpan.Zero;
        private DateTime _lastCpuSampleAt = DateTime.MinValue;
        private ulong _lastIoRead;
        private ulong _lastIoWrite;
        private DateTime _lastIoSampleAt = DateTime.MinValue;

        // ── IIS per-site performance counters ─────────────────────────────────
        private PerformanceCounter? _pcBytesSent;
        private PerformanceCounter? _pcBytesRecv;
        private PerformanceCounter? _pcConnections;
        private PerformanceCounter? _pcRequests;
        private DateTime _lastCounterInitAttempt = DateTime.MinValue;
        private bool _skipCounterRead;

        public SetsMonitorSampler(string appPoolName, string siteName,
            int sampleSeconds = 2, int historySize = 60)
        {
            _appPoolName = appPoolName;
            _siteName = siteName;
            _sampleSeconds = Math.Max(1, sampleSeconds);
            _historySize = Math.Max(10, historySize);
        }

        public void Start()
        {
            if (_timer != null) return;
            _timer = new System.Threading.Timer(_ => SafeSample(), null,
                TimeSpan.Zero, TimeSpan.FromSeconds(_sampleSeconds));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _timer?.Dispose();
            _timer = null;
            _proc?.Dispose();
            _pcBytesSent?.Dispose();
            _pcBytesRecv?.Dispose();
            _pcConnections?.Dispose();
            _pcRequests?.Dispose();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Sampling tick
        // ══════════════════════════════════════════════════════════════════════

        private int _sampling;

        private void SafeSample()
        {
            // Skip this tick if the previous one is still running (appcmd can be slow)
            if (Interlocked.Exchange(ref _sampling, 1) == 1) return;
            try { Sample(); }
            catch { /* never let the timer die */ }
            finally { Interlocked.Exchange(ref _sampling, 0); }
        }
        private void Sample()
        {
            var now = DateTime.Now;
            var sample = new MonitorSample { Time = now };

            // ── Process metrics (CPU / RAM / IO) ──────────────────────────────
            var proc = GetWorkerProcess();

            if (proc != null)
            {
                try
                {
                    proc.Refresh();

                    sample.Pid = proc.Id;
                    sample.WorkingSetBytes = proc.WorkingSet64;
                    sample.PrivateBytes = proc.PrivateMemorySize64;
                    sample.ThreadCount = proc.Threads.Count;
                    sample.HandleCount = proc.HandleCount;
                    sample.ProcessStart = proc.StartTime;

                    // CPU % — delta of TotalProcessorTime over wall time,
                    // normalized by core count (same scale as Task Manager)
                    var cpuNow = proc.TotalProcessorTime;
                    if (_lastCpuSampleAt != DateTime.MinValue)
                    {
                        var wallMs = (now - _lastCpuSampleAt).TotalMilliseconds;
                        var cpuMs = (cpuNow - _lastCpuTime).TotalMilliseconds;
                        if (wallMs > 0)
                            sample.CpuPercent = Math.Clamp(
                                cpuMs / wallMs / Environment.ProcessorCount * 100.0, 0, 100);
                    }
                    _lastCpuTime = cpuNow;
                    _lastCpuSampleAt = now;

                    // IO rates — kernel32 GetProcessIoCounters deltas
                    if (GetProcessIoCounters(proc.Handle, out var io))
                    {
                        if (_lastIoSampleAt != DateTime.MinValue)
                        {
                            var secs = (now - _lastIoSampleAt).TotalSeconds;
                            if (secs > 0)
                            {
                                sample.IoReadBytesPerSec =
                                    (io.ReadTransferCount - _lastIoRead) / secs;
                                sample.IoWriteBytesPerSec =
                                    (io.WriteTransferCount - _lastIoWrite) / secs;
                            }
                        }
                        _lastIoRead = io.ReadTransferCount;
                        _lastIoWrite = io.WriteTransferCount;
                        _lastIoSampleAt = now;
                    }
                }
                catch
                {
                    // Process died between resolve and sample — reset trackers
                    ResetProcessTracking();
                }
            }
            else
            {
                ResetProcessTracking();
            }

            // ── Per-site network counters: Web Service(SETS) ──────────────────
            EnsureCounters();
            if (_skipCounterRead)
            {
                _skipCounterRead = false; // leave this tick's network values at zero
            }
            else
            {
                sample.NetSentBytesPerSec = ReadCounter(ref _pcBytesSent);
                sample.NetRecvBytesPerSec = ReadCounter(ref _pcBytesRecv);
                sample.CurrentConnections = (int)ReadCounter(ref _pcConnections);
                sample.RequestsPerSec = ReadCounter(ref _pcRequests);
            }

            lock (_lock)
            {
                _history.Add(sample);
                if (_history.Count > _historySize)
                    _history.RemoveAt(0);
            }
        }

        private void ResetProcessTracking()
        {
            _proc?.Dispose();
            _proc = null;
            _lastCpuSampleAt = DateTime.MinValue;
            _lastIoSampleAt = DateTime.MinValue;
            _lastCpuTime = TimeSpan.Zero;
            _lastIoRead = 0;
            _lastIoWrite = 0;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  w3wp PID resolution (appcmd list wp /xml)
        // ══════════════════════════════════════════════════════════════════════

        private Process? GetWorkerProcess()
        {
            // Reuse cached process while it's still alive
            if (_proc != null)
            {
                try { if (!_proc.HasExited) return _proc; }
                catch { }
                ResetProcessTracking();
            }

            // Throttle appcmd shelling — at most once every 5 s while idle
            if ((DateTime.Now - _lastPidResolve).TotalSeconds < 5) return null;
            _lastPidResolve = DateTime.Now;

            var xml = RunAppcmd("list wp /xml");
            if (string.IsNullOrWhiteSpace(xml)) return null;

            try
            {
                var doc = XDocument.Parse(xml);
                var wp = doc.Descendants("WP").FirstOrDefault(e =>
                    string.Equals((string?)e.Attribute("APPPOOL.NAME"), _appPoolName,
                        StringComparison.OrdinalIgnoreCase));

                var pidStr = (string?)wp?.Attribute("WP.NAME");
                if (pidStr == null || !int.TryParse(pidStr, out var pid)) return null;

                _proc = Process.GetProcessById(pid);
                return _proc;
            }
            catch
            {
                return null;
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  IIS per-site performance counters
        // ══════════════════════════════════════════════════════════════════════

        private void EnsureCounters()
        {
            if (_pcBytesSent != null) return;

            // Retry init at most every 30 s (instance may not exist until the
            // site has served at least one request since last IIS restart)
            if ((DateTime.Now - _lastCounterInitAttempt).TotalSeconds < 30) return;
            _lastCounterInitAttempt = DateTime.Now;

            try
            {
                _pcBytesSent = new PerformanceCounter("Web Service", "Bytes Sent/sec", _siteName, readOnly: true);
                _pcBytesRecv = new PerformanceCounter("Web Service", "Bytes Received/sec", _siteName, readOnly: true);
                _pcConnections = new PerformanceCounter("Web Service", "Current Connections", _siteName, readOnly: true);
                _pcRequests = new PerformanceCounter("Web Service", "Total Method Requests/sec", _siteName, readOnly: true);

                // First NextValue() of rate counters always returns 0 — prime them
                _pcBytesSent.NextValue();
                _pcBytesRecv.NextValue();
                _pcConnections.NextValue();
                _pcRequests.NextValue();
                _skipCounterRead = true;
            }
            catch
            {
                _pcBytesSent?.Dispose(); _pcBytesSent = null;
                _pcBytesRecv?.Dispose(); _pcBytesRecv = null;
                _pcConnections?.Dispose(); _pcConnections = null;
                _pcRequests?.Dispose(); _pcRequests = null;
            }
        }

        private static double ReadCounter(ref PerformanceCounter? counter)
        {
            if (counter == null) return 0;
            try { return counter.NextValue(); }
            catch
            {
                // Instance vanished (site stopped) — drop and let EnsureCounters re-init
                counter.Dispose();
                counter = null;
                return 0;
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Snapshot for the /monitor/sets endpoint
        // ══════════════════════════════════════════════════════════════════════

        public object BuildSnapshot(bool includeRequests = true)
        {
            List<MonitorSample> history;
            lock (_lock)
            {
                history = _history.ToList();
            }

            var latest = history.LastOrDefault();
            bool running = latest?.Pid != null;

            return new
            {
                available = true,
                status = running ? "running" : "idle",
                siteName = _siteName,
                appPool = _appPoolName,
                siteState = GetSiteState(),
                pid = latest?.Pid,
                processStart = latest?.ProcessStart?.ToString("yyyy-MM-dd HH:mm:ss"),
                uptimeSeconds = latest?.ProcessStart != null
                    ? (long)(DateTime.Now - latest.ProcessStart.Value).TotalSeconds
                    : (long?)null,
                sampleSeconds = _sampleSeconds,
                sampledAt = latest?.Time.ToString("HH:mm:ss"),
                current = latest == null ? null : ToDto(latest),
                history = history.Select(ToDto).ToList(),
                requests = includeRequests ? GetActiveRequests() : new List<object>()
            };
        }

        private static object ToDto(MonitorSample s) => new
        {
            t = s.Time.ToString("HH:mm:ss"),
            cpu = Math.Round(s.CpuPercent, 1),
            workingSet = s.WorkingSetBytes,
            privateBytes = s.PrivateBytes,
            threads = s.ThreadCount,
            handles = s.HandleCount,
            ioRead = Math.Round(s.IoReadBytesPerSec),
            ioWrite = Math.Round(s.IoWriteBytesPerSec),
            netSent = Math.Round(s.NetSentBytesPerSec),
            netRecv = Math.Round(s.NetRecvBytesPerSec),
            connections = s.CurrentConnections,
            requestsPerSec = Math.Round(s.RequestsPerSec, 1)
        };

        // ── Active in-flight requests (on demand) ─────────────────────────────

        private List<object> GetActiveRequests()
        {
            var result = new List<object>();
            try
            {
                var xml = RunAppcmd($"list requests /apppool.name:\"{_appPoolName}\" /xml");
                if (string.IsNullOrWhiteSpace(xml)) return result;

                var doc = XDocument.Parse(xml);
                foreach (var el in doc.Descendants("REQUEST"))
                {
                    result.Add(new
                    {
                        url = (string?)el.Attribute("url") ?? "",
                        verb = (string?)el.Attribute("verb") ?? "",
                        timeElapsedMs = (string?)el.Attribute("timeElapsed") ?? "",
                        stage = (string?)el.Attribute("stage") ?? "",
                        module = (string?)el.Attribute("module") ?? ""
                    });
                }
            }
            catch { /* requests list is best-effort */ }
            return result;
        }

        // ── Site state (on demand) ────────────────────────────────────────────

        private string GetSiteState()
        {
            try
            {
                var xml = RunAppcmd($"list site \"{_siteName}\" /xml");
                if (string.IsNullOrWhiteSpace(xml)) return "unknown";

                var doc = XDocument.Parse(xml);
                var site = doc.Descendants("SITE").FirstOrDefault();
                return ((string?)site?.Attribute("state") ?? "unknown").ToLowerInvariant();
            }
            catch
            {
                return "unknown";
            }
        }

        // ── appcmd helper ─────────────────────────────────────────────────────

        private static string RunAppcmd(string args)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = APPCMD,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var proc = Process.Start(psi)!;
                var output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit(10_000);
                return output;
            }
            catch
            {
                return string.Empty;
            }
        }

        // ── P/Invoke — per-process IO counters ────────────────────────────────

        [StructLayout(LayoutKind.Sequential)]
        private struct IO_COUNTERS
        {
            public ulong ReadOperationCount;
            public ulong WriteOperationCount;
            public ulong OtherOperationCount;
            public ulong ReadTransferCount;
            public ulong WriteTransferCount;
            public ulong OtherTransferCount;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetProcessIoCounters(IntPtr hProcess, out IO_COUNTERS counters);

        // ── Sample model ──────────────────────────────────────────────────────

        private class MonitorSample
        {
            public DateTime Time { get; set; }
            public int? Pid { get; set; }
            public DateTime? ProcessStart { get; set; }
            public double CpuPercent { get; set; }
            public long WorkingSetBytes { get; set; }
            public long PrivateBytes { get; set; }
            public int ThreadCount { get; set; }
            public int HandleCount { get; set; }
            public double IoReadBytesPerSec { get; set; }
            public double IoWriteBytesPerSec { get; set; }
            public double NetSentBytesPerSec { get; set; }
            public double NetRecvBytesPerSec { get; set; }
            public int CurrentConnections { get; set; }
            public double RequestsPerSec { get; set; }
        }
    }
}