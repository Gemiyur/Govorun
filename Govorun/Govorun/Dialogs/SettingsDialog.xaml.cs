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
        NavAuthorFullNameCheckBox.IsChecked = Properties.Settings.Default.NavAuthorFullName;
        SaveBookWindowsLocationCheckBox.IsChecked = Properties.Settings.Default.SaveBookWindowsLocation;
        SaveMainWindowLocationCheckBox.IsChecked = Properties.Settings.Default.SaveMainWindowLocation;
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
        DbNameTextBox.Text = Db.EnsureDbExtension(dialog.FileName);
        CheckDbNameChanged();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        Properties.Settings.Default.LoadLastBook = LoadLastBookCheckBox.IsChecked == true;
        Properties.Settings.Default.NavAuthorFullName = NavAuthorFullNameCheckBox.IsChecked == true;
        App.GetMainWindow().CheckAuthorsNameFormat();
        Properties.Settings.Default.SaveBookWindowsLocation = SaveBookWindowsLocationCheckBox.IsChecked == true;
        if (!Properties.Settings.Default.SaveBookWindowsLocation)
        {
            Properties.Settings.Default.BookInfoPos = new System.Drawing.Point(0, 0);
            Properties.Settings.Default.BookInfoSize = new System.Drawing.Size(0, 0);
            Properties.Settings.Default.BookmarksPos = new System.Drawing.Point(0, 0);
            Properties.Settings.Default.BookmarksSize = new System.Drawing.Size(0, 0);
            Properties.Settings.Default.ChaptersPos = new System.Drawing.Point(0, 0);
            Properties.Settings.Default.ChaptersSize = new System.Drawing.Size(0, 0);
        }
        Properties.Settings.Default.SaveMainWindowLocation = SaveMainWindowLocationCheckBox.IsChecked == true;
        if (!Properties.Settings.Default.SaveMainWindowLocation)
        {
            Properties.Settings.Default.MainWindowPos = new System.Drawing.Point(0, 0);
            Properties.Settings.Default.MainWindowSize = new System.Drawing.Size(0, 0);
        }
#if DEBUG
        Properties.Settings.Default.DebugDbName = DbNameTextBox.Text;
#else
        Properties.Settings.Default.DbName = DbNameTextBox.Text;
#endif
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
