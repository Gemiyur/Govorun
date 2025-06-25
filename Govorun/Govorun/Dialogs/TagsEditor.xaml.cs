using System.Windows;
using System.Windows.Controls;
using Gemiyur.Collections;
using Govorun.Models;
using Govorun.Tools;

namespace Govorun.Dialogs;

/// <summary>
/// Класс редактора тегов.
/// </summary>
public partial class TagsEditor : Window
{
    /// <summary>
    /// Были ли изменения в коллекции тегов.
    /// </summary>
    public bool HasChanges;

    /// <summary>
    /// Коллекция тегов.
    /// </summary>
    private readonly ObservableCollectionEx<Tag> tags = [];

    public TagsEditor()
    {
        InitializeComponent();
        tags.AddRange(Db.GetTags());
        TagsListBox.ItemsSource = tags;
    }

    private void SortTags() => tags.Sort(x => x.Title, StringComparer.CurrentCultureIgnoreCase);

    private void TagsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        EditButton.IsEnabled = TagsListBox.SelectedIndex >= 0;
        DeleteButton.IsEnabled = TagsListBox.SelectedIndex >= 0 &&
                                 !Library.TagHasBook(((Tag)TagsListBox.SelectedItem).TagId);
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var editor = new TagEditor(null, tags) { Owner = this };
        if (editor.ShowDialog() != true)
            return;
        var tag = editor.EditedTag;
        tag.TagId = Db.InsertTag(tag);
        if (tag.TagId < 1)
        {
            MessageBox.Show("Не удалось добавить тег.", Title);
            return;
        }
        tags.Add(tag);
        SortTags();
        HasChanges = true;
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        var tag = (Tag)TagsListBox.SelectedItem;
        var editor = new TagEditor(tag, tags) { Owner = this };
        if (editor.ShowDialog() != true)
            return;
        if (!Db.UpdateTag(tag))
        {
            MessageBox.Show("Не удалось сохранить тег.", Title);
            return;
        }
        SortTags();
        HasChanges = true;
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        var tag = (Tag)TagsListBox.SelectedItem;
        if (!Db.DeleteTag(tag.TagId))
        {
            MessageBox.Show("Не удалось удалить тег.", Title);
            return;
        }
        tags.Remove(tag);
        HasChanges = true;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
