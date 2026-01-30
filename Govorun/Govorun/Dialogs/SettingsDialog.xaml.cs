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
        BookListAuthorFullNameCheckBox.IsChecked = Properties.Settings.Default.BookListAuthorFullName;
        BookInfoAuthorFullNameCheckBox.IsChecked = Properties.Settings.Default.BookInfoAuthorFullName;
        ChaptersAuthorFullNameCheckBox.IsChecked = Properties.Settings.Default.ChaptersAuthorFullName;
        BookmarksAuthorFullNameCheckBox.IsChecked = Properties.Settings.Default.BookmarksAuthorFullName;
        SaveMainWindowLocationCheckBox.IsChecked = Properties.Settings.Default.SaveMainWindowLocation;
        SaveBookInfoWindowLocationCheckBox.IsChecked = Properties.Settings.Default.SaveBookInfoWindowLocation;
        SaveChaptersWindowLocationCheckBox.IsChecked = Properties.Settings.Default.SaveChaptersWindowLocation;
        SaveBookmarksWindowLocationCheckBox.IsChecked = Properties.Settings.Default.SaveBookmarksWindowLocation;
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
        BookListAuthorFullNameCheckBox.IsChecked = Properties.Settings.Default.PresetBookListAuthorFullName;
        BookInfoAuthorFullNameCheckBox.IsChecked = Properties.Settings.Default.PresetBookInfoAuthorFullName;
        ChaptersAuthorFullNameCheckBox.IsChecked = Properties.Settings.Default.PresetChaptersAuthorFullName;
        BookmarksAuthorFullNameCheckBox.IsChecked = Properties.Settings.Default.PresetBookmarksAuthorFullName;
        SaveMainWindowLocationCheckBox.IsChecked = Properties.Settings.Default.PresetSaveMainWindowLocation;
        SaveBookInfoWindowLocationCheckBox.IsChecked = Properties.Settings.Default.PresetSaveBookInfoWindowLocation;
        SaveChaptersWindowLocationCheckBox.IsChecked = Properties.Settings.Default.PresetSaveChaptersWindowLocation;
        SaveBookmarksWindowLocationCheckBox.IsChecked = Properties.Settings.Default.PresetSaveBookmarksWindowLocation;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        Properties.Settings.Default.LoadLastBook = LoadLastBookCheckBox.IsChecked == true;

        Properties.Settings.Default.NavPanelAuthorFullName = NavPanelAuthorFullNameCheckBox.IsChecked == true;
        App.GetMainWindow().CheckNavPanelAuthorsNameFormat();

        Properties.Settings.Default.BookListAuthorFullName = BookListAuthorFullNameCheckBox.IsChecked == true;
        App.GetMainWindow().UpdateShownBooks();

        Properties.Settings.Default.BookInfoAuthorFullName = BookInfoAuthorFullNameCheckBox.IsChecked == true;
        App.FindBookInfoWindow()?.UpdateAuthors();

        Properties.Settings.Default.ChaptersAuthorFullName = ChaptersAuthorFullNameCheckBox.IsChecked == true;
        App.FindChaptersWindow()?.UpdateAuthors();

        Properties.Settings.Default.BookmarksAuthorFullName = BookmarksAuthorFullNameCheckBox.IsChecked == true;
        App.FindBookmarksWindow()?.UpdateAuthors();

        Properties.Settings.Default.SaveMainWindowLocation = SaveMainWindowLocationCheckBox.IsChecked == true;
        if (!Properties.Settings.Default.SaveMainWindowLocation)
        {
            Properties.Settings.Default.MainWindowPos = new System.Drawing.Point(0, 0);
            Properties.Settings.Default.MainWindowSize = new System.Drawing.Size(0, 0);
        }

        Properties.Settings.Default.SaveBookInfoWindowLocation = SaveBookInfoWindowLocationCheckBox.IsChecked == true;
        if (!Properties.Settings.Default.SaveBookInfoWindowLocation)
        {
            Properties.Settings.Default.BookInfoPos = new System.Drawing.Point(0, 0);
            Properties.Settings.Default.BookInfoSize = new System.Drawing.Size(0, 0);
        }

        Properties.Settings.Default.SaveChaptersWindowLocation = SaveChaptersWindowLocationCheckBox.IsChecked == true;
        if (!Properties.Settings.Default.SaveChaptersWindowLocation)
        {
            Properties.Settings.Default.ChaptersPos = new System.Drawing.Point(0, 0);
            Properties.Settings.Default.ChaptersSize = new System.Drawing.Size(0, 0);
        }

        Properties.Settings.Default.SaveBookmarksWindowLocation = SaveBookmarksWindowLocationCheckBox.IsChecked == true;
        if (!Properties.Settings.Default.SaveBookmarksWindowLocation)
        {
            Properties.Settings.Default.BookmarksPos = new System.Drawing.Point(0, 0);
            Properties.Settings.Default.BookmarksSize = new System.Drawing.Size(0, 0);
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
