using Gemiyur.Collections;
using System.Windows;
using System.Windows.Controls;
using Govorun.Models;
using Govorun.Tools;

namespace Govorun.Dialogs;

/// <summary>
/// Класс окна проверки библиотеки.
/// </summary>
public partial class CheckLibraryDialog : Window
{
    /// <summary>
    /// Коллекция книг у которых не найден файл.
    /// </summary>
    private readonly ObservableCollectionEx<Book> books = [];

    public CheckLibraryDialog(IEnumerable<Book> books)
    {
        InitializeComponent();
        this.books.AddRange(books);
        BooksListView.ItemsSource = books;
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

        var player = App.GetMainWindow().Player;
        if (player.Book == book)
        {
            player.PlayOnLoad = false;
            player.Book = book;
        }

        // TODO: Если открыто окно о книге, то надо обновить файл.
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
        // TODO: Если книга удалена, то надо закрыть открытые окна книги.

        // TODO: На кой хрен обновлять панель навигации?
        //mainWindow.UpdateNavPanel(false, false, true);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
