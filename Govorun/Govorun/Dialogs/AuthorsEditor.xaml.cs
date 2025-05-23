using System.Windows;
using System.Windows.Controls;
using Gemiyur.Collections;
using Govorun.Models;
using Govorun.Tools;

namespace Govorun.Dialogs;

/// <summary>
/// Класс окна редактора авторов.
/// </summary>
public partial class AuthorsEditor : Window
{
    /// <summary>
    /// Были ли изменения в коллекции авторов.
    /// </summary>
    public bool HasChanges;

    /// <summary>
    /// Коллекция авторов.
    /// </summary>
    private readonly ObservableCollectionEx<Author> Authors = [];

    /// <summary>
    /// Автор, загруженный в редактор автора.
    /// </summary>
    private Author? EditedAuthor;

    /// <summary>
    /// Инициализирует новый экземпляр класса.
    /// </summary>
    public AuthorsEditor()
    {
        InitializeComponent();
        Authors.AddRange(Db.GetAuthors());
        AuthorsListBox.ItemsSource = Authors;
    }

    /// <summary>
    /// Устанавливает доступность кнопок редактора автора.
    /// </summary>
    private void CheckEditorButtons()
    {
        var authorNameEmpty =
            string.IsNullOrWhiteSpace(LastNameTextBox.Text) &&
            string.IsNullOrWhiteSpace(FirstNameTextBox.Text) &&
            string.IsNullOrWhiteSpace(MiddleNameTextBox.Text);
        if (EditedAuthor != null)
        {
            ClearButton.IsEnabled = true;
            if (authorNameEmpty)
            {
                SaveButton.IsEnabled = false;
                return;
            }
            SaveButton.IsEnabled =
                LastNameTextBox.Text.Trim() != EditedAuthor.LastName || 
                FirstNameTextBox.Text.Trim() != EditedAuthor.FirstName ||
                MiddleNameTextBox.Text.Trim() != EditedAuthor.MiddleName ||
                AboutTextBox.Text.Trim() != EditedAuthor.About;
        }
        else
        {
            SaveButton.IsEnabled = !authorNameEmpty;
            ClearButton.IsEnabled = !authorNameEmpty || !string.IsNullOrWhiteSpace(AboutTextBox.Text);
        }
    }

    /// <summary>
    /// Очищает данные редактора автора.
    /// </summary>
    private void ClearEditor()
    {
        EditedAuthor = null;
        LastNameTextBlock.Text = string.Empty;
        FirstNameTextBlock.Text = string.Empty;
        MiddleNameTextBlock.Text = string.Empty;
        LastNameTextBox.Text = string.Empty;
        FirstNameTextBox.Text = string.Empty;
        MiddleNameTextBox.Text = string.Empty;
        AboutTextBox.Text = string.Empty;
        ClearButton.Content = "Очистить";
    }

    /// <summary>
    /// Загружает данные текущего автора в редактор автора.
    /// </summary>
    private void EditAuthor()
    {
        EditedAuthor = (Author)AuthorsListBox.SelectedItem;
        LastNameTextBlock.Text = EditedAuthor.LastName;
        FirstNameTextBlock.Text = EditedAuthor.FirstName;
        MiddleNameTextBlock.Text = EditedAuthor.MiddleName;
        LastNameTextBox.Text = EditedAuthor.LastName;
        FirstNameTextBox.Text = EditedAuthor.FirstName;
        MiddleNameTextBox.Text = EditedAuthor.MiddleName;
        AboutTextBox.Text = EditedAuthor.About;
        ClearButton.Content = "Отмена";
    }

    private void AuthorsListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.OriginalSource is TextBlock && AuthorsListBox.SelectedItem != null)
            EditAuthor();
    }

    private void AuthorsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        EditButton.IsEnabled = AuthorsListBox.SelectedIndex >= 0;
        DeleteButton.IsEnabled = AuthorsListBox.SelectedIndex >= 0;
    }

    private void EditButton_Click(object sender, RoutedEventArgs e) => EditAuthor();

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        const string message = "Автор будет так же удалён из всех книг.\nУдалить автора?";
        var author = (Author)AuthorsListBox.SelectedItem;
        var books = Books.GetAuthorBooks(author.AuthorId);
        if (books.Any() && MessageBox.Show(message, Title, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
        {
            return;
        }
        if (!Db.DeleteAuthor(author.AuthorId))
        {
            MessageBox.Show("Не удалось удалить автора.", Title);
            return;
        }
        if (EditedAuthor == author)
            ClearEditor();
        foreach (var book in books)
        {
            book.Authors.RemoveAll(x => x.AuthorId == author.AuthorId);
        }
        Authors.Remove(author);
        HasChanges = true;
    }

    private void LastNameTextBox_TextChanged(object sender, TextChangedEventArgs e) => CheckEditorButtons();

    private void FirstNameTextBox_TextChanged(object sender, TextChangedEventArgs e) => CheckEditorButtons();

    private void MiddleNameTextBox_TextChanged(object sender, TextChangedEventArgs e) => CheckEditorButtons();

    private void AboutTextBox_TextChanged(object sender, TextChangedEventArgs e) => CheckEditorButtons();

    private void ClearButton_Click(object sender, RoutedEventArgs e) => ClearEditor();

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (EditedAuthor == null)
        {
            var author = new Author()
            {
                FirstName = FirstNameTextBox.Text.Trim(),
                LastName = LastNameTextBox.Text.Trim(),
                MiddleName = MiddleNameTextBox.Text.Trim(),
                About = AboutTextBox.Text.Trim()
            };
            author.AuthorId = Db.InsertAuthor(author);
            if (author.AuthorId < 1)
            {
                MessageBox.Show("Не удалось добавить автора.", Title);
                return;
            }
            Authors.Add(author);
        }
        else
        {
            EditedAuthor.FirstName = FirstNameTextBox.Text.Trim();
            EditedAuthor.LastName = LastNameTextBox.Text.Trim();
            EditedAuthor.MiddleName = MiddleNameTextBox.Text.Trim();
            EditedAuthor.About = AboutTextBox.Text.Trim();
            if (!Db.UpdateAuthor(EditedAuthor))
            {
                MessageBox.Show("Не удалось изменить данные автора.", Title);
                return;
            }
            var books = Books.GetAuthorBooks(EditedAuthor.AuthorId);
            foreach (var book in books)
            {
                var author = book.Authors.Find(x => x.AuthorId == EditedAuthor.AuthorId);
                if (author == null)
                    continue;
                author.FirstName = EditedAuthor.FirstName;
                author.LastName = EditedAuthor.LastName;
                author.MiddleName = EditedAuthor.MiddleName;
                author.About = EditedAuthor.About;
            }
        }
        Authors.Sort(x => x.NameLastFirstMiddle, StringComparer.CurrentCultureIgnoreCase);
        ClearEditor();
        HasChanges = true;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
