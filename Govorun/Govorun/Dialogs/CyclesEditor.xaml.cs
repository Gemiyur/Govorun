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
        cycles.AddRange(Library.Cycles);
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
        var cycle = new Cycle();
        var editor = new CycleEditor(cycle) { Owner = this };
        if (editor.ShowDialog() != true)
            return;
        cycles.Add(cycle);
        SortCycles();
        HasChanges = true;
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        var cycle = (Cycle)CyclesListBox.SelectedItem;
        var editor = new CycleEditor(cycle) { Owner = this };
        if (editor.ShowDialog() != true || !editor.TitleChanged)
            return;
        SortCycles();
        HasChanges = true;
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        var cycle = (Cycle)CyclesListBox.SelectedItem;
        if (!App.ConfirmAction($"Удалить серию \"{cycle.Title}\" из библиотеки?", Title))
        {
            return;
        }
        if (!Library.DeleteCycle(cycle))
        {
            MessageBox.Show("Не удалось удалить серию.", Title);
            return;
        }
        cycles.Remove(cycle);
        HasChanges = true;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
