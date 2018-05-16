using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace AutoGrader
{
    public class Roster : IManager
    {
        private string _tempurl =
            @"https://canvas.northwestern.edu/api/v1/courses/72859/assignments/?access_token=1876~nSmP6pGTi0LsIdPe8h19TLVL9zAP5tHTgvfMd08cjLZAdarU0HF5KQSyss8JGcdp&per_page=99&page=1";

        public List<Submission> Submissions { get; private set; }

        public Roster () {
            Submissions = new List<Submission>();
        }

        public void Initialize () {
            Logger.Log(LoadURL(_tempurl));
        }

        public void PrepareRoster () {
        }

        private void FetchAndSaveRoster () {

        }

        private string LoadURL (string url) {
            return LoadJsonArrayFromURL(url).ToString();
        }

        // todo error catching
        private JArray LoadJsonArrayFromURL (string url) {
            using (var webClient = new WebClient()) {
                string json = webClient.DownloadString(url);
                return JArray.Parse(json);
            }
        }
    }

    public class Submission
    {
        public readonly string SubmissionID, UserID, NetID;
        public readonly int SecondsLate;
        public readonly bool Submitted;

        private JObject _json;

        public Submission (string netid, JObject json) {
            _json = json;
            NetID = netid;

            SubmissionID = json["id"].ToString();
            UserID = json["user_id"].ToString();

            Submitted = json.ContainsKey("attachments");
        }
    }
}
