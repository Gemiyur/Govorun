using System.IO;
using System.Windows;
using Govorun.Tools;

namespace Govorun.Dialogs;

/// <summary>
/// Класс окна настроек приложения.
/// </summary>
public partial class SettingsDialog : Window
{
    private bool DbNameChanged =>
        !DbNameTextBox.Text.Equals(App.DbName, StringComparison.CurrentCultureIgnoreCase);

    public SettingsDialog()
    {
        InitializeComponent();
        LoadLastBookCheckBox.IsChecked = Properties.Settings.Default.LoadLastBook;
        NavPanelAuthorFullNameCheckBox.IsChecked = Properties.Settings.Default.NavPanelAuthorFullName;
        BooksListAuthorFullNameCheckBox.IsChecked = Properties.Settings.Default.BooksListAuthorFullName;
        BookWindowsAuthorFullNameCheckBox.IsChecked = Properties.Settings.Default.BookWindowsAuthorFullName;
        SaveMainWindowLocationCheckBox.IsChecked = Properties.Settings.Default.SaveMainWindowLocation;
        SaveBookWindowsLocationCheckBox.IsChecked = Properties.Settings.Default.SaveBookWindowsLocation;
        SaveAuthorWindowSizeCheckBox.IsChecked = Properties.Settings.Default.SaveAuthorWindowSize;
        SaveCycleWindowSizeCheckBox.IsChecked = Properties.Settings.Default.SaveCycleWindowSize;
        SaveAuthorEditorSizeCheckBox.IsChecked = Properties.Settings.Default.SaveAuthorEditorSize;
        SaveCycleEditorSizeCheckBox.IsChecked = Properties.Settings.Default.SaveCycleEditorSize;
#if DEBUG
        DbNameTextBox.Text = Properties.Settings.Default.DebugDbName;
#else
        DbNameTextBox.Text = Properties.Settings.Default.DbName;
#endif
        CheckDbNameChanged();
    }

    private void CheckDbNameChanged()
    {
        DbChangedStackPanel.Visibility = DbNameChanged ? Visibility.Visible : Visibility.Collapsed;
        DbNotChangedStackPanel.Visibility = DbNameChanged ? Visibility.Collapsed : Visibility.Visible;
        DbShrinkButton.IsEnabled = !DbNameChanged;
    }

    private void SettingsTabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        ResetButton.IsEnabled = SettingsTabControl.SelectedItem == InterfaceTabItem;
    }

    private void CheckLibraryButton_Click(object sender, RoutedEventArgs e)
    {
        var books = Library.Books.FindAll(
            x => !x.FileExists).OrderBy(x => x.Title, StringComparer.CurrentCultureIgnoreCase);
        if (!books.Any())
        {
            MessageBox.Show("Проверка библиотеки выполнена.\nПроблем не обнаружено.", Title);
            return;
        }
        var dialog = new CheckLibraryDialog(books) { Owner = this };
        dialog.ShowDialog();
    }

    private void DbShrinkButton_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("Сжать базу данных библиотеки,", Title, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            return;
        var path = Path.GetDirectoryName(App.DbName) ?? "";
        var name = Path.GetFileNameWithoutExtension(App.DbName);
        var ext = Path.GetExtension(App.DbName);
        var filename = Path.Combine(path, name + "-backup" + ext);
        try { File.Delete(filename); }
        catch { }
        Db.Shrink();
        MessageBox.Show("Сжатие базы данных библиотеки завершено.", Title);
    }

    private void DbNameButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = App.PickDatabaseDialog;
        if (dialog.ShowDialog() != true)
            return;
        var dbName = Db.EnsureDbExtension(dialog.FileName);
        if (!Db.ValidateDb(dbName))
        {
            MessageBox.Show("Файл не является базой данных Говоруна или повреждён.", Title);
            return;
        }
        DbNameTextBox.Text = dbName;
        CheckDbNameChanged();
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        NavPanelAuthorFullNameCheckBox.IsChecked = Properties.Settings.Default.PresetNavPanelAuthorFullName;
        BooksListAuthorFullNameCheckBox.IsChecked = Properties.Settings.Default.PresetBooksListAuthorFullName;
        BookWindowsAuthorFullNameCheckBox.IsChecked = Properties.Settings.Default.PresetBookWindowsAuthorFullName;
        SaveMainWindowLocationCheckBox.IsChecked = Properties.Settings.Default.PresetSaveMainWindowLocation;
        SaveBookWindowsLocationCheckBox.IsChecked = Properties.Settings.Default.PresetSaveBookWindowsLocation;
        SaveAuthorWindowSizeCheckBox.IsChecked = Properties.Settings.Default.PresetSaveAuthorWindowSize;
        SaveCycleWindowSizeCheckBox.IsChecked = Properties.Settings.Default.PresetSaveCycleWindowSize;
        SaveAuthorEditorSizeCheckBox.IsChecked = Properties.Settings.Default.PresetSaveAuthorEditorSize;
        SaveCycleEditorSizeCheckBox.IsChecked = Properties.Settings.Default.PresetSaveCycleEditorSize;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        Properties.Settings.Default.LoadLastBook = LoadLastBookCheckBox.IsChecked == true;

        Properties.Settings.Default.NavPanelAuthorFullName = NavPanelAuthorFullNameCheckBox.IsChecked == true;
        App.GetMainWindow().CheckNavPanelAuthorsNameFormat();

        Properties.Settings.Default.BooksListAuthorFullName = BooksListAuthorFullNameCheckBox.IsChecked == true;
        App.GetMainWindow().UpdateShownBooks();

        Properties.Settings.Default.BookWindowsAuthorFullName = BookWindowsAuthorFullNameCheckBox.IsChecked == true;
        App.GetBookInfoDialog()?.UpdateAuthors();
        App.GetChaptersDialog()?.UpdateAuthors();
        App.GetBookmarksDialog()?.UpdateAuthors();

        Properties.Settings.Default.SaveMainWindowLocation = SaveMainWindowLocationCheckBox.IsChecked == true;
        if (!Properties.Settings.Default.SaveMainWindowLocation)
        {
            Properties.Settings.Default.MainWindowPos = new System.Drawing.Point(0, 0);
            Properties.Settings.Default.MainWindowSize = new System.Drawing.Size(0, 0);
        }

        Properties.Settings.Default.SaveBookWindowsLocation = SaveBookWindowsLocationCheckBox.IsChecked == true;
        if (!Properties.Settings.Default.SaveBookWindowsLocation)
        {
            Properties.Settings.Default.BookInfoPos = new System.Drawing.Point(0, 0);
            Properties.Settings.Default.BookInfoSize = new System.Drawing.Size(0, 0);
            Properties.Settings.Default.ChaptersPos = new System.Drawing.Point(0, 0);
            Properties.Settings.Default.ChaptersSize = new System.Drawing.Size(0, 0);
            Properties.Settings.Default.BookmarksPos = new System.Drawing.Point(0, 0);
            Properties.Settings.Default.BookmarksSize = new System.Drawing.Size(0, 0);
        }

        Properties.Settings.Default.SaveAuthorWindowSize = SaveAuthorWindowSizeCheckBox.IsChecked == true;
        if (!Properties.Settings.Default.SaveAuthorWindowSize)
        {
            Properties.Settings.Default.AuthorWindowSize = new System.Drawing.Size(0, 0);
        }

        Properties.Settings.Default.SaveCycleWindowSize = SaveCycleWindowSizeCheckBox.IsChecked == true;
        if (!Properties.Settings.Default.SaveCycleWindowSize)
        {
            Properties.Settings.Default.CycleWindowSize = new System.Drawing.Size(0, 0);
        }

        Properties.Settings.Default.SaveAuthorEditorSize = SaveAuthorEditorSizeCheckBox.IsChecked == true;
        if (!Properties.Settings.Default.SaveAuthorEditorSize)
        {
            Properties.Settings.Default.AuthorEditorSize = new System.Drawing.Size(0, 0);
        }

        Properties.Settings.Default.SaveCycleEditorSize = SaveCycleEditorSizeCheckBox.IsChecked == true;
        if (!Properties.Settings.Default.SaveCycleEditorSize)
        {
            Properties.Settings.Default.CycleEditorSize = new System.Drawing.Size(0, 0);
        }

        if (DbNameChanged)
        {
#if DEBUG
            Properties.Settings.Default.DebugDbName = DbNameTextBox.Text;
#else
            Properties.Settings.Default.DbName = DbNameTextBox.Text;
#endif
            var newDb = !File.Exists(DbNameTextBox.Text);
            using var db = Db.GetDatabase(DbNameTextBox.Text);
            if (newDb)
                Db.InitializeCollections(db);
        }

        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
