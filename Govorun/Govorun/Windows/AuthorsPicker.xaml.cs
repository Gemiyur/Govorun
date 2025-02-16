using System.Windows;
using System.Windows.Controls;
using Govorun.Models;
using Govorun.Tools;

namespace Govorun.Windows
{
    /// <summary>
    /// Класс окна выбора авторов.
    /// </summary>
    public partial class AuthorsPicker : Window
    {
        /// <summary>
        /// Список выбранных авторов.
        /// </summary>
        public List<Author> PickedAuthors = [];

        public AuthorsPicker()
        {
            InitializeComponent();
            AuthorsListBox.ItemsSource = Db.GetAuthors();
        }

        private void AuthorsListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.OriginalSource is TextBlock && AuthorsListBox.SelectedItem != null)
            {
                PickedAuthors.Add((Author)AuthorsListBox.SelectedItem);
                DialogResult = true;
            }
        }

        private void AuthorsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PickButton.IsEnabled = AuthorsListBox.SelectedIndex > -1;
        }

        private void PickButton_Click(object sender, RoutedEventArgs e)
        {
            PickedAuthors.AddRange(AuthorsListBox.SelectedItems.Cast<Author>());
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
