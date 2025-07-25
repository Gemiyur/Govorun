﻿using System.Globalization;
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
    public bool CycleNumberChanged;

    /// <summary>
    /// Было ли изменено имя файла книги.
    /// </summary>
    public bool FileChanged;

    /// <summary>
    /// Были ли добавлены новые авторы книг.
    /// </summary>
    public bool HasNewAuthors;

    /// <summary>
    /// Была ли добавлена новая серия книг.
    /// </summary>
    public bool HasNewCycle;

    /// <summary>
    /// Были ли добавлены новые теги книг.
    /// </summary>
    public bool HasNewTags;

    /// <summary>
    /// Были ли изменения в тегах книги.
    /// </summary>
    public bool TagsChanged;

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
    /// Список всех авторов в библиотеке.
    /// </summary>
    private readonly List<Author> allAuthors = Db.GetAuthors();

    /// <summary>
    /// Серия книги.
    /// </summary>
    private Cycle? cycle;

    /// <summary>
    /// Список всех серий в библиотеке.
    /// </summary>
    private readonly List<Cycle> allCycles = Db.GetCycles();

    /// <summary>
    /// Список тегов книги.
    /// </summary>
    private readonly ObservableCollectionEx<Tag> tags = [];

    /// <summary>
    /// Список всех тегов в библиотеке.
    /// </summary>
    private readonly List<Tag> allTags = Db.GetTags();

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
    /// Устанавливает доступность кнопки добавления нового автора.
    /// </summary>
    private void CheckNewAuthorButtons()
    {
        AddNewAuthorButton.IsEnabled =
            !string.IsNullOrWhiteSpace(NewAuthorLastNameTextBox.Text) ||
            !string.IsNullOrWhiteSpace(NewAuthorFirstNameTextBox.Text) ||
            !string.IsNullOrWhiteSpace(NewAuthorMiddleNameTextBox.Text);
        ClearNewAuthorButton.IsEnabled = AddNewAuthorButton.IsEnabled;
    }

    /// <summary>
    /// Очищает поля ввода имени и фамилии нового автора.
    /// </summary>
    private void ClearNewAuthor()
    {
        NewAuthorLastNameTextBox.Text = string.Empty;
        NewAuthorFirstNameTextBox.Text = string.Empty;
        NewAuthorMiddleNameTextBox.Text = string.Empty;
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
        cycle = book.Cycle;
        CycleTextBox.Text = cycle != null ? cycle.Title : string.Empty;
        CyclePartTextBox.Text = book.CyclePart;
        LectorTextBox.Text = book.Lector;
        TranslatorTextBox.Text = book.Translator;
        tags.AddRange(book.Tags);
        SortTags();
        TagsListBox.ItemsSource = tags;
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
        else if (trackData.Description.Length != 0)
            comments = comments + "\r\n" + trackData.Description;

        if (comments.Length == 0)
            comments = trackData.LongDescription;
        else if (trackData.LongDescription.Length != 0)
            comments = comments + "\r\n" + trackData.LongDescription;

        if (comments.Length == 0)
            comments = trackData.Lyrics;
        else if (trackData.Lyrics.Length != 0)
            comments = comments + "\r\n" + trackData.Lyrics;

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

        // Номер в серии.
        int.TryParse(CyclePartTextBox.Text, NumberStyles.None, null, out var cycleNumber);
        if (book.CycleNumber != cycleNumber)
        {
            book.CycleNumber = cycleNumber;
            changed = true;
            CycleNumberChanged = true;
        }

        // Чтец.
        if (book.Lector != LectorTextBox.Text)
        {
            book.Lector = LectorTextBox.Text;
            changed = true;
        }

        // Переводчик.
        if (book.Translator != TranslatorTextBox.Text)
        {
            book.Translator = TranslatorTextBox.Text;
            changed = true;
        }

        // Теги.
        if (tags.Count != book.Tags.Count ||
            tags.Any(x => !book.Tags.Exists(t => t.TagId == x.TagId)) ||
            book.Tags.Any(x => !tags.Any(t => t.TagId == x.TagId)))
        {
            book.Tags.Clear();
            book.Tags.AddRange(tags);
            changed = true;
            TagsChanged = true;
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
    /// Сохраняет новых авторов и присваивает им идентификаторы.
    /// </summary>
    /// <returns>Были ли новые авторы сохранены успешно.</returns>
    private bool SaveNewAuthors()
    {
        var newAuthors = authors.ToList().FindAll(x => x.AuthorId == 0);
        if (newAuthors.Count == 0)
            return true;
        HasNewAuthors = true;
        using var db = Db.GetDatabase();
        foreach (var author in newAuthors)
        {
            author.AuthorId = Db.InsertAuthor(author, db);
            if (author.AuthorId < 1)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Сохраняет новую серию и присваивает ей идентификатор.
    /// </summary>
    /// <returns>Была ли новая серия сохранена успешно.</returns>
    private bool SaveNewCycle()
    {
        if (cycle == null || cycle.CycleId > 0)
            return true;
        HasNewCycle = true;
        cycle.CycleId = Db.InsertCycle(cycle);
        return cycle.CycleId > 0;
    }

    /// <summary>
    /// Сохраняет новые теги и присваивает им идентификаторы.
    /// </summary>
    /// <returns>Были ли новые теги сохранены успешно.</returns>
    private bool SaveNewTags()
    {
        var newTags = tags.ToList().FindAll(x => x.TagId == 0);
        if (newTags.Count == 0)
            return true;
        HasNewTags = true;
        using var db = Db.GetDatabase();
        foreach (var tag in newTags)
        {
            tag.TagId = Db.InsertTag(tag, db);
            if (tag.TagId < 1)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Сортирует список авторов книги по фамилии, имени и отчеству.
    /// </summary>
    private void SortAuthors() => authors.Sort(x => x.NameLastFirstMiddle, StringComparer.CurrentCultureIgnoreCase);

    /// <summary>
    /// Сортирует список тегов книги в алфавитном порядке.
    /// </summary>
    private void SortTags() => tags.Sort(x => x.Title, StringComparer.CurrentCultureIgnoreCase);

    #region Обработчики событий элементов управления.

    private void TitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        SaveButton.IsEnabled = !string.IsNullOrWhiteSpace(TitleTextBox.Text);
    }

    private void AuthorsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
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

    private void RemoveAuthorsButton_Click(object sender, RoutedEventArgs e)
    {
        authors.RemoveRange(AuthorsListBox.SelectedItems.Cast<Author>());
    }

    private void NewAuthorLastNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        CheckNewAuthorButtons();
    }

    private void NewAuthorFirstNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        CheckNewAuthorButtons();
    }

    private void NewAuthorMiddleNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        CheckNewAuthorButtons();
    }

    private void AddNewAuthorButton_Click(object sender, RoutedEventArgs e)
    {
        var lastName = NewAuthorLastNameTextBox.Text.Trim();
        var firstName = NewAuthorFirstNameTextBox.Text.Trim();
        var middleName = NewAuthorMiddleNameTextBox.Text.Trim();

        var author = allAuthors.Find(x => x.LastName.Equals(lastName, StringComparison.CurrentCultureIgnoreCase) &&
                                          x.FirstName.Equals(firstName, StringComparison.CurrentCultureIgnoreCase) &&
                                          x.MiddleName.Equals(middleName, StringComparison.CurrentCultureIgnoreCase));
        if (author != null)
        {
            if (authors.Any(x => x.LastName.Equals(lastName, StringComparison.CurrentCultureIgnoreCase) &&
                                 x.FirstName.Equals(firstName, StringComparison.CurrentCultureIgnoreCase) &&
                                 x.MiddleName.Equals(middleName, StringComparison.CurrentCultureIgnoreCase)))
            {
                ClearNewAuthor();
                return;
            }
            else
            {
                authors.Add(author);
            }
        }
        else
        {
            if (authors.Any(x => x.LastName.Equals(lastName, StringComparison.CurrentCultureIgnoreCase) &&
                                 x.FirstName.Equals(firstName, StringComparison.CurrentCultureIgnoreCase) &&
                                 x.MiddleName.Equals(middleName, StringComparison.CurrentCultureIgnoreCase)))
            {
                ClearNewAuthor();
                return;
            }
            else
            {
                authors.Add(new Author() { LastName = lastName, FirstName = firstName, MiddleName = middleName });
            }
        }
        SortAuthors();
        ClearNewAuthor();
    }

    private void ClearNewAuthorButton_Click(object sender, RoutedEventArgs e)
    {
        ClearNewAuthor();
    }

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

    private void CycleTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        RemoveCycleButton.IsEnabled = !string.IsNullOrWhiteSpace(CycleTextBox.Text);
    }

    private void PickCycleButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new CyclePicker() { Owner = this };
        if (picker.ShowDialog() != true || picker.PickedCycle == null)
            return;
        if (cycle != null && picker.PickedCycle.CycleId == cycle.CycleId)
            return;
        cycle = picker.PickedCycle;
        CycleTextBox.Text = cycle.Title;
        CyclePartTextBox.Text = string.Empty;
    }

    private void RemoveCycleButton_Click(object sender, RoutedEventArgs e)
    {
        cycle = null;
        CycleTextBox.Text = string.Empty;
        CyclePartTextBox.Text = string.Empty;
    }

    private string oldCyclePartText = string.Empty;

    private void CyclePartTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (oldCyclePartText == CyclePartTextBox.Text)
            return;
        var text = CyclePartTextBox.Text;
        if (text == string.Empty)
        {
            oldCyclePartText = string.Empty;
            CyclePartTextBox.Text = oldCyclePartText;
            return;
        }
        var pos = CyclePartTextBox.SelectionStart;
        if (!int.TryParse(text, NumberStyles.None, null, out var value))
        {
            CyclePartTextBox.Text = oldCyclePartText;
            CyclePartTextBox.SelectionStart = pos - 1;
        }
        else
        {
            oldCyclePartText = CyclePartTextBox.Text;
        }
    }

    private void NewCycleTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        AddNewCycleButton.IsEnabled = !string.IsNullOrWhiteSpace(NewCycleTextBox.Text);
    }

    private void AddNewCycleButton_Click(object sender, RoutedEventArgs e)
    {
        var title = NewCycleTextBox.Text.Trim();
        var dbCycle = allCycles.Find(x => x.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase));
        if (dbCycle != null && cycle != null && cycle.CycleId == dbCycle.CycleId)
            return;
        cycle = dbCycle ?? new Cycle() { Title = title };
        CycleTextBox.Text = cycle.Title;
        CyclePartTextBox.Text = string.Empty;
        NewCycleTextBox.Text = string.Empty;
    }

    private void TagsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RemoveTagsButton.IsEnabled = TagsListBox.SelectedItems.Count > 0;
    }

    private void PickTagsButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new TagsPicker() { Owner = this };
        if (picker.ShowDialog() != true)
            return;
        tags.AddRange(picker.PickedTags.Where(x => !tags.Any(t => t == x)));
        SortTags();
    }

    private void RemoveTagsButton_Click(object sender, RoutedEventArgs e)
    {
        tags.RemoveRange(TagsListBox.SelectedItems.Cast<Tag>());
    }

    private void NewTagTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        AddNewTagButton.IsEnabled = !string.IsNullOrWhiteSpace(NewTagTextBox.Text);
    }

    private void AddNewTagButton_Click(object sender, RoutedEventArgs e)
    {
        var title = NewTagTextBox.Text.Trim();
        var tag = allTags.Find(x => x.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase));
        if (tag != null)
        {
            if (tags.Any(x => x.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase)))
            {
                NewTagTextBox.Text = string.Empty;
                return;
            }
            else
            {
                tags.Add(tag);
            }
        }
        else
        {
            if (tags.Any(x => x.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase)))
            {
                NewTagTextBox.Text = string.Empty;
                return;
            }
            else
            {
                tags.Add(new Tag() { Title = title });
            }
        }
        SortTags();
        NewTagTextBox.Text = string.Empty;
    }

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
        if (!SaveNewAuthors())
        {
            MessageBox.Show("Не удалось сохранить новых авторов.", Title);
            return;
        }
        if (!SaveNewCycle())
        {
            MessageBox.Show("Не удалось сохранить новую серию.", Title);
            return;
        }
        if (!SaveNewTags())
        {
            MessageBox.Show("Не удалось сохранить новые теги.", Title);
            return;
        }
        if (!SaveBook())
        {
            DialogResult = false;
            return;
        }
        if (book.BookId > 0)
        {
            if (!Db.UpdateBook(book))
            {
                MessageBox.Show("Не удалось сохранить книгу.", Title);
                return;
            }
        }
        else
        {
            book.BookId = Db.InsertBook(book);
            if (book.BookId < 1)
            {
                MessageBox.Show("Не удалось добавить книгу.", Title);
                return;
            }
        }
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    #endregion
}
