using System;
using System.Xml.Linq;

namespace Crawler.Lib
{
    public class CrawlerClass
    {
        public string Url
        {
            get;
            set;
        }

        public CrawlerClass() { }

        public CrawlerClass(string url)
        {
            this.Url = url;
        }

        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(Program));

        public XDocument GetXDocument()
        {
            try
            {
                HtmlAgilityPack.HtmlWeb doc1 = new HtmlAgilityPack.HtmlWeb();
                doc1.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)";

                HtmlAgilityPack.HtmlDocument doc2 = doc1.Load(Url);
                doc2.OptionOutputAsXml = true;
                doc2.OptionAutoCloseOnEnd = true;
                doc2.OptionDefaultStreamEncoding = System.Text.Encoding.UTF8;
               
                XDocument xdoc = XDocument.Parse(Optimise(doc2.DocumentNode.SelectSingleNode("html").OuterHtml));

                return xdoc;
            }
            catch (Exception ex)
            {
                _logger.Debug("Error: " + ex.Message);
                return null;
            }

        }

        private string Optimise(string input)
        {
            return input.Replace("1=\"\"", "").Replace("2=\"\"", "").Replace("3=\"\"", "").Replace("4=\"\"", "").Replace("5=\"\"", "").Replace("6=\"\"", "").Replace("7=\"\"", "").Replace("8=\"\"", "").Replace("9=\"\"", "").Replace("10=\"\"", "").Replace("11=\"\"", "").Replace("12=\"\"", "").Replace("\r\n", "");
        }
    }
}
