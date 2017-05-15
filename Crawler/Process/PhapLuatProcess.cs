using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Crawler.Lib;

namespace Crawler.Process
{
    public class PhapLuatProcess
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(Program));

        public static void Process(Record record)
        {
            try
            {
                #region Process

                string xmlns = "{http://www.w3.org/1999/xhtml}";
                var cl = new CrawlerClass(record.Url);
                XDocument xdoc = cl.GetXDocument();

                if (xdoc != null)
                {
                    var res = from item in xdoc.Descendants(xmlns + "div")
                              where
                                  item.Attribute("class") != null &&
                                  (item.Attribute("class").Value == "box14 fl" ||
                                   item.Attribute("class").Value == "box13 fl")
                              select new
                                         {
                                             Link =
                                  item.Elements(xmlns + "a").Count() > 1
                                      ? item.Elements(xmlns + "a").ElementAt(1).Attribute("href").Value
                                      : item.Elements(xmlns + "a").ElementAt(0).Attribute("href").Value,
                                             Image =
                                  item.Elements(xmlns + "a").Count() > 1
                                      ? item.Elements(xmlns + "a").ElementAt(0).Element(xmlns + "img").Attribute("src").
                                            Value
                                      : "",
                                             Title =
                                  item.Elements(xmlns + "a").Count() > 1
                                      ? item.Elements(xmlns + "a").ElementAt(1).Value
                                      : item.Elements(xmlns + "a").ElementAt(0).Value,
                                             Hour = "00:00",
                                             Date = "",
                                             Desc = item.Element(xmlns + "div").Value
                                         };
                    foreach (var node in res)
                    {
                        var info = new ContentInfo
                                       {
                                           Title = node.Title,
                                           Teaser = node.Desc,
                                           Image = node.Image,
                                           Link = record.HttpPrefix + node.Link,
                                           CategoryID = record.CategoryID,
                                           CrawlerUrl = record.Url,
                                           Hour = node.Hour,
                                           Date = node.Date
                                       };

                        cl = new CrawlerClass(info.Link);

                        xdoc = cl.GetXDocument();

                        #region Get Title

                        //var resTitle = from item in xdoc.Descendants(xmlns + "div")
                        //               where
                        //                   item.Attribute("class") != null &&
                        //                   item.Attribute("class").Value == "mt1 ml2 mr2 pt1 pb1"
                        //               select new
                        //                          {
                        //                              Title = item.Elements(xmlns + "div").ElementAt(0).Value,
                        //                          };

                        //info.Title = resTitle.ElementAt(0).Title;

                        #endregion

                        #region Get Hour and Date

                        var resDate = from item in xdoc.Descendants(xmlns + "p")
                                      where
                                          item.Attribute("class") != null &&
                                          item.Attribute("class").Value == "detail_time"
                                      select new
                                                 {
                                                     Date = item.Value,
                                                 };
                        //19/03/2011 - 12:56 AM
                        string newDate = resDate.ElementAt(0).Date.Trim();
                        string[] arr = newDate.Split('-');

                        info.Hour = arr[1].ToString().Trim();
                        info.Date = arr[0].ToString().Trim();

                        #endregion

                        #region Get body

                        var resBody = from item in xdoc.Descendants(xmlns + "div")
                                      where
                                          item.Attribute("id") != null && item.Attribute("id").Value == "contentdetail"
                                      select new
                                                 {
                                                     Description = item.Value,
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
