using System.Windows;
using System.Windows.Controls;

namespace Govorun.Dialogs;

/// <summary>
/// Класс редактора закладки книги.
/// </summary>
public partial class BookmarkEditor : Window
{
    /// <summary>
    /// Название закладки книги.
    /// </summary>
    public string BookmarkTitle;

    /// <summary>
    /// Инициализирует новый экземпляр класса.
    /// </summary>
    /// <param name="title">Название закладки книги.</param>
    public BookmarkEditor(string title)
    {
        InitializeComponent();
        BookmarkTitle = title;
        TitleTextBox.Text = title;
    }

    private void TitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        SaveButton.IsEnabled = TitleTextBox.Text.Any() && TitleTextBox.Text != BookmarkTitle;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        BookmarkTitle = TitleTextBox.Text;
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
