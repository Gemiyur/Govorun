namespace Govorun.Media;

/// <summary>
/// Класс раздела книги.
/// </summary>
public class ChapterData
{
    /// <summary>
    /// Название раздела книги.
    /// </summary>
    public string Title = string.Empty;

    /// <summary>
    /// Время начала раздела книги.
    /// </summary>
    public TimeSpan StartTime;

    /// <summary>
    /// Время конца раздела книги.
    /// </summary>
    public TimeSpan EndTime;
}
