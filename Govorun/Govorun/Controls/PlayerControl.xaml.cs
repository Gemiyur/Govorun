using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Govorun.Models;
using Govorun.Windows;

namespace Govorun.Controls
{
    #region Задачи (TODO).

    // TODO: Реализовать работу с закладками.
    // TODO: Стоит ли добавить кнопки перехода к предыдущей и следующей главе?
    // TODO: Сделать шрифт названия и времени жирным. Или не надо?

    #endregion

    /// <summary>
    /// Класс проигрывателя.
    /// </summary>
    public partial class PlayerControl : UserControl
    {
        /// <summary>
        /// Путь к изображениям для доступных кнопок.
        /// </summary>
        const string EnabledPath = @"PlayerImages\Enabled\";

        /// <summary>
        /// Путь к изображениям для недоступных кнопок.
        /// </summary>
        const string DisabledPath = @"PlayerImages\Disabled\";

        /// <summary>
        /// Воспроизводимая книга.
        /// </summary>
        private Book? book;

        /// <summary>
        /// Возвращает или задаёт воспроизводимую книгу.
        /// </summary>
        public Book? Book
        {
            get => book;
            set
            {
                book = value;
                IsEnabled = book != null;
                if (book == null)
                {
                    Player.Source = null;
                    TitleTextBlock.Text = string.Empty;
                    TimeSlider.Value = 0;
                    PassTimeTextBlock.Text = App.TimeSpanToString(TimeSpan.Zero);
                    FullTimeTextBlock.Text = App.TimeSpanToString(TimeSpan.Zero);
                    LeftTimeTextBlock.Text = App.TimeSpanToString(TimeSpan.Zero);
                    SetPlayingControlsEnabled(false);
                    SetInfoButtonEnabled(false);
                    SetChaptersButtonEnabled(false);
                    SetBookmarksButtonEnabled(false);
                    SetAddBookmarkButtonEnabled(false);
                    return;
                }
                Player.Source = new Uri(book.FileName);
                // TODO: Отображать не только название книги, но и авторов. Или не надо?
                TitleTextBlock.Text = book.Title;
                SetPlayingControlsEnabled(true);
                SetInfoButtonEnabled(true);
                SetChaptersButtonEnabled(book.Chapters.Any());
                SetBookmarksButtonEnabled(book.Bookmarks.Any());
                SetAddBookmarkButtonEnabled(true);
                Playing = true;
            }
        }

        /// <summary>
        /// Возвращает или задаёт позицию воспроизведения книги.
        /// </summary>
        /// <remarks>Работает только если книга загружена в проигрыватель.</remarks>
        public TimeSpan PlayPosition
        {
            get => Player.Position;
            set => TimeSlider.Value = value.TotalSeconds;
        }

        /// <summary>
        /// Таймер воспроизведения.
        /// </summary>
        private readonly DispatcherTimer playTimer = new() { Interval = TimeSpan.FromSeconds(1) };

        /// <summary>
        /// Пришло ли событие от таймера воспроизведения.
        /// </summary>
        private bool playTimerAction;

        /// <summary>
        /// Воспроизводится ли книги.
        /// </summary>
        private bool playing;

        /// <summary>
        /// Возвращает или задаёт воспроизведение книги.
        /// true - воспроизведение, false - пауза.
        /// </summary>
        private bool Playing
        {
            get => playing;
            set
            {
                playing = value;
                if (playing)
                {
                    Player.Play();
                    SetPlayButtonPauseIcon();
                    PlayButton.ToolTip = "Пауза";
                }
                else
                {
                    Player.Pause();
                    SetPlayButtonPlayIcon();
                    PlayButton.ToolTip = "Воспроизвести";
                }
            }
        }

        /// <summary>
        /// Закончилось ли воспроизведение книги.
        /// </summary>
        private bool mediaEnded;

        /// <summary>
        /// Возвращает или задаёт окончание воспроизведения книги.
        /// </summary>
        private bool MediaEnded
        {
            get => mediaEnded;
            set
            {
                mediaEnded = value;
                if (mediaEnded)
                {
                    TimeSlider.IsEnabled = false;
                    BackButton.IsEnabled = false;
                    ((Image)BackButton.Content).Source = App.GetBitmap($"{DisabledPath}Back.png");
                    SkipButton.IsEnabled = false;
                    ((Image)SkipButton.Content).Source = App.GetBitmap($"{DisabledPath}Skip.png");
                    ((Image)PlayButton.Content).Source = App.GetBitmap($"{EnabledPath}Replay.png");
                    PlayButton.ToolTip = "Повторить";
                }
                else
                {
                    TimeSlider.IsEnabled = true;
                    BackButton.IsEnabled = true;
                    ((Image)BackButton.Content).Source = App.GetBitmap($"{EnabledPath}Back.png");
                    SkipButton.IsEnabled = true;
                    ((Image)SkipButton.Content).Source = App.GetBitmap($"{EnabledPath}Skip.png");
                }
            }
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса.
        /// </summary>
        public PlayerControl()
        {
            InitializeComponent();
            playTimer.Tick += PlayTimer_Tick;
            Player.Volume = (double)Properties.Settings.Default.PlayerVolume / 100;
            VolumeSlider.Value = Player.Volume;
        }

        /// <summary>
        /// Устанавливает доступность кнопки добавления закладки.
        /// </summary>
        /// <param name="enabled">Должна ли быть доступна кнопка.</param>
        private void SetAddBookmarkButtonEnabled(bool enabled)
        {
            AddBookmarkButton.IsEnabled = enabled;
            ((Image)AddBookmarkButton.Content).Source = enabled
                ? App.GetBitmap($"{EnabledPath}AddBookmark.png")
                : App.GetBitmap($"{DisabledPath}AddBookmark.png");
        }

        /// <summary>
        /// Устанавливает доступность кнопки окна списка закладок.
        /// </summary>
        /// <param name="enabled">Должна ли быть доступна кнопка.</param>
        private void SetBookmarksButtonEnabled(bool enabled)
        {
            BookmarksButton.IsEnabled = enabled;
            ((Image)BookmarksButton.Content).Source = enabled
                ? App.GetBitmap($"{EnabledPath}Bookmarks.png")
                : App.GetBitmap($"{DisabledPath}Bookmarks.png");
        }

        /// <summary>
        /// Устанавливает доступность кнопки окна содержания.
        /// </summary>
        /// <param name="enabled">Должна ли быть доступна кнопка.</param>
        private void SetChaptersButtonEnabled(bool enabled)
        {
            ChaptersButton.IsEnabled = enabled;
            ((Image)ChaptersButton.Content).Source = enabled
                ? App.GetBitmap($"{EnabledPath}Chapters.png")
                : App.GetBitmap($"{DisabledPath}Chapters.png");
        }

        /// <summary>
        /// Устанавливает доступность кнопки окна информации о книге.
        /// </summary>
        /// <param name="enabled">Должна ли быть доступна кнопка.</param>
        private void SetInfoButtonEnabled(bool enabled)
        {
            InfoButton.IsEnabled = enabled;
            ((Image)InfoButton.Content).Source = enabled
                ? App.GetBitmap($"{EnabledPath}Info.png")
                : App.GetBitmap($"{DisabledPath}Info.png");
        }

        /// <summary>
        /// Устанавливает значок Pause кнопке воспроизведения PlayButton.
        /// </summary>
        private void SetPlayButtonPauseIcon()
        {
            ((Image)PlayButton.Content).Source = PlayButton.IsEnabled
                ? App.GetBitmap($"{EnabledPath}Pause.png")
                : App.GetBitmap($"{DisabledPath}Pause.png");
        }

        /// <summary>
        /// Устанавливает значок Play кнопке воспроизведения PlayButton.
        /// </summary>
        private void SetPlayButtonPlayIcon()
        {
            ((Image)PlayButton.Content).Source = PlayButton.IsEnabled
                ? App.GetBitmap($"{EnabledPath}Play.png")
                : App.GetBitmap($"{DisabledPath}Play.png");
        }

        /// <summary>
        /// Устанавливает доступность элементов управления воспроизведением.
        /// </summary>
        /// <param name="enabled">Должны ли быть доступны элементы управления воспроизведением.</param>
        private void SetPlayingControlsEnabled(bool enabled)
        {
            TimeSlider.IsEnabled = enabled;
            PlayButton.IsEnabled = enabled;
            SetPlayButtonPauseIcon();
            BackButton.IsEnabled = enabled;
            ((Image)BackButton.Content).Source = enabled
                ? App.GetBitmap($"{EnabledPath}Back.png")
                : App.GetBitmap($"{DisabledPath}Back.png");
            SkipButton.IsEnabled = enabled;
            ((Image)SkipButton.Content).Source = enabled
                ? App.GetBitmap($"{EnabledPath}Skip.png")
                : App.GetBitmap($"{DisabledPath}Skip.png");
        }

        /// <summary>
        /// Устанавливает доступность кнопок регулирования громкости.
        /// </summary>
        private void SetVolumeButtonsEnabled()
        {
            var decreaseWasEnabled = DecreaseButton.IsEnabled;
            DecreaseButton.IsEnabled = Player.Volume > 0;
            var increaseWasEnabled = IncreaseButton.IsEnabled;
            IncreaseButton.IsEnabled = Player.Volume < 1;
            if (DecreaseButton.IsEnabled != decreaseWasEnabled)
            {
                ((Image)DecreaseButton.Content).Source = DecreaseButton.IsEnabled
                    ? App.GetBitmap($"{EnabledPath}Decrease.png")
                    : App.GetBitmap($"{DisabledPath}Decrease.png");
            }
            if (IncreaseButton.IsEnabled != increaseWasEnabled)
            {
                ((Image)IncreaseButton.Content).Source = IncreaseButton.IsEnabled
                    ? App.GetBitmap($"{EnabledPath}Increase.png")
                    : App.GetBitmap($"{DisabledPath}Increase.png");
            }
        }

        #region Обработчики событий.

        private void UserControl_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsEnabled)
            {
                VolumeImage.Source = App.GetBitmap($"{EnabledPath}Volume.png");
                ((Image)DecreaseButton.Content).Source = App.GetBitmap($"{EnabledPath}Decrease.png");
                ((Image)IncreaseButton.Content).Source = App.GetBitmap($"{EnabledPath}Increase.png");
                ((Image)BackButton.Content).Source = App.GetBitmap($"{EnabledPath}Back.png");
                ((Image)PlayButton.Content).Source = App.GetBitmap($"{EnabledPath}Play.png");
                ((Image)SkipButton.Content).Source = App.GetBitmap($"{EnabledPath}Skip.png");
                ((Image)InfoButton.Content).Source = App.GetBitmap($"{EnabledPath}Info.png");
                ((Image)ChaptersButton.Content).Source = App.GetBitmap($"{EnabledPath}Chapters.png");
                ((Image)BookmarksButton.Content).Source = App.GetBitmap($"{EnabledPath}Bookmarks.png");
                ((Image)AddBookmarkButton.Content).Source = App.GetBitmap($"{EnabledPath}AddBookmark.png");
            }
            else
            {
                VolumeImage.Source = App.GetBitmap($"{DisabledPath}Volume.png");
                ((Image)DecreaseButton.Content).Source = App.GetBitmap($"{DisabledPath}Decrease.png");
                ((Image)IncreaseButton.Content).Source = App.GetBitmap($"{DisabledPath}Increase.png");
                ((Image)BackButton.Content).Source = App.GetBitmap($"{DisabledPath}Back.png");
                ((Image)PlayButton.Content).Source = App.GetBitmap($"{DisabledPath}Play.png");
                ((Image)SkipButton.Content).Source = App.GetBitmap($"{DisabledPath}Skip.png");
                ((Image)InfoButton.Content).Source = App.GetBitmap($"{DisabledPath}Info.png");
                ((Image)ChaptersButton.Content).Source = App.GetBitmap($"{DisabledPath}Chapters.png");
                ((Image)BookmarksButton.Content).Source = App.GetBitmap($"{DisabledPath}Bookmarks.png");
                ((Image)AddBookmarkButton.Content).Source = App.GetBitmap($"{DisabledPath}AddBookmark.png");
            }
        }

        private void PlayTimer_Tick(object? sender, EventArgs e)
        {
            playTimerAction = true;
            TimeSlider.Value = Player.Position.TotalSeconds;
            playTimerAction = false;
        }

        private void Player_MediaOpened(object sender, RoutedEventArgs e)
        {
            var position = book != null && book.PlayPosition < Player.NaturalDuration.TimeSpan
                ? book.PlayPosition
                : TimeSpan.Zero;
            PassTimeTextBlock.Text = App.TimeSpanToString(position);
            FullTimeTextBlock.Text = App.TimeSpanToString(Player.NaturalDuration.TimeSpan);
            LeftTimeTextBlock.Text = App.TimeSpanToString(Player.NaturalDuration.TimeSpan - position);
            TimeSlider.Maximum = Player.NaturalDuration.TimeSpan.TotalSeconds;
            TimeSlider.Value = position.TotalSeconds;
            SetPlayingControlsEnabled(true);
            Player.Position = position;
            playTimer.Start();
        }

        private void Player_MediaEnded(object sender, RoutedEventArgs e)
        {
            Playing = false;
            MediaEnded = true;
        }

        private void Player_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var message = "Не удалось воспроизвести книгу:\n";
            if (e is ExceptionRoutedEventArgs ex)
                message += ex.ErrorException.Message;
            else
                message += "Неизвестная ошибка.";
            MessageBox.Show(message, "Ошибка");
            SetPlayingControlsEnabled(false);
            Playing = false;
        }

        private void TimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!playTimerAction)
                Player.Position = TimeSpan.FromSeconds(TimeSlider.Value);
            PassTimeTextBlock.Text = App.TimeSpanToString(Player.Position);
            if (Player.NaturalDuration.HasTimeSpan)
                LeftTimeTextBlock.Text = App.TimeSpanToString(Player.NaturalDuration.TimeSpan - Player.Position);
            if (!playTimer.IsEnabled)
                playTimer.Start();
        }

        private void DecreaseButton_Click(object sender, RoutedEventArgs e)
        {
            VolumeSlider.Value = VolumeSlider.Value - 0.01;
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Player.Volume = VolumeSlider.Value;
            VolumeTextBlock.Text = VolumeSlider.Value.ToString("0%");
            SetVolumeButtonsEnabled();
        }

        private void IncreaseButton_Click(object sender, RoutedEventArgs e)
        {
            VolumeSlider.Value = VolumeSlider.Value + 0.01;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            TimeSlider.Value -= 10;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (MediaEnded)
            {
                TimeSlider.Value = 0;
                MediaEnded = false;
            }
            Playing = !Playing;
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            TimeSlider.Value += 30;
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (book != null)
                new BookInfoDialog(book) { Owner = Window.GetWindow(this) }.ShowDialog();
        }

        private void ChaptersButton_Click(object sender, RoutedEventArgs e)
        {
            if (book == null)
                return;
            var dialog = new ChaptersDialog(book) { Owner = Window.GetWindow(this) };
            if (App.SimpleBool(dialog.ShowDialog()) && dialog.Chapter != null)
                PlayPosition = dialog.Chapter.StartTime;
        }

        private void BookmarksButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddBookmarkButton_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion
    }
}
