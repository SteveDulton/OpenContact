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

        public string FeedHeader { get; set; }
        public DateTime PublicDate { get; set; }
        public string FeedContent { get; set; }
        public string Url { get; set; }

        public RssSource RssSource { get; set; }
    }
}
