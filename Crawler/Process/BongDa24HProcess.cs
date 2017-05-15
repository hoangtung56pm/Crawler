using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Xml.Linq;
using Crawler.Lib;

namespace Crawler.Process
{
    public class BongDa24HProcess
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
                    var res = from item in xdoc.Descendants(xmlns + "div")
                              where item.Attribute("class") != null && item.Attribute("class").Value == "DivImgSumSALEHNNP"
                              select new
                                         {
                                             Link = item.Elements(xmlns + "a").ElementAt(0).Attribute("href").Value,
                                             Image = item.Elements(xmlns + "a").ElementAt(0).Element(xmlns + "img").Attribute("src").Value,
                                             Title = item.Elements(xmlns + "a").ElementAt(1).Value,
                                             Hour = "00:00",
                                             Date = "",
                                             Desc = item.Value
                                         };
                    foreach (var node in res)
                    {
                        var info = new ContentInfo
                                       {
                                           Title = node.Title,
                                           Teaser = node.Desc,
                                           Image = record.HttpPrefix + "/" + node.Image,
                                           Link = record.HttpPrefix + "/" + node.Link,
                                           CategoryID = record.CategoryID,
                                           CrawlerUrl = record.Url,
                                           Hour = node.Hour,
                                           Date = node.Date
                                       };

                        cl = new CrawlerClass(info.Link);

                        xdoc = cl.GetXDocument();

                        #region Get Hour and Date

                        var resDate = from item in xdoc.Descendants(xmlns + "span")
                                      where
                                          item.Attribute("class") != null &&
                                          item.Attribute("class").Value == "News_adt_DatePl"
                                      select new
                                      {
                                          Date = item.Value,
                                      };
                        //Thứ bẩy, ngày 19 tháng 03 năm 2011 cập nhật lúc 14:08
                        string newDate = resDate.ElementAt(0).Date.Trim();
                        newDate = newDate.Substring(newDate.IndexOf(',') + 1).Trim();
                        //ngày 19 tháng 03 năm 2011 cập nhật lúc 14:08
                        string[] arr = newDate.Split(' ');

                        info.Hour = arr[9].ToString().Trim();
                        info.Date = arr[1].ToString().Trim() + "/" + arr[3].ToString().Trim() + "/" + arr[5].ToString().Trim();

                        #endregion

                        #region Get Body

                        var resBody = from item in xdoc.Descendants(xmlns + "span")
                                      where item.Attribute("class") != null && item.Attribute("class").Value == "News_ArticleDetailContent"
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
