using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Govorun
{
    #region Задачи (TODO).

    #endregion

    /// <summary>
    /// Класс приложения.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Имя файла базы данных с полным путём.
        /// </summary>
        public static string? DbName { get; set; }

        /// <summary>
        /// Аналог System.Windows.Forms.Application.DoEvents.
        /// </summary>
        public static void DoEvents() =>
            Current?.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));

        /// <summary>
        /// Возвращает BitmapFrame из указанного массива байт.
        /// </summary>
        /// <param name="data">Массив байт.</param>
        /// <returns>Изображение.</returns>
        public static BitmapFrame GetBitmap(byte[] data) =>
            BitmapDecoder.Create(new MemoryStream(data), BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames[0];

        /// <summary>
        /// Возвращает BitmapImage из указанного файла изображения.
        /// </summary>
        /// <param name="path">Путь к файлу.</param>
        /// <returns></returns>
        public static BitmapImage GetBitmap(string path) => new BitmapImage(new Uri(path, UriKind.Relative));

        /// <summary>
        /// Возвращает логическое значение из логического значения, допускающего неопределённое значение.
        /// </summary>
        /// <param name="value">Логическое значение, допускающее неопределённое значение.</param>
        /// <returns>Логическое значение.</returns>
        public static bool SimpleBool(bool? value) => value ?? false;

        /// <summary>
        /// Возвращает строковое представление указанного интервала времени в формате h:mm:ss.
        /// </summary>
        /// <param name="timeSpan">Интервал времени.</param>
        /// <returns>Строковое представление интервала времени.</returns>
        public static string TimeSpanToString(TimeSpan timeSpan) => timeSpan.ToString(@"h\:mm\:ss");
    }

}
