using System.Windows;
using System.Windows.Controls;
using Govorun.Models;
using Govorun.Tools;

namespace Govorun.Dialogs;

/// <summary>
/// Класс редактора тега.
/// </summary>
public partial class TagEditor : Window
{
    /// <summary>
    /// Редактируемый тег.
    /// </summary>
    public Genre EditedTag;

    /// <summary>
    /// Список существующих тегов.
    /// </summary>
    private readonly List<Genre> tags = [];

    /// <summary>
    /// Инициализирует новый экземпляр класса.
    /// </summary>
    /// <param name="tag">Редактируемый тег.</param>
    /// <param name="tags">Список существующих тегов.</param>
    public TagEditor(Genre? tag, IEnumerable<Genre>? tags)
    {
        InitializeComponent();
        EditedTag = tag ?? new Genre();
        TitleTextBox.Text = EditedTag.Title;
        this.tags.AddRange(tags ?? Db.GetTags());
    }

    private void TitleTextBox_TextChanged(object sender, TextChangedEventArgs e) =>
        SaveButton.IsEnabled = !string.IsNullOrWhiteSpace(TitleTextBox.Text) && TitleTextBox.Text.Trim() != EditedTag.Title;

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var title = TitleTextBox.Text.Trim();
        if (tags.Exists(x => x.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase)))
        {
            MessageBox.Show("Тег с таким названием уже существует.", Title);
            return;
        }
        EditedTag.Title = title;
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();
}
