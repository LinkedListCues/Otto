using AutoGrader.Canvas;
using AutoGrader.Utils;

namespace AutoGrader
{
    public class AutoGrader
    {
        public static Configuration Config;

        private readonly Roster _roster;

        public AutoGrader (Configuration config) {
            Config = config;
            _roster = new Roster();
        }

        public void PrepareRoster () { _roster.PrepareRoster(); }

        public void PrepareSubmissions () {
            Logger.Log("Preparing submissions...");
            for (int i = 0, c = _roster.Submissions.Count; i < c; i++) {
                var sub = _roster.Submissions[i];
                Logger.Log($"Preparing {sub.SubmissionID} ({i + 1} of {c})");
                if (sub.Submitted) sub.PrepareSubmissionFiles(i + 1, c);
            }
        }

        public void GradeSubmissions () {
            Logger.Log("Grading all submissions...");
            for (int i = 0, c = _roster.Submissions.Count; i < c; i++) {
                var sub = _roster.Submissions[i];
                Logger.Log($"Grading {sub.SubmissionID} ({i + 1} of {c})");
                if (sub.Submitted) Evaluater.Grade(sub);
            }
        }

        public void UploadSubmissions () {
            Logger.Log("Uploading all submissions...");
            for (int i = 0, c = _roster.Submissions.Count; i < c; i++) {
                var sub = _roster.Submissions[i];
                Logger.Log($"Uploading {sub.SubmissionID} ({i + 1} of {c})");
                if (sub.Submitted) sub.UploadResults();
            }
        }
    }
}
