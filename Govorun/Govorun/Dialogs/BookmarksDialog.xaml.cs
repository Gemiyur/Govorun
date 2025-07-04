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
    /// Выбранная закладка книги.
    /// </summary>
    public Bookmark? Bookmark;

    /// <summary>
    /// Книга.
    /// </summary>
    private readonly Book book;

    /// <summary>
    /// Коллекция закладок книги.
    /// </summary>
    private readonly ObservableCollectionEx<Bookmark> bookmarks = [];

    /// <summary>
    /// Были ли изменения в закладках книги.
    /// </summary>
    private bool hasChanges = false;

    /// <summary>
    /// Возвращает выбранную закладку в списке закладок.
    /// </summary>
    private Bookmark SelectedBookmark => (Bookmark)BookmarksListBox.SelectedItem;

    public BookmarksDialog(Book book)
    {
        InitializeComponent();
        this.book = book;
        TitleTextBlock.FontSize = FontSize + 2;
        AuthorsTextBlock.Text = book.AuthorNamesFirstLast;
        TitleTextBlock.Text = book.Title;
        bookmarks.AddRange(book.Bookmarks.OrderBy(x => x.Title));
        BookmarksListBox.ItemsSource = bookmarks;
        TitleEditor.Header = "Название закладки";
        TitleEditor.Visibility = Visibility.Collapsed;
        // Подписка в коде потому что при подписке в дизайнере обработчик отрабатывает,
        // но код обработчика отображается в редакторе как неиспользуемый, что не удобно.
        TitleEditor.IsVisibleChanged += TitleEditor_IsVisibleChanged;
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        if (!hasChanges)
            return;
        book.Bookmarks.Clear();
        book.Bookmarks.AddRange(bookmarks);
        Db.UpdateBook(book);
    }

    private void BookmarksListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        PlayButton.IsEnabled = BookmarksListBox.SelectedItems.Count == 1;
        EditButton.IsEnabled = BookmarksListBox.SelectedItems.Count == 1;
        DeleteButton.IsEnabled = BookmarksListBox.SelectedItems.Count > 0;
    }

    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        Bookmark = SelectedBookmark;
        DialogResult = true;
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
            hasChanges = true;
        }
        MainGrid.IsEnabled = true;
    }
}
