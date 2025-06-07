using System.Windows;
using System.Windows.Controls;
using Govorun.Models;
using Govorun.Tools;

namespace Govorun.Dialogs;

/// <summary>
/// Класс редактора автора.
/// </summary>
public partial class AuthorEditor : Window
{
    /// <summary>
    /// Редактируемый автор.
    /// </summary>
    public Author Author;

    /// <summary>
    /// Список существующих авторов.
    /// </summary>
    private readonly List<Author> authors = [];

    /// <summary>
    /// Инициализирует новый экземпляр класса.
    /// </summary>
    /// <param name="author">Редактируемый автор.</param>
    /// <param name="authors">Список существующих авторов.</param>
    public AuthorEditor(Author? author, IEnumerable<Author>? authors)
    {
        InitializeComponent();
        Author = author ?? new Author();
        LastNameTextBox.Text = Author.LastName;
        FirstNameTextBox.Text = Author.FirstName;
        MiddleNameTextBox.Text = Author.MiddleName;
        this.authors.AddRange(authors ?? Db.GetAuthors());
    }

    private void CheckSaveButton()
    {
        var authorNameEmpty =
            string.IsNullOrWhiteSpace(LastNameTextBox.Text) &&
            string.IsNullOrWhiteSpace(FirstNameTextBox.Text) &&
            string.IsNullOrWhiteSpace(MiddleNameTextBox.Text);

        var authorNameChanged =
            LastNameTextBox.Text.Trim() != Author.LastName ||
            FirstNameTextBox.Text.Trim() != Author.FirstName ||
            MiddleNameTextBox.Text.Trim() != Author.MiddleName;

        SaveButton.IsEnabled = !authorNameEmpty && authorNameChanged;
    }

    private void LastNameTextBox_TextChanged(object sender, TextChangedEventArgs e) => CheckSaveButton();

    private void FirstNameTextBox_TextChanged(object sender, TextChangedEventArgs e) => CheckSaveButton();

    private void MiddleNameTextBox_TextChanged(object sender, TextChangedEventArgs e) => CheckSaveButton();

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var lastName = LastNameTextBox.Text.Trim();
        var firstName = FirstNameTextBox.Text.Trim();
        var middleName = MiddleNameTextBox.Text.Trim();

        var fio = Author.ConcatNames(lastName, firstName, middleName);

        if (authors.Exists(x => x.NameLastFirstMiddle.Equals(fio, StringComparison.CurrentCultureIgnoreCase)))
        {
            MessageBox.Show("Автор с таким именем уже существует.", Title);
            return;
        }

        Author.LastName = lastName;
        Author.FirstName = firstName;
        Author.MiddleName = middleName;
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();
}
