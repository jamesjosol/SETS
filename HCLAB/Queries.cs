using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCLAB
{
	internal static class Queries
	{
		public static class User
		{


			public static string Auth = @"	
			        SELECT 
	                count(*)
	                FROM
	                user_account
	                WHERE suspend = 'N' 
	                AND user_id = :p0 
	                AND password = :p1
				";
		}


		public static class Transaction
		{
			public static string Get_Ord_Hdr = @"
					select oh_tno, oh_trx_dt, oh_pid, oh_last_name from ord_hdr	
					where oh_tno = :p0
				";

			public static string Validate_Ord_Specimen = @"
					select count(*) from ord_spl
					where os_tno = :p0 and os_spl_type = :p1
				";

			public static string Get_Ord_Test = @"
					  SELECT
						od.od_tno AS labno,
						od.od_testcode as testcode,
						ti.ti_name as testname,
						od.od_spl_type as sampletypecode,
						(select os_spl_rcvd_ws from ord_spl where os_tno = od.od_tno and os_spl_type = od.od_spl_type) as workstationcode
					FROM ord_dtl od
					LEFT JOIN test_item ti ON ti.ti_code = od.od_order_ti
					where od_item_parent = '000000' and od_test_grp not in ('IMAGE', '00')
					and od.od_tno = :p0 and od.od_spl_type = :p1
				";

		}
	}
}
