using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Govorun.Tools;

namespace Govorun.Dialogs;

/// <summary>
/// Класс окна выбора тегов.
/// </summary>
public partial class TagsPicker : Window
{
    /// <summary>
    /// Список выбранных тегов.
    /// </summary>
    public List<string> PickedTags = [];

    public TagsPicker()
    {
        InitializeComponent();
        TagsListBox.ItemsSource = Library.Tags;
    }

    private void TagsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is TextBlock && TagsListBox.SelectedItem != null)
        {
            PickedTags.Add((string)TagsListBox.SelectedItem);
            DialogResult = true;
        }
    }

    private void TagsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        PickButton.IsEnabled = TagsListBox.SelectedIndex > -1;
    }

    private void PickButton_Click(object sender, RoutedEventArgs e)
    {
        PickedTags.AddRange(TagsListBox.SelectedItems.Cast<string>());
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
