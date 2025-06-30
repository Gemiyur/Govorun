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
        AuthorsTextBlock.Text = book.AuthorNamesFirstLast;
        TitleTextBlock.FontSize = FontSize + 2;
        TitleTextBlock.Text = book.Title;
        if (book.Cycle != null)
        {
            CycleTitleTextBlock.Text = book.Cycle.Title;
            if (book.CyclePart.Length > 0)
                CycleNumberTextBlock.Text = book.CyclePart;
            else
                CycleNumberStackPanel.Visibility = Visibility.Collapsed;
        }
        else
        {
            CycleGrid.Visibility = Visibility.Collapsed;
        }
        LectorTextBlock.Text = book.Lector.Length > 0 ? book.Lector : "(не указано)";
        DurationTextBlock.Text = book.DurationText;
        if (book.Translator.Length > 0)
        {
            TranslatorTextBlock.Text = book.Translator;
        }
        else
        {
            TranslatorStackPanel.Visibility = Visibility.Collapsed;
        }
        if (book.Tags.Count > 0)
        {
            TagsTextBlock.Text =
                App.ListToString(book.Tags, "; ", x => ((Tag)x).Title, StringComparer.CurrentCultureIgnoreCase);
        }
        else
        {
            TagsGrid.Visibility = Visibility.Collapsed;
        }
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
