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
        private static List<RSSfeedModel.Feed> interfaxArticles;
        private static List<RSSfeedModel.Feed> habrArticles;

        /// <summary>
        /// Метод обработки xml-документа: http://www.realcoding.net/article/view/4649
        /// </summary>
        private static List<RSSfeedModel.Feed> ReadRssFeeds(string fileSource, RSSfeedModel.RssSource channel)
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
                            if (!string.IsNullOrWhiteSpace(feed.Title))
                            {
                                feed.RssSource = channel;
                                rssItems.Add(feed);
                            }
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
        /// <summary>
        /// ConsoleAction - Read. Читает RSS-ленты из заданных источников
        /// </summary>
        private static int Read()
        {
            try { interfaxArticles = ReadRssFeeds(@"http://www.interfax.by/news/feed​", new RSSfeedModel.RssSource()); }
            catch (Exception ex) { throw new Exception("\r\nОшибка чтения interfax.by\r\n", ex); }

            try { habrArticles = ReadRssFeeds(@"https://habr.com/rss/interesting/", new RSSfeedModel.RssSource()); }
            catch (Exception ex) { throw new Exception("\r\nОшибка чтения habr.com\r\n", ex); }

            return interfaxArticles.Count + habrArticles.Count;
        }
        /// <summary>
        /// ConsoleAction - Info. Сохраняет свежие новости в БД
        /// </summary>
        /// <returns>Количество добавленных новостей</returns>
        private static int Update()
        {
            int feedsAdded = 0;
            try
            {
                using (RSSfeedModel.RssDBContext db = new RSSfeedModel.RssDBContext())
                {
                    var newFeedsFromInterfax = db.Feeds.Except(interfaxArticles);
                    db.Feeds.AddRange(newFeedsFromInterfax);
                    feedsAdded = newFeedsFromInterfax.Count();
                    db.SaveChanges();

                }

            }
            catch (Exception ex)
            {
                throw new Exception("Не удалось добавить новости с interfax.by\r\n", ex);
            }
            try
            {
                using (RSSfeedModel.RssDBContext db = new RSSfeedModel.RssDBContext())
                {
                    var newFeedsFromHabr = db.Feeds.Except(interfaxArticles);
                    db.Feeds.AddRange(interfaxArticles);
                    feedsAdded += newFeedsFromHabr.Count();
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {

                throw new Exception("Не удалось добавить новости с interfax.by\r\n", ex);
            }

            return feedsAdded;
        }

        private static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\t\t####### HELLO #######\r\n\n");
            Console.ForegroundColor = ConsoleColor.Gray;

            int newsRead = 0;
            int newsAdded = 0;

            while (true)
            {
                string action = Console.ReadLine().ToLower();
                switch (action)
                {
                    case "read":
                    try
                    {
                        newsRead = Read();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Прочитано новостей: " + newsRead);
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(ex.Message);
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    break;

                    case "update":
                    try
                    {
                        newsAdded = Update();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Новостей добавлено: {0}", newsAdded);
                        Console.ForegroundColor = ConsoleColor.Gray;

                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(ex.Message);
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    break;

                    case "info":
                    Console.WriteLine("Всего прочитано: {0}\r\nДобавлено: {1}", newsRead, newsAdded);
                    break;

                    // Вывод существующих команд.
                    case "help":
                    Console.WriteLine("\r\nList of commands:\r\n");
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    Console.WriteLine("read");
                    Console.WriteLine("update");
                    Console.WriteLine("info");
                    Console.WriteLine("exit");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;

                    default:
                    Console.ForegroundColor = ConsoleColor.DarkRed; Console.WriteLine("#Undefined action\r\n");
                    Console.ForegroundColor = ConsoleColor.Gray; Console.WriteLine("Введите \"");
                    Console.ForegroundColor = ConsoleColor.DarkBlue; Console.WriteLine("exit");
                    Console.ForegroundColor = ConsoleColor.Gray; Console.WriteLine("\"- если хотите завершить работу\n");
                    break;
                }
            }

        }
    }
}
