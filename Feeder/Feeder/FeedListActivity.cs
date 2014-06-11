using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Linq;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Android.OS;
using Java.Sql;
using Newtonsoft.Json;
using Environment = System.Environment;
using System.IO;
using System.Net.Http;

namespace Feeder
{
    [Activity(Label = "Feeder", MainLauncher = true, Icon = "@drawable/icon")]
    public class FeedListActivity : Activity {
        private List<RssFeed> _feeds;
        private ListView _feedListView;
        private Button _addFeedButton;
        private const string FEED_FILE_NAME = "FeedData.bin";
        private string _filePath;

        public FeedListActivity( ) {
            _feeds = new List<RssFeed>( );
            var path = System.Environment.GetFolderPath( System.Environment.SpecialFolder.Personal );
            _filePath = System.IO.Path.Combine( path, FEED_FILE_NAME );
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.FeedList);
            _feedListView = FindViewById<ListView>( Resource.Id.feedList );
            _addFeedButton = FindViewById<Button>( Resource.Id.addFeedButton );

            _feedListView.ItemClick += FeedListViewOnItemClick;
            _addFeedButton.Click += AddFeedButtonOnClick;

            if ( File.Exists( _filePath ) ) {
                using ( var fs = new FileStream( _filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite ) ) {
                    var formatter = new BinaryFormatter( );

                    try {
                        _feeds = (List<RssFeed>) formatter.Deserialize( fs );
                    } catch ( Exception ex ) {
                        Log.Error( "Feeder", "Encountered an error attempting to deserialize feed data: {0}", ex.Message );
                    }

                    if ( _feeds.Count > 0 )
                        UpdateList( );
                }
            }
        }

        private void UpdateList( ) {
            _feedListView.Adapter = new FeedListAdapter( _feeds.ToArray( ), this );
        }

        private void AddFeedButtonOnClick( object sender, EventArgs eventArgs ) {
            var intent = new Intent( this, typeof ( AddFeedActivity ) );

            StartActivityForResult( intent, 0 );
        }

        protected override void OnActivityResult( int requestCode, Result resultCode, Intent data ) {
            base.OnActivityResult( requestCode, resultCode, data );

            if ( resultCode == Result.Ok ) {
                var url = data.GetStringExtra( "url" );
                AddFeedUrl( url );
            }
        }

        private async void AddFeedUrl( string url ) {
            var newFeed = new RssFeed {
                DateAdded = DateTime.Now,
                Url = url
            };

            using ( var client = new HttpClient( ) ) {
                var xmlFeed = await client.GetStringAsync( url );
                var doc = XDocument.Parse( xmlFeed );

                var channel = doc.Descendants( "channel" ).FirstOrDefault( ).Element( "title" ).Value;
                newFeed.Name = channel;

                XNamespace dc = "http://purl.org/dc/elements/1.1/";
                newFeed.Items = ( from item in doc.Descendants( "item" )
                    select new RssItem {
                        Title = item.Element( "title" ).Value,
                        PubDate = item.Element( "pubDate" ).Value,
                        Creator = item.Element( dc + "creator" ).Value,
                        Link = item.Element( "link" ).Value
                    } ).ToList( );

                _feeds.Add(newFeed);
                UpdateList( );
            }
        }

        private void FeedListViewOnItemClick( object sender, AdapterView.ItemClickEventArgs itemClickEventArgs ) {
            var intent = new Intent( this, typeof ( FeedListItemActivity ) );
            var selectedFeed = _feeds[itemClickEventArgs.Position];
            var feed = JsonConvert.SerializeObject( selectedFeed );
            intent.PutExtra( "feed", feed );

            StartActivity( intent );
        }
    }
}

