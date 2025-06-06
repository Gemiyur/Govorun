using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Gemiyur.Collections;
using Govorun.Models;
using Govorun.Tools;

namespace Govorun.Dialogs;

/// <summary>
/// Класс редактора серий книг.
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
    private readonly ObservableCollectionEx<Cycle> Cycles = [];

    public CyclesEditor()
    {
        InitializeComponent();
        Cycles.AddRange(Db.GetCycles());
        CyclesListBox.ItemsSource = Cycles;
    }

    private void SortCycles() => Cycles.Sort(x => x.Title, StringComparer.CurrentCultureIgnoreCase);

    private void CyclesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        // TODO: Нужно ли редактировать серию по двойному щелчку?
        if (e.OriginalSource is TextBlock && CyclesListBox.SelectedItem != null)
        {
            //EditCycle();
        }
    }

    private void CyclesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        EditButton.IsEnabled = CyclesListBox.SelectedIndex >= 0;
        DeleteButton.IsEnabled = CyclesListBox.SelectedIndex >= 0;
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var editor = new CycleEditor(null, Cycles) { Owner = this };
        if (editor.ShowDialog() != true)
            return;
        var cycle = editor.Cycle;
        cycle.CycleId = Db.InsertCycle(cycle);
        if (cycle.CycleId < 1)
        {
            MessageBox.Show("Не удалось добавить серию.", Title);
            return;
        }
        Cycles.Add(cycle);
        SortCycles();
        HasChanges = true;
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        var cycle = (Cycle)CyclesListBox.SelectedItem;
        var editor = new CycleEditor(cycle, Cycles) { Owner = this };
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
        Cycles.Remove(cycle);
        HasChanges = true;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
