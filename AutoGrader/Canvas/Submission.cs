using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace AutoGrader
{
    public class Submission
    {
        [JsonProperty("SubmissionID")] public readonly string SubmissionID;
        [JsonProperty("UserID")] public readonly string UserID;
        [JsonProperty("LatePenalty")] public readonly float LatePenalty;
        [JsonProperty("Submitted")] public readonly bool Submitted;

        public Submission () { }

        public Submission (JObject json) {
            SubmissionID = json["id"].ToString();
            UserID = json["user_id"].ToString();
            LatePenalty = CalculateLatePenalty((int)json["seconds_late"]);

            Submitted = json.ContainsKey("attachments");
        }

        private static float CalculateLatePenalty (int secondslate) {
            if (secondslate == 0) { return 0f; }

            int hourslate = secondslate / 3600;
            if (hourslate < 50) { return 0f; }
            return hourslate < 220 ? 0.3f : 1f;
        }
    }
}
