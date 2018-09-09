using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace RssFeedConsoleApp
{
    internal class Program
    {
        /// <summary>
        /// Метод обработки xml-документа: http://www.realcoding.net/article/view/4649
        /// </summary>
        private static List<RSSfeedModel.Feed> GetNewArticles(string fileSource, RSSfeedModel.RssSource channel)
        {
            // Сюда будут добавляться новости по мере их поступления.
            List<RSSfeedModel.Feed> rssItems = new List<RSSfeedModel.Feed>();

            try
            {
                XmlDocument doc = new XmlDocument();
                // Получаем Rss ленту по ссылке
                try { doc.Load(fileSource); }
                catch (XmlException xEx) { throw new Exception("Ошибка целостности Xml - документа\r\n", xEx); }
                catch (System.IO.DirectoryNotFoundException pEx) { throw new Exception("DirectoryNotFoundException\r\n", pEx); }
                catch (System.IO.FileNotFoundException fEx) { throw new Exception("FileNotFoundException\r\n", fEx); }

                XmlNodeList nodeList;
                XmlNode root = doc.DocumentElement;

                // Получаем все узлы xml-документа (новости с ленты)
                try { nodeList = root.ChildNodes; }
                catch (NullReferenceException nEx) { throw new Exception("В документе отсутствуют дочерние узлы\r\n", nEx); }

                // Из полученного xml-документа, проходимся по основым узлам.
                foreach (XmlNode node in nodeList)
                {
                    foreach (XmlNode chanel_item in node)
                    {
                        // Заполнение "RSS-источника":
                        // Название
                        if (chanel_item.Name == "title")
                        { channel.TitleRss = chanel_item.InnerText; }

                        // URL
                        if (chanel_item.Name == "link")
                        { channel.Url = chanel_item.InnerText; }

                        // Сама новость.
                        if (chanel_item.Name == "item")
                        {
                            // содержимое <item>...</item>
                            XmlNodeList itemsList = chanel_item.ChildNodes;

                            // Модель, которая будет добавлена в очередь rssItems.
                            RSSfeedModel.Feed feed = new RSSfeedModel.Feed();

                            foreach (XmlNode item in itemsList)
                            {
                                if (item.Name == "title")
                                { feed.Title = item.InnerText; } else
                                if (item.Name == "link")
                                { feed.Link = item.InnerText; } else
                                if (item.Name == "description")
                                { feed.Description = item.InnerText; } else
                                if (item.Name == "pubDate")
                                {
                                    try
                                    {
                                        // Если это дата, то произойдёт приведение к типу DateTime
                                        feed.PubDate = DateTime.
                                            ParseExact(item.InnerText, "ddd, dd MMM yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture);
                                    }
                                    catch (FormatException fEx) { throw new Exception("Не удалось преобразовать дату в: \"" + feed.Title + "\"\r\n", fEx); }
                                }
                            }
                            // Т.к. заголовок является уникальным идентификатором => он не может быть пустым!
                            if (!string.IsNullOrWhiteSpace(feed.Title)) rssItems.Add(feed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка в получении RSS-ленты\r\n", ex);
            }
            return rssItems;
        }

        private static void Main(string[] args)
        {
            string parseFormat = "ddd, dd MMM yyyy HH:mm:ss zzz";
            DateTime date = DateTime.ParseExact("", parseFormat, CultureInfo.InvariantCulture);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\t\t####### HELLO #######");
            Console.ForegroundColor = ConsoleColor.Gray;


            RSSfeedModel.RssSource interfaxChannel = new RSSfeedModel.RssSource();
            List<RSSfeedModel.Feed> interfaxArticles = GetNewArticles(@"http://www.interfax.by/news/feed​", interfaxChannel);

            RSSfeedModel.RssSource habrChannel = new RSSfeedModel.RssSource();
            List<RSSfeedModel.Feed> habrArticles = GetNewArticles(@"https://habr.com/rss/interesting/", habrChannel);

        }
    }
}
