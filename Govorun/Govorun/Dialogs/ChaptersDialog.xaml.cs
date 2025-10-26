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
    /// Книга.
    /// </summary>
    private Book book;

    /// <summary>
    /// Возвращает или задаёт книгу.
    /// </summary>
    public Book Book
    {
        get => book;
        set
        {
            if (book != value)
                SaveChanged();
            book = value;
            LoadBook();
        }
    }

    /// <summary>
    /// Коллекция глав книги.
    /// </summary>
    private readonly ObservableCollectionEx<Chapter> chapters = [];

    /// <summary>
    /// Были ли изменения в названиях глав книги.
    /// </summary>
    private bool hasChanges = false;

    /// <summary>
    /// Возвращает выбранную главу в списке глав.
    /// </summary>
    private Chapter SelectedChapter => (Chapter)ChaptersListView.SelectedItem;

    /// <summary>
    /// Инициализирует новый экземпляр класса.
    /// </summary>
    /// <param name="book">Книга.</param>
    public ChaptersDialog(Book book)
    {
        InitializeComponent();
        this.book = book;
        TitleTextBlock.FontSize = FontSize + 2;
        TitleEditor.Header = "Название главы";
        ChaptersListView.ItemsSource = chapters;
        LoadBook();
        // Подписка в коде потому что при подписке в дизайнере обработчик отрабатывает,
        // но код обработчика отображается в редакторе как неиспользуемый, что не удобно.
        TitleEditor.IsVisibleChanged += TitleEditor_IsVisibleChanged;
    }

    /// <summary>
    /// Загружает книгу.
    /// </summary>
    private void LoadBook()
    {
        AuthorsTextBlock.Text = book.AuthorNamesFirstLast;
        TitleTextBlock.Text = book.Title;
        chapters.ReplaceRange(book.Chapters);
        TitleEditor.Visibility = Visibility.Collapsed;
        if (IsInitialized)
            SelectCurrentChapter();
    }

    /// <summary>
    /// Сохраняет изменения в базе данных, если они были.
    /// </summary>
    private void SaveChanged()
    {
        if (hasChanges)
        {
            Db.UpdateBook(book);
            hasChanges = false;
        }
    }

    /// <summary>
    /// Выделяет в списке глав текущую (слушаемую).
    /// </summary>
    public void SelectCurrentChapter()
    {
        if (!book.Listening)
            return;
        var player = App.GetMainWindow().Player;
        var position = player.Book == book ? player.PlayPosition : book.PlayPosition;
        ChaptersListView.SelectedItem = chapters.FirstOrDefault(x => x.StartTime <= position && x.EndTime > position);
        ChaptersListView.ScrollIntoView(ChaptersListView.SelectedItem);
        CurrentButton.IsEnabled = book.Listening;
    }

    /// <summary>
    /// Устанавливает состояние элементов для не слушаемой книги.
    /// </summary>
    public void SetNotListening()
    {
        CurrentButton.IsEnabled = false;
        ChaptersListView.SelectedItem = null;
    }

    /// <summary>
    /// Обновляет данные книги в окне.
    /// </summary>
    /// <remarks>Обновляет только авторов и название книги.</remarks>
    public void UpdateBook()
    {
        AuthorsTextBlock.Text = book.AuthorNamesFirstLast;
        TitleTextBlock.Text = book.Title;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (Properties.Settings.Default.SaveBookWindowsLocation &&
            App.SizeDefined(Properties.Settings.Default.ChaptersSize))
        {
            Left = Properties.Settings.Default.ChaptersPos.X;
            Top = Properties.Settings.Default.ChaptersPos.Y;
            Width = Properties.Settings.Default.ChaptersSize.Width;
            Height = Properties.Settings.Default.ChaptersSize.Height;
        }
        SelectCurrentChapter();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        SaveChanged();
        if (Properties.Settings.Default.SaveBookWindowsLocation)
        {
            Properties.Settings.Default.ChaptersPos = new System.Drawing.Point((int)Left, (int)Top);
            Properties.Settings.Default.ChaptersSize = new System.Drawing.Size((int)Width, (int)Height);
        }
        var window = App.GetMainWindow();
        if (window != null)
        {
            App.RestoreWindow(window);
            window.Activate();
        }
    }

    private void ChaptersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        PlayButton.IsEnabled = ChaptersListView.SelectedItems.Count == 1;
        CurrentButton.IsEnabled = book.Listening;
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
        App.GetMainWindow().PlayBook(book, SelectedChapter.StartTime);
        CurrentButton.IsEnabled = true;
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        TitleEditor.Text = SelectedChapter.Title;
        TitleEditor.Visibility = Visibility.Visible;
        MainGrid.IsEnabled = false;
    }

    private void CurrentButton_Click(object sender, RoutedEventArgs e)
    {
        SelectCurrentChapter();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void TitleEditor_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (TitleEditor.Visibility != Visibility.Collapsed)
            return;
        if (TitleEditor.Result)
        {
            SelectedChapter.Title = TitleEditor.Text;
            hasChanges = true;
        }
        MainGrid.IsEnabled = true;
    }
}
