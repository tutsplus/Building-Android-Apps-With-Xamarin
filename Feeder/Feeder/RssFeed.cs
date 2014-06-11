using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Feeder
{
    public class RssFeed
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public DateTime DateAdded { get; set; }
        public List<RssItem> Items { get; set; }

        public RssFeed( ) {
            Items = new List<RssItem>( );
        }
    }
}