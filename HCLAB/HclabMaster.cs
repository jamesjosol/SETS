using Model.HCLAB;
using Model.Main;
using Oracle.ManagedDataAccess.Client;
using System.Diagnostics;
using static HCLAB.Queries;
using Model.Main;

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

            //public static List<User> GetUsers(string conn)
            //{
            //    return null;
            //}
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
                                    WORKSTATIONCODE = reader["workstationcode"].ToString()
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