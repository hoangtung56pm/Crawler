using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
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

        public static int Insert(ContentInfo _cMS_ContentsInfo)
        {
            var dbConn = new SqlConnection(ConnectionString);
            var dbCmd = new SqlCommand("CMS_Contents_Insert", dbConn);
            dbCmd.CommandType = CommandType.StoredProcedure;
            dbCmd.Parameters.AddWithValue("@Content_Headline", AppEnv.NCRToUnicode(_cMS_ContentsInfo.Content_Headline));
            dbCmd.Parameters.AddWithValue("@Content_Teaser", AppEnv.NCRToUnicode(_cMS_ContentsInfo.Content_Teaser));
            dbCmd.Parameters.AddWithValue("@Content_Body", _cMS_ContentsInfo.Content_Body);
            dbCmd.Parameters.AddWithValue("@Content_Avatar", _cMS_ContentsInfo.Content_Avatar);
            dbCmd.Parameters.AddWithValue("@Content_BigAvatar", _cMS_ContentsInfo.Content_Avatar);
            dbCmd.Parameters.AddWithValue("@Content_CreateDate", _cMS_ContentsInfo.Content_CreateDate);
            dbCmd.Parameters.AddWithValue("@Content_Status", _cMS_ContentsInfo.Content_Status);
            dbCmd.Parameters.AddWithValue("@CategoryID", _cMS_ContentsInfo.CategoryID);
            dbCmd.Parameters.AddWithValue("@Content_UserID", _cMS_ContentsInfo.Content_UserID);
            dbCmd.Parameters.AddWithValue("@Content_HeadlineKD", _cMS_ContentsInfo.Content_Headline);
            dbCmd.Parameters.AddWithValue("@Content_TeaserKD", _cMS_ContentsInfo.Content_Teaser);
            dbCmd.Parameters.AddWithValue("@Content_Source", _cMS_ContentsInfo.Content_Source);
            dbCmd.Parameters.AddWithValue("@Content_Rank", _cMS_ContentsInfo.Content_Rank);
            dbCmd.Parameters.AddWithValue("@Content_Url", _cMS_ContentsInfo.Content_Url);
            dbCmd.Parameters.AddWithValue("@IsPublished", _cMS_ContentsInfo.IsPublished);
            dbCmd.Parameters.AddWithValue("@RelateID", _cMS_ContentsInfo.RelateID);
            //dbCmd.Parameters.AddWithValue("@RETURN_VALUE", SqlDbType.Int).Direction = ParameterDirection.Output;
            try
            {
                dbConn.Open();
                dbCmd.ExecuteNonQuery();
                return 1;
            }
                catch(Exception ex)
                {
                    throw ex;
                }
            finally
            {
                dbConn.Close();
            }
        }

        public static Image DownloadImage(string _URL)
        {
            Image _tmpImage = null;

            try
            {
                // Open a connection
                System.Net.HttpWebRequest _HttpWebRequest = (System.Net.HttpWebRequest) System.Net.HttpWebRequest.Create(_URL);

                _HttpWebRequest.AllowWriteStreamBuffering = true;

                // You can also specify additional header values like the user agent or the referer: (Optional)
                _HttpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)";
                _HttpWebRequest.Referer = "http://www.google.com/";

                // set timeout for 20 seconds (Optional)
                _HttpWebRequest.Timeout = 20000;

                // Request response:
                System.Net.WebResponse _WebResponse = _HttpWebRequest.GetResponse();

                // Open data stream:
                System.IO.Stream _WebStream = _WebResponse.GetResponseStream();

                // convert webstream to image
                _tmpImage = Image.FromStream(_WebStream);

                // Cleanup
                _WebResponse.Close();
                _WebResponse.Close();
            }
            catch (Exception _Exception)
            {
                return null;
            }

            return _tmpImage;
        }

        public static string NCRToUnicode(string strInput)
        {
            string TCVN = "&#225;,&#224;,&#7841;,&#7843;,&#227;,&#226;,&#7845;,&#7847;,&#7853;,&#7849;,&#7851;,&#259;,&#7855;,&#7857;,&#7863;,&#7859;,&#7861;,&#é;,&#232;,&#7865;,&#7867;,&#7869;,&#234;,&#7871;,&#7873;,&#7879;,&#7875;,&#7877;,&#243;,&#242;,&#7885;,&#7887;,&#245;,&#244;,&#7889;,&#7891;,&#7897;,&#7893;,&#7895;,&#417;,&#7899;,&#7901;,&#7907;,&#7903;,&#7905;,&#250;,&#249;,&#7909;,&#7911;,&#361;,&#432;,&#7913;,&#7915;,&#7921;,&#7917;,&#7919;,&#237;,&#236;,&#7883;,&#7881;,&#297;,&#273;,&#253;,&#7923;,&#7925;,&#7927;,&#7929;";
            TCVN += "&#193;,&#192;,&#7840;,&#7842;,&#195;,&#194;,&#7844;,&#7846;,&#7852;,&#7848;,&#7850;,&#258;,&#7854;,&#7856;,&#7862;,&#7858;,&#7860;,&#200;,&#7864;,&#7866;,&#7868;,&#7870;,&#7872;,&#7878;,&#7874;,&#7876;,&#211;,&#210;,&#7884;,&#7886;,&#213;,&#212;,&#7888;,&#7890;,&#7896;,&#7892;,&#7894;,&#416;,&#7898;,&#7900;,&#7906;,&#7902;,&#7904;,&#218;,&#217;,&#7908;,&#7910;,&#360;,&#431;,&#7912;,&#7914;,&#7920;,&#7916;,&#7918;,&#272;,&#221;,&#7922;,&#7924;,&#7926;,&#7928;";
            string UNICODE = "á,à,ạ,ả,ã,â,ấ,ầ,ậ,ẩ,ẫ,ă,ắ,ằ,ặ,ẳ,ẵ,é,è,ẹ,ẻ,ẽ,ê,ế,ề,ệ,ể,ễ,ó,ò,ọ,ỏ,õ,ô,ố,ồ,ộ,ổ,ỗ,ơ,ớ,ờ,ợ,ở,ỡ,ú,ù,ụ,ủ,ũ,ư,ứ,ừ,ự,ử,ữ,í,ì,ị,ỉ,ĩ,đ,ý,ỳ,ỵ,ỷ,ỹ";
            UNICODE += "Á,À,Ạ,Ả,Ã,Â,Ấ,Ầ,Ậ,Ẩ,Ẫ,Ă,Ắ,Ằ,Ặ,Ẳ,Ẵ,È,Ẹ,Ẻ,Ẽ,Ế,Ề,Ệ,Ể,Ễ,Ó,Ò,Ọ,Ỏ,Õ,Ô,Ố,Ồ,Ộ,Ổ,Ỗ,Ơ,Ớ,Ờ,Ợ,Ở,Ỡ,Ú,Ù,Ụ,Ủ,Ũ,Ư,Ứ,Ừ,Ự,Ử,Ữ,Đ,Ý,Ỳ,Ỵ,Ỷ,Ỹ";
            string[] str = TCVN.Split(new Char[] { ',' });
            string[] str1 = UNICODE.Split(new Char[] { ',' });
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] != "")
                {
                    strInput = strInput.Replace(str[i], str1[i]);
                }
            }
            return strInput.Replace("&#233;", "é");
        }
	}
}
