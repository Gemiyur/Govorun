using System.Windows;
using System.Windows.Controls;
using Gemiyur.Collections;
using Govorun.Models;
using Govorun.Tools;

namespace Govorun.Dialogs;

/// <summary>
/// Класс окна содержания книги.
/// </summary>
public partial class ChaptersDialog : Window
{
    /// <summary>
    /// Выбранная глава книги.
    /// </summary>
    public Chapter? Chapter;

    /// <summary>
    /// Книга.
    /// </summary>
    private readonly Book book;

    /// <summary>
    /// Коллекция глав книги.
    /// </summary>
    private readonly ObservableCollectionEx<Chapter> chapters = [];

    /// <summary>
    /// Были ли изменения в названиях глав книги.
    /// </summary>
    private bool hasChanges = false;

    /// <summary>
    /// Инициализирует новый экземпляр класса.
    /// </summary>
    /// <param name="book">Книга.</param>
    public ChaptersDialog(Book book)
    {
        InitializeComponent();
        this.book = book;
        TitleTextBlock.FontSize = FontSize + 2;
        AuthorsTextBlock.Text = book.AuthorNamesFirstLast;
        TitleTextBlock.Text = book.Title;
        chapters.AddRange(book.Chapters);
        ChaptersListView.ItemsSource = chapters;
    }

    /// <summary>
    /// Выделяет в списке глав текущую (слушаемую).
    /// </summary>
    private void SelectCurrentChapter()
    {
        if (!book.Listening)
            return;
        var player = Owner is not null and MainWindow ? ((MainWindow)Owner).Player : null;
        var position = player != null && player.Book == book ? player.PlayPosition : book.PlayPosition;
        ChaptersListView.SelectedItem = chapters.FirstOrDefault(x => x.StartTime <= position && x.EndTime > position);
        ChaptersListView.ScrollIntoView(ChaptersListView.SelectedItem);
        CurrentButton.IsEnabled = book.Listening;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        SelectCurrentChapter();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        if (hasChanges)
            Db.UpdateBook(book);
    }

    private void ChaptersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        PlayButton.IsEnabled = ChaptersListView.SelectedItems.Count == 1;
        EditButton.IsEnabled = ChaptersListView.SelectedItems.Count == 1;
    }

    private void ChaptersListView_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        var listView = (ListView)sender;
        var gridView = (GridView)listView.View;
        var totalWidth = listView.ActualWidth - (SystemParameters.VerticalScrollBarWidth + 10);
        var usedWidth = 0.0;
        for (var i = 1; i < gridView.Columns.Count; i++)
        {
            usedWidth += gridView.Columns[i].Width;
        }
        gridView.Columns[0].Width = totalWidth - usedWidth;
    }

    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        Chapter = (Chapter)ChaptersListView.SelectedItem;
        DialogResult = true;
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        var chapter = (Chapter)ChaptersListView.SelectedItem;
        var title = chapter.Title;
        var editor = new ChapterEditor(title) { Owner = this };
        if (editor.ShowDialog() != true)
            return;
        chapter.Title = editor.ChapterTitle;
        hasChanges = true;
    }

    private void CurrentButton_Click(object sender, RoutedEventArgs e)
    {
        SelectCurrentChapter();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
