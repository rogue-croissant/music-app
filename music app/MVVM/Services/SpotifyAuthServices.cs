using Music_app.MVVM.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Packaging;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static SpotifyAPI.Web.PlayerSetRepeatRequest;

namespace Music_app.MVVM.Model
{
    public class SpotifyAuthServices
    {
        private const string ClientID = Secrets.SpotifyClientId;
        private const string RedirectUri = "http://127.0.0.1:4304/callback/";
        private const string AuthUrl = "https://accounts.spotify.com/authorize";
        private const string TokenUrl = "https://accounts.spotify.com/api/token";

        private string _codeVerifier;
        private string _codeChallenge;
        public async Task<string> AuthenticateUserAsync()
        {
            GeneratePkcCodees();

            var listener = new HttpListener();
            listener.Prefixes.Add(RedirectUri);
            listener.Start();


            string scope =  "streaming " +
                            "user-modify-playback-state " +
                            "user-read-playback-state " +
                            "user-read-currently-playing "  +
                            "user-read-private " +
                            "user-read-email " +
                            "playlist-read-private " +
                            "playlist-read-collaborative " +
                            "user-library-read ";

            string loginUrl =
                $"{AuthUrl}?client_id={ClientID}" +
                $"&response_type=code" +
                $"&redirect_uri={Uri.EscapeDataString(RedirectUri)}" +
                $"&code_challenge_method=S256" +
                $"&code_challenge={_codeChallenge}" +
                $"&scope={Uri.EscapeDataString(scope)}";
           
            Process.Start(new ProcessStartInfo(loginUrl) { UseShellExecute = true });



            // 4. Wait for the redirect callback
            var context = await listener.GetContextAsync();
            var request = context.Request;
            string code = request.QueryString["code"];

            // 5. Send response to the browser
            var response = context.Response;
            string responseString = "<html><body><h1>Login Successful!</h1><p>You can close this window now and return to the app.</p></body></html>";
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.OutputStream.Close();
            listener.Stop();

            // 6. Exchange the authorization code for an access token
            if (!string.IsNullOrEmpty(code))
            {
                return await ExchangeCodeForTokenAsync(code);
            }

            throw new Exception("Authorization code not received from Spotify.");

        }

        private void GeneratePkcCodees()
        {
           using var rng =RandomNumberGenerator.Create();

            byte[] bytes = new byte[32];
            rng.GetBytes(bytes);
            _codeVerifier = Base64UrlEncode(bytes);


            using var sha256 = SHA256.Create();
            byte[] challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(_codeVerifier));
            _codeChallenge = Base64UrlEncode(challengeBytes);
        }

        private string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private async Task<string> ExchangeCodeForTokenAsync(string code)
        {
            using var client = new HttpClient();
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", ClientID),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri",RedirectUri),
                new KeyValuePair<string, string>("code_verifier",_codeVerifier)
            });

            var response = await client.PostAsync(TokenUrl, content);
            string responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(responseString);
                return doc.RootElement.GetProperty("access_token").GetString();
            }
            throw new Exception($"Failed to get token: {responseString}");
        }
    }
}
