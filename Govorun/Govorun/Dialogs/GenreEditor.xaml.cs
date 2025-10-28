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
    public Genre EditedGenre;

    /// <summary>
    /// Список существующих жанров.
    /// </summary>
    private readonly List<Genre> genres = [];

    /// <summary>
    /// Инициализирует новый экземпляр класса.
    /// </summary>
    /// <param name="genre">Редактируемый жанр.</param>
    /// <param name="genres">Список существующих жанров.</param>
    public GenreEditor(Genre? genre, IEnumerable<Genre>? genres)
    {
        InitializeComponent();
        EditedGenre = genre ?? new Genre();
        TitleTextBox.Text = EditedGenre.Title;
        this.genres.AddRange(genres ?? Db.GetGenres());
    }

    private void TitleTextBox_TextChanged(object sender, TextChangedEventArgs e) =>
        SaveButton.IsEnabled = !string.IsNullOrWhiteSpace(TitleTextBox.Text) && TitleTextBox.Text.Trim() != EditedGenre.Title;

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var title = TitleTextBox.Text.Trim();
        if (genres.Exists(x => x.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase)))
        {
            MessageBox.Show("Жанр с таким названием уже существует.", Title);
            return;
        }
        EditedGenre.Title = title;
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();
}
