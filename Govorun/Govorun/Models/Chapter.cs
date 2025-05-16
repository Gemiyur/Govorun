using LiteDB;

namespace Govorun.Models;

/// <summary>
/// Класс раздела книги.
/// </summary>
public class Chapter : BaseModel
{
    private TimeSpan startTime;

    /// <summary>
    /// Позиция начала раздела книги.
    /// </summary>
    public TimeSpan StartTime
    {
        get => startTime;
        set
        {
            startTime = value;
            OnPropertyChanged("StartTime");
            OnPropertyChanged("StartTimeText");
            OnPropertyChanged("Duration");
            OnPropertyChanged("DurationText");
        }
    }

    /// <summary>
    /// Позиция начала раздела книги в виде строки.
    /// </summary>
    [BsonIgnore]
    public string StartTimeText => App.TimeSpanToString(StartTime);

    private TimeSpan endTime;

    /// <summary>
    /// Позиция конца раздела книги.
    /// </summary>
    public TimeSpan EndTime
    {
        get => endTime;
        set
        {
            endTime = value;
            OnPropertyChanged("EndTime");
            OnPropertyChanged("Duration");
            OnPropertyChanged("DurationText");
        }
    }

    /// <summary>
    /// Продолжительность раздела книги.
    /// </summary>
    [BsonIgnore]
    public TimeSpan Duration => EndTime - StartTime;

    /// <summary>
    /// Продолжительность раздела книги в виде строки.
    /// </summary>
    [BsonIgnore]
    public string DurationText => App.TimeSpanToString(Duration);

    private string title = string.Empty;

    /// <summary>
    /// Название раздела книги.
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
    /// Список подразделов раздела книги.
    /// </summary>
    public List<Chapter> Chapters { get; set; } = [];
}
