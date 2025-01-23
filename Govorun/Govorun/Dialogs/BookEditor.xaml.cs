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

namespace Govorun.Dialogs
{
    /// <summary>
    /// Класс редактора книги.
    /// </summary>
    public partial class BookEditor : Window
    {
        public BookEditor()
        {
            InitializeComponent();
        }

        private void LoadTagButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ChaptersButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BookmarksButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
