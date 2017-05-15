using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Crawler.Lib;

namespace Crawler.Lib
{
	public class AppEnv
	{
        public static string GetSetting(string key)
        {
            return ConfigurationSettings.AppSettings[key];
        }
		private static string ConnectionString
		{
            get { return ConfigurationSettings.AppSettings["localsql"]; }
		}

        public static DataTable GetAll()
        {
            DataTable retVal = null;
            SqlConnection dbConn = new SqlConnection(AppEnv.ConnectionString);
            SqlCommand dbCmd = new SqlCommand("Content_GetAll", dbConn);
            dbCmd.CommandType = CommandType.StoredProcedure;
            try
            {
                retVal = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(dbCmd);
                da.Fill(retVal);
            }
            finally
            {
                dbConn.Close();
            }
            return retVal;
        }


	}
}
