using Model.HCLAB;
using Model.Main;
using Oracle.ManagedDataAccess.Client;
using System.Diagnostics;
using static HCLAB.Queries;
using Model.Main;
using Model.HCLAB;
using System.Reflection.Emit;

namespace HCLAB
{
    public class HclabMaster
    {
        public static class HCLABUsers
        {
            public static int Auth(string userid, string password, string conn)
            {
                int _count = 0;
                try
                {
                    using OracleConnection con = new OracleConnection(conn);
                    using OracleCommand cmd = new OracleCommand();

                    cmd.CommandText = Queries.User.Auth;
                    cmd.Parameters.Add("p0", userid);
                    cmd.Parameters.Add("p1", password);
                    cmd.Connection = con;

                    con.Open();
                    var _c = cmd.ExecuteScalar();
                    _count = Convert.ToInt32(_c);
                }
                catch
                {
                    throw;
                }
                return _count;
            }

            public async static Task<List<Model.HCLAB.User>> GetUsers(string conn, string param)
            {
                List<Model.HCLAB.User> users = new List<Model.HCLAB.User>();
                try
                {
                    using (var con = new OracleConnection(conn))
                    using (var cmd = new OracleCommand(Queries.User.GetActiveUsers, con))
                    {
                        cmd.Parameters.Add("p0", param);
                        await con.OpenAsync();

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                users.Add(new Model.HCLAB.User
                                {
                                    UserID = reader["user_id"].ToString(),
                                    UserName = reader["user_name"].ToString()
                                });
                            }
                        }

                        return users;
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        public static class HCLABTests
        {
            public async static Task<List<Model.HCLAB.Test>> GetTests(string conn, string param)
            {
                List<Model.HCLAB.Test> tests = new List<Model.HCLAB.Test>();
                try
                {
                    using (var con = new OracleConnection(conn))
                    using (var cmd = new OracleCommand(Queries.Test.Get_Tests, con))
                    {
                        cmd.Parameters.Add("p0", param);
                        await con.OpenAsync();

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                tests.Add(new Model.HCLAB.Test
                                {
                                    TestCode = reader["test_code"].ToString(),
                                    TestName = reader["test_name"].ToString(),
                                    TestGroup = reader["test_group"].ToString()
                                });
                            }
                        }

                        return tests;
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }

            public async static Task<List<Model.HCLAB.Ref_Tables>> GetSampleTypes(string conn)
            {
                List<Model.HCLAB.Ref_Tables> sampleTypes = new List<Model.HCLAB.Ref_Tables>();
                try
                {
                    using (var con = new OracleConnection(conn))
                    using (var cmd = new OracleCommand(Queries.Test.Get_Sample_Types, con))
                    {
                        await con.OpenAsync();

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                sampleTypes.Add(new Model.HCLAB.Ref_Tables
                                {
                                    Code = reader["code"].ToString(),
                                    Name = reader["name"].ToString(),
                                });
                            }
                        }

                        return sampleTypes;
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }

            public async static Task<List<Model.HCLAB.Ref_Tables>> GetTestGroups(string conn)
            {
                List<Model.HCLAB.Ref_Tables> testGroups = new List<Model.HCLAB.Ref_Tables>();
                try
                {
                    using (var con = new OracleConnection(conn))
                    using (var cmd = new OracleCommand(Queries.Test.Get_Test_Groups, con))
                    {
                        await con.OpenAsync();

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                testGroups.Add(new Model.HCLAB.Ref_Tables
                                {
                                    Code = reader["code"].ToString(),
                                    Name = reader["name"].ToString(),
                                });
                            }
                        }

                        return testGroups;
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        public static class HCLABTransactions
        {
            public async static Task<Ord_Hdr> GetOrd_Hdr(string conn, string labNo)
            {
                Ord_Hdr trx = null;
                try
                {
                    using (var con = new OracleConnection(conn))
                    using (var cmd = new OracleCommand(Queries.Transaction.Get_Ord_Hdr, con))
                    {
                        cmd.Parameters.Add("p0", labNo);
                        await con.OpenAsync();

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                DateTime.TryParse(reader["oh_trx_dt"]?.ToString(), out DateTime parsedDate);
                                trx = new Ord_Hdr
                                {
                                    LABNO = reader["oh_tno"].ToString(),
                                    TRXDATE = parsedDate,
                                    PID = reader["oh_pid"].ToString(),
                                    PATIENTNAME = reader["oh_last_name"].ToString()
                                };
                            }
                        }

                        return trx;
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }

            public async static Task<int> Validate_Ord_Specimen(string conn, string labNo, string code)
            {
                int _count = 0;
                try
                {
                    using (var con = new OracleConnection(conn))
                    using (var cmd = new OracleCommand(Queries.Transaction.Validate_Ord_Specimen, con))
                    {
                        cmd.Parameters.Add("p0", labNo);
                        cmd.Parameters.Add("p1", code);
                        cmd.Connection = con;

                        con.Open();
                        var _c = cmd.ExecuteScalar();
                        _count = Convert.ToInt32(_c);
                    }
                    return _count;
                }
                catch (Exception)
                {

                    throw;
                }
            }

            public async static Task<List<Ord_Dtl>> GetOrd_Dtl(string conn, string labNo, string code)
            {
                List<Ord_Dtl> dtl = new List<Ord_Dtl>();
                try
                {
                    using (var con = new OracleConnection(conn))
                    using (var cmd = new OracleCommand(Queries.Transaction.Get_Ord_Test, con))
                    {
                        cmd.Parameters.Add("p0", labNo);
                        cmd.Parameters.Add("p1", code);
                        await con.OpenAsync();

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                dtl.Add(new Ord_Dtl
                                {
                                    LABNO = reader["labno"].ToString(),
                                    TESTCODE = reader["testcode"].ToString(),
                                    TESTNAME = reader["testname"].ToString(),
                                    SAMPLETYPECODE = reader["sampletypecode"].ToString(),
                                    TESTGROUP = reader["testgroup"].ToString()
                                });

                            }
                        }

                        return dtl;
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }

            public async static Task<bool> CheckSplRouted(string conn, string specimenNo)
            {
                try
                {
                    using (var con = new OracleConnection(conn))
                    using (var cmd = new OracleCommand(Queries.Transaction.Check_Spl_Routed, con))
                    {
                        cmd.Parameters.Add("p0", specimenNo);
                        await con.OpenAsync();

                        var result = await cmd.ExecuteScalarAsync();
                        if (result == null || result == DBNull.Value) return false;
                        return result.ToString().Trim().ToUpper() == "Y";
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }

            /// <summary>
            /// Checks if a test (including profiles) is fully released in HCLAB.
            /// For profiles: all child rows (od_item_type = 'U') must have a non-null od_release_on.
            /// For single tests: the single 'U' row must have a non-null od_release_on.
            /// Returns true if released, false if not yet.
            /// </summary>
            public static async Task<bool> CheckTestReleased(string conn, string labNo, string testCode)
            {
                try
                {
                    using var con = new OracleConnection(conn);
                    using var cmd = new OracleCommand(Queries.Transaction.Check_Test_Released, con);
                    cmd.Parameters.Add("p0", labNo);
                    cmd.Parameters.Add("p1", testCode);
                    await con.OpenAsync();

                    var children = new List<DateTime?>();

                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var releaseOn = reader["od_release_on"];
                        children.Add(releaseOn == DBNull.Value ? null : Convert.ToDateTime(releaseOn));
                    }

                    // No child rows found — can't confirm release
                    if (children.Count == 0) return false;

                    // All children must have a release date
                    return children.All(r => r.HasValue);
                }
                catch { throw; }
            }
        }
        
        public static class MISC
        {
            public static HealthCheckResult CheckHcLab(string conn)
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    // Replace with your actual Oracle connection open call

                    using var con = new OracleConnection(conn);
                    con.Open();
                    con.Close();
                    sw.Stop();
                    return new HealthCheckResult { Online = true, LatencyMs = sw.ElapsedMilliseconds };
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    return new HealthCheckResult { Online = false, LatencyMs = sw.ElapsedMilliseconds, Error = ex.Message };
                }
            }
        }
    }
}