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
using Govorun.Tools;

namespace Govorun.Dialogs
{
    #region Задачи (TODO).

    // TODO: Подобрать цвет фона для TextBox только для чтения.

    #endregion

    /// <summary>
    /// Класс окна редактора книги.
    /// </summary>
    public partial class BookEditor : Window
    {
        /// <summary>
        /// Редактируемая книга.
        /// </summary>
        private readonly Book book;

        /// <summary>
        /// Список авторов книги.
        /// </summary>
        private readonly List<Author> authors = [];

        /// <summary>
        /// Список всех авторов в библиотеке.
        /// </summary>
        private readonly List<Author> allAuthors = Db.GetAuthors();

        /// <summary>
        /// Инициализирует новый экземпляр класса.
        /// </summary>
        /// <param name="book">Книга.</param>
        /// <param name="loadTag">Загружать ли данные из тега файла книги.</param>
        /// <exception cref="ArgumentException"></exception>
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

        /// <summary>
        /// Устанавливает доступность кнопки добавления нового автора.
        /// </summary>
        private void CheckAddNewAuthorButton()
        {
            AddNewAuthorButton.IsEnabled =
                !string.IsNullOrWhiteSpace(NewAuthorNameTextBox.Text) ||
                !string.IsNullOrWhiteSpace(NewAuthorSurnameTextBox.Text);
        }

        /// <summary>
        /// Очищает поля ввода имени и фамилии нового автора.
        /// </summary>
        private void ClearNewAuthor()
        {
            NewAuthorNameTextBox.Text = string.Empty;
            NewAuthorSurnameTextBox.Text = string.Empty;
        }

        /// <summary>
        /// Загружает книгу в редактор.
        /// </summary>
        private void LoadBook()
        {
            TitleTextBox.Text = book.Title;
            authors.Clear();
            authors.AddRange(book.Authors);
            SortAuthors();
            UpdateAuthorsSource();
            LectorTextBox.Text = book.Lector;
            CommentTextBox.Text = book.Comment;
        }

        /// <summary>
        /// Загружает данные из тега файла книги в редактор.
        /// </summary>
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

        /// <summary>
        /// Сортирует список авторов книги по фамилии и имени.
        /// </summary>
        private void SortAuthors() => authors.Sort(new StringKeyComparer(x => ((Author)x).SurnameName));

        /// <summary>
        /// Обновляет источник элементов списка авторов книги.
        /// </summary>
        private void UpdateAuthorsSource()
        {
            AuthorsListBox.ItemsSource = null;
            AuthorsListBox.ItemsSource = authors;
        }

        #region Обработчики событий элементов управления.

        private void TitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveButton.IsEnabled = !string.IsNullOrWhiteSpace(TitleTextBox.Text);
        }

        private void AuthorsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RemoveAuthorsButton.IsEnabled = AuthorsListBox.SelectedItems.Count > 0;
        }

        private void PickAuthorsButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new AuthorsPicker() { Owner = this };
            if (!App.SimpleBool(picker.ShowDialog()))
                return;
            authors.AddRange(picker.PickedAuthors.Where(x => !authors.Exists(a => a.AuthorId == x.AuthorId)));
            SortAuthors();
            UpdateAuthorsSource();
        }

        private void RemoveAuthorsButton_Click(object sender, RoutedEventArgs e)
        {
            authors.RemoveAll(x => AuthorsListBox.SelectedItems.Cast<Author>().Contains(x));
            UpdateAuthorsSource();
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
            var name = NewAuthorNameTextBox.Text.Trim();
            var surname = NewAuthorSurnameTextBox.Text.Trim();
            var author = allAuthors.Find(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase) &&
                                              x.Surname.Equals(surname, StringComparison.CurrentCultureIgnoreCase));
            if (author != null)
            {
                if (authors.Exists(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase) &&
                                        x.Surname.Equals(surname, StringComparison.CurrentCultureIgnoreCase)))
                {
                    ClearNewAuthor();
                    return;
                }
                else
                {
                    authors.Add(author);
                }
            }
            else
            {
                author = new Author() { Name = name, Surname = surname };
                allAuthors.Add(author);
                authors.Add(author);
            }
            SortAuthors();
            UpdateAuthorsSource();
            ClearNewAuthor();
        }

        private void PickLectorButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new LectorPicker() { Owner = this };
            if (!App.SimpleBool(picker.ShowDialog()))
                return;
            LectorTextBox.Text = picker.PickedLector;
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
