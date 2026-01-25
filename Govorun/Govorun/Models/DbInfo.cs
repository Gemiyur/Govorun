namespace Govorun.Models;

/// <summary>
/// Класс информации о базе данных.
/// </summary>
public class DbInfo : BaseModel
{
    /// <summary>
    /// Возвращает или задаёт идентификатор информации о базе данных.
    /// </summary>
    public int DbInfoId { get; set; }

    /// <summary>
    /// Имя приложения базы данных.
    /// </summary>
    public string name = string.Empty;

    /// <summary>
    /// Возвращает или задаёт имя приложения базы данных.
    /// </summary>
    public string Name
    {
        get => name;
        set
        {
            name = value ?? string.Empty;
            OnPropertyChanged("Name");
        }
    }
}
