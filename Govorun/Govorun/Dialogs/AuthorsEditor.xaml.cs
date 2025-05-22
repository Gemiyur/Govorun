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
        if (!LastNameTextBox.Text.Any() && !FirstNameTextBox.Text.Any() && !MiddleNameTextBox.Text.Any())
        {
            SaveButton.IsEnabled = false;
            if (EditedAuthor == null)
                ClearButton.IsEnabled = false;
            return;
        }
        SaveButton.IsEnabled =
            LastNameTextBox.Text != LastNameTextBlock.Text ||
            FirstNameTextBox.Text != FirstNameTextBlock.Text ||
            MiddleNameTextBox.Text != MiddleNameTextBlock.Text;
        ClearButton.IsEnabled = true;
    }

    /// <summary>
    /// Очищает данные редактора автора.
    /// </summary>
    private void ClearEditor()
    {
        EditedAuthor = null;
        LastNameTextBlock.Text = string.Empty;
        FirstNameTextBlock.Text = string.Empty;
        LastNameTextBox.Text = string.Empty;
        FirstNameTextBox.Text = string.Empty;
        MiddleNameTextBox.Text = string.Empty;
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
        const string caption = "Подтверждение удаления";
        const string message = "Автор будет так же удалён из всех книг.\nУдалить автора?";
        var author = (Author)AuthorsListBox.SelectedItem;
        var books = Books.GetAuthorBooks(author.AuthorId);
        if (books.Any() && MessageBox.Show(message, caption, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
        {
            return;
        }
        if (!Db.DeleteAuthor(author.AuthorId))
        {
            MessageBox.Show("Не удалось удалить автора.", "Ошибка");
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

    private void ClearButton_Click(object sender, RoutedEventArgs e) => ClearEditor();

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (EditedAuthor == null)
        {
            var author = new Author()
            {
                FirstName = FirstNameTextBox.Text,
                LastName = LastNameTextBox.Text,
                MiddleName = MiddleNameTextBox.Text
            };
            author.AuthorId = Db.InsertAuthor(author);
            if (author.AuthorId < 1)
            {
                MessageBox.Show("Не удалось добавить автора.", "Ошибка");
                return;
            }
            Authors.Add(author);
        }
        else
        {
            EditedAuthor.FirstName = FirstNameTextBox.Text;
            EditedAuthor.LastName = LastNameTextBox.Text;
            EditedAuthor.MiddleName = MiddleNameTextBox.Text;
            if (!Db.UpdateAuthor(EditedAuthor))
            {
                MessageBox.Show("Не удалось изменить данные автора.", "Ошибка");
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
            }
        }
        Authors.Sort(x => x.NameLastFirstMiddle, StringComparer.CurrentCultureIgnoreCase);
        ClearEditor();
        HasChanges = true;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
