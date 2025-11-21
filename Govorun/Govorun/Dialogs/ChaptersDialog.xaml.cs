using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interop;
using Gemiyur.Collections;
using Govorun.Models;
using Govorun.Tools;

namespace Govorun.Dialogs;

/// <summary>
/// Класс окна содержания книги.
/// </summary>
public partial class ChaptersDialog : Window
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
    /// Коллекция глав книги.
    /// </summary>
    private readonly ObservableCollectionEx<Chapter> chapters = [];

    /// <summary>
    /// Глава в редакторе.
    /// </summary>
    private Chapter? chapter;

    /// <summary>
    /// Возвращает выбранную главу в списке глав.
    /// </summary>
    private Chapter SelectedChapter => (Chapter)ChaptersListView.SelectedItem;

    /// <summary>
    /// Инициализирует новый экземпляр класса.
    /// </summary>
    /// <param name="book">Книга.</param>
    public ChaptersDialog(Book book)
    {
        InitializeComponent();
        this.book = book;
        TitleTextBlock.FontSize = FontSize + 2;
        TitleEditor.Header = "Название главы";
        ChaptersListView.ItemsSource = chapters;
        LoadBook();
        // Подписка в коде потому что при подписке в дизайнере обработчик отрабатывает,
        // но код обработчика отображается в редакторе как неиспользуемый, что не удобно.
        TitleEditor.IsVisibleChanged += TitleEditor_IsVisibleChanged;
    }

    /// <summary>
    /// Загружает книгу.
    /// </summary>
    private void LoadBook()
    {
        UpdateAuthorsAndTitle();
        chapters.ReplaceRange(book.Chapters);
        TitleEditor.Visibility = Visibility.Collapsed;
        if (IsInitialized)
            SelectCurrentChapter();
    }

    /// <summary>
    /// Выделяет в списке глав текущую (слушаемую).
    /// </summary>
    public void SelectCurrentChapter()
    {
        if (!book.Listening)
            return;
        var player = App.GetMainWindow().Player;
        var position = player.Book == book ? player.PlayPosition : book.PlayPosition;
        ChaptersListView.SelectedItem = chapters.FirstOrDefault(x => x.StartTime <= position && x.EndTime > position);
        ChaptersListView.ScrollIntoView(ChaptersListView.SelectedItem);
        CurrentButton.IsEnabled = book.Listening;
    }

    /// <summary>
    /// Устанавливает состояние элементов для не слушаемой книги.
    /// </summary>
    public void SetNotListening()
    {
        CurrentButton.IsEnabled = false;
        ChaptersListView.SelectedItem = null;
    }

    /// <summary>
    /// Обновляет имена авторов книги.
    /// </summary>
    public void UpdateAuthors()
    {
        var authors = book.Authors.OrderBy(x => x.NameLastFirstMiddle).Cast<Author>().ToList();
        AuthorsTextBlock.Inlines.Clear();
        for (int i = 0; i < authors.Count; i++)
        {
            var run = new Run(authors[i].NameFirstLast);
            //var run = Properties.Settings.Default.BookInfoAuthorFullName
            //    ? new Run(authors[i].NameFirstMiddleLast)
            //    : new Run(authors[i].NameFirstLast);
            var link = new Hyperlink(run);
            link.Tag = authors[i];
            link.Style = (Style)FindResource("HyperlinkStyle");
            link.Click += AuthorLink_Click;
            AuthorsTextBlock.Inlines.Add(link);
            if (i < authors.Count - 1)
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
    /// Обновляет данные книги в окне.
    /// </summary>
    /// <remarks>Обновляет только авторов и название книги.</remarks>
    public void UpdateAuthorsAndTitle()
    {
        UpdateAuthors();
        UpdateTitle();
    }

    private void Window_SourceInitialized(object sender, EventArgs e)
    {
        IntPtr handle = new WindowInteropHelper(this).Handle;
        _ = SetWindowLong(handle, GWL_STYLE, GetWindowLong(handle, GWL_STYLE) & ~WS_MAXIMIZEBOX);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (Properties.Settings.Default.SaveChaptersWindowLocation &&
            App.SizeDefined(Properties.Settings.Default.ChaptersSize))
        {
            Left = Properties.Settings.Default.ChaptersPos.X;
            Top = Properties.Settings.Default.ChaptersPos.Y;
            Width = Properties.Settings.Default.ChaptersSize.Width;
            Height = Properties.Settings.Default.ChaptersSize.Height;
        }
        SelectCurrentChapter();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        if (Properties.Settings.Default.SaveChaptersWindowLocation)
        {
            Properties.Settings.Default.ChaptersPos = new System.Drawing.Point((int)Left, (int)Top);
            Properties.Settings.Default.ChaptersSize = new System.Drawing.Size((int)Width, (int)Height);
        }
    }

    private void AuthorLink_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Hyperlink)
            return;
        var author = (Author)((Hyperlink)sender).Tag;
        new AuthorInfoDialog(author) { Owner = this }.ShowDialog();
    }

    private void ChaptersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        PlayButton.IsEnabled = ChaptersListView.SelectedItems.Count == 1;
        CurrentButton.IsEnabled = book.Listening;
        EditButton.IsEnabled = ChaptersListView.SelectedItems.Count == 1;
    }

    private void ChaptersListView_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        var listView = (ListView)sender;
        var gridView = (GridView)listView.View;
        var totalWidth = listView.ActualWidth - (SystemParameters.VerticalScrollBarWidth + 10);
        var usedWidth = 0.0;
        for (var i = 1; i < gridView.Columns.Count; i++)
        {
            usedWidth += gridView.Columns[i].Width;
        }
        gridView.Columns[0].Width = totalWidth - usedWidth;
    }

    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        App.GetMainWindow().PlayBook(book, SelectedChapter.StartTime);
        CurrentButton.IsEnabled = true;
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        chapter = SelectedChapter;
        TitleEditor.Text = chapter.Title;
        TitleEditor.Visibility = Visibility.Visible;
        MainGrid.IsEnabled = false;
    }

    private void CurrentButton_Click(object sender, RoutedEventArgs e)
    {
        SelectCurrentChapter();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void TitleEditor_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (TitleEditor.Visibility != Visibility.Collapsed)
            return;
        if (!TitleEditor.Result || chapter == null)
        {
            MainGrid.IsEnabled = true;
            return;
        }
        var oldTitle = chapter.Title;
        chapter.Title = TitleEditor.Text;
        if (!Library.UpdateBook(book))
        {
            MessageBox.Show("Не удалось сохранить название главы.", Title);
            chapter.Title = oldTitle;
        }
        chapter = null;
        MainGrid.IsEnabled = true;
    }
}
