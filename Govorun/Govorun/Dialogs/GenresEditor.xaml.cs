using System.Windows;
using System.Windows.Controls;
using Gemiyur.Collections;
using Govorun.Models;
using Govorun.Tools;

namespace Govorun.Dialogs;

/// <summary>
/// Класс редактора жанров.
/// </summary>
public partial class GenresEditor : Window
{
    /// <summary>
    /// Были ли изменения в коллекции жанров.
    /// </summary>
    public bool HasChanges;

    /// <summary>
    /// Коллекция жанров.
    /// </summary>
    private readonly ObservableCollectionEx<Genre> genres = [];

    public GenresEditor()
    {
        InitializeComponent();
        genres.AddRange(Db.GetGenres());
        GenresListBox.ItemsSource = genres;
    }

    private void SortGenres() => genres.Sort(x => x.Title, StringComparer.CurrentCultureIgnoreCase);

    private void GenresListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        EditButton.IsEnabled = GenresListBox.SelectedIndex >= 0;
        DeleteButton.IsEnabled = GenresListBox.SelectedIndex >= 0 &&
                                 !Library.TagHasBook(((Genre)GenresListBox.SelectedItem).GenreId);
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var editor = new GenreEditor(null, genres) { Owner = this };
        if (editor.ShowDialog() != true)
            return;
        var genre = editor.EditedGenre;
        genre.GenreId = Db.InsertGenre(genre);
        if (genre.GenreId < 1)
        {
            MessageBox.Show("Не удалось добавить жанр.", Title);
            return;
        }
        genres.Add(genre);
        SortGenres();
        HasChanges = true;
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        var genre = (Genre)GenresListBox.SelectedItem;
        var editor = new GenreEditor(genre, genres) { Owner = this };
        if (editor.ShowDialog() != true)
            return;
        if (!Db.UpdateGenre(genre))
        {
            MessageBox.Show("Не удалось сохранить жанр.", Title);
            return;
        }
        SortGenres();
        HasChanges = true;
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        var genre = (Genre)GenresListBox.SelectedItem;
        if (!Db.DeleteGenre(genre.GenreId))
        {
            MessageBox.Show("Не удалось удалить жанр.", Title);
            return;
        }
        genres.Remove(genre);
        HasChanges = true;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
