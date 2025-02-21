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
using Govorun.Media;

namespace Govorun
{
    #region Задачи (TODO).

    // TODO: Сделать сохранение и загрузку при запуске программы книги в проигрывателе. 

    #endregion

    /// <summary>
    /// Класс главного окна.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Коллекция отображаемых книг.
        /// </summary>
        private readonly ObservableCollectionEx<Book> ShownBooks = [];

        /// <summary>
        /// Коллекция авторов.
        /// </summary>
        private readonly ObservableCollectionEx<Author> Authors = [];

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
            //if (!File.Exists(App.DbName))
            //{
            //    Db.GenerateTestDb();
            //}

            Authors.AddRange(Db.GetAuthors());
            AuthorsListBox.ItemsSource = Authors;
            ShownBooks.AddRange(Books.AllBooks);
            BooksListView.ItemsSource = ShownBooks;
            UpdateStatusBarBooksCount();
            Player.IsEnabled = false;
        }

        /// <summary>
        /// Запускает и выполняет диалоговое окно информации о книге.
        /// </summary>
        /// <param name="book"></param>
        private void RunBookInfoDialog(Book book)
        {
            var dialog = new BookInfoDialog(book) { Owner = this };
            if (!App.SimpleBool(dialog.ShowDialog()) || book == Player.Book)
                return;
            SaveBookPlayPosition();
            Player.Book = book;
        }

        /// <summary>
        /// Сохраняет позицию воспроизведения книги в проигрывателе в базе данных.
        /// </summary>
        private void SaveBookPlayPosition()
        {
            var book = Player.Book;
            if (book == null)
                return;
            var position = Player.Player.Position < Player.Player.NaturalDuration.TimeSpan
                ? Player.Player.Position
                : TimeSpan.Zero;
            book.PlayPosition = position;
            Db.UpdateBook(book);
            book.OnPropertyChanged("PlayPosition");
        }

        /// <summary>
        /// Сохраняет громкость проигрывателя в настройках приложения.
        /// </summary>
        private void SavePlayerVolume()
        {
            Properties.Settings.Default.PlayerVolume = (int)(Player.Player.Volume * 100);
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Сортирует коллекцию отображаемых книг по названию.
        /// </summary>
        private void SortShownBooks() => ShownBooks.Sort(x => x.Title, StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// Обновляет список авторов книг.
        /// </summary>
        private void UpdateAuthors()
        {
            var selectedAuthor = (Author)AuthorsListBox.SelectedItem;
            Authors.ReplaceRange(Db.GetAuthors());
            if (selectedAuthor != null)
            {
                var author = Authors.First(x => x.AuthorId == selectedAuthor.AuthorId);
                AuthorsListBox.SelectedItem = author;
                AuthorsListBox.ScrollIntoView(AuthorsListBox.SelectedItem);
            }
        }

        /// <summary>
        /// Обновляет список отображаемых книг.
        /// </summary>
        private void UpdateShownBooks()
        {
            var author = (Author)AuthorsListBox.SelectedItem;
            var books = author == null ? Books.AllBooks : Books.GetAuthorBooks(author.AuthorId);
            ShownBooks.ReplaceRange(books);
        }

        /// <summary>
        /// Обновляет количество отображаемых книг в строке статуса.
        /// </summary>
        private void UpdateStatusBarBooksCount() => StatusBarBooksCount.Text = BooksListView.Items.Count.ToString();

        private void Window_Closed(object sender, EventArgs e)
        {
            SaveBookPlayPosition();
            SavePlayerVolume();
        }

        #region Обработчики событий элементов управления.

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AllAuthorsButton_Click(object sender, RoutedEventArgs e)
        {
            AuthorsListBox.SelectedIndex = -1;
            SortShownBooks();
        }

        private void AuthorsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AllAuthorsButton.IsEnabled = AuthorsListBox.SelectedIndex >= 0;
            UpdateShownBooks();
        }

        private void BooksListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is TextBlock && BooksListView.SelectedItem != null)
                RunBookInfoDialog((Book)BooksListView.SelectedItem);
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
            var book = (Book)BooksListView.SelectedItem;
            if (book == Player.Book)
                return;
            SaveBookPlayPosition();
            Player.Book = book;
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
            RunBookInfoDialog((Book)BooksListView.SelectedItem);
        }

        private void Chapters_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = BooksListView != null && BooksListView.SelectedItems.Count == 1 &&
                ((Book)BooksListView.SelectedItem).Chapters.Any();
            if (!IsVisible)
                return;
            var bitmap = App.GetBitmap(
                e.CanExecute ? @"Images\Buttons\Enabled\Chapters.png" : @"Images\Buttons\Disabled\Chapters.png");
            ((Image)ChaptersButton.Content).Source = bitmap;
            ((Image)ChaptersMenuItem.Icon).Source = bitmap;
        }

        private void Chapters_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var book = (Book)BooksListView.SelectedItem;
            var dialog = new ChaptersDialog(book) { Owner = this };
            if (!App.SimpleBool(dialog.ShowDialog()) || dialog.Chapter == null)
                return;
            if (book != Player.Book)
            {
                SaveBookPlayPosition();
                book.PlayPosition = dialog.Chapter.StartTime;
                Player.Book = book;
            }
            else
                Player.PlayPosition = dialog.Chapter.StartTime;
        }

        private void Bookmarks_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = BooksListView != null && BooksListView.SelectedItems.Count == 1 &&
                ((Book)BooksListView.SelectedItem).Bookmarks.Any();
            if (!IsVisible)
                return;
            var bitmap = App.GetBitmap(
                e.CanExecute ? @"Images\Buttons\Enabled\Bookmarks.png" : @"Images\Buttons\Disabled\Bookmarks.png");
            ((Image)BookmarksButton.Content).Source = bitmap;
            ((Image)BookmarksMenuItem.Icon).Source = bitmap;
        }

        private void Bookmarks_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var book = (Book)BooksListView.SelectedItem;
            var dialog = new BookmarksDialog(book) { Owner = this };
            var dialogResult = App.SimpleBool(dialog.ShowDialog());
            Player.CheckBookmarksButton();
            if (!dialogResult || dialog.Bookmark == null)
                return;

            if (book != Player.Book)
            {
                SaveBookPlayPosition();
                book.PlayPosition = dialog.Bookmark.Position;
                Player.Book = book;
            }
            else
                Player.PlayPosition = dialog.Bookmark.Position;

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
            var editor = new BookEditor(book, null) { Owner = this };
            if (!editor.ShowDialog() != true)
                return;
            //if (!App.SimpleBool(editor.ShowDialog()))
            //    return;
            if (editor.HasNewAuthors)
                UpdateAuthors();
            if (AuthorsListBox.SelectedIndex >= 0 && editor.AuthorsChanged)
            {
                var author = (Author)AuthorsListBox.SelectedItem;
                if (!book.Authors.Exists(x => x.AuthorId == author.AuthorId))
                {
                    ShownBooks.Remove(book);
                    return;
                }
            }
            if (editor.TitleChanged)
            {
                SortShownBooks();
                BooksListView.SelectedItem = book;
                BooksListView.ScrollIntoView(BooksListView.SelectedItem);
            }
            book.OnPropertyChanged("AuthorsSurnameNameText");
            book.OnPropertyChanged("AuthorsNameSurnameText");
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
            if (BooksListView.SelectedItems.Count == 1)
            {
                var book = (Book)BooksListView.SelectedItem;
                if (MessageBox.Show($"Удалить книгу \"{book.Title}\"?", Title,
                                    MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return;
                }
                Db.DeleteBook(book);
                Books.AllBooks.Remove(book);
                ShownBooks.Remove(book);
                UpdateStatusBarBooksCount();
                return;
            }
            var books = BooksListView.SelectedItems.Cast<Book>().ToList();
            if (MessageBox.Show("Удалить выбранные книги?", Title,
                                MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                return;
            }
            Db.DeleteBooks(books);
            Books.AllBooks.RemoveAll(books.Contains);
            ShownBooks.RemoveRange(books);
            UpdateStatusBarBooksCount();
        }

        #endregion

        #region Обработчики команд группы "Библиотека".

        private void Listening_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new ListeningDialog() { Owner = this };
            if (!App.SimpleBool(dialog.ShowDialog()) || dialog.BookForPlay == Player.Book)
                return;
            SaveBookPlayPosition();
            Player.Book = dialog.BookForPlay;
        }

        private void AddBook_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var fileDialog = App.PickBookFileDialog;
            if (!App.SimpleBool(fileDialog.ShowDialog()))
                return;
            var filename = fileDialog.FileName;
            if (Books.BookWithFileExists(filename))
            {
                MessageBox.Show("Книга с этим файлом уже есть в библиотеке.", "Добавление книги");
                return;
            }
            var book = App.GetBookFromFile(filename, out TrackData tag);
            var editor = new BookEditor(book, tag) { Owner = this };
            if (!App.SimpleBool(editor.ShowDialog()))
                return;
            Books.AllBooks.Add(book);
            if (editor.HasNewAuthors)
                UpdateAuthors();
            if (AuthorsListBox.SelectedItem != null &&
                !Books.BookHasAuthor(book, ((Author)AuthorsListBox.SelectedItem).AuthorId))
            {
                AuthorsListBox.SelectedItem = null;
            }
            ShownBooks.Add(book);
            SortShownBooks();
            BooksListView.SelectedItem = book;
            BooksListView.ScrollIntoView(AuthorsListBox.SelectedItem);
            book.OnPropertyChanged("AuthorsSurnameNameText");
            book.OnPropertyChanged("AuthorsNameSurnameText");
        }

        private void FindBooks_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var folderDialog = App.PickBooksFolderDialog;
            if (!App.SimpleBool(folderDialog.ShowDialog()))
                return;
            var files = new List<string>(); // Новые файлы книг.
            var folders = folderDialog.FolderNames;
            foreach (var folder in folders)
            {
                foreach (var mask in App.BookFileMasks)
                {
                    var folderFiles = Directory.GetFiles(folder, mask, SearchOption.AllDirectories);
                    foreach (var file in folderFiles)
                    {
                        if (!Books.BookWithFileExists(file))
                            files.Add(file);
                    }
                }
            }
            files.Sort(StringComparer.CurrentCultureIgnoreCase);
            var dialog = new AddBooksDialog(files) { Owner = this };
            dialog.ShowDialog();
            if (!dialog.AddedBooks.Any())
                return;
            Books.AllBooks.AddRange(dialog.AddedBooks);
            if (dialog.HasNewAuthors)
                UpdateAuthors();
            UpdateShownBooks();
            SortShownBooks();
        }

        private void Authors_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var editor = new AuthorsEditor() { Owner = this };
            editor.ShowDialog();
            if (!editor.HasChanges)
                return;
            UpdateAuthors();
            foreach (var book in ShownBooks)
            {
                book.OnPropertyChanged("AuthorsSurnameNameText");
                book.OnPropertyChanged("AuthorsNameSurnameText");
            }
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