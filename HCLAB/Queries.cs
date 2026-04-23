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
					 select 		
						od.od_tno AS labno,
						od.od_testcode as testcode,
						ti.ti_name as testname,
						od.od_spl_type as sampletypecode,
						od.od_test_grp as testgroup
					from ord_dtl od
					left join test_item ti ON ti.ti_code = od.od_order_ti
					where od.od_tno = :p0 and od.od_spl_type = :p1
					and od_testcode = od_order_ti
				";

			public static string Check_Spl_Routed = @"
					SELECT os_spl_rcvd_flag 
					FROM ord_spl
					WHERE os_sno = :p0";

        }
	}
}
