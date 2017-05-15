using System;
using System.Linq;
using System.Xml.Linq;
using Crawler.Lib;

namespace Crawler.Process
{
    public class NhacVietProcess
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(Program));

        public static void Process(Record record)
        {
            try
            {
                #region Process

                string xmlns = "";
                var cl = new CrawlerClass(record.Url);
                XDocument xdoc = cl.GetXDocument();

                if (xdoc != null)
                {
                    var res = from item in xdoc.Descendants(xmlns + "td")
                              where
                                  item.Attribute("class") != null && item.Attribute("class").Value == "text" &&
                                  item.Attribute("colspan") != null && item.Attribute("colspan").Value == "2"
                              select new
                                         {
                                             Link =
                                  item.Elements(xmlns + "p").ElementAt(0).Element(xmlns + "span").Element(xmlns + "a").
                                  Attribute("href").Value,
                                             Image =
                                  item.Elements(xmlns + "table").ElementAt(0).Element(xmlns + "tr").Elements(xmlns +
                                                                                                             "td").
                                  ElementAt(0).Element(xmlns + "a").Element(xmlns + "img").Attribute("src").Value,
                                             Title =
                                  item.Elements(xmlns + "p").ElementAt(0).Element(xmlns + "span").Element(xmlns + "a").
                                  Value,
                                             Hour = "00:00",
                                             Date = "",
                                             Desc =
                                  item.Elements(xmlns + "p").ElementAt(0).Element(xmlns + "span").Value
                                         };
                    foreach (var node in res)
                    {
                        var info = new ContentInfo
                                       {
                                           Title = node.Title.Replace("\t", ""),
                                           Teaser =
                                               node.Desc.Replace("\t", "").Substring(node.Desc.IndexOf("<br>") + 4).
                                               Replace("<br>", ""),
                                           Image = record.HttpPrefix + node.Image,
                                           Link = record.HttpPrefix + node.Link,
                                           CategoryID = record.CategoryID,
                                           CrawlerUrl = record.Url,
                                           Hour = node.Hour,
                                           Date = node.Date
                                       };

                        cl = new CrawlerClass(info.Link);

                        xdoc = cl.GetXDocument();

                        #region Get Hour and Date

                        var resDate = from item in xdoc.Descendants(xmlns + "td")
                                      where
                                          item.Attribute("class") != null &&
                                          item.Attribute("class").Value == "posted_date"
                                      select new
                                                 {
                                                     Date = item.Value,
                                                 };
                        //Chủ nhật, 20/3/2011, 22:41 GMT+7 						
                        string newDate = resDate.ElementAt(0).Date.Trim();
                        string[] arr = newDate.Split(',');

                        info.Hour = arr[2].ToString().Trim().Trim().Substring(0, 5);
                        info.Date = arr[1].ToString().Trim();

                        #endregion

                        #region Get Body

                        var resBody = from item in xdoc.Descendants(xmlns + "span")
                                      where
                                          item.Attribute("class") != null && item.Attribute("class").Value == "textbai"
                                      select new
                                                 {
                                                     Description = item.Value
                                                 };

                        string body = resBody.ElementAt(0).Description;

                        if (body.IndexOf("//") > 0) body = body.Substring(0, body.IndexOf("//"));
                        info.Body = body;

                        #endregion

                        AppEnv.Insert(info);

                        _logger.Debug("---------------------------------");
                        _logger.Debug("Title: " + info.Title);
                        _logger.Debug("Desc : " + info.Teaser);
                        _logger.Debug("Image: " + info.Image);
                        _logger.Debug("Link : " + info.Link);
                        _logger.Debug("Url  : " + info.CrawlerUrl);
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                _logger.Debug("----------------Error-----------------");
                _logger.Debug("Message: " + ex.Message);
                _logger.Debug("StackTrace : " + ex.StackTrace);
                _logger.Debug("Category: " + record.CategoryID);
                _logger.Debug("Link : " + record.Url);
            }
        }
    }
}
