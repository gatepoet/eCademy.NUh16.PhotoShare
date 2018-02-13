﻿using eCademy.NUh16.PhotoShare;
using eCademy.NUh16.PhotoShare.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace eCademy.NUh15.PhotoShare.Controllers.API
{
    [Authorize]
    [RoutePrefix("api/Account")]
    public class OAuthController : ApiController
    {
        private const string LocalLoginProvider = "Local";
        private ApplicationUserManager _userManager;
        public OAuthController()
        {
        }

        public OAuthController(ApplicationUserManager userManager, ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }

            private set
            {
                _userManager = value;
            }
        }

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat
        {
            get;
            private set;
        }
        [Route("Test")]
        public IHttpActionResult GetTest()
        {
            return Ok(User.Identity.Name);
        }
        [OverrideAuthentication]
        [AllowAnonymous]
        [Route("RegisterExternalToken")]
        public async Task<IHttpActionResult> GetUserRegistration(ExternalTokenBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //validate token
            ExternalLoginData login = await ExternalLoginData.FromToken(model.Provider, model.Token);
            if (login == null)
            {
                return InternalServerError();
            }

            var user = await UserManager.FindByEmailAsync(login.Email);
            bool hasRegistered = user != null;
            return Ok(hasRegistered);
        }

        // POST api/Account/RegisterExternalToken
        [OverrideAuthentication]
        [AllowAnonymous]
        [Route("RegisterExternalToken")]
        public async Task<IHttpActionResult> PostRegisterExternalToken(RegisterExternalTokenBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //validate token
            ExternalLoginData login = await ExternalLoginData.FromToken(model.Provider, model.Token);
            if (login == null)
            {
                return InternalServerError();
            }

            if (login.LoginProvider != model.Provider)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                return InternalServerError();
            }

            //if we reached this point then token is valid
            ApplicationUser user = await UserManager.FindByEmailAsync(login.Email);
            bool hasRegistered = user != null;
            IdentityResult result;
            if (hasRegistered)
            {
                return GetErrorResult(IdentityResult.Failed("User aldready registered."));
            }

            user = new ApplicationUser()
            { UserName = login.Email, Email = login.Email };
            result = await UserManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            var loginInfo = new UserLoginInfo(login.LoginProvider, login.ProviderKey);
            result = await UserManager.AddLoginAsync(user.Id, loginInfo);
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            //authenticate
            JObject token = await Authenticate(login, user);
            return Ok(token);
        }

        // POST api/Account/RegisterExternalToken
        [OverrideAuthentication]
        [AllowAnonymous]
        [Route("VerifyExternalToken")]
        public async Task<IHttpActionResult> PostVerifyExternalToken([FromBody]ExternalTokenBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //validate token
            ExternalLoginData login = await ExternalLoginData.FromToken(model.Provider, model.Token);
            if (login == null)
            {
                return InternalServerError();
            }

            if (login.LoginProvider != model.Provider)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                return InternalServerError();
            }

            //if we reached this point then token is valid
            var user = await UserManager.FindByEmailAsync(login.Email);
            if (user == null)
            {
                user = new ApplicationUser()
                { UserName = login.Email, Email = login.Email };
                var result = await UserManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    return GetErrorResult(result);
                }
            }

            var logins = await UserManager.GetLoginsAsync(user.Id);
            var userLogin = logins.SingleOrDefault(i => i.LoginProvider == model.Provider);
            if (userLogin == null) {
                var loginInfo = new UserLoginInfo(login.LoginProvider, login.ProviderKey);
                var result = await UserManager.AddLoginAsync(user.Id, loginInfo);
                if (!result.Succeeded)
                {
                    return GetErrorResult(result);
                }
            }

            //authenticate
            JObject token = await Authenticate(login, user);
            return Ok(token);
        }

        private async Task<JObject> Authenticate(ExternalLoginData login, ApplicationUser user)
        {
            var identity = await UserManager.CreateIdentityAsync(user, OAuthDefaults.AuthenticationType);
            IEnumerable<Claim> claims = login.GetClaims();
            identity.AddClaims(claims);
            Authentication.SignIn(identity);
            ClaimsIdentity oAuthIdentity = new ClaimsIdentity(Startup.OAuthOptions.AuthenticationType);
            oAuthIdentity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
            oAuthIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
            AuthenticationTicket ticket = new AuthenticationTicket(oAuthIdentity, new AuthenticationProperties());
            DateTime currentUtc = DateTime.UtcNow;
            ticket.Properties.IssuedUtc = currentUtc;
            ticket.Properties.ExpiresUtc = currentUtc.Add(Startup.OAuthOptions.AccessTokenExpireTimeSpan);
            string accessToken = Startup.OAuthOptions.AccessTokenFormat.Protect(ticket);
            Request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            // Create the response building a JSON object that mimics exactly the one issued by the default /Token endpoint
            JObject token = new JObject(new JProperty("userName", user.UserName), new JProperty("userId", user.Id), new JProperty("access_token", accessToken), new JProperty("token_type", "bearer"), new JProperty("expires_in", Startup.OAuthOptions.AccessTokenExpireTimeSpan.TotalSeconds.ToString()), new JProperty("issued", currentUtc.ToString("ddd, dd MMM yyyy HH':'mm':'ss 'GMT'")), new JProperty("expires", currentUtc.Add(Startup.OAuthOptions.AccessTokenExpireTimeSpan).ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'")));
            return token;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers
        private IAuthenticationManager Authentication
        {
            get
            {
                return HttpContext.Current.GetOwinContext().Authentication;
            }
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private class ExternalLoginData
        {
            public string LoginProvider
            {
                get;
                set;
            }

            public string ProviderKey
            {
                get;
                set;
            }

            public string UserName
            {
                get;
                set;
            }

            public string Email
            {
                get;
                private set;
            }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));
                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer) || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData { LoginProvider = providerKeyClaim.Issuer, ProviderKey = providerKeyClaim.Value, UserName = identity.FindFirstValue(ClaimTypes.Name) };
            }

            public static async Task<ExternalLoginData> FromToken(string provider, string accessToken)
            {
                string verifyTokenEndPoint = "", verifyAppEndpoint = "";
                HttpClient client = new HttpClient();
                if (provider == "Facebook")
                {
                    verifyTokenEndPoint = string.Format("https://graph.facebook.com/me?fields=id,name,email&access_token={0}", accessToken);
                    verifyAppEndpoint = string.Format("https://graph.facebook.com/app?access_token={0}", accessToken);
                }
                else
                {
                    return null;
                }

                Uri uri = new Uri(verifyTokenEndPoint);
                HttpResponseMessage response = await client.GetAsync(uri);
                ClaimsIdentity identity = null;
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    dynamic iObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                    identity = new ClaimsIdentity(OAuthDefaults.AuthenticationType);
                    if (provider == "Facebook")
                    {
                        if (iObj["email"] == null)
                        {
                            return null;
                        }

                        identity.AddClaim(new Claim(ClaimTypes.Email, iObj["email"].ToString(), ClaimValueTypes.String, "Facebook", "Facebook"));
                        uri = new Uri(verifyAppEndpoint);
                        response = await client.GetAsync(uri);
                        content = await response.Content.ReadAsStringAsync();
                        dynamic appObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                        if (appObj["id"] != Startup.FacebookAuthOptions.AppId)
                        {
                            return null;
                        }

                        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, iObj["id"].ToString(), ClaimValueTypes.String, "Facebook", "Facebook"));
                    }
                }

                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer) || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name),
                    Email = identity.FindFirstValue(ClaimTypes.Email)
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();
            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;
                if (strengthInBits % bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits / bitsPerByte;
                byte[] data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return System.Web.HttpServerUtility.UrlTokenEncode(data);
            }
        }
        #endregion
    }
}