using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Gemiyur.Collections;
using Govorun.Media;
using Govorun.Models;
using Govorun.Tools;

namespace Govorun.Dialogs;

/// <summary>
/// Класс окна редактора книги.
/// </summary>
public partial class BookEditor : Window
{
    /// <summary>
    /// Были ли изменения в авторах книги.
    /// </summary>
    public bool AuthorsChanged;

    /// <summary>
    /// Была ли изменена серия книги.
    /// </summary>
    public bool CycleChanged;

    /// <summary>
    /// Был ли изменён номер книги в серии.
    /// </summary>
    public bool CycleNumbersChanged;

    /// <summary>
    /// Было ли изменено имя файла книги.
    /// </summary>
    public bool FileChanged;

    /// <summary>
    /// Были ли изменения в жанрах книги.
    /// </summary>
    public bool GenresChanged;

    /// <summary>
    /// Было ли изменено название книги.
    /// </summary>
    public bool TitleChanged;

    /// <summary>
    /// Редактируемая книга.
    /// </summary>
    private readonly Book book;

    /// <summary>
    /// Имя файла книги.
    /// </summary>
    private string filename;

    /// <summary>
    /// Данные книги из тега файла книги.
    /// </summary>
    private TrackData trackData;

    /// <summary>
    /// Список авторов книги.
    /// </summary>
    private readonly ObservableCollectionEx<Author> authors = [];

    /// <summary>
    /// Серия книги.
    /// </summary>
    private Cycle? cycle;

    /// <summary>
    /// Список жанров книги.
    /// </summary>
    private readonly ObservableCollectionEx<Genre> genres = [];

    /// <summary>
    /// Инициализирует новый экземпляр класса.
    /// </summary>
    /// <param name="book">Книга.</param>
    /// <param name="trackData">Данные трека файла книги.</param>
    /// <exception cref="ArgumentException"></exception>
    public BookEditor(Book book, TrackData? trackData)
    {
        InitializeComponent();
        if (book == null)
        {
            MessageBox.Show("Не указана книга: book == null.", Title);
            throw new ArgumentException("Не указана книга: book == null.", nameof(book));
        }
        this.book = book;
        this.trackData = trackData ?? new TrackData(book.FileName);
        filename = book.FileName;
        FileNotFoundTextBlock.Visibility = book.FileExists ? Visibility.Collapsed : Visibility.Visible;
        FileButton.IsEnabled = !book.FileExists;
        LoadBook();
        LoadTrack();
    }

    /// <summary>
    /// Загружает книгу в редактор.
    /// </summary>
    private void LoadBook()
    {
        TitleTextBox.Text = book.Title;
        authors.AddRange(book.Authors);
        SortAuthors();
        AuthorsListBox.ItemsSource = authors;
        FileNameTextBox.Text = book.FileName;
        AnnotationTextBox.Text = book.Annotation;
        SetCycle(book.Cycle);
        CycleNumbersTextBox.Text = book.CycleNumbers;
        LectorTextBox.Text = book.Lector;
        TranslatorTextBox.Text = book.Translator;
        genres.AddRange(book.Genres);
        SortGenres();
        GenresListBox.ItemsSource = genres;
    }

    /// <summary>
    /// Загружает данные трека файла книги в редактор.
    /// </summary>
    private void LoadTrack()
    {
        TrackTitleTextBox.Text = trackData.Title;
        TrackAuthorTextBox.Text = trackData.Author;
        TrackCycleTitleTextBox.Text = trackData.CycleTitle;
        TrackCyclePartTextBox.Text = trackData.CyclePart;

        var comments = trackData.Comment;

        if (comments.Length == 0)
            comments = trackData.Description;
        else if (trackData.Description.Length > 0)
            comments += "\r\n" + trackData.Description;

        if (comments.Length == 0)
            comments = trackData.LongDescription;
        else if (trackData.LongDescription.Length > 0)
            comments += "\r\n" + trackData.LongDescription;

        if (comments.Length == 0)
            comments = trackData.Lyrics;
        else if (trackData.Lyrics.Length > 0)
            comments += "\r\n" + trackData.Lyrics;

        TrackCommentsTextBox.Text = comments;
    }

    /// <summary>
    /// Сохраняет данные из редактора в редактируемую книгу.
    /// </summary>
    /// <returns>Были ли внесены изменения в книгу.</returns>
    private bool SaveBook()
    {
        // В книге есть изменения?
        var changed = false;

        // Новая книга.
        changed = book.BookId < 1;

        // Название.
        if (book.Title != TitleTextBox.Text)
        {
            book.Title = TitleTextBox.Text;
            changed = true;
            TitleChanged = true;
        }

        // Авторы.
        if (authors.Count != book.Authors.Count ||
            authors.Any(x => !book.Authors.Exists(a => a.AuthorId == x.AuthorId)) ||
            book.Authors.Any(x => !authors.Any(a => a.AuthorId == x.AuthorId)))
        {
            book.Authors.Clear();
            book.Authors.AddRange(authors);
            changed = true;
            AuthorsChanged = true;
        }

        // Аннотация.
        if (book.Annotation != AnnotationTextBox.Text)
        {
            book.Annotation = AnnotationTextBox.Text;
            changed = true;
        }

        // Серия.
        if ((cycle == null && book.Cycle != null) ||
            (cycle != null && book.Cycle == null))
        {
            book.Cycle = cycle;
            changed = true;
            CycleChanged = true;
        }
        else
        {
            if (cycle != null && book.Cycle != null &&
                cycle.CycleId != book.Cycle.CycleId)
            {
                book.Cycle = cycle;
                changed = true;
                CycleChanged = true;
            }
        }

        // Номера в серии.
        if (book.CycleNumbers != CycleNumbersTextBox.Text)
        {
            book.CycleNumbers = CycleNumbersTextBox.Text;
            changed = true;
            CycleNumbersChanged = true;
        }

        // Чтец.
        if (book.Lector != LectorTextBox.Text.Trim())
        {
            book.Lector = LectorTextBox.Text.Trim();
            changed = true;
        }

        // Переводчик.
        if (book.Translator != TranslatorTextBox.Text.Trim())
        {
            book.Translator = TranslatorTextBox.Text.Trim();
            changed = true;
        }

        // Жанры.
        if (genres.Count != book.Genres.Count ||
            genres.Any(x => !book.Genres.Exists(g => g.GenreId == x.GenreId)) ||
            book.Genres.Any(x => !genres.Any(g => g.GenreId == x.GenreId)))
        {
            book.Genres.Clear();
            book.Genres.AddRange(genres);
            changed = true;
            GenresChanged = true;
        }

        // Имя файла.
        if (book.FileName != filename)
        {
            book.FileName = filename;
            changed = true;
            FileChanged = true;
        }

        // Возврат результата: были ли внесены изменения в книгу.
        return changed;
    }

    /// <summary>
    /// Устанавливает серию книги.
    /// </summary>
    /// <param name="value">Серия книги.</param>
    private void SetCycle(Cycle? value)
    {
        cycle = value;
        EditCycleButton.IsEnabled = cycle != null;
        RemoveCycleButton.IsEnabled = cycle != null;
        CycleNumbersTextBox.IsEnabled = cycle != null;
        CycleTextBox.Text = cycle != null ? cycle.Title : string.Empty;
    }

    /// <summary>
    /// Сортирует список авторов книги по фамилии, имени и отчеству.
    /// </summary>
    private void SortAuthors() => authors.Sort(x => x.NameLastFirstMiddle, StringComparer.CurrentCultureIgnoreCase);

    /// <summary>
    /// Сортирует список жанров книги в алфавитном порядке.
    /// </summary>
    private void SortGenres() => genres.Sort(x => x.Title, StringComparer.CurrentCultureIgnoreCase);

    #region Обработчики событий элементов названия книги.

    private void TitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        SaveButton.IsEnabled = !string.IsNullOrWhiteSpace(TitleTextBox.Text);
    }

    #endregion

    #region Обработчики событий элементов авторов.

    private void AuthorsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        EditAuthorButton.IsEnabled = AuthorsListBox.SelectedItems.Count == 1;
        RemoveAuthorsButton.IsEnabled = AuthorsListBox.SelectedItems.Count > 0;
    }

    private void PickAuthorsButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new AuthorsPicker() { Owner = this };
        if (picker.ShowDialog() != true)
            return;
        authors.AddRange(picker.PickedAuthors.Where(x => !authors.Any(a => a.AuthorId == x.AuthorId)));
        SortAuthors();
    }

    private void NewAuthorButton_Click(object sender, RoutedEventArgs e)
    {
        var author = new Author();
        var editor = new AuthorEditor(author) { Owner = this };
        if (editor.ShowDialog() != true)
            return;
        authors.Add(author);
        SortAuthors();
        App.GetMainWindow().UpdateNavPanel(true, false, false);
    }

    private void EditAuthorButton_Click(object sender, RoutedEventArgs e)
    {
        var author = (Author)AuthorsListBox.SelectedItem;
        var editor = new AuthorEditor(author) { Owner = this };
        if (editor.ShowDialog() != true || !editor.NameChanged)
            return;
        SortAuthors();
        App.GetMainWindow().UpdateNavPanel(true, false, false);
    }

    private void RemoveAuthorsButton_Click(object sender, RoutedEventArgs e)
    {
        authors.RemoveRange(AuthorsListBox.SelectedItems.Cast<Author>());
    }

    #endregion

    #region Обработчики событий элементов серии.

    private void PickCycleButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new CyclePicker() { Owner = this };
        if (picker.ShowDialog() != true || picker.PickedCycle == null)
            return;
        if (cycle != null && picker.PickedCycle.CycleId == cycle.CycleId)
            return;
        SetCycle(picker.PickedCycle);
        CycleNumbersTextBox.Text = string.Empty;
    }

    private void NewCycleButton_Click(object sender, RoutedEventArgs e)
    {
        var newCycle = new Cycle();
        var editor = new CycleEditor(newCycle) { Owner = this };
        if (editor.ShowDialog() != true)
            return;
        SetCycle(newCycle);
        CycleNumbersTextBox.Text = string.Empty;
        App.GetMainWindow().UpdateNavPanel(false, true, false);
    }

    private void EditCycleButton_Click(object sender, RoutedEventArgs e)
    {
        if (cycle == null)
            return;
        var editor = new CycleEditor(cycle) { Owner = this };
        if (editor.ShowDialog() != true || !editor.TitleChanged)
            return;
        CycleTextBox.Text = cycle.Title;
        App.GetMainWindow().UpdateNavPanel(false, true, false);
    }

    private void RemoveCycleButton_Click(object sender, RoutedEventArgs e)
    {
        SetCycle(null);
        CycleNumbersTextBox.Text = string.Empty;
    }

    private string oldCycleNumbers = string.Empty;

    private bool ValidateCycleNumbers()
    {
        var array = CycleNumbersTextBox.Text.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries);
        foreach (var item in array)
        {
            if (!int.TryParse(item.Trim(), NumberStyles.None, null, out _))
                return false;
        }
        return true;
    }

    private void SortCycleNumbers()
    {
        var array = CycleNumbersTextBox.Text.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries);
        List<int> list = [.. array.Select(int.Parse)];
        list.Sort();
        var result = string.Empty;
        for (int i = 0; i < list.Count; i++)
        {
            if (i < list.Count - 1)
                result += $"{list[i]}, ";
            else
                result += list[i].ToString();
        }
        CycleNumbersTextBox.Text = result;
    }

    private void CycleNumbersTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        SortCycleNumbers();
    }

    private void CycleNumbersTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (oldCycleNumbers == CycleNumbersTextBox.Text)
            return;
        var text = CycleNumbersTextBox.Text;
        if (text == string.Empty)
        {
            oldCycleNumbers = string.Empty;
            CycleNumbersTextBox.Text = oldCycleNumbers;
            return;
        }
        var pos = CycleNumbersTextBox.SelectionStart;
        if (!ValidateCycleNumbers())
        {
            CycleNumbersTextBox.Text = oldCycleNumbers;
            CycleNumbersTextBox.SelectionStart = pos - 1;
        }
        else
        {
            oldCycleNumbers = CycleNumbersTextBox.Text;
        }
    }

    #endregion

    #region Обработчики событий элементов чтеца и переводчика.

    private void PickLectorButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new LectorPicker() { Owner = this };
        if (picker.ShowDialog() != true)
            return;
        LectorTextBox.Text = picker.PickedLector;
    }

    private void PickTranslatorButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new TranslatorPicker() { Owner = this };
        if (picker.ShowDialog() != true)
            return;
        TranslatorTextBox.Text = picker.PickedTranslator;
    }

    #endregion

    #region Обработчики событий элементов жанров.

    private void GenresListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        EditGenreButton.IsEnabled = GenresListBox.SelectedItems.Count == 1;
        RemoveGenresButton.IsEnabled = GenresListBox.SelectedItems.Count > 0;
    }

    private void PickGenresButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new GenresPicker() { Owner = this };
        if (picker.ShowDialog() != true)
            return;
        genres.AddRange(picker.PickedGenres.Where(x => !genres.Any(g => g == x)));
        SortGenres();
    }

    private void NewGenreButton_Click(object sender, RoutedEventArgs e)
    {
        var genre = new Genre();
        var editor = new GenreEditor(genre) { Owner = this };
        if (editor.ShowDialog() != true)
            return;
        genres.Add(genre);
        SortGenres();
        App.GetMainWindow().UpdateNavPanel(false, false, true);
    }

    private void EditGenreButton_Click(object sender, RoutedEventArgs e)
    {
        var genre = (Genre)GenresListBox.SelectedItem;
        var editor = new GenreEditor(genre) { Owner = this };
        if (editor.ShowDialog() != true)
            return;
        SortGenres();
        App.GetMainWindow().UpdateNavPanel(false, false, true);
    }

    private void RemoveGenresButton_Click(object sender, RoutedEventArgs e)
    {
        genres.RemoveRange(GenresListBox.SelectedItems.Cast<Genre>());
    }

    #endregion

    private void FileButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new BookFileDialog(book) { Owner = this };
        if (dialog.ShowDialog() != true)
            return;
        filename = dialog.FileName;
        FileNotFoundTextBlock.Visibility = Visibility.Collapsed;
        trackData = new TrackData(filename);
        LoadTrack();
        FileButton.IsEnabled = false;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!SaveBook())
        {
            DialogResult = false;
            return;
        }
        var saved = book.BookId > 0 ? Library.UpdateBook(book) : Library.AddBook(book);
        if (!saved)
        {
            MessageBox.Show("Не удалось сохранить книгу.", Title);
            DialogResult = false;
            return;
        }
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
