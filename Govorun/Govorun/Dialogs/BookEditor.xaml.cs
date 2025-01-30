using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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
using Gemiyur.Comparers;
using Govorun.Media;
using Govorun.Models;

namespace Govorun.Dialogs
{
    #region Задачи (TODO).

    // TODO: Подумать над сортировкой авторов - где и когда сортировать.
    // TODO: Подобрать цвет фона для TextBox только для чтения.

    #endregion

    /// <summary>
    /// Класс окна редактора книги.
    /// </summary>
    public partial class BookEditor : Window
    {
        private readonly Book book;

        private readonly List<Author> authors = [];

        public BookEditor(Book book, bool loadTag)
        {
            InitializeComponent();
            if (book == null)
            {
                // TODO: Выдавать сообщение всегда или только при отладке?
                MessageBox.Show("Не указана книга: book == null.", "Ошибка");
                throw new ArgumentException("Не указана книга: book == null.", nameof(book));
            }
            this.book = book;
            FileNotFoundTextBlock.Visibility = book.FileExists ? Visibility.Collapsed : Visibility.Visible;
            LoadBook();
            if (!book.FileExists)
                LoadTagButton.IsEnabled = false;
            else if (loadTag)
            {
                LoadTag();
                LoadTagButton.IsEnabled = false;
            }
        }

        private void LoadBook()
        {
            TitleTextBox.Text = book.Title;
            authors.Clear();
            authors.AddRange(book.Authors);
            // TODO: Нужно ли сортировать авторов при загрузке книги? Надо подумать.
            SortAuthors();
            UpdateAuthorsSource();
            LectorTextBox.Text = book.Lector;
            CommentTextBox.Text = book.Comment;
        }

        private void LoadTag()
        {
            var tag = new TrackData(book.FileName);
            TagTitleTextBox.Text = tag.Title;
            TagAuthorTextBox.Text = tag.Artist;
            var comments = tag.Comment;
            if (!comments.Any())
                comments = tag.Description;
            else if (tag.Description.Any())
                comments = comments + "\r\n" + tag.Description;
            if (!comments.Any())
                comments = tag.Lyrics;
            else if (tag.Lyrics.Any())
                comments = comments + "\r\n" + tag.Lyrics;
            TagCommentsTextBox.Text = comments;
        }

        private void SortAuthors() => authors.Sort(new StringKeyComparer(x => ((Author)x).SurnameName));

        private void UpdateAuthorsSource()
        {
            // TODO: Возможно, тут следует сортировать авторов. Надо подумать.
            AuthorsListBox.ItemsSource = null;
            AuthorsListBox.ItemsSource = authors;
        }

        private void CheckAddNewAuthorButton()
        {
            //if (NewAuthorSurnameTextBox.Text.Any())
            //AddNewAuthorButton.IsEnabled = NewAuthorSurnameTextBox.Text.Any() || NewAuthorNameTextBox.Text.Any();

            AddNewAuthorButton.IsEnabled =
                !string.IsNullOrWhiteSpace(NewAuthorSurnameTextBox.Text) ||
                !string.IsNullOrWhiteSpace(NewAuthorNameTextBox.Text);

        }

        #region Обработчики событий элементов управления.

        private void TitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveButton.IsEnabled = !string.IsNullOrWhiteSpace(TitleTextBox.Text);
        }

        private void AuthorsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RemoveAuthorButton.IsEnabled = AuthorsListBox.SelectedItems.Count > 0;
        }

        private void PickAuthorButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RemoveAuthorButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NewAuthorSurnameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckAddNewAuthorButton();
        }

        private void NewAuthorNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckAddNewAuthorButton();
        }

        private void AddNewAuthorButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PickLectorButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new LectorPicker() { Owner = this };
            if (!App.SimpleBool(picker.ShowDialog()))
                return;
            LectorTextBox.Text = picker.Lector;
        }

        private void LoadTagButton_Click(object sender, RoutedEventArgs e)
        {
            // Так сделано на случай если после загрузки книги в редактор файл книги был удалён или переименован.
            if (book.FileExists)
                LoadTag();
            else
                FileNotFoundTextBlock.Visibility = Visibility.Visible;
            LoadTagButton.IsEnabled = false;
        }

        private void ChaptersButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BookmarksButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        #endregion
    }
}
