using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Documents;
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
            UpdateBook();
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
        UpdateBook();
    }

    /// <summary>
    /// Обновляет данные книги.
    /// </summary>
    public void UpdateBook()
    {
        UpdateAuthors();
        UpdateTitle();
        UpdateCycle();
        UpdateDuration();
        UpdateLector();
        UpdateTranslator();
        UpdateGenres();
        UpdateAnnotation();
        UpdateFile();
    }

    /// <summary>
    /// Обновляет имена авторов книги.
    /// </summary>
    public void UpdateAuthors()
    {
        for (int i = 0; i < book.Authors.Count; i++)
        {
            var run = new Run(book.Authors[i].NameFirstLast);
            //var run = Properties.Settings.Default.BookInfoAuthorFullName
            //    ? new Run(book.Authors[i].NameFirstMiddleLast)
            //    : new Run(book.Authors[i].NameFirstLast);
            var link = new Hyperlink(run);
            link.Tag = book.Authors[i];
            link.Style = (Style)FindResource("HyperlinkStyle");
            link.Click += AuthorLink_Click;
            AuthorsTextBlock.Inlines.Add(link);
            if (i < book.Authors.Count - 1)
                AuthorsTextBlock.Inlines.Add(new Run(", "));
        }
    }

    /// <summary>
    /// Обновляет название книги.
    /// </summary>
    public void UpdateTitle()
    {
        TitleTextBlock.Text = book.Title;
    }

    /// <summary>
    /// Обновляет имена авторов и название книги.
    /// </summary>
    public void UpdateAuthorsAndTitle()
    {
        UpdateAuthors();
        UpdateTitle();
    }

    /// <summary>
    /// Обновляет серию книги.
    /// </summary>
    public void UpdateCycle()
    {
        if (book.Cycle != null)
        {
            CycleGrid.Visibility = Visibility.Visible;
            var link = new Hyperlink(new Run(book.Cycle.Title));
            link.Tag = book.Cycle;
            link.Style = (Style)FindResource("HyperlinkStyle");
            link.Click += CycleLink_Click;
            CycleTitleTextBlock.Inlines.Add(link);
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
    }

    /// <summary>
    /// Обновляет продолжительность книги.
    /// </summary>
    public void UpdateDuration()
    {
        DurationTextBlock.Text = book.DurationText;
    }

    /// <summary>
    /// Обновляет чтеца книги.
    /// </summary>
    public void UpdateLector()
    {
        LectorTextBlock.Text = book.Lector.Length > 0 ? book.Lector : "(не указано)";
    }

    /// <summary>
    /// Обновляет переводчика книги.
    /// </summary>
    public void UpdateTranslator()
    {
        if (book.Translator.Length > 0)
        {
            TranslatorStackPanel.Visibility = Visibility.Visible;
            TranslatorTextBlock.Text = book.Translator;
        }
        else
        {
            TranslatorStackPanel.Visibility = Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Обновляет жанры книги.
    /// </summary>
    public void UpdateGenres()
    {
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
    }

    /// <summary>
    /// Обновляет аннотацию к книге.
    /// </summary>
    public void UpdateAnnotation()
    {
        AnnotationTextBox.Text = book.Annotation;
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

    private void AuthorLink_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Hyperlink)
            return;
        var author = (Author)((Hyperlink)sender).Tag;
        new AuthorInfoDialog(author) { Owner = this }.ShowDialog();
    }

    private void CycleLink_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Hyperlink)
            return;
        var cycle = (Cycle)((Hyperlink)sender).Tag;
        new CycleInfoDialog(cycle) { Owner = this }.ShowDialog();
    }

    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        App.GetMainWindow().PlayBook(book);
    }

    private void ChaptersButton_Click(object sender, RoutedEventArgs e)
    {
        App.ShowChapters(book);
    }

    private void BookmarksButton_Click(object sender, RoutedEventArgs e)
    {
        App.ShowBookmarks(book);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
