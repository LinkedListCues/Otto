namespace AutoGrader
{
    public class AutoGrader : IManager
    {
        public static CanvasFetcher Fetcher;

        private Roster _roster;


        public void Initialize () {
            Fetcher = new CanvasFetcher();

            _roster = new Roster();
            _roster.Initialize();
        }
    }
}
