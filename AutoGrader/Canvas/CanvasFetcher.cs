using System;
using System.Net;
using AutoGrader.Utils;
using Newtonsoft.Json.Linq;

namespace AutoGrader.Canvas
{
    public class CanvasFetcher
    {
        private const string AUTHORIZATION = "Authorization", BEARER = "Bearer";
        private const int PER_PAGE = 99;
        private readonly Uri _baseURI;
        private static string _authKey, _arguments;

        public CanvasFetcher () {
            const string assignment = "460601"; // todo param

            _baseURI = new Uri($"https://canvas.northwestern.edu/api/v1/courses/72859/assignments/{assignment}/submissions"); // todo param
            _authKey = $"{BEARER} {Serializer.GetAPIKey()}";
            _arguments = $"?per_page={PER_PAGE}";
        }

        // todo error catching
        private static JArray LoadJsonArrayFromURL (Uri uri) {
            using (var webClient = new WebClient()) {
                webClient.Headers.Add(AUTHORIZATION, _authKey);
                string json = webClient.DownloadString(uri);
                return JArray.Parse(json);
            }
        }

        private Uri MakeURI (string uri) {
            return new Uri(_baseURI, uri);
        }

        //
        // public API

        public JArray FetchSubmissions (int page, out bool full) {
            Logger.Log("Fetching page " + page);

            var result = LoadJsonArrayFromURL(MakeURI(_arguments + "&page=" + page));
            full = result.Count >= PER_PAGE;
            return result;
        }
    }
}
