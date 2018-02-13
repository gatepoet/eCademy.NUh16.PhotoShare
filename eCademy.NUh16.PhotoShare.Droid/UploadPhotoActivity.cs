using System;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Android.Provider;
using Android;
using Android.Content.PM;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;

namespace eCademy.NUh16.PhotoShare.Droid
{
    public class GenericFileProvider : FileProvider { }

    [Activity(Label = "UploadPhotoActivity", Theme = "@style/MyTheme")]
    public class UploadPhotoActivity : Activity
    {
        private FloatingActionButton uploadButton;
        private ImageView imageView;
        private static Java.IO.File dir;
        private static Java.IO.File file;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.UploadPhoto);
            uploadButton = FindViewById<FloatingActionButton>(Resource.Id.uploadPhoto_uploadFab);
            uploadButton.Enabled = false;
            imageView = FindViewById<ImageView>(Resource.Id.uploadPhoto_image);
            imageView.Click += TakeAPhoto;
            uploadButton.Click += UploadPhoto;

            if (IsThereAnAppToTakePictures())
            {
                EnsurePermissions();
                EnsureDirectoryExists();
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (resultCode != Result.Ok)
                return;

            switch (requestCode)
            {
                case RequestCodes.TakePhotoRequest:
                    MakeAvailableInGallery(file);
                    ShowImage(file.AbsolutePath);
                    break;
                default:
                    break;
            }
        }

        private void EnsurePermissions()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                var permission = Manifest.Permission.WriteExternalStorage;
                if (CheckSelfPermission(permission) == Permission.Denied)
                {
                    RequestPermissions(new[] { permission }, RequestCodes.UploadPhotoPermissionRequest);
                };
            }
        }

        private void ShowImage(string path)
        {
            var bitmap = BitmapLoader.LoadImage(
                path,
                Resources.DisplayMetrics.HeightPixels,
                imageView.Height);

            if (bitmap != null)
            {
                imageView.SetImageBitmap(bitmap);
                uploadButton.Enabled = true;
                bitmap.Dispose();
            }
        }

        private void MakeAvailableInGallery(Java.IO.File file)
        {
            var mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
            var uri = FileProvider.GetUriForFile(this, PackageName + ".provider", file);
            mediaScanIntent.SetData(uri);

            SendBroadcast(mediaScanIntent);
        }


        private async void UploadPhoto(object sender, EventArgs e)
        {
            var progress = new ProgressDialog(this);
            progress.SetMessage(Resources.GetString(Resource.String.uploadPhoto_progressDialog_text));
            progress.SetProgressStyle(ProgressDialogStyle.Horizontal);
            progress.Show();

            var title = FindViewById<EditText>(Resource.Id.uploadPhoto_comment).Text;
            var filename = file.Name;

            var s = new MemoryStream();
            var stream = new BufferedStream(s);
            BitmapLoader.LoadImage(file.AbsolutePath).Compress(
                    Android.Graphics.Bitmap.CompressFormat.Jpeg,
                    100,
                    stream);

            var id = await new PhotoService().UploadPhoto(
                title,
                filename,
                s.ToArray(),
                args => RunOnUiThread(() => progress.Progress = args.ProgressPercentage));

            if (id > 0)
            {
                Finish();
            }
        }

        private void TakeAPhoto(object sender, EventArgs e)
        {
            var photoIntent = new Intent(MediaStore.ActionImageCapture);
            file = new Java.IO.File(dir, $"PhotoShare_{Guid.NewGuid()}.jpg");
            var uri = FileProvider.GetUriForFile(this, PackageName + ".provider", file);
            photoIntent.PutExtra(MediaStore.ExtraOutput, uri);
            //photoIntent.AddFlags(ActivityFlags.GrantReadUriPermission);


            StartActivityForResult(photoIntent, RequestCodes.TakePhotoRequest);
        }

        private bool IsThereAnAppToTakePictures()
        {
            var takePictureIntent = new Intent(MediaStore.ActionImageCapture);
            var availableActivities = PackageManager.QueryIntentActivities(
                takePictureIntent,
                PackageInfoFlags.MatchDefaultOnly);

            return availableActivities?.Count > 0;
        }

        private void EnsureDirectoryExists()
        {
            dir = new Java.IO.File(
                Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures),
                "PhotoShare");
            if (!dir.Exists())
            {
                dir.Mkdirs();
            }
        }
    }
}