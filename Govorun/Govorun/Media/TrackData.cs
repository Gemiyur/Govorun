using ATL;

namespace Govorun.Media;

/// <summary>
/// Класс данных трека файла книги.
/// </summary>
public class TrackData
{
    /// <summary>
    /// Имя файла с полным путём.
    /// </summary>
    public string FileName;

    /// <summary>
    /// Название книги.
    /// </summary>
    public string Title;

    /// <summary>
    /// Автор книги.
    /// </summary>
    public string Author;

    /// <summary>
    /// Комментарий к книге.
    /// </summary>
    public string Comment;

    /// <summary>
    /// Описание книги.
    /// </summary>
    public string Description;

    /// <summary>
    /// Длинное описание книги, иначе называемое подкаст-описание.
    /// </summary>
    /// <remarks>Пока не используется.</remarks>
    public string LongDescription;

    /// <summary>
    /// Дополнительное описание книги.
    /// </summary>
    public string Lyrics;

    /// <summary>
    /// Название серии книг.
    /// </summary>
    public string CycleTitle;

    /// <summary>
    /// Номер книги в серии книг.
    /// </summary>
    public string CyclePart;

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
        FileName = filename;
        Title = track.Title;
        Author = track.Artist;
        Comment = track.Comment;
        Description = track.Description;
        LongDescription = track.LongDescription;

        Lyrics = track.Lyrics[0].UnsynchronizedLyrics;
        //Lyrics = string.Empty;

        CycleTitle = track.SeriesTitle;
        CyclePart = track.SeriesPart;
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
