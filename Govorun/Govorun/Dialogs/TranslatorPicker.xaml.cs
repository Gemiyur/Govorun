using System.Windows;
using System.Windows.Controls;
using Govorun.Tools;

namespace Govorun.Dialogs;

/// <summary>
/// Класс окна выбора переводчика.
/// </summary>
public partial class TranslatorPicker : Window
{
    /// <summary>
    /// Выбранный переводчик.
    /// </summary>
    public string PickedTranslator = string.Empty;

    public TranslatorPicker()
    {
        InitializeComponent();
        TranslatorsListBox.ItemsSource = Books.Translators;
    }

    private void TranslatorsListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.OriginalSource is TextBlock && TranslatorsListBox.SelectedItem != null)
        {
            PickedTranslator = (string)TranslatorsListBox.SelectedItem;
            DialogResult = true;
        }
    }

    private void TranslatorsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        PickButton.IsEnabled = TranslatorsListBox.SelectedIndex > -1;
    }

    private void PickButton_Click(object sender, RoutedEventArgs e)
    {
        PickedTranslator = (string)TranslatorsListBox.SelectedItem;
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
