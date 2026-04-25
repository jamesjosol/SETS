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

			public static string GetActiveUsers = @"
					SELECT user_id, user_name 
					FROM user_account
					WHERE suspend = 'N'
					  AND (
						user_id   LIKE '%' || :p0 || '%'
						OR user_name LIKE '%' || :p0 || '%'
					  ) 
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

            public static string Check_Test_Released = @"
				SELECT od_item_type, od_release_on
				FROM ord_dtl
				WHERE od_tno = :p0
				  AND od_order_ti = :p1
				  AND od_item_type = 'U'
				";

        }

  
        public static class Test 
		{
			public static string Get_Tests = @"
				SELECT ti_code AS test_code, ti_name AS test_name, ti_test_grp AS test_group
				FROM test_item
				WHERE ti_rec_flag = 'Y'
				  AND (
					UPPER(ti_code) LIKE '%' || UPPER(:p0) || '%'
					OR UPPER(ti_name) LIKE '%' || UPPER(:p0) || '%'
				  )";

        }
	}
}
