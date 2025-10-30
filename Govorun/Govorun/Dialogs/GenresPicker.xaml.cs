using System.Windows;
using System.Windows.Controls;
using Govorun.Models;
using Govorun.Tools;

namespace Govorun.Dialogs;

/// <summary>
/// Класс окна выбора жанров.
/// </summary>
public partial class GenresPicker : Window
{
    /// <summary>
    /// Список выбранных жанров.
    /// </summary>
    public List<Genre> PickedGenres = [];

    public GenresPicker()
    {
        InitializeComponent();
        GenresListBox.ItemsSource = Library.Genres;
    }

    private void GenresListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        PickButton.IsEnabled = GenresListBox.SelectedItems.Count > 0;
    }

    private void PickButton_Click(object sender, RoutedEventArgs e)
    {
        PickedGenres.AddRange(GenresListBox.SelectedItems.Cast<Genre>());
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
