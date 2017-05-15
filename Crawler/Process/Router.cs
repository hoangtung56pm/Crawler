using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Crawler.Process
{
    public class Router
    {
        public static void RouterProcess(Record record)
        {
            //if(record.Page == "VnExpress.net")
            //{
            //    VnExpressProcess.Process(record);
            //    return;
            //}
            //if (record.Page == "DanTri.com.vn")
            //{
            //    DanTriProcess.Process(record);
            //    return;
            //}
            //if (record.Page == "2sao.vietnamnet.vn")
            //{
            //    TwoStarProcess.Process(record);
            //    return;
            //}
            //if (record.Page == "phapluattp.vn")
            //{
            //    PhapLuatProcess.Process(record);
            //    return;
            //}
            //if (record.Page == "nld.com.vn")
            //{
            //    NLDProcess.Process(record);
            //    return;
            //}
            //if (record.Page == "www.zing.vn")
            //{
            //    ZingProcess.Process(record);
            //    return;
            //}
            //if (record.Page == "vtc.vn")
            //{
            //    VTCProcess.Process(record);
            //    return;
            //}
            //if (record.Page == "vietnamnet.vn")
            //{
            //    VietnamnetProcess.Process(record);
            //    return;
            //}
            //if (record.Page == "kenh14.vn")
            //{
            //    Kenh14Process.Process(record);
            //    return;
            //}
            //if (record.Page == "nhacvietplus.com.vn")
            //{
            //    NhacVietProcess.Process(record);
            //    return;
            //}
            //if (record.Page == "genk.vn")
            //{
            //    GameThuProcess.Process(record);
            //    return;
            //}
            //if (record.Page == "gamethu.vnexpress.net")
            //{
            //    GameThuVnexpressProcess.Process(record);
            //    return;
            //}
            //if (record.Page == "www.bongda24h.vn")
            //{
            //    BongDa24HProcess.Process(record);
            //    return;
            //}
            //if (record.Page == "www.bongda24h.vn_1")
            //{
            //    BongDa24H_1Process.Process(record);
            //    return;
            //}
            Process.ProcessTest(record);
            return;
        }
    }
}
