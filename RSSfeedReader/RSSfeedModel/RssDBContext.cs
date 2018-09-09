using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace RSSfeedModel
{
    public class RssDBContext : DbContext
    {
        public RssDBContext() : base("RSSfeedModelConnection")
        { }

        public DbSet<Feed> Feeds { get; set; }
        public DbSet<RssSource> RssSources { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Связь один к одному между Новостью и источником новости,
            // т.к. можно трактовать поразному:
            // 1) У одного "названия новости" может быть несколько источников,
            // 2) У одного "URL новости" есть только один источник;
            modelBuilder.Entity<Feed>().
                HasRequired(PK => PK.RssSource).
                WithRequiredPrincipal(FK => FK.Feed);

            base.OnModelCreating(modelBuilder);
        }
    }

}
