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
    /// Длинное описание книги, иначе называемое подкаст-описание.
    /// </summary>
    /// <remarks>Пока не используется.</remarks>
    public string LongDescription;

    /// <summary>
    /// Дополнительное описание книги.
    /// </summary>
    public string Lyrics;

    /// <summary>
    /// Название цикла книг.
    /// </summary>
    /// <remarks>Пока не используется и будет ли использоваться - вопрос.</remarks>
    public string CycleTitle;

    /// <summary>
    /// Номер книги в цикле книг.
    /// </summary>
    /// <remarks>Пока не используется и будет ли использоваться - вопрос.</remarks>
    public int CyclePartNumber;

    /// <summary>
    /// Продолжительность воспроизведения книги.
    /// </summary>
    public TimeSpan Duration;

    /// <summary>
    /// Содержание книги.
    /// </summary>
    public List<ChapterData> Chapters = [];

    /// <summary>
    /// Список массивов байт изображений книги.
    /// </summary>
    public List<byte[]> PicturesData = [];

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
        LongDescription = track.LongDescription;
        Lyrics = track.Lyrics.UnsynchronizedLyrics;
        CycleTitle = track.SeriesTitle;
        CyclePartNumber = int.TryParse(track.SeriesPart, out int cyclePartNumber) ? cyclePartNumber : 0;
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
        foreach (var pictureInfo in track.EmbeddedPictures)
        {
            PicturesData.Add(pictureInfo.PictureData);
        }
    }

    /// <summary>
    /// Возвращает массив байт изображения по указанному индексу в списке массивов байт изображений.
    /// </summary>
    /// <param name="picturesData">Список массивов байт изображений.</param>
    /// <param name="index">Индекс изображения в списке.</param>
    /// <returns>Массив байт изображения.</returns>
    /// <remarks>Если индекс находится вне диапазона списка, то возвращает null.</remarks>
    public static byte[]? GetPictureData(List<byte[]> picturesData, int index)
    {
        if (index < 0 || picturesData.Count == 0 || picturesData.Count <= index)
            return null;
        return picturesData[index];
    }

    /// <summary>
    /// Возвращает массив байт изображения по указанному индексу в списке изображений файла книги.
    /// </summary>
    /// <param name="filename">Имя файла книги с полным путём.</param>
    /// <param name="index">Индекс изображения в списке.</param>
    /// <returns>Массив байт изображения</returns>
    /// <remarks>Если индекс находится вне диапазона списка, то возвращает null.</remarks>
    public static byte[]? GetPictureData(string filename, int index)
    {
        var picturesData = GetPicturesData(filename);
        return picturesData[index];
    }

    /// <summary>
    /// Возвращает список массивов байт изображений из указанного файла книги.
    /// </summary>
    /// <param name="filename">Имя файла книги с полным путём.</param>
    /// <returns>Список массивов байт изображений.</returns>
    public static List<byte[]> GetPicturesData(string filename)
    {
        var track = new Track(filename);
        List<byte[]> result = [];
        foreach (var pictureInfo in track.EmbeddedPictures)
        {
            result.Add(pictureInfo.PictureData);
        }
        return result;
    }
}
