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

    public BookmarksDialog(Book book)
    {
        InitializeComponent();
        this.book = book;
        TitleTextBlock.FontSize = FontSize + 2;
        AuthorsTextBlock.Text = book.AuthorNamesFirstLast;
        TitleTextBlock.Text = book.Title;
        bookmarks.AddRange(book.Bookmarks.OrderBy(x => x.Title));
        BookmarksListBox.ItemsSource = bookmarks;
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
        Bookmark = (Bookmark)BookmarksListBox.SelectedItem;
        DialogResult = true;
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        var bookmark = (Bookmark)BookmarksListBox.SelectedItem;
        var title = bookmark.Title;
        var editor = new BookmarkEditor(title) { Owner = this };
        if (editor.ShowDialog() != true)
            return;
        bookmark.Title = editor.BookmarkTitle;
        hasChanges = true;
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
}
