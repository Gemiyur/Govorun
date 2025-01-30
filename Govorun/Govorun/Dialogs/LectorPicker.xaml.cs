using System.Windows;
using System.Windows.Controls;
using Govorun.Tools;

namespace Govorun.Dialogs
{
    /// <summary>
    /// Класс окна выбора чтеца.
    /// </summary>
    public partial class LectorPicker : Window
    {
        public string Lector = string.Empty;

        public LectorPicker()
        {
            InitializeComponent();
            LectorsListBox.ItemsSource = Books.Lectors;
        }

        private void LectorsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PickButton.IsEnabled = LectorsListBox.SelectedIndex > -1;
        }

        private void PickButton_Click(object sender, RoutedEventArgs e)
        {
            Lector = (string)LectorsListBox.SelectedItem;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
