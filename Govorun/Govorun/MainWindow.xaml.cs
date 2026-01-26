using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Gemiyur.Collections;
using Govorun.Dialogs;
using Govorun.Media;
using Govorun.Models;
using Govorun.Tools;

namespace Govorun;

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
    /// Коллекция серий.
    /// </summary>
    private readonly ObservableCollectionEx<Cycle> Cycles = [];

    /// <summary>
    /// Коллекция тегов.
    /// </summary>
    private readonly ObservableCollectionEx<Genre> Genres = [];

    /// <summary>
    /// Инициализирует новый экземпляр класса.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        // Имя файла базы из параметров приложения.
#if DEBUG
        App.DbName = Properties.Settings.Default.DebugDbName;
#else
        App.DbName = Properties.Settings.Default.DbName;
#endif

        // Проверяем файл базы данных.
        if (!File.Exists(App.DbName))
        {
            MessageBox.Show("Файл базы данных не найден.\nУкажите имя существующего или нового файла.", Title);
            if (!PickDbFile(Path.GetFileName(App.DbName)))
            {
                Close();
                return;
            }
        }
        else if (!Db.ValidateDb(App.DbName))
        {
            MessageBox.Show(
                "Файл не является базой данных Говоруна или повреждён.\nУкажите имя существующего или нового файла.", Title);
            if (!PickDbFile(App.DbName))
            {
                Close();
                return;
            }
        }

        // Новая база данных. Инициализируем.
        if (!File.Exists(App.DbName))
            Db.InitializeCollections();

        // Такого не должно быть. Где-то косяк.
        if (Db.DbInfo == null)
        {
            MessageBox.Show("Непредвиденная ошибка: Db.DbInfo == null.\nПриложение закроется.", Title);
            Close();
            return;
        }

        // Загружаем данные приложения.
        Authors.AddRange(Library.Authors);
        AuthorsListBox.ItemsSource = Authors;
        CheckNavPanelAuthorsNameFormat();
        Cycles.AddRange(Library.Cycles);
        CyclesListBox.ItemsSource = Cycles;
        Genres.AddRange(Library.Genres);
        GenresListBox.ItemsSource = Genres;
        ShownBooks.AddRange(Library.Books);
        BooksListBox.ItemsSource = ShownBooks;
        BooksListBox.ItemTemplate = Properties.Settings.Default.BookListAuthorFullName
            ? (DataTemplate)FindResource("BookAuthorsFullNameDataTemplate")
            : (DataTemplate)FindResource("BookAuthorsShortNameDataTemplate");
        UpdateStatusBarBooksCount();
        Player.IsEnabled = false;
        LoadLastBook();
    }

    /// <summary>
    /// Устанавливает формат отображения имён авторов в панели навигации.
    /// </summary>
    public void CheckNavPanelAuthorsNameFormat()
    {
        AuthorsListBox.ItemTemplate = Properties.Settings.Default.NavPanelAuthorFullName
            ? (DataTemplate)FindResource("AuthorFullNameDataTemplate")
            : (DataTemplate)FindResource("AuthorShortNameDataTemplate");
    }

    /// <summary>
    /// Загружает в проигрыватель книгу, которая воспроизводилась при закрытии приложения.
    /// Книга только загружается, но не воспроизводится.
    /// </summary>
    /// <remarks>Используется при запуске приложения.</remarks>
    private void LoadLastBook()
    {
        if (!Properties.Settings.Default.LoadLastBook)
            return;
        var lastBookId = Properties.Settings.Default.LastBookId;
        if (lastBookId == 0)
            return;
        var lastBook = Library.GetBook(lastBookId);
        if (lastBook == null)
            return;
        Player.PlayOnLoad = false;
        Player.Book = lastBook;
    }

    /// <summary>
    /// Выполняет выбор файла базы данных и возвращает был ли выбран файл.
    /// </summary>
    /// <param name="filename">Имя файла.</param>
    /// <returns>Был ли выбран файл.</returns>
    private bool PickDbFile(string filename)
    {
        var dialog = App.PickDatabaseDialog;
        dialog.FileName = filename;
        string dbName;
        while (true)
        {
            if (dialog.ShowDialog() != true)
            {
                MessageBox.Show("Файл базы данных не выбран.\nПриложение закроется.", Title);
                return false;
            }
            dbName = Db.EnsureDbExtension(dialog.FileName);
            if (Db.ValidateDb(dbName))
                break;
            MessageBox.Show("Файл не является базой данных Говоруна или повреждён.", Title);
        }
        App.DbName = dbName;
#if DEBUG
        Properties.Settings.Default.DebugDbName = App.DbName;
#else
        Properties.Settings.Default.DbName = App.DbName;
#endif
        return true;
    }

    /// <summary>
    /// Воспроизводит указанную книгу.
    /// </summary>
    /// <param name="book">Книга.</param>
    public void PlayBook(Book book)
    {
        if (book == Player.Book)
        {
            Player.Play();
            return;
        }
        SavePlayerBookPlayPosition();
        if (book.PlayPosition == TimeSpan.Zero)
            book.PlayPosition = TimeSpan.FromMilliseconds(1);
        Player.Book = book;
    }

    /// <summary>
    /// Воспроизводит указанную книгу с указанной позиции.
    /// </summary>
    /// <param name="book">Книга.</param>
    /// <param name="position">Позиция воспроизведения.</param>
    public void PlayBook(Book book, TimeSpan position)
    {
        if (book != Player.Book)
        {
            SavePlayerBookPlayPosition();
            book.PlayPosition = position;
            Player.Book = book;
        }
        else
        {
            Player.PlayPosition = position;
            Player.Play();
        }
    }

    /// <summary>
    /// Сохраняет позицию воспроизведения книги в проигрывателе в базе данных.
    /// </summary>
    private void SavePlayerBookPlayPosition()
    {
        var book = Player.Book;
        if (book == null || Player.MediaFailed)
            return;
        var position = Player.Player.Position < Player.Player.NaturalDuration.TimeSpan
            ? Player.Player.Position
            : TimeSpan.Zero;
        book.PlayPosition = position;
        SaveBookPlayPosition(book);
    }

    /// <summary>
    /// Сохраняет позицию воспроизведения указанной книги в базе данных.
    /// </summary>
    /// <param name="book">Книга.</param>
    private static void SaveBookPlayPosition(Book book)
    {
        Library.UpdateBook(book);
        book.OnPropertyChanged("PlayPosition");
    }

    /// <summary>
    /// Сохраняет идентификатор воспроизводимой книги в настройках приложения.
    /// </summary>
    /// <remarks>Используется при закрытии приложения.</remarks>
    private void SaveLastBook()
    {
        var book = Player.Book;
        Properties.Settings.Default.LastBookId = book != null && !Player.MediaFailed ? book.BookId : 0;
    }

    /// <summary>
    /// Сохраняет громкость проигрывателя в настройках приложения.
    /// </summary>
    private void SavePlayerVolume()
    {
        Properties.Settings.Default.PlayerVolume = (int)(Player.Player.Volume * 100);
    }

    /// <summary>
    /// Выделяет указанную книгу в списке отображаемых книг.
    /// </summary>
    /// <param name="book">Книга.</param>
    /// <remarks>Если указанной книги в списке нет, то ничего не делает.</remarks>
    private void SelectBookInShownBooks(Book book)
    {
        if (ShownBooks.Contains(book))
        {
            BooksListBox.SelectedItem = book;
            BooksListBox.ScrollIntoView(BooksListBox.SelectedItem);
        }
    }

    /// <summary>
    /// Обновляет списки панели навигации.
    /// </summary>
    /// <param name="authors">Обновить список авторов.</param>
    /// <param name="cycles">Обновить список серий.</param>
    /// <param name="genres">Обновить список жанров.</param>
    public void UpdateNavPanel(bool authors, bool cycles, bool genres)
    {
        LockNavHandlers();
        if (authors)
        {
            var selectedAuthor = (Author)AuthorsListBox.SelectedItem;
            Authors.ReplaceRange(Library.Authors);
            if (selectedAuthor != null)
            {
                AuthorsListBox.SelectedItem = Authors.FirstOrDefault(x => x.AuthorId == selectedAuthor.AuthorId);
                if (AuthorsListBox.SelectedItem != null)
                    AuthorsListBox.ScrollIntoView(AuthorsListBox.SelectedItem);
            }
        }
        if (cycles)
        {
            var selectedCycle = (Cycle)CyclesListBox.SelectedItem;
            Cycles.ReplaceRange(Library.Cycles);
            if (selectedCycle != null)
            {
                CyclesListBox.SelectedItem = Cycles.FirstOrDefault(x => x.CycleId == selectedCycle.CycleId);
                if (CyclesListBox.SelectedItem != null)
                    CyclesListBox.ScrollIntoView(CyclesListBox.SelectedItem);
            }
        }
        if (genres)
        {
            var selectedGenre = (Genre)GenresListBox.SelectedItem;
            Genres.ReplaceRange(Library.Genres);
            if (selectedGenre != null)
            {
                GenresListBox.SelectedItem = Genres.FirstOrDefault(x => x.Equals(selectedGenre));
                if (GenresListBox.SelectedItem != null)
                    GenresListBox.ScrollIntoView(GenresListBox.SelectedItem);
            }
        }
        if (AuthorsListBox.SelectedItem == null &&
            CyclesListBox.SelectedItem == null &&
            GenresListBox.SelectedItem == null &&
            ListeningBooksToggleButton.IsChecked != true &&
            AllBooksToggleButton.IsChecked != true)
        {
            AllBooksToggleButton.IsChecked = true;
            UpdateShownBooks();
        }
        UnlockNavHandlers();
    }

    /// <summary>
    /// Обновляет список отображаемых книг.
    /// </summary>
    public void UpdateShownBooks()
    {
        if (AllBooksToggleButton.IsChecked == true)
        {
            ShownBooks.ReplaceRange(Library.Books.OrderBy(x => x.Title, StringComparer.CurrentCultureIgnoreCase));
            BooksListBox.ItemTemplate = Properties.Settings.Default.BookListAuthorFullName
                ? (DataTemplate)FindResource("BookAuthorsFullNameDataTemplate")
                : (DataTemplate)FindResource("BookAuthorsShortNameDataTemplate");
            NavItemTextBlock.Text = "Все книги";
        }
        else if (ListeningBooksToggleButton.IsChecked == true)
        {
            ShownBooks.ReplaceRange(Library.ListeningBooks);
            BooksListBox.ItemTemplate = Properties.Settings.Default.BookListAuthorFullName
                ? (DataTemplate)FindResource("BookAuthorsFullNameDataTemplate")
                : (DataTemplate)FindResource("BookAuthorsShortNameDataTemplate");
            NavItemTextBlock.Text = "Слушаемые книги";
        }
        else if (AuthorsListBox.SelectedItem != null)
        {
            var author = (Author)AuthorsListBox.SelectedItem;
            var books = Library.GetAuthorBooks(author.AuthorId);
            ShownBooks.ReplaceRange(books);
            BooksListBox.ItemTemplate = Properties.Settings.Default.BookListAuthorFullName
                ? (DataTemplate)FindResource("BookAuthorsFullNameDataTemplate")
                : (DataTemplate)FindResource("BookAuthorsShortNameDataTemplate");
            NavItemTextBlock.Text = $"Автор: {author.NameLastFirstMiddle}";
        }
        else if (CyclesListBox.SelectedItem != null)
        {
            var cycle = (Cycle)CyclesListBox.SelectedItem;
            var books = Library.GetCycleBooks(cycle.CycleId);
            ShownBooks.ReplaceRange(books);
            BooksListBox.ItemTemplate = Properties.Settings.Default.BookListAuthorFullName
                ? (DataTemplate)FindResource("BookCycleAuthorsFullNameDataTemplate")
                : (DataTemplate)FindResource("BookCycleAuthorsShortNameDataTemplate");
            NavItemTextBlock.Text = $"Серия: {cycle.Title}";
        }
        else if (GenresListBox.SelectedItem != null)
        {
            var genre = (Genre)GenresListBox.SelectedItem;
            var books = Library.GetGenreBooks(genre.GenreId);
            ShownBooks.ReplaceRange(books);
            BooksListBox.ItemTemplate = Properties.Settings.Default.BookListAuthorFullName
                ? (DataTemplate)FindResource("BookAuthorsFullNameDataTemplate")
                : (DataTemplate)FindResource("BookAuthorsShortNameDataTemplate");
            NavItemTextBlock.Text = $"Жанр: {genre.Title}";
        }
        UpdateStatusBarBooksCount();
    }

    /// <summary>
    /// Обновляет имена авторов в списке отображаемых книг.
    /// </summary>
    public void UpdateShownBooksAuthors()
    {
        foreach (var book in ShownBooks)
        {
            book.AuthorsChanged();
        }
    }

    /// <summary>
    /// Обновляет количество отображаемых книг в строке статуса.
    /// </summary>
    private void UpdateStatusBarBooksCount() => BooksCountTextBlock.Text = BooksListBox.Items.Count.ToString();

    #region Блокировка и разблокировка обработчиков событий элементов панели навигации.

    /// <summary>
    /// Блокируются ли обработчики событий элементов панели навигации.
    /// </summary>
    private bool NavHandlersLocked;

    /// <summary>
    /// Блокирует обработчики событий элементов панели навигации.
    /// </summary>
    private void LockNavHandlers() => NavHandlersLocked = true;

    /// <summary>
    /// Разблокирует обработчики событий элементов панели навигации.
    /// </summary>
    private void UnlockNavHandlers() => NavHandlersLocked = false;

    #endregion

    #region Обработчики событий окна.

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (Properties.Settings.Default.SaveMainWindowLocation &&
            App.SizeDefined(Properties.Settings.Default.MainWindowSize))
        {
            Left = Properties.Settings.Default.MainWindowPos.X;
            Top = Properties.Settings.Default.MainWindowPos.Y;
            Width = Properties.Settings.Default.MainWindowSize.Width;
            Height = Properties.Settings.Default.MainWindowSize.Height;
        }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        SavePlayerBookPlayPosition();
        SaveLastBook();
        SavePlayerVolume();
        if (Properties.Settings.Default.SaveMainWindowLocation)
        {
            Properties.Settings.Default.MainWindowPos = new System.Drawing.Point((int)Left, (int)Top);
            Properties.Settings.Default.MainWindowSize = new System.Drawing.Size((int)Width, (int)Height);
        }
        Properties.Settings.Default.Save();
    }

    #endregion

    #region Обработчики событий элементов панели навигации.

    private void AllBooksToggleButton_Click(object sender, RoutedEventArgs e)
    {
        if (AllBooksToggleButton.IsChecked != true)
        {
            AllBooksToggleButton.IsChecked = true;
            return;
        }
        ListeningBooksToggleButton.IsChecked = false;
        LockNavHandlers();
        AuthorsListBox.SelectedIndex = -1;
        CyclesListBox.SelectedIndex = -1;
        GenresListBox.SelectedIndex = -1;
        UnlockNavHandlers();
        UpdateShownBooks();
    }

    private void ListeningBooksToggleButton_Click(object sender, RoutedEventArgs e)
    {
        if (ListeningBooksToggleButton.IsChecked != true)
        {
            ListeningBooksToggleButton.IsChecked = true;
            return;
        }
        AllBooksToggleButton.IsChecked = false;
        LockNavHandlers();
        AuthorsListBox.SelectedIndex = -1;
        CyclesListBox.SelectedIndex = -1;
        GenresListBox.SelectedIndex = -1;
        UnlockNavHandlers();
        UpdateShownBooks();
    }

    private void AuthorsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        var author = (Author)AuthorsListBox.SelectedItem;
        new AuthorInfoDialog(author) { Owner = this }.ShowDialog();
    }

    private void AuthorsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (NavHandlersLocked)
            return;
        AllBooksToggleButton.IsChecked = AuthorsListBox.SelectedIndex < 0;
        ListeningBooksToggleButton.IsChecked = false;
        LockNavHandlers();
        CyclesListBox.SelectedIndex = -1;
        GenresListBox.SelectedIndex = -1;
        UnlockNavHandlers();
        UpdateShownBooks();
    }

    private void CyclesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        var cycle = (Cycle)CyclesListBox.SelectedItem;
        new CycleInfoDialog(cycle) { Owner = this }.ShowDialog();
    }

    private void CyclesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (NavHandlersLocked)
            return;
        AllBooksToggleButton.IsChecked = CyclesListBox.SelectedIndex < 0;
        ListeningBooksToggleButton.IsChecked = false;
        LockNavHandlers();
        AuthorsListBox.SelectedIndex = -1;
        GenresListBox.SelectedIndex = -1;
        UnlockNavHandlers();
        UpdateShownBooks();
    }

    private void GenresListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (NavHandlersLocked)
            return;
        AllBooksToggleButton.IsChecked = GenresListBox.SelectedIndex < 0;
        ListeningBooksToggleButton.IsChecked = false;
        LockNavHandlers();
        AuthorsListBox.SelectedIndex = -1;
        CyclesListBox.SelectedIndex = -1;
        UnlockNavHandlers();
        UpdateShownBooks();
    }

    #endregion

    #region Обработчики событий элемента списка книг.

    private void BooksListBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        if (e.OriginalSource is not TextBlock)
        {
            e.Handled = true;
        }
    }

    private void BooksListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (BooksListBox.SelectedItem != null && (e.OriginalSource is TextBlock || e.OriginalSource is Border))
            App.ShowBookInfo((Book)BooksListBox.SelectedItem);
    }

    #endregion

    #region Обработчики команд библиотеки.

    private void Settings_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        new SettingsDialog() { Owner = this }.ShowDialog();
    }

    private void Exit_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        Close();
    }

    #endregion

    #region Обработчики команд книг.

    private void AddBook_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var fileDialog = App.PickBookFileDialog;
        if (fileDialog.ShowDialog() != true)
            return;
        var filename = fileDialog.FileName;
        if (Library.BookWithFileExists(filename))
        {
            MessageBox.Show("Книга с этим файлом уже есть в библиотеке.", "Добавление книги");
            return;
        }
        var book = App.GetBookFromFile(filename, out TrackData trackData);
        var editor = new BookEditor(book, trackData) { Owner = this };
        if (editor.ShowDialog() != true)
            return;
        UpdateShownBooks();
        SelectBookInShownBooks(book);
    }

    private void FindBooks_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var folderDialog = App.PickBooksFolderDialog;
        if (folderDialog.ShowDialog() != true)
            return;
        ActionTextBlock.Text = "Поиск файлов книг...";
        ActionStatusBarItem.Visibility = Visibility.Visible;
        var files = new List<string>(); // Новые файлы книг.
        var folders = folderDialog.FolderNames;
        foreach (var folder in folders)
        {
            var folderFiles = Directory.GetFiles(folder, "*.m4b", SearchOption.AllDirectories);
            foreach (var file in folderFiles)
            {
                if (!Library.BookWithFileExists(file))
                    files.Add(file);
            }
        }
        files.Sort(StringComparer.CurrentCultureIgnoreCase);
        ActionStatusBarItem.Visibility = Visibility.Collapsed;
        var dialog = new AddBooksDialog(files) { Owner = this };
        dialog.ShowDialog();
        UpdateShownBooks();
    }

    private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = BooksListBox != null && BooksListBox.SelectedItem != null;
        if (!IsVisible)
            return;
        var bitmap = App.GetBitmapImage(
            e.CanExecute ? @"Images\Buttons\Enabled\Play.png" : @"Images\Buttons\Disabled\Play.png");
        ((Image)PlayButton.Content).Source = bitmap;
        ((Image)PlayMenuItem.Icon).Source = bitmap;
        ((Image)PlayContextMenuItem.Icon).Source = bitmap;
    }

    private void Play_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var book = (Book)BooksListBox.SelectedItem;
        PlayBook(book);
    }

    private void Info_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = BooksListBox != null && BooksListBox.SelectedItem != null;
        if (!IsVisible)
            return;
        var bitmap = App.GetBitmapImage(
            e.CanExecute ? @"Images\Buttons\Enabled\BookInfo.png" : @"Images\Buttons\Disabled\BookInfo.png");
        ((Image)InfoButton.Content).Source = bitmap;
        ((Image)InfoMenuItem.Icon).Source = bitmap;
        ((Image)InfoContextMenuItem.Icon).Source = bitmap;
    }

    private void Info_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        App.ShowBookInfo((Book)BooksListBox.SelectedItem);
    }

    private void Chapters_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = BooksListBox != null && BooksListBox.SelectedItem != null &&
            ((Book)BooksListBox.SelectedItem).Chapters.Any();
        if (!IsVisible)
            return;
        var bitmap = App.GetBitmapImage(
            e.CanExecute ? @"Images\Buttons\Enabled\Chapters.png" : @"Images\Buttons\Disabled\Chapters.png");
        ((Image)ChaptersButton.Content).Source = bitmap;
        ((Image)ChaptersMenuItem.Icon).Source = bitmap;
        ((Image)ChaptersContextMenuItem.Icon).Source = bitmap;
    }

    private void Chapters_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var book = (Book)BooksListBox.SelectedItem;
        App.ShowChapters(book);
    }

    private void Bookmarks_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = BooksListBox != null && BooksListBox.SelectedItem != null;
        if (!IsVisible)
            return;
        var bitmap = App.GetBitmapImage(
            e.CanExecute ? @"Images\Buttons\Enabled\Bookmarks.png" : @"Images\Buttons\Disabled\Bookmarks.png");
        ((Image)BookmarksButton.Content).Source = bitmap;
        ((Image)BookmarksMenuItem.Icon).Source = bitmap;
        ((Image)BookmarksContextMenuItem.Icon).Source = bitmap;
    }

    private void Bookmarks_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var book = (Book)BooksListBox.SelectedItem;
        App.ShowBookmarks(book);
    }

    private void NotListen_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = BooksListBox != null && BooksListBox.SelectedItem != null &&
            ((Book)BooksListBox.SelectedItem).Listening;
        if (!IsVisible)
            return;
        var bitmap = App.GetBitmapImage(
            e.CanExecute ? @"Images\Buttons\Enabled\NotListen.png" : @"Images\Buttons\Disabled\NotListen.png");
        ((Image)NotListenButton.Content).Source = bitmap;
        ((Image)NotListenMenuItem.Icon).Source = bitmap;
        ((Image)NotListenContextMenuItem.Icon).Source = bitmap;
    }

    private void NotListen_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var book = (Book)BooksListBox.SelectedItem;
        if (book == Player.Book)
        {
            const string message = "Книга находится в состоянии прослушивания.\n" +
                                   "Прослушивание книги будет прекращено.\n\n" +
                                   "Прекратить прослушивание книги?";
            if (MessageBox.Show(message, Title, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;
            Player.Book = null;
        }
        book.PlayPosition = TimeSpan.Zero;
        SaveBookPlayPosition(book);
        if (ListeningBooksToggleButton.IsChecked == true)
            UpdateShownBooks();
        var chaptersWindow = App.FindChaptersWindow();
        if (chaptersWindow != null && chaptersWindow.Book == book)
            chaptersWindow.SetNotListening();
    }

    private void Edit_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = BooksListBox != null && BooksListBox.SelectedItem != null;
        if (!IsVisible)
            return;
        var bitmap = App.GetBitmapImage(
            e.CanExecute ? @"Images\Buttons\Enabled\Edit.png" : @"Images\Buttons\Disabled\Edit.png");
        ((Image)EditButton.Content).Source = bitmap;
        ((Image)EditMenuItem.Icon).Source = bitmap;
        ((Image)EditContextMenuItem.Icon).Source = bitmap;
    }

    private void Edit_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var book = (Book)BooksListBox.SelectedItem;
        var editor = new BookEditor(book, null) { Owner = this };
        if (editor.ShowDialog() != true)
            return;
        var bookInfoWindow = App.FindBookInfoWindow();
        if (bookInfoWindow != null && bookInfoWindow.Book == book)
            bookInfoWindow.UpdateBook();
        var bookmarksWindow = App.FindBookmarksWindow();
        if (bookmarksWindow != null && bookmarksWindow.Book == book)
            bookmarksWindow.UpdateAuthorsAndTitle();
        var chaptersWindow = App.FindChaptersWindow();
        if (chaptersWindow != null && chaptersWindow.Book == book)
            chaptersWindow.UpdateAuthorsAndTitle();
        if (editor.TitleChanged || editor.AuthorsChanged ||
            editor.CycleChanged || editor.CycleNumbersChanged || editor.GenresChanged)
        {
            UpdateShownBooks();
            SelectBookInShownBooks(book);
            if (editor.TitleChanged && Player.Book == book)
                Player.TitleTextBlock.Text = book.Title;
        }
        if (editor.FileChanged && Player.Book == book)
        {
            Player.PlayOnLoad = false;
            Player.Book = book;
        }
        book.AuthorsChanged();
    }

    private void Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = BooksListBox != null && BooksListBox.SelectedItem != null;
        if (!IsVisible)
            return;
        var bitmap = App.GetBitmapImage(
            e.CanExecute ? @"Images\Buttons\Enabled\Delete.png" : @"Images\Buttons\Disabled\Delete.png");
        ((Image)DeleteButton.Content).Source = bitmap;
        ((Image)DeleteMenuItem.Icon).Source = bitmap;
        ((Image)DeleteContextMenuItem.Icon).Source = bitmap;
    }

    private void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var book = (Book)BooksListBox.SelectedItem;
        if (MessageBox.Show($"Удалить книгу \"{book.Title}\" из библиотеки?", Title,
                            MessageBoxButton.YesNo) != MessageBoxResult.Yes)
        {
            return;
        }
        if (!Library.DeleteBook(book))
        {
            MessageBox.Show($"Не удалось удалить книгу \"{book.Title}\" из библиотеки.", Title);
            return;
        }
        if (Player.Book == book)
            Player.Book = null;
        UpdateShownBooks();
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

    #endregion

    #region Обработчики команд авторов.

    private void Authors_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var editor = new AuthorsEditor() { Owner = this };
        editor.ShowDialog();
    }

    private void AuthorInfo_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var author = (Author)AuthorsListBox.SelectedItem;
        new AuthorInfoDialog(author) { Owner = this }.ShowDialog();
    }

    private void AuthorEdit_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var author = (Author)AuthorsListBox.SelectedItem;
        var editor = new AuthorEditor(author) { Owner = this };
        if (editor.ShowDialog() != true || !editor.NameChanged)
            return;
        UpdateNavPanel(true, false, false);
    }

    private void AuthorDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = AuthorsListBox != null && AuthorsListBox.SelectedItem != null &&
                       !Library.AuthorHasBooks(((Author)AuthorsListBox.SelectedItem).AuthorId);
        if (!IsVisible)
            return;
        var bitmap = App.GetBitmapImage(
            e.CanExecute ? @"Images\Buttons\Enabled\Delete.png" : @"Images\Buttons\Disabled\Delete.png");
        ((Image)AuthorDeleteContextMenuItem.Icon).Source = bitmap;
    }

    private void AuthorDelete_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var author = (Author)AuthorsListBox.SelectedItem;
        if (!App.ConfirmAction($"Удалить автора \"{author.NameLastFirstMiddle}\" из библиотеки?", Title))
        {
            return;
        }
        if (!Library.DeleteAuthor(author))
        {
            MessageBox.Show("Не удалось удалить автора.", Title);
            return;
        }
        Authors.Remove(author);
        UpdateNavPanel(true, false, false);
    }

    #endregion

    #region Обработчики команд серий.

    private void Cycles_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var editor = new CyclesEditor() { Owner = this };
        editor.ShowDialog();
    }

    private void CycleInfo_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var cycle = (Cycle)CyclesListBox.SelectedItem;
        new CycleInfoDialog(cycle) { Owner = this }.ShowDialog();
    }

    private void CycleEdit_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var cycle = (Cycle)CyclesListBox.SelectedItem;
        var editor = new CycleEditor(cycle) { Owner = this };
        if (editor.ShowDialog() != true || !editor.TitleChanged)
            return;
        UpdateNavPanel(false, true, false);
    }

    private void CycleDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = CyclesListBox != null && CyclesListBox.SelectedItem != null &&
                       !Library.CycleHasBooks(((Cycle)CyclesListBox.SelectedItem).CycleId);
        if (!IsVisible)
            return;
        var bitmap = App.GetBitmapImage(
            e.CanExecute ? @"Images\Buttons\Enabled\Delete.png" : @"Images\Buttons\Disabled\Delete.png");
        ((Image)CycleDeleteContextMenuItem.Icon).Source = bitmap;
    }

    private void CycleDelete_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var cycle = (Cycle)CyclesListBox.SelectedItem;
        if (!App.ConfirmAction($"Удалить серию \"{cycle.Title}\" из библиотеки?", Title))
        {
            return;
        }
        if (!Library.DeleteCycle(cycle))
        {
            MessageBox.Show("Не удалось удалить серию.", Title);
            return;
        }
        Cycles.Remove(cycle);
        UpdateNavPanel(false, true, false);
    }

    #endregion

    #region Обработчики команд жанров.

    private void Genres_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var editor = new GenresEditor() { Owner = this };
        editor.ShowDialog();
    }

    private void GenreEdit_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var genre = (Genre)GenresListBox.SelectedItem;
        var editor = new GenreEditor(genre) { Owner = this };
        if (editor.ShowDialog() != true)
            return;
        UpdateNavPanel(false, false, true);
    }

    private void GenreDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = GenresListBox != null && GenresListBox.SelectedItem != null &&
                       !Library.GenreHasBooks(((Genre)GenresListBox.SelectedItem).GenreId);
        if (!IsVisible)
            return;
        var bitmap = App.GetBitmapImage(
            e.CanExecute ? @"Images\Buttons\Enabled\Delete.png" : @"Images\Buttons\Disabled\Delete.png");
        ((Image)GenreDeleteContextMenuItem.Icon).Source = bitmap;
    }

    private void GenreDelete_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var genre = (Genre)GenresListBox.SelectedItem;
        if (!App.ConfirmAction($"Удалить жанр \"{genre.Title}\" из библиотеки?", Title))
        {
            return;
        }
        if (!Library.DeleteGenre(genre))
        {
            MessageBox.Show("Не удалось удалить жанр.", Title);
            return;
        }
        Genres.Remove(genre);
        UpdateNavPanel(false, false, true);
    }

    #endregion

    #region Обработчики команд справки.

    private void About_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        new AboutDialog() { Owner = this }.ShowDialog();
    }

    #endregion
}