using System.Windows;

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
        CreatorM4BTextBox.Text = Properties.Settings.Default.CreatorM4B;
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
    }

    private void CreatorM4BButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = App.PickCreatorM4BDialog;
        if (dialog.ShowDialog() != true)
            return;
        CreatorM4BTextBox.Text = dialog.FileName;
    }

    private void DbNameButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = App.PickDatabaseDialog;
        if (dialog.ShowDialog() != true)
            return;
        DbNameTextBox.Text = App.EnsureDbExtension(dialog.FileName);
        CheckDbNameChanged();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        Properties.Settings.Default.LoadLastBook = LoadLastBookCheckBox.IsChecked == true;
        Properties.Settings.Default.CreatorM4B = CreatorM4BTextBox.Text;
        if (DbNameChanged)
        {
#if DEBUG
            Properties.Settings.Default.DebugDbName = DbNameTextBox.Text;
#else
            Properties.Settings.Default.DbName = DbNameTextBox.Text;
#endif
        }
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
