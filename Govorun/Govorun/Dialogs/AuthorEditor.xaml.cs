using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Govorun.Models;
using Govorun.Tools;

namespace Govorun.Dialogs;

/// <summary>
/// Класс редактора автора.
/// </summary>
public partial class AuthorEditor : Window
{
    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    private const int GWL_STYLE = -16;
    private const int WS_MAXIMIZEBOX = 0x10000;
    private const int WS_MINIMIZEBOX = 0x20000;

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
    public AuthorEditor(Author author)
    {
        InitializeComponent();
        this.author = author;
        LastNameTextBox.Text = author.LastName;
        FirstNameTextBox.Text = author.FirstName;
        MiddleNameTextBox.Text = author.MiddleName;
        AboutTextBox.Text = author.About;
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

        SaveButton.IsEnabled = !nameEmpty && (NameChanged || AboutTextBox.Text.Trim() != author.About);
    }

    private void Window_SourceInitialized(object sender, EventArgs e)
    {
        IntPtr handle = new WindowInteropHelper(this).Handle;
        _ = SetWindowLong(handle, GWL_STYLE, GetWindowLong(handle, GWL_STYLE) & ~WS_MINIMIZEBOX);
        _ = SetWindowLong(handle, GWL_STYLE, GetWindowLong(handle, GWL_STYLE) & ~WS_MAXIMIZEBOX);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (Properties.Settings.Default.SaveAuthorEditorSize &&
            App.SizeDefined(Properties.Settings.Default.AuthorEditorSize))
        {
            Width = Properties.Settings.Default.AuthorEditorSize.Width;
            Height = Properties.Settings.Default.AuthorEditorSize.Height;
        }
        App.CenterOnScreen(this);
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        if (Properties.Settings.Default.SaveAuthorEditorSize)
        {
            Properties.Settings.Default.AuthorEditorSize = new System.Drawing.Size((int)Width, (int)Height);
        }
    }

    private void LastNameTextBox_TextChanged(object sender, TextChangedEventArgs e) => CheckSaveButton();

    private void FirstNameTextBox_TextChanged(object sender, TextChangedEventArgs e) => CheckSaveButton();

    private void MiddleNameTextBox_TextChanged(object sender, TextChangedEventArgs e) => CheckSaveButton();

    private void AboutTextBox_TextChanged(object sender, TextChangedEventArgs e) => CheckSaveButton();

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var lastName = LastNameTextBox.Text.Trim();
        var firstName = FirstNameTextBox.Text.Trim();
        var middleName = MiddleNameTextBox.Text.Trim();
        var about = AboutTextBox.Text.Trim();

        var fio = Author.ConcatNames(lastName, firstName, middleName);
        var foundAuthor = Library.Authors.Find(x => x.NameLastFirstMiddle.Equals(fio, StringComparison.CurrentCultureIgnoreCase));
        if (foundAuthor != null && foundAuthor.AuthorId != author.AuthorId)
        {
            MessageBox.Show("Автор с таким именем уже существует.", Title);
            return;
        }

        var origAuthor = author.Clone();

        author.LastName = lastName;
        author.FirstName = firstName;
        author.MiddleName = middleName;
        author.About = about;

        var saved = author.AuthorId > 0 ? Library.UpdateAuthor(author) : Library.AddAuthor(author);
        if (!saved)
        {
            MessageBox.Show("Не удалось сохранить автора.", Title);
            origAuthor.CopyTo(author);
            DialogResult = false;
            return;
        }

        if (NameChanged)
        {
            App.GetMainWindow().UpdateShownBooksAuthors();
            var bookInfoWindow = App.GetBookInfoDialog();
            if (bookInfoWindow != null && Library.BookHasAuthor(bookInfoWindow.Book, author.AuthorId))
                bookInfoWindow.UpdateAuthors();
            var bookmarksWindow = App.GetBookmarksDialog();
            if (bookmarksWindow != null && Library.BookHasAuthor(bookmarksWindow.Book, author.AuthorId))
                bookmarksWindow.UpdateAuthors();
            var chaptersWindow = App.GetChaptersDialog();
            if (chaptersWindow != null && Library.BookHasAuthor(chaptersWindow.Book, author.AuthorId))
                chaptersWindow.UpdateAuthors();
        }

        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();
}
