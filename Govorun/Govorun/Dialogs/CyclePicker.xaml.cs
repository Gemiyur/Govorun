using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Govorun.Models;
using Govorun.Tools;

namespace Govorun.Dialogs;

/// <summary>
/// Класс окна выбора серии.
/// </summary>
public partial class CyclePicker : Window
{
    /// <summary>
    /// Выбранная серия.
    /// </summary>
    public Cycle? PickedCycle;

    public CyclePicker()
    {
        InitializeComponent();
        CyclesListBox.ItemsSource = Db.GetCycles();
    }

    private void CyclesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is TextBlock && CyclesListBox.SelectedItem != null)
        {
            PickedCycle = (Cycle)CyclesListBox.SelectedItem;
            DialogResult = true;
        }
    }

    private void CyclesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        PickButton.IsEnabled = CyclesListBox.SelectedIndex > -1;
    }

    private void PickButton_Click(object sender, RoutedEventArgs e)
    {
        PickedCycle = (Cycle)CyclesListBox.SelectedItem;
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
