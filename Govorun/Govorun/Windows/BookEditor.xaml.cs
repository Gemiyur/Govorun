using System.Windows;
using System.Windows.Controls;
using Gemiyur.Comparers;
using Govorun.Media;
using Govorun.Models;
using Govorun.Tools;

namespace Govorun.Windows
{
    #region Задачи (TODO).

    #endregion

    /// <summary>
    /// Класс окна редактора книги.
    /// </summary>
    public partial class BookEditor : Window
    {
        /// <summary>
        /// Были ли изменения в авторах книги.
        /// </summary>
        public bool AuthorsChanged;

        /// <summary>
        /// Были ли добавлены новые авторы книг в библиотеку.
        /// </summary>
        public bool HasNewAuthors;

        /// <summary>
        /// Было ли изменено название книги.
        /// </summary>
        public bool TitleChanged;

        /// <summary>
        /// Редактируемая книга.
        /// </summary>
        private readonly Book book;

        /// <summary>
        /// Данные книги из тега файла книги.
        /// </summary>
        private TrackData? tag;

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
        /// <param name="tag">Данные книги из тега файла книги.</param>
        /// <exception cref="ArgumentException"></exception>
        public BookEditor(Book book, TrackData? tag)
        {
            InitializeComponent();
            if (book == null)
            {
                // TODOL: Выдавать сообщение всегда или только при отладке?
                MessageBox.Show("Не указана книга: book == null.", "Ошибка");
                throw new ArgumentException("Не указана книга: book == null.", nameof(book));
            }
            this.book = book;
            this.tag = tag;
            FileNotFoundTextBlock.Visibility = book.FileExists ? Visibility.Collapsed : Visibility.Visible;
            LoadTagButton.IsEnabled = book.FileExists && tag == null;
            LoadBook();
            LoadTag();
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
            if (tag == null)
                return;
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
        /// Сохраняет данные из редактора в редактируемую книгу.
        /// </summary>
        /// <returns>Были ли внесены изменения в книгу.</returns>
        private bool SaveBook()
        {
            // В книге есть изменения?
            var changed = false;
            // Новая книга.
            changed = book.BookId < 1;
            // Название.
            if (book.Title != TitleTextBox.Text)
            {
                book.Title = TitleTextBox.Text;
                changed = true;
                TitleChanged = true;
            }
            // Авторы.
            if (authors.Count != book.Authors.Count ||
                authors.Any(x => book.Authors.Exists(a => a.AuthorId != x.AuthorId)) ||
                book.Authors.Any(x => authors.Exists(a => a.AuthorId != x.AuthorId)))
            {
                book.Authors.Clear();
                book.Authors.AddRange(authors);
                changed = true;
                AuthorsChanged = true;
            }
            // Чтец.
            if (book.Lector != LectorTextBox.Text)
            {
                book.Lector = LectorTextBox.Text;
                changed = true;
            }
            // Комментарий.
            if (book.Comment != CommentTextBox.Text)
            {
                book.Comment = CommentTextBox.Text;
                changed = true;
            }
            // Возврат результата: были ли внесены изменения в книгу.
            return changed;
        }

        /// <summary>
        /// Сохраняет новых авторов и присваивает им идентификаторы.
        /// </summary>
        /// <returns>Были ли авторы сохранены успешно.</returns>
        private bool SaveNewAuthors()
        {
            var newAuthors = authors.FindAll(x => x.AuthorId == 0);
            if (!newAuthors.Any())
                return true;
            HasNewAuthors = true;
            using var db = Db.GetDatabase();
            foreach (var author in newAuthors)
            {
                author.AuthorId = Db.InsertAuthor(author, db);
                if (author.AuthorId < 1)
                    return false;
            }
            return true;
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
                authors.Add(new Author() { Name = name, Surname = surname });
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
            {
                tag = new TrackData(book.FileName);
                LoadTag();
            }
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
            if (!SaveNewAuthors())
            {
                MessageBox.Show("Не удалось сохранить новых авторов.", Title);
                return;
            }
            if (!SaveBook())
            {
                DialogResult = false;
                return;
            }
            if (book.BookId > 0)
            {
                if (!Db.UpdateBook(book))
                {
                    MessageBox.Show("Не удалось сохранить книгу.", Title);
                    return;
                }
            }
            else
            {
                book.BookId = Db.InsertBook(book);
                if (book.BookId < 1)
                {
                    MessageBox.Show("Не удалось добавить книгу.", Title);
                    return;
                }
            }
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        #endregion
    }
}
