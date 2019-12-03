using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace Disbot.Configurations
{
    public class SpotifyConfiguration
    {
        private readonly AuthorizationCodeAuth AuthenFactory;
        private readonly Timer _refreshTokenWorker;
        private string RefreshToken;
        public SpotifyWebAPI Client
        { get; private set; }
        public static SpotifyConfiguration Context { get; } = new SpotifyConfiguration();
        SpotifyConfiguration()
        {
            bool initialized = false;
            var client_id = "7b2f38e47869431caeda389929a1908e";
            var secret_id = "90dc137926e34bd78a1737266b3df20b";
            AuthenFactory = new AuthorizationCodeAuth(
                                client_id,
                                secret_id,
                                "http://localhost:8888",
                                "http://localhost:8888",
                                SpotifyAPI.Web.Enums.Scope.AppRemoteControl
                                );
            AuthenFactory.AuthReceived += async (s, p) =>
            {
                var ath = (AuthorizationCodeAuth)s;
                ath.Stop();

                var token = await ath.ExchangeCode(p.Code);
                initialized = true;
                RefreshToken = token.RefreshToken;
                if (Client == null)
                {
                    Client = new SpotifyWebAPI()
                    {
                        AccessToken = token.AccessToken,
                        TokenType = "Bearer"
                    };
                }
                else
                {
                    Client.AccessToken = token.AccessToken;
                }
            };
            AuthenFactory.Start();
            AuthenFactory.OpenBrowser();
            while (!initialized) System.Threading.Thread.Sleep(1000);
            AuthenFactory.Stop();
            _refreshTokenWorker = new Timer();
            _refreshTokenWorker.Interval = 30 * (1000 * 60);
            _refreshTokenWorker.Elapsed += async (s, e) =>
            {
                Console.WriteLine("Refreshing spotify token...");
                await AuthenFactory.RefreshToken(RefreshToken);
            };
            _refreshTokenWorker.Start();
        }
    }
}
