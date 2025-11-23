using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Gemiyur.Collections;
using Govorun.Models;
using Govorun.Tools;

namespace Govorun.Dialogs;

/// <summary>
/// Класс окна проверки библиотеки.
/// </summary>
public partial class CheckLibraryDialog : Window
{
    #region Для запрета сворачивания и разворачивания окна на весь экран.

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    private const int GWL_STYLE = -16;
    private const int WS_MAXIMIZEBOX = 0x10000;
    private const int WS_MINIMIZEBOX = 0x20000;

    #endregion

    /// <summary>
    /// Коллекция книг у которых не найден файл.
    /// </summary>
    private readonly ObservableCollectionEx<Book> books = [];

    public CheckLibraryDialog(IEnumerable<Book> books)
    {
        InitializeComponent();
        this.books.AddRange(books);
        BooksListView.ItemsSource = this.books;
    }

    private void Window_SourceInitialized(object sender, EventArgs e)
    {
        IntPtr handle = new WindowInteropHelper(this).Handle;
        _ = SetWindowLong(handle, GWL_STYLE, GetWindowLong(handle, GWL_STYLE) & ~WS_MINIMIZEBOX);
        _ = SetWindowLong(handle, GWL_STYLE, GetWindowLong(handle, GWL_STYLE) & ~WS_MAXIMIZEBOX);
    }

    private void BooksListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        FileButton.IsEnabled = BooksListView.SelectedItem != null;
        DeleteButton.IsEnabled = BooksListView.SelectedItem != null;
    }

    private void BooksListView_SizeChanged(object sender, SizeChangedEventArgs e)
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

    private void FileButton_Click(object sender, RoutedEventArgs e)
    {
        var book = (Book)BooksListView.SelectedItem;
        var dialog = new BookFileDialog(book) { Owner = this };
        if (dialog.ShowDialog() != true)
            return;
        var origFileName = book.FileName;
        book.FileName = dialog.FileName;
        if (!Library.UpdateBook(book))
        {
            MessageBox.Show("Не удалась обновить файл книги.", Title);
            book.FileName = origFileName;
            return;
        }
        books.Remove(book);
        var bookInfoWindow = App.FindBookInfoWindow();
        if (bookInfoWindow != null && bookInfoWindow.Book == book)
            bookInfoWindow.UpdateFile();
        var player = App.GetMainWindow().Player;
        if (player.Book == book)
        {
            player.PlayOnLoad = false;
            player.Book = book;
        }
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        var book = (Book)BooksListView.SelectedItem;
        if (!Library.DeleteBook(book))
        {
            MessageBox.Show("Не удалась удалить книгу из библиотеки.", Title);
            return;
        }
        books.Remove(book);
        var mainWindow = App.GetMainWindow();
        mainWindow.UpdateShownBooks();
        if (mainWindow.Player.Book == book)
            mainWindow.Player.Book = null;
        var bookInfoWindow = App.FindBookInfoWindow();
        if (bookInfoWindow != null && bookInfoWindow.Book == book)
            bookInfoWindow.Close();
        var chaptersWindow = App.FindChaptersWindow();
        if (chaptersWindow != null && chaptersWindow.Book == book)
            chaptersWindow.Close();
        var bookmarksWindow = App.FindBookmarksWindow();
        if (bookmarksWindow != null && bookmarksWindow.Book == book)
            bookmarksWindow.Close();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
