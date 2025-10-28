using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Govorun.Models;

namespace Govorun.Dialogs;

/// <summary>
/// Класс окна информации о книге.
/// </summary>
public partial class BookInfoDialog : Window
{
    #region Для запрета разворачивания окна на весь экран.

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    private const int GWL_STYLE = -16;
    private const int WS_MAXIMIZEBOX = 0x10000;

    #endregion

    /// <summary>
    /// Книга.
    /// </summary>
    private Book book;

    /// <summary>
    /// Возвращает или задаёт книгу.
    /// </summary>
    public Book Book
    {
        get => book;
        set
        {
            book = value;
            LoadBook();
        }
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса.
    /// </summary>
    /// <param name="book">Книга.</param>
    public BookInfoDialog(Book book)
    {
        InitializeComponent();
        this.book = book;
        TitleTextBlock.FontSize = FontSize + 2;
        LoadBook();
    }

    /// <summary>
    /// Загружает книгу.
    /// </summary>
    private void LoadBook()
    {
        AuthorsTextBlock.Text = book.AuthorNamesFirstLast;
        TitleTextBlock.Text = book.Title;
        if (book.Cycle != null)
        {
            CycleGrid.Visibility = Visibility.Visible;
            CycleTitleTextBlock.Text = book.Cycle.Title;
            if (book.CycleNumbers.Length > 0)
            {
                CycleNumbersStackPanel.Visibility = Visibility.Visible;
                CycleNumbersTextBlock.Text = book.CycleNumbers;
            }
            else
                CycleNumbersStackPanel.Visibility = Visibility.Collapsed;
        }
        else
        {
            CycleGrid.Visibility = Visibility.Collapsed;
        }
        LectorTextBlock.Text = book.Lector.Length > 0 ? book.Lector : "(не указано)";
        DurationTextBlock.Text = book.DurationText;
        if (book.Translator.Length > 0)
        {
            TranslatorStackPanel.Visibility = Visibility.Visible;
            TranslatorTextBlock.Text = book.Translator;
        }
        else
        {
            TranslatorStackPanel.Visibility = Visibility.Collapsed;
        }
        if (book.Genres.Count > 0)
        {
            GenressGrid.Visibility = Visibility.Visible;
            GenresTextBlock.Text =
                App.ListToString(book.Genres, "; ", x => ((Genre)x).Title, StringComparer.CurrentCultureIgnoreCase);
        }
        else
        {
            GenressGrid.Visibility = Visibility.Collapsed;
        }
        AnnotationTextBox.Text = book.Annotation;
        FileTextBox.Text = book.FileName;
    }

    /// <summary>
    /// Обновляет данные книги в окне.
    /// </summary>
    public void UpdateBook()
    {
        LoadBook();
    }

    /// <summary>
    /// Обновляет имя файла книги в окне.
    /// </summary>
    public void UpdateFile()
    {
        FileTextBox.Text = book.FileName;
    }

    private void Window_SourceInitialized(object sender, EventArgs e)
    {
        IntPtr handle = new WindowInteropHelper(this).Handle;
        _ = SetWindowLong(handle, GWL_STYLE, GetWindowLong(handle, GWL_STYLE) & ~WS_MAXIMIZEBOX);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (Properties.Settings.Default.SaveBookWindowsLocation &&
            App.SizeDefined(Properties.Settings.Default.BookInfoSize))
        {
            Left = Properties.Settings.Default.BookInfoPos.X;
            Top = Properties.Settings.Default.BookInfoPos.Y;
            Width = Properties.Settings.Default.BookInfoSize.Width;
            Height = Properties.Settings.Default.BookInfoSize.Height;
        }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        if (Properties.Settings.Default.SaveBookWindowsLocation)
        {
            Properties.Settings.Default.BookInfoPos = new System.Drawing.Point((int)Left, (int)Top);
            Properties.Settings.Default.BookInfoSize = new System.Drawing.Size((int)Width, (int)Height);
        }
    }

    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        App.GetMainWindow().PlayBook(book);
    }

    private void ChaptersButton_Click(object sender, RoutedEventArgs e)
    {
        App.GetMainWindow().ShowChapters(book);
    }

    private void BookmarksButton_Click(object sender, RoutedEventArgs e)
    {
        App.GetMainWindow().ShowBookmarks(book);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
