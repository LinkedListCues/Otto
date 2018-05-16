using System;
using System.Net;
using Newtonsoft.Json.Linq;

namespace AutoGrader
{
    public class CanvasFetcher
    {
        private static readonly Uri BASE_URI = new Uri("https://canvas.northwestern.edu/api/v1/courses/72859/assignments/458956/submissions");
        private const string ARGUMENTS =
            "?access_token=1876~nSmP6pGTi0LsIdPe8h19TLVL9zAP5tHTgvfMd08cjLZAdarU0HF5KQSyss8JGcdp&per_page=99";

        // todo error catching
        private static JArray LoadJsonArrayFromURL (Uri uri) {
            using (var webClient = new WebClient()) {
                string json = webClient.DownloadString(uri);
                return JArray.Parse(json);
            }
        }

        // todo error catching
        private static JObject LoadJsonFromURL (string url) {
            using (var webClient = new WebClient()) {
                string json = webClient.DownloadString(url);
                return JObject.Parse(json);
            }
        }

        private static Uri MakeURI (string uri) {
            return new Uri(BASE_URI, uri);
        }

        //
        // public API

        public JArray FetchSubmissions (int page) {
            Logger.Log("Fetching page " + page);
            return LoadJsonArrayFromURL(MakeURI(ARGUMENTS + "&page=" + page));
        }
    }
}
