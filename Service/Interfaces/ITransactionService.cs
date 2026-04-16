using Model.HCLAB;

namespace Service.Interfaces
{
    public interface ITransactionService
    {
        Task<Ord_Hdr> GetOrdHdr(string labNo);
        Task<List<Ord_Dtl>> GetOrdDtl(string labNo, string code);
        Task<bool> ValidateOrderSpecimen(string labNo, string code);
    }
}