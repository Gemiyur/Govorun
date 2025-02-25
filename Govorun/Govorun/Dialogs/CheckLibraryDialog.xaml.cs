using System.Windows;
using System.Windows.Controls;
using Gemiyur.Collections;
using Govorun.Models;
using Govorun.Tools;

namespace Govorun.Dialogs
{
    /// <summary>
    /// Класс окна проверки библиотеки.
    /// </summary>
    public partial class CheckLibraryDialog : Window
    {
        /// <summary>
        /// Список удалённых книг.
        /// </summary>
        public readonly List<Book> ChangedBooks = [];

        /// <summary>
        /// Список книг с изменённым файлом.
        /// </summary>
        public readonly List<Book> DeletedBooks = [];

        /// <summary>
        /// Коллекция книг у которых не найден файл.
        /// </summary>
        private readonly ObservableCollectionEx<Book> books = [];

        public CheckLibraryDialog()
        {
            InitializeComponent();
            books.AddRange(Books.AllBooks.FindAll(
                x => !x.FileExists).OrderBy(x => x.Title, StringComparer.CurrentCultureIgnoreCase));
            BooksListView.ItemsSource = books;
        }

        private void BooksListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FileButton.IsEnabled = BooksListView.SelectedItems.Count == 1;
            DeleteButton.IsEnabled = BooksListView.SelectedItems.Count > 0;
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
            book.FileName = dialog.FileName;
            ChangedBooks.Add(book);
            books.Remove(book);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var deletedBooks = BooksListView.SelectedItems.Cast<Book>();
            DeletedBooks.AddRange(deletedBooks);
            books.RemoveRange(deletedBooks);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
