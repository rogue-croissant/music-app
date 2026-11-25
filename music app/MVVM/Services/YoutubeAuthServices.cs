using Music_app.MVVM.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace Music_app.MVVM.Model
{
    public class YoutubeAuthServices
    {
        private const string ClientID = Secrets.YouTubeClientId;
        private const string RedirectUri = "http://127.0.0.1:4305/";
        private const string AuthUrl = "https://accounts.google.com/o/oauth2/v2/auth";
        private const string TokenUrl = "https://oauth2.googleapis.com/token";

        private string _codeVerifier;
        private string _codeChallenge;
        private string _state;

        public async Task<string> AuthenticateUserAsync()
        {
            GeneratePkcCodees();
            GenerateState();

            var listener = new HttpListener();
            listener.Prefixes.Add(RedirectUri);
            listener.Start();

            // Only request the permissions your app needs.
            string scope =
                "https://www.googleapis.com/auth/youtube.readonly";

            string loginUrl =
                $"{AuthUrl}?" +
                $"client_id={Uri.EscapeDataString(ClientID)}" +
                $"&response_type=code" +
                $"&redirect_uri={Uri.EscapeDataString(RedirectUri)}" +
                $"&scope={Uri.EscapeDataString(scope)}" +
                $"&code_challenge_method=S256" +
                $"&code_challenge={Uri.EscapeDataString(_codeChallenge)}" +
                $"&access_type=offline" +
                $"&prompt=consent" +
                $"&state={Uri.EscapeDataString(_state)}";

            try
            {
                Process.Start(new ProcessStartInfo(loginUrl)
                {
                    UseShellExecute = true
                });

                // Wait for Google to redirect back to the app.
                var context = await listener.GetContextAsync();

                var request = context.Request;

                string code = request.QueryString["code"];
                string error = request.QueryString["error"];

                string responseString;

                if (!string.IsNullOrEmpty(code))
                {
                    responseString =
                        "<html><body><h1>Login Successful!</h1>" +
                        "<p>You can close this window and return to the app.</p>" +
                        "</body></html>";
                }
                else
                {
                    responseString =
                        "<html><body><h1>Login Failed</h1>" +
                        "<p>Authentication was unsuccessful.</p>" +
                        "</body></html>";
                }

                var buffer = Encoding.UTF8.GetBytes(responseString);

                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(
                    buffer, 0, buffer.Length);

                context.Response.Close();

                string returnedState = request.QueryString["state"];

                if (returnedState != _state)
                {
                    throw new Exception("Invalid OAuth state.");
                }

                if (!string.IsNullOrEmpty(error))
                {
                    throw new Exception(
                        $"Google authentication failed: {error}");
                }

                if (string.IsNullOrEmpty(code))
                {
                    throw new Exception(
                        "Authorization code not received from Google.");
                }

                return await ExchangeCodeForTokenAsync(code);
            }
            finally
            {
                listener.Stop();
                listener.Close();
            }
        }

        private void GenerateState()
        {
            byte[] bytes = RandomNumberGenerator.GetBytes(32);
            _state = Base64UrlEncode(bytes);
        }

        private void GeneratePkcCodees()
        {
            using var rng = RandomNumberGenerator.Create();

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



 


