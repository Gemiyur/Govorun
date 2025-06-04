using System.Windows;
using System.Windows.Controls;
using Govorun.Models;
using Govorun.Tools;

namespace Govorun.Dialogs;

/// <summary>
/// Класс редактора серии книг.
/// </summary>
public partial class CycleEditor : Window
{
    /// <summary>
    /// Название серии книг.
    /// </summary>
    public string CycleTitle;

    private List<Cycle> cycles = Db.GetCycles();

    /// <summary>
    /// Инициализирует новый экземпляр класса.
    /// </summary>
    /// <param name="title">Название серии книг.</param>
    public CycleEditor(string title)
    {
        InitializeComponent();
        CycleTitle = title;
        TitleTextBox.Text = title;
    }

    private void TitleTextBox_TextChanged(object sender, TextChangedEventArgs e) =>
        SaveButton.IsEnabled = string.IsNullOrWhiteSpace(TitleTextBox.Text) && TitleTextBox.Text.Trim() != CycleTitle;

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var title = TitleTextBox.Text.Trim();
        if (cycles.Exists(x => x.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase)))
        {
            MessageBox.Show("Серия с таким названием уже существует.", Title);
            return;
        }
        CycleTitle = title;
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();
}
