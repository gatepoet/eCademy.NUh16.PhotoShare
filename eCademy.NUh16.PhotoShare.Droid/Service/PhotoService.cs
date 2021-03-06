﻿using Android.Graphics;
using Android.Util;
using Java.Util;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Cache;
using System.Threading.Tasks;
using Xamarin.Facebook;

namespace eCademy.NUh16.PhotoShare.Droid
{
    public class PhotoService
    {
        public enum LoginStatus
        {
            LoggedOut,
            NeedsWebApiToken,
            LoggedIn
        }

        private readonly string baseUrl;

        private readonly string GetGlobalStreamPhotosUrl = "/api/images/";
        private readonly string VerifyExternalTokenUrl = "/api/Account/VerifyExternalToken";
        public const string UploadPhotoUrl = "/api/photos/uploadMobile";
        private ExternalTokenResponse photoshareToken;
        private AndroidSecureDataProvider secureDataProvider;
        private const string PhotoShareStoreKey = "PhotoShare";

        public LoginStatus GetLoginStatus()
        {
            var facebookToken = AccessToken.CurrentAccessToken;
            if (facebookToken == null || string.IsNullOrWhiteSpace(facebookToken.Token) || facebookToken.Expires.Before(new Date()))
            {
                return LoginStatus.LoggedOut;
            }

            if (photoshareToken == null || string.IsNullOrWhiteSpace(photoshareToken.AccessToken) || photoshareToken.Expires < System.DateTime.Now)
            {
                return LoginStatus.NeedsWebApiToken;
            }

            return LoginStatus.LoggedIn;
        }

        private string AuthorizationHeader
        {
            get
            {
                return "Bearer " + photoshareToken.AccessToken;
            }
        }


        public PhotoService()
        {
#if DEBUG
            this.baseUrl = "http://192.168.1.7:51995/";
#else
            this.baseUrl = "http://photoshare.one/";
#endif
            secureDataProvider = new AndroidSecureDataProvider();
            photoshareToken = secureDataProvider.Retreive(PhotoShareStoreKey)
                .FromDictionary<ExternalTokenResponse>();
        }

        public async Task<Photo[]> GetGlobalStreamPhotos()
        {
            try
            {
                using (var client = CreateWebClient())
                {
                    Log.Info("PhotoShare", "Getting photos");
                    var json = await client.DownloadStringTaskAsync(GetGlobalStreamPhotosUrl);
                    return JsonConvert.DeserializeObject<Photo[]>(json);
                }
            }
            catch (Exception ex)
            {
                Log.Error("PhotoShare", Java.Lang.Throwable.FromException(ex), "Could not get photos");
                return new Photo[0];
            }
        }


        public void SignOut()
        {
            photoshareToken = null;
            secureDataProvider.Clear(PhotoShareStoreKey);
        }

        private WebClient CreateWebClient()
        {
            return new WebClient()
            {
                BaseAddress = baseUrl
            };
        }

        public async Task<Bitmap> GetImage(string url, int? size = null)
        {
            try
            {
                using (var client = CreateWebClient())
                {
                    client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable);
                    if (size.HasValue)
                    {
                        client.QueryString.Add("thumb", size.Value.ToString());
                    }
                    var imageBytes = await client.DownloadDataTaskAsync(url);
                    if (imageBytes == null || imageBytes.Length == 0)
                    {
                        return null;
                    }

                    var bitmap = await BitmapFactory.DecodeByteArrayAsync(imageBytes, 0, imageBytes.Length);
                    return bitmap;
                }
            }
            catch (WebException ex)
            {
                Log.Error("PhotoShare", Java.Lang.Throwable.FromException(ex), "Could not get image");
                return null;
            }
        }

        private void EnsureLoggedIn()
        {
            var status = GetLoginStatus();
            if (status == LoginStatus.LoggedIn)
                return;
            throw new NotLoggedInException(status);
        }

        public async Task SignInWithFacebookToken(string token)
        {
            try
            {
                using (var client = CreateWebClient())
                {
                    var request = new ExternalTokenRequest
                    {
                        Provider = "Facebook",
                        Token = token
                    };

                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");

                    var result = await client.UploadStringTaskAsync(
                        VerifyExternalTokenUrl,
                        "POST",
                        JsonConvert.SerializeObject(request));

                    var response = JsonConvert.DeserializeObject<ExternalTokenResponse>(result);

                    secureDataProvider.Store(response.UserId, PhotoShareStoreKey, response.ToDictionary());

                    photoshareToken = response;
                }
            }
            catch (Exception ex)
            {
                Log.Error("PhotoShare", Java.Lang.Throwable.FromException(ex), "Error signing in");
            }
        }

        public async Task<int> UploadPhoto(string title, string filename, byte[] file, Action<UploadProgressChangedEventArgs> progressHandler = null)
        {
            EnsureLoggedIn();
            try
            {
                using (var client = CreateWebClient())
                {
                    if (progressHandler != null)
                    {
                        client.UploadProgressChanged += (sender, args) => progressHandler(args);
                    }
                    client.Headers.Add(HttpRequestHeader.Authorization, AuthorizationHeader);
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    var request = new { Title = title, Filename = filename, File = file };
                    var result = await client.UploadStringTaskAsync(UploadPhotoUrl, "POST", JsonConvert.SerializeObject(request));
                    var newId = JsonConvert.DeserializeObject<int>(result);
                    return newId;
                }
            }
            catch (WebException ex)
            {
                Log.Error("PhotoShare", Java.Lang.Throwable.FromException(ex), "Could not upload photo");
                return -1;
            }
        }
    }
}