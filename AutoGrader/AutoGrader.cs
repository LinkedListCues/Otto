namespace AutoGrader
{
    public class AutoGrader
    {
        public static CanvasFetcher Fetcher;
        public static Builder Builder;

        private Roster _roster;
        private Grader _grader;

        public void Initialize () {
            Serializer.InitializeDirectories();

            Fetcher = new CanvasFetcher();
            Builder = new Builder();

            _roster = new Roster();
            _grader = new Grader();
        }

        public void DownloadSubmissions () {
            Logger.Log("Downloading all submissions...");
            for (int i = 0, c = _roster.Submissions.Count; i < c; i++) {
                _roster.Submissions[i].PrepareSubmissionFiles(i + 1, c);
            }
        }

        public void GradeSubmissions () {
            Logger.Log("Grading all submissions...");
            for (int i = 0, c = _roster.Submissions.Count; i < c; i++) {
                _grader.Grade(_roster.Submissions[i], i + 1, c);
            }
        }
    }
}
