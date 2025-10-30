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
        authors.AddRange(Library.Authors);
        AuthorsListBox.ItemsSource = authors;
    }

    private void SortAuthors() => authors.Sort(x => x.NameLastFirstMiddle, StringComparer.CurrentCultureIgnoreCase);

    private void AuthorsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        EditButton.IsEnabled = AuthorsListBox.SelectedIndex >= 0;
        DeleteButton.IsEnabled = AuthorsListBox.SelectedIndex >= 0 &&
                                 !Library.AuthorHasBooks(((Author)AuthorsListBox.SelectedItem).AuthorId);
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var author = new Author();
        var editor = new AuthorEditor(author) { Owner = this };
        if (editor.ShowDialog() != true)
            return;
        authors.Add(author);
        SortAuthors();
        HasChanges = true;
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        var author = (Author)AuthorsListBox.SelectedItem;
        var editor = new AuthorEditor(author) { Owner = this };
        if (editor.ShowDialog() != true || !editor.NameChanged)
            return;
        SortAuthors();
        HasChanges = true;
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        var author = (Author)AuthorsListBox.SelectedItem;
        if (!App.ConfirmAction($"Удалить автора \"{author.NameLastFirstMiddle}\" из библиотеки?", Title))
        {
            return;
        }
        if (!Library.DeleteAuthor(author))
        {
            MessageBox.Show("Не удалось удалить автора.", Title);
            return;
        }
        authors.Remove(author);
        HasChanges = true;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
