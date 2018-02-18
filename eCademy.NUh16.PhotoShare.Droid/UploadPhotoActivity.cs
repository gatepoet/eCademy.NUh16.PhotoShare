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
using static Android.Graphics.Bitmap;
using System.Threading.Tasks;
using Android.Util;
using System.Linq;
using Android.Gms.Location;
using Android.Locations;
using eCademy.NUh16.PhotoShare.Droid.Utils;
using Android.Media;

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
        private Button setLocationButton;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.UploadPhoto);
            uploadButton = FindViewById<FloatingActionButton>(Resource.Id.uploadPhoto_uploadFab);
            uploadButton.Enabled = false;
            imageView = FindViewById<ImageView>(Resource.Id.uploadPhoto_image);
            imageView.Click += TakeAPhoto;
            uploadButton.Click += UploadPhoto;

            setLocationButton = FindViewById<Button>(Resource.Id.setLocation);
            setLocationButton.Click += async delegate { await SetLocation(); };

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

                    setLocationButton.Visibility = Android.Views.ViewStates.Visible;
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
            var location = new ExifInterface(path).ReadLocation();
            FindViewById<TextView>(Resource.Id.imageLocation).Text = location.ToFormattedString(this);

            var bitmap = BitmapLoader.LoadImage(
                path,
                Resources.DisplayMetrics.WidthPixels,
                Resources.DisplayMetrics.HeightPixels);

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

            await Task.Run(async () =>
            {
                Compress(file.AbsolutePath);
                var id = await new PhotoService().UploadPhoto(
                    title,
                    filename,
                    File.ReadAllBytes(file.AbsolutePath),
                    args => RunOnUiThread(() => progress.Progress = args.ProgressPercentage));

                RunOnUiThread(() =>
                {
                    progress.Dismiss();
                    if (id > 0)
                    {
                        Finish();
                    }
                });
            });
        }

        private void Compress(string path)
        {
            var stream = new MemoryStream();
            var location = new ExifInterface(path).ReadLocation();
            BitmapLoader
                .LoadImage(file.AbsolutePath)
                .Compress(CompressFormat.Jpeg, 92, stream);
            File.WriteAllBytes(path, stream.ToArray());
            new ExifInterface(path).WriteLocation(location);
        }
        #region Get location

        private async Task SetLocation()
        {
            if (CheckSelfPermission(Manifest.Permission.AccessFineLocation) == Permission.Granted)
            {
                var locationClient = LocationServices.GetFusedLocationProviderClient(this);
                var deviceLocation = await locationClient.GetLastLocationAsync();
                if (deviceLocation != null)
                {
                    var exif = new ExifInterface(file.AbsolutePath);
                    exif.WriteLocation(deviceLocation);
                }
                else
                {
                    ShowError("Could not get location");
                }

                FindViewById<TextView>(Resource.Id.imageLocation).Text = deviceLocation.ToFormattedString(this);
            }
            else
            {
                RequestPermissions(new[] { Manifest.Permission.AccessFineLocation }, RequestCodes.GetLocationRequest);
            }
        }

        public async override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            switch (requestCode)
            {
                case RequestCodes.GetLocationRequest:
                    if (grantResults.All(result => result == Permission.Granted))
                    {
                        await SetLocation();
                    }
                    else
                    {
                        ShowError("Get GPS location permission not granted");
                    }
                    break;
                default:
                    break;
            }
        }

        #endregion

        private void TakeAPhoto(object sender, EventArgs e)
        {
            var photoIntent = new Intent(MediaStore.ActionImageCapture);
            file = new Java.IO.File(dir, $"PhotoShare_{Guid.NewGuid()}.jpg");
            var uri = FileProvider.GetUriForFile(this, PackageName + ".provider", file);
            photoIntent.PutExtra(MediaStore.ExtraOutput, uri);

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

        private void ShowError(string message)
        {
            Log.Error(PackageName, message);
            new AlertDialog.Builder(this)
                .SetMessage(message)
                .SetNeutralButton("Ok", delegate { })
                .Show();
        }
    }
}