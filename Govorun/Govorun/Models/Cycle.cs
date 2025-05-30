namespace Govorun.Models;

/// <summary>
/// Класс серии книг.
/// </summary>
public class Cycle : BaseModel
{
    /// <summary>
    /// Идентификатор серии книг.
    /// </summary>
    public int CycleId { get; set; }

    private string title = string.Empty;

    /// <summary>
    /// Название серии книг.
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
