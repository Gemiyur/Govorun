using System.Windows;
using System.Windows.Controls;
using Gemiyur.Collections;
using Govorun.Media;
using Govorun.Models;

namespace Govorun.Dialogs;

/// <summary>
/// Класс окна добавления книг из папки.
/// </summary>
public partial class AddBooksDialog : Window
{
    /// <summary>
    /// Список добавленных книг.
    /// </summary>
    public readonly List<Book> AddedBooks = [];

    /// <summary>
    /// Были ли добавлены новые авторы книг в библиотеку.
    /// </summary>
    public bool HasNewAuthors;

    /// <summary>
    /// Была ли добавлена новая серия книг.
    /// </summary>
    public bool HasNewCycle;

    /// <summary>
    /// Были ли изменения в тегах книг.
    /// </summary>
    public bool TagsChanged;

    /// <summary>
    /// Коллекция имён файлов книг с полным путём.
    /// </summary>
    private readonly ObservableCollectionEx<string> files = [];

    /// <summary>
    /// Инициализирует новый экземпляр класса.
    /// </summary>
    /// <param name="addedFiles">Коллекция имён файлов книг с полным путём.</param>
    public AddBooksDialog(IEnumerable<string> addedFiles)
    {
        InitializeComponent();
        files.AddRange(addedFiles);
        FilesListBox.ItemsSource = files;
    }

    private void FilesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        AddButton.IsEnabled = FilesListBox.SelectedItems.Count > 0;
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var addedFiles = FilesListBox.SelectedItems.Cast<string>().ToList();
        var addedFilesCount = addedFiles.Count;
        foreach (var file in addedFiles)
        {
            var book = App.GetBookFromFile(file, out TrackData trackData);
            var editor = new BookEditor(book, trackData) { Owner = this };
            if (editor.ShowDialog() != true)
            {
                if (addedFilesCount == 1)
                    return;
                if (MessageBox.Show("Продолжить добавление книг?", Title, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    return;
                addedFilesCount--;
                continue;
            }
            if (!HasNewAuthors && editor.HasNewAuthors)
                HasNewAuthors = true;
            if (!HasNewCycle && editor.HasNewCycle)
                HasNewCycle = true;
            if (!TagsChanged && editor.GenresChanged)
                TagsChanged = true;
            AddedBooks.Add(book);
            addedFilesCount--;
            files.Remove(file);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
