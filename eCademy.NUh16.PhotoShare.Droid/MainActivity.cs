using Android.App;
using Android.Widget;
using Android.OS;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Android.Util;
using System.Threading.Tasks;
using Android.Content;
using Android.Views;

namespace eCademy.NUh16.PhotoShare.Droid
{
    [Activity(Label = "eCademy.NUh16.PhotoShare.Droid", MainLauncher = true, Icon = "@drawable/logo", Theme = "@android:style/Theme.Material.NoActionBar")]
    public class MainActivity : Activity
    {
        ICallbackManager callbackManager;
        FacebookTokenTracker tokenTracker;
        PhotoService photoService;
        private LoginResult loginResult;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            callbackManager = CallbackManagerFactory.Create();

            var loginCallback = new FacebookCallback<LoginResult>
            {
                HandleSuccess = SignInWithFacebookToken,
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
                HandleLoggedOut = UpdateButtons
            };

            tokenTracker.StartTracking();

            if (photoService.GetLoginStatus() == PhotoService.LoginStatus.NeedsWebApiToken)
            {
                await photoService.SignInWithFacebookToken(AccessToken.CurrentAccessToken.Token);
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
            //var uploadPhotoButton = FindViewById<Button>(Resource.Id.main_uploadPhoto_button);
            switch (photoService.GetLoginStatus())
            {
                case PhotoService.LoginStatus.LoggedOut:
                case PhotoService.LoginStatus.NeedsWebApiToken:
                //    uploadPhotoButton.Visibility = ViewStates.Gone;
                    break;
                case PhotoService.LoginStatus.LoggedIn:
                //    uploadPhotoButton.Visibility = ViewStates.Visible;
                    break;
                default:
                    break;
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            switch (requestCode)
            {
                case RequestCodes.FacebookLoginRequest:
                    callbackManager.OnActivityResult(requestCode, (int)resultCode, data);
                    break;
                default:
                    break;
            }
        }

        private void SignInWithFacebookToken(LoginResult loginResult)
        {
            this.loginResult = loginResult;
            var token = loginResult.AccessToken.Token;
            Log.Debug(Application.PackageName, token);
            Task.Run(async () =>
            {
                await photoService.SignInWithFacebookToken(token);
                RunOnUiThread(() => UpdateButtons());
            });
        }
    }
}

