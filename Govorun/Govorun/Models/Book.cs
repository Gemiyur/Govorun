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
    /// Возвращает список авторов книги в виде строки Имя-Фамилия.
    /// Список отсортирован по Фамилия-Имя.
    /// </summary>
    [BsonIgnore]
    public string AuthorNamesFirstLast =>
        App.ListToString(Authors.OrderBy(x => x.NameLastFirst), ", ", x => ((Author)x).NameFirstLast);

    /// <summary>
    /// Возвращает список авторов книги в виде строки Имя-Отчество-Фамилия.
    /// Список отсортирован по Фамилия-Имя-Отчество.
    /// </summary>
    [BsonIgnore]
    public string AuthorNamesFirstMiddleLast =>
        App.ListToString(Authors.OrderBy(x => x.NameLastFirstMiddle), ", ", x => ((Author)x).NameFirstMiddleLast);

    /// <summary>
    /// Возвращает список авторов книги в виде строки Фамилия-Имя.
    /// Список отсортирован по Фамилия-Имя.
    /// </summary>
    [BsonIgnore]
    public string AuthorNamesLastFirst =>
        App.ListToString(Authors.OrderBy(x => x.NameLastFirst), ", ", x => ((Author)x).NameLastFirst);

    /// <summary>
    /// Возвращает список авторов книги в виде строки Фамилия-Имя-Отчество.
    /// Список отсортирован по Фамилия-Имя-Отчество.
    /// </summary>
    [BsonIgnore]
    public string AuthorNamesLastFirstMiddle =>
        App.ListToString(Authors.OrderBy(x => x.NameLastFirstMiddle), ", ", x => ((Author) x).NameLastFirstMiddle);

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

    private Cycle? cycle;

    /// <summary>
    /// Серия книг.
    /// </summary>
    [BsonRef("Cycles")]
    public Cycle? Cycle
    {
        get => cycle;
        set
        {
            cycle = value;
            OnPropertyChanged("Cycle");
            OnPropertyChanged("CycleTitle");
        }
    }

    /// <summary>
    /// Возвращает название серии книг.
    /// </summary>
    [BsonIgnore]
    public string CycleTitle => Cycle != null ? Cycle.Title : string.Empty;

    private string cycleNumbers = string.Empty;

    /// <summary>
    /// Номера книг в серии книг.
    /// </summary>
    public string CycleNumbers
    {
        get => cycleNumbers;
        set
        {
            cycleNumbers = value ?? string.Empty;
            OnPropertyChanged("CycleNumbers");
            OnPropertyChanged("CycleNumbersText");
        }
    }

    /// <summary>
    /// Возвращает строку номеров книг в серии для отображения.
    /// </summary>
    [BsonIgnore]
    public string CycleNumbersText =>
        CycleNumbers.Length > 0 ? $"Номера в серии: {CycleNumbers}" : "Номера в серии не указаны";

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
    [BsonRef("Tags")]
    public List<Genre> Tags { get; set; } = [];

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
