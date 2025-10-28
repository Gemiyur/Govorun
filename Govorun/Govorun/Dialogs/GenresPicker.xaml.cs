using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        GenresListBox.ItemsSource = Db.GetGenres();
    }

    private void GenresListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is TextBlock && GenresListBox.SelectedItem != null)
        {
            PickedGenres.Add((Genre)GenresListBox.SelectedItem);
            DialogResult = true;
        }
    }

    private void GenresListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        PickButton.IsEnabled = GenresListBox.SelectedIndex > -1;
    }

    private void PickButton_Click(object sender, RoutedEventArgs e)
    {
        PickedGenres.AddRange(GenresListBox.SelectedItems.Cast<Genre>());
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
