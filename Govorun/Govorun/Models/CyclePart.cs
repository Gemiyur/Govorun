using LiteDB;

namespace Govorun.Models;

/// <summary>
/// Класс номера книги в серии книг.
/// </summary>
public class CyclePart : BaseModel
{
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
        }
    }

    private int number;

    /// <summary>
    /// Номер книги в серии книг.
    /// </summary>
    public int Number
    {
        get => number;
        set
        {
            number = value;
            OnPropertyChanged("Number");
        }
    }
}
