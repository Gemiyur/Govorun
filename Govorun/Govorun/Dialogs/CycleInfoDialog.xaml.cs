using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Govorun.Models;

namespace Govorun.Dialogs;

/// <summary>
/// Класс окна "О серии".
/// </summary>
public partial class CycleInfoDialog : Window
{
    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    private const int GWL_STYLE = -16;
    private const int WS_MAXIMIZEBOX = 0x10000;
    private const int WS_MINIMIZEBOX = 0x20000;

    public CycleInfoDialog(Cycle cycle)
    {
        InitializeComponent();
        TitleTextBlock.FontSize = FontSize + 2;
        TitleTextBlock.Text = cycle.Title;
        AnnotationTextBox.Text = cycle.Annotation;
    }

    private void Window_SourceInitialized(object sender, EventArgs e)
    {
        IntPtr handle = new WindowInteropHelper(this).Handle;
        _ = SetWindowLong(handle, GWL_STYLE, GetWindowLong(handle, GWL_STYLE) & ~WS_MINIMIZEBOX);
        _ = SetWindowLong(handle, GWL_STYLE, GetWindowLong(handle, GWL_STYLE) & ~WS_MAXIMIZEBOX);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
