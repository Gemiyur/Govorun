using ATL;

namespace Govorun.Media;

/// <summary>
/// Класс данных книги из тега файла книги.
/// </summary>
public class TrackData
{
    /// <summary>
    /// Название книги.
    /// </summary>
    public string Title;

    /// <summary>
    /// Автор книги.
    /// </summary>
    public string Author;

    /// <summary>
    /// Название альбома.
    /// </summary>
    public string AlbumTitle;

    /// <summary>
    /// Автор альбома.
    /// </summary>
    public string AlbumAuthor;

    /// <summary>
    /// Комментарий к книге.
    /// </summary>
    public string Comment;

    /// <summary>
    /// Описание книги.
    /// </summary>
    public string Description;

    /// <summary>
    /// Дополнительное описание книги.
    /// </summary>
    public string Lyrics;

    /// <summary>
    /// Продолжительность воспроизведения книги.
    /// </summary>
    public TimeSpan Duration;

    /// <summary>
    /// Содержание книги.
    /// </summary>
    public List<ChapterData> Chapters = [];

    /// <summary>
    /// Инициализирует новый экземпляр класса.
    /// </summary>
    /// <param name="filename">Имя файла книги с полным путём.</param>
    public TrackData(string filename)
    {
        var track = new Track(filename);
        Title = track.Title;
        Author = track.Artist;
        AlbumTitle = track.Album;
        AlbumAuthor = track.AlbumArtist;
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
