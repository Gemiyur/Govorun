namespace Govorun.Models;

/// <summary>
/// Класс цикла (серии) книг.
/// </summary>
public class Cycle : BaseModel
{
    /// <summary>
    /// Идентификатор цикла книг.
    /// </summary>
    public int CycleId { get; set; }

    private string title = string.Empty;

    /// <summary>
    /// Название цикла книг.
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

    private string annotation = string.Empty;

    /// <summary>
    /// Аннотация к циклу книг.
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
}
