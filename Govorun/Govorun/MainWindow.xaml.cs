using Microsoft.Win32;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Gemiyur.Collections;
using Govorun.Dialogs;
using Govorun.Models;
using Govorun.Tools;

namespace Govorun
{
    #region Задачи (TODO).

    #endregion

    /// <summary>
    /// Класс главного окна.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Окно проигрывателя.
        /// </summary>
        public PlayerWindow? PlayerWindow;

        /// <summary>
        /// Список отображаемых книг.
        /// </summary>
        private ObservableCollectionEx<Book> shownBooks = [];

        private readonly OpenFileDialog AddBookDialog = new()
        {
            AddToRecent = false,
            CheckFileExists = true,
            CheckPathExists = true,
            ValidateNames = true,
            Title = "Добавить книгу",
            Filter = "Файлы книг|*.m4b;*.m4a;*.mp3"
        };

        private readonly OpenFolderDialog FindBooksDialog = new()
        {
            Title = "Найти книги в папке",
            ValidateNames = true
        };

        /// <summary>
        /// Инициализирует новый экземпляр класса.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            App.DbName = Properties.Settings.Default.DebugDbName;
#else
            App.DbName = Properties.Settings.Default.DbName;
#endif
            if (!File.Exists(App.DbName))
            {
                Db.GenerateTestDb();
            }
            //var books = Db.GetBooks();
            shownBooks.AddRange(Books.AllBooks);
            SortShownBooks();
            BooksListView.ItemsSource = shownBooks;
            UpdateStatusBarBooksCount();
        }

        /// <summary>
        /// Сортирует список отображаемых книг по названию.
        /// </summary>
        private void SortShownBooks() => shownBooks.Sort(x => x.Title, StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// Обновляет количество отображаемых книг в строке статуса.
        /// </summary>
        private void UpdateStatusBarBooksCount() => StatusBarBooksCount.Text = BooksListView.Items.Count.ToString();

        #region Обработчики событий элементов управления.

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BooksListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void BooksListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StatusBarSelectedCount.Text = BooksListView.SelectedItems.Count.ToString();
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

        #endregion

        #region Обработчики команд группы "Книга".

        private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = BooksListView != null && BooksListView.SelectedItems.Count == 1;
            if (!IsVisible)
                return;
            var bitmap = App.GetBitmap(
                e.CanExecute ? @"Images\Buttons\Enabled\Play.png" : @"Images\Buttons\Disabled\Play.png");
            ((Image)PlayButton.Content).Source = bitmap;
            ((Image)PlayMenuItem.Icon).Source = bitmap;
        }

        private void Play_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void Chapters_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = BooksListView != null && BooksListView.SelectedItems.Count == 1;
            if (!IsVisible)
                return;
            var bitmap = App.GetBitmap(
                e.CanExecute ? @"Images\Buttons\Enabled\Chapters.png" : @"Images\Buttons\Disabled\Chapters.png");
            ((Image)ChaptersButton.Content).Source = bitmap;
            ((Image)ChaptersMenuItem.Icon).Source = bitmap;
        }

        private void Chapters_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void Bookmarks_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = BooksListView != null && BooksListView.SelectedItems.Count == 1;
            if (!IsVisible)
                return;
            var bitmap = App.GetBitmap(
                e.CanExecute ? @"Images\Buttons\Enabled\Bookmarks.png" : @"Images\Buttons\Disabled\Bookmarks.png");
            ((Image)BookmarksButton.Content).Source = bitmap;
            ((Image)BookmarksMenuItem.Icon).Source = bitmap;
        }

        private void Bookmarks_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void Edit_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = BooksListView != null && BooksListView.SelectedItems.Count == 1;
            if (!IsVisible)
                return;
            var bitmap = App.GetBitmap(
                e.CanExecute ? @"Images\Buttons\Enabled\Edit.png" : @"Images\Buttons\Disabled\Edit.png");
            ((Image)EditButton.Content).Source = bitmap;
            ((Image)EditMenuItem.Icon).Source = bitmap;
        }

        private void Edit_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = BooksListView != null && BooksListView.SelectedItems.Count > 0;
            if (!IsVisible)
                return;
            var bitmap = App.GetBitmap(
                e.CanExecute ? @"Images\Buttons\Enabled\Delete.png" : @"Images\Buttons\Disabled\Delete.png");
            ((Image)DeleteButton.Content).Source = bitmap;
            ((Image)DeleteMenuItem.Icon).Source = bitmap;
        }

        private void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        #endregion

        #region Обработчики команд группы "Библиотека".

        private void AddBook_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AddBookDialog.FileName = string.Empty;
            if (!App.SimpleBool(AddBookDialog.ShowDialog()))
                return;

        }

        private void FindBooks_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FindBooksDialog.FolderName = string.Empty;
            if (!App.SimpleBool(FindBooksDialog.ShowDialog()))
                return;

        }

        private void Authors_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void Lectors_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void CheckLibrary_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void Shrink_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // TODOL: После каждого сжатия библиотеки создаётся файл резервной копии. Что с ним делать?
            if (MessageBox.Show("Сжать базу данных библиотеки?", Title, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                return;
            }
            Cursor = Cursors.Wait;
            App.DoEvents();
            Db.Shrink();
            Cursor = null;
            MessageBox.Show($"Сжатие базы данных библиотеки завершено.", Title);
        }

        private void Settings_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        #endregion

        #region Обработчики команд группы "Справка".

        private void About_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            new AboutDialog() { Owner = this }.ShowDialog();
        }

        #endregion
    }
}