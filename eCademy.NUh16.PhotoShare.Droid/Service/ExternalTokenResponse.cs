using Newtonsoft.Json;
using System;

namespace eCademy.NUh16.PhotoShare.Droid
{
    public class ExternalTokenResponse
    {
        [JsonProperty("userName")]
        public string Username { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public double ExpiresIn { get; set; }

        [JsonProperty("issued")]
        public DateTime Issued { get; set; }

        [JsonProperty("expires")]
        public DateTime Expires { get; set; }
    }
}