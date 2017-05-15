using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Crawler.Lib;
using System.IO;
using System.Net;
namespace WebApplication1
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //dgrTest.DataSource = AppEnv.GetAll();
            //dgrTest.DataBind();
            SaveFileFromURL("http://radiogioitinh.vn/files/252567/MT1-TT001.mp3", @"E:\Work\Projects\Crawler\MT1-TT001.mp3", 5);
        }

        public static bool SaveFileFromURL(string url, string destinationFileName, int timeoutInSeconds)
        {
            // Create a web request to the URL
            HttpWebRequest MyRequest = (HttpWebRequest)WebRequest.Create(url);
            MyRequest.Timeout = timeoutInSeconds * 1000;
            try
            {
                // Get the web response
                HttpWebResponse MyResponse = (HttpWebResponse)MyRequest.GetResponse();

                // Make sure the response is valid
                if (HttpStatusCode.OK == MyResponse.StatusCode)
                {
                    // Open the response stream
                    using (Stream MyResponseStream = MyResponse.GetResponseStream())
                    {
                        // Open the destination file
                        using (FileStream MyFileStream = new FileStream(destinationFileName, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            // Create a 4K buffer to chunk the file
                            byte[] MyBuffer = new byte[4096];
                            int BytesRead;
                            // Read the chunk of the web response into the buffer
                            while (0 < (BytesRead = MyResponseStream.Read(MyBuffer, 0, MyBuffer.Length)))
                            {
                                // Write the chunk from the buffer to the file
                                MyFileStream.Write(MyBuffer, 0, BytesRead);
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                throw new Exception("Error saving file from URL:" + err.Message, err);
            }
            return true;
        }
    }
}