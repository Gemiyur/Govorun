using System.Windows;
using System.Windows.Controls;

namespace Govorun.Dialogs;

/// <summary>
/// Класс редактора главы книги.
/// </summary>
public partial class ChapterEditor : Window
{
    /// <summary>
    /// Название главы книги.
    /// </summary>
    public string ChapterTitle;

    /// <summary>
    /// Инициализирует новый экземпляр класса.
    /// </summary>
    /// <param name="title">Название главы книги.</param>
    public ChapterEditor(string title)
    {
        InitializeComponent();
        ChapterTitle = title;
        TitleTextBox.Text = title;
    }

    private void TitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        SaveButton.IsEnabled = TitleTextBox.Text.Any() && TitleTextBox.Text != ChapterTitle;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        ChapterTitle = TitleTextBox.Text;
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
