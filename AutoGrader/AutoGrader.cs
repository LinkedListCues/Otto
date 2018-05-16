namespace AutoGrader
{
    public class AutoGrader : IManager
    {
        public static CanvasFetcher Fetcher;

        private Roster _roster;


        public void Initialize () {
            Fetcher = new CanvasFetcher();
            Serializer.InitializeDirectories();

            _roster = new Roster();
            _roster.Initialize();
        }

        public void DownloadSubmissions () {
            Logger.Log("Downloading all submissions...");
            for (int i = 0, c = _roster.Submissions.Count; i < c; i++) {
                _roster.Submissions[i].PrepareSubmissionFiles(i + 1, c);
            }
        }
    }
}
