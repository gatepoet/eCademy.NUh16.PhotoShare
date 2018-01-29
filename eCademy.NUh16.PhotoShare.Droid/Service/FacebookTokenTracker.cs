using System;
using Xamarin.Facebook;

namespace eCademy.NUh16.PhotoShare.Droid
{
    public class FacebookTokenTracker : AccessTokenTracker
    {
        private readonly PhotoService service;
        public Action HandleLoggedIn { get; set; }
        public Action HandleLoggedOut { get; set; }

        public FacebookTokenTracker(PhotoService service)
        {
            this.service = service;
        }

        protected override async void OnCurrentAccessTokenChanged(AccessToken oldAccessToken, AccessToken currentAccessToken)
        {
            if (currentAccessToken == null)
            {
                service.SignOut();
                HandleLoggedOut?.Invoke();
            }
            else
            {
                await service.SignInWithFacebookToken(currentAccessToken.Token);
                HandleLoggedIn?.Invoke();
            }
        }
    }
}

