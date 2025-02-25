using System.IO;
using System.Windows;
using System.Windows.Media;
using Govorun.Media;
using Govorun.Models;

namespace Govorun.Dialogs
{
    /// <summary>
    /// Класс окна файла книги.
    /// </summary>
    public partial class BookFileDialog : Window
    {
        /// <summary>
        /// Возвращает выбранное имя файла книги с полным путём.
        /// </summary>
        public string FileName => FileTextBox.Text;

        private readonly Book book;

        public BookFileDialog(Book book)
        {
            InitializeComponent();
            this.book = book;
            AuthorsTextBlock.Text = book.AuthorsNameSurnameText;
            TitleTextBlock.Text = book.Title;
            FileTextBox.Text = book.FileName;
        }

        private void PickButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = App.PickBookFileDialog;
            if (dialog.ShowDialog() != true)
                return;
            var tag = new TrackData(dialog.FileName);
            var size = new FileInfo(dialog.FileName).Length;
            if (tag.Duration != book.Duration || size != book.FileSize)
            {
                MessageBox.Show("Файл не подходит по размеру и продолжительности.", Title);
                return;
            }
            FileTextBox.Text = dialog.FileName;
            StatusTextBlock.Text = "Файл выбран";
            StatusTextBlock.Foreground = Brushes.Green;
            SaveButton.IsEnabled = true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
