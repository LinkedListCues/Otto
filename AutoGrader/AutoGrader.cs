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
                break;
            }
        }

        public void GradeSubmissions () {
            Logger.Log("Grading all submissions...");
            for (int i = 0, c = _roster.Submissions.Count; i < c; i++) {
                Grader.Grade(_roster.Submissions[i], i + 1, c);
                break;
            }
        }
    }
}
