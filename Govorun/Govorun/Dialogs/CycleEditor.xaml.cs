using System.Windows;
using System.Windows.Controls;
using Govorun.Models;
using Govorun.Tools;

namespace Govorun.Dialogs;

/// <summary>
/// Класс редактора серии.
/// </summary>
public partial class CycleEditor : Window
{
    /// <summary>
    /// Редактируемая серия.
    /// </summary>
    public Cycle Cycle;

    /// <summary>
    /// Список существующих серий.
    /// </summary>
    private readonly List<Cycle> cycles = [];

    /// <summary>
    /// Инициализирует новый экземпляр класса.
    /// </summary>
    /// <param name="cycle">Редактируемая серия.</param>
    /// <param name="cycles">Список существующих серий.</param>
    public CycleEditor(Cycle? cycle, IEnumerable<Cycle>? cycles)
    {
        InitializeComponent();
        Cycle = cycle ?? new Cycle();
        TitleTextBox.Text = Cycle.Title;
        this.cycles.AddRange(cycles ?? Db.GetCycles());
    }

    private void TitleTextBox_TextChanged(object sender, TextChangedEventArgs e) =>
        SaveButton.IsEnabled = !string.IsNullOrWhiteSpace(TitleTextBox.Text) && TitleTextBox.Text.Trim() != Cycle.Title;

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var title = TitleTextBox.Text.Trim();
        if (cycles.Exists(x => x.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase)))
        {
            MessageBox.Show("Серия с таким названием уже существует.", Title);
            return;
        }
        Cycle.Title = title;
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();
}
