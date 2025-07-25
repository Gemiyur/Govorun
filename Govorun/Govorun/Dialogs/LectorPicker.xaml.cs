﻿using System.Windows;
using System.Windows.Controls;
using Govorun.Tools;

namespace Govorun.Dialogs;

/// <summary>
/// Класс окна выбора чтеца.
/// </summary>
public partial class LectorPicker : Window
{
    /// <summary>
    /// Выбранный чтец.
    /// </summary>
    public string PickedLector = string.Empty;

    public LectorPicker()
    {
        InitializeComponent();
        LectorsListBox.ItemsSource = Library.Lectors;
    }

    private void LectorsListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.OriginalSource is TextBlock && LectorsListBox.SelectedItem != null)
        {
            PickedLector = (string)LectorsListBox.SelectedItem;
            DialogResult = true;
        }
    }

    private void LectorsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        PickButton.IsEnabled = LectorsListBox.SelectedIndex > -1;
    }

    private void PickButton_Click(object sender, RoutedEventArgs e)
    {
        PickedLector = (string)LectorsListBox.SelectedItem;
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
