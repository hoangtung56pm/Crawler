using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Xml.Linq;
using Crawler.Lib;

namespace Crawler.Process
{
    public class Kenh14Process
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
                              where item.Attribute("class") != null 
                              && item.Attribute("class").Value == "item clearfix"
                              && item.Elements(xmlns + "div").ElementAt(0).Attribute("class").Value == "meta"
                              select new
                                         {
                                             Link = item.Elements(xmlns + "div").ElementAt(1).Element(xmlns + "a").Attribute("href").Value,
                                             Image = item.Elements(xmlns + "div").ElementAt(1).Element(xmlns + "a").Element(xmlns + "img").Attribute("src").Value,
                                             Title = item.Elements(xmlns + "div").ElementAt(2).Elements(xmlns + "div").ElementAt(0).Element(xmlns + "a").Value,
                                             Hour = item.Elements(xmlns + "div").ElementAt(0).Value.Substring(11).Trim(),
                                             Date = item.Elements(xmlns + "div").ElementAt(0).Value.Substring(0,10).Trim(),
                                             Desc = item.Elements(xmlns + "div").ElementAt(2).Elements(xmlns + "div").ElementAt(0).Value,
                                             _LI = item.Elements(xmlns + "ul")
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

                        #region Get Body

                        var resBody = from item in xdoc.Descendants(xmlns + "div")
                                      where item.Attribute("class") != null && item.Attribute("class").Value == "content"
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

                        #region Get relate News

                        var resLI = from item in node._LI.Descendants(xmlns + "li")
                                    where item.Element(xmlns + "a") != null
                                    select new
                                    {
                                        Link = item.Element(xmlns + "a").Attribute("href").Value,
                                        Image = "",
                                        Title = item.Element(xmlns + "a").Value,
                                        Hour = "00:00",
                                        Date = "",
                                        Desc = ""
                                    };
                        foreach (var nodeLI in resLI)
                        {
                            var infoLI = new ContentInfo
                                             {
                                                 Title = nodeLI.Title,
                                                 Teaser = nodeLI.Desc,
                                                 Image = nodeLI.Image,
                                                 Link = record.HttpPrefix + nodeLI.Link,
                                                 CategoryID = record.CategoryID,
                                                 CrawlerUrl = record.Url,
                                                 Hour = nodeLI.Hour,
                                                 Date = nodeLI.Date
                                             };

                            cl = new CrawlerClass(infoLI.Link);

                            xdoc = cl.GetXDocument();

                            #region Get Hour and Date

                            var resDate = from item in xdoc.Descendants(xmlns + "div")
                                          where
                                              item.Attribute("class") != null &&
                                              item.Attribute("class").Value == "meta"
                                          select new
                                          {
                                              Hour = item.Elements(xmlns + "span").ElementAt(0).Value,
                                              Date = item.Elements(xmlns + "span").ElementAt(1).Value
                                          };
                           
                            infoLI.Hour = resDate.ElementAt(0).Hour;
                            infoLI.Date = resDate.ElementAt(0).Date;

                            #endregion

                            #region Get full teaser

                            var resTeaser = from item in xdoc.Descendants(xmlns + "p")
                                            where
                                                item.Attribute("class") != null &&
                                                item.Attribute("class").Value == "sapo"
                                            select new
                                            {
                                                Teaser = item.Value,
                                            };

                            info.Teaser = resTeaser.ElementAt(0).Teaser;

                            #endregion

                            #region Get Body

                            var resBodyLI = from item in xdoc.Descendants(xmlns + "div")
                                            where
                                                item.Attribute("class") != null &&
                                                item.Attribute("class").Value == "content"
                                            select new
                                                       {
                                                           Description = item.Value
                                                       };

                            string bodyLI = resBodyLI.ElementAt(0).Description;

                            if (bodyLI.IndexOf("//") > 0) bodyLI = bodyLI.Substring(0, bodyLI.IndexOf("//"));
                            infoLI.Body = bodyLI;

                            #endregion

                            AppEnv.Insert(infoLI);

                            _logger.Debug("---------------------------------");
                            _logger.Debug("Title: " + info.Title);
                            _logger.Debug("Desc : " + info.Teaser);
                            _logger.Debug("Image: " + info.Image);
                            _logger.Debug("Link : " + info.Link);
                            _logger.Debug("Url  : " + info.CrawlerUrl);
                        }

                        #endregion
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
