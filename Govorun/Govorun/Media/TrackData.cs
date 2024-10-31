using System.Windows.Media.Imaging;
using ATL;

namespace Govorun.Media
{
    public class TrackData
    {
        public string Title;

        public string Comment;

        public string Artist;

        public List<ChapterData> Chapters = [];

        public TimeSpan Duration;

        public List<BitmapFrame> Pictures = [];

        public List<byte[]> PicturesData = [];

        public byte[] PictureData => PicturesData.Count > 0 ? PicturesData[0] : [];

        public TrackData(string filename)
        {
            var track = new Track(filename);
            Title = track.Title;
            Comment = track.Comment;
            Artist = track.Artist;
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
            Duration = TimeSpan.FromSeconds(track.Duration);
            foreach (var picture in track.EmbeddedPictures)
            {
                Pictures.Add(App.GetBitmap(picture.PictureData));
                PicturesData.Add(picture.PictureData);
            }
        }
    }
}
