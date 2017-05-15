using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Crawler.Lib;
using HtmlAgilityPack;
using System.Web;
using System.Globalization;
using System.Text;

namespace Crawler.Process
{
    public class MyWebClient : WebClient
    {

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request.GetType() == typeof(HttpWebRequest))
            {
                ((HttpWebRequest)request).UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)";
            }
            return request;
        }
    }

    public class Process
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(Program));

        //public static void Process(Record record)
        //{
        //    try
        //    {
        //        #region Process

        //        string xmlns = "{http://www.w3.org/1999/xhtml}";
        //        var cl = new CrawlerClass(record.Url);
        //        XDocument xdoc = cl.GetXDocument();

        //        if (xdoc != null)
        //        {
        //            var res = from item in xdoc.Descendants(xmlns + "div")
        //                      where
        //                          item.Attribute("class") != null &&
        //                          (item.Attribute("class").Value == "folder-news" ||
        //                           item.Attribute("class").Value == "folder-top")
        //                          && item.Element(xmlns + "a") != null
        //                      select new
        //                                 {
        //                                     Link = item.Element(xmlns + "a").Attribute("href").Value,
        //                                     Image =
        //                          item.Element(xmlns + "a").Element(xmlns + "img").Attribute("src").Value,
        //                                     Title = item.Elements(xmlns + "p").ElementAt(0).Element(xmlns + "a").Value,
        //                                     Hour =
        //                          item.Elements(xmlns + "p").ElementAt(0).Elements(xmlns + "label").ElementAt(0).Value,
        //                                     Date =
        //                          item.Elements(xmlns + "p").ElementAt(0).Elements(xmlns + "label").ElementAt(1).Value,
        //                                     Desc = item.Elements(xmlns + "p").ElementAt(1).Value
        //                                 };
        //            foreach (var node in res)
        //            {
        //                var info = new ContentInfo
        //                               {
        //                                   Title = node.Title,
        //                                   Teaser = node.Desc,
        //                                   Image = record.HttpPrefix + node.Image,
        //                                   Link = record.HttpPrefix + node.Link,
        //                                   CategoryID = record.CategoryID,
        //                                   CrawlerUrl = record.Url,
        //                                   Hour = node.Hour,
        //                                   Date = node.Date
        //                               };

        //                cl = new CrawlerClass(info.Link);

        //                xdoc = cl.GetXDocument();
        //                var res1 = from item in xdoc.Descendants(xmlns + "div")
        //                           where item.Attribute("class") != null && item.Attribute("class").Value == "content"
        //                           select new
        //                                      {
        //                                          Description = item.Elements(xmlns + "div").ElementAt(0).Value
        //                                      };

        //                string body = res1.ElementAt(0).Description;

        //                if (body.IndexOf("//") > 0) body = body.Substring(0, body.IndexOf("//"));
        //                info.Body = body;

        //                AppEnv.Insert(info);

        //                _logger.Debug("---------------------------------");
        //                _logger.Debug("Title: " + info.Title);
        //                _logger.Debug("Desc : " + info.Teaser);
        //                _logger.Debug("Image: " + info.Image);
        //                _logger.Debug("Link : " + info.Link);
        //                _logger.Debug("Url  : " + info.CrawlerUrl);
        //            }
        //        }

        //        #endregion
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Debug("----------------Error-----------------");
        //        _logger.Debug("Message: " + ex.Message);
        //        _logger.Debug("StackTrace : " + ex.StackTrace);
        //        _logger.Debug("Category: " + record.CategoryID);
        //        _logger.Debug("Link : " + record.Url);
        //    }
        //}
        public static void ProcessTest(Record record)
        {
            string test = StartMining(record);
            Console.WriteLine(test);
        }

        private static void WriteTextToFile(string text, string fileName)
        {
            // Write the string to a file.
            System.IO.StreamWriter file = new System.IO.StreamWriter("c:\\" + fileName + ".html");
            file.WriteLine(text);

            file.Close();

        }

        //private static void GetTopNews(string html, FilterFullInfo record)
        //{
        //    try
        //    {
        //        long totalSize = 0;

        //        //string p = "<div class="folder-top">.*?<a.*?href="(?<Link>.*?)".*?><img.*?src="(?<ImagePath>.*?)".*?/></a>.*?<p><a.*?href=".*?".*?class="link-topnews">(?<Title>.*?)</a>.*?<label class="item-time">(?<Time>.*?)</label>.*?<label class="item-date">(?<Date>.*?)</label></p>.*?<p>(?<Teaser>.*?)</p>.*?</div>";
        //        MatchCollection mcList = Regex.Matches(html, record.ParternGetTopNews, RegexOptions.IgnoreCase | RegexOptions.Singleline);


        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Info(string.Format("Error = {0}", ex.Message) + Environment.NewLine);

        //    }

        //}

        //Tra ve 1 mang chua cac link va image thumb
        private static string StartMining(Record record)
        {
            string url = record.Source;
            long totalSize = 0;
            string html = RunBrowser(record.Source, out totalSize);

            if (record.ListStartAfter.IndexOf("*[@") > 0)
            {
                HtmlDocument docList = new HtmlDocument();
                docList.LoadHtml(html);
                HtmlNode nodes = docList.DocumentNode.SelectSingleNode(record.ListStartAfter);

                if (nodes != null)
                {
                    html = nodes.InnerHtml;
                }
                else
                {
                    html = "";
                }
            }
            else
            {
                if (html.IndexOf(record.ListStartAfter.Replace(".*", "")) > 0)
                {
                    html = Regex.Match(html, record.ListStartAfter, RegexOptions.IgnoreCase | RegexOptions.Singleline).Value;

                    // Remove doan duoi
                    html = Regex.Replace(html, record.ListEndAt, "", RegexOptions.Singleline);
                }
                else
                {
                    html = "";
                }
            }

            if (html != "")
            {

                html = html.Replace("\r\n", "").Replace("> <", "><");

                HtmlDocument doc1 = new HtmlDocument();
                doc1.LoadHtml(html);
                HtmlNodeCollection nodes = doc1.DocumentNode.SelectNodes("//li");

                if (nodes != null)
                {
                    foreach (HtmlNode itemList in nodes)
                    {
                        HtmlNode nodesImage = itemList.SelectSingleNode("//*[@class=\"box-wiget-content-img\"]/a");
                        string imgAvatar = nodesImage.InnerHtml;

                        HtmlNode nodesTitle = itemList.SelectSingleNode("//*[@class=\"box-wiget-content-title fsize14\"]/a");
                        string title = nodesTitle.InnerHtml;

                        HtmlNode nodesTeaser = itemList.SelectSingleNode("//*[@class=\"box-wiget-content-des\"]/span");
                        string Teaser = nodesTeaser.InnerHtml;

                        HtmlNode nodesLink = itemList.SelectSingleNode("//*[@class=\"box-wiget-content-img\"]/a[@href]");
                        HtmlAttribute linkAtt = nodesLink.Attributes["href"];
                        string link = linkAtt.Value;

                        HtmlNode nodesPubDate = itemList.SelectSingleNode("//*[@class=\"box-wiget-content-timpost\"]");
                        string pubDate = nodesPubDate.InnerHtml;
                        
                        //MatchCollection mcList = Regex.Matches(html, record.ParternList, RegexOptions.IgnoreCase | RegexOptions.Singleline);

                        var list = new List<ObjectLink>();
                        var listImage = new List<ObjectImage>();

                        int i = 0;

                        //if (mcList != null && mcList.Count > 0)
                        //{
                        //    _logger.Debug("Matchs  : " + mcList.Count.ToString());

                        //foreach (Match match in mcList)
                        //{
                        #region Get main content

                        var contentInfo = new ContentInfo();

                        contentInfo.Content_Headline = AppEnv.NCRToUnicode(Regex.Replace(title, "<.*?>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline));// AppEnv.NCRToUnicode(Regex.Replace(match.Groups["Title"].Value, "<.*?>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline));
                        contentInfo.Content_Url = link;// OptimiseUrl(match.Groups["Link"].Value, record);

                        try
                        {
                            Uri uri = new Uri(contentInfo.Content_Url);
                            Uri uri1 = new Uri(record.Source);
                            if (uri.Host != uri1.Host)
                            {
                                continue;
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }


                        long totalSizeTemp = 0;

                        string contentDetail = GetContentDetail(contentInfo.Content_Url, record, out totalSizeTemp);
                        if (contentDetail.Trim() == "")
                        {
                            continue;
                        }
                        contentInfo.Content_Teaser =  StripTagsRegex(Teaser);// StripTagsRegex(match.Groups["Teaser"].Value);
                        if (contentInfo.Content_Teaser.Trim() == "")
                        {
                            continue;
                        }
                        contentInfo.Content_Body = GetContentBody(contentDetail, record);
                        if (contentInfo.Content_Body.Trim() == "")
                        {
                            continue;
                        }

                        string date = "";
                        date = pubDate;// match.Groups["PubDate"].Value;
                        if (date == "")
                        {
                            if (record.PatternPubDate != "")
                            {
                                date = GetPubDateByParttern(contentDetail, record);
                            }
                            else
                            {
                                date = GetPubDate(contentInfo.Content_Body, record);
                            }
                        }

                        contentInfo.Content_CreateDate = OpitmisePubDate(date);

                        contentInfo.Content_Status = 1;
                        contentInfo.CategoryID = record.CategoryID;
                        contentInfo.Content_UserID = 212;
                        contentInfo.Content_HeadlineKD = UnicodeUtility.UnicodeToKoDau(contentInfo.Content_Headline);
                        contentInfo.Content_TeaserKD = UnicodeUtility.UnicodeToKoDau(contentInfo.Content_Teaser);
                        contentInfo.Content_Source = record.Page;

                        string imagePath = imgAvatar;// match.Groups["ImagePath"].Value.Trim();
                        if (imagePath.IndexOf("src=\"") > -1)
                        {
                            imagePath = imagePath.Substring(imagePath.IndexOf("src=\"") + 5);
                            imagePath = imagePath.Substring(0, imagePath.IndexOf("\""));
                        }
                        if (imagePath.IndexOf("src='") > -1)
                        {
                            imagePath = imagePath.Substring(imagePath.IndexOf("src='") + 5);
                            imagePath = imagePath.Substring(0, imagePath.IndexOf("'"));
                        }
                        string imageDownloadUrl = "";
                        string imageName = "";
                        if (Convert.ToInt32(AppEnv.GetSetting("SaveImage")) == 1)
                        {
                            #region lay anh dau tien trong bai chi tiet

                            HtmlDocument doc = new HtmlDocument();
                            doc.LoadHtml(contentInfo.Content_Body);

                            HtmlNodeCollection node = doc.DocumentNode.SelectNodes("//img");
                            if (node != null)
                            {
                                foreach (HtmlNode img in node)
                                {
                                    HtmlAttribute att = img.Attributes["src"];
                                    imageDownloadUrl = att.Value;

                                    string exceptionImg = AppEnv.GetSetting("exceptionImg");
                                    string[] arr = exceptionImg.Split(',');
                                    int flag = 0;
                                    foreach (string item in arr)
                                    {
                                        if (item != "")
                                        {
                                            if (imageDownloadUrl.Contains(item))
                                            {
                                                imageDownloadUrl = "";
                                                flag = 1;
                                                break;
                                            }
                                        }
                                    }

                                    if (flag == 1)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                imageDownloadUrl = imagePath;
                            }

                            #endregion

                            imageDownloadUrl = OptimiseUrl(imageDownloadUrl, record);
                            imageName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".jpg";

                            var objectImage = new ObjectImage
                            {
                                ImageName = imageName,
                                Link = imageDownloadUrl
                            };

                            listImage.Add(objectImage);

                            contentInfo.Content_Avatar = AppEnv.GetSetting("virtualPath") + DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + "/" + imageName;
                        }
                        else
                        {
                            contentInfo.Content_Avatar = record.HttpPrefix + imagePath;
                        }
                        contentInfo.Content_BigAvatar = "";

                        if (i < 6)
                        {
                            contentInfo.Content_Rank = 2;
                        }
                        else
                        {
                            contentInfo.Content_Rank = 1;
                        }

                        contentInfo.IsPublished = true;

                        try
                        {
                            DateTime dt = Convert.ToDateTime(contentInfo.Content_CreateDate);

                            if (dt.Year == DateTime.Now.Year && dt.Month == DateTime.Now.Month && dt.Day < DateTime.Now.Day)
                            {
                                continue;
                            }
                            else
                            {
                                if (dt.Year < DateTime.Now.Year || dt.Month < DateTime.Now.Month)
                                {
                                    continue;
                                }
                                else
                                {
                                    int returnValue = AppEnv.Insert(contentInfo);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            try
                            {
                                DateTime dt = DateTime.Parse(contentInfo.Content_CreateDate, new CultureInfo("fr-FR", false));
                                if (dt.Year == DateTime.Now.Year && dt.Month == DateTime.Now.Month && dt.Day < DateTime.Now.Day)
                                {
                                    continue;
                                }
                                else
                                {
                                    if (dt.Year < DateTime.Now.Year || dt.Month < DateTime.Now.Month)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        int returnValue = AppEnv.Insert(contentInfo);
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                int returnValue = AppEnv.Insert(contentInfo);
                            }
                        }

                        #endregion

                        i++;

                        totalSize += totalSizeTemp;
                        //}

                        string physicalPath = AppEnv.GetSetting("phyPath") + DateTime.Now.Year + "\\" + DateTime.Now.Month + "\\" + DateTime.Now.Day + "\\";

                        #region Download Image

                        if (listImage != null && listImage.Count > 0)
                        {
                            int totalImage = 0;
                            foreach (ObjectImage item in listImage)
                            {
                                Image image = DownloadImage(item.Link);

                                if (image != null)
                                {
                                    if (!Directory.Exists(physicalPath))
                                    {
                                        Directory.CreateDirectory(physicalPath);
                                    }

                                    image.Save(physicalPath + item.ImageName, ImageFormat.Jpeg);
                                    totalImage++;
                                }
                            }
                        }


                        #endregion

                        _logger.Info(string.Format("Finished mining with source = {0}", record.Source) + Environment.NewLine);
                        //}

                    }
                    return html;
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }

        private static string GetContentDetail(string url, Record record, out long totalSize)
        {
            try
            {
                string html = RunBrowser(url, out totalSize);

                html = html.Replace("\r\n", "").Replace("> <", "><");

                if (html != string.Empty)
                {
                    //if (html.IndexOf(record.DetailStartAfter.Replace(".*", "")) > 0)
                    //{

                    //    //Bo doan dau
                    //    html = Regex.Match(html, record.DetailStartAfter, RegexOptions.IgnoreCase | RegexOptions.Singleline).Value;

                    //    // Remove doan duoi
                    //    html = Regex.Replace(html, record.DetailEndAt, "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                    html = Regex.Replace(html, "\t", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    html = Regex.Replace(html, "\r", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    html = Regex.Replace(html, "\n", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                    return html;
                    //}
                    //else
                    //{
                    //    return "";
                    //}
                }
                else
                {
                    return "";
                }
            }
            catch (TimeoutException ex)
            {
                _logger.Info(string.Format("TimeoutException mining with source = {0}, {1}", url, ex.Message) + Environment.NewLine);
                totalSize = 0;
                return "";
            }
            catch (Exception ex)
            {
                _logger.Info(string.Format("Exception mining with source = {0}, {1}", url, ex.Message) + Environment.NewLine);
                totalSize = 0;
                return "";
            }
        }

        public static string StripTagsRegex(string source)
        {
            source = Regex.Replace(source, "<.*?>", string.Empty);
            if (source.IndexOf(">") > 0)
            {
                Match result = Regex.Match(source, @"^.*?(?=>)");
                return result.Value;
            }
            else
            {
                return source;
            }
        }
        private static string GetContentBodyWithNode(string content, Record record)
        {
            string contentBody = "";
            try
            {

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(content);
                HtmlNode nodes = doc.DocumentNode.SelectSingleNode(record.PatternDetail);

                if (nodes != null)
                {
                    return nodes.InnerHtml;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                contentBody = "";
            }

            return contentBody;

        }

        private static string GetContentBody(string content, Record record)
        {
            if (record.PatternDetail.IndexOf("*[@") > 0)
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(content);

                if (record.LinkCommonPartern != "" && record.LinkCommonPartern.IndexOf("*[@") > 0)
                {
                    var nodesToRemove = doc.DocumentNode.SelectSingleNode(record.LinkCommonPartern);
                    if (nodesToRemove != null)
                    {
                        nodesToRemove.Remove();
                    }
                }
                HtmlNode nodes = doc.DocumentNode.SelectSingleNode(record.PatternDetail);

                if (nodes != null)
                {
                    return nodes.InnerHtml;
                }
                else
                {
                    return "";
                }
            }
            else
            {
                string contentBody = "";
                try
                {
                    contentBody = Regex.Match(content, record.PatternDetail, RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups["ContentBody"].Value;

                }
                catch (TimeoutException ex)
                {
                    contentBody = "";
                }
                catch (Exception)
                {
                    contentBody = "";
                }

                return contentBody;
            }

        }

        private static string GetContentTeaser(string content, Record record)
        {
            string contentTeaser = "";
            try
            {

                contentTeaser = Regex.Match(content, record.PatternDetail, RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups["Teaser"].Value;

            }
            catch (TimeoutException ex)
            {
                contentTeaser = "";
            }
            catch (Exception)
            {
                contentTeaser = "";
            }

            return contentTeaser;

        }

        private static string OpitmisePubDate(string pubDate)
        {
            try
            {
                pubDate = Regex.Replace(pubDate, "<.*?>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                if (pubDate.IndexOf(",") > -1)
                {
                    string tempPubDate = pubDate.Substring(pubDate.IndexOf(","));

                    if (tempPubDate.IndexOf("/") > -1)
                    {
                        pubDate = pubDate.Substring(pubDate.IndexOf(",") + 1);
                    }
                    else
                    {
                        pubDate = pubDate.Replace(",", "");
                    }
                }

                //13:17 | 16/01/2014
                pubDate = pubDate.Replace(" - ", " ").Replace(", ", " ").Replace("ngày", "").Replace("Cập nhật lúc", "").Replace("&nbsp;|&nbsp;", " ").Trim();
                pubDate = pubDate.Replace(".", "/").Replace("PM", "").Replace("(GMT+7)", "").Replace("GMT+7", "").Replace("AM", "").Replace("SA", "").Replace("CH", "").Replace("| ", "");

                string[] arr = pubDate.Trim().Split(' ');

                if (arr.Count() > 1)
                {
                    if (arr[0].IndexOf(":") > -1)
                    {

                        if (arr[1].ToString().IndexOf("/") < 0)
                        {
                            pubDate = DateTime.Now.ToString("dd/MM/yyyy") + " " + arr[0];
                        }
                        else
                        {
                            pubDate = arr[1] + " " + arr[0];
                        }
                    }
                    else
                    {
                        pubDate = arr[0] + " " + arr[1];
                    }
                }
                else
                {
                    if (pubDate != "" && pubDate.IndexOf(":") > -1)
                    {
                        return DateTime.Now.ToString("dd/MM/yyyy") + " " + pubDate;
                    }
                }
                return pubDate == "" ? DateTime.Now.ToString("dd/MM/yyyy HH:mm") : pubDate;
            }
            catch (Exception)
            {
                return pubDate == "" ? DateTime.Now.ToString("dd/MM/yyyy HH:mm") : pubDate;
            }

        }

        private static string GetPubDateByParttern(string content, Record record)
        {
            string pubDate = "";
            try
            {


                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(content);
                HtmlNode nodes = doc.DocumentNode.SelectSingleNode(record.PatternPubDate);

                pubDate = nodes.InnerHtml;
                pubDate = Regex.Replace(pubDate, "<.*?>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                return pubDate;
            }
            catch (Exception)
            {
                pubDate = "";
            }

            return pubDate == "" ? DateTime.Now.ToString("dd/MM/yyyy HH:mm") : pubDate;

        }

        private static string GetPubDate(string content, Record record)
        {
            string pubDate = "";
            try
            {
                pubDate =
                    Regex.Match(content, record.PatternDetail, RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups
                        ["PubDate"].Value;
                pubDate = Regex.Replace(pubDate, "<.*?>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                if (pubDate.IndexOf(",") > -1)
                {
                    pubDate = pubDate.Substring(pubDate.IndexOf(","));
                }
                //13:17 | 16/01/2014
                pubDate = pubDate.Replace(" - ", " ").Replace(", ", " ").Replace("ngày", "").Replace("Cập nhật lúc", "").Replace("&nbsp;|&nbsp;", " ").Trim();
                pubDate = pubDate.Replace(".", "/").Replace("PM", "").Replace("AM", "").Replace("SA", "").Replace("CH", "").Replace("| ", "");

                string[] arr = pubDate.Trim().Split(' ');

                if (arr.Count() > 1)
                {
                    if (arr[0].IndexOf(":") > -1)
                    {
                        pubDate = arr[1] + " " + arr[0];
                    }
                    else
                    {
                        pubDate = arr[0] + " " + arr[1];
                    }
                }

            }
            catch (Exception)
            {
                pubDate = "";
            }

            return pubDate == "" ? DateTime.Now.ToString("dd/MM/yyyy HH:mm") : pubDate;

        }

        private static string RunBrowser(string link, out long TotalSize)
        {
            string[] useragent = { "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_2) AppleWebKit/537.13 (KHTML, like Gecko) Chrome/24.0.1290.1 Safari/537.13"
                                 , "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_7_4) AppleWebKit/537.13 (KHTML, like Gecko) Chrome/24.0.1290.1 Safari/537.13"
                                 , "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.13 (KHTML, like Gecko) Chrome/24.0.1284.0 Safari/537.13"
                                 , "Mozilla/5.0 (Windows NT 6.1; rv:15.0) Gecko/20120716 Firefox/15.0a2"
                                 , "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.1.16) Gecko/20120427 Firefox/15.0a1"
                                 , "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:15.0) Gecko/20120427 Firefox/15.0a1"
                                 , "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:15.0) Gecko/20120910144328 Firefox/15.0.2"
                                 , "Mozilla/5.0 (X11; Ubuntu; Linux i686; rv:15.0) Gecko/20100101 Firefox/15.0.1"
                                 , "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:14.0) Gecko/20120405 Firefox/14.0a1"
                                 , "Mozilla/5.0 (Windows NT 6.1; rv:14.0) Gecko/20120405 Firefox/14.0a1"
                                 , "Mozilla/5.0 (Windows NT 5.1; rv:14.0) Gecko/20120405 Firefox/14.0a1"
                                 , "Mozilla/5.0 (compatible; MSIE 10.6; Windows NT 6.1; Trident/5.0; InfoPath.2; SLCC1; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; .NET CLR 2.0.50727) 3gpp-gba UNTRUSTED/1.0"
                                 , "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; WOW64; Trident/6.0)"
                                 , "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)"
                                 , "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/5.0)"
                                 , "Mozilla/5.0 (Windows; U; MSIE 9.0; WIndows NT 9.0; en-US))"
                                 , "Mozilla/5.0 (Windows; U; MSIE 9.0; Windows NT 9.0; en-US)"
                                 , "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 7.1; Trident/5.0)"
                                 , "Mozilla/5.0 (compatible; MSIE 8.0; Windows NT 6.1; Trident/4.0; GTB7.4; InfoPath.2; SV1; .NET CLR 3.3.69573; WOW64; en-US)"
                                 , "Mozilla/5.0 (compatible; MSIE 8.0; Windows NT 6.0; Trident/4.0; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; .NET CLR 1.0.3705; .NET CLR 1.1.4322) Mozilla/5.0 (compatible; MSIE 8.0; Windows NT 6.0; Trident/4.0; InfoPath.1; SV1; .NET CLR 3.8.36217; WOW64; en-US)"
                                 , "Mozilla/4.0(compatible; MSIE 7.0b; Windows NT 6.0)"
                                 , "Mozilla/4.0 (compatible; MSIE 7.0b; Windows NT 6.0)"
                                 , "Mozilla/5.0 (Windows; U; MSIE 7.0; Windows NT 6.0; en-US)"
                                 , "Mozilla/5.0 (Windows; U; MSIE 7.0; Windows NT 6.0; el-GR)"
                                 , "Mozilla/5.0 (Windows; U; MSIE 7.0; Windows NT 5.2)"};
            Random rdn = new Random();
            int uan = rdn.Next(0, 24);
            string strUa = useragent[uan];
            WebClient client = new WebClient();
            client.Encoding = Encoding.UTF8;
            client.Headers.Set("User-Agent", strUa);
            string content = string.Empty;

            try
            {
                content = client.DownloadString(link);
                TotalSize = content.Length;
            }
            catch (Exception ex)
            {
                _logger.Info(string.Format("Error RunBrowser {0} {1}", link, ex.Message) + Environment.NewLine);
                TotalSize = 0;
                return string.Empty;
            }

            return HttpUtility.HtmlDecode(content);

            //try
            //{

            //    WebRequest obj = WebRequest.Create(link);
            //    obj.Headers.Add("UserAgent", strUa);
            //    obj.Headers.Add("Encoding", "UTF8");
            //    //Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)
            //    //"Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:25.0) Gecko/20100101 Firefox/25.0"
            //    //"Mozilla/5.0 (Macintosh; Intel Mac OS X 10.6; rv:25.0) Gecko/20100101 Firefox/25.0"

            //    obj.Credentials = CredentialCache.DefaultCredentials;
            //    // Lấy đáp ứng. công việc này sẽ lấy nội dung trang web về
            //    WebResponse webRespone = obj.GetResponse();
            //    TotalSize = webRespone.ContentLength;
            //    // Đọc đáp ứng (dạng stream).
            //    StreamReader sr = new StreamReader(webRespone.GetResponseStream());
            //    string result = sr.ReadToEnd();
            //    return HttpUtility.HtmlDecode(result);

            //}
            //catch (TimeoutException ex)
            //{
            //    _logger.Info(string.Format("Error RunBrowser {0} {1}", link, ex.Message) + Environment.NewLine);
            //    TotalSize = 0;
            //    return string.Empty;
            //}
            //catch (Exception ex)
            //{
            //    if (ex is WebException && ((WebException)ex).Status == WebExceptionStatus.ProtocolError)
            //    {
            //        WebResponse errResp = ((WebException)ex).Response;
            //        StreamReader respStream = new StreamReader(errResp.GetResponseStream());

            //        string result = respStream.ReadToEnd();
            //        TotalSize = errResp.ContentLength;
            //        return HttpUtility.HtmlDecode(result);

            //    }
            //    else
            //    {
            //        _logger.Info(string.Format("Error RunBrowser {0} {1}", link, ex.Message) + Environment.NewLine);
            //        TotalSize = 0;
            //        return string.Empty;
            //    }
            //}
        }

        private static string OptimiseUrl(string relativePath, Record record)
        {
            if (relativePath.StartsWith("//"))
            {
                return "http:" + relativePath;
            }
            if (relativePath.Trim() == string.Empty)
            {
                return relativePath;
            }
            string baseAddress = record.HttpPrefix;
            var u = new Uri(baseAddress);
            if (relativePath == "./")
            {
                relativePath = "/";
            }
            if (relativePath.StartsWith("/"))
            {
                return u.Scheme + Uri.SchemeDelimiter + u.Authority + relativePath;
            }
            if (relativePath.StartsWith("http://"))
            {
                return relativePath;
            }
            string pathAndQuery = u.AbsolutePath;
            //If the baseAddress contains a file name, like ..../ Something.aspx
            //Trim off the file name
            pathAndQuery = pathAndQuery.Split('?')[0].TrimEnd('/');
            if (pathAndQuery.Split('/')[pathAndQuery.Split('/').Count() - 1].Contains("."))
            {
                pathAndQuery = pathAndQuery.Substring(0, pathAndQuery.LastIndexOf("/"));
            }
            baseAddress = u.Scheme + Uri.SchemeDelimiter + u.Authority + pathAndQuery;

            //If the relativePath contains../ then
            //adjust the baseAddress accordingly

            while (relativePath.StartsWith("../"))
            {
                relativePath = relativePath.Substring(3);
                if (baseAddress.LastIndexOf("/") > baseAddress.IndexOf("//" + 2))
                {
                    baseAddress = baseAddress.Substring(0, baseAddress.LastIndexOf("/")).TrimEnd('/');
                }
            }
            return baseAddress + "/" + relativePath;
        }

        public static Image DownloadImage(string _URL)
        {
            Image _tmpImage = null;

            try
            {
                // Open a connection
                System.Net.HttpWebRequest _HttpWebRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(_URL);

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
            catch (TimeoutException ex)
            {
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }

            return _tmpImage;
        }
    }
}
