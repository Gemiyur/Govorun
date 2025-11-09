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
/// Класс окна закладок книги.
/// </summary>
public partial class BookmarksDialog : Window
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
            if (book != value)
                SaveChanged();
            book = value;
            LoadBook();
        }
    }

    /// <summary>
    /// Коллекция закладок книги.
    /// </summary>
    private readonly ObservableCollectionEx<Bookmark> bookmarks = [];

    /// <summary>
    /// Были ли изменения в закладках книги.
    /// </summary>
    private bool hasChanges = false;

    /// <summary>
    /// В редакторе новая закладка?
    /// </summary>
    private bool isNewBookmark;

    /// <summary>
    /// Возвращает выбранную закладку в списке закладок.
    /// </summary>
    private Bookmark SelectedBookmark => (Bookmark)BookmarksListBox.SelectedItem;

    /// <summary>
    /// Инициализирует новый экземпляр класса.
    /// </summary>
    /// <param name="book">Книга.</param>
    public BookmarksDialog(Book book)
    {
        InitializeComponent();
        this.book = book;
        TitleTextBlock.FontSize = FontSize + 2;
        TitleEditor.Header = "Название закладки";
        BookmarksListBox.ItemsSource = bookmarks;
        LoadBook();
        // Подписка в коде потому что при подписке в дизайнере обработчик отрабатывает,
        // но код обработчика отображается в редакторе как неиспользуемый, что не удобно.
        TitleEditor.IsVisibleChanged += TitleEditor_IsVisibleChanged;
    }

    /// <summary>
    /// Проверяет и устанавливает доступность кнопки добавления закладки.
    /// </summary>
    public void CheckAddButton()
    {
        AddButton.IsEnabled = book == App.GetMainWindow().Player.Book;
    }

    /// <summary>
    /// Загружает книгу.
    /// </summary>
    private void LoadBook()
    {
        UpdateAuthorsAndTitle();
        bookmarks.ReplaceRange(book.Bookmarks);
        SortBookmarks();
        CheckAddButton();
        TitleEditor.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// Сортирует закладки в алфавитном порядке.
    /// </summary>
    private void SortBookmarks()
    {
        bookmarks.Sort(x => x.Title, StringComparer.CurrentCultureIgnoreCase);
    }
    
    /// <summary>
    /// Сохраняет изменения в базе данных, если они были.
    /// </summary>
    private void SaveChanged()
    {
        if (hasChanges)
        {
            book.Bookmarks.Clear();
            book.Bookmarks.AddRange(bookmarks);
            Library.UpdateBook(book);
            hasChanges = false;
        }
    }

    /// <summary>
    /// Обновляет имена авторов книги.
    /// </summary>
    public void UpdateAuthors()
    {
        AuthorsTextBlock.Inlines.Clear();
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
        if (Properties.Settings.Default.SaveBookWindowsLocation &&
            App.SizeDefined(Properties.Settings.Default.BookmarksSize))
        {
            Left = Properties.Settings.Default.BookmarksPos.X;
            Top = Properties.Settings.Default.BookmarksPos.Y;
            Width = Properties.Settings.Default.BookmarksSize.Width;
            Height = Properties.Settings.Default.BookmarksSize.Height;
        }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        SaveChanged();
        if (Properties.Settings.Default.SaveBookWindowsLocation)
        {
            Properties.Settings.Default.BookmarksPos = new System.Drawing.Point((int)Left, (int)Top);
            Properties.Settings.Default.BookmarksSize = new System.Drawing.Size((int)Width, (int)Height);
        }
    }

    private void AuthorLink_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Hyperlink)
            return;
        var author = (Author)((Hyperlink)sender).Tag;
        new AuthorInfoDialog(author) { Owner = this }.ShowDialog();
    }

    private void BookmarksListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        PlayButton.IsEnabled = BookmarksListBox.SelectedItems.Count == 1;
        EditButton.IsEnabled = BookmarksListBox.SelectedItems.Count == 1;
        DeleteButton.IsEnabled = BookmarksListBox.SelectedItems.Count > 0;
    }

    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        App.GetMainWindow().PlayBook(book, SelectedBookmark.Position);
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        isNewBookmark = true;
        var bookmark = new Bookmark() { Position = App.GetMainWindow().Player.PlayPosition };
        bookmarks.Add(bookmark);
        BookmarksListBox.SelectedItem = bookmark;
        TitleEditor.Text = SelectedBookmark.Title;
        TitleEditor.Visibility = Visibility.Visible;
        MainGrid.IsEnabled = false;
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        TitleEditor.Text = SelectedBookmark.Title;
        TitleEditor.Visibility = Visibility.Visible;
        MainGrid.IsEnabled = false;
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        bookmarks.RemoveRange(BookmarksListBox.SelectedItems.Cast<Bookmark>());
        hasChanges = true;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void TitleEditor_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (TitleEditor.Visibility != Visibility.Collapsed)
            return;
        if (TitleEditor.Result)
        {
            SelectedBookmark.Title = TitleEditor.Text;
            SortBookmarks();
            hasChanges = true;
        }
        else if (isNewBookmark)
        {
            bookmarks.Remove(SelectedBookmark);
            isNewBookmark = false;
        }
        MainGrid.IsEnabled = true;
    }
}
