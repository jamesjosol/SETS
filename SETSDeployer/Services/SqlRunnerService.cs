using System.IO;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using SETSDeployer.Models;

namespace SETSDeployer.Services
{
    public class SqlRunnerResult
    {
        public string BranchName   { get; init; } = string.Empty;
        public bool   Success      { get; init; }
        public string Message      { get; init; } = string.Empty;
        public int    RowsAffected { get; init; }
    }

    public class SqlRunnerService
    {
        private readonly DeployerSettings _settings;
        private readonly Action<string, LogLevel> _log;

        // DDL keywords — only these are permitted
        private static readonly HashSet<string> _allowedVerbs = new(StringComparer.OrdinalIgnoreCase)
        {
            "ALTER", "CREATE", "DROP", "TRUNCATE", "RENAME", "EXEC", "EXECUTE", "SP_RENAME"
        };

        public SqlRunnerService(DeployerSettings settings, Action<string, LogLevel> log)
        {
            _settings = settings;
            _log      = log;
        }

        // ── Load connection strings from SETS.Server/appsettings.json ─────────

        public Dictionary<string, string> LoadConnectionStrings()
        {
            var path = Path.Combine(
                _settings.SolutionRootPath,
                "SETS.Server",
                "appsettings.json");

            if (!File.Exists(path))
                throw new FileNotFoundException($"appsettings.json not found at: {path}");

            var json = File.ReadAllText(path);
            var doc  = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("ConnectionStrings", out var cs))
                throw new InvalidOperationException("No ConnectionStrings section found in appsettings.json.");

            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in cs.EnumerateObject())
                result[prop.Name] = prop.Value.GetString() ?? string.Empty;

            return result;
        }

        // ── Validate — DDL only ───────────────────────────────────────────────

        public (bool valid, string reason) ValidateQuery(string sql)
        {
            var trimmed = sql.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
                return (false, "Query is empty.");

            // Get first word
            var firstWord = trimmed.Split(new[] { ' ', '\t', '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries)[0];

            if (!_allowedVerbs.Contains(firstWord))
                return (false, $"Only DDL statements are allowed (ALTER, CREATE, DROP, TRUNCATE, RENAME, EXEC). Got: '{firstWord}'.");

            return (true, string.Empty);
        }

        // ── Execute across selected branches ──────────────────────────────────

        public async Task<List<SqlRunnerResult>> ExecuteAsync(
            string sql,
            List<BranchConfig> branches,
            Dictionary<string, string> connectionStrings,
            CancellationToken ct = default)
        {
            var results = new List<SqlRunnerResult>();

            var tasks = branches.Select(async branch =>
            {
                if (!connectionStrings.TryGetValue(branch.Name, out var connStr) ||
                    string.IsNullOrWhiteSpace(connStr))
                {
                    _log($"[{branch.Name}] No connection string found — skipping.", LogLevel.Warning);
                    results.Add(new SqlRunnerResult
                    {
                        BranchName = branch.Name,
                        Success    = false,
                        Message    = "No connection string configured."
                    });
                    return;
                }

                _log($"[{branch.Name}] Executing...", LogLevel.Info);

                try
                {
                    await using var conn = new SqlConnection(connStr);
                    await conn.OpenAsync(ct);

                    await using var cmd = new SqlCommand(sql, conn)
                    {
                        CommandTimeout = 60
                    };

                    int rows = await cmd.ExecuteNonQueryAsync(ct);

                    _log($"[{branch.Name}] ✓ Success. Rows affected: {rows}", LogLevel.Success);
                    lock (results)
                    {
                        results.Add(new SqlRunnerResult
                        {
                            BranchName   = branch.Name,
                            Success      = true,
                            Message      = $"Rows affected: {rows}",
                            RowsAffected = rows
                        });
                    }
                }
                catch (OperationCanceledException)
                {
                    _log($"[{branch.Name}] Cancelled.", LogLevel.Warning);
                    lock (results)
                    {
                        results.Add(new SqlRunnerResult
                        {
                            BranchName = branch.Name,
                            Success    = false,
                            Message    = "Cancelled."
                        });
                    }
                }
                catch (Exception ex)
                {
                    _log($"[{branch.Name}] ✗ Failed: {ex.Message}", LogLevel.Error);
                    lock (results)
                    {
                        results.Add(new SqlRunnerResult
                        {
                            BranchName = branch.Name,
                            Success    = false,
                            Message    = ex.Message
                        });
                    }
                }
            });

            await Task.WhenAll(tasks);

            // Summary
            int ok   = results.Count(r => r.Success);
            int fail = results.Count(r => !r.Success);
            _log($"━━━ SQL Runner complete: {ok}/{branches.Count} succeeded, {fail} failed ━━━",
                fail == 0 ? LogLevel.Success : LogLevel.Warning);

            return results.OrderBy(r => r.BranchName).ToList();
        }
    }
}
