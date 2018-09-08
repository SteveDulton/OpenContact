using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSSfeedModel
{
    public class RssSource
    {
        public int Id { get; set; }

        public string Url { get; set; }
        public string NameRss { get; set; }

        public Feed Feed { get; set; }
    }
}
