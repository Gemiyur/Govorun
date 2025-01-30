using Microsoft.Win32;
using System.IO;
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

    // TODO: Надо бы подобрать картинку для кнопки "Все авторы" (AllAuthorsButton).

    #endregion

    /// <summary>
    /// Класс главного окна.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Список отображаемых книг.
        /// </summary>
        private readonly ObservableCollectionEx<Book> ShownBooks = [];

        /// <summary>
        /// Список авторов.
        /// </summary>
        private readonly ObservableCollectionEx<Author> Authors = [];

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

            Authors.AddRange(Db.GetAuthors());
            AuthorsListBox.ItemsSource = Authors;

            ShownBooks.AddRange(Books.AllBooks);
            //SortShownBooks();
            BooksListView.ItemsSource = ShownBooks;
            UpdateStatusBarBooksCount();
        }

        /// <summary>
        /// Сортирует список отображаемых книг по названию.
        /// </summary>
        private void SortShownBooks() => ShownBooks.Sort(x => x.Title, StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// Обновляет список отображаемых книг.
        /// </summary>
        private void UpdateShownBooks()
        {
            var author = (Author)AuthorsListBox.SelectedItem;
            var books = author == null ? Books.AllBooks : Books.GetAuthorBooks(author.AuthorId);
            if (ListeningMenuItem.IsChecked)
                books = books.FindAll(x => x.Listening);
            ShownBooks.ReplaceRange(books);
        }

        /// <summary>
        /// Обновляет количество отображаемых книг в строке статуса.
        /// </summary>
        private void UpdateStatusBarBooksCount() => StatusBarBooksCount.Text = BooksListView.Items.Count.ToString();

        #region Обработчики событий элементов управления.

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AllAuthorsButton_Click(object sender, RoutedEventArgs e)
        {
            AuthorsListBox.SelectedIndex = -1;
        }

        private void AuthorsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AllAuthorsButton.IsEnabled = AuthorsListBox.SelectedIndex >= 0;
            UpdateShownBooks();
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

        private void Info_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = BooksListView != null && BooksListView.SelectedItems.Count == 1;
            if (!IsVisible)
                return;
            var bitmap = App.GetBitmap(
                e.CanExecute ? @"Images\Buttons\Enabled\Info.png" : @"Images\Buttons\Disabled\Info.png");
            ((Image)InfoButton.Content).Source = bitmap;
            ((Image)InfoMenuItem.Icon).Source = bitmap;
        }

        private void Info_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void Chapters_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // TODOL: Надо ли проверять книгу на наличие содержания для разрешения команды?
            e.CanExecute = BooksListView != null && BooksListView.SelectedItems.Count == 1 &&
                ((Book)BooksListView.SelectedItem).Chapters.Any();

            // Без проверки книги на наличие содержания.
            //e.CanExecute = BooksListView != null && BooksListView.SelectedItems.Count == 1;

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
            // TODOL: Надо ли проверять книгу на наличие закладок для разрешения команды?
            e.CanExecute = BooksListView != null && BooksListView.SelectedItems.Count == 1 &&
                ((Book)BooksListView.SelectedItem).Bookmarks.Any();

            // Без проверки книги на наличие закладок.
            //e.CanExecute = BooksListView != null && BooksListView.SelectedItems.Count == 1;

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

        private void Reset_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // TODOL: Надо ли проверять позицию воспроизведения книги для разрешения команды?
            e.CanExecute = BooksListView != null && BooksListView.SelectedItems.Cast<Book>().Any(x => x.Listening);

            // Без проверки позиции воспроизведения книги.
            //e.CanExecute = BooksListView != null && BooksListView.SelectedItems.Count > 0;

            if (!IsVisible)
                return;
            var bitmap = App.GetBitmap(
                e.CanExecute ? @"Images\Buttons\Enabled\Reset.png" : @"Images\Buttons\Disabled\Reset.png");
            ((Image)ResetButton.Content).Source = bitmap;
            ((Image)ResetMenuItem.Icon).Source = bitmap;
        }

        private void Reset_Executed(object sender, ExecutedRoutedEventArgs e)
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
            var book = (Book)BooksListView.SelectedItem;
            var editor = new BookEditor(book, false) { Owner = this };
            if (!App.SimpleBool(editor.ShowDialog()))
                return;

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

        private void Listening_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            bool isChecked;
            if (e.Parameter == null)
                return;
            if (e.Parameter.ToString() == "ListeningMenuItem")
            {
                isChecked = ListeningMenuItem.IsChecked;
                ListeningCheckBox.IsChecked = isChecked;
            }
            else if (e.Parameter.ToString() == "ListeningCheckBox")
            {
                isChecked = App.SimpleBool(ListeningCheckBox.IsChecked);
                ListeningMenuItem.IsChecked = isChecked;
            }
            else
                return;
            UpdateShownBooks();
        }

        private void AddBook_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AddBookDialog.FileName = string.Empty;
            if (!App.SimpleBool(AddBookDialog.ShowDialog()))
                return;
            //var editor = new BookEditor() { Owner = this };
            //if (!App.SimpleBool(editor.ShowDialog()))
            //    return;

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
            //var lectors = Books.Lectors;
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