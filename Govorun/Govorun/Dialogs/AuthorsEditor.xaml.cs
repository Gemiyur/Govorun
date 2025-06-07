using System.Windows;
using System.Windows.Controls;
using Gemiyur.Collections;
using Govorun.Models;
using Govorun.Tools;

namespace Govorun.Dialogs;

/// <summary>
/// Класс редактора авторов.
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
    private readonly ObservableCollectionEx<Author> authors = [];

    /// <summary>
    /// Инициализирует новый экземпляр класса.
    /// </summary>
    public AuthorsEditor()
    {
        InitializeComponent();
        authors.AddRange(Db.GetAuthors());
        AuthorsListBox.ItemsSource = authors;
    }

    private void SortAuthors() => authors.Sort(x => x.NameLastFirstMiddle, StringComparer.CurrentCultureIgnoreCase);

    private void AuthorsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        EditButton.IsEnabled = AuthorsListBox.SelectedIndex >= 0;
        DeleteButton.IsEnabled = AuthorsListBox.SelectedIndex >= 0;
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var editor = new AuthorEditor(null, authors) { Owner = this };
        if (editor.ShowDialog() != true)
            return;
        var author = editor.Author;
        author.AuthorId = Db.InsertAuthor(author);
        if (author.AuthorId < 1)
        {
            MessageBox.Show("Не удалось добавить автора.", Title);
            return;
        }
        authors.Add(author);
        SortAuthors();
        HasChanges = true;
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        var author = (Author)AuthorsListBox.SelectedItem;
        var editor = new AuthorEditor(author, authors) { Owner = this };
        if (editor.ShowDialog() != true)
            return;
        if (!Db.UpdateAuthor(author))
        {
            MessageBox.Show("Не удалось сохранить автора.", Title);
            return;
        }
        SortAuthors();
        HasChanges = true;
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        var author = (Author)AuthorsListBox.SelectedItem;
        if (!Db.DeleteAuthor(author.AuthorId))
        {
            MessageBox.Show("Не удалось удалить автора.", Title);
            return;
        }
        authors.Remove(author);
        HasChanges = true;

        // TODO: Нужно ли каскадное удаление? Если да, то закомментированное сделано неправильно.
        //var books = Library.GetAuthorBooks(author.AuthorId);
        //const string message = "Автор будет так же удалён из всех книг.\nУдалить автора?";
        //if (books.Any() && MessageBox.Show(message, Title, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
        //{
        //    return;
        //}

        //foreach (var book in books)
        //{
        //    book.Authors.RemoveAll(x => x.AuthorId == author.AuthorId);
        //}

    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
