using System.Windows.Input;

namespace Govorun;

/// <summary>
/// Статический класс команд приложения.
/// </summary>
public static class AppCommands
{
    #region Команды группы "Книга".

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
    /// Команда редактирования данных книги.
    /// </summary>
    public static RoutedUICommand Edit { get; private set; }

    /// <summary>
    /// Команда удаления выбранных книг.
    /// </summary>
    public static RoutedUICommand Delete { get; private set; }

    /// <summary>
    /// Команда выхода из приложения.
    /// </summary>
    public static RoutedUICommand Exit { get; private set; }

    #endregion

    #region Команды группы "Библиотека".

    /// <summary>
    /// Команда добавления книги в библиотеку.
    /// </summary>
    public static RoutedUICommand AddBook { get; private set; }

    /// <summary>
    /// Команда поиска книг в папке.
    /// </summary>
    public static RoutedUICommand FindBooks { get; private set; }

    /// <summary>
    /// Команда редактирования авторов.
    /// </summary>
    public static RoutedUICommand Authors { get; private set; }

    /// <summary>
    /// Команда редактирования серий.
    /// </summary>
    public static RoutedUICommand Cycles { get; private set; }

    /// <summary>
    /// Команда проверки библиотеки на наличие файлов книг.
    /// </summary>
    public static RoutedUICommand CheckLibrary { get; private set; }

    /// <summary>
    /// Команда сжатия библиотеки.
    /// </summary>
    public static RoutedUICommand Shrink { get; private set; }

    /// <summary>
    /// Команда настроек приложения.
    /// </summary>
    public static RoutedUICommand Settings { get; private set; }

    #endregion

    #region Команды группы "Инструменты".

    /// <summary>
    /// Команда создания файла M4B из файлов MP3.
    /// </summary>
    public static RoutedUICommand CreateM4B { get; private set; }

    #endregion

    #region Команды группы "Справка".

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
        // Команды группы "Книга".
        Play = new RoutedUICommand("Слушать", "Play", typeof(AppCommands));
        Info = new RoutedUICommand("О книге...", "Info", typeof(AppCommands));
        Chapters = new RoutedUICommand("Содержание...", "Chapters", typeof(AppCommands));
        Bookmarks = new RoutedUICommand("Закладки...", "Bookmarks", typeof(AppCommands));
        Edit = new RoutedUICommand("Изменить...", "Edit", typeof(AppCommands));
        Delete = new RoutedUICommand("Удалить...", "Delete", typeof(AppCommands));
        Exit = new RoutedUICommand("Выход...", "Exit", typeof(AppCommands));

        // Команды группы "Библиотека".
        AddBook = new RoutedUICommand("Добавить книгу...", "AddBook", typeof(AppCommands));
        FindBooks = new RoutedUICommand("Найти книги в папке...", "FindBooks", typeof(AppCommands));
        Authors = new RoutedUICommand("Редактор авторов...", "Authors", typeof(AppCommands));
        Cycles = new RoutedUICommand("Редактор серий...", "Cycles", typeof(AppCommands));
        CheckLibrary = new RoutedUICommand("Проверить библиотеку...", "CheckLibrary", typeof(AppCommands));
        Shrink = new RoutedUICommand("Сжать библиотеку...", "Shrink", typeof(AppCommands));
        Settings = new RoutedUICommand("Настройки...", "Settings", typeof(AppCommands));

        // Команды группы "Инструменты".
        CreateM4B = new RoutedUICommand("Создать файл M4B из файлов MP3", "CreateM4B", typeof(AppCommands));

        // Команды группы "Справка"
        About = new RoutedUICommand("О программе...", "About", typeof(AppCommands));
    }
}
