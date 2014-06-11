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
using Newtonsoft.Json;

namespace Feeder
{
    [Activity(Label = "FeedListItemActivity")]
    public class FeedListItemActivity : Activity {
        private RssFeed _feed;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView( Resource.Layout.FeedItemList );

            _feed = JsonConvert.DeserializeObject<RssFeed>( Intent.GetStringExtra( "feed" ) );

            var feedNameTextView = FindViewById<TextView>( Resource.Id.feedItemName );
            var articleCountTextView = FindViewById<TextView>( Resource.Id.articleCount );
            var feedItemListView = FindViewById<ListView>( Resource.Id.feedItemList );

            feedNameTextView.Text = "Name: " + _feed.Name;
            articleCountTextView.Text = "Articles: " + _feed.Items.Count( );
            feedItemListView.ItemClick += FeedItemListViewOnItemClick;

            feedItemListView.Adapter = new FeedItemListAdapter(this, _feed.Items.ToArray());
        }

        private void FeedItemListViewOnItemClick( object sender, AdapterView.ItemClickEventArgs itemClickEventArgs ) {
            var item = _feed.Items[itemClickEventArgs.Position];

            var uri = Android.Net.Uri.Parse( item.Link );
            var intent = new Intent( Intent.ActionView, uri );
            StartActivity( intent );
        }
    }

    public class FeedItemListAdapter : BaseAdapter<RssItem> {
        private readonly FeedListItemActivity _feedListItemActivity;
        private readonly RssItem[] _rssItems;

        public FeedItemListAdapter( FeedListItemActivity feedListItemActivity, RssItem[] rssItems ) {
            _feedListItemActivity = feedListItemActivity;
            _rssItems = rssItems;
        }

        public override long GetItemId( int position ) {
            return position;
        }

        public override View GetView( int position, View convertView, ViewGroup parent ) {
            var view = convertView;
            if ( view == null ) {
                view = _feedListItemActivity.LayoutInflater.Inflate( Android.Resource.Layout.SimpleListItem2, null );
            }

            view.FindViewById<TextView>( Android.Resource.Id.Text1 ).Text = _rssItems[position].Title;
            view.FindViewById<TextView>( Android.Resource.Id.Text2 ).Text = string.Format( "{0} on {1}", _rssItems[position].Creator, _rssItems[position].PubDate );

            return view;
        }

        public override int Count {
            get { return _rssItems.Count( ); }
        }

        public override RssItem this[ int position ] {
            get { return _rssItems[position]; }
        }
    }
}