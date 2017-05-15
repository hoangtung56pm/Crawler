using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Xml.Linq;
using Crawler.Lib;

namespace Crawler.Process
{
    public class GameThuVnexpressProcess
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
                    var res = from item in xdoc.Descendants(xmlns + "ul").Elements(xmlns + "li")
                              where item.Elements(xmlns + "div").Count() > 1
                              select new
                                         {
                                             Link = item.Elements(xmlns + "div").ElementAt(0).Element(xmlns + "a").Attribute("href").Value,
                                             Image = item.Elements(xmlns + "div").ElementAt(0).Element(xmlns + "a").Element(xmlns + "img").Attribute("src").Value,
                                             Title = item.Elements(xmlns + "div").ElementAt(1).Elements(xmlns + "p").ElementAt(0).Element(xmlns + "a").Value,
                                             Hour = DateTime.Now.Hour + ":" + DateTime.Now.Minute,
                                             Date = DateTime.Now.ToString("dd/MM/yyyy"),
                                             Desc = item.Elements(xmlns + "div").ElementAt(1).Elements(xmlns + "p").ElementAt(1).Value
                                         };
                    foreach (var node in res)
                    {
                        var info = new ContentInfo
                                       {
                                           Title = node.Title,
                                           Teaser = node.Desc,
                                           Image = record.HttpPrefix + node.Image,
                                           Link = record.HttpPrefix + node.Link,
                                           CategoryID = record.CategoryID,
                                           CrawlerUrl = record.Url,
                                           Hour = node.Hour,
                                           Date = node.Date
                                       };

                        cl = new CrawlerClass(info.Link);

                        xdoc = cl.GetXDocument();

                        #region Get Body

                        string body = "";
                        if (xdoc != null)
                        {
                            var resBody = from item in xdoc.Descendants(xmlns + "div")
                                          where
                                              item.Attribute("cpms_content") != null &&
                                              item.Attribute("cpms_content").Value == "true"
                                          select new
                                                     {
                                                         Description = item.Value
                                                     };

                            body = resBody.ElementAt(0).Description;
                        }
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
