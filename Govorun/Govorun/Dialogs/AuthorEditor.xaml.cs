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
    /// Возвращает было ли изменено имя, фамилия или отчество автора.
    /// </summary>
    public bool NameChanged { get; private set; }

    /// <summary>
    /// Редактируемый автор.
    /// </summary>
    private readonly Author author;

    /// <summary>
    /// Инициализирует новый экземпляр класса.
    /// </summary>
    /// <param name="author">Редактируемый автор.</param>
    /// <param name="authors">Список существующих авторов.</param>
    public AuthorEditor(Author author)
    {
        InitializeComponent();
        this.author = author;
        LastNameTextBox.Text = author.LastName;
        FirstNameTextBox.Text = author.FirstName;
        MiddleNameTextBox.Text = author.MiddleName;
        //AboutTextBox.Text = author.About;
    }

    private void CheckSaveButton()
    {
        var nameEmpty =
            string.IsNullOrWhiteSpace(LastNameTextBox.Text) &&
            string.IsNullOrWhiteSpace(FirstNameTextBox.Text) &&
            string.IsNullOrWhiteSpace(MiddleNameTextBox.Text);

        NameChanged =
            LastNameTextBox.Text.Trim() != author.LastName ||
            FirstNameTextBox.Text.Trim() != author.FirstName ||
            MiddleNameTextBox.Text.Trim() != author.MiddleName;

        SaveButton.IsEnabled = !nameEmpty && NameChanged;
        //SaveButton.IsEnabled = !nameEmpty && (NameChanged || AboutTextBox.Text.Trim() != author.About);
    }

    private void LastNameTextBox_TextChanged(object sender, TextChangedEventArgs e) => CheckSaveButton();

    private void FirstNameTextBox_TextChanged(object sender, TextChangedEventArgs e) => CheckSaveButton();

    private void MiddleNameTextBox_TextChanged(object sender, TextChangedEventArgs e) => CheckSaveButton();

    //private void AboutTextBox_TextChanged(object sender, TextChangedEventArgs e) => CheckSaveButton();

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var lastName = LastNameTextBox.Text.Trim();
        var firstName = FirstNameTextBox.Text.Trim();
        var middleName = MiddleNameTextBox.Text.Trim();
        //var about = AboutTextBox.Text.Trim();

        var fio = Author.ConcatNames(lastName, firstName, middleName);
        var foundAuthor = Library.Authors.Find(x => x.NameLastFirstMiddle.Equals(fio, StringComparison.CurrentCultureIgnoreCase));
        if (foundAuthor != null && foundAuthor.AuthorId != author.AuthorId)
        {
            MessageBox.Show("Автор с таким именем уже существует.", Title);
            return;
        }

        var origLastName = author.LastName;
        var origFirstName = author.FirstName;
        var origMiddleName = author.MiddleName;
        var origAbout = author.About;

        author.LastName = lastName;
        author.FirstName = firstName;
        author.MiddleName = middleName;
        //author.About = about;

        var saved = author.AuthorId > 0 ? Library.UpdateAuthor(author) : Library.AddAuthor(author);
        if (!saved)
        {
            MessageBox.Show("Не удалось сохранить автора.", Title);
            author.LastName = origLastName;
            author.FirstName = origFirstName;
            author.MiddleName = origMiddleName;
            author.About = origAbout;
            DialogResult = false;
        }

        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();
}
