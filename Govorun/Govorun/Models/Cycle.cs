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

    private string annotation = string.Empty;

    /// <summary>
    /// Аннотация к серии книг.
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
    /// Копирует данные серии в указанную серию.
    /// </summary>
    /// <param name="cycle">Серия, в которую копируются данные серии.</param>
    /// <remarks>
    /// Копируются все данные серии кроме идентификатора.
    /// </remarks>
    public void CopyTo(Cycle cycle)
    {
        cycle.Title = Title;
        cycle.Annotation = Annotation;
    }

    /// <summary>
    /// Создаёт и возвращает неполную копию серии.
    /// </summary>
    /// <returns>Неполная копия серии.</returns>
    /// <remarks>
    /// Копия содержит все данные серии кроме идентификатора.
    /// </remarks>
    public Cycle Clone()
    {
        var cycle = new Cycle();
        CopyTo(cycle);
        return cycle;
    }
}
