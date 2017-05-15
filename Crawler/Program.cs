using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using System.Net;
using System.IO;
using Crawler.Process;
using System.Data;
using HtmlAgilityPack;

namespace Crawler
{
    class Program
    {
        static void CrawlData(object record)
        {
            var recordInfo = (Record)record;
            Router.RouterProcess(recordInfo);
        }

        public static string StripTagsRegex(string source)
        {
            source = Regex.Replace(source, "<.*?>", string.Empty);
            source = Regex.Replace(source, "<A.*?>", string.Empty);
            return source;
        }
        private static string RunBrowser(string link, out long TotalSize)
        {
            try
            {
                // Tạo yêu cầu.
                WebRequest obj = WebRequest.Create(link);

                // Lấy đáp ứng. công việc này sẽ lấy nội dung trang web về
                WebResponse webRespone = obj.GetResponse();
                TotalSize = webRespone.ContentLength;
                // Đọc đáp ứng (dạng stream).
                StreamReader sr = new StreamReader(webRespone.GetResponseStream());
                string result = sr.ReadToEnd();
                return result;
            }
            catch (Exception ex)
            {
                TotalSize = 0;
                return string.Empty;
            }
        }
        public static List<string> GetSrc(string htmlText)
        {
            List<string> imgScrs = new List<string>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlText);

            var nodes = doc.DocumentNode.SelectNodes(@"//img");//@"//img[@src]"
            if (nodes != null)
            {
                foreach (var img in nodes)
                {
                    //HtmlAttribute att = img.Attributes["src"];
                    //imgScrs.Add(att.Value);
                    imgScrs.Add(img.OuterHtml);
                }
            }

            return imgScrs;
        }
        static void Main(string[] args)
        {
            //long totalSize = 0;
            //string p = "<div.*?class=\"folder-top\">.*?<p>.*?<a.*?href=\".*?\".*?class=\"link-topnews\".*?>(?<Title>.*?)</a>.*?<label.*?class=\"item-time\".*?>(?<Time>.*?)</label>.*?<label.*?class=\"item-date\".*?>(?<Date>.*?)</label>.*?</p>.*?<p>(?<Teaser>.*?)</p>.*?</div>";
            //string html = RunBrowser("http://dantri.com.vn/c76/kinhdoanh.htm", out totalSize);

            //string html = "dasd dsasd adasd asd asdas d<img width=\"440\" src=\"//images.tienphong.vn/Uploaded/baodang/2014_03_13/huong_QZOR.jpg.ashx?width=600\" alt=\"\">fdaf fdsfasdf fasf adfas fsdf f <img width=\"440\" src=\"//images.tienphong.vn/Uploaded/baodang/2014_03_13/huong_QZOR.jpg.ashx?width=500\" alt=\"\">";
            //List<string> imgSrc = GetSrc(html);
            //foreach (var item in imgSrc)
            //{
            //    string imagePath = item;
            //    if (imagePath.IndexOf("src=\"") > -1)
            //    {
            //        imagePath = imagePath.Substring(imagePath.IndexOf("src=\"") + 5);
            //        imagePath = imagePath.Substring(0, imagePath.IndexOf("\""));
            //    }
            //    if (imagePath.IndexOf("src='") > -1)
            //    {
            //        imagePath = imagePath.Substring(imagePath.IndexOf("src='") + 5);
            //        imagePath = imagePath.Substring(0, imagePath.IndexOf("'"));
            //    }

            //    if (item.IndexOf("http://") == -1)
            //    {
            //        string newSrc = "http://domain/" + imagePath;

            //        string olgImg = item;
            //            html = html.Replace(olgImg, "<img src=\"" + newSrc + "\"/>");
                    
            //    }
            //    else
            //    {
            //        string newSrc = item;
            //        Match match = Regex.Match(html, "<img.*?src=\"" + item + "\".*?>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            //        if (match.Success)
            //        {
            //            string olgImg = match.Value.Substring(0, match.Value.IndexOf('>') + 1);
            //            html = html.Replace(olgImg, "<img src=\"" + newSrc + "\"/>");
            //        }
            //    }
            //}

            //string html = "Sau chuỗi ngày dài <A href=\"/gl/van-hoa/2012/01/demi-moore-phai-cap-cuu-vi-nghi-hit-nito-oxit-qua-lieu/\" class=\"Lead\">nhập viện</A>, <A href=\"/gl/van-hoa/guong-mat-nghe-sy/2012/02/demi-moore-di-cai-nghien/\" class=\"Lead\">cai nghiện</A> và <A href=\"/gl/van-hoa/2012/03/demi-moore-bi-mat-nghi-duong-sau-cai-nghien/\" class=\"Lead\">bí mật đi nghỉ dưỡng</A> tại vùng biển Caribbe, mỹ nhân phim “Oan hồn” trở về Los Angeles trong bộ dạng không thể tiều tụy hơn.<BR>> <A href=\"/gl/van-hoa/2012/03/demi-moore-bi-mat-nghi-duong-sau-cai-nghien/\" class=\"Lead\">Demi Moore bí mật nghỉ dưỡng sau cai nghiện</A>";
            //string test = StripTagsRegex(html);
            //log4net.Config.XmlConfigurator.Configure();

            var ds = new DataSet();
            ds.ReadXml(Crawler.Lib.AppEnv.GetSetting("ConfigPath"));

            DataTable dataTable = ds.Tables[0];
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                var thread = new Thread(new ParameterizedThreadStart(CrawlData));
                var record = new Record
                                 {
                                     Page = dataTable.Rows[i]["Page"].ToString(),
                                     Source = dataTable.Rows[i]["Source"].ToString(),
                                     CategoryID = Convert.ToInt32(dataTable.Rows[i]["CategoryID"].ToString()),
                                     HttpPrefix = dataTable.Rows[i]["HttpPrefix"].ToString(),
                                     ListStartAfter = dataTable.Rows[i]["ListStartAfter"].ToString(),
                                     PartternNodeList = dataTable.Rows[i]["PartternNodeList"].ToString(),
                                     PartternTitle = dataTable.Rows[i]["PartternTitle"].ToString(),
                                     PartternTeaser = dataTable.Rows[i]["PartternTeaser"].ToString(),
                                     PartternLink = dataTable.Rows[i]["PartternLink"].ToString(),
                                     PartternAvatar = dataTable.Rows[i]["PartternAvatar"].ToString(),
                                     PartternPubDate = dataTable.Rows[i]["PartternPubDate"].ToString(),
                                     PartternDetail = dataTable.Rows[i]["PartternDetail"].ToString(),
                                     PartternDetailRemove = dataTable.Rows[i]["PartternDetailRemove"].ToString(),
                                     FormatDate = dataTable.Rows[i]["FormatDate"].ToString(),
                                     PartternTags = dataTable.Rows[i]["PartternTags"].ToString(),
                                 };

                thread.Start(record);

            }

            Console.Write("Crawler is running ...");
            Console.ReadLine();
        }
    }

    public class Record
    {
        public string Page { get; set; }

        public string Source { get; set; }

        public int CategoryID { get; set; }

        public string HttpPrefix { get; set; }


        private string _ListStartAfter;
        public string ListStartAfter
        {
            get { return _ListStartAfter; }
            set { _ListStartAfter = value; }
        }

        private string _PartternNodeList;
        public string PartternNodeList
        {
            get { return _PartternNodeList; }
            set { _PartternNodeList = value; }
        }

        private string _PartternTitle;
        public string PartternTitle
        {
            get { return _PartternTitle; }
            set { _PartternTitle = value; }
        }

        private string _PartternTeaser;
        public string PartternTeaser
        {
            get { return _PartternTeaser; }
            set { _PartternTeaser = value; }
        }

        private string _PartternLink;
        public string PartternLink
        {
            get { return _PartternLink; }
            set { _PartternLink = value; }
        }

        private string _PartternAvatar;
        public string PartternAvatar
        {
            get { return _PartternAvatar; }
            set { _PartternAvatar = value; }
        }

        private string _PartternPubDate;
        public string PartternPubDate
        {
            get { return _PartternPubDate; }
            set { _PartternPubDate = value; }
        }

        private string _PartternDetail;
        public string PartternDetail
        {
            get { return _PartternDetail; }
            set { _PartternDetail = value; }
        }

        private string _PartternDetailRemove;
        public string PartternDetailRemove
        {
            get { return _PartternDetailRemove; }
            set { _PartternDetailRemove = value; }
        }

        private string _FormatDate;
         public string FormatDate
        {
            get { return _FormatDate; }
            set { _FormatDate = value; }
        }

         private string _PartternTags;
        public string PartternTags
        {
            get { return _PartternTags; }
            set { _PartternTags = value; }
        }  
        
    }
}

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Collections;
//using System.Net;
//using System.IO;

//namespace Crawler
//{
//    public class Record
//    {
//        public string Url { get; set; }

//        public string Page { get; set; }

//        public int MaxRecord { get; set; }

//        public int CategoryID { get; set; }

//        public string HttpPrefix { get; set; }

//        public string PatternGetTitle { get; set; }

//        public string PatternGetIntro { get; set; }

//        public string PatternGetContent { get; set; }
//    }
//    class Program
//    {
//        private static String strText;
//        static MatchCollection tagCollection;
//        public static HttpWebRequest req;
//        public static HttpWebResponse res;
//        static Stream resStream;
//        public static string baseUrl;

//        static void Main(string[] args)
//        {
//            //add the specific site that you want to crawl
//            baseUrl = "http://vnexpress.net";

//            ArrayList pages = new ArrayList();
//            pages.Add(baseUrl);

//            //start crawling
//            crawl(pages, 10);

//            Console.WriteLine("/nIndexing Complete!!");
//            Console.ReadLine();
//        }

//        public static void crawl(ArrayList pages, int depth)
//        {
//            MatchCollection mc;
//            ArrayList links = new ArrayList();

//            //Breadth-first search algorithm to crawl the pages and collect links
//            for (int i = 0; i < depth; i++)
//            {
//                ArrayList newpages = new ArrayList();

//                foreach (String page in pages)
//                {
//                    try
//                    {
//                        if (isValidUrl(page))
//                        {
//                            urlOpen();
//                        }
//                    }
//                    catch (Exception ex)
//                    {
//                        System.Console.WriteLine("Couldnot open {0} because {1}", page, ex.ToString());
//                        continue;
//                    }

//                    string pagecontent = read();

//                    //adding the page in the index
//                    addtoindex(page, pagecontent);

//                    mc = tagList(pagecontent, "a");

//                    links = getAttributeValue(mc, "href", baseUrl);

//                    foreach (string link in links)
//                    {
//                        String url, linktext;
//                        url = linktext = null;


//                        if (link.Contains("#"))
//                        {
//                            try
//                            {
//                                url = link;

//                            }
//                            catch (Exception ex)
//                            {
//                                Console.WriteLine("Error in Crawl " + ex.Message + " - " + url);
//                            }
//                        }
//                        else
//                        {
//                            url = link;
//                        }

//                        try
//                        {
//                            if ((url.Substring(0, 4) == "http") && (isindexed(url) == false))
//                            {

//                                newpages.Add(url);
//                            }

//                        }
//                        catch (Exception ex)
//                        {
//                            Console.WriteLine("Couldnot add new page " + url + " b/c {0}", ex.ToString());
//                        }
//                        linktext = gettextonly(pagecontent);
//                    }

//                }
//                pages = newpages;

//            }
//        }

//        //Returns false for now, but can be modified to query a database to check whether a page has already been indexed
//        public static bool isindexed(string url)
//        {
//            return false;
//        }

//        //Add page to the index, this is where a database or file system can be used
//        public static void addtoindex(string url, string pagecontent)
//        {

//            Console.WriteLine("Indexing : " + url);

//        }

//        //Get the collection of < a > tags in a page
//        public static MatchCollection tagList(String HTMLcontent, String tag)
//        {

//            Regex extractTags = new Regex(@"<" + tag + @"[^>]*?HREF\s*=\s*[""']?([^'"" >]+?)[ '""]?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
//            try
//            {
//                tagCollection = extractTags.Matches(HTMLcontent);

//                return tagCollection;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.ToString());
//            }
//            return null;
//        }

//        //Gets the HREF value from each <> tag
//        public static ArrayList getAttributeValue(MatchCollection mc, String Attr, string url)
//        {
//            ArrayList links = new ArrayList();//{ ""};

//            foreach (Match match in mc)
//            {

//                string temp = match.Value;

//                try
//                {
//                    if (temp.Contains("http"))
//                    {
//                        links.Add(temp.Substring(temp.IndexOf("href") + 6, temp.LastIndexOf(">") - temp.IndexOf("http") - 1));

//                    }

//                    else if (temp.Contains("://"))
//                    {
//                        links.Add(temp.Substring(temp.IndexOf("href") + 6, temp.LastIndexOf(">") - (temp.IndexOf("href") + 7)));
//                    }
//                    else
//                    {
//                        string strTemp = temp.Substring(temp.IndexOf("href") + 6, temp.LastIndexOf(">") - (temp.IndexOf("href") + 7));
//                        url.Replace("\n\r", "");

//                        //Ignore all anchor links
//                        if (strTemp.StartsWith("#"))
//                        {
//                            continue;
//                        }

//                        //Ignore all javascript calls

//                        if (strTemp.ToLower().StartsWith("javascript:"))
//                        {
//                            continue;
//                        }


//                        //Ignore all email links
//                        if (strTemp.ToLower().StartsWith("mailto:"))
//                        {
//                            continue;
//                        }

//                        //For internal links, build the url mapped to the base address
//                        if (!strTemp.StartsWith("http://") && !temp.StartsWith("https://"))
//                        {
//                            if (strTemp == "/")
//                            {
//                                continue;
//                            }
//                            else
//                            {
//                                if (strTemp[0] != '/' && url[url.Length - 1] != '/')
//                                {
//                                    strTemp = url + "/" + strTemp;
//                                }
//                                //else
//                                //{
//                                //    strTemp = MapUrl(url, strTemp);
//                                //}
//                            }

//                        }

//                        //if (strTemp[0] != '/' && url[url.Length - 1] != '/')
//                        //{
//                        //    strTemp = url + "/" + strTemp;
//                        //}
//                        //else
//                        //{
//                        //    strTemp = url + strTemp;
//                        //}
//                        links.Add(strTemp);
//                    }

//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine("Error in GetAttributes :" + ex.Message + " - " + url);
//                }

//            }
//            return links;

//        }

//        //reads that content of a web page
//        public static string gettextonly(string pagecontent)
//        {

//            string pattern = @"<(.|\n)*?>";
//            return Regex.Replace(pagecontent, pattern, String.Empty);

//        }


//        public static String read()
//        {
//            StreamReader sr = new StreamReader(resStream);
//            strText = sr.ReadToEnd();
//            return strText;
//        }

//        public static void urlOpen()
//        {
//            resStream = res.GetResponseStream();
//        }

//        public static bool isValidUrl(String url)
//        {
//            try
//            {
//                req = (HttpWebRequest)HttpWebRequest.Create(url);
//                res = (HttpWebResponse)req.GetResponse();
//                return (res.StatusCode == HttpStatusCode.OK);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Error in ISValidURL " + ex.Message + " - " + url);
//                return false;
//            }

//        }

//        private static string MapUrl(string baseAddress, string relativePath)
//        {
//            Uri u = new Uri(baseAddress);
//            if (relativePath == "./")
//            {
//                relativePath = "/";
//            }
//            if (relativePath.StartsWith("/"))
//            {
//                return u.Scheme + Uri.SchemeDelimiter + u.Authority + relativePath;
//            }
//            else
//            {
//                string pathAndQuery = u.AbsolutePath;
//                //If the baseAddress contains a file name, like ..../ Something.aspx
//                //Trim off the file name
//                pathAndQuery = pathAndQuery.Split('?')[0].TrimEnd('/');
//                if (pathAndQuery.Split('/')[pathAndQuery.Split('/').Count() - 1].Contains("."))
//                {
//                    pathAndQuery = pathAndQuery.Substring(0, pathAndQuery.LastIndexOf("/"));
//                }
//                baseAddress = u.Scheme + Uri.SchemeDelimiter + u.Authority + pathAndQuery;

//                //If the relativePath contains../ then
//                //adjust the baseAddress accordingly

//                while (relativePath.StartsWith("../"))
//                {
//                    relativePath = relativePath.Substring(3);
//                    if (baseAddress.LastIndexOf("/") > baseAddress.IndexOf("//" + 2))
//                    {
//                        baseAddress = baseAddress.Substring(0, baseAddress.LastIndexOf("/")).TrimEnd('/');
//                    }
//                }
//                return baseAddress + "/" + relativePath;
//            }
//        }
//    }
//}

