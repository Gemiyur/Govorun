using Gemiyur.Collections;
using Govorun.Models;
using Govorun.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Govorun.Windows
{
    /// <summary>
    /// Тип делегата для обработчика воспроизведения главы книги.
    /// </summary>
    /// <param name="book">Книга.</param>
    /// <param name="chapter">Глава книги.</param>
    public delegate void PlayChapterHandlerDelegate(Book book, Chapter chapter);

    /// <summary>
    /// Класс окна содержания книги.
    /// </summary>
    public partial class ChaptersWindow : Window
    {
        /// <summary>
        /// Обработчик воспроизведения главы книги.
        /// </summary>
        public PlayChapterHandlerDelegate? PlayChapterHandler;

        /// <summary>
        /// Книга.
        /// </summary>
        private Book? book;

        /// <summary>
        /// Возвращает или задаёт книгу.
        /// </summary>
        public Book? Book
        {
            get => book;
            set
            {
                if (book == value)
                    return;
                SaveChanges();
                hasChanges = false;
                book = value;
                if (book != null)
                {
                    AuthorsTextBlock.Text = book.AuthorsNameSurnameText;
                    TitleTextBlock.Text = book.Title;
                    chapters.ReplaceRange(book.Chapters);
                }
                else
                {
                    AuthorsTextBlock.Text = string.Empty;
                    TitleTextBlock.Text = string.Empty;
                    chapters.Clear();
                }
            }
        }

        /// <summary>
        /// Возвращает выбранную главу книги.
        /// </summary>
        public Chapter? Chapter => (Chapter)ChaptersListView.SelectedItem;

        /// <summary>
        /// Коллекция глав книги.
        /// </summary>
        private readonly ObservableCollectionEx<Chapter> chapters = [];

        /// <summary>
        /// Были ли изменения в названиях глав книги.
        /// </summary>
        private bool hasChanges = false;

        public ChaptersWindow(Book book)
        {
            InitializeComponent();
            TitleTextBlock.FontSize = FontSize + 2;
            ChaptersListView.ItemsSource = chapters;
            Book = book;
        }

        private void SaveChanges()
        {
            //if (hasChanges)
            //    Db.UpdateBook(book);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            SaveChanges();
            //((MainWindow)Owner).Chapter
            MainWindow.ChaptersWindow = null;
        }

        private void ChaptersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PlayButton.IsEnabled = Chapter != null;
            EditButton.IsEnabled = Chapter != null;
        }

        private void ChaptersListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var listView = (ListView)sender;
            var gridView = (GridView)listView.View;
            var totalWidth = listView.ActualWidth - (SystemParameters.VerticalScrollBarWidth + 10);
            var usedWidth = 0.0;
            for (var i = 1; i < gridView.Columns.Count; i++)
            {
                usedWidth += gridView.Columns[i].Width;
            }
            gridView.Columns[0].Width = totalWidth - usedWidth;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlayChapterHandler != null && Book != null && Chapter != null)
                PlayChapterHandler(Book, Chapter);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var chapter = (Chapter)ChaptersListView.SelectedItem;
            var title = chapter.Title;
            var editor = new ChapterEditor(title) { Owner = this };
            if (!App.SimpleBool(editor.ShowDialog()))
                return;
            chapter.Title = editor.ChapterTitle;
            hasChanges = true;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
