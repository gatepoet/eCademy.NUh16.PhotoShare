using Android.App;
using Android.Widget;
using Android.OS;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Android.Util;
using System.Threading.Tasks;
using Android.Content;
using Android.Views;
using Xamarin.Facebook.Login.Widget;
using Android.Graphics;
using Newtonsoft.Json;

namespace eCademy.NUh16.PhotoShare.Droid
{
    [Activity(MainLauncher = true)]
    public class MainActivity : Activity
    {
        ICallbackManager callbackManager;
        FacebookTokenTracker tokenTracker;
        PhotoService photoService;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var font = Typeface.CreateFromAsset(Assets, "Fonts/IndieFlower.ttf");
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            FindViewById<TextView>(Resource.Id.logo_text_part1).Typeface = font;
            FindViewById<TextView>(Resource.Id.logo_text_part2).Typeface = font;

            FindViewById<Button>(Resource.Id.main_viewGlobalStream_button).Click += (sender, args) =>
                StartActivity(typeof(GlobalStreamActivity));
            FindViewById<Button>(Resource.Id.main_uploadPhoto_button).Click += (sender, args) =>
                StartActivity(typeof(UploadPhotoActivity));
            FindViewById<LoginButton>(Resource.Id.login_button).SetReadPermissions("email");

            callbackManager = CallbackManagerFactory.Create();

            var loginCallback = new FacebookCallback<LoginResult>
            {
                //HandleSuccess = SignInWithFacebookToken,
                HandleSuccess = result => Log.Debug(
                    Application.PackageName,
                    JsonConvert.SerializeObject(result)),
                HandleCancel = () => Log.Debug(
                    Application.PackageName,
                    "Canceled"),
                HandleError = error => Log.Error(
                    Application.PackageName,
                    Java.Lang.Throwable.FromException(error),
                    "No access")
            };
            LoginManager.Instance.RegisterCallback(callbackManager, loginCallback);

            photoService = new PhotoService();
            tokenTracker = new FacebookTokenTracker(photoService)
            {
                HandleLoggedIn = UpdateButtons,
                HandleLoggedOut = UpdateButtons
            };

            tokenTracker.StartTracking();

            if (photoService.GetLoginStatus() == PhotoService.LoginStatus.NeedsWebApiToken)
            {
                await SignInWithFacebookToken(AccessToken.CurrentAccessToken.Token);
            }

            UpdateButtons();
        }

        protected override void OnDestroy()
        {
            tokenTracker.StopTracking();
            base.OnDestroy();
        }

        private void UpdateButtons()
        {
            var uploadPhotoButton = FindViewById<Button>(Resource.Id.main_uploadPhoto_button);
            switch (photoService.GetLoginStatus())
            {
                case PhotoService.LoginStatus.LoggedOut:
                case PhotoService.LoginStatus.NeedsWebApiToken:
                    uploadPhotoButton.Visibility = ViewStates.Gone;
                    break;
                case PhotoService.LoginStatus.LoggedIn:
                    uploadPhotoButton.Visibility = ViewStates.Visible;
                    break;
                default:
                    break;
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            callbackManager.OnActivityResult(requestCode, (int)resultCode, data);
        }

        private void SignInWithFacebookToken(LoginResult loginResult)
        {
            var token = loginResult.AccessToken.Token;
            Log.Debug(Application.PackageName, token);
            Task.Run(async () =>
            {
                await SignInWithFacebookToken(token);
                RunOnUiThread(() => UpdateButtons());
            });
        }

        private async Task SignInWithFacebookToken(string token)
        {
            await Task.Run(async () =>
            {
                try
                {
                    await photoService.SignInWithFacebookToken(token);
                    RunOnUiThread(() => UpdateButtons());
                }
                catch (System.Exception ex)
                {
                    RunOnUiThread(() => Toast.MakeText(this, ex.Message, ToastLength.Long)
                            .Show());
                }
            });
        }
    }
}

