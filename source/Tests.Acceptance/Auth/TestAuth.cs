using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Ploeh.AutoFixture.Kernel;
using Web.Host.Controllers.Api;
using Xunit;

namespace Tests.Acceptance.Auth
{
    public class TestAuth
    {
        [Fact]
        public void SomeNameForTestss()
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, "http://localhost:58018/api/auth/request_token");
            IEnumerable<KeyValuePair<string, string>> formContent = new Dictionary<string, string>()
            {
                {"username", "AndrewAllison"},
                {"password", "Pa$$word123"}
            };
            message.Content = new FormUrlEncodedContent(formContent);

            
            var response = client.SendAsync(message).Result;
            var result = response.Content.ReadAsStringAsync().Result;
            var token = JsonConvert.DeserializeObject<AuthenticateResponse>(result);

            HttpRequestMessage get = new HttpRequestMessage(HttpMethod.Get, "http://localhost:58018/api/Me");
            
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

            var authResponse = client.SendAsync(get).Result;

        }

        [Fact]
        public void FactMethodName()
        {
            // You need to set your own keys and screen name
            var oAuthConsumerKey = "2Q6wL08LDYEVygURwT3oiw";
            var oAuthConsumerSecret = "ytyfXG8ODV42ZabvI6st7n4kq2Dg1kct2kugXrbn2g";
            var oAuthUrl = "https://api.twitter.com/oauth2/token";
            var screenname = "AndrewAllison";

            // Do the Authenticate
            var authHeaderFormat = "Basic {0}";

            var authHeader = string.Format(authHeaderFormat,
                Convert.ToBase64String(Encoding.UTF8.GetBytes(Uri.EscapeDataString(oAuthConsumerKey) + ":" +
                Uri.EscapeDataString((oAuthConsumerSecret)))
            ));

            var postBody = "grant_type=client_credentials";

            HttpWebRequest authRequest = (HttpWebRequest)WebRequest.Create(oAuthUrl);
            authRequest.Headers.Add("Authorization", authHeader);
            authRequest.Method = "POST";
            authRequest.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
            authRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (Stream stream = authRequest.GetRequestStream())
            {
                byte[] content = ASCIIEncoding.ASCII.GetBytes(postBody);
                stream.Write(content, 0, content.Length);
            }

            authRequest.Headers.Add("Accept-Encoding", "gzip");

            WebResponse authResponse = authRequest.GetResponse();
            // deserialize into an object
            TwitAuthenticateResponse twitAuthResponse;
            using (authResponse)
            {
                using (var reader = new StreamReader(authResponse.GetResponseStream()))
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    var objectText = reader.ReadToEnd();
                    twitAuthResponse = JsonConvert.DeserializeObject<TwitAuthenticateResponse>(objectText);
                }
            }

            // Do the timeline
            var timelineFormat = "https://api.twitter.com/1.1/statuses/user_timeline.json?screen_name={0}&include_rts=1&exclude_replies=1&count=5";
            var timelineUrl = string.Format(timelineFormat, screenname);
            HttpWebRequest timeLineRequest = (HttpWebRequest)WebRequest.Create(timelineUrl);
            var timelineHeaderFormat = "{0} {1}";
            timeLineRequest.Headers.Add("Authorization", string.Format(timelineHeaderFormat, twitAuthResponse.token_type, twitAuthResponse.access_token));
            timeLineRequest.Method = "Get";
            WebResponse timeLineResponse = timeLineRequest.GetResponse();
            var timeLineJson = string.Empty;
            using (timeLineResponse)
            {
                using (var reader = new StreamReader(timeLineResponse.GetResponseStream()))
                {
                    timeLineJson = reader.ReadToEnd();
                }
            }


        }
    }


    public class TwitAuthenticateResponse
    {
        public string token_type { get; set; }
        public string access_token { get; set; }
    }
}