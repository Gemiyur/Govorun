using System.Diagnostics;
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

#region Задачи (TODO).

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

    private readonly ObservableCollectionEx<Cycle> Cycles = [];

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
        Cycles.AddRange(Db.GetCycles());
        CyclesListBox.ItemsSource = Cycles;
        ShownBooks.AddRange(Library.Books);
        BooksListBox.ItemsSource = ShownBooks;
        UpdateStatusBarBooksCount();
        Player.IsEnabled = false;
        LoadLastBook();
        CheckCreatorM4BExists();
    }

    /// <summary>
    /// Проверяет существование приложения создания файла M4B.<br/>
    /// Если приложение не найдено, то присваивает приложению в настройках пустую строку.
    /// </summary>
    private static void CheckCreatorM4BExists()
    {
        if (!CreatorM4BExists)
            Properties.Settings.Default.CreatorM4B = string.Empty;
    }

    /// <summary>
    /// Возвращает существует ли приложение создания файла M4B.
    /// </summary>
    /// <returns></returns>
    private static bool CreatorM4BExists => File.Exists(Properties.Settings.Default.CreatorM4B);

    /// <summary>
    /// Загружает в проигрыватель книгу, которая воспроизводилась при закрытии приложения.
    /// Книга только загружается, но не воспроизводится.
    /// </summary>
    /// <remarks>Используется при запуске приложения.</remarks>
    private void LoadLastBook()
    {
        if (!Properties.Settings.Default.LoadLastBook)
            return;
        var lastBookFileName = Properties.Settings.Default.LastBook;
        if (string.IsNullOrWhiteSpace(lastBookFileName))
            return;
        var lastBook = Library.GetBookWithFile(lastBookFileName);
        if (lastBook == null)
            return;
        Player.PlayOnLoad = false;
        Player.Book = lastBook;
    }

    /// <summary>
    /// Запускает и выполняет диалоговое окно информации о книге.
    /// </summary>
    /// <param name="book"></param>
    private void RunBookInfoDialog(Book book)
    {
        var dialog = new BookInfoDialog(book) { Owner = this };
        if (dialog.ShowDialog() != true || book == Player.Book)
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
        if (book == null || Player.MediaFailed)
            return;
        var position = Player.Player.Position < Player.Player.NaturalDuration.TimeSpan
            ? Player.Player.Position
            : TimeSpan.Zero;
        book.PlayPosition = position;
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
        var filename = book != null && !Player.MediaFailed ? book.FileName : "";
        Properties.Settings.Default.LastBook = filename;
    }

    /// <summary>
    /// Сохраняет громкость проигрывателя в настройках приложения.
    /// </summary>
    private void SavePlayerVolume()
    {
        Properties.Settings.Default.PlayerVolume = (int)(Player.Player.Volume * 100);
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
        if (AllBooksToggleButton.IsChecked == true)
        {
            ShownBooks.ReplaceRange(Library.Books);
            return;
        }
        if (ListeningBooksToggleButton.IsChecked == true)
        {
            ShownBooks.ReplaceRange(Library.GetListeningBooks());
            return;
        }
        if (AuthorsListBox.SelectedItem != null)
        {
            var author = (Author)AuthorsListBox.SelectedItem;
            var books = Library.GetAuthorBooks(author.AuthorId);
            ShownBooks.ReplaceRange(books);
            return;
        }
        if (CyclesListBox.SelectedItem != null)
        {
            var cycle = (Cycle)CyclesListBox.SelectedItem;
            var books = Library.GetCycleBooks(cycle.CycleId);
            ShownBooks.ReplaceRange(books);
            return;
        }
        // TODO: Тут будет ещё условие для тегов.

    }

    /// <summary>
    /// Обновляет количество отображаемых книг в строке статуса.
    /// </summary>
    private void UpdateStatusBarBooksCount() => StatusBarBooksCount.Text = BooksListBox.Items.Count.ToString();

    private void Window_Closed(object sender, EventArgs e)
    {
        SaveBookPlayPosition();
        SaveLastBook();
        SavePlayerVolume();
        Properties.Settings.Default.Save();
    }

    private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    #region Обработчики событий элемента списка книг.

    private void BooksListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (BooksListBox.SelectedItem != null && (e.OriginalSource is TextBlock || e.OriginalSource is Border))
            RunBookInfoDialog((Book)BooksListBox.SelectedItem);
    }

    private void BooksListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        StatusBarSelectedCount.Text = BooksListBox.SelectedItems.Count.ToString();
    }

    #endregion

    #region Обработчики событий элементов панели навигации.

    /// <summary>
    /// Блокируются ли обработчики событий элементов панели навигации.
    /// </summary>
    private bool NavHandlersLocked;

    private void AllBooksToggleButton_Click(object sender, RoutedEventArgs e)
    {
        if (AllBooksToggleButton.IsChecked != true)
        {
            AllBooksToggleButton.IsChecked = true;
            return;
        }
        ListeningBooksToggleButton.IsChecked = false;
        NavHandlersLocked = true;
        AuthorsListBox.SelectedIndex = -1;
        CyclesListBox.SelectedIndex = -1;
        NavHandlersLocked = false;
        UpdateShownBooks();
        //SortShownBooks();
    }

    private void ListeningBooksToggleButton_Click(object sender, RoutedEventArgs e)
    {
        if (ListeningBooksToggleButton.IsChecked != true)
        {
            ListeningBooksToggleButton.IsChecked = true;
            return;
        }
        AllBooksToggleButton.IsChecked = false;
        NavHandlersLocked = true;
        AuthorsListBox.SelectedIndex = -1;
        CyclesListBox.SelectedIndex = -1;
        NavHandlersLocked = false;
        UpdateShownBooks();
        //SortShownBooks();
    }

    private void AuthorsExpander_Collapsed(object sender, RoutedEventArgs e)
    {
        AuthorsListBox.Visibility = Visibility.Collapsed;
    }

    private void AuthorsExpander_Expanded(object sender, RoutedEventArgs e)
    {
        AuthorsListBox.Visibility = Visibility.Visible;
    }

    private void AuthorsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (NavHandlersLocked)
            return;
        AllBooksToggleButton.IsChecked = AuthorsListBox.SelectedIndex < 0;
        ListeningBooksToggleButton.IsChecked = false;
        NavHandlersLocked = true;
        CyclesListBox.SelectedIndex = -1;
        NavHandlersLocked = false;
        UpdateShownBooks();
    }

    private void CyclesExpander_Collapsed(object sender, RoutedEventArgs e)
    {
        CyclesListBox.Visibility = Visibility.Collapsed;
    }

    private void CyclesExpander_Expanded(object sender, RoutedEventArgs e)
    {
        CyclesListBox.Visibility = Visibility.Visible;
    }

    private void CyclesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (NavHandlersLocked)
            return;
        AllBooksToggleButton.IsChecked = CyclesListBox.SelectedIndex < 0;
        ListeningBooksToggleButton.IsChecked = false;
        NavHandlersLocked = true;
        AuthorsListBox.SelectedIndex = -1;
        NavHandlersLocked = false;
        UpdateShownBooks();
    }

    #endregion

    #region Обработчики команд группы "Книга".

    private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = BooksListBox != null && BooksListBox.SelectedItems.Count == 1;
        if (!IsVisible)
            return;
        var bitmap = App.GetBitmapImage(
            e.CanExecute ? @"Images\Buttons\Enabled\Play.png" : @"Images\Buttons\Disabled\Play.png");
        ((Image)PlayButton.Content).Source = bitmap;
        ((Image)PlayMenuItem.Icon).Source = bitmap;
    }

    private void Play_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var book = (Book)BooksListBox.SelectedItem;
        if (book == Player.Book)
            return;
        SaveBookPlayPosition();
        Player.Book = book;
    }

    private void Info_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = BooksListBox != null && BooksListBox.SelectedItems.Count == 1;
        if (!IsVisible)
            return;
        var bitmap = App.GetBitmapImage(
            e.CanExecute ? @"Images\Buttons\Enabled\Info.png" : @"Images\Buttons\Disabled\Info.png");
        ((Image)InfoButton.Content).Source = bitmap;
        ((Image)InfoMenuItem.Icon).Source = bitmap;
    }

    private void Info_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        RunBookInfoDialog((Book)BooksListBox.SelectedItem);
    }

    private void Chapters_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = BooksListBox != null && BooksListBox.SelectedItems.Count == 1 &&
            ((Book)BooksListBox.SelectedItem).Chapters.Any();
        if (!IsVisible)
            return;
        var bitmap = App.GetBitmapImage(
            e.CanExecute ? @"Images\Buttons\Enabled\Chapters.png" : @"Images\Buttons\Disabled\Chapters.png");
        ((Image)ChaptersButton.Content).Source = bitmap;
        ((Image)ChaptersMenuItem.Icon).Source = bitmap;
    }

    private void Chapters_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var book = (Book)BooksListBox.SelectedItem;
        var dialog = new ChaptersDialog(book) { Owner = this };
        if (dialog.ShowDialog() != true || dialog.Chapter == null)
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
        e.CanExecute = BooksListBox != null && BooksListBox.SelectedItems.Count == 1 &&
            ((Book)BooksListBox.SelectedItem).Bookmarks.Any();
        if (!IsVisible)
            return;
        var bitmap = App.GetBitmapImage(
            e.CanExecute ? @"Images\Buttons\Enabled\Bookmarks.png" : @"Images\Buttons\Disabled\Bookmarks.png");
        ((Image)BookmarksButton.Content).Source = bitmap;
        ((Image)BookmarksMenuItem.Icon).Source = bitmap;
    }

    private void Bookmarks_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var book = (Book)BooksListBox.SelectedItem;
        var dialog = new BookmarksDialog(book) { Owner = this };
        var dialogResult = dialog.ShowDialog() == true;
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
        e.CanExecute = BooksListBox != null && BooksListBox.SelectedItems.Count == 1;
        if (!IsVisible)
            return;
        var bitmap = App.GetBitmapImage(
            e.CanExecute ? @"Images\Buttons\Enabled\Edit.png" : @"Images\Buttons\Disabled\Edit.png");
        ((Image)EditButton.Content).Source = bitmap;
        ((Image)EditMenuItem.Icon).Source = bitmap;
    }

    private void Edit_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var book = (Book)BooksListBox.SelectedItem;
        var editor = new BookEditor(book, null) { Owner = this };
        if (editor.ShowDialog() != true)
            return;
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
            BooksListBox.SelectedItem = book;
            BooksListBox.ScrollIntoView(BooksListBox.SelectedItem);
            if (Player.Book == book)
                Player.TitleTextBlock.Text = book.Title;
        }
        if (editor.FileChanged && Player.Book == book)
            Player.Book = book;
        book.OnPropertyChanged("AuthorNamesLastFirst");
        book.OnPropertyChanged("AuthorNamesFirstLast");
    }

    private void Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        // TODO: Нужна ли возможность удаления нескольких книг? Сейчас можно.
        e.CanExecute = BooksListBox != null && BooksListBox.SelectedItems.Count > 0;
        if (!IsVisible)
            return;
        var bitmap = App.GetBitmapImage(
            e.CanExecute ? @"Images\Buttons\Enabled\Delete.png" : @"Images\Buttons\Disabled\Delete.png");
        ((Image)DeleteButton.Content).Source = bitmap;
        ((Image)DeleteMenuItem.Icon).Source = bitmap;
    }

    private void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (BooksListBox.SelectedItems.Count == 1)
        {
            var book = (Book)BooksListBox.SelectedItem;
            if (MessageBox.Show($"Удалить книгу \"{book.Title}\" из библиотеки?", Title,
                                MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                return;
            }
            if (Player.Book == book)
                Player.Book = null;
            if (!Library.DeleteBook(book))
            {
                MessageBox.Show($"Не удалось удалить книгу \"{book.Title}\" из библиотеки?", Title);
                return;
            }
            ShownBooks.Remove(book);
            UpdateStatusBarBooksCount();
            return;
        }
        var books = BooksListBox.SelectedItems.Cast<Book>().ToList();
        if (MessageBox.Show("Удалить выбранные книги из библиотеки?", Title,
                            MessageBoxButton.YesNo) != MessageBoxResult.Yes)
        {
            return;
        }
        if (Player.Book != null && books.Contains(Player.Book))
            Player.Book = null;
        var deletedBooks = Library.DeleteBooks(books);
        ShownBooks.RemoveRange(deletedBooks);
        UpdateStatusBarBooksCount();
        // TODO: Надо ли выдавать сообщение, что не все книги были удалены? Пока закомментировано.
        //if (deletedBooks.Count != books.Count)
        //{
        //    MessageBox.Show("Не удалось удалить некоторые книги из библиотеки?", Title);
        //}
    }

    #endregion

    #region Обработчики команд группы "Библиотека".

    private void Listening_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var dialog = new ListeningDialog() { Owner = this };
        if (dialog.ShowDialog() != true || dialog.BookForPlay == Player.Book)
            return;
        SaveBookPlayPosition();
        Player.Book = dialog.BookForPlay;
    }

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
        if (editor.HasNewAuthors)
            UpdateAuthors();
        if (AuthorsListBox.SelectedItem != null &&
            !Library.BookHasAuthor(book, ((Author)AuthorsListBox.SelectedItem).AuthorId))
        {
            AuthorsListBox.SelectedItem = null;
        }
        ShownBooks.Add(book);
        SortShownBooks();
        BooksListBox.SelectedItem = book;
        BooksListBox.ScrollIntoView(AuthorsListBox.SelectedItem);
        book.OnPropertyChanged("AuthorNamesLastFirst");
        book.OnPropertyChanged("AuthorNamesFirstLast");
    }

    private void FindBooks_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var folderDialog = App.PickBooksFolderDialog;
        if (folderDialog.ShowDialog() != true)
            return;
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
        var dialog = new AddBooksDialog(files) { Owner = this };
        dialog.ShowDialog();
        if (!dialog.AddedBooks.Any())
            return;
        Library.Books.AddRange(dialog.AddedBooks);
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
    }

    private void CheckLibrary_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var dialog = new CheckLibraryDialog() { Owner = this };
        dialog.ShowDialog();
        if (dialog.DeletedBooks.Any())
        {
            if (Player.Book != null && dialog.DeletedBooks.Contains(Player.Book))
                Player.Book = null;
            var deletedBooks = Library.DeleteBooks(dialog.DeletedBooks);
            ShownBooks.RemoveRange(deletedBooks);
            UpdateStatusBarBooksCount();
            // TODO: Надо ли выдавать сообщение, что не все книги были удалены? Пока закомментировано.
            //if (deletedBooks.Count != dialog.DeletedBooks.Count)
            //{
            //    MessageBox.Show("Не удалось удалить некоторые книги из библиотеки?", Title);
            //}
        }
        if (dialog.ChangedBooks.Any())
        {
            Db.UpdateBooks(dialog.ChangedBooks);
            if (Player.Book == null)
                return;
            var book = dialog.ChangedBooks.Find(x => x == Player.Book);
            if (book != null)
                Player.Book = book;
        }
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
        new SettingsDialog() { Owner = this }.ShowDialog();
    }

    #endregion

    #region Обработчики команд группы "Инструменты".

    private void CreateM4B_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = CreatorM4BExists;
        if (!IsVisible)
            return;
        var bitmap = App.GetBitmapImage(
            e.CanExecute ? @"Images\Buttons\Enabled\CreateM4B.png" : @"Images\Buttons\Disabled\CreateM4B.png");
        ((Image)CreateM4BButton.Content).Source = bitmap;
        ((Image)CreateM4BMenuItem.Icon).Source = bitmap;
    }

    private void CreateM4B_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var path = Properties.Settings.Default.CreatorM4B;
        try
        {
            Process.Start(path);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось запустить приложение создания M4B файла.{ex.Message}", Title);
        }
    }

    #endregion

    #region Обработчики команд группы "Справка".

    private void About_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        new AboutDialog() { Owner = this }.ShowDialog();
    }

    #endregion
}