using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Crawler.Lib;

namespace Crawler.Process
{
    public class NLDProcess
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
                                  (item.Attribute("class").Value == "clearfix")
                                  && (item.Element(xmlns + "h4") != null || item.Element(xmlns + "h3") != null)
                              select new
                                         {
                                             Link =
                                  item.Element(xmlns + "h4") != null
                                      ? item.Element(xmlns + "h4").Element(xmlns + "a").Attribute("href").Value
                                      : item.Element(xmlns + "h3").Element(xmlns + "a").Attribute("href").Value,
                                             Image =
                                  item.Element(xmlns + "a").Element(xmlns + "img") != null
                                      ? item.Element(xmlns + "a").Element(xmlns + "img").Attribute("src").Value
                                      : "",
                                             Title =
                                  item.Element(xmlns + "h4") != null
                                      ? item.Element(xmlns + "h4").Element(xmlns + "a").Value
                                      : item.Element(xmlns + "h3").Element(xmlns + "a").Value,
                                             Hour = item.Elements(xmlns + "p").ElementAt(0).Value,
                                             Date = item.Elements(xmlns + "p").ElementAt(0).Value,
                                             Desc = item.Elements(xmlns + "p").ElementAt(1).Value
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
                                           Hour = node.Hour.Split(' ')[0],
                                           Date = node.Date.Split(' ')[1]
                                       };

                        cl = new CrawlerClass(info.Link);

                        xdoc = cl.GetXDocument();

                        #region Get Title

                        //var resTitle = from item in xdoc.Descendants(xmlns + "div")
                        //               where
                        //                   item.Attribute("class") != null &&
                        //                   item.Attribute("class").Value == "box10"
                        //               select new
                        //               {
                        //                   Title = item.Element(xmlns + "h1").Value,
                        //               };

                        //info.Title = resTitle.ElementAt(0).Title;

                        #endregion

                        #region Get Hour and Date

                        var resDate = from item in xdoc.Descendants(xmlns + "p")
                                      where
                                          item.Attribute("class") != null &&
                                          item.Attribute("class").Value == "time"
                                      select new
                                      {
                                          Date = item.Value,
                                      };
                        //19/03/2011 - 12:56 AM
                        string newDate = resDate.ElementAt(0).Date.Trim();
                        newDate = newDate.Substring(newDate.IndexOf(',') + 1).Trim();

                        string[] arr = newDate.Split(' ');

                        info.Hour = arr[1].ToString().Trim();
                        info.Date = arr[0].ToString().Trim();

                        #endregion

                        #region Get Body

                        var resBody = from item in xdoc.Descendants(xmlns + "div")
                                   where item.Attribute("class") != null && item.Attribute("class").Value == "content"
                                   select new
                                              {
                                                  Description = item.Value
                                              };

                        string body = resBody.ElementAt(1).Description;

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
