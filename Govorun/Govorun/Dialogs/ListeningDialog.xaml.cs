using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Gemiyur.Collections;
using Govorun.Models;
using Govorun.Tools;

namespace Govorun.Dialogs
{
    /// <summary>
    /// Класс окна слушаемых книг.
    /// </summary>
    public partial class ListeningDialog : Window
    {
        /// <summary>
        /// Книга, выбранная для слушания.
        /// </summary>
        public Book? BookForPlay;

        /// <summary>
        /// Коллекция слушаемых книг.
        /// </summary>
        private readonly ObservableCollectionEx<Book> books = [];

        /// <summary>
        /// Возвращает книгу в проигрывателе.
        /// </summary>
        private Book? PlayingBook
        {
            get
            {
                var owner = Owner != null && Owner is MainWindow ? (MainWindow)Owner : null;
                return owner?.Player.Book;
            }
        }

        public ListeningDialog()
        {
            InitializeComponent();
            books.AddRange(Books.GetListeningBooks());
            BooksListView.ItemsSource = books;
        }

        private void BooksListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is TextBlock && BooksListView.SelectedItem != null)
            {
                BookForPlay = (Book)BooksListView.SelectedItem;
                DialogResult = true;
            }
        }

        private void BooksListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListenButton.IsEnabled = BooksListView.SelectedItems.Count == 1;
            ResetButton.IsEnabled =
                BooksListView.SelectedItems.Count == 1 && (Book)BooksListView.SelectedItem == PlayingBook
                    ? false
                    : BooksListView.SelectedItems.Count > 0;
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

        private void ListenButton_Click(object sender, RoutedEventArgs e)
        {
            BookForPlay = (Book)BooksListView.SelectedItem;
            DialogResult = true;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedBooks = BooksListView.SelectedItems.Cast<Book>().ToList();
            if (PlayingBook != null)
                selectedBooks.Remove(PlayingBook);
            using var db = Db.GetDatabase();
            foreach (var book in selectedBooks)
            {
                book.PlayPosition = TimeSpan.Zero;
                books.Remove(book);
                Db.UpdateBook(book, db);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
