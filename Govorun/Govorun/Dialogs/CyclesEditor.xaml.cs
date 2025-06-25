using System.Windows;
using System.Windows.Controls;
using Gemiyur.Collections;
using Govorun.Models;
using Govorun.Tools;

namespace Govorun.Dialogs;

/// <summary>
/// Класс редактора серий.
/// </summary>
public partial class CyclesEditor : Window
{
    /// <summary>
    /// Были ли изменения в коллекции серий.
    /// </summary>
    public bool HasChanges;

    /// <summary>
    /// Коллекция серий.
    /// </summary>
    private readonly ObservableCollectionEx<Cycle> cycles = [];

    public CyclesEditor()
    {
        InitializeComponent();
        cycles.AddRange(Db.GetCycles());
        CyclesListBox.ItemsSource = cycles;
    }

    private void SortCycles() => cycles.Sort(x => x.Title, StringComparer.CurrentCultureIgnoreCase);

    private void CyclesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        EditButton.IsEnabled = CyclesListBox.SelectedIndex >= 0;
        DeleteButton.IsEnabled = CyclesListBox.SelectedIndex >= 0 &&
                                 !Library.CycleHasBooks(((Cycle)CyclesListBox.SelectedItem).CycleId);
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var editor = new CycleEditor(null, cycles) { Owner = this };
        if (editor.ShowDialog() != true)
            return;
        var cycle = editor.Cycle;
        cycle.CycleId = Db.InsertCycle(cycle);
        if (cycle.CycleId < 1)
        {
            MessageBox.Show("Не удалось добавить серию.", Title);
            return;
        }
        cycles.Add(cycle);
        SortCycles();
        HasChanges = true;
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        var cycle = (Cycle)CyclesListBox.SelectedItem;
        var editor = new CycleEditor(cycle, cycles) { Owner = this };
        if (editor.ShowDialog() != true)
            return;
        if (!Db.UpdateCycle(cycle))
        {
            MessageBox.Show("Не удалось сохранить серию.", Title);
            return;
        }
        SortCycles();
        HasChanges = true;
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        var cycle = (Cycle)CyclesListBox.SelectedItem;
        if (!Db.DeleteCycle(cycle.CycleId))
        {
            MessageBox.Show("Не удалось удалить серию.", Title);
            return;
        }
        cycles.Remove(cycle);
        HasChanges = true;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
