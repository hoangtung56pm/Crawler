using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Crawler.Lib;

namespace Crawler.Process
{
    public class DanTriProcess
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
                              where item.Attribute("class") != null && (item.Attribute("class").Value == "mt3 clearfix")
                                    && item.Element(xmlns + "a") != null
                              select new
                                         {
                                             Link = item.Element(xmlns + "a").Attribute("href").Value,
                                             Image =
                                  item.Element(xmlns + "a").Element(xmlns + "img").Attribute("src").Value,
                                             Title = item.Element(xmlns + "div").Element(xmlns + "a").Value,
                                             Hour = "00:00",
                                             Date = "",
                                             Desc = item.Element(xmlns + "div").Element(xmlns + "div").Value
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

                        #region Get Hour and Date

                        var resDate = from item in xdoc.Descendants(xmlns + "span")
                                      where
                                          item.Attribute("class") != null &&
                                          item.Attribute("class").Value == "fr fon7 mr2"
                                      select new
                                                 {
                                                     Date = item.Value,
                                                 };

                        string newDate = resDate.ElementAt(0).Date;
                        newDate = newDate.Substring(newDate.IndexOf(',') + 1).Trim();

                        string[] arr = newDate.Split(' ');

                        info.Hour = arr[1].ToString();
                        info.Date = arr[0].ToString();

                        #endregion

                        #region Get body

                        var resBody = from item in xdoc.Descendants(xmlns + "div")
                                      where
                                          item.Attribute("class") != null &&
                                          item.Attribute("class").Value == "fon34 mt3 mr2 fon43"
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
