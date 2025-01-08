using ATL;

namespace Govorun.Media
{
    public class TrackData
    {
        public string Title;

        public string Artist;

        public string Comment;

        public string Description;

        public string Lyrics;

        public TimeSpan Duration;

        public List<ChapterData> Chapters = [];

        public TrackData(string filename)
        {
            var track = new Track(filename);
            Title = track.Title;
            Artist = track.Artist;
            Comment = track.Comment;
            Description = track.Description;
            Lyrics = track.Lyrics.UnsynchronizedLyrics;
            Duration = TimeSpan.FromSeconds(track.Duration);
            foreach (var chapter in track.Chapters)
            {
                var chapterData = new ChapterData()
                {
                    Title = chapter.Title,
                    StartTime = TimeSpan.FromMilliseconds(chapter.StartTime),
                    EndTime = TimeSpan.FromMilliseconds(chapter.EndTime),
                };
                Chapters.Add(chapterData);
            }
        }
    }
}
