namespace Govorun.Models;

/// <summary>
/// Класс тега книги.
/// </summary>
public class Tag : BaseModel
{
    /// <summary>
    /// Идентификатор тега книги.
    /// </summary>
    public int TagId { get; set; }

    private string title = string.Empty;

    /// <summary>
    /// Название тега книги.
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
}
