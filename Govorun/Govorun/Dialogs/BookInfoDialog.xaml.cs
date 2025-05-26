using System.Windows;
using Govorun.Models;

namespace Govorun.Dialogs;

/// <summary>
/// Класс окна информации о книге.
/// </summary>
public partial class BookInfoDialog : Window
{
    public BookInfoDialog(Book book)
    {
        InitializeComponent();
        TitleTextBlock.FontSize = FontSize + 2;
        AuthorsTextBlock.Text = book.AuthorNamesFirstLast;
        TitleTextBlock.Text = book.Title;
        LectorTextBlock.Text = book.Lector;
        DurationTextBlock.Text = book.DurationText;
        AnnotationTextBox.Text = book.Annotation;
        FileTextBox.Text = book.FileName;
    }

    private void ListenButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
