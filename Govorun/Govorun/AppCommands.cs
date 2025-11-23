using System.Windows.Input;

namespace Govorun;

/// <summary>
/// Статический класс команд приложения.
/// </summary>
public static class AppCommands
{
    #region Команды библиотеки.

    /// <summary>
    /// Команда настроек приложения.
    /// </summary>
    public static RoutedUICommand Settings { get; private set; }

    /// <summary>
    /// Команда выхода из приложения.
    /// </summary>
    public static RoutedUICommand Exit { get; private set; }

    #endregion

    #region Команды книг.

    /// <summary>
    /// Команда добавления книги в библиотеку.
    /// </summary>
    public static RoutedUICommand AddBook { get; private set; }

    /// <summary>
    /// Команда поиска книг в папке.
    /// </summary>
    public static RoutedUICommand FindBooks { get; private set; }

    /// <summary>
    /// Команда воспроизведения книги.
    /// </summary>
    public static RoutedUICommand Play { get; private set; }

    /// <summary>
    /// Команда информации о книге.
    /// </summary>
    public static RoutedUICommand Info { get; private set; }

    /// <summary>
    /// Команда содержания книги.
    /// </summary>
    public static RoutedUICommand Chapters { get; private set; }

    /// <summary>
    /// Команда закладок книги.
    /// </summary>
    public static RoutedUICommand Bookmarks { get; private set; }

    /// <summary>
    /// Команда исключения книги из слушаемых.
    /// </summary>
    public static RoutedUICommand NotListen { get; private set; }

    /// <summary>
    /// Команда редактирования данных книги.
    /// </summary>
    public static RoutedUICommand Edit { get; private set; }

    /// <summary>
    /// Команда удаления выбранных книг.
    /// </summary>
    public static RoutedUICommand Delete { get; private set; }

    #endregion

    #region Команды авторов.

    /// <summary>
    /// Команда редактирования авторов.
    /// </summary>
    public static RoutedUICommand Authors { get; private set; }

    /// <summary>
    /// Команда информации об авторе.
    /// </summary>
    public static RoutedUICommand AuthorInfo { get; private set; }

    /// <summary>
    /// Команда редактирования автора.
    /// </summary>
    public static RoutedUICommand AuthorEdit { get; private set; }

    /// <summary>
    /// Команда удаления автора.
    /// </summary>
    public static RoutedUICommand AuthorDelete { get; private set; }

    #endregion

    #region Команды серий.

    /// <summary>
    /// Команда редактирования серий.
    /// </summary>
    public static RoutedUICommand Cycles { get; private set; }

    /// <summary>
    /// Команда информации о серии.
    /// </summary>
    public static RoutedUICommand CycleInfo { get; private set; }

    /// <summary>
    /// Команда редактирования серии.
    /// </summary>
    public static RoutedUICommand CycleEdit { get; private set; }

    /// <summary>
    /// Команда удаления серии.
    /// </summary>
    public static RoutedUICommand CycleDelete { get; private set; }

    #endregion

    #region Команды жанров.

    /// <summary>
    /// Команда редактирования жанров.
    /// </summary>
    public static RoutedUICommand Genres { get; private set; }

    /// <summary>
    /// Команда редактирования жанра.
    /// </summary>
    public static RoutedUICommand GenreEdit { get; private set; }

    /// <summary>
    /// Команда удаления жанра.
    /// </summary>
    public static RoutedUICommand GenreDelete { get; private set; }

    #endregion

    #region Команды справки.

    /// <summary>
    /// Команда отображения окна "О программе".
    /// </summary>
    public static RoutedUICommand About { get; private set; }

    #endregion

    /// <summary>
    /// Статический конструктор класса. Инициализирует команды приложения.
    /// </summary>
    static AppCommands()
    {
        // Команды библиотеки".
        Settings = new RoutedUICommand("Настройки...", "Settings", typeof(AppCommands));
        Exit = new RoutedUICommand("Выход", "Exit", typeof(AppCommands));

        // Команды книг.
        AddBook = new RoutedUICommand("Добавить книгу...", "AddBook", typeof(AppCommands));
        FindBooks = new RoutedUICommand("Найти книги в папке...", "FindBooks", typeof(AppCommands));
        Play = new RoutedUICommand("Слушать", "Play", typeof(AppCommands));
        Info = new RoutedUICommand("О книге", "Info", typeof(AppCommands));
        Chapters = new RoutedUICommand("Содержание", "Chapters", typeof(AppCommands));
        Bookmarks = new RoutedUICommand("Закладки", "Bookmarks", typeof(AppCommands));
        NotListen = new RoutedUICommand("Не слушаю", "NotListen", typeof(AppCommands));
        Edit = new RoutedUICommand("Изменить...", "Edit", typeof(AppCommands));
        Delete = new RoutedUICommand("Удалить...", "Delete", typeof(AppCommands));

        // Команды авторов.
        Authors = new RoutedUICommand("Авторы...", "Authors", typeof(AppCommands));
        AuthorInfo = new RoutedUICommand("Об авторе...", "AuthorInfo", typeof(AppCommands));
        AuthorEdit = new RoutedUICommand("Изменить автора...", "AuthorEdit", typeof(AppCommands));
        AuthorDelete = new RoutedUICommand("Удалить автора...", "AuthorDelete", typeof(AppCommands));

        // Команды серий.
        Cycles = new RoutedUICommand("Серии...", "Cycles", typeof(AppCommands));
        CycleInfo = new RoutedUICommand("О серии...", "CycleInfo", typeof(AppCommands));
        CycleEdit = new RoutedUICommand("Изменить серию...", "CycleEdit", typeof(AppCommands));
        CycleDelete = new RoutedUICommand("Удалить серию...", "CycleDelete", typeof(AppCommands));

        // Команды жанров.
        Genres = new RoutedUICommand("Жанры...", "Genres", typeof(AppCommands));
        GenreEdit = new RoutedUICommand("Изменить жанр...", "GenreEdit", typeof(AppCommands));
        GenreDelete = new RoutedUICommand("Удалить жанр...", "GenreDelete", typeof(AppCommands));

        // Команды справки.
        About = new RoutedUICommand("О программе...", "About", typeof(AppCommands));
    }
}
