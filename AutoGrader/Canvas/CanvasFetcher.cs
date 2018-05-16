using System.Net;
using Newtonsoft.Json.Linq;

namespace AutoGrader
{
    public class CanvasFetcher
    {
        private string _tempurl =
            @"https://canvas.northwestern.edu/api/v1/courses/72859/assignments/458956/submissions?access_token=1876~nSmP6pGTi0LsIdPe8h19TLVL9zAP5tHTgvfMd08cjLZAdarU0HF5KQSyss8JGcdp&per_page=99&page=1";

        public JArray FetchSubmissions () {
            return LoadJsonArrayFromURL(_tempurl);
        }

        // todo error catching
        private static JArray LoadJsonArrayFromURL (string url) {
            using (var webClient = new WebClient()) {
                string json = webClient.DownloadString(url);
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
    }
}
