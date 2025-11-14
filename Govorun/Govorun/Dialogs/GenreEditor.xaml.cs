using System.Windows;
using System.Windows.Controls;
using Govorun.Models;
using Govorun.Tools;

namespace Govorun.Dialogs;

/// <summary>
/// Класс редактора жанра.
/// </summary>
public partial class GenreEditor : Window
{
    /// <summary>
    /// Редактируемый жанр.
    /// </summary>
    private readonly Genre genre;

    /// <summary>
    /// Инициализирует новый экземпляр класса.
    /// </summary>
    /// <param name="genre">Редактируемый жанр.</param>
    public GenreEditor(Genre genre)
    {
        InitializeComponent();
        this.genre = genre;
        TitleTextBox.Text = genre.Title;
    }

    private void TitleTextBox_TextChanged(object sender, TextChangedEventArgs e) =>
        SaveButton.IsEnabled = !string.IsNullOrWhiteSpace(TitleTextBox.Text) && TitleTextBox.Text.Trim() != genre.Title;

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var title = TitleTextBox.Text.Trim();

        var foundGenre = Library.Genres.Find(x => x.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase));
        if (foundGenre != null && foundGenre.GenreId != genre.GenreId)
        {
            MessageBox.Show("Жанр с таким названием уже существует.", Title);
            return;
        }

        var origTitle = genre.Title;
        genre.Title = title;
        var saved = genre.GenreId > 0 ? Library.UpdateGenre(genre) : Library.AddGenre(genre);
        if (!saved)
        {
            MessageBox.Show("Не удалось сохранить жанр.", Title);
            genre.Title = origTitle;
            DialogResult = false;
            return;
        }

        var bookInfoWindow = App.FindBookInfoWindow();
        if (bookInfoWindow != null && Library.BookHasGenre(bookInfoWindow.Book, genre.GenreId))
            bookInfoWindow.UpdateGenres();

        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();
}
