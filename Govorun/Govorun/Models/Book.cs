using LiteDB;
using System.IO;

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
    public string AuthorsSurnameNameText =>
        App.ListToString(Authors, ", ", x => ((Author)x).SurnameName, StringComparer.CurrentCultureIgnoreCase);

    /// <summary>
    /// Возвращает список авторов книги в виде строки Имя-Фамилия.
    /// Список отсортирован по Фамилия-Имя.
    /// </summary>
    [BsonIgnore]
    public string AuthorsNameSurnameText
    {
        get
        {
            var authorsList = Authors.OrderBy(x => x.SurnameName).ToList();
            return App.ListToString(Authors, ", ", x => ((Author)x).NameSurname);
        }
    }

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

    private string annotation = string.Empty;

    /// <summary>
    /// Комментарий к книге.
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
