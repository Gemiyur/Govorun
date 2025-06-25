using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Govorun.Models;
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
    public List<Tag> PickedTags = [];

    public TagsPicker()
    {
        InitializeComponent();
        TagsListBox.ItemsSource = Db.GetTags();
    }

    private void TagsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is TextBlock && TagsListBox.SelectedItem != null)
        {
            PickedTags.Add((Tag)TagsListBox.SelectedItem);
            DialogResult = true;
        }
    }

    private void TagsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        PickButton.IsEnabled = TagsListBox.SelectedIndex > -1;
    }

    private void PickButton_Click(object sender, RoutedEventArgs e)
    {
        PickedTags.AddRange(TagsListBox.SelectedItems.Cast<Tag>());
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
