using HCLAB;
using Model.HCLAB;
using Service.Interfaces;

namespace Service.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly string _conn;

        public TransactionService(string branch)
        {
            _conn = HclabConnection.ConnectionString(branch);
        }

        public async Task<Ord_Hdr> GetOrdHdr(string labNo)
        {
            try
            {
                return await HclabMaster.HCLABTransactions.GetOrd_Hdr(_conn, labNo);
            }
            catch { throw; }
        }

        public async Task<bool> ValidateOrderSpecimen(string labNo, string code)
        {
            try
            {
                var res = await HclabMaster.HCLABTransactions.Validate_Ord_Specimen(_conn, labNo, code);
                return res == 1;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ord_Dtl>> GetOrdDtl(string labNo, string code)
        {
            try
            {
                return await HclabMaster.HCLABTransactions.GetOrd_Dtl(_conn, labNo, code);
            }
            catch { throw; }
        }
    }
}