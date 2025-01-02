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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Govorun.Controls
{
    /// <summary>
    /// Проигрыватель.
    /// </summary>
    public partial class PlayerControl : UserControl
    {
        public PlayerControl()
        {
            InitializeComponent();
        }

        private void Player_MediaOpened(object sender, RoutedEventArgs e)
        {

        }

        private void Player_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {

        }

        private void Player_MediaEnded(object sender, RoutedEventArgs e)
        {

        }

        private void TimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void DecreaseButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void IncreaseButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ChaptersButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BookmarksButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddBookmarksButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
