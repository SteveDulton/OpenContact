using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSSfeedModel
{
    public class Feed
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public DateTime PubDate { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }

        public RssSource RssSource { get; set; }
    }
}
