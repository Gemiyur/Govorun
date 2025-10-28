namespace Govorun.Models;

/// <summary>
/// Класс жанра книги.
/// </summary>
public class Genre : BaseModel
{
    /// <summary>
    /// Идентификатор жанра книги.
    /// </summary>
    public int GenreId { get; set; }

    private string title = string.Empty;

    /// <summary>
    /// Название жанра книги.
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
