using System.Windows;
using System.Windows.Controls;
using Gemiyur.Collections;
using Govorun.Models;
using Govorun.Tools;

namespace Govorun.Dialogs;

/// <summary>
/// Класс окна закладок книги.
/// </summary>
public partial class BookmarksDialog : Window
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
        AuthorsTextBlock.Text = book.AuthorNamesFirstLast;
        TitleTextBlock.Text = book.Title;
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
            Db.UpdateBook(book);
            hasChanges = false;
        }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        SaveChanged();
        App.GetMainWindow().Activate();
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
