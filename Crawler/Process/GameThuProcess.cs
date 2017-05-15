using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Xml.Linq;
using Crawler.Lib;

namespace Crawler.Process
{
    public class GameThuProcess
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
                              where item.Attribute("class") != null && item.Attribute("class").Value == "clearfix fw fl"
                              select new
                                         {
                                             Link = item.Elements(xmlns + "p").ElementAt(0).Element(xmlns + "a").Attribute("href").Value,
                                             Image = "",
                                             Title = item.Elements(xmlns + "p").ElementAt(0).Element(xmlns + "a").Value,
                                             Hour = item.Elements(xmlns + "p").ElementAt(1).Value,
                                             Date = item.Elements(xmlns + "p").ElementAt(1).Value,
                                             Desc = item.Elements(xmlns + "div").ElementAt(0).Value
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
                                           Hour = node.Hour.Split(',')[2].ToString().Trim(),
                                           Date = node.Hour.Split(',')[1].ToString().Trim()
                                       };

                        cl = new CrawlerClass(info.Link);

                        xdoc = cl.GetXDocument();

                        #region Get Title

                        //var resTitle = from item in xdoc.Descendants(xmlns + "h1")
                        //               where item.Attribute("class") != null && item.Attribute("class").Value == "lblCateTitle"
                        //               select new
                        //               {
                        //                   Title = item.Value,
                        //               };

                        //info.Title = resTitle.ElementAt(0).Title;

                        #endregion

                        #region Get Body

                        var resBody = from item in xdoc.Descendants(xmlns + "div")
                                      where item.Attribute("class") != null && item.Attribute("class").Value == "w90 pdr15 news_content"
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
                throw ex;
                _logger.Debug("----------------Error-----------------");
                _logger.Debug("Message: " + ex.Message);
                _logger.Debug("StackTrace : " + ex.StackTrace);
                _logger.Debug("Category: " + record.CategoryID);
                _logger.Debug("Link : " + record.Url);
            }
        }
    }
}
