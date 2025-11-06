using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using Govorun.Dialogs;
using Govorun.Media;
using Govorun.Models;
using Govorun.Tools;

namespace Govorun;

/// <summary>
/// Класс приложения.
/// </summary>
public partial class App : Application
{
    #region Запуск только одного экземпляра приложения.

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr handle, int cmdShow);

    [DllImport("user32.dll")]
    private static extern int SetForegroundWindow(IntPtr handle);

    private readonly Mutex mutex = new(false, "Govorun");

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        if (!mutex.WaitOne(500, false))
        {
#if DEBUG
            MessageBox.Show("Приложение уже запущено.", "Говорун");
#endif
            var processName = Process.GetCurrentProcess().ProcessName;
            var process = Process.GetProcesses().Where(p => p.ProcessName == processName).FirstOrDefault();
            if (process != null)
            {
                IntPtr handle = process.MainWindowHandle;
                ShowWindow(handle, 1);
                if (SetForegroundWindow(handle) == 0)
                    MessageBox.Show("Приложение уже запущено.", "Говорун");
                Environment.Exit(0);
            }
        }
    }

    #endregion

    /// <summary>
    /// Имя файла базы данных с полным путём.
    /// </summary>
    public static string? DbName { get; set; }

    /// <summary>
    /// Закрывает открытые окна указанной книги.
    /// </summary>
    /// <param name="book">Книга.</param>
    public static void CloseBookWindows(Book book)
    {
        var bookInfoWindow = FindBookInfoWindow();
        if (bookInfoWindow != null && bookInfoWindow.Book == book)
            bookInfoWindow.Close();
        var bookmarksWindow = FindBookmarksWindow();
        if (bookmarksWindow != null && bookmarksWindow.Book == book)
            bookmarksWindow.Close();
        var chaptersWindow = FindChaptersWindow();
        if (chaptersWindow != null && chaptersWindow.Book == book)
            chaptersWindow.Close();
    }

    /// <summary>
    /// Отображает окно сообщения подтверждения операции.
    /// </summary>
    /// <param name="message">Сообщение.</param>
    /// <param name="caption">Заголовок окна.</param>
    /// <returns>Была ли подтверждена операция.</returns>
    /// <remarks>
    /// Это обёртка для MessageBox.Show с кнопками Да и Нет.
    /// </remarks>
    public static bool ConfirmAction(string message, string caption) =>
        MessageBox.Show(message, caption, MessageBoxButton.YesNo) == MessageBoxResult.Yes;

    /// <summary>
    /// Возвращает BitmapImage из указанного файла изображения.
    /// </summary>
    /// <param name="path">Путь к файлу.</param>
    /// <returns>BitmapImage.</returns>
    public static BitmapImage GetBitmapImage(string path) => new(new Uri(path, UriKind.Relative));

    /// <summary>
    /// Возвращает книгу из указанного файла. В выходной параметр trackData возвращаются данные трека.
    /// </summary>
    /// <param name="filename">Имя файла книги с полным путём.</param>
    /// <param name="trackData">Данные трека.</param>
    /// <returns>Книга.</returns>
    public static Book GetBookFromFile(string filename, out TrackData trackData)
    {
        var book = new Book();
        trackData = new TrackData(filename);
        book.Title = trackData.Title;
        book.FileName = filename;
        book.Duration = trackData.Duration;
        book.FileSize = new FileInfo(filename).Length;
        foreach (var chapter in trackData.Chapters)
        {
            book.Chapters.Add(new Chapter()
            {
                Title = chapter.Title,
                StartTime = chapter.StartTime,
                EndTime = chapter.EndTime
            });
        }
        return book;
    }

    /// <summary>
    /// Восстанавливает состояние указанного окна из свёрнутого.
    /// </summary>
    /// <param name="window">Окно.</param>
    public static void RestoreWindow(Window window)
    {
        if (window.WindowState != WindowState.Normal)
        {
            window.WindowState = WindowState.Normal;
        }
    }

    /// <summary>
    /// Восстанавливает состояние указанного окна в нормальное.
    /// </summary>
    /// <param name="window">Окно.</param>
    public static void RestoreWindowNormal(Window window)
    {
        if (window.WindowState != WindowState.Normal)
        {
            window.WindowState = WindowState.Normal;
            // Вторая установка для приведения окна в Normal, если оно было Maximized перед сворачиванием.
            window.WindowState = WindowState.Normal;
        }
    }

    /// <summary>
    /// Отображает окно информации о книге.
    /// </summary>
    /// <param name="book">Книга.</param>
    public static void ShowBookInfo(Book book)
    {
        var window = FindBookInfoWindow();
        if (window != null)
        {
            if (window.Book != book)
                window.Book = book;
            RestoreWindow(window);
            window.Activate();
        }
        else
        {
            new BookInfoDialog(book).Show();
        }
    }

    /// <summary>
    /// Отображает окно закладок указанной книги.
    /// </summary>
    /// <param name="book">Книга.</param>
    public static void ShowBookmarks(Book book)
    {
        var window = FindBookmarksWindow();
        if (window != null)
        {
            if (window.Book != book)
                window.Book = book;
            RestoreWindow(window);
            window.Activate();
        }
        else
        {
            new BookmarksDialog(book).Show();
        }
    }

    /// <summary>
    /// Отображает окно содержания указанной книги.
    /// </summary>
    /// <param name="book">Книга.</param>
    public static void ShowChapters(Book book)
    {
        var window = FindChaptersWindow();
        if (window != null)
        {
            if (window.Book != book)
                window.Book = book;
            else if (window.Book == GetMainWindow().Player.Book)
                window.SelectCurrentChapter();
            RestoreWindow(window);
            window.Activate();
        }
        else
        {
            new ChaptersDialog(book).Show();
        }
    }

    /// <summary>
    /// Возвращает задан ли указанный размер.
    /// </summary>
    /// <remarks>Возвращает true если высота и ширина больше нуля.</remarks>
    /// <param name="size">Размер.</param>
    /// <returns>Задан ли указанный размер.</returns>
    public static bool SizeDefined(System.Drawing.Size size) => size.Width > 0 && size.Height > 0;

    #region Редактирование и удаление авторов, серий и жанров.

    public static void EditGenre(Genre genre, Window? editorOwner)
    {
        var editor = new GenreEditor(genre) { Owner = editorOwner };
        if (editor.ShowDialog() != true)
            return;
        GetMainWindow().UpdateNavPanel(false, false, true);
        var bookInfoWindow = FindBookInfoWindow();
        if (bookInfoWindow != null && Library.BookHasGenre(bookInfoWindow.Book, genre.GenreId))
            bookInfoWindow.UpdateGenres();
    }

    #endregion

    #region Получение окон приложения.

    /// <summary>
    /// Возвращает главное окно приложения.
    /// </summary>
    /// <returns>Главное окно приложения.</returns>
    public static MainWindow GetMainWindow() => (MainWindow)Current.MainWindow;

    /// <summary>
    /// Возвращает окно информации о книге или null, если окна нет.
    /// </summary>
    /// <returns>Окно информации о книге или null, если окна нет.</returns>
    public static BookInfoDialog? FindBookInfoWindow()
    {
        foreach (var window in Current.Windows)
            if (window is BookInfoDialog bookInfoWindow)
                return bookInfoWindow;
        return null;
    }

    /// <summary>
    /// Возвращает окно закладок или null, если окна нет.
    /// </summary>
    /// <returns>Окно закладок или null, если окна нет.</returns>
    public static BookmarksDialog? FindBookmarksWindow()
    {
        foreach (var window in Current.Windows)
            if (window is BookmarksDialog bookmarksWindow)
                return bookmarksWindow;
        return null;
    }

    /// <summary>
    /// Возвращает окно содержания или null, если окна нет.
    /// </summary>
    /// <returns>Окно содержания или null, если окна нет.</returns>
    public static ChaptersDialog? FindChaptersWindow()
    {
        foreach (var window in Current.Windows)
            if (window is ChaptersDialog chaptersWindow)
                return chaptersWindow;
        return null;
    }

    #endregion

    #region Диалоги выбора файла и папки книг.

    /// <summary>
    /// Возвращает диалог выбора файла книги.
    /// </summary>
    public static OpenFileDialog PickBookFileDialog => new()
    {
        AddToRecent = false,
        Title = "Выбрать файл книги",
        Filter = $"Файлы книг|*.m4b"
    };

    /// <summary>
    /// Возвращает диалог выбора папки с файлами книг.
    /// </summary>
    public static OpenFolderDialog PickBooksFolderDialog => new()
    {
        AddToRecent = false,
        Multiselect = true,
        Title = "Выбрать папку с файлами книг",
    };

    /// <summary>
    /// Возвращает диалог выбора файла базы данных.
    /// </summary>
    public static SaveFileDialog PickDatabaseDialog => new()
    {
        AddToRecent = false,
        CheckFileExists = false,
        OverwritePrompt = false,
        Title = "Файл базы данных",
        Filter = $"Файлы базы данных|*{Db.DbExtension}"
    };

    #endregion

    #region Получение строковых представлений значений.

    /// <summary>
    /// Возвращает строковое представление указанного логического значения на русском языке.
    /// </summary>
    /// <param name="value">Логическое значение.</param>
    /// <returns>Строковое представление логического значения на русском языке.</returns>
    public static string BoolToString(bool value) => value ? "Да" : "Нет";

    /// <summary>
    /// Возвращает строку, содержащую строки списка с указанным разделителем.
    /// </summary>
    /// <param name="list">Список строк.</param>
    /// <param name="separator">Разделитель.</param>
    /// <returns>Строка, содержащая строки списка с указанным разделителем.</returns>
    public static string ListToString(IEnumerable<string> list, string separator)
    {
        var sb = new StringBuilder();
        foreach (var item in list)
            sb.Append(item == list.First() ? item : separator + item);
        return sb.ToString();
    }

    /// <summary>
    /// Возвращает строку строк, извлечённых из списка объектов, с указанным разделителем.
    /// </summary>
    /// <param name="list">Список объектов.</param>
    /// <param name="separator">Разделитель.</param>
    /// <param name="stringSelector">Функция извлечения строки из объекта.</param>
    /// <returns>Строка строк, извлечённых из списка объектов, с указанным разделителем.</returns>
    public static string ListToString(IEnumerable<object> list, string separator, Func<object, string> stringSelector)
    {
        var sb = new StringBuilder();
        foreach (var item in list)
            sb.Append(item == list.First() ? stringSelector(item) : separator + stringSelector(item));
        return sb.ToString();
    }

    /// <summary>
    /// Возвращает строку отсортированных строк, извлечённых из списка объектов, с указанным разделителем.
    /// </summary>
    /// <param name="list">Список объектов.</param>
    /// <param name="separator">Разделитель.</param>
    /// <param name="stringSelector">Функция извлечения строки из объекта.</param>
    /// <param name="comparer">Компаратор строк.</param>
    /// <returns>Строка отсортированных строк, извлечённых из списка объектов, с указанным разделителем.</returns>
    public static string ListToString(IEnumerable<object> list, string separator,
                                      Func<object, string> stringSelector, IComparer<string> comparer)
    {
        var strings = list.Select(item => stringSelector(item)).ToList();
        strings.Sort(comparer);
        return ListToString(strings, separator);
    }

    /// <summary>
    /// Возвращает строковое представление указанного интервала времени в формате h:mm:ss.
    /// </summary>
    /// <param name="timeSpan">Интервал времени.</param>
    /// <returns>Строковое представление интервала времени.</returns>
    public static string TimeSpanToString(TimeSpan timeSpan) => timeSpan.ToString(@"h\:mm\:ss");

    #endregion
}
