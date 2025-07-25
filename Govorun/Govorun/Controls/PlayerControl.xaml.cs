﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Govorun.Models;

namespace Govorun.Controls;

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
    /// Возвращает или задаёт воспроизводить ли книгу после загрузки в проигрыватель.
    /// По умолчанию - true.
    /// </summary>
    /// <remarks>Используется при запуске приложения для загрузки книги без воспроизведения.</remarks>
    public bool PlayOnLoad { get; set; } = true;

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
            FailedTextBlock.Visibility = Visibility.Collapsed;
            var bookmarksWindow = App.FindBookmarksWindow();
            if (bookmarksWindow != null)
                bookmarksWindow.CheckAddButton();
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
                return;
            }
            Player.Source = new Uri(book.FileName);
            TitleTextBlock.Text = book.Title;
            SetPlayingControlsEnabled(true);
            SetInfoButtonEnabled(true);
            SetChaptersButtonEnabled(book.Chapters.Any());
            SetBookmarksButtonEnabled(true);
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
    /// Была ли ошибка загрузки файла книги в проигрыватель.
    /// </summary>
    public bool MediaFailed => FailedTextBlock.IsVisible;

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
                PlayButton.ToolTip = "Слушать";
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
                ((Image)BackButton.Content).Source = App.GetBitmapImage($"{DisabledPath}Back.png");
                SkipButton.IsEnabled = false;
                ((Image)SkipButton.Content).Source = App.GetBitmapImage($"{DisabledPath}Skip.png");
                ((Image)PlayButton.Content).Source = App.GetBitmapImage($"{EnabledPath}Replay.png");
                PlayButton.ToolTip = "Повторить";
            }
            else
            {
                TimeSlider.IsEnabled = true;
                BackButton.IsEnabled = true;
                ((Image)BackButton.Content).Source = App.GetBitmapImage($"{EnabledPath}Back.png");
                SkipButton.IsEnabled = true;
                ((Image)SkipButton.Content).Source = App.GetBitmapImage($"{EnabledPath}Skip.png");
            }
        }
    }

    /// <summary>
    /// Последняя синхронизированная с содержанием глава.
    /// </summary>
    private Chapter? lastSyncChapter;

    /// <summary>
    /// Возвращает главу текущей позиции или null если главы нет.
    /// </summary>
    private Chapter? CurrentChapter => GetChapter(PlayPosition);

    /// <summary>
    /// Инициализирует новый экземпляр класса.
    /// </summary>
    public PlayerControl()
    {
        InitializeComponent();
        FailedTextBlock.Visibility = Visibility.Collapsed;
        playTimer.Tick += PlayTimer_Tick;
        Player.Volume = (double)Properties.Settings.Default.PlayerVolume / 100;
        VolumeSlider.Value = Player.Volume;
    }

    /// <summary>
    /// Проверяет и устанавливает доступность кнопки окна списка закладок.
    /// </summary>
    public void CheckBookmarksButton()
    {
        SetBookmarksButtonEnabled(book != null);
    }

    /// <summary>
    /// Возвращает главу указанной позиции или null если главы нет.
    /// </summary>
    /// <param name="position">Позиция.</param>
    /// <returns>Глава указанной позиции или null если главы нет.</returns>
    private Chapter? GetChapter(TimeSpan position) =>
        book?.Chapters.FirstOrDefault(x => x.StartTime <= position && x.EndTime > position);

    /// <summary>
    /// Воспроизводит книгу.
    /// </summary>
    /// <remarks> Если проигрыватель не доступен или книга воспроизводится, то ничего не делает.</remarks>
    public void Play()
    {
        if (IsEnabled && !Playing)
            Playing = true;
    }

    /// <summary>
    /// Устанавливает доступность кнопки окна списка закладок.
    /// </summary>
    /// <param name="enabled">Должна ли быть доступна кнопка.</param>
    private void SetBookmarksButtonEnabled(bool enabled)
    {
        BookmarksButton.IsEnabled = enabled;
        ((Image)BookmarksButton.Content).Source = enabled
            ? App.GetBitmapImage($"{EnabledPath}Bookmarks.png")
            : App.GetBitmapImage($"{DisabledPath}Bookmarks.png");
    }

    /// <summary>
    /// Устанавливает доступность кнопки окна содержания.
    /// </summary>
    /// <param name="enabled">Должна ли быть доступна кнопка.</param>
    private void SetChaptersButtonEnabled(bool enabled)
    {
        ChaptersButton.IsEnabled = enabled;
        ((Image)ChaptersButton.Content).Source = enabled
            ? App.GetBitmapImage($"{EnabledPath}Chapters.png")
            : App.GetBitmapImage($"{DisabledPath}Chapters.png");
    }

    /// <summary>
    /// Устанавливает доступность кнопки окна информации о книге.
    /// </summary>
    /// <param name="enabled">Должна ли быть доступна кнопка.</param>
    private void SetInfoButtonEnabled(bool enabled)
    {
        InfoButton.IsEnabled = enabled;
        ((Image)InfoButton.Content).Source = enabled
            ? App.GetBitmapImage($"{EnabledPath}Info.png")
            : App.GetBitmapImage($"{DisabledPath}Info.png");
    }

    /// <summary>
    /// Устанавливает значок Pause кнопке воспроизведения PlayButton.
    /// </summary>
    private void SetPlayButtonPauseIcon()
    {
        ((Image)PlayButton.Content).Source = PlayButton.IsEnabled
            ? App.GetBitmapImage($"{EnabledPath}Pause.png")
            : App.GetBitmapImage($"{DisabledPath}Pause.png");
    }

    /// <summary>
    /// Устанавливает значок Play кнопке воспроизведения PlayButton.
    /// </summary>
    private void SetPlayButtonPlayIcon()
    {
        ((Image)PlayButton.Content).Source = PlayButton.IsEnabled
            ? App.GetBitmapImage($"{EnabledPath}Play.png")
            : App.GetBitmapImage($"{DisabledPath}Play.png");
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
            ? App.GetBitmapImage($"{EnabledPath}Back.png")
            : App.GetBitmapImage($"{DisabledPath}Back.png");
        SkipButton.IsEnabled = enabled;
        ((Image)SkipButton.Content).Source = enabled
            ? App.GetBitmapImage($"{EnabledPath}Skip.png")
            : App.GetBitmapImage($"{DisabledPath}Skip.png");
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
                ? App.GetBitmapImage($"{EnabledPath}Decrease.png")
                : App.GetBitmapImage($"{DisabledPath}Decrease.png");
        }
        if (IncreaseButton.IsEnabled != increaseWasEnabled)
        {
            ((Image)IncreaseButton.Content).Source = IncreaseButton.IsEnabled
                ? App.GetBitmapImage($"{EnabledPath}Increase.png")
                : App.GetBitmapImage($"{DisabledPath}Increase.png");
        }
    }

    #region Обработчики событий.

    private void UserControl_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (IsEnabled)
        {
            VolumeImage.Source = App.GetBitmapImage($"{EnabledPath}Volume.png");
            ((Image)DecreaseButton.Content).Source = App.GetBitmapImage($"{EnabledPath}Decrease.png");
            ((Image)IncreaseButton.Content).Source = App.GetBitmapImage($"{EnabledPath}Increase.png");
            ((Image)BackButton.Content).Source = App.GetBitmapImage($"{EnabledPath}Back.png");
            ((Image)PlayButton.Content).Source = App.GetBitmapImage($"{EnabledPath}Play.png");
            ((Image)SkipButton.Content).Source = App.GetBitmapImage($"{EnabledPath}Skip.png");
            ((Image)InfoButton.Content).Source = App.GetBitmapImage($"{EnabledPath}Info.png");
            ((Image)ChaptersButton.Content).Source = App.GetBitmapImage($"{EnabledPath}Chapters.png");
            ((Image)BookmarksButton.Content).Source = App.GetBitmapImage($"{EnabledPath}Bookmarks.png");
        }
        else
        {
            VolumeImage.Source = App.GetBitmapImage($"{DisabledPath}Volume.png");
            ((Image)DecreaseButton.Content).Source = App.GetBitmapImage($"{DisabledPath}Decrease.png");
            ((Image)IncreaseButton.Content).Source = App.GetBitmapImage($"{DisabledPath}Increase.png");
            ((Image)BackButton.Content).Source = App.GetBitmapImage($"{DisabledPath}Back.png");
            ((Image)PlayButton.Content).Source = App.GetBitmapImage($"{DisabledPath}Play.png");
            ((Image)SkipButton.Content).Source = App.GetBitmapImage($"{DisabledPath}Skip.png");
            ((Image)InfoButton.Content).Source = App.GetBitmapImage($"{DisabledPath}Info.png");
            ((Image)ChaptersButton.Content).Source = App.GetBitmapImage($"{DisabledPath}Chapters.png");
            ((Image)BookmarksButton.Content).Source = App.GetBitmapImage($"{DisabledPath}Bookmarks.png");
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
        lastSyncChapter = GetChapter(position);
        PassTimeTextBlock.Text = App.TimeSpanToString(position);
        FullTimeTextBlock.Text = App.TimeSpanToString(Player.NaturalDuration.TimeSpan);
        LeftTimeTextBlock.Text = App.TimeSpanToString(Player.NaturalDuration.TimeSpan - position);
        TimeSlider.Maximum = Player.NaturalDuration.TimeSpan.TotalSeconds;
        TimeSlider.Value = position.TotalSeconds;
        SetPlayingControlsEnabled(true);
        Player.Position = position;
        if (PlayOnLoad)
            playTimer.Start();
        else
        {
            Playing = false;
            PlayOnLoad = true;
        }
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
        {
            message += ex.ErrorException.Message;
            FailedTextBlock.Text = ex.ErrorException.Message;
        }
        else
            message += "Неизвестная ошибка.";
        MessageBox.Show(message, Window.GetWindow(this).Title);
        Playing = false;
        FailedTextBlock.Visibility = Visibility.Visible;
        IsEnabled = false;
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
        if (lastSyncChapter == null)
            return;
        var chaptersWindow = App.FindChaptersWindow();
        if (chaptersWindow != null && chaptersWindow.Book == book && lastSyncChapter != CurrentChapter)
        {
            lastSyncChapter = CurrentChapter;
            chaptersWindow.SelectCurrentChapter();
        }
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
            ((MainWindow)Window.GetWindow(this)).ShowBookInfo(book);
    }

    private void ChaptersButton_Click(object sender, RoutedEventArgs e)
    {
        if (book != null)
            ((MainWindow)Window.GetWindow(this)).ShowChapters(book);
    }

    private void BookmarksButton_Click(object sender, RoutedEventArgs e)
    {
        if (book != null)
            ((MainWindow)Window.GetWindow(this)).ShowBookmarks(book);
    }

    #endregion
}
