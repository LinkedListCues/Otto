using AutoGrader.Canvas;
using AutoGrader.Utils;

namespace AutoGrader
{
    public class AutoGrader
    {
        public static CanvasFetcher Fetcher;
        public static Builder Builder;

        private Roster _roster;

        public void Initialize () {
            Serializer.InitializeDirectories();

            Fetcher = new CanvasFetcher();
            Builder = new Builder();

            _roster = new Roster();
        }

        public void PrepareSubmissions () {
            Logger.Log("Preparing submissions...");
            for (int i = 0, c = _roster.Submissions.Count; i < c; i++) {
                _roster.Submissions[i].PrepareSubmissionFiles(i + 1, c);
            }
        }

        public void GradeSubmissions () {
            Logger.Log("Grading all submissions...");
            for (int i = 0, c = _roster.Submissions.Count; i < c; i++) {
                var sub = _roster.Submissions[i];
                Grader.Grade(sub, i + 1, c);
                sub.UploadResults();
            }
        }
    }
}
