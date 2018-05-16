using System;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace AutoGrader
{
    public class CanvasFetcher : IManager
    {
        private const int PER_PAGE = 99;
        private static readonly Uri BASE_URI = new Uri("https://canvas.northwestern.edu/api/v1/courses/72859/assignments/458956/submissions");
        private static string ARGUMENTS;


        public void Initialize () {
            var secret = Serializer.GetAPIKey();
            ARGUMENTS = $"?access_token={secret}&per_page={PER_PAGE}";
        }


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

        public JArray FetchSubmissions (int page, out bool full) {
            Logger.Log("Fetching page " + page);

            var result = LoadJsonArrayFromURL(MakeURI(ARGUMENTS + "&page=" + page));
            full = result.Count >= PER_PAGE;
            return result;
        }
    }
}
