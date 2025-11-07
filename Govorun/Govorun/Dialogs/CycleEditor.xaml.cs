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
    /// Возвращает было ли изменено название серии.
    /// </summary>
    public bool TitleChanged { get; private set; }

    /// <summary>
    /// Редактируемая серия.
    /// </summary>
    private readonly Cycle cycle;

    /// <summary>
    /// Инициализирует новый экземпляр класса.
    /// </summary>
    /// <param name="cycle">Редактируемая серия.</param>
    public CycleEditor(Cycle cycle)
    {
        InitializeComponent();
        this.cycle = cycle;
        TitleTextBox.Text = cycle.Title;
        AnnotationTextBox.Text = cycle.Annotation;
    }

    /// <summary>
    /// Проверяет доступность кнопки Сохранить.
    /// </summary>
    private void CheckSaveButton()
    {
        var titleEmpty = string.IsNullOrWhiteSpace(TitleTextBox.Text);
        TitleChanged = TitleTextBox.Text.Trim() != cycle.Title;
        SaveButton.IsEnabled = !titleEmpty && (TitleChanged || AnnotationTextBox.Text.Trim() != cycle.Annotation);
    }

    private void TitleTextBox_TextChanged(object sender, TextChangedEventArgs e) => CheckSaveButton();

    private void AnnotationTextBox_TextChanged(object sender, TextChangedEventArgs e) => CheckSaveButton();

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var title = TitleTextBox.Text.Trim();
        var annotation = AnnotationTextBox.Text.Trim();

        var foundCycle = Library.Cycles.Find(x => x.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase));
        if (foundCycle != null && foundCycle.CycleId != cycle.CycleId)
        {
            MessageBox.Show("Серия с таким названием уже существует.", Title);
            return;
        }

        var origTitle = cycle.Title;
        var origAnnotation = cycle.Annotation;

        cycle.Title = title;
        cycle.Annotation = annotation;

        var saved = cycle.CycleId > 0 ? Library.UpdateCycle(cycle) : Library.AddCycle(cycle);
        if (!saved)
        {
            MessageBox.Show("Не удалось сохранить серию.", Title);
            cycle.Title = origTitle;
            cycle.Annotation = origAnnotation;
            DialogResult = false;
        }

        if (TitleChanged)
        {
            var bookInfoWindow = App.FindBookInfoWindow();
            if (bookInfoWindow != null && Library.BookInCycle(bookInfoWindow.Book, cycle.CycleId))
                bookInfoWindow.UpdateCycle();
        }

        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();
}
