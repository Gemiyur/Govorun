using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Gemiyur.Collections;
using Govorun.Tools;

namespace Govorun.Dialogs;

/// <summary>
/// Класс окна поиска файлов книг.
/// </summary>
public partial class FindFilesDialog : Window
{
    #region Для запрета сворачивания и разворачивания окна на весь экран.

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    private const int GWL_STYLE = -16;
    private const int WS_MAXIMIZEBOX = 0x10000;
    private const int WS_MINIMIZEBOX = 0x20000;

    #endregion
    /// <summary>
    /// Папка с файлами книг.
    /// </summary>
    private string folder = string.Empty;

    /// <summary>
    /// Список имён файлов книг в папке без пути папки.
    /// </summary>
    /// <remarks>Файлы книг отсортированы по имени.</remarks>
    private readonly List<string> files = [];

    /// <summary>
    /// Коллекция отображаемых файлов книг.
    /// </summary>
    private readonly ObservableCollectionEx<string> shownFiles = [];

    private readonly BackgroundWorker worker = new();

    /// <summary>
    /// Инициализирует новый экземпляр класса.
    /// </summary>
    public FindFilesDialog()
    {
        InitializeComponent();
        worker.DoWork += Worker_DoWork;
        worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        FilesListBox.ItemsSource = shownFiles;
    }

    /// <summary>
    /// Возвращает имя файла книги с полным путём.
    /// </summary>
    /// <param name="name">Имя файла книги без пути папки.</param>
    /// <returns>Имя файла книги с полным путём.</returns>
    private string FullName(string name) => Path.Combine(folder, name);

    /// <summary>
    /// Обновляет количество файлов книг в списке файлов книг.
    /// </summary>
    private void UpdateCount() => CountTextBlock.Text = shownFiles.Count.ToString();

    private void Window_SourceInitialized(object sender, EventArgs e)
    {
        IntPtr handle = new WindowInteropHelper(this).Handle;
        _ = SetWindowLong(handle, GWL_STYLE, GetWindowLong(handle, GWL_STYLE) & ~WS_MINIMIZEBOX);
        _ = SetWindowLong(handle, GWL_STYLE, GetWindowLong(handle, GWL_STYLE) & ~WS_MAXIMIZEBOX);
    }

    private void Worker_DoWork(object? sender, DoWorkEventArgs e)
    {
        var trimCount = folder.Length + 1;
        var options = new EnumerationOptions() { RecurseSubdirectories = true };
        var list = Directory.EnumerateFiles(folder, "*.m4b", options)
            .Where(x => !Library.BookWithFileExists(x))
            .Select(x => x[trimCount..])
            .OrderBy(x => x, StringComparer.CurrentCultureIgnoreCase);
        files.AddRange(list);
    }

    private void Worker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
        if (e.Error != null)
            MessageBox.Show(e.Error.Message, Title);
        else
            shownFiles.ReplaceRange(files);
        UpdateCount();
        FolderButton.IsEnabled = true;
        FindButton.IsEnabled = true;
        FoundStackPanel.Visibility = Visibility.Visible;
        FindingTextBlock.Visibility = Visibility.Collapsed;
    }

    private void FolderButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = App.PickBooksFolderDialog;
        if (dialog.ShowDialog() != true ||
            dialog.FolderName.Equals(folder, StringComparison.CurrentCultureIgnoreCase))
        {
            return;
        }
        folder = dialog.FolderName;
        FolderTextBox.Text = folder;
        files.Clear();
        shownFiles.Clear();
        UpdateCount();
        FindButton.IsEnabled = true;
    }

    private void FindButton_Click(object sender, RoutedEventArgs e)
    {
        files.Clear();
        shownFiles.Clear();
        FolderButton.IsEnabled = false;
        FindButton.IsEnabled = false;
        FoundStackPanel.Visibility = Visibility.Collapsed;
        FindingTextBlock.Visibility = Visibility.Visible;
        worker.RunWorkerAsync();
    }

    private void FilesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        BookButton.IsEnabled = FilesListBox.SelectedItems.Count == 1;
    }

    private void BookButton_Click(object sender, RoutedEventArgs e)
    {

    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
