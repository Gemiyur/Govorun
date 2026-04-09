using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Govorun.Models;
using Govorun.Tools;

namespace Govorun.Dialogs;

/// <summary>
/// Класс редактора серии.
/// </summary>
public partial class CycleEditor : Window
{
    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    private const int GWL_STYLE = -16;
    private const int WS_MAXIMIZEBOX = 0x10000;
    private const int WS_MINIMIZEBOX = 0x20000;

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

    private void Window_SourceInitialized(object sender, EventArgs e)
    {
        IntPtr handle = new WindowInteropHelper(this).Handle;
        _ = SetWindowLong(handle, GWL_STYLE, GetWindowLong(handle, GWL_STYLE) & ~WS_MINIMIZEBOX);
        _ = SetWindowLong(handle, GWL_STYLE, GetWindowLong(handle, GWL_STYLE) & ~WS_MAXIMIZEBOX);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (Properties.Settings.Default.SaveCycleEditorSize &&
            App.SizeDefined(Properties.Settings.Default.CycleEditorSize))
        {
            Width = Properties.Settings.Default.CycleEditorSize.Width;
            Height = Properties.Settings.Default.CycleEditorSize.Height;
        }
        App.CenterOnScreen(this);
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        if (Properties.Settings.Default.SaveCycleEditorSize)
        {
            Properties.Settings.Default.CycleEditorSize = new System.Drawing.Size((int)Width, (int)Height);
        }
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

        var origCycle = cycle.Clone();

        cycle.Title = title;
        cycle.Annotation = annotation;

        var saved = cycle.CycleId > 0 ? Library.UpdateCycle(cycle) : Library.AddCycle(cycle);
        if (!saved)
        {
            MessageBox.Show("Не удалось сохранить серию.", Title);
            origCycle.CopyTo(cycle);
            DialogResult = false;
            return;
        }

        if (TitleChanged)
        {
            var bookInfoWindow = App.GetBookInfoDialog();
            if (bookInfoWindow != null && Library.BookInCycle(bookInfoWindow.Book, cycle.CycleId))
                bookInfoWindow.UpdateCycle();
        }

        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();
}
