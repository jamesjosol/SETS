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

			//public static string Get_Ord_Test = @"
			//		 select 		
			//			od.od_tno AS labno,
			//			od.od_testcode as testcode,
			//			ti.ti_name as testname,
			//			od.od_spl_type as sampletypecode,
			//			od.od_test_grp as testgroup
			//		from ord_dtl od
			//		left join test_item ti ON ti.ti_code = od.od_order_ti
			//		where od.od_tno = :p0 and od.od_spl_type = :p1
			//		and od_testcode = od_order_ti
			//	";

			public static string Get_Ord_Test = @"
					SELECT
						od.od_tno       AS labno,
						od.od_testcode  AS testcode,
						ti.ti_name      AS testname,
						CASE 
							WHEN od.od_item_type = 'P' THEN :p1 
							ELSE od.od_spl_type 
						END             AS sampletypecode,
						od.od_test_grp  AS testgroup,
						od.od_item_type AS item_type
					FROM ord_dtl od
					LEFT JOIN test_item ti ON ti.ti_code = od.od_testcode
					WHERE od.od_tno = :p0
						AND od.od_item_type != 'N'
						AND (
							(
								od.od_item_type = 'U'
								AND od.od_spl_type = :p1
								AND od.od_testcode = od.od_order_ti
							)
							OR
							(
								od.od_item_type = 'P'
								AND EXISTS (
									SELECT 1 FROM ord_dtl child
									WHERE child.od_tno = :p0
										AND child.od_order_ti = od.od_testcode
										AND child.od_item_type = 'U'
										AND child.od_spl_type = :p1
								)
								AND NOT EXISTS (
									SELECT 1 FROM ord_dtl child
									WHERE child.od_tno = :p0
										AND child.od_order_ti = od.od_testcode
										AND child.od_item_type = 'U'
										AND child.od_spl_type != :p1
								)
							)
									OR
							(
								od.od_item_type = 'U'
								AND od.od_spl_type = :p1
								AND od.od_testcode != od.od_order_ti
								AND EXISTS (
									SELECT 1 FROM ord_dtl sibling
									WHERE sibling.od_tno = :p0
									AND sibling.od_order_ti = od.od_order_ti
									AND sibling.od_item_type = 'U'
									AND sibling.od_spl_type != :p1
								)
							)
								OR
								-- Case 4: Profile parent has real spl_type = :p1, 
								-- but ALL children have spl_type = 0 (inherit from parent)
							(
								od.od_item_type = 'P'
								AND od.od_spl_type = :p1
								AND EXISTS (
									SELECT 1 FROM ord_dtl child
									WHERE child.od_tno = :p0
										AND child.od_order_ti = od.od_testcode
										AND child.od_item_type = 'U'
										AND child.od_spl_type = '0'
								)
								AND NOT EXISTS (
									SELECT 1 FROM ord_dtl child
									WHERE child.od_tno = :p0
										AND child.od_order_ti = od.od_testcode
										AND child.od_item_type = 'U'
										AND child.od_spl_type != '0'
								)
							)
						)
					";

			public static string Check_Spl_Routed = @"
					SELECT os_spl_rcvd_flag 
					FROM ord_spl
					WHERE os_sno = :p0";

            // old code
            //        public static string Check_Test_Released = @"
            //	SELECT od_item_type, od_action_flag
            //	FROM ord_dtl
            //	WHERE od_tno = :p0
            //		AND od_order_ti = :p1
            //		AND od_item_type = 'U'
            //";


            public static string Check_Test_Released = @"
				SELECT od_action_flag, od_ctl_flag2, od_validate_by, od_validate_on
				FROM ord_dtl
				WHERE od_tno = :p0
				  AND od_item_type = 'U'
				  AND (
					  od_order_ti = :p1
					  OR (
						  od_testcode = :p1
						  AND NOT EXISTS (
							  SELECT 1 FROM ord_dtl child
							  WHERE child.od_tno = :p0
								AND child.od_order_ti = :p1
								AND child.od_item_type = 'U'
						  )
					  )
				  )";

            public static string Get_OnSite_Specimen = @"

                 SELECT 
						os.os_sno as specimenNo,
						oh.oh_tno as labNo,
						os.os_spl_type as sampleTypeCode,
						oh.oh_last_name as patientName,
						oh.oh_pid as PID,
						oh.oh_trx_dt as trxDate,
						od.od_testcode as testCode,
						ti.ti_name as testName,
						od.od_test_grp as testGroup
					FROM ord_spl os
					LEFT JOIN ord_hdr oh ON oh.oh_tno = os.os_tno
					LEFT JOIN ord_dtl od ON od.od_tno = os.os_tno 
						AND od.od_spl_type = os.os_spl_type
						AND od.od_testcode = od.od_order_ti
					LEFT JOIN test_item ti ON ti.ti_code = od.od_order_ti
					WHERE os.os_sno = :p0
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


			public static string Get_Sample_Types = @"
				select st_code as code, st_name as name from sample_type";

			public static string Get_Test_Groups = @"
				select tg_code as code, tg_name as name from test_group";
        }
	}
}
