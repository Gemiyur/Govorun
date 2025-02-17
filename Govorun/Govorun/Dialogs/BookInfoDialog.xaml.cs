using System.Windows;
using Govorun.Models;

namespace Govorun.Dialogs
{
    /// <summary>
    /// Класс окна информации о книге.
    /// </summary>
    public partial class BookInfoDialog : Window
    {
        public BookInfoDialog(Book book)
        {
            InitializeComponent();
            TitleTextBlock.FontSize = FontSize + 2;
            AuthorsTextBlock.Text = book.AuthorsNameSurnameText;
            TitleTextBlock.Text = book.Title;
            LectorTextBlock.Text = book.Lector;
            DurationTextBlock.Text = book.DurationText;
            CommentTextBox.Text = book.Comment;
            FileTextBox.Text = book.FileName;
            ChaptersButton.IsEnabled = book.Chapters.Any();
        }

        private void ChaptersButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
