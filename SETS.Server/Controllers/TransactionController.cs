using Microsoft.AspNetCore.Mvc;
using Service;

namespace SETS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        [HttpGet("hclaborder/{specimenNo}")]
        public async Task<IActionResult> GetHCLABOrder(string specimenNo)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                // extract labNo (first 10 chars) and sampleTypeCode (remainder)
                if (specimenNo.Length < 10)
                    return BadRequest(new { message = "Invalid specimen number." });

                var labNo = specimenNo.Substring(0, 10);
                var sampleTypeCode = specimenNo.Length > 10
                    ? specimenNo.Substring(10)
                    : string.Empty;

                using var master = new MasterService(branch);

                // fetch order from HCLAB
                var result = await master.Transaction.GetOrdHdr(labNo);
                if (result == null)
                    return NotFound(new { message = $"Order {labNo} not found." });

                // fetch sample type from SETSDB
                string sampleTypeName = sampleTypeCode;

                var validateSpecimen = await master.Transaction.ValidateOrderSpecimen(labNo, sampleTypeCode);
                if (!validateSpecimen) return NotFound(new { message = $"Order {labNo} doesn't have specimen type {sampleTypeCode}." });
                var sampleType = master.SampleType.GetSampleType(sampleTypeCode);

                if (sampleType == null)
                    return NotFound(new { message = $"Sample Type {sampleTypeCode} not found." });

                sampleTypeName = sampleType.Name;

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        specimenNo,
                        labNo,
                        sampleTypeCode,
                        sampleTypeName,
                        transactionDate = result.TRXDATE,
                        patientID = result.PID,
                        patientName = result.PATIENTNAME
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


    }
}
