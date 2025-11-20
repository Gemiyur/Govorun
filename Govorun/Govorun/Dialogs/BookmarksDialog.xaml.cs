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
            book = value;
            LoadBook();
        }
    }

    /// <summary>
    /// Коллекция закладок книги.
    /// </summary>
    private readonly ObservableCollectionEx<Bookmark> bookmarks = [];

    /// <summary>
    /// Закладка в редакторе.
    /// </summary>
    private Bookmark? bookmark;

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
        bookmark = new Bookmark() { Position = App.GetMainWindow().Player.PlayPosition };
        TitleEditor.Text = string.Empty;
        TitleEditor.Visibility = Visibility.Visible;
        MainGrid.IsEnabled = false;
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        isNewBookmark = false;
        bookmark = SelectedBookmark;
        TitleEditor.Text = bookmark.Title;
        TitleEditor.Visibility = Visibility.Visible;
        MainGrid.IsEnabled = false;
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (!App.ConfirmAction("Удалить выбранные закладки?", Title))
            return;
        var removed = BookmarksListBox.SelectedItems.Cast<Bookmark>();
        foreach (var item in removed)
        {
            book.Bookmarks.Remove(item);
        }
        if (Library.UpdateBook(book))
        {
            bookmarks.RemoveRange(removed);
        }
        else
        {
            MessageBox.Show("Не удалось удалить закладки.", Title);
            book.Bookmarks.AddRange(removed);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void TitleEditor_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (TitleEditor.Visibility != Visibility.Collapsed)
            return;
        if (!TitleEditor.Result || bookmark == null)
        {
            MainGrid.IsEnabled = true;
            return;
        }
        if (isNewBookmark)
        {
            bookmark.Title = TitleEditor.Text;
            book.Bookmarks.Add(bookmark);
            if (Library.UpdateBook(book))
            {
                bookmarks.Add(bookmark);
                SortBookmarks();
                BookmarksListBox.SelectedItem = bookmark;
            }
            else
            {
                MessageBox.Show("Не удалось добавить закладку.", Title);
                book.Bookmarks.Remove(bookmark);
            }
            isNewBookmark = false;
        }
        else
        {
            var oldTitle = bookmark.Title;
            bookmark.Title = TitleEditor.Text;
            if (Library.UpdateBook(book))
            {
                SortBookmarks();
                BookmarksListBox.SelectedItem = bookmark;
            }
            else
            {
                MessageBox.Show("Не удалось сохранить закладку.", Title);
                bookmark.Title = oldTitle;
            }
        }
        bookmark = null;
        MainGrid.IsEnabled = true;
    }
}
