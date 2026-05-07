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

        [HttpGet("joborder/{labNo}")]
        public async Task<IActionResult> GetJobOrder(string labNo)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                labNo = labNo.Trim().ToUpper();
                if (string.IsNullOrEmpty(labNo))
                    return BadRequest(new { message = "Lab number is required." });

                using var master = new MasterService(branch);

                var result = await master.Transaction.GetOrdHdr(labNo);
                if (result == null)
                    return NotFound(new { message = $"Lab number {labNo} not found." });

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        labNo,
                        patientID = result.PID,
                        patientName = result.PATIENTNAME,
                        transactionDate = result.TRXDATE
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
