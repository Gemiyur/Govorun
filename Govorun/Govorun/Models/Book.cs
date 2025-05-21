using LiteDB;
using System.IO;
using System.Windows.Media.Imaging;

namespace Govorun.Models;

/// <summary>
/// Класс книги.
/// </summary>
public class Book : BaseModel
{
    /// <summary>
    /// Идентификатор книги.
    /// </summary>
    public int BookId { get; set; }

    private string title = string.Empty;

    /// <summary>
    /// Название книги.
    /// </summary>
    public string Title
    {
        get => title;
        set
        {
            title = value ?? string.Empty;
            OnPropertyChanged("Title");
        }
    }

    /// <summary>
    /// Список авторов книги.
    /// </summary>
    [BsonRef("Authors")]
    public List<Author> Authors { get; set; } = [];

    /// <summary>
    /// Возвращает список авторов книги в виде строки Фамилия-Имя.
    /// Список отсортирован по Фамилия-Имя.
    /// </summary>
    [BsonIgnore]
    public string AuthorNamesLastFirst =>
        App.ListToString(Authors, ", ", x => ((Author)x).NameLastFirst, StringComparer.CurrentCultureIgnoreCase);

    /// <summary>
    /// Возвращает список авторов книги в виде строки Фамилия-Имя-Отчество.
    /// Список отсортирован по Фамилия-Имя-Отчество.
    /// </summary>
    [BsonIgnore]
    public string AuthorNamesLastFirstMiddle =>
        App.ListToString(Authors, ", ", x => ((Author)x).NameLastFirstMiddle, StringComparer.CurrentCultureIgnoreCase);

    /// <summary>
    /// Возвращает список авторов книги в виде строки Имя-Фамилия.
    /// Список отсортирован по Фамилия-Имя.
    /// </summary>
    [BsonIgnore]
    public string AuthorNamesFirstLast
    {
        get
        {
            var authorsList = Authors.OrderBy(x => x.NameLastFirst).ToList();
            return App.ListToString(Authors, ", ", x => ((Author)x).NameFirstLast);
        }
    }

    /// <summary>
    /// Возвращает список авторов книги в виде строки Имя-Отчество-Фамилия.
    /// Список отсортирован по Фамилия-Имя-Отчество.
    /// </summary>
    [BsonIgnore]
    public string AuthorNamesFirstMiddleLast
    {
        get
        {
            var authorsList = Authors.OrderBy(x => x.NameLastFirstMiddle).ToList();
            return App.ListToString(Authors, ", ", x => ((Author)x).NameFirstMiddleLast);
        }
    }

    private string annotation = string.Empty;

    /// <summary>
    /// Аннотация к книге.
    /// </summary>
    public string Annotation
    {
        get => annotation;
        set
        {
            annotation = value ?? string.Empty;
            OnPropertyChanged("Annotation");
        }
    }

    /// <summary>
    /// Индекс изображения обложки книги в теге файла книги.
    /// </summary>
    public int CoverIndex { get; set; } = -1;

    // TODO: Пока непонятно где должны находится методы получения изображения обложки.

    ///// <summary>
    ///// Массив байт изображения обложки книги.
    ///// </summary>
    //[BsonIgnore]
    //public byte[]? CoverData { get; set; }

    ///// <summary>
    ///// Изображение обложки книги.
    ///// </summary>
    //[BsonIgnore]
    //public BitmapFrame? Cover => CoverData != null ? App.GetBitmap(CoverData) : null;

    /// <summary>
    /// Список частей (номеров) книги в циклах книг.
    /// </summary>
    public List<CyclePart> CycleParts { get; set; } = [];

    private string lector = string.Empty;

    /// <summary>
    /// Чтец книги.
    /// </summary>
    public string Lector
    {
        get => lector;
        set
        {
            lector = value ?? string.Empty;
            OnPropertyChanged("Lector");
        }
    }

    private string translator = string.Empty;

    /// <summary>
    /// Переводчик книги.
    /// </summary>
    public string Translator
    {
        get => translator;
        set
        {
            translator = value ?? string.Empty;
            OnPropertyChanged("Translator");
        }
    }

    /// <summary>
    /// Теги книги.
    /// </summary>
    public List<Tag> Tags { get; set; } = [];

    private string filename = string.Empty;

    /// <summary>
    /// Файл книги с полным путём.
    /// </summary>
    public string FileName
    {
        get => filename;
        set
        {
            filename = value ?? string.Empty;
            OnPropertyChanged("FileName");
        }
    }

    /// <summary>
    /// Размер файла книги в байтах.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Существует ли файл книги.
    /// </summary>
    [BsonIgnore]
    public bool FileExists => File.Exists(FileName);

    /// <summary>
    /// Продолжительность книги.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Продолжительность книги в виде строки.
    /// </summary>
    [BsonIgnore]
    public string DurationText => App.TimeSpanToString(Duration);

    /// <summary>
    /// Позиция воспроизведения.
    /// </summary>
    public TimeSpan PlayPosition { get; set; }

    /// <summary>
    /// Позиция воспроизведения в виде строки.
    /// </summary>
    [BsonIgnore]
    public string PlayPositionText => App.TimeSpanToString(PlayPosition);

    /// <summary>
    /// Находится ли книга в состоянии прослушивания.
    /// </summary>
    [BsonIgnore]
    public bool Listening => PlayPosition > TimeSpan.Zero;

    /// <summary>
    /// Список разделов книги.
    /// </summary>
    public List<Chapter> Chapters { get; set; } = [];

    /// <summary>
    /// Список закладок книги.
    /// </summary>
    public List<Bookmark> Bookmarks { get; set; } = [];
}
