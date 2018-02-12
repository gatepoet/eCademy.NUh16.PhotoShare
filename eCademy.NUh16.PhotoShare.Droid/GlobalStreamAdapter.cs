using Android.App;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eCademy.NUh16.PhotoShare.Droid
{
    internal class GlobalStreamAdapter : BaseAdapter<Photo>
    {
        private PhotoService service;
        private IList<Photo> photos;
        private int size;
        private Activity activity;

        public GlobalStreamAdapter(Activity activity, PhotoService service)
        {
            this.service = service;
            this.photos = new List<Photo>();
            this.service = service;
            this.activity = activity;

            var displayMetrics = new DisplayMetrics();
            activity.WindowManager.DefaultDisplay.GetMetrics(displayMetrics);
            size = displayMetrics.WidthPixels / 3;
        }

        public override Photo this[int position] => photos[position];

        public override int Count => photos.Count;

        public override long GetItemId(int position) => this[position].GetHashCode();

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var photo = this[position];

            ImageView imageView;

            if (convertView == null)
            {
                imageView = new ImageView(activity);
                imageView.LayoutParameters = new GridView.LayoutParams(size, size);
                imageView.SetScaleType(ImageView.ScaleType.CenterCrop);
                imageView.Click += (sender, args) => OpenPhotoDetails(photo);
            }
            else {
                imageView = (ImageView)convertView;
            }

            Task.Run(async () => await LoadImage(imageView, photo.ImageUrl));

            return imageView;
        }

        private void OpenPhotoDetails(Photo photo)
        {
            var intent = new Intent(activity, typeof(PhotoDetailsActivity));
            intent.PutExtra("photo_url", photo.ImageUrl);

            activity.StartActivity(intent);
        }

        public async Task LoadPhotos()
        {
            photos = await service.GetGlobalStreamPhotos();
            activity.RunOnUiThread(() => NotifyDataSetInvalidated());
        }

        public async Task LoadImage(ImageView imageView, string imageUrl)
        {
            var image = await service.GetImage(imageUrl, size);
            activity.RunOnUiThread(() => imageView.SetImageBitmap(image));
        }
    }
}