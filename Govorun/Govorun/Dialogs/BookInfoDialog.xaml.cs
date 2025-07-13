using System.Windows;
using Govorun.Models;

namespace Govorun.Dialogs;

/// <summary>
/// Класс окна информации о книге.
/// </summary>
public partial class BookInfoDialog : Window
{
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
            if (book.CyclePart.Length > 0)
            {
                CycleNumberStackPanel.Visibility = Visibility.Visible;
                CycleNumberTextBlock.Text = book.CyclePart;
            }
            else
                CycleNumberStackPanel.Visibility = Visibility.Collapsed;
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
        if (book.Tags.Count > 0)
        {
            TagsGrid.Visibility = Visibility.Visible;
            TagsTextBlock.Text =
                App.ListToString(book.Tags, "; ", x => ((Tag)x).Title, StringComparer.CurrentCultureIgnoreCase);
        }
        else
        {
            TagsGrid.Visibility = Visibility.Collapsed;
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
        App.GetMainWindow().Activate();
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
