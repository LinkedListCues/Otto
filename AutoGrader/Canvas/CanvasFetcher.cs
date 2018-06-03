using System;
using System.Net;
using AutoGrader.Utils;
using Newtonsoft.Json.Linq;

namespace AutoGrader.Canvas
{
    public class CanvasFetcher
    {
        private const string AUTHORIZATION = "Authorization", BEARER = "Bearer";
        private readonly Uri _baseURI;
        private static string _authKey;

        public CanvasFetcher () {
            _baseURI = AutoGrader.Config.GetSubmissionsURL();
            _authKey = $"{BEARER} {AutoGrader.Config.APIKey}";
        }

        // todo error catching
        private static JArray LoadJsonArrayFromURL (Uri uri) {
            using (var webClient = new WebClient()) {
                webClient.Headers.Add(AUTHORIZATION, _authKey);
                string json = webClient.DownloadString(uri);
                return JArray.Parse(json);
            }
        }

        //
        // public API

        public JArray FetchSubmissions (int page, out bool full) {
            Logger.Log("Fetching page " + page);

            var result = LoadJsonArrayFromURL(new Uri(_baseURI + "&page=" + page));
            full = result.Count >= Configuration.PER_PAGE;
            return result;
        }
    }
}
