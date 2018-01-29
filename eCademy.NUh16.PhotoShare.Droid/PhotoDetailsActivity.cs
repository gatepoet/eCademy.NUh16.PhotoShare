using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace eCademy.NUh16.PhotoShare.Droid
{
    [Activity(Label = "PhotoDetailsActivity")]
    public class PhotoDetailsActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.PhotoDetails);

            var url = Intent.GetStringExtra("photo_url");

            Task.Run(async () => await LoadImage(url));

        }

        private async Task LoadImage(string url)
        {
            var service = new PhotoService();
            var image = await service.GetImage(url);
            var imageView = FindViewById<ImageView>(Resource.Id.image);
            RunOnUiThread(() => imageView.SetImageBitmap(image));
        }
    }
}