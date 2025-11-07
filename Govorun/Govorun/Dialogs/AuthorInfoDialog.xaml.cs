using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Govorun.Models;

namespace Govorun.Dialogs;

/// <summary>
/// Класс окна "Об авторе".
/// </summary>
public partial class AuthorInfoDialog : Window
{
    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    private const int GWL_STYLE = -16;
    private const int WS_MAXIMIZEBOX = 0x10000;
    private const int WS_MINIMIZEBOX = 0x20000;

    public AuthorInfoDialog(Author author)
    {
        InitializeComponent();
        NameTextBlock.FontSize = FontSize + 2;
        NameTextBlock.Text = author.NameFirstMiddleLast;
        AboutTextBox.Text = author.About;
    }

    private void Window_SourceInitialized(object sender, EventArgs e)
    {
        IntPtr handle = new WindowInteropHelper(this).Handle;
        _ = SetWindowLong(handle, GWL_STYLE, GetWindowLong(handle, GWL_STYLE) & ~WS_MINIMIZEBOX);
        _ = SetWindowLong(handle, GWL_STYLE, GetWindowLong(handle, GWL_STYLE) & ~WS_MAXIMIZEBOX);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        //if (Properties.Settings.Default.SaveInfoWindowsLocation &&
        //    App.SizeDefined(Properties.Settings.Default.AuthorInfoWindowSize))
        //{
        //    Left = Properties.Settings.Default.AuthorInfoWindowPos.X;
        //    Top = Properties.Settings.Default.AuthorInfoWindowPos.Y;
        //    Width = Properties.Settings.Default.AuthorInfoWindowSize.Width;
        //    Height = Properties.Settings.Default.AuthorInfoWindowSize.Height;
        //}
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        //if (Properties.Settings.Default.SaveInfoWindowsLocation)
        //{
        //    Properties.Settings.Default.AuthorInfoWindowPos = new System.Drawing.Point((int)Left, (int)Top);
        //    Properties.Settings.Default.AuthorInfoWindowSize = new System.Drawing.Size((int)Width, (int)Height);
        //}
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
