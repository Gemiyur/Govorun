using System.Windows;
using System.Windows.Controls;

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

    private void TitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        SaveButton.IsEnabled = TitleTextBox.Text.Any() && TitleTextBox.Text != CycleTitle;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        CycleTitle = TitleTextBox.Text;
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
