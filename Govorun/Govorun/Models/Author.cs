using System.Text;
using LiteDB;

namespace Govorun.Models;

/// <summary>
/// Класс автора.
/// </summary>
public class Author : BaseModel
{
    /// <summary>
    /// Идентификатор автора.
    /// </summary>
    public int AuthorId { get; set; }

    private string firstName = string.Empty;

    /// <summary>
    /// Имя.
    /// </summary>
    public string FirstName
    {
        get => firstName;
        set
        {
            firstName = value ?? string.Empty;
            OnPropertyChanged("FirstName");
            OnPropertyChanged("NameFirstLast");
            OnPropertyChanged("NameFirstMiddleLast");
            OnPropertyChanged("NameLastFirst");
            OnPropertyChanged("NameLastFirstMiddle");
        }
    }

    private string middleName = string.Empty;

    /// <summary>
    /// Отчество.
    /// </summary>
    public string MiddleName
    {
        get => middleName;
        set
        {
            middleName = value ?? string.Empty;
            OnPropertyChanged("MiddleName");
            OnPropertyChanged("NameFirstLast");
            OnPropertyChanged("NameFirstMiddleLast");
            OnPropertyChanged("NameLastFirst");
            OnPropertyChanged("NameLastFirstMiddle");
        }
    }

    private string lastName = string.Empty;

    /// <summary>
    /// Фамилия.
    /// </summary>
    public string LastName
    {
        get => lastName;
        set
        {
            lastName = value ?? string.Empty;
            OnPropertyChanged("LastName");
            OnPropertyChanged("NameFirstLast");
            OnPropertyChanged("NameFirstMiddleLast");
            OnPropertyChanged("NameLastFirst");
            OnPropertyChanged("NameLastFirstMiddle");
        }
    }

    private string about = string.Empty;

    /// <summary>
    /// Об авторе.
    /// </summary>
    public string About
    {
        get => about;
        set
        {
            about = value ?? string.Empty;
            OnPropertyChanged("About");
        }
    }

    /// <summary>
    /// Имя и фамилия.
    /// </summary>
    [BsonIgnore]
    public string NameFirstLast => ConcatNames(FirstName, string.Empty, LastName);

    /// <summary>
    /// Имя, отчество, фамилия.
    /// </summary>
    [BsonIgnore]
    public string NameFirstMiddleLast => ConcatNames(FirstName, MiddleName, LastName);

    /// <summary>
    /// Фамилия и имя.
    /// </summary>
    [BsonIgnore]
    public string NameLastFirst => ConcatNames(LastName, FirstName, string.Empty);

    /// <summary>
    /// Фамилия, имя, отчество.
    /// </summary>
    [BsonIgnore]
    public string NameLastFirstMiddle => ConcatNames(LastName, FirstName, MiddleName);

    /// <summary>
    /// Возвращает полное имя из составляющих имён в указанном порядке.
    /// </summary>
    /// <param name="name1">Первое имя.</param>
    /// <param name="name2">Второе имя.</param>
    /// <param name="name3">Третье имя.</param>
    /// <returns>Полное имя.</returns>
    private static string ConcatNames(string name1, string name2, string name3)
    {
        var sb = new StringBuilder(name1);
        if (sb.Length > 0 && name2.Length > 0)
            sb.Append(' ');
        sb.Append(name2);
        if (sb.Length > 0 && name3.Length > 0)
            sb.Append(' ');
        sb.Append(name3);
        return sb.ToString();
    }
}
