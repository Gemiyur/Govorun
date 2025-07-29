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
    private readonly ObservableCollectionEx<Tag> Tags = [];

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
            MessageBox.Show("Файл базы данных не найден.\nУкажите имя существующего или нового файла.", Title);
            var dialog = App.PickDatabaseDialog;
            dialog.FileName = Path.GetFileName(App.DbName);
            if (dialog.ShowDialog() != true)
            {
                MessageBox.Show("Файл базы данных не выбран.\nПриложение закроется.", Title);
                Close();
            }
            App.DbName = App.EnsureDbExtension(dialog.FileName);
#if DEBUG
            Properties.Settings.Default.DebugDbName = App.DbName;
#else
            Properties.Settings.Default.DbName = App.DbName;
#endif
        }
        Authors.AddRange(Db.GetAuthors());
        AuthorsListBox.ItemsSource = Authors;
        CheckAuthorsNameFormat();
        Cycles.AddRange(Db.GetCycles());
        CyclesListBox.ItemsSource = Cycles;
        Tags.AddRange(Db.GetTags());
        TagsListBox.ItemsSource = Tags;
        ShownBooks.AddRange(Library.Books);
        BooksListBox.ItemsSource = ShownBooks;
        UpdateStatusBarBooksCount();
        Player.IsEnabled = false;
        LoadLastBook();
    }

    /// <summary>
    /// Устанавливает формат отображения имён авторов в панели навигации.
    /// </summary>
    public void CheckAuthorsNameFormat()
    {
        AuthorsListBox.ItemTemplate = Properties.Settings.Default.NavAuthorFullName
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
        Db.UpdateBook(book);
        book.OnPropertyChanged("PlayPosition");
    }

    /// <summary>
    /// Сохраняет имя файла воспроизводимой книги в настройках приложения.
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
    /// Отображает окно информации о книге.
    /// </summary>
    /// <param name="book">Книга.</param>
    public void ShowBookInfo(Book book)
    {
        var window = App.FindBookInfoWindow();
        if (window != null)
        {
            if (window.Book != book)
                window.Book = book;
            App.RestoreWindow(window);
            window.Activate();
        }
        else
        {
            new BookInfoDialog(book) { Owner = this }.Show();
        }
    }

    /// <summary>
    /// Отображает окно закладок указанной книги.
    /// </summary>
    /// <param name="book">Книга.</param>
    public void ShowBookmarks(Book book)
    {
        var window = App.FindBookmarksWindow();
        if (window != null)
        {
            if (window.Book != book)
                window.Book = book;
            App.RestoreWindow(window);
            window.Activate();
        }
        else
        {
            new BookmarksDialog(book) { Owner = this }.Show();
        }
    }

    /// <summary>
    /// Отображает окно содержания указанной книги.
    /// </summary>
    /// <param name="book">Книга.</param>
    public void ShowChapters(Book book)
    {
        var window = App.FindChaptersWindow();
        if (window != null)
        {
            if (window.Book != book)
                window.Book = book;
            else if (window.Book == Player.Book)
                window.SelectCurrentChapter();
            App.RestoreWindow(window);
            window.Activate();
        }
        else
        {
            new ChaptersDialog(book) { Owner = this }.Show();
        }
    }

    /// <summary>
    /// Обновляет списки панели навигации.
    /// </summary>
    /// <param name="authors">Обновить список авторов.</param>
    /// <param name="cycles">Обновить список серий.</param>
    /// <param name="tags">Обновить список тегов.</param>
    private void UpdateNavPanel(bool authors, bool cycles, bool tags)
    {
        LockNavHandlers();
        if (authors)
        {
            var selectedAuthor = (Author)AuthorsListBox.SelectedItem;
            Authors.ReplaceRange(Db.GetAuthors());
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
            Cycles.ReplaceRange(Db.GetCycles());
            if (selectedCycle != null)
            {
                CyclesListBox.SelectedItem = Cycles.FirstOrDefault(x => x.CycleId == selectedCycle.CycleId);
                if (CyclesListBox.SelectedItem != null)
                    CyclesListBox.ScrollIntoView(CyclesListBox.SelectedItem);
            }
        }
        if (tags)
        {
            var selectedTag = (Tag)TagsListBox.SelectedItem;
            Tags.ReplaceRange(Db.GetTags());
            if (selectedTag != null)
            {
                TagsListBox.SelectedItem = Tags.FirstOrDefault(x => x.Equals(selectedTag));
                if (TagsListBox.SelectedItem != null)
                    TagsListBox.ScrollIntoView(TagsListBox.SelectedItem);
            }
        }
        if (AuthorsListBox.SelectedItem == null &&
            CyclesListBox.SelectedItem == null &&
            TagsListBox.SelectedItem == null &&
            ListeningBooksToggleButton.IsChecked != true)
        {
            AllBooksToggleButton.IsChecked = true;
        }
        UnlockNavHandlers();
    }

    /// <summary>
    /// Обновляет список отображаемых книг.
    /// </summary>
    private void UpdateShownBooks()
    {
        if (AllBooksToggleButton.IsChecked == true)
        {
            ShownBooks.ReplaceRange(Library.Books.OrderBy(x => x.Title, StringComparer.CurrentCultureIgnoreCase));
            BooksListBox.ItemTemplate = (DataTemplate)FindResource("BookDataTemplate");
        }
        else if (ListeningBooksToggleButton.IsChecked == true)
        {
            ShownBooks.ReplaceRange(Library.ListeningBooks);
            BooksListBox.ItemTemplate = (DataTemplate)FindResource("BookDataTemplate");
        }
        else if (AuthorsListBox.SelectedItem != null)
        {
            var author = (Author)AuthorsListBox.SelectedItem;
            var books = Library.GetAuthorBooks(author.AuthorId);
            ShownBooks.ReplaceRange(books);
            BooksListBox.ItemTemplate = (DataTemplate)FindResource("BookDataTemplate");
        }
        else if (CyclesListBox.SelectedItem != null)
        {
            var cycle = (Cycle)CyclesListBox.SelectedItem;
            var books = Library.GetCycleBooks(cycle.CycleId);
            ShownBooks.ReplaceRange(books);
            BooksListBox.ItemTemplate = (DataTemplate)FindResource("BookCycleDataTemplate");
        }
        else if (TagsListBox.SelectedItem != null)
        {
            var tag = (Tag)TagsListBox.SelectedItem;
            var books = Library.GetTagBooks(tag.TagId);
            ShownBooks.ReplaceRange(books);
            BooksListBox.ItemTemplate = (DataTemplate)FindResource("BookDataTemplate");
        }
        UpdateStatusBarBooksCount();
    }

    /// <summary>
    /// Обновляет количество отображаемых книг в строке статуса.
    /// </summary>
    private void UpdateStatusBarBooksCount() => StatusBarBooksCount.Text = BooksListBox.Items.Count.ToString();

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
        TagsListBox.SelectedIndex = -1;
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
        UnlockNavHandlers();
        UpdateShownBooks();
    }

    private void AuthorsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (NavHandlersLocked)
            return;
        AllBooksToggleButton.IsChecked = AuthorsListBox.SelectedIndex < 0;
        ListeningBooksToggleButton.IsChecked = false;
        LockNavHandlers();
        CyclesListBox.SelectedIndex = -1;
        TagsListBox.SelectedIndex = -1;
        UnlockNavHandlers();
        UpdateShownBooks();
    }

    private void CyclesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (NavHandlersLocked)
            return;
        AllBooksToggleButton.IsChecked = CyclesListBox.SelectedIndex < 0;
        ListeningBooksToggleButton.IsChecked = false;
        LockNavHandlers();
        AuthorsListBox.SelectedIndex = -1;
        TagsListBox.SelectedIndex = -1;
        UnlockNavHandlers();
        UpdateShownBooks();
    }

    private void TagsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (NavHandlersLocked)
            return;
        AllBooksToggleButton.IsChecked = TagsListBox.SelectedIndex < 0;
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
            ShowBookInfo((Book)BooksListBox.SelectedItem);
    }

    #endregion

    #region Обработчики команд группы "Библиотека".

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
        Library.Books.Add(book);
        UpdateNavPanel(editor.HasNewAuthors, editor.HasNewCycle, editor.TagsChanged);
        UpdateShownBooks();
        SelectBookInShownBooks(book);
    }

    private void FindBooks_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var folderDialog = App.PickBooksFolderDialog;
        if (folderDialog.ShowDialog() != true)
            return;
        StatusBarAction.Text = "Поиск файлов книг...";
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
        if (!dialog.AddedBooks.Any())
            return;
        Library.Books.AddRange(dialog.AddedBooks);
        UpdateNavPanel(dialog.HasNewAuthors, dialog.HasNewCycle, dialog.TagsChanged);
        UpdateShownBooks();
    }

    private void Authors_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var editor = new AuthorsEditor() { Owner = this };
        editor.ShowDialog();
        if (!editor.HasChanges)
            return;
        var selectedItem = AuthorsListBox.SelectedItem;
        UpdateNavPanel(true, false, false);
        if (selectedItem != null && AuthorsListBox.SelectedItem == null)
            UpdateShownBooks();
        foreach (var book in ShownBooks)
        {
            book.OnPropertyChanged("AuthorNamesFirstLast");
            book.OnPropertyChanged("AuthorNamesFirstMiddleLast");
            book.OnPropertyChanged("AuthorNamesLastFirst");
            book.OnPropertyChanged("AuthorNamesLastFirstMiddle");
        }
    }

    private void Cycles_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var editor = new CyclesEditor() { Owner = this };
        editor.ShowDialog();
        if (!editor.HasChanges)
            return;
        var selectedItem = CyclesListBox.SelectedItem;
        UpdateNavPanel(false, true, false);
        if (selectedItem != null && CyclesListBox.SelectedItem == null)
            UpdateShownBooks();
    }

    private void Tags_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var editor = new TagsEditor() { Owner = this };
        editor.ShowDialog();
        if (!editor.HasChanges)
            return;
        var selectedItem = TagsListBox.SelectedItem;
        UpdateNavPanel(false, false, true);
        if (selectedItem != null && TagsListBox.SelectedItem == null)
            UpdateShownBooks();
    }

    private void CheckLibrary_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var dialog = new CheckLibraryDialog() { Owner = this };
        dialog.ShowDialog();
        if (dialog.DeletedBooks.Count > 0)
        {
            var deletedBooks = Library.DeleteBooks(dialog.DeletedBooks);
            if (deletedBooks.Count == 0)
            {
                MessageBox.Show("Не удалось удалить выбранные книги из библиотеки.", Title);
                return;
            }
            if (deletedBooks.Count != dialog.DeletedBooks.Count)
            {
                MessageBox.Show("Не удалось удалить некоторые книги из библиотеки.", Title);
            }
            if (Player.Book != null && deletedBooks.Contains(Player.Book))
                Player.Book = null;
            UpdateNavPanel(false, false, true);
            UpdateShownBooks();
        }
        if (dialog.ChangedBooks.Count > 0)
        {
            var updatedBooks = Library.UpdateBooks(dialog.ChangedBooks);
            if (updatedBooks.Count == 0)
            {
                MessageBox.Show("Не удалось обновить файлы у выбранных книг.", Title);
                return;
            }
            if (updatedBooks.Count != dialog.ChangedBooks.Count)
            {
                MessageBox.Show("Не удалось обновить файлы у некоторых книг.", Title);
            }
            if (Player.Book == null)
                return;
            var book = updatedBooks.Find(x => x == Player.Book);
            if (book != null)
            {
                Player.PlayOnLoad = false;
                Player.Book = book;
            }
        }
    }

    private void Settings_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        new SettingsDialog() { Owner = this }.ShowDialog();
    }

    private void Exit_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        Close();
    }

    #endregion

    #region Обработчики команд группы "Книга".

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
            e.CanExecute ? @"Images\Buttons\Enabled\Info.png" : @"Images\Buttons\Disabled\Info.png");
        ((Image)InfoButton.Content).Source = bitmap;
        ((Image)InfoMenuItem.Icon).Source = bitmap;
        ((Image)InfoContextMenuItem.Icon).Source = bitmap;
    }

    private void Info_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        ShowBookInfo((Book)BooksListBox.SelectedItem);
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
        ShowChapters(book);
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
        ShowBookmarks(book);
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
        Library.UpdateBookWindows(book);
        UpdateNavPanel(editor.HasNewAuthors, editor.HasNewCycle, editor.HasNewTags);
        if (editor.TitleChanged || editor.AuthorsChanged ||
            editor.CycleChanged || editor.CycleNumberChanged || editor.TagsChanged)
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
        book.OnPropertyChanged("AuthorNamesFirstLast");
        book.OnPropertyChanged("AuthorNamesFirstMiddleLast");
        book.OnPropertyChanged("AuthorNamesLastFirst");
        book.OnPropertyChanged("AuthorNamesLastFirstMiddle");
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
        UpdateNavPanel(false, false, true);
        UpdateShownBooks();
    }

    #endregion

    #region Обработчики команд группы "Справка".

    private void About_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        new AboutDialog() { Owner = this }.ShowDialog();
    }

    #endregion
}